Role: test implementer
Commit: 32c4bc0 (tests committed on top of it)
Verdict: tests written — #963: the answer-B case pinned on the split side; #1384: no change

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the amended FMT-STRUCT-030, FMT-STRUCT-022, FMT-PIPE-031 and RH7101-007 (`10-spec-author.md`, 4672256) and the maintainer's answer B (`00-lead.md`). The spec examples' outputs were measured again on this head with `scripts/format.sh` (Parse): they match the spec verbatim, and a second pass reports `Formatted 0 of 3`.

Answer B keeps the guard's code behavior, so every new test is green on this head and must stay green. To show they discriminate, I temporarily patched `FieldDeclarationSplitTransform.CarriesCommentInRepeatedPart` so that it also blocked on a comment in the first declarator's leading trivia (the literal-spec repair the auditor described), ran the tests, and restored the file with `git checkout`. Every new split-side test failed under that patch: the 4 formatter, 6 predicate and 2 RH7101 tests. The `None` counterparts stayed green, as they should, because they assert the other side of the boundary.

Files changed (tests only, formatted with `scripts/format.sh`, which reported `0 of 3` changes):

- `src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterSplitsFieldDeclarationTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`

## thoenissen/Reihitsu#963

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FDS `SpecExampleDocumentationStyleCommentBehindTheTypeStaysWithTheFirstField` (new FMT-STRUCT-030 example, Parse, both declarations, exact output) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | green |
| FDS `SpecExampleDocumentationStyleCommentBehindTheTypeKeepsTheDeclarationUnsplitInDocumentationModeNone` (the example's `None` sentence) | FMT-STRUCT-030, FC-033, FC-003 | `AssertRuleResult` + None options | green |
| FDS `DelimitedDocumentationStyleCommentBehindTheTypeWithTheDeclaratorsOnTheNextLineStaysWithTheFirstField` (`/** c */` behind the type, first declarator on the next line, Parse) | FMT-STRUCT-030, FMT-STRUCT-022 | `AssertRuleResult` | green |
| FDS `SingleLineDocumentationStyleCommentBehindAGenericTypeStaysWithTheFirstFieldInDocumentationModeDiagnose` (`///` behind `List<int>`, Diagnose) | FMT-STRUCT-030, FMT-STRUCT-022, FC-033 | `AssertRuleResult` + Diagnose options | green |
| `ReihitsuFormatterSplitsFieldDeclarationTests.ReturnsFalseForADocumentedDeclarationAndForOneWithACommentBehindTheType` (changed: the example source gains `private int /** c */ _g, _h;` as in the amended FMT-PIPE-031 example; asserts true for `_g` in Parse) | FMT-PIPE-031 | direct call | green |
| `…ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone` (changed: asserts false for `_g` in None) | FMT-PIPE-031, FC-003 | direct call | green |
| `…ReturnsTrueForADocumentationStyleCommentBehindTheTypeUnlessDocumentationCommentsAreNotParsed`, 6 rows: `/** c */` behind the type on the declarators' line and with the declarators on the next line, `///` with the declarators on the next line, `///` behind a generic type, `/** c */` behind an array type, `/** c */` behind the type under an attribute list. Each row expects true in Parse and Diagnose and false in None. | FMT-PIPE-031, FC-001, FC-033 | direct call | green |
| RH7101 `SpecExampleDocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFix` (new RH7101-007 example, Parse) | RH7101-007, RH7101-004 | `Verify` + `AssertSingleFixUnderBothLineEndings` (LF, CRLF) | green |
| RH7101 `SingleLineDocumentationStyleCommentBehindTheTypeDoesNotWithholdTheFixInDocumentationModeDiagnose` (`///` behind the type, first declarator on the next line) | RH7101-007, RH7101-004, FC-033 | `AssertSingleFixUnderBothLineEndings(…, Diagnose)` | green |
| RH7101 `DocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeNone` (the example's `None` counterpart, both comment kinds) | RH7101-007, FC-009 | `VerifyNoCodeFixIsOffered(…, None)` | green |

### Scenarios (repair)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `private int /** c */ _first, _second;` and `private int /// <summary>c</summary>⏎    _g, _h;`, Parse | LF, CRLF | formatter | holds | split, comment in the first field only, as the FMT-STRUCT-030 example states |
| Same, None | LF, CRLF | formatter | holds | neither split |
| `/** c */` behind the type with the declarators on the next line, Parse | LF, CRLF | formatter | holds | split |
| `///` behind a generic type, Diagnose | LF, CRLF | formatter | holds | split, the type repeated without the comment |
| Spec example, Parse; `///` variant, Diagnose | LF, CRLF | RH7101 fix | holds | fix offered, output equals the formatter's |
| Same inputs, None | LF | RH7101 fix | holds | reported, no fix |

### Not covered, and why
- Layout inside the first field (blank line, comment moved off the type's line): FMT-STRUCT-030 says it is not that requirement's decision. The tests pin it only because `AssertRuleResult` asserts whole output, and the output is the spec example's. It is the documentation-comment phase's, which is a follow-up candidate in `00-lead.md`.
- The `None` formatter test writes `_g, _h` at the member column. In `None` the formatter moves a wrapped declarator list of a declaration that stays whole to that column, as the existing `LineCommentBehindTheTypeWithTheDeclaratorsOnTheNextLineKeepsTheDeclarationUnsplit` does. That is the pre-existing continuation-indent behavior in the `00-lead.md` follow-up candidate. FMT-STRUCT-030 decides only that the declaration stays whole, so the input is written at that fixed point.
- Fix All: no new test. The answer-B inputs take the same `SplitsFieldDeclaration` path as any splittable declaration, and the existing `FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` covers Fix All with a guarded neighbor.
- RH7101 `None` withheld case on LF only, for the reason given in `04-test-implementer.md`: the offer is a yes/no from the predicate, and the formatter tests cover CRLF.
- FC-019/FC-020 (directive in the gap): unchanged guard `CarriesDirective`; this round touches only the comment boundary.

## thoenissen/Reihitsu#1384

No change: nothing in the amended requirements reaches the documented-declaration guard (`10-spec-author.md`).

## Commands

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterSplitsFieldDeclarationTests"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

Results: formatter 135/135, RH7101 107/107, architecture 115/115 passed.
