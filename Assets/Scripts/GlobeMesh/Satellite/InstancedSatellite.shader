Shader "CustomRenderTexture/InstancedSatellite"
{
   Properties {
        _SatScale ("Satellite Scale", Float) = 0.05
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "Assets/Scripts/ShaderCommon/GeoMath.hlsl"

            struct SatData {
                float lat;
                float lon;
                float heading; // We use this as our Color Index
                float altitude;
            };

            StructuredBuffer<SatData> _SatelliteDataBuffer;
            float4x4 _GlobeMatrix;
            
            // Array to hold the colors for each category
            uniform float4 _CategoryColors[4]; 
            float _SatScale;

            struct appdata {
                float4 vertex : POSITION;
                uint id : SV_InstanceID;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR; // Pass color to fragment shader
            };

            v2f vert (appdata v) {
                v2f o;
                SatData data = _SatelliteDataBuffer[v.id];

                // --- NEW COLOR LOGIC ---
                // Pick the color based on the heading (index)
                int colorIdx = (int)data.heading;
                o.color = _CategoryColors[colorIdx];
                // -----------------------

                float2 coords = float2(data.lon, data.lat) * (3.14159265 / 180.0);
                float3 normal = longitudeLatitudeToPoint(coords);

                float3 vUp = normal;
                float3 vForward = float3(0, 1, 0); 
                if (abs(dot(vUp, vForward)) > 0.99) vForward = float3(0, 0, 1);
                float3 vRight = normalize(cross(vUp, vForward));
                vForward = cross(vRight, vUp);
                
                // We no longer rotate by heading because heading is our color index
                // If you want rotation AND color, you'd need to add a new field to SatData
                float3x3 orientMatrix = float3x3(vRight, vUp, vForward);
                
                float3 localMeshVertex = mul(v.vertex.xyz * _SatScale, orientMatrix);
                float totalRadius = 0.5 + data.altitude;
                float3 localPosInOrbit = (normal * totalRadius) + localMeshVertex;
                float3 worldPos = mul(_GlobeMatrix, float4(localPosInOrbit, 1.0)).xyz;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return i.color; // Use the per-instance color
            }
            ENDCG
        }
    }
}