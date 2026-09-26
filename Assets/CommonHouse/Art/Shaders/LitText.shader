// Lit, alpha-tested ink for TextMesh labels, so handwritten-style notes do not glow in dark rooms.
Shader "CommonHouse/LitText"
{
    Properties
    {
        _MainTex ("Font texture", 2D) = "white" {}
        _Color ("Ink", Color) = (0.1, 0.1, 0.1, 1)
        _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert alphatest:_Cutoff
        sampler2D _MainTex;
        fixed4 _Color;
        struct Input { float2 uv_MainTex; };
        void surf(Input input, inout SurfaceOutput output)
        {
            output.Albedo = _Color.rgb;
            output.Alpha = tex2D(_MainTex, input.uv_MainTex).a;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/Diffuse"
}
