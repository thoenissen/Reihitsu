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

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Drop issue goals: directive bullets 2–3, RH5416 test change, rule-page update, SHARED-TRAIL-002/DIAG-008 refs | Dropped | Bullet 2 contradicts accepted SHARED-DIAG-008; bullet 3 already holds and is pinned; rule pages already cover the form | Directive behavior stays as on `main`; no output changes | 9af7865 |
| 2 | Skeptic | Limit SHARED-EMPTY-003 comment sentence to a brace with no `;` behind it; cite #1169 Q2 instead of "#912 Q2" in change doc | Take both; route to spec author with the batch-1 answers | Removes a reading conflict with SHARED-EMPTY-005 | Wording only; output unaffected | pending |
| 3 | Skeptic | Declare unsupported / refuse? | Neither; withhold fix for comment/directive between `}` and `;` (cheap trivia check) | Declaring deletes no special case | Withholding is a rare-class refusal; reported case asked as output question | — |

## Follow-up candidates

1. RH5404 skips every type declaration with a semicolon, so `class C { int _x; };` is not reported while the formatter breaks it — 01-spec-author.md
2. Formatter indents a `;` on its own line behind a type's closing brace by four spaces, and with a comment/directive before it adds a blank line and indents both (types with members; empty type with comment before `;`) — 01-spec-author.md, 02-skeptic.md
   - RH5404 follow-up must name the second copy of the has-semicolon check at `RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52` — 02-skeptic.md
   - Test note: `UnionWithBracesAndSemicolonIsNotReported` (RH5418 tests:448) must be inverted — 02-skeptic.md

## State

- Last step: skeptic 02 (proceed narrower, d1c3c94).
- Waiting: output-question batch 1 (Q1 base forms + fix, Q2 RH5418, Q3 comment/directive between `}` and `;`, Q4 formatter, Q5 nested via other fixes).
- Next after answers: route answers + decision 2 to spec author (continue a3af192561c2bf905 if small, else fresh), status Accepted; then test implementer (03).
