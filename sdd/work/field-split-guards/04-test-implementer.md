Role: test implementer
Commit: 33a8607
Verdict: #963: reproduced; #1384: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the examples and normative text of FMT-STRUCT-022, FMT-STRUCT-030, FMT-PIPE-029, FMT-PIPE-031, RH7101-004, RH7101-006, and RH7101-007 as amended in 03-spec-author.md. Where a requirement decides only that a declaration stays whole, the rest of the layout was checked on `main` first. I formatted a single-declarator twin of each fixture (`scripts/format.sh`) and confirmed it is a fixed point, so a not-split expected value differs from today's output only by the split. Where no such twin exists, the test asserts only that the declaration stays whole and that a second pass is stable (`AssertDeclarationIsNotSplit`).

"Red" means the test fails on this head, on the issue's own difference. I read every failure message: each one fails on exactly that difference. A formatter case fails because the declaration is split, a `SplitsFieldDeclaration` case returns true, a withheld fix case offers one action, and the Fix All case splits the guarded declaration. "Green" means the test passes now and must keep passing.

Files changed (tests only, formatted with `scripts/format.sh`):

- `src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterSplitsFieldDeclarationTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterFieldSplitTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/StructuralTransformSeparatorTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Ordering/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`

## thoenissen/Reihitsu#963

Scope: the field split only. The `{// why` part is #1439 and is not tested here.

### Tests

The class is `FieldDeclarationSplitTests` (FDS) unless the row names another one.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentBehindTheTypeKeepsTheDeclarationUnsplit` (reproduction; replaces `CommentOnTheTypeLineIsStillDuplicatedOntoEveryField`, which pinned the copy) | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentBetweenAModifierAndTheTypeKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentInsideAnAttributeListKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentBehindAnAttributeListOnItsLineKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentOnItsOwnLineBetweenTheAttributeListAndTheModifiersKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentBetweenTwoModifiersKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentInsideTheTypeKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `LineCommentBehindTheTypeWithTheDeclaratorsOnTheNextLineKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `CommentThatSpansLinesBetweenAModifierAndTheTypeKeepsTheDeclarationUnsplit` | FMT-STRUCT-030, FC-027 | `AssertDeclarationIsNotSplit` | red |
| `DocumentationCommentBehindAnAttributeListKeepsTheDeclarationUnsplit` (Parse mode) | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `DocumentationStyleCommentBehindAnAttributeListKeepsTheDeclarationUnsplitInDocumentationModeNone` | FMT-STRUCT-030, FC-033 | `AssertRuleResult` + None options | red |
| `CommentBehindTheTypeInANestedTypeKeepsTheDeclarationUnsplit`, `…InAnInterface…`, `CommentBetweenAModifierAndTheTypeInARecordStruct…` | FMT-STRUCT-030, FC-016 | `AssertRuleResult` | red |
| `CommentBehindTheTypeOfOneDeclarationLeavesAnotherDeclarationSplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `SpecExampleDeclarationsWithDocumentationOrACommentInTheRepeatedPartAreNotSplit` (first spec example; covers #1384 too) | FMT-STRUCT-030 | `AssertRuleResult` | red |
| `SpecExampleOrdinaryCommentInFrontAndCommentInsideTheFirstDeclaratorDoNotKeepTheDeclarationUnsplit` | FMT-STRUCT-030, FC-003 | `AssertRuleResult` | green |
| `SpecExampleCommentOnItsOwnLineInFrontOfTheFirstNameStaysWithTheFirstField` (maintainer answer A) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| Existing tests that now carry the FMT-STRUCT-030 tag for the split side of the boundary, unchanged otherwise: `InlineBlockCommentCombinedFieldsAreSplitAtMemberIndentation`, `OwnLineBlockCommentCombinedFieldsAreSplitAtMemberIndentation` (leading trivia); `BlockCommentBeforeSeparatorStaysBeforeTheGeneratedSemicolon` (behind the first declarator); `TrailingBlockCommentAfterSeparatorOnTheDeclaratorsLineIsPreserved` (behind the comma); `CommentBeforeFirstDeclaratorIsPreserved`, `SingleLineDocumentationCommentBeforeFirstDeclaratorIsPreserved`, `DelimitedDocumentationCommentBeforeFirstDeclaratorIsPreserved` (the first declarator's own line); `DocumentationCommentBeforeDeclaratorIsPreserved` (later declarator) | FMT-STRUCT-030, FC-003 | `AssertRuleResult` | green |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForADocumentedDeclarationAndForOneWithACommentBehindTheType` (spec example 2, Parse) | FMT-PIPE-031 | direct call | red |
| `…ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone` (spec example 2, None) | FMT-PIPE-031 | direct call | red (on `_first`) |
| `…ReturnsFalseForACombinedDeclarationWithACommentInThePartEveryFieldRepeats`, 11 rows: inside or behind an attribute list, an own-line comment after the list, `///` after the list, between modifiers, modifier/type, inside the type, behind the type, `//` and `/* */` behind the type with the declarators on the next line, and a `/* */` spanning lines. Each row runs in Parse and None. | FMT-PIPE-031, FC-001 | direct call | red (11) |
| `…ReturnsTrueForACombinedDeclarationWithACommentOutsideThePartEveryFieldRepeats`, 12 rows: leading trivia; the first declarator's own line with `/* */`, `//`, `///`, `/** */`; initializer; behind the first declarator; behind the comma (`/* */`, `//`); `///` and `/** */` in front of a later declarator | FMT-PIPE-031, FC-003 | direct call | green |
| `RH7101DoNotCombineFieldsAnalyzerTests.SpecExampleCommentInThePartEveryFieldRepeatsWithholdsTheFix` | RH7101-007 | `VerifyNoCodeFixIsOffered` | red |
| `…CommentBehindTheAttributeListOnItsLineWithholdsTheFix`, `…CommentOnItsOwnLineBetweenTheAttributeListAndTheModifiersWithholdsTheFix`, `…CommentBetweenTwoModifiersWithholdsTheFix`, `…CommentInsideTheTypeWithholdsTheFix`, `…DocumentationStyleCommentBehindTheAttributeListWithholdsTheFix`, `…CommentBehindTheTypeOfADeclarationOfANestedTypeWithholdsTheFix` | RH7101-007, FC-009 | `VerifyNoCodeFixIsOffered` | red |
| `…DocumentationStyleCommentBehindTheAttributeListWithholdsTheFixInDocumentationModeNone` | RH7101-007, FC-033 | `VerifyNoCodeFixIsOffered(…, None)` | red |
| `…SpecExampleCommentInsideTheFirstDeclaratorDoesNotWithholdTheFix` | RH7101-007, RH7101-004 | `Verify` + `AssertSingleFixUnderBothLineEndings` | green |
| `…SpecExampleCommentOnItsOwnLineInFrontOfTheFirstNameDoesNotWithholdTheFix` | RH7101-007, RH7101-004 | same | green |
| `…DocumentationStyleCommentOnItsOwnLineInFrontOfTheFirstNameDoesNotWithholdTheFix` (`/** c */`, Parse) | RH7101-007, RH7101-004 | same | green |
| `…CommentInFrontOfTheDeclarationDoesNotWithholdTheFix`, `…CommentBehindTheFirstDeclaratorDoesNotWithholdTheFix` | RH7101-007, RH7101-004, FC-003 | same | green |
| `…FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` | RH7101-007, RH7101-004, FC-008 | `Verify` (iterative and Fix All) | red |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `private int /* c */ _first, _second;` (the issue input) | LF, CRLF | formatter (whole document) | reproduced | observed: `private int /* c */ _first;` / `private int /* c */ _second;`; expected: unchanged |
| `private /* c */ int`, `[Obsolete /* why */]`, `[Obsolete] /* c */`, `[Obsolete]⏎⏎// c`, `public /* c */ static`, `System. /* c */Int32`, `int // c⏎_e, _f;`, `///` after the attribute list (Parse and None), and a multi-line `/* a⏎ b */` between modifier and type | LF, CRLF | formatter | reproduced | the comment, and the attribute list, is copied onto every field; expected: one declaration |
| Class, nested class, interface, record struct | LF, CRLF | formatter | reproduced | same copy in each container |
| The same slots | LF | RH7101 fix | reproduced | one fix is offered; expected: diagnostic reported, no fix |
| Fix All over one guarded and two plain declarations | LF | RH7101 Fix All | reproduced | the guarded declaration is split; expected: it stays combined and keeps its diagnostic |
| Own line in front of the first name (answer A), initializer, behind the first declarator, leading trivia | LF, CRLF | formatter, RH7101 fix | holds | split as specified (control) |

### Not covered, and why

- FC-019 and FC-020 (directive in the gap): `DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact` and `DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact` (FMT-STRUCT-023) already cover a directive. `ReturnsFalseForACombinedDeclarationThatCarriesADirective` covers the predicate. The change touches only comments.
- One container per slot: the skeptic established that `SplitFields` is called from `VisitClassDeclaration`, `VisitStructDeclaration`, `VisitRecordDeclaration`, and `VisitInterfaceDeclaration`. All four call the same predicate, `FieldDeclarationSplitTransform.Splits`, which `ReihitsuFormatter.SplitsFieldDeclaration` returns. I checked this with `rg -n "SplitFields\(|Splits\(" src/Reihitsu.Formatter`. A nested type, an interface, and a record struct are still tested once each.
- RH7101 withheld cases run on LF only. The fix offer is a yes/no answer from `SplitsFieldDeclaration`, and its CRLF behavior is covered by the formatter tests, which run every slot under LF and CRLF.
- FC-028 (FormatNode against document formatting): no caller formats a field declaration detached.
- FC-013 and SHARED-DIAG-010: RH7101 is not a layout rule (02-skeptic.md), so I added no `VerifyFormatter` parity test.
- FC-018, FC-022, FC-023, FC-036: no rename and no literals are involved.

## thoenissen/Reihitsu#1384

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FDS `DocumentedDeclarationsAreNotSplitWhileAnUndocumentedDeclarationIs` (reproduction: the issue's three instances in one class) | FMT-STRUCT-030 | `AssertRuleResult` | red |
| FDS `DocumentedCombinedFieldsAreNotSplit`, `…InNestedTypeAreNotSplit`, `…WithThreeDeclaratorsAreNotSplit`, `DocumentationCommentInFrontOfTheAttributeListKeepsTheDeclarationUnsplit`, `DelimitedDocumentationCommentKeepsTheDeclarationUnsplit`, `DocumentedCombinedStructFieldsAreNotSplit`, `…RecordFields…`, `…RecordStructFields…`, `…InterfaceFields…` (these replace the nine `Documented…AreSplitAtMemberIndentation` and `MultiLineDocumentationCommentCombinedFieldsAreSplitAtMemberIndentation` tests that pinned the split) | FMT-STRUCT-030, FC-016 | `AssertRuleResult` | red |
| FDS `DocumentedDeclarationWithADocumentedLaterDeclaratorIsNotSplit` (replaces `FieldAndDeclaratorDocumentationCommentsAreBothPreserved`) | FMT-STRUCT-030 | `AssertDeclarationIsNotSplit` (the layout of the inner `///` is undecided) | red |
| FDS `DocumentedDeclarationWithACommentBeforeTheFirstDeclaratorIsNotSplit` (replaces `FieldDocumentationAndCommentBeforeFirstDeclaratorAreBothPreserved`) | FMT-STRUCT-030 | `AssertRuleResult` | red |
| FDS `DocumentedCombinedFieldsAreNotSplitInDocumentationModeDiagnose` | FMT-STRUCT-030, FC-033 | `AssertRuleResult` + Diagnose options | red |
| FDS `DocumentationCommentWrittenBehindTheCodeOfTheMemberInFrontKeepsTheDeclarationUnsplit`, `DelimitedDocumentationCommentWrittenBehindTheCodeOfTheMemberInFrontKeepsTheDeclarationUnsplit` | FMT-STRUCT-030 | `AssertRuleResult` | red |
| FDS `SpecExampleDocumentationStyleCommentsAreOrdinaryCommentsInDocumentationModeNone` | FMT-STRUCT-030, FC-003 | `AssertRuleResult` + None options | green |
| FDS `SpecExampleCommentsKeepDescribingTheDeclaratorTheyWereWrittenNextTo` (the realigned example) | FMT-STRUCT-022 | `AssertRuleResult` | green |
| `StructuralTransformSeparatorTests.DocumentedDeclarationIsLeftAsOneDeclaration` (phase output, Parse) | FMT-STRUCT-030 | `AssertSplitFields` | red |
| `StructuralTransformSeparatorTests.FieldCreatedBehindADeclarationWithADocumentationStyleCommentInDocumentationModeNoneGetsNoIndentation` (replaces `FieldCreatedBehindADocumentedDeclarationGetsNoIndentation`, which split a documented declaration in Parse) | FMT-STRUCT-026 | `AssertSplitFields` + None options (helper gained an optional `parseOptions`) | green |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForADocumentedDeclarationUnlessDocumentationCommentsAreNotParsed`, 5 rows: `///` own line, `/** */` own line, `///` and `/** */` behind code, `///` in front of the attribute list. Each row expects false in Parse and Diagnose and true in None. | FMT-PIPE-031, FC-033 | direct call | red (5) |
| `ReihitsuFormatterFieldSplitTests.DocumentationCommentBehindTheTargetGoesWithTheMemberBehindIt`, `…DocumentationCommentThatSpansLinesInFrontOfAMemberThatStartsItsLineStaysWhereItStands`, `…SingleLineDocumentationCommentInFrontOfAMemberThatStartsItsLineStaysWhereItStands`, `…DocumentationCommentThatStartsItsLineInFrontOfTheMemberBehindTheTargetStaysWhereItStands` (rewritten to the realigned examples; the target is no longer documented) | FMT-PIPE-029 (+010/018 as before) | `AssertSplitsTarget` | green |
| `RH7103…Tests.CodeFixLeavesADocumentedCombinedFieldOfTheTypeUnsplit`, `…UnderCarriageReturnLineFeed` (the code-fix regression requested on the issue, multi-line `<summary>`, Parse mode) | FMT-STRUCT-030, SHARED-FIX-003 | `Verify` + `ApplyDocumentationModeParseToTestProject` | red |
| Existing RH7101-006 tests (`DocumentedDeclarationsAreReportedWithoutAFix`, `…InDocumentationModeDiagnose`, `DocumentationCommentInFrontOfTheAttributesOfTheDeclarationWithholdsTheFix`, the gap-in-front `…WithholdsTheFix` family, the mode-None fix tests) | RH7101-006 | unchanged | green; they must stay green once the fix asks only `SplitsFieldDeclaration` (decision 5) |

Deleted from `ReihitsuFormatterFieldSplitTests`: these nine tests gave the entry point a target with a documentation comment in front of it, and FMT-PIPE-029 no longer covers that input. Its only caller, the RH7101 fix, is withheld for it (RH7101-006). The input is unreachable (AGENTS.md, *Supported inputs*).

- `MembersBehindADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine`
- `FirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine`
- `MembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsStartTheNextLine`
- `FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentInTheGapInFrontStartsTheNextLine`
- `MembersBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine`
- `FirstFieldBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine`
- `MembersBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine`
- `FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine`
- `FirstFieldBehindADocumentationCommentThatStartsItsLineInTheGapInFrontKeepsTheTargetsLine`

The behind-the-target half of each "Members…" example is still covered by the existing `MemberBehind…InTheGapBehind…` tests, which match the realigned spec examples.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `/// <summary>The coordinates</summary>⏎public int X, Y;` | LF, CRLF | formatter | reproduced | observed: `public int X;⏎public int Y;`; expected: one declaration |
| `/** <summary>The sizes</summary> */⏎public int Width, Height;` | LF, CRLF | formatter | reproduced | split; expected: unchanged |
| `public int A, B;` undocumented | LF, CRLF | formatter | holds | split (control) |
| Diagnose mode; `///` and `/** */` behind the code of the member in front; `///` in front of an attribute list; class, nested class, struct, record, record struct, interface | LF, CRLF | formatter | reproduced | split; expected: one declaration |
| None mode | LF, CRLF | formatter | holds | split, comment kept verbatim (spec example 3) |
| RH7103 on the issue comment's class (multi-line summary, Parse) | LF, CRLF | code fix (formats the whole type) | reproduced | observed: the fix moves `Create` and splits `X, Y`; expected: `Create` moved, field untouched |
| Structural phase alone | LF, CRLF | phase output | reproduced | `private int _x;private int _y;`; expected: unchanged |

Difference from the issue text: the issue expects the `///` instance "unchanged". The tests expect the declaration unsplit with the `<summary>` expanded over three lines. The documentation-comment phase writes that expansion whether or not the split runs (FMT-STRUCT-030 first example; the single-declarator twin on `main` gives the same expansion). The issue's purpose, no split, is what the tests assert.

### Not covered, and why

- RH7109/RH7102, RH7104, RH5110, and RH8305 also format the whole type (issue comment). They reach the split through the same `FormatNodeInDocumentAsync` path into the same predicate. The issue asked for one regression test, so RH7103 is the representative.
- FC-015 (decision 5, the private documentation check in `TryGetFixableFieldDeclaration`): its removal is not observable at the fix's output. The existing RH7101-006 tests pin that output and must stay green. Removing the copy is for the auditor to check against the code; no test can tell the copy and the predicate apart while they agree.
- FC-010 (second pass, CRLF): `AssertRuleResult` runs both passes under LF and CRLF, plus the per-phase stability check, for every formatter test above.

## Whole run

- Full formatter project: 48 failed and 4728 passed. The 48 are the red tests named above: 30 in FDS, 17 `SplitsFieldDeclaration` cases, and 1 separator test.
- Full analyzer project: 11 failed and 7357 passed. The 11 are the 9 red RH7101 tests and the 2 RH7103 tests.
- Architecture project: 115 passed (the requirement tags resolve).
- New helper in FDS: `AssertDeclarationIsNotSplit(input, variableName)`. It runs `AssertSecondPassIsStable` and compares the declarator list of the declaration, before and after `ReihitsuFormatter.FormatSyntaxTree`, under LF and CRLF.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterFieldSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.StructuralTransforms.StructuralTransformSeparatorTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.StructuralTransformRequirementTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH7101DoNotCombineFieldsFormatterTests"
```
