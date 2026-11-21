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

3. After CI repair round 1 (tests moved to their own class), the maintainer asked why a text change needs tests and chose **A**: remove the new test class `RH5201MemberAccessChainWordingTests` entirely; keep only the one-line update of the existing message test in `RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs`. Reason: title and message are already pinned by the architecture tests (title vs rule page heading and README row, message vs title); the code-fix title is a resource string visible in the diff.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Spec title, type names, other rules' "method chain" wording | Accept the spec author's choices: spec title follows the rule title; type names stay (code-fix equivalence key); other rules' pages and FMT-INDENT-029 untouched | Internal or outside #1900's scope | Spec title and type names are internal; other pages unchanged | 8aed075 |

## Follow-up candidates

1. Process change (squad), requested by the maintainer after this run (00-lead.md, maintainer answer 3 and the follow-up discussion). Source: lead, from this run's history (02-skeptic.md test list, 10-ci.md CI failure). One issue with four proposals:
   - **Wording lane**: a run whose diff only changes diagnostic titles/messages, code-fix titles, or rule-page text runs a short lane (spec author for text, spec checker, validator); no skeptic, test implementer, implementer split, or auditor unless a new test is needed. Evidence: this run and the RH5112/RH5048 wording run (#897, PR #1898) found nothing in the large-tier skeptic/auditor; the lane check classifies `.resx` as production, forcing the behavior-change lane.
   - **Coverage first**: skeptic and test implementer check whether existing tests already pin a value (e.g. `AnalyzerPackageMetadataTests` pins title vs rule-page heading/README row and message vs title) before requesting or writing a test; "no new test" is a valid outcome. Evidence: this run added three tests, two duplicating existing coverage, later removed at the maintainer's request.
   - **Cheap self-hosting check**: the default local run excludes `Reihitsu.SelfHosting.Test` (`scripts/README.md`, default test run); when a run adds lines to a test file, the validator runs the RH2111 self-hosting row (`scripts/test.sh --project selfhosting --filter ...`). Evidence: CI failed on RH2111 (test class at 3015 > 3000 lines), caught only in CI, costing a full repair round.
   - **Fewer pushes/notifications**: the lead pushes handover commits less often (stage boundaries only), since every pushed `[skip ci]` commit fires check-suite events on the PR.
   The squad never tunes itself (squad/notes/README.md); the issue proposes, the maintainer decides.

## State

- Audited SHA (repair round 1): 7107bb8 (14-auditor pass). Validator of round 1 stopped: superseded by maintainer answer 3.
- Next: test implementer removes the new test class (answer 3) → cleaner → auditor (repair only) → validator incl. selfhosting → run note (update: repair round, answer 3) → delete sdd/work → CI trigger.
