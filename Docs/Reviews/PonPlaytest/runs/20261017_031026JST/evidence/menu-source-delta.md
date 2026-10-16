# 操作の見た目とイベント実体：短い差分根拠

状態遷移・入力guardは既存08を参照。今回はクリックreceiverと独自描画の違いだけを抜粋。通常入力を実行した記録ではない。

## HimoHitoUiParts.cs:219–242

```text
219:         public static void DrawWoodButtonLabel(
220:             Rect rect,
221:             string text,
222:             GUIStyle style)
223:         {
224:             DrawWoodButton(rect);
225: 
226:             GUIStyle buttonLabelStyle = new GUIStyle(style)
227:             {
228:                 alignment = TextAnchor.MiddleCenter
229:             };
230:             buttonLabelStyle.normal.textColor = new Color(0.11f, 0.08f, 0.20f);
231:             GUI.Label(rect, text, buttonLabelStyle);
232:         }
233: 
234:         public static bool WoodButton(Rect rect, string text, GUIStyle style)
235:         {
236:             bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
237:             Color previous = GUI.color;
238:             if (GUI.enabled && rect.Contains(Event.current.mousePosition))
239:                 GUI.color = previous * new Color(1f, .9f, .8f, 1f);
240:             DrawWoodButtonLabel(rect, text, style);
241:             GUI.color = previous;
242:             return clicked;
```

## StageSelectionView.cs:154–164

```text
154:         private bool ClothButton(Rect rect, string text, GUIStyle style, bool selected)
155:         {
156:             bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
157:             Color old = GUI.color;
158:             bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
159:             GUI.color = selected || hover ? Color.white : new Color(.83f, .85f, .88f);
160:             if (pieces != null && pieces[2] != null) DrawSprite(rect, pieces[2], true);
161:             else { GUI.color = Cream; GUI.DrawTexture(rect, Texture2D.whiteTexture); }
162:             GUI.color = old;
163:             FitLabel(new Rect(rect.x + 32, rect.y + 8, rect.width - 64, rect.height - 16), text, style, style.fontSize, 14);
164:             return clicked;
```

## StageSelectionView.cs:339–344

```text
339:             foreach (GUIStyle style in new[] { heading, eyebrow, label, small, description, button, selectedLabel, credits, clearLabel })
340:             {
341:                 Color color = style.normal.textColor;
342:                 style.hover.textColor = style.active.textColor = style.focused.textColor = color;
343:                 style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = style.onFocused.textColor = color;
344:                 HimoHitoGuiTheme.ApplyToStyles(style);
```

## StageOverlayControls.cs:281–290

```text
281:             bool close = GUILayout.Button("Tab / Esc　閉じる", helpKeyStyle);
282:             GUILayout.EndArea();
283:             if (close) { CloseHelp(); GUIUtility.ExitGUI(); }
284:         }
285: 
286:         private void DrawHelpRow(string key, string description)
287:         {
288:             GUILayout.BeginHorizontal();
289:             GUILayout.Label(key, helpKeyStyle, GUILayout.Width(112f));
290:             GUILayout.Label(description, helpBodyStyle);
```

## StageOverlayControls.cs:475–481

```text
475:             helpKeyStyle = new GUIStyle(GUI.skin.label)
476:             {
477:                 alignment = TextAnchor.MiddleCenter,
478:                 fontSize = 20,
479:                 fontStyle = FontStyle.Bold,
480:                 normal = { textColor = new Color(0.78f, 0.20f, 0.38f) }
481:             };
```

## HimoHitoFailureScreen.cs:71–76

```text
71:             HimoHitoUiParts.DrawWoodButtonLabel(
72:                 retryButton,
73:                 retryPrompt,
74:                 promptStyle);
75:             GUILayout.Label("Esc　ステージ選択へ", bodyStyle);
76:             GUILayout.EndArea();
```

## ClearJourneyView.cs:41–45

```text
41:                 bool primary = HimoHitoUiParts.WoodButton(new Rect(108, 588, 540, 58), tutorial
42:                     ? "Enter　本編ステージへ" : "R　本編を最初から再挑戦", action);
43:                 bool stageSelection = HimoHitoUiParts.WoodButton(new Rect(672, 588, 500, 58),
44:                     "Esc　ステージ選択へ戻る", action);
45:                 return stageSelection ? Choice.StageSelection : primary ? Choice.Primary : Choice.None;
```
