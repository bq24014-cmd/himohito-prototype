using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>Rebuilds the currently implemented tutorial sections.</summary>
    [InitializeOnLoad]
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Tutorial.unity";
        private static readonly string[] LegacyObjectNames =
        {
            "Landing 1",
            "Hook 1",
            "Goal / Landing 3",
            "Hook 3",
            "Hook 2",
            TutorialSectionOneSetup.LegacyFlashlightSpotName
        };
        private static readonly Vector2[] LegacyObjectPositions =
        {
            new Vector2(0f, -2.3f),
            new Vector2(-5f, -0.2f),
            new Vector2(22.15f, -0.5f),
            new Vector2(13.8f, 3.9f),
            new Vector2(5f, 1.5f),
            new Vector2(-4.25f, -3.1f)
        };

        static PrototypeSceneBuilder()
        {
            EditorApplication.delayCall +=
                RemoveLegacyObjectsFromOpenTutorial;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("HimoHito/Rebuild Tutorial")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject player = CreatePlayer(
                TutorialSectionOneSetup.StartRespawnPosition);

            BuildImplementedSections();
            TutorialSectionGuide.EnsureSigns();

            CreateCamera(player.transform);
            GameObject hud = new("Tutorial HUD");
            hud.AddComponent<PrototypeHud>();
            hud.AddComponent<StageOverlayControls>();
            TutorialFirstSectionVisuals.Apply(player);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;
            Debug.Log($"HimoHito tutorial rebuilt through finale: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            BuildPrototypeScene();
            EditorApplication.Exit(0);
        }

        private static void BuildImplementedSections()
        {
            TutorialSectionOneSetup.EnsureCreated();
            TutorialSectionTwoSetup.EnsureCreated();
            TutorialSectionThreeSetup.EnsureCreated();
            TutorialSectionFourSetup.EnsureCreated();
        }

        private static void OnPlayModeStateChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall +=
                    RemoveLegacyObjectsFromOpenTutorial;
            }
        }

        [MenuItem("HimoHito/Clean Tutorial Legacy Objects")]
        public static void RemoveLegacyObjectsFromOpenTutorial()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                return;
            }

            bool changed = TutorialSectionOneSetup.ApplyCurrentScene();
            changed |= TutorialSectionTwoSetup.ApplyCurrentScene();
            changed |= TutorialSectionThreeSetup.ApplyCurrentScene();
            changed |= TutorialSectionFourSetup.ApplyCurrentScene();
            changed |= TutorialSectionGuide.EnsureSigns();
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                changed |= TutorialFirstSectionVisuals.Apply(player);
            }
            GameObject[] roots = scene.GetRootGameObjects();
            for (int legacyIndex = 0;
                 legacyIndex < LegacyObjectNames.Length;
                 legacyIndex++)
            {
                foreach (GameObject root in roots)
                {
                    if (root == null ||
                        root.name != LegacyObjectNames[legacyIndex] ||
                        Vector2.Distance(
                            root.transform.position,
                            LegacyObjectPositions[legacyIndex]) > 0.01f)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(root);
                    changed = true;
                    break;
                }
            }

            if (!changed)
            {
                return;
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("チュートリアルT1～最終区間の配置と旧オブジェクトを同期しました。");
        }

        private static GameObject CreatePlayer(Vector2 position)
        {
            GameObject player = new("Player");
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
            serialized.FindProperty("maximumLength").floatValue =
                PrototypeRunController.TutorialRopeCapacity;
            serialized.FindProperty("currentLength").floatValue =
                PrototypeRunController.TutorialRopeCapacity;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            player.AddComponent<RopePlatformBuilder>();
            player.AddComponent<PlayerMover>();
            RopeController rope = player.AddComponent<RopeController>();
            rope.RestoreSelectedRopeLength(6);
            player.AddComponent<PrototypeRunController>();
            player.AddComponent<TutorialSectionGuide>();
            return player;
        }

        private static void CreateCamera(Transform player)
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
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/MainStage.unity", true)
            };
        }
    }
}
