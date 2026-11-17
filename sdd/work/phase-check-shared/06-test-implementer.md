Role: test implementer
Commit: dcbf6d9d30a89dc4743a8d2278e4f02811aad24f
Verdict: tests written

Repair round 1, worklist item (a) only (decision 7 in `00-lead.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Change

`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.cs`, test `VerifyFormatterFixesGluedFixedKeyword`:

- After the second-pass assertion, each loop iteration now calls `PhaseStabilityCheck.AssertNoPhaseChanges(firstPass, endOfLine, CSharpParseOptions.Default)`. This is the text overload with the line ending the test formatted with and the parse options `Format` parses with (`CSharpSyntaxTree.ParseText` without options), the same shape the base uses under decision 3. The output is pinned to `expected`, which uses only that ending, so the forced ending equals the detected one (FC-033).
- The type is imported with a using alias, `using PhaseStabilityCheck = Reihitsu.Formatter.Test.Helpers.PhaseStabilityCheck;`. A plain `using Reihitsu.Formatter.Test.Helpers;` does not compile in this file: `[Requirement]` becomes ambiguous (CS0104) between `Reihitsu.Analyzer.Test.Base.RequirementAttribute` and `Reihitsu.Formatter.Test.Helpers.RequirementAttribute`. RH5604 had no `[Requirement]` and so did not hit it. A type alias has precedent in `src/Reihitsu.ArchitectureTests` (`FixAllProviderCreationTests.cs`, `CodeFixFormatterAccessTests.cs`).
- The test's summary now names the phase-level check.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.VerifyFormatterFixesGluedFixedKeyword` | RH6001-001 (existing tag, unchanged); FC-010, FC-012, FC-017 for the added check | `PhaseStabilityCheck.AssertNoPhaseChanges(string, string, CSharpParseOptions)` | pass (passes) |

No new test method; FMT-PIPE-027 stays untagged here, as in 02.

Falsifiability, by reverted mutation (not committed): passing `firstPass + " "` to the check only when `endOfLine == "\r\n"` failed the test with `CleanupPhase changes the final output under CRLF line endings when it runs alone (FMT-PIPE-027). First difference at line 9: expected "} ", phase returned "}".` The CRLF iteration runs after the LF one, so the LF call was reached and passed on the unmutated text before it.

## Not covered, and why

- The other members of the auditor's follow-up list: dropped by decision 7.
- The base-class summaries (worklist item b): the cleaner's item, not mine.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH6001"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter"
scripts/test.sh --project architecture
```

All pass: 4, 484, and 180 tests. `scripts/format.sh` left the file unchanged.

## Follow-up candidates

_None._
