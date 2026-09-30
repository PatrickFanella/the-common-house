// PlayStation-style surface for HDRP (decision D-24). Unlit as far as HDRP knows: it
// lights each vertex itself from globals set by RetroLighting and samples small textures
// without filtering. Geometry and UVs use ordinary perspective projection, with no screen-grid
// snapping or camera-dependent texture distortion. Values are written as display colour;
// the retro volume turns off tonemapping.
Shader "CommonHouse/Retro"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Colour", Color) = (1, 1, 1, 1)
        _Cutoff("Alpha cutoff", Range(0, 1)) = 0.5
        [Toggle] _WorldUV("World-space box mapping", Float) = 0
        _WorldUVScale("World-space texture repeats per metre", Float) = 0.625
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch switch2

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_point_repeat);

    CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float4 _Color;
        float _Cutoff;
        float _WorldUV;
        float _WorldUVScale;
        float _Cull;
    CBUFFER_END

    float4 _RetroLightPositions[8];
    float4 _RetroLightColors[8];
    int _RetroLightCount;
    float4 _RetroAmbient;
    float4 _RetroFogColor;
    float4 _RetroFog;

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
        float4 light : TEXCOORD1; // rgb vertex light, a fog amount
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        float3 positionRWS = TransformObjectToWorld(input.positionOS);
        float3 positionAWS = GetAbsolutePositionWS(positionRWS);
        float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
        float4 positionCS = TransformWorldToHClip(positionRWS);

        float2 uv = TRANSFORM_TEX(input.uv, _MainTex);
        if (_WorldUV > 0.5)
        {
            // Box mapping by the dominant normal axis, for the stretched blockout cubes.
            float3 n = abs(normalWS);
            float2 planar = n.y > max(n.x, n.z) ? positionAWS.xz : (n.x > n.z ? positionAWS.zy : positionAWS.xy);
            uv = planar * _WorldUVScale;
        }

        float3 light = _RetroAmbient.rgb;
        for (int i = 0; i < _RetroLightCount; i++)
        {
            float3 toLight = _RetroLightPositions[i].xyz - positionAWS;
            float distance = length(toLight);
            float falloff = saturate(1.0 - distance / max(_RetroLightPositions[i].w, 0.001));
            light += _RetroLightColors[i].rgb * saturate(dot(normalWS, toLight / max(distance, 0.0001))) * falloff * falloff;
        }
        float fog = saturate((length(positionRWS) - _RetroFog.x) / max(_RetroFog.y - _RetroFog.x, 0.001));

        output.positionCS = positionCS;
        output.uv = uv;
        output.light = float4(light, fog);
        return output;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        float2 uv = input.uv;
        float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_point_repeat, uv) * _Color;
        clip(texel.a - _Cutoff);
        float3 color = texel.rgb * input.light.rgb;
        color = lerp(color, _RetroFogColor.rgb, input.light.a);
        return float4(color, 1.0);
    }

    float4 DepthFrag(Varyings input) : SV_Target
    {
        float2 uv = input.uv;
        clip(SAMPLE_TEXTURE2D(_MainTex, sampler_point_repeat, uv).a * _Color.a - _Cutoff);
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
            #pragma fragment DepthFrag
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
            #pragma fragment Frag
            ENDHLSL
        }
    }
    Fallback Off
}
