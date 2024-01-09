Shader "Custom/Equirectangular" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _Alpha ("Alpha", Range(0, 1)) = 1
    _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "gray" {}
    _Rotation ("Rotation", Range(0,360)) = 0
}

SubShader{
    Pass {
        Tags {"LightMode" = "Always" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma glsl
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata {
               float4 vertex : POSITION;
               float3 normal : NORMAL;
            };

            struct v2f
            {
                float4    pos : SV_POSITION;
                float3    normal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Alpha;
            float _Rotation;

            #define PI 3.141592653589793

            inline float2 RadialCoords(float3 a_coords)
            {
                float3 a_coords_n = normalize(a_coords);
                float lon = atan2(a_coords_n.z, a_coords_n.x);
                float lat = acos(a_coords_n.y);
                float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
                return float2(1 - (sphereCoords.x * 0.5 + 0.5), 1 - sphereCoords.y);
            }

            float4 frag(v2f IN) : COLOR
            {
                float2 equiUV = RadialCoords(IN.normal);
                //equiUV = (equiUV * _MainTex_ST.xy) + _MainTex_ST.zw;
                equiUV = equiUV + float2(_Rotation/360,0);
                fixed4 color = tex2D(_MainTex, equiUV) * _Color;
                color.a *= _Alpha;
                return color;
            }
        ENDCG
    }
}
}