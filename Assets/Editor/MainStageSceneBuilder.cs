using HimoHito;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HimoHitoEditor
{
    /// <summary>
    /// Rebuilds the ten-section main stage from the 2026-08-29 stage manual.
    /// All coordinates are the document's starting estimates and remain tunable.
    /// </summary>
    public static class MainStageSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainStage.unity";
        private const float MainStageRopeLength = 50f;
        private static readonly Color TerrainColor = new(0.96f, 0.55f, 0.18f);
        private static readonly Color HookColor = new(0.30f, 0.76f, 1f);
        private static readonly Color SpikeColor = new(1f, 0.18f, 0.25f);

        [MenuItem("HimoHito/Rebuild Main Stage From 0829 Manual")]
        public static void BuildMainStageThroughSectionTen()
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
            BuildSection8();
            BuildSection9();
            GameObject goal = BuildSection10();

            CreateCamera(player.transform, goal.transform);
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
            BuildMainStageThroughSectionTen();
            EditorApplication.Exit(0);
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
            CreateTerrain("Main S04 Left Edge", new Vector2(41f, 5f), new Vector2(4f, 0.7f));
            CreateTerrain("Main S04 Intermediate Column", new Vector2(46.5f, 3f), new Vector2(2f, 6f));
            CreateHook("Main S04 Far Hook", new Vector2(54f, 7.2f));
            GameObject landing = CreateTerrain("Main S04 Landing", new Vector2(61f, 4.5f), new Vector2(5f, 0.7f));
            AddCheckpoint(landing, 5, new Vector2(60f, 5.15f), 40f);
        }

        private static void BuildSection5()
        {
            CreateTerrain("Main S05 Left Shelf", new Vector2(66f, 4.5f), new Vector2(3f, 0.7f));
            CreateTerrain("Main S05 Right Shelf", new Vector2(72.4f, 4.5f), new Vector2(3f, 0.7f));
            GameObject source = CreateLightSource("Main S05 Flashlight Source", new Vector2(69.2f, 8.2f));
            CreateOccludedLight("Main S05 Flashlight Spot", new Vector2(69.2f, 2f), 4f, source.transform);
            CreateHook("Main S05 Central Hook", new Vector2(77f, 7.2f));
            GameObject landing = CreateTerrain("Main S05 Landing", new Vector2(83f, 4.5f), new Vector2(5f, 0.7f));
            AddCheckpoint(landing, 6, new Vector2(82f, 5.15f), 34f);
        }

        private static void BuildSection6()
        {
            CreateTerrain("Main S06 Upper Left", new Vector2(87f, 5.5f), new Vector2(3f, 0.7f));
            CreateTerrain("Main S06 Upper Right", new Vector2(92f, 5.5f), new Vector2(3f, 0.7f));
            CreateHook("Main S06 Lower Hook A", new Vector2(87f, 1.8f));
            CreateHook("Main S06 Lower Hook B", new Vector2(92f, 1.8f));
            CreateHook("Main S06 Lower Hook C", new Vector2(97f, 1.8f));
            CreateTerrain("Main S06 Safety Floor", new Vector2(92f, -3f), new Vector2(16f, 0.7f));
            CreateTerrain("Main S06 Return Step", new Vector2(99f, -1.2f), new Vector2(2f, 0.5f));
            GameObject merge = CreateTerrain("Main S06 Merge", new Vector2(102f, 4.5f), new Vector2(5f, 0.7f));
            AddCheckpoint(merge, 7, new Vector2(101f, 5.15f), 34f);
        }

        private static void BuildSection7()
        {
            CreateTerrain("Main S07 Lower Start", new Vector2(106f, 4.5f), new Vector2(3f, 0.7f));
            CreateTerrain("Main S07 Middle Shelf", new Vector2(111f, 6.5f), new Vector2(3f, 0.7f));
            CreateTerrain("Main S07 Upper Step", new Vector2(115f, 8.5f), new Vector2(3f, 0.7f));
            CreateHook("Main S07 Upper Hook", new Vector2(120f, 11f));
            GameObject landing = CreateTerrain("Main S07 Landing", new Vector2(125f, 6f), new Vector2(5f, 0.7f));
            AddCheckpoint(landing, 8, new Vector2(124f, 6.65f), 30f);
        }

        private static void BuildSection8()
        {
            CreateTerrain("Main S08 Shaft Left Rim", new Vector2(130f, 5.5f), new Vector2(2f, 0.7f));
            CreateTerrain("Main S08 Shaft Right Rim", new Vector2(134f, 5.5f), new Vector2(2f, 0.7f));
            CreateTerrain("Main S08 Shaft Left Wall", new Vector2(129f, 1f), new Vector2(0.7f, 8f));
            CreateTerrain("Main S08 Shaft Right Wall", new Vector2(135f, 1f), new Vector2(0.7f, 8f));
            CreateSpike("Main S08 Bottom Spikes", new Vector2(132f, -3f), new Vector2(5f, 0.6f));
            GameObject exit = CreateTerrain("Main S08 Bottom Exit", new Vector2(140f, -1.5f), new Vector2(8f, 0.7f));
            AddCheckpoint(exit, 9, new Vector2(137f, -0.85f), 23f);
        }

        private static void BuildSection9()
        {
            // Facing shelf edges are exactly 10 units apart. Two length-6 ropes
            // reach the middle hook; removing it creates one length-12 rope.
            CreateTerrain("Main S09 Left Shelf", new Vector2(145f, 1f), new Vector2(4f, 0.7f));
            CreateHook("Main S09 Removable Hook", new Vector2(152f, 3.39f));
            GameObject landing = CreateTerrain(
                "Main S09 Right Shelf",
                new Vector2(159f, 1f),
                new Vector2(4f, 0.7f));
            CreateTerrain(
                "Main S09 Overhead Beam",
                new Vector2(152f, 1.47f),
                new Vector2(5f, 3.84f));
            AddCheckpoint(landing, 10, new Vector2(158f, 1.65f), 11f);
        }

        private static GameObject BuildSection10()
        {
            // Section 9's right shelf is the left bank. The facing edges are
            // 9 units apart, so the final bridge uses length 10 and sags by 1.
            GameObject goal = CreateTerrain(
                MainStageSectionTenSetup.GoalName,
                new Vector2(173f, 1f),
                new Vector2(6f, 0.7f));
            goal.AddComponent<MainStageGoalZone>();
            return goal;
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
            collider.edgeRadius = 0.08f;
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

        private static GameObject CreateLightSource(string name, Vector2 position)
        {
            return CreateSolidObject(name, position, new Vector2(0.8f, 0.8f), new Color(1f, 0.86f, 0.54f));
        }

        private static void CreateOccludedLight(
            string name,
            Vector2 position,
            float diameter,
            Transform source)
        {
            GameObject spot = new(name);
            FlashlightSpotVisual.ConfigureSpot(
                spot,
                position,
                diameter,
                new Color(1f, 0.82f, 0.56f, 0.62f));
            PlatformOccludedLightHazard hazard = spot.AddComponent<PlatformOccludedLightHazard>();
            hazard.Configure(source);
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
