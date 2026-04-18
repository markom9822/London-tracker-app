Shader "CustomRenderTexture/InstancedShip"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _ShipScale ("Ship Scale", Float) = 0.1 
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

            struct ShipData {
                float lat;
                float lon;
                float heading;
            };

            StructuredBuffer<ShipData> _ShipDataBuffer;
            float4x4 _GlobeMatrix;
            float4 _Color;
            float _ShipScale; // Declare scale variable

            struct appdata {
                float4 vertex : POSITION;
                uint id : SV_InstanceID;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;

                ShipData data = _ShipDataBuffer[v.id];

                // 1. Get the Surface Normal
                float2 coords = float2(data.lon, data.lat) * (3.14159265 / 180.0);
                float3 normal = longitudeLatitudeToPoint(coords);

                // 2. Build Orientation Matrix
                float3 vUp = normal;
                float3 vForward = float3(0, 1, 0); 
                if (abs(dot(vUp, vForward)) > 0.99) vForward = float3(0, 0, 1);
                float3 vRight = normalize(cross(vUp, vForward));
                vForward = cross(vRight, vUp);
                
                // Convert degrees to radians
                float angle = data.heading * (3.14159265 / 180.0);
                float s, c;
                sincos(angle, s, c);

                // Rotate vRight and vForward around vUp
                float3 rotatedRight = vRight * c + vForward * s;
                float3 rotatedForward = vForward * c - vRight * s;

                float3x3 orientMatrix = float3x3(rotatedRight, vUp, rotatedForward);
                
                // 3. Use _ShipScale here instead of hardcoded 0.05
                float3 localMeshVertex = mul(v.vertex.xyz * _ShipScale, orientMatrix);

                // 4. Place on surface
                float3 localPosOnSphere = (normal * 0.5) + localMeshVertex;

                // 5. Apply Globe Transform
                float3 worldPos = mul(_GlobeMatrix, float4(localPosOnSphere, 1.0)).xyz;

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