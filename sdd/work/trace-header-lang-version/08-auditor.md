Role: auditor
Commit: ec68841
Verdict: #1924: pass; #1949: pass

Read: `squad/roles/auditor.md`, `squad/README.md`, `squad/failure-classes.md`, `00-lead.md`, handovers 01-07, both change docs, both issues, `git diff 68f236ea..ec68841`, and the counterparts named below. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Whole run

### Scope

- Head: `ec68841c8613913dd082232455a8bc7ac0992e97` (checked out, clean). Base: `origin/main` = `68f236ea94fdcbcf4aa14f98ff759d2c06708f32` (matches `git ls-remote origin refs/heads/main`), an ancestor of the head. Full audit.
- The diff is what the handovers describe: production `PipelineTraceCommand.cs`, `CodeFixRunCommand.cs`, `FixtureRunner.cs`, `AssemblyInfo.cs`; the wrapper comments `scripts/trace.sh`, `scripts/apply-fix.sh`; `scripts/README.md`; three new test files; argument-only edits of `FixtureRunnerTests` (decisions 8, 9); two changed tests in `PipelineTraceCommandTests`; two change docs; `sdd/work/`.
- `scripts/lane-check.sh behavior-change` → `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- No test re-run: no finding turned on one. The roles' focused runs and the cleaner's build stand.

### Visible output is decided

Neither tool is a visible surface: `PipelineTraceCommand` is reached only from `scripts/trace/trace.cs` (`rg PipelineTraceCommand src scripts`), not from `reihitsu-format` (`Program`), and `sdd/specs/cli/reihitsu-format.md` excludes the trace; `Reihitsu.Tooling` is repository tooling. No formatter, analyzer, code-fix, or rule-page expected value changed. `reihitsu-format`'s own parsing (`Program.ApplyOptionValue`, CLI-001/CLI-004) and `--dry-run` header (`DefaultDiffGenerator`, CLI-024) are untouched. Nothing undecided.

## thoenissen/Reihitsu#1924

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._

Against the change doc and the issue: `WritePhaseChange` writes through `new DefaultDiffGenerator().Generate(fullPath, …)`, where `fullPath` is `DefaultFileSystem.GetFullPath(filePath)` = `Path.GetFullPath`, the same input `FormatCommandHandler` gives the CLI-024 owner. Examples 1-3 of the change doc follow: inside → `DiffGenerator.Generate(relativePath)` (`a/`, `b/`, `/` separators); outside or unreadable current directory → `GenerateWithNames(fullPath, fullPath)`. `.` and `..` segments disappear in `Path.GetFullPath` before the comparison. The `displayPath` plumbing (`filePath.Replace('\\', '/')`) is deleted from all four methods. Issue goals: "full path prints a header without `a//`" — done; "a test covers the chosen header for a full path" — `FullPathInsideTheCurrentDirectoryPrintsTheRelativeHeader`, `FullPathOutsideTheCurrentDirectoryPrintsTheFullPath`, and the updated `ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched`; the "trace spec" branch and the constraint about the existing test are dropped as recorded (decision 7, change doc *The issue's goals this change does not carry out*).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the traced file lies inside the current directory | none in the trace; delegated to `DefaultDiffGenerator.TryGetPathInside(Directory.GetCurrentDirectory(), fullPath)` | the same method, used by `reihitsu-format --dry-run` (`Program.cs:312`, `FormatCommandHandler.cs:610`) | one predicate, same input (`Path.GetFullPath`) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Diff header naming (CLI-024) | `rg '"a/\|\$"a/\|--- a/' src`: `DiffGenerator.Generate` (`a/{path}` rendering, only caller `DefaultDiffGenerator`), `DefaultDiffGenerator` (inside/outside decision); `Tooling/UnifiedDiffWriter.cs:40` writes `--- a/<fixture-relative path>` for the fixture runner, which only ever receives a path relative to the fixture directory (`CodeFixRunCommand.cs:173`), so it is a different header for a different input, not a second owner. `rg GetCurrentDirectory src`: only `DefaultDiffGenerator` and `Program`'s default path | one owner; the trace holds no copy |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PipelineTraceCommandDiffHeaderTests` (6 tests) | every `---`/`+++` line of every phase diff equals the expected header, `Tracing` line unchanged | any header line differing, or no diff printed (`Assert.IsNotEmpty`) | `AssertTrace` compares all header lines with `AreSequenceEqual`; paths built from `Directory.GetCurrentDirectory()` so a symlinked temp directory (macOS) does not break them; class `[DoNotParallelize]` because it sets the current directory, as `CliDryRunDiffHeaderEndToEndTests` does | adequate; inside (full, plain relative, `./`, `../`) and outside (relative, full) are both sides of the boundary (FC-003) |
| `ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched` (changed) | a file outside the current directory is named by its full path without prefixes | an `a/` header or a different path | exact lines with the writer's newline plus `DoesNotContain("--- a/")`; relies on the temp directory (`Path.GetTempPath()/ReihitsuCliTest/…`) lying outside the test process's current directory, which no parallel test changes (the classes that change it are `[DoNotParallelize]`) | adequate |
| `ExecuteAsyncMixedLineEndingsReportsCountFallback` (changed) | a separator-only change prints no diff | any `--- ` or `@@ ` line | stronger than before (FC-017): no longer depends on the `a/` prefix | adequate |

### Failure classes dismissed
- FC-001, FC-002, FC-044: no guard added; the decision is delegated to the existing owner.
- FC-003: covered by the six header tests (inside/outside, full/relative, dot segments).
- FC-015, FC-016: see *Policy ownership*; the sibling header writer (`UnifiedDiffWriter`) never receives a full path.
- FC-031: the four changed methods' parameter docs and `WritePhaseChange`'s summary describe the new header; `DiffGenerator.Generate`'s summary still holds for its one caller.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-043: formatter, code-fix, trivia, and precheck classes; the change touches only the header text of a repository tool's diff, not a phase, a fix, or a rule.
- FC-017: see *Assertion adequacy*.
- FC-032: no project result is reused across a compiled change.
- FC-033, FC-034, FC-035: expected headers come from the change doc's examples and CLI-024, not from the new code's output; the old header failed every changed case (`04-test-implementer.md`).
- FC-037: no comment-only change claimed.

### Architecture findings
_None._ The change removes a second header rule instead of adding one.

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1949

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding._

Against the change doc and the issue:
- **Where.** Both parsers test `--lang-version` before the generic `StartsWith('-')` rejection and only while `PathsOnly`/`PositionalOnly` is false; the value is `args[argumentIndex++]` unconditionally, so `--lang-version --passes` takes `--passes` as the value.
- **Values and default.** `LanguageVersionFacts.TryParse` plus `LanguageVersionResolver.Resolve`/`ExceedsMaximum`, exactly as `Program.ApplyOptionValue`; the default is `LanguageVersionResolver.MaxSupportedLanguageVersion` in both tools (decision 6). The warning sentence matches the change doc with the `trace`/`apply-fix` prefix and changes no exit code.
- **What the version applies to.** Trace: every pass parses with `CSharpParseOptions.Default.WithLanguageVersion(resolved)` (the `FormatCommandHandler.cs:143` construction), so `GetInvalidTraceExitCode` (syntax errors, `IsAutoGeneratedSource`, `HoldsHidingCarrier`, which reparses from `syntaxTree.Options`) follows it; `FormattingContext` takes the resolved version directly, equal to the old `Resolve(syntaxTree.Options)` for a concrete version at or below the maximum. Fixture runner: `rg "ParseText|CSharpParseOptions|LanguageVersion\." src/Reihitsu.Tooling` finds exactly the three uses — `HasSyntaxErrors` for `parse-error` (line 81) and `invalid-result` (line 129), and `CreateDocument`'s project options (line 448), rebuilt with the version on every iteration — and no `LanguageVersion.Latest` remains.
- **Errors.** Missing, invalid (including `""`), and repeated values return before any file or fixture is read, with the change doc's messages, the usage text, and exit `2`. A warning is printed only after argument validation and after `--help` returns.
- **Usage.** Both usage lines match the change doc; `scripts/README.md` and both wrapper comments list the option.
- Issue "Done when": both scripts accept the option, run with that version's parse options, fail with a clear message on an unknown value, and `scripts/README.md` lists it. The wrappers needed no logic change (they forward the argument).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Is the argument the `--lang-version` option | `argument == "--lang-version"` while not after `--` | `--passes` / `--max-iterations` checks in the same parsers; `Program`'s option match | same position (before the unknown-option check), same `--` handling | holds |
| Missing value | `argumentIndex >= args.Length` | `Program.cs:134` (`optionExpectingValue != null` at the end) and `TryParsePasses`/`TryParseMaximumIterations` | same condition: the option is the last argument | holds |
| Repeated option | `state.LanguageVersionSpecified` | `PassesSpecified` / `IterationsSpecified` | the tools' convention (change doc decision); `reihitsu-format` lets the last value win, deliberately different and recorded | holds |
| Accepted value and its resolution | `LanguageVersionFacts.TryParse(value)`, `Resolve`, `ExceedsMaximum` | `Program.ApplyOptionValue` | identical calls on the same value | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Newest supported version and the lowering | `rg "LanguageVersionResolver\.\|LanguageVersionFacts" src` outside the formatter: `Program.cs`, `PipelineTraceCommand.cs`, `CodeFixRunCommand.cs`, each only calling `LanguageVersionResolver`; no copy of `CSharp15` or of the clamp. The fixture runner reaches the resolver through `InternalsVisibleTo("Reihitsu.Tooling")` (decision 5) | one owner |
| Parse options of the tools | trace: `TracePasses`; runner: `HasSyntaxErrors`, `CreateDocument` — every parse site of each tool takes the one resolved value | one value per run |
| Warning sentence and invalid-value hint | three statements: `Program.cs:51`/`:217` (`reihitsu-format`), `PipelineTraceCommand.cs:66`/`:614`, `CodeFixRunCommand.cs:68`/`:493` | per-tool wording fixed by the change doc; decision 10 keeps them local. Not a finding |
| Argument parsing of the option | `TryParseLanguageVersion` in each tool, mirroring the existing separate `--passes`/`--max-iterations` parsers; the two tools share no assembly but the formatter | accepted duplication of the tools' convention, not of a policy |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| trace `ExecuteAsyncBeforeCSharp11KeepsTheInterpolationAsWritten` (3 rows) | under C# 10 the trace formats for C# 10, wherever the option stands | any phase change (exact full output asserted) | exact stdout, empty stderr; oracle `format.sh --check --lang-version 10` | adequate |
| trace `ExecuteAsyncWithoutLangVersion…` and `ExecuteAsyncFromCSharp11TracesAsWithoutTheOption` (6 rows) | the default and every value resolving to 11..15 trace the same; the default moves the operator | different output or a warning | full-output equality against the default run; together with the C# 10 test, both sides of the 10/11 boundary (FC-003, FC-033) | adequate |
| trace `ExecuteAsyncPreviewWarns…` | `preview` lowers with exactly one warning | missing/extra stderr line, different stdout | exact stderr, full-stdout equality | adequate |
| trace `ExecuteAsyncUnionUnderCSharp14IsSkipped…` / `ExecuteAsyncTypeNamedUnionUnderCSharp14IsTraced` | every pass parses with the version | skip decision not following the version, in either direction | exact output for the skip; both arms asserted | adequate |
| trace and apply-fix `ExecuteAsyncInvalidLangVersionArgumentsReturnError` (5 rows each) | each argument error, its message, usage, exit 2, nothing read | different message, missing usage, any stdout | exact stderr; a missing path/directory proves the error precedes the read | adequate |
| trace/apply-fix `…AfterDoubleDash…`, `…HelpNamesTheLangVersionOption` | positional after `--`; usage names the option | option consumed after `--`; usage text differs | exact stderr / stdout | adequate |
| apply-fix `ExecuteAsyncBeforeCSharp11FormatsTheFixResultForThatVersion` (3 rows) | the formatter inside the fix reads the project's version | the anonymous method's braces broken onto their own lines | outcome of both arms plus the LF arm's hunk; the CRLF arm's hunk is not compared, but both arms receive the same `languageVersion` argument in one loop (`CodeFixRunCommand.cs:176-178`), so no arm-specific path exists to miss | adequate |
| apply-fix `ExecuteAsyncAnalyzerReportsForTheProjectVersion` | the analyzer sees the project's version | `no-fix-offered` under 10 | both arms, both sides of 10/11; oracle RH5417-005 tests | adequate |
| apply-fix `ExecuteAsyncUnionUnderCSharp14IsAParseError` / `ExecuteAsyncTypeNamedUnionUnderCSharp14IsFixed` | all three parses use the version | any one parse site on 15: the fixture check gives `parse-error`, the project parse gives no RH3202 report on the 15-misparsed method, the result check gives `invalid-result` for `union M(int a) { return null; }` — each fails the `fixed` assertion | both arms, both directions | adequate (FC-016 observed per site) |
| apply-fix `ExecuteAsyncFromCSharp11RunsAsWithoutTheOption`, `ExecuteAsyncPreviewWarnsAndRunsAsWithoutTheOption`, `ExecuteAsyncWithoutLangVersion…` | as for the trace | as for the trace | full-output equality, exact warning | adequate |
| `FixtureRunnerTests` (13 call sites) | unchanged behavior | — | argument `LanguageVersion.Latest` only, the value the runner used before; no assertion or input changed (decisions 8, 9) | adequate |

The default-path change (decision 6) is not observable on Roslyn 5.12.0, which maps `Default`/`Latest` to C# 15 = the maximum; no test can pin it today, as `03-spec-author.md` and `04-test-implementer.md` state.

### Failure classes dismissed
- FC-001, FC-002, FC-044: the new predicates are argument matches; see *Guard scope*.
- FC-003: the 10/11 boundary, the 14/15 boundary (union), the value-vs-option boundary (`--passes` as value), and the `--` boundary are each tested on both sides.
- FC-015: see *Policy ownership*; no copy of the clamp.
- FC-016: all four parse uses (trace pass; runner fixture check, project, result check) take the version, each observed by a test.
- FC-031: every changed method's summary and parameter docs describe the version; `CreateDocument`'s summary names it; the inline comment on the missing value is accurate (`TryParse(null)` yields the default, as `Program.cs:134` also states). No `TODO`, no issue numbers in source.
- FC-004 to FC-014, FC-018 to FC-030, FC-036, FC-038 to FC-043: no phase, rule, or code fix changes; the tools only pass a version the formatter and analyzers already honor through `ParseOptions`.
- FC-017: see *Assertion adequacy*.
- FC-032: no project result reused across a compiled change.
- FC-033, FC-035: every version test uses a fixture whose result differs between the version and the default and asserts both; expected values come from `format.sh --lang-version`, FMT-BREAK-044, RH5417-005, and the CLI-004 union fixtures, not from the new code.
- FC-034: the tests use the issue's own shape (an interpolation hole below C# 11) and the change doc's examples.
- FC-037: no comment-only change claimed.

### Architecture findings
_None._

### Hints
- `InternalsVisibleTo("Reihitsu.Tooling")` (`src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`) opens every formatter internal to the tooling assembly, not only `LanguageVersionResolver`. Today the fixture runner reaches the formatter only through the code fixes it runs, which is what `scripts/README.md` (*The code-fix fixture runner*) describes as "the code-fix surface"; no architecture test keeps `Reihitsu.Tooling` from calling a phase or a structural transform directly. Not a defect of this change (no such call exists, and decision 5 chose this over a copy or a public widening); noted so a later change to the runner keeps going through the code fix.

### Follow-up candidates
_None._
