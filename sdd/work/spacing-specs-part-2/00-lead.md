Role: Lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed

# Run: thoenissen/Reihitsu#987

- Issue: thoenissen/Reihitsu#987 — SDD analyzer specs for RH6013–RH6024 (spacing, part 2), part of thoenissen/Reihitsu#899.
- Slug: `spacing-specs-part-2`
- Branch: `claude/confident-davinci-soq6vw` (the session's designated branch; the session may not push to `claude/issue-987-…`).
- Claim: draft PR thoenissen/Reihitsu#1103.
- Lane: **tests** (specs and tests for existing behavior) — the issue asks for specs of existing rules plus covering tests; no production change.

## Roles

| Role | Runs | Reason |
|---|---|---|
| Spec author | yes | writes the 12 specs and any shared-spec extension |
| Skeptic | yes | spec against the code |
| Test implementer | yes | tags existing tests, adds covering tests |
| Implementer | no | lane has no production change |
| Cleaner | yes | format and warning-free Release build |
| Spec checker | yes | |
| Auditor | no | not part of the tests lane |
| Validator | yes | |

## Maintainer clarifications (summarized)

- Read first: `AGENTS.md`, `sdd/README.md` (especially *Open questions*), `sdd/specs/shared/whitespace-removal-fixes.md`, `sdd/specs/formatter/horizontal-spacing.md`, `sdd/specs/shared/code-fixes.md`, and the principle decisions on thoenissen/Reihitsu#912 (P1–P8).
- Already-decided spacing follow-ups: thoenissen/Reihitsu#1074 (whitespace around literals and inside interpolation holes is code), thoenissen/Reihitsu#1077 (a `/** */` comment documenting nothing is an ordinary block comment), thoenissen/Reihitsu#1078 (spacing rules do not report whitespace touching a comment), thoenissen/Reihitsu#1079 (assorted spacing bugs). Specs describe current behavior, so those cases stay out of the spec. When a rule of this batch shows the same shape, do not file a question: list the rule and the case in the handover so the lead can comment on the matching issue.
- Analyzer/formatter disagreement (P4) has no general principle and is filed as an ordinary open question.
- A case matching P1, P2, P3, P5, P6, P7, or P8 is filed in the spec's open-questions issue marked `Decided by principle Pn (#912)`, naming the requirement it violates.
- Running concurrently: thoenissen/Reihitsu#1068, #1069, #981, #983, #990 — avoid editing their files beyond the shared index files (`sdd/specs/README.md`, `sdd/specs/requirement-ids.txt`).
- Must not: change production behavior, retire an existing requirement without asking the maintainer, touch `sdd/work/coordinator/`, or create any issue other than open questions without approval.
- Done: every rule of the batch has its spec, shared whitespace-removal behavior is reused or extended, every requirement has a covering test, open questions are filed, CI is green, PR says `Closes #987`.

## Open questions

Roles list open questions found in passing in their handover, drafted for the `SDD open questions: <spec path>` issue; only the lead files them on GitHub.

## Lead rulings

- Same-line "comment, whitespace, checked token" cases (RH6014–RH6017, SHARED-GAP-001) are covered by thoenissen/Reihitsu#1078, whose body defines the scope as whitespace between a comment and the token a spacing rule checks (its RH6006 instance is entirely on one line). They are left out of the specs and noted on #1078.
