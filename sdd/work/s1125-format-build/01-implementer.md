Role: implementer
Commit: ba3051a
Verdict: tests pass

No test project covers build configuration (00-lead.md, decision 1); the reproduction and the check of the fix are command runs, recorded below. My prompt carried no intended fix or suspected cause.

## Reproduction (unchanged tree, ba3051a, no `bin/`/`obj/` under `src/`)

```shell
scripts/format.sh src/Reihitsu.Cli/FormatCommandHandler.cs --check
```

Exit 0, `0 of 1 file(s) need formatting.`, and the build `dotnet run` performs printed 690 warning lines: 465 `S1125` and 225 `RH0002` (MSBuild prints each warning twice, so roughly half that many distinct ones), in `Reihitsu.Core`, `Reihitsu.Formatter`, and `Reihitsu.Cli`. Samples:

```text
src/Reihitsu.Cli/FormatCommandHandler.cs(162,83): warning S1125: Remove the unnecessary Boolean literal(s). [.../src/Reihitsu.Cli/Reihitsu.Cli.csproj]
src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs(47,109): warning S1125: Remove the unnecessary Boolean literal(s). [.../src/Reihitsu.Cli/Reihitsu.Cli.csproj]
src/Reihitsu.Cli/Diff/EditScriptBuilder.cs(91,125): warning S1125: Remove the unnecessary Boolean literal(s). [.../src/Reihitsu.Cli/Reihitsu.Cli.csproj]
src/Reihitsu.Core/AccessorOrderingUtilities.cs(1,1): warning RH0002: Documentation-category rules are currently not active for this project configuration. Set <GenerateDocumentationFile>true</GenerateDocumentationFile> in the project file to enable them. [.../src/Reihitsu.Core/Reihitsu.Core.csproj]
```

For comparison, `dotnet build src/Reihitsu.Core -c Debug --no-incremental` printed `0 Warning(s)`, and `scripts/build.sh` (Release) and `scripts/build.sh --configuration Debug` print `0 Warning(s)`. The issue reproduces. It also has a second symptom the issue does not name: `RH0002`.

## Cause

`Directory.Build.props` puts `CodeAnalysisRuleSet` (and `DocumentationFile`) in groups conditioned on `'$(Configuration)'=='Debug'` / `'Release'`. The SDK imports `Directory.Build.props` through `Microsoft.Common.props` (`Sdk/Sdk.props` line 51) **before** `Microsoft.NET.Sdk.props` (line 53) applies its default `<Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>` (`Microsoft.NET.Sdk.props` line 23). A build that names no configuration therefore sees an empty `Configuration` while the props file is evaluated, matches neither group, and compiles with no ruleset. SonarAnalyzer then applies S1125 at its default severity, and the Reihitsu analyzer reports RH0002 because no documentation file is generated. `dotnet run` in `scripts/format.sh` passes no configuration.

Evidence:

- `dotnet msbuild src/Reihitsu.Cli/Reihitsu.Cli.csproj -getProperty:Configuration -getProperty:CodeAnalysisRuleSet -getProperty:DocumentationFile` gives `Debug`, `""`, `""`. With `-p:Configuration=Debug` it gives `Debug`, `/home/user/Reihitsu/Debug.ruleset`, `bin\Debug\Reihitsu.Cli.xml`.
- The csc command line of `dotnet build src/Reihitsu.Core --no-incremental -v:detailed` (no `-c`) carries no `/ruleset:` argument and the build reports 181 warnings (S1125, RH0002); with `-c Debug` it carries `/ruleset:/home/user/Reihitsu/Debug.ruleset` and reports 0.
- `Debug.ruleset` and `Release.ruleset` both parse as XML and both set `S1125` to `None` (lines 102 and 433); the rulesets are not the cause.

## Other script entry points

| Script | Builds | Configuration passed | Shares the cause |
|---|---|---|---|
| `scripts/format.sh` | `src/Reihitsu.Cli` via `dotnet run` | none | yes (reproduced above) |
| `scripts/trace.sh` | `scripts/trace/trace.cs`, which references `src/Reihitsu.Cli` | none; passes `--property NoWarn=RH0002` | yes: on a clean tree, `scripts/trace.sh src/Reihitsu.Cli/ExitCodes.cs --passes 1` printed 465 `S1125` lines; its `NoWarn=RH0002` hides the second symptom |
| `scripts/apply-fix.sh` | `scripts/apply-fix/apply-fix.cs` -> `Reihitsu.Tooling` | `--property Configuration=Debug` | no |
| `scripts/lane-check.sh` | `scripts/lane-check/lane-check.cs` -> `Reihitsu.Tooling` | `--property Configuration=Debug` | no |
| `scripts/verify-text-only.sh` | `scripts/proof/verify-text-only.cs`, no project reference; its own `Directory.Build.props` stops the search | none | no (nothing imports the root props) |
| `scripts/build.sh`, `scripts/test.sh` | solution / test projects | `-c <configuration>`, default Release | no |

## Changes
| File | What changed | Requirement |
|---|---|---|
| `Directory.Build.props` | Defaults `Configuration` to `Debug` when empty, in a first property group before every configuration-conditioned group, with a comment stating why. This is the SDK's own default, applied before the props file's conditions instead of after them; a global `Configuration` (`-c`, `--property Configuration=...`) still wins. The `== false` style and both rulesets are unchanged; no suppression. | Issue #1800 (proposal: `format.sh` build reports no S1125 for the `== false` style) |

The fix sits at the cause rather than in `format.sh`, so it covers `format.sh`, `trace.sh`, and every other build that names no configuration (an IDE-less `dotnet run`/`dotnet build`), with one change and no per-script flag.

## Policy owners
`rg -n "Configuration" Directory.Build.props scripts/*.sh scripts/lib/*.sh` — the configuration conditions live only in `Directory.Build.props`; the scripts that pass a configuration (`build.sh`, `test.sh`, `apply-fix.sh`, `lane-check.sh`) pass it as a global property, which the new default does not override. `scripts/*/Directory.Build.props` stop the search for the file-based apps and are unaffected.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty. No production path and no new type; the only changed file is `Directory.Build.props` (+9 lines).

## Deleted special cases
_N/A._

## Tests run
All after the change, each on a tree with `src/*/bin` and `src/*/obj` deleted first so the build really compiles:

- `scripts/format.sh src/Reihitsu.Cli/FormatCommandHandler.cs --check` — exit 0, 0 warning lines, `0 of 1 file(s) need formatting.`
- `scripts/trace.sh src/Reihitsu.Cli/ExitCodes.cs --passes 1` — exit 0, 0 warning lines, `Stable after pass 1 (0 phase changes observed).`
- `dotnet msbuild src/Reihitsu.Cli/Reihitsu.Cli.csproj -getProperty:...` (no configuration) — `CodeAnalysisRuleSet` `/home/user/Reihitsu/Debug.ruleset`, `DocumentationFile` `bin\Debug\Reihitsu.Cli.xml`.
- `scripts/build.sh` — `0 Warning(s)`, `0 Error(s)`.
- `scripts/build.sh --configuration Debug` — `0 Warning(s)`, `0 Error(s)`.

No test project covers this; none was run.

## Disputed tests or scope
_None._ The change also removes the `RH0002` warnings of the same build, which the issue does not name; they share the cause and need no extra change.

## Architecture findings
_None._

## Follow-up candidates
- `scripts/trace.sh` passes `--property NoWarn=RH0002`, and `scripts/apply-fix.sh` and `scripts/lane-check.sh` pass `--property NoWarn=RH0002` next to `Configuration=Debug`. For `trace.sh` this flag compensated for the empty-configuration cause; after this change it may be redundant there. Whether it is still needed for the file-based apps themselves was not checked; removing it is a cleanup of a few characters per script, not needed for this issue. Adjacent-fix candidate if the lead wants it, otherwise drop.
