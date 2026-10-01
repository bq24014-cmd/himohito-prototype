# Submission build provenance

- The release build succeeded before the later Play-mode QA scripts were copied in.
- The final package is `Builds/Submission/1230460_TH_作品提出.zip` in the original project.
- The 151 ZIP payloads are SHA-256 verified against the staging folder; runtime files match the successful build.
- The source snapshot contains 462 files, and the original project files remained unchanged during packaging.
- Launcher smoke test: 145 runtime files copied to the dedicated ASCII path matched the package; the headless player loaded without logged game errors. The agent-started process was stopped.
- ClearReturnFlowCheck: 49 assertions and 5 scene loads passed in an isolated Editor Play session. Runtime sources match the original project (113 scripts).
- This project's Editor scripts/settings now include isolated QA guards and a test PlayerPrefs namespace. Do not rebuild this QA project for distribution; create a fresh snapshot from the original project instead.
- Windows Computer Use timed out after its documented retries. Visual gameplay/audio and a separate Windows PC playthrough were not verified in this task.
- Original source revision at snapshot: f0b0709b446a7f4372f2223aad38acaebaf8c6af, plus the existing MainStage scene changes.
- Final ZIP SHA-256: 4BE7B66DED1C645943E19D8E1FEC57B769E595939EA18BA854E843F766F1536D
