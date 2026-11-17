Role: test implementer
Commit: 704281fc725ea17e9820674d6d4fa55b63b52923
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane tests: no production code changed. Every test below passes on the commit above, under LF and CRLF (every test is a `[DataRow("\n")]` / `[DataRow("\r\n")]` pair). Expected values are the HEAD outputs `01-skeptic.md` measured, each re-derived here from SHARED-CHAIN-007/-008/-009 and SHARED-FIX-006 (anchor column, code in front kept as written); none comes from a patched build (FC-035). Every fix test runs through `VerifyCodeFixWithEndOfLine` (diagnostic, fixed source, re-analysis clean, convergence in the default iteration count of one diagnostic = one pass, and an exact text comparison including line endings) or through `Verify` with `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1`.

Files:
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs` (one `using Microsoft.CodeAnalysis;` added for `OutputKind`)
- `src/Reihitsu.Analyzer.Test/Formatting/RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.cs`

## thoenissen/Reihitsu#889

Per decision 1 of `00-lead.md` (option 1A of `01-skeptic.md`): mid-line layout tests plus refusal-scope tests.

Mutation check (FC-003, the side of the boundary the clause decides): a detached worktree in the scratchpad (removed afterwards) took both test files and removed `&& SyntaxTokenPositionUtilities.IsFirstOnLine(node.GetFirstToken())` from `GetContextNode` (`src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs:80`). Result: exactly the four refusal-scope runs failed (`No code fix may be offered for the diagnostic at [113..114)` — a fix was offered); all 137 other RH5201/RH5112 tests, every new layout test among them, passed on the mutant. This confirms the skeptic's finding that the refusal scope is the clause's only observable effect.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5201 `VerifyFixOfChainInEmbeddedStatementThatStartsMidLineKeepsTheCodeInFrontAsWritten` (LF, CRLF) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: `if(k)return  a.B()` kept, `.C();` at column 22 |
| RH5201 `VerifyFixOfChainInStatementBehindCaseLabelKeepsTheCodeInFrontAsWritten` (LF, CRLF) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: `case 1:return  a.B()` kept, `.C();` at 28 |
| RH5201 `VerifyFixOfChainInLambdaBlockOpenedMidLineKeepsTheCodeAroundAsWritten` (LF, CRLF) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: `Run(()=>{ var r=a.B()` kept, `.C(); });` at 25 |
| RH5201 `VerifyFixOfChainInTopLevelStatementBehindUsingDirectiveKeepsTheCodeInFrontAsWritten` (LF, CRLF) | RH5201-005, SHARED-CHAIN-009, SHARED-FIX-006 | `Verify` with `OutputKind.ConsoleApplication`, 1 iteration | pass: null-context branch (`FormatNodeInDocumentAsync`), `.C();` at 38 |
| RH5201 `VerifyFixOfChainWithWrappedArgumentInEmbeddedStatementThatStartsMidLineAlignsTheArgument` (LF, CRLF) | RH5201-005, SHARED-CHAIN-008; FC-027 | `VerifyCodeFixWithEndOfLine` | pass: `2)` at 25, `.C();` at 22 |
| RH5201 `VerifyFixOfChainWithLambdaBlockInEmbeddedStatementThatStartsMidLineAlignsTheBlock` (LF, CRLF) | RH5201-005, SHARED-CHAIN-008; FC-027 | `VerifyCodeFixWithEndOfLine` | pass: braces at 25, `return 1;` at 29, `.C();` at 27 |
| RH5201 `NoFixIsOfferedWhenTheStatementThatStartsTheLineOfAMidLineChainHoldsACommentTheFormatterRefuses` (LF, CRLF) | SHARED-FIX-010; FC-003, FC-043 | `VerifyNoCodeFixIsOffered` | pass: `if(k && /* c */ j)return  a.B()` reported, no fix; fails when the clause is removed |
| RH5201 `NoFixIsOfferedWhenTheStatementThatStartsTheLineOfAChainInALambdaBlockHoldsACommentTheFormatterRefuses` (LF, CRLF) | SHARED-FIX-010; FC-003, FC-043 | `VerifyNoCodeFixIsOffered` | pass: `1 + /* c */ 2` behind the lambda, no fix; fails when the clause is removed |
| RH5112 `VerifyFixOfChainInEmbeddedStatementThatStartsMidLineKeepsTheCodeInFrontAsWritten` (LF, CRLF) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: joined to `if(k)return  a.B()`, `.C();` at 22 |
| RH5112 `VerifyFixOfChainInStatementBehindCaseLabelKeepsTheCodeInFrontAsWritten` (LF, CRLF) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: `case 1:return  a.B()`, `.C();` at 28 |
| RH5112 `VerifyFixOfChainInLambdaBlockOpenedMidLineKeepsTheCodeAroundAsWritten` (LF, CRLF) | RH5112-003, SHARED-CHAIN-009, SHARED-FIX-006 | `VerifyCodeFixWithEndOfLine` | pass: `Run(()=>{ var r=a.B()`, `.C(); });` at 25 |

### Scenarios (bug fix)
_N/A_ — tests lane, no bug reported.

### Not covered, and why
- Layout tests cannot detect removal of the `IsFirstOnLine` clause (mutation check above); #889's goal "a layout test fails without the clause" is replaced by the two refusal-scope tests (decision 1). The misspacing `return  a` is kept as the SHARED-FIX-006 observable, not as a column probe.
- Other headers of an embedded statement (`else`, loops, `using (…)`): same `StatementSyntax`-behind-a-header shape as `if`; the climb in `GetContextNode` is one `Ancestors().FirstOrDefault(node is StatementSyntax or MemberDeclarationSyntax && IsFirstOnLine(...))` call with no per-kind branch, so `if` represents them.
- The skeptic's `case`-label and `Func<int,int>` cast variants of the wide-line fixtures: same call site and same SHARED-CHAIN-008 path as the two wide-line tests written; left out to keep the set small.
- Fix All (FC-008): one diagnostic per fixture; the classes' `GetFixAllScenario` already covers several diagnostics in one document, and the context choice does not differ between single and batch application.
- FC-019/FC-020 (comment/directive in a gap): no gap is joined by the context decision; the refusal tests carry the comment case. Directives were not added — no FMT-UNSUP shape with a directive is needed to show the refusal scope.
- FC-010/FC-012 formatter idempotency: no formatter change.

## thoenissen/Reihitsu#1476

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5201 `VerifyNoDiagnosticForChainBehindLineLeadingCommentLaidOutAsTheFormatter` (LF, CRLF) | RH5201-002; FC-013 | `AssertFormattingChangesNothing` + `Verify(source)` | pass: `/* c */ s.Trim()` at 8, `.ToUpper` at 17, formatter-stable, not reported |
| RH5201 `VerifyNoDiagnosticForChainBehindMultiLineCommentTailAndCommentLaidOutAsTheFormatter` (LF, CRLF) | RH5201-002; FC-013, FC-021 | `AssertFormattingChangesNothing` + `Verify(source)` | pass: `/* a` at 8, `b */ /* c */ s.Trim()` at 11, `.ToUpper` at 25 |
| RH5201 `VerifyNoDiagnosticForChainDirectlyBehindMultiLineCommentTailLaidOutAsTheFormatter` (LF, CRLF) | RH5201-002; FC-013, FC-021 | `AssertFormattingChangesNothing` + `Verify(source)` | pass: `/* a` at 8, `b */ s.Trim()` at 12, `.ToUpper` at 18 |
| RH5201 `VerifyNoDiagnosticForAlignedChainBehindIndentedLineLeadingComment` (LF, CRLF) | RH5201-002 | `Verify(source)` | pass: comment at 12 (the issue's snippet column), `.ToUpper` at 21, not reported |
| RH5201 `VerifyMisalignedLinkOfChainBehindLineLeadingCommentIsReportedAndFixed` (LF, CRLF) | RH5201-002, RH5201-005; FC-007, FC-013 | `VerifyCodeFixWithEndOfLine` + `Format` comparison | pass: `.ToUpper` at 9 reported, fixed to 17 = formatter output of the same input |
| RH5201 `VerifyMisalignedLinkOfChainBehindMultiLineCommentTailAndCommentIsReportedAndFixed` (LF, CRLF) | RH5201-002, RH5201-005; FC-007, FC-013, FC-021 | `VerifyCodeFixWithEndOfLine` + `Format` comparison | pass: fixed to 25 = formatter output |
| RH5201 `VerifyFixOfChainBehindIndentedLineLeadingCommentKeepsTheCommentLine` (LF, CRLF) | RH5201-002, RH5201-005, SHARED-CHAIN-009 | `VerifyCodeFixWithEndOfLine` | pass: comment line stays at 12, `.ToUpper` at 21 |
| RH5201 `VerifyFixOfChainDirectlyBehindMultiLineCommentTailKeepsTheComment` (LF, CRLF) | RH5201-002, RH5201-005, SHARED-CHAIN-009 | `VerifyCodeFixWithEndOfLine` | pass: comment as written, `.ToUpper` at 18 |
| RH5112 `VerifyFixOfChainBehindLineLeadingCommentMatchesTheFormatter` (LF, CRLF) | RH5112-003; FC-013 | `VerifyCodeFixWithEndOfLine` + `Format` comparison | pass: `/* c */ s.Trim()` / `.ToUpper` at 17 = formatter output |

The three cases the issue asks for: formatter output not reported (first three rows), old layout reported and fixed to the formatter output (rows 5–6), and the fix converging in one pass (every fix row).

### Scenarios (bug fix)
_N/A_ — tests lane.

### Observation against `01-skeptic.md`
The skeptic's table lists `            /* a` / `            b */ s.Trim()` / `                  .ToUpper();` (comment opening at column 12) as formatter output. It is not: `AssertFormattingChangesNothing` failed on it under LF and CRLF — the formatter moves the `/* a` line to column 8 (the column of `return`) and keeps the `b */` line as written. The test uses the measured output (`/* a` at 8). The column-12 form is still covered as fix input by `VerifyFixOfChainDirectlyBehindMultiLineCommentTailKeepsTheComment`. No disagreement between RH5201, its fix, and the formatter was found; nothing to file against RH5201.

### Not covered, and why
- Directive leading the line (FC-020): a directive occupies its own line and cannot lead the root's line; not the issue's construct.
- `//` comment leading the line: a single-line comment ends its line, so the root cannot stand behind it on the same line.
- Fix All: one diagnostic per fixture; covered by `GetFixAllScenario`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests"
```

Result on 704281f with the new tests: 141 passed, 0 failed.

## Follow-up candidates

_None._ (The refusal-scope narrowing was dropped by decision 2.)
