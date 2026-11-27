Role: test implementer
Commit: 21f5b0e
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. This run is a test refactoring (lane tests, `00-lead.md` decisions 1 to 7), so there is no reproduction and no new test; the existing tests are rewritten to read the resource text instead of quoting it.

## Tests

No test was added. Changed tests, by member of `01-skeptic.md` *Breaking inputs*:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Member 1: 258 `Diagnostics(<X>.DiagnosticId, "<literal>"[, n])` sites in the 20 files listed in `01-skeptic.md` now pass `MessageFormat` (decision 6, inline) | — (FC-017 checked, below) | `AnalyzerTestsBase<TAnalyzer>.MessageFormat`; every file's `TAnalyzer` is the analyzer whose ID the call names (checked per file) | pass |
| Member 2: `LineLimit` helpers in `Design/RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests.cs`, `Design/RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests.cs` and `Complexity` in `Design/RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests.cs` now use `.WithMessage(string.Format(MessageFormat, count|complexity, limit))` (decision 2) | — | `AnalyzerTestsBase<TAnalyzer>.MessageFormat` | pass |
| Member 3: the four `ScanJudges…` tests in `tests/Reihitsu.SelfHosting.Test/Analyzers/LineCountSelfHostingTests.cs` keep the scan's `[RH####] (<AnalyzerType>) ` prefix literal and build the message with `string.Format(analyzer.MessageFormat, count, limit)` from an `analyzer` local in a new `// Arrange` section (decision 3) | — | public `DiagnosticAnalyzerBase.MessageFormat` | pass |
| Member 4: `VerifyTitleNamesFirstMemberAccess` deleted from the RH5048 and RH5112 analyzer test classes, with the then unused `using System.Globalization;` (decision 4) | — | — | — |
| Member 5: `RH5112…AnalyzerTests.VerifyCodeFixTitleNamesFirstWrappedMemberAccess` asserts `CodeFixResources.RH5112Title` (decision 5); `using Reihitsu.Analyzer.CodeFixes;` added (`CodeFixResources` is internal, visible to `Reihitsu.Analyzer.Test`) | — | `GetCodeFixActionsAsync` | pass |

No touched test carries a `[Requirement]`, and none was added.

Left unchanged (decision 7): `tests/Reihitsu.ArchitectureTests/General/ApplyFixWrapperPositionalArgumentTests.cs:62`, `tests/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandLanguageVersionTests.cs:365`, `tests/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandTests.cs:539`, `:561` — command-line argument arrays.

## Evidence

- `rg -n 'DiagnosticId, *"' tests --glob '*.cs'` now matches only the 4 argument-array lines above.
- Resource-literal scan (every `<data>` value of `src/Reihitsu.Analyzer/AnalyzerResources.resx` and `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` searched with `rg -F --glob '*.cs'` over `tests/`: placeholder-free values as the quoted literal, placeholder values by their longest fixed fragment, fragments under 12 characters skipped): 0 hits.
- Wording mutation, to check the issue's goal: in the working tree I prefixed `Reworded ` to the `MessageFormat` value of all 18 rules (RH2008, RH2009, RH2111, RH3002 to RH3007, RH3101 to RH3105, RH3201, RH5048, RH5112, RH5201) and to `CodeFixResources` `RH5112Title`, ran the two commands below (the analyzer assemblies were rebuilt: `Reihitsu.Analyzer -> …` in the log), and got analyzer 1317/1317 passed and self-hosting 6/6 passed. The resource files were then restored with `git checkout --`, and `scripts/build.sh` rebuilt the solution from the restored tree (0 warnings, 0 errors). Outside that filter, `AnalyzerPackageMetadataTests` pins titles against the rule pages, as intended; I did not run it under the mutation.
- `scripts/build.sh` after the change: 0 warnings, 0 errors (no unused `using` left).
- `scripts/format.sh` over the 24 changed files: 0 of 24 reformatted.

## Scenarios (bug fix)

_N/A_ — not a bug fix.

## Not covered, and why

- No new test (`README.md`, *Coverage first*): the wording itself stays pinned by `AnalyzerPackageMetadataTests` (title against the rule-page heading, message against title for placeholder-free messages) and by `AnalyzerResourcesTests.EveryAnalyzerPublishesTheResourcesItsDiagnosticIdNames` (descriptor tied to its resource); the deleted member-4 tests are redundant with those two.
- FC-017 (weaker assertion): members 1 and 5 no longer fail on a wording change, which is the purpose of the run (decisions 2 to 6); each assertion still checks the diagnostic ID, location, count, and the descriptor's own message or code-fix title resource. Members 2 and 3 still check the substituted count, complexity, and limit.
- FC-035 (expected values from the code): the expected text is read from the resource the descriptor names, not from a patched copy of production logic; the arguments stay test-supplied constants.
- Every other failure class in `squad/failure-classes.md` concerns production code, formatter phases, or code-fix behavior; this run changes none of them.
- Formatter idempotency, CRLF variants, Fix All, convergence, comment/directive gaps: not applicable; no production behavior changes, and the touched tests keep their existing line-ending and Fix All variants unchanged.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH30|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH31|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3201|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2008|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2009|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2111|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5048|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5112|FullyQualifiedName~RH5201CommentExemptChainWithNonInvokedPrefixDotTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~LineCountSelfHostingTests"
```

The self-hosting run needs the whole solution restored first (`scripts/build.sh`); without it, every `SelfHostingSolution` test fails with `Could not find project assets file for 'Reihitsu.Tooling'`, which is the environment, not the change.

## Follow-up candidates

- The `Reihitsu.ArchitectureTests` convention check already named in `00-lead.md` (fail when a test file quotes a resource text as a literal); the scan under *Evidence* is its core.
