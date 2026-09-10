using UnityEngine;

namespace HimoHito
{
    /// <summary>Shared responsive clear layout, reusing the game's wood, yarn and chest artwork.</summary>
    internal sealed class ClearJourneyView
    {
        internal enum Choice { None, Primary, StageSelection }

        private readonly ClearJourneySnapshot snapshot;
        private GUIStyle title, text, small, heading, action;
        private static readonly Color Pink = new Color(1f, .46f, .64f);

        public ClearJourneyView(RopeResource owner, RopePlatformBuilder builder, bool tutorial)
        { snapshot = new ClearJourneySnapshot(owner, builder, tutorial); }

        public Choice Draw(bool tutorial, string elapsed, int refills)
            => DrawAtSize(tutorial, elapsed, refills, new Vector2(Screen.width, Screen.height));

        internal Choice DrawAtSize(bool tutorial, string elapsed, int refills, Vector2 viewport)
        {
            EnsureStyles();
            Matrix4x4 matrix = GUI.matrix;
            Color color = GUI.color;
            float scale = Mathf.Min(viewport.x / 1280f, viewport.y / 720f);
            if (scale <= 0f) return Choice.None;
            GUI.matrix = matrix * Matrix4x4.TRS(new Vector3((viewport.x - 1280f * scale) * .5f,
                (viewport.y - 720f * scale) * .5f), Quaternion.identity, new Vector3(scale, scale, 1f));
            try
            {
                GUI.color = new Color(.045f, .03f, .075f, .72f);
                GUI.DrawTexture(new Rect(60, 30, 1160, 650), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(80, 48, 1120, 65), tutorial ? "チュートリアルクリア" : "CLEAR", title);
                GUI.Label(new Rect(80, 127, 1120, 38),
                    $"残ったヒモ  {snapshot.Remaining:0.0} / {snapshot.Capacity:0.0}     残したヒモ橋  {snapshot.Bridges.Length} 本", text);
                GUI.Label(new Rect(80, 174, 1120, 32), tutorial
                    ? "ヒモを掛ける・長さを選ぶ・足場にする・まとめるを習得しました"
                    : $"補充  {refills} 回      かかった時間  {elapsed}", small);
                DrawMap(new Rect(96, 232, 1088, 310));
                bool primary = HimoHitoUiParts.WoodButton(new Rect(108, 588, 540, 58), tutorial
                    ? "Enter　本編ステージへ" : "R　本編を最初から再挑戦", action);
                bool stageSelection = HimoHitoUiParts.WoodButton(new Rect(672, 588, 500, 58),
                    "Esc　ステージ選択へ戻る", action);
                return stageSelection ? Choice.StageSelection : primary ? Choice.Primary : Choice.None;
            }
            finally { GUI.matrix = matrix; GUI.color = color; }
        }

        internal void DrawMap(Rect panel)
        {
            EnsureStyles();
            Color old = GUI.color;
            GUI.color = new Color(.16f, .09f, .085f, .96f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(.075f, .055f, .12f, 1f);
            GUI.DrawTexture(new Rect(panel.x + 3, panel.y + 3, panel.width - 6, panel.height - 6), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 22, panel.y + 12, panel.width - 44, 32), "あなたが残した道", heading);
            GUI.Label(new Rect(panel.x + 22, panel.yMax - 37, panel.width - 44, 27),
                "ピンク：クリア時に残ったヒモ橋　／　床と高さを簡略化した見取り図", small);
            Rect plot = new Rect(panel.x + 48, panel.y + 94, panel.width - 96, panel.height - 174);
            foreach (ClearJourneySnapshot.Floor floor in snapshot.Floors)
            {
                Vector2 a = snapshot.Project(new Vector2(floor.Left, floor.Top), plot);
                Vector2 b = snapshot.Project(new Vector2(floor.Right, floor.Top), plot);
                Rect plank = new Rect(a.x, a.y, Mathf.Max(1, b.x - a.x), floor.Rail ? 5 : 12);
                if (!floor.Rail && HimoHitoUiParts.WoodMountSprite != null)
                    DrawSprite(plank, HimoHitoUiParts.WoodMountSprite, false);
                else
                {
                    GUI.color = floor.Rail ? new Color(.35f, .66f, .82f) : new Color(.76f, .43f, .18f);
                    GUI.DrawTexture(plank, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
            Texture2D yarn = YarnRopeTexture.Load();
            foreach (Vector2[] bridge in snapshot.Bridges)
            {
                float distance = 0f;
                for (int i = 1; i < bridge.Length; i++)
                {
                    Vector2 a = snapshot.Project(bridge[i - 1], plot), b = snapshot.Project(bridge[i], plot);
                    DrawYarn(a, b, yarn, distance);
                    distance += Vector2.Distance(a, b);
                }
                Vector2 first = snapshot.Project(bridge[0], plot), last = snapshot.Project(bridge[^1], plot);
                HimoHitoUiParts.DrawKnot(new Rect(first.x - 4, first.y - 4, 8, 8));
                HimoHitoUiParts.DrawKnot(new Rect(last.x - 4, last.y - 4, 8, 8));
            }
            Vector2 start = snapshot.Project(snapshot.Start, plot), goal = snapshot.Project(snapshot.Goal, plot);
            GUI.Label(new Rect(start.x - 42, start.y - 31, 84, 26), "START", small);
            HimoHitoUiParts.DrawKnot(new Rect(start.x - 5, start.y - 7, 10, 10));
            if (snapshot.Chest != null) DrawSprite(new Rect(goal.x - 22, goal.y - 42, 44, 42), snapshot.Chest, true);
            else HimoHitoUiParts.DrawKnot(new Rect(goal.x - 8, goal.y - 16, 16, 16));
            GUI.Label(new Rect(goal.x - 42, goal.y - 68, 84, 26), "GOAL", small);
            if (snapshot.Floors.Length == 0 && snapshot.Bridges.Length == 0)
                GUI.Label(plot, "このシーンには表示できる床・ヒモ橋がありません", small);
            GUI.color = old;
        }

        private static void DrawYarn(Vector2 a, Vector2 b, Texture2D yarn, float distance)
        {
            Vector2 delta = b - a;
            if (delta.sqrMagnitude < .0001f) return;
            Matrix4x4 matrix = GUI.matrix;
            // Build local rotation with Unity's clip origin, then apply the responsive parent scale.
            GUI.matrix = Matrix4x4.identity;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg,
                a);
            GUI.matrix = matrix * GUI.matrix;
            if (yarn != null)
            {
                float tile = 4f * yarn.width / yarn.height;
                GUI.DrawTextureWithTexCoords(new Rect(a.x, a.y - 2, delta.magnitude + .35f, 4), yarn,
                    new Rect(distance / tile, 0, (delta.magnitude + .35f) / tile, 1));
            }
            else
            {
                GUI.color = Pink;
                GUI.DrawTexture(new Rect(a.x, a.y - 2, delta.magnitude, 4), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            GUI.matrix = matrix;
        }

        private static void DrawSprite(Rect rect, Sprite sprite, bool preserveAspect)
        {
            Rect uv = sprite.rect;
            if (preserveAspect)
            {
                float scale = Mathf.Min(rect.width / uv.width, rect.height / uv.height);
                rect = new Rect(rect.center.x - uv.width * scale * .5f, rect.yMax - uv.height * scale,
                    uv.width * scale, uv.height * scale);
            }
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, new Rect(uv.x / sprite.texture.width,
                uv.y / sprite.texture.height, uv.width / sprite.texture.width, uv.height / sprite.texture.height));
        }

        private void EnsureStyles()
        {
            if (title != null) return;
            GUIStyle Make(int size, Color color, bool bold) => new GUIStyle(GUI.skin.label) {
                fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter, wordWrap = false, normal = { textColor = color }
            };
            title = Make(46, Pink, true);
            text = Make(26, new Color(1f, .86f, .66f), true);
            small = Make(18, new Color(.8f, .78f, .86f), false);
            heading = Make(24, new Color(1f, .86f, .66f), true);
            action = Make(24, new Color(.1f, .07f, .15f), true);
            HimoHitoGuiTheme.ApplyToStyles(title, text, small, heading, action);
        }
    }
}
