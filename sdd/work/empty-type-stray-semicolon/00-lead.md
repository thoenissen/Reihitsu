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

(none yet)

## State

- Last step: claim, lane, prepare.sh green.
- Next: spawn spec author (01).
