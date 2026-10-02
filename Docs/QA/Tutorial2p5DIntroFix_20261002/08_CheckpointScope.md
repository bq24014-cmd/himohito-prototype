# Safety checkpoint — 2026-10-02

今回のユーザー承認は、採用済みTutorial 2.5D Production背景と採用判断資料のGitHub保存。Gameplay修正・再Build・cleanupは行わない。

## 採用根拠

- 第1区間Proof: P1。
- 前景調整: R1。
- Tutorial全区間trial: T1。
- Production Integration: PI2（イントロ表示条件の課題のみ残った）。
- Intro Fix: IF1 / Human Review APPROVED。「問題なし・採用してよい」。
- Intro Fixの既存回帰127項目、Windows Build、exe起動は既存資料でPASS。本checkpointで再実行した結果ではない。
- PI2の記録は過程として保持し、IF1の最終採用と混同しない。

## 保存範囲

Productionは既存背景入口1件、新しいTutorial背景Source/.meta、Edge/Floor Shader/.meta、Far/Mid/NearRefined PNG/.meta、画像フォルダ.meta。現在の採用済み内容を変更せず保存する。

5段階のQA文書、検証結果、比較PNG/GIF、検証手順を保存する。画像原案と前景調整原案も保存する。大量の生Capture PPMと試作用ImportedMetadataはGit対象外とし、ローカルに保持する。隔離Unity ProjectやWindows Buildを保存するという意味ではない。

容量監査は別commitへ分離する。容量監査は今回の背景変更ではなく、将来のcleanup判断の記録である。監査資料のGit保存は削除承認を意味しない。

保存・除外の具体的Pathと理由は `Evidence/Checkpoint/Selection.json`、`Evidence/Checkpoint/LocalOnly.json` に記録。Production内容のSHAと既存Build等の開始時状態は `Evidence/Checkpoint/ProtectionStart.json` に記録。

## 対象外・保護

- MainStage.unityの既存未commit差分。
- README.md / Docs/LEARNING_LOG.mdの既存Human Playtest差分（今回と無関係）。
- output / Builds / .codex_tmp / Unity cache / 旧Build。
- BGM実体 `Assets/Resources/Audio/YasashiiOdori.mp3`（既存Git除外方針）。
- 今回と無関係な旧QA、ArchiveExpansion、__pycache__。
- PPM等のローカル限定Evidenceは削除可に変更しない。

開始時branch/main、HEAD/origin/mainは `702a4bc044b0eb35bedb552753ca8495599bc1ee`。indexは空。push直前にremote再確認し、進んでいればpushしない。

Production/Scene/BGMのbyte保持とBuild等のファイル一覧・長さ・更新時刻を終了時にも照合する。既存Scene変更は含めず、Source/PNG/Shaderにも追加修正を行わない。
