Shader "Hidden/CommonHouse/HazeFilter"
{
    Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch switch2

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float _Haze;
    float _Radius;
    float4 _Tint;
    float _Lift;
    float _Saturation;
    TEXTURE2D_X(_MainTex);

    float3 Sample(float2 uv)
    {
        return SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(uv)).rgb;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = input.texcoord.xy;
        float2 step = _PostProcessScreenSize.zw * _Radius;
        float3 color = Sample(uv);
        // A 3x3 box of bilinear taps, two radii out, as a cheap soft blur.
        float3 blur = 0;
        [unroll] for (int y = -1; y <= 1; y++)
        [unroll] for (int x = -1; x <= 1; x++)
            blur += Sample(uv + float2(x, y) * step) + Sample(uv + float2(x, y) * step * 2.0);
        blur /= 18.0;
        color = lerp(color, max(color, blur), _Haze);
        color = color * _Tint.rgb + _Lift;
        color = lerp(Luminance(color).xxx, color, _Saturation);
        return float4(color, 1);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "HazeFilter"
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off
            HLSLPROGRAM
                #pragma fragment Frag
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
