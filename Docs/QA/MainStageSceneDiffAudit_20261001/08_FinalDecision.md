# Final Decision — CASE S4

**提出時状態の保存としては意味があるが、Runtime authorityとしては冗長。**

問い：「未Commit MainStageがHuman-approvedな現在のゲーム状態に必要か？」

回答：**最新HEAD Sourceを使う通常の起動後状態には不要。ただし提出時Sceneの歴史的保存としての意味はある。**

## 根拠と最終報告

1. Git保護：main / HEAD / origin/mainは基準`4919233cd1d0815afb438bd35a7336492ec09548`を維持。index空。既存のMainStage/README/LEARNING_LOG変更、旧QA、output、過去Buildを保持。新規本QAと新規隔離projectのみ作成。
2. MainStage開始/終了SHA一致：`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`。原本Scene 1byte不変。
3. 意味を持ち得る差分36単位：既存property16＋追加Unity document20。起動後に残る独立Scene authority差分は0。全property表は570行。
4. Noise：E 46行＋D 134行（default/header）＝180表行。diff-checkは49警告、うち46空白変更・2新規document・1既存同値documentのGit行対応。改行のみ0、fileID振り直し0。
5. 主なObject：S05 Left/Right Shelf、各Blue Railway Platform Visual、各Craft Rail Tie 0/1/2、左右CraftRailPlatformVisual追加。S02 Front/Far SpikeのWooden Toy Spike Face。
6. Runtime codeで上書き/再生成：CraftRail component追加とenabled/allowTutorialLegacy、描画位置・scale・drawMode・size、Tie生成/姿勢/Renderer、棘Faceのscale/sprite/描画size。Source位置は03に一対一対応。
7. Runtimeに残るScene差：対象のtransform/collider/active/component/Renderer/serialized値のA/B差0。意味を持つ未解明F 0。
8. 提出版：現在WTは提出用snapshot実Scene・保存manifestと完全一致。**WORKING TREE = SUBMISSION SCENE**。
9. A/B：対象14ノードのsnapshot一致。局所render3組、各1280×720で差0pixel、各組PNG SHA一致。比較前にSceneをEditor auto-saveして均したものではない。
10. 最終CASE：**S4**。RuntimeだけならS3条件に合うが、提出snapshotとの実ファイル一致という歴史的保存価値があるためS4を選ぶ。S1/S2の「実行に必要なScene差」とする根拠なし。
11. Commit推奨：**このraw Scene差分を必要なGameplay fixとしてそのままcommitすることは推奨しない。** 提出時保存を明示したarchival checkpointには価値があるが、その目的・方法の選択は次Taskでユーザー判断とする。不要だから即revertしてよい、という承認ではない。
12. 次に行う1作業：**提出時Sceneの安全な保存方針を決める**。本監査を根拠に、歴史的snapshotの保存と運用Scene整理をどう分離するか合意する。まだSceneを掃除・保存・revertしない。
13. stage / commit / pushなし、追加実装・調整なし。Final Decisionで停止。

## 留意点

Runtime A/B中にUnityEditor.Searchの起動index例外が双方にあった。05に記載しログ保存。今回の対象snapshot/画像生成は成功したが、全Editorログ無エラーとはしない。

既存Human承認H1/H2/H3/S2は維持。今回自動比較を新しいHuman Art PASSや全ゲーム回帰PASSとして扱わない。結果の範囲は、実Serialized差分とその対象の通常setup後状態。

終了保護の機械証拠は`Evidence/protection_result.json`。元のScene、Source、既存成果物を勝手にstage/commit/revertする次工程は開始していない。
