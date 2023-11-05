Shader "Ancible/GuyRecolor"
{
	Properties
	{
		_MainTex("Sprite", 2D) = "white" {}
		_OriginHairColorOne("Origin Hair Color 1", Color) = (1,1,1,1)
		_OriginHairColorTwo("Origin Hair Color 2", Color) = (1,1,1,1)
		_OriginEyeColor("Origin Eye Color", Color) = (1,1,1,1)
		_OriginPrimaryShirtColorOne("Origin Primary Shirt Color 1", Color) = (1,1,1,1)
		_OriginPrimaryShirtColorTwo("Origin Primary Shirt Color 2", Color) = (1,1,1,1)
		_OriginPrimaryShirtColorThree("Origin Primary Shirt Color 3", Color) = (1,1,1,1)
		_OriginSecondaryShirtColorOne("Origin Secondary Shirt Color 1", Color) = (1,1,1,1)
		_OriginSecondaryShirtColorTwo("Origin Secondary Shirt Color 2", Color) = (1,1,1,1)
		_OriginSecondaryShirtColorThree("Origin Secondary Shirt Color 2", Color) = (1,1,1,1)
		_HairColorOne("Hair Color 1", Color) = (1,1,1,1)
		_HairColorTwo("Hair Color 2", Color) = (1,1,1,1)
		_EyeColor("Eye Color", Color) = (1,1,1,1)
		_PrimaryShirtColorOne("Primary Shirt Color 1", Color) = (1,1,1,1)
		_PrimaryShirtColorTwo("Primary Shirt Color 2", Color) = (1,1,1,1)
		_PrimaryShirtColorThree("Primary Shirt Color 3", Color) = (1,1,1,1)
		_SecondaryShirtColorOne("Secondary Shirt Color 1", Color) = (1,1,1,1)
		_SecondaryShirtColorTwo("Secondary Shirt Color 2", Color) = (1,1,1,1)
		_SecondaryShirtColorThree("Secondary Shirt Color 3", Color) = (1,1,1,1)
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

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON

			sampler2D _MainTex;
			float4 _OriginHairColorOne;
			float4 _OriginHairColorTwo;
			float4 _OriginEyeColor;
			float4 _OriginPrimaryShirtColorOne;
			float4 _OriginPrimaryShirtColorTwo;
			float4 _OriginPrimaryShirtColorThree;
			float4 _OriginSecondaryShirtColorOne;
			float4 _OriginSecondaryShirtColorTwo;
			float4 _OriginSecondaryShirtColorThree;
			float4 _HairColorOne;
			float4 _HairColorTwo;
			float4 _PrimaryShirtColorOne;
			float4 _PrimaryShirtColorTwo;
			float4 _PrimaryShirtColorThree;
			float4 _SecondaryShirtColorOne;
			float4 _SecondaryShirtColorTwo;
			float4 _SecondaryShirtColorThree;
			float4 _EyeColor;

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
				if (c.a == 1)
				{
					if (abs(c.r - _OriginHairColorOne.r) < .1f && abs(c.b - _OriginHairColorOne.b) < .1f && abs(c.g - _OriginHairColorOne.g) < .1f)
					{
						return _HairColorOne;
					}
					if (abs(c.r - _OriginHairColorTwo.r) < .1f && abs(c.b - _OriginHairColorTwo.b) < .1f && abs(c.g - _OriginHairColorTwo.g) < .1f)
					{
						return _HairColorTwo;
					}
					if (abs(c.r - _OriginEyeColor.r) < .1f && abs(c.b - _OriginEyeColor.b) < .1f && abs(c.g - _OriginEyeColor.g) < .1f)
					{
						return _EyeColor;
					}
					if (abs(c.r - _OriginPrimaryShirtColorOne.r) < .1f && abs(c.b - _OriginPrimaryShirtColorOne.b) < .1f && abs(c.g - _OriginPrimaryShirtColorOne.g) < .1f)
					{
						return _PrimaryShirtColorOne;
					}
					if (abs(c.r - _OriginPrimaryShirtColorTwo.r) < .1f && abs(c.b - _OriginPrimaryShirtColorTwo.b) < .1f && abs(c.g - _OriginPrimaryShirtColorTwo.g) < .1f)
					{
						return _PrimaryShirtColorTwo;
					}
					if (abs(c.r - _OriginPrimaryShirtColorThree.r) < .1f && abs(c.b - _OriginPrimaryShirtColorThree.b) < .1f && abs(c.g - _OriginPrimaryShirtColorThree.g) < .1f)
					{
						return _PrimaryShirtColorThree;
					}
					if (abs(c.r - _OriginSecondaryShirtColorOne.r) < .1f && abs(c.b - _OriginSecondaryShirtColorOne.b) < .1f && abs(c.g - _OriginSecondaryShirtColorOne.g) < .1f)
					{
						return _SecondaryShirtColorOne;
					}
					if (abs(c.r - _OriginSecondaryShirtColorTwo.r) < .1f && abs(c.b - _OriginSecondaryShirtColorTwo.b) < .1f && abs(c.g - _OriginSecondaryShirtColorTwo.g) < .1f)
					{
						return _SecondaryShirtColorTwo;
					}
					if (abs(c.r - _OriginSecondaryShirtColorThree.r) < .1f && abs(c.b - _OriginSecondaryShirtColorThree.b) < .1f && abs(c.g - _OriginSecondaryShirtColorThree.g) < .1f)
					{
						return _SecondaryShirtColorThree;
					}
				}
				
				return c;
			}
				ENDCG
			}
		}
}