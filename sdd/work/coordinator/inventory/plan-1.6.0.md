# Plan 1.6.0 — dependency graph of the squad runs

Derived 2026-11-21 on `coordinator` = origin/main (ff687d2) + `sdd/work/coordinator/`. Inputs: roadmap section 1.6.0, the 41 issue bodies, every issue comment (maintainer triage comments are binding), `AGENTS.md`, `squad/lead.md` §1–4, `squad/README.md` (*Several issues in one run*, escalations), `scripts/README.md` (*The lane check*). File sets are grounded in grep on main; "probable" marks a file the run will most likely touch but whose need is decided by the run's own spec work.

v1.5.0 tag exists; `binaries/` were rebuilt by the bump commit 5af53bd (= v1.5.0), so #1880's precondition holds.

## 1. Roadmap corrections

1. **One lane per run** (`lead.md` §1 step 4, escalation 8). Five roadmap runs mix lanes and would stop at the claim; they are split below:
   - hole/lambda: #1566 is a bug fix (observed vs expected), #1567 #1568 #1698 behavior change.
   - indentation small + docs: #1399 #1432 behavior change, #1722 bug fix (refactor, no output change), #1400 docs, #1607 tests.
   - SHARED-DIAG-008: #1240 #1591 docs, #1275 #1836 behavior change.
   - directive guards: #1589 bug fix (audit), #970 #1668 #1837 behavior change (#970 reclassified as an extension by triage).
   - naming scope: #1085 #1090 bug fix, #1086 #1087 #1088 #1089 behavior change; it also names RH4001, RH4009, RH4115, RH4117, a new rule, RH4122, RH4124, RH4118 (more than a handful of rules, escalation 8).
2. **#1434 adds no rule.** It adds a requirement (SHARED-CASING-011 per the issue, but that ID is taken on main — "fix withheld when a method would receive a name the compiler looks up" — so the next free SHARED-CASING ID). The only run that adds a diagnostic ID, resource strings, a `Release.ruleset` row, a spec-index row, a package `README.MD` row, and a rule page is **#1088**. `Debug.ruleset` has no per-rule rows. The spec index `sdd/specs/README.md` is also touched by **#1090** (code owner rename).
3. **#953's run shares `sdd/specs/formatter/structural-transforms.md`** with the analyzer runs: #1852 (in #953's run) changes FMT-STRUCT-006 (the conversion's join of an expression after `=>`, its question 2) and #953 itself plans FMT-STRUCT-007/-023 examples; #1275 needs FMT-STRUCT-017 and #1837 needs FMT-STRUCT-019/-020. So #1275 and #1837 wait for #953. They also share SHARED-TRAIL-002 (`shared/trailing-commas.md`, which #1275 lists as a requirement), so they form one run.
4. **The formatter runs after #953 are serial among themselves**, not parallel: list/pattern, hole/lambda, #1399 and #1400 all edit `sdd/specs/formatter/indentation.md`; list/pattern (#1633 join, #1542 FMT-BREAK-022) and hole/lambda (#1567) both edit `line-breaks.md`.
5. **The blanket "everything formatter-side waits on #953" is too broad.** #1566 (no line break in a regular interpolation hole below C# 11: line-break phase plus language version), #1432 (tab-indented raw strings: `RawStringLineShifts`, `raw-string-alignment.md`, RH5202) and #1722 (`RawStringLineShifts` refactor) share no file and no logical link with #953. It is right for #1567 (option B is #953's rule), #1698 (hole lines; #953's comments list hole tokens at the statement column), #1568 (`LambdaAlignmentContributor`, `indentation.md`), #1633 (#953 comment: `class F⏎: IDisposable` depends on #1633's answer), #1542 (#953 lists "the pattern inside a parenthesized pattern"), #1685 (#953 lists case-label continuations), #1634/#1647 (`indentation.md`, contributors), #1399 (FMT-INDENT-010/-023, `LayoutComputer.SetDirectiveIndentation`), #1400 (`indentation.md`), #1607 (RH3004's fix formats through the formatter; `IndentationRewriter`).
6. **#1589 has an unknowable file set** (an audit of every brace/wrap/move fix: RH3202–RH3208, RH5405–RH5407, RH72xx, RH7310, "other fixes found by searching"). It runs alone, after every run that touches those fixes (#953 via #1852's conversion, #1836, #1275/#1837, #970/#1668), or moves to 1.7.0.
7. **#1088's casing choice decides whether its run is repository-wide.** The repository's own test code uses camelCase local constants (`const string testCode = """…`, `const int lineCount = …`) in about 870 files (`rg -l '^\s+const string [a-z]\w* = ("""|"|@")' src` = 870 files; one production file, `src/Reihitsu.Core/UsingDirectiveOrderingUtilities.cs`). The self-hosting scan (`AnalyzerSelfHostingTests`) runs every analyzer over test projects too. With the issue's starting point PascalCase, the run renames them all and must run alone; with camelCase it is an ordinary run. Ask this output question before the run starts.
8. **#1472** (written review of `IndentationPhase`, "rewrite or keep") is worth running now: it writes one new text file, and its verdict should be known before the serial chain of indentation runs builds further on `LayoutComputer`/`LayoutModel`.

## 2. Runs

Lanes: BC = behavior-change, BF = bug-fix. "Registry" = `*.resx`, `AnalyzerResources.cs`/`CodeFixResources.cs`, `Release.ruleset`/`Debug.ruleset`, `sdd/specs/README.md`, `src/Reihitsu.Analyzer.Package/README.MD`. Every run may touch `sdd/specs/requirement-ids.txt` and its own change doc.

### R1 · #1880 — tooling — start now
- Files: `Directory.Build.props` (comment at line 62 and the `AdditionalFiles` item), probably the build file importing `buildTransitive/Reihitsu.Analyzer.targets`; `src/Reihitsu.SelfHosting.Test/Repository/BuildConfigurationTests.cs`.
- No registry, no rule. Predecessors: none (precondition met).
- Note: from its merge on, the Release build fails on a production type over 500 lines in any run's diff (already enforced by the self-hosting scan, so no new coupling).

### R2 · #953 #1852 #1896 #1818 — BC — start now
- Production: `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` (fallback in `ComputeBlockIndentation`, `CreateContributors` if a contributor is added), probably a new contributor under `Pipeline/Indentation/Contributors/`, `Utilities/ListElementAligner.cs` (#1896: query clauses are aligned only inside list elements today), possibly `BaseTypeListContributor.cs` (#953 comment on `: Base` at column 0), `IndentationRewriter.cs` (directive-led lines, #1818); `Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` / `ExpressionBodiedTransformUtilities.cs` (#1852 question 2: blank line between `=>` and the expression).
- Specs: `sdd/specs/formatter/indentation.md` (non-goal at line 19, new FMT-INDENT requirement, FMT-INDENT-013 for #1896), `structural-transforms.md` (FMT-STRUCT-006 for #1852; FMT-STRUCT-007/-023 examples), `blank-lines.md` (FMT-BLANK-013 clarification); `shared/expression-body-conversion.md` only if its wording of the join changes (it cites FMT-STRUCT-006).
- Tests: `src/Reihitsu.Formatter.Test/Regression/Indentation/*`, `Unit/Indentation/*` (`LayoutComputerTests`, contributor tests), `Regression/Structural`, `Regression/FullPipeline/ExpressionBodied*`, `Idempotency`; `src/Reihitsu.Analyzer.Test` RH3202–RH3208 fix tests if #1852 Q2 = A (the fix base `ExpressionBodyToBlockCodeFixProviderBase` uses the same conversion); a two-entry-point test (RH5305 fix vs CLI) per the 2026-11-17 comment on #953. Which existing tests pin today's fallback is known only when built; a heuristic scan found no production source with an unowned continuation line, so no self-hosting reformat of `src/` is expected.
- No registry, no rule. Predecessors: none.

### R3 · #1566 — BF — start now (split from hole/lambda)
- Production: line-break rewriters under `src/Reihitsu.Formatter/Pipeline/LineBreaks/` (brace placement of an anonymous method/lambda block inside a hole), reading `FormattingContext.LanguageVersion` (`Data/FormattingContext.cs`, FMT-PIPE-008); possibly a shared cheap check in `Pipeline/Core/Utilities`.
- Specs (probable): `sdd/specs/formatter/line-breaks.md` or `pipeline.md` (FMT-PIPE-008).
- Tests: `src/Reihitsu.Formatter.Test/Regression/LineBreaks/*`, `Unit/LineBreaks/*`, a LangVersion-10 compile check.
- Predecessors: none. Collides with R4/R5 on `line-breaks.md`, which wait for R2 anyway. If its spec work lands in `blank-lines.md`, it waits for R2.

### R4 · #1568 #1698 #1567 — BC — after R5 (and after R3 if R3 edits `line-breaks.md`)
- Production: `Contributors/LambdaAlignmentContributor.cs` and the scope-depth block indentation in `LayoutComputer.cs` (#1568), hole-line indentation of regular interpolated strings (`LayoutComputer.cs`/`CommentIndentationContributor.cs`) plus the blank line in front of `// f` (`Pipeline/BlankLines`, `BraceBlankLinePolicy`) (#1698), a lambda-modifier join in `Pipeline/LineBreaks` (#1567).
- Specs: `indentation.md` (FMT-INDENT-028/-031), `blank-lines.md` (#1698), `line-breaks.md` (FMT-BREAK-015/-016 family, #1567).
- Tests: `Regression/Indentation/AnonymousMethodHoleAlignmentTests.cs`, `LambdaHoleAnchorInStatementColumnAlignmentTests.cs`, `HoleAnchor*`, `Unit/Indentation/Contributors/LambdaAlignmentContributorTests.cs`, `Regression/LineBreaks`, `Regression/BlankLines`.
- Predecessors: R2 (logical: #1567 option B is #953's rule; #1698 hole lines; files `indentation.md`, `LayoutComputer.cs`, `blank-lines.md`), R5 (`indentation.md`, `line-breaks.md`).

### R5 · #1633 #1634 #1647 #1685 #1542 — BC — after R2
- Production: `Contributors/BaseTypeListContributor.cs` (#1633), `AngleBracketListAlignmentContributor.cs` (+ qualified-name line, #1634), `ListPatternContributor.cs`/`CommentIndentationContributor.cs` (#1647), `CaseWhenClauseContributor.cs` (#1685), `ParenthesizedPatternContributor.cs`/`RecursivePatternContributor.cs`/`PatternAnchor.cs` (#1542); line-break side in `Pipeline/LineBreaks` (#1633 base-list join, #1542 FMT-BREAK-022 keeps `(` on the `is` line).
- Specs: `indentation.md` (FMT-INDENT-006, -015, -016, -021, -025, -026; non-goal on line-start commas), `line-breaks.md` (FMT-BREAK-022, base-list join), `pipeline.md` FMT-PIPE-014 if #1647 extends it.
- Tests: `Unit/Indentation/Contributors/{BaseTypeList,CaseWhenClause,ListPattern,RecursivePattern,CommentIndentation}ContributorTests.cs`, `Regression/Indentation/{BaseListContinuationAlignmentTests,WrappedBaseListGenericArgumentsTests,RecursivePatternAlignmentTests,ConstraintClauseAlignmentTests,ExpressionAndPatternAlignmentTests,CollectionExpressionClosingBracketCommentTests}.cs`, `Regression/LineBreaks`.
- Predecessors: R2 (logical links listed in §1.5; `indentation.md`, `LayoutComputer.cs`). Comma-first shapes of #1542/#1633 stay as written (roadmap).

### R6 · #1399 — BC — after R4
- Production: `LayoutComputer.cs` (`SetDirectiveIndentation`, region column inside initializers, collection expressions, switch expressions, argument lists, list/parenthesized patterns, switch sections), `ObjectInitializerContributor.cs`/`ListElementAligner.cs` (nested array rows, FMT-INDENT-023).
- Specs: `indentation.md` (FMT-INDENT-010, -023). Tests: `Regression/Indentation/{ComplexElementInitializerAlignmentTests,ObjectInitializerAlignmentTests,ScopeIndentationTests}.cs`, `Regression/Regions`.
- Predecessors: R2 (file), R4/R5 (`indentation.md`).

### R7 · #1400 — docs — after R6
- Files: `indentation.md` (FMT-INDENT-010 item 22, FMT-INDENT-028 item 23), characterization tests only if missing (that would widen to tests). Item 24 is already done (comment 2026-11-16).
- Predecessors: R6 (same requirement FMT-INDENT-010).

### R8 · #1432 — BC — start now (split from indentation small)
- Production: `Pipeline/Indentation/Utilities/RawStringLineShifts.cs`, `RawStringLineUtilities.cs`; RH5202 parity: `src/Reihitsu.Analyzer/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs`, `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs`.
- Specs: `raw-string-alignment.md` (tab non-goal at line 16, FMT-RAW-001/-002, line 24 ties the width to RH5202), `tab-replacement.md` (FMT-TAB-003 cross-reference), `sdd/specs/analyzer/RH5202.md`.
- Tests: `src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/*`, `Regression/FullPipeline/RawString*`, `src/Reihitsu.Analyzer.Test/Formatting/RH5202*`, `Formatter/Formatting/RH5202*`.
- Predecessors: none. Premise: the issue names `RawStringAlignmentPhase`, which no longer exists (raw-string moves are in `IndentationPhase`); the spec now says RH5202 counts a tab as one column, so the change moves RH5202 with it (a visible analyzer change the issue does not mention).

### R9 · #1722 — BF — after R8
- Files: `RawStringLineShifts.cs` (`IsHoleOpeningLine`), possibly `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` (`IsInterpolationBrace`); no spec; tests unchanged.
- Predecessors: R8 (same file).

### R10 · #1607 — tests (BF if item 3 changes production) — after R2 and R12
- Files: `sdd/specs/analyzer/RH3004.md`, possibly `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`, possibly `Pipeline/Indentation/Utilities/IndentationRewriter.cs` (`IsNonRegionDirectiveTrivia`); tests `src/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntax*Tests.cs`.
- Predecessors: R2 (RH3004's fix formats through the formatter; `IndentationRewriter`), R12 (three RH3004 test files are in #1912's list).

### R11 · #1472 — docs (issue says tooling; the output is a written review, text) — start now
- Files: one new review document (text). Predecessors: none.

### R12 · #1912 — tests — start now
- Files (`rg -l 'DiagnosticId, *"' src`, 22 files): `src/Reihitsu.Analyzer.Test/Clarity/RH3002*` (4), `RH3003`, `RH3004UseLambdaSyntax{AnalyzerTests,MultiLineTokenAndDirectiveTests,StatementShiftTests}`, `RH3005`, `RH3006`, `RH3007`, `RH3101`, `RH3102`, `RH3103`, `RH3104`, `RH3105`, `RH3201`; `Formatting/RH5048…`, `Formatting/RH5112…`, `Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs`; `src/Reihitsu.ArchitectureTests/General/ApplyFixWrapperPositionalArgumentTests.cs`; `src/Reihitsu.Tooling.Test/Unit/CodeFixRunCommandTests.cs`. Not repository-wide: rule (d) does not apply.
- Predecessors: none.

### R13 · #1240 #1591 — docs — start now
- Files: `sdd/specs/shared/diagnostics.md` (non-goal at line 14), `documentation/rules/RH7003.md` (the `## Exceptions` section at line 24 is still there).
- Predecessors: none.

### R14 · #1836 — BC (output question; B closes it) — start now
- Production: the shared guard `src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs` (also used by the formatter's `UsingDirectiveOrderingRewriter`, which keeps the block, so the analyzer side probably needs its own call) and the RH7201–RH7207 analyzers under `src/Reihitsu.Analyzer/Rules/Organization/`.
- Specs: `shared/using-directive-ordering.md` (SHARED-USING-009 at line 496), `analyzer/RH7201.md`…`RH7207.md`. Tests: `src/Reihitsu.Analyzer.Test/Organization/RH720*` (analyzer tests).
- Predecessors: none.

### R15 · #1275 #1837 — BC — after R2 and R13
- Production #1275: `src/Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.cs`, `src/Reihitsu.Analyzer/Base/EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase.cs`, `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs` (uses `ReihitsuFormatter`), `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs` (shares the Core utility).
- Production #1837: `Pipeline/StructuralTransforms/Utilities/TrailingCommaRemovalUtilities.cs`, `Rewriter/EnumTrailingCommaRemovalTransform.cs`, `Rewriter/InitializerTrailingCommaRemovalTransform.cs`, `TrailingCommaAnalyzerBase<TNode>`, `src/Reihitsu.Analyzer.CodeFixes/Base/TrailingCommaRemovalCodeFixProviderBase.cs`, `TrailingCommaCodeFixHelper`.
- Specs: `shared/empty-type-declarations.md`, `shared/trailing-commas.md` (SHARED-TRAIL-002), `formatter/structural-transforms.md` (FMT-STRUCT-017, -019, -020), `analyzer/RH5409.md`–`RH5416.md`; rule pages `documentation/rules/RH5412.md`–`RH5416.md` (the issue asks for updated rule pages).
- Tests: `src/Reihitsu.Analyzer.Test/Formatting/RH541[2-6]*AnalyzerTests.cs` (incl. `VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix`), `Formatter/Formatting/RH541[2-6]*FormatterTests.cs`, RH5409–RH5411 tests, `src/Reihitsu.Core.Test/EmptyTypeDeclarationSemicolonAnalysisUtilitiesTests.cs`, `src/Reihitsu.Formatter.Test/Regression/Structural/EmptyTypeDeclarationSemicolonConversionTests.cs`, trailing-comma structural tests.
- Predecessors: R2 (`structural-transforms.md`; the empty-type fix delegates layout to the formatter), R13 (#1275 is "blocked by #1240"; #1260 is closed).

### R16 · #970 #1668 — BC — start now
- Production: `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` (`ContainsPositionSensitiveDirectives`), `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` (`GetMembersOfDisabledText`, whole-unit helper), `src/Reihitsu.Core/OrderingMoveSafety.cs`, possibly `RH7309RegionsShouldFollowCategoryOrderCodeFixProvider.cs` (calls the guard).
- Specs: `shared/ordering-move-safety.md` (SHARED-ORDER-005, -006, -008, the open non-goal), `shared/code-fixes.md` (SHARED-FIX-003 references, #1668).
- Tests: `src/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs` (`MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective` is replaced), ordering-fix tests RH7102, RH7103, RH7107, RH7108, RH7109, RH7309.
- Predecessors: none.

### R17 · #1589 — BF (audit; may end as tests) — alone, last
- Files: unknowable before the audit (every fix that adds braces, wraps, or moves code; their tests; possibly `shared/code-fixes.md`).
- Predecessors: R2 (RH3202–RH3208 share the conversion #1852 changes), R14 (RH72xx), R15, R16. Recommend moving it to 1.7.0 if the milestone is tight.

### R18 · #971 #1072 #1451 #1434 — BC — start now
- Production: `src/Reihitsu.Core/CasingUtilities.cs` (#971, #1451), `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` (#1072), `src/Reihitsu.Analyzer/Base/CasingAnalyzerBase.cs` (#1434, probable; else the six analyzers RH4102, RH4103, RH4112, RH4113, RH4114, RH4116), `RH4005InterfaceNameCasingAnalyzer.cs` / `RH4121TypeParameterNameCasingAnalyzer.cs` and their fixes (prefix handling, #1451).
- Specs: `shared/casing.md` (SHARED-CASING-006, -008, new requirement for #1434 with the next free ID), `analyzer/RH4005.md`, `RH4106.md` (non-goal), `RH4121.md`, `RH4102.md`, `RH4103.md`, `RH4112.md`–`RH4114.md`, `RH4116.md`; `RH2001.md` if RH2001's backing-field name changes (#971 comment).
- Tests: `src/Reihitsu.Core.Test/CasingUtilitiesTests.cs`, Naming tests of the rules above, RH2001 fix tests (#971 instance).
- No new rule, ID, or resource string. Predecessors: none. Several rules, one mechanism (`CasingUtilities`/casing bases).

### R19 · #1771 — BF — after R18, escalation 9 before building
- Files: `CasingCodeFixProviderBase{T}.cs` (`RenameWithoutConflictAsync`, `RewritesOtherCodeAsync`), `Base/CasingRenameSafetyUtilities.cs`, `Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs`, a new shared component, `shared/casing.md` (SHARED-CASING-009), `analyzer/RH2001.md` (RH2001-011), tests.
- Predecessors: R18 (same base, RH2001 tests).

### R20 · #1087 #1088 — BC, adds a rule (new ID, new resource strings) — after the casing question (§1.7)
- Production: `RH4115LocalVariableCasingAnalyzer.cs` + fix, `RH4117DeconstructionVariableCasingAnalyzer.cs` + fix, new `RH41xx…Analyzer.cs` (+ fix if comprehensive).
- Specs/pages: `analyzer/RH4115.md`, `RH4117.md`, new `analyzer/RH41xx.md`, `documentation/rules/RH41xx.md`, `documentation/rules/RH4115.md`/`RH4117.md` if their "what is reported" changes.
- Registry: `src/Reihitsu.Analyzer/AnalyzerResources.resx` + `AnalyzerResources.cs`, `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` + `CodeFixResources.cs` (if a fix), `Release.ruleset`, `sdd/specs/README.md`, `src/Reihitsu.Analyzer.Package/README.MD`.
- Tests: `src/Reihitsu.Analyzer.Test/Naming/RH4115*`, `RH4117*`, new rule tests; with PascalCase also ~870 test files (camelCase `const` locals) for the self-hosting scan.
- Predecessors: R24 (`sdd/specs/README.md`); with PascalCase it runs alone (repository-wide).

### R21 · #1086 — BC — start now
- Files: `Rules/Naming/RH4001TypeNameShouldMatchFileNameAnalyzer.cs`, `RH4001TypeNameShouldMatchFileNameHelper.cs`, `analyzer/RH4001.md` (RH4001-001), RH4001 tests. Predecessors: none.

### R22 · #1089 — BC — start now
- Files: `RH4122SingleLetterParameterNamesAnalyzer.cs`, `RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.cs`, `shared/single-letter-names.md`, `analyzer/RH4122.md`, `RH4124.md`, their tests. Repository code holds query and extension-block single letters only inside test string literals, so no self-hosting churn. Predecessors: none.

### R23 · #1085 — BF — after R21
- Files: RH4001 analyzer/helper/fix (`RH4001TypeNameShouldMatchFileNameCodeFixProvider.cs`), `RH4009NamespaceNotAllowedAnalyzer.cs`, `analyzer/RH4001.md`, `RH4009.md`, tests. The issue allows splitting the two defects.
- Predecessors: R21 (RH4001 files).

### R24 · #1090 — BF (rename, no behavior change) — start now
- Files: `RH4118TupleElementCasingAnalyzer.cs` → `RH4118TupleArgumentNameCasingAnalyzer.cs` (no code-fix provider exists for RH4118), its test class, `analyzer/RH4118.md`, `sdd/specs/README.md` (index row at line 135).
- Predecessors: none; R20 waits for it (spec index).

### R25 · #975 — BC (triage: extension, CLI-013 pins today's header) — start now
- Files: `src/Reihitsu.Cli/FormatCommandHandler.cs` (line 610 passes the full path), `src/Reihitsu.Cli/DiffGenerator.cs` (lines 45–46), `sdd/specs/cli/reihitsu-format.md` (CLI-013 and its non-goal), `src/Reihitsu.Cli.Test/Unit/Diff/DiffGeneratorTests.cs`, an end-to-end test through `Program.Main()`.
- Predecessors: none. (R20 with PascalCase would also rename `const` locals in `DiffGeneratorTests.cs`.)

## 3. Start-now set

No unmerged predecessor, pairwise disjoint by the file sets above (only `requirement-ids.txt` shared):

| Run | Lane |
|---|---|
| `/squad #953 #1852 #1896 #1818` | behavior-change |
| `/squad #1880` | tooling |
| `/squad #975` | behavior-change |
| `/squad #1432` | behavior-change |
| `/squad #1566` | bug-fix |
| `/squad #1472` | docs |
| `/squad #1912` | tests |
| `/squad #1240 #1591` | docs |
| `/squad #1836` | behavior-change |
| `/squad #970 #1668` | behavior-change |
| `/squad #971 #1072 #1451 #1434` | behavior-change |
| `/squad #1086` | behavior-change |
| `/squad #1089` | behavior-change |
| `/squad #1090` | bug-fix |

Residual risks inside the set: #953's test updates are known only when built (a hit on a test file of #1912 or #1432 would collide); #1566's spec could land in `blank-lines.md` (then it waits for #953); #1432 could need `LayoutComputer`'s raw-string column helpers (then it waits for #953).

## 4. After-merge list

- after #953 merges → `/squad #1633 #1634 #1647 #1685 #1542`
- after #953 and #1240 #1591 merge → `/squad #1275 #1837`
- after #953 and #1912 merge → `/squad #1607`
- after #1633 … #1542 merges (and #1566, if it touched `line-breaks.md`) → `/squad #1568 #1698 #1567`
- after #1568 #1698 #1567 merges → `/squad #1399`
- after #1399 merges → `/squad #1400`
- after #1432 merges → `/squad #1722`
- after #1086 merges → `/squad #1085`
- after #971 … #1434 merges → `/squad #1771` (escalation 9 before building)
- after #1090 merges and the #1088 casing question is answered camelCase → `/squad #1087 #1088`; answered PascalCase → alone, at the end
- after #953, #1836, #1275 #1837, #970 #1668 merge → `/squad #1589` alone (or 1.7.0)

Critical path: #953 → list/pattern → hole/lambda → #1399 → #1400 (five serial formatter runs on `indentation.md`).

## 5. Premise flags

- #1880: precondition met (binaries rebuilt in 5af53bd, the v1.5.0 commit).
- #953: many instances added in comments (switch arm `=>`, `with`, `return⏎(1)`, query clauses, lambda body below `=>`, declarator lists, `[() =>⏎1]` CLI vs RH5305 fix); the 2026-11-17 comment asks for a test of both entry points. FMT-UNSUP-011 already removed the binary-operator comment cases.
- #1852, #1896, #1818: triage comments put their output questions into #953's run.
- #1566: triage (2026-11-06) "Maintenance — formatter used only from C# 12, not critical"; reachable only through `--lang-version` below 11 (reproduced with `--lang-version 10`).
- #1568, #1567, #1698: agent-filed output questions; #1698's recommendation is low-confidence by its own text.
- #1542: comments add `is not⏎(…)`, `{ X:⏎1 }` (falls to #953), case-label `(int⏎or long)` (also #1442), and a comma-first positional pattern (stays as written per roadmap; FMT-INDENT-033 now exists for other comma lists).
- #1633: comment adds comma-first base lists (stays as written per roadmap).
- #1399: comments add list/parenthesized patterns and a switch-section `#endregion` 4 columns left of its `#region`, whose requirement was not checked.
- #1400: item 24 already done; items 22 and 23 remain.
- #1432: names a phase that no longer exists; RH5202 parity widens it (see R8).
- #1722: premise holds (`IsHoleOpeningLine` still duplicates `IsInterpolationBrace`).
- #1240, #1591, #975, #1090: premises still hold on main (non-goal at `diagnostics.md:14`; `## Exceptions` in RH7003.md; `--- a/{filePath}` in `DiffGenerator.cs:45`; `RH4118TupleElementCasingAnalyzer` unchanged).
- #1275: comment correction — `class A { #if DEBUG #endif }` IS reported by RH5412 today, so the conditional-directive case is a change, not "already holds". Blocked by #1240 (open) and #1260 (closed).
- #1836, #1837: output questions; option B closes the issue.
- #1668: maintainer triage 2026-11-09 is binding: if checking which conditional blocks matter is non-trivial, refuse any conditional block in the moved span (fewer fixes: visible, output question).
- #970: triage reclassified as an extension (BC, not BF); the balanced-`#pragma` check must stay a cheap check (matching diagnostic IDs between disable and restore is the risk; otherwise withhold, or escalation 10).
- #971, #975: triage reclassified as extensions (BC).
- #1434: SHARED-CASING-011 is taken on main; use the next free ID. Fix-withholding for metadata overrides already exists (casing.md line 658); the change is to stop reporting.
- #1771: needs escalation 9 (new shared component in the casing base) before building.
- #1088: casing of local constants decides a repository-wide rename (§1.7).
- #1589: audit with an unknowable file set (§1.6).
- #1912: 22 files, 261 matching lines; not repository-wide.
