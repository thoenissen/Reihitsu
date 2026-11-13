# Lead — brace-fix-layout-line-endings

| | |
|---|---|
| Issues | thoenissen/Reihitsu#1416, #1388, #1627, #1142 (prompt order) — all four `agent-filed`: goals are proposals |
| Slug | brace-fix-layout-line-endings |
| Lane | bug fix |
| Branch | claude/laughing-wozniak-azmh7x |
| PR | #1691 (draft claim) |
| Base | origin/main c2866eaf294a9040b995a2b75de7587bdccc5efe |

## Lane per issue

- #1416 — RH5405–RH5407 fixes keep the wrapped statement's layout. Body says behavior change, but the maintainer's triage comments (2026-10-24) move it to the bug-fix lane, no change doc: SHARED-FIX-007 already requires it; SHARED-BRACE-002 is aligned to SHARED-FIX-007 as a clarification. Later comments: `else if` brace column still open here (decided by SHARED-FIX-007: column of `else`, must come from the formatter, not a prediction in the fix); `if (a) M(a, b); /* c */ b++;` leaves `b++;` behind the inserted `}` — whether it starts its own line is part of this issue's layout question.
- #1388 — fixes keep the document's line ending (RH5405, RH5011, RH8304, `FormatNode` via RH5401, RH5406 in lone-`\r` documents, RH7309 moved blocks, RH7004 with comment/directive before the next statement). Triage: bug fix against SHARED-FIX-008 / FMT-PIPE-007, no change doc; RH5011-004 and RH8304-004 aligned to SHARED-FIX-008 as a clarification.
- #1627 — RH5405–RH5407: trailing whitespace behind the body's trailing comment ends up behind the inserted `}`. Bug fix against SHARED-FIX-007.
- #1142 — attribute-list split fixes (RH5502…RH5531) change only the reported list and keep the line ending. Bug fix against SHARED-FIX-006 / SHARED-ATTR-017.

## Roles

Bug-fix lane: spec author (clarifications: SHARED-BRACE-002, RH5011-004, RH8304-004, SHARED-ATTR-017 if needed) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.

## Maintainer answers

_None yet._

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: playbook prescribes `claude/batch-1416-…`; the session mandates `claude/laughing-wozniak-azmh7x` | Use the session branch | Session environment allows pushing only to that branch | Branch name only | claim commit |
| 2 | Claim | Lane of #1416 and #1388 (bodies say behavior change) | Bug fix | Maintainer triage comments on both issues moved them to the bug-fix lane with spec clarifications | Lane only | — |

## Follow-up candidates

_None yet._

## Worklist / next step

- Done: claim, draft PR #1691, prepare.sh.
- Next: spec author (clarifications), then skeptic.
