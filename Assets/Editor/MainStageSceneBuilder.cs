using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>
    /// Builds only the agreed portion of MainStage. Tutorial.unity is never opened or changed.
    /// </summary>
    public static class MainStageSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainStage.unity";

        private const float MainStageRopeLength = 28f;

        [MenuItem("HimoHito/Build Main Stage Through Section 10")]
        public static void BuildMainStageThroughSectionTen()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject player = CreatePlayer(new Vector2(-6f, -4.2f));
            CreatePlatform(
                "Main Landing 1",
                new Vector2(4.5f, -3.2f),
                new Vector2(4.2f, 0.7f));

            CreatePlatform(
                "Main Walkway 1",
                new Vector2(10.1f, -3.2f),
                new Vector2(7f, 0.7f));

            CreateHookPoint(
                "Main Hook 2",
                new Vector2(18.2f, 0.7f),
                new Vector2(1.6f, 0.45f));

            GameObject landingTwo = CreatePlatform(
                "Main Landing 2",
                new Vector2(27.1f, -2.2f),
                new Vector2(11.2f, 0.7f));

            CreateHookPoint(
                "Main Hook 3",
                new Vector2(36f, 2.2f),
                new Vector2(1.6f, 0.45f));

            CreatePlatform(
                "Main Landing 3",
                new Vector2(47.425f, -0.5f),
                new Vector2(5.75f, 0.7f));

            CreateSectionsFourAndFive();
            CreateSectionSix();
            CreateSectionSeven();
            CreateSectionEight();
            CreateSectionNine();
            GameObject sectionTenTarget = CreateSectionTen();

            CreateCamera(player.transform, sectionTenTarget.transform);
            CreatePlatform(
                "Main Start Ground",
                new Vector2(-6f, -5.2f),
                new Vector2(5.5f, 0.7f));
            CreateHookPoint(
                "Main Hook 1",
                new Vector2(-1f, 0f),
                new Vector2(1.6f, 0.45f));
            new GameObject("Main Stage HUD").AddComponent<MainStageHud>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;
            Debug.Log($"HimoHito main-stage sections 1-10 created: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            BuildMainStageThroughSectionTen();
            EditorApplication.Exit(0);
        }

        [MenuItem("HimoHito/Add Main Stage Section 6")]
        public static void AddSectionSix()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionSixObjects();
            GameObject sectionSixTarget = CreateSectionSix();

            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, sectionSixTarget.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("HimoHito main-stage section 6 added.");
        }

        public static void AddSectionSixFromCommandLine()
        {
            AddSectionSix();
            EditorApplication.Exit(0);
        }

        [MenuItem("HimoHito/Add Main Stage Section 7")]
        public static void AddSectionSeven()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionSevenObjects();
            GameObject sectionSevenTarget = CreateSectionSeven();

            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, sectionSevenTarget.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("HimoHito main-stage section 7 added.");
        }

        [MenuItem("HimoHito/Add Main Stage Section 8")]
        public static void AddSectionEight()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionEightObjects();
            GameObject sectionEightTarget = CreateSectionEight();

            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, sectionEightTarget.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("HimoHito main-stage section 8 added.");
        }

        [MenuItem("HimoHito/Add Main Stage Section 9")]
        public static void AddSectionNine()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionNineObjects();
            GameObject sectionNineTarget = CreateSectionNine();

            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, sectionNineTarget.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("HimoHito main-stage section 9 added.");
        }

        [MenuItem("HimoHito/Add Main Stage Section 10")]
        public static void AddSectionTen()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionTenObjects();
            GameObject sectionTenTarget = CreateSectionTen();

            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, sectionTenTarget.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("HimoHito main-stage section 10 added.");
        }

        public static void AddSectionsFourAndFiveFromCommandLine()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveSectionFourAndFiveObjects();

            GameObject landingThree = GameObject.Find("Main Landing 3");
            if (landingThree != null &&
                landingThree.TryGetComponent(out MainStageSectionTarget oldTarget))
            {
                Object.DestroyImmediate(oldTarget);
            }

            GameObject midpoint = CreateSectionsFourAndFive();
            RopeResource ropeResource = Object.FindFirstObjectByType<RopeResource>();
            MainStagePreview preview = Object.FindFirstObjectByType<MainStagePreview>();
            if (ropeResource != null && preview != null)
            {
                preview.Configure(ropeResource.transform, midpoint.transform);
                EditorUtility.SetDirty(preview);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.Exit(0);
        }

        private static GameObject CreateSectionsFourAndFive()
        {
            CreateHookPoint(
                "Main Upper Route Hook",
                new Vector2(57f, 4.8f),
                new Vector2(1.6f, 0.45f));
            CreatePlatform(
                "Main Upper Route Landing",
                new Vector2(64.5f, 2.2f),
                new Vector2(6f, 0.7f));
            GameObject upperDescent = CreatePlatform(
                "Main Upper Route Descent",
                new Vector2(69.7f, 0f),
                new Vector2(6f, 0.7f));
            MainStageCheckpoint upperCheckpoint =
                upperDescent.AddComponent<MainStageCheckpoint>();
            upperCheckpoint.Configure(new Vector2(69.7f, 1.05f), 3, true);
            CreatePlatform(
                "Main Lower Walking Route",
                new Vector2(58.5f, -3.3f),
                new Vector2(16.4f, 0.7f));

            GameObject midpoint = CreatePlatform(
                "Main Midpoint Checkpoint",
                new Vector2(71.7f, -2.7f),
                new Vector2(10f, 0.7f));
            MainStageCheckpoint checkpoint = midpoint.AddComponent<MainStageCheckpoint>();
            checkpoint.Configure(new Vector2(69.7f, -1.65f), 0, false);
            return midpoint;
        }

        private static GameObject CreateSectionSix()
        {
            return MainStageSectionSixSetup.EnsureCreated();
        }

        private static GameObject CreateSectionSeven()
        {
            return MainStageSectionSevenSetup.EnsureCreated();
        }

        private static GameObject CreateSectionEight()
        {
            return MainStageSectionEightSetup.EnsureCreated();
        }

        private static GameObject CreateSectionNine()
        {
            return MainStageSectionNineSetup.EnsureCreated();
        }

        private static GameObject CreateSectionTen()
        {
            return MainStageSectionTenSetup.EnsureCreated();
        }

        private static void RemoveSectionFourAndFiveObjects()
        {
            string[] objectNames =
            {
                "Main Upper Route Hook",
                "Main Upper Route Landing",
                "Main Upper Route Descent",
                "Main Lower Walking Route",
                "Main Midpoint Checkpoint"
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void RemoveSectionSixObjects()
        {
            string[] objectNames =
            {
                "Main Section 6 Hook",
                "Main Section 6 Rope Hazard",
                "Main Section 6 Landing"
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void RemoveSectionSevenObjects()
        {
            string[] objectNames =
            {
                "Main Section 7 Hook",
                "Main Section 7 Weave Frame",
                "Main Section 7 Woven Platform",
                "Main Section 7 Weave Marker",
                "Main Section 7 Landing"
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void RemoveSectionEightObjects()
        {
            GameObject sectionSevenLanding = GameObject.Find(MainStageSectionSevenSetup.LandingName);
            if (sectionSevenLanding != null &&
                sectionSevenLanding.TryGetComponent(out MainStageSectionEightCheckpoint checkpoint))
            {
                Object.DestroyImmediate(checkpoint);
            }

            string[] objectNames =
            {
                MainStageSectionEightSetup.FirstHookName,
                MainStageSectionEightSetup.PlanningLandingName,
                MainStageSectionEightSetup.SecondHookName,
                MainStageSectionEightSetup.FinalLandingName
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void RemoveSectionNineObjects()
        {
            GameObject sectionEightLanding = GameObject.Find(MainStageSectionEightSetup.FinalLandingName);
            if (sectionEightLanding != null &&
                sectionEightLanding.TryGetComponent(out MainStageSectionNineCheckpoint checkpoint))
            {
                Object.DestroyImmediate(checkpoint);
            }

            string[] objectNames =
            {
                MainStageSectionNineSetup.HookName,
                MainStageSectionNineSetup.MovingHazardName,
                MainStageSectionNineSetup.LandingName
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void RemoveSectionTenObjects()
        {
            GameObject sectionNineLanding = GameObject.Find(MainStageSectionNineSetup.LandingName);
            if (sectionNineLanding != null &&
                sectionNineLanding.TryGetComponent(out MainStageSectionTenCheckpoint checkpoint))
            {
                Object.DestroyImmediate(checkpoint);
            }

            string[] objectNames =
            {
                MainStageSectionTenSetup.HookName,
                MainStageSectionTenSetup.GoalName
            };

            foreach (string objectName in objectNames)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void CreateCamera(Transform player, Transform previewTarget)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(player.position.x, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.052f, 0.11f);
            cameraObject.AddComponent<HorizontalCameraFollow>();
            MainStagePreview preview = cameraObject.AddComponent<MainStagePreview>();
            preview.Configure(player, previewTarget);
        }

        private static GameObject CreatePlayer(Vector2 position)
        {
            GameObject player = new GameObject("Main Player");
            player.transform.position = position;
            player.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            SolidSprite visual = player.AddComponent<SolidSprite>();
            visual.Color = new Color(0.33f, 1f, 0.76f);

            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.edgeRadius = 0.08f;

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.8f;
            body.mass = 1f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            DistanceJoint2D joint = player.AddComponent<DistanceJoint2D>();
            joint.enabled = false;

            LineRenderer ropeLine = player.AddComponent<LineRenderer>();
            ropeLine.sortingOrder = 5;

            RopeResource ropeResource = player.AddComponent<RopeResource>();
            ConfigureMainStageRopeLength(ropeResource);
            player.AddComponent<WeaveResource>();
            player.AddComponent<PlayerMover>();
            player.AddComponent<RopeController>();
            player.AddComponent<MainStageRespawnOnFall>();
            return player;
        }

        private static void ConfigureMainStageRopeLength(RopeResource ropeResource)
        {
            SerializedObject serializedResource = new SerializedObject(ropeResource);
            serializedResource.FindProperty("maximumLength").floatValue = MainStageRopeLength;
            serializedResource.FindProperty("currentLength").floatValue = MainStageRopeLength;
            serializedResource.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePlatform(string name, Vector2 position, Vector2 size)
        {
            return CreateSolidObject(name, position, size, new Color(0.38f, 0.41f, 0.52f));
        }

        private static void CreateHookPoint(string name, Vector2 position, Vector2 size)
        {
            GameObject hook = CreateSolidObject(
                name,
                position,
                size,
                new Color(1f, 0.72f, 0.18f));
            hook.AddComponent<HookPoint>();
        }

        private static GameObject CreateSolidObject(
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            gameObject.AddComponent<SpriteRenderer>();
            SolidSprite visual = gameObject.AddComponent<SolidSprite>();
            visual.Color = color;
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            return gameObject;
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
