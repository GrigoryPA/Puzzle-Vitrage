Shader "Custom/MaskSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex1 ("Main Texture 1 (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap1 ("Normal Map 1 (RGB)", 2D) = "bump" {}
        _BumpScale1 ("Bump Scale 1", Float) = 1
        _MainTex2 ("Main Texture 2 (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap2 ("Normal Map 2 (RGB)", 2D) = "bump" {}
        _BumpScale2 ("Bump Scale 2", Float) = 1
        _MainTex3 ("Main Texture 3 (RGB)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap3 ("Normal Map 3 (RGB)", 2D) = "bump" {}
        _BumpScale3 ("Bump Scale 3", Float) = 1
        [NoScaleOffset] _MaskMap ("Mask Map (RGB)", 2D) = "red" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        
        Tags { "LIGHTMODE"="FORWARDBASE" "SHADOWSUPPORT"="true" "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex1;
        sampler2D _MainTex2;
        sampler2D _MainTex3;
        sampler2D _BumpMap1;
        sampler2D _BumpMap2;
        sampler2D _BumpMap3;
        sampler2D _MaskMap;

        struct Input
        {
            float2 uv_MainTex1;
            float2 uv_MainTex2;
            float2 uv_MainTex3;
            float2 uv_MaskMap;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _BumpScale1;
        float _BumpScale2;
        float _BumpScale3;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)




        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 mask = tex2D (_MaskMap, IN.uv_MaskMap);
            fixed4 c = (tex2D (_MainTex1, IN.uv_MainTex1) * mask.r 
                        + tex2D (_MainTex2, IN.uv_MainTex2) * mask.g 
                        + tex2D (_MainTex3, IN.uv_MainTex3) * mask.b) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = UnpackScaleNormal(tex2D (_BumpMap1, IN.uv_MainTex1) * mask.r 
                        + tex2D (_BumpMap2, IN.uv_MainTex2) * mask.g 
                        + tex2D (_BumpMap3, IN.uv_MainTex3) * mask.b, 
                        _BumpScale1 * mask.r + _BumpScale2 * mask.g + _BumpScale3 * mask.b);
	        o.Normal = o.Normal.xzy;
	        o.Normal = normalize(o.Normal);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
