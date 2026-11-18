# Lead — expression-body-arrow-unsupported

- Issue: thoenissen/Reihitsu#1851 — `agent-filed` (goals are proposals; the decision to declare the shape is the maintainer's, see below)
- Slug: `expression-body-arrow-unsupported`
- Branch: `claude/charming-carson-n5mazp` (session-designated)
- Draft PR: thoenissen/Reihitsu#1856
- Lane: behavior change — the issue declares a new unsupported shape (new FMT-UNSUP requirement, formatter refusal, CLI report, withheld code fixes, possibly RH5401 reporting), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.

## Maintainer clarifications (summarized)

- The maintainer already answered escalation 10 for this shape: declare it unsupported, in a change of its own; this issue is that change (issue body and triage comments). The decision to declare stands; the run builds it.
- Prerequisite runs (#1847, #1848, merged through #1853) are on `main`; the sibling shape behind the arrow (FMT-UNSUP-012, #1839) is on `main`.
- Open in the issue, decided in the change: whether RH5401 stops reporting a member whose arrow line follows a comment, or keeps reporting with no fix (issue recommends the first). This is a visible output → output question.

## Maintainer answers

- Escalation 10 (declare the shape "comment, directive, or disabled text between an expression body's `=>` and the code in front of it"): declare it, in a change of its own — this run. (Given before the run, in #1851.)
- Batch 1, Q1 (change doc visible output change 12: `#if` group around the signature, a constraint clause, or a property's type, ending in front of a shared `=>`): **A** — refused as part of the shape; RH3202's and every formatting fix withheld on it; RH3202 and RH5401 keep reporting; declared in this work.
- Batch 1, Q2 (CLI reason text): **A** — `a comment or directive between the arrow of an expression body and the code in front of it`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Issue goal "RH5401 stops reporting a member whose arrow follows a comment" | Drop; RH5401 keeps reporting with no fix (spec author's Q1 option A) | SHARED-DIAG-010's last sentence already demands the report for a refused shape; option B would need a spec exception | Today's output is kept unchanged | — |
| 2 | skeptic | Spec author's Q3 (one-line `int A /* c */ => 1;`) as a separate question | Not asked separately; it is inside the span the maintainer answered and listed in the change doc's visible output changes | The issue's span is two trivia lists with no line-break condition | Covered by the maintainer's escalation-10 answer | — |
| 3 | skeptic | FMT-UNSUP-013 / change doc *Why* name accessors for the join refusal | Reword to name only properties and indexers (skeptic internal 1) | FMT-BREAK-026 joins properties and indexers only | Spec wording | pending |
| 4 | skeptic | SHARED-BODY-001 leaves a conditional group in front of `=>` unspecified | Specify the observed cases (reported, fix withheld) with an example (skeptic internal 2) | Lets the analyzer tests be tagged; reporting unchanged | Reporting unchanged; fix withholding follows from FMT-UNSUP-013 | pending |
| 6 | before test implementer | Ask change doc visible output changes 1–11 again | Not asked: each is a refusal, or a withheld fix, inside the span the maintainer already declared in #1851 (both trivia lists, comments and directives, all listed owners). Asked: change 12 (`#if` group around signature/constraint, unlisted in the issue, formatted well today) and the reason wording | Escalation 10 already answered for this span | Covered by the maintainer's answer | — |
| 5 | skeptic | Accessor `=>` on own line without a comment lands in accessor column | Follow-up candidate, carrying the skeptic's drafted output question | Outside #1851: no comment involved | Not changed in this run | — |

## Follow-up candidates

- Accessor's `=>` on a line of its own (no comment) lands in the accessor's column; output question drafted in 02-skeptic.md *Follow-up candidates* 1 (recommend joining onto the keyword's line).

## State

- Last step: maintainer answered batch 1 (1A, 2A).
- Spec author recorded answers, change doc Accepted (652f7f0, 04-spec-author.md).
- Test implementer done (35945c0, 05-test-implementer.md): 126 formatter, 33 analyzer, 11 CLI tests red as expected.
- Implementer done (51ffef7, 714d8e0; 06-implementer.md): all tests green in Debug.
- Next: cleaner (07).
