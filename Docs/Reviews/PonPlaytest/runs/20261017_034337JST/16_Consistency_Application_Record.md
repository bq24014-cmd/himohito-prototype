# 主レビュー4文書の訂正適用記録

**A=0、レビュー文書だけの適用。** 親の独立レビュー承認に基づき、未適用提案22編集をREADME/01/02/03へ適用した。ゲーム/UI本体・実入力・Unity・Production・mainは対象外。08:00の最終終了報告ではない。

- 適用前commit：`a31545d974ed4f5a1578b7c9a61698e4236f794e`。元4ファイルのbyte hashが提案基準へ一致した後に適用した。
- 提案run `20261017_033401JST` の5資料は未適用時点の履歴として同一byteを保存。提案内の「未適用/親未レビュー」は当時の状態であり、この適用記録と区別する。
- 元4文書は既存のCRLF/LFを保持。eol属性は変更せず、改行変換だけの全文差分を避けるため既存CRLF文書と初期manifestはraw blobとしてstageする。新規資料はLF。
- 対象はS1共通導入KEEP、S7 CP9/F解放、S10全体REMOVE撤回、UI条件/例/復帰、未実施比較としての上位5案/朝1件の整合。既に整合するT3/T4説明などは新しい不具合に数えない。
- S5解析は未レビューで別領域。今回のcommitへ含めない。

| 文書 | 適用前SHA-256 | 適用後SHA-256 | 改行 |
|---|---|---|---|
| README.md | `30af907e98ce17c56a292bd67ffca223af0caa6da95fdb0d7309451bdacda3a8` | `b78150fc8265f136e87985612feb966a0e71f3ef87569aa3fa9126bb8a3b678d` | CRLF |
| 01_Gameplay.md | `56672c28768f5e1ca47ee39a901bda774795702014a3bd2664b6f2b85a38de89` | `200b32e0d541bd1247e5b6231938171cd13ef9a0f94489fbf2f75306a5d28841` | CRLF |
| 02_UI.md | `6d1606a353d9a0b91c9040dd44f7ad82ec6338c046dd3cd07d3dd5ed8ee7e5f4` | `0d7f87feac2f401a3ef625516a77b906e41e9ecc3f9346aacdcf8487b1b584fb` | CRLF |
| 03_Priorities.md | `8097ef72b90150bca691fdad366b958aed26fd199fcaf55c01a829f5a0aeb7f3` | `99a51deba5599eb334b086f303d5dfefe9dc9979096ddfa70a913f9eacb3e7c1` | CRLF |

本文/原文の照合内容は[訂正提案](../20261017_033401JST/15_Consistency_Correction_Proposal.md)、適用対象/元hashは同提案のledger、旧根拠は適用前commitで追跡できる。公開成功はpush後のremote照合報告で判断する。
