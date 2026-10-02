# Goal-right seam fix

## 原因

分類は **image edge + mirrored tile boundary**。follow phaseのリセットや3層の基準位置の食い違いではない。

承認済みMid PNGのx1919は全1080行がRGBA=0。x1918は透明pixelなし、平均alpha251.583。Midを切り出した床帯も同じ右端を持つ。透明端がFarを露出させ、ゴール右に細い暗色/青色の線を出していた。さらにR1/T1の床ShaderはSpriteRendererの`_Flip`を適用せず、左右guardの`flipX=true`が描画へ反映されず、隣接する床帯の模様の切れ目が生じていた。

実際の1600x900撮影projectionではMid境界はviewport x約.91、床帯境界は約.825。両方が画面内にある。初期のEditor既定aspectでのx=1.0477（画面外）は撮影projectionと異なり、原因の切り分けには使わない。`seam_position.txt`の座標と2箇所の拡大画像で照合。Runtime04の末尾に残った「Mid画面外」という診断メモは誤りであり、その数値観測が示す約.91を採用する。

## 最小変更

- PNGを描き直さず、Shaderで最後の内側pixel中心`1 - 1.5 * texelWidth`までUVをclamp。中央部分のsampleは従来と同じ。
- 床Shaderのvertex処理を既存Unity `SpriteVert`にし、`flipX`を正しく適用。
- Midにも同じ端sample補正を適用。Unity標準Spriteと同じpremultiplied alphaを維持。
- 配置、タイル幅、follow .99/.92/.88、Z、色調、R1の床上端フェード、小物、接地影は維持。

Unity 6000.3.21f1のローカル`UnitySprites.cginc`の`SpriteVert` / `UnityFlipSprite`を確認。新しい背景方式やCamera調整ではない。

## 見た目

`Screenshots/S4_Clear_Comparison.png`は同じPlayer/Camera/Collider/Joint/Bridge状態を連続撮影したT1/Production比較。Before右下に見えていた縦線がAfterでは消え、木目が反転境界へ自然につながっている。上部・壁・棚・窓の構図は維持。層別画像も保存。

これは1600x900の実Unity world-camera描画の確認。IMGUIは含まない。実入力によるゴール付近の移動中の最終見た目はHuman Reviewで確認する。
