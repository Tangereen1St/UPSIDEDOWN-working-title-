Shader "Custom/SkyboxBlend"
{
    Properties
    {
        _BlendAmount ("Blend Amount", Range(0, 1)) = 0
        _TransitionState ("Transition State", Range(0, 4)) = 0 // 0=night->sunrise, 1=sunrise->day, 2=day->sunset, 3=sunset->night
        
        // Day skybox textures
        [NoScaleOffset] _DayTex ("Day Panoramic", 2D) = "white" {}
        _DayExposure ("Day Exposure", Range(0, 8)) = 1.0
        _DayRotation ("Day Rotation", Range(0, 360)) = 0
        
        // Night skybox textures
        [NoScaleOffset] _NightTex ("Night Panoramic", 2D) = "black" {}
        _NightExposure ("Night Exposure", Range(0, 8)) = 1.0
        _NightRotation ("Night Rotation", Range(0, 360)) = 0

        // Sunrise skybox textures
        [NoScaleOffset] _SunriseTex ("Sunrise Panoramic", 2D) = "white" {}
        _SunriseExposure ("Sunrise Exposure", Range(0, 8)) = 1.0
        _SunriseRotation ("Sunrise Rotation", Range(0, 360)) = 0

        // Sunset skybox textures
        [NoScaleOffset] _SunsetTex ("Sunset Panoramic", 2D) = "white" {}
        _SunsetExposure ("Sunset Exposure", Range(0, 8)) = 1.0
        _SunsetRotation ("Sunset Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DayTex;
            sampler2D _NightTex;
            sampler2D _SunriseTex;
            sampler2D _SunsetTex;
            float _BlendAmount;
            float _TransitionState;
            float _DayExposure;
            float _NightExposure;
            float _SunriseExposure;
            float _SunsetExposure;
            float _DayRotation;
            float _NightRotation;
            float _SunriseRotation;
            float _SunsetRotation;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            // Convert 3D direction to equirectangular UV coordinates
            float2 DirectionToEquirectangular(float3 dir, float rotation)
            {
                // Apply rotation around Y axis
                float rad = rotation * UNITY_PI / 180;
                float sinR = sin(rad);
                float cosR = cos(rad);
                float3 rotatedDir = float3(
                    dir.x * cosR - dir.z * sinR,
                    dir.y,
                    dir.x * sinR + dir.z * cosR
                );
                
                // Convert to spherical coordinates
                float2 uv = float2(
                    atan2(rotatedDir.x, rotatedDir.z) / (2.0 * UNITY_PI) + 0.5,
                    acos(rotatedDir.y) / UNITY_PI
                );
                
                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Normalize direction
                float3 dir = normalize(i.texcoord);
                
                // Get UV coordinates for all textures with their respective rotations
                float2 dayUV = DirectionToEquirectangular(dir, _DayRotation);
                float2 nightUV = DirectionToEquirectangular(dir, _NightRotation);
                float2 sunriseUV = DirectionToEquirectangular(dir, _SunriseRotation);
                float2 sunsetUV = DirectionToEquirectangular(dir, _SunsetRotation);

                // Sample all panoramic textures
                fixed4 dayColor = tex2D(_DayTex, dayUV) * _DayExposure;
                fixed4 nightColor = tex2D(_NightTex, nightUV) * _NightExposure;
                fixed4 sunriseColor = tex2D(_SunriseTex, sunriseUV) * _SunriseExposure;
                fixed4 sunsetColor = tex2D(_SunsetTex, sunsetUV) * _SunsetExposure;

                // Determine which two textures to blend based on transition state
                fixed4 color1, color2;
                if (_TransitionState < 1) // Night to Sunrise
                {
                    color1 = nightColor;
                    color2 = sunriseColor;
                }
                else if (_TransitionState < 2) // Sunrise to Day
                {
                    color1 = sunriseColor;
                    color2 = dayColor;
                }
                else if (_TransitionState < 3) // Day to Sunset
                {
                    color1 = dayColor;
                    color2 = sunsetColor;
                }
                else // Sunset to Night
                {
                    color1 = sunsetColor;
                    color2 = nightColor;
                }

                // Blend between them
                return lerp(color1, color2, _BlendAmount);
            }
            ENDCG
        }
    }
} 