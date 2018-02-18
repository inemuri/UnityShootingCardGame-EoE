Shader "INMR/CardEffects"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}			// 主材质
		BASE ("BaseTexture", 2D) = "white" {}									// 替换主材质的基础材质
		[Toggle] USEMAIN("UseMainTexture", Float) = 1							// 是否使用主材质
		[NoScaleOffset] MASK ("Mask", 2D) = "white" {}							// 遮罩(R通道用于波纹  G通道用于光效1  B通道用于光效2)
		[Toggle] APPLYALL("EnableEffects", Float) = 1							// 整体特效开关
		[Toggle] USEUNSCALEDTIME("UseUnscaledTime(NeedScript)", Float) = 0		// 是否无视时间控制（需要脚本辅助）
		[HideInInspector] UNSCALEDTIME("UseUnscaledTime", Float) = 0
		[Space(30)]

		[Header(Wave (R))]
		[NoScaleOffset] WAVE ("■ WaveTexture", 2D) = "grey" {}		// 波纹材质
		WSTR ("■ WaveStrength", Range(0, 0.2)) = 0					// 强度
		WMOVEX ("■ WaveMotion X", float) = 0						// 卷动速度X
		WMOVEY ("■ WaveMotion Y", float) = 0						// 卷动速度Y
		[Toggle] APPLYBASE("■ ApplyOnBase", Float) = 1				// 应用于基础
		[Toggle] APPLYWAVE1("■ ApplyOnGlow1", Float) = 0			// 应用于光效1
		[Toggle] APPLYWAVE2("■ ApplyOnGlow2", Float) = 0			// 应用于光效2
		[Space(30)]

		[Header(Glow1st (G))]
		[NoScaleOffset] GLOW1 ("◎ GlowTexture", 2D) = "grey" {}	// 光效材质1
		GCOLOR1 ("◎ TintColor", Color) = (1,1,1,1)					// 色调
		GSTR1 ("◎ Strength (1=Replace)", Range(-2, 2)) = 0			// 强度(负数变暗  正数变亮)
		GMOVEX1 ("◎ MotionSpeedX", float) = 0						// 卷动速度X
		GMOVEY1 ("◎ MotionSpeedY", float) = 0						// 卷动速度Y
		GSCALEX1 ("◎ ScaleX", float) = 1							// 缩放X
		GSCALEY1 ("◎ ScaleY", float) = 1							// 缩放Y
		GSOFFSETX1 ("◎ OffsetX", float) = 0						// 固定偏移X
		GSOFFSETY1 ("◎ OffsetY", float) = 0						// 固定偏移Y
		GROTATION1 ("◎ Rotation", float) = 0						// 旋转速度
		[Space(15)]
		[Toggle] APPLYEX1("---◎ ApplyExMotion", Float) = 0			// 启用额外偏移效果
		[NoScaleOffset] EXT1 ("---◎ ExTexture", 2D) = "grey" {}	// 额外偏移材质
		EXM1 ("---◎ ExSpeed", float) = 0							// 额外偏移卷动速度
		[Space(30)]

		[Header(Glow2nd (B))]
		[NoScaleOffset] GLOW2("★ GlowTexture", 2D) = "grey" {}		// 光效材质2
		GCOLOR2("★ TintColor", Color) = (1,1,1,1)
		GSTR2("★ Strength (1=Replace)", Range(-2, 2)) = 0
		GMOVEX2("★ MotionX", float) = 0
		GMOVEY2("★ MotionY", float) = 0
		GSCALEX2("★ ScaleX", float) = 1
		GSCALEY2("★ ScaleY", float) = 1
		GSOFFSETX2("★ OffsetX", float) = 0
		GSOFFSETY2("★ OffsetY", float) = 0
		GROTATION2("★ Rotation", float) = 0
		[Space(15)]
		[Toggle] APPLYEX2("---★ ApplyExMotion", Float) = 0
		[NoScaleOffset] EXT2 ("---★ ExTexture", 2D) = "grey" {}
		EXM2 ("---★ ExSpeed", float) = 0

	}
	SubShader
	{
		Cull Back
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "PreviewType" = "Plane" "IgnoreProjector" = "true" }
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 guv1 : TEXCOORD1; // 光效1
				float2 guv2 : TEXCOORD2; // 光效2
			};

			float4 BASE_ST;
			sampler2D _MainTex, BASE, MASK, WAVE, GLOW1, GLOW2, EXT1, EXT2;
			fixed WMOVEX, WMOVEY, GMOVEX1, GMOVEY1, GMOVEX2, GMOVEY2, WSTR, GSTR1, GSTR2;
			half4 GCOLOR1, GCOLOR2;
			float USEMAIN, APPLYALL, APPLYBASE, APPLYEX1, APPLYEX2, USEUNSCALEDTIME, UNSCALEDTIME;
			float GROTATION1, GSCALEX1, GSCALEY1, GSOFFSETX1, GSOFFSETY1, APPLYWAVE1, EXM1;
			float GROTATION2, GSCALEX2, GSCALEY2, GSOFFSETX2, GSOFFSETY2, APPLYWAVE2, EXM2;
			fixed2 waveResultBase, waveResultGlow1, waveResultGlow2;

			v2f vert (appdata v)
			{	
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, BASE);

				// 光效缩放偏移旋转
				float2x2 rotationMatrix1, rotationMatrix2;
				float sinTheta1, sinTheta2;
				float cosTheta1, cosTheta2;
				
				if (USEUNSCALEDTIME) {
					_Time = UNSCALEDTIME;
				}

				sinTheta1 = sin(GROTATION1 * _Time);
				cosTheta1 = cos(GROTATION1 * _Time);
				rotationMatrix1 = float2x2(cosTheta1, -sinTheta1, sinTheta1, cosTheta1);
				o.guv1 = mul((o.uv - float2(0.5 + GSOFFSETX1, 0.5 + GSOFFSETY1)) * float2(GSCALEX1, GSCALEY1), rotationMatrix1);

				sinTheta2 = sin(GROTATION2 * _Time);
				cosTheta2 = cos(GROTATION2 * _Time);
				rotationMatrix2 = float2x2(cosTheta2, -sinTheta2, sinTheta2, cosTheta2);
				o.guv2 = mul((o.uv - float2(0.5 + GSOFFSETX2, 0.5 + GSOFFSETY2)) * float2(GSCALEX2, GSCALEY2), rotationMatrix2);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				if (USEUNSCALEDTIME) {
					_Time = UNSCALEDTIME;
				}
				// 遮罩
				fixed4 mask = tex2D(MASK, i.uv);
				// 波纹处理
				fixed2 waveMove = fixed2(i.uv.x + _Time.x * WMOVEX, i.uv.y + _Time.x * WMOVEY);
				fixed2 wave = (tex2D(WAVE, waveMove) * 2 - 1) * WSTR * mask.r;
				if (APPLYBASE) {
					waveResultBase = wave;
				}
				if (APPLYWAVE1) {
					waveResultGlow1 = wave;
				}
				if (APPLYWAVE2) {
					waveResultGlow2 = wave;
				}
				// 应用缩放偏移旋转
				fixed2 glowMove1 = fixed2(i.guv1.x + 0.5 + _Time.x * GMOVEX1, i.guv1.y + 0.5 + _Time.x * GMOVEY1);
				fixed2 glowMove2 = fixed2(i.guv2.x + 0.5 + _Time.x * GMOVEX2, i.guv2.y + 0.5 + _Time.x * GMOVEY2);
				// 是否使用额外偏移效果
				fixed4 glowResult1, glowResult2;
				if (APPLYEX1) {
					glowResult1 = tex2D(EXT1, glowMove1);
					glowResult1.y -= _Time.x * EXM1;
					glowResult1 = (tex2D(GLOW1, glowResult1.xy + waveResultGlow1)) * glowResult1.a * GCOLOR1 * GSTR1 * mask.g;
				} else {
					glowResult1 = (tex2D(GLOW1, glowMove1 + waveResultGlow1)) * GCOLOR1 * GSTR1 * mask.g;
				}
				if (APPLYEX2) {
					glowResult2 = tex2D(EXT2, glowMove2);
					glowResult2.y -= _Time.x * EXM1;
					glowResult2 = (tex2D(GLOW2, glowResult2.xy + waveResultGlow2)) * glowResult2.a * GCOLOR2 * GSTR2 * mask.b;
				} else {
					glowResult2 = (tex2D(GLOW2, glowMove2 + waveResultGlow2)) * GCOLOR2 * GSTR2 * mask.b;
				}
				// 结果汇总 处理光效强度为1时的替换
				fixed4 result;
				if (USEMAIN){
					result = tex2D(_MainTex, i.uv + waveResultBase);
				} else {
					result = tex2D(BASE, i.uv + waveResultBase);
				}
				if (GSTR1 == 1) {
					result.rgb = glowResult1.rgb + (result.rgb * (1 - glowResult1.a));
				} else {
					result.rgb += glowResult1.rgb;
				}
				if (GSTR2 == 1) {
					result.rgb = glowResult2.rgb + (result.rgb * (1 - glowResult2.a));
				} else {
					result.rgb += glowResult2.rgb;
				}
				
				if (APPLYALL == 0) {
					if (USEMAIN){
						result = tex2D(_MainTex, i.uv);
					} else {
						result = tex2D(BASE, i.uv);
					}
				}
				return result;
			}
			ENDCG
		}
	}
}
