Role: test implementer
Commit: be6bd4d
Verdict: #1140: tests written; #1687: tests written; #1748: tests written

The prompt carried no intended fix, no suspected cause, and no hoped-for verdict. The maintainer answers M1–M9 and decisions D1–D16 in `00-lead.md` are taken as binding.

The expected values come from the spec examples and requirements of the spec diff against `origin/main` and from the change doc (`sdd/changes/2026-11/fix-layout-scope.md`). Where a construct is laid out "as the formatter lays it out" with formatter-clean code around it (the nested Fix All tests), the expected text is `main`'s `scripts/format.sh` output for that file. That is the unchanged formatter, not a patched copy.

**How contradicting tests were found.** I used a throw-away scratch worktree outside the branch (`git worktree`, removed afterwards; nothing was committed from it). It held minimal edits:
- `FormatNodeInDocumentAsync` keeps its target's leading trivia;
- RH5206, RH5303–RH5308, and RH5111 target the construct.

I used it for two things only:
- to list the existing tests whose expected values contradict the new spec;
- to cross-check that my spec-derived expectations agree with a minimal implementation.

No expected value was taken from it (FC-035). Every rewritten expectation is "the input's text outside the formatted node, the node formatted as before" or a spec example.

## thoenissen/Reihitsu#1140

### Tests

Unless the column says otherwise:
- each test runs under LF and CRLF (`[DataRow]` through `VerifyCodeFixWithEndOfLine`, or the formatter helpers that loop over both endings);
- formatter tests also assert a stable second pass.

The `Expected now` column gives the result on this head.

#### Formatter — FMT-PIPE-010/017/018/024 (D5), `src/Reihitsu.Formatter.Test/Unit/`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterNodeBlankLineTests`: 65 rewritten tests (list below) | FMT-PIPE-018 (FMT-PIPE-010 kept on `KeepsTabInFrontOfDelimitedDocumentationBehindPreviousMember`, `KeepsSingleLineDocumentationBehindPreviousMember`, `KeepsSingleLineDocumentationBehindPreviousTopLevelType`) | `AssertFormatsTarget` / `AssertFormatsTargetWithForeignLineBreaks` | fail (blank lines / moved doc comments inserted) |
| `SpecExampleKeepsCommentAboveStatementTargetBehindAssignment` (new) | FMT-PIPE-018 | `AssertFormatsTarget` | fail |
| `SpecExampleKeepsOneLineDocumentationCommentAndBlankLineBehindOpeningBraceOfPropertyTarget` (new) | FMT-PIPE-018 | `AssertFormatsTarget` | fail |
| `SpecExampleKeepsDelimitedDocumentationBehindPreviousMemberOfMultiLinePropertyTarget` (new) | FMT-PIPE-010, FMT-PIPE-018 | `AssertFormatsTarget` | fail |
| `KeepsWhitespaceOnBlankLineAboveTarget` (new) | FMT-PIPE-018 | `FormatTarget`, both endings | fail |
| `KeepsForeignLineBreakInLeadingTriviaOfTarget` (new) | FMT-PIPE-018 | `FormatTarget`, both endings | fail |
| `ReihitsuFormatterDetachedRootTests.StatementTargetBehindLocalDeclarationGetsNoBlankLine` (was `…GetsTheBlankLineOfItsListPosition`) | FMT-PIPE-018, FMT-PIPE-024 | `AssertFormatsTarget` | fail |
| `ReihitsuFormatterDetachedRootTests.InitializerTargetBehindMultiLineCommentKeepsTheCommentInItsColumn` (was `…TakesTheIndentationPhaseDepth`) | FMT-PIPE-018 | `AssertFormatsTarget` | fail |
| `ReihitsuFormatterNodeColumnTests.StatementTargetIndentedWithTabsCountsEachOfItsTabsAsOneColumn` (was `…AsFourColumns`, spec example) | FMT-PIPE-017, FMT-PIPE-018 | `AssertFormatsTarget` | fail |
| `ReihitsuFormatterNodeColumnTests.StatementTargetIndentedWithTabsHoldingAForeignLineBreakCountsEachOfItsTabsAsOneColumn` | FMT-PIPE-017, FMT-PIPE-018 | `AssertFormatsTargetWithForeignLineBreaks` | fail |

The 65 rewritten tests of `ReihitsuFormatterNodeBlankLineTests`:
- They are renamed from `Inserts…`, `Moves…`, `Separates…`, `Removes…`, `Collapses…`, and `Indents…` to `Keeps…` or `DoesNotInsert…`.
- Each now expects the target's leading trivia exactly as in the input.
- The obsolete tags FMT-PIPE-024, FMT-BLANK-016, FMT-BLANK-017, FMT-DOC-001, FMT-PIPE-017, and FMT-TAB-001 are removed from them.
- `git diff be6bd4d -- src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeBlankLineTests.cs` lists them.
- Examples:
  - `KeepsTwoBlankLinesAboveRewrittenProperty`
  - `KeepsBlankLineBetweenOpeningBraceAndRewrittenFirstMember`
  - `DoesNotInsertBlankLineAboveStatementAfterBreak`
  - `KeepsSingleLineDocumentationBehindCaseLabel`
  - `KeepsColumnOfDelimitedDocumentationOnOwnLineBelowOpeningBrace`
- The class summary now describes the write-back.
- `DocumentLevelFormattingSeparatesDelimitedDocumentationWithoutTrailingWhitespace` is document-level and unchanged.

#### Code fixes — leading trivia (SHARED-FIX-006 *Leading trivia*, D5), `src/Reihitsu.Analyzer.Test/`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5110…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheDeclaration` (spec example) | RH5110-003, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5111…Tests.VerifyFixKeepsTheOneLineDocumentationCommentAboveTheProperty` (spec example) | RH5111-004, SHARED-FIX-006 | same | fail |
| `RH5111…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheAssignment` | SHARED-FIX-006 | same | fail |
| `RH5113…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheDeclaration` | SHARED-FIX-006 | same | fail |
| `RH5401…Tests.VerifyFixKeepsTheOneLineDocumentationCommentAboveTheProperty` (spec example) | RH5401-003, SHARED-FIX-006 | same | fail |
| `RH5408…Tests.VerifyDocumentedPropertyWithoutTrailingInitializerCommentIsDetectedAndFixed` (rewritten; spec example) | RH5408-005, SHARED-FIX-006 | `Verify` | fail |
| `RH5408…Tests.VerifyDocumentedPropertyWithTrailingInitializerCommentIsDetectedAndFixed` (rewritten) | SHARED-FIX-006 | `Verify` | fail |
| `RH5412…Tests.VerifyNoBlankLineIsInsertedAboveCommentOfFixedClassInFileScopedNamespace` (was `…IsInserted`) | SHARED-FIX-006 | `Verify` | fail |
| `RH5417…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheMember` | SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH2001…Tests.VerifyFixKeepsTheOneLineDocumentationCommentAndInsertsNoBlankLine` (change doc change 2) | SHARED-FIX-006 | same | fail |
| `RH3202…Tests.VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs` (rewritten: block from column 2) | SHARED-FIX-006, SHARED-FIX-007, SHARED-BODY-003 | `Verify` | fail |
| `RH3202…Tests.VerifyFixMovesRawStringOfMethodIndentedWithTabsUnderTheOpeningQuotes` (rewritten: column 2, quotes at 13) | FMT-PIPE-017, FMT-RAW-001 | `Verify` | fail |
| `RH5112…Tests.VerifyFixOfChainThatStartsItsLineInsertsNoBlankLineAboveTheCommentThatLeadsIt` | SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass (holds on `main`; pins it) |

#### Code fixes — the code around the construct (M2, M4, M7, D10)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5111…Tests.VerifyFixOfDeclaratorInAssignedLambdaLeavesTheAssignmentAsWritten` (spec example) | RH5111-004, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5111…Tests.VerifyFixOfParameterDefaultInAssignedLambdaLeavesTheAssignmentAsWritten` | RH5111-004, SHARED-FIX-006 | same | fail |
| `RH5111…Tests.VerifyFixOfDeclaratorInAssignedAnonymousMethodLeavesTheAssignmentAsWritten` | RH5111-004, SHARED-FIX-006 | same | fail |
| `RH5111…Tests.VerifyFixOfDeclaratorInLambdaOfObjectInitializerLeavesTheMemberAssignmentAsWritten` | RH5111-004, SHARED-FIX-006 | same | fail |
| `RH5111…Tests.VerifyFixOfAssignmentInAssignedLambdaJoinsOnlyTheInnerAssignment` (other side) | RH5111-004, SHARED-FIX-006 | same | pass |
| `RH5111…Tests.VerifyFixOfDeclaratorInLambdaOfFieldInitializerLeavesTheLambdaAsWritten` (other side) | RH5111-004, SHARED-FIX-006 | same | pass |
| `RH5113…Tests.VerifyFixMovesOnlyTheSemicolonAndKeepsTheDocumentationComment` (spec example) | RH5113-003, SHARED-FIX-006 | same | fail |
| `RH5206…Tests.VerifyFixInsertsNoBlankLineInFrontOfTheStatementThatHoldsTheSwitchExpression` (spec example) | RH5206-003, SHARED-FIX-006 | same | fail |
| `RH5206…Tests.VerifyFixOfSwitchExpressionInIfConditionLeavesTheBodyAsWritten` (spec example) | RH5206-003, SHARED-FIX-006 | same | fail |
| `RH5206…Tests.VerifyFixLeavesTheCodeInFrontOfTheSwitchExpressionAsWritten` | RH5206-003, SHARED-FIX-006 | same | fail |
| `RH5112…Tests.VerifyFixOfChainInIfConditionLeavesTheRestOfTheConditionAndTheBodyAsWritten` (spec example) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | same | fail |
| `RH5112…Tests.VerifyFixOfChainBehindTabsCountsEachTabAsOneColumn` | SHARED-CHAIN-009, SHARED-FIX-007 | same | fail |
| `RH5112…Tests.VerifyFixJoinsFirstCallBehindMisspacedDeclaration` (rewritten = spec example) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | `Verify` | fail |
| `RH5112…Tests.VerifyFixJoinsFirstCallOfChainInConditionalExpression` (rewritten: conditional stays, chain from `a` at 23) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | `Verify` | fail |
| `RH5201…Tests.VerifyFixAlignsChainBehindMisspacedDeclaration` (rewritten) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `Verify` | fail |
| `RH5201…Tests.VerifyFixAlignsChainBehindMisspacedArgumentList` (rewritten: `Use( a.B()` stays) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `Verify` | fail |
| `RH5303…Tests.VerifyFixLeavesTheCodeInFrontOfTheObjectCreationAsWritten` (spec example) | RH5303-005, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5303…Tests.VerifyFixIsOfferedWhenTheStatementAroundTheObjectCreationHoldsACommentTheFormatterRefuses` (FMT-UNSUP-004 in a lambda elsewhere in the statement) | RH5303-005, SHARED-FIX-010 | same | fail (withheld today) |
| `RH5303…Tests.VerifyFixIsOfferedWhenTheDeclarationAroundTheObjectCreationHoldsACommentTheFormatterRefuses` | RH5303-005, SHARED-FIX-010 | same | pass: `main` already targets the equals-value clause there, which holds no comment |
| `RH5304…Tests.VerifyFixLeavesTheOtherAssignmentsOfTheInitializerAsWritten` (`X=1` stays) | RH5304-004, SHARED-FIX-006 | same | fail |
| `RH5305…Tests.VerifyFixLeavesTheCodeInFrontOfTheCollectionExpressionAsWritten` | RH5305-004, SHARED-FIX-006 | same | fail |
| `RH5306…Tests.VerifyFixLeavesTheCodeInFrontOfTheListPatternAsWritten` | RH5306-004, SHARED-FIX-006 | same | fail |
| `RH5306…Tests.VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken` and `…WithCarriageReturnLineFeed` (rewritten: `[` keeps column 16) | RH5306-001, RH5306-004, SHARED-FIX-006 | `Verify` | fail |
| `RH5307…Tests.VerifyFixLeavesTheCodeAroundTheElementAccessAsWritten` | RH5307-004, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5308…Tests.VerifyFixLeavesTheCodeInFrontOfTheConditionalAsWritten` (spec example) | RH5308-004, SHARED-FIX-006 | same | fail |
| `RH5308…Tests.SpecExampleConditionalWithACommentElsewhereInTheStatementDoesNotWithholdTheFix` (spec example, M8) | RH5308-004, RH5308-005 | same | fail (withheld today) |
| `RH5308…Tests.FixIsOfferedWhenTheStatementHoldsAnotherConditionalWithACommentBehindItsOperator` (was `NoFixIsOffered…`, M8) | RH5308-004, RH5308-005 | same | fail |
| `RH5308…Tests.FixIsOfferedWhenTheStatementHoldsACommentTheFormatterRefusesOutsideTheConditional` (was `NoFixIsOfferedWhenTheStatementHoldsACommentTheFormatterRefuses`, M8) | RH5308-004, RH5308-005, SHARED-FIX-010 | same | fail |
| `RH5308…Tests.NoFixIsOfferedWhenTheConditionalHoldsAConditionalWithACommentBehindItsOperatorInABranch` (other side of RH5308-005) | RH5308-005 | `VerifyNoCodeFixIsOffered`, LF | pass |
| `RH5308…Tests.SpecExampleFixIsOfferedForAConditionalInAnAssemblyAttribute` (was `SpecExampleNoFixIsOffered…`, M9) | RH5308-004 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5308…Tests.FixIsOfferedForAConditionalInAModuleAttribute` (was `NoFixIsOffered…`, M9) | RH5308-004 | same, LF | fail |
| `RH5308…Tests.FixIsOfferedForAConditionalInAnAttributeOnAType` (retagged from the retired RH5308-006) | RH5308-004 | unchanged | pass |
| `RH5308…Tests.VerifySiblingConditionalsInOneStatementAreDetectedAndFixed` (rewritten: `: "2", b` stays, second conditional from `b` at 34) | RH5308-002, RH5308-003, RH5308-004, SHARED-FIX-006 | `Verify` | fail |
| `RH5303…Tests.VerifyFixAllCorrectsAReportedObjectCreationInsideAReportedObjectCreationInOneApplication` | RH5303-005, SHARED-FIX-002 (FC-008) | `Verify`, `NumberOfFixAllIterations = 1` | pass (pin) |
| `RH5305…Tests.VerifyFixAllCorrectsAReportedCollectionExpressionInsideAReportedCollectionExpressionInOneApplication` | RH5305-004, SHARED-FIX-002 (FC-008) | same | pass (pin) |
| `RH5308…Tests.VerifyFixAllCorrectsAReportedConditionalInsideABranchOfAReportedConditionalInOneApplication` | RH5308-004, SHARED-FIX-002 (FC-008) | same | pass (pin) |

Existing tests that already pin outputs the implementer must keep, unchanged by me:
- `RH5308…Tests.VerifyNestedConditionalStairIsDetectedAndFixed`, with the inner `?` in column 28 (see *For the lead*);
- `RH5306…Tests.FixAllCorrectsEveryDiagnostic` and `RH5308…Tests.FixAllCorrectsEveryDiagnostic`, one Fix All iteration.

All three pass today. In the scratch check, they failed under a naive narrowing.

### Scenarios (bug fix)

_Not applicable_: behavior-change lane. The skeptics measured the issue's instances (`02-skeptic.md`, `04-skeptic.md`). Each one is a failing test above, under LF and CRLF.

### Not covered, and why

- **RH3004, RH5205, RH5301, RH5302, RH5413–RH5416** (change doc change 2: an expression target, or a type of the same base). There is no test per fix. Each passes its target to the one shared symbol `ReihitsuFormatter.FormatNodeInDocumentAsync`:
  - `rg -ln "FormatNodeInDocumentAsync" src/Reihitsu.Analyzer.CodeFixes` lists `RH3004UseLambdaSyntaxCodeFixProvider.cs`, `RH5205…`, `RH5301…`, and `RH5302…`;
  - RH5412–RH5416 share `Base/EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs:56`.

  The write-back of the leading trivia sits in that symbol and is pinned by the 74 formatter tests above. RH5412 pins the shared base. RH3004 builds its target with `WithTriviaFrom` (04-skeptic), so its trivia reaches the same write-back.
- **FMT-PIPE-017/018 through the context entry point**: unchanged by this run. The existing `ReihitsuFormatterNodeColumnTests` context tests still pass.
- **FC-028** (`FormatNode` still decides its node's leading trivia): no new test. `FormatNodeOnStatementBehindLocalDeclarationAddsNoBlankLine` passes and pins the `FormatNode` side; its summary no longer cites the list position. I did not assert what `FormatNode` does to a leading one-line `<summary>`, because no spec example states it.
- **RH5306 nested Fix All**: RH5306-001 does not report a list pattern whose inner pattern spans lines, so an outer and an inner list pattern are never reported together. The test was dropped.
- **RH5308 nested directly as a branch**: RH5308-003 reports one diagnostic per outermost conditional. The nested Fix All test therefore uses a conditional in an argument inside a branch.
- **RH5304 outer-plus-inner Fix All**: not added. RH5304 reports the member assignment, and a nested `Items = { … }` inside another reported assignment needs a type shape that no spec example gives.
- **RH5404 `class A { class B { } }`** (03-spec-author "effect to pin"): derived under M5, which M6 reverted. The inside stays as on `main`, so I wrote no test.
- **Code inside the construct (M3/M5)**: no test, as M6 and 05-spec-author require.
- **D6 header, other branches, and code in front of the clause** (RH5110, RH5403, RH5404): stay as on `main`, and existing tests cover them. `RH5403…NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses` and the RH5110 counterpart stay, because those fixes still format the statement or declaration.
- **FC-019/FC-020 (comment or directive in the gap)**:
  - RH5403 is covered with a directive above the target (`VerifyFixKeepsADirectiveDirectlyAboveTheStatement`) and with comments above it.
  - RH5308 is covered with comments elsewhere in the statement.
  - RH5303 is covered with a refused comment in the declaration or statement.
  - For #1687, see that section.
- **FC-022, FC-023, FC-024, FC-036, FC-018**: no literal, identifier, partial, or rename logic changes.
- **FC-038 to FC-043**: no shape is declared unsupported. The SHARED-FIX-010 node choice (FC-043) is covered by the RH5303 and RH5308 "fix is offered" tests above.

## thoenissen/Reihitsu#1687

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5517…Tests.VerifyFixGivesAFieldThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5513…Tests.VerifyFixGivesAMethodThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH3203…Tests.VerifyFixGivesAConstructorThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH3203…Tests.VerifyFixGivesAConstructorThatSharesItsLineWithAFieldInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH3205…Tests.VerifyFixGivesAnOperatorThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH3207…Tests.VerifyFixGivesAFinalizerThatSharesItsLineWithAFieldBehindItALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH5401…Tests.VerifyFixGivesThePropertyAndTheMemberBehindItLinesOfTheirOwn` | SHARED-FIX-007 | same | fail |
| `RH5408…Tests.VerifyFixGivesAPropertyThatSharesItsLineWithAMemberInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH5113…Tests.VerifyFixGivesADeclarationThatSharesItsLineWithAMemberInFrontALineOfItsOwn` | SHARED-FIX-007 | same | fail |
| `RH5404…Tests.VerifyFixGivesANestedTypeThatSharesItsLineWithOtherMembersALineOfItsOwn` (both sides) | SHARED-FIX-007 | same | fail |

### Scenarios (bug fix)

_Not applicable_ (behavior-change lane). Each test above is one of the skeptic's measured failing cells, under LF and CRLF.

### Not covered, and why

- **A comment in the gap to the sibling** (`int _a; /* c */ int _b;`, FC-019/FC-021): not pinned. SHARED-FIX-007 *Shared line* does not say where the line break goes relative to a comment between the two members, and SHARED-FIX-006 *Leading trivia* does not say whether that comment moves. A test would lock a visible output nobody decided. I report it to the lead as a candidate output question, or as a withheld rare class.
- **RH3206** (same base `ExpressionBodyToBlockCodeFixProviderBase` as RH3203, RH3205, and RH3207): not tested separately. Three subclasses of the base are covered. The base holds the decision, but it is a base class with overrides, so this is no strict single-call-site proof; the lead may ask for it.
- **RH3202 and RH5417 sibling cases** hold today through `FormatMemberOnLinesOfItsOwnAsync`, and existing tests cover them (SHARED-FIX-007 example).
- **The RH3208 statement case and the RH5417 rarity premise**: dropped (D9).

## thoenissen/Reihitsu#1748

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5403…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheStatement` (spec example, issue input) | RH5403-003, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `RH5403…Tests.VerifyFixInsertsNoBlankLineAboveStatementBehindAnotherStatement` (no comment) | SHARED-FIX-006 | same | fail |
| `RH5403…Tests.VerifyFixInsertsNoBlankLineAboveStatementBehindAnotherStatementOfASwitchSection` | SHARED-FIX-006 | same | fail |
| `RH5403…Tests.VerifyFixKeepsTheColumnOfACommentInAnotherColumnThanTheStatement` | SHARED-FIX-006 | same | fail |
| `RH5403…Tests.VerifyFixKeepsTheBlankLineBetweenTheOpeningBraceAndTheStatement` | SHARED-FIX-006 | same | fail |
| `RH5403…Tests.VerifyFixKeepsTwoBlankLinesAboveTheStatement` | SHARED-FIX-006 | same | fail |
| `RH5403…Tests.VerifyFixOfAStatementIndentedWithTabsKeepsTheTabsAndCountsEachAsOneColumn` (change 10) | SHARED-FIX-006, SHARED-FIX-007 | same | fail |
| `RH5403…Tests.VerifyFixKeepsADirectiveDirectlyAboveTheStatement` (other side) | SHARED-FIX-006 | same | pass |
| `RH5403…Tests.VerifyFixKeepsOneBlankLineAboveTheCommentThatLeadsTheStatement` (other side) | SHARED-FIX-006 | same | pass |
| `RH5404…Tests.VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheDeclaration` (spec example) | RH5404-003, SHARED-FIX-006 | same | fail |
| `RH5404…Tests.VerifyFixKeepsAOneLineDocumentationCommentAboveTheDeclaration` | SHARED-FIX-006 | same | fail |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| The issue's RH5403 input (comment above an `if` behind `a = 1;`) | LF, CRLF | code fix (`VerifyCodeFixWithEndOfLine`) | reproduced | observed: a blank line inserted above `// c`; expected: none (M1) |
| The RH5404 counterpart (`// c` above `class B { int _x; }` behind a type) | LF, CRLF | code fix | reproduced | observed: a blank line above `// c`; expected: none |
| The same leading trivia through `FormatNodeInDocumentAsync` directly | LF, CRLF | formatter entry point | reproduced | `SpecExampleKeepsCommentAboveStatementTargetBehindAssignment` fails the same way |

### Not covered, and why

See #1140: the shared mechanism is tested there.

## Whole run

### For the lead (observations, not a diagnosis)

1. **RH5308 nested stair.** `VerifyNestedConditionalStairIsDetectedAndFixed` reports only the inner conditional, nested as the false branch of a correctly laid-out outer one. It expects the inner `?` in column 28, the stair that formatting the document gives, which RH5308-002 checks. RH5308-004 now says "laid out from that column (FMT-PIPE-017)" from the inner conditional's first token `b` in column 26. Read literally as "four columns right of `b`", that gives 30. The scratch check produced 30, and the test fails on it. I kept 28: it is what the formatter gives in place (SHARED-FIX-007), and what RH5308-002 accepts. If the lead reads RH5308-004 the other way, the result is a visible output question.
2. **Fix All convergence after narrowing.** The test files now hold several cases where two narrowed fixes touch the same line:
   - sibling conditionals sharing the line `: "2", b`;
   - the existing `FixAllCorrectsEveryDiagnostic` scenarios of RH5306 and RH5308.

   Under the naive narrowing in my scratch check, they needed two Fix All iterations. They assert one (SHARED-FIX-002).
3. **Pinned by test, needing no decision from the lead:**
   - a blank line holding whitespace in kept leading trivia;
   - a foreign line ending in kept leading trivia.

   Both are written back as they stand, per FMT-PIPE-018 (*restores the tabs and line endings*).
4. **Candidate output question:** the #1687 comment-in-gap case, above.

### Checks run on head be6bd4d (production code unchanged)

- `scripts/build.sh`: 0 warnings, 0 errors.
- `scripts/format.sh --force` over every changed test file: no change.
- Formatter tests (three classes): 74 failed, 113 passed.
  - The 74 failures are exactly the rewritten and new tests above. Every failure is `Assert.AreEqual` on the expected text.
  - In the scratch check, all 187 passed.
- Analyzer tests (filter below): 104 failed, 1187 passed.
  - The failures are the tests marked "fail" above, counted per `DataRow`.
  - None of them fails on a diagnostic mismatch: every failure is a content difference of the fixed document.
- `scripts/test.sh --project architecture`: 118 of 118 passed. The retired RH5308-006 tag is gone from the tests.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~ReihitsuFormatterDetachedRootTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001|FullyQualifiedName~RH3202|FullyQualifiedName~RH3203|FullyQualifiedName~RH3205|FullyQualifiedName~RH3207|FullyQualifiedName~RH5110|FullyQualifiedName~RH5111|FullyQualifiedName~RH5112|FullyQualifiedName~RH5113|FullyQualifiedName~RH5201|FullyQualifiedName~RH5206|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304|FullyQualifiedName~RH5305|FullyQualifiedName~RH5306|FullyQualifiedName~RH5307|FullyQualifiedName~RH5308|FullyQualifiedName~RH5401|FullyQualifiedName~RH5403|FullyQualifiedName~RH5404|FullyQualifiedName~RH5408|FullyQualifiedName~RH5412|FullyQualifiedName~RH5417|FullyQualifiedName~RH5513|FullyQualifiedName~RH5517"
scripts/test.sh --project architecture
```
