Shader "Hidden/CommonHouse/ColorDepth"
{
    Properties
    {
        // Lets CommandBuffer.Blit bind the source texture.
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

    float _Levels;
    float _Dither;
    float _Saturation;
    TEXTURE2D_X(_MainTex);

    // 4x4 Bayer matrix, values 0..15.
    static const float Bayer[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = ClampAndScaleUVPostProcessTextureForPoint(input.texcoord.xy);
        float3 linearColor = SAMPLE_TEXTURE2D_X(_MainTex, s_point_clamp_sampler, uv).rgb;
        linearColor = lerp(Luminance(linearColor).xxx, linearColor, _Saturation);
        // Reduce colour depth in display (sRGB) space, as the console's framebuffer stored it.
        float3 display = LinearToSRGB(saturate(linearColor));
        uint2 pixel = (uint2)input.positionCS.xy;
        float threshold = (Bayer[(pixel.y & 3) * 4 + (pixel.x & 3)] + 0.5) / 16.0 - 0.5;
        float3 reduced = floor(display * _Levels + 0.5 + threshold * _Dither) / _Levels;
        return float4(SRGBToLinear(saturate(reduced)), 1);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "ColorDepth"
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
