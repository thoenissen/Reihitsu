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

## Follow-up candidates

1. RH5404 skips every type declaration with a semicolon, so `class C { int _x; };` is not reported while the formatter breaks it — 01-spec-author.md
2. Formatter indents a `;` on its own line behind a type's closing brace by four spaces (types with members; empty type with comment before `;`) — 01-spec-author.md

## State

- Last step: spec author 01 (change doc Draft `sdd/changes/2026-11/empty-type-stray-semicolon.md`, commit 9af7865); lane check ok.
- Next: skeptic (02), then output-question batch 1 (change doc visible output changes, incl. RH5418 inclusion and comment-between-`}`-and-`;` reported without fix).
