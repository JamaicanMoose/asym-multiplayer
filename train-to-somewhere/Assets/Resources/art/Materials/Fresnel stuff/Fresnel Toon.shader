Shader "Toon/Fresnel" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		_FresRamp("Rim-light Ramp (RGB)", 2D) = "white" {}
		_FresColor("Rim-light Color", Color) = (0.5,0.5,0.5,1)
		_Offset("Rim offset", Range(0,1)) = 1
		_Brightness("Rimlight Opacity", Range (0,1)) = 1

	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
CGPROGRAM
#pragma surface surf ToonRamp

sampler2D _Ramp;

// custom lighting function that uses a texture ramp based
// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass
inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
{
	#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
	#endif
	
	half d = dot (s.Normal, lightDir)*0.5 + 0.5;
	half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
	
	half4 c;
	c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
	c.a = 0;
	return c;
}


sampler2D _MainTex;
sampler2D _FresRamp;
float4 _FresColor;
float4 _Color;
float _Offset;
float _Brightness;

struct Input {
	float2 uv_MainTex : TEXCOORD0;
	float3 worldNormal;
	float3 viewDir;
};

void surf (Input IN, inout SurfaceOutput o) {
	half f = 1 - dot(o.Normal, IN.viewDir) + _Offset;
	half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	half3 r = tex2D(_FresRamp, IN.uv_MainTex)* f * _FresColor;
	
	o.Albedo = (c.rgb) + (r * _Brightness);
	o.Alpha = c.a;
}
ENDCG

	} 

	Fallback "Diffuse"
}
