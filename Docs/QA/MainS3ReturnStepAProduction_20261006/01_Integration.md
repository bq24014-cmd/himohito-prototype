# Integration and Human adoption

## Adopted decision

Human explicitly approved the S3 Return Step A jumbo unsharpened colour-pencil proof.
The owner confirmed its proportion/meaning, night-child-room fit, restrained detail
and clear walkable top. Remaining simple blue side is accepted without more changes.

No additional Human adoption or B/C/D rollout is inferred.
The approved reference remains in MainS3ReturnStepAVisualProof_20261006.
Its original documents/results are not edited or staged by this task.

## Production boundary

New files only:

- Assets/Scripts/MainS3PencilVisual.cs and its .meta
- Assets/Resources/MainS3Pencil.shader and its .meta

Runtime creates a Scene-scoped presentation owner only while MainStage is playing.
The existing Step A root remains position (42.8,-11.05,0), world size 1.8 x 10,
with walking top y=-6.05 and the original unit BoxCollider2D / static Rigidbody2D.
Only a render-only mesh child and material are added. Existing S4 Wood/EndGrain
textures are read without modification.

The approved mesh/UV/material calculations are unchanged. Production renames the
class, resource and shader, adds Scene bootstrap, and removes diagnostic toggles.
The stairs retain ownership of child rise, _Color fade, visibility and final pose.
No camera, gameplay source or Scene is edited.

No F7/F8, command-line review switch, input handler, teleport, GUI, Editor QA entry
or comparison method is introduced into Production.

## Validation isolation

The existing MainStageVisualAudit_20261003 Project is reused, not cloned again.
Its QA-only observer temporarily renders the archived approved reference and the
Production renderer synchronously at identical camera settings. That reference
and observer stay in isolated Assets; their reproducible source stays under Docs.
The Production renderer is discovered through its real Scene bootstrap, not added
by the observer.

Production Editor was found open. The owner closed it before Production writes.
511 pre-existing Production input/document/log files are hash-protected.

## First attempt — retained failure

Run01 stopped because the newly authored shader .meta GUID had 33 hex characters,
and Unity ignored the shader asset. The new GUID alone was corrected to 32 hex
characters. No visual math or existing asset was changed. Run01/log are retained;
only a later completed regression run can count as PASS.

## Not yet claimed

Human real-key play of the integrated version, full-route playthrough and all
checkpoint/retry permutations are unconfirmed unless explicitly recorded later.
Proof adoption is not substituted for those tests.
