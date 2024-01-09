Shader "Custom/360MaterialTransition"
{
    Properties
    {
        _MainTex("Main Material", 2D) = "white" {}
        _TransitionTex("Transition Material", 2D) = "white" {}
        _TransitionProgress("Transition Progress", Range(0, 1)) = 0
        _RotationX("Texture Rotation X", Range(-180, 180)) = 0
        _OffsetX("Texture Offset X", Range(-1, 1)) = 0
        
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Cull Front // Invert normal culling for the inside of the geometry

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 normal : NORMAL;
                };

                sampler2D _MainTex;
                sampler2D _TransitionTex;
                float _TransitionProgress;
                float _RotationX;
                float _OffsetX;
                float _OffsetY;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    float2 rotatedUV = float2(v.uv.x * cos(radians(_RotationX)) - v.uv.y * sin(radians(_RotationX)),
                                              v.uv.x * sin(radians(_RotationX)) + v.uv.y * cos(radians(_RotationX))); // Rotate the texture coordinates on the x-axis
                    o.uv = rotatedUV + float2(_OffsetX, _OffsetY); // Apply texture offset
                    o.normal = -v.normal; // Invert the vertex normal
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Interpolate between the two materials based on the transition progress
                    fixed4 mainColor = lerp(tex2D(_MainTex, i.uv), tex2D(_TransitionTex, i.uv), _TransitionProgress);
                    return mainColor;
                }
                ENDCG
            }
        }
}
