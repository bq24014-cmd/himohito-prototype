# Permitted Git checkpoint scope

The owner explicitly approved integration and authorised commit/push after regression.
Commit: Integrate approved S3 Return Step A pencil visual

Stage only the new MainS3Pencil Runtime Source/Shader and their .meta files, plus
this new task's adoption/QA documents and relevant evidence.

Relevant Evidence includes final Run02, retained Run01 failure/log, protected-state
snapshots, exact tested-byte/GUID/feature audit, visual identity measurements,
reproducible QA observer/entry and the approved Source/Shader reference under Docs.
None of those diagnostic sources is a Production asset.

Evidence/ProductionCandidate is a local construction mirror and is excluded.
It is not needed as a second committed copy of the Runtime files.

Do not stage old MainStage Scene / README / learning log / ExeA/ExeB logs,
the existing untracked integration capture, the earlier Proof folder, other old QA,
output, Builds, .codex_tmp or the BGM binary.

No permanent safe.directory, ownership, ACL, authentication, reset, clean, stash,
pull or unrelated cleanup is authorised. Use only per-command repo-scoped -c safe.directory.

Before commit, audit cached name/stat/status and run cached diff --check.
The full check reports trailing whitespace only in the two raw UnityRun01/02.log
files (Unity-generated connection/stack-trace lines). Preserve these logs as
evidence rather than silently rewriting them. The scoped check excluding exactly
those two logs must pass for all Production files, code and other QA records.
Before push, fetch origin again and require origin/main still equals the start
fdc7fd39b6ec2b5a9131b1ad2fc53f0adf370f7e.

After push, fetch and require local HEAD equals origin/main. Final protection
measurement is written to ignored .codex_tmp rather than modifying staged evidence.
