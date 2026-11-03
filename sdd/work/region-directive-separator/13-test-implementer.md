Role: test implementer
Commit: (this commit)
Verdict: tests written

Repair round 1, for decision 13 in `00-lead.md` and the test gap of audit finding 1 in `11-auditor.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (decision 13), `08-test-implementer.md`, `11-auditor.md`, `12-spec-author.md`, the amended FMT-PIPE-028 in `sdd/specs/formatter/pipeline.md`, FMT-TAB-002 in `sdd/specs/formatter/tab-replacement.md`, and the RH7105 tab paragraph in `sdd/changes/2026-11/region-directive-separator.md`.

Decision 13 keeps the behavior, so these tests pin existing behavior and pass on the current code (head `a2eb130`). They cannot be watched red without a production change. Each one is still falsifiable by the reading the auditor named, which keeps the tab: in every replacement test the expected text differs from the input exactly where the tab stands (four spaces versus a tab). The auditor's `apply-fix.sh RH7105` run observed the same four spaces this test now pins.

## Tests

`→` stands for a tab. The tests write it as a marker and replace it before formatting.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabsBehindRegionKeywordsOfTheTabExample`: the FMT-PIPE-028 tab example with class `C` as target. `#region→bar` becomes `#region    bar` and `#endregion→// Stale` becomes `#endregion    // Stale`, and the property is expanded | FMT-PIPE-028, FMT-TAB-002 | `AssertNodeInDocument` (LF, CRLF, second pass) | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentWithContextAsyncReplacesTabsBehindRegionKeywords`: the same example with namespace `N` as context | FMT-PIPE-028, FMT-TAB-002 | `AssertNodeInDocument` with context | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeReplacesTabsBehindRegionKeywords`: the same example through `FormatNode` on the detached class. The auditor traced this mechanism but did not run it | FMT-PIPE-028, FMT-TAB-002 | `FormatNode`, exact text, LF/CRLF, second pass | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabBetweenHashAndRegionKeyword`: `#→region bar` / `#→endregion` become `#    region bar` / `#    endregion` | FMT-PIPE-028, FMT-TAB-002 | `AssertNodeInDocument` | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabBehindBareRegionKeywords`: a bare `#region→` and `#endregion→` become the keyword plus four spaces, kept at the end of the line, because node level applies no trailing-whitespace removal | FMT-PIPE-028, FMT-TAB-002 | `AssertNodeInDocument` | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncKeepsTabsInsideRegionDescriptionAndEndRegionText`: `#region→bar→baz` becomes `#region    bar→baz`, and `#endregion // bar→baz` is kept. This covers the other side of the condition: a tab in free text is kept, a tab behind the keyword is replaced | FMT-PIPE-028, FMT-TAB-002 | `AssertNodeInDocument` | passes |
| `ReihitsuFormatterNodeRegionTests.FormatDocumentAsyncAppliesRegionRequirementsToTheTabExample`: the whole-document side of the example. The same input becomes `#region Bar` / `#endregion // Bar`. The property line `int P => 1;` is the output of the structural transforms that whole-document formatting applies. FMT-PIPE-028 does not state it, so I first expected the expanded node-level layout, and that expectation was my error, not a finding | FMT-PIPE-028 | `FormatDocumentAsync`, exact text, LF/CRLF | passes |
| `RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests.FixOfTypeWithRegionPairReplacesTabsBehindRegionKeywords`: the change doc's RH7105 tab case. The fix writes `public static class C` with `#region    bar` and `#endregion    // Stale` | FMT-PIPE-028, FMT-TAB-002 | `Verify` (`BatchCodeFixTestsBase`: exact fixed code, re-analysis) | passes |

## Not covered, and why

- A tab in the leading trivia of a `FormatNodeInDocumentWithContextAsync` target, or of a `FormatNodeInDocumentAsync` target that keeps its leading trivia. FMT-PIPE-018 writes that trivia back as in the document, and the auditor recorded both as "holds". Adding them would pin FMT-PIPE-018, which is not part of this repair.
- A CRLF variant of the RH7105 test. `Verify` runs the source as written (LF). The node-level tests above cover CRLF on the same entry point that the fix calls (`FormatNodeInDocumentAsync`).
- Fix All: no Fix All scenario holds a region (auditor, FC-007). The tab replacement does not depend on how many diagnostics are fixed.
- Other failure classes: this round adds tests for unchanged behavior only. The matrix and dismissals in `04-`, `06-`, `08-test-implementer.md` and `11-auditor.md` stand.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeRegionTests"   # Passed 19
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests"   # Passed 33
scripts/test.sh --project architecture   # Passed 92
```
