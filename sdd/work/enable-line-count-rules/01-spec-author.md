Role: spec author
Commit: f167c27c
Verdict: #1865: change doc written (one output question open); #1866: no spec change — tooling only

## thoenissen/Reihitsu#1865

Change doc: `sdd/changes/2026-11/enable-line-count-rules.md` (`Status: Draft`). It covers #1865 only; #1866 needs none (below).

No spec is changed. The repository's own build configuration and self-hosting scan are not a specified surface, and the production refactors the run makes keep behavior, so no requirement is added, changed, or retired. The change doc exists because it reverses decision 11A of `sdd/changes/2026-11/line-count-rules.md` and because the claimed lane needs one. If the maintainer changes the default test-type limit (output question below), RH2111-001, its example, the test-limit sentence at the top of `sdd/specs/analyzer/RH2111.md`, `documentation/rules/RH2111.md` (text and configuration example), the constant in `RH2111TypesShouldNotExceedTheLineLimitAnalyzer`, and the RH2111 tests that assert 1000 change with it; I write that spec diff after the answer.

The prompt carried no intended fix or suspected cause.

### Changed requirements

_None._

### Examples for the tests

No requirement examples. What the run has to establish (from the maintainer's binding scope in `00-lead.md` and the change doc's *What*):

- RH2008 and RH2111 are no longer `Action="None"` in `Debug.ruleset` (lines 98–99) and `Release.ruleset` (lines 113, 124).
- The repository's own build makes `IsTestProject` and `IsTestingPlatformApplication` compiler-visible to the analyzers it loads from `binaries/` (today only `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` declares them, and `Directory.Build.props` loads the DLLs as `Analyzer` items, so it is not imported).
- The self-hosting analyzer scan judges both rules, test projects as test code and the other projects as production code. Today `AnalyzerSelfHostingTests._analyzersLeftOut` and `AnalyzerViolationReportTests._analyzersLeftOutOfTheScan` leave both rules out, `AnalyzerViolationReportTests` asserts they have no row, and `AnalyzerViolationReport.Collect` passes no analyzer options, so every compilation would be production code there. Every test project of the repository references `Microsoft.NET.Test.Sdk` (which sets `IsTestProject` through its own props, not in the `.csproj` XML).
- No `reihitsu.json` and no build setting overrides a default limit. The repository has no `reihitsu.json` today.
- The issue's check — one over-limit test method and one over-limit production method judged by their own limits — is the natural test of the recognition.

Facts the next roles need:

- **The committed binaries do not contain the line-count rules.** `binaries/Reihitsu.Analyzer.dll` (version 1.4.0, commit 8d5fcaca) holds no `RH2008…`/`RH2111…`/`LineCountAnalyzerBase` type name; the rules arrived in e91afdf7, after the bump. Until `/bump-version` rebuilds `binaries/`, the ruleset change has no effect on the build; the self-hosting scan, which runs the analyzers built from source, is the only place the run's state is checked. The issue's "the build reports no new diagnostics" is therefore met trivially by the build itself.
- In `Release.ruleset`, every other enabled `Reihitsu.Analyzer` rule is listed with `Action="Error"` (331 entries; the only `None` entries are RH2008, RH2111, RH8402). `Debug.ruleset` lists Reihitsu rules only to switch them off.
- `Reihitsu.Playground` (`Microsoft.NET.Sdk.BlazorWebAssembly`) is not in the self-hosting scan; its three `.cs` files have 19, 56, and 86 physical lines, so it cannot hold a type over 500 lines of code; its largest file can hold no method over 60 either without being nearly all one method — check it once after the build carries the rules.

Measurement (how): a temporary MSTest in a scratch worktree of f167c27c ran both analyzers built from source over `SelfHostingSolution.Instance.Compilations`, with `build_property.IsTestProject = true` for every compilation whose assembly name ends in `.Test` or `Tests` (exactly the repository's test projects), once without `reihitsu.json` (the violations) and once with all four limits set to 1 (the distribution). Nothing of it is committed. Results with the default limits — 137 diagnostics:

| Code | Rule | Default | Over it | p99 | Largest |
|---|---|---|---|---|---|
| production | RH2008 | 60 | 18 of 2514 | 54 | 132 |
| production | RH2111 | 500 | 5 of 878 | 436 | 812 |
| test | RH2008 | 100 | 20 of 16395 | 61 | 270 |
| test | RH2111 | 1000 | 94 of 1024 | 2899 | 6946 |

(Counts and percentiles are over declarations with more than one line of code.) Test types over a limit of 1000: 94; 1500: 50; 2000: 28; 2500: 16; 3000: 11; 4000: 6; 5000: 3. By project: 64 in `Reihitsu.Analyzer.Test`, 29 in `Reihitsu.Formatter.Test`, 1 in `Reihitsu.Cli.Test`. Test methods over 100: 20; over 120: 9; over 150: 5; over 200: 2.

Production violations (rule, location of the declaration, lines of code):

- RH2008 `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:721` — 64
- RH2008 `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs:86` — 64
- RH2008 `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs:795` — 67
- RH2008 `src/Reihitsu.Cli/FormatCommandHandler.cs:272` — 88
- RH2008 `src/Reihitsu.Cli/FormatCommandHandler.cs:414` — 63
- RH2008 `src/Reihitsu.Cli/Program.cs:165` — 132
- RH2008 `src/Reihitsu.Cli/Program.cs:39` — 72
- RH2008 `src/Reihitsu.Core/AttributeTargetUtilities.cs:100` — 62
- RH2008 `src/Reihitsu.Core/AttributeTargetUtilities.cs:473` — 85
- RH2008 `src/Reihitsu.Core/FluentChain.cs:356` — 61
- RH2008 `src/Reihitsu.Core/ModifierOrderingUtilities.cs:91` — 67
- RH2008 `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:504` — 121
- RH2008 `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/DeclarationBraceLineBreakRewriter.cs:151` — 69
- RH2008 `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs:78` — 75
- RH2008 `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:335` — 84
- RH2008 `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs:663` — 64
- RH2008 `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs:63` — 87
- RH2008 `src/Reihitsu.Formatter/ReihitsuFormatter.cs:705` — 98
- RH2111 `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs:25` — 812
- RH2111 `src/Reihitsu.Analyzer/AnalyzerResources.cs:8` — 704
- RH2111 `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs:19` — 693
- RH2111 `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:19` — 544
- RH2111 `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:15` — 505

`AnalyzerResources` is a hand-maintained accessor class of 698 one-line properties, two per rule; it grows with every new rule, so it will exceed any fixed limit as one type. `CodeFixResources` (231 properties) is under 500.

Test methods over 100 (all 20):

- `src/Reihitsu.Analyzer.Test/Documentation/RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.cs:226` — 109
- `src/Reihitsu.Analyzer.Test/Documentation/RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.cs:431` — 104
- `src/Reihitsu.Analyzer.Test/Documentation/RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzerTests.cs:794` — 190
- `src/Reihitsu.Analyzer.Test/Documentation/RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.cs:147` — 109
- `src/Reihitsu.Analyzer.Test/Documentation/RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzerTests.cs:563` — 190
- `src/Reihitsu.Analyzer.Test/Documentation/RH8101ElementParametersMustBeDocumentedAnalyzerTests.cs:297` — 105
- `src/Reihitsu.Analyzer.Test/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests.cs:213` — 119
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs:175` — 270
- `src/Reihitsu.Analyzer.Test/Formatting/RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.cs:107` — 117
- `src/Reihitsu.Analyzer.Test/Formatting/RH7301RegionsShouldMatchAnalyzerTests.cs:29` — 103
- `src/Reihitsu.Analyzer.Test/Naming/RH4109PublicFieldCasingAnalyzerTests.cs:571` — 103
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineRegionTests.cs:221` — 148
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/CommentBeforeBraceBlankLineTests.cs:1327` — 220
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/CommentBeforeBraceBlankLineTests.cs:189` — 121
- `src/Reihitsu.Formatter.Test/Regression/FullPipeline/GetOnlyMemberFullPipelineTests.cs:20` — 117
- `src/Reihitsu.Formatter.Test/Regression/Indentation/CollectionExpressionAlignmentTests.cs:223` — 110
- `src/Reihitsu.Formatter.Test/Regression/Indentation/CollectionExpressionAlignmentTests.cs:569` — 141
- `src/Reihitsu.Formatter.Test/Regression/Indentation/ObjectInitializerAlignmentTests.cs:351` — 123
- `src/Reihitsu.Formatter.Test/Regression/Indentation/ObjectInitializerAlignmentTests.cs:775` — 106
- `src/Reihitsu.Formatter.Test/Regression/Structural/ElseKeywordClosingBraceAuditFanOutTests.cs:26` — 169

Test types over 3000 (the 11 that remain under the recommended answer; the full 94 are reproduced by the measurement above):

- `src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs:24` — 3053
- `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs:23` — 3115
- `src/Reihitsu.Analyzer.Test/Naming/RH4103MethodNameCasingAnalyzerTests.cs:16` — 3308
- `src/Reihitsu.Formatter.Test/Regression/Indentation/HoleAnchorInStatementColumnAlignmentTests.cs:13` — 3522
- `src/Reihitsu.Analyzer.Test/Documentation/RH8201InheritdocShouldBeUsedAnalyzerTests.cs:16` — 3932
- `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs:20` — 4416
- `src/Reihitsu.Analyzer.Test/Formatting/RH7309RegionsShouldFollowCategoryOrderAnalyzerTests.cs:19` — 4453
- `src/Reihitsu.Analyzer.Test/Formatting/RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs:21` — 4492
- `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs:20` — 5269
- `src/Reihitsu.Analyzer.Test/Ordering/RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.cs:21` — 5435
- `src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs:23` — 6946

### Non-goals

- Changing a default other than the test-type limit of RH2111: the production defaults (60, 500) and the test-method default (100) sit where the rules' stated reasoning puts them (18, 5, and 20 declarations over them; near or above the 99th percentile).
- A `reihitsu.json` for the repository, a `design` setting, or an `.editorconfig` severity override for single files or folders (maintainer's binding scope).
- Rebuilding `binaries/` — that is `/bump-version`'s job.
- Any change of RH2008, RH2111, or SHARED-LINES behavior, other than the default the maintainer may change.

### Output questions

**1 · RH2111 · Types should not exceed the line limit — default limit for test code**

Evidence: with the defaults, this repository's test projects hold 94 test classes over 1000 lines of code (9 % of 1024; median of the violations 1545, largest 6946), against 5 of 878 production types over 500. The rules' change doc set the defaults "where few declarations reach"; for test types the 99th percentile is 2899. The other three defaults hold (18 production methods over 60, 5 production types over 500, 20 test methods over 100). The maintainer's scope asks for proposed new defaults when a default proves poorly chosen.

Input — a test class in a test project, no `reihitsu.json`, whose body holds 1500 lines of code:

```csharp
[TestClass]
public class ParenthesesAnalyzerTests
{
    // 1500 lines of code
}
```

Before (`main`):

```csharp
[TestClass]
public class {|RH2111:ParenthesesAnalyzerTests|}
{
    // 1500 lines of code
}
```

"This type has 1500 lines of code, which exceeds the limit of 1000."

After, A — keep 1000: as before. The run splits 94 test classes (64 in `Reihitsu.Analyzer.Test`, 29 in `Reihitsu.Formatter.Test`, 1 in `Reihitsu.Cli.Test`).

After, B (recommended) — default 3000: not reported. A test class with 3001 lines of code is reported, "This type has 3001 lines of code, which exceeds the limit of 3000."; the rule page states 3000 for test code. The run splits 11 test classes.

After, C — default 2000: not reported. A test class with 2001 lines of code is reported, "…exceeds the limit of 2000."; the rule page states 2000 for test code. The run splits 28 test classes.

Why B: 3000 puts the test-type default where the other defaults are, just above the 99th percentile with about 1 % of the types over it, while still marking the few test classes that hold several features' worth of cases.

Note for the lead: the rules' change doc rejected "test defaults of 150 and 2000" as a pair (14B); that alternative's reasoning addressed test methods ("marks almost no outlier"), and the measured test-type distribution was not part of it. Since each part of a partial type is judged on its own (RH2111-004), option A can be met by splitting files into partial parts; the count of classes to split is the same.

### Internal choices

- No spec diff before the answer to question 1; a default change is written into RH2111-001, the spec's summary, and the rule page after the answer. Alternative: write it on the recommendation now — rejected, it is an output question (`sdd/README.md`, *Open questions*).
- One change doc for #1865 only; #1866 changes no intended behavior and gets none. Alternative: one doc for both — rejected, they share no cause, only files.
- The change doc's `Affected requirements` reads "none — repository build change", naming RH2111-001 as conditional on the answer.
- The rule page `documentation/rules/RH2111.md` is left alone until the answer; RH2008's page is not affected by any option.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1866

No spec, change doc, or rule page changes: the rulesets are tooling, and no shipped behavior changes.

Facts, checked at f167c27c:

- `Debug.ruleset:101` holds `<Rule Id="RH08402" Action="None" />`; no rule has that ID. `Release.ruleset:433` already holds `<Rule Id="RH8402" Action="None" />`.
- Every other `Reihitsu.Analyzer` ID in both rulesets names an existing rule (compared against every `DiagnosticId = "RH…"` in `src/Reihitsu.Analyzer`, 349 IDs). The issue's "no ruleset entry names an ID that no rule has" holds once this one entry is corrected.
- RH8402 checks nothing without a `reihitsu.json` that configures a `copyright` header (RH8402-001), and the repository has no `reihitsu.json`. So RH8402 reports nothing in either configuration today, with or without the entry, and the Debug and Release builds report the same RH8402 diagnostics (none) after the correction. (The committed `binaries/` do contain RH8402.)

### Changed requirements

_None._

### Examples for the tests

_None_ — no requirement; the correction has no observable effect on diagnostics (above).

### Output questions

_None._

### Internal choices

- Correct the entry to `RH8402` rather than drop it, so `Debug.ruleset` and `Release.ruleset` state the same switch for the rule. Alternative: drop it, since it switches off a rule that reports nothing without configuration — equivalent in effect, but the two rulesets would then differ.

### Follow-up candidates

_None._

## Whole run

- No requirement-ID reference, `Uses` row, or code owner was added or removed, so the architecture tests were not run for this handover.
- Files written: `sdd/changes/2026-11/enable-line-count-rules.md`, this handover.
