Role: test implementer
Commit: 161d307 (tests written on it; production code unchanged from `origin/main`)
Verdict: tests written

Follow-up round for lead decision 14 (rows 9–14 of `00-lead.md`, `04-test-implementer.md`, `05-spec-author.md` including *Row 13 carried out*, and FMT-SPACE-024/025 as they stand in `sdd/specs/formatter/horizontal-spacing.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-SPACE-024's list of positions and second example, and from FMT-SPACE-025's corrected example. Layout no requirement of this run decides (line breaks, blank lines, the `with` initializer's columns) is the unchanged pipeline's, run on this commit; each expected output was checked to be a fixed point of today's formatter, and to differ from today's output for its input only in the gaps the requirement decides.

No existing test was changed apart from the rename the lead asked for. The renamed test keeps its body and its assertion.

## Tests

Helper `AssertRuleResult` is `FormatterTestsBase.AssertRuleResult`: full pipeline under LF and CRLF, second pass, phase stability, per-phase round trip. "Expected now" is the result on 161d307.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SameLineGapRulePhaseTests.OneLineWithExpressionsGetOneSpaceOnEachSideInTheSpacingPhase` (renamed from `WithRequirementExampleGetsOneSpaceOnEachSide`; summary no longer claims the requirement example) | FMT-SPACE-025 | HorizontalSpacingPhase alone | red |
| `HorizontalSpacingRequirementTests.WithGetsOneSpaceOnEachSide` — FMT-SPACE-025's corrected example | FMT-SPACE-025 | AssertRuleResult | red |
| `HorizontalSpacingRequirementTests.ModifiersInFrontOfTypesAndRefExpressionsGetOneSpaceBehind` — FMT-SPACE-024's second example | FMT-SPACE-024 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.LambdaModifiersGetOneSpaceInFrontOfATupleReturnType` — `static`/`async`, joined and one line | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.RefAndReadonlyOfALambdaReturnTypeGetOneSpaceInFrontOfATupleType` — `ref`, `ref readonly` joined and one line, `static ref(int, int) ()` one line | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.RefOfARefExpressionInAnArgumentGetsOneSpace` — `b ? ref(x) : ref y`, `() => ref(x)`, joined and one line | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.RefOfARefExpressionInAStatementGetsOneSpace` — `ref var z = ref(x);`, `ref(b ? … )`, `z = ref(y);`, `return ref(x);`, `return ref*p;`; one line only (no join reaches a statement, `05-spec-author.md`) | FMT-SPACE-024 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.FunctionPointerParameterModifiersGetOneSpaceInFrontOfATupleType` — `ref`, `in`, `out` joined and one line, `ref readonly` one line | FMT-SPACE-024, FMT-BREAK-031 | AssertRuleResult | red |
| `PrefixKeywordSpacingTests.RefTypesOfDeclarationsKeepTheirGap` — outside the rule: `ref`/`ref readonly` of a local, a `foreach` variable, a ref field, a property, an indexer, a method's, a local function's and a delegate's return type; tight and spaced forms | FMT-SPACE-024 | AssertRuleResult (input unchanged) | green |
| `PrefixKeywordSpacingTests.LocalFunctionModifierInFrontOfATupleTypeKeepsItsGap` — `static(int, int) K()` and `static (int, int) L()` | FMT-SPACE-024 | AssertRuleResult (input unchanged) | green |
| `PrefixKeywordSpacingTests.MemberReadonlyInFrontOfATupleTypeKeepsItsGap` — `public readonly(int, int) M()` and the spaced form | FMT-SPACE-024 | AssertRuleResult (input unchanged) | green |
| `PrefixKeywordSpacingTests.ConstructorInitializersAndIndexerDeclarationsKeepTheirGap` — `: base(a)`, `: base (1)`, `: this(a)`, `: this (1)`, `this[int i]`, `this [int i, int j]` | FMT-SPACE-024 | AssertRuleResult (input unchanged) | green |

Why the red ones are red: every one fails only on the one-line forms of the position it covers, which keep their tight gap today (`static(int, int) ()`, `ref(x)`, `delegate*<ref(int, int), …>`, `return ref*p`, `var(u, w) = …`, `p.Q()with`). Each joined form in these tests already comes out as expected today (the join writes the space). For `WithGetsOneSpaceOnEachSide` the difference is the space in front of `with` and the initializer one column further right, which follows from that space. The phase test is red for the same reason as in `04-test-implementer.md`. Checked by formatting each test's input and expected output with `scripts/format.sh` on this commit: every expected output is unchanged by a pass, and the diff against today's output of the input is only those gaps.

## Not covered, and why

- Wrapped forms of the positions no join reaches today (`ref⏎(int, int)` of a local, field, property or `foreach` variable; `static⏎(int, int) K()`; `return ref⏎(x);`): no joined form exists to put beside the one-line form (`05-spec-author.md`, lead decision 10's reasoning); the join itself is a recorded follow-up candidate.
- `scoped(int, int)`: never compiles (`05-spec-author.md`), so no caller reaches it.
- A keyword in front of a pointer type: reduces to the tuple and operand rows already covered (`05-spec-author.md`).
- Comment and directive in the gap: FMT-SPACE-024's comment case is `CommentBehindTheKeywordKeepsItsPlace` and the phase test for a documentation comment (from `04-test-implementer.md`); the new positions reach the same keyword-gap rule and add no new comment handling.
- CRLF and second pass: inside `AssertRuleResult` for every test above.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PrefixKeywordSpacingTests|FullyQualifiedName~HorizontalSpacingRequirementTests.ModifiersInFrontOfTypesAndRefExpressionsGetOneSpaceBehind|FullyQualifiedName~HorizontalSpacingRequirementTests.WithGetsOneSpaceOnEachSide|FullyQualifiedName~OneLineWithExpressionsGetOneSpaceOnEachSideInTheSpacingPhase"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

The architecture `Sdd` tests pass 51 of 51 with these tests.
