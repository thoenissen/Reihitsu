Role: Lead
Commit: d83f2ae0b88a68fc626936144a7d7b1e01b861a4
Verdict: lane claimed — tests

# Run

- Issue: thoenissen/Reihitsu#986 — analyzer-spec backfill, part of thoenissen/Reihitsu#899.
- Slug: `spacing-specs-part-1`
- Branch: `claude/beautiful-galileo-9w815j` (the session's designated branch); claim PR thoenissen/Reihitsu#1036 (draft).

# Lane

**Specs and tests for existing behavior.** The issue asks for specs of existing rules and for tagging and, where missing, adding tests of their current behavior. No production code changes.

Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production code in this lane), auditor (the lane table does not run it).

# Scope

One spec per rule under `sdd/specs/analyzer/`, from `sdd/templates/analyzer-spec.md`:
RH5601, RH5602, RH5603, RH5604, RH6001, RH6003, RH6004, RH6005, RH6006, RH6007, RH6008, RH6009, RH6010, RH6011, RH6012 (15 rules).

Plus: new IDs in `sdd/specs/requirement-ids.txt`, new specs in `sdd/specs/README.md`, existing tests tagged with `[Requirement("<ID>")]`, a focused test only where a requirement has no covering test.

# Maintainer clarifications (summarized)

- Read first: `AGENTS.md`, `sdd/README.md`, `sdd/templates/analyzer-spec.md`, `sdd/specs/analyzer/RH6002.md` (the only spacing rule specified so far, the reference example), `sdd/specs/formatter/horizontal-spacing.md` (FMT-SPACE), `sdd/specs/formatter/cleanup.md` (FMT-CLEAN).
- Where the formatter already owns the policy a rule checks, the analyzer spec references the `FMT-…` ID instead of restating it.
- A shared spec for the whitespace-run removal fix (`RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase`) is created only if more than one spec relies on the same requirement. If it is created, design it so the next batch (RH6013–RH6024, thoenissen/Reihitsu#987) can reuse it.
- Every example is verified against a test or by running it.
- Current behavior only: output that looks wrong is not written into a spec (no requirement, non-goal, or note); it is filed as an open question per `sdd/README.md` (sub-issues of thoenissen/Reihitsu#912). Report each such finding in your handover; the lead files them.
- Known formatter bugs thoenissen/Reihitsu#957, #958, #959, #960 are not re-filed. Where a rule checks the same policy as one of those bugs, leave those cases out of the spec.
- No existing requirement of another spec (including FMT-SPACE, FMT-CLEAN, RH6002) is changed without the maintainer's approval — report it as a finding instead.
- Other squads (thoenissen/Reihitsu#979, #980, #982) edit `sdd/specs/README.md` and `sdd/specs/requirement-ids.txt` concurrently; on a merge both sides' rows are kept.
- Done: 15 specs, tagged tests, focused tests only where needed, examples verified, open questions filed, CI green, PR based on main without `sdd/work/`.

# Lane widened — tooling

After the spec author's third pass, `scripts/lane-check.sh tests --head worktree` reported a mismatch: the lead's decisions entry `squad/decisions/2026-10-18-rh6002-003-why-left-unchanged.md` (required by `squad/decisions/README.md` for every answered escalation) is classified as tooling. `scripts/lane-check.sh tooling --head worktree` confirms. The implementer had not started, so the lane widens along docs → tests → tooling.

Roles now: spec author (ran), skeptic (ran), test implementer → cleaner → spec checker → auditor → validator. The implementer is skipped: the run changes no production or tooling code; the only tooling path is the lead's decisions entry.

# Repair round (auditor `09-auditor.md`, audited head 96ed3201fb5c23998e8d73d0557664b7f6e5bc28)

All nine findings concern text this PR introduced (spec requirements and one test tag), so all stay in this PR. Owners:

| # | Finding (see 09-auditor.md) | Owner |
|---|---|---|
| 1 | RH5601-001 claims unreported tabs (continuation `///` indentation; tab after a mid-line `/** */` before a literal) | spec author |
| 2 | RH6001-002 contradicts the code for a `/** */` comment between keyword and `(` | spec author |
| 3 | RH6004-002 contradicts the code for `# region` / `# endregion` and settles thoenissen/Reihitsu#1043 | spec author |
| 4 | RH6001-001, RH6005-001, RH6011-001 claim the `/** */` case and a false Why | spec author |
| 5 | RH5603-001's `//` branch admits a final directive line with a trailing comment (thoenissen/Reihitsu#1040's shape) | spec author |
| 6 | RH5601-002, RH5602-001 wording on interpolation holes | spec author |
| 7 | RH6003-001, RH6008-001, RH6012-001 Whys miss FMT-SPACE-014 | spec author |
| 8 | RH6009-001, RH6010-001 wording reads as claiming `/** y */{` | spec author |
| 9 | `VerifyFirstEncounteredLineEndingWinsWhenCountsAreTied` tagged RH5604-001 although its input cannot tell the tie rules apart | test implementer |

Behavior left out of a spec by this round is an open question. Questions for a spec that already has an issue (thoenissen/Reihitsu#1038–#1051) are added there by the lead; others get a new issue. The auditor's non-blocking hints are out of scope (follow-up candidates for the maintainer).

After the owners: cleaner → spec checker → auditor (repair only) → validator. A second block is escalation 7.
