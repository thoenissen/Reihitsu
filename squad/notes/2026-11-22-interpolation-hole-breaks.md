# interpolation-hole-breaks

| | |
|---|---|
| Date | 2026-11-22 |
| Run | issue #1566 |
| Pull request | #1945 |

## Questions to the maintainer

None. No output question and no escalation reached the maintainer: every output change was demanded by unchanged FMT-PIPE-003/FMT-PIPE-008 (output must compile under the project's language version), and the remaining choices concerned rare input classes left as written.

## Cost

- No stops for the maintainer.
- Two repair rounds plus one architecture repair between them:
  - Audit 1: two more phases (documentation-comment relocation, control-flow brace transform) wrote line breaks into a C# 10 `$"` hole when a `/** */` comment stood in it — a member of the issue's class the skeptic's enumeration (36 fixtures, no comments) missed. Repair round 1.
  - Audit 2: the round-1 brace guard was per-brace and missed nested bodies — the same class again, handled as an architecture finding: one cheap check on the cause (a doc comment in the containing hole) replaced the per-brace guard.
  - Audit 3: one untested condition side; repair round 2 added one test.
- Roles spawned more than once: spec author 5×, test implementer 6×, implementer 3×, and four full cleaner/spec-checker/auditor cycles. The four audits together are the largest single cost item (about 17M cache read).
- The skeptic's fixture set covered constructs but not trivia inside the hole; a comment axis in its first enumeration would likely have saved both audit repeats.

## Usage

Session `607db21f-0bb7-513f-bc4a-3b90b98b867e`. Responses counted once per message id. Models are named by tier (large, medium, small), as `.claude/agents/README.md` assigns them. Prices are not applied; compute a cost from the tokens and the tier.

| Agent | Spawn | Agent ID | Description | Tier | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write | Context growth |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | large | medium | 29 | 96 | 253 | 46.082 | 6.292 | 17.056.802 | 181.204 | 148.347 |
| skeptic | 1 | a666e8ac8646df9e2 | Skeptic premise check #1566 | large | high | 1 | 49 | 95 | 44.430 | 17.895 | 6.816.188 | 189.329 | 110.578 |
| spec-author | 1 | a2b03817877cfcaf6 | Spec author clarification #1566 | large | medium | 1 | 16 | 31 | 10.708 | 3.135 | 1.636.875 | 63.741 | 38.916 |
| test-implementer | 1 | a6182bdb91f0d21ff | Test implementer #1566 | large | medium | 1 | 47 | 89 | 43.759 | 11.404 | 7.004.382 | 147.469 | 121.960 |
| test-implementer | 2 | a5ed47934b6e210e8 | Test amendment switch #1566 | large | medium | 1 | 10 | 20 | 5.293 | 943 | 919.018 | 49.974 | 25.086 |
| implementer | 1 | a08051de1cc67b8b1 | Implementer #1566 | large | medium | 2 | 54 | 108 | 40.279 | 16.123 | 7.833.934 | 141.117 | 115.712 |
| spec-author | 2 | abcb8e9478882afb3 | Spec author switch clarification | large | medium | 1 | 17 | 34 | 10.466 | 3.051 | 1.594.981 | 56.094 | 31.272 |
| spec-author | 3 | aed848cfd2f5919ad | Spec author blank-line phase | large | medium | 1 | 10 | 20 | 7.902 | 3.584 | 897.290 | 57.443 | 31.947 |
| test-implementer | 3 | a7a0b55c789db9efe | Test corrections #1566 | large | medium | 2 | 10 | 20 | 6.906 | 1.318 | 957.562 | 64.405 | 39.537 |
| cleaner | 1 | af493d7b0d4300d61 | Cleaner #1566 | medium | low | 1 | 5 | 10 | 1.148 | 0 | 386.524 | 36.533 | 11.885 |
| spec-checker | 1 | a843af2a385c683ec | Spec checker #1566 | medium | medium | 1 | 6 | 12 | 2.782 | 261 | 475.665 | 44.205 | 19.588 |
| auditor | 1 | a051546a50da67c88 | Auditor #1566 | large | high | 1 | 47 | 94 | 52.597 | 30.830 | 7.657.483 | 178.268 | 153.608 |
| test-implementer | 4 | adf468717bc5f80f1 | Test implementer repair round 1 | large | medium | 2 | 23 | 46 | 21.166 | 6.073 | 2.758.618 | 98.850 | 74.078 |
| spec-author | 4 | a874d59c6a5571178 | Spec author repair round 1 | large | medium | 1 | 16 | 32 | 14.494 | 5.411 | 1.600.756 | 66.543 | 41.133 |
| implementer | 2 | a319a0ed4a388d3e3 | Implementer repair round 1 | large | medium | 1 | 18 | 36 | 14.525 | 3.369 | 1.775.335 | 147.053 | 51.697 |
| cleaner | 2 | af71b49279420b0a5 | Cleaner round 1 | medium | low | 1 | 5 | 10 | 1.208 | 0 | 318.943 | 81.895 | 6.345 |
| spec-checker | 2 | a1c59a2253d8448a2 | Spec checker round 1 | medium | medium | 1 | 7 | 14 | 2.942 | 80 | 531.064 | 101.457 | 26.381 |
| auditor | 2 | ae8b386177fe6fca0 | Auditor round 1 | large | high | 1 | 32 | 64 | 34.025 | 19.095 | 4.337.697 | 124.339 | 99.585 |
| test-implementer | 5 | a5b643642bd0804dd | Test implementer decision 17 | large | medium | 2 | 20 | 40 | 18.312 | 4.836 | 2.204.132 | 86.483 | 61.056 |
| spec-author | 5 | af1967b30a1f2304f | Spec author decision 17 | large | medium | 1 | 19 | 38 | 16.060 | 4.387 | 1.863.810 | 65.483 | 40.074 |
| implementer | 3 | a575431abf5030226 | Implementer decision 17 | large | medium | 1 | 19 | 38 | 11.971 | 3.264 | 1.901.681 | 69.685 | 44.875 |
| cleaner | 3 | ad3a868bf784612a1 | Cleaner decision 17 | medium | low | 1 | 7 | 14 | 2.432 | 0 | 497.322 | 87.012 | 11.419 |
| spec-checker | 3 | a1ef164920909fde0 | Spec checker decision 17 | medium | medium | 1 | 6 | 12 | 2.609 | 137 | 426.172 | 98.881 | 23.812 |
| auditor | 3 | aeff55244eb1d7567 | Auditor decision 17 delta | large | high | 1 | 33 | 66 | 28.197 | 13.523 | 4.158.650 | 169.030 | 93.829 |
| test-implementer | 6 | aa88cac22e5097635 | Test implementer round 2 | large | medium | 1 | 13 | 26 | 5.550 | 363 | 1.142.202 | 47.836 | 22.482 |
| cleaner | 4 | a99d78837f2e7c751 | Cleaner round 2 | medium | low | 1 | 5 | 10 | 1.208 | 0 | 316.013 | 81.189 | 5.650 |
| spec-checker | 4 | acf79e54b18c69525 | Spec checker round 2 | medium | medium | 1 | 6 | 12 | 1.844 | 60 | 435.000 | 94.224 | 19.113 |
| auditor | 4 | a7ab3febf1c4e9145 | Auditor round 2 | large | high | 1 | 8 | 16 | 5.658 | 1.403 | 629.072 | 101.114 | 25.925 |
| validator | 1 | acb9409e95e2b7eb3 | Validator #1566 | small | low | 2 | 8 | 16 | 2.854 | 370 | 193.037 | 70.106 | 13.077 |
| issue-manager | 1 | ae1f0d82040b36965 | Issue manager #1566 | medium | medium | 1 | 10 | 22 | 4.947 | 423 | 989.825 | 69.780 | 44.464 |

| Tier | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| large | 557 | 1.166 | 438.380 | 156.299 | 74.746.468 | 2.105.460 |
| medium | 57 | 116 | 21.120 | 961 | 4.376.528 | 695.176 |
| small | 8 | 16 | 2.854 | 370 | 193.037 | 70.106 |
| **Total** | 622 | 1.298 | 462.354 | 157.630 | 79.316.033 | 2.870.742 |

## Decisions taken

22 decisions, recorded in the run's lead file. The ones a reviewer may want to see:

- Below C# 11, inside a hole of a `$"` string, inserted braces get no line break and the spacing rules give `if (b) {return 1; }return 2;` (unchanged FMT-SPACE-002/009 decide it; no new spacing rule for a rare class).
- A switch statement inside such a hole is left unbraced (FMT-SWITCH-017), and an `if`/`else` body is left unbraced when any containing `$"` hole holds a `/** */` comment.
- RH5417's private copy of the "single-line interpolation" check moved into `Reihitsu.Core` and is shared with the formatter.
- A comment in an interpolation alignment clause losing its following space (also on `main`) was dropped as a rare class, not filed.

## Suggestions

- The skeptic's class enumeration for a "line break inside X" defect should include a trivia axis (no comment, `/* */`, `/** */`, directive where legal) from the start; in this run two of three audits found members on that axis.
