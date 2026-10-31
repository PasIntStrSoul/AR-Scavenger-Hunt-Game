Shader "Custom/PerObjectHDRI"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        _Environment ("HDRI Cubemap", Cube) = "" {}

        _HDRIIntensity ("HDRI Intensity", Range(0,5)) = 1
        _HDRIRotation ("HDRI Rotation", Range(0,360)) = 0
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.25
        _Contrast ("Lighting Contrast", Range(0.25,3)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 200

        Pass
        {
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            samplerCUBE _Environment;

            fixed4 _Color;

            float _HDRIIntensity;
            float _HDRIRotation;
            float _AmbientStrength;
            float _Contrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldNormal =
                    UnityObjectToWorldNormal(v.normal);

                o.uv =
                    TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float3 RotateAroundY(
                float3 direction,
                float degrees)
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

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample original object texture.
                fixed4 baseTexture =
                    tex2D(_MainTex, i.uv);

                // Apply base color tint.
                float3 objectColor =
                    baseTexture.rgb * _Color.rgb;

                // Normalize surface normal.
                float3 worldNormal =
                    normalize(i.worldNormal);

                // Rotate HDRI sampling direction.
                float3 rotatedDirection =
                    RotateAroundY(
                        worldNormal,
                        _HDRIRotation
                    );

                // Sample HDRI environment.
                float3 hdri =
                    texCUBE(
                        _Environment,
                        rotatedDirection
                    ).rgb;

                // Prevent negative values.
                hdri = max(hdri, 0.0);

                // Apply HDRI intensity.
                hdri *= _HDRIIntensity;

                // Tone-map HDR brightness.
                hdri = hdri / (1.0 + hdri);

                // Calculate luminance.
                float luminance =
                    dot(
                        hdri,
                        float3(
                            0.2126,
                            0.7152,
                            0.0722
                        )
                    );

                // Apply lighting contrast.
                float3 contrastAdjusted =
                    lerp(
                        luminance.xxx,
                        hdri,
                        _Contrast
                    );

                // Calculate HDRI illumination.
                float3 lighting =
                    contrastAdjusted +
                    _AmbientStrength;

                // Apply HDRI lighting to original texture.
                float3 finalColor =
                    objectColor * lighting;

                // Clamp output.
                finalColor = saturate(finalColor);

                return fixed4(
                    finalColor,
                    baseTexture.a * _Color.a
                );
            }

            ENDCG
        }
    }

    Fallback Off
}