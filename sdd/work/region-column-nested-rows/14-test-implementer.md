Role: test implementer
Commit: 65a0969
Verdict: tests written

I worked from these sources:
- `00-lead.md`: the *State* worklist A1–A3, and decision 8, which supersedes decision 6;
- `13-auditor.md`: findings 1–3, with their failing and holding members;
- my handovers `06` and `11`;
- the requirements as of a92c8a3: FMT-BREAK-046, FMT-BREAK-015, -021, -022, FMT-BLANK-011, -018, -020, and FMT-INDENT-018, -020, -029.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Where the expected values come from:
- **A1:** FMT-BREAK-046's *On one line* bullet. The initializer is judged once the other requirements of the phase have placed and removed their line breaks, which is batch 2 answer 6A. I assert the spec's current text, not the alternative wording the auditor offered as option 2. If the lead picks that option, these tests change with the spec.
- **A2:** FMT-BREAK-046, which says the closing brace MUST start its line, and its comment paragraph ("a comment behind the last element stays on that element's line, and the closing brace starts the line below it"); also batch 3 answer A and decision 8.
  - The blank line above a comment that now stands on its own line in front of `}` comes from FMT-BLANK-011, and FMT-BLANK-020 removes any blank line below it. This is the layout `TrailingCommaCommentOwnerTests` already pins for `2⏎⏎// comma⏎};`.
  - The text inside a multi-line comment is kept as written.
- **A3:** FMT-BREAK-015 says no line inside a nested braced or bracketed construct is joined. FMT-BREAK-021 and -022 say the first element starts the line after the opening delimiter. Each test pairs the interpolated string with the plain-literal layout.

## Tests
`AssertRuleResult` runs every test under LF and CRLF, with a second pass and the per-phase stability check.

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/ArrayInitializerBraceLineTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `InitializerWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine`. Members: `int[] a = { z⏎.Length }`, `new object[] { z?⏎.Length }`, `stackalloc int[] { z⏎.Length }`. All three become one line. | FMT-BREAK-046 | `AssertRuleResult` | **fails**: the head puts the braces on lines of their own around `z.Length` |
| `InitializerWhoseOnlyLineBreakTheListLayoutJoinsStaysOnOneLine`. Members: `N(⏎1, 2)`, `w[⏎0]`, `typeof(Dictionary<⏎int, int>)`, `(⏎int q) => q` | FMT-BREAK-046 | `AssertRuleResult` | **fails** for the same reason |
| `RowWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine`: `int[,] h = { { z⏎.Length, 1 }, { 2, 3 } };` becomes one line | FMT-BREAK-046 | `AssertRuleResult` | **fails**: the row and the outer initializer are spread out |
| `InitializerWithAWrapTheOtherRulesKeepPutsItsBracesOnLinesOfTheirOwn`. This is the holding side: a binary wrap, a ternary wrap, and a chain with two wrapped links. Their inner columns follow FMT-INDENT-018, -020, and -029. | FMT-BREAK-046 | `AssertRuleResult` | passes |
| `ClosingBraceBehindACommentThatStartsItsLineStartsTheLineBelowIt`. Members: `/* c */` leading the `}` line behind `new[]` and behind a declaration's `=`, plus the `// c` holding side | FMT-BREAK-046 | `AssertRuleResult` | **fails**: `/* c */ };` stays at the brace column |
| `ClosingBraceOfARowBehindACommentThatStartsItsLineStartsTheLineBelowIt`: `{ { 1,⏎2⏎/* c */ }, { 3, 4 } }` | FMT-BREAK-046 | `AssertRuleResult` | **fails**: `/* c */ },` |
| `ClosingBraceBehindAMultiLineCommentBehindTheLastElementStartsTheLineBelowIt`: `2 /* x⏎y */ }` for `new[]` and for a row | FMT-BREAK-046 | `AssertRuleResult` | **fails**: `y */ };` and `y */ },` |

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/ListElementInteriorJoinTests.cs`. All three tests are A3 regression tests. On the base 5ef477e each one fails on the reported join; I checked this in a scratch worktree, since removed.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `InterpolatedStringStartingTheFirstElementOfACollectionExpressionInAnArgumentIsNotJoinedOntoTheBracket` | FMT-BREAK-015, FMT-BREAK-022 | `AssertRuleResult` | passes (base: `[$"a{y}",`) |
| `InterpolatedStringStartingTheFirstElementOfACollectionInitializerInAnArgumentIsNotJoinedOntoTheBrace` | FMT-BREAK-015, FMT-BREAK-021 | `AssertRuleResult` | passes (base: `{$"a{y}",`) |
| `InterpolatedStringStartingTheFirstElementOfAComplexElementInitializerInAnArgumentIsNotJoinedOntoTheBrace` | FMT-BREAK-015, FMT-BREAK-022 | `AssertRuleResult` | passes (base: `{$"a{y}",` inside the dictionary initializer) |

Result on 65a0969: the two classes run 95 tests. 6 fail, exactly the six A1/A2 tests marked **fails** above. 89 pass.

## Scenarios (bug fix)
_None._ This is a repair round of a behavior change.

## Not covered, and why
- **Holding members of A1 already pinned:**
  - the cast join and the object initializer are pinned by `InitializerIsJudgedOnTheLinesTheOtherRulesLeave`;
  - the conditional-access member `z?⏎.Length` is in the chain test.
- **Holding members I left out on purpose:** the attributed lambda (`[A] (⏎int q) => q`) and the empty argument list (`new List<int>(⏎)`).
  - Both stay multi-line, so the braces get lines of their own, which is the same side of the condition the binary, ternary, and two-link-chain members already cover.
  - Their inner layout is decided by no requirement. The head gives `[A]⏎(int q) => q` and `new List<int>(⏎)` with `)` at a continuation column, and a test would pin that unspecified output.
- **Opening-side comment members that the auditor reports as holding** (`{ /* x⏎y */ 1`, `}, /* x⏎y */ { 3, 4 }`, `new[]⏎/* c */ {`, `},⏎/* c */ { 3, 4 }`): these are not part of A2. `CommentsNextToTheBracesKeepTheirLines`, `CommentInFrontOfAMultiLineRowKeepsItsLineAndTheBraceMovesBelowIt`, and `CommentBehindTheCommaBehindARowKeepsItsLine` pin the single-line forms of the same moves. The comment-led `new[]⏎/* c */ {` column stays unstated, as 06 records.
- **A directive in front of `}`:** `ConditionalGroupBetweenRowsIsKept` covers directives next to row braces. The A2 change concerns comments only.
- **Failure classes:**
  - FC-019 and FC-021 (a comment in a moved gap) are the A2 tests.
  - FC-027 and FC-016 (a later phase or rewriter changes the judged lines) are the A1 tests.
  - FC-044 is the implementer's to respect: the tests do not care how the later joins are reached.
  - The other classes stand as 06 and 13 dismissed them; this round changes only line-break layout inside array initializers.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ArrayInitializerBraceLineTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ListElementInteriorJoinTests"
```

## Follow-up candidates
_None._
