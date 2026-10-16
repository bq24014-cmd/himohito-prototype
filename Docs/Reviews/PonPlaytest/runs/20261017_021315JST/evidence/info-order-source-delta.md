# 情報提示順：追加根拠と既存根拠への入口

A=0。既存の長い抜粋を再複製せず、今回の判断に必要な追加箇所だけを収録する。
- [既存の基本ソース抜粋](https://github.com/bq24014-cmd/himohito-prototype/blob/6822eaa610476aac7a2adc5838d37fcd9f5c1f8b/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/source-excerpts.md)
- [T4アニメ/図の既存抜粋](https://github.com/bq24014-cmd/himohito-prototype/blob/6822eaa610476aac7a2adc5838d37fcd9f5c1f8b/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/t4-s10-source-excerpts.md)
- [UI状態遷移の既存抜粋](https://github.com/bq24014-cmd/himohito-prototype/blob/6c767d1f843d86eabfad5342bd805917d40f9ad0/Docs/Reviews/PonPlaytest/runs/20261017_014618JST/evidence/ui-state-source-excerpts.md)

## Assets/Resources/StageCatalog.json:1–31

```text
1: {
2:   "stages": [
3:     {
4:       "id": "tutorial",
5:       "mapArt": "practice",
6:       "title": "はじめての毛糸",
7:       "subtitle": "TUTORIAL  /  チュートリアル",
8:       "description": "掛ける・振る・編む。看板と一緒に、4つの区間で練習しよう。\nヒモ残量 99 ／ 第1区間から",
9:       "scenePath": "Assets/Scenes/Tutorial.unity",
10:       "available": true
11:     },
12:     {
13:       "id": "main",
14:       "mapArt": "toybox",
15:       "title": "おもちゃ箱への道",
16:       "subtitle": "STAGE 01  /  本編",
17:       "description": "自分のヒモを使って、夜の子供部屋を冒険。\nヒモ残量 50 ／ 第1区間から",
18:       "scenePath": "Assets/Scenes/MainStage.unity",
19:       "available": true
20:     },
21:     {
22:       "id": "upcoming",
23:       "mapArt": "Art/HimoHitoStageMapUpcoming-v1",
24:       "title": "つぎの冒険",
25:       "subtitle": "COMING SOON  /  追加予定",
26:       "description": "新しいステージを追加予定です。\n今はまだあそべません。お楽しみに。",
27:       "scenePath": "",
28:       "available": false
29:     }
30:   ]
31: }
```

## Assets/Scripts/TutorialSignInscription.cs:59–100

```text
59:             // Coordinates are normalized to the original square artwork, inside the board face.
60:             switch (section)
61:             {
62:                 case 1:
63:                     Vector2 swingHead = PlayerPicture(new Vector2(.255f, .54f), .125f);
64:                     firstRope=Curve(swingHead, new Vector2(.34f, .72f), new Vector2(.42f, .80f), true);
65:                     Ring(new Vector2(.42f, .80f), Blue);
66:                     Arrow(new Vector2(.27f, .50f), new Vector2(.51f, .50f));
67:                     Key("E", new Vector2(.72f, .69f), .15f, .23f);
68:                     break;
69:                 case 2:
70:                     LengthChoicePicture();
71:                     Key("W", new Vector2(.72f, .76f), .11f, .22f);
72:                     Key("S", new Vector2(.72f, .61f), .11f, .22f);
73:                     break;
74:                 case 3:
75:                     Shelf(.20f, .76f, -.055f);
76:                     Shelf(.55f, .76f, .055f);
77:                     firstRope=Curve(new Vector2(.20f, .76f), new Vector2(.375f, .48f), new Vector2(.55f, .76f), true);
78:                     Ring(new Vector2(.20f, .76f), Green);
79:                     Ring(new Vector2(.55f, .76f), Green);
80:                     PlayerPicture(new Vector2(.375f, .625f), .125f);
81:                     Key("Q", new Vector2(.72f, .69f), .15f, .23f);
82:                     break;
83:                 case 4:
84:                     firstRope=Curve(new Vector2(.20f, .76f), new Vector2(.27f, .65f), new Vector2(.375f, .81f), false);
85:                     secondRope=Curve(new Vector2(.375f, .81f), new Vector2(.48f, .65f), new Vector2(.55f, .76f), false);
86:                     mergedRope=Curve(new Vector2(.20f, .76f), new Vector2(.375f, .40f), new Vector2(.55f, .76f), true);
87:                     middleRing=Ring(new Vector2(.375f, .81f), Blue);
88:                     Ring(new Vector2(.20f, .76f), Green);
89:                     Ring(new Vector2(.55f, .76f), Green);
90:                     Arrow(new Vector2(.375f, .72f), new Vector2(.375f, .62f));
91:                     PlayerPicture(new Vector2(.46f, .625f), .115f);
92:                     Key("F", new Vector2(.72f, .69f), .15f, .23f);
93:                     break;
94:             }
95:             if(section>=3)
96:             {
97:                 connectionRope=Curve(Vector2.zero,Vector2.zero,Vector2.zero,true);
98:                 SetRopeAppearance(connectionRope,0f);
99:             }
100:             if(Application.isPlaying) AdvanceAnimation(0f);
```

## Assets/Scripts/TutorialSectionGuide.cs:37–50

```text
37:         private static readonly string[] SectionSummaries =
38:         {
39:             "青いHookへヒモを掛け、歩き出して振り子になり、対岸へ渡ります。",
40:             "長さで振り子の底が変わります。トゲを避ける長さを選びます。",
41:             "頭上にHookがない谷です。対岸の緑フックへ掛けたヒモを足場にします。",
42:             "2本の足場を中央で外し、梁の下を通れる1本の橋へまとめます。"
43:         };
44: 
45:         private static readonly string[] SectionSteps =
46:         {
47:             "1　W / Sで長さ、矢印キーで向きを調整\n2　青いHookへ照準を合わせて E\n3　右へ歩いて振り子になり、対岸へ着地",
48:             "1　長さ8では底が下がり、トゲに当たる\n2　Eで外し、W / Sで長さ6～7を選ぶ\n3　もう一度 Eで掛け、トゲの上を渡る",
49:             "1　W / Sで長さ7にする（推奨）\n2　対岸の緑フックへ E → Qで足場にする\n3　残量を7使ってできた橋を歩いて渡る",
50:             "1　左岸→中央→右岸へ長さ6の足場を2本作る\n2　中央の青いHookへ照準を合わせる\n3　Fで中央を外し、深くたわんだ1本の橋を渡る"
```

## Assets/Scripts/TutorialSectionGuide.cs:762–764

```text
762:             GUI.Label(NormalizedRect(rect,.2f,.88f,.6f,.1f),
763:                 f.Bridge>0f ? "残量の例　99 → 92（戻らない）" : "掛けるだけなら消費なし",
764:                 diagramLabelStyle);
```

## Assets/Scripts/TutorialSectionGuide.cs:1107–1120

```text
1107:             summaryStyle = new GUIStyle(GUI.skin.label)
1108:             {
1109:                 alignment = TextAnchor.MiddleLeft,
1110:                 fontSize = 18,
1111:                 wordWrap = true,
1112:                 normal = { textColor = new Color(0.26f, 0.22f, 0.36f) }
1113:             };
1114:             stepStyle = new GUIStyle(GUI.skin.label)
1115:             {
1116:                 alignment = TextAnchor.UpperLeft,
1117:                 fontSize = 20,
1118:                 fontStyle = FontStyle.Bold,
1119:                 wordWrap = true,
1120:                 normal = { textColor = ink }
```

## Assets/Scripts/TutorialGuideAnimation.cs:64–65

```text
64:                     f.Caption=t<1.1f ? "推奨：長さ7で対岸の緑Hookを狙う" : t<2.5f ? "E　対岸へ接続" :
65:                         t<4.4f ? "Q　編んで足場にする（消費7）" : "D　できたヒモ橋を歩いて渡る";
```

## Assets/Scripts/TutorialGuideAnimation.cs:79–93

```text
79:                         f.Caption=t<.8f ? "長さ6で中央の青いHookを狙う" : t<1.8f ? "E　左岸から中央へ接続" :
80:                             t<3.3f ? "Q　1本目を編む（消費6）" : t<5.5f ? "D　できた橋を歩いて中央へ" :
81:                             t<6.5f ? "長さ6で右岸へ E" : t<8.5f ? "Q　2本目を編む（消費6）" :
82:                             "2本の橋ができた！";
83:                         break;
84:                     }
85:                     // Continue directly from the completed pair, with no position reset.
86:                     t-=9f;
87:                     f.Merge=Ease(t,4.8f,5.7f);
88:                     float along=.5f+.24f*Ease(t,1.4f,2.6f)-.24f*Ease(t,3.6f,4.8f);
89:                     along+=.74f*Ease(t,6.6f,10f);
90:                     f.Feet=along<=1f ? MergedPoint(along,f.Merge) : new Vector2(.76f+(along-1f)*.52f,.67f);
91:                     f.Caption=t<2.6f ? "2本の橋では通り道が高い" : t<3.6f ? "梁があって進めない" :
92:                         t<4.8f ? "中央の青いHookへ戻って狙う" : t<6.6f ? "F　中央を外すと1本に下がる" :
93:                         "D　低くなった橋で梁の下へ";
```

## Assets/Scripts/RopePlatformBuilder.cs:47–51

```text
47:         public float CurrentPlatformCost =>
48:             IsPlatformBuildingUnlocked &&
49:             ropeController != null && ropeController.IsAttached
50:                 ? GetCurrentPlatformCost()
51:                 : 0f;
```

## Assets/Scripts/RopePlatformBuilder.cs:403–406

```text
403:         private float GetCurrentPlatformCost()
404:         {
405:             return ropeController.ActiveRopeLength;
406:         }
```

## Assets/Scripts/RopeResourceGauge.cs:39–65

```text
39:         // Explicit success notification: a restore or temporary attachment is
40:         // not a permanent spend, even when it changes the displayed amount.
41:         public static void NotifyPlatformBuilt(RopeResource resource, float spentLength)
42:         {
43:             if (resource == null || spentLength <= 0f || float.IsNaN(spentLength) ||
44:                 float.IsInfinity(spentLength)) return;
45:             float now = Time.time;
46:             float before = resource.CurrentLength + spentLength;
47:             float from = Mathf.Clamp01(before / resource.MaximumLength);
48:             float cost = spentLength;
49:             // Rapid successful builds continue from the visible marker and combine their labels.
50:             if (spendingResource == resource && now >= spendStartedAt &&
51:                 now - spendStartedAt < LabelDuration &&
52:                 Mathf.Approximately(capacityAtSpend, resource.MaximumLength) &&
53:                 Mathf.Approximately(remainingAfterSpend, before))
54:             {
55:                 from = Mathf.Max(from, AnimatedRatio(now));
56:                 cost += displayedCost;
57:             }
58:             spendingResource = resource;
59:             spendStartedAt = now;
60:             startRatio = from;
61:             remainingAfterSpend = resource.CurrentLength;
62:             capacityAtSpend = resource.MaximumLength;
63:             displayedCost = cost;
64:             costText = "−" + cost.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
65:         }
```

## Assets/Scripts/RopeResourceGauge.cs:94–107

```text
94:         public static void Draw(RopeResource ropeResource)
95:         {
96:             if (ropeResource == null)
97:             {
98:                 return;
99:             }
100: 
101:             Rect outerRect = GUILayoutUtility.GetRect(
102:                 1f,
103:                 30f,
104:                 GUILayout.ExpandWidth(true),
105:                 GUILayout.Height(30f));
106:             DrawAtRect(ropeResource, outerRect, Time.time);
107:         }
```

## Assets/Scripts/RopeResourceGauge.cs:172–188

```text
172:             if (frame.LabelAlpha > 0f)
173:             {
174:                 if (costStyle == null)
175:                 {
176:                     costStyle = new GUIStyle(GUI.skin.label) {
177:                         fontSize = 15, fontStyle = FontStyle.Bold,
178:                         alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip,
179:                         padding = new RectOffset(), margin = new RectOffset(), wordWrap = false
180:                     };
181:                     costStyle.normal.textColor = SpentColor;
182:                     HimoHitoGuiTheme.ApplyToStyles(costStyle);
183:                 }
184:                 GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b,
185:                     previousColor.a * frame.LabelAlpha);
186:                 GUI.Label(new Rect(outerRect.xMax + 2f, slot.y + 3f,
187:                     Mathf.Max(0f, gutter - 2f), 24f), costText, costStyle);
188:             }
```

## Assets/Scripts/MainStageSectionThreeRecoverySwitch.cs:138–145

```text
138:             GUI.Box(
139:                 new Rect(
140:                     (Screen.width - 360f) * 0.5f,
141:                     Screen.height - 92f,
142:                     360f,
143:                     48f),
144:                 "F：帰り道のスイッチを押す",
145:                 style);
```

## 保存Sceneと呼出しの反証確認

- Tutorial.unity:1819は `startFromSectionFourForDevelopment: 0`。
- PrototypeHudのscript GUIDで保存Sceneを照合：Tutorialに1個、MainStageに0個。固定Scripts内の `AddComponent<PrototypeHud>` は0件。
- `RopeResourceGauge.NotifyPlatformBuilt` の固定Scripts側呼出しはRopePlatformBuilder.cs:113。
- Scene原本もBuildInputsへハッシュ一致。通常状態のOnGUIが消費後のゲージを描くことと、Q前の予告があることを分ける。
