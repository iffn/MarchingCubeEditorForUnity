Shader "Custom/RaymarchingWithDepth"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        _BaseAmplitude ("Base Amplitude", Range(0, 0.15)) = 0.05
        _BaseFrequency ("Base Frequency", Range(1, 50)) = 10.0
        _BaseScale("Base Scale", Range(0.1, 10)) = 1
        
        _VoronoiStrength ("Voronoi Strength", Range(0, 0.2)) = 0.02
        _VoronoiScale("Voronoi Scale", Range(0.1, 10)) = 1
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

            float _BaseScale;
            float _BaseFrequency;
            float _BaseAmplitude;
            
            float _VoronoiScale;
            float _VoronoiStrength;

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

            float voronoi(float3 p)
            {
                float3 cell = floor(p);
                float minDist = 1.0;

                for (int z = -1; z <= 1; z++) // Loop over neighboring cells in the z-direction
                {
                    for (int y = -1; y <= 1; y++) // Loop over neighboring cells in the y-direction
                    {
                        for (int x = -1; x <= 1; x++) // Loop over neighboring cells in the x-direction
                        {
                            float3 neighbor = cell + float3(x, y, z);

                            // Generate a random offset for the cell point
                            float3 cellPoint = neighbor + frac(sin(dot(neighbor, float3(12.9898, 78.233, 37.719))) * 43758.5453);

                            // Compute the distance from the input point to this cell point
                            float dist = length(p - cellPoint);

                            // Track the minimum distance
                            minDist = min(minDist, dist);
                        }
                    }
                }

                return minDist;
            }

            float combinedNoise(
                float3 localPos
            )
            {
                float returnValue = 0.0;

                // Base noise layer
                float3 basePosition = localPos * _BaseFrequency * _BaseScale;
                returnValue += sin(basePosition.x) * sin(basePosition.y) * sin(basePosition.z) * _BaseAmplitude;

                // Add Voronoi cracks
                returnValue -= voronoi(localPos * _VoronoiScale) * _VoronoiStrength;

                return returnValue;
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
                float3 normalizedPos = localPos * scale / (scale.x + scale.y + scale.z) * 3;

                // Base sphere shape
                float sphereBase = sphereSDF(localPos, 0.4);

                // Add noise to normalized position
                float noise = combinedNoise(localPos);

                // Combine base shape and noise
                float rock = sphereBase + noise;

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
                float t = rockSDF(ro);

                for (int i = 0; i < 640; i++) // Number of steps
                {
                    hitPoint = ro + rd * t;

                    float dist = rockSDF(hitPoint);

                    if (dist < t * 0.01) // Hit threshold
                        return t;

                    if (t > 2000.0) // Far plane
                        return -1.0;

                    t += rockSDF(hitPoint);
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
