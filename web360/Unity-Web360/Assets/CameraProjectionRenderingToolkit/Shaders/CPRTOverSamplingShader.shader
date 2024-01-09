

Shader "CPRT/OverSamplingShader" {
	Properties{
		_MainTex("Base (RGB)",2D)="" {}
	}


// Shader code pasted into all further CGPROGRAM blocks
CGINCLUDE
#include "UnityCG.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	sampler2D _MainTex;

	float SampleRatio;
	float2 SampleSize;
	float SubSampleDistance;





	v2f vert(appdata_img v) {
		v2f o;

		o.pos=v.vertex;
		o.pos.xy=o.pos.xy*2.0f-1.0f;
		o.uv=v.texcoord.xy;

		return o;
	}



	half4 fragOneSampleLinear(v2f i):SV_Target{
		return tex2D(_MainTex,i.uv);
	}
	half4 fragFourSamplesDistorded(v2f i):SV_Target{
		return tex2D(_MainTex,i.uv);
	}
	half4 fragFourSamplesBilinear(v2f i): SV_Target{
		float2 coords=i.uv;
		float2 dcdx=ddx(coords)*SubSampleDistance*0.33333333f;
		float2 dcdy=ddy(coords)*SubSampleDistance*0.33333333f;
		float4 color;

		color=	tex2D(_MainTex,coords-dcdx-dcdy)+
				tex2D(_MainTex,coords+dcdx-dcdy)+
				tex2D(_MainTex,coords-dcdx+dcdy)+
				tex2D(_MainTex,coords+dcdx+dcdy);

		return color*0.25f;
	}
ENDCG

	Subshader {
		Pass{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragOneSampleLinear
			ENDCG
		}
		Pass{
			ZTest Always Cull Off ZWrite Off //Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragFourSamplesDistorded
			ENDCG
		}
		Pass{
			ZTest Always Cull Off ZWrite Off //Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragFourSamplesDistorded
			ENDCG
		}
		Pass{
			ZTest Always Cull Off ZWrite Off //Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragFourSamplesBilinear
			ENDCG
		}
	}

	Fallback off

} // shader


