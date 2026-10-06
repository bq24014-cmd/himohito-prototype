# Focused Production regression

Executed with Unity 6000.3.21f1 in the existing isolated Editor using the exact
Production Source/Shader/meta bytes. Final Run02: **59/59 PASS**.
Gameplay Error/Exception/Assert: **0**. A full Console-zero claim is not made.

| Area | Result | Executed scope |
|---|---|---|
| Compile / Play | PASS | Runtime, Shader and isolated Editor QA compile; MainStage/Tutorial load in Play |
| Scene bootstrap | PASS | Production owner discovered automatically, prepared while Step A is inactive |
| Hidden / reveal | PASS | Existing switch proximity method and original stairs lifecycle; hide/re-reveal final Production pose |
| Standing / collider | PASS | PlayerMover grounded, Rigidbody2D contact with the actual Step A BoxCollider2D |
| Walk | PASS | Original FixedUpdate with diagnostic input injection, x 42.8 to 42.97104 |
| Jump / air / landing | PASS | Original jump buffer; y velocity 8.33469, airborne then grounded |
| Aim / attach / detach | PASS | Original S3 lower Hook resolution and rope API calls |
| Rope amount | PASS | Existing attach/detach semantics and resource value retained |
| Target support | PASS | Existing position/scale, unit collider/offset/non-trigger, full flat rim within support |
| Physics / Camera | PASS | Static physics and real Camera state unchanged across synchronous presentation switches |
| Other 20 floors | PASS | Transform/collider contracts and renderer pose/mask/sort/mesh/material/sprite state retained |
| B/C/D | PASS | No pencil child or horizontal rollout; current terrain retained |
| Start Ground / S4 ruler | PASS | Approved presentation children present; protected original Source/Assets hash-identical |
| Tutorial | PASS | No Production S3 owner; adopted Hybrid and 2.5D components present; original files hash-identical |
| Scene reload | PASS | MainStage -> Tutorial -> MainStage creates one owner again, no automatic QA reference |
| Approved identity | PASS | Mesh vertices/normals/triangles and UVs exact; identical wood/end-grain texture references |

One UnityEditor.Search SearchDatabase initialisation exception is recorded
separately in Result.json and UnityRun02.log. It also existed in earlier audit/
proof logs. It was not fixed or treated as a Gameplay failure.
Run01's new-meta import failure remains recorded separately; it is not PASS.

## Unconfirmed / not executed

- Human real-key integrated-version play: unconfirmed.
- Full MainStage/Tutorial route play and difficulty: not executed.
- Nonzero-speed dynamic swing-release momentum: not executed; measured detach velocity was (0,0).
- Every retry/checkpoint/merge combination: not executed.
- Pixel-perfect comparison of all other floors separately: not executed; their
  object/renderer contracts were checked instead.
- New Windows Build / executable launch: not performed for this integration.
  The earlier Human-approved Proof Build is not relabelled as a Production Build.

Diagnostic placement, input-field injection and reference rendering are QA-only.
They are not shipped gameplay changes and do not establish Human keyboard PASS.
