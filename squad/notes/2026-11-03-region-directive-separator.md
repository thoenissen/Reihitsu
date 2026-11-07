# region-directive-separator

| | |
|---|---|
| Date | 2026-11-03 |
| Run | issue #1486 |
| Pull request | #1578 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Several spaces before an upper-case region description become one space | A (one space) | yes |
| 2 | output question | A tab separator behind `#region` ends as one space | A (one space) | yes |
| 3 | output question | The same inside `#if false` | A (applies) | yes |
| 4 | output question | `#endregion` of a region without a description gets one space before its text | B (one space) | yes |
| 5 | output question | Trailing whitespace on a `#region` line with a description | C (always removed) | yes |
| 6 | output question | Code fixes that format a node holding a region pair rewrite the separator | B (no; a fix changes only what its diagnostic is about) | no |
| 7 | output question | Trailing whitespace behind the text of an undescribed region's `#endregion` | B (removed; trailing whitespace is always removed) | yes |

## Cost

Two stops for output questions: the batch of six before the test implementer, and one follow-up question the spec author's amendment raised. Answer 6 widened the run: unrelated code fixes no longer apply any region requirement (new FMT-PIPE-028), which also absorbed a pre-existing half-pair defect. Repair rounds: two (audit 1 findings; the public-XML-doc proof failure after audit 3's findings), plus one structural change after audit 2 repeated audit 1's class (the node-level rule restated as an exhaustive list in several places). The spec author ran five times and the test implementer five times; most re-spawns were small amendments (maintainer answer 7, a decision derived from answer 7, four stale tests the first test round missed, the repair, the structural change). The auditor ran four times; audits 2 to 4 each found documentation-only restatements.

## Usage

The session that ran this note withholds model identifiers from pushed files, so the model column reads `(withheld)`; the role-to-model assignment is in `.claude/agents/README.md`. Otherwise the output is unchanged.

Session `aadd16af-6447-5dbd-b660-f207471a6418`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | (withheld) | medium | 25 | 90 | 239 | 47.305 | 6.513 | 14.796.324 | 185.615 |
| spec-author | 1 | a37491e63e43533a0 | Spec author for #1486 | (withheld) | medium | 3 | 64 | 128 | 60.594 | 23.571 | 8.660.722 | 571.708 |
| skeptic | 1 | aedf565db1114d137 | Skeptic for #1486 | (withheld) | high | 1 | 34 | 67 | 38.014 | 21.380 | 3.953.152 | 166.370 |
| test-implementer | 1 | a81ed74bc4f5e725d | Test implementer for #1486 | (withheld) | medium | 1 | 70 | 140 | 56.366 | 12.131 | 10.971.000 | 216.326 |
| spec-author | 2 | a37e9c439f8630eb4 | Spec amendment for #1486 | (withheld) | medium | 1 | 14 | 28 | 9.805 | 1.461 | 1.090.386 | 55.793 |
| test-implementer | 2 | a5275982c6734957f | Test amendment for #1486 | (withheld) | medium | 1 | 12 | 24 | 9.368 | 1.200 | 946.074 | 55.003 |
| implementer | 1 | a66398f0233b68864 | Implementer for #1486 | (withheld) | medium | 1 | 32 | 66 | 21.134 | 3.927 | 3.899.825 | 119.028 |
| test-implementer | 3 | a04190474c2723476 | Fix four stale tests #1486 | (withheld) | medium | 1 | 17 | 34 | 10.446 | 1.701 | 1.307.419 | 53.722 |
| cleaner | 1 | a4d113d66554b7217 | Cleaner for #1486 | (withheld) | low | 1 | 4 | 8 | 1.156 | 68 | 205.211 | 73.227 |
| spec-checker | 1 | ab6986c3663aebfc2 | Spec checker for #1486 | (withheld) | medium | 1 | 11 | 22 | 5.348 | 1.318 | 888.371 | 106.406 |
| auditor | 1 | a6b6e9e74ba9a00d4 | Auditor for #1486 | (withheld) | high | 1 | 42 | 86 | 40.115 | 23.806 | 6.398.641 | 222.378 |
| spec-author | 3 | acc39f36c686efae1 | Repair round 1 spec author | (withheld) | medium | 2 | 18 | 36 | 13.162 | 3.416 | 1.476.221 | 102.425 |
| test-implementer | 4 | a5f24763639019f68 | Repair round 1 tests | (withheld) | medium | 1 | 17 | 34 | 15.598 | 3.462 | 1.612.144 | 81.939 |
| cleaner | 2 | adfefb3f7dc1af606 | Repair round 1 cleaner | (withheld) | low | 1 | 9 | 18 | 3.921 | 135 | 562.742 | 75.791 |
| spec-checker | 2 | a178f554ba03a227a | Spec checker repair round 1 | (withheld) | medium | 1 | 6 | 12 | 2.630 | 73 | 388.538 | 90.874 |
| auditor | 2 | ac748e7ce7d88ef69 | Auditor repair round 1 | (withheld) | high | 1 | 27 | 54 | 26.241 | 12.429 | 2.935.953 | 149.233 |
| spec-author | 4 | ac471aeb03ae17967 | Structural spec change #1486 | (withheld) | medium | 1 | 8 | 16 | 7.555 | 2.148 | 581.340 | 97.154 |
| test-implementer | 5 | a609a2ba5d7301ae0 | Line-ending node test #1486 | (withheld) | medium | 1 | 13 | 25 | 13.047 | 3.115 | 1.237.899 | 73.551 |
| cleaner | 3 | ac65644fb0fb0ec8f | Structural comments cleaner | (withheld) | low | 1 | 5 | 10 | 2.676 | 65 | 301.729 | 80.944 |
| spec-checker | 3 | a1c5f13b677bc5da5 | Spec checker structural change | (withheld) | medium | 1 | 7 | 14 | 3.256 | 410 | 451.241 | 85.384 |
| auditor | 3 | aec3b0139010532d1 | Auditor structural change | (withheld) | high | 2 | 20 | 38 | 22.079 | 11.121 | 2.060.548 | 143.691 |
| cleaner | 4 | aeefc6e5d43ff53b6 | Comment leftovers cleaner | (withheld) | low | 1 | 6 | 12 | 3.124 | 86 | 367.242 | 79.019 |
| spec-author | 5 | a70783c09526267b6 | Change doc leftover fix | (withheld) | medium | 1 | 8 | 16 | 5.254 | 1.440 | 549.501 | 91.405 |
| spec-checker | 4 | a12ab3452ab132391 | Spec checker repair round 2 | (withheld) | medium | 1 | 6 | 12 | 2.733 | 104 | 374.956 | 84.040 |
| auditor | 4 | add4dbc3650f0f6b0 | Auditor repair round 2 | (withheld) | high | 1 | 15 | 29 | 10.312 | 2.826 | 1.197.937 | 98.818 |
| cleaner | 5 | a2c78ca19107465f1 | Rewrap remark cleanup | (withheld) | low | 1 | 7 | 14 | 2.957 | 260 | 416.599 | 73.419 |
| validator | 1 | ac004214e41b7d50a | Validator for #1486 | (withheld) | low | 1 | 8 | 16 | 3.436 | 589 | 460.771 | 162.305 |
| issue-manager | 1 | acda95fe9d195b82b | Issue manager for #1486 | (withheld) | medium | 1 | 8 | 17 | 4.632 | 601 | 593.262 | 99.262 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| (withheld) | 8 | 16 | 3.436 | 589 | 460.771 | 162.305 |
| (withheld) | 501 | 1.060 | 406.395 | 135.647 | 63.675.086 | 2.484.159 |
| (withheld) | 69 | 139 | 32.433 | 3.120 | 4.549.891 | 848.366 |
| **Total** | 578 | 1.215 | 442.264 | 139.356 | 68.685.748 | 3.494.830 |

## Decisions taken

20. The ones the maintainer may want to see: decision 7 (taking the pre-existing region rewrites of unrelated code fixes into the run under SHARED-FIX-006 on answer 6), decision 8 (a bare `#region···`/`#endregion···` loses its trailing whitespace, read from answer 7's general statement), and decision 20 (accepting a whitespace-only re-wrap of a public XML remark without a further audit although `verify-text-only.sh --strict-docs` fails).

## Suggestions

- The first test round listed existing tests with changed expectations from one probe that only covered the node-level change; four whole-document tests surfaced only at the implementer. A test implementer could probe each visible output change of the change doc separately before handing over.
- A statement of what a skipped phase leaves untouched invites exhaustive lists that drift (three audits found restatements of the same rule). Specs could state such exemptions by reference to the requirement that still applies, from the first draft.
- `verify-text-only.sh --strict-docs` fails on any public XML-doc change, also a whitespace-only re-wrap; the playbook's "a failing proof makes it a repair round" then forces a round for a reflow. A strict mode that compares normalized XML-doc text would avoid it.
