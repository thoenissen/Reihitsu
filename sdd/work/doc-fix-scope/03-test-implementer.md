Role: test implementer
Commit: fc567de
Verdict: #1269: reproduced; #1342: reproduced; #1343: reproduced; #1459: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirement sentences at `fc567de` (RH8201-005, RH8202-001/003, RH8204-003, RH8305-004/005, RH8307-004/005, RH8308-001/003/004, RH8309-001/004/006/007, RH8401-005, RH8402-006). No production file was touched. Every "fails" below was run on `fc567de` and fails on the issue's own expected-versus-actual difference; the observed side is quoted. All tests are in `src/Reihitsu.Analyzer.Test/Documentation/`. `Verify(source, fixed, …)` is the Roslyn code-fix verifier: it compares text exactly, line endings included (a `<CR><LF>` vs `<LF>` diff shows in its output), and fails when the fix needs more iterations than diagnostics. `VerifyNoCodeFixIsOffered` checks every diagnostic the analyzer reports.

## thoenissen/Reihitsu#1269

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8201…`VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedComment` | RH8201-005 | `Verify` | fails |
| RH8201…`VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedCommentWithCarriageReturnLineFeed` | RH8201-005 | `Verify` (CRLF) | fails |
| RH8201…`VerifySpecificationExampleReplacesCommentWithInheritdocWithCarriageReturnLineFeed` | RH8201-005 | `Verify` (CRLF) | fails |
| RH8201…`VerifyFixKeepsCarriageReturnLineFeedOfPlainMember` | RH8201-005 | `Verify` (CRLF) | fails |
| RH8201…`VerifyFixKeepsIndentedRegionAndBlankLineWithCarriageReturnLineFeed` | RH8201-005 | `Verify` (CRLF) | fails |
| RH8201…`VerifyFixKeepsIndentedNullableDirectiveAndDelimitedCommentBehindComment` | RH8201-005 | `Verify` | fails |
| RH8201…`VerifyFixKeepsIndentedConditionalGroupWithExcludedCodeInFrontOfComment` | RH8201-005 | `Verify` | fails |
| RH8201…`VerifyMultiLineDocumentationCommentReplacementUsesDocumentEndOfLine` (rewritten, see below) | RH8201-005 | `ApplyCodeFixAsync` | passes |
| RH8202…`VerifySpecificationExampleRemovesSeveralValueElementsInOneApplication` | RH8202-001, RH8202-003 | `Verify` | fails |
| RH8202…`VerifySpecificationExampleRemovesSeveralValueElementsInCarriageReturnLineFeedDocument` | RH8202-003 | `Verify` (CRLF) | fails |
| RH8202…`VerifyCodeFixRemovesTwoTopLevelValueElementsInOneApplication` | RH8202-003 | `Verify` | fails |
| RH8202…`VerifyCodeFixRemovesValueElementsNestedInDifferentElementsInOneApplication` | RH8202-001, RH8202-003 | `Verify` | fails |
| RH8202…`VerifyCodeFixRemovesNestedAndTopLevelValueElementsInOneApplication` | RH8202-001, RH8202-003 | `Verify` | fails |
| RH8204…`VerifySpecificationExampleKeepsContentAsWritten` | RH8204-003 | `Verify` | fails |
| RH8204…`VerifyCodeFixKeepsCharacterReferenceInContent` | RH8204-003 | `Verify` via `VerifyPlaceholderContentIsKept` | fails |
| RH8204…`VerifyCodeFixKeepsGreaterThanSignInContent` | RH8204-003 | same | fails |
| RH8204…`VerifyCodeFixKeepsQuotesAndEmptyElementFormOfNestedElement` | RH8204-003 | same | fails |
| RH8204…`VerifyCodeFixKeepsWhiteSpaceOnlyContent` | RH8204-003 | same | fails |
| RH8204…`VerifyCodeFixKeepsQuoteEntityReferencesInContent` | RH8204-003 | same | fails |
| RH8204…`VerifyCodeFixKeepsCdataSectionInContent` | RH8204-003 | same | passes (guard) |
| RH8204…`VerifyCodeFixKeepsXmlCommentInContent` | RH8204-003 | same | passes (guard) |
| RH8204…`VerifyCodeFixKeepsAttributesOfNestedElement` | RH8204-003 | `Verify` | fails |
| RH8204…`VerifySpecificationExampleReplacesPlaceholderWithItsContentWithCarriageReturnLineFeed` | RH8204-003 | `Verify` (CRLF) | fails |
| RH8204…`VerifyCodeFixKeepsCarriageReturnLineFeedInsideContent` | RH8204-003 | `ApplyCodeFixAsync` (raw text) | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH8201-005 example 2 (`#if`/`#endif`/`#pragma` at 6 spaces, member at 6, `/** */ public override void Stop()` at 2) | LF, CRLF | code fix | reproduced | directives written at column 0, `/// <inheritdoc/>` and members at 4, vs. every line in its own column |
| RH8201-005 example 1 | CRLF | code fix | reproduced | `{<LF>`, `// Ordinary comment<LF>`, `}<LF>`, blank `<LF>`, `// Trailing<LF>` vs. CRLF |
| Plain override, nothing else in the trivia | CRLF | code fix | reproduced | the line in front of the comment (`{`) ends in `<LF>` |
| Indented `#region B` + blank line before the comment | CRLF | code fix | reproduced | `{<LF>`, blank line `<LF>` |
| `#nullable enable` + `/* Note */` between comment and member (indented) | LF | code fix | reproduced | `#nullable enable` moved to column 0 |
| `#if NEVER` / excluded code / `#else` / `#endif` before the comment (indented) | LF | code fix | reproduced | all three directives moved to column 0 |
| RH8202-003 several-elements example | LF, CRLF | code fix | reproduced | `<remarks><value>Second</value></remarks>` left behind after one application |
| Two top-level / two nested in different elements / nested + top-level | LF | code fix | reproduced | `Expected '1' iterations but found '2' iterations` |
| RH8204-003 content example: `a &#32; <see cref='C'/> b`, `a > b`, `the<placeholder> </placeholder>customer` | LF | code fix | reproduced | `a   <see cref="C" /> b`, `a &gt; b`, white space dropped |
| `&quot;a&quot; &apos;b&apos;` | LF | code fix | reproduced | `"a" 'b'` |
| `<c  lang = 'x' >` nested, `hint` attribute on placeholder | LF | code fix | reproduced | `<c lang="x">` |
| CDATA section, XML comment in content | LF | code fix | holds | kept as written today |
| Two-line content | CRLF | code fix (Verify and raw `ApplyCodeFixAsync`) | reproduced | `current<LF>` inside the content vs. `current<CR><LF>` |

### Not covered, and why

- FC-019/FC-020/FC-021 (comment and directive in the gap): covered for RH8201 by the `#if`/`#pragma`/`#region`/`#nullable`/`/* */`/ordinary-comment tests above. RH8202 and RH8204 edit inside one documentation comment, where no directive can stand.
- FC-008 (Fix All): the existing `GetFixAllScenario` of each class and the verifier's own Fix All pass over every multi-diagnostic test above (RH8201 two diagnostics, RH8204 three) cover it; RH8202 reports one diagnostic per declaration.
- FC-006, FC-018, FC-022–FC-029, FC-036: no rename, no expression rewrite, no formatter path in these three fixes (01-skeptic.md: formatter not involved).
- FC-038–FC-043: no unsupported shape is declared or touched by these fixes.
- RH8201 member whose comment and attribute list start at different columns (`indent.cs` in 01-skeptic.md): the indented-example test covers members at 6 and 2 spaces; a separate odd-column attribute list was not added (same trivia sequence as the `#pragma` line).
- RH8201 on Windows (environment newline CRLF in an LF document): not runnable here; the rewritten `VerifyMultiLineDocumentationCommentReplacementUsesDocumentEndOfLine` now asserts `\n` in an LF document instead of `Environment.NewLine`, so it would catch that on a Windows run.

### Rewritten existing test

- `RH8201…VerifyMultiLineDocumentationCommentReplacementUsesEnvironmentEndOfLine` → `…UsesDocumentEndOfLine`, tagged RH8201-005: it asserted `Environment.NewLine` after `/// <inheritdoc/>`, which on Windows contradicts RH8201-005 / SHARED-FIX-008 (the document's line ending). On Linux both are `\n`, so it passes before and after.

## thoenissen/Reihitsu#1342

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8305…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnit` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed` | RH8305-004 | `Verify` (CRLF) | fails |
| RH8305…`VerifySpecificationExampleKeepsOtherDocumentationCommentAndBlankLine` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifyNoCodeFixForSummaryInCommentWrittenBehindCode` | RH8305-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8305…`VerifyNoCodeFixForSummaryInCommentWrittenBehindCodeWithCarriageReturnLineFeed` | RH8305-005 | `VerifyNoCodeFixIsOffered` (CRLF) | fails |
| RH8305…`VerifyFixKeepsMethodBodyAsWritten` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifyFixKeepsEmptyTypeBodyInNamespace` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifyFixKeepsNestedMembersOfDocumentedTypeAsWritten` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifyFixKeepsOtherLineOfTheSameCommentAsWritten` | RH8305-004 | `Verify` | fails |
| RH8305…`VerifyFixLaysOutSummaryOfMemberThatHoldsACommentTheFormatterRefuses` (rewritten, see below) | RH8305-004 | `Verify` | fails |
| RH8307…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnit` | RH8307-004 | `Verify` | fails |
| RH8307…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed` | RH8307-004 | `Verify` (CRLF) | fails |
| RH8307…`VerifyNoCodeFixForElementInCommentWrittenBehindCode` | RH8307-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8307…`VerifyNoCodeFixForSummaryInCommentWrittenBehindCode` | RH8307-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8307…`VerifyFixKeepsMethodBodyAsWritten` | RH8307-004 | `Verify` | fails |
| RH8307…`VerifyFixKeepsNestedMembersOfDocumentedTypeAsWritten` | RH8307-004 | `Verify` | fails |
| RH8307…`VerifyFixKeepsOtherDocumentationCommentAndBlankLine` | RH8307-004 | `Verify` | fails |
| RH8307…`VerifyFixLaysOutElementOfMemberThatHoldsACommentTheFormatterRefuses` (rewritten, see below) | RH8307-004 | `Verify` | fails |
| RH8309…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnit` | RH8309-004 | `Verify` | fails |
| RH8309…`VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed` | RH8309-004 | `Verify` (CRLF) | fails |
| RH8309…`VerifyFixKeepsExpressionBodyAndElementLinesAsWritten` | RH8309-004 | `Verify` | fails |
| RH8309…`VerifyFixKeepsTypeBodyAndNestedMembersAsWritten` | RH8309-004 | `Verify` (2 diagnostics, Fix All) | fails |
| RH8309…`VerifyNoCodeFixForCommentWrittenBehindCode` | RH8309-006 | `VerifyNoCodeFixIsOffered` | fails |
| RH8309…`VerifyNoCodeFixForCommentWrittenBehindCodeWithCarriageReturnLineFeed` | RH8309-006 | `VerifyNoCodeFixIsOffered` (CRLF) | fails |
| RH8309…`VerifyFixSortsElementsOfMemberThatHoldsACommentTheFormatterRefuses` (rewritten, see below) | RH8309-004 | `Verify` | fails |
| RH8402…`FixReplacesOldHeaderAndKeepsBlankLineBehindIt` | RH8402-006 | `VerifySpecificationFileFix` | fails |
| RH8402…`FixReplacesOldHeaderDirectlyInFrontOfCode` | RH8402-006 | `VerifySpecificationFileFix` | passes (other side) |
| RH8402…`FixReplacesOldHeaderAndKeepsTwoBlankLinesBehindIt` | RH8402-006 | `VerifySpecificationFileFix` | fails |
| RH8402…`FixReplacesOldHeaderAndKeepsBlankLineInFrontOfDirective` | RH8402-006 | `VerifySpecificationFileFix` | fails |
| RH8402…`FixReplacesOldHeaderAndKeepsBlankLineInFrontOfDocumentationComment` | RH8402-006 | `VerifySpecificationFileFix` | fails |
| RH8402…`FixReplacesOldDelimitedHeaderFollowedByCodeOnItsLine` | RH8402-006 | `VerifySpecificationFileFix` | passes (other side) |
| RH8402…`FixReplacesOldHeaderAndKeepsBlankLineBehindItWithCarriageReturnLineFeed` | RH8402-006 | `VerifySpecificationFileFix` (CRLF) | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH8305: file-level `public class C { int A(int a) => a; }` with `///Text` | LF, CRLF | code fix | reproduced | summary stays one line, class and method re-laid out to block bodies |
| RH8305: `<remarks>` comment, blank line, reported summary | LF | code fix | reproduced | blank line removed |
| RH8305: `void M(){int a=1;⏎a++;}` | LF | code fix | reproduced | body re-laid out, blank line inserted between statements |
| RH8305: documented `public class C { }` in block namespace | LF | code fix | reproduced | `public class C;` |
| RH8305: nested members of documented type (`=> a`, `void B(){ }`, `///Text`, blank line between two doc comments) | LF | code fix | reproduced | block body, braces on own lines, `/// Text`, blank line removed |
| RH8305: `///Text` in the same comment as the summary | LF | code fix | reproduced | `/// Text` |
| RH8305/RH8307/RH8309-005/-006 behind code (`int A; /// …⏎int B;`) | LF, CRLF (RH8305, RH8309) | code fix | reproduced | an action is registered (`Assert.IsEmpty(actions)` fails) |
| RH8307: file-level spec example, `void M(int v){int a=1;}`, nested members, other comment + blank line | LF, CRLF (example) | code fix | reproduced | element not laid out at file level, class re-laid out; body re-laid out; nested re-laid out; blank line removed |
| RH8309: file-level `void I<T>() { }`, `int Add(int a) => a;`, `{ }` type in namespace | LF, CRLF (example) | code fix | reproduced | braces on own lines / block body / `class C;`; `<summary>Adds</summary>` expanded to three lines |
| RH8305/RH8307/RH8309 with `int /* c */ y` in the method body | LF | code fix | reproduced | no fix offered (SHARED-FIX-010 refusal of the whole declaration) vs. fix offered, body kept |
| RH8402-006 example 1, two blank lines, blank line before `#nullable`, before `///` | LF, CRLF (example 1) | code fix | reproduced | the blank lines between the old header and the next line are removed |
| RH8402-006 example 2 (no blank line), `/* Copyright … */ namespace N;` | LF | code fix | holds | today's output is the specified one |

### Rewritten existing tests

`NoFixIsOfferedWhenTheDocumentedMemberHoldsACommentTheFormatterRefuses` in the RH8305, RH8307, and RH8309 classes (each tagged SHARED-FIX-010) asserted that the fix is withheld because the documented method's body holds `int /* c */ y` (FMT-UNSUP-004), i.e. because the fix formats the whole declaration. RH8305-004/RH8307-004/RH8309-004 now cover that input (comment starts its own line, no syntax error) and require the fix to change only the reported element; SHARED-FIX-010's *Why* says a node wider than what the formatter receives "would withhold a fix the formatter carries out". Each was rewritten to: fix offered, documentation corrected, body (refused comment included) unchanged, tagged with the rule's -004 only. They now fail. `scripts/test.sh --project architecture` still passes, so SHARED-FIX-010 keeps covering tests elsewhere. Consequence for the lead: if the new fixes still pass a node through the formatter, no input of these three rules can hold a refused shape inside a documentation comment (FMT-UNSUP-004 is a comment inside a variable declaration), so their SHARED-FIX-010 path has no reachable refusing input; FC-043 is then checked by review, not by a test.

### Not covered, and why

- FC-030 is the defect itself; every "keeps … as written" test above is one member of it (body, `{ }`, nested members, expression body, other comment line, blank line between comments, file level).
- FC-025: expression-bodied *method* is tested (nested and own); other expression-bodied kinds reach the same structural transform the fix must no longer run, and the assertion "declaration code unchanged" does not depend on the kind. No separate per-kind test.
- FC-040 (documentation mode): `VerifyNoCodeFixIsOffered` runs in `Parse` mode; the existing `DocumentationModeIsNone` tests of each class cover mode `None` (nothing reported).
- FC-004/FC-021 for the withheld behind-code shape: nothing is rewritten, so nothing can move.
- RH8402 mixed run (copyright comment plus a note): outside RH8402-006's coverage (spec author's follow-up candidate, dropped by decision 16); no test.
- RH8309 accessor comment (follow-up candidate in `00-lead.md`): outside the issues; no test.

## thoenissen/Reihitsu#1343

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8308…`VerifySpecificationExampleDiagnosticRunsThroughReferences` | RH8308-001, RH8308-003 | `Verify` (analyzer) | fails |
| RH8308…`VerifySpecificationExampleRemovesTextWithReferenceInOneApplication` | RH8308-004 | `Verify` | fails |
| RH8308…`VerifyTextWithReferencesIsReportedThroughTheEndOfTheLineAndRemovedInOneApplication` (6 rows: `a &amp;`, `&amp; b`, `a &#32; b`, `a &#x41; b`, `a &lt; &gt; b`, `&amp;`) | RH8308-001, RH8308-003, RH8308-004 | `Verify` | 5 rows fail; `&amp;` alone passes (other side: a single reference) |
| RH8308…`VerifyTextWithReferenceIsRemovedInOneApplicationWithCarriageReturnLineFeed` | RH8308-004 | `Verify` (CRLF) | fails |
| RH8308…`VerifyTextWithReferenceInFrontOfElementIsRemovedInOneApplication` | RH8308-003, RH8308-004 | `Verify` | fails |
| RH8309…`VerifyElementOutOfOrderInLaterDocumentationCommentIsReported` | RH8309-001 | `Verify` (analyzer) | passes |
| RH8309…`VerifyNoCodeFixForLaterDocumentationCommentAfterBlankLine` | RH8309-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH8309…`VerifyNoCodeFixForLaterDocumentationCommentAfterOrdinaryComment` | RH8309-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH8309…`VerifyNoCodeFixForLaterDocumentationCommentAfterDelimitedDocumentationComment` | RH8309-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH8309…`VerifyNoCodeFixForLaterDocumentationCommentAfterDirective` | RH8309-007 | `VerifyNoCodeFixIsOffered` | fails |
| RH8309…`VerifyNoCodeFixForLaterDocumentationCommentWithCarriageReturnLineFeed` | RH8309-007 | `VerifyNoCodeFixIsOffered` (CRLF) | fails |
| RH8309…`VerifySpecificationExampleSortsFirstCommentAndKeepsLaterComment` | RH8309-004, RH8309-007 | `Verify` | fails |
| RH8309…`VerifySpecificationExampleSortsFirstCommentAndKeepsLaterCommentWithCarriageReturnLineFeed` | RH8309-007 | `Verify` (CRLF) | fails |
| RH8401…`VerifyNoCodeFixForByteOrderMarkInFrontOfSecondLine` | RH8401-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8401…`VerifyNoCodeFixForSubstituteCharacterInFrontOfSecondLine` | RH8401-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8401…`VerifyNoCodeFixForByteOrderMarkInFrontOfThirdOfFourLines` | RH8401-005 | `VerifyNoCodeFixIsOffered` | fails |
| RH8401…`VerifyNoCodeFixForByteOrderMarkInFrontOfSecondLineWithCarriageReturnLineFeed` | RH8401-005 | `VerifyNoCodeFixIsOffered` (CRLF) | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH8308 `</summary> a &amp; b` and the five other reference rows | LF, CRLF | analyzer span (then fix) | reproduced | span ends in front of the first reference (e.g. column 35 vs 42) |
| RH8308 `{|a &amp; b |}<c>x</c> {|c &lt; d|}` | LF | analyzer span | reproduced | span ends at column 36 vs 44 |
| RH8308 `&amp;` as the whole text | LF | analyzer + fix | holds | single reference token |
| RH8309 later comment after blank line / `//` / `/** */` first comment / `#pragma` | LF, CRLF (blank) | code fix | reproduced | an action is registered |
| RH8309-007 example 2 (first comment reported, blank line, later `<param>` comment) | LF, CRLF | code fix | reproduced | blank line removed, later comment joined into the first |
| RH8309 later comment reported by the analyzer | LF | analyzer | holds | already reported |
| RH8401 U+FEFF line 2, U+001A line 2, U+FEFF line 3 of 4 | LF, CRLF (U+FEFF line 2) | code fix | reproduced | an action is registered; both/all lines reported (analyzer part of the tests holds) |

### Not covered, and why

- RH8308 text whose first non-white character is a reference standing for white space (`&#32; b` first): outside RH8308-001's coverage; no test.
- RH8309 order across two comments: outside RH8309-001's coverage (decision 15); no test.
- FC-009 (offered action changes nothing): the RH8309 ordinary-comment and `/** */` cases were `no-progress` in 01-skeptic.md; they are now withheld-fix tests, which also exclude an offered no-op.
- FC-015 (RH8401 analyzer vs. `DocumentationCommentUtilities`): decision 7 refuses the class; the four withheld tests are the refusal, both stray characters and a non-adjacent line covered.

## thoenissen/Reihitsu#1459

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8305…`VerifyFixLaysOutSummaryOfDeclarationDirectlyInTheCompilationUnit` (10 rows: class with braces on own lines — the issue's input —, `class C { }`, struct, interface, enum, delegate, record, second type, after `using`, after a `// File header`) | RH8305-004 | `Verify` | all 10 rows fail |
| RH8305…`VerifyFixLaysOutSummaryOfTypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed` | RH8305-004 | `Verify` (CRLF) | fails |
| RH8305…`VerifyFixAllLaysOutSummariesOfTypesDirectlyInTheCompilationUnit` | RH8305-004, FC-008 | `Verify` (2 diagnostics, Fix All) | fails |
| RH8307…`VerifyFixLaysOutElementOfDeclarationDirectlyInTheCompilationUnit` (7 rows: class, struct, enum, delegate, record, second type, after `using`) | RH8307-004 | `Verify` | all 7 rows fail |

The RH8305/RH8307/RH8309 file-level spec-example tests listed under #1342 cover this issue too.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `/// <summary>S</summary>⏎public class C⏎{⏎}` as the whole file | LF, CRLF | code fix | reproduced | `/// <summary>S</summary>⏎public class C;` (summary unchanged, body changed) vs. three-line summary, `{ }` kept |
| struct, interface, enum, second type, after `using`, after header | LF | code fix | reproduced | summary unchanged, body changed |
| delegate, record | LF | code fix | reproduced | summary unchanged (`/// <summary>S</summary>` remains) |
| RH8307 same declarations | LF | code fix | reproduced | element not laid out |

### Not covered, and why

- Global statements with a documentation comment: 01-skeptic.md did not establish that one is reported; no caller case shown, no test.
- Target whose first token shares its line with the token before it: not reachable for a comment that starts its own line (01-skeptic.md); the behind-code case is RH8305-005/RH8307-005 (withheld), tested under #1342.

## Whole run

- Lane check not run (role does not change paths outside test files and this handover).
- `scripts/format.sh` over the nine test files: `Formatted 0 of 9 file(s).`
- Final focused run on `fc567de` plus these tests: 443 tests, 90 failed (64 methods, data rows counted per row), 353 passed. Every failure was inspected and fails on the expected-versus-observed difference stated above; no compile error, no unrelated layout difference.
- `scripts/test.sh --project architecture --no-install`: 117 passed (requirement coverage for RH8305-005, RH8307-005, RH8309-006, RH8309-007, RH8401-005, RH8402-006 now satisfied).
- Characterization search: `rg -i "formatter|format"` over the RH8305/RH8307/RH8309/RH8402 test classes and a read of the RH8201 `/** */` tests found the three SHARED-FIX-010 tests and the RH8201 environment-newline test, rewritten above. The other existing tests of these classes were not audited line by line for fixed outputs that reformat code outside the reported element; all of them passed on `fc567de`, and one that asserts such reformatting will fail once the fixes change — the implementer reports it to the lead rather than editing it.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Documentation.RH8201|FullyQualifiedName~Documentation.RH8202|FullyQualifiedName~Documentation.RH8204"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Documentation.RH8305|FullyQualifiedName~Documentation.RH8307|FullyQualifiedName~Documentation.RH8309|FullyQualifiedName~Documentation.RH8402"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Documentation.RH8308|FullyQualifiedName~Documentation.RH8309|FullyQualifiedName~Documentation.RH8401"
scripts/test.sh --project architecture
```
