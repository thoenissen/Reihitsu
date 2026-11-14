Role: test implementer
Commit: 402aa86
Verdict: #1105: reproduced; #1266: reproduced; #1267: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec diff against `origin/main` (6f629b1) and `00-lead.md` (Q1 answer, Decisions 2–5). Where an expected output also depends on the horizontal-spacing phase, which the run must keep as it is, I ran `scripts/format.sh` on `main`'s code for a single directive with no ordering involved. The results: `System . IO` becomes `System.IO`, `A = System . IO` becomes `A = System.IO`, and `Comparer< int >` becomes `Comparer<int>`. `global :: System.IO` and `System/* x */.IO` stay as written.

Every new test was run on 402aa86. Each one that asserts a defect fails on the issue's own expected-versus-actual difference: a diagnostic count of expected N versus actual M, or a formatter output that differs. No new test fails on a compile error. I also formatted each formatter fixture once with `scripts/format.sh` on `main` and checked that its output shows the reported grouping or ordering defect (see Scenarios).

Test files (all in test projects):
- `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveNameValueTests.cs` (new, 19 tests).
- `src/Reihitsu.Analyzer.Test/Ordering/RH7201…AnalyzerTests.cs`, `RH7203…`, `RH7204…`, `RH7206…` (new tests appended).
- `src/Reihitsu.Analyzer.Test/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests.cs` (new tests appended).
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsFormatterTests.cs` (new tests appended).
- The comments in the `GetFixAllScenario` overrides of RH7203, RH7204, and RH7206 said "immediate predecessor". They now say "every earlier one of the group". The assertions are unchanged, and the diagnostic count of 2 holds under both readings.

Several fixtures contain the literal text `\u0053ystem`, which is a C# Unicode escape inside raw string literals. Raw strings do not process escapes, so the source under test holds the escape itself. Keep it that way. An editing tool that decodes `\u0053` to `S` would silently turn those tests into plain `System` tests.

## Finding against the spec

`sdd/specs/analyzer/RH7201.md`, RH7201-001, in the Unicode-escape example (`using Microsoft.Win32; using {|RH7201:\u0053ystem.IO|}; using System;`), marks only `\u0053ystem.IO`. Under RH7201-001's own text ("every regular namespace directive of the `System` group … that follows a regular namespace directive of another root"), the `using System;` below `Microsoft.Win32` must be reported too. `main` already reports it. My test `UnicodeEscapedSystemDirectiveIsReported` follows the requirement and marks both directives. The example lacks the markup `{|RH7201:System|}`. This is a spec-example fix only, and the spec author owns it.

## thoenissen/Reihitsu#1105

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7201 `SystemDirectiveWithWhitespaceInItsNameIsReported` | RH7201-001 | `Verify(source)` | fail (0 of 1 diagnostics) — issue instance |
| RH7201 `VerbatimSystemDirectiveIsReported` | RH7201-001 | `Verify` | fail |
| RH7201 `SystemDirectiveWithCommentInItsNameIsReported` | RH7201-001 | `Verify` | fail |
| RH7201 `GlobalQualifiedSystemDirectiveWithWhitespaceIsReported` | RH7201-001 | `Verify` | fail |
| RH7201 `GlobalQualifiedVerbatimSystemDirectiveIsReported` | RH7201-001 | `Verify` | fail |
| RH7201 `UnicodeEscapedSystemDirectiveIsReported` | RH7201-001 | `Verify` | fail (1 of 2) |
| RH7201 `VerbatimSystemDirectiveOfTheGlobalSectionIsReported` | RH7201-001, SHARED-USING-002 | `Verify` | fail |
| RH7201 `CodeFixPlacesVerbatimSystemDirectiveInTheSystemGroup` | RH7201-001, RH7201-004, SHARED-USING-004 | `Verify(source, fixed)` (re-analysis = convergence, FC-007) | fail |
| RH7201 `CodeFixPlacesVerbatimSystemDirectiveInTheSystemGroupWithCarriageReturnLineFeed` | RH7201-004, SHARED-USING-004 | `VerifyCodeFixWithEndOfLine(…, "\r\n")` | fail |
| RH7201 `CodeFixOfSystemDirectiveWithWhitespaceInItsNameConverges` | RH7201-004 | `Verify` + `ApplyCodeFixAsync` + `Verify(result)` (FC-007; the spelling of the result is not pinned) | fail |
| RH7203 `DirectiveWithWhitespaceInItsNameIsComparedByItsValue` | RH7203-001 | `Verify` | fail |
| RH7203 `VerbatimDirectiveIsComparedByItsValue` | RH7203-001 | `Verify` | fail |
| RH7203 `DirectiveWithCommentInItsNameIsComparedByItsValue` | RH7203-001 | `Verify` | fail |
| RH7203 `UnicodeEscapedDirectiveIsComparedByItsValue` | RH7203-001 | `Verify` | fail |
| RH7203 `VerbatimSegmentThatSortsAfterByItsValueIsNotReported` | RH7203-002 | `Verify` | fail (false positive, 1 instead of 0) |
| RH7204 `AliasWithWhitespaceInItsTargetIsGroupedByTheTargetsValue` | RH7204-001 | `Verify` | fail |
| RH7204 `AliasWithVerbatimTargetIsGroupedByTheTargetsValue` | RH7204-001 | `Verify` | fail |
| RH7206 `StaticDirectiveWithWhitespaceInItsTypeNameIsComparedByItsValue` | RH7206-001 | `Verify` | fail |
| RH7206 `VerbatimStaticDirectiveIsComparedByItsValue` | RH7206-001 | `Verify` | fail |
| RH7206 `TypeArgumentWhitespaceIsNotPartOfTheSortKey` | RH7206-001 | `Verify` | fail |
| RH7206 `StaticDirectivesInOrderByTheirValueAreNotReported` | RH7206-002 | `Verify` | fail (false positive) |
| RH7207 `BlankLineInFrontOfAVerbatimSystemDirectiveIsReported` | RH7207-001 | `Verify` | fail |
| RH7207 `BlankLineInFrontOfASystemDirectiveWithWhitespaceInItsNameIsReported` | RH7207-001 | `Verify` | fail |
| RH7207 `UnicodeEscapedSystemDirectiveBehindAnotherRootIsReported` | RH7207-001 | `Verify` | fail |
| RH7207 `BlockGroupedAndOrderedByTheValueOfItsNamesIsNotReported` | RH7207-002 | `Verify` | fail (false positive) |
| RH7207 `CodeFixRemovesTheBlankLineInFrontOfAVerbatimSystemDirective` | RH7207-004, SHARED-USING-004 | `Verify(source, fixed)` | fail |
| RH7207 formatter `FormatterPlacesAVerbatimDirectiveInTheSystemGroup` | SHARED-USING-004 | `FormatterTestsBase.VerifyFormatter(source, fixed)` (analyzer ↔ formatter parity, 2 passes, LF+CRLF) | fail |
| Formatter `VerbatimNameBelongsToTheSystemGroup` | SHARED-USING-004 | `AssertRuleResult` (2 passes, LF+CRLF, phase stability) | fail |
| Formatter `UnicodeEscapedNameBelongsToTheSystemGroupAndKeepsItsSpelling` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `NameWithWhitespaceBetweenItsTokensBelongsToTheSystemGroupInOnePass` | SHARED-USING-004, FMT-PIPE-004 | `AssertRuleResult` | fail |
| Formatter `NameWithCommentBetweenItsTokensBelongsToTheSystemGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `GlobalQualifierWithWhitespaceBelongsToTheSystemGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `VerbatimSegmentSortsByItsValue` | SHARED-USING-007 | `AssertRuleResult(input)` | fail |
| Formatter `TypeArgumentWhitespaceDoesNotDecideTheOrderOfStaticDirectivesInOrder` | SHARED-USING-007, FMT-PIPE-004 | `AssertRuleResult` | fail |
| Formatter `TypeArgumentWhitespaceDoesNotDecideTheOrderOfStaticDirectivesOutOfOrder` | SHARED-USING-004, FMT-PIPE-004 | `AssertRuleResult` | fail |
| Formatter `VerbatimStaticTypeNameBelongsToTheSystemStaticGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `VerbatimAliasTargetBelongsToTheSystemAliasGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `AliasTargetWithWhitespaceBelongsToTheSystemAliasGroupInOnePass` | SHARED-USING-004, FMT-PIPE-004 | `AssertRuleResult` | fail |
| Formatter `GlobalSectionGroupsVerbatimNameByItsValue` | SHARED-USING-004, SHARED-USING-002 | `AssertRuleResult` | fail |

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `Microsoft.Win32; System . IO` (issue instance) | LF | RH7201 | reproduced | 0 diagnostics vs 1 on `System . IO` |
| `@System.Text`, `System/* x */.Linq`, `global :: System.Xml`, `global::@System.IO`, global-section `@System.IO` after `Microsoft.Win32` | LF | RH7201 | reproduced | 0 vs 1 each |
| `Microsoft.Win32; \u0053ystem.IO; System` | LF | RH7201 | reproduced | only `System` reported, vs both |
| Code fix on `Microsoft.Win32; @System.IO; System` | LF, CRLF | RH7201 fix | reproduced | the expected diagnostic on `@System.IO` is missing; the expected fix is `System; @System.IO; ⏎ Microsoft.Win32` |
| `System.Linq` then `System . IO` / `@System.IO` / `System/* x */.IO` / `\u0053ystem.IO` | LF | RH7203 | reproduced | 0 vs 1 |
| `System.Collections; System.@Linq` | LF | RH7203 | reproduced | 1 (false positive) vs 0 |
| alias `B = System.Text` then `A = System . IO` / `A = @System.IO` | LF | RH7204 | reproduced | 0 vs 1 |
| static `System . Math` / `@System.Math` then `System.Console`; `Comparer< int >` then `Comparer<byte>` | LF | RH7206 | reproduced | 0 vs 1 |
| static `Comparer<byte>` then `Comparer< int >` | LF | RH7206 | reproduced | 1 (false positive) vs 0 |
| `System; ⏎ @System.IO` / `System; ⏎ System . IO` / `System; ⏎ Microsoft.Win32; ⏎ \u0053ystem.IO` | LF | RH7207 | reproduced | 0 vs 1 |
| value-organized block `System; @System.IO; System/* x */.Linq; \u0053ystem.Text; global :: System.Xml; ⏎ Microsoft.Win32` | LF | RH7207 | reproduced | 1 (false positive) vs 0 |
| `Microsoft.Win32; @System.IO; System` | LF+CRLF, 2 passes | formatter, RH7207 parity | reproduced | `main` gives `System; ⏎ @System.IO; ⏎ Microsoft.Win32` (3 groups) vs `System; @System.IO; ⏎ Microsoft.Win32` |
| `Microsoft.Win32; System . IO; System` | LF+CRLF, 2 passes | formatter | reproduced | `main` pass 1 gives `System; ⏎ Microsoft.Win32; ⏎ System.IO` (and pass 2 differs, FMT-PIPE-004) vs `System; System.IO; ⏎ Microsoft.Win32` |
| `\u0053ystem.IO`, `System/* x */.IO`, `global :: System.IO` behind `Microsoft.Win32` | LF+CRLF | formatter | reproduced | own group (in front of or behind `Microsoft`) vs inside the `System` group |
| `System.Collections; System.@Linq` | LF+CRLF | formatter | reproduced | reordered vs unchanged |
| static `Comparer<byte>; Comparer< int >` and the reverse | LF+CRLF, 2 passes | formatter | reproduced | pass 1 `Comparer<int>; Comparer<byte>` vs `Comparer<byte>; Comparer<int>` |
| static `@System.Math; System.Console`; aliases `B = System.Text; ⏎ A = @System.IO` / `A = System . IO` | LF+CRLF | formatter | reproduced | two groups vs one sorted group |
| global section `Microsoft.Win32; @System.IO; System` | LF+CRLF | formatter | reproduced | three groups vs `System; @System.IO; ⏎ Microsoft.Win32` |

### Not covered, and why
- Comment between `using` and the name (leading trivia of the name), and alias names `@B` versus `B`: 02-skeptic shows both hold on `main`. The first is not part of the name text, and the second is already read through `ValueText`. They are no member of the defect.
- Code fix with a comment inside the name: the RH7201 convergence test uses whitespace, and the RH7207 fix test uses `@`. The fix's output spelling for a name with interior trivia is not specified (03-spec-author, Internal choices), so I pinned only convergence. The formatter test `NameWithCommentBetweenItsTokensBelongsToTheSystemGroup` covers the comment member on the document path.
- Unit tests of `UsingDirectiveOrderingUtilities` in `Reihitsu.Core.Test`: not added. Both independent reads of the root are reached through surfaces (FC-016). The `System` check reaches RH7201 and RH7203's group filter. The root read reaches RH7203, RH7204, RH7206, RH7207, and the formatter. A Core-level test would lock the helper's API shape, which the implementer may change.
- FC-021 (comment and directive in a gap): the comment in the gap is covered. A directive inside a name cannot occur, because a preprocessor directive must start a line and a block with one is not reordered (SHARED-USING-009, existing tests).

## thoenissen/Reihitsu#1266

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7203 `DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossARegularDirectiveOfAnotherRoot` | RH7203-001 | `Verify` | fail (0 of 1) — issue instance |
| RH7203 `CodeFixOrganizesABlockOutOfOrderAcrossARegularDirectiveOfAnotherRoot` | RH7203-001, RH7203-004 | `Verify(source, fixed)` | fail |
| RH7203 `DirectiveIsComparedWithEveryEarlierDirectiveOfItsGroup` | RH7203-001 | `Verify` | fail (1 of 2) |
| RH7203 `SystemDirectiveIsComparedWithEveryEarlierDirectiveOfTheSystemGroup` | RH7203-001 | `Verify` | fail (1 of 2) |
| RH7203 `GlobalDirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherRoot` | RH7203-001, SHARED-USING-002 | `Verify` | fail |
| RH7203 `OrderedDirectivesOfAGroupAreNotReportedAcrossARegularDirectiveOfAnotherRoot` | RH7203-002 | `Verify` | pass (other side of the condition, FC-003) |
| RH7204 `AliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfAnotherRoot` | RH7204-001 | `Verify` | fail |
| RH7204 `CodeFixOrganizesABlockOutOfOrderAcrossAnAliasOfAnotherRoot` | RH7204-001, RH7204-004 | `Verify(source, fixed)` | fail |
| RH7204 `AliasIsComparedWithEveryEarlierAliasOfItsGroup` | RH7204-001 | `Verify` | fail (1 of 2) |
| RH7204 `AliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfATypeThatIsNotAName` | RH7204-001 | `Verify` | fail |
| RH7204 `GlobalAliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfAnotherRoot` | RH7204-001, SHARED-USING-002 | `Verify` | fail |
| RH7204 `OrderedAliasesOfAGroupAreNotReportedAcrossAnAliasOfAnotherRoot` | RH7204-002 | `Verify` | pass (FC-003) |
| RH7206 `StaticDirectiveIsComparedWithAnEarlierOneOfItsGroupAcrossAStaticDirectiveOfAnotherRoot` | RH7206-001 | `Verify` | fail |
| RH7206 `CodeFixOrganizesABlockOutOfOrderAcrossAStaticDirectiveOfAnotherRoot` | RH7206-001, RH7206-004 | `Verify(source, fixed)` | fail |
| RH7206 `StaticDirectiveIsComparedWithEveryEarlierOneOfItsGroup` | RH7206-001 | `Verify` | fail (1 of 2) |
| RH7206 `GlobalStaticDirectiveIsComparedWithAnEarlierOneOfItsGroupAcrossAnotherRoot` | RH7206-001, SHARED-USING-002 | `Verify` | fail |
| RH7206 `OrderedStaticDirectivesOfAGroupAreNotReportedAcrossAnotherRoot` | RH7206-002 | `Verify` | pass (FC-003) |

The tests that already existed for another group or another kind between the two directives (`DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherGroup`, `…AcrossAnotherKind`, and `OrderedDirectivesOfAGroupAreNotReportedAcrossAnotherGroup` in RH7203) pass on this head and stay as they are.

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `Microsoft.B; Alpha.X; Microsoft.A` (issue instance) | LF | RH7203 | reproduced | 0 vs 1 on `Microsoft.A` |
| `Microsoft.C; Microsoft.A; Microsoft.B` | LF | RH7203 | reproduced | `A` only, vs `A` and `B` |
| `System.Text; System.Collections; System.IO` | LF | RH7203 | reproduced | `Collections` only, vs `Collections` and `IO` |
| global `Microsoft.B; Alpha.X; Microsoft.A` | LF | RH7203 | reproduced | 0 vs 1 |
| `B = …SyntaxNode; C2 = System.Text; A = …SyntaxToken` | LF | RH7204 | reproduced | 0 vs 1 |
| `C2 = System.Text; A = System.IO; B = System.Linq` | LF | RH7204 | reproduced | `A` only, vs `A` and `B` |
| `B = System.Text; P = int; A = System.IO` | LF | RH7204 | reproduced | 0 vs 1 |
| `…SyntaxFacts; Reihitsu.Core.CasingUtilities; …SyntaxFactory` (static) | LF | RH7206 | reproduced | 0 vs 1 |
| `System.Math; System.Console; System.Convert` (static) | LF | RH7206 | reproduced | `Console` only, vs both |

### Not covered, and why
- Formatter and RH7207: 02-skeptic shows that both already order by `ComputeCanonicalOrder`, which this issue does not change. RH7207 already reports these blocks. There is no new expected output there.
- CRLF on these analyzer reports: the rules compare directive keys, and no line-ending trivia is involved. The fix path's line endings are covered by the CRLF fix tests of #1105 and #1267, which run through the same `OrganizeUsingDirectivesInDocumentAsync`.
- RH7201, RH7202, RH7205: 02-skeptic checked the "same pattern" goal by reading the code. No test was added, because no requirement changed for them.

## thoenissen/Reihitsu#1267

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7204 `AliasOfAPredefinedTypeIsNotComparedWithAnAliasOfAName` | RH7204-002 | `Verify` | pass (characterization, Decision 4) |
| RH7204 `AliasOfATupleTypeIsNotComparedWithAnAliasOfAName` | RH7204-002 | `Verify` | pass (characterization) |
| RH7207 `AliasOfAPredefinedTypeInFrontOfAnAliasGroupOfNamesIsReported` | RH7207-001, RH7207-003 | `Verify` | fail (0 of 1) |
| RH7207 `AliasOfATupleTypeInFrontOfAnAliasGroupOfNamesIsReported` | RH7207-001, RH7207-003 | `Verify` | fail |
| RH7207 `BlockEndingWithAnAliasOfAPredefinedTypeIsNotReported` | RH7207-002 | `Verify` | fail (false positive) — issue instance |
| RH7207 `BlockEndingWithAnAliasOfATupleTypeIsNotReported` | RH7207-002 | `Verify` | fail |
| RH7207 `GlobalSectionEndingWithAnAliasOfAPredefinedTypeIsNotReported` | RH7207-002, SHARED-USING-002 | `Verify` | fail |
| RH7207 `CodeFixMovesAnAliasOfAPredefinedTypeBehindTheAliasGroupsOfNames` | RH7207-004, SHARED-USING-004 | `Verify(source, fixed)` | fail |
| RH7207 `CodeFixMovesAnAliasOfAPredefinedTypeBehindTheAliasGroupsOfNamesWithCarriageReturnLineFeed` | RH7207-004, SHARED-USING-004 | `VerifyCodeFixWithEndOfLine(…, "\r\n")` | fail |
| RH7207 `FixAllMovesAliasesOfPredefinedTypesInEveryBlock` | RH7207-004, SHARED-USING-004 | `Verify(source, fixed)` with 2 diagnostics in 2 blocks (Fix All) | fail |
| RH7207 formatter `FormatterPlacesAnAliasOfAPredefinedTypeInTheLastGroup` | SHARED-USING-004 | `VerifyFormatter(source, fixed)` | fail |
| RH7207 formatter `FormatterKeepsAnAliasOfAPredefinedTypeInTheLastGroup` | SHARED-USING-004 | `VerifyFormatter(source)` | fail |
| Formatter `AliasOfAPredefinedTypeFormsTheLastGroup` (spec example) | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `BlockEndingWithAnAliasOfAPredefinedTypeIsLeftUnchanged` | SHARED-USING-004 | `AssertRuleResult(input)` | fail |
| Formatter `AliasOfATupleTypeFormsTheLastGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `AliasOfAnArrayTypeFormsTheLastGroup` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `AliasOfANullableTypeFormsTheLastGroup` (`System.Int32?`) | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `AliasOfAPredefinedTypeComesAfterTheAliasGroupOfAnotherRoot` | SHARED-USING-004 | `AssertRuleResult` | fail |
| Formatter `AliasOfAPredefinedTypeFormsTheLastGroupOfTheGlobalSection` | SHARED-USING-004, SHARED-USING-002 | `AssertRuleResult` | fail |

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `A = System.Text; ⏎ B = int` (issue instance) | LF | RH7207 | reproduced | reported vs not reported |
| same | LF+CRLF, 2 passes | formatter | reproduced | `main` moves `B = int` first vs unchanged |
| `B = int; ⏎ A = System.Text` | LF, CRLF | RH7207 + fix, Fix All | reproduced | not reported vs reported, fixed to `A = System.Text; ⏎ B = int` |
| tuple `(int X, int Y)`, `int[]`, `System.Int32?`, behind `Z = Microsoft.Win32.Registry`, global section | LF+CRLF | formatter | reproduced | non-name alias first vs last group |
| `B = System.Text; A = int` / `A = (int X, int Y)` | LF | RH7204 | holds | not reported, as RH7204-002 requires |

### Not covered, and why
- Pointer and function-pointer alias targets: they need `using unsafe` and unsafe compilation. 02-skeptic shows the mechanism is the null `UsingDirectiveSyntax.Name`, which predefined, tuple, array, and nullable targets share. I added tests for four of those target kinds rather than one, so a per-kind switch would still be caught.
- Two non-name aliases in one section: SHARED-USING-004 scopes this out, and Decision 4 keeps `main`'s behavior. No test locks it.
- SHARED-USING-005: dropped from scope (Decision 4).

## Whole run

### Failure classes walked
- FC-003 (both sides of a condition): the "in order across another root" tests for RH7203, RH7204, and RH7206, and the "not reported" tests next to every "reported" test of a spelling.
- FC-007 (fix convergence): every `Verify(source, fixed)` re-analyzes the result. The `System . IO` fix test asserts that RH7201 reports nothing on the fix output.
- FC-009: `CodeFixRemovesTheBlankLineInFrontOfAVerbatimSystemDirective` asserts that the fix changes the text.
- FC-010 / FC-017 (idempotency): every `AssertRuleResult` runs 2 passes plus a check that each phase is stable on its own. The `System . IO`, `A = System . IO`, and `Comparer< int >` tests are tagged FMT-PIPE-004.
- FC-013 (analyzer ↔ formatter): three RH7207 `VerifyFormatter` tests.
- FC-015 / FC-016: every analyzer that reads the root, and both reads, have tests (above).
- FC-023 (verbatim and Unicode identifiers): covered on every surface.
- FC-033 (CRLF and siblings): CRLF runs through `AssertRuleResult`/`VerifyFormatter` and two fix tests. The sibling shapes are the global section and a nesting level deeper (the Fix All namespace block).
- Not applicable: naming, Renamer (no rename), unsupported shapes FC-038 to FC-043 (none declared), token-gap joins (no rule joins lines).

### Results on 402aa86
- Analyzer, filter `RH720`: 232 tests, 51 failing. The failing tests are exactly the new tests marked *fail* above. Every test that existed before passes.
- Formatter, filter `UsingDirective`: 169 tests, 19 failing, all of them in `UsingDirectiveNameValueTests`.
- Architecture: 118 passed (requirement tags valid).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH720"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives.UsingDirectiveNameValueTests"
scripts/test.sh --project architecture
```
