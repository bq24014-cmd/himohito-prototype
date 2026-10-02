# Final Decision

**IF1 — イントロの不統一解消。技術検証PASS / Human Review APPROVED。**

## 判定根拠

1. 現行ProductionのTutorialCraftRoomLayers.Refreshで、Preview/StageStartTransition中に新背景を隠しlegacyを戻す条件を特定。
2. WaitingToStartとenabledの契約を残し、Preview/Transitionの否定2条件だけ削除。Sourceのbyte差分は予定した置換だけと完全一致。
3. 独立Before/Afterコピーで実Stage Selection開始布→スキップしないIntro→操作復帰を実行。Before A〜Dはlegacy、新版はA〜FでFar/Mid/Near表示・legacy0。
4. AFTER Editorの13 assertions PASS / playing frames1,733 / 新3層非表示0 / 二重drawable0。
5. Windows新規Build Succeeded。実exeの同じ13 assertions PASS / playing frames2,156 / 新3層非表示0 / 二重drawable0。通常Human起動は診断フラグなし。
6. 既存127回帰checksすべてPASS / failures0 / Runtime errors0 / 3実物理Section遷移 / 35motion samples。
7. ゴール端Shaderとfloor flipのassert PASS。PNG/Shaderは改変なし。全区間の背景位相、橋・Merge・R・Clear・メニュー復帰を維持。
8. Cameraは既存自然経路。Preview全CSVとPanProgress式のX誤差6.53e-6未満。Y/Z/orthoはBefore/After一致。Gameplay/Physics/Player/Hook/Stage構成を変更していない。
9. Performance：owned最大15は既存構成、legacy0。新素材・Renderer・Shader追加なし。限定Editor参考測定は04に記録。Humanの体感確認は別。
10. 保護5,434既存ファイル中、許可Source1件以外はすべてSHA一致。新Productionファイル0。既存Scene・BGM・QA・過去output/Builds・README/LEARNING_LOGを保持。DESERT LOOPへアクセスしていない。

## SHA / Git

- main / HEAD / origin/main：`702a4bc044b0eb35bedb552753ca8495599bc1ee`（開始・終了で同一）。
- TutorialCraftRoomLayers.cs Before：`1AE69EDB11D8328B405C0EE0F5FEACC869640BF92312072D119AFA13044E4CF6`。
- TutorialCraftRoomLayers.cs After：`E08312BCC8B61F31C9139EA0122AEA57726721263B4FB744A7AF7CDB10B7CE4F`。
- MainStage.unity：`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`。既存未Commit状態を維持。
- Tutorial.unity：`DC1BED54CC083256CE895041B1C93578101E19B9738E7CF260619E8F7E1E4B92`。
- BGM：`25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。
- index空。stage・commit・push未実施。既存global diff whitespace警告は未変更MainStage由来で、今回条件patchの個別diff-checkは問題なし。

## Human Review

新規Windows exeを診断フラグなしで通常起動し、Introをスキップせず「途中で旧版へ戻らない」「操作開始時に切替感がない」ことを確認する依頼を出した。2026-10-02、ユーザーから **「問題なし・採用してよい」** と回答を受領。今回版のIntro修正をHuman APPROVEDと記録する。

Build：`output/Tutorial2p5DIntroFix_20261002_Review01/Game/HimoHitoIntroFix.exe`。

Human承認を記録して停止。全4区間の127項目は自動診断であり、今回Humanが全項目を再実施したとは主張しない。このTaskの追加Gameplay修正・commit・pushは行わない。
