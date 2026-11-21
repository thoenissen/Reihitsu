# Lead — rh5201-member-access-wording

| | |
|---|---|
| Issues | thoenissen/Reihitsu#1900 (`agent-filed`: goals are proposals) |
| Slug | rh5201-member-access-wording |
| Branch | claude/determined-carson-gm4pow |
| PR | #1902 (draft) |
| Lane | behavior change |

## Lane and reason

#1900 changes visible text of RH5201: the diagnostic title and message (analyzer resources), the code-fix title (code-fix resources), and the rule page heading plus every place a title-matching architecture test pins. No reporting condition, location, or fix output changes. A change to visible diagnostic text is a behavior change and gets a change doc, as the precedent run for RH5112/RH5048 (#897, PR #1898, change doc `sdd/changes/2026-11/` member-access wording) did.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates arise. None skipped.

## Maintainer answers

1. #1900 output question (RH5201 title/message/fix title), answered in the issue's triage comment of 2026-11-20 by the maintainer: **option A**. Title and message become "Member-access chains should be aligned." and the code-fix title becomes "Align member-access chain". The rule page heading and every place the title test pins follow. No reporting condition, location, or fix output changes. (The maintainer also notes the run for #893 starts after this one merges; both edit RH5201's files — not part of this run.)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, draft PR #1902, prepare.sh green, 00-lead.md written.
- Next: spec author.
