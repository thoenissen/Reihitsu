Role: test implementer
Commit: b950cb7 (tests written against this head)
Verdict: tests written

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions 4 and 7, so there was nothing to ignore.

## thoenissen/Reihitsu#1808

Nothing to do: this amendment concerns only the Roslyn-version guard of #1817.

## thoenissen/Reihitsu#1817

The amendment covers decision 4 and the class summary (decision 7). The only file changed is `src/Reihitsu.ArchitectureTests/General/RoslynPackageVersionTests.cs`.

- Contract addition: a Roslyn `#:package` directive without `@<version>` is read and recorded as an item, and it is reported with the same prefix as every other drift: `<relative path>:<line>: #:package <package as written>`.
- Boundary: the guard decides "Roslyn package" before it looks at the version. A version-less directive for a package outside the Roslyn version line (a non-Roslyn package, or a `*.Testing` package) is therefore neither read nor reported, as with versioned directives. The neighbour test pins this.
- Class summary: it now says that the guard covers non-testing `Microsoft.CodeAnalysis*` `#:package` directives in front of the first token of a C# file (file-based programs), which must state exactly the value of `RoslynVersion`. Build files are covered as before.

## Whole run

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PackageDirectiveWithoutVersionIsReported` | decision 4; FC-003 reported side | fixture, `AssertSingleViolation` | Passes on `b950cb7`. Fails, with `Assert.HasCount(1, violations)` (actual 0), when the guard skips version-less directives. I checked this with a throwaway mutation of `RoslynVersionOwnership.ReadPackageDirectives` and reverted it. |
| `PackageDirectiveWithoutVersionOutsideTheRoslynVersionLineIsAccepted` (`SonarAnalyzer.CSharp`, `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing`, both without a version) | decision 4; FC-003 not-Roslyn side | fixture | Passes; must stay green. |

The class summary changed only in its text. No requirement tags apply, because tooling-lane tests verify lead decisions, not spec requirements.

### Scenarios (bug fix)
Not a bug-fix lane. No scenarios.

### Not covered, and why
- A version-less directive whose package id is written in another casing is not covered. It goes through the same `IsRoslynPackage` call that `PackageDirectiveInAnotherCasingIsReported` already covers, and the version is read after that call (`RoslynVersionOwnership.cs`, `ReadPackageDirectives`). So one more test would add nothing.
- `#:package Microsoft.CodeAnalysis.CSharp@` (an `@` with an empty version) is not covered. It is a different input from a missing version: it records `""`, which the ordinal comparison reports like any other wrong version. No file-based program in the repository writes it.
- FC-004 to FC-043 cannot reach this change, for the same reasons given in `01-test-implementer.md`.

### Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests"
```
Result: `Passed! - Failed: 0, Passed: 38, Skipped: 0, Total: 38`.

### Follow-up candidates
_None._
