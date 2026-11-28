Role: auditor
Commit: 7847c6ca90171381a6ac9aab9f31406dbbf2fcd4
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `7847c6ca90171381a6ac9aab9f31406dbbf2fcd4` on `claude/determined-mccarthy-dpombd`, checked out, clean tree. Base `origin/main` = `707d4b805f6da94910a921acdfa5c0f281bd1f42`, an ancestor of the head (merged). The diff matches the handovers: `src/Reihitsu.Tooling/FixtureRunner.cs`, `Enumerations/FixtureOutcome.cs`, `CodeFixRunCommand.cs` (doc only), `scripts/README.md`, `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs`, and `sdd/work/`. `scripts/lane-check.sh tooling`: confirmed. Issue thoenissen/Reihitsu#1977 and its maintainer-account comment read; no spec under `sdd/specs/` covers the fixture runner, so the requirements are the issue's "Done when" and `00-lead.md` decision 1.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Tooling/FixtureRunner.cs:136`, `:186-189` (`GetCompileErrorsAsync` keys errors by ID and message) | – (nearest: FC-044, the key holds a name the Renamer rewrites) | – | PR-introduced | A fix that renames a symbol named in the message of an error the fixture already had turns that error into a "new" one, so `apply-fix.sh` reports `invalid-result` and exits 2 for a fix that added no error. On `main` these results parse, so they were `fixed`, exit 0. Constructed and run on this head, under LF and CRLF alike (all `invalid-result (1 iteration(s))`, exit 2): **RH4002** on `internal class sample { internal int M() { } }` (CS0161 `'sample.M()': not all code paths return a value` becomes `'Sample.M()': ...`: the containing type's name); **RH4103** on `internal class Sample { internal int compute() { } }` (CS0161, the member's own name); **RH4105** on `internal class Sample { internal void M(out int Value) { } }` (CS0177, the parameter's name). Holds: **RH4002** on `internal class sample { private Missing _field; }` (CS0246 does not name the renamed type) — `fixed`, exit 0. Class axes: the renamed symbol kind (every RH4xxx rename fix, RH4001 included: type, member, parameter, local, type parameter) × where the message names it (the symbol itself, or as the container of a displayed member). No other way for a shipped fix to change an existing error's message without adding an error was found: layout fixes do not change symbol display, and Roslyn messages do not carry positions or line endings. Since, per the test implementer, no shipped fix produces a result that compiles worse, this false positive is the change's main observable effect on real rules | The owner re-derives; options: key the comparison so that a rename does not change it while a new same-ID error for another name still counts (for example, per ID, the number of distinct messages must not grow — this keeps every test of the run green and the rename members `fixed`); or keep (ID, message) and state in `scripts/README.md` that a fix renaming a symbol an existing error names reports `invalid-result`. Either way, add a test for the rename member. The first option revises `00-lead.md` decision 1, which did not consider renames |
| 2 | low | `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerTests.cs:95-96` (summary of `HasSyntaxErrorsDetectsUnparseableSource`) | FC-031 | – | PR-introduced | The summary still says `HasSyntaxErrors` is "the same check that ... guards against reporting an unparseable fix result as a success". After this change it only checks the fixture; the result is checked by `GetCompileErrorsAsync` | Drop the second half of the sentence, or say that it is the fixture's parse check. Comment only |

Finding #2 alone would be `pass, text cleanup` (a test's XML summary, no compiled behavior); #1 is behavioral, so the verdict is `findings`.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix result is `InvalidResult` (`resultErrors.IsSubsetOf(originalErrors) == false`) | Every error-severity diagnostic of the whole one-document ad-hoc compilation of the result, under `state.DocumentPath` and the run's `languageVersion` | `originalErrors`: the same compilation of the normalized fixture under `fixturePath` (`RunAsync:87`) | Same project setup (`CreateDocument`: references, parse options, global config); a document rename (RH4001) changes only the path, which no compiler message embeds. Disagreement in the key, not the span: see #1 | finding #1 |
| Fixture is `ParseError` (`HasSyntaxErrors`, unchanged) | Syntax-tree diagnostics of a bare parse with `new CSharpParseOptions(languageVersion)` | The project's parse options in `CreateDocument` (`new CSharpParseOptions(languageVersion)`) | Same options (no preprocessor symbols, default documentation mode in both), so a fixture that passes the parse check carries no parse error in `originalErrors`, and the code comment "every parse error of the result counts as new" holds | holds |
| Only errors count (`Severity == Error`) | Compilation diagnostics | Old parse check also filtered on `Error` | Same severity filter; `#pragma warning` cannot suppress errors, and the project sets no warnings-as-errors | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Validity of a fix result | `rg -n "GetDiagnostics\(|HasSyntaxErrors|GetCompilationAsync|InvalidResult" src/Reihitsu.Tooling`: only `FixtureRunner.RunAsync` with `GetCompileErrorsAsync`; `CodeFixRunCommand.DescribeOutcome`/`GetExitCode` map the outcome only | one owner |
| Ad-hoc fixture project setup | `FixtureRunner.CreateDocument`, reused by `RunIterationAsync` and `GetCompileErrorsAsync` | one owner |
| Parse check | `FixtureRunner.HasSyntaxErrors` (tooling, fixture only); `ReihitsuFormatterHelpers.HasSyntaxErrors` (formatter, CLI, code-fix base) is a separate decision on another surface | no duplication introduced |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RunAsyncReportsInvalidResultWhenTheFixBreaksALineInsideAnInterpolationHoleBeforeCSharp11` (LF, CRLF) | A result that parses but has CS8967 under C# 10 is invalid | The parse-only check of `main` returns `Fixed`; the test also asserts the result has no parse error under C# 10, so it cannot pass on a parse check | stub analyzer + stub fix, `RunAsync` | adequate |
| `RunAsyncReportsFixedWhenTheFixBreaksALineInsideAnInterpolationHoleFromCSharp11` (11, Latest) | Same result is fixed where it compiles | A check that ignores the run's language version (or hard-codes a lower one) reports `InvalidResult` | same | adequate |
| `RunAsyncReportsInvalidResultWhenTheFixIntroducesAnUndefinedName` | Any new compile error counts, not only version errors | A version-only or parse-only check returns `Fixed` | same | adequate |
| `RunAsyncReportsFixedWhenTheFixIntroducesOnlyAWarning` | Warnings do not count | Counting all severities reports `InvalidResult` (CS0219 is certain for `var unused = 1;`) | same | adequate |
| `RunAsyncReportsNoDiagnosticForAFixtureThatDoesNotCompile` | A fixture's compile errors are no `parse-error` | Using the compile check for the fixture's parse check reports `ParseError` | same | adequate |
| `RunAsyncReportsFixedWhenTheFixKeepsACompileErrorTheFixtureAlreadyHad` | Existing errors are not new | "Result has any error" reports `InvalidResult` | same | adequate |
| `RunAsyncReportsFixedWhenTheFixRepeatsACompileErrorTheFixtureAlreadyHad` | Repeats of an existing (ID, message) are not new | A multiset comparison reports `InvalidResult` | same | adequate |
| `RunAsyncReportsInvalidResultWhenTheFixAddsAnotherCompileErrorToAFixtureThatDoesNotCompile` | A new ID on a non-compiling fixture counts | "Invalid only if the fixture compiled" returns `Fixed` | same | adequate |
| `RunAsyncReportsInvalidResultWhenTheFixAddsAnErrorWithTheSameIdForAnotherName` | A new same-ID error for another name counts | An ID-only set returns `Fixed` | same | adequate |
| missing | A fix that only renames a symbol an existing error names is `fixed` | — | — | missing; part of finding #1 |

## Failure classes dismissed
- FC-002, FC-003: the qualifiers of the issue's "Done when" (under the chosen language version; errors the original did not have; errors, not warnings) are explicit conditions, and each is tested on both sides (C# 10 vs 11/Latest; kept/repeated vs new error; warning vs error).
- FC-004, FC-005, FC-019 to FC-021: the runner rewrites no trivia; it only compiles and compares.
- FC-006, FC-022 to FC-025, FC-027 to FC-029: no rewrite of user code; the runner only classifies. The interpolated-string member of FC-022 is the reproduction itself and is covered under LF and CRLF.
- FC-007 to FC-009, FC-018, FC-030: no code fix changes. FC-036 and FC-044 reach the runner only through finding #1 (a rename changes an error message the key compares).
- FC-010 to FC-014: no formatter phase changes.
- FC-015, FC-016: one owner (above); the only sibling validity check was the replaced parse check of the result.
- FC-017: see the table; every assertion pins the outcome the invariant names.
- FC-032: no project result is reused; the tests run the changed runner.
- FC-033: the reproduction runs under both line endings, through `RunAsync`, the same path `CodeFixRunCommand` uses.
- FC-034: the shape is the issue's own example (an object creation written onto several lines inside a `$"` hole).
- FC-035: expected values are hand-written constants, not taken from output.
- FC-037: no text-only claim.
- FC-038 to FC-043: no unsupported shape is declared or checked.
- FC-031: covered by finding #2; the changed method summaries (`RunAsync` `languageVersion`, `GetCompileErrorsAsync`, `FixtureOutcome.InvalidResult`, both `CodeFixRunCommand` param docs) and the inline comment at `FixtureRunner.cs:131-133` describe the new code.

## Architecture findings
_None._ Finding #1 is a local choice of comparison key, not two components owning one decision.

## Hints
- Cost: `GetCompileErrorsAsync` builds a fresh workspace and compiles once per fixture arm before the loop and once per applied fix, while `RunIterationAsync` compiles the same source again in its own workspace right after. The tooling suite still runs in about 16 s, so not a finding; reusing the iteration's compilation would halve the extra work.
- No test pins that the check runs after every iteration rather than on the final result only (the implementer chose every iteration, as the former parse check did).
- `HasSyntaxErrors` stays `public` although only `RunAsync` and one test call it; unchanged by this run.

## Follow-up candidates
_None._
