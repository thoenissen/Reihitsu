Role: skeptic
Commit: d686d38
Verdict: #1924: proceed; #1949: proceed

Read: `squad/roles/skeptic.md`, `squad/README.md`, `00-lead.md`, `01-spec-author.md`, both change docs, both issues and the one comment (on #1949: the two run together). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Fixtures for every command below were in a scratch directory outside the repository (shortened to `/s`), run on d686d38, and deleted afterwards.

## thoenissen/Reihitsu#1924

### Premise

Real. Caller: `scripts/trace.sh` (file-based app `scripts/trace/trace.cs` → `PipelineTraceCommand.ExecuteAsync`); it is not reachable through `reihitsu-format` (no reference to `PipelineTraceCommand` in `src/` outside its own file). The squad itself is the main caller, and roles keep fixtures outside the repository, so a full path outside the current directory is the common input, not a corner.

Mechanism in the code: `PipelineTraceCommand.ExecuteCoreAsync` passes `filePath.Replace('\\', '/')` (the path as given) as `displayPath`, and `WritePhaseChange` calls `DiffGenerator.Generate(displayPath, …)`, which writes `a/<displayPath>` unconditionally (`src/Reihitsu.Cli/DiffGenerator.cs:32-35`). The CLI-024 rule already exists in one place, `DefaultDiffGenerator.Generate(fullPath, …)` (`src/Reihitsu.Cli/Abstractions/DefaultDiffGenerator.cs`), fed by `DefaultFileSystem.GetFullPath` — the same `Path.GetFullPath` the trace already computes for its `Tracing` line.

Reproduced, run in `/s/work` with `src/H.cs`:

```shell
scripts/trace.sh /s/work/src/H.cs --passes 1
```

```text
Tracing /s/work/src/H.cs (maximum passes: 1)
== Pass 1: LineBreakPhase ==
--- a//s/work/src/H.cs
+++ b//s/work/src/H.cs
```

(exit 1). The spec author's `./` and `../` reproductions match the code above.

### Mechanism and candidates

_N/A_ as a bug mechanism; the change replaces the trace's own header rule (`a/<path as given>`) with the existing CLI-024 owner. Candidate path shapes, from the one input `filePath` → `GetFullPath`: full path inside the current directory; relative path without dot segments; relative path with `./`; relative path with `../` that lands inside; path outside the current directory (full or `../`). Every other CLI-024 cell (root as current directory, same-name-prefix sibling, case, backslash in a Linux name, symbolic links, deleted current directory) is decided inside `DefaultDiffGenerator` and is already pinned by `tests/Reihitsu.Cli.Test/EndToEnd/CliDryRunDiffHeaderEndToEndTests.cs` (16 tests); the trace needs no second copy of them.

Sibling checked (FC-016): the code-fix fixture runner writes its own diff header through `src/Reihitsu.Tooling/UnifiedDiffWriter.cs` with the fixture's path relative to the fixture directory (`--- a/E.cs` in the run below); it never gets a full path, so it does not share the defect. Holds.

### Breaking inputs

- FC-015 (one rule, one owner): the header must come from `DefaultDiffGenerator` (or whatever single owner CLI-024 has), not from a second "is it inside the current directory" check in the trace. A copy would be a third statement of the header rule next to CLI-024's code and the change doc.
- FC-031: the XML summary of `DiffGenerator.Generate` and the `displayPath` parameter docs in `PipelineTraceCommand` (`TracePasses`, `ExecuteFormattingPass`, `WritePhaseChange`) describe today's path-as-given header.
- FC-003: inside and outside are two tests.

Inputs the test implementer must cover (all on `PipelineTraceCommand.ExecuteAsync`; a test that sets the current directory needs `[DoNotParallelize]`, as `CliDryRunDiffHeaderEndToEndTests` does, because `tests/Reihitsu.Cli.Test` runs methods in parallel):

| Input | Expected header | Existing test |
|---|---|---|
| full path in a temp directory, current directory elsewhere | `--- <full path>` / `+++ <full path>` | `ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched` passes exactly this (`TemporaryDirectoryFixture` is under `Path.GetTempPath()`) and asserts `Contains("--- a/")`; it becomes wrong and must be updated, not duplicated |
| full path inside the current directory | `--- a/<relative>` | none found |
| relative path with `./` or `../` that lands inside | `--- a/<normalized relative>` | none found |

`ExecuteAsyncMixedLineEndingsReportsCountFallback` asserts `DoesNotContain("--- a/")` on a line-ending-only change; after the change a full-path header without `a/` could appear there only if a diff were printed, which that test asserts is not the case — it stays valid but no longer proves "no diff" for an outside file. The test implementer should check that its assertion still means what it says (FC-017).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PipelineTraceCommand` (trace header) | must change |
| `DefaultDiffGenerator` / `DiffGenerator` | must stay (behavior); `DiffGenerator.Generate`'s doc may need FC-031 attention only if the trace was its documented caller |
| `reihitsu-format` `--dry-run` (CLI-024) | must stay |
| `scripts/README.md` | changed by the spec author |
| analyzers, code fixes, formatter, rule pages | not involved |

### Refuse option

No input class to refuse; this is not a formatter or code-fix shape, so declaring a shape unsupported is not an option. The "refuse" option is the issue's option B: keep `a/<path as given>` and record it. Cost: zero production lines, but a second header rule next to CLI-024 and a header that names no file for the common full-path input. The requested change is a few lines that reuse the existing owner and delete the trace's own rule (`displayPath` plumbing). The change is cheaper in rules than refusing it. Simpler options: none.

### Output questions

_None._ The trace is repository tooling, not `reihitsu-format` (`sdd/specs/cli/reihitsu-format.md`, non-goals) and none of the visible surfaces `squad/README.md` lists.

### Internal choices

- Reuse `DefaultDiffGenerator` (it reads the current directory per call; the trace calls it once per changed phase, which is cheap) rather than computing the inside/outside decision once in the trace. Recommendation: reuse it; any second computation is FC-015.

### Issue goals

- "or the trace command's spec states that the trace keeps `a/<path>`": drop — there is no trace spec, and the change doc takes the other branch. No visible output.
- The note "`PipelineTraceCommandTests` pins the `--- a/` header for relative paths today, so a change to full paths must leave that header as it is": wrong premise (the test passes a full path, see above). Relative paths without dot segments keep their header; relative paths with `./` or `../` change (change doc, case 2). Drop the constraint as stated. No visible output.
- "a test covers the chosen header for a full path": keep.
- Severity: the issue rates the value low. It is low, but the change also removes a second header rule; it does not justify more than the few lines above.

### Architecture findings

_None._ A copy of CLI-024's inside check in the trace would be one; reusing the owner avoids it.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1949

### Premise

Real. Callers: `scripts/trace.sh` → `PipelineTraceCommand`, and `scripts/apply-fix.sh` → `scripts/apply-fix/apply-fix.cs` → `Reihitsu.Tooling.CodeFixRunCommand`/`FixtureRunner`. Both wrappers forward unknown arguments (apply-fix.sh up to and including `--`; trace.sh all but `--no-install`), so no wrapper logic change is needed beyond their header-comment synopsis.

Parse sites today: the trace parses each pass with `CSharpSyntaxTree.ParseText(currentContent, path: fullPath, …)` (Roslyn default options) and builds `FormattingContext` from `LanguageVersionResolver.Resolve(syntaxTree.Options)`; the fixture runner parses with `new CSharpParseOptions(LanguageVersion.Latest)` in two places: `FixtureRunner.HasSyntaxErrors` (called for `parse-error` at line 76 and `invalid-result` at line 123) and `CreateDocument`'s project options (line 437), which the analyzer and, through `document.Project.ParseOptions`, the formatter inside the fix read (`ReihitsuFormatter.cs:228/398/642`). That is three parse uses, matching the change doc's *What the version applies to*.

Reproduced:

```shell
cd /s/work && scripts/trace.sh src/H.cs --lang-version 10
```

```text
trace: unknown argument '--lang-version'.
usage: trace <file> [--passes <positive-integer>]
       trace --help
```

exit 2; `scripts/apply-fix.sh RH5417 fx5417 --lang-version 10` gives `apply-fix: unknown argument '--lang-version'.`, exit 2.

The version does change the result of both tools' inputs, settled with the existing entry points (`src/H.cs` is change-doc example 1):

```shell
scripts/format.sh src/H.cs --dry-run --verbose --lang-version 10   # hole kept: only the final newline changes
scripts/format.sh src/H.cs --dry-run --verbose                     # "-        return $\"{a +" / "+        return $\"{a" / "+                  + 1}\";"
```

Under C# 10 the file is formatted, not skipped for syntax errors, so a line break in a non-verbatim hole is not a parse error in the Roslyn the repository uses; the spec author's probe agrees.

### Mechanism and candidates

_N/A_ (new option, not a bug).

### Breaking inputs

- FC-015 (one owner of the clamp): `LanguageVersionResolver` is `internal` to `Reihitsu.Formatter`, whose `InternalsVisibleTo` list names `Reihitsu.Analyzer.CodeFixes`, `Reihitsu.Cli`, and test assemblies, but not `Reihitsu.Tooling` (`src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`). The fixture runner cannot call `Resolve`/`ExceedsMaximum` today. A private copy of the clamp in `Reihitsu.Tooling` would be a second owner of the newest supported version (the type's own summary calls itself "the single owner of the supported maximum and of the clamp"). See *Internal choices*.
- FC-016 (sibling sites): all three fixture-runner parse uses and the trace's per-pass parse must take the version; missing `HasSyntaxErrors` would let `parse-error`/`invalid-result` disagree with the project. `HasSyntaxErrors` is `public` and called directly by `FixtureRunnerTests` (line 99).
- FC-033 (a reproduction the harness normalizes away): a test of `--lang-version` whose fixture gives the same result with and without the option proves nothing. Every version test must use a fixture whose result differs between the default and the value, and assert both.
- FC-035: the expected C# 10 results must come from an oracle independent of the new code. Two exist:
  - trace: `scripts/format.sh <file> --lang-version 10` (the CLI's existing path, `CliLanguageVersionEndToEndTests`), whose result the trace's last pass must equal (change doc, example 1).
  - fixture runner: RH5417 on the fixture below. `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.LambdaBodyInsideAnInterpolationIsNotReportedBeforeCSharp11` (RH5417-005) pins "not reported under C# 10" for exactly this input, and `LambdaBodyInsideAnInterpolationIsReportedFromCSharp11` pins the report from C# 11. Observed default run:

    ```csharp
    using System;

    class C
    {
        string M() => $"{((Func<int>)(() => { return 1; }))()}";
    }
    ```

    `scripts/apply-fix.sh RH5417 fx5417` → `== L.cs [LF] == no-fix-offered`, `== L.cs [CRLF] == no-fix-offered`, exit 0. With `--lang-version 10` the analyzer's own spec gives `no-diagnostic` for both arms. This proves the project's parse options reach the analyzer, which the RH3202 example of the change doc (formatter inside the fix) does not.
  - RH3202 on change-doc example 2: default observed `fixed (1 action(s), 1 iteration(s))` with `return $"{a` / `+ 1}";` (quoted in the change doc). The C# 10 result is not stated anywhere; FMT-PIPE-008 with the interpolation requirement of `line-breaks.md` (C# 10 or earlier: no line break between the hole's braces) is its oracle, not the new code's output.
- Argument parsing (FC-003 boundaries): the value is always the next argument (`--lang-version --passes` → invalid value `--passes`); `--lang-version` last → missing value; given twice → error; after `--` → positional; empty string `""` → invalid (observed for the CLI: `scripts/format.sh src/H.cs --dry-run --lang-version ""` → `Invalid value for --lang-version: ''. …`, exit 2; "exactly when `reihitsu-format` accepts" therefore rejects it); `preview` → warning, result as default; `latest`, `default`, `latestmajor` → accepted (observed for the CLI: `default` and `latestmajor` format as the default).

| Input | Existing test |
|---|---|
| trace / apply-fix with `--lang-version 10` on a version-dependent fixture | none found |
| invalid, empty, missing, repeated value; after `--` (each tool) | none found; `ExecuteAsyncInvalidArgumentsAndMissingFilesReturnError` (trace) and the `usage: apply-fix` / `unknown argument` tests in `CodeFixRunCommandTests` are where they extend |
| `preview` warning (each tool) | none found |
| usage line names the option | `CodeFixRunCommandTests` line 504 asserts only `Contains("usage: apply-fix")`; no test pins the trace usage text |
| CLI's own value set and resolution | `ProgramTests` / `CliLanguageVersionEndToEndTests` (unchanged; not re-tested in the tools) |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PipelineTraceCommand` (arguments, per-pass parse options, usage) | must change |
| `CodeFixRunCommand` (arguments, usage) and `FixtureRunner` (three parse uses) | must change |
| `LanguageVersionResolver` | must stay the only owner of the clamp; its visibility to `Reihitsu.Tooling` must change or the clamp gets a copy (see *Internal choices*) |
| `scripts/trace.sh`, `scripts/apply-fix.sh` | header-comment synopsis only; argument forwarding already works |
| `reihitsu-format` (`Program`, CLI-001/CLI-004) | must stay |
| formatter phases, analyzers, code fixes, rule pages | not involved |

### Refuse option

No input class or formatter shape; declaring a shape unsupported is not an option. Refusing means not adding the option: roles keep writing throwaway probes for version-dependent questions (the issue's motivation, and this handover needed RH5417-005's tests as a stand-in for exactly that). The requested change is two argument parsers of a few lines each calling `LanguageVersionFacts.TryParse` and `LanguageVersionResolver`, the version threaded to four parse sites, and one `InternalsVisibleTo` line. Proportionate; no simpler option.

### Output questions

_None._ Both tools are repository tooling; without the option their output is unchanged.

### Internal choices

1. **Where the fixture runner gets the clamp.** A — add `InternalsVisibleTo("Reihitsu.Tooling")` to `Reihitsu.Formatter` (one production line; the precedent is `Reihitsu.Cli`). B — make `LanguageVersionResolver` public. C — copy the clamp into `Reihitsu.Tooling`. D — the change doc's rejected alternative: pass the parsed value unlowered and let the formatter clamp it (FMT-PIPE-008); it still needs `ExceedsMaximum` for the warning, so it does not avoid the dependency. Recommendation: **A**; C is FC-015, B widens the shipped assembly's surface for a tool.
2. **The default is the resolved newest supported version, not "as today".** The change doc's *What* says without the option the tools use "the newest version the formatter supports, today 15.0", and its last paragraph says they "behave as today: the trace parses with Roslyn's default parse options and the fixture runner with `LanguageVersion.Latest`". These agree only while Roslyn 5.12.0 maps `Default`/`Latest` to 15 (the spec author's probe); after a Roslyn upgrade that maps them to 16, `format.sh` (which parses with `CSharpParseOptions.Default.WithLanguageVersion(<resolved>)`, `FormatCommandHandler.cs:143`) and the tools would parse differently, against the issue's "same default as `format.sh`". Recommendation: the tools always parse with the resolved version — with the option, `Resolve(value)`; without it, `MaxSupportedLanguageVersion` — so there is one code path, and the change doc's last paragraph states that as the default, keeping "Roslyn maps both to 15 today" as the reason nothing changes now. No visible output either way today.
3. The spec author's choices (error on a repeated option; lower-case tool-prefixed messages; warning sentence repeated behind the prefix; no version line in the output): agree with all; each is tooling-internal.

### Issue goals

- "the same values and default as `format.sh`": keep, read as internal choice 2 (the default is the resolved maximum, not Roslyn's `Latest`).
- "fail with a clear message on an unknown value": keep; the change doc adds missing and repeated values, which the issue did not ask for but which follow the tools' existing `--passes`/`--max-iterations` convention.
- Lane "tooling" in the issue body: superseded by the lead's lane decision (`PipelineTraceCommand` is in `src/Reihitsu.Cli`, and internal choice 1 A touches `src/Reihitsu.Formatter`); behavior-change admits both.

### Architecture findings

_None_ in the code as it stands. Internal choice 1 C would create one (two owners of the supported maximum).

### Follow-up candidates

_None._

## Whole run

- No formatter, analyzer, or code-fix behavior is in question; no output question for the maintainer.
- Production paths the run will touch, by the evidence above: `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs`, `src/Reihitsu.Tooling/CodeFixRunCommand.cs`, `src/Reihitsu.Tooling/FixtureRunner.cs`, and, under internal choice 1 A, `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`; plus the two wrapper header comments. The last one is not named in the handovers so far; it is one line.
- Commands run: `scripts/trace.sh` (full path; `--lang-version 10`), `scripts/format.sh --dry-run` (with `--lang-version 10`, `""`, `default`, `latestmajor`, and without), `scripts/apply-fix.sh` (RH5417 and RH3202 fixtures; `--lang-version 10`). No test run was needed.
