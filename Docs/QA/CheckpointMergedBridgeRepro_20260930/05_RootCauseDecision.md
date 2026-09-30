# Root cause decision

## 分類

- MainStage: **CASE R1**（隔離コピーの制御Runtime再現）。既存checkpoint10にmerged bridgeが保存され、同じ1本橋を復元しながら中央Hookが復活した。
- Tutorial: **CASE R3 / NOT REPRODUCIBLE WITH AUTHORED TUTORIAL FLOW**。後続checkpointがない。入口状態へ戻る結果をR1と誤判定しない。
- R2ではない: MainStageの橋数1、両端、長さ12が保存/復元で一致。
- R4ではない: MainStageで中央のactiveSelfとactiveInHierarchyの誤復活を実測。

前半を省略した準備、メソッド呼び出し/入力注入による再現条件は02/03に開示。全区間の手動プレイ完了を主張しない。

## Confirmed Cause

checkpoint snapshotが橋のStart/End/RopeLengthのみを保存し、**そのcheckpoint時点で解除されていた中央Hook identity/状態を保存していない**。

MainStage seq19で橋1の保存を確認した一方、中央inactive状態はsnapshotの保存対象ではない。restart中のRestorePlatformStatesがClearPlatformsを呼び、RestoreRemovedHooksが現在のremovedHooksをSetActive(true)で初期状態へ戻してリストを消す（seq28→29）。その後保存配列から橋だけが再生成される（seq30→31）。解除Hookをsnapshotに基づいて再適用する情報・処理がないため、merged bridgeと中央active Hookが同時存在する。

橋の端点や長さの消失、rope残量の変化、player teleportが今回のPrimary causeではない。38/選択6はこの再現で保存/復元一致。ただし値を事前に変えて復元を試す独立回帰は未実行。

## Bindingは別

ReleaseCenterはmerge直後2本の一時releaseを作り、0.4秒で終わる。今回のcheckpoint時点では既に0。復元では外端wrap2を再生成し、終了したreleaseを再演しない。履歴の保存はないが、今回の実測では別の永続binding不整合を確認していない。中央Hook復活をbindingアニメーションの欠落と混同しない。

## 最小修正候補（文章のみ・未実装）

**checkpoint snapshotに「解除済みHookの識別情報」のコピーを橋配列と一緒に保持し、橋復元後にそのHookをinactiveへ再適用してremovedHooks管理へ戻す。**

橋の幾何保存や物理モデルを変えず、入口snapshot（解除なし）と統合後snapshot（解除あり）を区別できる候補。単にRestoreRemovedHooksを削除する案では、統合前checkpointへのrestartでHookが復活しなくなるため不十分。

実装時には同一Scene内の安定したidentityと、snapshot配列の独立コピー、Scene終了時の無効参照を検討する。この設計・コードは今回は作らない。

Runtime再現・原因判断完了につきSTOP。バグ修正なし。
