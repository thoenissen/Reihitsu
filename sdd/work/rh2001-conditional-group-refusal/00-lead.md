# Lead — rh2001-conditional-group-refusal

- Issue: thoenissen/Reihitsu#1943 — `agent-filed` (goals are proposals; the maintainer's decision in the issue comment is binding).
- Slug: `rh2001-conditional-group-refusal`
- Branch: `claude/epic-gauss-svuukx` (session-designated); PR thoenissen/Reihitsu#1951 (draft claim).
- Lane: **behavior-change** — the issue requests changed code-fix behavior (RH2001 fix withheld in more cases), which changes a requirement (RH2001-010) and needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications

- Issue comment (2026-11-22), decision **option A**: the RH2001 fix is withheld for any `#if` conditional group directly in front of the property (leading trivia of its first modifier), or directly behind a property without an initializer (leading trivia of the next token); the diagnostic stays. Same answer as for the member-moving ordering fixes (thoenissen/Reihitsu#1668). The excluded-branch parse that only RH2001 still uses is deleted.

## Maintainer answers

- Issue #1943 output question 1 (RH2001, conditional group of whole members next to the property): **A** — no fix offered, diagnostic stays (answered in the issue comment before the run).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author.
