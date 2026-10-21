Role: test implementer
Commit: c43845c
Verdict: tests written

Lane: specs and tests for existing behavior. No production code changed. Every test below passes on the unchanged code. The prompt carried no intended fix, suspected cause, or hoped-for verdict. SHARED-FIX-006, -007 and -008 each have conforming shapes, so none of them blocks. `EveryRequirementOfAnEnforcedSpecIsCovered` now passes.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test`. "New" marks a test added in this run. "Tag" marks an existing test that only gained a `[Requirement]` attribute.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| New: `RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests.FixChangesOnlyTheGapsNextToTheOperator`. `int  y=1;`, then `int  z = a{\|RH6024:+\|}b;`, then `return  z;`. Only the two gaps next to `+` change. | SHARED-FIX-006 (Formatting), FC-030 | `Verify` | pass |
| New: `RH4106PrivateFieldCasingAnalyzerTests.FixChangesOnlyTheNamesAndKeepsTheLayoutAroundThem`. The field, two blank lines, then `public int  Get( ) => Value;`. Only the names change. | SHARED-FIX-006 (Blank lines, Formatting), FC-030 | `Verify` | pass |
| Tag: `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyBlankLineAboveFixedMethodIsKept`. Same shape as the spec's RH3202 blank-line example. | SHARED-FIX-006 (Blank lines: not the diagnostic's code) | `Verify` | pass |
| Tag: `RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests.OneApplicationCorrectsTheLineInFrontOfTheReportedIf`. The SHARED-FIX-001 RH5002 example that SHARED-FIX-006 cites. | SHARED-FIX-006 (Blank lines: the diagnostic's code), FC-003 | `Verify` | pass |
| Tag: `RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests.CodeFixRemovesBothDirectiveLinesBetweenStatements`. The spec's RH7303 example, verbatim. | SHARED-FIX-007 (Removed directive line, Blank lines), FC-019/FC-020 | `Verify` | pass |
| New: `RH7310EmptyRegionsShouldBeRemovedAnalyzerTests.CodeFixKeepsASingleBlankLineAndRemovesADoubledOneBetweenMembers`, plus `…WithCarriageReturnLineFeed`. The spec's RH7310 between-members example, verbatim. Each test also asserts that formatting the result changes nothing. | SHARED-FIX-007 (Removed directive line, both sides), FC-003, FC-013, FC-034 | `Verify` + `AssertFormattingChangesNothing` | pass |
| Tag: `RH7310EmptyRegionsShouldBeRemovedAnalyzerTests.CodeFixRemovesTheRegionAndTheBlankLinesAfterIt`. The `Methods` half. | SHARED-FIX-007 | `Verify` | pass |
| New: `RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.VerifyFixOfAMemberOnItsOwnLineKeepsTheBlankLineAbove`, plus `…WithCarriageReturnLineFeed`. The `N` half of the spec's shared-line example, after `int _y;` and a blank line. Each test also asserts that formatting the result changes nothing. | SHARED-FIX-007 (own-line side of Shared line), FC-013 | `Verify` + `AssertFormattingChangesNothing` | pass |
| Tag: `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyFixWrapsBodyOnLineOfIfInBracesWithCarriageReturnLineFeed`. The spec's RH5405 example under CRLF. | SHARED-FIX-008 | `Verify` | pass |
| New: `RH5405…FixWritesCarriageReturnLineFeedInADocumentThatUsesIt` and `RH5405…FixWritesLineFeedInADocumentThatUsesIt`. The spec's example under CRLF and under LF, with exact bytes. | SHARED-FIX-008, FC-017, FC-033 | `ApplyCodeFixAsync` + `Assert.AreEqual` | pass |
| New: `RH5405…FixKeepsTheLineFeedsInsideALiteralAndWritesTheDocumentsCarriageReturnLineFeed`. A CRLF document with a verbatim literal that holds 13 LF line breaks, more than the document's 8 CRLF line breaks. The literal keeps its LF line breaks, and the fix writes CRLF. | SHARED-FIX-008 (literal clause, "does not count") | `ApplyCodeFixAsync` + `Assert.AreEqual` | pass |
| New: `RH5405…FixKeepsTheLineFeedsInsideACommentAndWritesTheDocumentsCarriageReturnLineFeed`. The same check with a `/* */` comment that holds 13 LF line breaks. | SHARED-FIX-008 (comment clause) | `ApplyCodeFixAsync` + `Assert.AreEqual` | pass |
| Tag: `RH7303…CodeFixRemovesBothDirectiveLinesWithCarriageReturnLineFeed` and `RH7310…CodeFixRemovesTheRegionWithCarriageReturnLineFeed`. A removal keeps every CRLF, with exact bytes. | SHARED-FIX-008 (kept line breaks) | `ApplyCodeFixAsync` + `Assert.AreEqual` | pass |
| New: `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.OneFixAllApplicationCorrectsOnlyTheDiagnosticsWhoseFixIsOfferedOnTheStartingCode`. The SHARED-FIX-003 RH7103 example that SHARED-FIX-002 points to. One Fix All application in document, project and solution scope, and one single fix, both give the second block, in which `_b` is still reported. | SHARED-FIX-002, FC-008 | `Verify` with `FixOne`, `NumberOfIncrementalIterations = 1`, `NumberOfFixAllIterations = 1`, `FixedState.MarkupHandling = Allow` | pass |

Test infrastructure: `AnalyzerTestsBase<TAnalyzer, TCodeFix>` gains a protected `AssertFormattingChangesNothing(string)`. It runs `ReihitsuFormatter.FormatSyntaxTree` and compares the output exactly. `RH5405…Tests` gains a private `NormalizeToLineFeed`.

Evidence that the assertions can fail:
- `Verify` compares line-ending bytes. A temporary test (deleted) gave it CRLF input and an LF expectation. It failed in "Iterative code fix application", and the diff showed `<CR><LF>` against `<LF>`. So the tagged CRLF `Verify` test would catch a fix that writes LF.
- The RH7103 test runs the Fix All path. With `NumberOfFixAllIterations = 0` as a temporary mutation (reverted), it failed in "Fix all in document". Without `FixOne`, the verifier's single-fix loop also applies the `_b` fix once the guard releases it. That loop result differs from the second block, so `FixOne` is needed to model "those single fixes" of SHARED-FIX-002.

## Scenarios (bug fix)

_N/A: not a bug fix._

## Not covered, and why

These shapes were probed, are non-conforming today, and were removed. No test contradicts a requirement:
- RH3202 on `private int  _value;` / `public int  GetValue( ) => _value;`. Observed: the fix rewrites the signature to `public int GetValue()`. Expected under SHARED-FIX-006: `public int  GetValue( )` stays. The RH3203 instance of this is tracked by thoenissen/Reihitsu#1284. This RH3202 instance is a follow-up candidate for that issue.
- RH3202 on `public int GetValue() => _value; private int _x;`. Observed: `    } private int _x;`. Expected under SHARED-FIX-007: `private int _x;` on the next line. This is a follow-up candidate for thoenissen/Reihitsu#1144, which lists the RH5417 instances only.

The "shares its line" side of SHARED-FIX-007's Shared line bullet has no conforming shape I could find. RH5417 `M`/`_x` and `_y`/`P` (thoenissen/Reihitsu#1144) and the RH3202 probe above all fail it. Only the own-line side is covered. The requirement as a whole is covered by its removed-directive and blank-line shapes.

Spec examples named in the handovers as non-conforming are left out: the RH6024 lambda (thoenissen/Reihitsu#1141), the RH3203 attribute and doc comment, and the RH3202 two blank lines (thoenissen/Reihitsu#1284), the RH5101 raw string (thoenissen/Reihitsu#1144), and RH7310 at the start and end of a body (thoenissen/Reihitsu#1263).

Existing tests that pin behavior SHARED-FIX-006 now forbids are left unchanged, because the bug issues own them:
- `RH3202…VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals` (tagged SHARED-FIX-003; it pins the `///` reflow of `M`).
- `RH3202…VerifyFixCollapsesTwoBlankLinesAboveTheMethod` (untagged).

SHARED-FIX-007 RH7303: there is no formatter-stability assertion. The spec states that formatting would insert a blank line after `var x = 1;` and that the fix inserts none (Blank lines bullet). That test therefore pins exact output only.

SHARED-FIX-008 disabled text: no test. That clause is not written (`03-spec-author.md`, Decisions needed). A lone-CR or Unicode-separator document is not specified either.

Failure classes:
- Applied: FC-003, FC-008, FC-013, FC-017, FC-019/FC-020 (the region-directive gaps of RH7303/RH7310), FC-030, FC-033 (LF, CRLF and literal/comment line breaks), FC-034 (spec examples verbatim where they conform), FC-035 (expected values come from the spec examples; no patched code exists).
- FC-021: covered only by the `#endregion // …` comments in the RH7310/RH7303 gaps and by the SHARED-FIX-008 comment test. This run adds no gap-handling logic.
- Not applicable, because this lane changes no production code, guard, rewrite, phase or naming fix: FC-001, -002, -004 to -007, -009 to -012, -014 to -016, -018, -022 to -029, -031, -032, -036, -037. FC-028 does not apply either, because no fix's formatting entry point changes.

## Commands

```shell
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-002|TestCategory=SHARED-FIX-006|TestCategory=SHARED-FIX-007|TestCategory=SHARED-FIX-008"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7310EmptyRegionsShouldBeRemovedAnalyzerTests|FullyQualifiedName~RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests|FullyQualifiedName~RH4106PrivateFieldCasingAnalyzerTests|FullyQualifiedName~RH5405BracesMustNotBeOmittedAnalyzerTests|FullyQualifiedName~CodeFixTestBaseCoverageTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

Results on this tree: analyzer 202 passed and 0 failed; architecture `Sdd` 39 passed; the whole architecture project 72 passed. `scripts/format.sh` on the 10 changed test files changed nothing.
