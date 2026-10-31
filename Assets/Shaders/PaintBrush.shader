Shader "Custom/PaintBrush"
{
    Properties
    {
        _MainTex ("Previous Paint", 2D) = "black" {}
        _BrushUV ("Brush UV", Vector) = (0, 0, 0, 0)
        _BrushColor ("Brush Color", Color) = (1, 0, 0, 1)
        _BrushRadius ("Brush Radius", Float) = 0.02
        _BrushHardness ("Brush Hardness", Range(0, 1)) = 0.7
        _TextureAspect ("Texture Aspect", Float) = 1
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float2 _BrushUV;
            float4 _BrushColor;
            float _BrushRadius;
            float _BrushHardness;
            float _TextureAspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 oldPaint = tex2D(_MainTex, i.uv);

                float2 delta = i.uv - _BrushUV;
                delta.x *= _TextureAspect;

                float distanceToBrush = length(delta);

                float innerRadius = _BrushRadius * _BrushHardness;

                float coverage = 1.0 - smoothstep(
                    innerRadius,
                    _BrushRadius,
                    distanceToBrush
                );

                coverage *= _BrushColor.a;

                float newAlpha = coverage + oldPaint.a * (1.0 - coverage);

                float3 newColor = lerp(
                    oldPaint.rgb,
                    _BrushColor.rgb,
                    coverage / max(newAlpha, 0.0001)
                );

                return float4(newColor, newAlpha);
            }

            ENDHLSL
        }
    }
}
