Shader "Custom/UVScroll"
{
	Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Tint     ("Tint (RGB)", Color) = (1,1,1,1)
        _Scroll   ("UV Speed (XY)", Vector) = (1,0,0,0)
        _Tiling   ("UV Tiling (XY)", Vector) = (1,1,0,0)
        _Additive ("Use Additive (0/1)", Float) = 1   // 1=���Z, 0=�ʏ탿
        _KeepTexAlphaOnly ("Ignore Tint Alpha (0/1)", Float) = 1 // 1=�e�N�X�`�����̂ݍ̗p
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off

        // �u�����h�̓L�[���[�h�ؑւ̑���� if �ŕ���ł��Ȃ��̂ŁA�����́u��U���Z�v�ɂ���
        // ���ŕ␳�B���Ή��������ꍇ�̓p�X��2�{�ɕ�����̂����������ǁA���̊ȈՔłł��\�����p�b�X
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
                o.vc  = v.color; // ���_�J���[�͕ʕێ�
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV�X�N���[������
                float2 uv = i.uv * _Tiling + _Scroll * _Time.y;
                fixed4 tex = tex2D(_MainTex, uv); // sRGB/Linear��Unity�����ʓ|���Ă����

                // --- �F�̊|������T�d�� ---
                // RGB�� Tint*VertexColor ���|���邯�ǁAAlpha�́u�e�N�X�`������D��v
                fixed3 rgb = tex.rgb * _Tint.rgb * i.vc.rgb;

                // ���� �g�e�N�X�`�����̂݁h �� �gTint*Vertex ������Z�h ��I�ׂ�
                float aTex = tex.a;
                float aMul = tex.a * _Tint.a * i.vc.a;
                float a = lerp(aMul, aTex, step(0.5, _KeepTexAlphaOnly)); // 1�Ȃ�aTex���̗p

                fixed4 col = fixed4(rgb, a);

                // --- �u�����h���F���Z/�ʏ핗�̌��������o�� ---
                if (_Additive > 0.5)
                {
                    // �^�����Z�F�o�͎��Ƀ��Œ��߂��ARGB�̂݋��߂�
                    // �w�i�ƍ��킳��ŏI���ʂ� One OneMinusSrcAlpha �ł��g�قډ��Z���h�ɂȂ�
                    col.rgb *= 1.0 + 1.0 * a; // a�ɉ����Ĕ�������
                    col.a   = a;              // ���͌��̂܂�
                }
                else
                {
                    // �ʏ탿�u�����h�z��F���̂ɂ��݂�}���邽�߂�RGB�����ŏ������߂�
                    col.rgb *= saturate(a * 1.0);
                }

                return col;
            }
            ENDCG
        }
    }
}
