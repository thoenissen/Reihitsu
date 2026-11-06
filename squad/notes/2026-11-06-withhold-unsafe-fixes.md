# withhold-unsafe-fixes

| | |
|---|---|
| Date | 2026-11-06 |
| Run | issues #1136, #1134, #1135, #1262, #1332, #1336, #1340 |
| Pull request | #1481 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | escalation (architecture finding) | RH7306 analyzer and fix judge different text | A (narrow fix, one predicate) | yes |
| 2 | output question | RH7309: backing fields count as layout fields | B (yes) | yes |
| 3 | output question | RH7303: withhold the fix for the mirror of the straddling region | B (withhold) | yes |
| 4 | escalation (scope) | RH7309 regions that `GetTopLevelRegions` miscounts | C (follow-up) | yes |
| 5 | output question | RH7309: comment above a moved block, blank line between | A (withhold) | yes |
| 6 | output question | RH7309: comment above a block that keeps its place | A (offer) | yes |
| 7 | output question | RH7306: partner `#endregion` left without description | B (withhold) | yes |
| 8 | output question | RH7105/RH7106: comment between last modifier and type | A (withhold) | yes |
| 9 | escalation (lane) | Decisions entry under `squad/` fails the bug-fix lane check | widen to behavior change with change docs | no (the lead recommended keeping the entry off the branch) |
| 10 | output question | Approve the visible output changes of both change docs | approved, after `main` was merged | yes |
| 11 | output question | RH7101: doc comment behind code in front of a combined field | A (withhold) | yes |

## Cost

- Stops: one batch of eight questions after the skeptic, a lane escalation, the change-doc approval after a re-check against a moved `main` (29 commits, including a new squad process), and one RH7101 output question. The maintainer first asked for a pause after every role, then withdrew it.
- Repair rounds: 3 of 3. One structural change (single owner of the member-order decision, which also takes the layout check) replaced a repair round after audit 2 found a repeated class.
- Roles spawned more than once: spec author 6×, test implementer 4×, implementer 4×, cleaner 4×, spec checker 4×, auditor 4× — the merge of `main`, three repair rounds, and the structural change.
- Expensive: the first spec author (a long-running agent continued three times before the process moved to fresh agents per follow-up), the skeptic, the first test implementer and implementer, and the auditors.
- CI after publishing: the self-hosting scans failed because decision 15 had put `InternalsVisibleTo` in `Reihitsu.Core.csproj`, which the self-hosting compilation does not read; the local validator does not run those scans. A first narrowing (making the method public again) reopened a class three audits had closed, and the CI audit caught it; the fix moved the attribute to `Properties/AssemblyInfo.cs`. Cost: two implementers, two auditors, two validators.
- The process changed under the run: `squad/decisions/` was removed on `main` mid-run; the two entries the run had written moved into its lead handover.

## Usage

The output of `scripts/usage.sh` is not pasted here: this session's environment forbids pushing model identifiers to the repository, and every row of the script's output names a model. The lead reported the totals to the maintainer in the session.

## Decisions taken

19, listed in the pull request's run report. The ones the lead would have liked the maintainer to see: decision 12 (structural change after a repeated audit class, RH7103 taken in as an adjacent fix) and decision 11 (withheld fixes added after the change-doc approval, judged covered by the approved *Build configurations* condition).

## Suggestions

- `scripts/lane-check.sh` classifies `squad/notes/` as tooling, so every bug-fix run that writes its note fails the lane check unless it widens to behavior change. A note is the lead's own record, like `sdd/work/`; excluding it from the classification would keep the bug-fix lane usable.
- The self-hosting scans are the one check the local validator skips, and the only one that failed in CI; a quick self-hosting compile of changed projects before publishing would have caught it.
- The usage section requires model identifiers, which some session environments forbid pushing.
