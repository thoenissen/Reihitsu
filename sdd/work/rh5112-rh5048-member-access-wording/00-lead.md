# Lead — rh5112-rh5048-member-access-wording

- Issue: thoenissen/Reihitsu#897 — `agent-filed` (goals are proposals)
- Branch: `claude/loving-albattani-9bcmvq` (session-designated); draft PR #1898
- Slug: `rh5112-rh5048-member-access-wording`
- Lane: behavior-change — the issue requests changed diagnostic titles/messages (and possibly code-fix titles and rule-page headings) of RH5112 and RH5048. Resource files under `src/` are production paths, and diagnostic text is visible output, so the change needs a change doc.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.
- Maintainer clarifications: none.

## Maintainer answers

Batch 1 (before test implementer), answered "as recommended":
1. RH5112 title/message → A: "Wrapped chains should keep the first member access on the original line." (period kept)
2. RH5048 title/message → A: "Comments must not be placed before the first member access of a wrapped chain" (no period)
3. RH5112 code-fix title → A: "Keep first wrapped member access on original line"
4. RH5112 rule-page body text → A: "call" wording replaced by "member access"/"chains" as drafted in 2523a56
5. RH5048 rule-page "Why" → B: "The first member access of a chain belongs on the line its receiver ends on. A comment in front of it keeps the member access apart from its receiver."

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
- Auditor PASS at ab73ac6; validator PASS (5b1f34c); follow-up #1900 (RH5201). Next: note, delete sdd/work, trigger, PR.
- Was: output-question batch 1 (title/message RH5112, RH5048; RH5112 fix title; rule-page body text; RH5048 "Why" wording) — waiting for maintainer. Then route answers to spec author, then test implementer.
