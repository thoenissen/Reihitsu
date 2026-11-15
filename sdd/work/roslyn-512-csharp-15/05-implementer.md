Role: implementer
Commit: b5bea19
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answer 3A (rename a test type named `union`/`closed` if one breaks); nothing to ignore.

Before the change the test projects did not compile on this head (`LanguageVersion.CSharp15` is undefined in Roslyn 5.9), as 04-test-implementer states; the red states W0/W1 are recorded there.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Utilities/LanguageVersionResolver.cs` | `MaxSupportedLanguageVersion` = `LanguageVersion.CSharp15` (was `CSharp14`). Its summary and the `Resolve`/`ExceedsMaximum` docs name the constant, not a version, and stay accurate (FC-031) | FMT-PIPE-008, CLI-004 |
| `src/Reihitsu.{Core,Formatter,Analyzer,Analyzer.CodeFixes,Tooling}/*.csproj` | `Microsoft.CodeAnalysis.CSharp[.Workspaces]` 5.9.0 → 5.12.0 | change doc, *One Roslyn version for every project* |
| `src/Reihitsu.{Core.Test,Formatter.Test,Cli.Test,Analyzer.Test,ArchitectureTests,SelfHosting.Test}/*.csproj` | `Microsoft.CodeAnalysis[.CSharp]` 5.9.0 → 5.12.0 (the `*.Testing` 1.1.4 packages unchanged) | same |

The CLI help line, the `preview` warning, and acceptance of `15`/`15.0` all derive from the constant (`Program.cs:150`, `:292`, `LanguageVersionFacts.TryParse` + `ExceedsMaximum`); no CLI source change was needed.

## Policy owners
`rg -n "CSharp14|MaxSupportedLanguageVersion" src --glob '!*Test*/**'` — the only owner of the maximum is `LanguageVersionResolver.MaxSupportedLanguageVersion`; `Program.cs` and `FormattingContext.cs` only reference it.
`rg -n 'Microsoft\.CodeAnalysis[A-Za-z.]*" Version=' src --glob '*.csproj'` — after the change every non-`Testing` Roslyn reference is 5.12.0; there is no central package-version file.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 6 files changed, 6 insertions(+), 6 deletions(-). No new type or member.
`scripts/lane-check.sh behavior-change`: confirmed.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — Build succeeded, 0 warnings.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.LanguageVersionResolverTests"` — Passed 11/11.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliLanguageVersionEndToEndTests|FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArguments"` — Passed 47/47.
- Whole projects (Debug): formatter 5198/5198, core 682/682, cli 301/301, architecture 118/118, tooling 126/126, playground 23/23.
- Analyzer project: a single `--project analyzer` run had its test host killed for lack of memory in this sandbox (all tests reported up to then passed). Run in namespace chunks instead, all passed: Analyzer 47, Base 13, Clarity 868, Core 40, Design 704, Performance 94, Resources 2, SelfHosting 16, Shared 5, Documentation+Naming+Formatter 2330, Ordering 892, Formatting.RH5* 3335, Formatting (rest) 1745 — 10,091 in total, 0 failed. The validator should expect the same memory limit.
- Answer 3A: no existing test broke, so no test type named `union` or `closed` was renamed.

## Disputed tests or scope
- `src/Reihitsu.Cli/README.MD:35` still says `latest` selects C# 14.0 and `preview` is lowered to 14.0. It is user documentation of the CLI-004 change and now wrong; it belongs to the spec author (documentation), so I left it. One-line text change.
- `src/Reihitsu.Playground.Test/PlaygroundFormatterTests.cs:90` — the doc comment says the default language version is C# 14. The test still passes (it checks an extension block parses), but the comment is now stale. Test file, the test implementer's; comment-only.

## Architecture findings
_None._

## Follow-up candidates
_None._
