# H3 — Section 9 Aim Diagnostic

## 再現と測定

本編の実checkpoint9に上面着地し、左→中央をE/Q・長さ6で生成。生成橋をDで歩いて到達した3位置で、keyboardAimDirectionを-100〜+80度、0.25度刻み（721点/位置）で走査しました。右が0度、下方向は負角です。

CSV `angles_left_of_center.csv / angles_near_center.csv / angles_at_center.csv` に角度、Player位置、SelectedRopeLength、右との距離、RaycastAllのcollider/hit距離/Hook順、resolved、available、highlight、F用currentAimを記録。AFTERはBEFOREで実際に歩いた座標を再現して、フレーム時間による位置差を排除しています（同一幾何比較用のQA位置設定）。

## 原因

**Aが確認された主原因**：near_centerでは右フックに実際に当たる27角度のうち11角度で、中央Hookの1.6×0.45 colliderが先に当たり、既存左橋へ戻る中央Hookを選択していました。例：-18度はPlayer→Center→Rightの順。highlightもCenterとなり、Eの接続先もCenterになります。

**Cは条件として存在**：left_of_centerの右アンカー距離6.53685は長さ6の許容6.51（grace込み）より長いので、正当に接続できません。この位置は修正後も0度幅です。もう少し右へ歩く/長さを選び直す必要があり、距離制限を緩めていません。

**B**：右の実colliderによる幾何的受付幅はnear_centerで6.5度。修正後はこの全範囲を利用できます。colliderを拡大する根拠はありません。

**D**：照準速度120度/秒は変更せず。実キーによる微調整の主観評価は未実施で、速度が主原因と断定しません。**E**：ビーム/床など非Hookのhitは現行仕様どおり無視。別の新規障害は検出していません。

## 最小修正と角度幅

TryResolveAttachmentPointで、対になる点との橋が既にある候補をfallbackにし、同じray上の有効・到達可能な未生成接続先を先に選択します。他の候補がなければ元のHookを選択できます。一般の「既存橋が新しい接続先を隠す」競合を解くロジックで、Section9専用条件は増やしていません。

| 実測位置（x,y）/長さ6 | BEFORE 右選択角 | AFTER 右選択角 | 幅BEFORE→AFTER | 有効+highlightサンプル |
| --- | --- | --- | --- | --- |
| (184.67150,-0.51270) | なし | なし | 0→0度（距離外） | 0→0 |
| (184.90390,-0.40253) | -16.75〜-13 | -19.5〜-13 | **3.75→6.5度** | **16→27** |
| (185.06340,-0.32724) | -20.75〜-13.75 | 同左 | 7→7度 | 29→29 |

near_centerは幅約1.73倍、右に当たる角度の選択率16/27→27/27。中央が右を奪う有効角度は11→0。中央だけを狙った際のfallbackは維持。F用TryResolveCurrentAimHookは意図的に変更せず、両橋統合の中央選択を維持します。EのhighlightはTryResolveAvailableAttachmentと共通の解決処理なので実接続先と一致します。

中心選択範囲：near_centerの走査範囲内ではBEFORE -100〜-17度（333点）、AFTER -100〜-19.75度（322点）。走査全域の生データと各位置の中心範囲はEvidence/angular_summary.json。

地形・Hook collider・aimRotationSpeed・距離grace・移動物理・resource・F処理は変更なし。MainRegressionでfirst/second E/Q→F→実checkpoint10→R3回→長さ10橋→実宝箱Clearが成立。

判定：競合除去の数値回帰 **PASS**。Humanによる照準の手触り追試は未実施。
