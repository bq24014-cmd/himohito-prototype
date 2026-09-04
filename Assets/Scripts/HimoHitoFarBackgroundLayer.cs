using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates a distant, horizontally repeated room layer behind the adopted
    /// room artwork. Mirrored neighbours make both tile boundaries exact.
    /// </summary>
    public static class HimoHitoFarBackgroundLayer
    {
        private const string ResourcePath =
            "Art/HimoHitoFarBackground-v1";
        private const string TileNamePrefix = "Far Background Tile ";
        private const int TileCount = 3;
        private const int SortingOrder = -110;

        public static bool Ensure(
            string rootName,
            float worldWidth,
            float horizontalFollow)
        {
            Sprite sprite = Resources.Load<Sprite>(ResourcePath);
            if (sprite == null)
            {
                Debug.LogWarning(
                    $"Far background was not found: {ResourcePath}");
                return false;
            }

            bool changed = false;
            GameObject root = SceneObjectLookup.Find(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
                changed = true;
            }
            if (!root.activeSelf)
            {
                root.SetActive(true);
                changed = true;
            }

            Camera targetCamera = Camera.main;
            float cameraX = targetCamera != null
                ? targetCamera.transform.position.x
                : 0f;
            Vector3 targetPosition = new Vector3(cameraX, 0f, 2f);
            if (root.transform.position != targetPosition)
            {
                root.transform.position = targetPosition;
                changed = true;
            }
            if (root.transform.localScale != Vector3.one)
            {
                root.transform.localScale = Vector3.one;
                changed = true;
            }

            float spriteWidth = Mathf.Max(0.01f, sprite.bounds.size.x);
            float uniformScale = worldWidth / spriteWidth;
            for (int index = 0; index < TileCount; index++)
            {
                int offset = index - 1;
                changed |= EnsureTile(
                    root,
                    sprite,
                    index + 1,
                    offset,
                    worldWidth,
                    uniformScale);
            }

            if (!root.TryGetComponent(
                    out TutorialBackgroundParallax parallax))
            {
                parallax = root.AddComponent<TutorialBackgroundParallax>();
                changed = true;
            }
            parallax.Configure(horizontalFollow);
            return changed;
        }

        private static bool EnsureTile(
            GameObject root,
            Sprite sprite,
            int tileNumber,
            int offset,
            float worldWidth,
            float uniformScale)
        {
            bool changed = false;
            string tileName = TileNamePrefix + tileNumber;
            Transform tile = root.transform.Find(tileName);
            if (tile == null)
            {
                GameObject tileObject = new GameObject(tileName);
                tile = tileObject.transform;
                tile.SetParent(root.transform, false);
                changed = true;
            }
            if (!tile.gameObject.activeSelf)
            {
                tile.gameObject.SetActive(true);
                changed = true;
            }

            Vector3 localPosition = new Vector3(offset * worldWidth, 0f, 0f);
            if (tile.localPosition != localPosition)
            {
                tile.localPosition = localPosition;
                changed = true;
            }
            if (tile.localRotation != Quaternion.identity)
            {
                tile.localRotation = Quaternion.identity;
                changed = true;
            }

            // Side copies are mirrored. The touching pixels therefore come
            // from the same source edge and cannot open a visible seam.
            float horizontalScale = offset == 0
                ? uniformScale
                : -uniformScale;
            Vector3 localScale = new Vector3(
                horizontalScale,
                uniformScale,
                1f);
            if (tile.localScale != localScale)
            {
                tile.localScale = localScale;
                changed = true;
            }

            if (!tile.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = tile.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (renderer.sprite != sprite)
            {
                renderer.sprite = sprite;
                changed = true;
            }
            if (!renderer.enabled)
            {
                renderer.enabled = true;
                changed = true;
            }
            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
                changed = true;
            }
            if (renderer.sortingOrder != SortingOrder)
            {
                renderer.sortingOrder = SortingOrder;
                changed = true;
            }
            return changed;
        }
    }
}
