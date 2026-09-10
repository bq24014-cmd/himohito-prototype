using System.Collections.Generic;
using UnityEngine;

namespace HimoHito
{
    /// <summary>Modular toy islands on felt. Input and scene loading belong to the run controller.</summary>
    public sealed class StageSelectionView
    {
        public const int StagesPerPage = 3;
        private const string ArtRoot = "Art/HimoHitoStageMap";
        private const string UpcomingArtPath = ArtRoot + "Upcoming-v1";
        private Texture2D background, yarn;
        private GUIStyle heading, eyebrow, label, small, description, button, selectedLabel, credits, clearLabel;
        private static readonly Color Ink = new Color(.17f, .12f, .20f);
        private static readonly Color Cream = new Color(1f, .91f, .76f);
        private static readonly Color Muted = new Color(.76f, .75f, .79f);
        private static readonly Color Rose = new Color(1f, .57f, .67f);
        private static readonly Dictionary<string, Sprite> CustomArt = new Dictionary<string, Sprite>();
        private static Sprite[] pieces;
        private static Sprite upcomingPiece;
        private readonly StageSelectionWeave selectionWeave = new StageSelectionWeave();
        private readonly StageSelectionDioramaState dioramaState = new StageSelectionDioramaState();
        public static int PageCount(int count) => Mathf.Max(1, (count + StagesPerPage - 1) / StagesPerPage);

        internal static Vector2 SelectedIslandOrigin(int selected, int total, float width, float height)
        {
            int first = selected / StagesPerPage * StagesPerPage;
            int count = Mathf.Min(StagesPerPage, total - first);
            int local = selected - first;
            float x = count == 1 ? 640 : count == 2 ? 375 + local * 530 : 275 + local * 365;
            float scale = Mathf.Min(width / 1280f, height / 720f);
            return new Vector2(((width - 1280 * scale) / 2 + x * scale) / Mathf.Max(1, width),
                ((height - 720 * scale) / 2 + 393 * scale) / Mathf.Max(1, height));
        }

        public void Draw(PrototypeRunController run) => Draw(run, Screen.width, Screen.height);

        // Explicit viewport lets editor previews exercise the exact runtime layout.
        public void Draw(PrototypeRunController run, float viewportWidth, float viewportHeight)
            => DrawAtTime(run, viewportWidth, viewportHeight, Time.unscaledTimeAsDouble);

        internal void DrawAtTime(PrototypeRunController run, float viewportWidth, float viewportHeight, double now)
        {
            EnsureResources();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            bool oldEnabled = GUI.enabled;
            bool quit = false, help = false, start = false;
            try
            {
                GUI.color = new Color(.055f, .045f, .085f);
                GUI.DrawTexture(new Rect(0, 0, viewportWidth, viewportHeight), Texture2D.whiteTexture);
                float scale = Mathf.Min(viewportWidth / 1280f, viewportHeight / 720f);
                GUI.matrix = oldMatrix * Matrix4x4.TRS(
                    new Vector3((viewportWidth - 1280 * scale) / 2, (viewportHeight - 720 * scale) / 2, 0),
                    Quaternion.identity, new Vector3(scale, scale, 1));
                GUI.color = Color.white;
                if (background != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), background);
                GUI.enabled = oldEnabled && run.CanUseTitleMenu();

                GUI.Label(new Rect(240, 51, 800, 23), "H I M O H I T O", eyebrow);
                GUI.Label(new Rect(180, 74, 920, 66), "ヒモヒト", heading);
                GUI.Label(new Rect(240, 140, 800, 29), "あそぶ場所を、えらぼう", description);

                var stages = StageCatalog.Entries;
                int selected = run.SelectedStageIndex;
                selectionWeave.Observe(stages[selected].id, now, GUI.enabled);
                dioramaState.Configure(stages);
                dioramaState.Observe(stages[selected].id, now, GUI.enabled);
                int page = selected / StagesPerPage;
                int first = page * StagesPerPage;
                int count = Mathf.Min(StagesPerPage, stages.Count - first);
                Vector2 Node(int i) => count == 1 ? new Vector2(640, 393) :
                    new Vector2(count == 2 ? 375 + i * 530 : 275 + i * 365,
                        393);

                // Paths and destinations are live UI, never painted into the background.
                for (int i = 1; i < count; i++)
                    DrawPath(Node(i - 1), Node(i), !stages[first + i].available);
                if (page + 1 < PageCount(stages.Count))
                {
                    GUI.color = new Color(1, 1, 1, .35f);
                    DrawPath(Node(count - 1), new Vector2(1160, 350), true);
                    GUI.color = Color.white;
                }
                for (int i = 0; i < count; i++)
                {
                    int index = first + i;
                    StageCatalog.Entry stage = stages[index];
                    Vector2 node = Node(i);
                    bool active = index == selected;
                    Rect iconRect = new Rect(node.x - 127, node.y - 174, 254, 197);
                    Rect nameRect = new Rect(node.x - 152, node.y + 25, 304, 64);
                    if (active)
                    {
                        DrawSelectionWeave(new Vector2(node.x, node.y - 82));
                        GUI.Label(new Rect(node.x - 100, node.y - 214, 200, 24), "いま選んでいる場所", selectedLabel);
                    }
                    GUI.color = stage.available ? Color.white : new Color(.86f, .88f, .94f, .85f);
                    // Only the miniature actor/lid moves. Bases, input rectangles and camera stay fixed.
                    Sprite stageArt = GetStageArt(stage);
                    StageSelectionDioramaState.Motion motion = dioramaState.GetMotion(stage.id);
                    if (!stage.available || stage.mapArt != "toybox" ||
                        !StageSelectionDioramaArt.DrawToybox(iconRect, motion))
                        DrawSprite(iconRect, stageArt, false);
                    if (stage.available && stage.mapArt == "practice")
                        StageSelectionDioramaArt.DrawPractice(iconRect, stageArt, motion);
                    GUI.color = Color.white;
                    if (StageProgress.IsCleared(stage)) DrawClearFlag(node);
                    if (ClothButton(nameRect, stage.title, label, active)) run.SelectStage(index);
                    if (GUI.Button(iconRect, GUIContent.none, GUIStyle.none)) run.SelectStage(index);
                    GUI.Label(new Rect(node.x - 162, node.y + 91, 324, 28),
                        stage.available ? stage.subtitle : "COMING SOON  /  追加予定",
                        stage.available ? small : selectedLabel);
                }

                StageCatalog.Entry chosen = stages[run.SelectedStageIndex];
                bool hasError = !string.IsNullOrEmpty(run.StageSelectionError);
                FitLabel(new Rect(255, 522, 770, 52), hasError ? run.StageSelectionError : chosen.description,
                    hasError ? selectedLabel : description, hasError ? 15 : 19, 14);
                bool enabled = GUI.enabled;
                quit = ClothButton(new Rect(151, 602, 255, 56), "Esc　ゲーム終了", button, false);
                help = ClothButton(new Rect(512, 602, 255, 56), "Tab　操作説明", button, false);
                GUI.enabled = enabled && chosen.available;
                start = ClothButton(new Rect(873, 602, 255, 56),
                    chosen.available ? "Enter　ここであそぶ" : "追加をお楽しみに", button, chosen.available);
                GUI.enabled = enabled;

                int pages = PageCount(stages.Count);
                if (pages > 1)
                {
                    GUI.enabled = enabled && page > 0;
                    if (GUI.Button(new Rect(463, 574, 45, 27), "◀", selectedLabel)) run.SelectStage(first - StagesPerPage);
                    GUI.enabled = enabled && page + 1 < pages;
                    if (GUI.Button(new Rect(773, 574, 45, 27), "▶", selectedLabel)) run.SelectStage(first + StagesPerPage);
                    GUI.enabled = enabled;
                    GUI.Label(new Rect(511, 574, 258, 27), $"{page + 1} / {pages}", small);
                }
                GUI.Label(new Rect(330, 662, 620, 25), "← → / A D：選ぶ　　クリックでも選択", eyebrow);
                GUI.Label(new Rect(220, 688, 840, 20),
                    "制作：bq24014-cmd　／　Unity 6.3 LTS　／　使用素材はREADME参照", credits);
            }
            finally
            {
                GUI.matrix = oldMatrix;
                GUI.color = oldColor;
                GUI.enabled = oldEnabled;
            }
            if (quit) { run.QuitFromTitle(); GUIUtility.ExitGUI(); }
            if (help) { run.OpenMenuHelp(); GUIUtility.ExitGUI(); }
            if (start && run.StartSelectedStage()) GUIUtility.ExitGUI();
        }

        private bool ClothButton(Rect rect, string text, GUIStyle style, bool selected)
        {
            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            Color old = GUI.color;
            bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
            GUI.color = selected || hover ? Color.white : new Color(.83f, .85f, .88f);
            if (pieces != null && pieces[2] != null) DrawSprite(rect, pieces[2], true);
            else { GUI.color = Cream; GUI.DrawTexture(rect, Texture2D.whiteTexture); }
            GUI.color = old;
            FitLabel(new Rect(rect.x + 32, rect.y + 8, rect.width - 64, rect.height - 16), text, style, style.fontSize, 14);
            return clicked;
        }

        private static void FitLabel(Rect rect, string text, GUIStyle style, int preferred, int minimum)
        {
            int oldSize = style.fontSize;
            style.fontSize = preferred;
            while (style.fontSize > minimum && style.CalcHeight(new GUIContent(text), rect.width) > rect.height)
                style.fontSize--;
            GUI.Label(rect, text, style);
            style.fontSize = oldSize;
        }

        private void DrawPath(Vector2 a, Vector2 b, bool planned = false)
        {
            Color oldColor = GUI.color;
            if (planned) GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a * .55f);
            Vector2 last = a;
            float distance = 0;
            for (int i = 1; i <= 48; i++)
            {
                float t = i / 48f;
                Vector2 next = Vector2.Lerp(a, b, t) + Vector2.up * (55 * Mathf.Sin(t * Mathf.PI));
                if (!planned || (i - 1) / 3 % 2 == 0)
                    DrawThread(last, next, distance, planned ? 6 : 9);
                distance += Vector2.Distance(last, next); last = next;
            }
            GUI.color = oldColor;
        }

        private void DrawSelectionWeave(Vector2 center)
        {
            Color old = GUI.color;
            float trace = selectionWeave.TraceProgress;
            float knot = selectionWeave.KnotProgress;
            // Leave the selected label above and the cloth nameplate below unobstructed.
            Vector2 Point(float t) => center + new Vector2(
                Mathf.Cos(Mathf.PI + t * Mathf.PI * 2) * 147,
                Mathf.Sin(Mathf.PI + t * Mathf.PI * 2) * 103);
            const int segments = 96;
            float distance = 0;
            Vector2 previous = Point(0);
            for (int i = 1; i <= segments; i++)
            {
                float from = (i - 1) / (float)segments;
                if (from >= trace) break;
                Vector2 next = Point(Mathf.Min(i / (float)segments, trace));
                GUI.color = new Color(.09f, .035f, .07f, .42f);
                DrawThread(previous + Vector2.down * 1.5f, next + Vector2.down * 1.5f, 0, 8, false);
                GUI.color = yarn != null ? Color.white : Rose;
                DrawThread(previous, next, distance, 5.5f);
                distance += Vector2.Distance(previous, next);
                previous = next;
            }

            // A short loose end tightens into the same knot used elsewhere in the UI.
            // Only the yarn moves: no new hitbox, island bounce, sound or selection delay.
            if (knot > 0)
            {
                Vector2 join = Point(0);
                GUI.color = yarn != null ? Color.white : Rose;
                DrawThread(join + new Vector2(-8, 5), join + new Vector2(-12, 17 - 5 * knot), 0, 4 * knot);
                DrawThread(join + new Vector2(4, 6), join + new Vector2(8, 15 - 3 * knot), 0, 4 * knot);
                float size = Mathf.Lerp(25, 20, knot);
                GUI.color = new Color(1, 1, 1, knot);
                Sprite knotSprite = HimoHitoUiParts.MountingKnotSprite;
                if (knotSprite != null)
                    DrawSprite(new Rect(join.x - size / 2, join.y - size / 2, size, size), knotSprite, false);
                else
                {
                    GUI.color = new Color(Rose.r, Rose.g, Rose.b, knot);
                    DrawThread(join - new Vector2(4, 4), join + new Vector2(4, 4), 0, 7);
                    DrawThread(join + new Vector2(-4, 4), join + new Vector2(4, -4), 0, 7);
                }
            }
            GUI.color = old;
        }

        // A planted knitted pennant and cloth tag, independent of selection; never unlocks stages.
        // Reuse the bridge yarn so this remains crisp when the whole map is scaled.
        private void DrawClearFlag(Vector2 node)
        {
            Color old = GUI.color;
            Vector2 top = node + new Vector2(74, -118);
            Vector2 foot = node + new Vector2(74, -4);
            GUI.color = new Color(.34f, .16f, .065f);
            DrawThread(top + Vector2.right, foot + Vector2.right, 0, 6, false);
            GUI.color = new Color(.86f, .56f, .27f);
            DrawThread(top, foot, 0, 3, false);
            GUI.color = yarn != null ? Color.white : Rose;
            for (int row = 0; row < 8; row++)
            {
                Vector2 start = top + new Vector2(0, 5 + row * 4);
                float width = Mathf.Lerp(52, 10, row / 7f);
                DrawThread(start, start + new Vector2(width, 1.5f), 0, 5);
            }
            DrawThread(top + new Vector2(-4, 5), top + new Vector2(5, 5), 0, 7);

            Rect tag = new Rect(node.x + 21, node.y - 65, 106, 37);
            GUI.color = Color.white;
            if (pieces != null && pieces[2] != null) DrawSprite(tag, pieces[2], true);
            else
            {
                GUI.color = Cream;
                GUI.DrawTexture(tag, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(tag.x + 18, tag.y + 4, tag.width - 36, tag.height - 8), "CLEAR", clearLabel);
            GUI.color = old;
        }

        private void DrawThread(Vector2 a, Vector2 b, float distance, float width, bool textured = true)
        {
            Vector2 delta = b - a;
            Matrix4x4 matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            GUI.matrix = matrix * GUI.matrix;
            Texture2D texture = textured && yarn != null ? yarn : Texture2D.whiteTexture;
            float tile = textured && yarn != null ? width * yarn.width / yarn.height : 16;
            GUI.DrawTextureWithTexCoords(new Rect(a.x, a.y - width / 2, delta.magnitude + .5f, width), texture,
                new Rect(distance / tile, 0, (delta.magnitude + .5f) / tile, 1));
            GUI.matrix = matrix;
        }

        private static void DrawSprite(Rect rect, Sprite sprite, bool fill)
        {
            if (sprite == null) return;
            Rect source = sprite.textureRect;
            if (!fill)
            {
                float scale = Mathf.Min(rect.width / source.width, rect.height / source.height);
                rect = new Rect(rect.center.x - source.width * scale / 2, rect.yMax - source.height * scale,
                    source.width * scale, source.height * scale);
            }
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }

        private static Sprite GetStageArt(StageCatalog.Entry stage)
        {
            if (stage.mapArt == UpcomingArtPath)
            {
                if (upcomingPiece == null)
                    upcomingPiece = LoadKeyedPieces(Resources.Load<Texture2D>(UpcomingArtPath), 1)?[0];
                if (upcomingPiece != null) return upcomingPiece;
            }
            if (pieces == null) return null;
            if (stage.mapArt == "practice") return pieces[0];
            if (string.IsNullOrEmpty(stage.mapArt) || stage.mapArt == "toybox") return pieces[1];
            if (!CustomArt.TryGetValue(stage.mapArt, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>(stage.mapArt);
                CustomArt[stage.mapArt] = sprite;
            }
            return sprite != null ? sprite : pieces[1];
        }

        private void EnsureResources()
        {
            if (heading != null) return;
            background = Resources.Load<Texture2D>(ArtRoot + "Background-v1");
            yarn = YarnRopeTexture.Load();
            EnsurePieces();
            GUIStyle Style(int size, bool bold, Color color) => new GUIStyle(GUI.skin.label) {
                fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = color }
            };
            heading = Style(48, true, Rose); eyebrow = Style(14, false, Muted);
            label = Style(22, true, Ink); small = Style(16, false, Muted);
            description = Style(19, false, Cream); button = Style(18, true, Ink);
            selectedLabel = Style(15, true, Rose);
            credits = Style(11, false, Cream);
            clearLabel = Style(15, true, new Color(.49f, .16f, .27f));
            clearLabel.wordWrap = false;
            foreach (GUIStyle style in new[] { heading, eyebrow, label, small, description, button, selectedLabel, credits, clearLabel })
            {
                Color color = style.normal.textColor;
                style.hover.textColor = style.active.textColor = style.focused.textColor = color;
                style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = color;
                HimoHitoGuiTheme.ApplyToStyles(style);
            }
        }

        private static void EnsurePieces()
        {
            if (pieces != null && pieces[0] != null) return;
            pieces = LoadKeyedPieces(Resources.Load<Texture2D>(ArtRoot + "Pieces-v1"), 3);
        }

        private static Sprite[] LoadKeyedPieces(Texture2D source, int columns)
        {
            if (source == null) return null;
            // Key the generated atlas's technical white backdrop, keeping warm cream and colored fibers.
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 p = pixels[i];
                int low = Mathf.Min(p.r, Mathf.Min(p.g, p.b));
                int high = Mathf.Max(p.r, Mathf.Max(p.g, p.b));
                if (low >= 228 && high - low <= 15) { p.a = 0; pixels[i] = p; }
            }
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false) {
                name = source.name + " transparent", filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var result = new Sprite[columns];
            for (int part = 0; part < columns; part++)
            {
                int left = part * source.width / columns, right = (part + 1) * source.width / columns;
                int minX = right, maxX = left, minY = source.height, maxY = 0;
                for (int y = 0; y < source.height; y++)
                    for (int x = left; x < right; x++)
                        if (pixels[y * source.width + x].a > 20)
                        { minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                if (minX > maxX || minY > maxY) continue;
                result[part] = Sprite.Create(texture, new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
                    new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                result[part].hideFlags = HideFlags.HideAndDontSave;
            }
            return result;
        }
    }
}
