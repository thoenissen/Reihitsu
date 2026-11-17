# Lead — formatter-test-helper-reuse

- Issues: thoenissen/Reihitsu#1847 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#1848 (`agent-filed`, goals are proposals)
- Branch: `claude/squad-1847-1848-ifrr2q` (session-designated); draft PR thoenissen/Reihitsu#1853
- Slug: `formatter-test-helper-reuse`
- Lane: tests
  - #1847: test-helper code only (`FormatterTestsBase.ApplyRule` assertions in `Reihitsu.Formatter.Test`) plus a test that shows the check; no production change, no visible output.
  - #1848: test-helper code only (`VerifyFormatterFixCore` in `Reihitsu.Analyzer.Test` reusing the formatter test project's parse-and-run and round-trip steps); no production change, no visible output.
  - Shared mechanism: both are the formatter test helpers' parse-run-check path (`ApplyRule`, `PhaseTreeRoundTrip`). A `*.csproj` change (shared helper project, linked sources) is tooling under the lane check and would widen the lane tests → tooling.
- Roles run: skeptic (premise and design check of the helpers), test implementer (writes the helper changes and tests), cleaner, spec checker, validator.
- Roles skipped: spec author (no spec is added; the run changes no specified behavior), implementer (no production code), auditor (not part of the tests lane).
- Maintainer clarifications: triage comments on both issues plan them for 1.4.0 as one run, to start after #1732 merges (it has, via #1839).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## State

- Audited SHA: n/a (no auditor in this lane)
- Last step: claim, lane, `00-lead.md`.
- Next step: skeptic.
