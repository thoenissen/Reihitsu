Role: auditor
Commit: e835366
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit of thoenissen/Reihitsu#1494.
- Head e835366037a1c89b8037ab0a2200e283ba907fd8 is checked out. `origin/main` is 932761907ae10ef593a4616d09bc5ba141c8bc84, as the prompt says, and it is an ancestor of head.
- The diff against main is what handovers 01–11 describe:
  - two production files: `LineBreakListRewriter.VisitParameterList` and `LambdaAlignmentContributor`;
  - the tests and test helpers listed in 06 and 08;
  - FMT-BREAK-011, FMT-INDENT-031, FMT-RAW-007 and the ID ledger;
  - the change doc;
  - `sdd/work/`.
- `scripts/lane-check.sh behavior-change` confirms the claimed lane.
- Evidence comes from scratch fixtures in the session scratchpad, outside the repository:
  - `scripts/format.sh --force` was run twice per corpus, under LF and CRLF;
  - `scripts/trace.sh` was run on the issue's first input;
  - `scripts/apply-fix.sh` was run for RH5008 and RH5417, plus analyzer sweeps of RH5002, RH5008, RH5103, RH5204, RH5206, RH5301, RH5308 and RH5417 over formatted output.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` (`IsAlignedInRawStringHole`); `sdd/specs/formatter/indentation.md` (FMT-INDENT-031, first and second paragraph); change doc *Unchanged*, third bullet | FC-001, FC-002, FC-016, FC-022 | – | same requirement | See *Finding 1* below. | See *Finding 1* below. |

### Finding 1

**What goes wrong.**

- FMT-INDENT-031's first sentence covers every anonymous method that "lies in an interpolation hole of a multi-line interpolated raw string literal". The guard, however, stops at the innermost hole of *any* interpolated string. When that hole belongs to a regular `$"…"`, a verbatim `$@"…"`, or a single-line raw `$"""…"""` string that is nested in a hole of a multi-line raw literal, the guard returns false.
- `LayoutComputer.RegisterRawStringHoleLines` still registers those lines with the outer raw hole. So FMT-RAW-007 forbids the scope-depth column, and `ReleaseUnplacedRawStringHoleLines` releases the lines unplaced. The issue's defect survives for this construct.

Counterexample, formatted twice, stable under LF and CRLF:

```csharp
var b = $"""
    Nest {$"in {M(delegate { return 1; })}"}
    """;
var c = $"""
                 Nest {$"in {M(delegate { return 1; })}"}
                 """;
```

Output: `{`, `return 1;`, and `})}"}` end in column 13 for `b` (moved literal) and in column 0 for `c` (aligned literal). So the column depends on where the literal stood. The block-lambda counterpart in the same position (`{$"in {M(() => { return 1; })}"}`) is aligned under its `(`, which goes against the requirement's "aligns like a block lambda".

Failing members:

| Nested string between the anonymous method and the multi-line raw hole | Aligned literal | Moved literal |
|---|---|---|
| regular `$"…"` | fails | fails |
| verbatim `$@"…"` | fails | fails |
| single-line raw `$"""…"""` | fails | fails |

The spec's second paragraph also says that in a hole of a single-line raw literal the block "takes its column from the scope depth as before". The change doc's *Unchanged* says the same. For the nested single-line member this is false: the lines take column 0 or 13, not the scope depth.

Members that hold:
- a nested *multi-line* raw literal (`AnonymousMethodInNestedLiteralInsideBlockLambdaOfOuterHoleIsAligned`);
- a block lambda in the same nested position;
- an anonymous method inside a block body, which takes the scope depth by design;
- a single-line raw literal that is not nested in a multi-line one.

**Reachability.** Whole-document formatting (CLI, `FormatDocumentAsync`) of C# 11+ code with such a nested string reaches the case.

**Suggested change.** The owner decides between two options. In either case, a test pins the member set.
- (a) Make the guard skip holes of strings that are not multi-line raw, and keep walking. Only a `BlockSyntax`, or the first multi-line raw hole, ends the walk. The decision then agrees with `RegisterRawStringHoleLines` (see *Architecture findings*), and the spec's single-line sentence is narrowed to a literal that is not nested in a multi-line raw hole.
- (b) Narrow FMT-INDENT-031's first sentence to the innermost hole. State the nested members as not covered, and file a follow-up. Note that (b) leaves the issue's literal-position-dependent column in place for these members.

Visible output is in question for (a) unless the lead reads maintainer answer 1 and the general clarification ("same formatting in holes as everywhere") as covering it. Otherwise it is an output question.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Lambda `(` is exempt from the declaration-line collapse | `ParameterListSyntax.Parent is ParenthesizedLambdaExpressionSyntax`, evaluated before the base visit, so the original parent is used | `ListElementInteriorJoinRewriter.IsOwnedGap` (`ParameterListSyntax { Parent: not ParenthesizedLambdaExpressionSyntax }`); `ParameterListParentPolicy` in the analyzer (RH5105) | Same predicate and same span in all three | ok |
| Comma and `)` clauses still apply to lambdas | `CollapseFirstElementToSameLine`, `CollapseSeparatorsToPreviousParameterLine`, and `CollapseCloseParenToParameterLine` are unconditional | RH5106, RH5107 (lambdas included) | agree | ok |
| An anonymous method is aligned in a hole | Ancestors of the anonymous method, up to the first `BlockSyntax` or the first `InterpolationSyntax` of any string kind | `LayoutComputer.RegisterRawStringHoleLines` registers lines with the innermost *multi-line raw* hole and skips other interpolated strings; `LayoutModel.AcceptsWrite` (contributor node inside the registered hole) | Disagree for holes of non-raw or single-line raw strings nested in a multi-line raw hole | finding #1 |
| "Inside the block body of a lambda or anonymous method in the same hole" | First `BlockSyntax` ancestor before the hole | Spec exception paragraph | Inside a hole's expression, a block can only be a lambda or anonymous-method body, so the two agree | ok |
| "Its opening brace starts a line" (spec qualifier) | Not checked; the contributor shifts from the brace's line | FMT-BREAK-002 always puts the brace on its own line, including with a block comment or an end-of-line comment behind `delegate` (traced) and for an empty body (`EmptyAnonymousMethodInHoleIsAlignedUnderDelegate`) | No reachable input where the brace does not start a line | ok |
| Anchor token | `async` if present, else `delegate` | FMT-INDENT-028 lambda anchors (`async`, else `(`, so a `static` lambda is anchored past `static`) | Matches maintainer answers 4 and 6 and lead decision 3 | ok |
| `delta == 0` early return (no placement recorded) | Brace line layout against the anchor | `LayoutModel.ShiftRange` records placement only when delta ≠ 0 | Sweep of anchor columns 0–12 characters into the hole (pre-shift columns 3–15), for anonymous methods and block lambdas, with the literal moved: all aligned and stable, so no released-unplaced case occurs | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Who owns the gap in front of a parenthesized lambda's `(` | `LineBreakListRewriter.VisitParameterList` (`isParenthesizedLambdaParameterList`), `ListElementInteriorJoinRewriter.IsOwnedGap` (line 166), `Reihitsu.Analyzer/Core/ParameterListParentPolicy` | Three statements of one predicate, identical today. The formatter-analyzer split is pre-existing and intended (SHARED-DIAG-010) | ok |
| Anchor of a block-bodied anonymous function | `LambdaAlignmentContributor` only (`rg "DelegateKeyword\|AsyncKeyword" src/Reihitsu.Formatter`; the `ExpressionBodiedTransformUtilities` hit is a different decision) | ok |
| Which raw-string hole a line or node belongs to | `LayoutComputer.RegisterRawStringHoleLines` (inline `InterpolatedMultiLineRawStringStartToken` check, innermost multi-line raw hole); `LambdaAlignmentContributor.IsAlignedInRawStringHole` (innermost hole of any kind, then `RawStringLineShifts.IsMultiLineRawStringLiteral`); `RawStringLineShifts.IsNestedInRawStringHole` (any multi-line raw ancestor) | The contributor's statement differs from the registration's statement; this causes finding #1. See *Architecture findings* |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ParenthesizedLambdaLineStartTests` (15 tests) | FMT-BREAK-011 and FMT-BREAK-030: the lambda's line break and blank lines are kept, the joined gaps are joined, and no phase changes the final output on its own | A removed blank line, an undone FMT-BREAK-021 or FMT-BREAK-022 break, a phase reversal | `AssertRuleResult` (exact output, LF and CRLF, two passes, phase check, tree round-trip) | adequate |
| `BlankLineInFrontOfLambdaBehind{ExpressionLambdaArrow,Return}IsKept`, `LambdaBehindReturnWithoutBlankLineKeepsItsLineBreak` | Blank-line state only; the column is undecided (lead decision 6, follow-up candidate 3) | The blank line is lost or added; instability | `AssertBlankLinesAbove` (blank-line state, LF and CRLF, and `AssertSecondPassIsStable`) | Deliberately weaker; justified by an open column, recorded in decision 6 |
| `AnonymousMethodHoleAlignmentTests` (19 tests) | FMT-INDENT-031 columns: under `delegate` or `async`, measured after the move, for left, right, and no shift; the exceptions (block body, single-line literal); comments kept | Any column off, a lost comment, instability | `AssertRuleResult` | Adequate for the members covered. Finding #1's nested-string members are missing |
| `RawStringInterpolationHoleTests.AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` | Entry 1, exactly (it was second-pass stability only) | Columns 13 or 0 | `AssertRuleResult` | Stronger than before |
| `RawStringInterpolationHoleTests.ParenthesizedLambdaStartingAnUnplacedHoleLineKeepsItsColumn` | Entry 6, aligned literal | Lambda in column 0 | `AssertRuleResult` | Adequate. The moved-literal cell was traced here and holds (column 30 after a 13-column shift) |
| `ConstraintClauseInHole*` (two tests) | FMT-INDENT-017 under the FMT-INDENT-031 layout | Wrong `where` column or block column | `AssertRuleResult` | adequate |
| `PhaseStabilityCheckTests.CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack` | The check reports exactly the earlier phase of a reversal pair, with its output, under LF and CRLF | The check reports nothing, or the wrong phase or output; the fixture is not a fixed point (asserted first) | `PhaseStabilityCheck.FindChangingPhases` with injected phases | adequate; independent of any production defect |
| `RH5417…FormatterTests.VerifyFormatterFixesAnonymousMethodBodyInRawStringHole` | Formatter output in a hole is RH5417-clean and matches the aligned shape | A diagnostic on the output, or a different layout | `VerifyFormatter` | adequate |
| `PhaseReversalExceptionList` emptied | Every former reversal is gone | A stale entry fails its own test; a new reversal fails the phase check | built in | Adequate: the issue's Done-when (exceptions removed) is enforced |

## Failure classes dismissed

- **FC-003.** Both sides are covered:
  - a blank line and no blank line;
  - a joined gap and a kept gap;
  - a left shift, a right shift, and no shift;
  - `static`, `async`, `static async`, and no modifier;
  - a block body against an expression body;
  - a single-line against a multi-line literal.
- **FC-004, FC-005, FC-019, FC-020, FC-021.**
  - The lambda change removes an edit and adds no join. The corpus with an end-of-line comment, a `/* */` line, `#if`, and blank lines in front of a lambda gives the output of the `(1)` control, apart from shapes whose syntax differs.
  - The anonymous-method change only shifts whole lines. A block comment behind `delegate` and an end-of-line comment behind `delegate` were traced and are kept; comment tests exist.
  - A directive cannot stand in a hole.
- **FC-006, FC-023 to FC-026, FC-029, FC-036.** No syntax rewrite, rename, or key/value layout. Shifting hole lines changes code whitespace, not literal text. Several anonymous methods in one hole are covered (FC-026).
- **FC-007, FC-008, FC-009, FC-018, FC-030.**
  - No code fix changed.
  - RH5417 withholds its fix in a hole.
  - The RH5008 fix inside an anonymous method in an aligned hole and in a moved hole only inserts the blank line and keeps every column. It converges in one action under LF and CRLF.
- **FC-010, FC-011, FC-012.**
  - The exception list is empty, and the phase check runs in every `AssertRuleResult`.
  - Every scratch corpus (lambda gaps, 15 anonymous-method shapes, tab variants, unplaced anchors) is a fixed point after one pass under LF and CRLF.
  - The issue's first input is stable in pass 1 of `trace.sh`.
  - The alignment sweep is bounded (`MaxAlignmentPasses`).
- **FC-013.** No diagnostic on formatted output from RH5002, RH5008, RH5103, RH5204, RH5206, RH5301, RH5308, or RH5417, for anonymous methods in an argument, a second argument, a conditional branch, an object initializer, a switch arm, a chain, a format clause, a field initializer, and nested positions. The kept blank lines in front of lambdas are what `main` already gives the `(1)` control, and no rule reports blank lines between arguments.
- **FC-014.** No new rule.
- **FC-015.** See *Policy ownership*.
- **FC-016.** Enumerated in finding #1.
- **FC-017.** See *Assertion adequacy*.
- **FC-022.** `$$` literals are covered. Nested regular, verbatim, and single-line raw strings fall under finding #1.
- **FC-027.** These enclosures hold:
  - argument;
  - wrapped second argument;
  - conditional;
  - object initializer;
  - switch-arm value on its own (unplaced) line, aligned and moved, with a mis-indented anchor;
  - chain;
  - format clause;
  - hole at the line start;
  - field initializer;
  - nested anonymous method;
  - anonymous method in a nested single-line raw string inside an anonymous-method block (scope depth, by design);
  - anonymous method in a lambda block around the literal.

  The nested-string members fail (finding #1).
- **FC-028.** The node-level path inside a hole is unspecified (FMT-PIPE-023). The one reached caller, the RH5008 fix on a statement inside the block, keeps the aligned columns.
- **FC-031.** The class summary of `LambdaAlignmentContributor`, the new XML docs, the `VisitParameterList` inline comment, and the comment at `ListElementInteriorJoinRewriter` line 163 describe the current code. There is no `TODO` and no tracker number in the changed sources.
- **FC-032.** The analyzer project ran after the only compiled production commit (07: 6397 passed at 4d65391). Later commits change tests and docs only.
- **FC-033.** Every helper runs LF and CRLF.
- **FC-034.** The issue's second input is the entry-1 test verbatim. The issue's first input (`var total = …`) was traced separately: no phase changes it.
- **FC-035.** 06 shows the 28 tests failing before the implementation. Expected values come from spec examples.
- **FC-037.** Not applicable.
- **Tab matrix.** The preparation phase now replaces whitespace tabs before every phase. A tab in front of a wrapped anonymous-method argument and of an unplaced-line lambda gave the same columns as spaces.

## Architecture findings

**Components:**
- `LayoutComputer.RegisterRawStringHoleLines` together with `LayoutModel.AcceptsWrite`/`ReleaseUnplacedRawStringHoleLines`;
- the new `LambdaAlignmentContributor.IsAlignedInRawStringHole`.

**Shared decision.** Whether a line inside an interpolated string belongs to a hole of a multi-line raw literal, and so cannot take the scope-depth column and must be placed by a construct inside that hole.

The registration takes the innermost *multi-line raw* hole. The contributor re-derives the hole from syntax and takes the innermost hole of *any* interpolated string, so it predicts the registration's decision and gets it wrong for the members of finding #1. The input is finding #1's counterexample.

A fix that reads the decision from one place keeps the two from drifting. For example, the guard could walk to the first multi-line raw hole exactly as the registration does, or ask the model whether the brace line is a registered hole line. That fix stays inside the components the run changes and their counterpart. The lead decides under `squad/README.md`, *Architecture findings*.

## Hints

1. **Pre-existing, not caused by the run.** A switch-expression arm behind a multi-line arm value stays on the closing brace's line: `}, _ => null`. This happens outside holes too, and for block lambdas as well as anonymous methods. Input: `M(v switch { 1 => delegate { return 1; }, _ => null })`. It is a follow-up candidate if no FMT-BREAK requirement already decides arms.
2. **Pre-existing; same class as follow-up candidate 3.** A value continued on the line after the `(` of a parenthesized expression (`var g = (⏎⏎(int x) => x);`), or after a switch-expression arm's `=>`, starts in the statement's column (8). The `(1)` control does the same. Follow-up 3 can be widened to these gaps.
3. **Uncertain, pre-existing, not verified for block lambdas.** A tab in a raw literal's content prefix, which is literal text and so not replaced, counts as one column when a hole line aligns under a token behind it. An example is `\tT {M(delegate …` with closing `\t"""`. The block lands under `delegate` by character count, not visually.
