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
- Q7 (batch 2) a second comment between `*/` (of an own-line comment) and the link, `*/ /* y */ .Self()`: A — reported; fix gives `*/ /* y */` ⏎ `.Self();` (same as formatter).
- Q8 (batch 2) multi-line comment starting behind another comment on its own line, `/* w */ /* x` ⏎ `*/ .Self()`: A — reported; fix gives `*/` ⏎ `.Self();` (same as formatter).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Where does the first-call rule live? | In shared SHARED-CHAIN-005, with new RH5201-006 (not a wider RH5201-001) | Formatter and analyzer share the chain layout; a separate requirement keeps tests traceable | Spec structure only; outputs are asked as output questions | 064eb20 |
| 2 | Spec author | Drop issue goal 2 (columns of trivia-kept pieces)? | Drop | Precheck refuses those shapes (FMT-UNSUP-014/015): no caller reaches the case; issue is `agent-filed` | Nothing changes for those inputs; PR states the difference | 064eb20 |
| 3 | Skeptic | Architecture finding FC-015: where does "the root's line reaches the first invoked link" live? | Move the decision into `Reihitsu.Core` `FluentChain` (next to `IsFirstLinkBlocked`/`IsPrefixLink`/`GetAnchorLink`); formatter `ChainLineBreakRewriter` and RH5201 both call it. Inner-break reading follows the maintainer's answer to Q4 | A private copy in the analyzer would be a second statement of one rule; the change stays within the components the run touches and their counterparts, small, no new type | Formatter output unchanged (comment-in-operator inputs are refused by the precheck, so the inner-break reading cannot differ on any formatter input) | — |
| 4 | Skeptic | Adjacent fix: RH5201 misses a link behind the `*/` of an own-line delimited comment when the line starts in the anchor column (skeptic G1, G2) | Take it into the run | Unchanged requirements SHARED-COMMENT-012 and SHARED-CHAIN-005 exception demand it; few lines in RH5201 `IsMisplaced`; fits #893's purpose; gets a test | Restores an unchanged requirement | — |
| 6 | Repair 1 | Test implementer hint: RH5201-002's `*/` paragraph does not itself exclude the chain's first link | Spec author states the exclusion in RH5201-002 | Requirement reads like the code and the corrected summary | Wording only; behavior is the tested, unchanged-from-main exclusion | — |
| 5 | Skeptic | Change doc: list F11; RH5201 spec `Uses` adds SHARED-COMMENT-012 | Route to spec author with the maintainer answers | Completeness / wording | Listing only | — |

## Follow-up candidates

- From `17-auditor.md` (pre-existing on `main`, safety rail: fix convergence): RH5201 judges the column of the chain's first link behind an own-line `*/` by the `*/` line (contrary to SHARED-COMMENT-012); the fix cannot move that line, so when the `*/` line is not in the root column the diagnostic stays and the fix never converges (auditor fixtures y01, y02, y09, y10, LF and CRLF; e.g. `var a = c` ⏎ `        /* x` ⏎ `            */ .Next.Self();`). File as `bug`; name first under PR Review notes.

## Worklist / next step

- Spec author done (064eb20, lane check confirmed behavior-change). Skeptic running → 02-skeptic.md.
- Skeptic done (9fc40a5): output question on comment inside a prefix link operator; architecture finding (decision 3); adjacent fix (decision 4).
- Batch 1 answered (see Maintainer answers).
- Spec author follow-up done (1986bc0, Status Accepted, lane confirmed). It changed RH5201-001's MUST NOT to carry out decision 4 → change doc item 6 is a visible change not covered by an answer.
- Q6 answered A. Test implementer done (4b92d67): 28 RH5201 tests red as expected; two existing tests updated (covered by answer 2A).
- Open worklist for a later spec-author pass: RH5201-006 example 2 and example `g` in the spec declare `C? Next`, which makes `c.Next?.Next` raise CS8602; tests use `C Next` + `C? Opt`. Spec example fixture to align (route with spec-checker findings).
- Implementer done (dde283b, e92a72b): all tests green; decision 3 via `FluentChain.IsRootLineShortOfFirstInvokedLink`; decision 4 via `IsBehindMultiLineCommentTail` moved to Core `SyntaxTokenPositionUtilities` (shared helper, 2 consumers; ~82+/50-).
- Open for batch 2 (before auditor): Q7 — `/* x` ⏎ `*/ /* y */ .Next` (second comment between `*/` and link) is now reported by RH5201 (helper reads wider than RH5201-002's wording); formatter breaks there today.
- Cleaner done (b532773, clean). Q7 outputs verified with apply-fix/format: fix and formatter both give `*/ /* y */` ⏎ `.Self();`.
- Auditor (fc9c9e2) on audited SHA 2ca825e: findings #1 (medium, tests for two untested condition sides) → test implementer; #2 (low, IsMisplaced summary comment) → cleaner. Repair round 1 of 3. Test implementer done (7e11552, 191/191). Cleaner done (9fc036d, text-only proof pass). Spec author done (adb01b0). Spec checker consistent (c158e93). Merged origin/main 77a1f37 (version bumps only) in 06786b3, build green. Auditor repair audit (ca0c87e, audited 6fd338f): #1, #2 closed; new #3 (low, missing `[Requirement("RH5201-002")]` on `VerifyFirstLinkBehindOwnLineCommentTailInRootColumnIsNotReported`). Repair round 2 of 3. NOW: test implementer → 18; cleaner → 19; spec checker → 20; auditor on repair → 21; validator → 22. spec checker → 16; auditor on repair → 17; validator.
- Spec author Q8 pass done (30ddbd7). Spec author Q7 pass done (3f8d20f). It raised Q8: `/* w */ /* x` ⏎ `*/ .Self()` (multi-line comment starting behind another comment on its own line) — branch reports it; spec silent. Q8 answered A. NOW: spec author (clause + example `g`) → 08-spec-author.md; then test implementer (Q7, Q8 tests) → 09-test-implementer.md. Then: spec author adds clause+example `g` if A (wording in 07-spec-author.md), test implementer adds Q7 (+Q8) tests.
- (done) Q7 spec author pass (Q7 wording into RH5201-002/change doc; CS8602 fixture item) → 07-spec-author.md; test implementer adds a Q7 test if none exists. Then: spec author pass (Q7 wording + CS8602 fixture item), spec checker, auditor.
- Then: spec author follow-up (answers, decisions 4–5, Status Accepted), lane check, test implementer.
- Old next: output-question batch from change doc visible output changes 1–7 + skeptic's questions, before test implementer.
