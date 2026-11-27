Role: test implementer
Commit: 5d70005
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4001TypeNameShouldMatchFileNameAnalyzerTests.cs` (class derives from `SingleCodeFixTestsBase`). Every test with top-level statements, and every test in a project that holds them, sets `TestState.OutputKind = OutputKind.ConsoleApplication` (skeptic's FC-033 harness note); the verifier saw no compiler error.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FileWithTopLevelStatementsIsNotReported` — spec ex. 1, `Program.cs` with statement + `class Helper` | RH4001-007 | `Verify(source, configure)` | Fails: expected 0 diagnostics, actual 1 at `/0/Program.cs(5,16)` (`Helper`) |
| `FileWithTopLevelLocalFunctionAndNamespacedTypeIsNotReported` — spec ex. 2, statement + top-level local function + `namespace Tools { class Calculator }` | RH4001-007 | `Verify(source, configure)` | Fails: expected 0, actual 1 at `/0/Program.cs(7,20)` (`Calculator`) |
| `FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported` — spec ex. 3 | RH4001-007 | `Verify(source, configure)` | Passes |
| `FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed` — skeptic fixture `k`: `Startup.cs` with `class Program` whose `Main` holds a statement; reported on `Program`, fix renames to `Program.cs` (FC-003/FC-002 other side of the boundary) | RH4001-007 | `Verify(source, null, configure)` with fixed state | Passes |
| `FileWithoutTopLevelStatementsIsReportedWhenAnotherFileHoldsThem` — spec ex. 4: `Program.cs` holds top-level statements, `Startup.cs` declares `Helper`; `Startup.cs` reported and renamed to `Helper.cs`, `Program.cs` unchanged | RH4001-007 | `Verify(source, null, configure)` with two documents | Passes |

## Not covered, and why
- Template shape `record WeatherForecast` after statements (skeptic fixture `l`): the same predicate as ex. 1; the type kind only decides which type RH4001-001 picks, already covered by `RecordNameMismatch`. The skeptic marked it optional.
- `partial class Program` after statements (fixture `m`): unchanged either way; skeptic marked it not required.
- Parse-error shapes, statements before a file-scoped namespace, lone `;`, a statement only in an inactive `#if` group, `.csx` files: settled by the skeptic as unsupported, harmless, or unreachable.
- Code-fix path for the exempt files: `Verify` without a diagnostic also asserts no fix applies; RH4001 has no Fix All (FC-008 not applicable).
- Existing tests whose output the change alters: `rg RH4001 tests` finds only this test class, `RuleDocumentationExampleOptOuts`, and the tooling fixtures of `CodeFixRunCommandTests`/fake helpers; none holds top-level statements, so none changes.
- Failure classes: FC-002/FC-003 covered by the boundary test above. Trivia, directives, formatter stability, rewrite semantics, unsupported shapes (FC-004–FC-014, FC-019–FC-029, FC-038–FC-043): the change is a member-kind check in an analyzer, with no rewrite, no token gap, and no formatter involvement. FC-018/FC-036: the fix renames a file, not a symbol, and is unchanged. Line endings: no layout involved, so no CRLF variant.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests"
scripts/test.sh --project architecture
```
Observed: analyzer 47 passed, 2 failed (the two RH4001-007 not-reported tests, as above); architecture 198 passed (`EveryRequirementOfAnEnforcedSpecIsCovered` now green).

## Follow-up candidates
_None._
