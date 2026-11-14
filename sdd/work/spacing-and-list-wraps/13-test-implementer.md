Role: test implementer
Commit: <filled below>
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out decision 18 of `00-lead.md` on the test side: the refused side of the opener join (11-auditor finding 1, FC-003). Expected values come from the spec author's concurrent FMT-BREAK-012 text (a first element held off the opening line by a comment, directive, or disabled text stays at the start of its own line and the list becomes one element per line; a line break only inside a multi-line comment's text holds nothing) and its new example, verbatim. Columns follow the alignment under the first element (FMT-INDENT) that the neighbouring tests already pin. No production code is touched; every test passes on 6710433 as the requirement demands (the output equals `main`'s, per 11-auditor).

## Tests

All in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ListWrapByElementsTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecificationExampleHeldFirstElementMakesListOneElementPerLine` (the new example verbatim: parameter list and invocation with `//`, invocation with `#pragma`) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `LineCommentBehindOpeningParenthesisMakesArgumentListOneArgumentPerLine` (the auditor's counterexample) | FMT-BREAK-012; FC-003 | `AssertRuleResult` | pass |
| `BlockCommentInFrontOfFirstArgumentOnLineBelowOpeningParenthesisMakesArgumentListOneArgumentPerLine` | FMT-BREAK-012; FC-003 | `AssertRuleResult` | pass |
| `DirectiveInFrontOfFirstArgumentMakesArgumentListOneArgumentPerLine` (`#pragma`) | FMT-BREAK-012; FC-003, FC-019 | `AssertRuleResult` | pass |
| `DisabledTextInFrontOfFirstArgumentMakesArgumentListOneArgumentPerLine` (`#if X⏎0,⏎#endif`, `X` undefined) | FMT-BREAK-012; FC-003, FC-020 | `AssertRuleResult` | pass |
| `LineCommentBehindObjectCreationOpeningParenthesisMakesArgumentListOneArgumentPerLine` | FMT-BREAK-012; FC-027 | `AssertRuleResult` | pass |
| `LineCommentBehindAttributeOpeningParenthesisMakesAttributeArgumentListOneArgumentPerLine` | FMT-BREAK-012; FC-027 | `AssertRuleResult` | pass |
| `LineCommentBehindOpeningParenthesisMakesParameterListOneParameterPerLine` | FMT-BREAK-012; FC-027 | `AssertRuleResult` | pass |
| `LineCommentBehindOpeningParenthesisMakesLambdaParameterListOneParameterPerLine` | FMT-BREAK-012; FC-027 | `AssertRuleResult` | pass |
| `LineCommentBehindOpeningBracketMakesIndexerParameterListOneParameterPerLine` | FMT-BREAK-012; FC-027 | `AssertRuleResult` | pass |
| `MultiLineCommentBehindOpeningParenthesisLeavesArgumentListSingleLine` (the holding member: `Foo(/* a⏎ b */1, 2)` stays as written) | FMT-BREAK-012; FC-003 | `AssertRuleResult` (input unchanged) | pass |

`AssertRuleResult` checks LF and CRLF, a second pass, and phase stability, so idempotency and line endings are covered per test.

## Not covered, and why

- **A `//` comment on its own line between `(` and the first argument** (`Foo(⏎// c⏎1, 2)`), listed by the auditor: the formatter also inserts a blank line between `(` and the comment (observed with `scripts/format.sh` on 6710433: `Foo(⏎⏎    // c⏎    1,⏎    2);`). That blank line is a blank-line-phase decision outside FMT-BREAK-012 and this run; a test would pin it. The held-first-element case it shares is covered by the `//`-behind-`(` and block-comment-below-`(` tests. Hint for the lead: a candidate follow-up, whether a blank line in front of a comment directly behind an opening parenthesis is intended (not checked against `main`).
- **The holding member with a space in front of the first element** (`Foo( /* a⏎ b */ 1, 2)`): the formatter removes the space behind `*/` exactly as it does for the single-line twin `Foo( /* a */ 1, 2)` → `Foo( /* a */1, 2)` (spacing, not this requirement), so the test uses the already-spaced form to pin only the line-break decision.
- No new production condition exists in this round, so the failure-class walk of 04-test-implementer stands; FC-003 is closed by the held/holding pair above.
- These tests cannot fail on 6710433 by construction: decision 18 keeps the code's output and changes the spec. They fail against the earlier FMT-BREAK-012 text (which demanded the single-line list), which is the disagreement they now pin.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ListWrapByElementsTests"
```

Result on 6710433 plus these tests (Release): 42 of 42 passed.
