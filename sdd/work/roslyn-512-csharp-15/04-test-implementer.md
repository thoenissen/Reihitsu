Role: test implementer
Commit: 6b7fb49
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Expected values come from the accepted change doc `sdd/changes/2026-11/roslyn-512-csharp-15.md` and the CLI-004 and FMT-PIPE-008 text, not from a patched copy of the code (FC-035). The change doc's C# 14 "before" outputs for the `union` and `closed` type names were confirmed on a build whose maximum is still C# 14.

**The test projects do not compile on this head.** The tests name `LanguageVersion.CSharp15`, which Roslyn 5.9 does not define. The package bump to 5.12.0 is part of the change. How the tests were watched:

- **W0**, a scratch worktree of b702f4c plus these tests. Every `Microsoft.CodeAnalysis*` `5.9.0` reference in `src/**/*.csproj` was set to `5.12.0`. The maximum was left at C# 14.
- **W1**, W0 with `MaxSupportedLanguageVersion` set to `CSharp15`.

The worktree was removed afterwards. No production file of this branch was touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatter.Test/Unit/LanguageVersionResolverTests.ResolveMapsSymbolicVersionsAndPreviewToCSharp15` (new) | FMT-PIPE-008 | MSTest asserts | W0 red: assert, C# 14 ≠ C# 15. W1 green |
| `…LanguageVersionResolverTests.ResolveTreatsMissingParseOptionsAsCSharp15` (new) | FMT-PIPE-008 | MSTest asserts | W0 red: assert. W1 green |
| `…LanguageVersionResolverTests.ResolveKeepsConcreteVersionsUpToTheMaximum` (adds `CSharp15 → CSharp15`) | FMT-PIPE-008 | MSTest asserts | W0 red: assert. W1 green |
| `…LanguageVersionResolverTests.ResolveReadsTheLanguageVersionOfParseOptions` (adds C# 15 parse options) | FMT-PIPE-008 | MSTest asserts | W0 red: assert. W1 green |
| `…LanguageVersionResolverTests.ResolveClampsVersionsAboveTheMaximum` (`1500` → `1600`) | FMT-PIPE-008; FC-017 | MSTest asserts | W0 and W1 green. With `1500` it passed on W1 without testing a clamp |
| `…LanguageVersionResolverTests.ExceedsMaximumIsTrueOnlyAboveTheMaximum` (adds `ExceedsMaximum(CSharp15) == false`, `1500` → `1600`, now tagged) | FMT-PIPE-008; FC-017 | MSTest asserts | W0 red: `CSharp15` exceeds C# 14. W1 green |
| `…LanguageVersionResolverTests.MaxSupportedLanguageVersionMatchesRoslynLatest` (unchanged, existing) | FMT-PIPE-008 | MSTest asserts | W0 red: assert. W1 green |
| `Cli.Test/Unit/ProgramTests.ParseArgumentsWithoutLangVersionSelectsCSharp15` | CLI-004 | `Program.ParseArguments` | W0 red. W1 green |
| `…ProgramTests.ParseArgumentsLangVersionLatestSelectsCSharp15` | CLI-004 | `Program.ParseArguments` | W0 red. W1 green |
| `…ProgramTests.ParseArgumentsLangVersion15SelectsCSharp15WithoutWarning` | CLI-004 | `Program.ParseArguments` | W0 red: clamped, with a warning value. W1 green |
| `…ProgramTests.ParseArgumentsLangVersion15Point0SelectsCSharp15WithoutWarning` | CLI-004 | `Program.ParseArguments` | W0 red. W1 green |
| `…ProgramTests.ParseArgumentsLangVersionPreviewIsClampedToCSharp15` | CLI-004 | `Program.ParseArguments` | W0 red. W1 green |
| `…ProgramTests.ParseArgumentsLangVersion14SelectsCSharp14WithoutWarning` | CLI-004 | `Program.ParseArguments` | green on both. This is the other side of the boundary: 14 is below the maximum and gets no warning |
| `Cli.Test/EndToEnd/CliLanguageVersionEndToEndTests.HelpNamesCSharp15AsTheVersionLatestSelects` | — (change-doc visible output; no requirement states the help line) | `Program.Main` + `ConsoleCapture` | W0 red: `latest selects 14.0`. W1 green |
| `…CliLanguageVersionEndToEndTests.UnionIsFormattedWithoutLangVersion`: change-doc union example, exact file and summary, then a second pass with `--check` | CLI-004; FC-010, FC-017 | `Program.Main` | W0 red: skipped with syntax errors. W1 green |
| `…UnionWithCrlfIsFormattedWithoutLangVersion`: the same with CRLF, then a second pass with `--check` | CLI-004; FC-010, FC-017 | `Program.Main` | W0 red. W1 green |
| `…LangVersion15FormatsUnionWithoutWarning` | CLI-004 | `Program.Main` | W0 red: `Warning: … '15' is newer …`. W1 green |
| `…LangVersion15Point0FormatsUnionWithoutWarning` | CLI-004 | `Program.Main` | W0 red. W1 green |
| `…LangVersionPreviewWarnsWithCSharp15AndFormatsUnion`: the exact warning line naming 15.0, and the union formatted | CLI-004 | `Program.Main` | W0 red. W1 green |
| `…OneLineUnionHasNoSyntaxErrorWithoutLangVersion`: CLI-004 example `public union Pet(Cat, Dog);` with `--check --verbose`; no "syntax errors" in the output | CLI-004 | `Program.Main` | W0 red: `Skipped (syntax errors)`. W1 green |
| `…OneLineUnionIsSkippedUnderLangVersion14`: CLI-004 example under `--lang-version 14` | CLI-004 | `Program.Main` | green on both |
| `…UnionIsSkippedUnderLangVersion14`: change-doc union under 14, file unchanged, exit 0 | CLI-004 | `Program.Main` | green on both |
| `…UnionWithOneLineBracesKeepsThemWithoutLangVersion`: the interim layout the maintainer accepted in answer 2A, then a second pass with `--check` | — (characterization of the change doc's accepted interim output; #1762 will rewrite it) | `Program.Main` | W0 red: skipped. W1 green |
| `…TypeNamedUnionIsSkippedWithoutLangVersion`: change-doc `class union` file | CLI-004 | `Program.Main` | W0 red: formatted. W1 green |
| `…TypeNamedUnionIsFormattedUnderLangVersion14`: the change doc's "before" output, exactly | CLI-004 | `Program.Main` | green on both |
| `…TypeNamedClosedIsSkippedWithoutLangVersion`: change-doc `class closed` file | CLI-004 | `Program.Main` | W0 red: formatted. W1 green |
| `…TypeNamedClosedIsFormattedUnderLangVersion14`: the change doc's "before" output, exactly | CLI-004 | `Program.Main` | green on both |

The existing symbolic tests stay unchanged and pass on W1. They assert against `MaxSupportedLanguageVersion` (`ParseArguments…SelectsMaxSupportedVersion`, `ResolveMapsSymbolicVersionsToTheMaximum`, `FormattingContextStoresTheResolvedLanguageVersion`, and the E2E `MainLangVersionPreviewWarnsOnce…`). Because they compare against the constant itself, they cannot tell C# 14 from C# 15. The new C# 15 tests above pin the concrete version.

Full CLI project on W1: 301 passed, 0 failed.

## Not covered, and why

- **A test that asserts `MaxSupportedLanguageVersion == LanguageVersion.CSharp15` directly.** The build rejects it: MSTEST0025 flags an assert between two constants as always failing, and on W0 that is a build error. The value is pinned through `Resolve(Latest/Default/LatestMajor/Preview/null) == CSharp15` instead, because `Resolve` returns the constant for those inputs.
- **`--lang-version 16`.** It is rejected as unknown, but no requirement states that for 16 specifically. `ParseArgumentsLangVersionUnknownNumberReturnsArgumentError` (`99`) already covers the unknown-number path.
- **The other `union`/`closed` positions in the change doc's list** (property, indexer, generic, interface return, top-level statement, constructor of `closed`). All of them reach one call site: the parse in `FormatCommandHandler` with the CLI's version, followed by the syntax-error check (`rg -n "ParseText|HasSyntaxErrors\|GetDiagnostics" src/Reihitsu.Cli/FormatCommandHandler.cs`). Which positions break is Roslyn's parser behavior, not Reihitsu's. The two change-doc files cover a method return type for `union`, and a field type plus a method return type for `closed`.
- **Code-fix and analyzer paths on C# 15 trees.** The change doc states that no entry point used by code fixes changes, because the project parses the tree. Compiling a union in analyzer tests is out of this run (00-lead decision 4). The skeptic ran the whole analyzer suite green on the bump.
- **Formatter phase tests (`AssertRuleResult`) for union layout.** This change specifies no layout for C# 15 constructs; that is #1762–#1767. The CLI E2E tests already cover idempotency and CRLF for the change-doc examples.
- **Failure classes.**
  - FC-001–FC-005, FC-019–FC-029, FC-036, FC-038–FC-043: no guard, gap, join, rewrite, or precheck shape changes.
  - FC-007–FC-009, FC-018, FC-030: no code fix changes.
  - FC-011, FC-012: no loop or phase pair changes.
  - FC-013, FC-014: no rule changes.
  - FC-015, FC-016: the maximum has one owner.
  - FC-031: the doc comments edited in `LanguageVersionResolverTests` now name 1600 and Roslyn 5.12. The production summaries are the implementer's.
  - FC-032: no project result was carried over. Every result above was run on W0/W1.
  - FC-033, FC-034: these are not reproductions.
  - FC-037: no comment-only proof was made.
- **Existing tests that use a type named `union` or `closed` (answer 3A).** None broke: full CLI on W1 was green, and the skeptic's whole-suite run on the bump was green apart from the FC-017 test fixed here. No rename was needed.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.LanguageVersionResolverTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliLanguageVersionEndToEndTests|FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArguments"
```
