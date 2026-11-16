Role: test implementer
Commit: f2abd60
Verdict: #1764: tests written (reproduced); #1765: tests written (reproduced)

Inputs: `00-lead.md` (answers 1B, 2B; decisions 1–8), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, change doc `sdd/changes/2026-11/rh7105-closed-safe-modifiers.md`, specs RH7105, RH5010, RH5011, RH5012, RH7501. The prompt carried no intended fix or verdict. The issues carry a suspected cause and a prescribed solution each (#1764: explicit ranks; #1765: `GetNextToken`); they were ignored, and the tests assert only spec-derived outputs.

Every test listed as "fails" below was run on f2abd60 with only these test files added and fails on the difference named in its row; every "passes" row was run green on the same tree. No production code was touched.

## thoenissen/Reihitsu#1764

### Tests

`src/Reihitsu.Analyzer.Test/Ordering/RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.cs` (class `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ClosedInFrontOfPartialOnAClassIsNotReported` (issue's own input, doc comment included) | RH7105-001, RH7105-002; FC-024, FC-034 | `Verify(source, onConfigure)` | fails: reported at (4,8), on `closed` |
| `ClosedInFrontOfPartialOnARecordIsNotReported` | RH7105-002 | `Verify(source, onConfigure)` | fails: reported on `closed` |
| `ClosedInFrontOfNewAndPartialIsNotReported` | RH7105-002 | `Verify(source, onConfigure)` | fails: reported on `closed` |
| `ClosedInFrontOfUnsafeAndPartialIsNotReported` | RH7105-002 | `Verify(source, onConfigure)` | fails: reported on `closed` |
| `NewInFrontOfClosedIsReportedOnNewAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | fails: diagnostic on `closed` (col 16), expected on `new` (col 12) |
| `UnsafeInFrontOfClosedIsReportedOnUnsafeAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | fails: diagnostic on `closed`, expected on `unsafe` |
| `ClosedInFrontOfTheAccessibilityIsReportedAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | passes (holds today, must keep holding) |
| `ClosedBehindTheAccessibilityWithoutPartialIsNotReported` (`public closed class`, `public closed record class`, `file closed class`) | RH7105-002 | `Verify(source, onConfigure)` | passes |
| `SpecExampleClosedAndSafeOutOfOrderAreReported` | RH7105-001, RH7105-003 | `Verify(source, onConfigure)` | fails: first diagnostic on `closed`, expected on `new` |
| `SpecExampleClosedAndSafeInOrderAreNotReported` | RH7105-002 | `Verify(source, onConfigure)` | fails: 7 diagnostics, expected 0 |
| `SpecExampleClosedAndSafeAreFixed` (two diagnostics in one document, so the verifier's Fix All pass runs over both) | RH7105-004; FC-007 | `Verify(source, fixedSource, onConfigure)` | fails: diagnostic on `closed`, expected on `new` |
| `SafeInFrontOfPartialOnAMethodIsNotReported` (both parts) | RH7105-001, RH7105-002; SHARED-DIAG-004 context, FC-024 | `Verify(source, onConfigure)` | fails: 2 diagnostics on `safe` |
| `SafeInFrontOfRefOnAStructIsNotReported` | RH7105-001, RH7105-002; FC-016 | `Verify(source, onConfigure)` | fails: reported on `safe` |
| `SafeInFrontOfRequiredIsNotReported` | RH7105-002 | `Verify(source, onConfigure)` | fails: reported on `safe` |
| `RequiredInFrontOfSafeIsReportedOnRequiredAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | fails: not reported, expected on `required` |
| `SafeInFrontOfExternIsReportedOnSafeAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | passes |
| `SafeInFrontOfAsyncIsReportedOnSafeAndFixed` | RH7105-001, RH7105-003, RH7105-004 | `Verify(source, fixedSource, onConfigure)` | passes |
| `SafeInFrontOfReadonlyOnARefPartialStructIsReportedAndFixed` | RH7105-001, RH7105-003, RH7105-004; FC-016 | `Verify(source, fixedSource, onConfigure)` | fails: fix writes `public readonly ref partial safe struct S`, expected `public readonly safe ref partial struct S` |

`src/Reihitsu.Core.Test/ModifierOrderingUtilitiesTests.cs` (class `ModifierOrderingUtilitiesTests`), via a new private `AssertRh7105Order(first, second)` that checks the pair in order is accepted and the reverse is reported on its first token and reordered:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Rh7105RanksClosedInThePositionOfSealed` — rows `public/file/static` before `closed` | RH7105-001 | `TryGetRh7105Violation`, `OrderModifiersForRh7105` | passes |
| `Rh7105RanksClosedInThePositionOfSealed` — rows `closed` before `new/unsafe/ref/partial` | RH7105-001 | same | fails (4 rows) |
| `Rh7105RanksSafeInThePositionOfUnsafe` — rows `public/static/new/readonly/async/extern` before `safe` | RH7105-001 | same | passes |
| `Rh7105RanksSafeInThePositionOfUnsafe` — rows `safe` before `required/ref/partial` | RH7105-001 | same | fails (3 rows) |
| `Rh7105RanksUnnamedModifiersLast` — `partial/ref` before `const/fixed` | RH7105-001 | same | passes |

`src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyPrivateGetOnlySafeAutoPropertyCodeFixWritesReadonlyInFrontOfSafe` (`private safe int Count { get; } = 1;` → `private readonly safe int _count = 1;`) | RH2001-004; FC-016 (second consumer of `OrderModifiersForRh7105`) | `Verify(source, fixedSource, onConfigure, diagnostic)` | passes (must keep passing) |

Test infrastructure added to the RH7105 test class (test code only):
- `PreviewLanguageVersion` / `ApplyPreviewLanguageVersionToTestProject`: set `LanguageVersion.Preview` after the verifier's own `Latest` transform; every `safe` test uses it (FC-033).
- `IsClosedTypeAttributeSource` with `ClosedTypes` (analyzer tests) and `ClosedTypesInFix` (adds it to `TestState` and `FixedState`): the verifier's `net9.0` reference assemblies do not declare `System.Runtime.CompilerServices.IsClosedTypeAttribute`, so every `closed` declaration was CS0656 without it. With it, the source and the fixed code are compiled, so a fix result such as `partial closed` (CS0267) or `ref safe` (CS1585) fails the test as a compile error.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input `/// doc` + `public closed partial class Gate` | LF | analyzer | reproduced | reported at (4,8) on `closed` (not on `public` as the issue states) vs not reported |
| `public new closed partial class N` | LF | analyzer + fix | reproduced | reported on `closed` vs on `new`, fixed to `closed new partial` |
| `public safe readonly ref partial struct S` | LF | fix | reproduced | `readonly ref partial safe` (does not compile) vs `readonly safe ref partial` |

### Not covered, and why

- Line endings for RH7105: modifier reordering writes single spaces between modifiers on one line (SHARED-MODORDER-002); the existing CRLF/trivia tests of the class cover that path, and these inputs change only which order is computed, not how it is written.
- `closed` with `abstract`/`sealed`/`static`, `safe` with `unsafe`, `closed` on structs: do not compile (CS9381/CS9384/CS9388/CS0106), so no caller produces them; the spec leaves the pair order undecided.
- `const`/`fixed` in front of other modifiers: the C# parser does not accept that (`const public int` is a syntax error), so the analyzer test I drafted was dropped; the Core rank test covers "every other modifier last".
- `partial safe` / `partial closed` as input: does not compile, no caller.
- Comment/directive in the modifier gap: unchanged code path (SHARED-MODORDER-002/-003), covered by existing tests; `closed`/`safe` only change the rank.
- Local functions with `safe`: thoenissen/Reihitsu#1338's scope (skeptic).
- Formatter: no phase orders modifiers (skeptic).

### Observation for the implementer's route

`SyntaxKind.SafeKeyword` is marked experimental in Roslyn 5.12: referencing it fails the build with `RSEXPERIMENTAL006` (seen when a Core test used it). The Core test therefore takes the `safe` token from preview source and compares token text. A production reference to that member would need a suppression, which is escalation 6; I did not investigate alternatives.

## thoenissen/Reihitsu#1765

### Tests

`src/Reihitsu.Analyzer.Test/Formatting/RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests.cs` (class `RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests`; two shared sources added as constants):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoDiagnosticForLabeledBreakBeforeClosingBraceOfBlock` (issue's own input) | RH5011-002; FC-034 | `Verify(source)` | fails: reported |
| `VerifyNoDiagnosticForLabeledBreakBeforeClosingBraceOfSwitch` | RH5011-002 | `Verify(source)` | fails: reported |
| `VerifyNoDiagnosticForLabeledBreakBeforeDirective` | RH5011-002; directive in the gap | `Verify(source)` | fails: reported |
| `VerifySpecExampleLabeledBreakBeforeClosingBraceOrDirectiveIsNotReported` | RH5011-002 | `Verify(source)` | fails: reported |
| `VerifyNoDiagnosticForLabeledBreakBeforeElseOnTheSameLine` | RH5011-008 | `Verify(source)` | fails: reported |
| `VerifyNoDiagnosticForLabeledBreakBeforeElseOnTheNextLine` | RH5011-008 | `Verify(source)` | fails: reported |
| `VerifyNoDiagnosticForLabeledBreakBeforeDoStatementFooter` | RH5011-008 | `Verify(source)` | fails: reported |
| `VerifySpecExampleLabeledBreakBeforeElseOrDoFooterIsNotReported` | RH5011-008 | `Verify(source)` | fails: reported |
| `VerifyLabeledBreakBeforeWhileStatementIsReportedAndFixed` (other side of RH5011-008's condition) | RH5011-001, RH5011-008; FC-003, FC-007 | `Verify(source, fixedSource)` | fails: re-reported on the fixed code |
| `VerifyNoDiagnosticForLabeledBreakFollowedByBlankLine` | RH5011-001 | `Verify(source)` | fails: reported |
| `VerifyLabeledBreakFollowedByStatementIsReportedAndFixedWithLineFeed` | RH5011-001, RH5011-003, RH5011-004; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\n")` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakFollowedByStatementIsReportedAndFixedWithCarriageReturnLineFeed` | RH5011-001, RH5011-004; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\r\n")` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakFollowedByStatementOnTheSameLineIsReportedAndFixedWithLineFeed` | RH5011-001, RH5011-005; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\n")` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakFollowedByStatementOnTheSameLineIsReportedAndFixedWithCarriageReturnLineFeed` | RH5011-001, RH5011-005; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\r\n")` | fails: re-reported on the fixed code |
| `VerifyBreakNamingItsSwitchFollowedBySectionIsReportedAndFixed` (`break sw;`) | RH5011-001, RH5011-004; FC-007 | `Verify(source, fixedSource)` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakAsEmbeddedBodyFollowedByStatementIsReportedAndFixed` | RH5011-001, RH5011-004; FC-007 | `Verify(source, fixedSource)` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakFollowedByCommentLineIsReportedAndFixedAboveTheComment` | RH5011-001, RH5011-004; comment in the gap | `Verify(source, fixedSource)` | fails: re-reported on the fixed code |
| `VerifyLabeledBreakWithCommentBeforeSameLineStatementIsReportedWithoutFix` | RH5011-006; comment in the gap | `VerifyNoCodeFixIsOffered` | passes (must keep passing) |
| `VerifyLabeledBreakWithCommentSpanningItsLineIsFixedBelowTheComment` | RH5011-007; FC-007 | `Verify(source, fixedSource)` | fails: re-reported on the fixed code |
| `VerifyFixAllCorrectsSeveralLabeledBreakStatements` (two reported, one before `}` not reported) | RH5011-001, RH5011-002, RH5011-004; FC-007 | `Verify(source, fixedSource)` (verifier runs Fix All) | fails: 3 diagnostics, expected 2 |

`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5011BreakStatementsShouldBeFollowedByABlankLineFormatterTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFormatterKeepsLabeledBreakStatementsAndAnalyzerReportsNothing` (labeled `break` before `}`, before a `do` footer, above a blank line; LF and CRLF) | SHARED-DIAG-010; FC-013 | `VerifyFormatter(source)` | fails: RH5011 diagnostics on the formatter's (unchanged) output |
| `VerifyFormatterFixesLabeledBreakViolation` (LF and CRLF, second pass) | SHARED-DIAG-010; FC-013 | `VerifyFormatter(source, fixedSource, expected)` | fails: RH5011 re-reported on the formatted output |

The input of the first formatter test was confirmed stable by running `scripts/format.sh` on it. An unbraced `if … break outer; else …` was left out of it because the formatter braces it (the analyzer test covers that shape).

Sibling rules (all pass today; they pin the specs' labeled clarifications):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5010…AnalyzerTests.VerifySpecExampleLabeledBreakEndingSectionIsExemptAndLabeledBreakInsideBlockIsReported` | RH5010-002; FC-016 | `Verify(source)` | passes |
| `RH5010…AnalyzerTests.VerifyLabeledBreakFollowingAStatementIsReportedAndFixed` | RH5010-001, RH5010-003; FC-016 | `Verify(source, fixedSource)` | passes |
| `RH5012…AnalyzerTests.VerifyLabeledContinueFollowingAStatementIsReportedAndFixed` | RH5012-001, RH5012-002; FC-016 | `Verify(source, fixedSource)` | passes |
| `RH5012…AnalyzerTests.VerifyLabeledContinueFirstInBlockOrAfterBlankLineIsNotReported` | RH5012-001 | `Verify(source)` | passes |
| `RH7501…AnalyzerTests.VerifySpecExampleLabeledBreakEndingSectionBlockIsReportedAndLabeledContinueIsNot` | RH7501-001, RH7501-002, RH7501-003; FC-016 | `Verify(source)` | passes |
| `RH7501…AnalyzerTests.VerifyLabeledBreakEndingSectionBlockIsMovedBehindTheBlock` (`break outer;` and `break inner;`) | RH7501-001, RH7501-004 | `Verify(source, fixedSource)` | passes |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (nested `foreach`, `break outer;` before `}`) | LF | analyzer | reproduced | reported vs not reported |
| `break outer;` + statement on the next line | LF, CRLF | code fix | reproduced | fix result re-reported (no convergence) vs fixed, not reported |
| Formatter output with labeled `break` | LF, CRLF | formatter + analyzer | reproduced | formatter output reported vs not reported |

### Not covered, and why

- RH5009: judges only `goto` (decision 7).
- `StatementShouldBeFollowedByABlankLineAnalyzerBase` other subclasses: none exist (skeptic's `rg`), so no other consumer can change output.
- Lone-CR and mixed line endings (RH5011-004's SHARED-FIX-008 paragraphs): the fix's line-ending path is keyed on the semicolon and the line, not on the label; existing tests cover it, and the labeled LF/CRLF tests reach the same code.
- Formatter phases: no change expected; covered for analyzer parity by the two formatter tests.
- Unsupported shapes (FC-038 to FC-043): the run declares none.

## Whole run

Files changed: the eight test files above and this handover. `scripts/format.sh` on all eight changed nothing.

Results on f2abd60 + these tests: analyzer classes RH7105, RH5011 (analyzer and formatter), RH5010, RH5012, RH7501, RH2001 — 469 total, 35 failing, every one a new test listed as "fails" above; Core `ModifierOrderingUtilitiesTests` — 35 total, 7 failing, all new rows listed as "fails". All pre-existing tests in these classes pass.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests|FullyQualifiedName~VerifyPrivateGetOnlySafeAutoProperty"
scripts/test.sh --project core --filter "FullyQualifiedName~ModifierOrderingUtilitiesTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5011BreakStatements"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH7501Break"
```
