Shader "Custom/RaymarchingWithDepth"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _SphereRadius ("Sphere Radius", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _Color;
            float _SphereRadius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Sphere SDF
            float sphereSDF(float3 p, float radius)
            {
                return length(p) - radius;
            }

            float layeredNoise(float3 localPos)
            {
                float n = 0.0;

                // Base noise
                n += sin(localPos.x * 10.0) * sin(localPos.y * 10.0) * sin(localPos.z * 10.0) * 0.05;

                // Fine details
                n += sin(localPos.x * 20.0) * sin(localPos.y * 20.0) * sin(localPos.z * 20.0) * 0.02;

                // Add a third layer for smaller, sharper bumps
                //n += sin(localPos.x * 50.0) * sin(localPos.y * 50.0) * sin(localPos.z * 50.0) * 0.01;

                return n;
            }

            float crackPattern(float3 localPos)
            {
                // Cracks based on a periodic function
                return abs(sin(localPos.x * 15.0) * sin(localPos.z * 15.0)) * 0.02;
            }


            float layeredJaggedNoise(float3 localPos)
            {
                float n = 0.0;
                n += sin(localPos.x * 10.0) * sin(localPos.y * 10.0) * sin(localPos.z * 10.0) * 0.05; // Base noise
                n += max(0.0, sin(localPos.y * 30.0) * 0.03); // Sharp jaggedness
                return n;
            }

            float combinedNoise(float3 localPos)
            {
                float n = 0.0;

                // Base noise layer
                n += sin(localPos.x * 10.0) * sin(localPos.y * 10.0) * sin(localPos.z * 10.0) * 0.05;

                // Jagged, sharper layer (applied to all axes)
                n = max(n, sin(localPos.x * 20.0) * sin(localPos.y * 20.0) * 0.03);

                // Additional finer details
                n = max(n, sin(localPos.z * 30.0) * sin(localPos.x * 30.0) * 0.02);

                return n;
            }

            float rockSDF(float3 worldPos)
            {
                // Convert world position to local position
                float3 localPos = mul(unity_WorldToObject, float4(worldPos, 1.0)).xyz;

                float3 scale;
                scale.x = length(unity_ObjectToWorld[0].xyz); // Scale along X
                scale.y = length(unity_ObjectToWorld[1].xyz); // Scale along Y
                scale.z = length(unity_ObjectToWorld[2].xyz); // Scale along Z

                // Normalize local position by scale ratios
                float3 normalizedPos = localPos * scale;

                // Base sphere shape
                float sphereBase = length(localPos) - 0.4;

                // Add noise to normalized position
                float jaggedNoise = combinedNoise(normalizedPos);

                // Combine base shape and noise
                float rock = sphereBase + jaggedNoise;

                return rock;
            }


            // Estimate normals via finite differences
            float3 estimateNormal(float3 p)
            {
                float d = rockSDF(p);
                float epsilon = 0.001; // Small offset for normal estimation
                float3 n = float3(
                    rockSDF(p + float3(epsilon, 0, 0)) - d,
                    rockSDF(p + float3(0, epsilon, 0)) - d,
                    rockSDF(p + float3(0, 0, epsilon)) - d
                );
                return normalize(n);
            }

            // Raymarching function
            float raymarch(float3 ro, float3 rd, out float3 hitPoint)
            {
                float t = 0.0;
                for (int i = 0; i < 64; i++) // Number of steps
                {
                    hitPoint = ro + rd * t;
                    float dist = rockSDF(hitPoint);
                    if (dist < 0.001) // Hit threshold
                        return t;
                    t += dist;
                    if (t > 10.0) // Far plane
                        break;
                }
                return -1.0; // No hit
            }

            fixed4 frag (v2f i, out float depth : SV_Depth) : SV_Target
            {
                float3 rayOrigin = i.worldPos;
                float3 rayDir = normalize(i.worldPos - _WorldSpaceCameraPos);

                float3 hitPoint;
                float t = raymarch(rayOrigin, rayDir, hitPoint);

                if (t > 0.0)
                {
                    // Calculate normal at the hit point
                    float3 normal = estimateNormal(hitPoint);

                    // Ambient and gradient shading
                    float ambient = 0.2; // Base ambient light
                    float viewFactor = max(dot(normal, normalize(-rayDir)), 0.0); // Gradient based on view direction
                    float shading = ambient + viewFactor * 0.8; // Combine ambient and gradient

                    // Compute clip space position for depth
                    float4 clipPos = UnityWorldToClipPos(hitPoint);
                    depth = clipPos.z / clipPos.w; // Normalize depth for SV_Depth output

                    return fixed4(_Color.rgb * shading, 1.0); // Apply shading
                }

                // Clip pixels outside the rock
                clip(-1.0); // Discard this fragment
                return fixed4(0, 0, 0, 0); // Should never reach here
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
