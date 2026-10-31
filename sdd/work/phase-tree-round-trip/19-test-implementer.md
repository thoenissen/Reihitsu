Role: test implementer
Commit: 98fcf0d
Verdict: reproduced

Repair round 2, scope audit finding #8 (`18-auditor.md`) under decision `2026-10-31-phase-tree-round-trip-raw-string-hole-line.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

All tests are in `src/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs`. Each member goes through `AssertMoved`, which checks under LF and CRLF, in this order:

1. FMT-PIPE-022: an annotation on every token of each literal, and on every node inside its holes, stays on exactly one element of the same kind after every phase.
2. The final output of `ReihitsuFormatter.FormatSyntaxTree`, which does not compare phase trees, equals the expected text.
3. `AssertRuleResult`, which checks the same output, a stable second pass, the phase stability check, and FMT-PIPE-021 with token values for every phase.

Because of this order, a failure names the first invariant that breaks. The expected text comes from FMT-RAW-001 to FMT-RAW-004, not from the code. Every line and the closing marker move by the distance from the closing marker to the opening quotes. The quotes stand in column 16 plus the number of `$`. Hole lines are written so that their uniformly shifted column is the argument alignment the FMT-PIPE-022 example shows.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FirstLineStartingWithAHoleAtColumnZeroMovesRight` (`$`, `{x} a`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: FMT-PIPE-021 in IndentationPhase and CleanupPhase |
| `DoubleDollarFirstLineStartingWithAHoleAtColumnZeroMovesRight` (`{{x}} a`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-003 | `AssertMoved` | fails: same |
| `OnlyLineBeingAHoleAtColumnZeroMovesRight` (`{x}`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `LaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight` (`a` / `{x}`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `DoubleDollarLaterLineStartingWithAHoleBehindTextAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-003 | `AssertMoved` | fails: same |
| `LaterLineStartingWithAHoleBehindAHoleAtColumnZeroMovesRight` (`a {x}` / `{y} b`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `LaterLineStartingWithAHoleBehindAMultiLineHoleAtColumnZeroMovesRight` (`a {Call(1,` / `2)}` / `{y} b`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `LineStartingWithAHoleBehindAnEmptyLineAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-002 | `AssertMoved` | fails: same |
| `LineStartingWithAHoleBehindAnEmptyFirstLineAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-002 | `AssertMoved` | fails: same |
| `LineStartingWithAHoleBehindAWhitespaceOnlyLineAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `LastLineStartingWithAHoleInFrontOfTheClosingLineAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `FirstLineStartingWithAMultiLineHoleAtColumnZeroMovesRight` (`{Call(1,` / `2)} b`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `NestedLiteralLineStartingWithAHoleAtColumnZeroMovesRight` (nested `$"""` in a hole, its `{x}` line at column 0) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | fails: same |
| `LinesStartingWithTextAtColumnZeroMoveRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `DoubleDollarLineStartingWithASingleBraceAtColumnZeroMovesRight` (`{a} {{x}}`: the `{` is text) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-003 | `AssertMoved` | passes |
| `HoleLineStartingWithAnArgumentAtColumnZeroMovesRight` (`a {Call(1,` / `2)} b`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `HoleLineStartingWithTheHolesClosingBraceAtColumnZeroMovesRight` (`a {x` / `} b`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `LineStartingWithAHoleBehindRelativeIndentationAtColumnZeroMovesRight` (`a` / `  {x}`) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-002 | `AssertMoved` | passes |
| `EmptyLineBetweenTextLinesAtColumnZeroStaysEmpty` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-002 | `AssertMoved` | passes |
| `WhitespaceOnlyLineBetweenTextLinesAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `FirstLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight` (closing at column 4) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `DoubleDollarFirstLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-003 | `AssertMoved` | passes |
| `LaterLineStartingWithAHoleIndentedLeftOfTheQuotesMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `FirstLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft` (closing at column 24) | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `DoubleDollarFirstLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-003 | `AssertMoved` | passes |
| `LaterLineStartingWithAHoleIndentedRightOfTheQuotesMovesLeft` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `PlainLiteralAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `PlainLiteralIndentedRightOfTheQuotesMovesLeft` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-001 | `AssertMoved` | passes |
| `Utf8LiteralAtColumnZeroMovesRight` | FMT-PIPE-021, FMT-PIPE-022, FMT-RAW-004 | `AssertMoved` | passes |

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| A moved content line of an interpolated literal starts with a hole's `{` (or `{{`), and the closing marker is in column 0. This holds for the first or a later line, behind text, a hole, a multi-line hole, an empty line, or a whitespace-only line, as the last content line, and in a nested literal | LF, CRLF | formatter pipeline, every phase (`PhaseTreeRoundTrip`) | reproduced (13 members) | The IndentationPhase and CleanupPhase trees differ from their reparse. On the first line, `InterpolatedStringExpression has 4 children in the tree and 5 in the reparsed text`: the reparse adds a text node in front of the hole. On a later line, `InterpolatedStringText spans [62..81) in the tree and [62..98) in the reparsed text`: the reparse extends the text token over the moved indentation. Expected: no difference (FMT-PIPE-021) |
| Same members, annotations | LF, CRLF | every phase | holds | Every annotation stays on one element of its kind (FMT-PIPE-022) |
| Same members, final text | LF, CRLF | `ReihitsuFormatter.FormatSyntaxTree` | holds | The output equals the FMT-RAW-derived expectation. The fix must keep it unchanged |
| A line starts with text, an argument or the `}` inside a multi-line hole, extra relative indentation, an empty or whitespace-only line, or the closing marker; or the closing marker is indented and the literal moves right or left; or the literal is plain or UTF-8 | LF, CRLF | every phase plus final output | holds (16 members) | – |

## Not covered, and why
- **An interpolated literal moved left until its closing marker reaches column 0.** This member cannot occur. FMT-RAW-003 puts the closing marker under the first `"` behind at least one `$`, so the target column is at least 1. Any content line therefore keeps at least one character of indentation text in front of a hole. Plain and UTF-8 literals have no holes.
- **A hole line starting with a `{` of a construct inside the hole**, such as an array initializer. This is the hole-line kind, whose indentation is trivia in the hole, as for the argument and `}` members that hold. Its column is a hole-layout question outside this finding.
- **The closing line as its own member.** Every fixture ends with one, and the end token's text begins with the line break in front of it. `LastLineStartingWithAHole…` and `OnlyLineBeingAHole…` put it directly behind a hole.
- **Failure classes.**
  - FC-022 and FC-001 are the classes of this finding and are covered above.
  - Comment and directive gaps (FC-005, FC-019 to FC-021) cannot occur inside a content line, because a comment there is text.
  - Code-fix and naming classes do not apply to a formatter phase.
  - Idempotency (FC-010, FC-012) is checked by `AssertRuleResult`'s second pass and stability check.
- Node-level entry points are outside FMT-PIPE-021 by its own scope sentence, so they are not tested here.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests"
```
