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
| 1 | skeptic | #1847: which `ApplyRule` paths get the non-refusal check | Both `ApplyRule` branches (round-trip and syntax-error) and `FormatWithPipeline`, beyond the issue's list | The skeptic showed all five helpers pass silently on a refused canonical input on `main` | Test code only | |
| 2 | skeptic | #1847: demonstration fixture and the goal to move refusing fixtures to `AssertUnsupportedFor` | Demonstrate on a canonical refused input (skeptic's `private int /* comment */ _value;` class), not the fixed-buffer fixture; drop the move goal | The fixed-buffer fixture already fails on `main` via FMT-PIPE-027; no `ApplyRule` fixture is refused today (5738/5738 with the check) | Test code only | |
| 3 | skeptic | #1848: how the analyzer test project reuses the parse-and-run and round-trip steps | Option B': a `public` entry in `Reihitsu.Formatter.Test` whose signature names no internal type (like `PhaseStabilityCheck`), owning parse, run, round-trip checks and the non-refusal check once; fall back to option A (`InternalsVisibleTo` in the test project's `AssemblyInfo.cs`) only if B' cannot be built without exposing internal types | Matches the existing public-entry convention, keeps the grant surface narrow, stays in the tests lane; C and D widen the lane to tooling for nothing | Test code only | |
| 4 | skeptic | #1848: dead `transformParseOptions` parameter of `VerifyFormatterFixCore` | Remove it as part of the restatement cleanup | `null` at every call site; the run rewrites that method anyway | Test code only | |
| 5 | skeptic | Order of work | One owner for parse-run-check (#1848) carries the non-refusal check once; the analyzer side's own `Assert.IsNull(result.Unsupported)` goes away when the shared entry covers it | One statement of the rule | Test code only | |

## Follow-up candidates

None yet.

## State

- Audited SHA: n/a (no auditor in this lane)
- Last step: skeptic (`01-skeptic.md`): #1847 proceed narrower, #1848 proceed; no output questions, no follow-up candidates.
- Next step: test implementer.
