Shader "Custom/PerObjectHDRI"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _EnvCube ("HDRI Cubemap", Cube) = "" {}

        _HDRIIntensity ("HDRI Intensity", Range(0,3)) = 1
        _HDRIRotation ("HDRI Rotation", Range(0,360)) = 0

        _AmbientStrength ("Ambient Strength", Range(0,2)) = 0.35
        _Contrast ("Lighting Contrast", Range(0.25,3)) = 1

        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        samplerCUBE _EnvCube;

        fixed4 _Color;
        half _HDRIIntensity;
        half _HDRIRotation;
        half _AmbientStrength;
        half _Contrast;
        half _Metallic;
        half _Smoothness;

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
                c * direction.x - s * direction.z,
                direction.y,
                s * direction.x + c * direction.z
            );
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 N = normalize(IN.worldNormal);

            float3 sampleDirection =
                RotateAroundY(N, _HDRIRotation);

            half3 environment =
                texCUBE(_EnvCube, sampleDirection).rgb;

            environment *= _HDRIIntensity;

            // Convert the HDRI sample into a controlled
            // illumination factor instead of simply making
            // the object glow through emission.
            half luminance =
                dot(environment, half3(0.2126, 0.7152, 0.0722));

            luminance =
                pow(max(luminance, 0.001), _Contrast);

            half lighting =
                _AmbientStrength + luminance;

            o.Albedo =
                _Color.rgb * lighting;

            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = _Color.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}