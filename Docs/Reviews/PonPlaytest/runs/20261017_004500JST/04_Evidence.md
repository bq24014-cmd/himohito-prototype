# 対象版・証拠索引・未確認事項

## 今回の実測

00:34〜00:37 JST頃、computer-useのSKILL/guidance/api/confirmationsを読み、利用可能な`cua_repl`で画面状態取得を試した。初回とreset後の短い再確認が同じエラーで終了。

```text
MXC launcher: cannot locate required Windows platform directory: HRESULT 0x80070003
node_repl kernel exited unexpectedly
```

ゲーム画面は0枚、通常キー/マウス入力0回。音声取得/聴取なし。画面の交代確認も未成立であり、候補2の作業に入力を割り込ませていない。独自helper起動、別の入力経路、内部API、テレポートによる代用は行っていない。

詳細：[connection-result.json](evidence/connection-result.json)。このエラーはゲームのクラッシュを意味しない。操作環境の未成立として扱う。

## 対象Build候補の実体

Production起点：`C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype`

相対パッケージ：`output/FriendPlay_20261006_214500/HimoHito_20261006/`

実行ファイル：`Game/HimoHitoPrototype.exe`

VERSION記録：2026-10-06 / Windows x64 / Unity 6000.3.21f1 / 基点`44f81fe2a27da2661d2607c35e1cd84c657f95f6`。当時のtracked working treeの隔離コピーで、既存未コミットMainStageを含む。診断observer/Proof切替を追加しない配布版という記録。

今回、exeとゲームAssemblyの実体を読み取りハッシュで照合した。

| ファイル | SHA-256 | 結果 |
|---|---|---|
| exe | `837986201d89be25426b8415cb3bc945f077e5b13f4722d6763d04ebd38c5472` | 既存PlayerManifest一致 |
| Assembly-CSharp.dll | `9587c9e485b2dcf0bb6538b4d58740b5b0e6d3ba40092a5c7785d48bc5095bc5` | 既存PlayerManifest/FinalVerification一致 |

固定ソース：同runの`.build/Project/`。29件の使用ソース/Scene/設定をBuildInputsへ照合し全件一致。書き換わり得るProductionのAssetsを読んで10/6固定Buildの仕様と推測する方法を避けた。

詳細：[build-identity.json](evidence/build-identity.json)、[source-hashes.json](evidence/source-hashes.json)、[行番号付き抜粋](evidence/source-excerpts.md)。抜粋も静的根拠であり、実行結果ではない。

## 版と承認の境界

- ビルド対象にTutorialとMainStageの2Sceneがあることを設定とmanifestで確認した。全区間を今回通したわけではない。
- 旧HumanMenuSmokeは「選択画面→Tutorial開始→ポーズから選択へ戻る→Main開始」に対する「正常にできた」という記録。`human_full_route=NOT_PERFORMED`。メニューの承認を作品全体/全キーの承認へ拡張しない。
- 今回読み取ったremote mainは`7a4f011b7ffc0343acd4727a75255495a3881fb3`、2026-10-06のStep B統合。対象Build基点44f81feより後なので、対象版を最新版とは言わない。
- 友人向けREADMEも本編番号1〜7・9・10と、敵・大砲が未実装の版であることを記す。大砲/ネズミProofや後の振り子候補は混ぜない。
- 以前のHuman試遊で看板可読性・S9照準・Clear後足音・99→92を修正承認した記録がある。再現なしで再オープンしない。

旧記録の保存コピー：[VERSION](evidence/records/VERSION_20261006.txt)、[Build検証](evidence/records/BuildFinalVerification.json)、[Humanメニュースモーク](evidence/records/HumanMenuSmoke_20261006.json)、[Human指摘](evidence/records/HumanFindings_20260930.md)、[追試承認](evidence/records/HumanRetest_20261001.md)。過去日時の記録は書き換えていない。

## 画像索引

画像は全10枚（当初E01～E08と追補E09/E10）。全て既存画像の同一バイトコピー。新規実プレイ画像ではない。SHA-256、元のProduction相対パス、サイズ、限界は[image-index.json](evidence/image-index.json)へ保存した。GitHubへ保存する時も通常PNGのまま置く。期限付きURLやZIPだけにしない。

| ID | ファイル | 使える根拠 / 使えない根拠 |
|---|---|---|
| E01 | [メニュー](evidence/images/E01_menu.png) | 文言/選択の見た目の旧参考。対象Buildの実フォーカス/クリックの証拠ではない |
| E02 | [Tutorial HUD](evidence/images/E02_tutorial_hud.png) | 旧通常画角の構成。最初の3分の記録ではない |
| E03 | [ヘルプ](evidence/images/E03_help_fixture.png) | 9/30静的Draw fixture。固定ソースの同じ文言と照合。OS入力の証拠ではない |
| E04 | [ポーズ](evidence/images/E04_pause_fixture.png) | 9/30静的fixture。実スクロールや音量の聴感は未確認 |
| E05 | [失敗](evidence/images/E05_failure_fixture.png) | 9/30静的fixture。復帰時間/クリック試行の証拠ではない |
| E06 | [T4看板](evidence/images/E06_sign4_fixture.png) | 9/30静的fixtureの1コマ。図のfade-in時点で見えないものを欠落バグとしない |
| E07 | [T4統合後](evidence/images/E07_merged_diagnostic.png) | 9/30診断ハーネス。通路の形の参考。通常キー攻略/人間の感情の証明ではない |
| E08 | [Clear](evidence/images/E08_clear_reference.png) | 旧UI確認状態。表示50/橋0などを今回の通過結果にしない |

音付きの短いプレイ映像は取得していない。静止画をつないで通常入力の動画に見せかけることも行っていない。今回の判断に必要な既存資料はPNGとテキストで読めるようにした。

## 未確認と、解消に必要な確認

| 未確認 | 必要な確認 |
|---|---|
| 最初の3分/最初の成功/読む負担 | 交代が確認できた操作環境、またはHumanの通常入力録画 |
| 振り子の気持ちよさ/解除時の読みやすさ | 同じ通常画角で、長さ・加速・解除を変える入力比較 |
| 橋と統合の意味の理解 | 攻略を先に教えず、Q/F前の結果予測と理由をHumanに説明してもらう |
| 再挑戦テンポ | 失敗確定→入力可能→再接続までの時刻と試行記録 |
| 弱い反復/S10の余韻 | Tutorial済み/直Mainの違いと終盤を通常入力で観察 |
| 本編UIの実Window、focus、hover、クリック | 対象版で各画面を操作。静的fixtureだけでは確認不可 |
| 音 | 実音声を取得して聴く。イベントログは代用不可 |
| 振り子候補2 | 担当と画面利用終了/交代の確認、完成したProofへの別レビュー。現時点では効果未推定 |

## 保存範囲と競合回避

今回の作業領域はProductionとは別の`2026-10-17/task/pon-review-draft`。Productionでgit操作をしていない。レビュー対象ファイルだけが`Docs/Reviews/PonPlaytest/`以下にある。

remoteは読み取りでrepository権限とmainのSHA、専用branch不存在を確認済み。保存直前には再確認が必要。親が本文と対象一覧をレビューするまではcommit/pushしない。mainへ統合しない。共有commit/URLはremoteを読み戻せた時だけ記載する。

親への中間連絡は指定source thread宛てに試したが、Codexアプリの送信ツールが`thread not found`を返した。連絡成功とは扱わない。親レビュー用の資料をこのチェックポイントで提示し、採否を代行しない。


## Humanの環境承認後の再確認

Humanが00:57 JSTに環境を承認したとの親連絡を受け、新しい条件として接続を最小再確認した。初回・短い再確認とも `trusted Node process exited unexpectedly; kernel reset, rerun your request`。以前のHRESULTエラーとは変化したが、画面取得前に終了し、承認反映状態は観測できない。画面/通常入力/到達/音は引き続き0。候補2との交代も未確認のため入力なし。これ以上再試行しない。


資源/復帰追補：[本文](07_Resource_Retry_Addendum.md)、[条件付き状態追跡JSON](evidence/resource-retry-state-trace.json)、[実ソース抜粋](evidence/resource-retry-source-excerpts.md)、[15入力のハッシュ照合](evidence/resource-retry-source-hashes.json)。E09/E10は[06追補](06_T4_S10_Addendum.md)と[t4-s10-image-index.json](evidence/t4-s10-image-index.json)を参照。追加した画像も旧診断記録で、Aは0。
