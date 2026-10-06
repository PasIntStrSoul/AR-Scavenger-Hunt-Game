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

        // HDRI directly controls the object's appearance.
        // Unity scene lights are intentionally ignored.
        #pragma surface surf NoLighting noforwardadd

        samplerCUBE _Environment;

        fixed4 _Color;
        half _HDRIIntensity;
        half _HDRIRotation;
        half _AmbientStrength;
        half _Contrast;

        struct Input
        {
            float3 worldNormal;
        };

        float3 RotateAroundY(float3 dir, float degrees)
        {
            float angle = radians(degrees);
            float s = sin(angle);
            float c = cos(angle);

            return float3(
                c * dir.x + s * dir.z,
                dir.y,
               -s * dir.x + c * dir.z
            );
        }

        half4 LightingNoLighting(
            SurfaceOutput s,
            half3 lightDir,
            half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Explicitly normalize the WORLD-SPACE surface normal.
            float3 N = normalize(IN.worldNormal);

            // Rotate the cubemap sampling direction around world Y.
            float3 rotatedN =
                normalize(RotateAroundY(N, _HDRIRotation));

            // Sample HDRI using the rotated direction.
            half3 hdri =
                texCUBE(_Environment, rotatedN).rgb;

            hdri *= _HDRIIntensity;

            half luminance =
                dot(hdri, half3(0.2126, 0.7152, 0.0722));

            hdri =
                lerp(luminance.xxx, hdri, _Contrast);

            half3 finalColor =
                _Color.rgb *
                (hdri + _AmbientStrength);

            o.Albedo = finalColor;
            o.Alpha = _Color.a;
        }

        ENDCG
    }

    FallBack Off
}