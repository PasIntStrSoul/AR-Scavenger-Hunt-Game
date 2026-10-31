Shader "Custom/PerObjectHDRI"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Environment ("HDRI Cubemap", Cube) = "" {}
        _HDRIIntensity ("HDRI Intensity", Range(0,5)) = 1
        _HDRIRotation ("HDRI Rotation", Range(0,360)) = 0
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.25
        _Contrast ("Lighting Contrast", Range(0.25,3)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        // Custom lighting ignores Unity's scene light contributions.
        #pragma surface surf HDRIOnly noambient noforwardadd

        #include "UnityCG.cginc"

        fixed4 _Color;
        samplerCUBE _Environment;

        half _HDRIIntensity;
        half _HDRIRotation;
        half _AmbientStrength;
        half _Contrast;

        struct Input
        {
            float3 worldNormal;
        };

        float3 RotateAroundY(float3 direction, float degrees)
        {
            float angle = radians(degrees);

            float s = sin(angle);
            float c = cos(angle);

            return float3(
                c * direction.x + s * direction.z,
                direction.y,
                -s * direction.x + c * direction.z
            );
        }

        half4 LightingHDRIOnly(
            SurfaceOutput s,
            half3 lightDir,
            half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Obtain the world-space normal.
            float3 worldNormal = normalize(IN.worldNormal);

            // Rotate HDRI sampling direction.
            float3 rotatedDirection =
                RotateAroundY(worldNormal, _HDRIRotation);

            // Sample RGB lighting from the HDRI cubemap.
            half3 hdri =
                texCUBE(_Environment, rotatedDirection).rgb;

            // Apply individual material intensity.
            hdri *= _HDRIIntensity;

            // Calculate HDRI luminance.
            half luminance =
                dot(hdri, half3(0.2126, 0.7152, 0.0722));

            // Adjust contrast while preserving RGB colors.
            half3 contrastAdjusted =
                lerp(luminance.xxx, hdri, _Contrast);

            // Prevent negative lighting values.
            contrastAdjusted = max(contrastAdjusted, 0.0);

            // Apply optional ambient contribution.
            half3 finalColor =
                contrastAdjusted + _AmbientStrength;

            // Apply base material color.
            o.Albedo = finalColor * _Color.rgb;
            o.Alpha = _Color.a;
        }

        ENDCG
    }

    Fallback Off
}