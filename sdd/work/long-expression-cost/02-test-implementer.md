Role: test implementer
Commit: 23b7f33
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

Two new classes under `src/Reihitsu.Formatter.Test/Regression/FullPipeline/`:

- `LongExpressionCostTests` (`[DoNotParallelize]`): the reproduction. Each test formats one generated document of a base length and one eight times as long through `ReihitsuFormatter.FormatSyntaxTree` (the CLI path), under LF and CRLF, and asserts that neither the memory the formatting thread allocates (`GC.GetAllocatedBytesForCurrentThread`, exact for an input: the formatter runs on one thread) nor the shortest wall-clock time (min of 5 base runs, min of up to 3 large runs, stopping once within the bound) grows by more than 16. Linear cost gives about 8, quadratic about 64. No absolute budget (lead decision 3). The skeptic's generator shapes are reused.
- `LongExpressionLayoutTests`: exact-output tests at small size, with comments and a `#if` near the rewritten tokens, through `AssertRuleResult` (LF and CRLF, second pass, phase stability). Expected outputs are today's output, which this bug fix must keep byte-identical; I checked each against FMT-BREAK-018 where it applies.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LongExpressionCostTests.ConditionalChainInFalseBranchOneBranchPerLineGrowsLinearly` | defect A; FC-033 (reported shape) | `AssertCostGrowsLinearly` | fail: allocated 9.2 MB vs 401.5 MB (ratio 43.7), time ratio 11.4 |
| `LongExpressionCostTests.ConditionalChainInFalseBranchOnOneLineGrowsLinearly` | defect A; FC-027 | same | fail: alloc ratio 44.5, time ratio 38.3 |
| `LongExpressionCostTests.ConditionalChainInTrueBranchGrowsLinearly` | defect A; FC-027 | same | fail: alloc ratio 45.1, time ratio 41.5 |
| `LongExpressionCostTests.ConditionalChainInFieldInitializerGrowsLinearly` | defect A; FC-016 (statement position) | same | fail: alloc ratio 44.1, time ratio 40.8 |
| `LongExpressionCostTests.ParenthesizedConditionalChainGrowsLinearly` | defect A boundary (other side) | same | pass: alloc ratio 7.8, time ratio 8.1 to 9.1 |
| `LongExpressionCostTests.ArrayInitializerOnOneLineGrowsLinearly` | defect B; FC-033 (reported shape) | same | fail: time 77 ms vs 4432 ms (ratio 57.4); alloc ratio 7.7 |
| `LongExpressionCostTests.ArrayInitializerWithSeveralElementsPerLineGrowsLinearly` | defect B; FC-016 | same | fail: time ratio 58.9 |
| `LongExpressionCostTests.CollectionExpressionGrowsLinearly` | defect B; FC-016 | same | fail: time ratio 59.2 |
| `LongExpressionCostTests.ArgumentListGrowsLinearly` | defect B; FC-016 | same | fail: time ratio 58.6 |
| `LongExpressionCostTests.EnumMembersGrowLinearly` | defect B; FC-016 | same | fail: time ratio 64.1 |
| `LongExpressionCostTests.ClassMembersGrowLinearly` | defect B; FC-016 | same | fail: time ratio 47.5 |
| `LongExpressionCostTests.BlockStatementsGrowLinearly` | defect B; FC-016 | same | fail: time ratio 40.9 |
| `LongExpressionLayoutTests.FalseBranchChainWithCommentIsLaidOutOneBranchPerLine` | FMT-BREAK-018; FC-004/FC-021 (end-of-line comment behind a branch) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.TrueBranchChainOnOneLineIsLaidOutOneBranchPerLine` | FMT-BREAK-018; FC-027 | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.FieldInitializerChainOnOneLineIsLaidOutOneBranchPerLine` | FMT-BREAK-018 | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.DeepFalseBranchChainIsIndentedOneLevelPerConditional` | FMT-BREAK-018 (12 levels, expected generated) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.ParenthesizedChainOnOneLineStaysOnOneLine` | characterization (boundary of defect A) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.ArrayInitializerWithCommentAndDirectiveKeepsBoth` | characterization; FC-004/FC-021 (comment behind a comma, `#if` disabled element) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.WrappedCollectionExpressionAndArgumentListPutOneElementPerLine` | characterization; FC-021 (comment behind a comma) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.EnumMembersWithCommentAndBlankLinesAreNormalized` | characterization; FC-021 (comment line, blank lines) | `AssertRuleResult` | pass |
| `LongExpressionLayoutTests.ClassMembersAndBlockStatementsAreNormalized` | characterization (fields, comment, statements, one-line block) | `AssertRuleResult` | pass |

Every failing cost test fails on its own expected-versus-actual difference: a growth ratio of 41 to 64 against the bound 16. For defect A both measures fail (allocation is the deterministic one); for defect B only time fails, while allocation already grows linearly (ratio 7.7 to 8.0), so the allocation assertion there guards against a fix that trades time for quadratic allocation. The cost tests carry no requirement tag: no spec states a cost requirement (01-skeptic.md, *Surfaces*).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| right-nested `?:` in `return`, one branch per line, depth 100 vs 800 | LF | formatter, whole document | reproduced | alloc ratio 43.7, time ratio 11.4; expected about 8 (bound 16) |
| same, on one line / in the true branch / in a field initializer | LF | formatter, whole document | reproduced | alloc ratios 44 to 45, time ratios 38 to 42 |
| parenthesized nesting, depth 100 vs 800 | LF, CRLF | formatter, whole document | holds (boundary) | ratios 7.8 / 8.1 to 9.1 |
| single-line array initializer, 500 vs 4000 elements | LF | formatter, whole document | reproduced | time ratio 57.4 |
| wrapped array, collection expression, argument list, enum members, class fields, block statements | LF | formatter, whole document | reproduced | time ratios 41 to 64 |

CRLF runs only after the LF arm of a test passes, so the CRLF arm of each failing test is exercised once the fix lands; the passing boundary test ran both. Counterpart surface: no analyzer reports a cost, and code fixes format one node (01-skeptic.md, *Surfaces*), so neither is a cost surface here.

## Not covered, and why

- **Chain and concatenation rows of the issue**: dropped by lead decision 1 (already linear at d55885c); no test written.
- **Absolute time budget, operation count**: no deterministic operation count exists for defect B (the cost is inside Roslyn's `SyntaxNavigator`, which exposes no hook); allocation is used where it is deterministic and tracks the defect (A).
- **Comments on the `?` / `:` lines in a span FMT-UNSUP-008 refuses, and comments in front of a comma on a later line (FMT-UNSUP-009)**: refused inputs, already covered by `UnsupportedConditionalOperatorCommentTests` and the precheck tests; the layout tests use only supported comment positions.
- **Code-fix / Fix All, naming, token-gap failure classes**: no code fix or naming rule is changed; output must stay byte-identical, which the layout tests and the self-hosting run (`FormatterSelfHostingTests`, run in CI) cover.
- **FC-038 to FC-043 (unsupported shapes)**: the run declares no shape (01-skeptic.md, *Refuse option*).
- **Existing tests whose asserted output the change alters**: none; the change must not alter any output, so no existing test is changed or left with a disputed assertion.
- **Deep spine stack overflow**: outside the issue, recorded as a follow-up in 00-lead.md.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.FullPipeline.LongExpressionCostTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.FullPipeline.LongExpressionLayoutTests"
```

Add `-- --logger "console;verbosity=detailed"` to see the measured sizes and ratios each cost test writes. On the unchanged code the cost class takes about 1 m 47 s (the large list inputs take 3 to 8 s each); on linear code it should take a few seconds.

## Follow-up candidates

_None._ Observation only, for the lead: `ParenthesizedChainOnOneLineStaysOnOneLine` pins that a conditional nested in parentheses in a branch stays on one line; FMT-BREAK-018 says "every conditional expression that contains a nested conditional in a branch" is spread out and does not state whether a parenthesized one counts. The test characterizes today's output, which this run must keep.
