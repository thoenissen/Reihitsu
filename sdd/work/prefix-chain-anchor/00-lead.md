# Lead — prefix-chain-anchor

- Issue: thoenissen/Reihitsu#896 — "Align a wrapped first call after a property prefix under the prefix's last dot". Label `agent-filed`: its goals, examples, and proposed solution are proposals.
- Slug: `prefix-chain-anchor`
- Branch: `claude/lucid-mccarthy-oamuxd` (session-designated)
- Draft PR: thoenissen/Reihitsu#1891
- Lane: **behavior change** — the issue requests changed formatter output (the anchor column of a wrapped chain link after a property prefix) and, through the shared anchor, changed RH5201 diagnostics. Needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Skeptic's follow-up candidate: formatter does not break in front of a chain link whose operator stands behind the `*/` of a multi-line comment that starts behind code (FMT-BREAK-019) | Dropped, not filed | Only a rare input class (a comment spanning lines inside a chain) is served; `AGENTS.md`, *Code-fix coverage* / lead.md *Follow-ups* | Nothing changes; the construct stays as main writes it | — |
| 2 | Skeptic | Skeptic's internal findings (RH5112-003 text, RH5201-002 `Why:`) | Route to the spec author together with the answer to the `*/` reading question | Spec wording only | Spec text is internal | — |

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, toolchain prepared.
- Last step finished: skeptic (proceed), handover 02-skeptic.md.
- Next: output-question batch 1 sent to maintainer (Q1 prefix call, Q2 call-less chain, Q3 `*/` reading); wait; then spec author amendment (RH5112-003, RH5201-002 Why, Q3 wording, Status Accepted), then test implementer.
