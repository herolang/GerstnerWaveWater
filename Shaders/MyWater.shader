Shader "HL/MyWater"
{
	Properties
	{
		_Color("WaterColor",COLOR) = (1,1,1,1)
		_FesnelTex("Fesnel", 2D) = "white" {}
		//_ReflectTex("_ReflectTex", 2D) = "white" {}
		//_RefractTex("_RefractTex", 2D) = "white" {}
		_DumpTex("Normal", 2D) = "white" {}
		_RefrectFac("RefrectFac",Range(0,1)) = 0.5
		_RefractFac("RefractFac",Range(0,1)) = 0.5
	}
		SubShader
		{
			Tags { "WaterMode" = "Refractive" "RenderType" = "Opaque" }
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
								float3 normal:NORMAL;

							};

							struct v2f
							{
								float4 pos : SV_POSITION;
								float4 ref:TEXCOORD0;
								float2 bumpUv1 : TEXCOORD1;
								float2 bumpUv2 : TEXCOORD2;
								float3 normal:NORMAL;
								float3 viewDir:TEXCOORD3;
							};

							sampler2D _FesnelTex;
							sampler2D _DumpTex;
							sampler2D _ReflectTex;
							sampler2D _RefractTex;

							float4 _Color;
							float4 _WaveScale;
							float4 _WaveOffset;
							float _RefrectFac;
							float _RefractFac;

							v2f vert(appdata v)
							{
								v2f o;
								o.pos = UnityObjectToClipPos(v.vertex);
								o.normal = v.normal;
								float4 wpos = mul(unity_ObjectToWorld,v.vertex);
								float4 temp;
								temp.xyzw = wpos.xzxz* _WaveScale + _WaveOffset;
								o.bumpUv1 = temp.xy;
								o.bumpUv2 = temp.wz;
								o.ref = ComputeScreenPos(o.pos);
								o.viewDir.xzy = WorldSpaceViewDir(v.vertex);
								return o;
							}

							half4 frag(v2f i) : SV_Target
							{
								float3 bump1 = UnpackNormal(tex2D(_DumpTex,i.bumpUv1)).rgb;
								float3 bump2 = UnpackNormal(tex2D(_DumpTex, i.bumpUv2)).rgb;
								float3 bump = (bump1 + bump2)*0.5 + i.normal;
								//float3 bump = i.normal;
								float3 viewDir = normalize(i.viewDir);
								half fresnelFac = dot(viewDir,bump);
								//菲涅尔贴图，Y值越大值越大。X值没变化。
								half fesnel = UNITY_SAMPLE_1CHANNEL(_FesnelTex, float2(fresnelFac, fresnelFac));

								float4 uv1 = i.ref;
								uv1.xy += bump * _RefrectFac;
								float4 relCol = tex2Dproj(_ReflectTex, UNITY_PROJ_COORD(uv1));

								float4 uv2 = i.ref;
								uv2.xy -= bump * _RefractFac;
								float4 refraCol = tex2Dproj(_RefractTex, UNITY_PROJ_COORD(uv2))*_Color;

								half4 fincol = 0;
								fincol = lerp(refraCol, relCol, fesnel);
								return fincol;
							}
							ENDCG
						}
		}
}
