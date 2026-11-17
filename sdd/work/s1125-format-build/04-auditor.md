Role: auditor
Commit: 0b5b66084505ea4c9f31055ad85e7f130cc02411
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope
Full audit. Head `0b5b660`, base `origin/main` = `1102aa52fe204b2c5b423d14ccd697a433466e13` (matches `git ls-remote origin refs/heads/main`; it is the merge base, so `main` is merged). The diff is `Directory.Build.props` (+9 lines: a first property group defaulting an empty `Configuration` to `Debug`, with a comment) and the run's `sdd/work/s1125-format-build/` handovers, as `01-implementer.md` describes. Issue thoenissen/Reihitsu#1800 (`agent-filed`), including the sighting folded in from #1801.

### Requirements of the issue
- Reproduce first: done on an unchanged tree (`01-implementer.md`, *Reproduction*).
- The `format.sh` build reports no S1125 for the `== false` style, style unchanged: met. No `.cs` file and neither ruleset changed. Confirmed independently that the build with no configuration now resolves the ruleset: `dotnet msbuild src/Reihitsu.Cli/Reihitsu.Cli.csproj -getProperty:...` with no configuration gives `Configuration=Debug`, `CodeAnalysisRuleSet=/home/user/Reihitsu/Debug.ruleset`, `DocumentationFile=bin\Debug\Reihitsu.Cli.xml`, `Optimize=false`, `DebugSymbols=true`; with `-p:Configuration=Release` it gives the Release ruleset, `TreatWarningsAsErrors=true`, `Optimize=true` — a global configuration still wins.
- The cause stated in the issue's comments ("the Debug build configuration") is not the cause; the cause is an empty `Configuration` while `Directory.Build.props` is evaluated (`Sdk.props` imports `Microsoft.Common.props` at line 51, before `Microsoft.NET.Sdk.props` at line 53, which sets the Debug default at its line 23 — I re-read both). The shipped change fixes the actual defect; the PR should say so.

### Interactions checked outside the diff
- `dotnet pack`/`dotnet publish` default to Release through the CLI's `PackRelease`/`PublishRelease` pre-evaluation, which passes `Configuration` as a global property. Checked that the earlier default does not suppress it: `dotnet pack src/Reihitsu.Cli/Reihitsu.Cli.csproj -o <scratch>` with no configuration built into `obj/Release` and `bin/Release`, exit 0, 0 warnings.
- CI and release workflows (`SonarCloud.yml`, `release.yml`, `deploy-pages.yml`) all pass `-c Release`/`--configuration Release`; unaffected.
- `scripts/*/Directory.Build.props` (apply-fix, lane-check, proof, trace) stop the property search and hold no `Configuration` condition; the file-based apps themselves are unaffected, their referenced repository projects now get the ruleset.
- No `*.csproj`, `*.props`, or `*.targets` other than the root props names `$(Configuration)`; `Microsoft.NET.Sdk.BeforeCommon.props`, imported before the root props, does not read it.
- Documentation: `scripts/README.md`, `AGENTS.md`, `CLAUDE.md`, and the role files state nothing about the no-configuration build that is now stale.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Default `Configuration` when none is given | `'$(Configuration)' == ''` in `Directory.Build.props` | `Microsoft.NET.Sdk.props` line 23, `<Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>` | Same condition, same value; the SDK's becomes a no-op after it. A global property (`-c`, `--property Configuration=...`, a solution's configuration, the CLI's PackRelease/PublishRelease) still wins, checked for `-p:Configuration=Release` and `dotnet pack` | ok |
| Which configuration group applies | `'$(Configuration)'=='Debug'` / `=='Release'` groups (unchanged) | the new default | The default now holds before both groups are evaluated | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Default build configuration | `Directory.Build.props:8` (new), SDK `Microsoft.NET.Sdk.props:23` (external, cannot be removed); `--property Configuration=Debug` in `scripts/apply-fix.sh:51`, `scripts/lane-check.sh:107` (global, same value) | ok — every statement gives `Debug`; the props default has to precede the SDK's to take effect, which the comment states. The script flags are a cleanup candidate (below), not a drift |
| Per-configuration ruleset and documentation file | `Directory.Build.props:40-50` only | ok, single owner |

## Assertion adequacy
_N/A — the run adds and changes no test; no test project covers build configuration (`00-lead.md`, decision 1). The evidence is command runs on a tree with `bin/`/`obj/` removed (`01-implementer.md`), plus my property evaluation above._

## Failure classes dismissed
- FC-001, FC-002, FC-003 — the one condition added is the SDK's own empty-check, tested on both sides (no configuration; `-p:Configuration=Release`).
- FC-015, FC-016 — see *Policy ownership*; sibling builds (`trace.sh`, `apply-fix.sh`, `lane-check.sh`, `verify-text-only.sh`, `build.sh`, `test.sh`, CI) enumerated in `01-implementer.md` and re-checked against the props files and workflows above.
- FC-004, FC-005, FC-019 to FC-021 — no edit to C# trivia, comments, or directives.
- FC-006, FC-022 to FC-030, FC-036 — no rewrite, code fix, or formatter change.
- FC-007 to FC-009, FC-018 — no code fix.
- FC-010 to FC-014 — no formatter phase or analyzer changed.
- FC-038 to FC-043 — no unsupported shape declared or precheck changed.
- FC-044 — no guard models a computed result.
- FC-017, FC-034, FC-035 — no tests.
- FC-031 — the only new comment describes the new property group; checked accurate against `Sdk.props` (see *Hints* for one nuance).
- FC-032 — the cleaner's build ran on `fbff660`, after the compiled change `7c8bd87`; no compiled change followed.
- FC-033 — the reproduction and the check ran with `src/*/bin` and `src/*/obj` deleted, so incremental builds did not hide the warnings.
- FC-037 — no comment-only claim about a `.cs` file.

## Architecture findings
_None._

## Hints
- The comment in `Directory.Build.props:2-6` names "dotnet build without -c" as a build that would skip the configuration groups. That holds for a project build; a solution build without `-c` (`dotnet build Reihitsu.sln`) passes the solution's default configuration as a global property and was not affected. Comment-only, not wrong for the example it gives; no change needed.
- The issue comments attribute the warnings to "the Debug build configuration". `scripts/build.sh --configuration Debug` was clean before the change (`01-implementer.md`); the defect was the empty configuration. The PR text should state the actual cause so the issue's framing is not carried forward.

## Follow-up candidates
- Extends the lead's existing candidate: besides `--property NoWarn=RH0002` (`scripts/trace.sh:31`, `scripts/apply-fix.sh:51`, `scripts/lane-check.sh:107`, all added in d6a01ee), the `--property Configuration=Debug` in `scripts/apply-fix.sh:51` and `scripts/lane-check.sh:107` may now duplicate the default the root props applies to the referenced projects. Whether either flag is still needed for the file-based apps was not checked. Cleanup, not a defect.
