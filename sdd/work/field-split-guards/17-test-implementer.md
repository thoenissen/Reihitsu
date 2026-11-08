Role: test implementer
Commit: e50a788 (tests committed on top of it)
Verdict: tests written — #963: every member of finding #2's class pinned, on both sides of the ownership boundary; #1384: no change

Structural change after the architecture finding (decision 14). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the clarified FMT-STRUCT-030, FMT-STRUCT-022, FMT-PIPE-031 and RH7101-007 (`16-spec-author.md`, a88bc7a) and their examples. For the sibling shapes the specs do not show as examples, the outputs were measured with `scripts/format.sh` (Parse, fixture in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/t17/`; second pass `Formatted 0 of 1`) and checked against the requirement: split, comments in the first field only, nothing copied, the layout inside the first field being the documentation-comment phase's as in the spec examples.

No output changes in this step, so every new test is green on this head. To show they discriminate, I patched `FieldDeclarationSplitTransform.CarriesCommentInRepeatedPart` twice, ran the tests, and restored the file with `git checkout` (production diff empty afterwards):

- Mutation 1, the line-based boundary the auditor described (also block on an ordinary comment in the first declarator's leading trivia while the type's trailing trivia holds no line break): every new split-side test failed — 8 formatter, the 9 predicate rows plus the extended example test, 3 RH7101 (spec example, Diagnose `//` sibling, Fix All). Every other-side test stayed green.
- Mutation 2, a boundary moved the other way (ignore comments in the trailing trivia of the type's last token): every new other-side test failed — the formatter `None` example and 3 of 5 `KeepsTheDeclarationUnsplit` rows (the two rows behind a modifier or an attribute list are not that token's trivia and stayed green, as they should), the 4 type-trailing predicate rows, and all 3 RH7101 `/* d */ /** c */` mode rows, the `None` withheld test and the Fix All test.

Files changed (tests only; `scripts/format.sh` reported `0 of 3`):

- `src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterSplitsFieldDeclarationTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`

## thoenissen/Reihitsu#963

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FDS `SpecExampleCommentBehindTheFirstDocumentationStyleCommentOnTheTypesLineBelongsToTheFirstDeclarator` (new FMT-STRUCT-030 example, all three declarations, exact output, Parse) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | green |
| FDS `SpecExample…KeepsTheDeclarationUnsplitInDocumentationModeNone` (the example's `None` sentence, fixed point) | FMT-STRUCT-030, FC-003, FC-033 | `AssertRuleResult` + None options | green |
| FDS `DelimitedCommentBehindADocumentationStyleCommentWithTheDeclaratorsOnTheNextLineStaysWithTheFirstFieldInDocumentationModeDiagnose` (`/** c */ /* d */⏎ _first, _second`) | FMT-STRUCT-030, FMT-STRUCT-022, FC-033 | `AssertRuleResult` + Diagnose | green |
| FDS `CommentThatSpansLinesBehindADocumentationStyleCommentStaysWithTheFirstField` (`/** c */ /* d⏎ e */`) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| FDS `TwoDocumentationStyleCommentsBehindTheTypeStayWithTheFirstField` (`/** c */ /** d */`) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| FDS `DelimitedCommentOnTheLineBehindASingleLineDocumentationStyleCommentStaysWithTheFirstField` (`/// c⏎ /* d */ _first`) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| FDS `DelimitedCommentBehindADocumentationStyleCommentBehindTheTypeOfAnAttributedDeclarationStaysWithTheFirstField` (attribute list repeated without the comments) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| FDS `LineCommentBehindADocumentationStyleCommentBehindAGenericTypeStaysWithTheFirstFieldInDocumentationModeDiagnose` (`List<int> /** c */ // d⏎`) | FMT-STRUCT-030, FMT-STRUCT-022, FC-033 | `AssertRuleResult` + Diagnose | green |
| FDS `OrdinaryCommentThatDoesNotFollowADocumentationStyleCommentOnTheTypesLineKeepsTheDeclarationUnsplit`, 5 rows: `/* d */ /** c */⏎`, `/* d */ /// c⏎`, `// d⏎ /** c */ _a`, `private /** c */ /* d */ int`, `[Obsolete] /** c */ /* d */⏎` | FMT-STRUCT-030, FC-003 | `AssertDeclarationIsNotSplit` (Parse, LF, CRLF, second pass) | green |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForADocumentedDeclarationAndForOneWithACommentBehindTheType` (changed: example source gains `_i, _j` and `_k, _l` as in FMT-PIPE-031; true for `_i`, false for `_k` in Parse) | FMT-PIPE-031 | direct call | green |
| `…ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone` (changed: false for `_i` and `_k` in None) | FMT-PIPE-031, FC-003 | direct call | green |
| `…ReturnsTrueForACommentBehindADocumentationStyleCommentBehindTheTypeUnlessDocumentationCommentsAreNotParsed`, 9 rows: `/* d */` same line and next-line declarators, `// d`, spanning `/* d⏎ e */`, `/** d */`, `/* d */ /* e */`, `/// c⏎ /* d */`, generic type, attribute list above. True in Parse and Diagnose, false in None | FMT-PIPE-031, FC-001, FC-033 | direct call | green |
| `…ReturnsFalseForAnOrdinaryCommentThatDoesNotFollowADocumentationStyleCommentBehindTheTypeInEveryDocumentationMode`, 6 rows (the other side, incl. the two non-type slots). False in Parse, Diagnose and None | FMT-PIPE-031, FC-003 | direct call | green |
| RH7101 `SpecExampleOrdinaryCommentBehindADocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFix` (new RH7101-007 example, Parse) | RH7101-007, RH7101-004 | `Verify` + `AssertSingleFixUnderBothLineEndings` (LF, CRLF) | green |
| RH7101 `SpecExampleOrdinaryCommentBehindADocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeNone` | RH7101-007, FC-033 | `VerifyNoCodeFixIsOffered(…, None)` | green |
| RH7101 `SpecExampleOrdinaryCommentInFrontOfADocumentationStyleCommentBehindTheTypeWithholdsTheFix`, rows Parse, Diagnose, None | RH7101-007, FC-003 | `VerifyNoCodeFixIsOffered(…, mode)` | green |
| RH7101 `LineCommentBehindADocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFixInDocumentationModeDiagnose` (`/** c */ // d⏎ _g, _h`) | RH7101-007, RH7101-004, FC-033 | `AssertSingleFixUnderBothLineEndings(…, Diagnose)` | green |
| RH7101 `FixAllSplitsTheDeclarationWhoseOrdinaryCommentFollowsADocumentationStyleCommentBehindTheType` (both sides plus a plain declaration in one document, Parse) | RH7101-007, RH7101-004 | `Verify` (iterative + Fix All, `MarkupMode.Allow` for the kept diagnostic) | green |

### Scenarios (structural change)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Spec example, Parse | LF, CRLF | formatter | holds | output equals FMT-STRUCT-030's example; second pass stable |
| Spec example, None | LF, CRLF | formatter | holds | none split |
| Six sibling split-side shapes (Parse/Diagnose) | LF, CRLF | formatter | holds | split, comments in the first field only, attribute list/type repeated without them |
| Five other-side shapes, Parse | LF, CRLF | formatter | holds | not split |
| `/** c */ /* d */`, Parse; `/** c */ // d⏎`, Diagnose | LF, CRLF | RH7101 fix | holds | fix offered, output equals the formatter's |
| `/** c */ /* d */`, None; `/* d */ /** c */`, every mode | LF | RH7101 fix | holds | reported, no fix |
| Both sides in one document, Parse | LF | RH7101 Fix All | holds | one split, one kept with its diagnostic |

### Not covered, and why
- Layout inside the first field (blank line, comments moved off the type's line): FMT-STRUCT-030 says it is not its decision; the tests pin it only because `AssertRuleResult` asserts whole output, and it is the spec examples' output. Follow-up candidate already in `00-lead.md`.
- The `None` formatter test writes `_g, _h` at the member column, the fixed point a wrapped declarator list that stays whole gets (existing continuation-indent candidate in `00-lead.md`), as in `11-test-implementer.md`.
- Other-side formatter rows assert only that the declaration stays whole (`AssertDeclarationIsNotSplit`, Parse); the `None` side is asserted on the predicate (6 rows × 3 modes) and in the spec-example fixed point.
- RH7101 withheld cases on LF only: the offer is a yes/no from `SplitsFieldDeclaration`, which is line-ending independent; the formatter tests cover CRLF.
- FC-019/FC-020 (directive in the gap): `CarriesDirective` is untouched by this step.
- Three `/** */` in a row, `///` after `/** */`: a `///` runs to the end of the line, and further doc-style comments are the same ownership arm as the `/** c */ /** d */` row (`CarriesCommentInRepeatedPart` checks the trivia of tokens in front of the first declarator only; the first declarator's leading trivia is never inspected, so every comment kind in it takes one path).

## thoenissen/Reihitsu#1384

No change: the documented-declaration condition is untouched by the clarification (`16-spec-author.md`).

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterSplitsFieldDeclarationTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

Results: formatter 163/163, RH7101 114/114, architecture 115/115 passed.
