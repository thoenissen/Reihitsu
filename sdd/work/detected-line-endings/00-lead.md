# Lead — detected-line-endings

- Slug: `detected-line-endings`
- Branch: `claude/keen-feynman-1ww7fe` (the session's designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1743 (draft claim)
- Base at claim: `846aca462c94c0d85bc19a1d5ad9d78605590c29`
- Lane: **behavior change** (widened from bug fix after Q2=B, Decision 8)

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1695 | yes | bug fix | Node-in-document entry points write back the target's trailing trivia unnormalized (foreign line ending, trailing whitespace behind appended tokens); observed-vs-expected against FMT-PIPE-010 / SHARED-FIX-008. Needs a FMT-PIPE-010 clarification. |
| thoenissen/Reihitsu#1696 | yes | bug fix | RH8402 fix copies the first line's break instead of the detected ending (SHARED-FIX-008). Derived from reading code; to be reproduced. |
| thoenissen/Reihitsu#1707 | yes | bug fix | RH3003 (confirmed) / RH3005 (unconfirmed) fixes turn a CRLF into LF via elastic trivia (SHARED-FIX-006). |
| thoenissen/Reihitsu#1721 | yes | bug fix | Unconfirmed: formatter output for an all-lone-`\r` document mixes `\n` outside literals and `\r` inside; question whether detection (FMT-PIPE-007/019) is wrong. |

All goals are proposals (agent-filed).

## Roles

Bug-fix lane: spec author (for the FMT-PIPE-010 clarification of #1695, and any clarification #1721 needs) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if follow-up candidates exist.

## Maintainer answers

- Batch 1, Q1 (#1695, whitespace at the end of the fixed target's last line): maintainer chose B — always remove it, as document formatting does.
- Batch 1, Q2 (#1696, RH8402 inserted header's line endings): maintainer chose B — use the detected line ending (`\n` for all-lone-`\r`, `Environment.NewLine` for a file without a line break).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/keen-feynman-1ww7fe` instead of `claude/batch-1695-<slug>` | The session environment permits pushes only to that branch | Branch name is internal | claim commit |
| 2 | Skeptic | Accept spec diff ec7c0c6 (FMT-PIPE-010 line ending)? | Accept; wording notes (a) "when it ends in a line break" to be applied with the Q1 amendment | States what SHARED-FIX-008 already requires | Clarification to an unchanged requirement | ec7c0c6 |
| 3 | Skeptic | #1695 architecture finding: write-back after last phase restates preparation/cleanup rules | Fix narrowly at the three write-back sites, reusing the existing shared line-ending detection (precedent in `FormatSplitFieldInContext`); no new type, no copy of a phase | Structural move of the write-back would change FMT-PIPE-010's contract for every fix; narrow fix is a few lines | Output follows the clarified spec | – |
| 4 | Skeptic | L1 (blank line lost when the line before the target ends in a lone `\r`) | Drop: rare input class (lone `\r` breaks in a document whose detected ending is `\n`), not a safety rail (no comment/directive/literal/meaning lost) | AGENTS.md Code-fix coverage, rare inputs | No output change | – |
| 5 | Skeptic | Spec author's leading-trivia candidate | Drop | FMT-PIPE-018 already decides it; probe confirmed the code does it | – | – |
| 6 | Skeptic | #1721 premise | Do not change; link `Refs #1721`; PR states the observed output (`\n` outside, lone `\r` inside literal) is what FMT-PIPE-007/FMT-EOL-004 specify and that `ReihitsuFormatterLineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy` already pins it; skip remaining roles for it except spec checker and validator | Specified behavior, agent-filed, nothing visible in question | Output unchanged | – |
| 7 | Skeptic | #1707 RH3005 part does not reproduce | Narrow #1707 to RH3003; RH3005 gets characterization tests as the issue's own Done-when prescribes; keep `Closes #1707` (RH3003 reproduces) | Issue anticipated it; no escalation 4 since the issue's bug reproduces | RH3005 output unchanged | – |
| 8 | Answers | Lane after Q2=B | Widen to behavior change; one change doc for RH8402 (RH8402-002 fix part, RH8402-005); Q1=B recorded as a FMT-PIPE-010 clarification (SHARED-FIX-007/008 span), mentioned in the change doc's visible output changes too if the spec author finds it changes a requirement | RH8402-005 changes an unchanged requirement's output | Maintainer answered both | – |

## Follow-up candidates

- RH5403/RH5404 (FormatNodeInDocumentAsync callers) insert a blank line above a comment that leads their target, against SHARED-FIX-006; carries the output question in 02-skeptic.md (#1695 follow-ups, item 1). Source: 02-skeptic.md.
- (dropped, Decision 5) FMT-PIPE-018 writes back the target's leading trivia with its original line endings; SHARED-FIX-008's span names trailing trivia only (e.g. RH5111 fix, LF blank line above target in a CRLF document). Source: 01-spec-author.md.

## State

- Last step finished: maintainer answered batch 1 (1B, 2B).
- Next: spec author (continued) writes change doc + spec amendments; lane check; then test implementer.
