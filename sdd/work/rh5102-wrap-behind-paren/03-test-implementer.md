Role: test implementer
Commit: e863195
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

All tests are in `src/Reihitsu.Analyzer.Test/Formatting/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.cs` (analyzer class, `BatchCodeFixTestsBase`) and `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesFormatterTests.cs` (formatter parity). Expected values come from RH5102-001's examples, the change doc's entries 1–5, and the skeptic's *Breaking inputs* matrix. Tests with `endOfLine` run once with LF and once with CRLF (`DataRow`). On this head, 28 runs fail, every one on `Mismatch between number of diagnostics returned` with RH5102 reported where the spec says MUST NOT. The 48 other runs pass: the old tests plus every still-reported boundary case.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyListWrappedOnlyBehindOpeningParenthesisIsNotReported` (LF, CRLF) — `M(⏎1, 2)` | RH5102-001 | `Verify(source)` | fails: 1 RH5102 reported |
| `VerifyListWrappedOnlyInFrontOfClosingParenthesisIsNotReported` (LF, CRLF) — `M(1, 2⏎)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWrappedBehindOpeningAndInFrontOfClosingParenthesisIsNotReported` (LF, CRLF) — `M(⏎1, 2⏎)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseClosingParenthesisACommentHoldsOnALaterLineIsNotReported` (LF, CRLF) — `M(1, 2 // c⏎)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseFirstArgumentACommentKeepsOffTheOpeningParenthesisLineIsNotReported` (LF, CRLF) — `M( // c⏎1, 2)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseArgumentsFollowADelimitedCommentBelowTheOpeningParenthesisIsNotReported` (LF, CRLF) — `M(⏎/* c */ 1, 2)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseFirstArgumentFollowsAMultiLineCommentIsNotReported` (LF, CRLF) — `M( /* a⏎b */ 1, 2)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseFirstArgumentADirectiveKeepsOffTheOpeningParenthesisLineIsNotReported` (LF, CRLF) — `#pragma` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListWhoseArgumentsARegionEnclosesOnOneLineIsNotReported` (LF, CRLF) — `M(⏎#region R⏎1, 2⏎#endregion // R⏎)` | RH5102-001 | `Verify(source)` | fails: 1 reported |
| `VerifyListsWrappedOnlyBehindOpeningParenthesisAreNotReportedForEveryOwner` (LF, CRLF) — `: base(⏎…)`, `: this(⏎…)`, `new B(⏎…)`, `new(⏎…)`, `class D() : B(⏎…)` | RH5102-001; FC-027 | `Verify(source)` | fails: 5 reported |
| `VerifyNamedAndOutArgumentsWrappedOnlyBehindOpeningParenthesisAreNotReported` (LF, CRLF) | RH5102-001; FC-027 | `Verify(source)` | fails: 2 reported |
| `VerifySpecificationExampleOfListsWhoseArgumentsAllStartOnOneLineIsNotReported` — RH5102-001 not-reported block, verbatim | RH5102-001 | `Verify(source)` | fails: 5 reported |
| `VerifyFixLeavesListsWhoseArgumentsAllStartOnOneLineAsWritten` (LF, CRLF) — a reported `M(⏎1, 2,⏎3)` between `M(⏎1, 2)` and `M(1, 2⏎)`; one fix pass and Fix All lay out only the reported list | RH5102-001, RH5102-003; FC-008 | `Verify(source, fixed)` | fails: 3 reported, 1 expected |
| `VerifyListsWithEveryArgumentOnItsOwnLineAreNotReported` (LF, CRLF) — `M(1,⏎2)`, `M(⏎1,⏎2)` (issue's "Done when" shape) | RH5102-001; FC-003 | `Verify(source)` | passes (unchanged behavior) |
| `VerifySpecificationExampleOfReportedAndCompliantListsIsReportedAsMarked` — RH5102-001 first example, verbatim | RH5102-001 | `Verify(source)` | passes |
| `VerifyListWithArgumentsOnTwoLinesBelowOpeningParenthesisIsFixed` (LF, CRLF) — `M(⏎1, 2,⏎3)` → `M(1,⏎  2,⏎  3)`, then formatting changes nothing | RH5102-001, RH5102-003; FC-003, FC-007 | `Verify(source, fixed)`, `AssertFormattingChangesNothing` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithASpanningLambdaIsFixed` (LF, CRLF) — `M(⏎1, () =>⏎{⏎})` | RH5102-001, RH5102-003; FC-003, FC-027 | `Verify(source, fixed)`, `AssertFormattingChangesNothing` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithASpanningVerbatimStringIsReported` (LF, CRLF) | RH5102-001; FC-022 | `Verify(source)` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithASpanningRawStringIsReported` (LF, CRLF) | RH5102-001; FC-022 | `Verify(source)` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithASpanningInterpolatedStringIsReported` (LF, CRLF) — line break in a hole | RH5102-001; FC-022 | `Verify(source)` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithASpanningNestedCallIsReported` (LF, CRLF) — outer reported, inner `P(2,⏎3)` not | RH5102-001; FC-027 | `Verify(source)` | passes |
| `VerifyListWrappedBehindOpeningParenthesisWithACommentSpanningAnArgumentIsReportedWithoutCodeFix` (LF, CRLF) — `M(⏎1, 2 /* a⏎b */ + 3)` | RH5102-001, RH5102-004; FC-021 | `VerifyNoCodeFixIsOffered` | passes |
| Formatter class: `VerifyFormatterJoinsListWrappedOnlyBehindOpeningParenthesisWithoutAReport` — `Call(⏎1, 2)` → `Call(1, 2)`, nothing reported before or after, second pass stable | RH5102-001; FC-013 | `VerifyFormatter(source, fixed)` (LF and CRLF) | fails: 1 reported on the input |
| Formatter class: `VerifyListWrappedOnlyInFrontOfClosingParenthesisIsKeptWithoutAReport` — `Call(1, 2⏎ )` | RH5102-001, SHARED-DIAG-010; FC-013 | `VerifyFormatter(source)` (LF and CRLF) | fails: 1 reported |
| Formatter class: `VerifyListWhoseClosingParenthesisACommentHoldsIsKeptWithoutAReport` — `Call(1, 2 // c⏎ )` | RH5102-001, SHARED-DIAG-010; FC-013 | `VerifyFormatter(source)` (LF and CRLF) | fails: 1 reported |

The input for the two "kept" formatter tests puts `)` under the first argument, as the change doc's entry 2 shows. I ran `scripts/format.sh` on exactly that text, under LF and CRLF: it changes nothing. With `)` one column to the left, the formatter moves `)` under the first argument. So the formatter part of these tests holds on this head, and they fail only on the report.

## Scenarios (bug fix)

_N/A_ — behavior change.

## Not covered, and why

- **Existing tests flipped:** none. No existing RH5102 test expects a report for a list whose arguments all start on one line (grep over all test projects; only the analyzer and formatter classes name RH5102). `FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` and `FixMovesLeadingCommasOfSeveralArgumentListsInOneDocument` have arguments on two lines and stay green.
- **Formatter parity for `M( // c⏎1, 2)`, `#pragma`, `M( /* a⏎b */ 1, 2)`:** not added. Entry 4's formatter output (one argument per line) is FMT-BREAK-012 behavior, which this change leaves alone, and the analyzer tests cover the not-reported side. `M( /* a⏎b */ 1, 2)` through the formatter hits the dropped space behind the comment (skeptic follow-up 2, dropped by the lead, decision 6). A parity test would assert that defect or fail on it.
- **RH5101 still reports entry 1:** RH5101 is not involved in this change, and its existing tests cover `(⏎` reports.
- **Conditional directive in a gap (RH5102-004 / SHARED-DIAG-008):** unchanged and already covered by `VerifyConditionalDirective…` tests, which still pass.
- **FC-004, FC-005, FC-006, FC-018, FC-023–FC-026, FC-029, FC-036:** not applicable. The change only removes diagnostics. The fix and its rewrite do not change, and no rename, member kind, or literal rewrite is involved.
- **FC-010–FC-012, FC-014, FC-028, FC-030:** no formatter phase changes. The fix still formats only the argument list node through `FormatNodeInDocumentAsync`.
- **FC-038–FC-043:** no shape is declared unsupported.
- **FC-031, FC-032, FC-035, FC-037:** these concern the implementation and audit stages. Expected values here come from the spec and change doc, not from patched code.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLines"
```

## Amendment

Lead decision 10 asked for direct Core tests for `SyntaxNodeUtilities.IsPartlyWrapped<TNode>`, which the implementer introduced in 5c8c36c (`04-implementer.md`, *Amendment*). The tests are added to `src/Reihitsu.Core.Test/SyntaxNodeUtilitiesTests.cs`, next to the `AreAllSingleLine` tests. They follow the project's conventions: `[TestMethod]`, `[DataRow]` inputs, and the `GetArgumentList` helper, plus a new private `GetParameterList` helper.

Each `DataRow` is written with LF. The rows that matter for layout are asserted a second time with every line break of the list written as CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `IsPartlyWrappedReturnsTrueForArgumentsSharingALineInAMultiLineList` — `M(1, 2,⏎3)`, `M(⏎1, 2,⏎3)`, `M(1,⏎2, 3)`, block lambda behind `(` and behind `(⏎`, verbatim string behind `(⏎`, nested wrapped call, multi-line comment inside an argument | RH5102-001; FC-003, FC-022, FC-027 | `Assert.IsTrue`, LF and CRLF | passes |
| `IsPartlyWrappedReturnsFalseForArgumentsThatAllStartOnOneLineOrEachOnTheirOwn` — single line; `M(⏎1, 2)`; `M(1, 2⏎)`; `M(⏎1, 2⏎)`; `M(1, 2 // c⏎)`; `M( // c⏎1, 2)`; `M(⏎/* c */ 1, 2)`; `M( /* a⏎b */ 1, 2)`; `#pragma`; `#region`/`#endregion`; `M(1,⏎2)`; `M(⏎1,⏎2)`; `M(1,⏎() =>⏎{⏎})` | RH5102-001; FC-003, FC-019, FC-020, FC-021 | `Assert.IsFalse`, LF and CRLF | passes |
| `IsPartlyWrappedReturnsFalseForFewerThanTwoArguments` — `M()`, `M(1)`, a lone spanning lambda, `M(⏎1⏎)` | RH5102-001 ("at least two arguments"); FC-003 | `Assert.IsFalse` | passes |
| `IsPartlyWrappedReturnsTrueForParametersSharingALineInAMultiLineList` — `M(int a, int b,⏎int c)`, the same below `(`, and `M(⏎int a, Dictionary<int,⏎int> b)` (a type that spans lines) | RH5109-001 | `Assert.IsTrue`, LF and CRLF | passes |
| `IsPartlyWrappedReturnsFalseForParametersThatAllStartOnOneLineOrEachOnTheirOwn` — single line, `(⏎`, `⏎)`, `// c⏎)`, `( // c⏎`, `#pragma`, one per line, and one per line with a spanning type | RH5109-001 | `Assert.IsFalse`, LF and CRLF | passes |

All of them pass on this head. A direct test of a helper the implementer has already written cannot be watched failing before that code exists.

The false rows `M(⏎1, 2)`, `M(1, 2⏎)`, `M(1, 2 // c⏎)`, and `M( // c⏎1, 2)` are the inputs the old reading, by the lines of the parentheses, called multi-line. The true rows behind `(⏎` cover the other side of that boundary.

A separate test for "returns false" with an argument list and with a parameter list was a duplicate body, which Sonar S4144 refuses. I merged it into one test per list kind.

Command:

```shell
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.SyntaxNodeUtilitiesTests"
```

Result: Passed 67, Failed 0.
