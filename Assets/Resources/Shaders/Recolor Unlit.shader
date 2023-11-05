Shader "Ancible/Recolor"
{
	Properties
	{
		_MainTex("Sprite", 2D) = "white" {}
		_OriginColor("Origin Color", Color) = (1,1,1,1)
		_ChangeColor("Change Color", Color) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		

	}
		SubShader
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Transparent+1"
			}


			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
			ColorMask[_ColorMask]

			Pass
			{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON

			sampler2D _MainTex;
			float4 _OriginColor;
			float4 _ChangeColor;

			struct Vertex
			{
				float4 vertex : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct Fragment
			{
				float4 vertex : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			Fragment vert(Vertex v)
			{
				Fragment o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex = v.uv_MainTex;
				o.uv2 = v.uv2;

				return o;
			}


			float4 frag(Fragment IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv_MainTex);
				if (c.a == 1 && abs(c.r - _OriginColor.r) < .1f && abs(c.b - _OriginColor.b) < .1f && abs(c.g - _OriginColor.g) < .1f)
				{
					return _ChangeColor;
				}
				return c;
			}
				ENDCG
			}
		}
}