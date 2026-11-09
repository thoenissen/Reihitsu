Role: test implementer
Commit: af76436c
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (binding clarifications, answers of batches 1 and 2, Decisions taken), handovers 01–04, the accepted change doc, and the spec diff against `158e8fc`.

## API the tests are written against

The specs name `FormattingResult<T>` (`Output`, `Unsupported`), `UnsupportedSyntax` (reason and location), `ReihitsuFormatter.FindUnsupportedSyntax`, `SyntaxNotSupportedException`, and the phase name `PrecheckPhase`. Where the specs leave a shape open, the tests assume this one. The test projects do not compile until it exists:

- `Reihitsu.Formatter.FormattingResult<T>`: public, properties `T Output` and `UnsupportedSyntax Unsupported`.
- `Reihitsu.Formatter.UnsupportedSyntax`: public, constructor `(string reason, Location location)`, properties `string Reason` and `Location Location`. The tests read `Location.SourceSpan`, `Location.SourceTree`, and `Location.GetLineSpan()`.
- `Reihitsu.Formatter.SyntaxNotSupportedException`: reachable from the test assembly, constructor `(UnsupportedSyntax unsupported)`.
- Namespace: all three types are in `Reihitsu.Formatter`. The test namespaces see them without a `using`.
- Return types: `ReihitsuFormatter.FormatDocumentAsync`, `FormatSyntaxTree`, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` return `FormattingResult<…>` of their former type (`Task<FormattingResult<Document>>` for the async ones). `FindUnsupportedSyntax(SyntaxNode)` returns `UnsupportedSyntax` or null.
- `FormattingPipeline.Execute` (both overloads) and `FormattingPipeline.ExecutePhases` return `FormattingResult<SyntaxNode>`. Decision 3 makes `ExecutePhases` the place where a phase's refusal becomes the outcome.
- The observer reports the precheck as `"PrecheckPhase"`, before `PreparationPhase/TabReplacement`.
- `ParseResult.SuppressUnsupported` (bool) exists.
- Location of a comment: the span starts at the comment's first character, also for `///` and `/** */`. Roslyn's `SyntaxTrivia.Span` of documentation-comment trivia starts behind the `///`/`/**` exterior, so it does not meet this. The span's text is the comment, apart from a line break that a single-line comment ends with (`FormatterTestsBase.AssertLocationIsShape`).

Every existing call of these methods in the test projects was migrated to `.Output`. That is about 370 call sites in 62 files, by a paren-matching script, plus manual fixes. No assertion was changed by that migration.

## Tests

New files:

- `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedVariableDeclarationTests.cs`: 68 tests.
- `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedSyntaxEntryPointTests.cs`: 18 tests.
- `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedSyntaxClosureTests.cs`: 2 tests, 10 cases.
- `src/Reihitsu.Cli.Test/EndToEnd/CliUnsupportedSyntaxEndToEndTests.cs`: 13 tests.

New helpers in `FormatterTestsBase`: `AssertUnsupported` (FormatSyntaxTree under LF and CRLF; output unchanged, reason, location in the caller's tree), `AssertSupported` (not refused, second pass stable), `AssertLocationIsShape`, `GetShapeSpan`, and the constant `VariableDeclarationCommentReason`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `UnsupportedVariableDeclarationTests.SpecExample*` (6: issue cases, attributes/modifiers/type, value-to-terminator, other side, nested declaration, every kind) | FMT-UNSUP-004 | `AssertUnsupported` / `AssertSupported` | compile error |
| `UnsupportedVariableDeclarationTests` — one test per place: type/name, name/`=`, behind `=`, between declarators, behind/in front of `,`, in front of a value (one token, several tokens, own line), behind a value's last token (one token, several, before `,`), inside a value (first token's trailing, last token's leading, own line), comment-looking string text, own line in front of `;`, behind `;`, above and in front of the declaration, attribute list/arguments/after list, between modifiers, inside type, in front of later declarator | FMT-UNSUP-004; FC-003 (both sides of each boundary), FC-022 | `AssertUnsupported` / `AssertSupported` | compile error |
| `…` per kind: event field, fixed buffer brackets and size, `for` initializer (comma, before/behind first `;`), using declaration (type, `using` keyword), `const`, using statement (type, before/behind `)`, behind `(`), `fixed` (type, own line before `)`) | FMT-UNSUP-004 | same | compile error |
| `…` documentation modes: `/** */` in front of `;` (Parse, None), between value and `,` (Parse, None), `///` in front of later declarator, `/** */`/`///` behind type (Parse, Diagnose) | FMT-UNSUP-004 | same | compile error |
| `DirectiveInsideADeclarationIsSupported`, `CommentLookingDisabledTextInsideADeclarationIsSupported` | FMT-UNSUP-004; FC-005, FC-020 | `AssertSupported` | compile error |
| Siblings without a variable declaration: property initializer, parameter default, enum value, `foreach`, `out var`, deconstruction, `catch`, declaration pattern | FMT-UNSUP-004; FC-016 | `AssertSupported` | compile error |
| `FirstOfSeveralCommentsInTheTextIsReported`, `RefusedDeclarationBehindSupportedCommentsIsReported` | FMT-UNSUP-001, FMT-UNSUP-004 | `AssertUnsupported` | compile error |
| `UnsupportedSyntaxEntryPointTests.SpecExampleFormatSyntaxTreeReturnsTheTreeUnchangedAndReportsTheComment` (line 3, col 13, LF+CRLF), `SpecExampleShapeThatStartsFirstInTheTextIsReported`, `SpecExampleSyntaxErrorIsNotReportedAsUnsupported`, `FormattedTreeHasNoUnsupportedSyntax` | FMT-UNSUP-001 | direct | compile error |
| `SyntaxErrorBesideAnUnsupportedShapeIsNotReportedAsUnsupported`, `GeneratedCodeWithAnUnsupportedShapeIsNotReportedAsUnsupported`, `FormatDocumentAsyncReturnsTheDocumentUnchangedAndReportsTheComment`, `FormatNodeReturnsTheNodeUnchangedAndReportsTheComment`, `SpecExampleFormatNodeInDocumentAsyncRefusesATargetThatHoldsTheShape`, `SpecExampleContextEntryPointRefusesAContextThatHoldsTheShape` | FMT-UNSUP-001, FMT-UNSUP-002 | direct | compile error |
| `SpecExampleFormatNodeInDocumentAsyncFormatsATargetWithoutTheShape` | FMT-UNSUP-002 | direct | compile error |
| `FormatNodeFormatsANodeBesideAnUnsupportedShape`, `ContextEntryPointFormatsATargetWhoseContextHoldsNoShape` | FMT-UNSUP-002, FMT-UNSUP-003 | direct | compile error |
| `SpecExampleTargetBelowTheDeclarationIsFormattedWhileTheDeclarationIsRefused` | FMT-UNSUP-003 | direct | compile error |
| `SpecExampleFindUnsupportedSyntaxAnswersForTheNodeItIsGiven`, `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (4 rows), `FindUnsupportedSyntaxAnswersForABuiltNode` | FMT-UNSUP-007; FC-015 (one predicate) | direct | compile error |
| `FindUnsupportedSyntaxAnswersForTheNodeAlone` | FMT-UNSUP-003, FMT-UNSUP-007 | direct | compile error |
| `UnsupportedSyntaxClosureTests.SpecExampleFieldSplitKeepsACommentInsideAValueInsideThatValue`, `OutputOfAFormattedInputIsNotUnsupported` (9 rows, incl. doc-comment move, statement split, wrapped value) | FMT-UNSUP-005; FC-010, FC-012 | two FormatSyntaxTree passes, LF+CRLF; `AssertRuleResult` | compile error |
| `FormattingPipelineTests.ExecuteRunsNoPhaseBehindThePrecheckOnARefusedInput` | FMT-PIPE-001, FMT-UNSUP-001 | `FormattingPipeline.Execute` + observer | compile error |
| `FormattingPipelineTests.ExecutePhasesTurnsAPhasesRefusalIntoTheUnsupportedOutcome`, `…AnObservedPhasesRefusal…` (refusing `ThrowingPhase` behind a `TextChangingPhase`, with and without observer) | FMT-UNSUP-006 | `ExecutePhases` | compile error |
| `FormattingPipelineTests.ExecuteReportsNoUnsupportedOutcomeForAFormattedInput` | FMT-UNSUP-001 | `Execute` | compile error |
| `FormattingPipelineTests.ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput` (now expects `PrecheckPhase` first) | FMT-PIPE-001 | changed | compile error |
| `ReihitsuFormatterUsingDirectivesTests.UsingBlockOfADocumentWithAnUnsupportedShapeIsOrganized` | FMT-UNSUP-002 | `AssertOrganizes` | passes on today's code once it compiles |
| `ReihitsuFormatterNodeColumnTests.EqualsValueTargetBehindTheLastLineOfAMultiLineCommentIsLaidOutFromNewWhereItStands` (document-level assertion is now refusal) | FMT-PIPE-017, FMT-INDENT-030, FMT-UNSUP-003 | `AssertUnsupported` | compile error |
| `ReihitsuFormatterNodeColumnTests.ChainTargetWithContextBehindTheLastLineOfAMultiLineCommentAlignsTheWrappedLink` (same change) | FMT-PIPE-017, FMT-INDENT-029 | `AssertUnsupported` | compile error |
| `ReihitsuFormatterSplitsFieldDeclarationTests.SpecExampleReturnsFalseOnlyForTheDocumentedDeclaration`, `ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone`, `ReturnsTrueForACombinedDeclarationWithACommentInEveryDocumentationMode` (44 rows merged from five former tests; Parse/Diagnose/None) | FMT-PIPE-031 | direct | compile error; then red: the split still has its comment conditions |
| `ReihitsuFormatterFieldSplitTests.SplitTargetIsWrittenBackWithTheLayoutOfThePhasesAndNothingElseOfTheContext` (new spec example) | FMT-PIPE-029 | `AssertSplitsTarget` | compile error |
| `FieldDeclarationSplitTests.SpecExampleSplitFieldsKeepTheirLeadingTriviaTheirValueCommentsAndTheCommentBehindTheSemicolon`, `CommentInsideTheValueOfALaterDeclaratorStaysInThatValue` | FMT-STRUCT-022 | `AssertRuleResult` | compile error |
| `FieldDeclarationSplitTests.SpecExampleDocumentedDeclarationsAreNotSplit`, `SpecExampleOrdinaryCommentInFrontAndCommentInsideTheFirstValueDoNotKeepTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | compile error |
| `HorizontalSpacingRequirementTests.SpecExampleOneSpaceBetweenFixedAndTheTypeOfAFixedSizeBuffer` | FMT-SPACE-012 | `AssertRuleResult` | compile error |
| `HorizontalSpacingRequirementTests.CommentBetweenFixedAndTheTypeOfAFixedSizeBufferIsUnsupported` | FMT-UNSUP-004 | `AssertUnsupported` | compile error |
| `HorizontalSpacingRequirementTests.CommentOfTheRightTokenKeepsItsPlace` (input now `int P => 1 /** x */;`) | FMT-SPACE-014 | `AssertRuleResult` | compile error |
| `DocumentationCommentRequirementTests.StrandedDocumentationBeforeSemicolon…` (`int A => 1 /// stray`) | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | compile error |
| `GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedComment…` (`int[] G { get; } =`) | FMT-BLANK-011, FMT-BLANK-018 | `AssertRuleResult` | compile error |
| `ListAndHeaderAlignmentTests.AttributeListsAndTypeArgumentListsAlignToTheirFirstElement` (`string> Map => null;`) | FMT-INDENT-015 | `AssertRuleResult` | compile error |
| RH7101: `SpecExampleCommentInsideTheDeclarationWithholdsTheFix`, `SpecExampleCommentElsewhereInTheTypeWithholdsTheFix`, `CommentInANestedTypeWithholdsTheFixOfADeclarationOfTheOuterType`, `SpecExampleCommentInsideAnInitializerValueDoesNotWithholdTheFix` (+RH7101-004), and 22 converted tests (comma, later declarator, first name, first value, `;`, doc comments in Parse/Diagnose/None, comments spanning lines, both line endings) | RH7101-007 (doc ones also RH7101-006) | `VerifyNoCodeFixIsOffered` / `Verify` | compile error; then red: fix offered |
| RH7101: `CommentInOneDeclarationWithholdsTheFixForEveryDeclarationOfTheType`, `DocumentationStyleCommentsBehindTheTypesWithholdTheFixForEveryDeclarationOfTheType` | RH7101-007, SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | same |
| RH7101: `CommentInTheOuterTypeDoesNotWithholdTheFixOfADeclarationOfANestedType` | SHARED-FIX-010 | `Verify` | compile error; then green |
| RH7101: `FixAllSplitsOnlyTheDeclarationsOfTypesTheFormatterDoesNotRefuse` | SHARED-FIX-010, SHARED-FIX-002; FC-008 | `Verify` (Fix All) | then red: 2 iterations |
| RH7101: `SpecExampleFixKeepsTheCommentsInFrontOfTheDeclarationInsideAValueAndBehindTheSemicolon` | RH7101-004 | `Verify` | then green |
| RH2001: `VerifyNoFixIsOfferedWhenTheBuiltFieldHoldsACommentTheFormatterRefuses` | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | then red |
| RH2001: `VerifyNoFixIsOfferedForACommentLineInFrontOfTheType` (RH2001-007, -009, SHARED-FIX-010), `…BehindTheTypeOnTheSameLine` (RH2001-007, SHARED-FIX-010), `…InFrontOfTheInitializerValue`, `…CommentLineInFrontOfTheInitializerValue` (LF/CRLF), `…BehindTheInitializerValue` | RH2001-007 | `VerifyNoCodeFixIsOffered` | then red |
| RH2001: `VerifyFixIsOfferedForACommentInsideTheInitializerValue` | RH2001-007 | `Verify` | then green |
| RH2001: `VerifyNoFixIsOfferedForACommentAmongTheModifiersOrInFrontOfTheType` (spec example, LF/CRLF), `…ThatNoModifierCrosses`, `…InFrontOfUnsafe`, `…AnEndOfLineCommentBetweenModifiersThatKeepTheirOrder` | RH2001-009 | `VerifyNoCodeFixIsOffered` | then red |
| RH7004: `NoFixIsOfferedForACommentInFrontOfTheSemicolon` (built using statement holds a comment on its own line in front of `)`) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | then red |
| RH5111: `VerifyNoFixIsOfferedForADeclaratorBehindAMultiLineCommentBehindTheSeparatingComma` | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | then red |
| RH5102: `VerifyNoFixIsOfferedWhenTheArgumentListHoldsADeclarationTheFormatterRefuses` | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | then green (an existing guard already withholds it) |
| RH5303: `VerifyCreationBehindTheLastLineOfAMultiLineCommentIsFixedFromNewWhereItStands` (tag added; fix stays offered) | SHARED-FIX-010 | `Verify` | then green |
| `ProgramTests.ParseArgumentsSuppressUnsupportedFlagSetsSuppressUnsupported`, `…MixesWithPathsAndOptions` | CLI-001 | `Program.ParseArguments` | compile error; then red |
| `CliUnsupportedSyntaxEndToEndTests.SpecExampleWriteModeLeavesTheFileUnchangedAndReportsAnError`, `SpecExampleCheckReportsTheRefusedFileOnlyOnStandardError` | CLI-019, CLI-014, CLI-017 | `Program.Main` | red: exit 0, no error line |
| `…SpecExampleCheckExitsWithErrorAlsoWhenAnotherFileNeedsFormatting`, `…SpecExampleDryRunReportsTheRefusedFileOnlyOnStandardError`, `…WriteModeFormatsTheOtherFilesAndExitsWithError` | CLI-019, CLI-017 | `Program.Main` | red |
| `…Utf8BomDoesNotChangeTheEncodingOfARefusedFile`, `…VerbosePrintsNoFurtherLineForARefusedFile`, `…FileWithSeveralShapesGetsOneLineForTheFirstShape` (4,17), `…LocationOfAShapeInACrlfFileIsReportedAsInAnLfFile`, `…FileWithCommentsOutsideTheDeclarationsOwnTokensIsFormatted` | CLI-019 | `Program.Main` | red (the last one is green) |
| `…SpecExampleSuppressUnsupportedReportsAWarningAndKeepsTheExitCodeOfTheOtherFiles` | CLI-020, CLI-017 | `Program.Main` | red |
| `…SuppressUnsupportedWriteRunLeavesTheFileUnchangedAndSucceeds`, `…SuppressUnsupportedDryRunWithOtherwiseFormattedFilesSucceeds` | CLI-020 | `Program.Main` | red |
| `HomePageTests.SpecExampleInputWithUnsupportedSyntaxIsShownUnchangedWithMessage`, `UnsupportedSyntaxMessageDisappearsOnNextSuccessfulFormat` | PLAY-008 | bUnit | red: no message |

Existing tests whose input FMT-UNSUP-004 now refuses, and whose subject is something else, keep their subject. The construct carrying the shape is replaced by one with no variable declaration, as the spec author did for the spec examples. In each case the column structure is the same, so the expected output follows directly from the old one:

- **Expression-bodied property or method instead of a field:** `ArgumentContinuationLineAlignmentTests` (2), `NestedListInParameterAlignmentTests`, `ListAndHeaderAlignmentTests.WrappedFunctionPointer…`, `ParameterContinuationLineAlignmentTests`, `WrappedAngleBracketListTests` (2), `StrandedDocumentationLineStartTests`, `HorizontalSpacingTests` (2, method return type), `TrailingDocumentationCommentPreservationTests` (2, renamed `…BeforeMemberSemicolon`), `StrandedDocumentationTokenSpacingTests` (renamed `KeepsMemberTerminator…`), `LineBreakRequirementTests.DeclarationSemicolonAfterCommentIsNotJoined`, `DocumentationCommentCollapseGuardTests` (`[field:]` on an auto-property).
- **`return` instead of `var x =`:** `CommentLedFirstTokenAnchorTests` (6). The analyzer formatter tests `RH5301`/`RH5303`/`RH5305 …BehindCommentThatLeadsTheLine` changed the same way.
- **Property initializer `int[] B { get; } =` for array initializers:** `CommentLedFirstTokenAnchorTests` (2; columns shifted by the class depth), `GluedCommentBlankLineTests` (2), `MultiLineGluedCommentBlankLineTests` (2), `CommentBeforeBraceBlankLineTests`.
- **Other:** `RH6003…FormatterTests.VerifyDocumentationCommentBeforeSemicolonStaysAnalyzerClean` (expression-bodied property). `RH5113…FormatterTests` (`VerifySplitFieldWithLineBrokenSeparator…` became `VerifySemicolonBehindACommentOnTheLineAboveStaysAnalyzerClean`, a property with an initializer).

Infrastructure tests changed by the new phase order and the new public types (FMT-PIPE-001, FMT-UNSUP-001):

- `ObservedPipeline.Run`: asserts that the first observed phase is `PrecheckPhase` and that it changes nothing, then returns the phases behind it. With this, the tab and line-ending ownership tests keep their indices.
- `PhaseStabilityCheckTests.CheckRunsTheProductionPhasesInPipelineOrder`: now includes `PrecheckPhase`.
- `PhaseOwnershipDocumentationTests`: 13 phases.
- `PublicApiSurfaceTests`: now admits `FormattingResult`1` and `UnsupportedSyntax` as public types.
- `CodeFixFormatterAccessTests`: exempts both types from the transitive ban, with 2 new DataRows, because a fix must read the result (SHARED-FIX-010).

Deleted tests. Each asserted comment carry-over or comment boundaries that the accepted spec removes (FMT-STRUCT-022, -026, -030, FMT-PIPE-029, FMT-PIPE-031), or was left trivially passing because its input is now refused. Their positions are covered by `UnsupportedVariableDeclarationTests`:

- `FieldDeclarationSplitTests`: 76 tests (every method whose input holds a refused shape; the list is in the diff).
- `StructuralTransformSeparatorTests`: the 12 field-split comment-separator tests.
- `ReihitsuFormatterFieldSplitTests`: `LaterFieldBehindACommentThatSpansLinesBehindTheSeparatorStartsItsOwnLine` and `LaterFieldBehindACommentThatSpansLinesStandsAtTheMemberColumn`.
- `RH7101…`: `VerifyCommentsArePreservedWhenSplitting` (duplicate of the converted spec example) and `VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix` (duplicate of `CommentOnItsOwnLineInFrontOfTheFirstNameWithholdsTheFix`).
- `HorizontalSpacingRequirementTests`: `OneSpaceBehindSpacedCommentGluedToFixedSizeBufferType` (refused, a duplicate of the converted one).

## How the expectations were checked

**Affected tests were found by mechanical scans, not by reading.** First, a scratch copy of the FMT-UNSUP-004 predicate was hooked into `FormattingPipeline.Execute` and the suites were run, which surfaced 129 failures. Second, a Roslyn scan of every string literal in `src/`, in documentation modes Parse and None, listed 378 literals holding a refused shape. Third, the same scan over every `.cs` file under `src/` finds no refused shape in repository source, the new test files included. So the self-hosting scans meet no refused file.

**The suites were run against a scratch implementation, not committed.** It lives in a separate worktree (`git worktree` under the scratchpad). It has the API above, the precheck, the exception catch in `ExecutePhases`, and the entry-point wrappers. Code-fix withholding, the split change, the CLI, and the Playground are not implemented there. Results:

- formatter: 4795 passed, 34 failed. All 34 are the `SplitsFieldDeclaration` tests, which need the split's comment conditions removed (FMT-PIPE-031).
- analyzer: 7679 passed, 52 failed. All are "no code fix may be offered", plus the Fix All iteration count, which is the same cause.
- cli: 14 failed. These are the new E2E and parse tests, failing on exit code, stderr line, or the missing option.
- playground: 2 failed (no message).
- architecture: only `FormattingPhasesShouldBeInFormattingPhaseNamespace` failed, caused by where the scratch put its precheck type. `EveryRequirementOfAnEnforcedSpecIsCovered` passes: every new ID is tagged.
- core and tooling: pass.

**Every rewritten expected output passed against that scratch implementation.** Its formatting behavior is `main`'s for inputs it does not refuse. No expected value of a refused input was taken from code: it is the input unchanged (FC-035).

**The worktree's production code is never committed.** The branch's `src/` production files are unchanged; the commit holds test projects and this handover only.

## Findings for the lead

1. **Spec examples disagree with FMT-SPACE-013.** Four spec examples write `Offset(/* … */ 1)` in their formatted output:
   - FMT-STRUCT-022, the example `private int _x = Offset(/* horizontal */ 1), _y;`
   - FMT-STRUCT-030, the second example (`_first = Offset(/* c */ 1)`)
   - RH7101-004, its example
   - RH7101-007, the third example

   `main` writes `Offset(/* horizontal */1)`. The space behind a comment that `(` carries follows the rule for `(` and `1`, which allows none (FMT-SPACE-013). I checked this with `scripts/format.sh` on a scratch file. FMT-SPACE-013 is unchanged, so the tests assert `Offset(/* … */1)`, and each such test's summary names FMT-SPACE-013. The spec examples need their output corrected, or an input whose spacing is unambiguous, such as `1 + /* c */ 2`. This is spec wording, not a visible output in question: an unchanged requirement decides it.
2. **`ExecuteRunsNoPhaseBehindThePrecheckOnARefusedInput` leaves one point open.** It asserts that no phase other than `PrecheckPhase` is observed on a refused input. It does not assert whether the refusing precheck itself is reported to the observer; the spec does not decide that.

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- **SHARED-FIX-010 for every one of the ~45 formatting fixes.** It is covered for RH7101, RH2001, RH7004, RH5111, RH5102, and RH5303. One requirement applies to all fixes through one query, but no static proof shows that each fix passes exactly the node it formats. The auditor should check the call sites. RH7105/RH7106 already withhold for comments among a member's modifiers (SHARED-MODORDER-003 tests); no test was added.
- **FMT-UNSUP-006 through a public entry point.** No production phase throws, so it is tested through `ExecutePhases` with a refusing test phase, as Decision 3 provides.
- **The trace (`PipelineTraceCommand`) on a refused input.** Its output is not specified. The observer case of FMT-UNSUP-006 is covered in `FormattingPipelineTests`.
- **CLI-018 confirmation count.** It is unchanged by the spec, and existing tests cover it.
- **FC-007, FC-018, FC-036 (naming), FC-025, FC-029, and the tab matrix.** No changed code path reaches them.
- **FC-031** (stale XML summaries in `FieldDeclarationSplitTransform`). It concerns production text and is the auditor's.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck|FullyQualifiedName~FormattingPipelineTests|FullyQualifiedName~ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterFieldSplitTests|FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests"
scripts/test.sh --project formatter
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFields|FullyQualifiedName~RH2001|FullyQualifiedName~RH7004|FullyQualifiedName~RH5111|FullyQualifiedName~RH5102|FullyQualifiedName~RH5303|FullyQualifiedName~RH5301|FullyQualifiedName~RH5305|FullyQualifiedName~RH6003|FullyQualifiedName~RH5113"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliUnsupportedSyntaxEndToEndTests|FullyQualifiedName~ProgramTests"
scripts/test.sh --project playground --filter "FullyQualifiedName~HomePageTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~PublicApiSurfaceTests|FullyQualifiedName~CodeFixFormatterAccessTests|FullyQualifiedName~PhaseOwnershipDocumentationTests|FullyQualifiedName~RequirementCoverage"
```
