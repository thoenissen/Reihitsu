Role: test implementer
Commit: 91822fd (tests changed on top; see the commit carrying this handover)
Verdict: tests written

Repair round 3, lead decision 13 (auditor 16, thoenissen/Reihitsu#1289 findings #3 and #4). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 13 itself. Expected values come from decision 13 (C# comments and literals inside `@code`/`@functions` are read like the rest of the file, as on `main`) and RH2109-001 for the real directives (reported, as on `main`). Kept parts of the fix — line-start-only directives (Q3), line-starting Razor/HTML comment openers, `@@`, `@` after a letter or digit — keep their expectations.

## Tests

File: `src/Reihitsu.Analyzer.Test/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.cs`. Inputs of the turned tests are unchanged.

Turned to `main`'s report (refused class of decision 13). Each now asserts that a line-starting `@code {` inside a C# comment or literal of a block is reported. That contradicts RH2109-001/SHARED-DIAG-003 (a known gap carried by the follow-up), so the tests are characterizations: tags `RH2109-001` and `SHARED-DIAG-003` removed, summary states "pins today's behavior without stating it as intended".

| Test (old name → new name) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCodeBlockTextInCommentsAndStringsIsNotReported` → `VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot` | none (characterization) | `Verify(TestCode, onConfigure, RazorCodeBlockSpan…)` | fail — (3,1), (5,5) expected; 1 reported |
| `VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsNotReported` → `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` | none | same | fail — (1,1), (3,5), (7,1); 1 reported |
| `VerifyCodeBlockLineInsideDocumentationBlockCommentIsNotReported` → `…IsReported` | none | same | fail — (1,1), (3,1); 1 reported |
| `VerifyCodeBlockLineInsideVerbatimStringIsNotReported` → `…IsReported` | none | same | fail — (1,1), (3,1); 1 reported |
| `VerifyCodeBlockLineInsideInterpolatedVerbatimStringIsNotReported` → `…IsReported` | none | same | fail — (1,1), (3,1); 1 reported |
| `VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported` → `…IsReported` | none | same | fail — (1,1), (3,1), (7,1); 2 reported |
| `VerifyCodeBlockLineInsideBlockCommentOfFunctionsBlockIsNotReported` → `…IsReported` | none | same | fail — (3,1), (6,1); 1 reported |
| `VerifyClosingBracesInsideLiteralsAndCommentsDoNotEndCodeBlock` → `VerifyCodeBlockLineInsideBlockCommentAfterClosingBracesInLiteralsIsReported` | none | same | fail — (1,1), (6,1); 1 reported |
| `VerifyNestedBracesKeepCodeBlockOpenUntilItsClosingBrace` → `VerifyCodeBlockLineInsideBlockCommentAfterNestedBracesIsReported` | none | same | fail — (1,1), (7,1), (12,1); 2 reported |

Judged and left unchanged (brace or literal dependent input, but `main`'s report already equals the expectation, or the expectation rests on a kept part):
- `VerifyOpeningBracesInsideLiteralsAndCommentsDoNotKeepCodeBlockOpen`, `VerifyCommentAndStringStartsInsideLiteralsStartNothing`, `VerifyCSharpCommentStartInMarkupDoesNotHideDirective`, the four `…InTemplateMarkupOf…` tests from 12: expectations equal `main`'s report; pass on head and with `main`'s analyzer.
- `VerifyCodeBlockLineInsideRazorCommentWithinCodeBlockIsNotReported`: a line-starting `@*` inside a `@code` block — a kept part (line-starting Razor comment opener), not the refused class. Stays not reported; `main` reports (3,1).

New, auditor 16 finding #3 counterexamples, each followed by a real line-starting `@code {` at (7,1); all assert exactly `main`'s report:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyOpeningBraceInMarkupOfBuilderMethodDoesNotHideLaterDirective` (`<p>{</p>` in a `__builder` method, auditor `a1`) | RH2109-001; FC-027 | same | fail — (1,1), (7,1); 1 reported |
| `VerifyBlockCommentOpenerInMarkupOfBuilderMethodDoesNotHideLaterDirective` (`<p>/* note</p>`, `a2`) | RH2109-001; FC-027 | same | fail — (1,1), (7,1); 1 reported |
| `VerifyOpeningBraceInMarkupOfBuilderLambdaDoesNotHideLaterDirective` (`__builder =>` lambda, `a3`) | RH2109-001; FC-027 | same | fail — (1,1), (7,1); 1 reported |
| `VerifyOpeningBraceInMarkupOfBuilderMethodInFunctionsBlockDoesNotHideLaterDirective` (`a4`) | RH2109-001; FC-027 | same | fail — (7,1); 0 reported |
| `VerifyBlockCommentOpenerInMarkupOfBuilderMethodInFunctionsBlockDoesNotHideLaterDirective` | RH2109-001; FC-027 | same | fail — (7,1); 0 reported |
| `VerifyOpeningBraceInTextElementOfCodeBlockDoesNotHideLaterDirective` (`<text>{</text>`, `a5`) | RH2109-001; FC-027 | same | fail — (1,1), (7,1); 1 reported |
| `VerifyOpeningBraceInInlineMarkupOfCodeBlockDoesNotHideLaterDirective` (`if (true) { <p>{</p> }`, `e3`) | RH2109-001; FC-027 | same | fail — (1,1), (7,1); 1 reported |

All 16 failures on head fail on the diagnostic count only (fewer reported than expected), i.e. on the missing line named in the table.

## Verification against `main`

`RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` temporarily restored to `c2866ea` (reverted afterwards, not committed): 11 failed, 42 passed, 53 total.
- All 7 new tests and 7 of the 9 turned tests pass, so their expectations are `main`'s output exactly.
- `VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot`: `main` reports 5 — the expected (3,1) and (5,5), plus the `<!--`/`@*` lines and the mid-line `"@code{"`, which kept parts (comments, Q3) do not report. The refused line (5,5) is reported by `main`.
- `VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported`: `main` reports 4 — the expected three plus the mid-line `"@code{"` (Q3). The refused lines (3,5) and (7,1) are reported by `main`.
- The other 9 failures are the run's reproductions of kept parts (shared example ×2, escaped/email, mid-line, `@@` at line start, multi-line Razor ×2 and HTML comment, Razor comment inside a block) — expected to differ from `main`.

## Not covered, and why

- Auditor 16 finding #4 (line-starting `@*`/`<!--` inside a multi-line C# literal or comment after a template, and now in any block): decision 13 accepts this miss; per the lead's instruction no test pins it.
- The `@functions` variant of `<text>` and inline markup: same scanning path as the `@functions` `__builder` method tests (the block keyword only selects the start; decision 13 removes the block lexing for both keywords alike). Not duplicated.
- CRLF: the change removes a block lexer and adds no line-ending decision; CRLF stays covered by the existing `…WithCarriageReturnLineFeed…` tests.
- `VerifyFunctionsBlockIsNotReported` (thoenissen/Reihitsu#1452) unchanged.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"
```

Result on 91822fd plus these tests: Failed 16 (9 turned, 7 new), Passed 37, Total 53.
