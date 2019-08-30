#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;

float _WaveHeight1, _WaveHeight2, _WaveHeight3;
float _WaveSteepness1, _WaveSteepness2, _WaveSteepness3;
float4 _WaveParam1, _WaveParam2, _WaveParam3;
float4 _LightColor0;


sampler2D _FesnelTex;
sampler2D _DumpTex;
sampler2D _ReflectTex;
sampler2D _RefractTex;

float4 _Color;
float4 _WaveScale;
float4 _WaveOffset;
float _RefrectFac;
float _RefractFac;

struct waveAppData {
	float4 vertex : POSITION;
	float2 uv:TEXCOORD0;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
};

struct waveV2f {
	float4 pos : SV_POSITION;
	float2 uv:TEXCOORD0;
	float3 normal:NORMAL;
	float4 tangent : TANGENT;
	float3 lightDir : TEXCOORD1;

	float4 ref:TEXCOORD2;
	float2 bumpUv1 : TEXCOORD3;
	float2 bumpUv2 : TEXCOORD4;
	float3 viewDir:TEXCOORD5;
};

float3 GerstnerWave(float2 pos,float A,float sharp, float4 param,out float3 normal) {
	float3 WaveDir = float3(param.xy, 0);
	float waveT = param.z;
	//float w = (float)(2 * UNITY_PI / waveT);
	//下面这个效果更突出
	float w = 1 / waveT;
	half speed = param.w;
	float WA = w * A;
	float Qi = sharp / WA;
	float f = _Time.y * speed + w * dot(WaveDir, pos);
	float cosNum = cos(f);
	float sinNum = sin(f);
	float3 vertice = 0;

	vertice.x = Qi * A * WaveDir.x * cosNum;
	vertice.z = Qi * A * WaveDir.y * cosNum;
	vertice.y = sinNum * A;
	float3 P = float3(vertice.x, vertice.z, vertice.y);
	float sFun = sin(w * dot(WaveDir, P) + speed * _Time.y);
	float cFun = cos(w * dot(WaveDir, P) + speed * _Time.y);
	normal.xz = WaveDir * WA*cFun;
	normal.y = Qi * WA * sFun - 1;
	return vertice;
}

waveV2f waveVert(waveAppData i) {
	waveV2f o;
	o.uv = i.uv;
	float3 normal = 0;
	float3 calVertex = 0;
	float3 displayVertex = i.vertex.xyz;
	float3 _normal = 0;

	calVertex += GerstnerWave(i.vertex.xz, _WaveHeight1, _WaveSteepness1, _WaveParam1, normal);
	_normal += normal;
	calVertex += GerstnerWave(i.vertex.xz, _WaveHeight2, _WaveSteepness2, _WaveParam2, normal);
	_normal += normal;

	calVertex += GerstnerWave(i.vertex.xz, _WaveHeight3, _WaveSteepness3, _WaveParam3, normal);
	_normal += normal;

	displayVertex.x += calVertex.x;
	displayVertex.z += calVertex.z;
	////这样波浪更尖锐
	displayVertex.xyz += i.vertex.xyz + i.normal * calVertex.y;
	//displayVertex.y = calVertex.y;
	o.pos = UnityObjectToClipPos(displayVertex);

	o.lightDir = ObjSpaceLightDir(i.vertex);

	normal.x = -_normal.x;
	normal.y = 1 - _normal.y;
	normal.x = -_normal.z;
	o.normal = normalize(normal);

	float4 wpos = mul(unity_ObjectToWorld, i.vertex);
	float4 temp;
	temp.xyzw = wpos.xzxz* _WaveScale + _WaveOffset;
	o.bumpUv1 = temp.xy;
	o.bumpUv2 = temp.wz;
	o.ref = ComputeScreenPos(o.pos);
	o.viewDir.xzy = WorldSpaceViewDir(i.vertex);
	return o;
}

half4 waveFrag(waveV2f i) : SV_Target
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