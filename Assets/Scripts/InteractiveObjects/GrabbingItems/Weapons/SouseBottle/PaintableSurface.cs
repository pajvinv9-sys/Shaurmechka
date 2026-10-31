
using UnityEngine;

public class PaintableSurface : MonoBehaviour
{
    private RenderTexture paintTexture;
    private RenderTexture bufferTexture;

    private Material brushMaterial;
    private Material[] paintMaterials;

    private bool initialized;

    public bool IsInitialized => initialized;

    public void Initialize(Shader surfaceShader, int resolution)
    {
        if (initialized)
            return;

        if (surfaceShader == null)
        {
            Debug.LogError("Paintable shader is missing!");
            return;
        }

        Renderer rend = GetComponent<Renderer>();

        if (rend == null)
            return;

        // Создаём текстуры
        paintTexture = CreateTexture(resolution);
        bufferTexture = CreateTexture(resolution);

        // Создаём материал кисти
        Shader brushShader = Shader.Find("Custom/PaintBrush");

        if (brushShader == null)
        {
            Debug.LogError("PaintBrush shader not found!");
            return;
        }

        brushMaterial = new Material(brushShader);

        // Создаём материалы для всех sub-materials объекта
        Material[] originalMaterials = rend.materials;
        paintMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            Material original = originalMaterials[i];

            Material paintMat = new Material(surfaceShader);

            Texture baseTexture = Texture2D.whiteTexture;

            if (original.HasProperty("_BaseMap"))
                baseTexture = original.GetTexture("_BaseMap");
            else if (original.HasProperty("_MainTex"))
                baseTexture = original.GetTexture("_MainTex");

            Color baseColor = Color.white;

            if (original.HasProperty("_BaseColor"))
                baseColor = original.GetColor("_BaseColor");
            else if (original.HasProperty("_Color"))
                baseColor = original.GetColor("_Color");

            paintMat.SetTexture("_BaseMap", baseTexture);
            paintMat.SetColor("_BaseColor", baseColor);

            if (original.HasProperty("_BaseMap"))
            {
                paintMat.SetTextureScale(
                    "_BaseMap",
                    original.GetTextureScale("_BaseMap")
                );

                paintMat.SetTextureOffset(
                    "_BaseMap",
                    original.GetTextureOffset("_BaseMap")
                );
            }

            if (original.HasProperty("_Metallic"))
                paintMat.SetFloat("_Metallic", original.GetFloat("_Metallic"));

            if (original.HasProperty("_Smoothness"))
                paintMat.SetFloat("_Smoothness", original.GetFloat("_Smoothness"));

            paintMat.SetTexture("_PaintTex", paintTexture);

            paintMaterials[i] = paintMat;
        }

        rend.materials = paintMaterials;

        initialized = true;
    }

    private RenderTexture CreateTexture(int resolution)
    {
        RenderTexture texture = new RenderTexture(
            resolution,
            resolution,
            0,
            RenderTextureFormat.ARGB32
        );

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.useMipMap = false;
        texture.autoGenerateMips = false;

        texture.Create();

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = texture;
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        RenderTexture.active = previous;

        return texture;
    }

    public void DrawPaint(
        Vector2 uv,
        Color color,
        float size,
        float hardness
    )
    {
        if (!initialized)
            return;

        brushMaterial.SetVector(
            "_BrushUV",
            new Vector4(uv.x, uv.y, 0, 0)
        );

        brushMaterial.SetColor("_BrushColor", color);

        brushMaterial.SetFloat(
            "_BrushRadius",
            size / paintTexture.height
        );

        brushMaterial.SetFloat("_BrushHardness", hardness);

        brushMaterial.SetFloat(
            "_TextureAspect",
            (float)paintTexture.width / paintTexture.height
        );

        // Рисуем в buffer, используя предыдущую текстуру
        Graphics.Blit(paintTexture, bufferTexture, brushMaterial);

        // Меняем текстуры местами
        RenderTexture temp = paintTexture;
        paintTexture = bufferTexture;
        bufferTexture = temp;

        // Обновляем текстуру на материалах
        foreach (Material mat in paintMaterials)
        {
            mat.SetTexture("_PaintTex", paintTexture);
        }
    }

    private void OnDestroy()
    {
        if (paintTexture != null)
        {
            paintTexture.Release();
            Destroy(paintTexture);
        }

        if (bufferTexture != null)
        {
            bufferTexture.Release();
            Destroy(bufferTexture);
        }

        if (brushMaterial != null)
            Destroy(brushMaterial);

        if (paintMaterials != null)
        {
            foreach (Material mat in paintMaterials)
            {
                if (mat != null)
                    Destroy(mat);
            }
        }
    }
}
