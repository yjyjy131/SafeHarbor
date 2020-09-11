// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/BackGroundGradient" {

	Properties{

		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		_Color("Begin Color", Color) = (1,1,1,1)

		_Color2("End Color", Color) = (1,1,1,1)

	}



		SubShader{

			Tags {

				"Queue" = "Background"

				"IgnoreProjector" = "True"

			}



			LOD 100



			ZWrite On



			Pass {

				CGPROGRAM



				#pragma vertex vert  

				#pragma fragment frag

				#include "UnityCG.cginc"



				fixed4 _Color;

				fixed4 _Color2;



				struct v2f {

					float4 pos : SV_POSITION;

					fixed4 col : COLOR;

				};



				v2f vert(appdata_full v) {

					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);

					o.col = lerp(_Color, _Color2, v.texcoord.y);

					return o;

				}



				float4 frag(v2f i) : COLOR {

					float4 c = i.col;

					c.a = 1;

					return c;

				}



				ENDCG

			}

		}

}