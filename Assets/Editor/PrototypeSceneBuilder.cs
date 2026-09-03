using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>Rebuilds the five-section tutorial from the 0829 manual.</summary>
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Tutorial.unity";
        private static readonly Color TerrainColor = new(0.96f, 0.55f, 0.18f);
        private static readonly Color HookColor = new(0.30f, 0.76f, 1f);
        private static readonly Color SpikeColor = new(1f, 0.18f, 0.25f);

        [MenuItem("HimoHito/Rebuild Tutorial From 0829 Manual")]
        public static void BuildPrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject player = CreatePlayer(
                TutorialSectionOneSetup.StartRespawnPosition);

            BuildT1();
            BuildT2();
            BuildT3();
            BuildT4();
            GameObject goal = BuildT5();

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
            Debug.Log($"HimoHito 0829 tutorial rebuilt: {ScenePath}; goal={goal.name}");
        }

        public static void BuildFromCommandLine()
        {
            BuildPrototypeScene();
            EditorApplication.Exit(0);
        }

        private static void BuildT1()
        {
            TutorialSectionOneSetup.EnsureCreated();
        }

        private static void BuildT2()
        {
            CreateTerrain("T2 Start Shelf", new Vector2(12f, 0f), new Vector2(3f, 0.7f));
            CreateHook("Hook 1", new Vector2(18f, 7.4f));
            CreateSpike("T2 Center Spike", new Vector2(18f, 0.1f), new Vector2(1.2f, 0.4f));
            GameObject landing = CreateTerrain("Landing 1", new Vector2(24f, 0f), new Vector2(5f, 0.7f));
            AddCheckpoint(landing, 3, new Vector2(24f, 0.65f), 7);
        }

        private static void BuildT3()
        {
            // Facing edges are exactly 6 units apart. Length 7 therefore makes
            // the manual's one-unit sag and permanently consumes 7 on Q.
            CreateTerrain("T3 Left Shelf", new Vector2(29f, 4f), new Vector2(4f, 0.7f));
            GameObject rightWall = CreateTerrain(
                "T3 Right Shelf And T4 High Wall",
                new Vector2(38f, 0f),
                new Vector2(2f, 8f));
            AddCheckpoint(rightWall, 4, new Vector2(37.5f, 4.65f), 10);
        }

        private static void BuildT4()
        {
            // From the bottom of the T3 bridge this hook is about 9.5 units
            // away; the high wall prevents simply walking right.
            CreateHook("Hook 2", new Vector2(42.5f, 7.2f));
            GameObject landing = CreateTerrain("T4 Landing", new Vector2(48f, 0f), new Vector2(4f, 0.7f));
            AddCheckpoint(landing, 5, new Vector2(47f, 0.65f), 6);
        }

        private static GameObject BuildT5()
        {
            // Outer attachment points are 10 units apart. Two length-6 bridges
            // meet at Hook 3 but run into the beam. F joins them into one
            // length-12 bridge whose two-unit sag passes below y=4.4.
            CreateTerrain("T5 Left Shelf", new Vector2(52f, 5.85f), new Vector2(4f, 0.7f));
            CreateHook("Hook 3", new Vector2(59f, 8.24f));
            GameObject goal = CreateTerrain(
                "Goal / Landing 3",
                new Vector2(67f, 5.85f),
                new Vector2(6f, 0.7f));
            CreateTerrain(
                "T5 Overhead Beam",
                new Vector2(59f, 6.32f),
                new Vector2(2f, 3.84f));
            goal.AddComponent<GoalZone>();
            return goal;
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
            serialized.FindProperty("maximumLength").floatValue = 20f;
            serialized.FindProperty("currentLength").floatValue = 20f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            player.AddComponent<RopePlatformBuilder>();
            player.AddComponent<PlayerMover>();
            RopeController rope = player.AddComponent<RopeController>();
            rope.RestoreSelectedRopeLength(6);
            player.AddComponent<PrototypeRunController>();
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

        private static void CreateSpike(string name, Vector2 position, Vector2 size)
        {
            GameObject spike = CreateSolidObject(name, position, size, SpikeColor);
            spike.GetComponent<BoxCollider2D>().isTrigger = true;
            spike.AddComponent<RopeSpikeHazard>();
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
            Vector2 position,
            int startingRopeLength)
        {
            TutorialCheckpoint checkpoint = floor.AddComponent<TutorialCheckpoint>();
            checkpoint.Configure(section, position, startingRopeLength);
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
