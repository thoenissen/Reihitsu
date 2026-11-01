Role: issue manager
Commit: 10cc3bc
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| Narrow the directive guards of `ControlFlowBraceTransform` and `SwitchCaseBraceRewriter` (behavior change) | created (feature-request form; no open or closed issue covers it) | #1537 | enhancement, formatter, agent-filed | #1482 |
| FMT-STRUCT-003 `Why:` is stale since #1513 (text-only) | created (internal, lane docs) | #1538 | documentation, maintenance, formatter, agent-filed | none |
| Comment at an expression body's arrow lands on the new brace's line | not filed: duplicate of #1515 (open; same `{/* d */` output, FMT-STRUCT-006 unspecified, same input shapes) and #1439 (open); the candidate adds no fact they lack | #1515 | - | - |
| `ExpressionBodyToBlockConverter` is a third carrier of a wrap's comment trivia | created (internal, lane bug-fix; #1515 and #1439 are about placement, not ownership) | #1539 | formatter, maintenance, agent-filed | #1482 |
| `case 1: /* c */ A();` in a section that gets braces (output question) | created (feature-request form, output question with Input, Before, After A and B, recommendation A; #1381 and #1518 do not cover this shape) | #1540 | enhancement, formatter, agent-filed | none |

## Notes

- Output question of #1540 (output reproduced with `scripts/format.sh`; the formatted copy was in the scratchpad, not the repository): recommendation A, keep the comment behind the label.
- Searches were by title over the open and closed issue lists (the search tool is not available); the formatter-labelled open list was read in full.
