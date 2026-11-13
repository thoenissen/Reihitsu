Role: test implementer
Commit: 4765235 (tests added on top; see the commit carrying this handover)
Verdict: tests written

Repair round 2, lead decision 11 (auditor 11, thoenissen/Reihitsu#1289 findings #1 and #2). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decision itself; the expected values come from RH2109-001 and decision 11 (a real line-starting directive is reported, as on `main`).

## Tests

File: `src/Reihitsu.Analyzer.Test/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.cs`. Each test puts one auditor counterexample in `Counter.razor`, followed by a real line-starting `@code {`, and asserts the exact set of diagnostics `main` reports (exact span, exact count). No fixture holds a markup token (`$$`, `[|`, `|]`, `{|`, `|}`), so markup handling stays at its default (the 06 concern does not apply). All existing tests are unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyHtmlCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective` (`"<!--"` in `@{ }`, then `@code {` at (5,1)) | RH2109-001; FC-002 | `Verify(TestCode, onConfigure, RazorCodeBlockSpan…)` | fail — 0 reported, (5,1) expected |
| `VerifyHtmlCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirectiveBeforeLaterHtmlComment` (same, with a real `<!-- real -->` after the directive) | RH2109-001; FC-002 | same | fail — 0 reported, (4,1) expected |
| `VerifyRazorCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective` (`"@*"` in `@{ }`) | RH2109-001; FC-002 | same | fail — 0 vs (4,1) |
| `VerifyHtmlCommentOpenerInImplicitExpressionDoesNotHideDirective` (`@M("<!--")`) | RH2109-001; FC-016 | same | fail — 0 vs (2,1) |
| `VerifyRazorCommentOpenerInExplicitExpressionDoesNotHideDirective` (`@("@*")`) | RH2109-001; FC-016 | same | fail — 0 vs (2,1) |
| `VerifyHtmlCommentOpenerInCSharpCommentOfControlFlowBlockDoesNotHideDirective` (`// <!--` in `@if { }`) | RH2109-001; FC-016 | same | fail — 0 vs (5,1) |
| `VerifyRazorCommentOpenerAfterLetterDoesNotHideDirective` (`<p>user@*x</p>`) | RH2109-001; FC-002 | same | fail — 0 vs (2,1) |
| `VerifyHtmlCommentOpenerInAttributeValueDoesNotHideDirective` (`<input value="<!--" />`) | RH2109-001; FC-016 | same | fail — 0 vs (2,1) |
| `VerifyHtmlCommentOpenerInScriptStringDoesNotHideDirective` (`"<!--"` in `<script>`) | RH2109-001; FC-016 | same | fail — 0 vs (4,1) |
| `VerifyOpeningBraceInTemplateMarkupOfCodeBlockDoesNotHideLaterDirective` (`@<p>{</p>` in `@code`, second `@code`) | RH2109-001; FC-027 | same | fail — 1 reported, (1,1) and (4,1) expected |
| `VerifyBlockCommentOpenerInTemplateMarkupOfCodeBlockDoesNotHideLaterDirective` (`@<p>/* note</p>` in `@code`) | RH2109-001; FC-027 | same | fail — 1 vs (1,1), (4,1) |
| `VerifyOpeningBraceInTemplateMarkupOfFunctionsBlockDoesNotHideLaterDirective` (`@<p>{</p>` in `@functions`, then `@code`) | RH2109-001; FC-027 | same | fail — 0 vs (4,1) |
| `VerifyBlockCommentOpenerInTemplateMarkupOfFunctionsBlockDoesNotHideLaterDirective` (`@<p>/* note</p>` in `@functions`) | RH2109-001; FC-027 | same | fail — 0 vs (4,1) |

Why they fail on this head (observation only): in every case the line-starting `@code {` after the opener or template is not reported; the diagnostic count is short by exactly that directive. The template cases still report the first `@code` block at (1,1).

Expected values checked against `main`: with `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` temporarily restored to `c2866ea` (reverted afterwards, not committed), all 13 new tests pass; the only failures there are the run's 18 earlier reproductions, as expected. So the new tests pin `main`'s output for these inputs, which is what decision 11 requires.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Auditor #1, 8 members (+ later `-->` variant) | LF | RH2109 analyzer | reproduced | directive after opener missed vs reported |
| Auditor #2, `{` and `/*` in template markup, inside `@code` and inside `@functions` | LF | RH2109 analyzer | reproduced | directive after block missed vs reported |

## Not covered, and why

- CRLF variants: the scanner's line-start check is already tested on CRLF (`…WithCarriageReturnLineFeed…`); the defect is the extent of a skipped region, not a line-ending decision. Not duplicated per member.
- The remaining false positives in these rare shapes (e.g. a `@code {` line that really is inside such a C# string or template) are not pinned either way: decision 11 lets them fall back to `main`'s behavior (reported), and they join the decision-7 follow-up; a test would pin a known gap.
- The `@*`/`<!--` openers inside a lexed `@code`/`@functions` block and `@@`: already covered and holding (auditor 11).
- `'` and `"` in template markup: hold per auditor (Roslyn ends the literal at the line end); a `}` in template markup closes the block early but is no regression (auditor 11). Not added.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"
```

Result on 4765235 plus these tests: Failed 13 (the new tests), Passed 33 (all existing), Total 46.
