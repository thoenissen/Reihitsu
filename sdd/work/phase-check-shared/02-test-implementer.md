Role: test implementer
Commit: 52cdb5ac422cd74fb2e35f03da435e1bcee5f958
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The run is in the tests lane with no implementer, so the change is test code and I wrote all of it, following the lead's decisions 1 to 4 in `00-lead.md`. One path is tooling: the `.csproj` reference (decision 2). After my commit, `scripts/lane-check.sh tests` will report that the run widens to tooling, as the lead expected.

## Changes

- `src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj` adds a `ProjectReference` to `..\Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj` (decision 2). The build has no `RequirementAttribute` ambiguity and no other conflict, and the analyzer run gains no Formatter.Test tests: its total is 10460 tests, all in `Reihitsu.Analyzer.Test.dll`.
- `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`:
  - `VerifyFormatterFixCore` now returns `(Formatted, ParseOptions, IsRefused)`. `IsRefused` is `FormattingResult.Unsupported != null`.
  - The private `VerifyFormatter(source, fixedSource, endOfLine, onConfigure, expected)` calls `PhaseStabilityCheck.AssertNoPhaseChanges(reformatted, endOfLine, parseOptions)` after its second pass, unless the input was refused. This is the text overload with the base's own line ending and parse options (decision 3).
  - Every public `VerifyFormatter` overload runs this private method once under the test's line ending and once under the alternate one, so the check runs on the output for each of the two endings.
  - Generated-code and hiding-carrier output is still checked, because the base still formats it through `FormattingPipeline.Execute` (decision 3).
  - The class summary names the check.
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5604CodeMustNotContainMixedLineEndingsFormatterTests.cs` adds `PhaseStabilityCheck.AssertNoPhaseChanges(firstTree)` to two places (decision 4):
  - `VerifyFormatterEntryPointsAndIdempotency`, the method the skeptic named.
  - `AssertExemptInteriorsSurviveNormalization`, its sibling in the same file. It has the same shape: a `FormatSyntaxTree` output that is asserted idempotent, on a genuinely mixed file. That makes it the check's case where the line ending is detected. I took it in under the same decision and did not ask about it separately; the lead may drop it.
- The CLI test is unchanged (decision 1).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Every test of the 158 classes derived from `FormatterTestsBase<TAnalyzer>` (459 `VerifyFormatter` calls observed, about 918 checked outputs) | FC-010, FC-012, FC-017 (no tag: the check is incidental to each rule's test) | `FormatterTestsBase<TAnalyzer>.VerifyFormatter` → `PhaseStabilityCheck.AssertNoPhaseChanges(string, string, CSharpParseOptions)` | pass (passes) |
| `RH5604CodeMustNotContainMixedLineEndingsFormatterTests.VerifyFormatterNormalizesXmlCrLfToPredominantLfAndIsIdempotent` | FC-033 (no tag) | `PhaseStabilityCheck.AssertNoPhaseChanges(SyntaxTree)` | pass (passes) |
| `RH5604CodeMustNotContainMixedLineEndingsFormatterTests.VerifyFormatterNormalizesXmlLfToPredominantCrLfAndIsIdempotent` | FC-033 (no tag) | `PhaseStabilityCheck.AssertNoPhaseChanges(SyntaxTree)` | pass (passes) |
| `RH5604CodeMustNotContainMixedLineEndingsFormatterTests.VerifyExemptInteriorsSurviveNormalizationInAGenuinelyMixedFile` | FC-033 (no tag) | `PhaseStabilityCheck.AssertNoPhaseChanges(SyntaxTree)` | pass (passes) |

No new test method is added. The change adds an assertion to existing tests of existing behavior, and FMT-PIPE-027 is tagged where the check itself is tested, in `Reihitsu.Formatter.Test`.

## Established facts (skeptic's open candidates)

All runs below are on this head with my change applied.

- **Candidate 1, refused fixtures.** None. I added temporary instrumentation (not committed) that logged every check and every refusal in the base, then ran the filter `FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter` (484 tests).
  - It logged 459 LF checks and 458 CRLF checks, plus one line garbled by concurrent appends (459 pairs in total).
  - It logged no refused input.
  - The skip branch is therefore unreached by today's corpus. It exists for the decision 3 case.
- **Candidate 7, pre-existing failures.** None. The whole analyzer project passes with the check: 10460 tests passed, 0 failed.
- **Candidate 6, falsifiability.** I verified it by temporary mutation (reverted, not committed).
  - Mutation 1 passed `reformatted + " "` to the check. `RH5604...VerifyFormatterFixesViolation` failed with `CleanupPhase changes the final output under LF line endings when it runs alone (FMT-PIPE-027). First difference at line 4: expected "} ", phase returned "}".`
  - Mutation 2 applied that change only when `endOfLine == "\r\n"`. The same test failed with the same text under `CRLF line endings`. So the base reaches the check on the alternate-ending pass as well, and it fails with the owner's failure text.
  - The RH5604 direct calls are straight-line code after assertions that pass, so the green runs show they execute.
- **Candidates 2 and 3.** No fixture has a generated-code marker (skeptic's `rg`), so candidate 2 is not exercised. For candidate 3, `RH5604...VerifyFormatterFixesViolation` passes mixed input through the base. Its output is fully normalized to the forced ending, so the text overload with the forced ending checks the input that FMT-PIPE-027 specifies.

## Not covered, and why

- **A committed test that the base calls the check.** `PhaseStabilityCheck.OverrideForCurrentTest` is `internal` to `Reihitsu.Formatter.Test` and bound to its session. The session-free overload the base uses always runs the production phases. With no real reversal between phases, no committed input can make the base's check fail. Building such an input would need a seam in the test base or a new `InternalsVisibleTo` in `Reihitsu.Formatter.Test`, which the lead's decisions do not include. The mutation runs above are the evidence instead.
- **The other derived classes that call `FormatSyntaxTree` themselves** (RH5021, RH5029, RH5030, RH5104, RH5111, RH5201, RH5412 to RH5416, RH6001; skeptic's list). They are outside decision 4, so I left them unchanged and list them under follow-up candidates.
- **Failure classes.**
  - FC-010, FC-012, FC-017, and FC-033 are covered above.
  - FC-042 is covered by the refusal skip, and no fixture reaches it.
  - The code-fix, naming, token-gap, and FMT-UNSUP classes do not apply: no production behavior, fix, rule, or declared shape changes.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH5604"
scripts/test.sh --project analyzer
scripts/test.sh --project architecture
scripts/test.sh --project tooling
```

All of them pass: 484, 4, 10460, 180, and 133 tests.

## Follow-up candidates

- Analyzer formatter tests that call `FormatSyntaxTree` directly (RH5021, RH5029, RH5030, RH5104, RH5111, RH5201, RH5412 to RH5416, RH6001) may assert full-run idempotency without the per-phase check. I did not check which of them do.
