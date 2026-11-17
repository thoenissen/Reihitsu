# Lead — arrow-comment-unsupported

- Issue: thoenissen/Reihitsu#1732 — label `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter. Milestone 1.4.0.
- Slug: `arrow-comment-unsupported`
- Branch: `claude/loving-ritchie-ss3rjx` (session-designated). Draft PR: #1839.
- Base at claim: origin/main 17fb08c.
- Lane: **behavior change** — the issue requests a new unsupported shape (new FMT-UNSUP requirement, visible formatter/CLI/code-fix output change), which needs a change doc.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. Roles skipped: none.
- Related: the issue states it supersedes #1730 and #1731, and follows FMT-UNSUP-008 (conditional) and the binary-operator shape FMT-UNSUP-011 (#1726, merged as #1831).
- The issue text records that the maintainer decided on 2026-11-14 to declare the shape unsupported (comment or directive between `=>` of an expression-bodied member and its expression), in a change of its own. The issue is `agent-filed`, so the lead confirms the declaration and its visible outputs with the maintainer in the first output-question batch (escalation 10 form) before the test implementer.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Issue goal "delete RH5401-004's format-and-compare" | Dropped; `CanCollapse` kept | It also decides the wrapped expression and the comment in front of the arrow; the shape behind the arrow is withheld via FindUnsupportedSyntax | RH5401 stays reported, no fix, before and after | 6990dee |
| 2 | skeptic | Issue goal "column-4 layout stops being reachable" | Corrected: holds only behind the arrow | Skeptic showed the in-front and next-line `return` cases remain | Statement only | — |
| 3 | skeptic | SHARED-BODY-001 Why sentence stale for the directive clause | Reword it (analyzers keep the predicate; the formatter decides the gap through FMT-UNSUP-012) | Skeptic recommendation, accurate | Spec wording | pending (spec author follow-up) |
| 4 | skeptic | FMT-UNSUP-012 Why for local functions | Cite RH3208 (and RH3202–RH3207) instead of "no more common" | Skeptic recommendation | Spec wording | pending |
| 5 | spec author | `BlocksRewrite` directive check | Keep unchanged | Changing it changes RH3202–RH3208 diagnostics | Nothing changes | 6990dee |
| 6 | lead | Output question 5 (code fixes withheld) | Folded into question 1 as the declaration's code-fix line | Withholding follows from SHARED-FIX-010 for a declared shape; no real alternative | Shown to the maintainer in question 1 | — |

## Follow-up candidates

- Comment directly in front of the arrow on its own line (`int B()` / `/* c */ => 1;`) formats as `/* c */ {` in front of the new brace; FMT-STRUCT-006 leaves the placement unspecified; carries an open output question with three readings (01-spec-author.md). May be absorbed by batch-1 question 6 (escalation 10, in-front shape).
- 02-skeptic follow-ups 1–4: (1) RH5401 reports `int B // note` / `=> 1;` — SHARED-DIAG-010 conflict; (2) in-front comment/directive puts arrow line in member's column (twin of #1731); (3) `return` value on the next line lands in `return`'s column, blank line between arrow and expression reads two ways in FMT-STRUCT-006; (4) `return // c` / `_v;` in a getter — same column class as (3). 1, 2 may be absorbed by batch-1 question 6.

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01, commit 6990dee/bea1289), change doc Draft; lane check confirmed.
- Done: skeptic (02, b746fed), verdict output question.
- Batch 1 asked (questions 1–6; 01's Q5 folded into Q1; 01's Q6 reason text = batch Q5; 02's Q7 escalation 10 = batch Q6). Waiting for answers.
- Then: route answers + decisions 3,4 to the spec author; spec author sets Status: Accepted; then test implementer.
