Role: Lead
Commit: ede10b3bd9d6867a4c9eb8a5f70bf876550adba8
Verdict: lane claimed — tests

# Run

- Issue: thoenissen/Reihitsu#988 — analyzer-spec backfill batch under thoenissen/Reihitsu#899.
- Slug: `other-blank-line-specs`.
- Branch: `claude/nifty-dijkstra-qq87or` (the session's designated branch; it replaces the `claude/issue-<N>-<slug>` name of `squad/lead.md`, because this session may push only to that branch). Claim PR: thoenissen/Reihitsu#1057 (draft).

# Lane

Specs and tests for existing behavior (`tests`): the issue asks for specs of existing rules and their covering tests, with no production change.

Roles run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (the lane does not run it).

# Rules in scope (12)

RH5011, RH5020, RH5022, RH5023, RH5024, RH5025, RH5026, RH5027, RH5028, RH5030, RH5031, RH5032.

# Maintainer's clarifications (summarized)

- Reuse `sdd/specs/shared/blank-line-spacing.md` (SHARED-BLANK) and `sdd/specs/formatter/blank-lines.md` (FMT-BLANK). Where the formatter or a shared spec already owns the policy a rule checks, reference its requirement ID instead of restating it. Model for member specs: `sdd/specs/shared/statement-blank-lines.md` with `sdd/specs/analyzer/RH5001.md`.
- The span-removal fix base (RH5022–RH5028) and the region-directive pair (RH5031/RH5032) get a shared spec only if more than one spec relies on the same requirement.
- Do not change an existing requirement of SHARED-BLANK, FMT-BLANK, or SHARED-STMT; a need to do so is reported in the handover, and the lead asks the maintainer.
- Current behavior only. Output that looks defective is left out of the spec and filed as an open question per `sdd/README.md`; the lead files them (sub-issues of thoenissen/Reihitsu#912).
- Already filed, do not re-file: thoenissen/Reihitsu#1000, thoenissen/Reihitsu#1001 (statement-blank-line open questions), thoenissen/Reihitsu#949 (blank line before a comment that leads a terminal `break` — check RH5020 against it and leave that case out of RH5020's spec), thoenissen/Reihitsu#954.
- Every example is verified against a test or by running it. Existing tests get `[Requirement]` tags; a focused test is added only where a requirement has no covering test.
- Other squads concurrently edit `sdd/specs/README.md` and `sdd/specs/requirement-ids.txt`; when main moves, it is merged before the spec checker and both sides' rows are kept.
