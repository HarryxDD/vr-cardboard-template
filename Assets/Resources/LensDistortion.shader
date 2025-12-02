Shader "Custom/LensDistortion"
{
    Properties
    {
        _Color ("Tint Color", Color) = (0.8, 0.9, 1.0, 0.3)
        _RefractStrength ("Refraction Strength", Range(0, 0.5)) = 0.1
        _Glossiness ("Smoothness", Range(0,1)) = 0.95
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 8.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 1.0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        
        // Grab the screen behind the object into a texture
        GrabPass { "_GrabTexture" }
        
        CGPROGRAM
        #pragma surface surf Standard alpha finalcolor:RefractionFunction
        #pragma target 3.0
        
        sampler2D _GrabTexture;
        float4 _GrabTexture_TexelSize;
        
        fixed4 _Color;
        half _RefractStrength;
        half _Glossiness;
        half _Metallic;
        fixed4 _RimColor;
        half _RimPower;
        half _RimIntensity;
        
        struct Input
        {
            float4 screenPos;
            float3 worldNormal;
            float3 viewDir;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate refraction offset based on viewing angle
            half3 normal = normalize(IN.worldNormal);
            half3 viewDir = normalize(IN.viewDir);
            
            // Fresnel rim lighting effect for visible edge
            half rim = 1.0 - saturate(dot(viewDir, normal));
            half3 rimLight = _RimColor.rgb * pow(rim, _RimPower) * _RimIntensity;
            
            // Set material properties
            o.Albedo = _Color.rgb;
            o.Emission = rimLight; // Add rim glow to emission
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a + rim * 0.3; // Slightly more opaque at edges
        }
        
        void RefractionFunction(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            // Calculate screen UV coordinates
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            
            // Calculate refraction offset based on surface normal
            half3 normal = normalize(IN.worldNormal);
            half3 viewDir = normalize(IN.viewDir);
            
            // Refract the view direction
            half3 refractDir = refract(-viewDir, normal, 0.67); // 1.0/1.5 = 0.67 (air to glass)
            
            // Convert refraction to screen space offset
            float2 offset = refractDir.xy * _RefractStrength;
            
            // Sample the grabbed screen with offset
            float2 distortedUV = screenUV + offset;
            fixed4 refractColor = tex2D(_GrabTexture, distortedUV);
            
            // Blend between surface color and refracted background
            color = lerp(color, refractColor, 0.7) * _Color;
        }
        
        ENDCG
    }
    
    FallBack "Standard"
}
