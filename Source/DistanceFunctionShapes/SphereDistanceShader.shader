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

            float rockSDF(float3 p)
            {
                // Base shape: a distorted sphere
                float sphereBase = length(p) - 1.0;

                // Add bumpy surface with noise
                float noise = sin(p.x * 5.0) * sin(p.y * 5.0) * sin(p.z * 5.0); // Basic procedural noise
                noise *= 0.2; // Scale noise amplitude

                // Combine the sphere with the noise
                float rock = sphereBase + noise;

                return rock;
            }

            // Estimate normals via finite differences
            float3 estimateNormal(float3 p)
            {
                float d = sphereSDF(p, _SphereRadius);
                float epsilon = 0.001; // Small offset for normal estimation
                float3 n = float3(
                    sphereSDF(p + float3(epsilon, 0, 0), _SphereRadius) - d,
                    sphereSDF(p + float3(0, epsilon, 0), _SphereRadius) - d,
                    sphereSDF(p + float3(0, 0, epsilon), _SphereRadius) - d
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
                    float dist = sphereSDF(hitPoint, _SphereRadius);
                    //float dist = rockSDF(hitPoint);
                    if (dist < 0.001) // Hit threshold
                        return t;
                    t += dist;
                    if (t > 10.0) // Far plane
                        break;
                }
                return -1.0; // No hit
            }

            fixed4 frag (v2f i) : SV_Target
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

                    // Compute depth manually
                    float4 clipPos = UnityWorldToClipPos(hitPoint);

                    // Output color and depth
                    return fixed4(_Color.rgb * shading, 1.0); // Apply shading
                }

                // Clip pixels outside the sphere
                clip(-1.0); // Discard this fragment
                return fixed4(0, 0, 0, 0); // Should never reach here
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
