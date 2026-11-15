Role: auditor
Commit: b0522fd16fcd2fd4b2a445a272ac20e4feb167bc
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

Full audit of head b0522fd against base `origin/main` 1defac1. Checked: the checkout is b0522fd, `origin/main` is 1defac1 and is an ancestor of the head, and the diff matches 01–08. It contains the Roslyn 5.9.0 → 5.12.0 bump in 11 project files, `LanguageVersionResolver.MaxSupportedLanguageVersion` = `CSharp15`, FMT-PIPE-008 and CLI-004, the change doc, `src/Reihitsu.Cli/README.MD`, the `LanguageVersionResolverTests`/`ProgramTests` additions, the new `CliLanguageVersionEndToEndTests`, and the Playground test comment. `scripts/lane-check.sh behavior-change`: confirmed.

How the claims were checked:
- Roslyn 5.12 probes were run with `dotnet fsi` against `~/.nuget/packages/microsoft.codeanalysis.*/5.12.0/lib/net10.0`. They cover the `LanguageVersion` enum, the shape of `UnionDeclarationSyntax`, and C# 14 against C# 15 parse errors for 47 snippets.
- The Release CLI of this head (`src/Reihitsu.Cli/bin/Release/net11.0/Reihitsu.Cli.dll`) was run on fixtures in the scratchpad, outside the repository: write, then `--check`.
- I re-ran no repository tests, because no finding depends on one.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `sdd/changes/2026-11/roslyn-512-csharp-15.md`, *Visible output changes*, "CLI, a type named `union` or `closed`"; reached through `src/Reihitsu.Cli/FormatCommandHandler.cs:307` (parse with the CLI's version) | – (incomplete enumeration of the skeptic's member class 1; closest catalog class FC-016) | – | undecided output | Under the default version, the CLI now skips with syntax errors a file whose **top-level statement starts with a local variable or local function named `union`**. That is not a type name, so neither the change doc nor maintainer answer 3A covers it. Counterexample, run on this head's CLI: `using System.Linq;` / `var a = new[] { 1, 2 };` / `var union = a.Union(new[] { 3 }).ToList();` / `union.Add(4);` / `System.Console.WriteLine(union.Count);`. Default: `Skipped (syntax errors)`, file unchanged. `--lang-version 14`: processed as before. Failing members, as a top-level statement on 5.12 with C# 15 (CS1001/CS1514/CS1513/CS8803): `union = 1;`, `union.Add(1);`, `union++;`, `union[0] = 1;`, `union?.ToString();`, `union(1);` (local function), `union<int>(1);`, also when the file declares a class after them. Holding members (no error under C# 15): `int union = 0;` / `var union = 0;` alone, `union` as an argument (`Console.WriteLine(union)`), the same statements inside an `if` block at top level, inside any method body (`union = 1;`, `union.Add`, `union?.X`, `union[0]`, `union++`, `union(1)`), and every one of these positions with `closed`. The file is left unchanged (non-destructive), and code fixes are not affected because the project's own parser decides their trees. | Add the member to the change doc's type-name section (a name `union` at the start of a top-level statement, besides a type named `union`/`closed`), and have the lead ask it as an output question with the next batch. The only options are the 3A outcome (skipped by default, `--lang-version 14` restores the old result) or keeping the maximum at C# 14, which answer 1 already rejected. No code change is suggested. |
| 2 | low | `src/Reihitsu.Playground.Test/PlaygroundFormatterTests.cs:90` | FC-031 | – | PR-introduced | The summary now reads "the default language version, C# 15: an extension block, which only C# 14 parses without an error". C# 15 parses an extension block too, so "only C# 14" contradicts the version named in the same sentence. Text-only: the method body and the assertion message ("C# 14 input must not be reported as a syntax error.") are still accurate. | Reword to "which C# 14 and later parse without an error" (or similar). |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A requested version exceeds the maximum (warning) | `LanguageVersionResolver.ExceedsMaximum`: `MapSpecifiedToEffectiveVersion() > MaxSupportedLanguageVersion`, symbolic `Default`/`Latest`/`LatestMajor` excluded | `LanguageVersionResolver.Resolve(LanguageVersion)`: the same mapping and comparison, the same symbolic exclusion | Same constant, same comparison, same symbolic set. The CLI (`Program.cs:162–165`) calls both on the same `requestedVersion` | holds |
| Version gates in the phases | `StructuralTransformPhase.cs:52,57` (≥ C# 7, ≥ C# 6), `GetOnlyMemberExpressionBodyTransform.cs:176` (< C# 7), `EmptyTypeDeclarationSemicolonTransform.cs:103–153` (C# 9/10/12) | The change doc's claim "no gate above C# 12" | `rg 'LanguageVersion\.' src/Reihitsu.Formatter` shows no gate above C# 12, so C# 14 and C# 15 select the same rules | holds |

No predicate or guard was added or changed beyond the constant.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Newest supported language version | `LanguageVersionResolver.MaxSupportedLanguageVersion` only. `Program.cs:150,292`, `FormattingContext`, and `ReihitsuFormatter.cs:224,392,660` reference it or call `Resolve` (`rg -n 'MaxSupportedLanguageVersion\|CSharp14' src`, production files) | one owner |
| Roslyn package version | One `PackageReference … Version="5.12.0"` in each of 11 project files. No central package management, and no test enforces that they agree (`rg 'Microsoft\.CodeAnalysis' -g '*.csproj' src`); every non-Testing reference reads 5.12.0, and no `5.9` remains anywhere | consistent now; pre-existing multi-owner structure (see *Hints*) |
| Parse with default options (no `--lang-version`) | `DefaultFileSystem.HasCompleteSourceHeader`, `FormatCommandHandler.IsAutoGeneratedFileAsync` (header only, first token / auto-generated comment), `PipelineTraceCommand:145`, `PlaygroundFormatter:25` | Header checks are version-independent. Trace and Playground follow Roslyn's default, now C# 15. That is PLAY-005's wording, and it was the default before the change too |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LanguageVersionResolverTests.ResolveMapsSymbolicVersionsAndPreviewToCSharp15`, `ResolveTreatsMissingParseOptionsAsCSharp15` | FMT-PIPE-008: symbolic versions, `Preview`, and null resolve to C# 15 | any of them resolves to C# 14 | MSTest | adequate. These pin the concrete value that the symbolic tests cannot pin |
| `…ResolveKeepsConcreteVersionsUpToTheMaximum`, `…ResolveReadsTheLanguageVersionOfParseOptions` | C# 15 maps to itself | C# 15 clamped | MSTest | adequate |
| `…ResolveClampsVersionsAboveTheMaximum`, `…ExceedsMaximumIsTrueOnlyAboveTheMaximum` | a value above the maximum is clamped/exceeds; C# 15 does not exceed | clamp missing; C# 15 reported as exceeding | MSTest | adequate. 1600 is above every 5.12 enum value (probe: the enum ends at `CSharp15=1500`, then `LatestMajor`/`Preview`/`Latest`). Both sides of the boundary are tested (FC-003) |
| `ProgramTests.ParseArguments…CSharp15` (5 tests), `…LangVersion14SelectsCSharp14WithoutWarning` | CLI-004 resolution and warning value | wrong version, a spurious or missing warning value, an argument error | `Program.ParseArguments` | adequate; both sides of the maximum are covered |
| `CliLanguageVersionEndToEndTests.UnionIsFormattedWithoutLangVersion`, `UnionWithCrlfIsFormattedWithoutLangVersion`, `UnionWithOneLineBracesKeepsThemWithoutLangVersion` | exact output under C# 15, stable on a second pass, LF and CRLF | different text, skip, a non-converging second pass | `Program.Main` + `ConsoleCapture` | adequate (exact file text, exit code, `--check` second pass, CRLF) |
| `…LangVersion15/15Point0FormatsUnionWithoutWarning`, `…LangVersionPreviewWarnsWithCSharp15AndFormatsUnion` | CLI-004 acceptance and warning text | warning present or absent, wrong text, skip | `Program.Main` | adequate (exact stderr and stdout) |
| `…OneLineUnionHasNoSyntaxErrorWithoutLangVersion` | the CLI-004 example parses without a syntax error under the default | a syntax-error skip | `Program.Main` | adequate for its invariant: both the verbose line and the summary contain "syntax errors", and stderr is checked to be empty. It does not assert the exit code or the summary, but the exact-output union tests next to it cover "is formatted" |
| `…OneLineUnionIsSkippedUnderLangVersion14`, `…UnionIsSkippedUnderLangVersion14`, `…TypeNamed{Union,Closed}IsSkippedWithoutLangVersion`, `…TypeNamed{Union,Closed}IsFormattedUnderLangVersion14` | skipped and unchanged, or formatted exactly as before | formatted, or a different text | `Program.Main` | adequate (exact summary and file content) |
| `…HelpNamesCSharp15AsTheVersionLatestSelects` | the help line names 15.0 | 14.0 | `Program.Main` | adequate (substring of the one line that changes) |

Visible output is decided (check 10). The help line, `preview` warning, `15`/`15.0`, and union formatting are covered by answer 1A; the one-line braces by 2A; the `union`/`closed` type-name skips and the `--lang-version 14` "before" outputs by 3A. The CLI README row follows 1A. No rule page changed. The one uncovered visible case is finding #1, and no test asserts it.

## Failure classes dismissed
- FC-001, FC-002, FC-003: no guard changed; the one boundary (the maximum) is tested on both sides.
- FC-004, FC-005, FC-019, FC-020, FC-021: no rewrite or gap edit changed. The phases now reach union declarations through the default CLI, so I ran unions holding `//` and `/* */` in the case list, `#if`/`#endif` around a case and around a member, `#region` around a union, and doc comments. Every comment and directive was kept, and the result was stable on a second pass.
- FC-006: the language-version gates were checked under *Guard scope*. The rest of the class (rewrites that change meaning) is not touched.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix changed. Code fixes get their trees from the host's parser.
- FC-010: idempotency of the union outputs was covered by the E2E `--check` second passes (LF and CRLF) and by my corpus. The corpus had generic unions with constraints, attributes, base lists, empty, `;`, and multi-line case lists, nested unions, a union with a switch expression, and `partial union`. All four files gave `0 of 4 file(s) need formatting` on the second pass.
- FC-011, FC-012: no loop or phase pair changed.
- FC-013, FC-014: no rule changed. The analyzers see C# 15 trees in a 5.12 host with or without this change.
- FC-015, FC-016: see *Policy ownership*. The maximum has one owner. The incomplete enumeration in #1 is a gap in the change doc, not a sibling hazard in code.
- FC-017: see *Assertion adequacy*.
- FC-022 to FC-029, FC-036: no rewrite changed.
- FC-031: finding #2. The `LanguageVersionResolver` summaries name the constant, not a version, and stay accurate. `OrderingDeclarationUtilities.cs:555` ("`field` … from C# 14 on") is still true.
- FC-032: the implementer's results were taken on b5bea19 with the real bump. After the merge of 1defac1 the lead reports the build green, and the validator runs the default set next.
- FC-033, FC-034: no reproduction (behavior change).
- FC-035: the expected values come from the accepted change doc, which was produced on a scratch worktree before the tests. Probes on this head confirm them.
- FC-037: the cleaner used `scripts/verify-text-only.sh` for the Playground comment.
- FC-038 to FC-042: no shape is declared and the precheck is unchanged. A union holds none of the three declared shapes in the corpus above.
- Class matrix "Tab width across formatter phases": no column computation changed.

## Architecture findings

_None._ The run's decision (the newest version) has one owner, and no component predicts another.

## Hints
- **Roslyn version in 11 places.** The change doc's decision "One Roslyn version for every project" is stated once in prose and carried by 11 separate `PackageReference` versions. Nothing (central package management or an architecture test) keeps them equal, so a later bump that misses a project would compile the formatter against one Roslyn and run it against another. This is pre-existing structure and does not block this run. Follow-up candidate: one version property or `Directory.Packages.props`, or a test that every `Microsoft.CodeAnalysis*` non-Testing reference names the same version.
- **Playground and trace tool.** Both parse with Roslyn's default options (`PlaygroundFormatter.cs:25`, `PipelineTraceCommand.cs:145`), so they now also parse C# 15: a union is formatted, and a type named `union`/`closed` becomes a syntax error. PLAY-005 already demands this ("Roslyn's default parse options"). The change doc's *Visible output changes* names only the CLI and the package. The lead may add one line for the Playground.
- **Interim union layout seen in my corpus.** Everything below is non-destructive and stable; answer 2A and the change doc's "whatever the phases do … they keep doing" cover it. It is input for #1762–#1767, not for this run:
  - `int _f, _g;` inside a union body is not split;
  - consecutive union declarations get no blank line between them;
  - a `where` clause stays on the header line;
  - a case after an `#if`-guarded case in a multi-line case list is aligned to the column after `(` (`internal union B(` / `#if DEBUG` / `int,` / `#endif` / `string);`).
