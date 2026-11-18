Role: test implementer
Commit: 83242b2
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All tests are in two files:

- `src/Reihitsu.Analyzer.Test/Formatting/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`

Every test with an `endOfLine` parameter runs as `\n` and `\r\n` (DataRow). `Verify(source, fixed)` covers the iterative fix, Fix All in document, project, and solution, and re-analysis of the result. `VerifyCodeFixWithEndOfLine` also compares the raw result of one fix, line endings included. Expected values come from the RH5307-004 and RH5401-003/-004 examples, the change doc's *Visible output changes*, and the skeptic's matrices (02). None comes from running a patched copy of the code.

Run on 83242b2: 60 fail, 59 pass (counting each DataRow). Each failure was checked against its diff. The result is either the input unchanged (no fix offered, or an action that changes nothing) or, for the nested case, the receiver's list joined instead of the reported one. These are the issue's own differences. No failure comes from a compile error or from unrelated layout.

## Tests

### RH5307

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixJoinsElementAccessInsideAnotherArgumentList` (replaces `VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList`) — `x[m[1,⏎2]]` | RH5307-004; FC-001, FC-027 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListBehindAReceiverElementAccess` — `j[0][1,⏎2]` | RH5307-004; FC-009 | `VerifyCodeFixWithEndOfLine` | fail (action changes nothing) |
| `VerifyFixJoinsListBehindAReceiverElementAccessWithAComment` (replaces `VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess`) — `j[0 /* c */][1,⏎2]` | RH5307-004; FC-021 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListBehindAReceiverElementAccessWithAMultiLineArgument` — `j[N(1,⏎2)][1,⏎2]`, receiver list kept | RH5307-004 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListBehindACommentInAnotherArgumentList` — `x[/* c */ m[1,⏎2]]`, the space behind the comment stays | RH5307-004, SHARED-FIX-006; FC-021, FC-030 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListBehindAnElementBinding` — `c?[0][1,⏎2]` reported and fixed | RH5307-001, RH5307-004 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListInsideAnElementBinding` — `n?[m[1,⏎2]]` | RH5307-004 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixJoinsListBehindAMemberAccessThatStartsWithAnElementAccess` — `a[0].P[0][1,⏎2]` | RH5307-004; FC-009 | `VerifyCodeFixWithEndOfLine` | fail (action changes nothing) |
| `VerifyFixJoinsListBehindAThisIndexerAccess` — `this[0][1,⏎2]` | RH5307-004; FC-009 | `VerifyCodeFixWithEndOfLine` | fail (action changes nothing) |
| `VerifyFixJoinsListInsideAnImplicitElementAccess` — `[m[1,⏎2]] = 1` in an initializer | RH5307-004; FC-027 | `VerifyCodeFixWithEndOfLine` | fail (no fix offered) |
| `VerifyFixesJoinBothListsWhenTheReceiverListIsReportedToo` — `j[0,⏎1][1,⏎2]`, both reported; iterative and Fix All give `j[0, 1][1, 2]` | RH5307-004; FC-008 | `Verify` | fail (outer list never joined) |
| `VerifyEachFixJoinsItsOwnListWhenTheReceiverListIsReportedToo` — fix of each diagnostic at its own location (`TryApplyCodeFixAtAsync`); the exact intermediate text is asserted (the other list as written, at its columns); then the remaining fix, in both orders | RH5307-004; FC-008, FC-009 | `TryApplyCodeFixAtAsync`, `ApplyCodeFixAsync` | fail (the outer diagnostic's fix joins the receiver's list) |
| `VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer` — the change doc's whole input (`va`–`vj`, `vm`) in one document | RH5307-004; FC-008 | `Verify` | fail |
| `VerifyFixJoinsListOfAnInvocationArgument` — `N(m[1,⏎2])` | RH5307-004 | `VerifyCodeFixWithEndOfLine` | pass (stays fixed) |
| `VerifyFixJoinsListOfACollectionExpressionElement` — `[m[1,⏎2]]`, collection expression kept | RH5307-004, SHARED-FIX-006; FC-030 | `VerifyCodeFixWithEndOfLine` | pass (stays fixed) |
| `VerifyFixJoinsListBehindAParenthesizedReceiver` — `(j[0])[1,⏎2]` | RH5307-004 | `VerifyCodeFixWithEndOfLine` | pass (stays fixed) |
| `VerifyFixJoinsListBehindAMemberBinding` — `c?.P[1,⏎2]` | RH5307-004 | `VerifyCodeFixWithEndOfLine` | pass (stays fixed) |
| `VerifyNullConditionalElementAccessIsNotReported` — `c?[1,⏎2]` | RH5307-001 | `Verify` (analyzer) | pass |

`GetFixAllScenario`: the scenario is unchanged. Its code comment described the old fix target ("both resolve to the identical fix target: the enclosing return statement…"); I reworded it to describe only the input (FC-031).

### RH5401

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixPutsAPropertyWithAttributeListsOnOneLine` — RH5401-003 attributed example (`A` two lists + modifier, `B` list on the property's line), two diagnostics | RH5401-003, RH5401-004; FC-001 | `Verify` | fail (no fix offered) |
| `VerifyFixPutsAPropertyBelowItsAttributeListsOnOneLine` — `A` alone | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixMovesAnAttributeListOnThePropertysLineOntoALineOfItsOwn` — `B` alone (maintainer Q1 A) | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixSplitsAnAttributeListWithSeveralAttributesOnThePropertysLine` — `[Obsolete, CLSCompliant(false)] public int E` → one list per line (FMT-STRUCT-028, FMT-BREAK-027) | RH5401-003 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixIsOfferedForAnAttributeListOnSeveralLines` — `[Obsolete("a",⏎false)]` kept over two lines | RH5401-004 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsAnAttributeListWithACommentBetweenItsBracketsOnThePropertysLine` — `[Obsolete /* c */] int E => 1;` | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | pass (stays fixed) |
| `VerifyFixKeepsALineCommentBehindAnAttributeList` — `[Obsolete] // why` | RH5401-003, RH5401-004; FC-021 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsABlockCommentBehindAnAttributeListOnThePropertysLine` — `[Obsolete] /* c */ int E` → `[Obsolete] /* c */⏎int E => 1;` | RH5401-003, RH5401-004; FC-021 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixInsertsNoBlankLineInFrontOfACommentBehindAnAttributeList` — RH5401-003 comment-between example (maintainer Q3) | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsTheBlankLinesBehindAnAttributeList` — RH5401-003 example `E`, `G`, `H`, `K` in one document | RH5401-003; FC-008 | `Verify` | fail |
| `VerifyFixKeepsTwoBlankLinesInFrontOfACommentBehindAnAttributeList` — `H` alone (maintainer Q5 A) | RH5401-003 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsTheBlankLinesBetweenAnAttributeListAndTheProperty` — one and two blank lines, no comment | RH5401-003 | `Verify` | fail |
| `VerifyFixKeepsTwoBlankLinesBetweenTwoAttributeLists` — `K` alone | RH5401-003 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsAConditionalDirectiveBetweenTwoAttributeLists` — `#if DEBUG` around the second list | RH5401-003, RH5401-004; FC-020 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsAConditionalCommentBetweenAnAttributeListAndTheProperty` — `#if DEBUG⏎// note⏎#endif` | RH5401-003, RH5401-004; FC-020, FC-019 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixKeepsTheOneLineDocumentationCommentAboveTheAttributeLists` | RH5401-003, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixGivesAnAttributedPropertyBehindAMemberALineOfItsOwn` — `int _f; [Obsolete] int E` | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fail |
| `VerifyFixIsOfferedForACommentBehindTheSemicolon` — with and without an attribute list | RH5401-004 | `Verify` | fail (only the attributed property) |
| `VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` (existing; extended to the full RH5401-004 example: `using System;` and the attributed `E`) | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixIsWithheldForAnAttributedPropertyWhoseExpressionStaysOnSeveralLines` — collection expression, verbatim string | RH5401-004; FC-022 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixIsWithheldForACommentInFrontOfTheArrowOfAnAttributedProperty` — the other side of "a comment between attributes and first token does not count" | RH5401-004; FC-003 | `VerifyNoCodeFixIsOffered` | pass |
| `VerifyFixIsWithheldForAnAttributeListTheFormatterRefuses` — `[Obsolete("a" // c⏎, false)]` (FMT-UNSUP-009 shape in the attribute list) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | pass |

### Existing tests whose asserted output changes

- RH5307 `VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList` and `VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess` asserted that the fix is withheld. I rewrote them to the fix (rows 1 and 3 above).
- Searches run for every other changed output: `rg -n "RH5307|RH5401" src --glob '*Test*/**'`, plus `rg "Obsolete"` and `rg "\?\["` in the RH5307/RH5401 analyzer and formatter test files. The formatter test classes `RH5307…FormatterTests` and `RH5401…FormatterTests` assert formatter output, which this run does not change, so I left them as they are. `VerifyDiagnosticCoversAttributeListsAndModifiers` runs only the analyzer and is unchanged. No CLI, formatter, or other analyzer test references either rule's fix.

## Not covered, and why

- FC-018, FC-036 (naming), and FC-010, FC-011, FC-012 (formatter phases): no rename and no formatter phase change.
- FC-038 to FC-043: the run declares no shape unsupported. SHARED-FIX-010 is covered once, for RH5401, through the FMT-UNSUP-009 attribute-list case. The RH5307-002 shapes are not reported at all.
- FC-006, FC-023, FC-024, FC-025: both fixes only re-lay whitespace in a construct that is already fixed in kind. RH5401's explicit interface implementation is covered by existing tests, and its other expression-bodied kinds are outside RH5401.
- FC-029, FC-040: no key/value layout. No test of `///` mode beyond the default `Parse`, because neither fix reads documentation trivia beyond keeping it in place (covered by the doc-comment test).
- RH5401 switch-expression member `n` (02 matrix): left out. It is the same cell as the collection-expression and verbatim-string withheld cases, an expression the formatter keeps multi-line.
- RH5307 `i` and `j`: both written, although 02 says one is enough.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests|FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
scripts/test.sh --project architecture
```

Architecture tests on the new tests: 198 passed.

## Follow-up candidates

_None._
