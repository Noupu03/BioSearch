Shader "BioSearch/QuantizedVerticalReveal_UIStable"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (1,1,1,1)

        _Steps    ("Steps", Range(1,256)) = 16
        _Feather  ("Feather", Range(0,0.1)) = 0.0
        _InvertY  ("InvertY", Float) = 1
        _Start    ("StartTimeSec", Float) = 0
        _Duration ("DurationSec", Float) = 0.35
        _HardCut  ("Hard Cut (1=On)", Float) = 1

        // UI ���ٽ�/Ŭ�� ǥ�� ������Ƽ (Text/TMP/RectMask�� �����ϰ� ����)
        _StencilComp     ("Stencil Comparison", Float) = 8
        _Stencil         ("Stencil ID", Float) = 0
        _StencilOp       ("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask       ("Color Mask", Float) = 15
        _UseUIAlphaClip  ("Use Alpha Clip", Float) = 0
        _ClipRect        ("Clip Rect", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags{
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // UI ���ٽ� ���������ΰ� ����
        Stencil{
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Reveal"
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1; // UI Ŭ���ο�
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _Steps, _Feather, _InvertY, _Start, _Duration, _HardCut;
            float4 _ClipRect;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;

                // �����(�ð� ���) �� ���ȭ
                float prog  = saturate( (_Time.y - _Start) / max(_Duration, 1e-4) );
                float steps = max(1.0, _Steps);
                float edge  = floor(prog * steps) / steps;

                // ����Ʒ� ����(uv.y ���)
                float y = (_InvertY > 0.5) ? (1.0 - i.texcoord.y) : i.texcoord.y;

                // �ϵ���(�⺻) �Ǵ� ����Ʈ
                float vis = (_HardCut > 0.5) ? step(y, edge)
                                             : smoothstep(edge + _Feather, edge - _Feather, y);

                c.a *= vis;

                // UGUI RectMask/Mask Ŭ����
                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                // ����Ŭ��/��ī��
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #else
                if (c.a <= 0.001) discard;
                #endif

                return c;
            }
            ENDCG
        }
    }
}
