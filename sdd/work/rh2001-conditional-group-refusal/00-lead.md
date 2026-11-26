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
| 1 | Skeptic | A group that wraps the property itself (`#if NET` / property / `#endif`), fixed today, is withheld under the new RH2001-010 but missing from the change doc | Covered by the maintainer's answer A ("any `#if` group directly in front of the property ... whatever the group holds"); no new output question. Spec author adds it to the change doc's *Visible output changes*; test implementer pins it (with and without initializer) | The answer's wording names the shape literally; the same shape without a modifier is already refused today (RH2001-009) | Covered by a maintainer answer | (pending) |
| 2 | Skeptic | Issue names helpers `WrapsWholeUnitsOf` / `ReadsAsWholeUnits` type-body arm; code has `WrapsWholeMembers` / `ReadsAsWholeMembers` (one arm) | Delete the helpers as they exist (`ConditionalGroupsWrapWholeMembers`, `ParseInTypeBody`, `WrapsWholeMembers`, `GetExcludedText`, `ReadsAsWholeMembers`) | Same intent as the issue; names in the issue were inexact | Internal API | (pending) |
| 3 | Spec author | Accept change doc | Accepted after the skeptic and the amendment in decision 1; status set with the amendment | No open output questions; answer A covers the visible changes | Covered by a maintainer answer | (pending) |
| 4 | Lead | Continue or spawn fresh spec author for the amendment | Fresh (context growth 86k, cache cold) | lead.md section 4 table | Internal | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Output-question batch before test implementer: nothing open (answer A covers all), no stop.
- Next: fresh spec author for change-doc amendment + Status: Accepted, then test implementer.
