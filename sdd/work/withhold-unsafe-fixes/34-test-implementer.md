Role: test implementer
Commit: b03a20cc
Verdict: #1279: tests written (RH3004-008 item 1, RH3004-009); #1281: tests written (RH3106-003 gap); #1257, #1282, #1417, #1422: nothing to do

Final narrowing round, lead rows 44–45 (`00-lead.md`), auditor findings 17–19 (`32-auditor.md`), spec b03a20cc (`33-spec-author.md`). Expected values are copied from the spec examples of RH3004-008, RH3004-009, and RH3106-003, never from the code. No production code and no spec touched. My prompt carried no intended fix, suspected cause, or hoped-for verdict. No output questions raised (standing instruction).

"Fails" means the test fails on b03a20cc for the reason the requirement states; I read every failure: each withheld-fix test passes the diagnostic check of `VerifyNoCodeFixIsOffered` (reported exactly as marked, compiles) and then fails on "a code fix is offered". Class names drop the `AnalyzerTests` suffix.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixWithheldForAStringOfAnActiveBranchOpenedBeforeTheAnonymousMethodThatHoldsADirectiveLine` (spec example, audit 17(b), `#define A`) | RH3004-008 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAStringThatHoldsALineStartingWithHashWithoutAnyDirective` (spec example `@"a⏎#b"`) | RH3004-008 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAStringThatHoldsALineStartingWithHashWithCarriageReturnLineFeed` (same, CRLF) | RH3004-008 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAStringThatHoldsALineStartingWithHashBehindWhitespace` (`#b` behind four spaces: "first character other than whitespace") | RH3004-008 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixKeepsAStringWhoseLineHoldsHashBehindOtherTextInItsColumns` (spec example `@"a⏎b #c"`, exact output) | RH3004-006, RH3004-008 | Verify | passes |
| `….FixKeepsAStringWhoseLineHoldsHashBehindOtherTextInItsColumnsWithCarriageReturnLineFeed` | RH3004-008 | ApplyCodeFixAsync (CRLF) | passes |
| `….FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody` (`return @"a⏎#b";`, expression-body path stays fixed) | RH3004-003, RH3004-008 | Verify | passes |
| `….FixWithheldForAChainOpenedBeforeTheAnonymousMethodThatEndsInsideItsBlock` (spec example, audit 18 input 1, symbol `A`) | RH3004-009 | VerifyNoCodeFixIsOffered (symbol `A`) | fails: fix offered |
| `….FixWithheldForABlockThatHoldsOnlyTheEndifOfAChainOpenedBeforeTheAnonymousMethod` (spec example, audit 18 input 2, no symbol) | RH3004-009 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAChainOpenedBeforeTheAnonymousMethodWhoseElseLiesInAStringOfTheBlock` (spec example, audit 17(a), `#define A`) | RH3004-008, RH3004-009 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBrace` (`#if A` before `}`, `#else`/`#endif` after `};`, no symbol) | RH3004-009 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAChainOpenedInsideTheBlockThatEndsAfterItsClosingBraceWithTheFirstBranchActive` (same, symbol `A`) | RH3004-009 | VerifyNoCodeFixIsOffered (symbol `A`) | fails: fix offered |
| `….FixRewritesAnAnonymousMethodInsideAChainThatLiesWhollyOutsideIt` (spec example, `#define A`, exact output) | RH3004-009 | Verify | passes |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodsInImplicitlyTypedArraysWhoseElementIsInvokedInTheSameExpressionAreReportedWithoutACodeFix` (spec example, the three inputs verbatim) | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementResultIsAssignedIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedAsAStatementIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughInvokeIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodsInImplicitlyTypedArraysUsedWithoutInvokingAnElementAreNotReported` (spec example: `.Length`, `[0].Method`, `[0] is Action`) | RH3106-003 | Verify (analyzer) | passes |

The RH3106 tests characterize today's behavior, which row 45 keeps: they pass on this head, and the implementer must keep them green. The RH3004 tests that pass are the "offered" sides and must stay green after the narrowing.

The two "ends after its closing brace" tests are not spec examples; they cover the other half of RH3004-009's normative sentence ("before `delegate` or after the block's closing brace"), which the spec examples only exercise on the `delegate` side. Both inputs compile with and without `A` (the diagnostic check passes in each configuration).

## Existing tests the narrowing contradicts

None. I read every RH3004 test with a conditional directive (`rg -n '^ *#(if|elif|else|endif)'` on the class): every chain lies wholly inside the block or wholly in the anonymous method's head gap (RH3004-004 tests, already withheld), and no test string holds a line that starts with `#`. RH3106: no existing test invokes an array element in the expression that creates the array (`rg -n '\[0\]|Invoke\(\)'`: only locals `h[0]()`, `z[0]()`, `arr[0]()`).

## Not covered, and why

- RH3004-008 item 1 with an interpolated verbatim string or a `#` line inside a hole: the requirement decides on "a line that starts inside the string as RH3004-006 defines it", whose string span the row 41 tests already pin; one shape per side of the new `#` condition suffices for the narrowing.
- Fix All for RH3004: each new case has one diagnostic whose fix is withheld; Fix All runs the same provider registration, so a withheld single fix is withheld in Fix All too (the existing Fix All scenario still passes).
- RH3106 CRLF and Fix All: no code changes in row 45; the cases are reported with no fix, so neither line endings nor Fix All have an output to check.
- Failure classes: FC-003 (both sides: `#` at line start vs behind text; chain crossing vs wholly outside; block path vs expression-body path), FC-016 (sibling shapes: `#` behind whitespace, chain closing after `}`), FC-022 (string value), FC-033 (CRLF) apply and are covered. No formatter phase, rename, token gap, or Fix All provider changes: FC-008, FC-010–FC-014, FC-018/036, FC-029 cannot reach it.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

Result on b03a20cc plus these tests: analyzer 9 failed / 171 passed — exactly the nine tests marked "fails" above. Architecture 115 passed (`EveryRequirementOfAnEnforcedSpecIsCovered` now passes with the RH3004-009 tags).
