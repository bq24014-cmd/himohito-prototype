# 壁を押す主人公の素材

- 作成日：2026-09-09
- 使用方法：imagegenスキル／内蔵image_gen（CLI/APIではない）
- 採用素材：Assets/Resources/Art/HimoHitoWallPush-v1.png（1536×1024、3列×2行）
- 参照：Assets/Resources/Art/HimoHitoWeave-v1.png。元素材は変更なし。
- 生成結果の薄いチェック背景は、他のアニメーション素材と同じ既存のLoadAnimationFramesによる読み込み時の明色無彩色除去で透過する。生成PNG自体を完全透過と誤認しない。ゲーム内の描画で背景が残らないことを確認する。
- コマは左上から右へ読み、次に下段。0・1が手を伸ばす動き、2・3・4・3が控えめな踏ん張り、5は大きさの基準となる中立姿勢。
- 頭幅と足裏を既存ローダーで登録し、手先を壁に合わせるのは描画用Transformのみ。新しい物理動作は追加しない。

## 採用画像の生成プロンプト

Use case: stylized-concept. Asset type: production Unity 2D character sprite sheet. Input image is CHARACTER IDENTITY AND ART STYLE REFERENCE ONLY. Generate a NEW wall-pushing animation sheet of exactly this pink yarn doll, same dark outline, pink knitted loops, huge round head, small oval torso, tiny legs, two loose yarn tails, crown yarn strand, black oval eyes, same right-facing 3/4 profile. Genuinely transparent RGBA background, no checkerboard painted in, no shadows or ground. Canvas 1536x1024: EXACT 3 columns by 2 rows, six equal 512x512 cells. One complete character per cell, with generous clear margin. Keep same head diameter in every cell, same planted foot baseline and same foot center. All poses face RIGHT toward an invisible wall (DO NOT draw a wall). Both little yarn arms reach forward and palms contact the same imaginary vertical plane just in front of the head. Row-major frames: 1 starting to raise hands to wall, feet planted; 2 hands contact wall elbows slightly bent and knees bend; 3 leaning torso gently forward with both hands braced, one foot behind firmly planted; 4 effort pose, knees slightly deeper, head unchanged size and hands stay on wall, crown yarn bends gently backward; 5 settles slightly, still pushing and hands planted, yarn tip swings a little forward; 6 neutral standing pose with arms down as in reference, full head and feet. No head morph or body stretching, no extra appendages, no sweat marks, no text, numbers, grid lines, wall or props. Clear readable arm and leg articulation; same appealing handmade pink protagonist and color as reference. Fixed orthographic view, same scale across six cells.
