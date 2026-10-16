# Human Findings

ユーザーのHuman Playtestで Tutorial、MainStage Section 5 → 6、Section 9到達、Clear到達が成立しました。前回自動回帰で未確定だった「人間が5→6を突破できるか」は、成立として扱います。

| ID | 人間が報告した問題 | 今回の確認範囲 |
| --- | --- | --- |
| H1 | 白系のゲーム内看板に明るい文字が重なり読みにくい | TutorialSignInscription のOperation KeyとCream図形。全画面本文は対象外 |
| H2 | Clear画面でA/Dを押すと足音が鳴る | Tutorial/MainStageのphysics停止中の入力・移動・jump・footstep経路 |
| H3 | 本編第9区間の2本目を掛けにくい | 左橋生成後、中央付近から右フックへのRaycast順・選択・highlight・角度幅 |
| S2 | 看板3は消費7なのに99→93と表記 | 現行Resource仕様と実際のQ生成で確認し99→92へ限定修正 |

5→6の成立を覆す難易度変更や新しい移動補助は今回行いません。実キー追試と自動診断は混同しません。
