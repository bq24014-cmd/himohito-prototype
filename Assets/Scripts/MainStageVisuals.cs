using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHito
{
    /// <summary>
    /// Extends the adopted night-child-room toy language to the main stage.
    /// Only renderers are changed; colliders, positions, and physics stay intact.
    /// </summary>
    public static class MainStageVisuals
    {
        private const string BackgroundResourcePath =
            "Art/TutorialNightChildRoom-v1";
        private const string BlockResourcePath =
            "Art/TutorialBlockPlatform-v1";
        private const string RailResourcePath =
            "Art/TutorialRailPlatform-v1";
        private const string HookResourcePath =
            "Art/TutorialHookConnector-v1";
        private const string ToyBoxResourcePath =
            "Art/TutorialToyBoxGoal-v2";
        private const string BackgroundName =
            "Main Stage Night Child Room Background";
        private const string HookVisualName = "Blue Toy Hook Visual";
        private const string BridgeAnchorVisualName =
            "Green Rope Anchor Ring Visual";

        private static Sprite bridgeAnchorRingSprite;

        private static readonly Color PlayerColor =
            new Color(1f, 0.365f, 0.561f);
        private static readonly Color BlockColor =
            new Color(1f, 0.706f, 0.235f);
        private static readonly Color TerrainBodyColor =
            new Color(0.56f, 0.29f, 0.09f);
        private static readonly Color TerrainTopColor =
            new Color(0.96f, 0.49f, 0.10f);
        private static readonly Color BeamFaceColor =
            new Color(0.35f, 0.22f, 0.16f);
        private static readonly Color BeamEdgeColor =
            new Color(0.62f, 0.40f, 0.24f);
        private static readonly Color BeamBracketColor =
            new Color(0.25f, 0.15f, 0.12f);
        private static readonly Color RailColor =
            new Color(0.298f, 0.765f, 1f);
        private static readonly Color HookColor = RailColor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMainStageBackgroundAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name == "MainStage")
            {
                EnsureBackground();
            }
        }

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= EnsureBackground();
            changed |= RestorePlayerVisual(player);

            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!candidate.scene.IsValid() ||
                    !candidate.activeInHierarchy ||
                    !candidate.name.StartsWith(
                        "Main ",
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (candidate == player)
                {
                    continue;
                }

                if (candidate.TryGetComponent(out RopeSpikeHazard _))
                {
                    changed |= RemoveChild(
                        candidate,
                        "Orange Block Platform Visual");
                    changed |= RemoveChild(
                        candidate,
                        "Blue Railway Platform Visual");
                    changed |= ApplyColor(
                        candidate,
                        new Color(1f, 0.18f, 0.25f));
                    if (!candidate.TryGetComponent(
                            out ToySpikeVisual spikeVisual))
                    {
                        spikeVisual = candidate.AddComponent<ToySpikeVisual>();
                        changed = true;
                    }
                    changed |= spikeVisual.Refresh();
                    continue;
                }

                if (candidate.TryGetComponent(out HookPoint hookPoint))
                {
                    bool isBridgeAnchor = candidate.TryGetComponent(
                        out RopePlatformAnchor _);
                    Color hookColor = isBridgeAnchor
                        ? MainStageSectionFourSetup.BridgeAnchorColor
                        : HookColor;
                    changed |= ApplyColor(candidate, hookColor);
                    if (isBridgeAnchor)
                    {
                        changed |= RemoveChild(candidate, HookVisualName);
                        changed |= EnsureBridgeAnchorRingVisual(
                            candidate,
                            hookColor);
                    }
                    else
                    {
                        changed |= RemoveChild(
                            candidate,
                            BridgeAnchorVisualName);
                        changed |= EnsureToyVisual(
                            candidate,
                            HookVisualName,
                            HookResourcePath,
                            6,
                            hookColor);
                    }
                    changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
                    continue;
                }

                if (candidate.name == MainStageSectionTenSetup.GoalName)
                {
                    changed |= ApplyColor(candidate, RailColor);
                    changed |= EnsureToyVisual(
                        candidate,
                        "Blue Railway Goal Base Visual",
                        RailResourcePath,
                        1);
                    changed |= EnsureGoalToyBox(candidate);
                    continue;
                }

                if (candidate.name == MainStageSectionTwoSetup.BoardName)
                {
                    changed |= EnsureLengthWindowBeamVisual(candidate);
                    continue;
                }

                if (candidate.TryGetComponent(out BoxCollider2D _) &&
                    candidate.TryGetComponent(out SolidSprite _))
                {
                    if (IsBankTerrain(candidate.name))
                    {
                        changed |= EnsureSectionOneTerrainVisual(candidate);
                        continue;
                    }

                    bool isStartGround = candidate.name == "Main Start Ground";
                    bool isSolidToyBoard =
                        isStartGround ||
                        candidate.TryGetComponent(out SolidSwingSurface _);
                    changed |= RemoveChild(
                        candidate,
                        isSolidToyBoard
                            ? "Blue Railway Platform Visual"
                            : "Orange Block Platform Visual");
                    changed |= ApplyColor(
                        candidate,
                        isSolidToyBoard ? BlockColor : RailColor);
                    changed |= EnsureToyVisual(
                        candidate,
                        isSolidToyBoard
                            ? "Orange Block Platform Visual"
                            : "Blue Railway Platform Visual",
                        isSolidToyBoard ? BlockResourcePath : RailResourcePath,
                        1);
                }
            }

            return changed;
        }

        private static bool IsBankTerrain(string objectName)
        {
            return objectName == "Main Start Ground" ||
                   objectName == "Main Landing 1" ||
                   objectName == "Main Landing 2" ||
                   objectName == MainStageSectionThreeSetup.LowDeadEndName ||
                   objectName == MainStageSectionThreeSetup.HighShelfName ||
                   objectName == MainStageSectionThreeSetup.ReturnStepAName ||
                   objectName == MainStageSectionThreeSetup.ReturnStepBName ||
                   objectName == MainStageSectionThreeSetup.ReturnStepCName ||
                   objectName == MainStageSectionThreeSetup.ReturnStepDName ||
                   objectName == MainStageSectionFourSetup.IntermediateColumnName ||
                   objectName == MainStageSectionFourSetup.LandingName ||
                   objectName == MainStageSectionFiveSetup.LandingName ||
                   objectName == "Main S01 Start Shelf" ||
                   objectName == "Main S01 Landing" ||
                   objectName == "Main S02 Raised Landing" ||
                   objectName == "Main S03 Lower Dead End" ||
                   objectName == "Main S03 High Shelf";
        }

        private static bool EnsureSectionOneTerrainVisual(GameObject terrain)
        {
            if (terrain == null ||
                !terrain.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveChild(terrain, "Orange Block Platform Visual");
            changed |= RemoveChild(terrain, "Blue Railway Platform Visual");
            changed |= ApplyColor(terrain, TerrainBodyColor);
            changed |= EnsureSolidVisualPart(
                terrain,
                "Terrain Top Edge",
                new Vector2(0f, 0.475f),
                new Vector2(1f, 0.05f),
                TerrainTopColor,
                sourceRenderer.sortingOrder + 2);

            if (!sourceRenderer.enabled)
            {
                sourceRenderer.enabled = true;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureLengthWindowBeamVisual(GameObject board)
        {
            if (board == null ||
                !board.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveChild(board, "Orange Block Platform Visual");
            changed |= EnsureSolidVisualPart(
                board,
                "Wooden Beam Face",
                Vector2.zero,
                new Vector2(1f, 0.72f),
                BeamFaceColor,
                sourceRenderer.sortingOrder + 2);
            changed |= EnsureSolidVisualPart(
                board,
                "Wooden Beam Lower Edge",
                new Vector2(0f, -0.42f),
                new Vector2(1.06f, 0.22f),
                BeamEdgeColor,
                sourceRenderer.sortingOrder + 3);
            changed |= EnsureSolidVisualPart(
                board,
                "Wooden Beam Left Bracket",
                new Vector2(-0.36f, -0.74f),
                new Vector2(0.08f, 0.56f),
                BeamBracketColor,
                sourceRenderer.sortingOrder + 1);
            changed |= EnsureSolidVisualPart(
                board,
                "Wooden Beam Right Bracket",
                new Vector2(0.36f, -0.74f),
                new Vector2(0.08f, 0.56f),
                BeamBracketColor,
                sourceRenderer.sortingOrder + 1);

            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureSolidVisualPart(
            GameObject parent,
            string partName,
            Vector2 localPosition,
            Vector2 localScale,
            Color color,
            int sortingOrder)
        {
            bool changed = false;
            Transform partTransform = parent.transform.Find(partName);
            if (partTransform == null)
            {
                GameObject partObject = new GameObject(partName);
                partTransform = partObject.transform;
                partTransform.SetParent(parent.transform, false);
                changed = true;
            }

            Vector3 targetPosition = new Vector3(
                localPosition.x,
                localPosition.y,
                0f);
            Vector3 targetScale = new Vector3(
                localScale.x,
                localScale.y,
                1f);
            if (partTransform.localPosition != targetPosition)
            {
                partTransform.localPosition = targetPosition;
                changed = true;
            }
            if (partTransform.localRotation != Quaternion.identity)
            {
                partTransform.localRotation = Quaternion.identity;
                changed = true;
            }
            if (partTransform.localScale != targetScale)
            {
                partTransform.localScale = targetScale;
                changed = true;
            }

            if (!partTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = partTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!partTransform.TryGetComponent(out SolidSprite solidSprite))
            {
                solidSprite = partTransform.gameObject.AddComponent<SolidSprite>();
                changed = true;
            }
            if (solidSprite.Color != color)
            {
                solidSprite.Color = color;
                changed = true;
            }
            if (renderer.sortingOrder != sortingOrder)
            {
                renderer.sortingOrder = sortingOrder;
                changed = true;
            }

            return changed;
        }

        private static bool RemoveChild(GameObject parent, string childName)
        {
            Transform child = parent != null
                ? parent.transform.Find(childName)
                : null;
            if (child == null)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
            return true;
        }

        private static bool RestorePlayerVisual(GameObject player)
        {
            if (player == null)
            {
                return false;
            }

            bool changed = ApplyColor(player, PlayerColor);
            Transform wrongRailVisual =
                player.transform.Find("Blue Railway Platform Visual");
            if (wrongRailVisual != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(wrongRailVisual.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(wrongRailVisual.gameObject);
                }
                changed = true;
            }

            Transform ropeBodyVisual = player.transform.Find("Rope Body Visual");
            if (ropeBodyVisual != null)
            {
                changed |= ApplyColor(ropeBodyVisual.gameObject, PlayerColor);
            }

            if (player.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                bool shouldEnableSource = ropeBodyVisual == null;
                if (sourceRenderer.enabled != shouldEnableSource)
                {
                    sourceRenderer.enabled = shouldEnableSource;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureToyVisual(
            GameObject target,
            string visualName,
            string resourcePath,
            int sortingOrderOffset,
            Color? tint = null)
        {
            if (target == null ||
                !target.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            Sprite processedSprite =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(resourcePath);
            if (processedSprite == null)
            {
                return false;
            }

            bool changed = false;
            Transform visualTransform = target.transform.Find(visualName);
            if (visualTransform == null)
            {
                GameObject visualObject = new GameObject(visualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(target.transform, false);
                changed = true;
            }

            Vector2 spriteSize = processedSprite.bounds.size;
            Vector3 targetScale = new Vector3(
                1f / Mathf.Max(0.01f, spriteSize.x),
                1f / Mathf.Max(0.01f, spriteSize.y),
                1f);
            if (visualTransform.localPosition != Vector3.zero)
            {
                visualTransform.localPosition = Vector3.zero;
                changed = true;
            }
            if (visualTransform.localRotation != Quaternion.identity)
            {
                visualTransform.localRotation = Quaternion.identity;
                changed = true;
            }
            if (visualTransform.localScale != targetScale)
            {
                visualTransform.localScale = targetScale;
                changed = true;
            }

            if (!visualTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (renderer.sprite != processedSprite)
            {
                renderer.sprite = processedSprite;
                changed = true;
            }
            Color targetColor = tint ?? Color.white;
            if (renderer.color != targetColor)
            {
                renderer.color = targetColor;
                changed = true;
            }
            if (renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }

            int targetOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
            if (renderer.sortingOrder != targetOrder)
            {
                renderer.sortingOrder = targetOrder;
                changed = true;
            }
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureBridgeAnchorRingVisual(
            GameObject target,
            Color tint)
        {
            if (target == null ||
                !target.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            Sprite ringSprite = GetBridgeAnchorRingSprite();
            if (ringSprite == null)
            {
                return false;
            }

            bool changed = false;
            Transform visualTransform =
                target.transform.Find(BridgeAnchorVisualName);
            if (visualTransform == null)
            {
                GameObject visualObject =
                    new GameObject(BridgeAnchorVisualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(target.transform, false);
                changed = true;
            }

            Vector2 spriteSize = ringSprite.bounds.size;
            Vector3 targetScale = new Vector3(
                1f / Mathf.Max(0.01f, spriteSize.x),
                1f / Mathf.Max(0.01f, spriteSize.y),
                1f);
            if (visualTransform.localPosition != Vector3.zero)
            {
                visualTransform.localPosition = Vector3.zero;
                changed = true;
            }
            if (visualTransform.localRotation != Quaternion.identity)
            {
                visualTransform.localRotation = Quaternion.identity;
                changed = true;
            }
            if (visualTransform.localScale != targetScale)
            {
                visualTransform.localScale = targetScale;
                changed = true;
            }

            if (!visualTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (renderer.sprite != ringSprite)
            {
                renderer.sprite = ringSprite;
                changed = true;
            }
            if (renderer.color != tint)
            {
                renderer.color = tint;
                changed = true;
            }
            if (renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            int targetOrder = sourceRenderer.sortingOrder + 6;
            if (renderer.sortingOrder != targetOrder)
            {
                renderer.sortingOrder = targetOrder;
                changed = true;
            }
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            return changed;
        }

        private static Sprite GetBridgeAnchorRingSprite()
        {
            if (bridgeAnchorRingSprite != null)
            {
                return bridgeAnchorRingSprite;
            }

            Sprite sourceSprite =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    HookResourcePath);
            if (sourceSprite == null || sourceSprite.texture == null)
            {
                return null;
            }

            const int textureSize = 128;
            Texture2D sourceTexture = sourceSprite.texture;
            Rect sourceRect = sourceSprite.textureRect;
            float sampleSide = Mathf.Min(sourceRect.width, sourceRect.height);
            Vector2 sampleOrigin = new Vector2(
                sourceRect.center.x - sampleSide * 0.5f,
                sourceRect.center.y - sampleSide * 0.5f);
            Color[] pixels = new Color[textureSize * textureSize];

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float normalizedX = (x + 0.5f) / textureSize;
                    float normalizedY = (y + 0.5f) / textureSize;
                    float sourceX = sampleOrigin.x + normalizedX * sampleSide;
                    float sourceY = sampleOrigin.y + normalizedY * sampleSide;
                    Color sampled = sourceTexture.GetPixelBilinear(
                        sourceX / sourceTexture.width,
                        sourceY / sourceTexture.height);

                    Vector2 centered = new Vector2(
                        normalizedX - 0.5f,
                        normalizedY - 0.5f);
                    float radius = centered.magnitude;
                    float circularMask = 1f - Mathf.SmoothStep(
                        0.46f,
                        0.5f,
                        radius);
                    sampled.a *= circularMask;
                    pixels[y * textureSize + x] = sampled;
                }
            }

            Texture2D ringTexture = new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false);
            ringTexture.name = "Generated Green Rope Anchor Ring";
            ringTexture.hideFlags = HideFlags.HideAndDontSave;
            ringTexture.filterMode = FilterMode.Bilinear;
            ringTexture.wrapMode = TextureWrapMode.Clamp;
            ringTexture.SetPixels(pixels);
            ringTexture.Apply(false, false);

            bridgeAnchorRingSprite = Sprite.Create(
                ringTexture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                textureSize,
                0,
                SpriteMeshType.FullRect);
            bridgeAnchorRingSprite.name =
                "Generated Green Rope Anchor Ring Sprite";
            return bridgeAnchorRingSprite;
        }

        private static bool EnsureGoalToyBox(GameObject goal)
        {
            if (goal == null ||
                !goal.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            Sprite toyBoxSprite =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    ToyBoxResourcePath);
            if (toyBoxSprite == null)
            {
                return false;
            }

            const string visualName = "Main Goal Open Toy Box Visual";
            const float toyBoxWorldWidth = 4.5f;
            const float leftInset = 0.4f;
            bool changed = false;
            Transform visualTransform = goal.transform.Find(visualName);
            if (visualTransform == null)
            {
                GameObject visualObject = new GameObject(visualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(goal.transform, false);
                changed = true;
            }

            Vector2 spriteSize = toyBoxSprite.bounds.size;
            float toyBoxWorldHeight =
                toyBoxWorldWidth * spriteSize.y / Mathf.Max(0.01f, spriteSize.x);
            float goalWidth = Mathf.Abs(goal.transform.lossyScale.x);
            float goalHeight = Mathf.Abs(goal.transform.lossyScale.y);
            float goalLeft = goal.transform.position.x - goalWidth * 0.5f;
            float goalTop = goal.transform.position.y + goalHeight * 0.5f;
            Vector3 targetWorldPosition = new Vector3(
                goalLeft + leftInset + toyBoxWorldWidth * 0.5f,
                goalTop + toyBoxWorldHeight * 0.5f,
                goal.transform.position.z);
            if (visualTransform.position != targetWorldPosition)
            {
                visualTransform.position = targetWorldPosition;
                changed = true;
            }
            if (visualTransform.rotation != Quaternion.identity)
            {
                visualTransform.rotation = Quaternion.identity;
                changed = true;
            }

            Vector3 parentScale = goal.transform.lossyScale;
            Vector3 targetScale = new Vector3(
                toyBoxWorldWidth /
                    Mathf.Max(0.01f, spriteSize.x * Mathf.Abs(parentScale.x)),
                toyBoxWorldHeight /
                    Mathf.Max(0.01f, spriteSize.y * Mathf.Abs(parentScale.y)),
                1f);
            if (visualTransform.localScale != targetScale)
            {
                visualTransform.localScale = targetScale;
                changed = true;
            }

            if (!visualTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (renderer.sprite != toyBoxSprite)
            {
                renderer.sprite = toyBoxSprite;
                changed = true;
            }
            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
                changed = true;
            }
            if (renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            if (renderer.sortingOrder != sourceRenderer.sortingOrder + 2)
            {
                renderer.sortingOrder = sourceRenderer.sortingOrder + 2;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureBackground()
        {
            Sprite backgroundSprite =
                Resources.Load<Sprite>(BackgroundResourcePath);
            if (backgroundSprite == null)
            {
                Debug.LogWarning(
                    $"Main-stage background was not found: {BackgroundResourcePath}");
                return false;
            }

            bool changed = false;
            GameObject background = FindSceneObject(BackgroundName);
            if (background == null)
            {
                background = new GameObject(BackgroundName);
                changed = true;
            }
            else if (!background.activeSelf)
            {
                background.SetActive(true);
                changed = true;
            }

            Camera targetCamera = Camera.main;
            float cameraX = targetCamera != null
                ? targetCamera.transform.position.x
                : 0f;
            Vector3 targetPosition = new Vector3(cameraX, 0f, 1f);
            if (background.transform.position != targetPosition)
            {
                background.transform.position = targetPosition;
                changed = true;
            }

            float width = Mathf.Max(0.01f, backgroundSprite.bounds.size.x);
            float scale = 60f / width;
            Vector3 targetScale = new Vector3(scale, scale, 1f);
            if (background.transform.localScale != targetScale)
            {
                background.transform.localScale = targetScale;
                changed = true;
            }

            if (!background.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = background.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!renderer.enabled)
            {
                renderer.enabled = true;
                changed = true;
            }
            if (renderer.forceRenderingOff)
            {
                renderer.forceRenderingOff = false;
                changed = true;
            }
            if (renderer.sprite != backgroundSprite)
            {
                renderer.sprite = backgroundSprite;
                changed = true;
            }
            if (renderer.color != Color.white)
            {
                renderer.color = Color.white;
                changed = true;
            }
            if (renderer.sortingOrder != -100)
            {
                renderer.sortingOrder = -100;
                changed = true;
            }

            if (!background.TryGetComponent(
                    out TutorialBackgroundParallax parallax))
            {
                parallax = background.AddComponent<TutorialBackgroundParallax>();
                changed = true;
            }
            parallax.Configure(0.97f);
            return changed;
        }

        private static bool ApplyColor(GameObject target, Color color)
        {
            if (target == null || !target.TryGetComponent(out SolidSprite visual))
            {
                return false;
            }

            if (visual.Color == color)
            {
                return false;
            }

            visual.Color = color;
            return true;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
