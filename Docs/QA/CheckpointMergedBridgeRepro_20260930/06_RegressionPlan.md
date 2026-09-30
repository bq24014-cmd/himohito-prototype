# Regression candidates — 未実行

今回のRuntime再現をもとに、将来の修正後に実施する候補。今回ここに挙げた独立回帰は実行していない。

| ID | ケース | 合格条件 |
|---|---|---|
| A | 未統合2本のcheckpoint復元 | 個別両端/長さ、橋2本、中央activeが保存時と一致 |
| B | 統合1本のcheckpoint復元 | 合計長さ/両端一致、中央inactive、removedHooksに正しいidentity |
| C | Hook未解除checkpointへの巻き戻し | 保存後にFで解除しても、Rで保存時のactive Hook/未統合橋に戻る |
| D | rope残量を保存後に消費→R | checkpoint残量と補充条件を正確に復元、二重消費/返金なし |
| E | selected lengthを保存後変更→R | 保存時の選択へ復元（残量クランプも確認） |
| F | repeated R | 3回以上の再開でHook/橋重複、リスト喪失、参照破損なし |
| G | Tutorial通常ルート | T4入口保存→統合→Rで入口の橋/Hook状態へ戻る。存在しない後続checkpointを仮設しない |
| H | MainStage通常ルート | 区間1〜9の通常プレイから既存checkpoint10保存→R/落下の両経路でB成立 |

追加候補: 6+7など長さが異なる2本の統合、別中央Hookを複数解除したsnapshot、checkpoint後に新規解除して巻き戻すケース、release演出中/完了後の再開、Scene移動と再入場。

推奨順序: B → C → A → D/E → F → G/H。Production変更は別Taskで承認後に行う。
