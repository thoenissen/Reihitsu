# Lead — prefix-chain-anchor

- Issue: thoenissen/Reihitsu#896 — "Align a wrapped first call after a property prefix under the prefix's last dot". Label `agent-filed`: its goals, examples, and proposed solution are proposals.
- Slug: `prefix-chain-anchor`
- Branch: `claude/lucid-mccarthy-oamuxd` (session-designated)
- Draft PR: thoenissen/Reihitsu#1891
- Lane: **behavior change** — the issue requests changed formatter output (the anchor column of a wrapped chain link after a property prefix) and, through the shared anchor, changed RH5201 diagnostics. Needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: none beyond the issue.

## Maintainer answers

Batch 1 (before test implementer), answer "as recommended" = 1A, 2A, 3A:
- 1A: a wrapped link after a prefix of property reads aligns under the last link in front of the first wrapped link (`x.Items.Length` ⏎ `.ToString()` under `.Length`); also later prefix wraps and conditional links (`d`, `g` examples); RH5201 reports/fixes accordingly, RH5112/RH5111 fixes write those columns, rule page updated.
- 2A: a chain without any call follows the same rule (`order.Customer.Address` ⏎ `.City` under `.Address`).
- 3A: code reading of "starts a line" — a line break inside the text of a delimited comment does not count; `x.Next /* c` ⏎ `*/ .Next` ⏎ `.Self()` puts `.Self()` under the `.` of the `.Next` behind `*/`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Skeptic's follow-up candidate: formatter does not break in front of a chain link whose operator stands behind the `*/` of a multi-line comment that starts behind code (FMT-BREAK-019) | Dropped, not filed | Only a rare input class (a comment spanning lines inside a chain) is served; `AGENTS.md`, *Code-fix coverage* / lead.md *Follow-ups* | Nothing changes; the construct stays as main writes it | — |
| 3 | Spec author amendment | Should tests pin `main`'s `*/.Next` join for the `q` shape (answer 3A)? | No. Test case `q` only through the anchor: RH5201 reports nothing on the 3A layout, and a unit test of the anchor selection if one fits. No full formatter-output test pins the join | The join departs from unchanged SHARED-CHAIN-005/FMT-BREAK-019 (rare shape, decision 1); pinning it would lock a deviation in | The maintainer chose the column in 3A; the join is `main`'s unchanged output, not asserted | — |
| 2 | Skeptic | Skeptic's internal findings (RH5112-003 text, RH5201-002 `Why:`) | Route to the spec author together with the answer to the `*/` reading question | Spec wording only | Spec text is internal | — |

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, toolchain prepared.
- Last step finished: skeptic (proceed), handover 02-skeptic.md.
- Batch 1 answered. Spec amendment done (03-spec-author.md, change doc Accepted).
- Test implementer done (04-test-implementer.md, ce21c7d).
- Implementer done (05-implementer.md, 004d8f4).
- Cleaner done (06-cleaner.md, 90831d1): four repository chains rewritten so the shipped analyzer binaries under binaries/ (old anchor) and the new anchor agree; Release build clean.
- Spec checker: consistent (07-spec-checker.md). main not ahead (9a89c85), build green.
- Next: auditor at base 9a89c85.
- Next (was: output-question batch 1 sent to maintainer (Q1 prefix call, Q2 call-less chain, Q3 `*/` reading); wait; then spec author amendment (RH5112-003, RH5201-002 Why, Q3 wording, Status Accepted), then test implementer.
