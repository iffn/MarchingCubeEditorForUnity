Shader "Custom/HeightmapTessellationDiscard"
{
    Properties
    {
        _MainTex ("Heightmap", 2D) = "white" {}    // Heightmap texture
        _Height ("Height Scale", Float) = 1.0     // Displacement intensity
        _Tessellation ("Tessellation Level", Range(1, 32)) = 8.0  // Tessellation amount
        _ColorLow ("Low Color", Color) = (0,0.3,0,1)  // Color at low heights
        _ColorHigh ("High Color", Color) = (1,1,1,1)  // Color at high heights
        _Contrast ("Height Contrast", Float) = 1.0   // Contrast adjustment
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessEdge addshadow
        #pragma target 4.6

        sampler2D _MainTex;
        float _Height;
        float _Tessellation;
        float _Contrast;
        fixed4 _ColorLow;
        fixed4 _ColorHigh;
        float _Smoothness;
        float _Metallic;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Tessellation function
        float tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return _Tessellation;  // Uniform tessellation
        }

        // Vertex shader: Displacement
        void vert(inout appdata_full v)
        {
            float2 uv = v.texcoord;
            float heightValue = tex2Dlod(_MainTex, float4(uv, 0, 0)).r;

            // Apply contrast adjustment
            heightValue = (heightValue - 0.5) * _Contrast + 0.5;
            heightValue = saturate(heightValue);

            // Displace vertices
            v.vertex.xyz += v.normal * heightValue * _Height;
        }

        // Surface shader: Discard pixels where heightmap is 0
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Sample heightmap
            float heightValue = tex2D(_MainTex, IN.uv_MainTex).r;

            // Apply contrast adjustment
            heightValue = (heightValue - 0.5) * _Contrast + 0.5;
            heightValue = saturate(heightValue);

            // Discard pixels where heightValue is effectively 0
            clip(heightValue > 0.01 ? 1 : -1);

            // Interpolate between low and high colors based on height
            o.Albedo = lerp(_ColorLow.rgb, _ColorHigh.rgb, heightValue);

            // Add smoothness and metallic properties
            o.Smoothness = _Smoothness;
            o.Metallic = _Metallic;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
