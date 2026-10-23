Role: test implementer
Commit: 14f52fd
Verdict: tests written

Specs and tests for existing behavior: every test below is new or newly tagged, pins only a shape that conforms today, and passes on the unchanged code. No production code was touched. No existing test was changed apart from added `[Requirement]` tags on tests that pin a conforming shape. My prompt carried no intended fix or verdict.

Blockers: none. No existing test is tagged with a requirement whose amended wording it contradicts. I checked the tests tagged SHARED-COMMENT-011, SHARED-FIX-008, FMT-PIPE-007, FMT-BLANK-005 and RH5203-001: none uses a comment-led code line in a wrong column, an all-`\r` document, a comment between a directive and the statement after a closing brace, or a multi-line comment tail in front of an argument. Every new requirement has at least one conforming shape.

## Tests

`⏎` is a line break, `␍` a lone `\r`. "Formatter" tests use `FormatterTestsBase.AssertRuleResult`, which runs LF and CRLF and asserts a second pass.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PipelineRequirementTests.WrappingInABlockKeepsOwnLineAndTrailingCommentsOnTheirLines` (spec example) | FMT-PIPE-013; FC-010, FC-021 | `AssertRuleResult` | pass |
| `PipelineRequirementTests.ConvertedExpressionBodyKeepsTheSpaceBetweenTwoTrailingComments` (`=> 42 /* a */ /* b */;`) | FMT-PIPE-013 (space between two comments) | `AssertRuleResult` | pass |
| `PipelineRequirementTests.EveryConvertedExpressionBodyKindKeepsItsTrailingComment` (constructor, finalizer, operator, conversion, local function) | FMT-PIPE-013; FC-025 | `AssertRuleResult` | pass |
| `PipelineRequirementTests.BracedElseBodyKeepsOwnLineAndTrailingCommentsOnTheirLines` | FMT-PIPE-013 | `AssertRuleResult` | pass |
| `PipelineRequirementTests.BracedSwitchSectionKeepsOwnLineAndTrailingCommentsOnTheirLines` (label, own-line and trailing comments) | FMT-PIPE-013 | `AssertRuleResult` | pass |
| `PipelineRequirementTests.ParameterListsAndAssignmentsAreLaidOutAlike` (spec example) | FMT-PIPE-014 | `AssertRuleResult` | pass |
| `PipelineRequirementTests.FunctionPointerParameterListKeepsItsOwnJoinedLayout` | FMT-PIPE-014 (FMT-BREAK-014 choice kept) | `AssertRuleResult` | pass |
| `PipelineRequirementTests.OneLineArrayInitializerAndCollectionExpressionKeepTheirOwnLayout` | FMT-PIPE-014 (FMT-BREAK-021/-022 choices kept) | `AssertRuleResult` | pass |
| `PipelineRequirementTests.PropertyArrowKeepsItsOwnJoinedLayout` (`int P⏎=> 1;`) | FMT-PIPE-014 (FMT-BREAK-026 choice kept) | `AssertRuleResult` | pass |
| `PipelineRequirementTests.StatementBehindTheEndOfAMultiLineCommentIsPutOnItsOwnLine` (spec example 1) | SHARED-COMMENT-012 (formatter bullet); FC-021 | `AssertRuleResult` | pass |
| `StructuralTransformRequirementTests.ExpressionBodyWhoseConversionWouldNotCompileIsLeftUnchanged` (spec example) | FMT-STRUCT-024; FC-020 | `AssertRuleResult` | pass |
| `StructuralTransformRequirementTests.AccessorConversionIsAppliedOnlyWhereEachCommentStaysNextToItsCode` (FMT-STRUCT-012 spec example) | FMT-STRUCT-024, both sides: `get` kept, `set` converted with its comment | `AssertRuleResult` | pass |
| `ExpressionBodyDirectiveGuardTests.{Method,Constructor,Operator,ConversionOperator,Finalizer,LocalFunction}WithDirectiveBeforeExpressionIsNotConverted` — existing, tag added | FMT-STRUCT-024; FC-025 | `AssertRuleResult` | pass |
| `BlankLineRequirementTests.BlankLineAfterClosingBraceGoesBetweenDirectiveAndComment` (spec example 2) | FMT-BLANK-016, FMT-BLANK-005 (clarified clause) | `AssertRuleResult` | pass |
| `BlankLineRequirementTests.BlankLineAfterClosingBraceGoesBehindLastDirectiveOfConditionalGroup` (`}⏎#if NEVER⏎a--;⏎#endif⏎// why⏎a++;`) | FMT-BLANK-016, FMT-BLANK-005; FC-020 (disabled text) | `AssertRuleResult` | pass |
| `BlankLineRequirementTests.DirectiveDirectlyBeforeStatementSuppressesStatementKindBlankLine` — existing (spec example 1), tag added | FMT-BLANK-016 | `AssertRuleResult` | pass |
| `BlankLineRequirementTests.BlankLineAfterClosingBraceGoesBehindDirective` — existing, tag added | FMT-BLANK-016 | `AssertRuleResult` | pass |
| `ReihitsuFormatterHelpersTests.DetectEndOfLineReturnsLineFeedWhenEveryLineBreakIsALoneCarriageReturn` | FMT-PIPE-007 (lone `\r` counts as `\n`) | `DetectEndOfLine` | pass |
| `ReihitsuFormatterHelpersTests.DetectEndOfLineCountsALoneCarriageReturnAsLineFeed` (two `␍`, one CRLF → `\n`) | FMT-PIPE-007; FC-003 | `DetectEndOfLine` | pass |
| `ReihitsuFormatterHelpersTests.DetectEndOfLineReturnsCarriageReturnLineFeedWhenALoneCarriageReturnTiesWithIt` (one `␍`, one CRLF → `\r\n`) | FMT-PIPE-007; FC-003 (other side of the count) | `DetectEndOfLine` | pass |
| `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenEveryLineBreakIsALoneCarriageReturn` | FMT-PIPE-007; FC-017, FC-033 (string built in the test, not normalized) | `ReihitsuFormatter.FormatSyntaxTree` | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (spec example in an all-`␍` document: three created breaks `\n`, the kept one behind `}` stays `␍`, all other breaks stay `␍`) | SHARED-FIX-008; FC-033 | `ApplyCodeFixAsync` (no line-ending normalization) | pass |
| `RH5204…AnalyzerTests.VerifyMemberBehindMisplacedBlockCommentStartingTheLineIsDetected` (spec example, both lines) | SHARED-COMMENT-011, RH5204-001 | analyzer `Verify` | pass |
| `RH5204…AnalyzerTests.VerifyMemberInScopeColumnBehindMisplacedBlockCommentIsDetected` and `…WithCrLf` (`/**/private`, token in the member column) | SHARED-COMMENT-011, RH5204-001 | analyzer `Verify` | pass |
| `RH5201…AnalyzerTests.VerifyLinkInAnchorColumnBehindMisplacedBlockCommentIsDetected` (`/* all */` col 0, `.` in anchor col 24) | SHARED-COMMENT-011, RH5201-002 | analyzer `Verify` | pass |
| `RH5301…AnalyzerTests.VerifyMemberInItsColumnBehindMisplacedBlockCommentIsDetected` (`/* c */` col 0, `Value = 1` col 24) | SHARED-COMMENT-011, RH5301-002 | analyzer `Verify` | pass |
| `RH5303…AnalyzerTests.VerifyElementInItsColumnBehindMisplacedBlockCommentIsDetected` (`/* c */` col 0, `1` col 25) | SHARED-COMMENT-011, RH5303-002 | analyzer `Verify` | pass |
| `RH5112…AnalyzerTests.VerifyNoDiagnosticForFirstLinkBehindBlockCommentStartingTheLine` and `…WithCrLf` (spec example, not reported) | SHARED-COMMENT-011 (exemption sentence), SHARED-CHAIN-004 | analyzer `Verify` | pass |
| `RH5203…AnalyzerTests.VerifyArgumentBehindEndOfMultiLineCommentIsDetected` and `…WithCrLf` (spec example 2) | SHARED-COMMENT-012 (rule bullet), RH5203-001 (amended clause); FC-007 → analyzer only | analyzer `Verify` | pass |

Files:
- `src/Reihitsu.Formatter.Test/Regression/FullPipeline/PipelineRequirementTests.cs` (new)
- `src/Reihitsu.Formatter.Test/Regression/Structural/StructuralTransformRequirementTests.cs`
- `src/Reihitsu.Formatter.Test/Regression/Structural/ExpressionBodyDirectiveGuardTests.cs` (tags only)
- `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineRequirementTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHelpersTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests.cs`

Expected values come from the spec examples and requirement texts, not from running the code first. Each test passed on its first run; none was adjusted to the code.

## Not covered, and why

Non-conforming today, deliberately not pinned (clarification 4/5 of `00-lead.md`; the existing untagged tests that pin them stay unchanged):

- FMT-PIPE-013: a blank line between leading comments (no wrap shape keeps it with a sensible layout), two comments on one line above the body, a same-line leading comment `/* c */ Foo();`, glued trailing comments after control-flow bracing, `=> /* c */ 42;` (listed in 02; thoenissen/Reihitsu#1381, #963).
- FMT-PIPE-014: indexer parameter lists, `+=`/`??=` wraps, attribute-target colon spacing, arrow spacing, comment before a closing `]` (thoenissen/Reihitsu#1383, #958).
- FMT-PIPE-014, switch-expression arm choice: no test. Observation: `return a switch⏎{⏎1⏎=> 2,⏎_ => 0⏎};` with the arm laid out at column 19 and `=>` at column 23 keeps the line break, but IndentationPhase moves `=> 2,` to column 8, left of the arm (`scripts/trace.sh`, stable after pass 2). Pinning that output would pin a layout no requirement states, so the arm choice has no test. Follow-up candidate for the lead: check it against FMT-INDENT and thoenissen/Reihitsu#1383.
- FMT-STRUCT-024, "documentation that applied to it" condition: its only reachable shape, the documented field split, is non-conforming (thoenissen/Reihitsu#1384). Other non-conforming shapes: thoenissen/Reihitsu#963, #1390, #1395. Directive and disabled-text refusals of FMT-STRUCT-003/-013/-018/-019/-023 and the pragma refusals of FMT-STRUCT-012 were not tagged, as 03 directs (they refuse under FMT-PIPE-002/-003).
- FMT-BLANK-016 on the FMT-BLANK-001/-002/-004 paths, and the FormatNode scope (FC-028): non-conforming (thoenissen/Reihitsu#1385); the two untagged `ReihitsuFormatterNodeBlankLineTests` stay unchanged.
- SHARED-COMMENT-011: RH5110, RH5203, RH5205, RH5206, RH5302, RH5305, RH5306, RH5308 misplaced-comment shapes are non-conforming (thoenissen/Reihitsu#1147). RH5204's fix result on the spec example is not asserted; the requirement covers the report, and RH5204-006 owns the fix.
- SHARED-COMMENT-012: the RH5203 fix ends no-progress on its example (FC-007), so the tests are analyzer-only. RH5203 with `2` in the alignment column behind the tail, RH5204 tail shapes, `int a; /* first⏎second */ int b;` and `{⏎/* p⏎q */ Foo();⏎}` are non-conforming (thoenissen/Reihitsu#1382).
- SHARED-FIX-008: RH5417 in an all-`␍` document was not added. The spec's example names RH5405. I did not prove that both fixes reach one line-ending call site, so this is a gap of breadth and not a static proof.
- FC-008 (Fix All), FC-018/FC-036 (renames), FC-022/FC-023 (literals, identifiers), FC-024, FC-026, FC-029: no fix or rewrite of this run reaches them; the run changes no code.
- FC-013 (analyzer ↔ formatter parity for P37): the shared side is deferred (`squad/decisions/2026-10-22-1380-p37-shared-side-deferred.md`), so no parity test.

## Commands

```shell
scripts/test.sh --project formatter --filter "TestCategory=FMT-PIPE-007|TestCategory=FMT-PIPE-013|TestCategory=FMT-PIPE-014|TestCategory=FMT-STRUCT-024|TestCategory=FMT-BLANK-016|TestCategory=FMT-BLANK-005|TestCategory=SHARED-COMMENT-012"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-COMMENT-011|TestCategory=SHARED-COMMENT-012|TestCategory=SHARED-FIX-008"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Results on the working tree:
- Formatter project: all 2682 tests passed, `SelfHostingTests` among them.
- Analyzer project, the seven touched test classes: 221 passed.
- Architecture project: 72 passed. The three named classes account for 39 of them, and `EveryRequirementOfAnEnforcedSpecIsCovered` is green.
- `scripts/format.sh` was run on every changed test file. The second run changed 0 files.
