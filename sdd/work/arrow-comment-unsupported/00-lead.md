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

Batch 1 (before test implementer), answered `1A 2A 3A 4A 5A 6A`:
1. Declare a comment/directive behind `=>` of property/indexer/method unsupported, in this change (confirms the issue's decision); RH3202/3203/3205–3208/RH7101 and other formatter-delegating fixes withheld on such code, diagnostics stay, RH5401 reported without a fix — A.
2. Accessors and local functions belong to the shape — A.
3. Directives and disabled text belong to the shape (refused in every build) — A.
4. Also a block comment on one line (`int Get() => /* c */ 42;`) — A.
5. CLI reason text: `a comment or directive between the arrow of an expression body and its expression` — A.
6. Escalation 10: declare a comment/directive in front of the `=>` unsupported too — A, **declared in a change of its own** → follow-up issue carrying this answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Issue goal "delete RH5401-004's format-and-compare" | Dropped; `CanCollapse` kept | It also decides the wrapped expression and the comment in front of the arrow; the shape behind the arrow is withheld via FindUnsupportedSyntax | RH5401 stays reported, no fix, before and after | 6990dee |
| 2 | skeptic | Issue goal "column-4 layout stops being reachable" | Corrected: holds only behind the arrow | Skeptic showed the in-front and next-line `return` cases remain | Statement only | — |
| 3 | skeptic | SHARED-BODY-001 Why sentence stale for the directive clause | Reword it (analyzers keep the predicate; the formatter decides the gap through FMT-UNSUP-012) | Skeptic recommendation, accurate | Spec wording | e95c126 |
| 4 | skeptic | FMT-UNSUP-012 Why for local functions | Cite RH3208 (and RH3202–RH3207) instead of "no more common" | Skeptic recommendation | Spec wording | e95c126 |
| 5 | spec author | `BlocksRewrite` directive check | Keep unchanged | Changing it changes RH3202–RH3208 diagnostics | Nothing changes | 6990dee |
| 6 | lead | Output question 5 (code fixes withheld) | Folded into question 1 as the declaration's code-fix line | Withholding follows from SHARED-FIX-010 for a declared shape; no real alternative | Shown to the maintainer in question 1 | — |
| 8 | auditor | Hint: `KeepsCommentTrailingArrowInIndexer` feeds the structural phase a refused input | Delete it in repair round 1, like the other phase-level tests 04 deleted | Consistency; no caller passes that input | Test only | pending |
| 9 | auditor | Hint: lambda `[A] /* h */ [B]` attribute lists split behind a property arrow | Dropped, not a follow-up | Not the run's code, output stable, not shown to be a defect against a spec | Nothing changes | — |
| 7 | test implementer | RH5401-004 spec example `int C` inside `class C` does not compile | Spec author renames it to match the test (`D`) | Spec defect within the run | Example text only | bb95f71 |

## Follow-up candidates

- **Declare a comment or directive between an expression body's `=>` and the code in front of it unsupported** (maintainer answer, batch 1 Q6 = A, in a change of its own). Absorbs: 01-spec-author's `/* c */ {` follow-up and 02-skeptic follow-ups 1 and 2. Carries the question/answer and the special cases it deletes (02-skeptic, Output questions 7).

- Comment directly in front of the arrow on its own line (`int B()` / `/* c */ => 1;`) formats as `/* c */ {` in front of the new brace; FMT-STRUCT-006 leaves the placement unspecified; carries an open output question with three readings (01-spec-author.md). May be absorbed by batch-1 question 6 (escalation 10, in-front shape).
- 02-skeptic follow-ups 1–4: (1) RH5401 reports `int B // note` / `=> 1;` — SHARED-DIAG-010 conflict; (2) in-front comment/directive puts arrow line in member's column (twin of #1731); (3) `return` value on the next line lands in `return`'s column, blank line between arrow and expression reads two ways in FMT-STRUCT-006; (4) `return // c` / `_v;` in a getter — same column class as (3). 1, 2 may be absorbed by batch-1 question 6.

## Audited SHA

- Audit 1: a23ec37 (base 71a89ec) — findings (1 medium, test strength).

## Open worklist / next step

- Done: spec author (01, commit 6990dee/bea1289), change doc Draft; lane check confirmed.
- Done: skeptic (02, b746fed), verdict output question.
- Batch 1 asked (questions 1–6; 01's Q5 folded into Q1; 01's Q6 reason text = batch Q5; 02's Q7 escalation 10 = batch Q6). Waiting for answers.
- Done: spec author 03 (e95c126), change doc Accepted; decisions 3,4 carried out.
- Done: test implementer 04 (b1efb29): 138 formatter, 8 CLI, 23 analyzer failing as expected.
- Done: spec author 06 (bb95f71); implementer 05 (4680af8), all default projects pass in Debug, 4 production files, +70/-118.
- Done: cleaner 07 (ffd85d3), clean.
- Done: spec checker 08 (c320618), consistent. Merged origin/main 71a89ec cleanly (incl. #1840 precheck comma change, #1842 test helper routing); Release build green.
- Done: auditor 09 (b80ab0b): finding #1 (RH3202 neighbour test too weak, PR-introduced) → repair round 1.
- Repair round 1 worklist: (a) test implementer 10 — strengthen `VerifyFixIsOfferedForAMethodBesideAMethodWithACommentBehindItsArrow` (09 #1); (b) same agent — delete the phase-level test `ExpressionBodiedTransformTests.KeepsCommentTrailingArrowInIndexer` (decision 8).
- Then: cleaner 11, spec checker 12, auditor 13 (repair only), validator 14.
