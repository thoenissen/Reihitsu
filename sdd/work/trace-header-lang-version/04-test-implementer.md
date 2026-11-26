Role: test implementer
Commit: 9fc3495
Verdict: #1924: tests written; #1949: tests written

Read: `squad/roles/test-implementer.md`, `squad/README.md`, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, both change docs, `scripts/README.md`, FMT-BREAK-044. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No requirement covers either tool (both change docs: `Affected requirements: none`), so no test carries a `[Requirement]` tag; CLI-024 is a requirement of `reihitsu-format` only and is not tagged on the trace tests.

Every new test was run on 9fc3495 and fails for the stated reason; every characterization arm passes. Tests call `PipelineTraceCommand.ExecuteAsync` and `CodeFixRunCommand.ExecuteAsync` only, never `FixtureRunner.RunAsync`/`HasSyntaxErrors`, so the implementer is free in how the version reaches the runner.

## thoenissen/Reihitsu#1924

### Tests

File: `tests/Reihitsu.Cli.Test/Unit/PipelineTraceCommandDiffHeaderTests.cs` (new, `[DoNotParallelize]`: every test sets the process's current directory, as `CliDryRunDiffHeaderEndToEndTests` does). Each asserts the unchanged `Tracing <full path>` line and that every `---`/`+++` line of every phase diff is the expected pair.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FullPathInsideTheCurrentDirectoryPrintsTheRelativeHeader` | change doc example 1 | `PipelineTraceCommand.ExecuteAsync` in a set current directory | fails: `--- a//tmp/.../work/src/A.cs` instead of `--- a/src/A.cs` |
| `RelativePathInsideTheCurrentDirectoryPrintsTheRelativeHeader` | example 2 (unchanged case), FC-003 other side | same | passes |
| `RelativePathWithCurrentDirectorySegmentPrintsTheNormalizedHeader` | example 2 (`./`) | same | fails: `--- a/./src/A.cs` |
| `RelativePathWithParentSegmentLandingInsidePrintsTheNormalizedHeader` | example 2 (`../src/A.cs` from `src` → `a/A.cs`) | same | fails: `--- a/../src/A.cs` |
| `RelativePathOutsideTheCurrentDirectoryPrintsTheFullPath` | example 3 (`../other/B.cs`) | same | fails: `--- a/../other/B.cs` |
| `FullPathOutsideTheCurrentDirectoryPrintsTheFullPath` | example 3 (full path outside) | same | fails: `--- a//tmp/.../other/B.cs` |
| `PipelineTraceCommandTests.ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched` (changed) | change doc decision: existing test updated | `PipelineTraceCommand.ExecuteAsync` | fails: asserted `--- a/` is now `--- <full path>`/`+++ <full path>` plus `DoesNotContain("--- a/")`; the temp file lies outside the test process's current directory |
| `PipelineTraceCommandTests.ExecuteAsyncMixedLineEndingsReportsCountFallback` (changed) | FC-017 (skeptic's note) | same | passes: `DoesNotContain("--- a/")` became `DoesNotContain("--- ")` and `DoesNotContain("@@ ")`, so it still proves "no diff" once an outside file's header has no `a/` |

### Not covered, and why

- The CLI-024 cells decided inside `DefaultDiffGenerator` (root as current directory, same-name-prefix sibling, case, backslash in a Linux name, symbolic links, deleted current directory): pinned by `CliDryRunDiffHeaderEndToEndTests` (16 tests); the change doc requires the trace to reach them through the same code, so no second copy (skeptic, *Mechanism and candidates*).
- Line endings: the header does not depend on them; `ExecuteAsyncPreservesDetectedLineEndings` covers CRLF tracing.
- Failure-class walk: FC-015 (one owner) is a code-structure property, not testable here; the auditor checks it. Formatter/code-fix classes (idempotency, Fix All, comments/directives in gaps, unsupported shapes, renamer) do not reach a diff header.

### Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~PipelineTraceCommandDiffHeaderTests|FullyQualifiedName~PipelineTraceCommandTests"
```

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1949

### Tests

Files: `tests/Reihitsu.Cli.Test/Unit/PipelineTraceCommandLanguageVersionTests.cs` (trace) and `tests/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandLanguageVersionTests.cs` (fixture runner), both new. Every version test uses a fixture whose result differs between the version and the default and asserts both sides (FC-033).

Oracles (FC-035), all independent of the new code: the C# 10 trace result is `scripts/format.sh --check --lang-version 10` on the same file (0 of 1 need formatting; 1 of 1 without the option), run on 9fc3495; the C# 10 RH3202 result is FMT-BREAK-044 (no line break written into a non-verbatim interpolation) and is format-stable under `format.sh --check --lang-version 10`; RH5417 under C# 10/11 is RH5417-005 (`LambdaBodyInsideAnInterpolationIsNotReportedBeforeCSharp11` / `...IsReportedFromCSharp11`); the union fixtures are the ones `CliLanguageVersionEndToEndTests` pins for CLI-004 (a union is a syntax error under 14; a type named `union` as return type is one under 15). The default arms were run with `scripts/trace.sh`/`scripts/apply-fix.sh` on 9fc3495.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| trace `ExecuteAsyncBeforeCSharp11KeepsTheInterpolationAsWritten` (`10` behind, `10` in front of the file, `10.0`) | change doc example 1, *Where*; FC-033 | `PipelineTraceCommand.ExecuteAsync` | fails: exit 2 (unknown argument); expected exactly `Tracing …`, `Pass 1`, `Stable after pass 1 (0 phase changes observed).` |
| trace `ExecuteAsyncWithoutLangVersionFormatsTheInterpolationForTheNewestVersion` | example 1 default side | same | passes |
| trace `ExecuteAsyncFromCSharp11TracesAsWithoutTheOption` (`11`, `15`, `15.0`, `latest`, `default`, `latestmajor`) | *Values and default*; FC-003 boundary 10/11 | same | fails: exit 2; expected output identical to the run without the option, empty stderr |
| trace `ExecuteAsyncPreviewWarnsAndTracesAsWithoutTheOption` | example 4 | same | fails: exit 2; expected stderr exactly the warning line, stdout as without the option |
| trace `ExecuteAsyncUnionUnderCSharp14IsSkippedForSyntaxErrors` | *What the version applies to* (trace parses every pass with it) | same | fails: exit 2 with 14; default arm (traced) holds today |
| trace `ExecuteAsyncTypeNamedUnionUnderCSharp14IsTraced` | same, other direction | same | fails: exit 2 with 14; default arm (skipped for syntax errors) holds today |
| trace `ExecuteAsyncInvalidLangVersionArgumentsReturnError` (`99`, `""`, `--passes` as value, missing value, repeated) | *Errors*, example 3; skeptic's boundaries | same (missing file, to show the argument error comes before the file is read) | fails: `trace: unknown argument '--lang-version'.` and the old usage; expected exact message + new usage lines, empty stdout, exit 2 |
| trace `ExecuteAsyncLangVersionAfterDoubleDashIsTheFilePath` | *Where* (after `--` positional) | same | passes (`trace: file not found: --lang-version`) |
| trace `ExecuteAsyncHelpNamesTheLangVersionOption` | *Usage* | same | fails: usage line lacks `[--lang-version <version>]` |
| apply-fix `ExecuteAsyncBeforeCSharp11FormatsTheFixResultForThatVersion` (`10` behind, `10` in front of the ID, `10.0`) | *What the version applies to* (formatter inside the fix), *Where*; FC-033 | `CodeFixRunCommand.ExecuteAsync` | fails: exit 2; expected `fixed (1 action(s), 1 iteration(s))` on both arms and the hunk with `return $"a {F(delegate { return 1; })} b";` on one line |
| apply-fix `ExecuteAsyncWithoutLangVersionFormatsTheFixResultForTheNewestVersion` | default side | same | passes |
| apply-fix `ExecuteAsyncFromCSharp11RunsAsWithoutTheOption` (`11`, `15`, `15.0`, `latest`, `default`, `latestmajor`) | *Values and default*; FC-003 | same | fails: exit 2; expected output identical to the default run |
| apply-fix `ExecuteAsyncAnalyzerReportsForTheProjectVersion` (`10` → `no-diagnostic`, `11` → `no-fix-offered`, none → `no-fix-offered`) | project parse options reach the analyzer; FC-003 | same | `10`, `11` fail with exit 2; the no-option row passes |
| apply-fix `ExecuteAsyncUnionUnderCSharp14IsAParseError` | fixture parse check (`parse-error`) | same | fails: no fixture output with 14; default arm (`fixed`) holds today |
| apply-fix `ExecuteAsyncTypeNamedUnionUnderCSharp14IsFixed` | all three parses (fixture check, project, `invalid-result` check: the fix result `union M(int a) { return null; }` is invalid under 15) | same | fails: exit 2 with 14; default arm (`parse-error`) holds today |
| apply-fix `ExecuteAsyncPreviewWarnsAndRunsAsWithoutTheOption` | *Values and default* (warning with prefix `apply-fix`) | same | fails: exit 2 |
| apply-fix `ExecuteAsyncInvalidLangVersionArgumentsReturnError` (`99`, `""`, `--max-iterations` as value, missing value, repeated) | *Errors* | same (missing directory) | fails: `apply-fix: unknown argument '--lang-version'.` |
| apply-fix `ExecuteAsyncLangVersionAfterDoubleDashIsTheFixtureDirectory` | *Where* | same | passes (`apply-fix: fixture directory not found: --lang-version`) |
| apply-fix `ExecuteAsyncHelpNamesTheLangVersionOption` | *Usage* | same | fails: usage line lacks the option |

The warning tests hard-code `15.0`, as `CliLanguageVersionEndToEndTests.PreviewWarning` does; a Roslyn upgrade that raises the newest supported version updates both. The usage tests pin the existing second line `       trace --help` / `       apply-fix --help`, which the change doc keeps.

### Not covered, and why

- The default parsing with the resolved version instead of Roslyn's `Default`/`Latest` (decision 6): indistinguishable on Roslyn 5.12.0 (both map to 15); `03-spec-author.md` says no test is required, and none is possible today.
- An `invalid-result` that depends only on the result check's version, separate from the other two parses: no fixture can make the fixture and the project parse under one version while only the result check differs; `ExecuteAsyncTypeNamedUnionUnderCSharp14IsFixed` covers the result check together with the other two (a 15-parsed result check would report `invalid-result`).
- Line endings: the fixture runner always runs LF and CRLF arms, and both are asserted where an outcome is asserted; the trace's version handling does not depend on line endings.
- Existing tests: `CodeFixRunCommandTests.ExecuteAsyncHelpPrintsUsageAtSuccessExitCode` (`Contains("usage: apply-fix")`) and `ExecuteAsyncRejectsUnknownOption` stay valid unchanged. `FixtureRunnerTests` calls `FixtureRunner.HasSyntaxErrors(source, token)` directly (line 99); if the implementer adds a version parameter to that public method, that call site needs a mechanical update, which goes through the lead.
- Failure-class walk: FC-015 (one owner of the clamp) and FC-016 (all parse sites) are structural; the version tests above observe every parse site, the clamp's owner is the auditor's check. Formatter/code-fix classes (idempotency of a rule, Fix All, gap trivia, unsupported shapes, renamer) are not changed by a tool option.

### Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~PipelineTraceCommandLanguageVersionTests"
scripts/test.sh --project tooling --filter "FullyQualifiedName~CodeFixRunCommandLanguageVersionTests"
```

### Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project cli` on 9fc3495 with these tests: 24 failed (exactly the new and changed tests listed as failing above), 443 passed, 1 skipped. `scripts/test.sh --project tooling`: 20 failed (the new failing tests above), 207 passed.
- Test files formatted with `scripts/format.sh`; RH1003 findings in the first draft fixed.
- No production file touched.
