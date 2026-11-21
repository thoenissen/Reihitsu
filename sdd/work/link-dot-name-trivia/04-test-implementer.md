Role: test implementer
Commit: 132fa37c
Verdict: tests written

Inputs read: thoenissen/Reihitsu#1888, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, the change doc `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md`, FMT-UNSUP-015/016 in `sdd/specs/formatter/unsupported-syntax.md`, FMT-BREAK-020, RH2001-007, RH7101-007. The prompt carried no intended fix, cause, or verdict.

Every expected value comes from the spec examples, the requirement text, and the change doc (reason text, locations, refusal, withheld fixes). The analyzer markup of the new code-fix tests is today's diagnostic set, which this run does not change (RH5201 reports two dots in the change-doc input: `(6,22)` behind `.C()` and `(8,13)` in front of `E()`).

## Tests

All new tests were run on the unchanged code (132fa37c plus these tests). "Fails (right reason)" means: the formatter does not refuse the input — either it changes it ("must return a refused input unchanged") or returns it unchanged without reporting ("must report the input as unsupported" / `FindUnsupportedSyntax` null) — or a code fix is still offered, or the CLI exits 0. No failure is a compile error or an unrelated assertion.

New file `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkDotNameCommentTests.cs` (helper `FormatterTestsBase.AssertUnsupportedFor` / `AssertSupported`, LF and CRLF each; new constant `FormatterTestsBase.LinkDotNameCommentReason`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleTheGapBehindEachLinksDotIsUnsupported` (location 7,24) | FMT-UNSUP-016 | `AssertUnsupportedFor` + `FormatSyntaxTree` location | fails (right reason) |
| `EachLinkOfTheSpecExampleIsUnsupported` (4 rows) | FMT-UNSUP-016 | `AssertUnsupportedFor` | fails (right reason) |
| `SpecExampleDirectiveBetweenTheDotAndTheMemberNameIsUnsupportedInEveryBuild` (location 8,1; DEBUG on/off) | FMT-UNSUP-016; FC-005, FC-020 | `AssertUnsupportedFor` | fails (right reason) |
| `SpecExampleCommentsOutsideTheDotToNameGapsAreSupported` | FMT-UNSUP-016 | `AssertSupported` | passes |
| `EveryCommentAndDirectiveBetweenTheDotAndTheNameOfEveryLinkIsUnsupported` (8 link kinds × 15 trivia = 120: `t.`, `s?.`, `t!.`, `u!?.`, `c.N(1).`, `this.`, `base.`, `global::System.String.`; block/line/own-line/blank-line/multi-line comments, `#pragma`, `#region`, `#if false` disabled text, `#nullable`, `#line`, `#if/#elif/#endif`, one line and across lines) | FMT-UNSUP-016; FC-003, FC-005, FC-019, FC-020, FC-021 | `AssertUnsupportedFor` | fails (right reason) |
| `DocumentationCommentBetweenTheDotAndTheNameIsUnsupportedInEveryDocumentationMode` (8 × 4 × modes None/Parse/Diagnose = 96) | FMT-UNSUP-016; FC-040 | `AssertUnsupportedFor` | fails (right reason) |
| `ConditionalGroupBetweenTheDotAndTheNameIsUnsupportedInEveryBuild` (8 × 2 builds) | FMT-UNSUP-016; FC-020 | `AssertUnsupportedFor` | fails (right reason) |
| `ConditionalGroupAroundTheWholeStatementIsSupported` (2 builds) | FMT-UNSUP-016 | `AssertSupported` | passes |
| `EveryLinkIsCheckedWithItsOwnGap` (20 scopes: later link of a wrapped chain, invoked member, member read, chain without calls, assignment target, null-conditional assignment target, list element, interpolation hole, lambda, switch arm, argument of another link, index of `?[`, local initializer, `nameof(C.Name)`, `nameof(System.String)`, extended property pattern `{ Next.Name: null }`, query, object initializer, collection expression, local function in a lambda) | FMT-UNSUP-016; FC-027, FC-039 | `AssertUnsupportedFor` | fails (right reason) |
| `LinkInAMemberIsUnsupported` (expression body, property initializer, field with one declarator, value of a field with several declarators, line comment in a field value) | FMT-UNSUP-016; FC-027 | `AssertUnsupportedFor` | fails (right reason) |
| `MemberAccessInDisabledTextIsNotChecked` (X on/off) | FMT-UNSUP-016 | both | X off passes; X on fails (right reason) |
| `CommentOutsideTheGapBehindTheDotIsSupported` (14 rows: in front of `.`/`?.`/later dot, comment line and directive in front of a later link, `///` and `/** */` in front of the dot, inside the name's type-argument list, inside the argument list, behind the link, behind/above the statement, in front of the first token) | FMT-UNSUP-016; FC-003 | `AssertSupported` | passes |
| `CommentBehindADotThatIsNoLinkIsSupported` (qualified name in `typeof`, `new`, cast, `default`; behind `::`, `->`, `..`) | FMT-UNSUP-016; FC-003 | `AssertSupported` | passes |
| `CommentBehindTheDotOfAQualifiedNameIsSupported` (using directive, base list) | FMT-UNSUP-016 | `AssertSupported` | passes |
| `CommentInTheGapOfAnotherShapeKeepsItsReason` (inside `?.`, `!.`, `!?.` → FMT-UNSUP-015's reason; between name and `(` → FMT-UNSUP-014's) | FMT-UNSUP-016 | `AssertUnsupportedFor` | passes (those shapes exist today) |
| `FirstShapeInTheTextIsReported` (six orderings with FMT-UNSUP-011/014/015) | FMT-UNSUP-001, FMT-UNSUP-016 | `AssertUnsupportedFor` | fails at the first assertion (today reports the binary-operator comment behind it) |
| `SpecExampleOverlapWithTheVariableDeclarationShape` (`[Obsolete(nameof(C. /* c */ M))] private int _e;` → FMT-UNSUP-004; `int x = t. /* c */ Length;` → FMT-UNSUP-016) | FMT-UNSUP-004, FMT-UNSUP-016 | `AssertUnsupportedFor` | first assertion passes, second fails (right reason) |
| `CommentBetweenTheDotAndTheNameInAnAttributeArgumentOfAVariableDeclarationGivesTheReasonOfTheVariableDeclaration` (field one/several declarators, event field × 2 comments) | FMT-UNSUP-004, FMT-UNSUP-016 | `AssertUnsupportedFor` | passes (FMT-UNSUP-004 today) |
| `CommentBetweenTheDotAndTheNameInAnAttributeArgumentOfAnotherMemberGivesTheReasonOfTheLink` (method, property, nested class, local function × 2) | FMT-UNSUP-004, FMT-UNSUP-016 | `AssertUnsupportedFor` | fails (right reason) |
| `DirectiveBetweenTheDotAndTheNameInAFieldValueKeepsTheReasonOfTheFieldDeclaration` (2 builds) | FMT-UNSUP-010, FMT-UNSUP-016 | `AssertUnsupportedFor` | several declarators passes (FMT-UNSUP-010); one declarator fails (right reason) |
| `DirectiveBetweenTheDotAndTheNameInAnAttributeArgumentOfAFieldKeepsTheReasonOfTheFieldDeclaration` (2 builds) | FMT-UNSUP-010, FMT-UNSUP-016 | `AssertUnsupportedFor` | same split as above |
| `FormatDocumentAsyncRefusesTheShape` | FMT-UNSUP-016 (FMT-UNSUP-002 entry point) | direct API | fails (right reason) |
| `NodeLevelEntryPointsRefuseATargetThatHoldsTheShape` (`FormatNode` method/statement, `FormatNodeInDocumentAsync` statement/type, `FormatNodeInDocumentWithContextAsync`, the method beside is formatted) | FMT-UNSUP-016; FC-028 | direct API | fails (right reason) |
| `NameOrExpressionOfAMemberAccessAloneDoesNotHoldTheShape` (comment behind the dot; own-line comment in the name's leading trivia) | FMT-UNSUP-016, FMT-UNSUP-003; FC-041 | `FindUnsupportedSyntax`, `FormatNode`, `FormatNodeInDocumentAsync` | fails at the first assertion (member access not reported) |
| `NameOfAMemberBindingAloneDoesNotHoldTheShape` (same two placements) | FMT-UNSUP-016, FMT-UNSUP-003; FC-041 | `FindUnsupportedSyntax`, `FormatNode` | fails at the first assertion |
| `FindUnsupportedSyntaxAnswersForABuiltNode` (5 parsed expressions) | FMT-UNSUP-003, FMT-UNSUP-016; FC-043 | `FindUnsupportedSyntax` | fails (returns null) |
| `FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck` (4 inputs, one supported) | FMT-UNSUP-016 | `FindUnsupportedSyntax` vs `FormatSyntaxTree` | passes (both agree today, and must still agree) |
| `OutputOfFormattedLinksIsNotUnsupported` (13 rows: names split from the dot by whitespace alone next to comments in front of the dot, behind the name, inside and behind the link, above the statement; assignment target, `nameof`, lambda body; doc modes None/Parse/Diagnose) | FMT-UNSUP-005, FMT-UNSUP-016; FC-038, FC-010 | two `FormatSyntaxTree` passes | passes |
| `OutputOfTheOtherSideExampleIsNotUnsupported` | FMT-UNSUP-005, FMT-UNSUP-016; FC-038 | two `FormatSyntaxTree` passes | passes |

Other new tests:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ChainSplitMemberNameRejoinTests.SplitMemberNameRejoinsInEveryEnclosingConstruct` (assignment target, `nameof`, interpolation hole, local initializer joined onto the dot) | FMT-BREAK-020; FC-027 | `AssertRuleResult` | passes |
| `UnsupportedLinkOperatorCommentTests.CommentBetweenTheDotAndTheMemberNameGivesTheReasonOfTheDotToNameGap` (5 rows, moved from the FMT-UNSUP-015 tests, below) | FMT-UNSUP-015, FMT-UNSUP-016 | `AssertUnsupportedFor` | fails (right reason) |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests.NoFixIsOfferedForACommentBetweenTheDotAndTheMemberName` (LF, CRLF; change-doc input) | SHARED-FIX-010, FMT-UNSUP-016; FC-043 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberName` (LF, CRLF) | SHARED-FIX-010, FMT-UNSUP-016; FC-043 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberName` (LF, CRLF) | SHARED-FIX-010, FMT-UNSUP-016; FC-043 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyNoFixIsOfferedForABlockCommentBetweenTheDotAndTheMemberName` (`t.`, `s?.`, `t!.` on one line) | SHARED-FIX-010, FMT-UNSUP-016 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberNameOfTheInitializerValue` (spec example `= Source. /* three */ Length;`, LF, CRLF) | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyNoFixIsOfferedForALineCommentBetweenTheDotAndTheMemberNameOfTheInitializerValue` (change-doc input, LF, CRLF) | RH2001-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH7101DoNotCombineFieldsCommentAndDirectiveTests.ChangeExampleCommentBetweenTheDotAndTheMemberNameWithholdsTheFix` (LF, CRLF) | RH7101-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `RH7101DoNotCombineFieldsCommentAndDirectiveTests.CommentBetweenTheDotAndTheMemberNameElsewhereInTheTypeWithholdsTheFix` (member access, member binding with own-line comment) | RH7101-007 | `VerifyNoCodeFixIsOffered` | fails (fix offered) |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveBetweenADotAndItsMemberNameIsReportedAsUnsupported` (the change doc's four inputs, (5,24), (8,1), (7,18), (7,21)) | FMT-UNSUP-016 | `Program.Main` end to end | fails (exit code 0, file formatted) |

Existing tests that asserted an output the change alters — each rewritten to the refusal, retagged from FMT-BREAK-001 to FMT-UNSUP-016:

| Test (old name → new name) | File | Change | Expected now |
|---|---|---|---|
| `SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` → `SingleLineCommentBetweenDotAndMemberNameIsUnsupported` | `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainGapJoinTests.cs` | asserted today's layout; now `AssertUnsupportedFor` at `// c` | fails (right reason) |
| `DirectiveBetweenDotAndMemberNameKeepsItsLine` → `DirectiveBetweenDotAndMemberNameIsUnsupported` | same | now refusal at `#if X` | fails (right reason) |
| `CommentBetweenDotAndMemberNameKeepsSplit` → `CommentBetweenDotAndMemberNameIsUnsupported` | `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainSplitMemberNameRejoinTests.cs` | asserted unchanged output; now refusal at `// keep` | fails (right reason) |
| `BlockCommentBetweenDotAndMemberNameKeepsSplit` → `BlockCommentBetweenDotAndMemberNameIsUnsupported` | same | refusal at `/* keep */` | fails (right reason) |
| `OwnLineCommentBeforeMemberNameKeepsSplit` → `OwnLineCommentBeforeMemberNameIsUnsupported` | same | asserted a blank line in front of the comment; now refusal | fails (right reason) |
| `DirectiveBetweenDotAndMemberNameKeepsSplit` → `DirectiveBetweenDotAndMemberNameIsUnsupported` (now both builds) | same | asserted different branch columns; now refusal at `#if DEBUG` | fails (right reason) |
| `UnsupportedLinkOperatorCommentTests.CommentOutsideTheLinkOperatorIsSupported` rows `Use(s?. /* c */ Length)`, `Use(t!. /* c */ Length)`, `Use(u!?. /* c */ Length)` | `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkOperatorCommentTests.cs` | removed; moved to the new `CommentBetweenTheDotAndTheMemberNameGivesTheReasonOfTheDotToNameGap` | — |
| `UnsupportedLinkOperatorCommentTests.CommentNextToATokenThatIsNoLinkOperatorIsSupported` row `Use(c. /* c */ Name)` | same | removed; moved to the same new test | — |
| `UnsupportedLinkOperatorCommentTests.OutputOfFormattedLinkOperatorsIsNotUnsupported` row `Use(s?⏎. // c⏎Length)` | same | asserted the input is formatted; removed, moved to the same new test as a refusal | — |
| `UnsupportedConditionalOperatorCommentTests.CommentOutsideTheShapeIsSupported` row `var l = s?. /* c */ Length;` | `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedConditionalOperatorCommentTests.cs` | the row now holds FMT-UNSUP-016's shape; changed to `var l = s /* c */ ?.Length;` (still a `?` that is no conditional operator, outside every shape); summary adjusted | passes |

Summaries of the three FMT-UNSUP-015 tests were adjusted so they no longer name the gap behind the operator's `.` as supported.

Searched and left unchanged (the `.` belongs to a qualified name, or the fix does not format): `MultiLineCommentGapSpacingTests`, `BaseListContinuationAlignmentTests`, `ArgumentContinuationLineAlignmentTests:352`, `ParameterContinuationLineAlignmentTests:392`, `ReihitsuFormatterSplitsFieldDeclarationTests:335`, `UnsupportedVariableDeclarationTests:1114`, `RH4009…Tests` (`namespace MyCompany./* x */Project`), `RH2003…Tests`, `RH3103…Tests`, `RH7101DoNotCombineFieldsCommentAndDirectiveTests:1668` (all qualified names); `RH6016…AnalyzerTests:358/723/795` (member accesses, but `RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider` never calls the formatter, and `RH6016…FormatterTests` holds no such gap). Searches: `rg --pcre2 '[\w\)\]\?!]\.[ \t]*(//|/\*)'` and `rg -U --pcre2 '[\w\)\]\?!]\.[ \t]*\r?\n[ \t]*(//|/\*|#(if|else|elif|endif|region|pragma|nullable|line|define))'` over `src/*Test*/`. The full analyzer, formatter, and CLI suites confirm it: the only failures are the tests listed above.

## Not covered, and why

- A `.` inside a documentation comment's `cref`: a cref is structured trivia and cannot hold a comment, a directive, or disabled text between its tokens, so the other side cannot be written.
- FMT-UNSUP-006 throw sites: the change adds no phase refusal; the shape is a precheck shape.
- FC-007, FC-008 (convergence, Fix All): every fix test here withholds the fix; nothing is applied.
- FC-013, FC-014: no analyzer changes.
- FC-018, FC-036 (naming): not a naming fix.
- FC-022 (literals), FC-023 (identifiers): a literal is one token and cannot stand between a `.` and its name; verbatim identifiers as the name do not change which trivia the check reads (the name's first token either way).
- FC-006, FC-024, FC-025 (rewrite meaning, partial, expression-bodied kinds): the change rewrites nothing; FC-025's method and property expression bodies are covered in `LinkInAMemberIsUnsupported` and the RH3202 tests.
- FC-003 for the deleted `CloseGap` skip in `NormalizeChain`: the refusal tests are one side; the other side is the join without trivia (existing `ChainSplitMemberNameRejoinTests` FMT-BREAK-020 tests plus `SplitMemberNameRejoinsInEveryEnclosingConstruct`). The kept `CloseGap` check for the gap in front of the first link stays covered by the existing, unchanged `ChainGapJoinTests` directive/comment-in-front-of-first-link tests and `ChainSplitMemberNameRejoinTests.CommentAboveFirstInvokedLinkDotStillRejoins…`; all pass today.
- Whole-repository stability: `FormatterSelfHostingTests` (CI). The `rg` searches above found the shape only inside test string literals, not in repository code.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedLinkDotNameCommentTests|FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~ChainSplitMemberNameRejoinTests|FullyQualifiedName~UnsupportedLinkOperatorCommentTests|FullyQualifiedName~UnsupportedConditionalOperatorCommentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~BetweenTheDotAndTheMemberName"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests.CommentOrDirectiveBetweenADotAndItsMemberNameIsReportedAsUnsupported"
scripts/test.sh --project architecture
```

Results on the unchanged code: formatter 302 failing (all in the tests above, right reason), 6524 passing; analyzer 17 failing (all new), 11618 passing; CLI 4 failing (all new), 421 passing; architecture 198 passing (FMT-UNSUP-016 is now covered).

## Follow-up candidates

_None._
