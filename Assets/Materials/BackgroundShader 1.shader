// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BackgroundShader"
{
    Properties
    {
    [KeywordEnum(X, Y)] _FadeWith ("Fade With", Float) = 0
        _Color1 ("Color", Color) = (1,1,1,1)
        _Color2 ("Color", Color) = (0,0,0,1)
        [PowerSlider(1)] _Weights ("Weights", Range (0.0, 2.0)) = 1.0
    }
    SubShader
    {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color1;
            fixed4 _Color2;
            float _Weights;

            //计算给定point点到经过linePoint的直线line的距离
            float getPointLineDist(float2 pt, float2 linePoint, float2 ln)
            {
                float dist;
                float2 point2point = pt - linePoint;
                dist = length(dot(pt, ln) * ln/length(ln) - point2point);
                return dist;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //注意，uv的最大值是(1, 1)
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                float2 pt;                      //point
                float2 linePoint = float2(1,1);
                float2 ln = float2(-1, 1);      //line

                pt = float2(i.uv.x, i.uv.y * 1);
                float pointLineDist = getPointLineDist(pt, linePoint, ln);
                float maxLineDist = getPointLineDist(float2(0, 0), linePoint, ln);

                float lp = pointLineDist / maxLineDist * _Weights;
                col = lerp(_Color1, _Color2, lp);

                return col;
            }
            ENDCG
        }
    }
}
