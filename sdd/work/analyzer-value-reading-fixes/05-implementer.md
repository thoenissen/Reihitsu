Role: implementer
Commit: 87a472f (head the changes were made on; production changes and this handover are in the commit that adds this file)
Verdict: #1071: tests pass; #1268: tests pass; #1289: test disputed (one fixture); #974: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. No test, assertion, input, or expected output was changed. The test implementer's warning about Unicode-escape fixtures needed no action: no fixture was touched.

## thoenissen/Reihitsu#1071

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs` | `GetExpectedFileNameStem` reads `Identifier.ValueText` instead of `Text` for the type name and every type-parameter name (no `@`, escapes replaced); analyzer and code fix share it, so RH4001-006's taken-name check uses the same name | RH4001-002, -003, -005, -006 |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4009NamespaceNotAllowedAnalyzer.cs` | New private `GetNamespaceName`: joins the `ValueText` parts that `NamespaceCasingHelper.GetLocations` yields with `.`; replaces `Name.ToString()`. The location stays the whole written name. Configuration entries are compared as written | RH4009-002 |

### Policy owners
- `rg -n "GetLocations" -g '*.cs' src/Reihitsu.Analyzer` — `NamespaceCasingHelper.GetLocations` is the one owner of "namespace parts by value" (RH4007/RH4008 via casing bases, RH7002 `GetTypeKey`); reused, no copy (lead decision 6).
- `rg -n "GetExpectedFileNameStem|GetRenamedFileName" src` — the RH4001 helper is the only owner; analyzer and fix call it.

## thoenissen/Reihitsu#1268

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.cs` | The syntactic pre-filter of the explicit-creation path now lets every single `IdentifierNameSyntax` type through to the semantic check (an alias can have any name); qualified, alias-qualified, and generic names are still rejected by their written rightmost identifier. A using alias can only be referenced as a single identifier, so no alias form is lost. Comment updated | RH2003-001, RH2003-004 |

### Policy owners
- `rg -n "GetSimpleTypeName" src` — only this analyzer.

Cost note (internal): creations with a simple identifier type (`new Foo()`) are now bound semantically; qualified and generic creations are still filtered cheaply.

## thoenissen/Reihitsu#1289

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` | The file-wide regex is replaced by a forward scan (`FindCodeDirectives`): Razor comments `@* *@`, HTML comments `<!-- -->` and `@@` are skipped as text; an `@` is a directive candidate only when it starts its line after spaces or tabs (`StartsLine`), and the anchored pattern `\G@(code|functions)\s*\{` decides it. A `@code` directive is reported; for both `@code` and `@functions` the C# block is skipped up to its matching brace (`SkipCSharpBlock`), counting `{`/`}` tokens from Roslyn's lexer (`SyntaxFactory.ParseTokens`), so braces and `@code` lines in C# comments and in every literal kind (verbatim, raw, interpolated, char) are not seen; a Razor comment inside the block is skipped whole. Helpers `StartsWith`, `SkipPast` | RH2109-001, RH2109-003, SHARED-DIAG-003 |

Scope per decision 7: `@{ }`, control-flow, and expression hosts are not lexed; a line-starting `@code {` inside a C# comment or literal there is still reported (the known follow-up). A mid-line `@code` is never reported (Q3). No new type.

### Policy owners
- `rg -n "ParseTokens" src` — `Reihitsu.Core/TokenSeparationUtilities.cs` uses the lexer for an unrelated purpose; no existing Razor or comment scanner in the analyzer (`rg -n "@code|<!--" src/Reihitsu.Analyzer src/Reihitsu.Core` finds only RH2109).

## thoenissen/Reihitsu#974

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Data/ConfigurationManager.cs` | `ParseConfiguration` calls the new `ReadToEndOfDocument` right after the root value's first token: it takes a copy of the reader, `Skip()`s the root value and `Read()`s once more, so any content after the root other than JSON white space throws `JsonException`. The existing catch in `GetConfiguration` reports "invalid JSON syntax" at the exception position and leaves `Configuration` null, so the root's own problems (unknown section, non-object root) are not reported and RH4009/RH8402 become inactive. `<exception>` doc added | RH0001-003, RH4009-001, RH8402-001 |

### Policy owners
- `rg -ln "Utf8JsonReader" src` — only `ConfigurationManager`.

## Whole run

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this commit: 5 files changed, 183 insertions(+), 15 deletions(-). No new type, no new public member, no new dependency.

Per file: RH2109 +155/−10 (gross, including XML docs and blank lines); of these, 91 added lines are neither blank nor `///`, and 53 when lines holding only a brace are also left out. RH4009 +16, ConfigurationManager +14, RH4001 helper +4/−3, RH2003 +4/−2.

### Deleted special cases
_N/A._

### Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~…RH4001TypeNameShouldMatchFileNameAnalyzerTests"` — Passed 44/44
- `…RH4009NamespaceNotAllowedAnalyzerTests` — Passed 24/24
- `…RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests` — Passed 19/19
- `…RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests` — Failed 1, Passed 32 (the disputed test below)
- `…RH0001ConfigurationFileMustBeValidAnalyzerTests` — Passed 40/40
- `…RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests` — Passed 45/45
- `scripts/test.sh --configuration Debug --project analyzer` — Failed 1, Passed 8591, Total 8592; the one failure is the disputed test.
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors for the changed files (an RH7103 member-order warning my first draft caused was fixed by ordering the static helpers first). `scripts/format.sh` on the five files: formatted 0 of 5.

### Disputed tests or scope

1. **Test fixture: `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported`.** Its assertion (diagnostics at (1,1) and (7,1) only) matches the spec and my implementation, but the fixture cannot reach it. The Razor source holds `$$"""`, and the testing library reads additional files as markup, where `$$` is a position marker. The marker is removed from the file, so the analyzer sees a plain raw string, not an interpolated one, and the framework adds a third expected diagnostic at that position. Result: `Mismatch between number of diagnostics returned, expected "3" actual "2"`, and the actual two are exactly (1,1) and (7,1). Evidence: with `test.TestState.MarkupHandling = MarkupMode.None` added locally to this one test (not committed, reverted), it passes. The test implementer's "Expected now: fail" was the right result, but for this reason, not for the reason the handover gives. Fix belongs to the test implementer (turn markup off for this file, or avoid `$$`); the interpolated-raw-string case is not covered until then.
2. **Size against decision 7 (~100 production lines, no new type) for RH2109.** No new type. Gross +155 lines, of which 91 lines are code or `//` comments and 53 excluding brace-only lines. Whether this counts as within the limit is the lead's call; I kept working.

### Architecture findings
_None._

### Follow-up candidates
- The RH2109 gap of decision 7 stays as recorded in `00-lead.md` (C# comments and literals in `@{ }`, control-flow, and expression hosts). `SkipCSharpBlock` could serve those hosts later, but finding where they begin and end is the extra cost.
