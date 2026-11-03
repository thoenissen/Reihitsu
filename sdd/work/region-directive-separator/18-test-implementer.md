Role: test implementer
Commit: (this commit)
Verdict: tests written

Test half of decision 17 in `00-lead.md` (audit 2, finding 4 and the architecture finding in `16-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (decision 17), `13-test-implementer.md`, `16-auditor.md`, `17-spec-author.md`, FMT-PIPE-028 and FMT-PIPE-019 in `sdd/specs/formatter/pipeline.md`, FMT-PIPE-007, and FMT-EOL-001/002 in `sdd/specs/formatter/line-endings.md`.

The tests pin the reading that FMT-PIPE-028 exempts region directives only from the region phase: the preparation phase runs as a whole at node level (FMT-PIPE-019), so the line break that ends a region directive takes the detected line ending (FMT-EOL-002). Expected values come from those requirements. The layout of the rest of the class is the one the FMT-PIPE-028 tab example already decides, with one space where that example has a tab. No production code was run to get them.

Decision 17 changes no behavior, so these tests pass on head `ece0ac1` and cannot be seen to fail without a production change. To check that they can fail, I changed the expected values in a scratch edit so that the foreign line break behind `#region bar` and `#endregion // Stale` was kept, which is the reading the old exhaustive wording allowed. All three tests failed on the first-pass assertion. The scratch edit was then reverted. This also shows that the marker helper really puts foreign line breaks into the input. If it did not, the changed expected value would not match the output and the tests would fail anyway.

## Tests

`¶` marks a line whose line break is the non-predominant one: LF in a CRLF document, CRLF in an LF document. `WithForeignLineBreaks` writes it before formatting. The fixture has seven line breaks inside class `C` (eleven inside the namespace in the context variant) and two foreign ones. So the predominant line ending is the same whether it is counted over the document or over the detached node (FMT-PIPE-007).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncRewritesForeignLineBreaksEndingRegionDirectives`: class `C` as target, `#region bar¶` and `#endregion // Stale¶`. Every line break comes out as the predominant ending. `bar` stays lower case, `// Stale` is kept, and the property is expanded. Runs with CRLF predominant and with LF predominant, with a second pass | FMT-PIPE-028, FMT-EOL-002 | `AssertNodeInDocumentWithForeignLineBreaks` (new; same passes as `AssertNodeInDocument`) | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentWithContextAsyncRewritesForeignLineBreaksEndingRegionDirectives`: the same fixture inside namespace `N`, with `N` as context. The auditor traced this entry point but did not run it | FMT-PIPE-028, FMT-EOL-002 | `AssertNodeInDocumentWithForeignLineBreaks` with context | passes |
| `ReihitsuFormatterNodeRegionTests.FormatNodeRewritesForeignLineBreaksEndingRegionDirectives`: the same fixture through `FormatNode` on the detached class | FMT-PIPE-028, FMT-EOL-002 | `FormatNode`, exact text, both predominant endings, second pass | passes |

## Not covered, and why

- Foreign line breaks elsewhere in the fixture (blank lines, code lines). FMT-EOL-001 covers them, and they have nothing to do with region directives. Keeping only the two directive breaks foreign means a failure points to the member under test.
- Lone `\r` and the Unicode terminators. FMT-EOL-001 lists them, and `ReihitsuFormatterLineEndingTests` covers them at document level. The question here is whether the preparation phase reaches a region directive at node level, not which terminators it rewrites. The LF/CRLF pair answers that, and a terminator matrix would grow the list that decision 17 removes from the spec.
- An RH7105 (or other code-fix) variant with mixed line endings. `Verify` runs the source as written, and its text normalization cannot hold a deliberate mixture. The fix calls `FormatNodeInDocumentAsync`, which the first test covers.
- A whole-document counterpart. `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` already pins it through `FormatSyntaxTree` (FMT-EOL-002).
- Other failure classes. This round adds tests for unchanged behavior only. The matrix and the dismissals in `04-`, `06-`, `08-`, and `13-test-implementer.md`, and in `11-` and `16-auditor.md`, still apply.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeRegionTests"   # Passed 22
scripts/test.sh --project architecture   # Passed 92
```
