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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "RaymarchedShaderStart.cginc"
            
            float _BaseScale;
            float _BaseFrequency;
            float _BaseAmplitude;
            
            float _VoronoiScale;
            float _VoronoiStrength;

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

            float combinedNoise(float3 localPos)
            {
                float returnValue = 0.0;

                // Base noise layer
                float3 basePosition = localPos * _BaseFrequency * _BaseScale;
                returnValue += sin(basePosition.x) * sin(basePosition.y) * sin(basePosition.z) * _BaseAmplitude;

                // Add Voronoi cracks
                returnValue -= voronoi(localPos * _VoronoiScale) * _VoronoiStrength;

                return returnValue;
            }

            float sphereSDF(float3 p, float radius)
            {
                return length(p) - radius;
            }

            float SDF(float3 localPosition)
            {
                // Convert world position to local position
                float3 scale;
                scale.x = length(unity_ObjectToWorld[0].xyz); // Scale along X
                scale.y = length(unity_ObjectToWorld[1].xyz); // Scale along Y
                scale.z = length(unity_ObjectToWorld[2].xyz); // Scale along Z

                // Normalize local position by scale ratios
                float3 normalizedPos = localPosition * scale / (scale.x + scale.y + scale.z) * 3;

                // Base sphere shape
                float sphereBase = sphereSDF(localPosition, 0.4);

                // Add noise to normalized position
                float noise = combinedNoise(localPosition);

                // Combine base shape and noise
                float rock = sphereBase + noise;

                return rock;
            }
            
            #include "RaymarchedShaderEnd.cginc"
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
