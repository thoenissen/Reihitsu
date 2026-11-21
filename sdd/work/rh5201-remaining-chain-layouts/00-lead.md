# Lead — rh5201-remaining-chain-layouts

- Issue: thoenissen/Reihitsu#893 — "Report the remaining chain layouts the formatter changes without a diagnostic". Labels: enhancement, analyzer, `agent-filed` (goals are proposals).
- Slug: `rh5201-remaining-chain-layouts`
- Branch: `claude/busy-cori-4e7jd0` (session-designated). Draft PR: thoenissen/Reihitsu#1907.
- Base at claim: `d63eab4`.
- Lane: **behavior change** — the issue requests RH5201 to report inputs it does not report today (new diagnostics = changed visible behavior), which needs a change doc.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. Roles skipped: none.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Where does the first-call rule live? | In shared SHARED-CHAIN-005, with new RH5201-006 (not a wider RH5201-001) | Formatter and analyzer share the chain layout; a separate requirement keeps tests traceable | Spec structure only; outputs are asked as output questions | 064eb20 |
| 2 | Spec author | Drop issue goal 2 (columns of trivia-kept pieces)? | Drop | Precheck refuses those shapes (FMT-UNSUP-014/015): no caller reaches the case; issue is `agent-filed` | Nothing changes for those inputs; PR states the difference | 064eb20 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Spec author done (064eb20, lane check confirmed behavior-change). Skeptic running → 02-skeptic.md.
- Next: output-question batch from change doc visible output changes 1–7 + skeptic's questions, before test implementer.
