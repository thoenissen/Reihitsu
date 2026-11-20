# Lead — rh5112-rh5048-member-access-wording

- Issue: thoenissen/Reihitsu#897 — `agent-filed` (goals are proposals)
- Branch: `claude/loving-albattani-9bcmvq` (session-designated); draft PR #1898
- Slug: `rh5112-rh5048-member-access-wording`
- Lane: behavior-change — the issue requests changed diagnostic titles/messages (and possibly code-fix titles and rule-page headings) of RH5112 and RH5048. Resource files under `src/` are production paths, and diagnostic text is visible output, so the change needs a change doc.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Change doc although no requirement changes? | Keep it | Diagnostic and code-fix text are visible output; the change doc records the decision | Internal: process artifact | 2523a56 |
| 2 | Skeptic | Issue goal "update localized resources" | Drop | No localized resources exist | Nothing to change | — |
| 3 | Skeptic | Rename `...FirstCall...` types? | Keep names | Provider type name is the equivalence key; no benefit | Type names are internal | — |

## Follow-up candidates

- RH5201 ("Method chains should be aligned.", fix "Align method chain") reports call-less chains too; same wording problem. Carry the output question; skeptic recommends "Member-access chains should be aligned." / "Align member-access chain". Source: 02-skeptic.md

## State

- Last step: spec author (2523a56), skeptic (ad95a49, proceed). Lane check confirmed.
- Next: output-question batch 1 (title/message RH5112, RH5048; RH5112 fix title; rule-page body text; RH5048 "Why" wording) — waiting for maintainer. Then route answers to spec author, then test implementer.
