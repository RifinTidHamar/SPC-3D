Shader "Unlit/HeightPlane"
//makes a square of connected dots based off their four positions
{
	Properties
	{
		_Pos0("Pos0", Float) = -2.836596
		_Pos1("Pos1", Float) = -2.836596
		_Pos2("Pos2", Float) = -2.836596
		_Pos3("Pos3", Float) = -2.836596
		_Transparency("Transparency", Range(0, 1)) = 0.5
		_Color("Color", Color) = (0, 1, 0.73, 1)
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
			float _Transparency;
			float4 _Color;
			float _Pos0;
			float _Pos1;
			float _Pos2;
			float _Pos3;


			struct v2g
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;

			};

			v2g vert(appdata_full v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.color = 0;
				return o;
			}

			//makes a square of connected dots based off their four positions
			[maxvertexcount(6)]
			void geom(point v2g IN[1], inout LineStream <g2f> vert)
			{
				g2f o[5];
				o[0].vertex = UnityObjectToClipPos(float3(-2.5, _Pos0, -2.51));
				o[0].color = _Color;// +0.5;
				vert.Append(o[0]);
				o[1].vertex = UnityObjectToClipPos(float3(2.5, _Pos1, -2.51));
				o[1].color = _Color;// -0.5;
				vert.Append(o[1]);
				o[2].vertex = UnityObjectToClipPos(float3(2.5, _Pos2, 2.51));
				o[2].color = _Color;// -0.8;
				vert.Append(o[2]);
				o[3].vertex = UnityObjectToClipPos(float3(-2.5, _Pos3, 2.51));
				o[3].color = _Color;// -0.5;
				vert.Append(o[3]);
				//makes a full connected square
				o[4].vertex = UnityObjectToClipPos(float3(-2.5, _Pos0, -2.51));
				o[4].color = _Color;// +0.5;
				vert.Append(o[4]);
				vert.RestartStrip();
			}
			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col;
				col.rgb = i.color.rgb;;
				col.a = _Transparency;
				return col;
			}
			ENDHLSL
		}
	}
}
