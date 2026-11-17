Role: test implementer
Commit: f480120 (tests written against this head)
Verdict: tests written

## thoenissen/Reihitsu#1808

No tests apply, and I changed nothing for this issue.

- It is true that the repository has no tests for `scripts/proof/verify-text-only.cs`. `rg "verify-text-only|scripts/proof" src/` finds it only in two doc comments: `src/Reihitsu.Tooling/LaneCheckCommand.cs:159` and `src/Reihitsu.Tooling/LaneClassifier.cs:45`. No test project runs or references the proof program. The `_behavioralPrefixes` / `.md` classification that decision 1 changes (`scripts/proof/verify-text-only.cs:227`, `:600`–`:606`) exists only inside that file-based program.
- `src/Reihitsu.Tooling.Test/Unit/LaneClassifierTests.cs` and `LaneCheckCommandTests.cs` test the lane check. That check takes the proof's verdict as an input (`isTextOnlyProven`) and does not compute it. Decision 1 leaves the lane-check classification unchanged, so none of those tests asserts an output this run changes.
- The implementer checks the proof's verdict by hand: a diff that touches only `scripts/README.md` (or another `scripts/**/*.md`) must be text-only after the change and behavioral before it. A diff that touches a `scripts/*.sh` or `.github/workflows/*` file must stay behavioral.

## thoenissen/Reihitsu#1817

The guard is `RoslynVersionOwnership.Check(root)` in `src/Reihitsu.ArchitectureTests/General/RoslynVersionOwnership.cs`. It reads only `*.csproj`, `*.props` and `*.targets` files. It does not read `.cs` files, so a `#:package` directive is never seen. Every new test uses the existing fixture style (`RoslynVersionFixtureDirectory`, `RootPropsWithVersion`, `AssertSingleViolation`). I added one helper, `FileBasedProgram(directives)`, which writes the given directive lines, a blank line, and one statement.

The tests fix this contract:

- A file-based program's leading `#:package <id>@<version>` directive whose package is a Roslyn package is read. "Roslyn package" means the same `IsRoslynPackage` rule as for build files: `Microsoft.CodeAnalysis` or `Microsoft.CodeAnalysis.*`, case-insensitive, not `*.Testing`.
- Each such directive is recorded in `items` as `(relative path, package as written)`.
- The directive's version must equal the literal value of the root `RoslynVersion`, ordinal. Floating and prerelease suffixes are drift.
- A violation starts with `<relative path>:<1-based line>: #:package <package as written>`. The tests use `StartsWith`, so the implementer may add explanatory text after the prefix.
- Only directives before the file's first C# token are read. A `#:package`-looking line inside a string literal is not a directive. This also keeps the guard from flagging the raw-string fixtures in this test file during the repository-wide check.
- `bin/`, `obj/` and dot-directories are skipped, as for build files.

## Whole run

The only file changed is `src/Reihitsu.ArchitectureTests/General/RoslynPackageVersionTests.cs`, plus this handover. I touched no production or tooling code.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryRoslynPackageItemUsesTheRepositoryVersionProperty` (extended: asserts `scripts/proof/verify-text-only.cs` is among the items read) | decision 2 (real repository) | `RoslynVersionOwnership.Check` on the repo | **Fails**. The guard reads no `.cs` file, so the item is missing. Once the guard reads directives, it also fails until the pin `5.6.0` is bumped to `5.12.0`. |
| `PackageDirectiveAtTheRepositoryVersionIsAccepted` | decision 2; FC-003 accepted side | fixture | **Fails**. No item is recorded. |
| `PackageDirectiveWithAnotherVersionIsReported` | decision 2; FC-003 reported side | fixture, `AssertSingleViolation` | **Fails**. 0 violations. |
| `PackageDirectiveIsComparedWithTheDefinedVersion` | decision 2 (compared with `RoslynVersion`, not a constant) | fixture | **Fails**. 0 violations. |
| `PackageDirectiveWithFloatingVersionIsReported` (`@5.*`) | decision 2 | fixture | **Fails**. 0 violations. |
| `PackageDirectiveWithLongerVersionIsReported` (`@5.12.0-beta1`; exact match, not prefix) | decision 2; FC-003 | fixture | **Fails**. 0 violations. |
| `EveryPackageDirectiveOfAProgramIsCheckedOnItsLine` (shebang, `#:sdk`, `#:property`, two Roslyn pins; violation on line 5; 2 items) | decision 2; FC-016 sibling pins | fixture | **Fails**. 0 violations. |
| `PackageDirectiveInAnotherCasingIsReported` | decision 2 (NuGet ids are case-insensitive, like the existing casing tests) | fixture | **Fails**. 0 violations. |
| `PackageDirectivesOutsideTheRoslynVersionLineAreAccepted` (`*.Testing`, `Microsoft.CodeAnalysisExtras`, `SonarAnalyzer.CSharp`) | FC-003 not-Roslyn side | fixture | Passes now (vacuously); must stay green. |
| `PackageDirectiveTextAfterTheFirstTokenIsNotRead` (a `#:package` line at column 0 inside a raw string literal) | FC-003 directive-position boundary | fixture | Passes now (vacuously); must stay green. |
| `PackageDirectivesInBuildOutputAndDotDirectoriesAreSkipped` | same skip rule as build files | fixture | Passes now (vacuously); must stay green. |

On this head, the 8 failing tests all fail the same way: the guard does not read `.cs` files. The 7 fixture tests fail on `Assert.HasCount(1, violations)` (actual 0) or on the missing item. The repository test fails on the message "The guard found no Roslyn #:package directive in scripts/proof/verify-text-only.cs." The 25 pre-existing tests in the class still pass.

## Scenarios (bug fix)
Not a bug-fix lane. No scenarios.

## Not covered, and why
- A `#:package` without a version (`#:package Microsoft.CodeAnalysis.CSharp`) is not covered. Without central package management it does not restore, and the repository does not use central package management. Decision 2 does not say whether such a directive counts as drift. The implementer may report it, and no test forbids that.
- Line endings: the guard reads files, and there is no layout. A CRLF fixture would only test line counting. The existing build-file tests do not vary line endings either, so I left it out.
- FC-004 to FC-043 (trivia, rewrites, code fixes, formatter stability, unsupported shapes) cannot reach this change. It is an architecture-test helper that reads text and changes no file.
- The proof's verdicts before and after the bump (decision 2, last clause) are a manual check for the implementer. No test runs the proof.

## Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.RoslynPackageVersionTests"
```

## Follow-up candidates
_None._

Prompt note: the prompt carried no intended fix or hoped-for verdict beyond the lead's recorded decisions, so there was nothing to ignore.
