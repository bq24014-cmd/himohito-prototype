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

        static PrototypeSceneBuilder()
        {
            EditorApplication.delayCall += BuildSceneOnFirstOpen;
        }

        [MenuItem("HimoHito/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreatePlayer();
            CreatePlatform("Start Ground", new Vector2(-9f, -5.2f), new Vector2(5f, 0.7f));

            // Three sections test different rope lengths: roughly 6, 4, then 8.
            // The third landing is the goal, so none of the three decisions is optional.
            CreateHookPoint("Hook 1", new Vector2(-5f, -0.2f), new Vector2(1.6f, 0.45f));
            CreatePlatform("Landing 1", new Vector2(0f, -2.3f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 2", new Vector2(5f, 1.5f), new Vector2(1.6f, 0.45f));
            CreatePlatform("Landing 2", new Vector2(8f, -2f), new Vector2(4f, 0.7f));
            CreateHookPoint("Hook 3", new Vector2(13.8f, 3.9f), new Vector2(1.6f, 0.45f));
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
            }
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
