# csharp15-union-with-element

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issues #1762, #1763 |
| Pull request | #1782 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Wrapped union case-type list: keep the primary-constructor aligned layout, or join onto one line? | A — keep aligned layout | yes |
| 2 | output question | Empty union body `{ }`: becomes `;`, or keeps braces on their own lines? | A — becomes `;` | yes |
| 3 | output question | Unions get the class layout (braces, members, `(` joins name, `where` per line, field split; RH7101 and RH5404 fixes follow)? | A — yes | yes |
| 4 | output question | `with(...)` collection element: no space between `with` and `(` (RH5305 fix follows)? | A — no space | yes |

## Cost

One output-question batch (before the test implementer), no escalations. One repair round, triggered by the spec checker: one spec sentence (FMT-PIPE-029) still listed the type kinds without union, and the change doc's affected-requirements list was incomplete; the cleaner was skipped for that text-only round. The spec author ran three times: initial specs, recording the answers plus the skeptic's code-fix amendment, and the repair. The expensive spawns were the spec author's first run, the skeptic, the test implementer, and the auditor (each about 8–10 M cache-read tokens), largely from CLI probe runs on scratch fixtures.

## Usage

Session `86c85cc9-b36d-50bc-9b9b-9bafe1100a82`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 14 | 46 | 114 | 21.244 | 1.571 | 6.310.960 | 183.324 |
| spec-author | 1 | a52d569fa3d3b1b7f | Spec author for #1762 #1763 | claude-opus-5-5 | medium | 1 | 58 | 112 | 43.188 | 13.738 | 9.025.198 | 216.157 |
| skeptic | 1 | a1acb69a853b6758c | Skeptic for #1762 #1763 | claude-opus-5-5 | high | 1 | 54 | 110 | 51.927 | 24.049 | 7.939.138 | 210.440 |
| spec-author | 2 | aee872a48567a2811 | Spec author amendment #1762 #1763 | claude-opus-5-5 | medium | 1 | 8 | 16 | 5.370 | 624 | 663.727 | 111.787 |
| test-implementer | 1 | ad031dc4488c7794f | Test implementer #1762 #1763 | claude-opus-5-5 | medium | 1 | 52 | 104 | 47.849 | 9.174 | 8.390.287 | 175.485 |
| implementer | 1 | aa5935c4ab686426f | Implementer #1762 #1763 | claude-opus-5-5 | medium | 1 | 23 | 46 | 13.031 | 597 | 2.433.583 | 76.066 |
| cleaner | 1 | a30c06874bffe0c70 | Cleaner #1762 #1763 | claude-sonnet-5-5 | low | 1 | 4 | 8 | 1.458 | 31 | 249.860 | 89.544 |
| spec-checker | 1 | a728649b122c29f09 | Spec checker #1762 #1763 | claude-sonnet-5-5 | medium | 1 | 10 | 20 | 5.147 | 1.213 | 881.495 | 115.784 |
| spec-author | 3 | a1fb9a2187007cf3f | Spec author repair #1762 | claude-opus-5-5 | medium | 1 | 9 | 18 | 4.394 | 460 | 742.858 | 45.311 |
| spec-checker | 2 | a5bd9c6bee465dfc2 | Spec checker re-run #1762 #1763 | claude-sonnet-5-5 | medium | 1 | 4 | 8 | 1.512 | 0 | 287.850 | 33.712 |
| auditor | 1 | a40a9ffe96e173643 | Auditor #1762 #1763 | claude-opus-5-5 | high | 1 | 55 | 112 | 39.375 | 16.363 | 9.877.103 | 248.968 |
| issue-manager | 1 | a49f1b0ff69968783 | Issue manager follow-ups | claude-sonnet-5-5 | medium | 1 | 8 | 16 | 7.101 | 600 | 746.631 | 67.663 |
| validator | 1 | a219847b645989134 | Validator #1762 #1763 | claude-haiku-5-5 | low | 2 | 8 | 16 | 3.062 | 477 | 565.802 | 130.461 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 8 | 16 | 3.062 | 477 | 565.802 | 130.461 |
| claude-opus-5-5 | 305 | 632 | 226.378 | 66.576 | 45.382.854 | 1.267.538 |
| claude-sonnet-5-5 | 26 | 52 | 15.218 | 1.844 | 2.165.836 | 306.703 |
| **Total** | 339 | 700 | 244.658 | 68.897 | 48.114.492 | 1.704.702 |

## Decisions taken

Seven. The one the maintainer may want to see: the skeptic's architecture finding (the list of type-declaration kinds is stated in seven places and has drifted, with extension blocks already left out) was fixed narrowly by adding unions to the six formatter lists; generalizing would have changed extension-block output, which is undecided, so the consolidation went to #1785. The union C# 14 gate in FMT-STRUCT-017 got no test, because Roslyn 5.12 produces no union nodes under C# 14. The run used the session's designated branch instead of the playbook's `claude/issues-…` name, because the environment allows pushes only there.

## Suggestions

- The skeptic's "change-doc amendment" (code-fix results that change because fixes format through the formatter) cost an extra spec-author spawn. The spec author's role could ask it to list, for each formatter output change, the code fixes that format the affected node, so the first change doc already holds them. Evidence: RH5404 and RH5305 results were added only after the skeptic ran.
