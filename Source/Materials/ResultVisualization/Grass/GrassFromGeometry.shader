Shader "VoxelMesh/GrassFromArea"
{
    Properties
    {
        _MainTex ("Grass Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _GrassHeight ("Grass Height", Range(0.1, 1)) = 0.3
        _GrassWidth ("Grass Width", Range(0.01, 0.3)) = 0.05
        _GrassDensityPerSquareMeter ("Grass Density per m²", Range(0, 1000)) = 100
        _WindSwayFrequency ("Wind Sway Frequency", Range(0, 5)) = 1.0
        _WindStrength ("Wind Strength", Range(0, 0.5)) = 0.1
        _CullDistance ("Grass Cull Distance", Float) = 50
        _SlopeThreshold ("Slope Threshold (Y Dot)", Range(0,1)) = 0.3
        _MaxBladesPerTriangle ("Max Blades Per Triangle", Range(1, 20)) = 6
        _ChunkPosition ("Chunk World Position", Vector) = (0,0,0,0)
        _GrassColor ("Grass Color", Color) = (0.3, 0.7, 0.3, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        Cull off
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _GrassTintMap;
            float _Cutoff;
            float _GrassHeight;
            float _GrassWidth;
            float _GrassDensityPerSquareMeter;
            float _WindSwayFrequency;
            float _WindStrength;
            float _CullDistance;
            float _SlopeThreshold;
            float4 _ChunkPosition;
            float4 _GrassColor;
            int _MaxBladesPerTriangle;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2g
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2g vert (appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            static const int MAX_BLADES = 20;

            [maxvertexcount(MAX_BLADES * 6)] // blades max * 6 vertices per quad
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                // World-space triangle corners
                float3 A = input[0].worldPos;
                float3 B = input[1].worldPos;
                float3 C = input[2].worldPos;

                // Triangle normal and slope
                float3 edge1 = B - A;
                float3 edge2 = C - A;
                float3 triNormal = normalize(cross(edge1, edge2));

                // Skip steep triangles (slope too high)
                float slope = dot(triNormal, float3(0, 1, 0));
                if (slope < _SlopeThreshold) return;

                // Camera distance culling
                float3 triCenter = (A + B + C) / 3.0;
                float dist = distance(triCenter, _WorldSpaceCameraPos);
                if (dist > _CullDistance) return;

                // Estimate world-space triangle area
                float area = 0.5 * length(cross(edge1, edge2));

                // Compute how many blades to emit for this triangle
                float expectedBlades = area * _GrassDensityPerSquareMeter;
                int bladeCount = min((int)(expectedBlades + 0.5), _MaxBladesPerTriangle);
                bladeCount = min(bladeCount, MAX_BLADES);
                if (bladeCount <= 0) return;

                // Grass orientation: up and right vectors
                float3 up = float3(0, 1, 0);

                for (int i = 0; i < bladeCount; i++)
                {
                    // Generate random barycentric coords inside triangle
                    float2 rand = float2(
                        Hash(triCenter.xz + i * 17.3),
                        Hash(triCenter.xz + i * 23.1 + 5.0)
                    );

                    if (rand.x + rand.y > 1.0) {
                        rand = 1.0 - rand;
                    }

                    // Convert to world-space position
                    float3 pos = rand.x * A + rand.y * B + (1.0 - rand.x - rand.y) * C;

                    // Create a rotation angle per blade (based on blade index)
                    float randAngle = Hash(pos.xz + i * 11.123) * 6.2831853; // 0 to 2*PI

                    // Build rotation using sin/cos
                    float cosA = cos(randAngle);
                    float sinA = sin(randAngle);

                    // Basis vectors for rotation around Y (up)
                    float3 localRight = float3(cosA, 0, -sinA);
                    float3 localForward = float3(sinA, 0, cosA);

                    // Final rotated right vector
                    float3 right = localRight;

                    // Wind sway on top point
                    float sway = sin(_Time.y * _WindSwayFrequency + dot(pos.xz, float2(0.1, 0.1))) * _WindStrength;
                    float3 topPos = pos + up * _GrassHeight + right * sway;

                    // Compute grass quad corners
                    float3 v0 = pos - right * _GrassWidth;
                    float3 v1 = pos + right * _GrassWidth;
                    float3 v2 = topPos + right * _GrassWidth;
                    float3 v3 = topPos - right * _GrassWidth;
                    
                    // Emit first triangle of quad
                    g2f o0; o0.uv = float2(0, 0); o0.pos = UnityWorldToClipPos(v0); triStream.Append(o0);
                    g2f o1; o1.uv = float2(1, 0); o1.pos = UnityWorldToClipPos(v1); triStream.Append(o1);
                    g2f o2; o2.uv = float2(1, 1); o2.pos = UnityWorldToClipPos(v2); triStream.Append(o2);
                    
                    triStream.RestartStrip();
                    
                    // Emit second triangle of quad
                    g2f o3; o3.uv = float2(1, 1); o3.pos = UnityWorldToClipPos(v2); triStream.Append(o3);
                    g2f o4; o4.uv = float2(0, 1); o4.pos = UnityWorldToClipPos(v3); triStream.Append(o4);
                    g2f o5; o5.uv = float2(0, 0); o5.pos = UnityWorldToClipPos(v0); triStream.Append(o5);
                    
                    triStream.RestartStrip();
                }
            }


            fixed4 frag(g2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float grayscale = tex.r;

                float3 finalColor = _GrassColor.rgb * grayscale;

                clip(tex.a - _Cutoff);
                return float4(finalColor, tex.a);
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
