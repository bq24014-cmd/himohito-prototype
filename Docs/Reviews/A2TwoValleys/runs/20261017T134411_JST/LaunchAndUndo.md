# 起動・終了・元へ戻す

固定ルート：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\HimoHitoReviewRuntime\
```

1. `START_Review.cmd` をダブルクリック。現在の腕修正版。
2. 終了は右上の×またはAlt+F4。
3. もう一度同じ `START_Review.cmd` を起動し、Windowsの通信許可画面が出ないか本人が確認する。
4. 比較は一方を閉じてから `COMPARE_PreviousArms.cmd`。同じ `Windows\HimoHitoReview.exe` が、前回の腕を表示する。

固定exeを直接ダブルクリックしても修正版が既定で手動起動する。自動撮影フラグは付けない。既存キー、通常時間、入口24／6／0。音は通常ランチャーで元どおり。

## 元の環境へ戻す

固定版を終了し、従来のルート内

```text
PlayerIntegratedArmsProof_20261017\START_ArmsRevision.cmd
```

を使う。元の `Build_r3\IntegratedArmsProof.exe` と付属ファイルはbyte不変で保持。今回はWindows規則を変更していないため、規則の復元作業は不要。固定版を消す必要もない。

## 通信許可画面が出た場合（本人操作）

まず対象が固定版の **`HimoHitoReviewRuntime\Windows\HimoHitoReview.exe`** か確認する。古い `Build_r1/r2/r3`、別のアプリ、Unity Editorの許可と混同しない。新固定版では受信通信が観測されていないため、原因を確認せず広い許可を追加しない。

必要性を確認したうえで設定する場合は、Windowsセキュリティ→「ファイアウォールとネットワーク保護」→「ファイアウォールによるアプリケーションの許可」→「設定の変更」。正規の管理者確認を本人が行い、対象の完全一致EXEだけを選ぶ。現在の分類はPublicだが、Private／Publicを一括で許可したり、ネットワーク分類自体を変更したりしない。通信の必要性・用途・接続先に合う範囲を決めてから行う。

同じ表示名の項目が複数あるので、必ず詳細のパスを見る。対象パスに明示Blockがある場合、Allowだけを足しても解決しない。[優先順位](https://learn.microsoft.com/en-us/windows/security/operating-system-security/network-security/windows-firewall/rules)。今回監査時には新固定版の規則0、旧r3のBlock0だった。

本人が後日追加した固定版の例外を戻す場合も、同じ設定画面でその固定EXEのチェックだけを解除する。他のProofやアプリを変更しない。[Microsoft公式の追加・解除手順](https://support.microsoft.com/en-us/windows/security/firewall/risks-of-allowing-apps-through-windows-firewall)。Codexによるセキュリティ画面の代理操作・設定変更はしない。

## 今後の更新

レビュー用の固定パス・EXE名を維持する。更新はHumanが指定したProofだけを対象に、ゲームが終了していることと元Proof・現在の固定版の保存を確認してから行う。Unity付属ファイル一式で更新し、EXEだけを改名・差し替えない。今回の方針を将来の未承認候補の採用許可と解釈しない。
