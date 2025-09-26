Shader "Custom/UVScroll"
{
	Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Tint     ("Tint (RGB)", Color) = (1,1,1,1)
        _Scroll   ("UV Speed (XY)", Vector) = (1,0,0,0)
        _Tiling   ("UV Tiling (XY)", Vector) = (1,1,0,0)
        _Additive ("Use Additive (0/1)", Float) = 1   // 1=加算, 0=通常α
        _KeepTexAlphaOnly ("Ignore Tint Alpha (0/1)", Float) = 1 // 1=テクスチャαのみ採用
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off

        // ブレンドはキーワード切替の代わりに if で分岐できないので、ここは「一旦加算」にして
        // 中で補正。両対応したい場合はパスを2本に分けるのが厳密だけど、この簡易版でも十分実用ッス
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR; // SpriteRenderer Color
            };
            struct v2f {
                float4 pos: SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 vc : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Tint;
            float2 _Scroll;
            float2 _Tiling;
            float  _Additive;
            float  _KeepTexAlphaOnly;

            v2f vert (appdata v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.vc  = v.color; // 頂点カラーは別保持
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UVスクロールだけ
                float2 uv = i.uv * _Tiling + _Scroll * _Time.y;
                fixed4 tex = tex2D(_MainTex, uv); // sRGB/LinearはUnity側が面倒見てくれる

                // --- 色の掛け方を慎重に ---
                // RGBは Tint*VertexColor を掛けるけど、Alphaは「テクスチャαを優先」
                fixed3 rgb = tex.rgb * _Tint.rgb * i.vc.rgb;

                // αは “テクスチャαのみ” か “Tint*Vertex αも乗算” を選べる
                float aTex = tex.a;
                float aMul = tex.a * _Tint.a * i.vc.a;
                float a = lerp(aMul, aTex, step(0.5, _KeepTexAlphaOnly)); // 1ならaTexを採用

                fixed4 col = fixed4(rgb, a);

                // --- ブレンド感：加算/通常風の見え方を出す ---
                if (_Additive > 0.5)
                {
                    // 疑似加算：出力時にαで締めず、RGBのみ強める
                    // 背景と合わさる最終結果は One OneMinusSrcAlpha でも“ほぼ加算風”になる
                    col.rgb *= 1.0 + 1.0 * a; // aに応じて発光寄りに
                    col.a   = a;              // αは元のまま
                }
                else
                {
                    // 通常αブレンド想定：縁のにじみを抑えるためにRGBをαで少し締める
                    col.rgb *= saturate(a * 1.0);
                }

                return col;
            }
            ENDCG
        }
    }
}
