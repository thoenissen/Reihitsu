# Lead — line-break-token-kinds

- Issue: thoenissen/Reihitsu#1886 — `agent-filed` (goals are proposals)
- Slug: `line-break-token-kinds`
- Branch: `claude/clever-sagan-uxp15i` (session-designated)
- Draft PR: thoenissen/Reihitsu#1894
- Lane: **bug fix** — a structural defect (one rule — which tokens/trivia carry a line break — stated in three places: detection, RH5604 analyzer, normalization) fixed by changing production code only; no visible output is meant to change, so no change doc.
- Roles run: skeptic (premise, reachability), test implementer (tests that pin the shared definition / unchanged outputs), implementer, cleaner, spec checker, auditor, validator. Spec author: only for a clarification.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Issue reports no wrong output — escalation 4? | No escalation; the issue is a structural (FC-015) refactor, not an observed bug. Proceed with characterization tests instead of a reproduction | Escalation 4 covers a reported wrong output that does not reproduce; #1886 reports none | Refactor keeps every output; tests pin it | — |
| 2 | Skeptic | Scope of the shared definition | Narrower than the issue: share only which trivia/tokens carry a line break; the analyzer's "mixed" rule and the detection's "predominant" rule stay separate; the analyzer keeps using `DetectEndOfLine`. Shared predicate(s) or one enumeration — implementer's choice | Merging the two decisions would change visible output | Outputs unchanged by construction | — |
| 3 | Skeptic | Optional RH5604-002 clarification (all-U+2028 file not reported) | Skip; no spec author stage | P11 already covers the relation; not a defect | No text or code change | — |
| 4 | Auditor | Hint 1: is the untagged "all one other terminator — not reported" test an undecided output? | Decided: RH5604-001's precondition ("not all the same") is false for this input, RH5604-002's Why and the rule page describe consistency; today's `main` behavior. Keep the test; no spec clarification (decision 3 stands) | One reading of the unchanged spec decides it | Output unchanged from `main` | 5adc655 |

## Follow-up candidates

- RH5604 under `DocumentationMode.None` (FC-040), auditor found evidence (a `/** */` line break under None is comment text, not reported; formatter rewrites it); unknown which mode builds without doc generation pass — `01-skeptic.md`, `06-auditor.md`

## State

- Audited SHA: 0c154f10285c400b033a1c202c83ef73c94608f6 (auditor pass, 06-auditor.md).
- Last step: auditor pass.
- Next: validator, then issue manager (FC-040), note, publish.
