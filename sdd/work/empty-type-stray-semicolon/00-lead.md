# Run: empty-type-stray-semicolon

- Issue: thoenissen/Reihitsu#1275 — `agent-filed` (goals are proposals)
- Branch: `claude/serene-gauss-hi2uw4` (session-designated); draft PR thoenissen/Reihitsu#1954
- Lane: behavior change — the issue requests changed behavior of RH5412–RH5416 (report `class C { };`, a fix that writes `class C;` and drops the stray `;`), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.

## Maintainer clarifications

- The prompt named #1275 and #1837. The lead proposed splitting them (different mechanisms, 8 rules plus the formatter). The maintainer chose to split: this run covers #1275 only; #1837 runs separately.
- Issue comment (2026-10-22): a body holding `#if DEBUG` / `#endif` is reported by RH5412 today without a fix; the issue's "already holds" claim for that case is wrong. SHARED-DIAG-008 on `main` now states that such a body is reported, without a fix.

## Maintainer answers

| # | Question | Answer |
|---|---|---|
| 1 | Escalation 8: split #1275 and #1837? | A — split; this run is #1275 only |
| 2 | Batch 1, Q1: RH5412–RH5416 report `};` forms (A–D incl. `}⏎;` and trailing comment) and fix to `class X;` / `class D; // note` | 1A |
| 3 | Batch 1, Q2: RH5418 union same shape reported and fixed to `union U(int, string);` | 2A |
| 4 | Batch 1, Q3: comment or directive (`/* */`, `//`, `#region`/`#endregion`) between `}` and `;` → reported, no fix | 3A |
| 5 | Batch 1, Q4: formatter converts the same shapes; leaves comment/directive-between case as written | 4A |
| 6 | Batch 1, Q5: other formatter-backed fixes (e.g. RH5404) convert nested `class I { };` to `class I;` | 5A |

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Drop issue goals: directive bullets 2–3, RH5416 test change, rule-page update, SHARED-TRAIL-002/DIAG-008 refs | Dropped | Bullet 2 contradicts accepted SHARED-DIAG-008; bullet 3 already holds and is pinned; rule pages already cover the form | Directive behavior stays as on `main`; no output changes | 9af7865 |
| 2 | Skeptic | Limit SHARED-EMPTY-003 comment sentence to a brace with no `;` behind it; cite #1169 Q2 instead of "#912 Q2" in change doc | Take both; route to spec author with the batch-1 answers | Removes a reading conflict with SHARED-EMPTY-005 | Wording only; output unaffected | 9e559c8 |
| 3 | Skeptic | Declare unsupported / refuse? | Neither; withhold fix for comment/directive between `}` and `;` (cheap trivia check) | Declaring deletes no special case | Withholding is a rare-class refusal; reported case asked as output question | — |
| 4 | Test implementer | Formatter tests for comment/directive between `}` and `;` pin order and idempotency, not columns | Accept | FMT-STRUCT-018 does not decide the kept `;`'s column; that indentation is follow-up candidate 2 | Output there unchanged from `main` | 49f88f0 |
| 5 | Implementer | RH5404 nested test, CRLF row: input has no line break, so the formatter uses `Environment.NewLine` and the row cannot pass on Linux | Test is wrong; test implementer gives the CRLF row an input with a line break (or drops the row if no line break fits the case) | FMT-PIPE line-ending detection is unchanged and decides it | Expected output unchanged; only the test input changes | 715d3df |
| 6 | Implementer | RH5412 Fix All test with `class A { }; class B { };` on one line needs 2 iterations; same on `main` without semicolons | Option (b): test puts `class B` on its own line; the two-iteration Fix All is a follow-up (c) | Pre-existing in the shared-line helper under BatchFixer, not caused by this change | Fix result is unchanged; only iteration count of an existing path | 715d3df |

## Follow-up candidates

1. RH5404 skips every type declaration with a semicolon, so `class C { int _x; };` is not reported while the formatter breaks it — 01-spec-author.md
2. Formatter indents a `;` on its own line behind a type's closing brace by four spaces, and with a comment/directive before it adds a blank line and indents both (types with members; empty type with comment before `;`) — 01-spec-author.md, 02-skeptic.md
   - RH5404 follow-up must name the second copy of the has-semicolon check at `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52` — 02-skeptic.md
3. Fix All of RH5412–RH5418 needs two iterations when two reported declarations share a line (with or without `;`); both fixes break the same gap via `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` and BatchFixer drops one edit — 05-implementer.md
   - Test note: `UnionWithBracesAndSemicolonIsNotReported` (RH5418 tests:448) must be inverted — 02-skeptic.md

## State

- Last step: spec author 03 (answers recorded, change doc Accepted, 9e559c8).
- Done: output-question batch 1 answered `1A, 2A, 3A, 4A, 5A` (Q1 base forms + fix, Q2 RH5418, Q3 comment/directive between `}` and `;`, Q4 formatter, Q5 nested via other fixes).
- Test implementer 04: tests written (49f88f0, handover 0bc6f0a); new-behavior tests fail as expected.
- Implementer 05: test disputed (9ee01ab, handover bed96da); all other tests pass.
- Test implementer 06: decisions 5, 6 done (715d3df).
- Cleaner 07: clean (0d2d025).
- Next: spec checker (08).
