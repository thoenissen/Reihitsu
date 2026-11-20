Role: test implementer
Commit: ce05e95 (tests written on this head; the commit carrying them follows)
Verdict: tests written

Repair round 3: auditor finding #4 (`16-auditor.md`) and the amended RH5419-004 (`17-spec-author.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests.cs`, each under LF and CRLF. The amendment is a clarification of behavior the code already has (auditor B1, B2, B4), so every test passes on this head, as a test for existing behavior must.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifySpecificationExampleOfWithheldFixes` (gains the spec's new `int⏎ this[decimal a,⏎ decimal b] => 1;`) | RH5419-001, RH5419-004 | `VerifyNoCodeFixIsOffered` | passes |
| `VerifyFixIsWithheldForALineBreakBetweenTheTypeAndThis` (new; auditor B2, the test finding #4 asks for) | RH5419-004 | `VerifyNoCodeFixIsOffered` | passes |
| `VerifyFixIsWithheldForALineBreakBetweenAModifierAndTheType` (new; auditor B1) | RH5419-004 | `VerifyNoCodeFixIsOffered` | passes |
| `VerifyFixIsWithheldForALineBreakBetweenAnExplicitInterfaceNameAndThis` (new; auditor B4, `int I.⏎ this[...]`) | RH5419-004 | `VerifyNoCodeFixIsOffered` | passes |

The three new tests cover each position the spec names ("between a modifier, the type, an explicit interface name, and `this`"). The opposite side of the boundary (FC-002, FC-003), a bare line break between `this` and `[` that the formatter joins and that keeps the fix, stays covered by `VerifyFixPutsIndexersOnOneLine` and the offered spec example, unchanged and passing. `VerifyNoCodeFixIsOffered` checks the exact diagnostic markup, so each test also pins that the diagnostic stays.

Existing tests asserting a changed output: none. The amendment changes no output; `rg "this$"` over the RH5419 test files finds only the bare-line-break-behind-`this` cases (offered side) and the withheld cases from round 2, all consistent with the amended text. The RH5419 formatter tests assert formatter output, which this round does not change.

RH5419 run: 98 passed, 0 failed. Architecture tests: 198/198. `scripts/format.sh` on the test file: no change.

## Not covered, and why

- A failing-first observation: none possible, the behavior predates the tests (spec clarification, no code change requested). The tests would fail on the code before round 2 (`57a8dea`), which the auditor observed offering two-line results for B1, B2, B4.
- Fix All: the new cases go through the same withheld path (`RegisterCodeFixesAsync` registers no action) that `VerifyFixAllLeavesTheIndexersWhoseFixIsWithheld` already mixes with offered indexers; no new arm.
- FC-004/019/020/021: the new cases are withheld; the fix rewrites nothing.
- Formatter, CLI, naming, unsupported-shape classes: no formatter or shape change in this round.
- Finding #3 (shared `StandsOnOneLineKeepingBlankLines`): a refactoring with unchanged behavior; the existing RH5401 and RH5419 tests cover both callers (`18-implementer.md`: 218/218).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5419"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
