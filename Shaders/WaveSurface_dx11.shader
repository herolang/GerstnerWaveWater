Shader "Custom/WaveSurface" {
	Properties{
		//_Color("Color", Color) = (1,1,1,1)
		//_MainTex("Albedo (RGB)", 2D) = "white" {}
		//_BumpMap("Bumpmap", 2D) = "bump" {}

		[Header(Wave Type)]
		[Toggle(ENABLE_GERSTNER)] _Gerstner("Gerstner Wave", Int) = 0

		[Header(Wave 1)]
		_WaveHeight1("Wave Height", float) = 1
		_WaveSteepness1("Wave Steepness", float) = 1
		_WaveParam1("Direction (XY), Wave Length (Y), Speed (W)", Vector) = (1, 0, 1, 1)

		[Header(Wave 2)]
		_WaveHeight2("Wave Height", float) = 1
		_WaveSteepness2("Wave Steepness", float) = 1
		_WaveParam2("Direction (XY), Wave Length (Y), Speed (W)", Vector) = (1, 0, 1, 1)

		[Header(Wave 3)]
		_WaveHeight3("Wave Height", float) = 1
		_WaveSteepness3("Wave Steepness", float) = 1
		_WaveParam3("Direction (XY), Wave Length (Y), Speed (W)", Vector) = (1, 0, 1, 1)

		_Color("WaterColor",COLOR) = (1,1,1,1)
		_FesnelTex("Fesnel", 2D) = "white" {}
		_DumpTex("Normal", 2D) = "white" {}
		_RefrectFac("RefrectFac",Range(0,1)) = 0.5
		_RefractFac("RefractFac",Range(0,1)) = 0.5
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		Pass
			{
				CGPROGRAM
				#pragma vertex waveVert
				#pragma fragment waveFrag

				//#pragma multi_compile _ ENABLE_GERSTNER
				#include "WaterWave.cginc"

				ENDCG
			}
	}
	FallBack "Diffuse"
}
