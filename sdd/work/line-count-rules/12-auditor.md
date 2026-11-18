Role: auditor
Commit: dfe3ac09e40b221bf100bce4c32d3af766e01126
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named the package targets file as untested, which this audit checked by hand (*Whole run*).

## thoenissen/Reihitsu#353

### Scope

Full audit of RH2008 (`RH2008MethodsShouldNotExceedTheLineLimitAnalyzer`) on the shared base `LineCountAnalyzerBase`, against `sdd/specs/analyzer/RH2008.md`, `sdd/specs/shared/line-count.md`, and the change doc `sdd/changes/2026-11/line-count-rules.md`.

### Findings

_N/A — no confirmed finding for this issue._

Requirements traced in the code:

- RH2008-001: production/test setting through `design => design.MaximumMethodLines` / `MaximumTestMethodLines`, defaults 60/100 as constants; `ResolveLimit` falls back per setting (15A, 16A).
- RH2008-002: the registered kinds are exactly the spec's list (method, constructor, finalizer, operator, conversion operator, local function, get/set/init/add/remove accessor, property, indexer); `TryGetBodySpan` counts between the braces of a block body or the `ArrowExpressionClauseSyntax` span (`=>` plus expression, without the `;`, which belongs to the declaration). Signature, constructor initializer, attributes lie outside the span.
- RH2008-003: lambdas and local functions inside a body are tokens of the span; `LocalFunctionStatement` is registered, so it is judged on its own as well.
- RH2008-004: a property or indexer with an accessor list yields `(null, null)` and is not judged; body-less accessors, abstract/extern/interface members, and the defining part of a partial member have no body.
- RH2008-005: identifier, `this`, operator token, `operator` keyword, accessor keyword, as the switch in `TryGetCountedSpan` maps them.

Constructed inputs (scratch probe calling the built `TryGetCountedSpan` and `CountLinesOfCode` through reflection, outside the repository): an expression-bodied `get` accessor over two lines → 2; an expression-bodied local function → 2 and counted toward its method; `#line 500` and `#nullable enable` lines inside a body → not counted, and `#line` does not shift the count (the code uses `SourceText.Lines`, not mapped spans); a `/* */` running from one statement line into the next → each line with a token counted once; `void N() { L(); int L() { return 1; } }` → method 1, local function 1; empty bodies `{ }` on one line and on two lines → 0; a member of an extension block → judged.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which member is judged (RH2008-002/004) | the declaration's `Body` and `ExpressionBody` only | the registered `SyntaxKind` list in the constructor | every registered kind has a switch arm; a kind without a body returns `false` | holds |
| Counted span of a member (RH2008-002) | block: `openBrace.Span.End` to `closeBrace.SpanStart`; expression body: `ArrowExpressionClauseSyntax.Span` | spec: tokens between the braces; `=>` and expression, `;` excluded | same boundaries; the `;` is a token of the declaration, not of the arrow clause | holds |
| Location (RH2008-005) | `nameToken` chosen per node type | spec's location list | one arm per kind, matches | holds |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests` (all) | exact set of RH2008 diagnostics, locations by markup key, message with count and limit | an extra, missing, or moved diagnostic, or a wrong count/limit in the message | `VerifyLines` → `AnalyzerTestsBase.Verify` (Roslyn verifier, exact match), `LineLimit` sets `WithMessage` | adequate |
| `DefaultProductionLimitIsSixty`, `DefaultTestLimitIsOneHundred`, `ConfiguredProductionSettingLeavesTestDefault`, `InvalidSettingKeepsDefaultProductionLimit`, `InvalidTestSettingKeepsDefaultTestLimit` | FC-003 boundary: limit not reported, limit + 1 reported, in both kinds of code | a default of 59/61 or 99/101, or an invalid setting that silences the rule | `CreateMethodsAround` writes both sides of the boundary in one source | adequate |
| `MethodInTestProjectIsJudgedAgainstTestLimit` / `MethodOutsideTestProjectIsJudgedAgainstProductionLimit` | 12D in both directions, both properties, letter case, `false`, empty, absent, other values and keys | either property ignored, a case-sensitive compare, or any non-`true` value read as test code | global config through `AnalyzerConfigFiles` (`is_global = true`), as the build writes it | adequate |
| `CountIsTheSameForEveryLineEnding` | CRLF, LF, CR alone give the same count (FC-033) | a count that follows the line ending | `WithEndOfLine` rewrites the source text; the verifier does not normalize it | adequate |
| `DocumentationCommentInsideBodyIsNotCountedInEveryDocumentationMode` | FC-040: `///` is trivia in every documentation mode | a count that changes with the mode | parse options per data row | adequate |

### Failure classes dismissed

See *Whole run*.

### Architecture findings

_None._

### Hints

_None_ beyond *Whole run*.

### Follow-up candidates

_None_ beyond *Whole run*.

## thoenissen/Reihitsu#354

### Scope

Full audit of RH2111 (`RH2111TypesShouldNotExceedTheLineLimitAnalyzer`) on `LineCountAnalyzerBase`, against `sdd/specs/analyzer/RH2111.md` and `sdd/specs/shared/line-count.md`.

### Findings

_N/A — no confirmed finding for this issue._

Requirements traced in the code:

- RH2111-001: `MaximumTypeLines` / `MaximumTestTypeLines`, defaults 500/1000.
- RH2111-002: registered kinds class, struct, interface, record, record struct, union; enum and delegate are not registered, nor `ExtensionBlockDeclaration` (its lines count toward the enclosing type, which the span covers). Span between the braces; header, base list, primary-constructor parameters, and braces lie outside it.
- RH2111-003: a nested type's tokens lie in the outer span, and the nested declaration is a registered node of its own.
- RH2111-004: the node action runs per `TypeDeclarationSyntax`, i.e. per part; nothing sums parts.
- RH2111-005: `OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken)` is false for `class C;` and `record R(int X);` (default token).
- RH2111-006: `Identifier.GetLocation()`.

Constructed inputs (scratch probe): a class whose methods hold raw, interpolated raw, and interpolated verbatim strings with blank lines inside them → 35, every body line counted; a class holding an extension block → the block's lines count toward the class.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether a type has a brace body (RH2111-005) | `OpenBraceToken` kind | spec: declarations ending in `;` are not reported | a `;`-terminated declaration has a default `OpenBraceToken`; a body-ful one has the brace | holds |
| Counted span of a type (RH2111-002) | `openBrace.Span.End` to `closeBrace.SpanStart` | spec: tokens between the braces | same | holds |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests` (all) | exact set of RH2111 diagnostics with location and message | an extra, missing, or moved diagnostic, or a wrong count | `VerifyLines` / `LineLimit`, same exact verifier as RH2008 | adequate |
| `DefaultProductionLimitIsFiveHundred`, `DefaultTestLimitIsOneThousand` | FC-003 boundary at both defaults | a default off by one | `CreateTypesAround` | adequate |
| `EachPartIsJudgedOnItsOwn`, `EveryPartOverTheLimitIsReportedOnThatPart`, `PartInGeneratedFileIsNotReported` | RH2111-004 per part, no sum, generated part skipped | a summed count or a diagnostic on the wrong part | multi-source verifier | adequate |
| `ExtensionBlockCountsTowardItsTypeAndIsNotJudged`, `UnionIsJudgedAndEnumIsNot` | registered kinds exactly as specified | an extension block or enum reported, a union missed | — | adequate |

### Failure classes dismissed

See *Whole run*.

### Architecture findings

_None._

### Hints

_None_ beyond *Whole run*.

### Follow-up candidates

_None_ beyond *Whole run*.

## Whole run

### Scope

Head `dfe3ac09e40b221bf100bce4c32d3af766e01126` (checked out, clean tree). Base `origin/main` = `899c0c787cee350c54044f81c3c5b72d24c140b9`, equal to `git ls-remote origin refs/heads/main`, and an ancestor of the head (merged in 07b3e8b; the merge brought formatter changes and tests of code fixes that use the formatter, and no analyzer production, configuration, or package file). Full audit. The diff (42 files) matches what the handovers 01–11 describe. `scripts/lane-check.sh behavior-change`: confirmed.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no confirmed finding._

Shared requirements traced:

- SHARED-LINES-001: `CountLinesOfCode` walks `DescendantTokens(span)` without descending into trivia, skips zero-width tokens and tokens outside the span, and counts each `SourceText` line from a token's first to its last character once (`lastCountedLine`). `SourceText.Lines` splits at CR, LF, CRLF, U+0085, U+2028, U+2029 — the compiler's own newline set; a probe with a U+2028 between two statements counted two lines. Structured trivia (documentation comments, directives) and disabled text are trivia and never reach the walk. Blank lines inside raw, interpolated raw, and interpolated verbatim literals are counted (probe: blank first content line, blank line between holes, blank line before the closing quotes); a blank line inside an interpolation hole is whitespace trivia of the hole and is not counted, which is what "a token whose text runs over several lines" states.
- SHARED-LINES-002: `ResolveLimit` uses the configured value when the loader produced one and the rule's own default otherwise. The loader returns no `Configuration` for an absent, empty, unreadable, or unparseable file, leaves `Design` null for a missing or non-object section, and `ParseLineLimit` returns `null` for exactly the values it reports — so a setting without a valid value keeps its default, and an error elsewhere (other section, other setting) leaves a valid setting in place. Compared with `count > limit`.
- SHARED-LINES-003: `IsTestCode` reads `build_property.IsTestProject` and `build_property.IsTestingPlatformApplication` from the compilation's global options, `true` compared with `OrdinalIgnoreCase`, once per compilation.
- RH0001-004/007: `design` accepted as a section; the four settings are validated by `ParseLineLimit` (`JsonTokenType.Number`, ASCII digits only — no sign, fraction, or exponent — `TryGetInt32`, `>= 1`); unknown settings reported on the property name; messages as specified. RH8402: a valid `design` section adds no error, so RH8402 is no longer silenced (visible change 7, answer 1A); an invalid one still silences it (RH8402-001, unchanged).

**Package targets, checked by hand.** `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` adds an unconditional item group with `CompilerVisibleProperty Include="IsTestProject"` and `Include="IsTestingPlatformApplication"`. The file is packed (`Reihitsu.Analyzer.Package.csproj`: `None Include="buildTransitive\Reihitsu.Analyzer.targets" Pack="true" PackagePath="buildTransitive"`) under the name NuGet imports for package id `Reihitsu.Analyzer` (`<PackageId>Reihitsu.Analyzer</PackageId>`), the same file that already carries the Razor `AdditionalFiles` item. Static items in an imported `.targets` are evaluated with every other item, and the SDK's editorconfig generation expands each property's value when its target runs, so the order of import against the test SDKs' property assignments does not matter. The names match the option names `LineCountAnalyzerBase` reads (`build_property.` + property name). No `build/` folder exists, which is the existing arrangement for the Razor item. Holds SHARED-LINES-003's second sentence.

**Visible output is decided.** Every expected value the diff adds is demanded by a requirement the change doc introduces and the maintainer answered (1A–16A with 2B, 9B, 12D): RH2008/RH2111 diagnostics, locations, messages, and defaults (4A, 9B, 14A); test code (12D); RH0001-007 messages; the RH0001 value location follows the loader's existing convention (lead decision 6, unchanged RH0001-002); RH8402 with a valid `design` section (1A, visible change 7) and with an invalid one (RH8402-001, unchanged). The rule pages `RH2008.md` and `RH2111.md` and the package README rows are visible change 8. The rulesets (`None`) and the self-hosting exclusions carry answer 11A and change nothing a package user sees.

**Comments.** Every new or changed summary describes the code it documents (`LineCountAnalyzerBase`, `ConfigurationCategoryDesign`, `ParseDesign`/`ParseLineLimit`/`IsWrittenAsDigits`, the two self-hosting fields and the changed test summary). No `TODO`, no commented-out code, no tracker number in source.

**Scope.** Every changed path belongs to the two rules, their configuration, their documentation, their tests, and answer 11A.

**Special cases.** No guard serves a rare input class or an unsupported shape; the run has no code fix and no formatter change.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A token makes its lines lines of code (SHARED-LINES-001) | tokens of `DescendantTokens(span)` with a non-empty span inside the counted span; trivia not descended | the counted spans of the two rules | a zero-width token (missing, empty literal text) holds no character; every token of a counted span lies inside it | holds |
| Test code (SHARED-LINES-003) | the two global options, value `true` case-insensitively | the `CompilerVisibleProperty` items of the package targets | same two property names | holds |
| A setting has a valid value (SHARED-LINES-002 vs RH0001-007) | `ParseLineLimit`: number token, digits only, fits `int`, `>= 1` | the RH0001 error for the same setting | one predicate produces both the error and the `null`, so a reported setting always keeps its default and an unreported one always applies | holds |
| RH8402 silenced by configuration problems | `ConfigurationLoadResult.Errors` non-empty | RH0001's diagnostics | unchanged; a valid `design` section adds no error | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Lines of code of a span | `LineCountAnalyzerBase.CountLinesOfCode` only (`rg "GetLineFromPosition\|GetLineSpan\(\)\.(Start\|End)Line"` over `Reihitsu.Analyzer` and `Reihitsu.Core` finds no other line-of-code counter) | one owner |
| Test code | `LineCountAnalyzerBase.IsTestCode` (options), `buildTransitive/Reihitsu.Analyzer.targets` (visibility); `rg "IsTestProject\|IsTestingPlatformApplication"` finds no other reader | one owner |
| Validity of a line limit | `ConfigurationManager.ParseLineLimit` only | one owner |
| Default limits | `DefaultProductionLimit`/`DefaultTestLimit` constants in each rule; specs and rule pages restate them as documentation | one owner per rule |
| Rules left out of the self-hosting scan | `AnalyzerSelfHostingTests._analyzersLeftOut` (owner); `AnalyzerViolationReportTests._analyzersLeftOutOfTheScan` is the expected value of the test that pins it, so drift fails a test | one owner plus its test oracle |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH0001ConfigurationFileMustBeValidAnalyzerTests` (new rows) | RH0001-004/007: accepted settings, bounds 1 and 2147483647, every invalid value form with message and position, unknown settings, non-object section, independence of test and production settings | a value accepted or rejected wrongly, a wrong message, or a wrong position | `VerifyConfiguration` / `InvalidConfiguration` with line and column | adequate |
| `RH8402...Tests.ConfigurationWithValidDesignSection` / `ConfigurationWithInvalidDesignSetting` | 1A and the unchanged RH8402-001 | RH8402 silent with a valid section, or reporting with an invalid one | exact verifier | adequate |
| `AnalyzerViolationReportTests.AnalyzerRowsMatchTheAnalyzersTheCompilerLoadsFromTheAnalyzerAssembly` / `LineCountAnalyzersHaveNoRow` | 11A: exactly RH2008 and RH2111 lack a row | another analyzer dropped, or either rule scanned | `AreSequenceEqual` on the sorted list; `DoesNotContain` | adequate |

### Failure classes dismissed

- FC-001, FC-002, FC-003: traced above (*Guard scope*); every boundary has a test on both sides (limit and limit + 1 at all four defaults and at configured limits; `0`/`1` and `2147483647`/`2147483648` in RH0001).
- FC-004 to FC-012, FC-018, FC-026, FC-028 to FC-030, FC-036, FC-038 to FC-043: no code fix, no rewrite, no formatter phase, no precheck shape in the change.
- FC-005, FC-019, FC-020, FC-021: comments and directives are trivia by construction, not by a list; tests cover `//`, multi-line `/* */`, `///`, `#region`, `#pragma`, `#if` with an undefined symbol and `#if false`, an active `#else`, a defined symbol.
- FC-006: no rewrite.
- FC-013, FC-014: the rules judge size, not layout or a form a formatter phase rewrites; the formatter may raise a count, which SHARED-LINES-001 *Why* states (lead decision 1).
- FC-015, FC-016: *Policy ownership*.
- FC-017: *Assertion adequacy*.
- FC-022: verbatim, raw, interpolated raw strings with multi-line holes, comment- and directive-looking text in literals tested; blank lines in interpolated literals probed (above).
- FC-023: `VerbatimIdentifierIsReportedOnTheWholeToken` in both rules.
- FC-024: partial method, property, event, constructor, partial types in one and two files, explicit interface implementation, `file` type, static class tested.
- FC-025: every expression-bodied kind and expression-bodied accessors tested.
- FC-027: local function in local function, functions in lambdas, local function in accessor and in top-level statements, lambda in field initializer, nested types two levels deep tested.
- FC-031: *Comments* above.
- FC-032: the analyzer production and package files are unchanged since the cleaner's build (935d1da); the merge of `main` touched formatter files and code-fix tests, none of which reach the line-count rules or the configuration loader, and `00-lead.md` records a green Release build after it.
- FC-033: CRLF, LF, CR alone tested on raw source text.
- FC-034, FC-035: no reproduction; expected values derive from the spec examples, not from the implementation.
- FC-037: no text-only claim.
- FC-040: documentation-mode test (#353).
- FC-044: no guard models a compiler result; the count reads the syntax tree's own token/trivia split and the compiler's line split.
- Class matrix *Tab width across formatter phases*: no formatter change.

### Architecture findings

_None._

### Hints

- Three new production files carry no UTF-8 byte-order mark, while `.editorconfig` sets `charset = utf-8-bom` for `*.cs`: `src/Reihitsu.Analyzer/Base/LineCountAnalyzerBase.cs`, `src/Reihitsu.Analyzer/Rules/Design/RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.cs`, `src/Reihitsu.Analyzer/Rules/Design/RH2111TypesShouldNotExceedTheLineLimitAnalyzer.cs` (the new test files and `ConfigurationCategoryDesign.cs` carry one). 21 existing files of `src/Reihitsu.Analyzer`, `DiagnosticAnalyzerBase.cs` among them, lack it too, and no build, formatter, or test check enforces it; it has no compiled effect. Not reported as a finding for that reason; a cheap cleanup if the lead wants the new files to match the setting.
- `LineCountAnalyzerBase.CountLinesOfCode` and `IsTestCode` are `internal static` but have no caller outside the class (`rg` over `src`); `private static` would state their scope.
- No test pins a blank line inside the content of an interpolated raw or interpolated verbatim string (the raw-string test has no holes, the interpolated one no blank line). The probe confirms the code counts it; a regression there would go unnoticed.
- `buildTransitive/` is the package's only build-asset folder. A consumer whose `PackageReference` lists `IncludeAssets` without `buildtransitive` may not import it, so its test projects would count as production code. Uncertain, and the same pre-existing mechanism already carries the Razor `AdditionalFiles` item.

### Follow-up candidates

- `Debug.ruleset` line 101 reads `<Rule Id="RH08402" Action="None" />`, an ID no analyzer has, while `Release.ruleset` turns off `RH8402`; RH8402 therefore keeps its default severity in Debug builds of this repository. Pre-existing (introduced with 846aca4), unrelated to this run; no structural cause beyond the typo. Today it has no visible effect, because RH8402 reports nothing without a `copyright` section.
- The candidates already in `00-lead.md` stand: this repository's own limits for RH2008/RH2111 with `CompilerVisibleProperty` made visible through `Directory.Build.props` and the self-hosting scans when the rules are turned on here; duplicate keys in `design` (a later invalid duplicate of a valid setting resets it to the default without a separate RH0001 report).
