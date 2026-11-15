# Lead — detected-line-endings

- Slug: `detected-line-endings`
- Branch: `claude/keen-feynman-1ww7fe` (the session's designated branch; see Decisions #1)
- PR: thoenissen/Reihitsu#1743 (draft claim)
- Base at claim: `846aca462c94c0d85bc19a1d5ad9d78605590c29`
- Lane: **bug fix** (all four issues)

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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/keen-feynman-1ww7fe` instead of `claude/batch-1695-<slug>` | The session environment permits pushes only to that branch | Branch name is internal | claim commit |

## Follow-up candidates

- FMT-PIPE-018 writes back the target's leading trivia with its original line endings; SHARED-FIX-008's span names trailing trivia only (e.g. RH5111 fix, LF blank line above target in a CRLF document). Source: 01-spec-author.md.

## State

- Last step finished: spec author (01, commit ec7c0c6). Open output questions: Q1 (#1695 trailing whitespace, rec. A), Q2 (#1696 RH8402 ending, rec. B -> change doc + lane widening).
- Next: skeptic, then output-question batch before the test implementer.
