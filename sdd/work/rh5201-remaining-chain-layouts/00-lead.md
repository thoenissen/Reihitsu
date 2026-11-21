# Lead — rh5201-remaining-chain-layouts

- Issue: thoenissen/Reihitsu#893 — "Report the remaining chain layouts the formatter changes without a diagnostic". Labels: enhancement, analyzer, `agent-filed` (goals are proposals).
- Slug: `rh5201-remaining-chain-layouts`
- Branch: `claude/busy-cori-4e7jd0` (session-designated). Draft PR: thoenissen/Reihitsu#1907.
- Base at claim: `d63eab4`.
- Lane: **behavior change** — the issue requests RH5201 to report inputs it does not report today (new diagnostics = changed visible behavior), which needs a change doc.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. Roles skipped: none.
- Maintainer clarifications: none.

## Maintainer answers

Batch 1 (before test implementer), answered "as recommended: 1A 2A 3A 4B 5A":
- Q1 first call behind a wrapped prefix link (incl. prefix behind own-line `*/`, F11): A — report on the first call's `.`, fix gives formatter layout.
- Q2 first call behind a first link held by a comment or directive below the root (change doc 2, 3, 4): A — report; with `#if`, only the active branch is reported and fixed.
- Q3 chains already reported for another link: A — one more diagnostic on the first call; fix result unchanged.
- Q4 comment-held line break inside a prefix link's operator (`c.Opt? // keep` ⏎ `.Next.Self()`): B — not reported (counted as RH5201-003 counts it, `HasInnerLineBreak(true)`); sibling first-link case stays unreported.
- Q5 rule page: A — the new Description bullet and How-to-fix wording as drafted.
- Q6 (asked after spec follow-up; change doc item 6) link behind the `*/` of an own-line comment, `*/` in anchor column (later call, chain without a call, prefix link): A — report each, fix gives formatter layout. Spec as written in 1986bc0 stands.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Where does the first-call rule live? | In shared SHARED-CHAIN-005, with new RH5201-006 (not a wider RH5201-001) | Formatter and analyzer share the chain layout; a separate requirement keeps tests traceable | Spec structure only; outputs are asked as output questions | 064eb20 |
| 2 | Spec author | Drop issue goal 2 (columns of trivia-kept pieces)? | Drop | Precheck refuses those shapes (FMT-UNSUP-014/015): no caller reaches the case; issue is `agent-filed` | Nothing changes for those inputs; PR states the difference | 064eb20 |
| 3 | Skeptic | Architecture finding FC-015: where does "the root's line reaches the first invoked link" live? | Move the decision into `Reihitsu.Core` `FluentChain` (next to `IsFirstLinkBlocked`/`IsPrefixLink`/`GetAnchorLink`); formatter `ChainLineBreakRewriter` and RH5201 both call it. Inner-break reading follows the maintainer's answer to Q4 | A private copy in the analyzer would be a second statement of one rule; the change stays within the components the run touches and their counterparts, small, no new type | Formatter output unchanged (comment-in-operator inputs are refused by the precheck, so the inner-break reading cannot differ on any formatter input) | — |
| 4 | Skeptic | Adjacent fix: RH5201 misses a link behind the `*/` of an own-line delimited comment when the line starts in the anchor column (skeptic G1, G2) | Take it into the run | Unchanged requirements SHARED-COMMENT-012 and SHARED-CHAIN-005 exception demand it; few lines in RH5201 `IsMisplaced`; fits #893's purpose; gets a test | Restores an unchanged requirement | — |
| 5 | Skeptic | Change doc: list F11; RH5201 spec `Uses` adds SHARED-COMMENT-012 | Route to spec author with the maintainer answers | Completeness / wording | Listing only | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Spec author done (064eb20, lane check confirmed behavior-change). Skeptic running → 02-skeptic.md.
- Skeptic done (9fc40a5): output question on comment inside a prefix link operator; architecture finding (decision 3); adjacent fix (decision 4).
- Batch 1 answered (see Maintainer answers).
- Spec author follow-up done (1986bc0, Status Accepted, lane confirmed). It changed RH5201-001's MUST NOT to carry out decision 4 → change doc item 6 is a visible change not covered by an answer.
- Q6 answered A. NOW: test implementer running → 04-test-implementer.md.
- Then: spec author follow-up (answers, decisions 4–5, Status Accepted), lane check, test implementer.
- Old next: output-question batch from change doc visible output changes 1–7 + skeptic's questions, before test implementer.
