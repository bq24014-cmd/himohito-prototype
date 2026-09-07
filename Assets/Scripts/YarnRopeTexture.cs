using UnityEngine;

namespace HimoHito
{
    /// <summary>One shared, tightly cropped yarn tile, using the toy-art background key.</summary>
    internal static class YarnRopeTexture
    {
        private static Texture2D cached;

        public static Texture2D Load()
        {
            if (cached != null) return cached;
            Texture2D source = Resources.Load<Texture2D>("Art/HimoHitoYarnRope-v1");
            if (source == null || !source.isReadable) return null;
            Color32[] pixels = source.GetPixels32();
            int bottom = source.height;
            int top = -1;
            for (int y = 0; y < source.height; y++)
            for (int x = 0; x < source.width; x++)
            {
                int i = y * source.width + x;
                Color32 p = pixels[i];
                int low = Mathf.Min(p.r, Mathf.Min(p.g, p.b));
                int high = Mathf.Max(p.r, Mathf.Max(p.g, p.b));
                if (low >= 205 && high - low <= 28) p.a = 0;
                pixels[i] = p;
                if (p.a == 0) continue;
                bottom = Mathf.Min(bottom, y);
                top = Mathf.Max(top, y);
            }
            if (top < bottom) return null;
            bottom = Mathf.Max(0, bottom - 2);
            top = Mathf.Min(source.height - 1, top + 2);
            int height = top - bottom + 1;
            Color32[] tile = new Color32[source.width * height];
            System.Array.Copy(pixels, bottom * source.width, tile, 0, tile.Length);
            // Match the boundary pixels to avoid a hard seam in a generated repeat.
            const int feather = 12;
            for (int y = 0; y < height; y++)
            {
                int row = y * source.width;
                Color edge = Color.Lerp(tile[row], tile[row + source.width - 1], 0.5f);
                for (int x = 0; x < feather; x++)
                {
                    float weight = 1f - x / (float)feather;
                    tile[row + x] = Color.Lerp(tile[row + x], edge, weight);
                    int right = row + source.width - 1 - x;
                    tile[right] = Color.Lerp(tile[right], edge, weight);
                }
            }
            cached = new Texture2D(source.width, height, TextureFormat.RGBA32, true)
            {
                name = "Yarn Rope Transparent Tile",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapModeU = TextureWrapMode.Repeat,
                wrapModeV = TextureWrapMode.Clamp
            };
            cached.SetPixels32(tile);
            cached.Apply(true, true);
            return cached;
        }
    }
}
