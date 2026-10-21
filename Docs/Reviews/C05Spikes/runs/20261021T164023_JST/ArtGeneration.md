# 緑の樹脂製ハサミの生成素材

組み込みimagegenで新しく作成した[透過PNG](Assets/HimoHitoGreenScissorsParts-v2.png)と[meta](Assets/HimoHitoGreenScissorsParts-v2.png.meta)。[実際の生成指示](Assets/GenerationPrompt.txt)を保存した。1254×1254 RGBA。左半分・右半分・ネジ頭の3部品で、各半分は持ち手から刃まで一体。

Humanの追記により実物写真との一致は要件にせず、緑の細い楕円ハンドル、長い柄、中央ネジ、先細りの金属刃という文章の構造基準を使った。写真を参照入力したとは扱わない。既存ゲームの実画面は質感・明暗の判断に使い、生成ツールへは画像を渡していない。

生成PNGのRGBとalphaは加工していない。Unity内のSprite矩形と支点で部品を指定し、ネジ頭のみの矩形も使用。スケール550px/unitは1280基準の座標指定から実画像寸法へ正規化した。各部品の角度を共通の支点で変え、その後に同じ床面投影を適用する。これにより持ち手と刃が別々に滑らない。

PNG単体は**生成素材**であって実ゲーム画像ではない。[Report](Report.md)の通常画角・連続コマ・動画は、その素材をSpriteRendererで表示した**実Unity Player撮影**。図面は計測と素材の代表点による説明図として区別した。

ローカル保存先は `C05FloorScissorsProof_20261021/Assets/HimoHitoGreenScissorsParts-v2.png`。Runtimeにも同一SHAのPNGを配置。旧ハサミ・旧三角トゲの素材は保持した。importer／Resources焼込みは未検証で、外部PNG実読込の検証と分ける。
