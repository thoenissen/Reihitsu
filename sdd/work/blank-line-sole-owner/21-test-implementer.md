Role: test implementer
Commit: see the commit that adds this file (subject "Test implementer: raw-string argument test follows FMT-BLANK-018, no blank line above glued comment [skip ci]")
Verdict: tests written

Repair round 2, routed implementer finding D1 (`19-implementer.md`, `00-lead.md` row 27). The prompt named the one test to change and the line to remove. It carried no suspected cause or hoped-for verdict. The expected value comes from FMT-BLANK-018's third bullet: above a token that no requirement decides the blank lines for, none is inserted and the author's are kept. The input has no blank line above `/* c */ """`, so the output has none. Nothing else in the test changes. No assertion is weakened. Production code is untouched.

Run on 6259735 plus this change: formatter test project 3664 passed, 0 failed.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringArgumentOnCommentLedLine` (blank line between `Call(1,` and `/* c */ """` removed from the expected output) | FMT-INDENT-030 (tag unchanged; the blank line is incidental to it) | `AssertRuleResult` | passes |

## Re-scan

I scanned every `.cs` file of the test projects (bin/obj excluded) for a blank line followed by a line that starts with one or more delimited comments and then code. Only a delimited comment can be glued; a `//` comment ends its line. This pattern catches literals, raw strings, and any other token behind the comment, which the earlier scan (16) missed. The hits outside this run's own test classes (`GluedCommentBlankLineTests`, `BracedSwitchSectionCommentLayoutTests`, which this run already reviewed) fall into these groups. None expects a blank line that FMT-BLANK-018 forbids:

- A documentation comment (`/** … */`) that starts its line. It is not glued; FMT-BLANK-011 applies (`DelimitedDocumentation*Tests`, `TabReplacementTests`, `FieldDeclarationSplitTests:1115`, `RH8201` tests).
- The author's blank line, already in the input, above a token that no requirement decides. It is kept (FMT-BLANK-018 third bullet): `RH5303`/`RH5304` "Keep value"/"Keep two" lines, `RH5524:816/828` (`int Id`), `RH5301` formatter test (`new P`), `RH5303` formatter test (`new List<int>`), `RH5305` formatter test (opening `[`, which is not in the removal set), `ChainLinkBehindMultiLineCommentTests:587` (`.D()`, unchanged input), `FieldDeclarationSplitTests:1576` (unchanged input).
- A blank line that a statement requirement inserts above a block statement (FMT-BLANK-017, FMT-BLANK-018 second bullet): `BlockStatementBlankLineTests:775`, `ReihitsuFormatterNodeBlankLineTests:2943`.
- Line-break-phase-only tests that keep their input, or a directive in the gap: `LineBreakPhaseBlankLineTests:42/160`.
- An analyzer-only input: `RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests:906`. It asserts no RH6001 diagnostic and runs no formatter. Its summary says "this is the formatter's output for it", but nothing asserts that output, so it has no expectation to correct.

## Not covered, and why

- No new test. D1 is a correction of one existing expectation. The glued-comment-above-argument shape is already covered by FMT-BLANK-018's own tests in `GluedCommentBlankLineTests`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment.RawStringAlignmentPipelineTests"
scripts/test.sh --project formatter
```
