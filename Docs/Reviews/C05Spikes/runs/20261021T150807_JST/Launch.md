# C05 手動比較の起動

エクスプローラーのアドレス欄へ次を貼り付ける。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C05SpikeProof_20261021\Manual
```

| ダブルクリックするファイル | 内容 |
|---|---|
| 01_TUTORIAL_CANDIDATE.cmd | 候補、Tutorial第2区間入口、3本 |
| 02_TUTORIAL_CURRENT.cmd | 現行、同じTutorial入口 |
| 03_MAIN_CANDIDATE.cmd | 候補、本編第2区間入口、通常と横長の2本 |
| 04_MAIN_CURRENT.cmd | 現行、同じ本編入口 |

一方ずつ起動する。どちらも1600×900。EXEだけを移動・改名せず、Runtime／BeforeRuntimeのUnity付属ファイルと、候補側のReviewAssetsのPNG2枚を保持する。起動時だけ外部fixtureが対象区間の入口とCheckpointを設定し、その後は既存操作で遊べる。自動入力・自動撮影・時計固定・自動終了は始まらない。

A／D移動・振り子加速、未接続でSpace、矢印で照準、未接続でW／Sを使って長さを選び、床上でE接続、接続後E解除。RはこのProofの入口へ戻る。Tabで既存の操作方法、Escでポーズ・復帰。終了はウィンドウ×／Alt+F4または既存のポーズからメニューへ戻り終了する。

Tutorialは残量99／選択長8、MainStageは残量50／選択長7で開始する。長さは変更してよい。正解攻略を要求する検証ではなく、通常画角のトゲの見え方と既存の接触解除を比較するための入口である。

候補は既存の非Development Player＋managed overlay＋実際の外部PNG読込で、新規Unity Buildではない。両Sceneの候補で通常起動の準備完了・4秒以上の待機・エラーなしをログ確認した。OS実キーの反応とHumanの実プレイは未確認。既存の腕・Q・Tab・汽車等の固定ランチャーは上書きしていない。
