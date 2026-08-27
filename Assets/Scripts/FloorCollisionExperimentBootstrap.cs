using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Creates the comparison layout when the lightweight experiment scene starts.
    /// The editor builder can later replace it with a fully serialized copy.
    /// </summary>
    public sealed class FloorCollisionExperimentBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            GameObject player = CreatePlayer(new Vector2(-8f, -4.2f));
            CreateCamera();
            CreateSolidPlatform(
                "Experiment Start Ground",
                new Vector2(-8f, -5.2f),
                new Vector2(6f, 0.7f),
                new Color(1f, 0.706f, 0.235f));
            CreateHook("Experiment Rail Hook", new Vector2(-4f, 1.3f));
            CreateOneWayRail(
                "Experiment One-Way Blue Rail",
                new Vector2(0.75f, -5.15f),
                new Vector2(5.5f, 0.6f));

            CreateHook("Experiment Solid Floor Hook", new Vector2(5f, 3f));
            GameObject solidBoard = CreateSolidPlatform(
                "Experiment Solid Board",
                new Vector2(9.5f, -5.2f),
                new Vector2(5.5f, 0.7f),
                new Color(0.38f, 0.41f, 0.52f));
            solidBoard.AddComponent<SolidSwingSurface>();

            new GameObject("Experiment HUD").AddComponent<PrototypeHud>();
            Destroy(gameObject);
        }

        private static GameObject CreatePlayer(Vector2 position)
        {
            GameObject player = new GameObject("Experiment Player");
            player.transform.position = position;
            player.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            SolidSprite visual = player.AddComponent<SolidSprite>();
            visual.Color = new Color(1f, 0.365f, 0.561f);

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

            player.AddComponent<RopeResource>();
            player.AddComponent<PlayerMover>();
            player.AddComponent<RopeController>();
            player.AddComponent<CollisionExperimentController>();
            return player;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-8f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.052f, 0.11f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<HorizontalCameraFollow>();
        }

        private static void CreateHook(string name, Vector2 position)
        {
            GameObject hook = CreateSolidPlatform(
                name,
                position,
                new Vector2(1.6f, 0.45f),
                new Color(0.298f, 0.765f, 1f));
            hook.AddComponent<HookPoint>();
        }

        private static void CreateOneWayRail(
            string name,
            Vector2 position,
            Vector2 size)
        {
            GameObject rail = CreateSolidPlatform(
                name,
                position,
                size,
                new Color(0.298f, 0.765f, 1f));
            rail.AddComponent<PlatformEffector2D>();
            rail.AddComponent<OneWayRailPlatform>();
        }

        private static GameObject CreateSolidPlatform(
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
