Shader "CustomRenderTexture/InstancedShip"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5 // Required for StructuredBuffers
            #include "UnityCG.cginc"
            #include "Assets/Scripts/ShaderCommon/GeoMath.hlsl"

            struct ShipData {
                float lat;
                float lon;
            };

            // This is the GPU buffer containing all ship positions
            StructuredBuffer<ShipData> _ShipDataBuffer;
            float4x4 _GlobeMatrix;
            float4 _Color;

            struct appdata {
                float4 vertex : POSITION;
                uint id : SV_InstanceID; // Use the built-in Instance ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;

                // Grab the data for THIS specific instance from the buffer
                ShipData data = _ShipDataBuffer[v.id];

                float2 coords = float2(data.lon, data.lat) * (3.14159265 / 180.0);
                float3 unitVector = longitudeLatitudeToPoint(coords);

                float3 localSpherePos = unitVector * 0.505; 
                float3 worldPosOnSurface = mul(_GlobeMatrix, float4(localSpherePos, 1.0)).xyz;
                
                float3 shipVertex = v.vertex.xyz * 0.1; 
                float3 finalWorldPos = worldPosOnSurface + shipVertex;

                o.pos = mul(UNITY_MATRIX_VP, float4(finalWorldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}
