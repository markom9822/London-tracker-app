Shader "Custom/AreaCircleShader"
{
    Properties
    {
        _MainColor ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0.0, 0.5)) = 0.4
        _Thickness ("Thickness", Range(0.0, 0.5)) = 0.1
        _Sharpness ("Sharpness", Range(0, 1)) = 0
        
        [Header(Dashes)]
        _DashCount ("Dash Count", Float) = 10
        _DashSize ("Dash Size (0 to 1)", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _MainColor;
            float _Radius;
            float _Thickness;
            float _Sharpness;
            float _DashCount;
            float _DashSize;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                
                // 1. Calculate Distance Alpha (The Ring Shape)
                float halfThickness = _Thickness * 0.5;
                float distFromRingCenter = abs(dist - _Radius);
                float edgeDelta = max(_Sharpness, 0.0001) * 0.1;
                
                float ringAlpha = 1.0 - smoothstep(halfThickness - edgeDelta, halfThickness, distFromRingCenter);
                if(_Sharpness <= 0) ringAlpha = distFromRingCenter < halfThickness ? 1.0 : 0.0;

                // 2. Calculate Angular Alpha (The Dashes)
                // atan2 returns values between -PI and PI
                float angle = atan2(uv.y, uv.x); 
                
                // Normalize angle to 0.0 - 1.0 range
                float angle01 = (angle / 6.283185) + 0.5;
                
                // Create the dash pattern using fractional part of (angle * count)
                // If the fractional part is less than _DashSize, it's visible.
                float dashPattern = frac(angle01 * _DashCount);
                float dashAlpha = step(1.0 - _DashSize, dashPattern);

                // 3. Combine them
                float finalAlpha = ringAlpha * dashAlpha;

                return fixed4(_MainColor.rgb, finalAlpha * _MainColor.a);
            }
            ENDCG
        }
    }
}