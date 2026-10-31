Role: test implementer
Commit: 6cb4329
Verdict: tests written

Repair round 3, scope audit finding #9 (`23-auditor.md`) only. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## What changed

`AssertAnnotationsKept` in `src/Reihitsu.Formatter.Test/Unit/Pipeline/RawStringMovedLineStartTests.cs` now annotates every node and every token of the code, not only each literal's tokens and the nodes inside its holes.

I derived the check from FMT-PIPE-022 and the scope decision `2026-10-31-phase-tree-round-trip-annotation-scope.md` (option A: the nodes and tokens of the code). It is not taken from the auditor's narrower suggestion. The helper takes `root.DescendantNodesAndSelf()` and `root.DescendantTokens()`, and neither one descends into structured trivia, so the set is exactly what the requirement covers. It covers:

- the `InterpolatedStringExpressionSyntax` node that the insertion path rebuilds, with its `InterpolatedStringText` nodes;
- every node and token around the literal, up to the root.

The pass criterion has not changed: in every phase's output, each annotation is carried by exactly one element of the original kind. The exemptions in FMT-PIPE-022 do not come up in these fixtures:

- no phase removes an element;
- no phase writes an element more than once;
- annotations a phase adds itself have a different kind.

The 29 test methods, their tags, inputs, and expected texts are unchanged. The `Microsoft.CodeAnalysis.CSharp.Syntax` using became unused and was removed.

**Falsification, watched.** As a temporary, uncommitted mutation, I applied the auditor's falsifying change to `RawStringLineShifts.cs:305`. It builds the node with `SyntaxFactory.InterpolatedStringExpression(node.StringStartToken, …, node.StringEndToken)` instead of `node.WithContents(…)`. With that change, the five members that take the insertion path fail in `IndentationPhase` with `dropped annotations (FMT-PIPE-022)`:

- `FirstLineStartingWithAHoleAtColumnZeroMovesRight`
- `DoubleDollarFirstLineStartingWithAHoleAtColumnZeroMovesRight`
- `OnlyLineBeingAHoleAtColumnZeroMovesRight`
- `FirstLineStartingWithAMultiLineHoleAtColumnZeroMovesRight`
- `NestedLiteralLineStartingWithAHoleAtColumnZeroMovesRight`

The other 24 pass, which is expected because they never rebuild the node. I reverted the mutation with `git checkout`, and the production code at `6cb4329` matches its parent.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| All 29 methods of `RawStringMovedLineStartTests` (tags unchanged: FMT-PIPE-021, FMT-PIPE-022, and FMT-RAW-001 or FMT-RAW-003 per method, as listed in `19-test-implementer.md`); the strengthened `AssertAnnotationsKept` closes FC-017 for FMT-PIPE-022 | as tagged; FC-017 | `AssertMoved` → `AssertAnnotationsKept`, `FormatSyntaxTree`, `AssertRuleResult` | pass (29/29 at `6cb4329`) |

`PhaseAnnotationTests` also passes (7/7). I did not change it.

## Not covered, and why

- **`PhaseAnnotationTests`.** The auditor noted that its annotation set also leaves out the literal nodes. Finding #9 names only `RawStringMovedLineStartTests`, and the node rebuild it guards is reached only by first-line-hole inputs, which this class covers. I left `PhaseAnnotationTests` unchanged to stay inside this round's scope. The lead can extend it if wanted.
- **Findings #10 and #11** are documentation and dead-branch findings in production code. They are outside the test implementer's scope.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.RawStringMovedLineStartTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Pipeline.PhaseAnnotationTests"
```
