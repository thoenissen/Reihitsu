Role: test implementer
Commit: c57b601
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

Paths: `F` = `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterUsingDirectivesTests.cs` (through `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync`, the fixes' entry point; LF and CRLF in every test), `R` = `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveRequirementTests.cs`, `A72xx` = the RH72xx analyzer test class under `src/Reihitsu.Analyzer.Test/Ordering/` (RH7207: `Formatting/`). Expected values are the FMT-USING-011 spec examples, or for members the spec states only in prose, the same block laid out by FMT-USING-008/009/010 (unchanged) at the original first directive's whitespace.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| F `HeaderWithoutBlankLineChangesNoDirectiveIndentationInANamespaceWithABody` (spec example 1, issue input) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `HeaderWithBlankLineChangesNoDirectiveIndentationInANamespaceWithABody` (spec example 2) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `CommentLeadingTheNewFirstDirectiveStartsAtTheOriginalFirstDirectiveIndentation` (spec example 3) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `BlockWithoutHeaderKeepsItsIndentationInANamespaceWithABody` (spec: "the same holds without the header") | FMT-USING-011 | `AssertOrganizes` | pass |
| F `TabIsKeptInFrontOfEveryDirectiveBelowAHeaderWithoutBlankLine` (spec tab prose, answer 2A) | FMT-USING-011 | `AssertOrganizes` + `IndentWithTabs` | fail |
| F `TabIsKeptInFrontOfEveryDirectiveBelowAHeaderWithBlankLine` | FMT-USING-011 | same | fail |
| F `HeaderChangesNoDirectiveIndentationInAnIndentedFileScopedNamespace` (spec example 4, answer 1A) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `HeaderChangesNoDirectiveIndentationInAnIndentedCompilationUnit` (spec prose, u1) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `HeaderChangesNoDirectiveIndentationInANestedNamespace` (g, FC-027) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `DelimitedHeaderChangesNoDirectiveIndentation` (c, FC-021) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `MultiLineDelimitedHeaderChangesNoDirectiveIndentation` (c5) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `SingleLineDocumentationHeaderChangesNoDirectiveIndentation` (c1) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `DelimitedDocumentationHeaderChangesNoDirectiveIndentation` (c2) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `HeaderOfTwoCommentLinesChangesNoDirectiveIndentation` (c4) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `HeaderBelowABlankLineAfterTheBraceChangesNoDirectiveIndentation` (b3) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `OriginalFirstDirectiveMovedBehindAMiddleDirectiveKeepsItsIndentation` (f) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `OriginalFirstDirectiveMovedIntoAnotherGroupKeepsItsIndentation` (nb) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `CommentMovingWithTheOriginalFirstDirectiveKeepsItsIndentation` (b1) | FMT-USING-011 | `AssertOrganizes` | fail |
| F `CommentOnTheOriginalFirstDirectivesLineChangesNoDirectiveIndentation` (c3, holds side) | FMT-USING-011 | `AssertOrganizes` | pass |
| F `CommentLeadingASecondDirectiveThatMovesChangesNoDirectiveIndentation` (e, holds side) | FMT-USING-011 | `AssertOrganizes` | pass |
| F `HeaderKeepsEveryDirectiveInColumnZeroInACompilationUnit` (d, both sides of the split) | FMT-USING-011 | `AssertOrganizes` | pass |
| F `HeaderKeepsEveryDirectiveInColumnZeroInAFileScopedNamespace` (s2) | FMT-USING-011 | `AssertOrganizes` | pass |
| R `ReorderedBlockBelowAHeaderWithoutBlankLineIsIndentedInANamespaceWithABody` (whole-document guard, a) | FC-033, FC-010 (no tag) | `AssertRuleResult` (LF/CRLF, second pass, phase stability) | pass |
| R `ReorderedBlockBelowAHeaderWithBlankLineIsIndentedInANamespaceWithABody` (whole-document guard, h) | FC-033, FC-010 (no tag) | `AssertRuleResult` | pass |
| A7203 `CodeFixKeepsNamespaceIndentationBelowAHeaderWithoutBlankLine` (LF, CRLF) — **the reproduction** | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationBelowAHeaderWithBlankLine` (LF, CRLF; h) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationOfTheCommentLeadingTheNewFirstDirective` (LF, CRLF; b2) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationOfACommentMovingWithTheOriginalFirstDirective` (LF, CRLF; b1) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationBelowADelimitedHeader` (LF, CRLF; c) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationBelowADocumentationHeader` (LF, CRLF; c1) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationWhenTheOriginalFirstDirectiveMovesToTheEnd` (LF, CRLF; f, two diagnostics) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNestedNamespaceIndentationBelowAHeader` (LF, CRLF; g) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsTabIndentationBelowAHeader` (LF, CRLF; t1) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsIndentationOfIndentedFileScopedNamespaceDirectivesBelowAHeader` (LF, CRLF; s1) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsIndentationOfIndentedCompilationUnitDirectivesBelowAHeader` (LF, CRLF; u1) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | fail |
| A7203 `CodeFixKeepsNamespaceIndentationWithoutHeader` (LF, CRLF; b) | RH7203-004, FMT-USING-011 | `VerifyCodeFixWithEndOfLine` | pass |
| A7203 `FixAllKeepsNamespaceIndentationOfTwoBlocksBelowHeaders` (two namespaces, both sides of the split) | RH7203-004, FMT-USING-011 | `Verify` (iterative and Fix All) | fail |
| A7201 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`Zeta.Alpha; System`) | RH7201-004, FMT-USING-011 | `Verify`, `CompilerDiagnostics.None` | fail |
| A7202 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`A = System.Text; System`) | RH7202-004, FMT-USING-011 | `Verify` | fail |
| A7204 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`B = …; A = …`) | RH7204-004, FMT-USING-011 | `Verify` | fail |
| A7205 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`static System.Math; System`) | RH7205-004, FMT-USING-011 | `Verify` | fail |
| A7206 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`static Math; static Console`) | RH7206-004, FMT-USING-011 | `Verify` | fail |
| A7207 `CodeFixKeepsNamespaceIndentationBelowAHeader` (`Zeta.Alpha; System`) | RH7207-004, FMT-USING-011 | `Verify`, `CompilerDiagnostics.None` | fail |

Every failing test was watched fail on this head, and each fails only on the leading whitespace of one line — the moved original first directive, or on the blank-line side the new first directive or its leading comment — which is column 0 instead of the original first directive's whitespace (4, 8, or a tab). Diagnostics and every other line match. The analyzer `Verify` tests also re-run the analyzer on the fixed code, so one-pass convergence (FC-007) is asserted.

`RequirementCoverageTests` now passes (FMT-USING-011 is tagged); the 35 requirement architecture tests pass.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (a), RH7203 fix | LF | code fix | reproduced | `using System.Linq;` at column 0 vs column 4 |
| Issue input (a), RH7203 fix | CRLF | code fix | reproduced | same |
| Issue input (a), entry point `OrganizeUsingDirectivesInDocumentAsync` | LF, CRLF | formatter (fix path) | reproduced | same |
| Issue input (a) and h, whole-document formatting | LF, CRLF | formatter (CLI path) | not affected | column 4, stable on second pass, as expected |
| Fix All, two block namespaces | LF | code fix | reproduced | both blocks have one line at column 0 |
| Nearest sibling: nested namespace (g) | LF, CRLF | code fix | reproduced | column 0 vs 8 |
| Blank-line side (h, b1, b2) | LF, CRLF | code fix | reproduced | new first directive / its comment at column 0 |
| Six other rules, block namespace | LF | code fix | reproduced | one line at column 0 vs 4 |

## Not covered, and why

- Other rules under CRLF and on the blank-line side: all seven providers reach `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync` → `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` (`rg -n "OrganizeScopeUsingsAsync" src/Reihitsu.Analyzer.CodeFixes`: the shared base `UsingDirectiveOrderingCodeFixProviderBase` for RH7201–RH7206 and `RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider`), and the provider only chooses the scope. The line-ending and split dimensions are covered on RH7203 and on the entry point; each other provider gets one test so a provider that stopped routing there would fail.
- FC-004, FC-020: no comment is dropped or relocated by the change, and blocks with directives get no fix (`UsingDirectiveOrderingSafety.CanSafelyReorder`), as the skeptic states. FC-019's comment case is the whole matrix; a directive in the gap cannot reach the phase.
- FC-008 overlap: covered by the Fix All test and the f test (two diagnostics in one block, one span).
- FC-018, FC-036 (naming), FC-006, FC-022–FC-026, FC-029 (syntax forms), FC-038–FC-043 (no shape declared unsupported), FC-014 (no new rule), FC-037 (no comment-only change): not reachable by this change.
- FC-028: `FormatNode` / `FormatNodeInDocumentAsync` are not the fixes' path; the fixes call `OrganizeUsingDirectivesInDocumentAsync` only.
- FC-031 (stale summaries of `SplitOriginalFirstHeaderTrivia` / `CreateLeadingTrivia`) is for the implementer and reviewer; no test can assert it.
- FC-013 / RH5204: the skeptic found no layout rule reporting the column-0 line; the existing RH72xx `Verify` re-analysis covers only the rule itself, so no parity test was added.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests|FullyQualifiedName~UsingDirectiveRequirementTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7201|FullyQualifiedName~RH7202|FullyQualifiedName~RH7203|FullyQualifiedName~RH7204|FullyQualifiedName~RH7205|FullyQualifiedName~RH7206|FullyQualifiedName~RH7207"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Requirement"
```
