# 外部BGM承認後の最終判定

**MainStage Scene Cleanup = FAIL（検証BuildコピーのScene差分0が未成立）。Commit / Push禁止を維持。**

Build / exe / Human開始確認 / controlled smokeは成立したが、全条件PASSではない。
`PASS: clean HEAD + documented external BGM dependency` を最終成立結果としてはまだ宣言しない。

1. **BGM original path:** `C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype\Assets\Resources\Audio\YasashiiOdori.mp3`
2. **BGM SHA256:** `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。1,786,604 bytes。Git管理外、.gitignore:42で明示除外。原本変更なし。
3. **copied SHA一致:** BuildProject / QAProjectとも原本SHA一致。必要なResources位置に配置、音源実体はstageなし。
4. **Source diff=0:** Productionと両検証コピーのcommitted Sourceで確認。診断専用追加ファイルはQAProjectのみ。
5. **Scene diff:** Production=0 / QAProject=0。ただしBuildProjectのMainStage SceneはBuild中に自動保存され、697追加・117削除。検証コピーも含めたScene差分0条件はFAIL。
6. **Windows Build:** SUCCESS。新規output `output/MainStageCleanReproduction_ExternalBGM_20261001_151833/Game/`。過去Build上書きなし。
7. **exe起動:** process / Player初期化ログで起動を確認。Stage Selection→Tutorial→戻る→MainStage開始はHumanの「完璧」回答によりPASS。
8. **smoke:** Full148+Focused32 assertion PASS。看板、Clear足音ガード、S5遮光、S9照準 / merge / 3回Restore、S10 Clear、看板3 99→92、BGM clip playback状態。実キーボード通し試験ではない。Editor SearchDatabase例外各1件は記録・分類済み。
9. **最終再現条件:** HEAD `4919233cd1d0815afb438bd35a7336492ec09548` + Unity6000.3.21f1 + documented external BGM at `Assets/Resources/Audio/YasashiiOdori.mp3` + 既存BuilderのScene同期生成経路 + ASCII exe起動パス。HEADだけの完全再現ではない。Scene同期後差分の等価性は未確認。
10. **Cleanup:** FAIL / STOP。Production整理状態とArchiveは保護されているが、strict verification gateは未達。
11. **commit/pushしてよい状態:** いいえ。index空、HEAD / origin/main不変。今回資料は保存候補に留める。
12. **新機能検討へ進める状態:** Cleanup完了条件を先に解決する必要あり。Gameplayの問題を新規発見したという意味ではない。

## 次に必要な判断

Productionへの追加変更は行わず停止。
Build時の既存Scene同期を正規の生成工程として扱い、生成差分のRuntime / Visual等価性を別監査するか、Scene入力をbyte-for-byte固定する別Build手順を検証するか、ユーザーの方向指定が必要。

今回のHuman開始確認は成立したため、UI補助機能停止は未解決の開始確認ブロッカーではない。

詳細: [Build / Smoke記録](06_ExternalBuildAndSmoke.md)、[BGM依存](05_ExternalBGMDependency.md)、[clone配置手順](../../Dependencies/ExternalAudio_20261001.md)。
