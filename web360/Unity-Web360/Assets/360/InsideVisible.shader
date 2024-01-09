Shader "InsideVisible" {
    Properties{
        _MainCube("Base Cube", Cube) = "" {}           // Cubemap principale
        _SecondCube("Second Cube", Cube) = "" {}       // Secondo cubemap
        _Slide("Transition Slide", Range(0, 1)) = 0    // Valore di transizione tra i due cubemap
        _StretchAmount("Stretch Amount", Range(0, 10)) = 1    // Quantità di stretch dell'immagine
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            Cull Front

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                samplerCUBE _MainCube;
                samplerCUBE _SecondCube;
                float _Slide;
                float _StretchAmount;

                v2f vert(appdata_t v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    o.normal = v.normal;

                    // Applica lo stretch dell'immagine alle coordinate della texture
                    float2 stretchedTexCoord = v.texcoord * float2(_StretchAmount, 1); // Stretch orizzontale, puoi modificare i valori per uno stretch verticale
                    o.texcoord = stretchedTexCoord;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    float3 normal = i.normal;

                    float4 col1 = texCUBE(_MainCube, normal); // Colore dal cubemap principale
                    float4 col2 = texCUBE(_SecondCube, normal); // Colore dal secondo cubemap

                    float transition = smoothstep(0, 1, _Slide); // Valore di transizione graduale tra le due texture
                    float4 finalColor = lerp(col1, col2, transition); // Lerp tra i colori dei due cubemap in base al valore di transizione

                    return finalColor; // Colore finale del pixel
                }
                ENDCG
            }
        }
}
