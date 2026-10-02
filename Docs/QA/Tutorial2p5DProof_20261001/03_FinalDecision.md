# Final Decision — P1

2026-10-01。対象: Tutorial第1区間のみ。Production未採用。

## 判定

**P1: 2.5D proofとして成立。今後拡張する価値が高い。**

Review前はP2としたが、その後ユーザーから「奥行きが分かり、見やすい」と回答を得たためP1へ更新した。個別操作や全区間回帰までHuman PASSと拡大解釈しない。詳しい回答と確認範囲は `05_HumanReviewResult.md`。

独立した3つのSprite平面を異なるZと追従率で動かし、実際のUnity描画で窓外 / 部屋 / 手前おもちゃの相対移動差を確認した。追加の背景コンポーネントは描画対象だけを扱い、Gameplayカメラ・Collider・Rigidbody・Joint・入力仕様を変更しない。

## 最終診断

採用する結果は `Evidence/RuntimeAttempt02/result.txt`。30項目PASS、Gameplay Runtime Error 0。

- Stage Selection状態、Tutorial開始、第1区間3層表示。
- 既存PlayerMoverで右移動・ジャンプ・着地。
- 既存RopeControllerの照準 / 接続、DistanceJointの維持。
- A/D相当の入力キャッシュで振り子を駆動し、実Cameraの移動と3層位置を30フレーム記録。
- 同フレームの5組のBefore / AfterでPhysics・Camera状態一致。
- 第2区間のガードで元背景復帰。MainStageにProofコンポーネントなし。

**これはcached-input / public-methodによるEditor診断であり、OS実キーボード / Human PlaythroughのPASSではない。** 区間2への切替はcontrolled scope testであり、通常操作での到達テストではない。

Unity Editor起動時のSearchDatabase初期インデックス例外1件を別記録した。Gameplay例外として隠して除外したものではなく、`editor_search_errors.txt` に保存。最初の起動ではライセンス接続に失敗し、隔離Unityだけを再起動した。

## 見た目

|観点|現時点の評価|
|---|---|
|ヒモヒト|中央の紺フェルトに対してピンクが読みやすい。大きさ・描画順は元のまま。|
|青Hook / 照準 / ヒモ|中央に装飾を増やさず、フック・ピンクのヒモを識別できる。|
|足場|元の木材デザイン・位置を維持。背景床や玩具とは高さ・形で区別できる。実操作での読みやすさはHuman確認が必要。|
|世界観|毛糸・布・木材・本の既存方向性を維持。月は元より大きく、ガーランドの見える範囲は小さくなった。|
|奥行き|窓外はほぼ留まり、棚・壁、手前のおもちゃの順に相対変位が増える。単なる合成1枚絵ではない。|
|対象範囲|Tutorial Playing / 第1区間だけ。メニュー・見渡し中・他区間・MainStageは元背景。|

## パフォーマンス

1600×900のEditor Camera.RenderのCPU呼び出し時間、ウォームアップ10回後・各15サンプルの中央値:

- Before: 2.7195 ms
- After: 3.0016 ms
- 差: +0.2821 ms（約+10.4%）。大幅なCPU増加はこの短い測定では見られないが、**FPS維持やGPU負荷の保証ではない**。

Proof画像3枚のProfiler上のTextureメモリ: 47,695,488 bytes（約45.5 MiB）。旧背景画像も比較用に保持されるため、これは追加Texture分である。圧縮なしの試作設定であり、本採用には圧縮・読み書き設定・ロード/解放の検討が必要。

計測条件と生CSVは `EvidenceManifest.json` / `render_timing.csv` に記録。Standalone FPS、低スペックPC、複数アスペクト比は未計測。

## 残る課題

1. 有限キャンバスのため、長いCamera移動ではNearの相対移動がcoverage clampに達する。Section 1終端・横長画面で確認し、必要なら背景画像の横拡張を検討。
2. 第2区間では即座に元背景へ戻す。今回のscope保護として正しいが、本採用時は境界の見た目を別途検討。
3. 静止画像の光・陰を利用した平面レイヤー方式であり、3D立体モデル / 動的な遮蔽 / 動的ライティングではない。
4. Humanは奥行き・見やすさを肯定。個別操作感、長時間の酔い、区間終端・画面比率は独立した追試が必要。
5. Gameplayへの変更は禁止されているため、背景に合わせたプレイヤーや足場の調整は行っていない。

## 次の判断

専用Windows ProofのHuman評価を記録済み。Baseline比較の詳細、本採用・他Section展開の許可は今回に含まれない。

commit / push: **未実施**。QA資料は保存候補にできる状態だが、Production Assets採用やcheckpointは別途承認が必要。ここで停止する。
