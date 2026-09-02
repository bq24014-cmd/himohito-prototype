using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>
    /// Rebuilds the implemented part of the main stage from the 2026-08-29
    /// stage manual. Section eight was removed after playtesting, so section
    /// seven connects directly to the implemented section nine.
    /// </summary>
    public static class MainStageSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainStage.unity";
        private const float MainStageRopeLength = 50f;
        private static readonly string[] ObsoleteMainStageObjectNames =
        {
            "Main Section 4 Bridge Start Marker",
            "Main Section 4 Intermediate Anchor",
            "Main Section 6 Flashlight Spot",
            "Main Section 6 Hook",
            "Main Section 6 Landing",
            "Main Section 6 Rope Hazard",
            "Main S06 Safety Floor",
            "Main S06 Return Step",
            "Main Midpoint Checkpoint",
            "Main Upper Route Hook",
            "Main Upper Route Landing",
            "Main Upper Route Descent",
            "Main Upper Route Descent Marker",
            "Main Lower Walking Route",
            "Main Section 7 Hook",
            "Main Section 7 Final Hook",
            "Main Section 7 Landing",
            "Main Section 7 Weave Frame",
            "Main Section 7 Woven Platform",
            "Main Section 7 Weave Marker",
            "Main S07 Lower Start",
            "Main Section 8 Hook A",
            "Main Section 8 Planning Landing",
            "Main Section 8 Hook B",
            "Main Section 8 Legacy Landing",
            "Main Section 8 Hook",
            "Main Section 8 Flashlight Spot",
            "Main Section 8 Landing",
            "Main Section 9 Hook",
            "Main Section 9 Final Hook",
            "Main Section 9 Flashlight Spot",
            "Main Section 9 Flashlight Beam",
            "Main Section 9 Moving Hazard",
            "Main Section 9 Landing",
            "Main Stage Goal",
            "Main Section 10 Final Hook"
        };
        private static readonly Color TerrainColor = new(0.96f, 0.55f, 0.18f);
        private static readonly Color HookColor = new(0.30f, 0.76f, 1f);
        private static readonly Color SpikeColor = new(1f, 0.18f, 0.25f);

        [MenuItem("HimoHito/Rebuild Main Stage Through Section 9")]
        public static void BuildMainStageThroughCurrentSection()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject player = CreatePlayer(new Vector2(-1.6f, 0.95f));

            BuildSection1();
            BuildSection2();
            BuildSection3();
            BuildSection4();
            BuildSection5();
            BuildSection6();
            BuildSection7();
            GameObject previewTarget = BuildSection9();

            CreateCamera(player.transform, previewTarget.transform);
            GameObject hud = new("Main Stage HUD");
            hud.AddComponent<MainStageHud>();
            hud.AddComponent<StageOverlayControls>();
            MainStageVisuals.Apply(player);
            MainStageFloorCollisionSetup.ApplyCurrentScene();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;
            Debug.Log($"HimoHito 0829 main stage rebuilt: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            BuildMainStageThroughCurrentSection();
            EditorApplication.Exit(0);
        }

        [MenuItem("HimoHito/Clean Main Stage Legacy Objects")]
        public static void CleanMainStageLegacyObjects()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
            }

            int removedObjectCount = 0;
            int removedMissingScriptCount = 0;
            GameObject[] sceneObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (GameObject sceneObject in sceneObjects)
            {
                if (sceneObject == null || sceneObject.scene != scene)
                {
                    continue;
                }

                int missingScriptCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        sceneObject);
                if (missingScriptCount > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(
                        sceneObject);
                    removedMissingScriptCount += missingScriptCount;
                }

                if (IsObsoleteMainStageObject(sceneObject.name))
                {
                    Object.DestroyImmediate(sceneObject);
                    removedObjectCount++;
                }
            }

            MainStageFloorCollisionSetup.ApplyCurrentScene();
            RopeResource player = Object.FindFirstObjectByType<RopeResource>();
            if (player != null)
            {
                MainStageVisuals.Apply(player.gameObject);
            }

            bool sceneChanged = scene.isDirty ||
                                removedObjectCount > 0 ||
                                removedMissingScriptCount > 0;
            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"MainStage cleanup complete: removed {removedObjectCount} " +
                $"legacy objects and {removedMissingScriptCount} missing scripts; " +
                $"saved={sceneChanged}.");
        }

        public static void CleanFromCommandLine()
        {
            CleanMainStageLegacyObjects();
            EditorApplication.Exit(0);
        }

        private static bool IsObsoleteMainStageObject(string objectName)
        {
            foreach (string obsoleteName in ObsoleteMainStageObjectNames)
            {
                if (objectName == obsoleteName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void BuildSection1()
        {
            // Slide 20 of the 0829 manual: two level banks with a 7-unit
            // valley, a centered Hook, and a comfortable length-7 opening.
            // Both banks extend down like terrain so the collision silhouette
            // is readable before the player starts swinging.
            CreateTerrain(
                "Main S01 Start Shelf",
                new Vector2(-5f, -4.65f),
                new Vector2(8f, 10f));
            CreateHook("Main S01 Hook", new Vector2(2.5f, 6.2f));
            GameObject landing = CreateTerrain(
                "Main S01 Landing",
                new Vector2(9.5f, -4.65f),
                new Vector2(8f, 10f));
            AddCheckpoint(landing, 2, new Vector2(6.1f, 0.95f), 50f);
        }

        private static void BuildSection2()
        {
            // Slide 21: a 7.5-unit valley with a centered Hook. Length 5
            // cannot reach the raised bank, length 6 clears both spikes, and
            // length 7 or more falls low enough to hit the 0.9-unit spike.
            const float valleyBaseline = -3.15f;
            CreateHook(
                "Main S02 Length Window Hook",
                new Vector2(17.25f, valleyBaseline + 7.5f));
            CreateSpike(
                "Main S02 Front Spike",
                new Vector2(15.35f, valleyBaseline + 0.45f),
                new Vector2(0.9f, 0.9f));
            CreateSpike(
                "Main S02 Far Spike",
                new Vector2(19.55f, valleyBaseline + 0.15f),
                new Vector2(0.7f, 0.3f));
            GameObject landing = CreateTerrain(
                "Main S02 Raised Landing",
                new Vector2(29f, valleyBaseline - 1f),
                new Vector2(8f, 10f));
            AddCheckpoint(
                landing,
                3,
                new Vector2(29f, valleyBaseline + 4.65f),
                50f);
        }

        private static void BuildSection3()
        {
            CreateHook(
                MainStageSectionThreeSetup.LowerHookName,
                MainStageSectionThreeSetup.LowerHookPosition);
            CreateHook(
                MainStageSectionThreeSetup.UpperHookName,
                MainStageSectionThreeSetup.UpperHookPosition);
            CreateTerrain(
                MainStageSectionThreeSetup.LowDeadEndName,
                MainStageSectionThreeSetup.LowDeadEndPosition,
                MainStageSectionThreeSetup.LowDeadEndSize);
            CreateTerrain(
                MainStageSectionThreeSetup.ReturnStepAName,
                MainStageSectionThreeSetup.ReturnStepAPosition,
                MainStageSectionThreeSetup.ReturnStepASize);
            CreateTerrain(
                MainStageSectionThreeSetup.ReturnStepBName,
                MainStageSectionThreeSetup.ReturnStepBPosition,
                MainStageSectionThreeSetup.ReturnStepBSize);
            CreateTerrain(
                MainStageSectionThreeSetup.ReturnStepCName,
                MainStageSectionThreeSetup.ReturnStepCPosition,
                MainStageSectionThreeSetup.ReturnStepCSize);
            CreateTerrain(
                MainStageSectionThreeSetup.ReturnStepDName,
                MainStageSectionThreeSetup.ReturnStepDPosition,
                MainStageSectionThreeSetup.ReturnStepDSize);
            GameObject landing = CreateTerrain(
                MainStageSectionThreeSetup.HighShelfName,
                MainStageSectionThreeSetup.HighShelfPosition,
                MainStageSectionThreeSetup.HighShelfSize);
            AddCheckpoint(
                landing,
                4,
                MainStageSectionThreeSetup.HighShelfRespawnPosition,
                45f);
        }

        private static void BuildSection4()
        {
            CreateTerrain(
                MainStageSectionFourSetup.IntermediateColumnName,
                MainStageSectionFourSetup.IntermediateColumnPosition,
                MainStageSectionFourSetup.IntermediateColumnSize);
            GameObject bridgeStartAnchor = CreateSolidObject(
                MainStageSectionFourSetup.BridgeStartMarkerName,
                MainStageSectionFourSetup.BridgeStartMarkerPosition,
                MainStageSectionFourSetup.BridgeStartMarkerSize,
                MainStageSectionFourSetup.BridgeAnchorColor);
            bridgeStartAnchor.AddComponent<HookPoint>()
                .ConfigureFixedAttachmentPoint(Vector2.zero);
            bridgeStartAnchor.GetComponent<BoxCollider2D>().enabled = false;
            bridgeStartAnchor.AddComponent<RopePlatformAnchor>().Configure(
                MainStageSectionFourSetup.BridgeRopeLength,
                MainStageSectionFourSetup.BridgeAnchorName);
            GameObject bridgeAnchor = CreateSolidObject(
                MainStageSectionFourSetup.BridgeAnchorName,
                MainStageSectionFourSetup.BridgeAnchorPosition,
                MainStageSectionFourSetup.BridgeAnchorSize,
                MainStageSectionFourSetup.BridgeAnchorColor);
            bridgeAnchor.AddComponent<HookPoint>()
                .ConfigureFixedAttachmentPoint(Vector2.zero);
            bridgeAnchor.AddComponent<RopePlatformAnchor>().Configure(
                MainStageSectionFourSetup.BridgeRopeLength,
                MainStageSectionFourSetup.BridgeStartMarkerName);
            CreateHook(
                MainStageSectionFourSetup.FarHookName,
                MainStageSectionFourSetup.FarHookPosition);
            GameObject landing = CreateTerrain(
                MainStageSectionFourSetup.LandingName,
                MainStageSectionFourSetup.LandingPosition,
                MainStageSectionFourSetup.LandingSize);
            AddCheckpoint(
                landing,
                5,
                MainStageSectionFourSetup.LandingRespawnPosition,
                45f);
        }

        private static void BuildSection5()
        {
            MainStageSectionFiveSetup.ApplyCurrentScene();
        }

        private static void BuildSection6()
        {
            MainStageSectionSixSetup.ApplyCurrentScene();
        }

        private static GameObject BuildSection7()
        {
            return MainStageSectionSevenSetup.EnsureCreated();
        }

        private static GameObject BuildSection9()
        {
            return MainStageSectionNineSetup.EnsureCreated();
        }

        private static GameObject CreatePlayer(Vector2 position)
        {
            GameObject player = new("Main Player");
            player.transform.position = position;
            player.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            player.AddComponent<SolidSprite>().Color = new Color(1f, 0.365f, 0.561f);
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.edgeRadius = 0f;
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.8f;
            body.mass = 1f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<DistanceJoint2D>().enabled = false;
            player.AddComponent<LineRenderer>().sortingOrder = 5;
            RopeResource resource = player.AddComponent<RopeResource>();
            SerializedObject serialized = new(resource);
            serialized.FindProperty("maximumLength").floatValue = MainStageRopeLength;
            serialized.FindProperty("currentLength").floatValue = MainStageRopeLength;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            player.AddComponent<RopePlatformBuilder>();
            player.AddComponent<PlayerMover>();
            RopeController ropeController = player.AddComponent<RopeController>();
            ropeController.RestoreSelectedRopeLength(7);
            player.AddComponent<MainStageRespawnOnFall>();
            return player;
        }

        private static void CreateCamera(Transform player, Transform goal)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(player.position.x, 2f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.052f, 0.11f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<HorizontalCameraFollow>();
            MainStagePreview preview = cameraObject.AddComponent<MainStagePreview>();
            preview.Configure(player, goal);
        }

        private static GameObject CreateTerrain(string name, Vector2 position, Vector2 size)
        {
            GameObject terrain = CreateSolidObject(name, position, size, TerrainColor);
            Rigidbody2D body = terrain.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            terrain.AddComponent<SolidSwingSurface>();
            return terrain;
        }

        private static GameObject CreateHook(string name, Vector2 position)
        {
            GameObject hook = CreateSolidObject(name, position, new Vector2(1.6f, 0.45f), HookColor);
            hook.AddComponent<HookPoint>().ConfigureFixedAttachmentPoint(Vector2.zero);
            return hook;
        }

        private static GameObject CreateSpike(string name, Vector2 position, Vector2 size)
        {
            GameObject spike = CreateSolidObject(name, position, size, SpikeColor);
            BoxCollider2D collider = spike.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(0.86f, 0.9f);
            collider.offset = new Vector2(0f, -0.03f);
            collider.isTrigger = true;
            spike.AddComponent<RopeSpikeHazard>();
            spike.AddComponent<ToySpikeVisual>().Refresh();
            return spike;
        }

        private static GameObject CreateSolidObject(
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            gameObject.AddComponent<SpriteRenderer>();
            gameObject.AddComponent<SolidSprite>().Color = color;
            gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
            return gameObject;
        }

        private static void AddCheckpoint(
            GameObject floor,
            int section,
            Vector2 respawn,
            float lowerBound)
        {
            MainStageCheckpoint checkpoint = floor.AddComponent<MainStageCheckpoint>();
            checkpoint.Configure(section, respawn, lowerBound);
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Tutorial.unity", true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
