# Final Decision — Tutorial全区間 2.5D trial

2026-10-01 / **T1** / Human Review APPROVED / Production未適用。

## 根拠と評価

| 評価ポイント | 確認結果 |
|---|---|
| Tutorial全体の統一感 | 同じ素材/色/連続した基準座標を保持。Human「自然で見やすい」 |
| 第1区間同等の奥行き | 承認済みFar/Mid/Near追従率とR1を継承。後半でもclampで止まらない |
| Section境界で飛ばない | 実進行3回で背景変位=Follow×Camera変位、全PASS。Sectionで画像を交換しない |
| 主人公/Hook/床/Rope | 全区間の実画像を目視確認。Human視認性承認。看板/HUDはCaptureに含まれず項目別Human未確定 |
| 小物の接地 | R1影/反射/床帯を維持。toy足元は床帯となじむ。足場下端の接地は完全解決ではない |
| 豪華さが攻略を邪魔しない | 中央は暗い布壁、Gameplay layerを変更せず背景が後ろに残る。121診断チェックPASS |
| 全体展開の価値 | Human「全体展開してよい」。後半境界の残課題を含め、proof採用価値T1 |

## 判定の経緯

最初の目視評価はT2: 第4区間ゴール右端にmirror延長の境界/反転棚の断片があるため。ユーザーへこの点を明示し、全4区間の奥行き/自然さ/視認性についてHuman Reviewを依頼。

回答「自然で見やすい。全体展開してよい」を受領したため、Tutorial全体の方式・採用価値はT1とする。境界が消えた、panoramaが完成した、全チェック項目が実キーボードで個別PASSした、とは書かない。

## 技術・Build

- Runtime02: 121 checks、0 FAIL、0 runtime errors、3 physical section transitions、31 sampled motion frames。
- Runtime01の診断撮影負荷によるovershootは証拠を保存。ゲーム本体修正ではなく検証記録時計/入力ハーネスを調整。
- Editor Search起動例外1件を別記録。C# compile/Runtime gameplay errorは0。
- Windows build Succeeded、exe起動、Player log/プロセス応答確認。自動取得ではMainWindowHandle=0のため、画面を独立に撮影/操作できたとは扱わない。表示/見やすさの根拠はHuman回答。
- Gameplay Source/Editor setup source/Packagesの隔離コピー差分=0。Tutorial/MainStage Sceneの隔離Build後SHAもProductionの開始SHAと一致。
- 設定変更は隔離コピーのcompanyName/productNameだけ。BGMはコピーのみ。

## 残課題と停止境界

後半の端延長、広いaspect、低性能PCの負荷、Profiler、IMGUI込みの個別操作回帰は正式採用時に必要。床影は視覚補助で、Gameplay床やColliderに合わせた立体geometryではない。

今回は新QA・隔離trial・専用Buildのみ。MainStage展開、Production適用、新Gameplay、stage/commit/pushは行わない。Human Reviewで停止する。

終了時保護はPASS: 3495既存ファイルが開始SHAと一致、Production追加0、index空、mainのHEAD / origin/main=`702a4bc044b0eb35bedb552753ca8495599bc1ee`のまま。既存未Commit MainStage Scene・README・LEARNING_LOGおよび旧Build/QA/BGMを維持した。
