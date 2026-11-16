Role: test implementer
Commit: 89ca229
Verdict: tests written

Repair round 1: auditor finding #1 (the test side) and finding #2 (`08-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I re-derived each change from FMT-BLANK-008, -011, -018, -019, and -020 as they stand after `09-spec-author.md`. I did not touch production code. Two guard mutations were applied only locally and then reverted, and the production diff is empty.

## Finding #1: undecided columns removed from exact-output assertions

No requirement decides the column of the first element behind the `<` of a type parameter or type argument list, or behind the `[` of an array size. Those cells were moved out of `AssertRuleResult`/`VerifyFormatter(source, fixedSource)`. Where they still cover FMT-BLANK-011, they are now asserted with `AssertBlankLinesAbove`, which checks only the blank-line state:

| Test (file) | Change |
|---|---|
| `SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanGetsNoBlankLine` → renamed `SpecExampleFirstCommentBehindOpeningParenthesisOrBracketGetsNoBlankLine` (`FirstCommentBehindOpeningTokenTests.cs`) | Removed the `void O<` member. The test now mirrors the trimmed FMT-BLANK-011 example. |
| `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine` (same file) | Removed the `void M<` and `new int[` cells. The exact output now covers the attribute argument list, indexer, tuple, and element access, all aligned by FMT-INDENT-012/025. |
| `AuthorsBlankLineBehindOpeningParenthesisBracketOrLessThanIsKept` → renamed `AuthorsBlankLineBehindOpeningParenthesisOrBracketIsKept` (same file) | Removed the `O<` cell. The `{` side (FMT-BLANK-007) now sits in a non-generic `void O()`. |
| `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` (same file) | Added the cells `// array size` (`new int[`) and `// type parameter` (`void O<`) as blank-line-only checks. |
| `AuthorsBlankLineBehindLessThanOfTypeParameterListIsKept` (same file, new) | The author's blank line behind the type-parameter `<` is kept. Blank-line-only check. |
| `RH5020CommentsShouldBePrecededByABlankLineFormatterTests.VerifyFormatterLeavesFirstCommentBehindOpeningTokenWithoutBlankLine` | Removed the `O<` cell from the input and from the fixed output. |

## Finding #2: the cleanup guard `isAuthorsBlankLineBehindOpeningToken`

The guard decides whether to collapse the leading blank lines of a token that `RemovesBlankLinesDirectlyBefore` covers. It reads two conditions: whether the previous token opens content, and whether the token's leading trivia holds a comment. The new tests pin both sides of each condition.

## Tests

All tests are in `src/Reihitsu.Formatter.Test/Regression/BlankLines/FirstCommentBehindOpeningTokenTests.cs` unless the name says otherwise. Every test runs under LF and CRLF, with a second pass and the per-phase stability check.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleFirstCommentBehindOpeningParenthesisOrBracketGetsNoBlankLine` (changed) | FMT-BLANK-011 | `AssertRuleResult` | pass |
| `FirstCommentBehindOtherOpeningTokensGetsNoBlankLine` (changed) | FMT-BLANK-011 | `AssertRuleResult` | pass |
| `AuthorsBlankLineBehindOpeningParenthesisOrBracketIsKept` (changed) | FMT-BLANK-011, FMT-BLANK-007 | `AssertRuleResult` | pass |
| `FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine` (two cells added) | FMT-BLANK-011 | `AssertBlankLinesAbove` | pass |
| `AuthorsBlankLineBehindLessThanOfTypeParameterListIsKept` (new) | FMT-BLANK-011 | `AssertBlankLinesAbove` | pass |
| `AuthorsBlankLineBehindOpeningBracketOfEmptyCollectionExpressionIsKept` (new; `[⏎⏎// c⏎]`: blank line kept above the comment, none above `]`) | FMT-BLANK-011, FMT-BLANK-019 | `AssertBlankLinesAbove` | pass |
| `AuthorsBlankLineBehindOpeningBracketOfEmptyListPatternIsKept` (new; auditor's side (a)) | FMT-BLANK-011, FMT-BLANK-019 | `AssertBlankLinesAbove` | pass |
| `AuthorsBlankLineBehindOpeningBracketIsKeptWhileBlankLineBehindCommentBeforeClosingBracketIsRemoved` (new; `[⏎⏎// c⏎⏎]`) | FMT-BLANK-011, FMT-BLANK-020 | `AssertBlankLinesAbove` | pass |
| `BlankLineBehindOpeningBracketIsRemovedAboveCommentGluedToClosingBracket` (new; collection and list pattern, `[⏎⏎/* c */ ]`; auditor's side (b)) | FMT-BLANK-011, FMT-BLANK-018 | `AssertBlankLinesAbove` | pass |
| `AuthorsBlankLineBehindOpeningParenthesisIsKeptAboveCommentBeforePropertyPattern` (new; `t is (⏎⏎// c⏎{ Length: 0 }, 1)`; auditor's side (c)) | FMT-BLANK-011 | `AssertBlankLinesAbove` | pass |
| `BlankLineBehindOpeningParenthesisIsRemovedDirectlyBeforePropertyPatternWithoutComment` (new; the guard's side without a comment) | FMT-BLANK-008 | `AssertBlankLinesAbove` | pass |
| `RH5020CommentsShouldBePrecededByABlankLineFormatterTests.VerifyFormatterLeavesFirstCommentBehindOpeningTokenWithoutBlankLine` (changed) | RH5020-002, FMT-BLANK-011 | `VerifyFormatter(source, fixedSource)` | pass |

The new guard tests correctly pass on this head, because they pin existing behavior that the auditor confirmed. I checked that they decide the guard by applying two local mutations to `BlankLineTokenCleanupRewriter.cs`, both reverted:

- **Guard forced to `false`.** Four tests failed on the expected blank-line state: the collection, list-pattern, comment-before-`]`, and property-pattern tests. The glued-comment test still passed, as it should: FMT-BLANK-018 removes that blank line independently of the guard.
- **Comment conjunct forced to `true`.** The whole formatter suite (5285 tests) stayed green. That is why I added `BlankLineBehindOpeningParenthesisIsRemovedDirectlyBeforePropertyPatternWithoutComment`, which then failed on the expected line, `{ Length: 0 }`.

## Not covered, and why

- **Empty `[⏎⏎]` without a comment.** This is the other comment-free cell of the guard. FMT-BLANK-019 says both "remove the blank lines directly before the closing bracket" and "a blank line behind the opening bracket is kept", and both describe this same blank line. The property-pattern `{` cell decides the same conjunct under FMT-BLANK-008 without that ambiguity.
- **A comment behind a comma in front of `]`.** This is the opener side's counterpart. The guard is false there and FMT-BLANK-011 inserts the blank line anyway. `CollectionExpressionClosingBracketCommentTests.CommentBeforeClosingBracketOfArgumentIsIndented` already covers it.
- **Formatter parity (FC-013) for the type-parameter `<`.** `FormatterTestsBase<TAnalyzer>` has no helper that checks "the output is not reported" without asserting the exact output. RH5020 does not depend on the column: its analyzer tests (`SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanIsNotReported`, `FirstCommentBehindEveryOtherOpeningTokenIsNotReported`) cover `<`, and the formatter blank-line tests cover the formatter side.
- **RH8303.** Its analyzer tests with a fix pin the analyzer fix's output, not a formatter column. Its parity test holds no `<` cell. The auditor raised neither.
- **Failure classes.** These are unchanged from `04-test-implementer.md`. This round adds FC-003 (both sides of each guard condition) and FC-017 (no assertion wider than a requirement).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.FirstCommentBehindOpeningTokenTests|FullyQualifiedName~CollectionExpressionClosingBracketCommentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5020CommentsShouldBePrecededByABlankLine"
scripts/test.sh --project architecture
```

Results on the clean tree: formatter 29/29, analyzer 41/41, architecture 153/153. `scripts/format.sh` changed neither test file.

## Follow-up candidates

FMT-BLANK-019 contradicts itself for an empty collection expression or list pattern `[⏎⏎]` without a comment: one sentence removes the blank line directly before the closing token, and another keeps the blank line behind the opening token, and both describe the same blank line. The formatter removes it today. This is a spec wording question outside this run's construct.
