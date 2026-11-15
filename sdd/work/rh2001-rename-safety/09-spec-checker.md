Role: spec checker
Commit: 57f69d1
Verdict: findings

The prompt carried no intended fix or hoped-for verdict. Diff base: c6d199c (merge base with origin/main).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | sdd/changes/2026-11/rh2001-rename-binding.md (Status) | Status is Draft; decision 7 in 00-lead.md accepts the change doc, and it must be Accepted before the PR is marked ready. The Decisions section already records the maintainer's answer B. | spec author (or the lead, at acceptance) |

## Checked
1. Change doc: required (requirement added, RH2001-004 coverage changed) and present; no task list or plan; has Visible output changes; the maintainer's answer to output question 1 is in Decisions. Status Draft: finding 1.
2. Specs match the diff: RH2001-011 states the withheld classes (rewritten, left as written, renamed reference misbinding) and the kept this.-qualified cases; matches the code fix change and the measured outputs in 05; format (heading, normative sentence, Why, examples, markup) ok.
3. IDs: RH2001-011 is the next free number, inserted in sorted position in requirement-ids.txt; nothing reused or removed.
4. Code owners: unchanged and exist; SpecConsistencyTests passed within the architecture run; sdd/specs/README.md needs no change (no spec added).
5. User documentation: no rule page or documentation file in the diff; the fix's existing output for offered cases is unchanged, so the RH2001 page needs no edit. ok.
6. Open questions: the only output question has an answer under Maintainer answers in 00-lead.md; no open question in 02/06 handovers; the follow-up (unify rename-safety owner) is under Follow-up candidates.
7. scripts/test.sh --project architecture: Passed! Failed: 0, Passed: 118, Skipped: 0, Total: 118. RH2001-011 is covered by 42 [Requirement("RH2001-011")] tests in the diff.
8. Tags: all 42 test methods the diff adds carry [Requirement("RH2001-011")] (the only requirement the 03/05/07 tables list); every example named by the spec author (withheld block-body lambda, delegate-typed extension, expression-body lambda, int extension, inherited field, out/primary-constructor) has a test.
9. sdd/work holds only rh2001-rename-safety.
