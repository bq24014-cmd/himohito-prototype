using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Measures the dark sewn eyes once, while each existing pose's pixels are available.</summary>
    internal static class RopeFaceLandmarks
    {
        internal readonly struct Eye
        {
            public readonly Vector2 Top;
            public readonly float Width, Height;
            public Eye(Vector2 top, float width, float height) { Top = top; Width = width; Height = height; }
        }
        private static readonly Dictionary<Sprite, Eye[]> Cache = new();
        public static bool TryGet(Sprite sprite, out Eye[] eyes) => Cache.TryGetValue(sprite, out eyes);

        public static void Register(Sprite sprite, Color32[] pixels, int stride, bool headOnly = false)
        {
            if (sprite == null || Cache.ContainsKey(sprite)) return;
            Rect rect = sprite.rect;
            int x0 = Mathf.RoundToInt(rect.x), y0 = Mathf.RoundToInt(rect.y);
            int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (pixels[(y0 + y) * stride + x0 + x].a < 128) continue;
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
            }
            if (maxX < minX) { Cache[sprite] = System.Array.Empty<Eye>(); return; }
            int bottom = headOnly ? minY : minY + Mathf.FloorToInt((maxY - minY) * .42f);
            float contentWidth = maxX - minX + 1;
            var visited = new bool[w * h];
            var queue = new int[w * h];
            var candidates = new List<(Rect bounds, int area)>();
            for (int y = bottom; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                int start = y * w + x;
                if (visited[start] || !Dark(pixels[(y0 + y) * stride + x0 + x])) continue;
                int read = 0, count = 1, left = x, right = x, low = y, high = y;
                queue[0] = start; visited[start] = true;
                while (read < count)
                {
                    int index = queue[read++], px = index % w, py = index / w;
                    left = Mathf.Min(left, px); right = Mathf.Max(right, px);
                    low = Mathf.Min(low, py); high = Mathf.Max(high, py);
                    Visit(px - 1, py); Visit(px + 1, py); Visit(px, py - 1); Visit(px, py + 1);
                    void Visit(int nx, int ny)
                    {
                        if (nx < minX || nx > maxX || ny < bottom || ny > maxY) return;
                        int ni = ny * w + nx;
                        if (visited[ni]) return;
                        visited[ni] = true;
                        if (Dark(pixels[(y0 + ny) * stride + x0 + nx])) queue[count++] = ni;
                    }
                }
                int width = right - left + 1, height = high - low + 1;
                // Reject the connected head outline/tuft, yarn seams, and isolated anti-aliasing specks.
                if (count < contentWidth * contentWidth * .0005f || width < contentWidth * .025f ||
                    width > contentWidth * .25f || height > contentWidth * .30f ||
                    height < contentWidth * .01f || right < minX + contentWidth * .48f) continue;
                candidates.Add((new Rect(left, low, width, height), count));
            }
            candidates.Sort((a, b) => b.area.CompareTo(a.area));
            if (candidates.Count > 2) candidates.RemoveRange(2, candidates.Count - 2);
            candidates.Sort((a, b) => a.bounds.center.x.CompareTo(b.bounds.center.x));
            var result = new Eye[candidates.Count];
            for (int i = 0; i < result.Length; i++)
            {
                Rect eye = candidates[i].bounds;
                result[i] = new Eye(new Vector2(eye.center.x - sprite.pivot.x, eye.yMax - sprite.pivot.y) / sprite.pixelsPerUnit,
                    eye.width / sprite.pixelsPerUnit, eye.height / sprite.pixelsPerUnit);
            }
            Cache[sprite] = result;
        }

        private static bool Dark(Color32 pixel) => pixel.a >= 192 && pixel.r < 85 && pixel.g < 55 && pixel.b < 110;
    }
}
