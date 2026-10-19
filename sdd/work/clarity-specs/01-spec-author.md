Role: spec author
Commit: 22a17fb
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Three points need the lead (see *Decisions needed*); none blocks the specs, so all 21 rules are written.

## What was written

- One spec per rule: `sdd/specs/analyzer/RH3001.md` … `RH3007.md`, `RH3101.md` … `RH3107.md`, `RH3201.md`, `RH3203.md` … `RH3208.md`.
- Two shared specs, each used by more than one rule spec:
  - `sdd/specs/shared/expression-body-conversion.md` (SHARED-BODY), used by RH3203, RH3205, RH3206, RH3207, RH3208. Code owners `ExpressionBodyAnalyzerBase<TNode>`, `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, `ExpressionBodyRewriteUtilities`.
  - `sdd/specs/shared/empty-parentheses.md` (SHARED-EMPTY), used by RH3106 and RH3107. Code owner `EmptyParenthesesAnalyzerBase<TNode>`.
- `sdd/specs/analyzer/RH3202.md`: only the header changed — `ExpressionBodyRewriteUtilities` moved from its code owners to SHARED-BODY (clarification (a) of `00-lead.md`). No RH3202 requirement was moved, changed, or retired.
- `sdd/specs/README.md`: 23 new rows; RH3202 row updated. `sdd/specs/requirement-ids.txt`: 77 IDs inserted in ordinal order (ledger was sorted before and after; no line removed or moved).
- `documentation/rules/RH3201.md`: the *Exceptions* sentence said an empty `//` between "two other comment lines" is not reported; the analyzer reports it when the neighbor is a `///` documentation comment (RH3201-002 example, verified). Now: "two other `//` comment lines", plus one sentence that a `///` neighbor does not count. No other rule page was wrong.
- No change doc: every requirement states current behavior.

## How the examples were verified

Every C# example with a fix or a negative case was extracted from the specs and run through `scripts/apply-fix.sh <rule> <dir> --max-iterations 30` (LF and CRLF). For before/after pairs, the runner's diff was applied to the input and compared with the spec's result text: 71 cases, 0 mismatches. Not covered by the runner, and therefore checked against existing tests or the analyzer source instead:

- RH3007 (no code-fix provider, so the runner refuses it): RH3007-001/002 match `NestedQuerySyntaxIsReported`, `QuerySyntaxWithIntoContinuationIsReported`, `QuerySyntaxWithMultipleFromClausesIsReported`.
- RH3201-003 (the fixture does not parse): matches `UnterminatedMultiLineCommentAtEndOfFileIsNotReported`.
- The two negative blocks of SHARED-BODY-001 (no rule ID in their trailing sentence): the same code was run as RH3203 fixtures and gave `no-diagnostic`.
- Diagnostic spans (`{|RH####:…|}` extents): the runner does not report locations. They follow the analyzers' `CreateDiagnostic(...)` arguments and match the markup in the existing tests; the test implementer's tests confirm them.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"`: 31 passed, 1 failed — `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` with exactly the 77 new IDs uncovered (expected until the tests are tagged). `SpecConsistencyTests` and `SpecStructureTests` pass (code owners exist, one owner per type, Uses exact, ledger sorted, analyzer metadata rows match the descriptors).

## Changed requirements

All added (current behavior):

- RH3001-001 `!` on a `bool` operand is reported; -002 an operand that is not `bool` is not reported; -003 the diagnostic is on the operator; -004 the fix compares the operand with `false`; -005 a comment after the operator moves in front of the operand; -006 a directive after the operator withholds the fix.
- RH3002-001 parentheses around a whole expression are reported; -002 parentheses around a receiver are reported only around a primary expression; -003 parentheses around a cast, a lambda, an anonymous method, or a `switch` expression are not reported; -004 parentheses the parser needs are not reported; -005 parentheses in front of an assignment operator that it would join are not reported; -006 of nested pairs, the one that is needed stays; -007 the diagnostic covers the parentheses; -008 the fix removes the parentheses and keeps the code around them; -009 a comment or a directive inside the parentheses withholds the fix.
- RH3003-001 an empty string literal is reported; -002 a literal where a constant is required is not reported; -003 the diagnostic is on the literal; -004 the fix writes `string.Empty`.
- RH3004-001 an anonymous method with a parameter list is reported; -002 the diagnostic is on `delegate`; -003 the fix writes a lambda; -004 a comment or a directive outside the parameter list and the body withholds the fix.
- RH3005-001 a constant on the left of a comparison is reported; -002 other comparisons are not reported; -003 the diagnostic is on the operator; -004 the fix swaps the operands and mirrors the operator; -005 the fix is withheld unless the built-in operator compares both ways; -006 a comment inside the comparison withholds the fix.
- RH3006-001 `&&` directly inside `||` is reported; -002 the diagnostic covers the operand; -003 the fix puts the operand in parentheses.
- RH3007-001 every query expression is reported; -002 the diagnostic is on the first `from`.
- RH3101-001 a `base` access that binds the same without `base` is reported; -002 a `base` access the type needs is not reported; -003 the diagnostic is on `base`; -004 the fix removes the qualifier.
- RH3102-001 every empty statement is reported; -002 the diagnostic is on the semicolon; -003 the fix removes the statement from its block or switch section; -004 the fix is withheld for an embedded or labeled statement and for a statement with comments.
- RH3103-001 `Nullable<T>` with a type argument is reported; -002 the argument of `nameof` is not reported; -003 the diagnostic covers the whole type name; -004 the fix writes `T?`; -005 a comment the fix cannot place withholds the fix.
- RH3104-001 a parameterless creation of a value type is reported; -002 a creation that runs code is not reported; -003 the diagnostic is on the type or on `new`; -004 the fix writes `default` with the type.
- RH3105-001 a `this.` access that binds the same without `this` is reported; -002 a `this.` access the code needs is not reported; -003 the diagnostic is on `this`; -004 the fix removes the qualifier.
- RH3106-001 an empty parameter list of an anonymous method is reported.
- RH3107-001 an empty argument list of an attribute is reported.
- RH3201-001 an empty comment is reported; -002 an empty `//` line inside a block of `//` comments is not reported; -003 an unterminated block comment is not reported; -004 the diagnostic covers the comment; -005 the fix removes the comment.
- RH3203-001, RH3205-001, RH3206-001, RH3207-001 — every expression-bodied constructor / operator / conversion operator / finalizer is reported.
- RH3204-001 an interpolated string without interpolation used as a `string` is reported; -002 the diagnostic covers the interpolated string; -003 the fix writes the literal with the same value.
- RH3208-001 every expression-bodied local function is reported; -002 the fix separates the new block from the next statement; -003 the block of a labeled local function is laid out from the start of its line.
- SHARED-BODY-001 a member the formatter cannot convert is not reported; -002 the diagnostic covers the expression body; -003 the fix converts the member as the formatter does.
- SHARED-EMPTY-001 the diagnostic covers the empty list; -002 the fix removes the empty list.

## Examples for the tests

Every requirement above has examples. Existing tests that already exercise a requirement (to tag, or to replace with the spec's example where the existing one differs):

- RH3001: `NotOperatorOn*` (-001, -003, -004 for plain operands), `NotOperatorOnNullableBoolIsNotReported`, `NotOperatorOnUserDefinedOperatorIsNotReported` (-002), `CommentBetweenNotOperatorAndOperandIsPreserved`, `EndOfLineCommentBetweenNotOperatorAndOperandIsPreserved` (-005), `NestedOperandDirectivesRemainAvailableAndArePreserved`, `FixIsNotOfferedAcrossDirectiveAndDisabledText`, `FixIsNotOfferedAcrossSkippedSyntax` (-006). No existing test covers the added parentheses of RH3001-004 (`b == (a == false)`).
- RH3002: `UnnecessaryParenthesesIn{Return,Throw,Assignment,Argument,ExpressionBodiedMember}AreReportedAndFixed` (-001), `NecessaryParenthesesAroundConditionalInThrowExpressionAreNotReported` (-001 negative), `UnnecessaryParenthesesAround{MemberAccess,Invocation}AreReportedAndFixed` (-002), `NecessaryParenthesesAround{Cast,Lambda}AreNotReported` (-003), `ParenthesesAroundDelegateInvokedThroughTwoPairsAreNotReported`, `ParenthesesAroundArgumentsJoiningIntoGenericInvocationKeepTheClosingPair` (-004), `ParenthesesAroundRefConditionalAssignmentTargetAreNotReported`, `UnnecessaryParenthesesAroundAssignmentTargetAreReportedAndFixed` (-005), `NestedParenthesesAroundBinaryExpressionInMemberAccessChainKeepTheNecessaryPair` (-006), `ReturnWithoutSpaceBeforeParenthesesKeepsTheTokensSeparate`, `VerifyCommentBeforeTheParenthesesIsOfferedACodeFix` (-008), `VerifyCommentInsideTheParenthesesWithholdsTheCodeFix` (-009).
- RH3003: the `…IsReportedAndFixed` tests (-001, -003, -004), the `…IsNotReported` tests (-002).
- RH3004: `AnonymousMethodWith*`, `AsyncAnonymousMethodIsConvertedToAsyncLambda`, `CommentInsideSingleReturnBlockIsPreserved` (-001, -003), `AnonymousMethodWithoutParameterListIsNotReported` (-001), `Comment…/Directive…IsNotOfferedACodeFix` (-004).
- RH3005: `YodaConditionWith*` (-001, -004), `NormalConditionIsNotReported`, `ConstantToConstantComparisonIsNotReported`, `VariableToVariableComparisonIsNotReported` (-002), `AsymmetricUserDefinedOperator*`, `DynamicOperandIsNotFixed`, `UserDefinedEqualityOperatorIsNotFixed` (-005), `VerifyCommentInsideTheComparisonWithholdsTheCodeFix`, `VerifyCommentBeforeTheComparisonIsOfferedACodeFix` (-006).
- RH3006: `MixedLogicalOperators*`, `MixedOrPattern*` (-001 to -003), `AndOnly…`, `OrOnly…`, `ParenthesizedMixedExpressionIsNotReported`, `AndPatternOnly…`, `OrPatternOnly…` (-001 negative). No test keeps trivia around the wrapped operand (-003).
- RH3007: all `QuerySyntax…IsReported` (-001, -002), `MethodSyntaxIsNotReported`.
- RH3101: `BaseQualifierOnInherited{Method,Property,Indexer,Field}IsReportedAndFixed` (-001, -003, -004), `BaseQualifierWithLocalOverrideIsNotReported`, `BaseQualifierWithShadowedMemberIsNotReported` (-002).
- RH3102: `EmptyStatementIn*IsReportedAndFixed` (-001 to -003), `FixIsNotOfferedForEmbeddedEmptyStatement`, `FixIsNotOfferedWhenCommentIsAttachedToEmptyStatement` (-004). No test reports a labeled empty statement (-001, -004).
- RH3103: `…IsReportedAndFixed` (-001, -003, -004), `NullableGenericInNameofIsNotReported` (-002), `NullableGenericNestedInNameofIsReportedAndFixed` (-001), `NullableGenericInTypeofPreservesInnerComments` (-004), `QualifiedNullableGenericWithWrapperCommentDoesNotOfferFix`, `NullableGenericInTypeofWithOwnLineBlockCommentDoesNotOfferFix`, `…WithDirectivesDoesNotOfferFix` (-005).
- RH3104: `Default…ConstructorIsReportedAndFixed`, `ImplicitValueTypeCreationIsReportedAndFixed` (-001, -003, -004), `…IsNotReported` (-002).
- RH3105: `UnnecessaryThisQualifier…IsReportedAndFixed` (-001, -003, -004), `NecessaryThisQualifier…IsNotReported`, `ThisQualifierOnUnresolvableMemberIsNotReported` (-002).
- RH3106/RH3107: one test each (`VerifyDiagnosticAndCodeFix`, `VerifyDiagnosticsAndCodeFix`) — RH3106-001/RH3107-001 negatives and SHARED-EMPTY-001/002 need focused tests.
- RH3201: `Empty…IsReportedAndFixed` (-001, -004, -005), `EmptySingleLineCommentInsideCommentBlockIsNotReported`, `EmptySingleLineCommentBeforeFourSlashCommentIsNotReported` (-002), `Unterminated…IsNotReported` (-003). No test for the `///` neighbor (-002, reported side).
- RH3204: `Verify…IsDetectedAndFixed` (-001 to -003), `Verify…IsNotFlagged` (-001 negative).
- SHARED-BODY: `VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported` (every rule), `VerifyExpressionBodyWithRegionAfterExpressionIsNotReported` (RH3207), `VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed` (-001); the `…IsDetectedAndFixed` tests of RH3203, RH3205–RH3208 (-002, -003, and each rule's -001).
- RH3208-002: `VerifyStatementDirectlyAfter*IsSeparatedByBlankLine`, `VerifyBlankLineIsInsertedAboveCommentLeadingNextStatement`, `VerifyBlankLineIsInsertedAfterDirectiveLeadingNextStatement`, `VerifyTerminalSwitchSectionBreakAfterLocalFunctionStaysAttached`, `VerifyExistingBlankLineAfterLocalFunctionIsKept`, `VerifyLocalFunctionEndingItsBlockGetsNoTrailingBlankLine`. RH3208-003: `VerifyLabeledLocalFunction…IsIndentedFromStatementColumn`, `VerifyLocalFunctionBelowOutdentedLabelIsIndentedFromOwnColumn` and its variants.

Current behavior that is deliberately not a requirement: the second Fix All pass of RH3001, RH3002, RH3004, RH3102 (thoenissen/Reihitsu#1143; e.g. RH3001's `GetFixAllScenario` needs `NumberOfFixAllIterations = 2`). Keep those tests as they are.

## Open questions to file

Each block below was reproduced with `scripts/apply-fix.sh` on the commit above (LF and CRLF alike unless stated). None of the specs contains these cases. No `SDD open questions: …` issue exists yet for any of these paths; each needs a new one.

### `SDD open questions: sdd/specs/analyzer/RH3002.md` (new)

**Q1 — Decided by principle P11 (#912).** Input:

```csharp
class C
{
    int M(int a, bool c)
    {
        if ((c)) { }
        while ((c)) { }
        return a + (a);
    }
}
```

Real output: nothing is reported. Why it looks wrong: the inner pair of `if ((c))` / `while ((c))` and the pair around the operand `(a)` are unnecessary parentheses — the construct RH3002 names — but the rule only reports the positions listed in RH3002-001/-002 (a parenthesized expression whose parent is a statement header or a binary expression is never reported). Touches RH3002-001, RH3002-002.

### `SDD open questions: sdd/specs/analyzer/RH3004.md` (new)

**Q1.** Input: `Func<int, int> a = static delegate (int x) { return x; };` Real output of the fix: `Func<int, int> a = (int x) => x;`. Why it looks wrong: the fix drops the user-written `static` modifier, so the compiler no longer rejects captures in the lambda; the rewrite rebuilds the lambda from `async`, the parameter list, and the body only. Competing readings: keep `static` (`static (int x) => x`), or withhold the fix for modified anonymous methods. Touches RH3004-003.

**Q2 — Decided by principle P10 (#912).** Input:

```csharp
Func<int, int> a = delegate (int x)
{
    var y = x + 1;
    return y;
};
```

Real output: `Func<int, int> a = (int x) => {` followed by the unchanged statement lines — the line break and indentation in front of `{` are dropped, which the formatter would not produce. Touches RH3004-003.

### `SDD open questions: sdd/specs/analyzer/RH3006.md` (new)

**Q1.** Input (CRLF line endings):

```csharp
class C
{
    bool M(bool a, bool b, bool c)
    {
        return a ||
            /* x */ b && c;
    }
}
```

Real output: the text becomes `/* x */ (b && c);` as expected, but the result is no longer CRLF-only — `apply-fix` reports `[line-ending-drift]`, line endings `CRLF=7, LF=1`. The LF arm is clean, and inputs without a line break in the operand's leading context show no drift. Why it looks wrong: a fix must not introduce a line ending the document does not use. Touches RH3006-003.

### `SDD open questions: sdd/specs/analyzer/RH3101.md` (new)

**Q1 — Decided by principle P1 (#912), SHARED-FIX-003 (run time).** Input:

```csharp
class B
{
    public virtual int M() => 1;
}

class D : B
{
    public int Run()
    {
        return base.M();
    }
}

class E : D
{
    public override int M() => 2;
}
```

Real output: reported, and the fix writes `return M();`. Why it looks wrong: `base.M()` is a non-virtual call to `B.M`; `M()` dispatches virtually, so `new E().Run()` returns 2 instead of 1. The analyzer's check (the name binds to the same symbol) does not see dispatch. Applies whenever the member is virtual and the containing type is not sealed; a further override may live outside the compilation. Touches RH3101-001, RH3101-004.

**Q2 — Decided by principle P1 (#912), SHARED-FIX-003 (comment text).** Input: `return base /* a */ . /* b */ M();` (with `M` inherited, not overridden). Real output: `return M();` — both comments are deleted. Touches RH3101-004.

### `SDD open questions: sdd/specs/analyzer/RH3102.md` (new)

**Q1 — Decided by principle P10 (#912).** Input:

```csharp
class C
{
    void M(int a)
    {
        a++; ;
        a++;
    }
}
```

Real output: `        a++;         a++;` on one line — the removed empty statement took the line break after it, joining the next statement. Touches RH3102-003 (which specifies only the case where the empty statement stands on its own line).

### `SDD open questions: sdd/specs/analyzer/RH3103.md` (new)

**Q1 — fix part decided by principle P1 (#912), SHARED-FIX-003 (compile error).** Input: `var t = typeof(Nullable<>);` Real output: reported, and the fix writes `typeof(?)`, which does not parse (`invalid-result`). Why it looks wrong: an unbound `Nullable<>` has no shorthand, so the report itself has no correction; the fix breaks the code. Competing readings: do not report an unbound generic name (an omitted type argument), or report it without a fix. Touches RH3103-001, RH3103-004.

### `SDD open questions: sdd/specs/analyzer/RH3104.md` (new)

**Q1 — Decided by principle P1 (#912), SHARED-FIX-003 (comment text).** Inputs: `var a = new int(/* x */);` and `var a = new /* x */ int();`. Real output: `var a = default(int);` in both — the comment is deleted. Touches RH3104-004.

### `SDD open questions: sdd/specs/analyzer/RH3105.md` (new)

**Q1 — Decided by principle P1 (#912), SHARED-FIX-003 (comment text).** Input: `return this /* a */ . /* b */ _a;` Real output: `return _a;` — both comments are deleted. Touches RH3105-004.

### `SDD open questions: sdd/specs/analyzer/RH3106.md` (new)

**Q1 — fix part decided by principle P1 (#912), SHARED-FIX-003 (compile error).** Input:

```csharp
using System;

class C
{
    void Run(Action a) { }
    void Run(Action<int> a) { }
    void M() { Run(delegate () { }); }
}
```

Real output: reported, and the fix writes `Run(delegate  { });`, which does not compile: `error CS0121: The call is ambiguous between … 'C.Run(System.Action)' and 'C.Run(System.Action<int>)'` (confirmed with `csc`). Why it looks wrong: without a parameter list, an anonymous method converts to every delegate type, so removing `()` can change overload resolution. Competing readings: withhold the fix (and keep the diagnostic) when the conversion target is not fixed, or do not report such an anonymous method. Touches RH3106-001, SHARED-EMPTY-002.

### `SDD open questions: sdd/specs/shared/empty-parentheses.md` (new)

**Q1 — Decided by principle P1 (#912), SHARED-FIX-003 (comment text).** Inputs: `Action a = delegate ( /* none */ ) { };` (RH3106) and `[Obsolete(/* none */)]` (RH3107). Real output: `delegate  { }` and `[Obsolete]` — the comment inside the list is deleted. Both fixes remove the list with `SyntaxRemoveOptions.KeepExteriorTrivia`. Touches SHARED-EMPTY-002.

**Q2 — Decided by principle P10 (#912).** Inputs: `Action a = delegate () { };` (RH3106) and `[Obsolete /* a */ ( ) /* b */]` (RH3107). Real output: `delegate  { }` and `[Obsolete /* a */  /* b */]` — the whitespace on both sides of the removed list is kept, leaving two spaces. Touches SHARED-EMPTY-002 (whose examples use the forms without a space in front of `(`, which give one space).

### `SDD open questions: sdd/specs/analyzer/RH3201.md` (new)

**Q1 — Decided by principle P13 (#912).** Inputs:

```csharp
class C
{
    /* First. */
    //
    // Second.
    int _a;

    // First.
    /* */
    // Second.
    int _b;
}
```

Real output: both empty comments are reported (and removed by the fix). Why it looks wrong: RH3201-002 exempts an empty line inside a block of `//` comments; under P13 a line holding only a `/* */` comment counts like a `//` line, so the first is inside a comment block, and the second is an empty comment line between two `//` lines like the exempted `//`. Touches RH3201-002.

**Q2 — Decided by principle P9 (#912).** Input: `    /**/ int _a;` (comment at the start of a line, code after it). Real output: ` int _a;` — the indentation of the line is removed together with the comment. Touches RH3201-005 (which specifies only a comment alone on its line and a comment after code).

**Q3 — Decided by principle P10 (#912).** Input:

```csharp
class C
{
    /*

    */
    int _a;
}
```

Real output: the comment's first and last line are removed, but an empty line stays where the comment was, directly after `{` — which the formatter would remove. Touches RH3201-005.

## Decisions needed

Not blocking; the specs are complete without them. Each is a point the lead reserved.

1. **RH3202 and SHARED-BODY (moving or retiring RH3202 IDs).** RH3202-002, RH3202-003, and RH3202-004 state for methods exactly what SHARED-BODY-001, SHARED-BODY-002, and SHARED-BODY-003 state for the other five kinds (same predicate `ExpressionBodyRewriteUtilities.BlocksRewrite`, same `ArrowExpressionClause` location, same `ReihitsuFormatter.FormatNodeInDocumentAsync` fix). Letting RH3202 use SHARED-BODY would retire those three IDs. I did not touch RH3202's requirements; I only moved `ExpressionBodyRewriteUtilities` from RH3202's code owners to SHARED-BODY, as clarification (a) allows. RH3202-005 (separators kept) has no SHARED-BODY counterpart: with two blank lines above a converted constructor the fix collapses them to one, so "keeps the blank lines" did not hold as a general statement and I left it out of SHARED-BODY. Alternative owner for `ExpressionBodyRewriteUtilities`: FMT-STRUCT, whose FMT-STRUCT-007 states the decision the type implements.
2. **`BlankLineCodeFixUtilities` (thoenissen/Reihitsu#1067).** Not claimed by any spec. Facts: `RH5030BlankLineAfterClosingBraceCodeFixProvider` and `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider` both call the one method `BlankLineCodeFixUtilities.InsertBlankLineBefore(root, token)` with the first token of the statement after the closing brace; the RH5030 analyzer and the RH3208 fix both decide the need through `BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace` (SHARED-BLANK-003). So both rely on the same placement rule — RH5030-003's "in front of a comment that leads the statement, after the last directive". RH3208-002 currently states that placement in its own words (its example shows the comment and the directive case). If the maintainer wants a shared spec, the placement sentence of RH3208-002 and RH5030-003 would move there, which retires RH5030-003 and changes RH3208-002.
3. **RH3201 and SHARED-DOC.** The lead's note says RH3201 uses only SHARED-DOC. RH3201-002 relies on one SHARED-DOC code owner — `DocumentationAnalysisUtilities.IsDocumentationLine` (a line starting with `///` but not `////` is a documentation line) — but no SHARED-DOC requirement states that classification, so RH3201's `Uses` is `—` and RH3201-002 states the classification in its own terms. If the maintainer wants it shared, SHARED-DOC needs a new requirement; that is outside this batch's shared files.

## Follow-up candidates (not open questions)

- None found beyond the questions above.
