Role: test implementer
Commit: c5f948c
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. Expected values come from FMT-BREAK-044's example and the unchanged requirements (decision 2), never from a patched copy; the three C# 11 boundary fixtures below the spec example assert today's `main` output, which the run does not change.

## Tests

File `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs` (new, class `NonVerbatimInterpolationLineBreakTests`) unless noted. "Builder" fixtures wrap the statement in `using System; class C { int F(Func<int> f) {…} string M(<params>) { <statement> ⏎⏎ return s; } }`, already formatted outside the interpolation. Skeptic fixture IDs (01-skeptic.md) in brackets.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleKeepsNonVerbatimInterpolationsOnOneLineInCSharp10 | FMT-BREAK-044 | `AssertRuleResult` (LF+CRLF, 2nd pass, phase stability) | fail |
| SpecExampleBreaksEveryInterpolationInCSharp11 | FMT-BREAK-044 | `AssertRuleResult` | pass |
| AnonymousMethodInInterpolationKeepsItsLineInCSharp10 (issue input, exact) | FMT-BREAK-044 | `AssertRuleResult` | fail |
| AnonymousMethodInInterpolationKeepsItsLineInCSharp73 [c01 at 7.3] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| ExpressionBodiedMethodGetsBlockBodyWhileInterpolationKeepsItsLineInCSharp10 [c23] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| EmptyAnonymousMethodInInterpolationKeepsItsLineInCSharp10 [c27] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LambdaBlockInInterpolationKeepsItsLineInCSharp10 [c02] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| EmptyLambdaBlockInInterpolationKeepsItsLineInCSharp10 [c26] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LambdaBlockStatementsInInterpolationKeepTheirLineInCSharp10 [c10] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| IfStatementInInterpolationGetsBracesOnItsLineInCSharp10 [c11] → `if (b) { return 1; } return 2;` | FMT-BREAK-044 | `AssertRuleResult` | fail |
| SwitchStatementInInterpolationKeepsItsLineInCSharp10 [c12] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| SwitchSectionWithStatementAndReturnInInterpolationStaysUnbracedInCSharp10 [c28] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LocalFunctionInInterpolationGetsBlockBodyOnItsLineInCSharp10 [c18] → `int G() { return 1; }` | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LocalFunctionConstraintInInterpolationKeepsItsLineInCSharp10 [c35] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LocalFunctionAttributeInInterpolationKeepsItsLineInCSharp10 [c37] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| TryCatchInInterpolationKeepsItsLineInCSharp10 [c25] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| UsingStatementInInterpolationGetsBracesOnItsLineInCSharp10 [c34] → `using (…) { return 1; }` | FMT-BREAK-044 | `AssertRuleResult` | fail |
| NestedLambdaBlockInInterpolationKeepsItsLineInCSharp10 [c32] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| ObjectInitializerInInterpolationKeepsItsLineInCSharp10 [c03] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| NestedObjectInitializerInInterpolationKeepsItsLineInCSharp10 [c21] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| CollectionInitializerInInterpolationKeepsItsLineInCSharp10 [c04] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| AnonymousObjectInInterpolationKeepsItsLineInCSharp10 [c05] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| NestedAnonymousObjectInInterpolationKeepsItsLineInCSharp10 [c33] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| WithInitializerInInterpolationKeepsItsLineInCSharp10 [c13] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| SwitchExpressionInInterpolationKeepsItsLineInCSharp10 [c06] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| SwitchExpressionArmWithWhenInInterpolationKeepsItsLineInCSharp10 [c36] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| LambdaAttributeInInterpolationKeepsItsLineInCSharp10 [c14] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| VerbatimStringNestedInNonVerbatimInterpolationKeepsItsLineInCSharp10 [c16] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| NonVerbatimStringNestedInVerbatimInterpolationKeepsItsLineInCSharp10 [c17] | FMT-BREAK-044 | `AssertRuleResult` | fail |
| AtDollarVerbatimInterpolationIsBrokenInCSharp10 (`@$"`; `$@"` [c15] is the spec example's `b`) | FMT-BREAK-044 | `AssertRuleResult` | pass |
| PropertyPatternInInterpolationKeepsItsLineInCSharp10 [c07] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| ArrayInitializerInInterpolationKeepsItsLineInCSharp10 [c08] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| QueryExpressionInInterpolationKeepsItsLineInCSharp10 [c09] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| CommentInInterpolationKeepsItsLineInCSharp10 [c24] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| DelimitedDocumentationCommentInInterpolationKeepsItsLineInCSharp10 [c29] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| SwitchSectionWithTwoStatementsInInterpolationGetsNoLineBreakInCSharp10 (new, `case 1: G(); G(); return 1;`) — asserts only that the `var s = …` line ends with ` b";` plus second-pass/phase stability | FMT-PIPE-003 | `FormatWithPipeline` + `AssertSecondPassIsStable` | fail |
| LambdaBlockInInterpolationIsBrokenInCSharp11 [c02 at 11] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| ObjectInitializerInInterpolationIsBrokenInCSharp11 [c03 at 11] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| SwitchExpressionInInterpolationIsBrokenInCSharp11 [c06 at 11] | FMT-BREAK-044 | `AssertRuleResult` | pass |
| FormatDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project (spec input, C# 10 project) | FMT-BREAK-044 | `ReihitsuFormatter.FormatDocumentAsync`, LF+CRLF | fail |
| FormatNodeInDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project (target method `M`, as a code fix calls it) | FMT-BREAK-044 | `ReihitsuFormatter.FormatNodeInDocumentAsync`, LF+CRLF | fail |
| `tests/Reihitsu.Cli.Test/EndToEnd/CliLanguageVersionEndToEndTests.cs` LangVersion10KeepsInterpolationOnOneLine (issue input via `--lang-version 10`, then `--check`) | FMT-BREAK-044 | `Program.Main` e2e | fail |
| `tests/Reihitsu.Analyzer.Test/Formatting/RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests.cs` LambdaBodyHoldingAnInterpolatedStringIsReportedBeforeCSharp11 (body that contains `$"{1}"` outside its interpolation, C# 10, reported) | RH5417-005 | `Verify` | pass |

Every failing test fails on the issue's own difference: the first mismatch is a line break written after `delegate`/`=>`/`new P`/`switch`/`with`/`[A]` inside the interpolation where the expected output has one space (checked for all 31 failing formatter tests, the CLI test, and the property test).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input, `--lang-version 10` | LF | CLI (`Program.Main`) | reproduced | observed exactly the issue's "Actual Formatted Output" (`delegate⏎{⏎return 1;⏎})`); expected one line `M(delegate { return 1; })` |
| Issue input, C# 10 parse options | LF, CRLF | formatter pipeline | reproduced | same difference under both endings |
| Spec input, C# 10 project | LF, CRLF | `FormatDocumentAsync`, `FormatNodeInDocumentAsync` (code-fix path) | reproduced | `a`, `c`, `d` broken in the interpolation |
| Same input, C# 7.3 | LF, CRLF | formatter pipeline | reproduced | same difference |
| Sibling shapes c02–c37 (block, statements, initializers, switch expression, lambda attribute, nested strings both ways, lambda in lambda) | LF, CRLF | formatter pipeline | reproduced | first line break written into the interpolation |
| Same constructs at C# 11; `$@"`/`@$"` at C# 10 | LF, CRLF | formatter pipeline | holds | broken as the other requirements state (unchanged) |
| RH5417 counterpart | — | analyzer | holds | existing RH5417-005 tests pin both sides; new test pins "contains a `$"` string" side |

## Not covered, and why

- **Directive in the gap (FC-019/FC-020).** A directive needs a line of its own, so it cannot stand in a one-line non-verbatim interpolation of code that compiles before C# 11; the comment case is covered (c24, c29). c30 (`x /** d */`) not added: same gap kind as c24/c29, both green.
- **Join direction.** Input that already holds a line break in such an interpolation does not compile and is explicitly not the requirement's subject (FMT-BREAK-044, last paragraph).
- **c19 (ternary), c31 (chain), c22 (stackalloc initializer)** — no new test: the skeptic found them already one-line and they reach the same "nothing to break" outcome as c07–c09; c08 pins the array-initializer sibling of c22.
- **c14 and c28 at C# 11** — not pinned: the run does not change C# 11 output; the version boundary is pinned by the spec example (four string shapes) and c02/c03/c06 at C# 11. `AnonymousMethodHoleAlignmentTests` already pins the Latest-version layout of anonymous methods and lambdas in `$"` holes.
- **FC-028 (`FormatNode` on a `SyntaxFactory` node)** resolves to the newest version by FMT-PIPE-008; not this run (skeptic).
- **Raw strings** need C# 11, so they cannot meet the condition.
- **Existing tests whose output changes:** none. `rg` over `tests/` for files using `LanguageVersion.CSharp1–10`/`CSharp7_3`/`--lang-version` ≤ 10 that also hold an interpolation with braces found only test-harness strings, no fixture.
- **FC-007/008/009, FC-018, FC-038–043** — no code fix and no unsupported shape in this run.
- **RH5417 refactor (decision 4)** — the existing RH5417-005 tests (non-verbatim, nested both ways, C# 11, `$@"`, `@$"`) plus the new contains-side test pin the predicate's both sides.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliLanguageVersionEndToEndTests.LangVersion10KeepsInterpolationOnOneLine"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests"
```

Results on this head: formatter 31 failed / 10 passed; CLI new test failed (16 others passed); analyzer 61 passed; architecture 198 passed (FMT-BREAK-044 now covered).

## Open point for the lead

**A switch section with two statements inside such an interpolation, C# 10** (`$"a {F(() => { switch (x) { case 1: G(); G(); return 1; default: return 2; } })} b"`). FMT-SWITCH-003 counts the section as multi-line (more than one statement), so FMT-SWITCH-001 demands braces for every section and FMT-SWITCH-015 demands they start their own lines, which FMT-BREAK-044 does not cover (it names only the line-break phase) and CS8967 forbids. The spec does not say which wins; the test I wrote (`SwitchSectionWithTwoStatementsInInterpolationGetsNoLineBreakInCSharp10`) asserts only what FMT-PIPE-003 guarantees — no line break on the statement's line and a stable result — and leaves braces-on-one-line versus unbraced open. Today the line-break phase breaks the block before the switch phase runs, so the switch phase's own behavior here is not observable on `main`. The single-statement-plus-return c28 is settled by FMT-SWITCH-003 (not multi-line, stays unbraced) and asserted exactly.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
