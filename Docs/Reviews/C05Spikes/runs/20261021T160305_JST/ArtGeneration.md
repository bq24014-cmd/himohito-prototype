# C05 ハサミ素材

新素材は `Assets/HimoHitoCraftScissorsParts-v1.png` と同名 `.meta`。1536×1024 RGBAの透過atlas1枚に左右のハサミ部品と布ループを収めた。これは1つのハサミのパーツであり、複数のデザイン案ではない。

既存の実ゲーム画面と毛糸リング素材を目視し、赤い木の持ち手・金属の刃・綿の吊りループへ質感を整理した。内蔵image_genで生成。生成への入力画像は使わず、下記の記述を用いた。生成後のピクセル編集、背景抜き、描き足しは行っていない。UnityのSprite矩形と支点による切り出し・回転だけで組み立てた。

- SHA256：`8791676e78e3ca32f5de1809be42938f641043fe25564a5158d9a13d8b332b71`
- meta GUID：`0d7da59a98c145d2bc2e974dd4d06889`
- 保存先：このrunの[元PNG](Assets/HimoHitoCraftScissorsParts-v1.png)／[meta](Assets/HimoHitoCraftScissorsParts-v1.png.meta)。ローカル隔離Proofにも保持。
- 原本および旧三角素材を保持。ProductionのAssetsへ追加していない。
- `.meta`は新規素材の識別用。今回Unity importerでの焼込み成功を示すものではない。

実際のプロンプトは[GenerationPrompt.txt](Assets/GenerationPrompt.txt)。ゲーム画面へ合成する生成画像ではなく、UnityのSpriteとして描画する素材atlasとして生成した。
