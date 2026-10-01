Shader "Custom/PerObjectHDRI"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _EnvCube ("HDRI Cubemap", Cube) = "" {}
        _HDRIIntensity ("HDRI Intensity", Range(0,5)) = 1
        _HDRIRotation ("HDRI Rotation", Range(0,360)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
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
        half _Metallic;
        half _Smoothness;

        struct Input
        {
            float3 worldNormal;
        };

        float3 RotateAroundY(float3 direction, float degrees)
        {
            float radians = degrees * 0.01745329252;
            float s = sin(radians);
            float c = cos(radians);

            float3 rotatedDirection;
            rotatedDirection.x = c * direction.x - s * direction.z;
            rotatedDirection.y = direction.y;
            rotatedDirection.z = s * direction.x + c * direction.z;

            return rotatedDirection;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 normalDirection = normalize(IN.worldNormal);

            normalDirection =
                RotateAroundY(normalDirection, _HDRIRotation);

            half3 hdriLighting =
                texCUBE(_EnvCube, normalDirection).rgb;

            hdriLighting *= _HDRIIntensity;

            o.Albedo = _Color.rgb;
            o.Emission = hdriLighting * _Color.rgb;

            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = _Color.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}