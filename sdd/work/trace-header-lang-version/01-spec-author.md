Role: spec author
Commit: b8b7365
Verdict: #1924: change doc written; #1949: change doc written

Read: `squad/roles/spec-author.md`, `squad/README.md`, `sdd/README.md`, `00-lead.md`, both issues and their comments (one comment on #1949: the two issues run together; none on #1924). My prompt carried no intended fix or suspected cause.

## thoenissen/Reihitsu#1924

### Changed requirements

None. The pipeline trace has no spec: `sdd/specs/cli/reihitsu-format.md` excludes it as a non-goal, and the precedent `sdd/changes/2026-11/trace-hiding-skip.md` handled a trace behavior change the same way (change doc, `Affected requirements: none`, `scripts/README.md` as the trace's documentation). CLI-024 is unchanged and keeps naming only `reihitsu-format`.

Written:
- `sdd/changes/2026-11/trace-diff-header.md` (Status: Draft) — the trace's phase-diff header names the file as CLI-024 decides for `--dry-run`, applied to the trace's full path and the trace process's current directory.
- `scripts/README.md`, *Formatter pipeline trace* — one clause stating the header.

Premise check (reproduced on b8b7365, in a scratch directory, with `scripts/trace.sh`): a full path prints `--- a//<full path>` / `+++ b//<full path>`; `src/A.cs` prints `--- a/src/A.cs`; `./src/A.cs` prints `--- a/./src/A.cs`; run in `src/`, `../src/A.cs` prints `--- a/../src/A.cs`. The trace process's current directory is the caller's (a relative path resolves), so CLI-024's "inside the current directory" applies unchanged.

The issue's note that `PipelineTraceCommandTests` "pins the `--- a/` header for relative paths" is wrong: `ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched` passes a full path in a temporary directory (`fixture.CreateFile(...)` returns a full path) and asserts `Assert.Contains("--- a/", trace)`. Under the change doc that file is outside the test process's current directory, so the header becomes `--- <full path>` and that assertion has to change. `ExecuteAsyncMixedLineEndingsReportsCountFallback` asserts `DoesNotContain("--- a/")` and is unaffected.

### Examples for the tests

No requirement IDs (no spec). The tests derive from the change doc's *Visible output changes* 1 to 3: a full path inside the current directory (`a/<relative>`), a relative path with `./` and `../` segments (normalized), and a file outside the current directory (full path, no prefixes). A test that sets the process's current directory must run without parallelization, as the CLI-024 tests do.

### Output questions

_None._ The trace is repository tooling, not one of the visible surfaces `squad/README.md` lists; the precedent change doc states "None" for the same reason. The choice between changing the header and recording today's header is therefore internal (below).

### Internal choices

- Change the header to CLI-024's rule rather than record `a/<path as given>` as a decision (the issue's two options). Alternatives in the change doc: keep as given; drop only the doubled slash; always full path.
- The trace follows CLI-024 "through the same code" as `--dry-run`, so the rule is stated once (`squad/README.md`, *Architecture findings*: one rule in more than one place). The change doc names no type; which component is the implementer's.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1949

### Changed requirements

None. Neither tool has a spec (`sdd/README.md`: `Reihitsu.Tooling` gets no spec; the trace is a CLI-spec non-goal).

Written:
- `sdd/changes/2026-11/tooling-lang-version.md` (Status: Draft) — `--lang-version <version>` for `scripts/trace.sh` and `scripts/apply-fix.sh`: position, values, resolution, what the version applies to, the three argument errors with their exact messages, the usage lines.
- `scripts/README.md`, *Formatter pipeline trace* and *The code-fix fixture runner* — the option in each synopsis and one paragraph each.

Not written (executable tooling, the implementer's): the header comments of `scripts/trace.sh` and `scripts/apply-fix.sh` that repeat the synopsis. Both wrappers already forward every argument except `--no-install` (and, for apply-fix, everything after `--`), so `--lang-version 10` reaches the C# commands without a wrapper change.

Premise check (b8b7365): `scripts/trace.sh <file> --lang-version 10` → `trace: unknown argument '--lang-version'.`, exit 2; `scripts/apply-fix.sh RH5001 <dir> --lang-version 10` → `apply-fix: unknown argument '--lang-version'.`, exit 2. `scripts/format.sh src/H.cs --dry-run --lang-version 10` keeps the interpolation hole of change-doc example 1 as written; without the option (and with 15) it moves `+` to the hole's second line. Default unchanged: a probe against Roslyn 5.12.0 shows `LanguageVersion.Latest` and `CSharpParseOptions.Default` both map to `CSharp15`, which is `LanguageVersionResolver.MaxSupportedLanguageVersion`; so today's trace and runner already parse as 15.0, the default the change doc keeps.

Probe facts the test implementer should know before choosing fixtures: under `CSharpParseOptions(LanguageVersion.CSharp10)`, none of these produce a parse diagnostic in Roslyn 5.12.0 — a line break in a non-verbatim interpolation hole, a raw string literal, `required`, an expression body. Language-version feature errors for them come from binding, not from parsing, so a "parse-error under an old version" fixture needs a construct whose parse actually depends on the version; the visible effect of `--lang-version` in both tools is the version-dependent formatting (change-doc examples 1 and 2), e.g. RH3202 on the fixture of example 2, whose result under 15 is quoted there (run on b8b7365).

### Examples for the tests

No requirement IDs. Tests derive from the change doc's *What* bullets and *Visible output changes* 1 to 4: trace and runner with `--lang-version 10` on the hole fixtures (result differs from the default run); invalid value, missing value, repeated option (exact messages, exit 2, usage on stderr, for each tool); `preview` (warning line on stderr, result as without the option); the option after `--` is positional.

### Output questions

_None._ Both tools are repository tooling; without the option their output is unchanged.

### Internal choices

- `apply-fix` resolves the value like `reihitsu-format` (lowering `preview` to 15.0 with a warning) rather than passing `preview` unlowered to the fixture project, as a real project with `LangVersion preview` would. Recommendation in the change doc; alternative recorded there.
- A repeated `--lang-version` is an error in both tools (their convention for `--passes`/`--max-iterations`) rather than last-wins as in `reihitsu-format`.
- Error messages use the tool prefix and lower-case start of the tools' existing errors; the warning repeats CLI-004's sentence verbatim behind the prefix (`trace: Warning: …`).
- The trace now parses with the resolved version (not Roslyn's default options), so its syntax-error skip follows the version like the CLI's.
- No line in either tool's output names the version (non-goal of the change doc).

### Follow-up candidates

_None._

## Whole run

- No spec requirement added, changed, or retired; no requirement ID, `Uses` row, or code owner touched, so `scripts/test.sh --project architecture` was not run (no change it checks).
- Files written: `sdd/changes/2026-11/trace-diff-header.md`, `sdd/changes/2026-11/tooling-lang-version.md`, `scripts/README.md`, this handover.
- Both change docs are `Draft`, list no visible output change, and need no maintainer answer; the lead may accept them.
- The two issues have different causes, so each has its own change doc.
- Rule pages: none in scope.
