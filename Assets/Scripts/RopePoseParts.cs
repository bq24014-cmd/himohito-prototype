using UnityEngine;

namespace HimoHito
{
    /// <summary>Cached atlas slices, sharing the original texture without copying pixels.</summary>
    internal sealed class RopePoseParts
    {
        public Sprite Head { get; private set; }
        public Sprite Body { get; private set; }
        public Vector3 Neck { get; private set; }

        public static RopePoseParts Create(Sprite frame, Color32[] pixels, int textureWidth,
            float headCenterY, int headWidth)
        {
            Rect cell = frame.rect;
            // Find the narrow neck just below the round head. Keep a little
            // overlap, so mirrored head/body poses do not expose a seam.
            int first = Mathf.Max(1, Mathf.FloorToInt(headCenterY - headWidth * .65f));
            int last = Mathf.Min((int)cell.height - 2, Mathf.CeilToInt(headCenterY - headWidth * .40f));
            int cut = -1, neckMin = 0, neckMax = 0, narrowest = int.MaxValue;
            for (int y = first; y <= last; y++)
            {
                int min = (int)cell.width, max = -1;
                for (int x = 0; x < cell.width; x++)
                {
                    if (pixels[((int)cell.y + y) * textureWidth + (int)cell.x + x].a == 0) continue;
                    min = Mathf.Min(min, x); max = Mathf.Max(max, x);
                }
                int width = max - min + 1;
                if (width < headWidth * .32f || width >= narrowest) continue;
                narrowest = width; cut = y; neckMin = min; neckMax = max;
            }
            if (cut <= frame.pivot.y || cut < 0 || narrowest > headWidth * .85f) return null;
            float neckX = (neckMin + neckMax + 1f) * .5f;
            Sprite head = Sprite.Create(frame.texture,
                new Rect(cell.x, cell.y + cut, cell.width, cell.height - cut),
                new Vector2(neckX / cell.width, 0f), frame.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            Sprite body = Sprite.Create(frame.texture,
                new Rect(cell.x, cell.y, cell.width, cut + 4),
                new Vector2(frame.pivot.x / cell.width, frame.pivot.y / (cut + 4)),
                frame.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            head.name = frame.name + " Head";
            body.name = frame.name + " Body";
            head.hideFlags = body.hideFlags = HideFlags.HideAndDontSave;
            return new RopePoseParts
            {
                Head = head, Body = body,
                Neck = new Vector3((neckX - frame.pivot.x) / frame.pixelsPerUnit,
                    (cut - frame.pivot.y) / frame.pixelsPerUnit, 0f)
            };
        }
    }
}
