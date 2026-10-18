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
