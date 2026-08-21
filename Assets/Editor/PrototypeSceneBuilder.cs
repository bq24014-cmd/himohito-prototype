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
        private const string ScenePath = "Assets/Scenes/Tutorial.unity";
        private const string MainStageScenePath = "Assets/Scenes/MainStage.unity";
        private const float ExperimentalRopeLength = 12f;
        private const string TutorialFlashlightSpotName = "Tutorial Flashlight Spot";
        private const string LegacyTutorialHazardName = "Practice Long Rope Obstacle";

        static PrototypeSceneBuilder()
        {
            EditorApplication.delayCall += BuildSceneOnFirstOpen;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("HimoHito/Build Tutorial Scene")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreatePlayer();
            CreatePlatform("Start Ground", new Vector2(-18f, -5.2f), new Vector2(5f, 0.7f));
            CreateHookPoint(
                "Tutorial Hook",
                new Vector2(-14f, -0.2f),
                new Vector2(1.6f, 0.45f));
            GameObject tutorialLanding = CreatePlatform(
                "Tutorial Landing",
                new Vector2(-9f, -5.2f),
                new Vector2(5f, 0.7f));
            ConfigureTutorialCheckpoint(tutorialLanding, 2, new Vector2(-9f, -4.2f));
            CreateHookPoint(
                "Hook 1",
                new Vector2(-5f, -0.2f),
                new Vector2(1.6f, 0.45f));

            // The route now keeps only the upper planning landing.
            // Touching the flashlight spot in the airborne gap leads to a fall and retry.
            GameObject landing1 = CreatePlatform(
                "Landing 1",
                new Vector2(0f, -2.3f),
                new Vector2(4f, 0.7f));
            ConfigureTutorialCheckpoint(landing1, 3, new Vector2(0f, -1.3f));
            CreateHookPoint(
                "Hook 2",
                new Vector2(5f, 1.5f),
                new Vector2(1.6f, 0.45f));
            CreateRopeReleaseHazard(
                TutorialFlashlightSpotName,
                new Vector2(-4.25f, -3.1f),
                3.2f,
                2);
            CreatePlatform(
                "Planning Landing",
                new Vector2(9.5f, -0.4f),
                new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 3", new Vector2(13.8f, 3.9f), new Vector2(1.6f, 0.45f));
            CreateGoalPlatform("Goal / Landing 3", new Vector2(26.15f, -0.5f), new Vector2(4f, 0.8f));

            GameObject hud = new GameObject("Tutorial HUD");
            hud.AddComponent<PrototypeHud>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(MainStageScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = GameObject.Find("Player");
            Debug.Log($"HimoHito tutorial scene created: {ScenePath}");
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

            changed |= EnsureExperimentalRopeLength(prototypeScene);
            changed |= EnsureHorizontalCameraFollow(prototypeScene);
            changed |= RemoveRootObject(prototypeScene, "Practice Safety Floor");
            changed |= RemoveRootObject(prototypeScene, "Landing 2");
            changed |= RemoveRootObject(prototypeScene, "Planning Hook");
            changed |= EnsureTutorialLayout(prototypeScene);
            changed |= EnsurePracticeSection(prototypeScene);
            changed |= EnsureWeaveExperiment(prototypeScene);
            changed |= EnsureTutorialCheckpoints(prototypeScene);

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
            if (player != null && !player.TryGetComponent(out RopePlatformBuilder _))
            {
                player.AddComponent<RopePlatformBuilder>();
                changed = true;
            }

            changed |= RemoveRootObject(scene, "Tutorial Woven Platform");
            changed |= RemoveRootObject(scene, "Tutorial Weave Frame");

            GameObject goal = FindRootObject(scene, "Goal / Landing 3");
            if (goal != null)
            {
                changed |= ApplyTransform(goal, new Vector2(26.15f, -0.5f), new Vector2(4f, 0.8f));
            }

            return changed;
        }

        private static bool EnsurePracticeSection(Scene scene)
        {
            GameObject legacyHazard = FindRootObject(scene, LegacyTutorialHazardName);
            if (legacyHazard != null &&
                FindRootObject(scene, TutorialFlashlightSpotName) == null)
            {
                legacyHazard.name = TutorialFlashlightSpotName;
                EditorUtility.SetDirty(legacyHazard);
            }

            return EnsureRopeReleaseHazard(
                scene,
                TutorialFlashlightSpotName,
                new Vector2(-4.25f, -3.1f),
                3.2f,
                2);
        }

        private static bool EnsureTutorialLayout(Scene scene)
        {
            bool changed = false;
            changed |= EnsurePlatform(
                scene,
                "Start Ground",
                new Vector2(-18f, -5.2f),
                new Vector2(5f, 0.7f));
            changed |= EnsureHookPoint(
                scene,
                "Tutorial Hook",
                new Vector2(-14f, -0.2f),
                new Vector2(1.6f, 0.45f));
            changed |= EnsurePlatform(
                scene,
                "Tutorial Landing",
                new Vector2(-9f, -5.2f),
                new Vector2(5f, 0.7f));

            GameObject player = FindRootObject(scene, "Player");
            Vector2 playerStart = new Vector2(-18f, -4.2f);
            if (player != null && (Vector2)player.transform.position != playerStart)
            {
                player.transform.position = playerStart;
                changed = true;
            }

            changed |= RemoveTutorialCheckpoint(scene, "Planning Landing");
            return changed;
        }

        private static bool EnsureTutorialCheckpoints(Scene scene)
        {
            bool changed = false;
            changed |= EnsureTutorialCheckpoint(
                scene,
                "Tutorial Landing",
                2,
                new Vector2(-9f, -4.2f));
            changed |= EnsureTutorialCheckpoint(
                scene,
                "Landing 1",
                3,
                new Vector2(0f, -1.3f));
            changed |= EnsureTutorialCheckpoint(
                scene,
                "Tutorial Woven Platform",
                4,
                new Vector2(21.1f, 0.55f));
            return changed;
        }

        private static bool RemoveTutorialCheckpoint(Scene scene, string objectName)
        {
            GameObject platform = FindRootObject(scene, objectName);
            if (platform == null ||
                !platform.TryGetComponent(out TutorialCheckpoint checkpoint))
            {
                return false;
            }

            Object.DestroyImmediate(checkpoint);
            return true;
        }

        private static bool EnsureHookPoint(
            Scene scene,
            string objectName,
            Vector2 position,
            Vector2 size)
        {
            GameObject hookObject = FindRootObject(scene, objectName);
            if (hookObject == null)
            {
                CreateHookPoint(objectName, position, size);
                return true;
            }

            bool changed = ApplyTransform(hookObject, position, size);
            if (!hookObject.TryGetComponent(out HookPoint _))
            {
                hookObject.AddComponent<HookPoint>();
                changed = true;
            }

            if (hookObject.TryGetComponent(out SolidSprite visual))
            {
                Color hookColor = new Color(1f, 0.72f, 0.18f);
                if (visual.Color != hookColor)
                {
                    visual.Color = hookColor;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureTutorialCheckpoint(
            Scene scene,
            string objectName,
            int sectionNumber,
            Vector2 respawnPosition)
        {
            GameObject platform = FindRootObject(scene, objectName);
            if (platform == null)
            {
                return false;
            }

            bool changed = false;
            if (!platform.TryGetComponent(out TutorialCheckpoint checkpoint))
            {
                checkpoint = platform.AddComponent<TutorialCheckpoint>();
                changed = true;
            }

            if (checkpoint.SectionNumber != sectionNumber ||
                checkpoint.RespawnPosition != respawnPosition)
            {
                checkpoint.Configure(sectionNumber, respawnPosition);
                EditorUtility.SetDirty(checkpoint);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveRootObject(Scene scene, string objectName)
        {
            GameObject rootObject = FindRootObject(scene, objectName);
            if (rootObject == null)
            {
                return false;
            }

            Object.DestroyImmediate(rootObject);
            return true;
        }

        private static bool EnsureRopeReleaseHazard(
            Scene scene,
            string objectName,
            Vector2 position,
            float diameter,
            int activeTutorialSection)
        {
            GameObject hazard = FindRootObject(scene, objectName);
            if (hazard == null)
            {
                hazard = new GameObject(objectName);
            }

            bool changed = ApplyTransform(
                hazard,
                position,
                new Vector2(diameter, diameter));
            CircleCollider2D circle = FlashlightSpotVisual.ConfigureSpot(
                hazard,
                position,
                diameter,
                new Color(1f, 1f, 1f, 0.72f));
            EditorUtility.SetDirty(hazard);
            EditorUtility.SetDirty(circle);

            if (!hazard.TryGetComponent(out RopeReleaseHazard releaseHazard))
            {
                releaseHazard = hazard.AddComponent<RopeReleaseHazard>();
                changed = true;
            }

            if (releaseHazard.ActiveTutorialSection != activeTutorialSection)
            {
                releaseHazard.Configure(activeTutorialSection);
                EditorUtility.SetDirty(releaseHazard);
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
            player.transform.position = new Vector3(-18f, -4.2f, 0f);
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
            player.AddComponent<RopePlatformBuilder>();
            player.AddComponent<PlayerMover>();
            player.AddComponent<RopeController>();
            player.AddComponent<PrototypeRunController>();
        }

        private static GameObject CreatePlatform(string name, Vector2 position, Vector2 size)
        {
            return CreatePlatformVisual(
                name,
                position,
                size,
                new Color(0.38f, 0.41f, 0.52f));
        }

        private static void ConfigureTutorialCheckpoint(
            GameObject platform,
            int sectionNumber,
            Vector2 respawnPosition)
        {
            TutorialCheckpoint checkpoint = platform.AddComponent<TutorialCheckpoint>();
            checkpoint.Configure(sectionNumber, respawnPosition);
        }

        private static void CreateRopeReleaseHazard(
            string name,
            Vector2 position,
            float diameter,
            int activeTutorialSection)
        {
            GameObject hazard = new GameObject(name);
            FlashlightSpotVisual.ConfigureSpot(
                hazard,
                position,
                diameter,
                new Color(1f, 1f, 1f, 0.72f));
            RopeReleaseHazard releaseHazard = hazard.AddComponent<RopeReleaseHazard>();
            releaseHazard.Configure(activeTutorialSection);
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
            frame.Configure(wovenPlatform, 3);

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
