Shader "Basic Surface" {

Properties {

    _Color ("Main Color", Color) = (1,1,1,1)

    _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)

    _Shininess ("Shininess", Range (0.01, 1)) = 0.078125

    _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)

    _Parallax ("Height", Range (0.005, 0.08)) = 0.02

    _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" { }

    _SpecTex ("Spec(RGB)", 2D) = "white" { }

    _Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }

    _BumpMap ("Normalmap", 2D) = "bump" { }

    _ParallaxMap ("Heightmap (A)", 2D) = "black" {}

}

SubShader {

    Tags { "RenderType"="Opaque" }

    LOD 600

    

CGPROGRAM

#pragma surface surf Custom

#pragma target 3.0

 

sampler2D _MainTex;

sampler2D _SpecTex;

 

sampler2D _BumpMap;

samplerCUBE _Cube;

sampler2D _ParallaxMap;

 

float4 _Color;

float4 _ReflectColor;

float _Shininess;

float _Parallax;

 

 

struct SurfaceOutputCustom {

    half3 Albedo;

    half3 Normal;

    half3 Emission;

    half3 Reflection;

    half Specular;

    half3 Gloss;

    half Alpha;

};

 

inline half4 LightingCustom_PrePass (SurfaceOutputCustom s, half4 light)

{

    half3 spec =(s.Gloss)*light.a ;

    //float spec = pow (nh, s.Specular*128.0) * s.Gloss;

 

    half4 c;

    c.rgb = (s.Albedo * light.rgb + light.rgb * _SpecColor.rgb * spec);

    c.a = s.Alpha + spec * _SpecColor.a;

    return c;

}

 

struct Input {

    float2 uv_MainTex;

    float2 uv_SpecTex;

    float2 uv_BumpMap;

    float3 worldRefl;

    float3 viewDir;

    INTERNAL_DATA

};

 

void surf (Input IN, inout SurfaceOutputCustom o) {

    half h = tex2D (_ParallaxMap, IN.uv_BumpMap).w;

    float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);

    IN.uv_MainTex += offset;

    IN.uv_BumpMap += offset;

    half4 tex = tex2D(_MainTex, IN.uv_MainTex);

 

    half3 spec = tex2D(_SpecTex, IN.uv_SpecTex).rgb;

    o.Albedo = tex.rgb * _Color.rgb;

    o.Gloss = spec ;

    o.Specular = _Shininess;

    

    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

    

    float3 worldRefl = WorldReflectionVector (IN, o.Normal);

    half4 reflcol = texCUBE (_Cube, worldRefl);

    //reflcol.rgb *= o.Gloss;

    o.Emission = reflcol.rgb * _ReflectColor.rgb*_ReflectColor.a*o.Specular*o.Gloss;

    o.Alpha = reflcol.a * _ReflectColor.a;

}

ENDCG

}

 

FallBack "Reflective/Bumped Specular"

}