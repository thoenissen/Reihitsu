Role: implementer
Commit: 21e6421
Verdict: #1924: tests pass; #1949: tests pass

Read: `squad/roles/implementer.md`, `squad/README.md`, `00-lead.md` (decisions 5, 6, 8), `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, both change docs. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1924

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` | `WritePhaseChange` writes the diff through `new DefaultDiffGenerator().Generate(fullPath, …)`, the CLI-024 owner; the `displayPath` parameter (`filePath.Replace('\\', '/')`) is removed from `ExecuteCoreAsync` → `TracePasses` → `ExecuteFormattingPass` → `WritePhaseChange`, which now carry `fullPath`; XML docs of the four methods updated (FC-031) | change doc `trace-diff-header.md` (CLI-024 rule, reused) |

### Policy owners
- `rg -n "class .*DiffGenerator|IDiffGenerator" src`: `DefaultDiffGenerator` (the inside/outside decision, CLI-024) and `DiffGenerator` (rendering). The trace now calls `DefaultDiffGenerator`; it holds no inside/outside check of its own. `DiffGenerator.Generate`'s doc ("path behind the `a/` and `b/` prefixes") still holds; its only caller is `DefaultDiffGenerator`.

### Production diff size
See *Whole run*.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~PipelineTraceCommand"` → `Passed! - Failed: 0, Passed: 49` (covers `PipelineTraceCommandDiffHeaderTests`, `PipelineTraceCommandTests`, and the #1949 trace tests).

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1949

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs` | `InternalsVisibleTo("Reihitsu.Tooling")` | decision 5 |
| `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` | `--lang-version` parsing (`TryParseLanguageVersion`: repeated, missing, invalid via `LanguageVersionFacts.TryParse`; resolution and warning decision via `LanguageVersionResolver.Resolve`/`ExceedsMaximum`); default `MaxSupportedLanguageVersion`; warning line on stderr after argument validation; every pass parses with `CSharpParseOptions.Default.WithLanguageVersion(resolved)` (as `FormatCommandHandler` does) and the `FormattingContext` takes the resolved version directly; usage line | change doc `tooling-lang-version.md`; decision 6 |
| `src/Reihitsu.Tooling/CodeFixRunCommand.cs` | same option parsing (`TryParseLanguageVersion`), the options tuple carries iterations, resolved version, exceeding value; warning; version passed to `FixtureRunner.RunAsync`; usage line | same |
| `src/Reihitsu.Tooling/FixtureRunner.cs` | `RunAsync` and `HasSyntaxErrors` take a `LanguageVersion`; all three parse uses (fixture `parse-error` check, project parse options in `CreateDocument`, `invalid-result` check) use it instead of `LanguageVersion.Latest`; docs updated | same, FC-016 |
| `scripts/trace.sh`, `scripts/apply-fix.sh` | header-comment synopsis names `[--lang-version <version>]` | same |
| `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerTests.cs` | arguments only: `LanguageVersion.Latest` added to the 11 `RunAsync` calls and the 2 `HasSyntaxErrors` calls, plus `using Microsoft.CodeAnalysis.CSharp;`; no assertion or input changed | decision 8 (see *Disputed tests or scope*) |

### Policy owners
- Clamp / newest version: `rg -n "LanguageVersion\.Latest|MaxSupportedLanguageVersion\b" src/Reihitsu.Tooling src/Reihitsu.Cli` → only references to `LanguageVersionResolver.MaxSupportedLanguageVersion` (Program, trace, apply-fix); no `LanguageVersion.Latest` left in either tool; the single owner stays `LanguageVersionResolver`.
- Message texts: `rg -n "is newer than the newest supported version|Expected a C# language version such as" src` → three statements each, in `Program.cs` (`reihitsu-format`, capitalized, no prefix), `PipelineTraceCommand.cs`, `CodeFixRunCommand.cs`. The change doc fixes different wording per tool (prefix, lower-case start), and the trace (Cli) and the runner (Tooling) share no assembly except the formatter, so I kept the strings local; see *Follow-up candidates* for the lead.
- Argument parsing of the option (missing / invalid / repeated) exists twice, mirroring how `--passes` and `--max-iterations` are already parsed separately in the two tools.

### Production diff size
See *Whole run*.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~PipelineTraceCommand"` → `Passed! - Failed: 0, Passed: 49`
- `scripts/test.sh --configuration Debug --project tooling --filter "FullyQualifiedName~CodeFixRunCommand|FullyQualifiedName~FixtureRunner"` → `Passed! - Failed: 0, Passed: 57`

### Disputed tests or scope
- Decision 8 covered the `HasSyntaxErrors` call in `FixtureRunnerTests`. `FixtureRunner.RunAsync` (also public, also called directly by `FixtureRunnerTests`, 11 times) needed the same mechanical argument; the test implementer's handover missed those call sites. I applied the same rule (argument `LanguageVersion.Latest`, which is what the runner used before, so each test runs exactly as before; no assertion or input touched). Alternative that avoids the test edit: a second `RunAsync` overload without the version, used only by tests — rejected as dead production surface. Lead to ratify.
- `scripts/trace.sh` and `scripts/apply-fix.sh` header comments: one line each, named by the skeptic (*Whole run*) but not in the earlier production-path list.

### Architecture findings
_None._

### Follow-up candidates
- (lead's call, small) The warning sentence and the "Expected a C# language version such as …" hint are now written three times (`Program`, trace, apply-fix). A shared formatter-internal helper (for example on `LanguageVersionResolver`) could own the sentence; not done because it adds a member to the shipped formatter assembly for message text and the change doc fixes per-tool wording.

## Whole run

- `git diff --shortstat origin/main...HEAD -- src` → `4 files changed, 201 insertions(+), 43 deletions(-)`; no new type. Production paths: the four the skeptic named (`PipelineTraceCommand.cs`, `CodeFixRunCommand.cs`, `FixtureRunner.cs`, `AssemblyInfo.cs`) plus the two wrapper comments.
- `scripts/format.sh` over the five changed C# files: `Formatted 0 of 5 file(s).`
- `scripts/build.sh --configuration Debug`: 0 errors, 0 warnings.
- `scripts/test.sh --configuration Debug --project cli` → `Passed! - Failed: 0, Passed: 467, Skipped: 1`
- `scripts/test.sh --configuration Debug --project tooling` → `Passed! - Failed: 0, Passed: 227, Skipped: 0`
- `scripts/test.sh --configuration Debug --project architecture` → `Passed! - Failed: 0, Passed: 198, Skipped: 0`
