Role: test implementer
Commit: <filled below>
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's "Probable owners" note and its expected output for `where TSecond :⏎ IDisposable` (one level under `where`) were not used; the maintainer's Q2 (join) overrides the latter, and the reproduction test expects `where TSecond : IDisposable`.

Expected values come from the spec examples (FMT-BREAK-037, FMT-INDENT-032, RH5110-003), the change doc entries 1–13, and the skeptic's candidate list, with columns derived from the spec text (`where` + 4 for a pushed-down colon, colon + 2 behind it, first-constraint column otherwise). No production code was touched.

All new tests were run on the unchanged code (84f5f18). Every failing test fails on an output mismatch (a column or a join), under LF already; none fails on a compile error, a syntax error the formatter skips, a phase-stability check, or a missing helper. In the RH5110 tests the fix is offered and reached iterative fix application in every case; the mismatch is the fixed text.

## Tests

`src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConstraintClauseLineBreakTests.cs` and `src/Reihitsu.Formatter.Test/Regression/Indentation/ConstraintClauseAlignmentTests.cs` use `FormatterTestsBase.AssertRuleResult`, which runs the full pipeline under LF and CRLF, a second pass, the per-phase stability check, and the phase tree round trip. Combined-pipeline coverage is therefore built in (structural empty-type transform, blank-line phase, horizontal spacing, indentation).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ConstraintClauseLineBreakTests.ReportedWrappedConstraintClausesAreLaidOutOneConstraintPerLine | FMT-BREAK-037, FMT-INDENT-032 (issue reproduction) | AssertRuleResult | fail |
| …PartlyWrappedClauseBecomesOneConstraintPerLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …SingleConstraintWrappedBehindColonIsJoined | FMT-BREAK-037 | AssertRuleResult | fail |
| …FirstOfSeveralConstraintsWrappedBehindColonIsJoined | FMT-BREAK-037 | AssertRuleResult | fail |
| …CommaAtLineStartMovesToEndOfPreviousLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …ClauseOnOneLineStaysUnchanged | FMT-BREAK-037 (not-wrapped side) | AssertRuleResult | pass |
| …ClauseAlreadyOneConstraintPerLineStaysUnchanged | FMT-BREAK-037, FMT-INDENT-032 (stable valid code) | AssertRuleResult | fail (today pushes the lines to column 4) |
| …ColonWrappedBelowTypeParameterJoinsItsLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …ColonWrappedBelowTypeParameterJoinsAndWrappedClauseIsLaidOut | FMT-BREAK-037 | AssertRuleResult | fail |
| …WrappedTypeArgumentListInsideConstraintJoinsAndClauseStaysOnOneLine | FMT-BREAK-037 (judged after join) | AssertRuleResult | pass (already joined by FMT-BREAK-014; must stay) |
| …WrappedQualifiedNameInsideConstraintJoins | FMT-BREAK-037 | AssertRuleResult | fail |
| …WrappedAliasQualifiedNameInsideConstraintJoins | FMT-BREAK-037 | AssertRuleResult | fail |
| …WrappedConstructorConstraintJoins | FMT-BREAK-037 | AssertRuleResult | fail |
| …WrappedNullableClassConstraintJoins | FMT-BREAK-037 | AssertRuleResult | fail |
| …WrappedAllowsRefStructConstraintJoins | FMT-BREAK-037 | AssertRuleResult | fail |
| …ClauseWithJoinedInnerWrapIsStillWrappedByCommaWrap | FMT-BREAK-037 (other side of "judged after joins") | AssertRuleResult | fail |
| …ConstraintHeldOverTwoLinesByCommentWrapsItsClause | FMT-BREAK-037, FMT-INDENT-032 (chained anchor `string>` at 30) | AssertRuleResult | fail |
| …CommentBehindTypeParameterKeepsColonOnItsOwnLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …CommentBehindColonKeepsFirstConstraintOnNextLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …CommentBehindColonWrapsClauseWithConstraintsOnOneLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …DirectiveBehindColonKeepsFirstConstraintOnNextLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …CommentInFrontOfCommaKeepsCommaAtLineStart | FMT-BREAK-037 | AssertRuleResult | fail |
| …DirectiveInFrontOfCommaKeepsCommaAtLineStart | FMT-BREAK-037 | AssertRuleResult | fail |
| …BlockCommentBehindCommaStaysBehindIt | FMT-BREAK-037 | AssertRuleResult | fail |
| …LineCommentBehindCommaStaysBehindIt | FMT-BREAK-037 | AssertRuleResult | fail |
| …CommaWithCommentBehindItMovesToEndOfPreviousLine | FMT-BREAK-037 | AssertRuleResult | fail |
| …BlankLineBehindCommaIsKept | FMT-BREAK-037 | AssertRuleResult | fail (column only; blank line already kept) |
| …BlankLineBehindColonGoesWithTheJoin | FMT-BREAK-037 | AssertRuleResult | fail |
| …ClauseOnOneLineWithTrailingCommentStaysUnchanged | FMT-BREAK-037 | AssertRuleResult | pass |
| …ClauseOnOneLineWithInlineBlockCommentStaysUnchanged | FMT-BREAK-037 | AssertRuleResult | pass |
| …AllowsRefStructAsLastConstraintOfWrappedClauseIsLaidOut | FMT-BREAK-037 | AssertRuleResult | fail |
| …TwoWrappedClausesOfOneDeclarationAreEachLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …WrappedClauseOfVerbatimTypeParameterIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| ConstraintClauseAlignmentTests.ConstraintsOfTypeMethodAndExtensionBlockAlignUnderFirstConstraint | FMT-INDENT-032 | AssertRuleResult | fail |
| …LinesPushedDownByCommentsAndDirectivesTakeTheFirstConstraintColumn | FMT-INDENT-032 | AssertRuleResult | fail |
| …ColonPushedDownByDirectiveIsIndentedOneLevelDeeperThanWhere | FMT-INDENT-032 | AssertRuleResult | fail |
| …ConstraintsOfMemberWithoutBodyLocalFunctionAndDelegateAlign | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …StructClauseIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …InterfaceClauseIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …RecordClauseIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …RecordStructClauseIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 | AssertRuleResult | fail |
| …ExtensionBlockClauseWrappedInFrontOfCommaIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 (FC-016) | AssertRuleResult | fail |
| …EmptyInterfaceWithWrappedClauseIsLaidOut | FMT-BREAK-037, FMT-INDENT-032 (structural interaction) | AssertRuleResult | fail |
| …ConstraintsFollowWhereLineMovedRight | FMT-INDENT-032 (FC-027, anchor after FMT-INDENT-017 move) | AssertRuleResult | fail |
| …ConstraintsFollowWhereLineMovedLeft | FMT-INDENT-032 | AssertRuleResult | fail |
| …NestedTypeInFileScopedNamespaceClauseAligns | FMT-INDENT-032 (FC-027) | AssertRuleResult | fail |
| …LocalFunctionInLambdaClauseAligns | FMT-INDENT-032 (FC-027) | AssertRuleResult | fail |
| RH5110…AnalyzerTests.VerifyFixLaysOutWrappedConstraints | RH5110-003 (two diagnostics in one document, so Fix All / batch too; FC-007, FC-008) | Verify (BatchCodeFixTestsBase) | fail |
| RH5110…AnalyzerTests.VerifyFixLaysOutWrappedConstraintsWithCrLf | RH5110-003 | Verify | fail |
| RH5110…AnalyzerTests.VerifyFixIsOfferedWhereCommentsBlockJoins | RH5110-003 (spec example; two diagnostics) | Verify | fail |
| RH5110…AnalyzerTests.VerifyFixIsOfferedWhereCommentKeepsColonOnItsOwnLine | RH5110-003 | Verify | fail |
| RH5110…AnalyzerTests.VerifyFixIsOfferedWhereDirectiveKeepsFirstConstraintOnNextLine | RH5110-003 | Verify | fail |
| RH5110…FormatterTests.VerifyFormatterFixesViolationWithWrappedConstraints | — (FC-013 parity) | VerifyFormatter | fail |

## Not covered, and why

- FC-004/FC-005/FC-019/FC-020/FC-021: covered — a comment and a directive in each joinable gap (type parameter–colon, colon, comma front), comments behind commas, a blank line in a joined and a kept gap. A `#pragma` or `#region` in a gap is not tested separately: FMT-BREAK-001 treats every directive alike and the spec names "a directive"; one `#if` per gap shape stands for the class. Disabled text (`#if X` branch inactive) is not tested separately for the same reason; the lead may ask for one if the implementer's guard distinguishes them.
- A comment-held wrap inside a constraint other than a type argument list (`System. // c⏎ IDisposable`): its inner line's column is not decided by this change (Q5, follow-up 7), so no test asserts it.
- FC-006/FC-022/FC-025/FC-036/FC-018: no rewrite of meaning, literals, expression bodies, or names reaches a constraint clause.
- FC-023: verbatim identifier covered; digit/Unicode identifiers do not change the column computation beyond width, which `TwoWrappedClauses…` covers.
- FC-024: `partial` and explicit implementations cannot change a clause's layout; not tested.
- FC-028/FC-030: RH5110's fix uses `FormatNodeInDocumentAsync`; the RH5110 tests are that path. `FormatNode` (detached) is reached by no caller for constraint clauses.
- FC-029, FC-037: not applicable.
- FC-032: `rg` over analyzer/CLI/formatter tests for fixed outputs with a wrapped clause found only `WrappedAngleBracketListTests.WrappedGenericConstraintTypeJoins` (single constraint, already joined; stays valid). The implementer must still rerun the full analyzer suite.
- Self-hosting stability: CI's `FormatterSelfHostingTests`.
- The new() ordering in `BlockCommentBehindCommaStaysBehindIt` (`new(), IDisposable`) copies the spec example; it is a semantic error only, not a syntax error, so the formatter still formats it (observed: today's output differs, not the input returned).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ConstraintClauseLineBreakTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Indentation.ConstraintClauseAlignmentTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5110"
```
