Role: test implementer
Commit: (this commit; see `git log`)
Verdict: tests written

Structural round after audit 25, for lead decisions 15 (auditor finding #3) and 16 (finding #4). Fresh agent. The prompt named the change asked for and pointed to the inputs; it carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

Expected values come from the requirements and change-doc entry 9, not from code output:
- FMT-INDENT-031 (anonymous method: braces under `delegate`, or under `async` for an `async` one) and FMT-INDENT-028 (block lambda: braces at `async`, else at `(` of a parenthesized lambda or the parameter of a simple lambda), statements one level deeper (FMT-INDENT-002 via 031; the line-break phase's relative indentation via 028).
- FMT-INDENT-030 and FMT-RAW-007 bullet 1: the anchor is measured after the move. The anchor stands on a line FMT-RAW-009 leaves where it is, so its column is its written column (8, or 12 one block deeper).
- Entry 9, second case: "after" is the block under the anchor, body one level deeper, whether the literal moves or not.
- Nested constructs: FMT-INDENT-029 / SHARED-CHAIN-007 (links under `.A`), FMT-INDENT-012 (`2` under `1`), FMT-RAW-001 (inner literal under its quotes), FMT-INDENT-028 (inner lambda block at its `(`), each measured from the block's final column. I cross-checked these inner columns against the same block in plain code outside any hole (block indentation places it there; scratch fixture, not committed): identical.
- `///` lines: FMT-INDENT-008 (every `///` marker in the column of the documented token).

Literal states (quotes in column 17, or 21 one block deeper): moved right = content 13 columns left of the quotes; aligned = content under the quotes; moved left = content 8 columns right of the quotes.

## Tests

New class `src/Reihitsu.Formatter.Test/Regression/Indentation/HoleAnchorInStatementColumnAlignmentTests.cs` (80 tests) and two tests appended to `src/Reihitsu.Formatter.Test/Regression/Indentation/HoleAnchorOnUnmovedLineAlignmentTests.cs`. All use `AssertRuleResult` (exact output LF and CRLF, second pass, phase check, tree round-trip). "Expected now" is on b0b8aa7 plus these test files.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| Family A, 60 tests: `{Construct}{Unmoved}InStatementColumn{Literal}IsAlignedUnderItsAnchor` — Construct ∈ `AnonymousMethod`, `AsyncAnonymousMethod` (tagged FMT-INDENT-031) / `ParenthesizedLambda`, `AsyncLambda`, `SimpleLambda` (tagged FMT-INDENT-028); Unmoved ∈ `BehindCommentEndLine` (`aaaaa */<anchor>`), `BehindDocumentationCommentEndLine` (`K {q/** one` ⏎ `aa */.N(<anchor>`), `OnVerbatimContinuationLine` (`$@`, `aaaaa{M(<anchor>`), `OnInterpolatedVerbatimContinuationLine` (`@$`); Literal ∈ `InLiteralMovedRight`, `InAlignedLiteral`, `InLiteralMovedLeft`. Anchor in column 8, the statement's column | FMT-INDENT-031 or FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-001, FC-003, FC-010, FC-016 | `AssertRuleResult` | all 60 fail: block lines in column 13 (moved right) or column 0 (aligned, moved left), body not deeper; stable on head. Every other line matches |
| Family B, 8 tests: `{AnonymousMethod,ParenthesizedLambda}BehindCommentEndLine{OneColumnLeftOf,OneColumnRightOf}StatementColumn{InLiteralMovedRight,InAlignedLiteral}IsAlignedUnderItsAnchor` (anchor 7 and 9) | FMT-INDENT-031 or FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | all 8 pass (holding members on either side of the boundary) |
| Family C, 4 tests: `{AnonymousMethod,ParenthesizedLambda}BehindCommentEndLineInColumnOfStatementOneBlockDeeper{InLiteralMovedRight,InAlignedLiteral}IsAlignedUnderItsAnchor` (statement in an `if` block, column 12; anchor 12) | FMT-INDENT-031 or FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007; FC-003 | `AssertRuleResult` | all 4 fail: block lines in column 13 / 0 |
| Family D, 8 tests: `{AnonymousMethod,ParenthesizedLambda}WithInner{Chain,WrappedArguments,NestedRawLiteral,NestedBlockLambda}BehindCommentEndLineInStatementColumnInLiteralMovedLeftIsAlignedAndStable` (anchor 8, literal moved left; the auditor's non-idempotent members) | FMT-INDENT-031 or FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007, plus FMT-INDENT-029 / FMT-INDENT-012 / FMT-RAW-001 / FMT-INDENT-028 for the inner construct; FC-010, FC-016 | `AssertRuleResult` | all 8 fail: block in column 0, inner aligned lines in the wrong column; head is not idempotent on all 8 (e.g. `2);` in column 1 on pass 1, column 9 on pass 2) |
| `DocumentationCommentInBlockOfAnonymousMethodLeftOfShiftKeepsEveryLineInTheStatementColumn` (`@$`, anchor 4, moved right; body column −5 before the move; `///` lines input at 4/8/2) | FMT-INDENT-031, FMT-INDENT-030, FMT-INDENT-008, FMT-RAW-007; FC-017, FC-021 | `AssertRuleResult` | pass |
| `DocumentationCommentInBlockOfBlockLambdaLeftOfShiftKeepsEveryLineInTheStatementColumn` (same with `() =>`) | FMT-INDENT-028, FMT-INDENT-030, FMT-INDENT-008, FMT-RAW-007; FC-017, FC-021 | `AssertRuleResult` | pass |

Totals: the two classes give 98 tests, 72 failing (families A, C, D) and 26 passing (B, the two `///` tests, the 16 earlier tests). The full formatter project gives Failed 72, Passed 4189 — no other test is affected. Architecture tests pass (92).

Each failure was checked for the right reason: I ran every input through the CLI built at b0b8aa7 twice and diffed against the expected output. In all 72, the only differing lines are the block's lines (and the inner construct's lines in family D), with the same text and only a different leading whitespace.

**Decision 16 mutation.** In a scratch worktree at b0b8aa7 I replaced `placedColumns[continuationLine] = column;` in `IndentationRewriter.ApplyIndentation` with a no-op. Both `///` tests then fail (`/// b` and `/// c` in column 13 instead of 8, `/// a` stays in 8); the other 16 tests in the class still pass. On head both pass. The worktree is removed.

## Not covered, and why

- **A `/** */` comment directly behind `(` (`M(/** one⏎aaaaa */delegate`).** On head, the documentation phase moves `/** one` onto a new line with a blank line above it. FMT-DOC-001 says it does not decide a documentation comment "inside an expression", so that layout is unspecified, and pinning it would assert code output. The `/** */` member uses a comment that a `.` follows instead: FMT-DOC-003 keeps it as written, and its end line is still an unmoved line in the hole. Observation for the lead, not a diagnosis: the relocation inside an expression is behavior no requirement decides.
- **A `///` comment between `delegate` and `{`** (auditor's optional second case). On head, the blank-line phase puts a blank line between `delegate` and the comment. No requirement I found decides that blank line, so the test would pin unspecified output. The two tests inside the block already reach the loop (mutation above).
- **Anchor column sweep beyond 7/8/9.** Columns 3–7 and 9–22 are the holding members the auditor ran, and the earlier 16 tests pin 4, 6, 12, 13, 22.
- **Family D for the aligned and moved-right literals, and for the three other unmoved kinds.** The auditor's non-idempotent members are the left-moving literal behind a `/* */` end line; families A and C pin the block's placement for every kind and state, and the inner alignment follows the block (FMT-INDENT-030).
- **Family B and C for the async and simple-lambda constructs and the other unmoved kinds.** Family A covers every construct × kind × literal state at the failing column; B and C pin the boundary and the statement depth for one anonymous-method and one lambda member each, as the auditor suggested.
- **CRLF and second pass.** `AssertRuleResult` runs both in every test. Failing tests stop at LF.
- **FC-020.** A directive cannot stand in a hole. **FC-007, FC-008, FC-028.** No code fix and no node-level entry point in scope.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~HoleAnchorInStatementColumnAlignmentTests|FullyQualifiedName~HoleAnchorOnUnmovedLineAlignmentTests"
```
