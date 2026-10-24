Role: test implementer
Commit: f9c3b53
Verdict: #1329: reproduced; #1389: reproduced; #1241: reproduced; #1344: reproduced; #1242: tests written

Expected values come from the spec diff since `f3d514d` up to `f9c3b53` (handovers `04-spec-author.md` and `06-spec-author.md`), `00-lead.md`'s maintainer answers, and `03-skeptic.md`'s breaking inputs. No production file was touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Every test below was run on this head (production code of `f3d514d`). "red" means it fails today on the issue's own expected-versus-actual difference, which I checked for each failure message; "green" means it passes today and must stay green.

Helpers:
- **FmtBase** — `Reihitsu.Formatter.Test.Helpers.FormatterTestsBase.AssertRuleResult`: full pipeline, LF and CRLF, plus a second pass when output differs from input.
- **Verify** — `AnalyzerTestsBase<TAnalyzer, TCodeFix>.Verify(source, fixed, …)`: the code-fix verifier, which applies single fixes iteratively and Fix All, then re-analyzes.
- **NoFix** — `VerifyNoCodeFixIsOffered(source)`: exactly the marked diagnostics, and no code action registered for any of them (FC-009).
- **Apply** — `ApplyCodeFixAsync`: raw text, no line-ending normalization.
- **Parity** — `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source)`: analyzer-clean before and after, formatter unchanged, two passes, LF and CRLF.
- **Analyzer** — `AnalyzerTestsBase<TAnalyzer>.Verify(source)`.

## thoenissen/Reihitsu#1329

### Tests

File: `src/Reihitsu.Formatter.Test/Regression/FullPipeline/DocumentationCommentWithoutOwnEndTagTests.cs` (new).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SummaryWithoutEndTagKeepsTheFollowingFieldAsCode` (issue shape 1) | FMT-DOC-013, FMT-PIPE-003 | FmtBase | red |
| `RemarksWithoutEndTagAfterCompleteSummaryLeavesTheCommentAsWritten` (issue shape 2; FC-001) | FMT-DOC-013, FMT-PIPE-003 | FmtBase | red |
| `SpecificationExampleLeavesEveryCommentUnchanged` | FMT-DOC-013 | FmtBase | red |
| `SummaryClosedByEndTagOfAnotherNameIsLeftAsWritten` | FMT-DOC-013 | FmtBase | red |
| `SummaryClosedByEndTagDifferingOnlyInCaseIsLeftAsWritten` (`<Summary>S</summary>`) | FMT-DOC-013 | FmtBase | red |
| `SummaryClosedByItsOwnEndTagIsStillLaidOut` (other side, FC-003) | FMT-DOC-013, FMT-DOC-008 | FmtBase | green |
| `SummaryWhoseEndTagClosesANestedElementIsLeftAsWritten` | FMT-DOC-013 | FmtBase | red |
| `NestedElementClosedByEndTagOfAnotherNameLeavesTheCommentAsWritten` (`<summary>a <see>b</x></summary>`) | FMT-DOC-013 | FmtBase | red |
| `MissingSeparatorIsNotInsertedInCommentWithoutEndTag` (FC-001) | FMT-DOC-013, FMT-DOC-005 | FmtBase | red |
| `OneLineRemarksWithoutEndTagAfterThreeLineSummaryIsLeftAsWritten` (collapsed branch) | FMT-DOC-013 | FmtBase | red |
| `OneLineRemarksWithoutEndTagEndingInWhiteSpaceIsLeftAsWritten` | FMT-DOC-013 | FmtBase | red |
| `ElementClosedByEndTagOfTheEnclosingElementLeavesTheCommentAsWritten` (SHARED-DOC-017's RH8107 input) | FMT-DOC-013 | FmtBase | red |
| `SummaryWithoutEndTagBeforeClosingBraceKeepsTheBraceAsCode` (FC-027) | FMT-DOC-013 | FmtBase | red |
| `SummaryWithoutEndTagInsideMethodBodyKeepsTheStatementAsCode` (FC-027) | FMT-DOC-013 | FmtBase | red |
| `SummaryWithoutEndTagFollowedByCommentKeepsTheCommentAndTheField` (FC-019, FC-021) | FMT-DOC-013 | FmtBase | red |
| `SummaryWithoutEndTagFollowedByDirectiveKeepsTheDirectiveAndTheField` (FC-019, FC-020) | FMT-DOC-013 | FmtBase | red |
| `RelocatedCommentWithoutEndTagIsMovedAndLeftAsWritten` (spec example, answer 2A) | FMT-DOC-013, FMT-DOC-001, FMT-DOC-004 | FmtBase | red |
| `MisindentedCommentWithoutEndTagIsReindentedOnly` ("text after the indentation") | FMT-DOC-013 | FmtBase | green |

Analyzer side (answer 1A). These are the RH8305 and RH8307 paths of #1329: today both rules report these comments, and their formatter-delegating fixes destroy the declaration.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8305…AnalyzerTests.VerifySpecificationExampleSummariesInCommentsLeftAsWrittenDoNotProduceDiagnostics` | RH8305-002 | Analyzer | red (4 reported) |
| `RH8305…AnalyzerTests.VerifySummaryWithoutEndTagDoesNotProduceDiagnostics` | RH8305-002 | Analyzer | red |
| `RH8305…AnalyzerTests.VerifySummaryClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics` | RH8305-002 | Analyzer | red |
| `RH8305…AnalyzerTests.VerifyCompleteSummaryBesideElementWithoutEndTagDoesNotProduceDiagnostics` | RH8305-002 | Analyzer | red |
| `RH8305…FormatterTests.VerifyFormatterLeavesCommentsKeptAsWrittenUnchanged` | SHARED-DIAG-010, RH8305-002 | Parity | red |
| `RH8307…AnalyzerTests.VerifySpecificationExampleElementsInCommentsLeftAsWrittenDoNotProduceDiagnostics` | RH8307-002 | Analyzer | red (5 reported) |
| `RH8307…AnalyzerTests.VerifyElementWithoutEndTagDoesNotProduceDiagnostics` | RH8307-002 | Analyzer | red |
| `RH8307…AnalyzerTests.VerifyElementClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics` | RH8307-002 | Analyzer | red |
| `RH8307…AnalyzerTests.VerifyCompleteElementBesideElementWithoutEndTagDoesNotProduceDiagnostics` | RH8307-002 | Analyzer | red |
| `RH8307…FormatterTests.VerifyFormatterLeavesCommentsKeptAsWrittenUnchanged` | SHARED-DIAG-010, RH8307-002 | Parity | red |
| `RH8301…AnalyzerTests.VerifyCommentWithElementWithoutEndTagDoesNotProduceDiagnostics` | RH8301-002 | Analyzer | red |
| `RH8301…AnalyzerTests.VerifyCommentWithElementClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics` | RH8301-002 | Analyzer | red |

The RH8301 spec example and parity tests are listed under #1389, because they also hold the stranded shape.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `/// <summary>S⏎int A;` | LF, CRLF | formatter | reproduced | observed `/// <summary>⏎/// S⏎///     int A;`; expected unchanged |
| complete summary + `/// <remarks>Text⏎/// more⏎int A;` | LF, CRLF | formatter | reproduced | observed: summary expanded, remarks expanded, `///     int A;`; expected unchanged |
| collapsed branch `/// <remarks>Text` after a 3-line summary | LF, CRLF | formatter | reproduced | observed `/// <remarks>Text    int A;`; expected unchanged |
| comment before `}` | LF, CRLF | formatter | reproduced | observed `    /// }`; expected unchanged |
| inside a method body | LF, CRLF | formatter | reproduced | observed `///         int x = 1;` |
| followed by `// c` / `#if DEBUG` | LF, CRLF | formatter | reproduced | observed `///     // c` / `/// #if DEBUG` |
| relocated `int A; /// <summary>S⏎int B;` | LF, CRLF | formatter | reproduced | observed `///     int B;`; expected `/// <summary>S⏎    int B;` |
| issue shapes | LF | RH8305, RH8307 analyzers (code-fix path) | reproduced | reported today; expected not reported (RH8305-002, RH8307-002) |

## thoenissen/Reihitsu#1389

### Tests

Stranded documentation, file `src/Reihitsu.Formatter.Test/Regression/FullPipeline/StrandedDocumentationCommentTextTests.cs` (new):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `OneLineSummaryBehindLastFieldKeepsItsText` (issue instance) | FMT-DOC-003, FMT-PIPE-002 | FmtBase | red |
| `MultiLineRemarksWithoutSeparatorBehindLastFieldIsLeftAsWritten` (spec example) | FMT-DOC-003 | FmtBase | red |
| `ThreeLineSummaryBehindLastFieldKeepsEveryLine` (issue's multi-line instance) | FMT-DOC-003 | FmtBase | green — does not reproduce, as the skeptic recorded |
| `TwoLineRemarksBehindLastFieldKeepsItsSecondLine` | FMT-DOC-003, FMT-PIPE-002 | FmtBase | red |
| `TwoLineSummaryBehindLastFieldIsNotExpanded` | FMT-DOC-003 | FmtBase | red |
| `TwoLineRemarksBehindArgumentKeepsItsText` (`)`) | FMT-DOC-003 | FmtBase | red |
| `OneLineSummaryBeforeBinaryOperatorKeepsItsText` (`+`) | FMT-DOC-003 | FmtBase | red |
| `OneLineSummaryBeforeMemberAccessDotKeepsItsText` (`.`) | FMT-DOC-003 | FmtBase | red |
| `MissingSeparatorIsNotInsertedBehindLastField` (FC-001) | FMT-DOC-003, FMT-DOC-005 | FmtBase | red |
| `MisindentedContinuationLineBehindLastFieldIsLeftAsWritten` | FMT-DOC-003 | FmtBase | red (text lost today) |
| `OneLineSummaryBehindFieldBeforeAnotherMemberIsMovedAndLaidOut` (other side, FC-003) | FMT-DOC-001, FMT-DOC-004 | FmtBase | green |

Analyzer side, for stranded comments (answer 1A):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8301…AnalyzerTests.VerifySpecificationExampleCommentsLeftAsWrittenDoNotProduceDiagnostics` | RH8301-002 | Analyzer | red (2 reported) |
| `RH8301…AnalyzerTests.VerifyCommentKeptBehindCodeDoesNotProduceDiagnostics` | RH8301-002 | Analyzer | red (continuation line reported) |
| `RH8301…FormatterTests.VerifyFormatterLeavesCommentsKeptAsWrittenUnchanged` | SHARED-DIAG-010, RH8301-002 | Parity | red |
| `RH8305…AnalyzerTests.VerifySummaryKeptBehindCodeDoesNotProduceDiagnostics` | RH8305-002 | Analyzer | red |
| `RH8307…AnalyzerTests.VerifyElementKeptBehindCodeDoesNotProduceDiagnostics` | RH8307-002 | Analyzer | red |

Byte order mark, file `src/Reihitsu.Formatter.Test/Regression/Cleanup/ByteOrderMarkTests.cs` (new). All fixtures are strings that contain `U+FEFF` (FC-033).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ByteOrderMarkBeforeLineBreakIsKept` (issue instance) | FMT-CLEAN-009 | FmtBase | red |
| `ByteOrderMarkBeforeSeveralBlankLinesIsKept` | FMT-CLEAN-009 | FmtBase | red |
| `ByteOrderMarkBeforeWhiteSpaceAndLineBreakIsKept` | FMT-CLEAN-009 | FmtBase | red |
| `ByteOrderMarkBeforeLineBreakAndCommentIsKept` | FMT-CLEAN-009 | FmtBase | red |
| `ByteOrderMarkAloneBeforeLineBreakIsKept` (expected output: `U+FEFF` alone) | FMT-CLEAN-009 | FmtBase | red |
| `ByteOrderMarkBeforeIndentedFirstTokenIsKept` | FMT-CLEAN-009 | FmtBase | green |
| `ByteOrderMarkBeforeCommentOnTheSameLineIsKept` | FMT-CLEAN-009 | FmtBase | green |
| `RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark` (FMT-DOC-011 example) | FMT-DOC-011, FMT-CLEAN-009 | FmtBase | red (`U+FEFF` on every rebuilt line) |
| `CollapsedDocumentationElementDoesNotRepeatTheByteOrderMark` | FMT-DOC-011 | FmtBase | green |

Byte order mark in the Cleanup phase alone, file `src/Reihitsu.Formatter.Test/Unit/Cleanup/CleanupPhaseTests.cs` (FC-015, FC-016: the second removal step the skeptic found):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `KeepsByteOrderMarkBeforeLineBreakOfTheFirstLine` | FMT-CLEAN-009 | `CleanupPhase.Execute` | red |
| `RemovesWhiteSpaceAfterByteOrderMarkBeforeLineBreakOfTheFirstLine` | FMT-CLEAN-009, FMT-CLEAN-001 | `CleanupPhase.Execute` | red |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `U+FEFF⏎class C{int x;}` | LF, CRLF | formatter | reproduced | observed `class C⏎{⏎    int x;⏎}` without the BOM; expected `U+FEFF` first |
| `U+FEFF⏎class Foo⏎{⏎}` | LF | Cleanup phase alone | reproduced | observed: BOM removed |
| `int A; /// <summary>end</summary>` before `}` | LF, CRLF | formatter | reproduced | observed `int A; /// <summary>`; expected unchanged |
| `int A; /// <summary>⏎/// end⏎/// </summary>` before `}` | LF, CRLF | formatter | not reproduced | kept today; the test characterizes it |
| behind `)`, `+`, `.` | LF, CRLF | formatter | reproduced | every line after the first is dropped |
| stranded shapes | LF | RH8301, RH8305, RH8307 analyzers | reproduced | reported today; expected not reported |

## thoenissen/Reihitsu#1241

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8204….VerifyNoCodeFixForPlaceholderWithoutEndTag` (issue input, SHARED-DOC-017 example) | SHARED-DOC-017, RH8204-002, RH8204-003 | NoFix | red |
| `RH8204….VerifyNoCodeFixForPlaceholderWithoutEndTagWithCarriageReturnLineFeed` | SHARED-DOC-017, RH8204-003 | NoFix | red |
| `RH8204….VerifyNoCodeFixForPlaceholderClosedByEndTagOfTheEnclosingElement` | SHARED-DOC-017, RH8204-002, RH8204-003 | NoFix | red |
| `RH8204….VerifyNoCodeFixForPlaceholderClosedByEndTagDifferingOnlyInCase` (answer 5A) | SHARED-DOC-017, RH8204-003 | NoFix | red |
| `RH8204….VerifySpecificationExampleLocatesPlaceholdersWithoutEndTagOfTheirOwn` | RH8204-002, SHARED-DOC-017 | NoFix | red (locations already match; the fix is offered) |
| `RH8204….VerifyCodeFixReplacesPlaceholderBesideElementWithoutEndTagOfItsOwn` (other side: guard on the reported element only) | SHARED-DOC-017, RH8204-003 | Verify | green |
| `RH8204….VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged` (FC-008) | SHARED-DOC-017 | Verify | red |
| `RH8107….VerifyNoCodeFixForReturnsElementClosedByEndTagOfTheEnclosingElement` (spec example) | SHARED-DOC-017, RH8107-007 | NoFix | red |
| `RH8107….VerifyNoCodeFixForReturnsElementClosedByEndTagDifferingOnlyInCase` (spec example) | SHARED-DOC-017, RH8107-007 | NoFix | red |
| `RH8107….VerifySpecificationExampleWithholdsFixForReturnsElementWithoutEndTag` | RH8107-007 | NoFix | green |
| `RH8107….VerifyCodeFixRemovesReturnsElementBesideElementWithoutEndTagOfItsOwn` (other side) | SHARED-DOC-017, RH8107-006, SHARED-DOC-018 | Verify | green |
| `RH8107….VerifyFixAllLeavesReturnsElementWithoutEndTagOfItsOwnUnchanged` (FC-008) | SHARED-DOC-017 | Verify | red (`</remarks>` deleted) |
| `RH8202….VerifyNoCodeFixForValueElementClosedByEndTagOfTheEnclosingElement` | SHARED-DOC-017, RH8202-004 | NoFix | red |
| `RH8202….VerifyNoCodeFixForValueElementClosedByEndTagDifferingOnlyInCase` | SHARED-DOC-017, RH8202-004 | NoFix | red |
| `RH8309….VerifySpecificationExampleWithholdsFixForElementsWithoutEndTag` (all three comments) | SHARED-DOC-017, RH8309-004 | NoFix | red |
| `RH8309….VerifyNoCodeFixForReportedElementWithoutEndTagWithCarriageReturnLineFeed` | SHARED-DOC-017 | NoFix | red |
| `RH8309….VerifyNoCodeFixForReportedElementClosedByEndTagOfAnotherName` | SHARED-DOC-017 | NoFix | red |
| `RH8309….VerifyNoCodeFixForReportedElementClosedByEndTagDifferingOnlyInCase` | SHARED-DOC-017 | NoFix | red |
| `RH8309….VerifyCodeFixIsOfferedWhenOnlyANestedElementLacksAnEndTagOfItsOwn` (other side: top level only) | SHARED-DOC-017, RH8309-004 | Verify | green |
| `RH8309….VerifyFixAllLeavesCommentsWithWithheldFixUnchanged` (FC-008; also RH8309-005) | RH8309-005, SHARED-DOC-017 | Verify | red |

The RH8305 and RH8307 parts of #1241 are the "not reported" tests under #1329, since SHARED-DOC-017 no longer lists those fixes (answer 1A).

Existing tests that already cover the offered side are unchanged: own end tag (`VerifyDiagnosticAndCodeFor…` and the RH8309 reorder tests), and empty elements (`VerifyCodeFixRemovesEmptyReturnsElement`, `VerifyCodeFixRemovesEmptyValueElement`, and RH8204's `<placeholder/>` in `VerifySpecificationExampleReplacesPlaceholderWithItsContent`).

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| issue input (`<placeholder>Later text`) | LF, CRLF | RH8204 code fix | reproduced | fix offered; Fix All produced `    ///     public void Run()`; expected: reported, no fix |
| `<remarks><placeholder>Later text</remarks>` | LF | RH8204 code fix | reproduced | fix offered |
| `<remarks><returns>Nothing</remarks>` | LF | RH8107 code fix | reproduced | fix offered; Fix All produced `/// <remarks>` |
| `<Returns>Nothing</returns>`, `<Value>…</value>`, `<Placeholder>…</placeholder>` | LF | RH8107, RH8202, RH8204 code fixes | reproduced against answer 5A | fix offered; expected withheld |
| SHARED-DOC-017 RH8309 example, `</returns>` and `</Summary>` shapes | LF, CRLF | RH8309 code fix | reproduced | fix offered; Fix All moved `</returns>` with the summary |
| the same shapes | LF, CRLF | formatter | reproduced | see #1329 |

## thoenissen/Reihitsu#1344

### Tests

RH8304, file `src/Reihitsu.Analyzer.Test/Documentation/RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests.cs`. The RH8304-002 tests follow `06-spec-author.md`; the stranded shape is a negative case only, as the lead noted.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifySpecificationExampleFixDoesNotRepeatCodeInFrontOfTheComment` (two-class example) | RH8304-004 | Verify | red |
| `VerifyFixDoesNotRepeatFieldInFrontOfTheComment` (issue instance, CS0102) | RH8304-004 | Verify | red |
| `VerifyFixDoesNotRepeatOpeningBraceInFrontOfTheComment` (issue instance, does not parse) | RH8304-004 | Verify | red |
| `VerifyFixDoesNotRepeatEnumMemberInFrontOfTheComment` | RH8304-004 | Verify | red |
| `VerifyFixDoesNotRepeatDelimitedCommentInFrontOfTheComment` (FC-021) | RH8304-004 | Verify | red |
| `VerifyFixDoesNotRepeatTheByteOrderMark` | RH8304-004 | Apply | red |
| `VerifyFixKeepsTabIndentationInFrontOfCode` | RH8304-004 | Apply | red |
| `VerifyFixDoesNotRepeatCodeInFrontOfTheCommentWithCarriageReturnLineFeed` | RH8304-004 | Apply | red |
| `VerifyFixAllDoesNotRepeatCodeInFrontOfTheComment` (FC-008, two elements on one line) | RH8304-004 | Verify | red |
| `VerifySpecificationExampleElementsInCommentsLeftAsWrittenDoNotProduceDiagnostics` | RH8304-002 | Analyzer | red (3 reported) |
| `VerifyElementWithoutEndTagOnTheLineOfThePreviousElementDoesNotProduceDiagnostics` | RH8304-002 | Analyzer | red |
| `VerifyElementClosedByEndTagOfAnotherNameOnTheLineOfThePreviousElementDoesNotProduceDiagnostics` | RH8304-002 | Analyzer | red |
| `VerifyCompleteElementsBesideElementWithoutEndTagDoNotProduceDiagnostics` | RH8304-002 | Analyzer | red |
| `VerifyElementsInCommentKeptBehindCodeDoNotProduceDiagnostics` | RH8304-002 | Analyzer | red |

Shared helper (answer 7A), file `src/Reihitsu.Core.Test/DocumentationCommentUtilitiesTests.cs`. These are untagged: they test the helper's contract, which the maintainer decided, not a requirement directly. The tagged consumer tests are the RH8304 tests above and `RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark` under #1389.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `GetContinuationPrefixExcludesCodeInFrontOfTheExterior` | — | direct call | red |
| `GetContinuationPrefixExcludesOpeningBraceInFrontOfTheExterior` | — | direct call | red |
| `GetContinuationPrefixExcludesDelimitedCommentInFrontOfTheExterior` | — | direct call | red |
| `GetContinuationPrefixExcludesByteOrderMarkInFrontOfTheExterior` | — | direct call | red |
| `GetContinuationPrefixKeepsTabIndentationInFrontOfCode` | — | direct call | red |
| `GetContinuationPrefixKeepsMixedIndentationInFrontOfTheExterior` | — | direct call | green |

RH8309-005, file `src/Reihitsu.Analyzer.Test/Documentation/RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifySpecificationExampleWithholdsFixForTextAndNodesBesideTheElements` (all six comments) | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForTextBetweenTheElements` (issue instance) | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForTextBetweenTheElementsWithCarriageReturnLineFeed` | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForXmlCommentBetweenTheElements` (issue instance) | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForTextBeforeTheFirstElement` | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForTextAfterTheLastElement` | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForCDataSectionBetweenTheElements` | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForProcessingInstructionBetweenTheElements` | RH8309-005 | NoFix | red |
| `VerifyNoCodeFixForEntityReferenceBetweenTheElements` (skeptic's candidate `/// &amp;`) | RH8309-005 | NoFix | red |
| `VerifyCodeFixIsOfferedWhenOnlyWhiteSpaceStandsBetweenTheElements` (other side: a bare `///` line) | RH8309-005, RH8309-004 | Verify | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `int A; /// <summary>S</summary><remarks>R</remarks>⏎int B;` | LF, CRLF | RH8304 code fix | reproduced | observed new line `    int A; /// <remarks>R</remarks>`; expected `    /// <remarks>R</remarks>` |
| `{ /// …` | LF | RH8304 code fix | reproduced | observed `{ /// <remarks>R</remarks>` |
| enum, `/* c */`, tab, BOM prefixes | LF | RH8304 code fix | reproduced | each prefix is repeated |
| stranded `int D; /// …` before `}` | LF | RH8304 analyzer | reproduced against `06-spec-author.md` | reported today; expected not reported |
| text or `<!-- c -->` between the elements | LF, CRLF | RH8309 code fix | reproduced | fix offered; elements moved around the text |
| BOM prefix | LF, CRLF | formatter (second consumer) | reproduced | see #1389 |

## thoenissen/Reihitsu#1242

### Tests

No assertion changed. Tags only:
- `[Requirement("SHARED-DOC-018")]` was added next to every existing `[Requirement("RH8107-006")]`: 8 tests in `RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests.cs`.
- The same tag was added next to every existing `[Requirement("RH8202-003")]`: 13 tests in `RH8202ValueTagMustNotBeUsedAnalyzerTests.cs`.
- Each of these tests verifies a removal shape that SHARED-DOC-018 now states.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH8107….VerifySpecificationExampleRemovesReturnsElementWithTheLinesItOwns` (new; SHARED-DOC-018's example verbatim, which no test held before) | SHARED-DOC-018, RH8107-006 | Verify | green |
| the 21 re-tagged existing tests | SHARED-DOC-018 plus their existing tags | Verify | green |

### Scenarios (bug fix)

None. The issue is a spec-owner move with no behavior to reproduce.

## Whole run

### Not covered, and why

- **RH8304 on a stranded comment, fix path.** Per the maintainer's answer and `06-spec-author.md`, it is not reported. Only the negative test exists, and no fix test.
- **RH8308.** It is unchanged (maintainer answer 2), so it has no test.
- **RH8301 on a well-formed comment behind code that documents the next member.** I wrote a test for this boundary and then removed it: RH8301-001's coverage sentence excludes a `///` comment that starts behind code, so its outcome is unspecified. The "reported" side of RH8301-002 is covered by the existing own-line tests, for example `VerifyIssueIsDetectedAndFixed`.
- **One test per rule for the shared guard (RH8107, RH8202).** Both fixes reach the same guard: `rg -n "CanRemoveElement" src --glob '!*Test*'` shows only `RH8107…CodeFixProvider.cs:65` and `RH8202…CodeFixProvider.cs:65` calling `DocumentationCommentCodeFixUtilities.CanRemoveElement`. So the nested-positive and Fix All cases are written once, for RH8107. Each rule still has its own withheld tests, because each has its own requirement.
- **The two `GetContinuationPrefix` consumers.** `rg -n "GetContinuationPrefix\(" src --glob '!*Test*'` shows `DocCommentElementNormalizer` (3 calls) and `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync` (1 call). Both have consumer tests: the FMT-DOC-011 BOM test, and the RH8304 tests.
- **BlankLines phase unit test.** It is not written, because `00-lead.md` puts that phase's files under the parallel run's ownership. The full-pipeline `ByteOrderMarkTests` cover `CollapseLeadingBlankLines`.
- **Failure classes that cannot reach this change:**
  - FC-006, FC-022, FC-023, FC-024, FC-025, FC-026, FC-029: no syntax rewrite of code, literals, or identifiers is added.
  - FC-018, FC-036: no rename.
  - FC-028: the RH8305 and RH8307 fixes no longer reach the comments in question, because they are not reported there.
  - FC-012: the change only removes edits from the doc-comment and BOM paths.
  - FC-007, FC-010, FC-011: every changed path is covered by a second-pass or iterative helper (FmtBase, Verify, Parity).
- **FC-030.** The RH8309 positive tests use block-bodied methods and fields, so the member-wide formatting of the RH8309 fix does not show in them. The `int A() => 0;` shapes appear only in withheld tests.
- **Note for the spec checker.** FMT-DOC-011's new prose example ends in `class C`, `{`, `}`. Through the full pipeline, an empty class becomes `class C;` (FMT-STRUCT empty-type rewrite, observed). `RebuiltDocumentationLinesDoNotRepeatTheByteOrderMark` therefore gives the class a field (`int A;`). The documentation-comment part of the example is asserted unchanged.

### Results on this head

- Formatter project, full run: 2724 tests, 33 failed. All 33 are new tests listed red above. No existing test failed.
- Analyzer project, `FullyQualifiedName~Documentation|FullyQualifiedName~RH8301`: 1094 tests, 53 failed. Then 3 CRLF tests were added, all red. Every failure is a new test listed red above.
- Core, `DocumentationCommentUtilitiesTests`: 5 red (new), the rest green.
- Architecture, `RequirementCoverage|Spec`: 51 passed. Every tag names an active ID, and `RH8309-005`, `FMT-DOC-013`, and `SHARED-DOC-018` are now covered.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~DocumentationCommentWithoutOwnEndTagTests|FullyQualifiedName~StrandedDocumentationCommentTextTests|FullyQualifiedName~ByteOrderMarkTests|FullyQualifiedName~CleanupPhaseTests"
scripts/test.sh --project core --filter "FullyQualifiedName~DocumentationCommentUtilitiesTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8107|FullyQualifiedName~RH8202|FullyQualifiedName~RH8204|FullyQualifiedName~RH8301|FullyQualifiedName~RH8304|FullyQualifiedName~RH8305|FullyQualifiedName~RH8307|FullyQualifiedName~RH8309"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverage|FullyQualifiedName~Spec"
```

## Repair

Round 1. The finding came from `07-implementer.md` (commit `2d810b9`). I checked it on `2d810b9` and it holds.

**The finding.** Three Fix All tests failed on `Diagnostics of fixed state`, with "expected 0, actual 1" (or 2):
- `RH8107….VerifyFixAllLeavesReturnsElementWithoutEndTagOfItsOwnUnchanged`
- `RH8204….VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged`
- `RH8309….VerifyFixAllLeavesCommentsWithWithheldFixUnchanged`

In each, the actual diagnostics were the withheld ones at exactly the spans the fixed source marks, and the fixed text already matched. The cause is the verifier's default, `MarkupMode.IgnoreFixable`: it drops the `{|RH####:…|}` markup in the fixed states, so the remaining diagnostic was never expected.

**The repair.** Each of the three tests now sets `test.FixedState.MarkupHandling` and `test.BatchFixedState.MarkupHandling` to `MarkupMode.Allow`, as `RH4107ProtectedFieldCasingAnalyzerTests` does. The RH8107 and RH8204 files gain `using Microsoft.CodeAnalysis.Testing;`. No source, fixed source, markup, or tag changed. The assertion still requires what SHARED-DOC-017, SHARED-FIX-002, and RH8309-005 specify: the fixable diagnostics are fixed, and the withheld diagnostics stay, unchanged, at their spans.

**Result.** `scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyFixAllLeaves"`: 3 passed, 0 failed.

## Repair round 1

Finding F2 of `09-spec-checker.md`. The spec change is in `10-spec-author.md` (commit `dc0a37f`): RH8309-005 now reads text the way the compiler does, so a character or entity reference counts as the character it stands for. I ran everything on head `dc0a37f`, with the implementer's code from `2d810b9` in place.

All tests are in `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Result |
|---|---|---|---|
| `VerifyCodeFixIsOfferedWhenACharacterReferenceForASpaceStandsBetweenTheElements` (new; the spec's `&#32;`, other side of the boundary, FC-003) | RH8309-005, RH8309-004 | Verify | green |
| `VerifyCodeFixIsOfferedWhenAHexadecimalCharacterReferenceForATabStandsBetweenTheElements` (new; `&#x9;`, the hexadecimal form of a white-space reference) | RH8309-005, RH8309-004 | Verify | green |
| `VerifyNoCodeFixForCharacterReferenceForALetterBetweenTheElements` (new; `&#65;`, a character reference to a character that is not white space) | RH8309-005 | NoFix | green |
| `VerifySpecificationExampleWithholdsFixForTextAndNodesBesideTheElements` (changed) | RH8309-005 | NoFix | green |

- **Fix-offered tests.** In both, the reference stays in its slot: the sorted elements fill the element positions around it (RH8309-004), and the reference's text is kept as written.
- **Changed spec-example test.** It gains the spec example's seventh comment (`/// &amp;` before `int H;`), so it again holds the spec example in full. Its summary now names the entity reference. No other assertion changed.
- **Unchanged test.** `VerifyNoCodeFixForEntityReferenceBetweenTheElements` keeps its `RH8309-005` tag and its expectation, as `10-spec-author.md` says.

Command: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests"`. Result: 44 passed, 0 failed.
