Shader "CustomRenderTexture/InstancedSatellite"
{
   Properties {
        _Color ("Color", Color) = (1, 1, 0, 1) // Yellow for satellites
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
                float heading;  
                float altitude;
            };

            StructuredBuffer<SatData> _SatelliteDataBuffer;
            float4x4 _GlobeMatrix;
            float4 _Color;
            float _SatScale;

            struct appdata {
                float4 vertex : POSITION;
                uint id : SV_InstanceID;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;

                SatData data = _SatelliteDataBuffer[v.id];

                // 1. Get the direction vector from the globe center
                float2 coords = float2(data.lon, data.lat) * (3.14159265 / 180.0);
                float3 normal = longitudeLatitudeToPoint(coords);

                // 2. Build Orientation Matrix (Standard Tangent Space)
                float3 vUp = normal;
                float3 vForward = float3(0, 1, 0); 
                if (abs(dot(vUp, vForward)) > 0.99) vForward = float3(0, 0, 1);
                float3 vRight = normalize(cross(vUp, vForward));
                vForward = cross(vRight, vUp);
                
                // 3. Apply Heading (Yaw) rotation
                float angle = data.heading * (3.14159265 / 180.0);
                float s, c;
                sincos(angle, s, c);

                float3 rotatedRight = vRight * c + vForward * s;
                float3 rotatedForward = vForward * c - vRight * s;
                float3x3 orientMatrix = float3x3(rotatedRight, vUp, rotatedForward);
                
                // 4. Transform mesh locally
                float3 localMeshVertex = mul(v.vertex.xyz * _SatScale, orientMatrix);

                // 5. POSITIONING: Radius (0.5) + Altitude
                // This is the key change for satellites
                float totalRadius = 0.5 + data.altitude;
                float3 localPosInOrbit = (normal * totalRadius) + localMeshVertex;

                // 6. Final World Positioning
                float3 worldPos = mul(_GlobeMatrix, float4(localPosInOrbit, 1.0)).xyz;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}