Role: test implementer
Commit: 641f087c
Verdict: #1257: reproduced; #1279: reproduced; #1281: reproduced; #1282: reproduced; #1417: reproduced; #1422: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict; there was nothing to set aside. Expected values come from the spec examples and requirements on 641f087c; layout expectations for RH3004-006 were additionally checked against `scripts/format.sh` (unchanged production formatter, the oracle SHARED-FIX-007 names), never against a patched fix.

Test classes are in `src/Reihitsu.Analyzer.Test/` (`Clarity/`, `Design/`, `Formatting/`). Class names below drop the `AnalyzerTests` suffix. "Fails" means it fails on this head for the reason the requirement states (fix offered, diagnostic reported, or the fix output differs only on the withheld/laid-out line); every failure was inspected. Each "no code fix" test also proves its input compiles: `VerifyNoCodeFixIsOffered` runs the analyzer verifier, which reports compiler errors.

Fix All: wherever instances can co-occur, a `FixAll…` test runs the verifier's single, iterative, and Fix All passes over a document with a withheld instance and fixable ones; the expected document keeps the withheld instance's markup.

## thoenissen/Reihitsu#1257

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsed.NegationAfterGreaterThanInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationAfterGreaterThanInAComparisonChainThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationStartingASwitchAfterGreaterThanInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationAfterShiftRightInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationAfterUnsignedShiftRightInAnArgumentThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationOfAParenthesizedOperandThatWouldReadAsAGenericNameIsReportedWithoutACodeFix` | RH3001-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3001NotOperatorShouldNotBeUsed.NegationAfterGreaterThanWithoutALessThanInFrontIsFixed` | RH3001-007 | Verify (code fix) | passes |
| `RH3001NotOperatorShouldNotBeUsed.FixAllLeavesANegationThatWouldReadAsAGenericNameReported` | RH3001-007 | Verify (code fix) | fails |
| `RH3005UseReadableConditions.ConstantLeftArgumentBeforeAGreaterThanArgumentIsReportedWithoutACodeFix` | RH3005-008 | VerifyNoCodeFixIsOffered | fails |
| `RH3005UseReadableConditions.StaticReadonlyLeftElementBeforeAGreaterThanElementIsReportedWithoutACodeFix` | RH3005-008 | VerifyNoCodeFixIsOffered | fails |
| `RH3005UseReadableConditions.LiteralLeftArgumentAfterALessThanArgumentIsReportedWithoutACodeFix` | RH3005-008 | VerifyNoCodeFixIsOffered | fails |
| `RH3005UseReadableConditions.ConstantLeftArgumentWhoseSwapStillReadsAsAComparisonIsFixed` | RH3005-008 | Verify (code fix) | passes |
| `RH3005UseReadableConditions.FixAllLeavesAComparisonThatWouldReadAsAGenericNameReported` | RH3005-008 | Verify (code fix) | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.BaseOperandAfterLessThanBeforeAGreaterThanArgumentIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.BaseArgumentBetweenLessThanAndGreaterThanArgumentsIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.BaseOperandBeforeGreaterThanAfterALessThanArgumentIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.ParenthesizedBaseAccessFollowedByAnArgumentListIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.ParenthesizedBaseAccessFollowedByANullForgivingOperatorIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.ParenthesizedBaseAccessFollowedByWithIsReportedWithoutACodeFix` | RH3101-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.ParenthesizedBaseAccessFollowedByABinaryOperatorIsFixed` | RH3101-004, RH3101-005 | Verify (code fix) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.FixAllLeavesAnAccessThatWouldReadAsACastReported` | RH3101-005 | Verify (code fix) | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.QualifiedOperandAfterLessThanBeforeAGreaterThanArgumentIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.QualifiedOperandBeforeGreaterThanAfterALessThanArgumentIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.QualifiedArgumentBetweenLessThanAndGreaterThanArgumentsIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.QualifiedOperandInAComparisonChainIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.ParenthesizedThisAccessFollowedByAnArgumentListIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.ParenthesizedThisAccessFollowedByANullForgivingOperatorIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.ParenthesizedThisAccessFollowedByWithIsReportedWithoutACodeFix` | RH3105-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.ParenthesizedThisAccessFollowedByABinaryOperatorIsFixed` | RH3105-004, RH3105-005 | Verify (code fix) | passes |
| `RH3105DoNotPrefixLocalMembersWithThis.FixAllLeavesAnAccessThatWouldReadAsAGenericNameReported` | RH3105-005 | Verify (code fix) | fails |

The seven `…WritesAGenericReading` characterization tests in RH3001 (4), RH3005 (3, incl. `(7) < B`), RH3101 (2) and RH3105 (4) were green on this head and pinned the reported broken output; they were rewritten in place to "reported, no code fix" and retagged.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3001 `M(G < A, B > !x switch {…})` (issue's valid form, decision 8), `>`/`>>`/`>>>`, chain, `!(x)` | LF | code fix, single and Fix All | reproduced | observed: fix offered, writes `B > (x == false) switch …` (generic `G<A, B>`); expected: no fix (RH3001-007) |
| RH3005 `M(Max > G, B > (7))`, array element, `(7) < B` | LF | code fix, single and Fix All | reproduced | observed: `M(G < Max, B > (7))`; expected: no fix (RH3005-008) |
| RH3101 `M(A < base.B, D > (7))`, `base.X` between, `base.D > (7)`; cast reading `(base.A)(5)`, `(base.S)!.Length`, `(base.Q) with {…}` | LF | code fix, single and Fix All | reproduced | observed: fix offered; expected: no fix (RH3101-005) |
| RH3105 same four generic shapes; `(this.A)(5)`, `(this.S)!.Length`, `(this.Q) with {…}` | LF | code fix, single and Fix All | reproduced | observed: fix offered; expected: no fix (RH3105-005) |

Line endings play no part: the withheld decision changes no layout, and the rewrite is single-line.

### Not covered, and why

- The `&` follower of the generic reading (`M(G < Max, p > &x)`): no compiling input exists (the skeptic could not build one either); a test whose input does not compile is no test.
- Counter-examples cover the holding side of each condition: `M(G > A, B > !x switch …)` fixed (RH3001-007), `M(Max > G, B > 7)` fixed (RH3005-008), `(base.N) - 1` / `(this.N) - 1` fixed to `(N) - 1` (RH3101-005, RH3105-005).

## thoenissen/Reihitsu#1279

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.AnonymousMethodWithBlockBodyIsConverted` | RH3004-003, RH3004-006 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.CommentInsideSingleReturnBlockIsPreserved` | RH3004-003, RH3004-006 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.DocumentationCommentedAnonymousMethodKeepsItsBody` | RH3004-006 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.FixWritesAnExpressionBodyOnlyForASingleReturnStatement` | RH3004-003, RH3004-006 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.CastOperandIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.LeftBinaryOperandIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.RightBinaryOperandIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.MiddleBinaryOperandIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.RightOperandOfNullCoalescingIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.LeftBinaryOperandWithBlockBodyIsReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.CastAndBinaryOperandsAreReportedWithoutACodeFix` | RH3004-005 | VerifyNoCodeFixIsOffered | fails |
| `RH3004UseLambdaSyntax.ParenthesizedCastOperandAndCompoundAssignmentValueAreFixed` | RH3004-003, RH3004-005 | Verify (code fix) | passes |
| `RH3004UseLambdaSyntax.ConditionalBranchIsFixed` | RH3004-003, RH3004-005 | Verify (code fix) | passes |
| `RH3004UseLambdaSyntax.FixAllLeavesABinaryOperandReported` | RH3004-005 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.FixLaysOutABlockOnItsOwnLinesAsTheFormatter` | RH3004-003, RH3004-006 | Verify (code fix) | fails |
| `RH3004UseLambdaSyntax.FixLaysOutABlockOnItsOwnLinesAsTheFormatterWithCarriageReturnLineFeed` | RH3004-006 | ApplyCodeFixAsync | fails |
| `RH3004UseLambdaSyntax.FixOfAOneLineBlockMatchesTheFormatter` | RH3004-006 | ApplyCodeFixAsync | fails |

Changed existing tests: `AnonymousMethodWithBlockBodyIsConverted`, `CommentInsideSingleReturnBlockIsPreserved`, `DocumentationCommentedAnonymousMethodKeepsItsBody`, `FixWritesAnExpressionBodyOnlyForASingleReturnStatement` now expect the formatter layout (RH3004-003 example, RH3004-006); the four `FixOf…Operand…WritesTheLambdaWithoutParentheses` characterizations became "no code fix" tests (RH3004-005). `FixOfStaticAnonymousMethodDropsTheStaticModifier` is #1436 and stays untouched.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| cast operand `(Func<int, int>)delegate (int x) { … }` | LF | code fix | reproduced | observed: `(Func<int, int>)(int x) => x + 1`; expected: no fix |
| left operand `delegate () { return 1; } + h`, right `g + delegate () { }`, `g ?? delegate () { }`, middle, left block body `delegate () { } + g` | LF | code fix, single and Fix All | reproduced | observed: fix offered; expected: no fix |
| `= delegate (int x)` with `{` on the next line | LF, CRLF | code fix | reproduced | observed: `(int x) => {` with the block at its old columns; expected: `{` on its own line under `(` |
| one-line block `async delegate () { await Task.Delay(1); }` | LF | code fix + formatter | reproduced | observed: one line, the formatter changes it; expected: formatting the result changes nothing |

### Not covered, and why

- Holding sides covered: parenthesized cast operand, compound assignment `+=`, conditional branch (catches a guard on "any expression parent", FC-001).
- `is`/`as` operand, unary operand, member access/invocation of the anonymous method: their inputs do not compile (skeptic matrix).

## thoenissen/Reihitsu#1281

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3005UseReadableConditions.ComparisonWithAComparisonOperandThatWouldRegroupIsReportedWithoutACodeFix` | RH3005-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3005UseReadableConditions.ComparisonWithAnInequalityOperandThatWouldRegroupIsReportedWithoutACodeFix` | RH3005-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3005UseReadableConditions.ComparisonWithAComparisonOperandThatKeepsItsGroupingIsFixed` | RH3005-007 | Verify (code fix) | passes |
| `RH3005UseReadableConditions.ComparisonWithARelationalLeftOperandThatKeepsItsGroupingIsFixed` | RH3005-007 | Verify (code fix) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.VirtualMethodInUnsealedTypeIsReportedWithoutACodeFix` | RH3101-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.VirtualMethodInSealedTypeIsFixed` | RH3101-004, RH3101-007 | Verify (code fix) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.UnsealedOverrideInUnsealedTypeIsReportedWithoutACodeFix` | RH3101-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.SealedOverrideInUnsealedTypeIsFixed` | RH3101-004, RH3101-007 | Verify (code fix) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.VirtualPropertyAndIndexerInUnsealedTypeAreReportedWithoutACodeFix` | RH3101-007 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.VirtualIndexerInSealedTypeIsFixed` | RH3101-004, RH3101-007 | Verify (code fix) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.InheritedOverrideInStructIsFixed` | RH3101-004, RH3101-007 | Verify (code fix) | passes |
| `RH3201CommentsMustContainText.CommentBetweenKeywordAndIdentifierIsReportedWithoutACodeFix` | RH3201-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3201CommentsMustContainText.CommentSpanningSeveralLinesBetweenKeywordAndIdentifierIsReportedWithoutACodeFix` | RH3201-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3201CommentsMustContainText.AdjacentCommentsBetweenKeywordAndIdentifierAreReportedWithoutACodeFix` | RH3201-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3201CommentsMustContainText.FixAllLeavesAdjacentCommentsBetweenTwoTokensReported` | RH3201-006 | Verify (code fix) | fails |
| `RH3201CommentsMustContainText.CommentBetweenTypeKeywordAndIdentifierIsReportedWithoutACodeFix` | RH3201-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3201CommentsMustContainText.CommentBetweenTwoOperatorsThatWouldMergeIsReportedWithoutACodeFix` | RH3201-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3201CommentsMustContainText.CommentBetweenTokensThatDoNotMergeIsRemoved` | RH3201-006 | Verify (code fix) | passes |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToOverloadsIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToAConstructorInitializerOrAnInferredGenericMethodIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodWhoseCallWouldBindToAnotherOverloadIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToAnObjectCreationWithOverloadsIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToATargetTypedCreationOrAnElementAccessWithOverloadsIsReportedWithoutACodeFix` | RH3106-002 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToASingleOverloadIsFixed` | RH3106-002 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodPassedToOverloadsThatStillBindTheSameIsFixed` | RH3106-002 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodInImplicitlyTypedVariableIsNotReported` | RH3106-003 | Verify (analyzer) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodsThatTakeTheirNaturalTypeAreNotReported` | RH3106-003 | Verify (analyzer) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodAssignedToObjectOrCastToDelegateIsNotReported` | RH3106-003 | Verify (analyzer) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodWithADelegateTargetThroughAConditionalIsReported` | RH3106-001, RH3106-003 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodAssignedOrReturnedAsADelegateIsReported` | RH3106-001 | Verify (analyzer) | passes |

Rewritten characterizations: RH3005 `FixOfComparisonWithComparisonOperandWritesTheSwapWithoutParentheses`, RH3101 `FixOfVirtualMethodInUnsealedTypeRemovesTheQualifier`, RH3106 `FixOfAnonymousMethodPassedToOverloadsRemovesTheParameterList` and `FixOfAnonymousMethodInImplicitlyTypedVariableRemovesTheParameterList` (now "not reported", RH3106-003), RH3201 `FixOfCommentBetweenKeywordAndIdentifierJoinsTheTokens`.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3005 `1 == 1 == b`, `1 != 2 == b` | LF | code fix | reproduced | observed: `b == 1 == 1` (CS0019); expected: no fix (RH3005-007) |
| RH3101 `base.M()` virtual / unsealed override / virtual property and indexer, unsealed type | LF | code fix | reproduced | observed: qualifier removed; expected: no fix (RH3101-007) |
| RH3106 overloads, constructor initializer, `Run<T>(T)`, `Action<int>` + `object`, `new Thread(…)`, target-typed `new(…)`, element access | LF | code fix | reproduced | observed: fix offered; expected: no fix (RH3106-002) |
| RH3106 `var`, `Delegate`, `object`, `(Delegate)` cast, return as `object`, sole `Run(object)` | LF | analyzer | reproduced | observed: reported; expected: not reported (RH3106-003) |
| RH3201 `return/**/a`, multi-line `/*⏎*/`, two adjacent comments, `int/**/x`, `a+/**/+b` | LF | code fix, single and Fix All | reproduced | observed: `returna`, `intx`, `a++b`; expected: no fix (RH3201-006) |

### Not covered, and why

- Holding sides covered: `true == a < b`, `1 < 2 == b` (RH3005-007); sealed type, sealed override, struct, sealed indexer (RH3101-007); single overload, overloads that bind the same (RH3106-002); delegate target through a conditional, assigned/returned as `Action` (RH3106-001/003); `a+/**/b` (RH3201-006).
- RH3201-006's other holding side, "whitespace also separates the two tokens": not tested, see *Whole run*, finding 1.

## thoenissen/Reihitsu#1282

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.CommentsAroundTheDotAreReportedWithoutACodeFix` | RH3101-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.CommentInFrontOfTheDotIsReportedWithoutACodeFix` | RH3101-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.CommentBehindTheDotIsReportedWithoutACodeFix` | RH3101-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.DirectiveBetweenBaseAndTheMemberNameIsReportedWithoutACodeFix` | RH3101-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExists.CommentInAnElementAccessStaysBehindThis` | RH3101-006 | Verify (code fix) | passes |
| `RH3104DoNotUseDefaultValueTypeConstructor.CommentInsideTheArgumentListIsReportedWithoutACodeFix` | RH3104-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CommentBetweenNewAndTheTypeIsReportedWithoutACodeFix` | RH3104-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CommentInsideATargetTypedCreationIsReportedWithoutACodeFix` | RH3104-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.DirectiveInsideTheCreationIsReportedWithoutACodeFix` | RH3104-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CommentsAroundTheCreationDoNotWithholdTheFix` | RH3104-004, RH3104-006 | Verify (code fix) | passes |
| `RH3104DoNotUseDefaultValueTypeConstructor.FixAllLeavesACreationWithACommentInsideReported` | RH3104-006 | Verify (code fix) | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.CommentsAroundTheDotAreReportedWithoutACodeFix` | RH3105-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.CommentInFrontOfTheDotIsReportedWithoutACodeFix` | RH3105-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.CommentBehindTheDotIsReportedWithoutACodeFix` | RH3105-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.DirectiveBetweenThisAndTheMemberNameIsReportedWithoutACodeFix` | RH3105-006 | VerifyNoCodeFixIsOffered | fails |
| `RH3105DoNotPrefixLocalMembersWithThis.CommentsOutsideTheAccessDoNotWithholdTheFix` | RH3105-004, RH3105-006 | Verify (code fix) | passes |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.CommentInsideTheParameterListIsReportedWithoutACodeFix` | SHARED-PARENS-003 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.DirectiveInsideTheParameterListIsReportedWithoutACodeFix` | SHARED-PARENS-003 | VerifyNoCodeFixIsOffered | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.CommentInFrontOfTheParameterListDoesNotWithholdTheFix` | SHARED-PARENS-002, SHARED-PARENS-003 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.FixAllLeavesAParameterListWithACommentInsideReported` | SHARED-PARENS-003 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.FixLeavesOneSpaceBetweenDelegateAndTheBody` | SHARED-PARENS-002 | Verify (code fix) | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.FixLeavesOneSpaceBetweenDelegateAndTheBodyWithCarriageReturnLineFeed` | SHARED-PARENS-002 | ApplyCodeFixAsync | fails |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.FixKeepsTheLineBreakInFrontOfABodyOnTheNextLine` | SHARED-PARENS-002 | Verify (code fix) | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.CommentInsideTheArgumentListIsReportedWithoutACodeFix` | SHARED-PARENS-003 | VerifyNoCodeFixIsOffered | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.DirectiveInsideTheArgumentListIsReportedWithoutACodeFix` | SHARED-PARENS-003 | VerifyNoCodeFixIsOffered | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.FixAllLeavesAnArgumentListWithACommentInsideReported` | SHARED-PARENS-003 | Verify (code fix) | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.FixLeavesOneSpaceBetweenTheCommentsOnBothSidesOfTheArgumentList` | SHARED-PARENS-002 | Verify (code fix) | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.FixLeavesNoSpaceInFrontOfTheClosingBracket` | SHARED-PARENS-002 | Verify (code fix) | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.FixLeavesNoSpaceInFrontOfTheComma` | SHARED-PARENS-002 | Verify (code fix) | fails |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemoved.FixLeavesNoSpaceInFrontOfTheClosingBracketWithCarriageReturnLineFeed` | SHARED-PARENS-002 | ApplyCodeFixAsync | fails |

Rewritten characterizations: RH3101 and RH3105 `FixRemovesCommentsAroundTheDotWithTheQualifier`, RH3104 `FixRemovesCommentsInsideTheCreation` (split into the argument-list and `new /* x */ int()` cases), RH3106 `FixRemovesACommentInsideTheParameterList` and `FixKeepsTheWhitespaceOnBothSidesOfTheParameterList`, RH3107 `FixRemovesACommentInsideTheArgumentList` and `FixKeepsTheWhitespaceOnBothSidesOfTheArgumentList` (the last two now expect SHARED-PARENS-002's whitespace, lead decision 7).

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| comment / end-of-line comment / directive between `base`/`this` and the member name | LF | code fix | reproduced | observed: `M()`, `_a`, comments deleted; expected: no fix |
| `new int(/* x */)`, `new /* x */ int()`, `new(/* x */)`, directive in the argument list | LF | code fix, single and Fix All | reproduced | observed: `default(int)`; expected: no fix |
| `delegate ( /* none */ ) { }`, directive inside; `[Obsolete(/* none */)]`, directive inside | LF | code fix, single and Fix All | reproduced | observed: list removed with its trivia; expected: no fix (SHARED-PARENS-003) |
| whitespace left by the removal: `delegate () { }`, `[Obsolete /* a */ ()]`, `[Obsolete (), Serializable]`, `/* a */ ( ) /* b */`, `delegate ()⏎{` | LF, CRLF | code fix | reproduced | observed: `delegate  { }`, `[Obsolete /* a */ ]`, `[Obsolete , Serializable]`, `/* a */  /* b */`, `delegate ⏎`; expected: one space / no space / no trailing space (SHARED-PARENS-002) |

### Not covered, and why

- Holding sides covered: comment in a `base` element access stays behind `this`; comments outside the access/creation kept; comment in front of `(` kept for RH3106 (RH3107 already had `FixRemovesTheEmptyArgumentListAndKeepsACommentInFrontOfIt`).

## thoenissen/Reihitsu#1417

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3103UseShorthandForNullableTypes.UnboundNullableGenericInTypeofIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericQualifyingAMemberInNameofIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericReceivingAStaticCallIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericInUsingAliasIsReportedAndFixedFromCSharp12` | RH3103-001, RH3103-004, RH3103-007 | Verify (code fix) | passes |
| `RH3103UseShorthandForNullableTypes.NullableGenericInUsingAliasIsNotReportedBeforeCSharp12` | RH3103-001, RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericAsTypeArgumentInUsingAliasIsReportedAndFixedBeforeCSharp12` | RH3103-001, RH3103-007 | Verify (code fix) | passes |
| `RH3103UseShorthandForNullableTypes.NullableGenericInUsingStaticIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericAsTypeArgumentInUsingStaticIsReportedAndFixed` | RH3103-001, RH3103-007 | Verify (code fix) | passes |
| `RH3103UseShorthandForNullableTypes.DottedNullableGenericAsNameofArgumentIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.GlobalDottedNullableGenericAsNameofArgumentIsNotReported` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.PositionsWithoutTheShorthandAreNotReportedBeforeCSharp12` | RH3103-007 | Verify (analyzer) | fails |
| `RH3103UseShorthandForNullableTypes.OnlyTheUsingAliasIsReportedFromCSharp12` | RH3103-007 | Verify (code fix) | fails |
| `RH3103UseShorthandForNullableTypes.NullableGenericInIsAsAndCastIsReportedAndFixed` | RH3103-001, RH3103-007 | Verify (code fix) | passes |

Rewritten: `FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly`, `FixOfNullableGenericQualifyingAMemberInNameofWritesTheShorthand`, `FixOfNullableGenericReceivingAStaticCallWritesTheShorthand` (now also dotted and `global::`), `DottedNullableGenericAsNameofArgumentDoesNotOfferFix` and `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix` (retagged from retired RH3103-006 to RH3103-007, now "not reported"), `FixOfNullableGenericInUsingAliasWritesTheShorthand` (now pinned to C# 12, tagged RH3103-004). A private `ApplyLanguageVersion` helper was added to the class.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `typeof(Nullable<>)`, `nameof(Nullable<int>.HasValue)`, `Nullable<int>.Equals(1, 2)` (3 spellings), dotted/`global::` `nameof` argument, `using static System.Nullable<int>;` | LF | analyzer | reproduced | observed: reported; expected: not reported |
| `using N = System.Nullable<int>;` at C# 11 | LF | analyzer | reproduced | observed: reported; expected: not reported |
| spec example at C# 12 | LF | analyzer + code fix | reproduced | observed: 8 diagnostics; expected: only the alias, fixed to `using N = int?;` |

### Not covered, and why

- Holding sides covered: alias at C# 12 fixed; a type argument inside an alias (C# 11) and inside `using static` reported and fixed; `is`/`as`/cast reported and fixed (catches a guard that skips every non-declaration position, FC-001). Type arguments inside `nameof` are already covered by `NullableGenericNestedInNameofIsReportedAndFixed`.

## thoenissen/Reihitsu#1422

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationsThatStandAsStatementsAreNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationsWhoseValueIsUsedAreReportedAndFixed` | RH3104-001, RH3104-005 | Verify (code fix) | passes |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationsAsExpressionBodiesOfFunctionsThatReturnAValueAreReported` | RH3104-001, RH3104-005 | Verify (analyzer) | passes |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionStatementIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsForInitializerAndIteratorIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfVoidMethodIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfVoidLocalFunctionIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfActionLambdaIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfAsyncTaskLambdaIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfAsyncValueTaskMethodIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfAsyncVoidMethodIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfConstructorIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfFinalizerIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfSetAndInitAccessorsIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH3104DoNotUseDefaultValueTypeConstructor.CreationAsExpressionBodyOfEventAccessorsIsNotReported` | RH3104-005 | Verify (analyzer) | fails |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.LambdaBodyInsideAnInterpolationIsNotReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | fails |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.AnonymousMethodBodyInsideAnInterpolationIsNotReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | fails |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.BodyInsideAVerbatimInterpolationNestedInAnInterpolationIsNotReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | fails |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.BodyInsideAnInterpolationNestedInAVerbatimInterpolationIsNotReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | fails |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.LambdaBodyInsideAnInterpolationIsReportedFromCSharp11` | RH5417-005 | Verify (analyzer) | passes |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.BodyInsideAVerbatimInterpolationNestedInAnInterpolationIsReportedFromCSharp11` | RH5417-005 | Verify (analyzer) | passes |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.LambdaBodyInsideAVerbatimInterpolationIsReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | passes |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLine.LambdaBodyInsideAnAtDollarVerbatimInterpolationIsReportedBeforeCSharp11` | RH5417-005 | Verify (analyzer) | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3104 `new S();` and every statement-expression position RH3104-005 names | LF | analyzer | reproduced | observed: reported (fix writes `default(S);`, CS0201); expected: not reported |
| RH5417 lambda / anonymous-method body in a `$"…"` hole, and in `$@` inside `$` / `$` inside `$@`, C# 10 | LF | analyzer | reproduced | observed: reported; expected: not reported |

### Not covered, and why

- Holding sides covered: discard, `Func<S>` lambda, `return`, `get` accessor, expression-bodied property, `async` `Func<Task<S>>` lambda (RH3104); C# 11 for `$"…"` and the nested form, `$@` and `@$` at C# 10 (RH5417).
- RH5417 code-fix path: RH5417 offers no fix for lambda and anonymous-method bodies (RH5417-004), the only bodies that can stand in an interpolation.

## Whole run

### Findings for the lead

1. **RH3201: the whitespace carve-out of RH3201-006 leaves a token-merging fix specified as allowed.** `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens` (unchanged, green) pins `return /**/a;` → `returna;`. RH3201-006 says it does not withhold "where whitespace or a line break also separates the two tokens", and RH3201-005 covers no comment followed directly by code, so no requirement forbids today's output while SHARED-FIX-003 does. Same class as RH3201-006 (token merge). I did not invent an expected output; the spec needs to decide (withhold, or keep a separator). Until then the characterization stays as it is.
2. **RH3004-006 and doc comments.** `DocumentationCommentedAnonymousMethodKeepsItsBody` expects the braces laid out as the formatter does but the `/// <summary>Result of the callback.</summary>` line unchanged. The formatter would also split that doc comment into three lines; RH3004-003 keeps "the block's statements, unchanged", and FC-030 forbids formatting more than the fix changes. If the implementer formats the whole lambda through the formatter, this test shows whether the documentation phase leaks in.
3. **SHARED-PARENS-002 tests follow the spec's tokens, not whole-formatter layout.** The formatter would also break `delegate { }` onto three lines and split `[Obsolete, Serializable]` into two lists; the spec example writes `delegate { }` and the tests assert that (only the whitespace the removal leaves is laid out).
4. `VerifyNoCodeFixIsOffered` asks the code fix at `LanguageVersion.Latest`; the language-version tests (RH3103, RH5417) are analyzer-only where nothing is reported, and use `Verify(…, onConfigure)` where something is.

### Failure classes

- Applied: FC-001 (holding conditional/`is`/`as`/struct cases), FC-003 (both sides of every new condition: generic vs. comparison reading, sealed vs. unsealed, C# 10/11, C# 11/12, natural type vs. delegate target, comments inside vs. outside), FC-004/FC-005/FC-019/FC-020 (comment and directive inside each withheld span), FC-006 (every withheld rewrite), FC-008 (Fix All with withheld and fixable instances, adjacent RH3201 comments), FC-016/FC-027 (sibling shapes per skeptic matrix), FC-017 (exact outputs), FC-022 (verbatim and nested interpolated strings), FC-025 (expression-bodied constructor, finalizer, accessors, `async` `Task`/`ValueTask`/`void`), FC-033 (CRLF where layout is involved: RH3004-006, SHARED-PARENS-002), FC-034 (the issues' own shapes, `switch` form for #1257 per decision 8).
- Not applicable: FC-010–FC-014, FC-028, FC-029 (no formatter phase changes); FC-018, FC-036 (no rename); FC-023, FC-024 (identifier shapes and `partial` do not reach these decisions); FC-007 (each withheld case leaves no edited span; fixed cases verified through the verifier's one-pass re-analysis); FC-009 is exactly what the "no code fix" tests assert; FC-031, FC-032, FC-035, FC-037 concern later roles.

### Run state

Full analyzer project on this head with these tests: 116 failed, 6709 passed (6825 total). All 116 failures are tests listed above as "fails"; no test outside the eleven touched classes fails. `scripts/format.sh` over the eleven files changed nothing.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3001NotOperatorShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3005UseReadableConditionsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests|FullyQualifiedName~RH3103UseShorthandForNullableTypesAnalyzerTests|FullyQualifiedName~RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests|FullyQualifiedName~RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests|FullyQualifiedName~RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests|FullyQualifiedName~RH3201CommentsMustContainTextAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests"
```

## Amendment — RH3201-007

Commit: 5ee90bf2 (spec); tests written on that head.
Verdict: tests written

My prompt named the change asked for (bring `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens` in line with RH3201-007); that is the work, not a hoped-for verdict, and expected values come from RH3201-007's example and qualifiers only. Maintainer answer 5 (A) settles *Whole run*, finding 1.

### Tests

All in `src/Reihitsu.Analyzer.Test/Clarity/RH3201CommentsMustContainTextAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixOfCommentDirectlyInFrontOfAnIdentifierKeepsTheWhitespaceInFrontOfIt` (was `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens`, a characterization pinning `returna;`) | RH3201-007 | Verify (code fix) | fails: `returna;` instead of `return a;` |
| `FixOfCommentDirectlyInFrontOfAnIdentifierKeepsTheTabInFrontOfIt` | RH3201-007 | Verify (code fix) | fails: `returna;` instead of `return<tab>a;` |
| `FixOfCommentDirectlyInFrontOfAnOperatorKeepsTheWhitespaceInFrontOfIt` | RH3201-007 | Verify (code fix) | fails: `a ++b` instead of `a + +b` |
| `FixOfCommentSpanningSeveralLinesDirectlyInFrontOfAnIdentifierKeepsTheWhitespaceInFrontOfIt` | RH3201-007 | Verify (code fix) | fails: `returna;` instead of `return a;` |
| `FixOfCommentWithWhitespaceOnBothSidesRemovesTheWhitespaceInFrontOfIt` | RH3201-005, RH3201-007 | Verify (code fix) | passes |
| `FixAllKeepsTheWhitespaceInFrontOfACommentDirectlyInFrontOfAToken` | RH3201-007 | Verify (code fix, single, iterative, Fix All) | fails: `returna;` and `a ++b` |

Conditions of RH3201-007, both sides:

- "separated by spaces or tabs": space (example) and tab tests; the other side, no whitespace in front, is RH3201-006 and stays covered by `CommentBetweenKeywordAndIdentifierIsReportedWithoutACodeFix` (withheld) and `CommentBetweenTokensThatDoNotMergeIsRemoved` (`a+/**/b` → `a+b`), both unchanged and not retagged.
- "the next token follows the comment directly": the holding side above; the other side, whitespace behind the comment, is `FixOfCommentWithWhitespaceOnBothSidesRemovesTheWhitespaceInFrontOfIt` (`return /**/ a;` → `return a;`, RH3201-005's removal) and the existing `FixRemovesTheCommentLineOrTheWhitespaceInFrontOfTheComment`.
- "code precedes the comment on its line": the holding side above; a comment at the start of a line directly followed by code is in neither RH3201-005 nor RH3201-007 (follow-up candidate already listed in `00-lead.md`), so no expected output exists and no test was written.

### Not covered, and why

- `return /**//**/a;` (whitespace in front of the first of two adjacent comments): whether "the next token follows the comment directly" holds for the first comment is not decided by the requirement; no expected output, no test. For the lead: RH3201-007 may need a sentence on adjacent comments, as RH3201-006 has.
- CRLF: the fix keeps or removes only spaces and tabs within one line, and the multi-line comment test's removal is the comment's own text; no line ending is written or moved. FC-033 does not reach it.
- FC-008: Fix All with several instances is covered by the Fix All test above.

### Run state

`RH3201CommentsMustContainTextAnalyzerTests` on this head: 11 failed, 28 passed. The 11 failures are the five "fails" rows above and the six RH3201-006 tests listed under #1281 as "fails"; each new failure was inspected and differs only in the joined tokens. `scripts/format.sh` changed nothing.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3201CommentsMustContainTextAnalyzerTests"
```

## Amendment — rows 10 and 13

Commit: f0421490 (head the tests were written on)
Verdict: tests written

My prompt named the two changes of `00-lead.md` rows 10 and 13; it carried no intended fix, suspected cause, or hoped-for verdict to set aside.

### Row 10 — markup configuration of the nine `FixAll…Reported` tests

Checked on f0421490 before changing anything: all nine failed with `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"` (`"2"` for RH3201), as `05-implementer.md` states. Each test now passes the configuration the repository's existing tests use for a withheld diagnostic that stays marked in the fixed source (`RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests`, `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests`):

```csharp
test.FixedState.MarkupHandling = MarkupMode.Allow;
test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
```

No source, fixed source, markup, or tag changed; `using Microsoft.CodeAnalysis.Testing;` was added where the file lacked it.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001NotOperatorShouldNotBeUsedAnalyzerTests.FixAllLeavesANegationThatWouldReadAsAGenericNameReported` | unchanged | Verify (Fix All) | passes |
| `RH3004UseLambdaSyntaxAnalyzerTests.FixAllLeavesABinaryOperandReported` | unchanged | Verify (Fix All) | passes |
| `RH3005UseReadableConditionsAnalyzerTests.FixAllLeavesAComparisonThatWouldReadAsAGenericNameReported` | unchanged | Verify (Fix All) | passes |
| `RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests.FixAllLeavesAnAccessThatWouldReadAsACastReported` | unchanged | Verify (Fix All) | passes |
| `RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests.FixAllLeavesACreationWithACommentInsideReported` | unchanged | Verify (Fix All) | passes |
| `RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests.FixAllLeavesAnAccessThatWouldReadAsAGenericNameReported` | unchanged | Verify (Fix All) | passes |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests.FixAllLeavesAParameterListWithACommentInsideReported` | unchanged | Verify (Fix All) | passes |
| `RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests.FixAllLeavesAnArgumentListWithACommentInsideReported` | unchanged | Verify (Fix All) | passes |
| `RH3201CommentsMustContainTextAnalyzerTests.FixAllLeavesAdjacentCommentsBetweenTwoTokensReported` | unchanged | Verify (Fix All) | passes |

### Row 13 — `RewriteParseUtilities` unit tests

New file `src/Reihitsu.Core.Test/RewriteParseUtilitiesTests.cs`. Each rewrite is built as syntax nodes the way the consumers build theirs: `RemovePrefix` replaces a `this.`/`base.` member access by its name with the access's trivia (RH3101, RH3105), a swap builds a new `BinaryExpression` (RH3005), `RemoveComments` drops a multi-line comment from a token's trailing trivia (RH3201). No requirement tag: the requirements are the fixes', and these tests check the shared helper only.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ParsesAsWrittenReturnsTrueForUnchangedRoot` | — | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRemovedPrefixThatKeepsItsReading` | FC-003 (holding side) | direct call | passes |
| `ParsesAsWrittenReturnsFalseForRemovedPrefixThatReadsAsGenericName` (`F(a < this.b, c > (d))`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRemovedPrefixBetweenComparisonsThatStayComparisons` (`F(a < this.b, c > d)`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsFalseForSwappedComparisonThatReadsAsGenericName` (`F(b > a, c > (d))` → `F(a < b, c > (d))`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsFalseForRemovedPrefixThatReadsAsCast` (`(base.A)(x)`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRemovedPrefixInParenthesesFollowedByBinaryOperator` (`(base.A) + x`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsFalseForRemovedCommentThatMergesKeywordAndIdentifier` (`return/**/a`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsFalseForRemovedCommentThatMergesOperators` (`a+/**/+b`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRemovedCommentBetweenTokensThatDoNotMerge` (`a+/**/b`) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRemovedCommentWithWhitespaceOnBothSides` (trivia not compared) | FC-003 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRewriteOutsideConditionalDirectives` (the `#if` shape where `IsEquivalentTo` failed) | FC-005 | direct call | passes |
| `ParsesAsWrittenReturnsTrueForRewriteInBranchActiveThroughPreprocessorSymbol` (`DEBUG` defined in the parse options) | FC-005 | direct call | passes |
| `ParsesAsWrittenReturnsFalseForRewriteInConditionalBranchThatReadsAsGenericName` | FC-005 | direct call | passes |

Observation on the way, no finding: a first draft put the prefix on the left operand (`F(this.a < b, c > (d))`) and expected `false`; the check returned `true`. The input itself already parses as the generic member access `this.a<b, c>(d)`, so the rewrite keeps its reading and `true` is right. The draft was corrected to the consumer's shape (`a < this.b`), and a temporary RH3105 analyzer test of the same input (it did not compile: CS0307) was removed again.

### Not covered, and why

- Language version: no consumer's rewrite parses differently by language version in a way the tests could reach without also changing the tokens; the preprocessor-symbol test shows the original parse options are the ones used.
- CRLF: the check compares tokens and structure, not trivia; line endings never reach the comparison.

### Run state

`scripts/test.sh --project core`: Passed 621, Failed 0. The nine touched analyzer classes: Passed 287, Failed 0. `scripts/format.sh` changed nothing after the edits.

### Commands

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~RewriteParseUtilitiesTests"
scripts/test.sh --project analyzer --filter "Name~FixAllLeavesANegationThatWouldReadAsAGenericNameReported|Name~FixAllLeavesABinaryOperandReported|Name~FixAllLeavesAComparisonThatWouldReadAsAGenericNameReported|Name~FixAllLeavesAnAccessThatWouldReadAsACastReported|Name~FixAllLeavesACreationWithACommentInsideReported|Name~FixAllLeavesAnAccessThatWouldReadAsAGenericNameReported|Name~FixAllLeavesAParameterListWithACommentInsideReported|Name~FixAllLeavesAnArgumentListWithACommentInsideReported|Name~FixAllLeavesAdjacentCommentsBetweenTwoTokensReported"
```
