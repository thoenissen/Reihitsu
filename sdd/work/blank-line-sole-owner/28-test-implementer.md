Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: drop blank lines above comments glued to a member or an embedded statement [skip ci]")
Verdict: tests written

Repair round 3, routed implementer findings D1 and D2 (`27-implementer.md`), decided in `00-lead.md` rows 36 and 37. The prompt named the two changes and the scope. It carried no suspected cause and no hoped-for verdict. No production code is touched, and no other test changed. Neither change weakens an assertion. Each one removes an expected blank line that no requirement asks for. FMT-BLANK-018 says that above a comment glued to a member's first token, or to a statement whose requirements insert nothing, the author's lines are kept, and the inputs have none.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `MultiLineGluedCommentBlankLineTests.InsertsBlankLineAfterClosingBraceAboveMultiLineCommentGluedToStatementButNotMember` (renamed from `...GluedToStatementOrMember`; the summary is updated) | FMT-BLANK-018 | `AssertRuleResult` | passes. The statement half still expects the blank line above `/* z⏎ */ int i = 0;` (SHARED-BLANK-003). The member half now expects `}⏎/* x⏎ */ int I;` unchanged, with no blank line (row 36). |
| `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` and `...WithCarriageReturnLineFeed` (shared `EmbeddedIfBehindMultiLineCommentFixedData`) | none (unchanged tags) | `Verify` | passes. The fixed data no longer has a blank line between `while (a)` and `/* note⏎ */ if (b)` (row 37). The columns and braces are unchanged. |

## Re-scan

I scanned every `.cs` file under the test projects (excluding `bin/` and `obj/`) for a blank line directly above a `/*` comment that is not a `/**` comment, spans several lines, and ends on a line with code.
- **All hits.** The hits are `ChainLinkBehindMultiLineCommentTests` (inputs only, with the author's blank line; row 35 moves the link off the comment line), the `RH5201` analyzer and formatter test inputs (same shape), and `MultiLineGluedCommentBlankLineTests`.
- **Insertions.** A second scan paired each `input`/`expected` (and `TestData`/`FixedData`) literal pair. It reports an expected blank line above such a comment when the input has none there. After this change it finds five, all in `MultiLineGluedCommentBlankLineTests`, and a statement requirement decides each one:
  - `case 2:` after `break`, the blank line between switch sections;
  - `return 1;` after a statement;
  - `int i = 0;` after `}` (SHARED-BLANK-003);
  - two block statements whose `{` has a glued comment, after `}` and after `break` (FMT-BLANK-017).
- **Result.** No other test expects a blank line above a comment glued to a member or to an embedded statement.

## Not covered, and why

Nothing new is added. The rows ask only to correct two expectations.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.BlankLines.MultiLineGluedCommentBlankLineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.VerifyEmbeddedIfBehindMultiLineComment"
```

Full runs on this head with both edits: formatter 3711/3711 passed, analyzer 6351/6351 passed. `scripts/format.sh` on both test files changed 0 of 2.
