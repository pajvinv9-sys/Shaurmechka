Shader "Custom/Pixelation"
{
    Properties
    {
        _PixelSize ("Pixel Size", Range(1, 16)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Pixelation"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _PixelSize;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Координаты пикселя на экране
                float2 uv = input.texcoord;

                // Размер блока пикселизации
                float pixelSize = max(_PixelSize, 1.0);

                // Количество блоков по ширине и высоте
                float2 resolution = _ScreenParams.xy / pixelSize;

                // Берём центр каждого блока
                uv = (floor(uv * resolution) + 0.5) / resolution;

                // Читаем цвет из исходной картинки
                return SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );
            }

            ENDHLSL
        }
    }
}
