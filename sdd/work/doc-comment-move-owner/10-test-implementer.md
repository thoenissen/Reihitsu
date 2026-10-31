Role: test implementer
Commit: 0c1b1a7
Verdict: tests written

Repair round 1, decision #13 (09-auditor.md finding #1). My prompt said to expect `main`'s output. That instruction matches decision #13 and FMT-BREAK-018, so I followed it. It carried no suspected cause, and I give no diagnosis here.

Every expected value is `main`'s output (8c7d5bec). I observed it with `scripts/format.sh` and `scripts/apply-fix.sh RH3202` from a detached `main` worktree in the scratchpad, which I have since removed. Then I ran the finished test files against that worktree: formatter 24/24 passed, and the RH3202 class 35/35 passed. On `main`, the RH3202 fix output equals the whole-document output for all eight expression-body shapes. On `main`, a second formatting pass changes none of the ternary outputs.

## Tests

New file: `src/Reihitsu.Formatter.Test/Regression/FullPipeline/DelimitedDocumentationInExpressionTests.cs`. Every test uses `AssertRuleResult`, which runs LF and CRLF, two passes, and the phase-stability check.
Added to `src/Reihitsu.Analyzer.Test/Formatting/RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.cs`: 9 code-fix tests. Each runs `Verify` under LF and under CRLF.

Finding #1's members map to test names as follows: (a) behind `:` = `…ColonOfOneLineConditional…`; (b) behind `?` = `…QuestionMarkOfOneLineConditional…`; behind a trailing `/* c */` = `…BlockCommentBehind…`; (c) = `…ColonOfPartlyWrappedConditional…`; (d) = `…QuestionMarkOfPartlyWrappedConditional…`; (e) = `…ParenthesizedNestedConditional…` and `…QuestionMarkOfNestedConditional…`. Whole-document formatting covers each one twice, once in an expression body and once in a `return` statement.

| Test | Requirement / failure class | Helper | Expected now (head 0c1b1a7) |
|---|---|---|---|
| **DelimitedDocumentationInExpressionTests** | | | |
| MovesDelimitedDocumentationBehindColonOfOneLineConditionalInExpressionBody | FMT-BREAK-018; FC-013, FC-027 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonOfOneLineConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInExpressionBody | FMT-BREAK-018; FC-021 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInReturnStatement | FMT-BREAK-018; FC-021 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInExpressionBody | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInExpressionBody | FMT-BREAK-018; FC-021 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInReturnStatement | FMT-BREAK-018; FC-021 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonOfPartlyWrappedConditionalInExpressionBody | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonOfPartlyWrappedConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInExpressionBody | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInExpressionBody | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfNestedConditionalInExpressionBody | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindQuestionMarkOfNestedConditionalInReturnStatement | FMT-BREAK-018 | AssertRuleResult | fails |
| MovesDelimitedDocumentationBehindColonStartingItsLineInReturnStatement | FMT-BREAK-018; FC-003 (holding side) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindColonStartingItsLineInLocalDeclaration | FMT-BREAK-018; FC-003 (holding side) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindAdditionOperator | — (holding member, characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindCoalesceOperator | — (holding member, characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindConditionalAndOperator | — (holding member, characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindAssignmentOperator | — (holding member, characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindLambdaArrow | — (holding member, characterization) | AssertRuleResult | passes |
| MovesDelimitedDocumentationBehindNamedArgumentColon | — (holding member, characterization) | AssertRuleResult | passes |
| **RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests** (code fix) | | | |
| VerifyFixMovesDelimitedDocumentationBehindColonOfOneLineConditional | FMT-BREAK-018; FC-007 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindColonOfPartlyWrappedConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindQuestionMarkOfNestedConditional | FMT-BREAK-018 | Verify (LF, CRLF) | fails |
| VerifyFixMovesDelimitedDocumentationBehindColonStartingItsLine | FMT-BREAK-018; FC-003 (holding side) | Verify (LF, CRLF) | passes |

On head 0c1b1a7: formatter class 16 failed, 8 passed; RH3202 class 8 failed, 27 passed. Each failure is the auditor's difference and nothing else. The operator ends its line: expected `? 1` / `:` / blank / `/** d */ 2;`, actual `? 1 :` / blank / `/** d */ 2;`. For the `?` form, expected `b` / `?` / blank / `/** d */ 1`, actual `b ?` / blank / `/** d */ 1`. Architecture tests pass (92/92), and so do the formatter tests matching `Requirement` (136/136).

`main`'s output, as pinned, keeps the moved comment at the statement's column (8), not under its operator. It also puts a blank line between the operator and the comment. Example for (a): `return b` / `? 1` / `:` / blank / `/** d */ 2;`, with `?` and `:` at column 19 and `/** d */` at column 8. That is today's output on `main`, and decision #13 restores it. The test that pins it does not endorse it.

## Not covered, and why

- **The `///` form behind `?`/`:`.** It predates the run (09-auditor.md *Hints*, follow-up candidate 3), and its output differs from FMT-BREAK-018 on `main`. A test of `main`'s output would pin a known violation, and a test of FMT-BREAK-018 would fail on `main`. The follow-up owns it.
- **The other holding members the auditor lists**: `,` in lists and switch arms, `==`, `||`, `is`, `.`/`?.`, `+=`, the switch-arm and expression-body `=>`, `(`, `[`, `<`, base-list and constraint `:`, `case … when`, `return`/`throw`/`in`/`where`, and `;`/`}` before members and statements. Existing tests already pin `,` in an argument list (`MovesDelimitedDocumentationInsideArgumentList`), `.` (`KeepsStrandedDelimitedDocumentationBeforeMemberAccessDot`), and `;` before members and statements (`DelimitedDocumentationRelocationTests`). I added seven cheap representatives: two operators that leave `:` at its line start, and `+`, `??`, `&&`, `=`, lambda `=>`, and the named-argument `:`. The auditor's 895-fixture differential gives identical output for the rest, and the implementer's re-run of that corpus is the guard for them.
- **RH3202 in a `return` statement.** RH3202 reports only expression-bodied members. The `return` form reaches only whole-document formatting, and the formatter tests cover it there.
- **RH5308 on the outputs.** RH5308 stays silent on `main`'s output (09-auditor.md). The formatter tests assert that output exactly, and that output keeps `?` and `:` at their line starts, so I wrote no separate RH5308 test.
- **Fix All (FC-008).** These shapes put one RH3202 diagnostic in each document. `GetFixAllScenario` already covers Fix All.
- **Node level (FC-028).** The RH3202 tests exercise the code fix's node-level formatting path. I added no direct `FormatNodeInDocumentAsync` test.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~DelimitedDocumentationInExpressionTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests"
```
