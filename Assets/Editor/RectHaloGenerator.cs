using UnityEngine;
using UnityEditor;
using System.IO;

public class RectHaloGenerator
{
    [MenuItem("Tools/Generate Rect Halo Texture")]
    static void Generate()
    {
        int size = 256;

        // --- tweak these ---
        float cornerRadius = 0.45f;  // 0 = sharp corners, 0.5+ = very round
        float feather = 0.6f;   // how far the fade reaches inward (bigger = softer)
        float falloff = 1.6f;   // >1 = softer tail, <1 = harder edge
        float noiseAmount = 0.08f;  // edge irregularity (0 = perfectly clean)
        float noiseScale = 4f;     // bigger = finer wobble
        // -------------------

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float seed = 37.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // normalized coords, -1 to 1
                float px = (x + 0.5f) / size * 2f - 1f;
                float py = (y + 0.5f) / size * 2f - 1f;

                // rounded-rectangle signed distance
                float bx = 1f - cornerRadius;
                float qx = Mathf.Abs(px) - bx;
                float qy = Mathf.Abs(py) - bx;
                float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                float dist = outside + inside - cornerRadius; // negative inside the shape

                float depth = -dist; // 0 at the edge, grows toward the center

                // organic wobble
                float n = Mathf.PerlinNoise(x / (float)size * noiseScale + seed,
                                            y / (float)size * noiseScale + seed) - 0.5f;
                depth += n * noiseAmount;

                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(depth / feather));
                a = Mathf.Pow(a, falloff);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();

        string path = "Assets/RectHalo.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteBorder = new Vector4(96, 96, 96, 96); // scaled up for the 256px texture
        importer.SaveAndReimport();
    }
}