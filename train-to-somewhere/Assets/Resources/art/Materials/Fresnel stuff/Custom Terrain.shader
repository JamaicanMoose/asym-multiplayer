Shader "Custom/Custom Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}
	_FresRamp("Rim-light Ramp (RGB)", 2D) = "white" {}
	_FresColor("Rim-light Color", Color) = (0.5,0.5,0.5,1)
		_Offset("Rim offset", Range(0,1)) = 1
		_Brightness("Rimlight Opacity", Range(0,1)) = 1

			// Splat Map Control Texture
			[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}

		// Textures
		[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}

		// Normal Maps
		[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _Control;
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		sampler2D _FresRamp;
		float4 _FresColor;
		
		float _Offset;
		float _Brightness;

        struct Input
        {
            float2 uv_Control;
			float2 uv_Splat0 : TEXCOORD1;
			float2 uv_Splat1 : TEXCOORD2;
			float2 uv_Splat2 : TEXCOORD3;
			float2 uv_Splat3 : TEXCOORD4;
			float3 worldNormal;
			float3 viewDir;
        };

        
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			half f = 1 - dot(o.Normal, IN.viewDir) + _Offset;
			half t = 1 - dot(o.Normal, float3(0, 1, 0)) + _Offset;
			f = saturate(f);
			t = saturate(t);
			half3 r = tex2D(_FresRamp, IN.uv_Control)* f *t* _FresColor;
			
            // Albedo comes from a texture tinted by color
			fixed4 splat_control = tex2D(_Control, IN.uv_Control);
			fixed3 col;
			col = splat_control.r * tex2D(_Splat0, IN.uv_Control).rgb;
			col += splat_control.g * tex2D(_Splat1, IN.uv_Splat1).rgb;
			col += splat_control.b * tex2D(_Splat2, IN.uv_Splat2).rgb;
			col += splat_control.a * tex2D(_Splat3, IN.uv_Splat3).rgb;
			o.Albedo = col * _Color + (r * _Brightness);
			o.Alpha = 0.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
