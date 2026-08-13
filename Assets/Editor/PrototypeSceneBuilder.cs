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
        private static readonly Color RopeReleaseHazardColor = new Color(0.95f, 0.28f, 0.35f);

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
            CreatePlatform("Practice Safety Floor", new Vector2(-4.5f, -7.25f), new Vector2(4f, 0.7f));
            HookPoint hook1 = CreateHookPoint(
                "Hook 1",
                new Vector2(-5f, -0.2f),
                new Vector2(1.6f, 0.45f));
            CreateRopeReleaseHazard(
                "Practice Long Rope Obstacle",
                new Vector2(1.5f, -4.5f),
                new Vector2(0.6f, 2f),
                hook1);

            // The lower route preserves the established three-section test.
            // The planning branch spends more rope at Hook 2 to reach an upper landing,
            // then offers a closer hook for the final approach.
            CreatePlatform("Landing 1", new Vector2(0f, -2.3f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 2", new Vector2(5f, 1.5f), new Vector2(1.6f, 0.45f));
            CreatePlatform("Landing 2", new Vector2(8f, -2f), new Vector2(4f, 0.7f));
            CreatePlatform("Planning Landing", new Vector2(9.5f, -0.4f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 3", new Vector2(13.8f, 3.9f), new Vector2(1.6f, 0.45f));
            CreateHookPoint("Planning Hook", new Vector2(14.8f, 3f), new Vector2(1.6f, 0.45f));
            GameObject wovenPlatform = CreateWovenPlatform(
                "Tutorial Woven Platform",
                new Vector2(19.1f, -0.5f),
                new Vector2(4f, 0.8f));
            CreateWeaveFrame(
                "Tutorial Weave Frame",
                new Vector2(8f, -0.7f),
                new Vector2(4f, 3f),
                wovenPlatform);
            CreateGoalPlatform("Goal / Landing 3", new Vector2(24.15f, -0.5f), new Vector2(4f, 0.8f));

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
            changed |= EnsureWeaveExperiment(prototypeScene);

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

        private static bool EnsureWeaveExperiment(Scene scene)
        {
            bool changed = false;
            GameObject player = FindRootObject(scene, "Player");
            if (player != null && !player.TryGetComponent(out WeaveResource _))
            {
                player.AddComponent<WeaveResource>();
                changed = true;
            }

            const string platformName = "Tutorial Woven Platform";
            Vector2 platformPosition = new Vector2(19.1f, -0.5f);
            Vector2 platformSize = new Vector2(4f, 0.8f);
            GameObject wovenPlatform = FindRootObject(scene, platformName);
            if (wovenPlatform == null)
            {
                wovenPlatform = CreateWovenPlatform(platformName, platformPosition, platformSize);
                changed = true;
            }
            else
            {
                changed |= ApplyTransform(wovenPlatform, platformPosition, platformSize);
            }

            const string frameName = "Tutorial Weave Frame";
            Vector2 framePosition = new Vector2(8f, -0.7f);
            Vector2 frameSize = new Vector2(4f, 3f);
            GameObject frameObject = FindRootObject(scene, frameName);
            if (frameObject == null)
            {
                CreateWeaveFrame(frameName, framePosition, frameSize, wovenPlatform);
                changed = true;
            }
            else
            {
                changed |= ApplyTransform(frameObject, framePosition, frameSize);
                WeaveFrame frame = frameObject.GetComponent<WeaveFrame>();
                if (frame == null)
                {
                    frame = frameObject.AddComponent<WeaveFrame>();
                    changed = true;
                }

                SerializedObject serializedFrame = new SerializedObject(frame);
                SerializedProperty platformProperty = serializedFrame.FindProperty("wovenPlatform");
                SerializedProperty costProperty = serializedFrame.FindProperty("requiredThreads");
                if (platformProperty.objectReferenceValue != wovenPlatform || costProperty.intValue != 2)
                {
                    platformProperty.objectReferenceValue = wovenPlatform;
                    costProperty.intValue = 2;
                    serializedFrame.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }

                if (!frameObject.TryGetComponent(out BoxCollider2D frameTrigger))
                {
                    frameTrigger = frameObject.AddComponent<BoxCollider2D>();
                    frameTrigger.isTrigger = true;
                    frameTrigger.size = Vector2.one;
                    changed = true;
                }
            }

            GameObject goal = FindRootObject(scene, "Goal / Landing 3");
            if (goal != null)
            {
                changed |= ApplyTransform(goal, new Vector2(24.15f, -0.5f), new Vector2(4f, 0.8f));
            }

            if (wovenPlatform.activeSelf)
            {
                wovenPlatform.SetActive(false);
                changed = true;
            }

            return changed;
        }

        private static bool EnsurePracticeSection(Scene scene)
        {
            bool changed = false;
            GameObject hookObject = FindRootObject(scene, "Hook 1");
            HookPoint hook1 = hookObject != null ? hookObject.GetComponent<HookPoint>() : null;
            changed |= EnsurePlatform(
                scene,
                "Practice Safety Floor",
                new Vector2(-4.5f, -7.25f),
                new Vector2(4f, 0.7f));
            changed |= EnsureRopeReleaseHazard(
                scene,
                "Practice Long Rope Obstacle",
                new Vector2(1.5f, -4.5f),
                new Vector2(0.6f, 2f),
                hook1);
            return changed;
        }

        private static bool EnsureRopeReleaseHazard(
            Scene scene,
            string objectName,
            Vector2 position,
            Vector2 size,
            HookPoint affectedHook)
        {
            bool changed = EnsurePlatform(scene, objectName, position, size);
            GameObject hazard = FindRootObject(scene, objectName);
            if (hazard == null)
            {
                return changed;
            }

            if (hazard.TryGetComponent(out SolidSprite visual) &&
                visual.Color != RopeReleaseHazardColor)
            {
                visual.Color = RopeReleaseHazardColor;
                changed = true;
            }

            if (!hazard.TryGetComponent(out RopeReleaseHazard releaseHazard))
            {
                releaseHazard = hazard.AddComponent<RopeReleaseHazard>();
                changed = true;
            }

            if (releaseHazard.AffectedHook != affectedHook)
            {
                releaseHazard.Configure(affectedHook);
                EditorUtility.SetDirty(releaseHazard);
                changed = true;
            }

            bool hasTrigger = false;
            foreach (BoxCollider2D collider in hazard.GetComponents<BoxCollider2D>())
            {
                if (collider.isTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }

            if (!hasTrigger)
            {
                BoxCollider2D trigger = hazard.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = Vector2.one;
                changed = true;
            }

            return changed;
        }

        private static bool EnsurePlatform(
            Scene scene,
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            GameObject platform = FindRootObject(scene, objectName);
            if (platform == null)
            {
                CreatePlatform(objectName, position, size);
                return true;
            }

            return ApplyTransform(platform, position, size);
        }

        private static bool ApplyTransform(GameObject gameObject, Vector2 position, Vector2 size)
        {
            bool changed = false;
            if ((Vector2)gameObject.transform.position != position)
            {
                gameObject.transform.position = position;
                changed = true;
            }

            Vector3 targetScale = new Vector3(size.x, size.y, 1f);
            if (gameObject.transform.localScale != targetScale)
            {
                gameObject.transform.localScale = targetScale;
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
            player.AddComponent<WeaveResource>();
            player.AddComponent<PlayerMover>();
            player.AddComponent<RopeController>();
            player.AddComponent<PrototypeRunController>();
        }

        private static void CreatePlatform(string name, Vector2 position, Vector2 size)
        {
            CreatePlatformVisual(name, position, size, new Color(0.38f, 0.41f, 0.52f));
        }

        private static void CreateRopeReleaseHazard(
            string name,
            Vector2 position,
            Vector2 size,
            HookPoint affectedHook)
        {
            GameObject hazard = CreatePlatformVisual(name, position, size, RopeReleaseHazardColor);
            BoxCollider2D trigger = hazard.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;
            RopeReleaseHazard releaseHazard = hazard.AddComponent<RopeReleaseHazard>();
            releaseHazard.Configure(affectedHook);
        }

        private static HookPoint CreateHookPoint(string name, Vector2 position, Vector2 size)
        {
            GameObject hookPoint = CreatePlatformVisual(
                name,
                position,
                size,
                new Color(1f, 0.72f, 0.18f));
            return hookPoint.AddComponent<HookPoint>();
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

        private static GameObject CreateWovenPlatform(string name, Vector2 position, Vector2 size)
        {
            GameObject platform = CreatePlatformVisual(
                name,
                position,
                size,
                new Color(0.72f, 0.42f, 1f));
            platform.SetActive(false);
            return platform;
        }

        private static void CreateWeaveFrame(
            string name,
            Vector2 position,
            Vector2 size,
            GameObject wovenPlatform)
        {
            GameObject frameObject = new GameObject(name);
            frameObject.transform.position = position;
            frameObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            BoxCollider2D trigger = frameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = Vector2.one;

            WeaveFrame frame = frameObject.AddComponent<WeaveFrame>();
            frame.Configure(wovenPlatform, 2);

            GameObject marker = CreatePlatformVisual(
                "Weave Frame Marker",
                position + new Vector2(1.7f, 0.1f),
                new Vector2(0.25f, 1.6f),
                new Color(0.72f, 0.42f, 1f));
            marker.GetComponent<BoxCollider2D>().enabled = false;
            marker.transform.SetParent(frameObject.transform, true);
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
