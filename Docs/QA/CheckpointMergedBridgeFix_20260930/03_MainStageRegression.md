# MainStage regression

Unity Editor 6000.3.21f1、修正後Sourceをコピーした隔離Projectで実行。Production MainStage.unityは開始時の未Commit版をそのままコピーし、編集・保存していない。

## A/E: merged restore / R x3 — PASS

既存checkpoint9床へ診断準備位置移動し、物理着地で区間9を捕捉。長さ6でE/Q、歩行、もう1本E/Q、Fで統合。その後は通常物理の歩行でcheckpoint10を捕捉。

保存時とR各回の結果:

|項目|保存時|R1|R2|R3|
|---|---|---|---|---|
|中央Hook activeSelf/activeInHierarchy|false/false|false/false|false/false|false/false|
|中央Hook InstanceID|20962|20962|20962|20962|
|Generated platform count|1|1|1|1|
|removedHooks count|1|1|1|1|
|端点|(180,-2.15) → (191,-2.15)|同一|同一|同一|
|Platform RopeLength|12|12|12|12|
|CurrentLength|38|38|38|38|
|SelectedRopeLength|6|6|6|6|
|CurrentSection|10|10|10|10|

R前にCurrentLengthを23/22/21、選択長を4へ意図的に変更し、保存値38/6へ戻ることを検証。Hook参照は失効せず、保存参照と同一。解除リスト/橋の増殖なし。他の全authored Hookの参照・active状態も保存時と一致。

根拠: Evidence/Merged/assertions.txt、state.jsonl、result.txt、04_after_restore.png、Merged_runtime.log。

## B: unmerged restore — PASS

長さ6の橋を2本生成、Fは実行しない。未統合経路は上の梁に阻まれるため、診断で既存checkpoint10床上へ位置移動し、物理着地で保存。checkpointやsnapshotの直接生成は行わない。このケースは通常経路の到達性の検証ではなく、未統合状態の保存/復元検証。

R後も2本のStart/End/RopeLengthが保存配列と一致。中央Hookと他の全authored Hookは参照・active状態が不変、removedHooks=0。CurrentLength=38、選択長=6、区間10が復元された。

根拠: Evidence/Unmerged/assertions.txt、state.jsonl、result.txt、Unmerged_runtime.log。

## C/D: rope / normal Hook — PASS

Fも橋生成もしていない既存checkpoint9。保存値はCurrentLength=50、選択長7、生成橋0。診断で31/3に変更後、Rで50/7へ復元。全authored Hookの参照・active状態が不変、removedHooks=0、橋0。

根拠: Evidence/Normal/assertions.txt、state.jsonl、result.txt、Normal_runtime.log。

## テスト範囲

同一Scene・同一Play session内のRuntime checkpointに限定。Sceneを跨ぐ永続save、新しいScene、配布build、キーボードの手動入力テストは今回の対象外。
