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
        private const string ToyBoxResourcePath =
            "Art/TutorialToyBoxGoal-v2";
        private const string BackgroundName =
            "Main Stage Night Child Room Background";
        private const string FarBackgroundName =
            "Main Stage Far Child Room Background";
        private const string HookVisualName = "Blue Toy Hook Visual";
        private const string BridgeAnchorVisualName =
            "Green Rope Anchor Ring Visual";
        private const string GoalVisualName = "Open Toy Box Goal Visual";
        private const string BlockTileVisualPrefix =
            "Orange Block Platform Tile ";
        private const float MaximumBlockStripWorldWidth = 7.5f;
        private const float HookVisualWorldDiameter = 0.72f;

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
        private static int lastReloadRestoreFrame = -1;
        private static bool isRestoringAfterReload;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMainStageVisualsAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name == "MainStage")
            {
                Apply(FindSceneObject("Main Player"));
            }
        }

        public static bool Apply(GameObject player)
        {
            bool changed = false;
            changed |= RecoverySwitchVisual.Ensure(
                FindSceneObject(MainStageSectionThreeSetup.RecoverySwitchName));
            changed |= HimoHitoFarBackgroundLayer.Ensure(
                FarBackgroundName,
                60f,
                0.78f);
            changed |= EnsureBackground();
            changed |= RestorePlayerVisual(player);
            changed |= GoalChestPresentation.Ensure(
                FindSceneObject(MainStageSectionTenSetup.GoalMarkerName),
                FindSceneObject(MainStageSectionTenSetup.GoalFloorName));

            foreach (GameObject candidate in
                     Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate == null ||
                    !candidate.scene.IsValid() ||
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
                    changed |= RemoveRepeatedBlockVisuals(candidate);
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

                if (candidate.TryGetComponent(out HookPoint _))
                {
                    changed |= EnsureHookVisual(candidate);
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
                        changed |= EnsureBankTerrainVisual(candidate);
                        continue;
                    }

                    bool isStartGround = candidate.name == "Main Start Ground";
                    bool isSwingPassThroughRail =
                        candidate.TryGetComponent(
                            out SwingPassThroughRailPlatform _);
                    bool isSolidToyBoard =
                        !isSwingPassThroughRail &&
                        (isStartGround ||
                         candidate.TryGetComponent(out SolidSwingSurface _));
                    changed |= RemoveRepeatedBlockVisuals(candidate);
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

        public static void RestoreSceneVisualsAfterReload()
        {
            if (!Application.isPlaying ||
                SceneManager.GetActiveScene().name != "MainStage" ||
                isRestoringAfterReload ||
                lastReloadRestoreFrame == Time.frameCount)
            {
                return;
            }

            lastReloadRestoreFrame = Time.frameCount;
            isRestoringAfterReload = true;
            try
            {
                Apply(FindSceneObject("Main Player"));
            }
            finally
            {
                isRestoringAfterReload = false;
            }
        }

        /// <summary>Rebuilds only the two section-five rail images.</summary>
        public static bool RestoreSectionFiveRailVisuals()
        {
            bool changed = false;
            changed |= EnsureToyVisual(
                FindSceneObject(MainStageSectionFiveSetup.LeftShelfName),
                "Blue Railway Platform Visual",
                RailResourcePath,
                1);
            changed |= EnsureToyVisual(
                FindSceneObject(MainStageSectionFiveSetup.RightShelfName),
                "Blue Railway Platform Visual",
                RailResourcePath,
                1);
            return changed;
        }

        /// <summary>
        /// Restores one main-stage hook independently of the scene-wide visual
        /// pass. HookPoint calls this when it is enabled so a hook cannot keep a
        /// textureless child after scene reloads or Play-mode transitions.
        /// </summary>
        public static bool EnsureHookVisual(GameObject hook)
        {
            if (hook == null ||
                !hook.scene.IsValid() ||
                hook.scene.name != "MainStage" ||
                !hook.TryGetComponent(out HookPoint hookPoint))
            {
                return false;
            }

            bool isBridgeAnchor = hook.TryGetComponent(
                out RopePlatformAnchor _) &&
                hook.name != MainStageSectionNineSetup.CenterHookName;
            Color hookColor = isBridgeAnchor
                ? MainStageSectionFourSetup.BridgeAnchorColor
                : HookColor;

            bool changed = ApplyColor(hook, hookColor);
            if (isBridgeAnchor)
            {
                changed |= RemoveChild(hook, HookVisualName);
                changed |= EnsureBridgeAnchorRingVisual(hook, hookColor);
            }
            else
            {
                changed |= RemoveChild(hook, BridgeAnchorVisualName);
                changed |= EnsurePartDHookRingVisual(hook);
            }

            changed |= hookPoint.ConfigureFixedAttachmentPoint(Vector2.zero);
            return changed;
        }

        private static bool EnsurePartDHookRingVisual(GameObject target)
        {
            if (target == null ||
                !target.TryGetComponent(out SpriteRenderer sourceRenderer))
            {
                return false;
            }

            bool changed = false;
            Transform visualTransform = target.transform.Find(HookVisualName);
            if (visualTransform == null)
            {
                GameObject visualObject = new GameObject(HookVisualName);
                visualTransform = visualObject.transform;
                visualTransform.SetParent(target.transform, false);
                changed = true;
            }
            if (!visualTransform.gameObject.activeSelf)
            {
                visualTransform.gameObject.SetActive(true);
                changed = true;
            }
            if (!visualTransform.TryGetComponent(out SpriteRenderer _))
            {
                visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!visualTransform.TryGetComponent(
                    out HimoHitoHookRingVisual ringVisual))
            {
                ringVisual = visualTransform.gameObject.AddComponent<
                    HimoHitoHookRingVisual>();
                changed = true;
            }

            changed |= ringVisual.Configure(
                sourceRenderer.sortingLayerID,
                sourceRenderer.sortingOrder + 6,
                HookVisualWorldDiameter);
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
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
                   objectName == MainStageSectionSixSetup.MergeName ||
                   objectName == MainStageSectionSevenSetup.GoalFloorName ||
                   objectName == MainStageSectionNineSetup.GoalFloorName ||
                   objectName == MainStageSectionTenSetup.GoalFloorName ||
                   objectName == "Main S01 Start Shelf" ||
                   objectName == "Main S01 Landing" ||
                   objectName == "Main S02 Raised Landing" ||
                   objectName == "Main S03 Lower Dead End" ||
                   objectName == "Main S03 High Shelf";
        }

        private static bool EnsureBankTerrainVisual(GameObject terrain)
        {
            if (terrain == null ||
                !terrain.TryGetComponent(out SpriteRenderer sourceRenderer) ||
                !terrain.TryGetComponent(out BoxCollider2D terrainCollider))
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveChild(terrain, "Orange Block Platform Visual");
            changed |= RemoveChild(terrain, "Blue Railway Platform Visual");
            changed |= RemoveChild(terrain, "Terrain Top Edge");
            changed |= ApplyColor(terrain, TerrainBodyColor);

            Sprite blockSprite =
                TutorialFirstSectionVisuals.LoadProcessedToySprite(
                    BlockResourcePath);
            if (blockSprite == null)
            {
                changed |= RemoveRepeatedBlockVisuals(terrain);
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

            float worldWidth = Mathf.Max(
                0.01f,
                terrainCollider.bounds.size.x);
            int stripCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    worldWidth / MaximumBlockStripWorldWidth));
            Vector2 spriteSize = blockSprite.bounds.size;
            float localStripWidth = terrainCollider.size.x / stripCount;
            float localLeft = terrainCollider.offset.x -
                              terrainCollider.size.x * 0.5f;

            for (int stripIndex = 0;
                 stripIndex < stripCount;
                 stripIndex++)
            {
                string visualName = BlockTileVisualPrefix +
                                    (stripIndex + 1).ToString("D2");
                Vector2 localPosition = new Vector2(
                    localLeft + localStripWidth * (stripIndex + 0.5f),
                    terrainCollider.offset.y);
                Vector2 localScale = new Vector2(
                    localStripWidth /
                    Mathf.Max(0.01f, spriteSize.x),
                    terrainCollider.size.y /
                    Mathf.Max(0.01f, spriteSize.y));
                changed |= EnsureSpriteVisualPart(
                    terrain,
                    visualName,
                    blockSprite,
                    localPosition,
                    localScale,
                    sourceRenderer.sortingOrder + 1);
            }

            changed |= RemoveExcessRepeatedBlockVisuals(
                terrain,
                stripCount);

            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureSpriteVisualPart(
            GameObject parent,
            string partName,
            Sprite sprite,
            Vector2 localPosition,
            Vector2 localScale,
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
            if (!partTransform.gameObject.activeSelf)
            {
                partTransform.gameObject.SetActive(true);
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
            if (parent.TryGetComponent(out SpriteRenderer sourceRenderer) &&
                renderer.sortingLayerID != sourceRenderer.sortingLayerID)
            {
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                changed = true;
            }
            if (renderer.sortingOrder != sortingOrder)
            {
                renderer.sortingOrder = sortingOrder;
                changed = true;
            }

            return changed;
        }

        private static bool RemoveRepeatedBlockVisuals(GameObject parent)
        {
            return RemoveRepeatedBlockVisuals(parent, 0);
        }

        private static bool RemoveExcessRepeatedBlockVisuals(
            GameObject parent,
            int retainedStripCount)
        {
            return RemoveRepeatedBlockVisuals(parent, retainedStripCount);
        }

        private static bool RemoveRepeatedBlockVisuals(
            GameObject parent,
            int retainedStripCount)
        {
            if (parent == null)
            {
                return false;
            }

            bool changed = false;
            for (int childIndex = parent.transform.childCount - 1;
                 childIndex >= 0;
                 childIndex--)
            {
                Transform child = parent.transform.GetChild(childIndex);
                if (!child.name.StartsWith(
                        BlockTileVisualPrefix,
                        System.StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = child.name.Substring(
                    BlockTileVisualPrefix.Length);
                bool keep = int.TryParse(suffix, out int stripNumber) &&
                            stripNumber >= 1 &&
                            stripNumber <= retainedStripCount;
                if (keep)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
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
                // A Play-mode transition can destroy SolidSprite's temporary
                // sprite while leaving the renderer enabled. Reapply the same
                // color to rebuild a visible fallback before exposing it.
                if (target.TryGetComponent(out SolidSprite fallbackVisual))
                {
                    fallbackVisual.Color = fallbackVisual.Color;
                }
                if (!sourceRenderer.enabled)
                {
                    sourceRenderer.enabled = true;
                    return true;
                }

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
            if (!visualTransform.gameObject.activeSelf)
            {
                visualTransform.gameObject.SetActive(true);
                changed = true;
            }

            Vector2 spriteSize = processedSprite.bounds.size;
            Vector3 targetScale = new Vector3(
                1f / Mathf.Max(0.01f, spriteSize.x),
                1f / Mathf.Max(0.01f, spriteSize.y),
                1f);
            Vector3 targetLocalPosition = Vector3.zero;
            if (target.name == MainStageSectionTenSetup.GoalMarkerName &&
                visualName == GoalVisualName)
            {
                // Align only the artwork with the visible wooden surface.
                // Keep the goal trigger and its gameplay position unchanged.
                float floorTop = MainStageSectionTenSetup.GoalFloorPosition.y +
                    MainStageSectionTenSetup.GoalFloorSize.y * 0.5f;
                const float woodSurfaceInset = 0.16f;
                float visualWorldHeight = spriteSize.y * targetScale.y *
                    Mathf.Abs(target.transform.lossyScale.y);
                targetLocalPosition = target.transform.InverseTransformPoint(
                    new Vector3(target.transform.position.x,
                        floorTop - woodSurfaceInset + visualWorldHeight * 0.5f,
                        target.transform.position.z));
            }
            if (visualTransform.localPosition != targetLocalPosition)
            {
                visualTransform.localPosition = targetLocalPosition;
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
            if (!renderer.enabled)
            {
                renderer.enabled = true;
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
            if (!visualTransform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
            if (!visualTransform.TryGetComponent(
                    out RopeAnchorRingVisual ringVisual))
            {
                ringVisual = visualTransform.gameObject.AddComponent<
                    RopeAnchorRingVisual>();
                changed = true;
            }
            int targetOrder = sourceRenderer.sortingOrder + 6;
            changed |= ringVisual.Configure(
                sourceRenderer.sortingLayerID,
                targetOrder);
            if (sourceRenderer.enabled)
            {
                sourceRenderer.enabled = false;
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
            Color foregroundTint = new Color(1f, 1f, 1f, 0.84f);
            if (renderer.color != foregroundTint)
            {
                renderer.color = foregroundTint;
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
            return SceneObjectLookup.Find(objectName);
        }
    }
}
