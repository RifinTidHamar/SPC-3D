Shader "Unlit/GLCL"
//sets an RGB coordinate based off 4 values
{
	Properties
	{
		//arbitrary default positions
		_Pos0("Pos0", Vector) = (0,0,0,0)
		_Pos1("Pos1", Vector) = (0,1,1,0)
		_Pos2("Pos2", Vector) = (0,1,1,0)
		_Pos3("Pos3", Vector) = (1,2,0,0)
		_Pos4("Pos4", Vector) = (1,2,0,0)
		_C("C", Vector) = (1,1,1,1)
		_Transparency("Transparency", Range(0, 1)) = 0.5
	}

		SubShader
	{

		Pass
		{
			Ztest Always
			Tags {"LightMode" = "UniversalForward"}
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 100

			HLSLPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			// include file that contains UnityObjectToWorldNormal helper function
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0

			float _Transparency;
			float4 _Pos0;
			float4 _Pos1;
			float4 _Pos2;
			float4 _Pos3;
			float4 _Pos4;
			float4 _C;
			static float3 fadedCol = 0.2;
			bool faded;
			static float3 col = float3(1, 1, 1);



			struct v2g
			{
				float4 vertex : SV_POSITION;
				float3 color : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float3 color : TEXCOORD0;
			};

			v2g vert(appdata_full v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.color = 0;
				return o;
			}

			//sets an RGB coordinate based off 4 values
			[maxvertexcount(18)]
			void geom(point v2g IN[1], inout LineStream <g2f> vert)
			{
				g2f o[18];
				o[0].vertex = UnityObjectToClipPos(_Pos0);
				o[0].color = faded ? fadedCol : col;
				vert.Append(o[0]);
				o[1].vertex = UnityObjectToClipPos(_Pos1);
				o[1].color = faded ? fadedCol : col;
				vert.Append(o[1]);
				vert.RestartStrip();

				o[2].vertex = UnityObjectToClipPos(_Pos1);
				o[2].color = faded ? fadedCol : col;
				vert.Append(o[2]);
				o[3].vertex = UnityObjectToClipPos(_Pos2);
				o[3].color = faded ? fadedCol : col;
				vert.Append(o[3]);
				vert.RestartStrip();

				o[4].vertex = UnityObjectToClipPos(_Pos2);
				o[4].color = faded ? fadedCol : col;
				vert.Append(o[4]);
				o[5].vertex = UnityObjectToClipPos(_Pos3);
				o[5].color = faded ? fadedCol : col;
				vert.Append(o[5]);
				vert.RestartStrip();

				o[6].vertex = UnityObjectToClipPos(_Pos3);
				o[6].color = faded ? fadedCol : col;
				vert.Append(o[6]);
				o[7].vertex = UnityObjectToClipPos(_Pos4);
				o[7].color = faded ? fadedCol : col;
				vert.Append(o[7]);
				vert.RestartStrip();
			}

			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col;
				col.rgb = i.color.rgb;
				col.a = _Transparency;
				return col;
			}
			ENDHLSL
		}
	}
}

