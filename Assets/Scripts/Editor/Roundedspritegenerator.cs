using UnityEngine;
using UnityEditor;
using System.IO;

// Place this file in a folder named "Editor" anywhere under Assets
// (e.g. Assets/Editor/RoundedSpriteGenerator.cs). UnityEditor code
// cannot live alongside runtime scripts or it'll break player builds.
public static class RoundedSpriteGenerator
{
    private const int Size = 128; // texture resolution; higher = smoother corners
    private const string OutputFolder = "Assets/UI/GeneratedSprites";

    [MenuItem("Tools/Zennef/Generate Rounded Bar Sprite (Subtle)")]
    public static void GenerateSubtle() => Generate("RoundedBar_Subtle", radiusPx: 12);

    [MenuItem("Tools/Zennef/Generate Rounded Bar Sprite (Pill)")]
    public static void GeneratePill() => Generate("RoundedBar_Pill", radiusPx: Size / 2);

    private static void Generate(string fileName, int radiusPx)
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float alpha = RoundedRectAlpha(x + 0.5f, y + 0.5f, Size, Size, radiusPx);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
        string pngPath = Path.Combine(OutputFolder, fileName + ".png");
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(pngPath);
        ConfigureSpriteImport(pngPath, radiusPx);

        Debug.Log($"Generated {pngPath} (corner radius {radiusPx}px). " +
                   "Assign to your Slider's Background and Fill Images, " +
                   "set Image Type to Sliced.");
    }

    // Standard rounded-box signed distance field (Inigo Quilez form),
    // with ~1px antialiasing so edges aren't jagged.
    private static float RoundedRectAlpha(float px, float py, float width, float height, float radius)
    {
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float r = Mathf.Min(radius, Mathf.Min(hw, hh));

        float qx = Mathf.Abs(px - hw) - (hw - r);
        float qy = Mathf.Abs(py - hh) - (hh - r);

        float outsideX = Mathf.Max(qx, 0f);
        float outsideY = Mathf.Max(qy, 0f);
        float dist = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY)
                     + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;

        return Mathf.Clamp01(0.5f - dist);
    }

    private static void ConfigureSpriteImport(string path, int radiusPx)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;

        // Border = where the stretchable middle begins on each side.
        // Symmetric since the generated rect is symmetric.
        importer.spriteBorder = new Vector4(radiusPx, radiusPx, radiusPx, radiusPx);

        importer.SaveAndReimport();
    }
}