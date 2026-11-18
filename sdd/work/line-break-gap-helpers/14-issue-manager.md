Role: issue manager
Commit: 90eac43
Verdict: filed

## Issues
| Candidate | Outcome | Issue | Labels | Parent |
|---|---|---|---|---|
| 1. Bug: tokens behind a multi-line comment's closing line are not broken onto their own line (SHARED-COMMENT-012); carries the FMT-BREAK-037 output question | created (spec-bug format; reachable through the CLI and RH5302/RH5308/RH5203) | #1864 | agent-filed, bug, formatter | none |

Notes:
- Duplicate search covered the 200 newest issues, open and closed (no search tool was available). Related, not duplicates: #1572 (open, `agent-filed`; proposes the opposite spec direction, so a comment was added pointing at #1864), #1571 and #1642 (closed, different concern).
- The issue states the maintainer's answer to escalation 10 as a decision (shape not declared unsupported, the missing line break is a bug) without the run's internals.
- "Before" outputs come from the skeptic's runs of `scripts/format.sh` on 731f093; the cell F output question carries its observed "Before".
