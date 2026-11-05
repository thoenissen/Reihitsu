Role: test implementer
Commit: 3de4ed75
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (FMT-PIPE-006/029/030/031, RH7101-001/002/004), the change doc's cases 1–12, and, where a requirement says "as the phases lay out", from document-level formatting on `main` (`scripts/format.sh`) or, for FMT-PIPE-030, from today's RH7203 fix (`scripts/apply-fix.sh RH7203`), which the spec names as the oracle. None comes from a patched copy of the code (FC-035).

## Compile state at this commit

The tests call two members that do not exist yet: `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync(Document, SyntaxNode, CancellationToken)` and `ReihitsuFormatter.SplitsFieldDeclaration(FieldDeclarationSyntax)`, with the signatures FMT-PIPE-009/030/031 name. Until the implementer adds them, `Reihitsu.Formatter.Test` does not compile, and therefore neither does `Reihitsu.ArchitectureTests` (it references that project). No production code was touched on the branch.

To watch every test fail for the right reason, I built the tests in a scratch worktree (deleted since) with two throwaway stubs added to `ReihitsuFormatter` (`OrganizeUsingDirectivesInDocumentAsync` returning the document, `SplitsFieldDeclaration` returning false). Results there, Debug build:

- Formatter: 39 failed, 4430 passed. The 39 are exactly the new or flipped tests below marked "fails". Every FMT-PIPE-029 failure shows the document returned unchanged, as 02-skeptic predicted (`FindCorrespondingNode` misses the duplicated first token). Every FMT-PIPE-030 failure shows the stub's unchanged document. The FMT-PIPE-006 node-level failures show the document unchanged because of the header check.
- Analyzer: 32 failed, 6461 passed. The 32 are exactly the RH7101 and RH5417 tests below marked "fails". The diffs show today's split-only layout (`/* c */` at column 0, `[NonSerialized] private int _x;` on one line, shared lines kept, a second declaration split too, lambda left unbraced), the RH7101 diagnostic reported in the syntax-error and extension-block files, and RH5417's fix producing no change in a header-marked file.
- Architecture (`CodeFixFormatterAccessTests` and `Sdd`): 1 failed, 58 passed. The failure names exactly `RH7101DoNotCombineFieldsCodeFixProvider → FieldDeclarationSplitTransform` and `UsingDirectiveCodeFixUtilities → UsingDirectiveOrderingPhase`. The RH3202/RH3205–RH3208 use of `ExpressionBodiedTransformUtilities` is not flagged. `EveryRequirementOfAnEnforcedSpecIsCovered` passes now that FMT-PIPE-029/030/031 are tagged.

## Tests

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterFieldSplitTests.cs` (new). Each test runs `FormatNodeInDocumentWithContextAsync` with the combined field as target and its type as context, under LF and CRLF, and runs a second pass with the field that now declares the variable.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SplitTargetIsWrittenBackWithTheLayoutOfThePhasesAndNothingElseOfTheContext | FMT-PIPE-029 | `AssertSplitsTarget` | fails |
| LaterFieldOfATargetOffItsMemberColumnStandsAtTheMemberColumn | FMT-PIPE-029 | same | fails |
| FirstFieldOfATargetBehindAMemberOnItsLineStartsTheNextLine | FMT-PIPE-029, FMT-PIPE-018, FMT-PIPE-010 | same | fails |
| CommentsInTheGapsStayBehindTheTokenInFrontOfThem | FMT-PIPE-029, FMT-PIPE-010 | same | fails |
| MemberBehindTheTargetOnItsLineStartsTheNextLine | FMT-PIPE-029, FMT-PIPE-010 | same | fails |
| DocumentationCommentsInTheGapsGoWithTheMemberBehindThem | FMT-PIPE-029, FMT-PIPE-018 | same | fails |
| MemberBehindTheLastLineOfATwoLineTargetStartsTheNextLine | FMT-PIPE-029 | same | fails |
| TabInFrontOfTheTargetStaysAndTheLaterFieldIsIndentedWithSpaces | FMT-PIPE-029 | same | fails |
| TabInFrontOfTheContextCountsAsOneColumn (maintainer answer: 5 spaces) | FMT-PIPE-029 | same | fails |
| TargetBetweenTheBracesOfItsTypeKeepsItsLine | FMT-PIPE-029 | same | fails |
| MethodInFrontOfTheTargetOnItsLineIsNotFormatted | FMT-PIPE-029, FMT-PIPE-010 | same | fails |
| SecondOfTwoCombinedDeclarationsOnOneLineStartsTheNextLine | FMT-PIPE-029 | same | fails |
| FirstOfTwoCombinedDeclarationsOnOneLineStartsTheOtherOnTheNextLine | FMT-PIPE-029 | same | fails |
| SplitFieldsAreWrittenBackWhateverTheFirstTokenOfTheTarget (5 rows: type, modifier, `[A]` on its own line, `[A]` on the line, `[A, B]`) | FMT-PIPE-029; FC-009 (04-skeptic class C) | same | fails |
| SplitFieldsAreWrittenBackInEveryTypeKind (class, struct, record, record struct, interface) | FMT-PIPE-029 | same | fails |
| LeadingTriviaOfTheTargetStaysAsInTheDocument | FMT-PIPE-029 | same | fails |

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterSplitsFieldDeclarationTests.cs` (new)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ReturnsTrueForACombinedDeclarationAndFalseForOneWithADirective (spec example) | FMT-PIPE-031 | direct call | fails (compile; with a false stub, fails on the true half) |
| ReturnsTrueForACombinedDeclarationInEveryTypeKindTheSplitSplitsIn (5 rows) | FMT-PIPE-031 | direct call | fails |
| ReturnsFalseForADeclarationWithOneDeclarator | FMT-PIPE-031 | direct call | compile only; passes on the false side |
| ReturnsFalseForACombinedDeclarationInAnExtensionBlock | FMT-PIPE-031 | direct call | compile only; passes on the false side |
| ReturnsFalseForACombinedDeclarationThatCarriesADirective (`#region` in leading trivia, `#pragma` between declarators, `#if` around) | FMT-PIPE-031 | direct call | compile only; passes on the false side |

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterUsingDirectivesTests.cs` (new). Each test runs under LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| NamespaceScopeIsOrganizedAndTheCodeBehindTheBlockIsNotFormatted (spec example 1) | FMT-PIPE-030 | `AssertOrganizes` | fails |
| ScopeThatDoesNotStartItsLineIsReadWhereItStandsInTheDocument (spec example 2, 04-skeptic class D) | FMT-PIPE-030 | same | fails |
| CompilationUnitScopeIsOrganizedAndANestedNamespaceIsLeftAsItIs | FMT-PIPE-030 | same | fails |
| FileScopedNamespaceScopeIsOrganized | FMT-PIPE-030 | same | fails |
| BlockOfADocumentWithASyntaxErrorIsOrganized | FMT-PIPE-030, FMT-PIPE-005 | same | fails |
| BlockOfADocumentWithAGeneratedHeaderIsOrganized | FMT-PIPE-030, FMT-PIPE-006 | same | fails |

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterEntryPointTests.cs` (changed). Three tests flip under answer 4b A. They replace `…ReturnsGeneratedDocumentUnchanged…`, whose expected values the change reverses.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FormatNodeInDocumentAsyncFormatsTargetOfGeneratedDocument (spec example) | FMT-PIPE-006 | `AssertNodeInDocumentText` | fails |
| FormatNodeInDocumentWithContextAsyncFormatsTargetOfGeneratedDocument | FMT-PIPE-006 | same | fails |
| FormatNodeInDocumentWithContextAsyncFormatsTargetOfGeneratedDocumentWhenTargetIsContext | FMT-PIPE-006 | same | fails |
| FormatNodeFormatsNodeOfGeneratedFile (existing; tag added) | FMT-PIPE-006, FMT-PIPE-012 | existing | passes |

`src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs` (changed). Five existing tests change their expected output to the change doc's "after" (cases 2 and 3). Two of them are renamed because their old names asserted the removed layout. Stale summaries that said "the fix runs the split without the pipeline" are rewritten (FC-031). New tests follow.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifySingleLineDocumentationCommentBeforeDeclaratorIsLaidOutAsTheFormatterLaysItOut (was …GetsNoBlankLine) | RH7101-004 | `Verify` | fails |
| VerifyMultiLineDocumentationCommentBeforeDeclaratorKeepsItsLineBreak | RH7101-004 | `Verify` | fails |
| VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix | RH7101-004 | `Verify` | fails |
| VerifyFixLaysOutTheCommentBeforeFirstDeclaratorAsTheFormatterLaysItOut (was VerifyFixKeepsTheOwnIndentation…; case 3) | RH7101-004 | `Verify` | fails |
| VerifyFixLaysOutAFirstDeclaratorWithoutACommentAsTheFormatterLaysItOut (was VerifyFixKeepsTheIndentation…) | RH7101-004 | `Verify` | fails |
| FixGivesACommentInFrontOfALaterDeclaratorTheBlankLineTheFormatterWrites (spec example, case 1) | RH7101-004 | `Verify` + `AssertFormattingChangesNothing` | fails |
| FixKeepsOneBlankLineTheAuthorWroteInFrontOfACommentInFrontOfALaterDeclarator | RH7101-004 | `Verify` | fails |
| FixLaysOutDocumentationCommentsInFrontOfLaterDeclaratorsAsTheFormatterLaysThemOut (case 2) | RH7101-004 | `Verify` + formatter stability | fails |
| FixSpacesTheSplitDeclarationAsTheFormatterSpacesIt (case 4) | RH7101-004 | same | fails |
| FixSplitsTheAttributeListsOfTheSplitDeclarationAsTheFormatterSplitsThem (case 5) | RH7101-004 | same | fails |
| FixSplitsADeclarationThatStartsWithAnAttributeListOnItsLine | RH7101-004; FC-009 | `Verify` | fails |
| FixSplitsADeclarationThatStartsWithItsType | RH7101-004; FC-009 | `Verify` | passes (characterization) |
| FixFormatsTheInitializersOfTheFieldsItCreates (case 6, two diagnostics; single fixes and Fix All) | RH7101-004; FC-027, FC-030 | `Verify` + formatter stability | fails |
| FixGivesTheFieldsAndTheMembersOnTheirLineALineOfTheirOwn (spec example, case 7) | RH7101-004 | `Verify` | fails |
| FixStartsTheFirstFieldOnTheNextLineBehindAMemberInFront | RH7101-004 | `Verify` | fails |
| FixStartsTheMemberBehindTheDeclarationOnTheNextLine | RH7101-004 | `Verify` | fails |
| FixStartsTheFirstFieldOnTheNextLineBehindAMethodInFront | RH7101-004 | `Verify` | fails |
| FixMovesADocumentationCommentInTheGapInFrontWithTheFirstField (case 7) | RH7101-004; FC-020/021 | `Verify` | fails |
| FixMovesADocumentationCommentInTheGapBehindWithTheMemberItDocuments (case 7) | RH7101-004; FC-020/021 | `Verify` | fails |
| FixKeepsTheLineOfADeclarationBehindTheOpeningBraceOfItsType | RH7101-004 | `Verify` | fails |
| FixPlacesTheLaterFieldOfADeclarationOffItsMemberColumnAtTheMemberColumn (spec example, case 8) | RH7101-004 | `Verify` + `AssertSingleFixUnderBothLineEndings` | fails |
| FixKeepsTheTabInFrontOfTheFirstFieldAndIndentsTheLaterFieldWithSpaces (case 8, tab) | RH7101-004 | same | fails |
| FixCountsTheTabOfATabIndentedTypeAsOneColumn (maintainer answer, 5 spaces) | RH7101-004 | `Verify` | fails |
| FixOfOneDeclarationLeavesAnotherDeclarationOfTheTypeCombined (spec example; the other keeps its diagnostic) | RH7101-004; FC-030 | `AssertSingleFixUnderBothLineEndings` + `Verify` | fails |
| FixOfADeclarationLeavesADeclarationOfANestedTypeCombined (case 9) | RH7101-004; FC-030 | same | fails |
| FixOfADeclarationOfANestedTypeLeavesADeclarationOfTheOuterTypeCombined | RH7101-004 | `AssertSingleFixUnderBothLineEndings` | passes (characterization) |
| FixOfTheFirstOfTwoDeclarationsOnOneLineStartsTheOtherOnTheNextLine | RH7101-004 | same | fails |
| FixOfTheSecondOfTwoDeclarationsOnOneLineStartsItsFirstFieldOnTheNextLine | RH7101-004 | same | fails |
| FixAllSplitsTwoDeclarationsOnOneLine (harness default Fix All iterations, 04-skeptic class B) | RH7101-004; FC-008 | `Verify` | fails |
| FixSplitsACombinedDeclarationOfEveryTypeKind (5 rows) | RH7101-004 | `Verify` | passes (characterization) |
| FixLaysOutACommentInFrontOfALaterDeclaratorUnderBothLineEndings | RH7101-004 | `AssertSingleFixUnderBothLineEndings` | fails |
| FixGivesTheFieldsAndTheMembersOnTheirLineALineOfTheirOwnUnderBothLineEndings (`/* c */ … /* d */`) | RH7101-004 | same | fails |
| CombinedDeclarationIsNotReportedInAFileWithASyntaxError (case 10) | RH7101-001, SHARED-JOIN-002 | `Verify` (compiler diagnostics off) | fails |
| CombinedDeclarationIsReportedAndFixedOnceTheSyntaxErrorIsRemoved | RH7101-001, SHARED-JOIN-002 | `Verify` | passes (other side of the condition) |
| CombinedDeclarationInAnExtensionBlockIsNotReported (spec example, case 11) | RH7101-002 | `Verify` (compiler diagnostics off, CS9282) | fails |
| FixSplitsADeclarationInAFileWithAGeneratedHeaderThatTheAnalyzersAnalyze (case 12, `generated_code = false` global config; FC-033) | RH7101-004 | `Verify` | passes today (today's fix needs no entry point); must still pass after |

`src/Reihitsu.Analyzer.Test/Formatting/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.cs` (changed)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyMethodBodyInAFileWithAGeneratedHeaderThatTheAnalyzersAnalyzeIsFixed (change doc case 12, other node-level fixes; FC-009, FC-033) | FMT-PIPE-006 | `Verify` with `generated_code = false` | fails |

`src/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs` (changed). These pin the using fixes' unchanged output through the new entry point.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixOfABlockInANamespaceThatDoesNotStartItsLineKeepsTheLineIndentation (04-skeptic class D) | RH7203-004 | `Verify` | passes, must stay |
| CodeFixOfANamespaceBlockPlacesSplitDirectivesAtTheScopeIndentation | RH7203-004 | `Verify` | passes, must stay |

`src/Reihitsu.ArchitectureTests/Analyzer/CodeFixFormatterAccessTests.cs` (new). It is tagged with `Reihitsu.Analyzer.Test.Base.RequirementAttribute` through a using alias, because the architecture project has no attribute of its own. The coverage catalog reads the attribute by name from source.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixesDoNotDependOnFormatterPhasesTransformsOrThePipeline (banned: every non-interface `IFormattingPhase`, every `CSharpSyntaxRewriter` of the formatter assembly, `FormattingPipeline`) | FMT-PIPE-009; FC-015 | ArchUnitNET `NotDependOnAny` | fails (names the two violators) |
| BannedTypesIncludeThePhasesTheTransformsAndThePipeline (4 rows) | FMT-PIPE-009 | reflection | passes |
| BannedTypesExcludeHelpersAndUtilityClasses (`ReihitsuFormatter`, `ReihitsuFormatterHelpers`, `ExpressionBodiedTransformUtilities`) | FMT-PIPE-009 | reflection | passes |

Existing tests that keep their outputs and now run through the new paths are unchanged. They include the other RH7101 plain, documented, misaligned-doc-comment, brace-line doc-comment, `/** */` before `;` and `,`, and directive-withholding tests, the Fix All scenario, every RH7201–RH7207 code-fix test, `UsingDirectiveCodeFixUtilitiesTests`, and `RH7101DoNotCombineFieldsFormatterTests` (document level).

## Not covered, and why

- **FMT-STRUCT-026** (the split's phase output without indentation, with the three example shapes). It is not in my prompt's scope list. The change is observable through every FMT-PIPE-029 and RH7101 test above, and document-level output is guarded by `FieldDeclarationSplitTests` and `RH7101DoNotCombineFieldsFormatterTests`. A phase-output test on `StructuralTransformSeparatorTests` (`GetPhaseOutput` with `StructuralTransformPhase`) is the place, if the lead wants one.
- **FMT-PIPE-030 outside its supported input** (fewer than two directives, a block that may not be reordered). The spec decides only the RH7201–RH7207 input (`AGENTS.md`, *Supported inputs*). The existing `UsingDirectiveCodeFixUtilitiesTests` characterize both.
- **FC-020 directive in a shared-line gap.** A directive must start its own line, so no shared line holds one. Directives inside or in front of the declaration are covered by the existing RH7101-005 tests and the FMT-PIPE-031 directive rows.
- **FC-018, FC-022–FC-026, FC-029, FC-036.** No rename, literal rewrite, expression-bodied conversion, or paired key/value layout is in the change. FC-022 literals reach the split only as initializers, which case 6 covers by its structural rewrite.
- **FC-010/FC-012 (phase idempotency).** Every FMT-PIPE-029 test runs a second pass. Document-level stability is covered by the self-hosting scan in CI.
- **FC-028 (FormatNode vs node-in-document).** `FormatNode` is not on the RH7101 path; its header behavior is pinned by `FormatNodeFormatsNodeOfGeneratedFile`.
- **Header marker matched with a different case** (02-skeptic's unverified route). This is a follow-up candidate in `00-lead.md` (detection alignment), outside the run.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterFieldSplitTests|FullyQualifiedName~ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~CodeFixFormatterAccessTests|FullyQualifiedName~Sdd"
```
