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

## Follow-up candidates

- RH5604 under `DocumentationMode.None` (FC-040), unverified — `01-skeptic.md`

## State

- Last step: implementer (03-implementer.md, 92678eb). Lane confirmed.
- Next: cleaner, then spec checker.
