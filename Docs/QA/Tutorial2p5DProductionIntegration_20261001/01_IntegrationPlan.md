# Integration Plan

基準main / HEAD / origin/main: `702a4bc044b0eb35bedb552753ca8495599bc1ee`。開始fetch成功。既存MainStage Scene/README/LEARNING_LOG変更、旧Build、BGM、P1/R1/T1とQAを保護する。

1. 開始時の全既存ファイルSHAを採取し、変更前Working TreeをBaselineProjectへ隔離コピー。
2. 継ぎ目を画像端/タイル/phaseごとに調査。SourceEdges.jsonでMid右端x1919は全1080pxがRGBA=0、内側x1918はalpha平均251.583。Far端はopaque、Near端は玩具のない透明余白。Midの透明端でFarが露出することを層別描画で検証する。撮影projectionとEditor既定aspectを混同しない。
3. Mid/床帯のSprite samplingを最後の内側pixel中心までclampする。PNGはbyte無変更。Z、follow、配置関数、中央部分の色・質感はT1から変更しない。
4. HimoHitoCraftRoomBackground.Ensureを背景の入口のまま維持し、Tutorial runtimeでTutorialCraftRoomLayersをrootにEnsureする。保存済み背景のOnEnable経路も同じEnsureを呼び、Editor setupに依存させない。独立したtrial bootstrapをProductionへ入れない。旧背景はmenu/preview/fallbackを担当し、新3層が有効なときは旧Rendererの描画だけ止める。MainStageではscene guardで新3層を設置しない。
5. 3つの承認済みPNGとshaderをResourcesへ配置、Unity .metaを保持。Scene/ProjectSettings/PackagesをProductionで編集しない。
6. Production Working TreeをIntegrationProjectへ新規コピー。Editor専用診断でT1 Before/Production Afterを同一物理状態で比較。4区間の実物理ルート、全操作、Section境界、原背景二重表示、再開、Clearを確認。
7. 同じUnity 6000.3.21f1とBuild手順でBefore/After Windows Buildを作成。CPU frame/texture memory/build sizeを比較し、同条件でFPSを観測。診断時計の撮影GIFをFPS評価に使わない。
8. Human用画像と新Buildを用意し、PI判定と保護照合を記録。commit/pushせずHuman Reviewで停止。

Gameplay変更は禁止。Cameraを読むだけで、Player/Hook/Platform/Collider/Joint/Resource/Bridge/Tutorial進行/入力/看板のSourceは触らない。検証時計/操作キャッシュ/APIはEditor専用QAのみ。
