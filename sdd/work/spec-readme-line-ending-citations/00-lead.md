Role: Lead
Commit: 3dde060
Verdict: docs lane claimed for both issues

# Run

- Issues, in prompt order: thoenissen/Reihitsu#1401, thoenissen/Reihitsu#1377. Parent tracker: thoenissen/Reihitsu#899.
- Slug: `spec-readme-line-ending-citations`.
- Branch: `claude/sweet-pasteur-mcie4f` (the session's designated branch, used instead of the playbook's `claude/issues-1401-1377-...` name). Base: `main`. Draft PR: thoenissen/Reihitsu#1414.

# Lane: docs (both issues)

- #1401: text-only updates to `sdd/README.md` ("Tests and requirements", "Enforced today") and to non-goal wording in `sdd/specs/formatter/` specs. No behavior, no requirement change.
- #1377: citation and wording changes in existing requirements plus `Uses` lines. No requirement changes meaning.

Roles run: spec author (writes all text), spec checker. Skipped: skeptic, test implementer, implementer, cleaner, auditor (docs lane). Validator: only if a compiled file changes; the lead still runs SpecConsistencyTests, SpecStructureTests and RequirementCoverageTests because the maintainer's done criteria require them green.

# Maintainer's clarifications (summarized)

- #1401: bring the "Tests and requirements" section and its "Enforced today" list in line with `RequirementCoverageTests` as it stands on main (formatter/ and playground/ are enforced since #939; Formatter.Test and Playground.Test carry `RequirementAttribute`). Reword the formatter-spec non-goals that call #951, #953, #956, #958, #959, #960, #976 open to their decided state, as the issue describes.
- #1377: besides the seven requirements in the issue body, a round-10 decision adds RH5011-004 and RH8304-004 to the list. P35: a fix writes the line ending FMT-PIPE-007 detects, a lone CR counts as LF; so the SHARED-ATTR-008/-017 wording does not differ from SHARED-FIX-008 and needs no open question (round-9 comment on #1377). Citation and wording only; where a requirement's text says something SHARED-FIX-008 does not, keep that part and report it.
- Must not: change code, tests or behavior; add, retire, move or renumber a requirement ID; edit AGENTS.md (#1402 runs in parallel there); anticipate #998's later change to "Tests and requirements".
- A docs-lane diff cannot add a requirement to an enforced spec; if one seems needed, it is an escalation.

# Open question issues on touched specs

- thoenissen/Reihitsu#1408 (shared/code-fixes.md): its only question is decided by P26 and concerns brace-fix layout, not line endings. Does not affect this run.
