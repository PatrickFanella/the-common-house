// Experimental 1990s TV-anime surface for HDRP (issue #73). Unlit as far as HDRP knows: textures
// are flattened by sampling a blurred mip level and posterised, lighting from PS1Lighting's
// globals is cut into a lit tone and a cool shadow tone, and a second pass draws an ink outline
// by pushing back faces outwards along their normals.
Shader "CommonHouse/Anime"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Colour", Color) = (1, 1, 1, 1)
        _Cutoff("Alpha cutoff", Range(0, 1)) = 0
        [Toggle] _WorldUV("World-space box mapping", Float) = 0
        _WorldUVScale("World-space texture repeats per metre", Float) = 0.3
        _Flatten("Mip level used to flatten texture detail", Range(0, 10)) = 5
        _Levels("Colour levels per channel", Range(2, 16)) = 6
        _Saturation("Saturation", Range(0, 2)) = 1.25
        _ShadowTint("Shadow tone", Color) = (0.55, 0.55, 0.72, 1)
        _Threshold("Light needed for the lit tone", Range(0, 2)) = 0.55
        _OutlineWidth("Outline width (fraction of distance)", Range(0, 0.02)) = 0.0025
        _OutlineColor("Outline colour", Color) = (0.07, 0.06, 0.08, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch switch2

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_linear_repeat);

    CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float4 _Color;
        float _Cutoff;
        float _WorldUV;
        float _WorldUVScale;
        float _Flatten;
        float _Levels;
        float _Saturation;
        float4 _ShadowTint;
        float _Threshold;
        float _OutlineWidth;
        float4 _OutlineColor;
        float _Cull;
    CBUFFER_END

    float4 _PS1LightPositions[8];
    float4 _PS1LightColors[8];
    int _PS1LightCount;
    float4 _PS1Ambient;

    struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 positionAWS : TEXCOORD1;
        float3 normalWS : TEXCOORD2;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    float2 SurfaceUV(float2 uv, float3 positionAWS, float3 normalWS)
    {
        if (_WorldUV > 0.5)
        {
            float3 n = abs(normalWS);
            float2 planar = n.y > max(n.x, n.z) ? positionAWS.xz : (n.x > n.z ? positionAWS.zy : positionAWS.xy);
            return planar * _WorldUVScale;
        }
        return TRANSFORM_TEX(uv, _MainTex);
    }

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        float3 positionRWS = TransformObjectToWorld(input.positionOS);
        output.positionCS = TransformWorldToHClip(positionRWS);
        output.positionAWS = GetAbsolutePositionWS(positionRWS);
        output.normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
        output.uv = input.uv;
        return output;
    }

    float4 Fill(Varyings input) : SV_Target
    {
        float3 normalWS = normalize(input.normalWS);
        float2 uv = SurfaceUV(input.uv, input.positionAWS, normalWS);
        float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_linear_repeat, uv).a * _Color.a;
        clip(alpha - _Cutoff);
        // Flat colour: a blurred mip, pushed towards saturated, posterised in display space.
        float3 base = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_linear_repeat, uv, _Flatten).rgb * _Color.rgb;
        base = lerp(Luminance(base).xxx, base, _Saturation);
        float3 display = LinearToSRGB(saturate(base));
        display = floor(display * _Levels + 0.5) / _Levels;
        base = SRGBToLinear(display);

        float light = Luminance(_PS1Ambient.rgb);
        float3 tint = _PS1Ambient.rgb;
        for (int i = 0; i < _PS1LightCount; i++)
        {
            float3 toLight = _PS1LightPositions[i].xyz - input.positionAWS;
            float distance = length(toLight);
            float falloff = saturate(1.0 - distance / max(_PS1LightPositions[i].w, 0.001));
            float3 contribution = _PS1LightColors[i].rgb * saturate(dot(normalWS, toLight / max(distance, 0.0001))) * falloff * falloff;
            light += Luminance(contribution);
            tint += contribution;
        }
        // Two tones, as cel animation paints them: lit, or a cool shadow.
        float3 lightColor = tint / max(Luminance(tint), 0.0001);
        float3 tone = light >= _Threshold ? lerp(1.0.xxx, lightColor, 0.25) : _ShadowTint.rgb;
        return float4(base * tone, 1.0);
    }

    Varyings OutlineVert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        float3 positionRWS = TransformObjectToWorld(input.positionOS);
        float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
        // Width grows with distance, so lines stay about the same thickness on screen.
        positionRWS += normalWS * _OutlineWidth * length(positionRWS);
        output.positionCS = TransformWorldToHClip(positionRWS);
        output.positionAWS = GetAbsolutePositionWS(positionRWS);
        output.normalWS = normalWS;
        output.uv = input.uv;
        return output;
    }

    float4 Outline(Varyings input) : SV_Target
    {
        clip(_OutlineWidth - 0.00001);
        return float4(_OutlineColor.rgb, 1.0);
    }

    float4 Depth(Varyings input) : SV_Target
    {
        float2 uv = SurfaceUV(input.uv, input.positionAWS, normalize(input.normalWS));
        clip(SAMPLE_TEXTURE2D(_MainTex, sampler_linear_repeat, uv).a * _Color.a - _Cutoff);
        return 0;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "DepthForwardOnly"
            Tags { "LightMode" = "DepthForwardOnly" }
            Cull [_Cull]
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Depth
            ENDHLSL
        }

        Pass
        {
            Name "SRPDefaultUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fill
            ENDHLSL
        }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            Cull Front
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment Outline
            ENDHLSL
        }
    }
    Fallback Off
}
