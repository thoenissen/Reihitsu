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

2. Output question 1 of the batch before the test implementer (RH5201 rule page body wording): **A**. "method chain" becomes "member-access chain" in the Description's first sentence and in the Why sentence, and "chain of calls" becomes "chain of member accesses"; the "first call" bullets, "How to fix it", and the examples stay unchanged.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Spec title, type names, other rules' "method chain" wording | Accept the spec author's choices: spec title follows the rule title; type names stay (code-fix equivalence key); other rules' pages and FMT-INDENT-029 untouched | Internal or outside #1900's scope | Spec title and type names are internal; other pages unchanged | 8aed075 |

## Follow-up candidates

(none yet)

## State

- Last step: skeptic proceed (7f40b46, 02-skeptic.md); change doc accepted by the lead (spec author sets Status: Accepted in its next follow-up). Output-question batch asked: Q1 RH5201 rule page body wording (A recommended).
- Answer received (A). Next: fresh spec author applies it and sets Status: Accepted; then test implementer.
