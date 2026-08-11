using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>
    /// Creates the entire graybox scene from code so every setup decision is reviewable in Git.
    /// </summary>
    [InitializeOnLoad]
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";
        private const float ExperimentalRopeLength = 12f;

        static PrototypeSceneBuilder()
        {
            EditorApplication.delayCall += BuildSceneOnFirstOpen;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("HimoHito/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreatePlayer();
            CreatePlatform("Start Ground", new Vector2(-9f, -5.2f), new Vector2(5f, 0.7f));
            CreatePlatform("Practice Safety Floor", new Vector2(-4.5f, -7.75f), new Vector2(4f, 0.7f));

            // The lower route preserves the established three-section test.
            // The planning branch spends more rope at Hook 2 to reach an upper landing,
            // then offers a closer hook for the final approach.
            CreateHookPoint("Hook 1", new Vector2(-5f, -0.2f), new Vector2(1.6f, 0.45f));
            CreatePlatform("Landing 1", new Vector2(0f, -2.3f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 2", new Vector2(5f, 1.5f), new Vector2(1.6f, 0.45f));
            CreatePlatform("Landing 2", new Vector2(8f, -2f), new Vector2(4f, 0.7f));
            CreatePlatform("Planning Landing", new Vector2(9.5f, -0.4f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 3", new Vector2(13.8f, 3.9f), new Vector2(1.6f, 0.45f));
            CreateHookPoint("Planning Hook", new Vector2(14.8f, 3f), new Vector2(1.6f, 0.45f));
            CreateGoalPlatform("Goal / Landing 3", new Vector2(18.1f, -0.5f), new Vector2(4f, 0.8f));

            GameObject hud = new GameObject("Prototype HUD");
            hud.AddComponent<PrototypeHud>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = GameObject.Find("Player");
            Debug.Log($"HimoHito prototype scene created: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            BuildPrototypeScene();
            EditorApplication.Exit(0);
        }

        private static void BuildSceneOnFirstOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                BuildPrototypeScene();
                return;
            }

            EnsurePlanningBranch();
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += BuildSceneOnFirstOpen;
            }
        }

        private static void EnsurePlanningBranch()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();
            Scene prototypeScene = SceneManager.GetSceneByPath(ScenePath);
            bool openedForUpdate = false;

            if (!prototypeScene.IsValid() || !prototypeScene.isLoaded)
            {
                prototypeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                openedForUpdate = true;
            }

            SceneManager.SetActiveScene(prototypeScene);
            bool changed = false;

            Vector2 planningLandingPosition = new Vector2(9.5f, -0.4f);
            GameObject planningLanding = FindRootObject(prototypeScene, "Planning Landing");
            if (planningLanding == null)
            {
                CreatePlatform("Planning Landing", planningLandingPosition, new Vector2(4f, 0.7f));
                changed = true;
            }
            else if ((Vector2)planningLanding.transform.position != planningLandingPosition)
            {
                planningLanding.transform.position = planningLandingPosition;
                changed = true;
            }

            if (!HasRootObject(prototypeScene, "Planning Hook"))
            {
                CreateHookPoint("Planning Hook", new Vector2(14.8f, 3f), new Vector2(1.6f, 0.45f));
                changed = true;
            }

            changed |= EnsureExperimentalRopeLength(prototypeScene);
            changed |= EnsureHorizontalCameraFollow(prototypeScene);
            changed |= EnsurePracticeSection(prototypeScene);

            if (changed)
            {
                EditorSceneManager.SaveScene(prototypeScene);
                Debug.Log("HimoHito prototype scene updates applied.");
            }

            if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousActiveScene);
            }

            if (openedForUpdate)
            {
                EditorSceneManager.CloseScene(prototypeScene, true);
            }
        }

        private static bool HasRootObject(Scene scene, string objectName)
        {
            return FindRootObject(scene, objectName) != null;
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                if (rootObject.name == objectName)
                {
                    return rootObject;
                }
            }

            return null;
        }

        private static bool EnsureExperimentalRopeLength(Scene scene)
        {
            GameObject player = FindRootObject(scene, "Player");
            if (player == null || !player.TryGetComponent(out RopeResource ropeResource))
            {
                return false;
            }

            SerializedObject serializedResource = new SerializedObject(ropeResource);
            SerializedProperty maximumLength = serializedResource.FindProperty("maximumLength");
            SerializedProperty currentLength = serializedResource.FindProperty("currentLength");
            if (maximumLength == null || currentLength == null)
            {
                return false;
            }

            bool changed = !Mathf.Approximately(maximumLength.floatValue, ExperimentalRopeLength) ||
                           !Mathf.Approximately(currentLength.floatValue, ExperimentalRopeLength);
            if (!changed)
            {
                return false;
            }

            maximumLength.floatValue = ExperimentalRopeLength;
            currentLength.floatValue = ExperimentalRopeLength;
            serializedResource.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool EnsureHorizontalCameraFollow(Scene scene)
        {
            GameObject cameraObject = FindRootObject(scene, "Main Camera");
            if (cameraObject == null || cameraObject.TryGetComponent<HorizontalCameraFollow>(out _))
            {
                return false;
            }

            cameraObject.AddComponent<HorizontalCameraFollow>();
            return true;
        }

        private static bool EnsurePracticeSection(Scene scene)
        {
            Vector2 position = new Vector2(-4.5f, -7.75f);
            Vector2 size = new Vector2(4f, 0.7f);
            GameObject safetyFloor = FindRootObject(scene, "Practice Safety Floor");
            if (safetyFloor == null)
            {
                CreatePlatform("Practice Safety Floor", position, size);
                return true;
            }

            bool changed = false;
            if ((Vector2)safetyFloor.transform.position != position)
            {
                safetyFloor.transform.position = position;
                changed = true;
            }

            Vector3 targetScale = new Vector3(size.x, size.y, 1f);
            if (safetyFloor.transform.localScale != targetScale)
            {
                safetyFloor.transform.localScale = targetScale;
                changed = true;
            }

            return changed;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(3.5f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.052f, 0.11f);
            cameraObject.AddComponent<HorizontalCameraFollow>();
        }

        private static void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(-9f, -4.2f, 0f);
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
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            DistanceJoint2D joint = player.AddComponent<DistanceJoint2D>();
            joint.enabled = false;

            LineRenderer ropeLine = player.AddComponent<LineRenderer>();
            ropeLine.sortingOrder = 5;

            player.AddComponent<RopeResource>();
            player.AddComponent<PlayerMover>();
            player.AddComponent<RopeController>();
            player.AddComponent<PrototypeRunController>();
        }

        private static void CreatePlatform(string name, Vector2 position, Vector2 size)
        {
            CreatePlatformVisual(name, position, size, new Color(0.38f, 0.41f, 0.52f));
        }

        private static void CreateHookPoint(string name, Vector2 position, Vector2 size)
        {
            GameObject hookPoint = CreatePlatformVisual(
                name,
                position,
                size,
                new Color(1f, 0.72f, 0.18f));
            hookPoint.AddComponent<HookPoint>();
        }

        private static void CreateGoalPlatform(string name, Vector2 position, Vector2 size)
        {
            GameObject goal = CreatePlatformVisual(
                name,
                position,
                size,
                new Color(0.28f, 0.9f, 0.58f));
            goal.AddComponent<GoalZone>();
        }

        private static GameObject CreatePlatformVisual(
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject platform = new GameObject(name);
            platform.transform.position = position;
            platform.transform.localScale = new Vector3(size.x, size.y, 1f);

            platform.AddComponent<SpriteRenderer>();
            SolidSprite visual = platform.AddComponent<SolidSprite>();
            visual.Color = color;

            BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            return platform;
        }
    }
}
