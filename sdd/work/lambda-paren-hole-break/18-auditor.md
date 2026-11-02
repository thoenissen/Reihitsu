Role: auditor
Commit: 21ecce6
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Repair round 1, delta audit against e835366 (the previous audit, `12-auditor.md`). Fresh agent.
- Head 21ecce6dd301c8365b5d8db7ffd4e49664f2e500 is checked out. `origin/main` is 932761907ae10ef593a4616d09bc5ba141c8bc84, as the prompt says, and it is an ancestor of head.
- `git diff e835366 21ecce6` is what handovers 13–17 describe:
  - production: `LambdaAlignmentContributor.IsAlignedInRawStringHole` now reads the hole from the model; new `LayoutModel.TryGetRawStringHole`;
  - 21 new tests in `AnonymousMethodHoleAlignmentTests`;
  - FMT-INDENT-031 (definition of *the hole*, narrowed exception and scope-depth sentences, three examples);
  - change doc entry 8 and the *Unchanged* bullets;
  - `sdd/work/`.
- The repair stays within FMT-INDENT-031 and the components the run already changes. Scope did not grow. `scripts/lane-check.sh behavior-change` confirms the lane.
- Evidence comes from scratch fixtures outside the repository. `scripts/format.sh --force` was run twice per corpus, at head, at e835366 and at `main` (git worktrees in the session scratchpad); one corpus also under CRLF.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member listed in finding #1 is aligned under its anchor and stable, in both literal states: regular, verbatim, and single-line raw. Each has its own test, and my fixtures add members: two anonymous methods on one line, a nested string in an expression lambda's body, a multi-line raw literal in a regular string, a hole that starts its line, a switch arm, an object initializer, `static async`, and comments behind `delegate`. A sweep over a nested regular string covered literal content in columns 0, 4, and 17 with 0–15 characters of prefix: 48 cells, all aligned and stable. The spec's single-line sentence and the change doc's *Unchanged* now name only strings that no multi-line raw hole encloses. |
| Architecture (12) | closed | Lead decision 11 is carried out. `LayoutComputer.RegisterRawStringHoleLines` is now the only statement of which multi-line raw hole owns a line. The guard reads it through `LayoutModel.TryGetRawStringHole`; see *Policy ownership*. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` (`ShiftRange`, clamp `newColumn < 0`); `IndentationRewriter` (writes whitespace only for `layout.Column > 0`); reached through `LambdaAlignmentContributor` (FMT-INDENT-031) | FC-010, FC-016, FC-021, FC-022, FC-027 | – | same requirement (pre-existing mechanism the change newly exposes) | See *Finding 2* below. | See *Finding 2* below. |

### Finding 2

**What goes wrong.** An anonymous method that FMT-INDENT-031 aligns can have its anchor on a line that does not move with the literal while its block lines do. Two kinds of line do not move: a line that starts inside text begun on an earlier line, such as the continuation of a verbatim string's text, and the line where a multi-line `/* */` comment ends.

When the anchor's column is smaller than the literal's rightward shift, the block's column before the move would be negative. `ShiftRange` clamps that to 0, and the move then adds the shift. On pass 1, the braces and the body all land in the shift's column (13), and the body is not one level deeper. On pass 2 the literal no longer moves, so the block aligns under the anchor. The output is not idempotent, and pass 1 breaks FMT-INDENT-031: "the column of the anchor is measured after the literal moved".

Counterexample (LF and CRLF alike):

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var h = $"""
    U {$@"first
s{M(delegate { return 1; })} end"}
    """;
        var c = $"""
    K {M(/* one
two */ delegate { return 1; })}
    """;

        return 0;
    }
}
```

Pass 1 at 21ecce6:

```csharp
        var h = $"""
                 U {$@"first
s{M(delegate
             {
             return 1;
             })} end"}
                 """;
        var c = $"""
                 K {M(/* one
two */delegate
             {
             return 1;
             })}
                 """;
```

Pass 2 moves `{`, `return 1;` and `})}…` to columns 4/8/4 for `h` and 6/10/6 for `c`, which is the FMT-INDENT-031 layout under `delegate`.

On `main`, both anonymous methods are stable but unaligned (column 13), which is the issue's defect. So the run brought them into the lambda alignment and with it into this mechanism:
- `c` (anonymous method directly in the hole) fails since the original change: e835366 is already unstable. The first audit did not enumerate this member.
- `h` (anonymous method in a verbatim string nested in the hole) fails since the repair (89cd33d). e835366 was stable and unaligned here.

Failing members:

| Anchor line that does not move | Anchor column against rightward shift 13 | Anonymous method (head) | Block lambda |
|---|---|---|---|
| end line of a `/* */` comment, directly in the hole | 6 | fails | fails on `main` already (`M(/* one⏎two */ () => { … })`) |
| continuation of `$@"…"` text, nested in the hole | 4, 10, 12 | fails | fails on `main` already (`second {M(() => { … })`, anchor 10) |
| continuation of `@$"…"` text, nested in the hole | 4 | fails | not run; same mechanism |

Members that hold:
- the anchor column equals the shift (13): this is the boundary, and the column one below it fails;
- the anchor column is larger than the shift (22);
- the literal is already aligned (no shift);
- the literal moves left (content in column 25);
- the anchor stands on the closing line of a nested multi-line raw literal (`""" + M(delegate …`), which moves;
- a non-interpolated verbatim literal or raw literal as an earlier argument: the line-break phase puts `delegate` on its own line, which moves.

**Reachability.** Whole-document formatting (CLI, `FormatDocumentAsync`) of C# 11+ source with such a comment or verbatim string in a hole of a multi-line raw literal that the formatter moves right.

**Suggested change.**
- Compose the floor with the move: a placed hole line may hold a pre-move column below 0 as long as that column plus the line's shift is at least 0. Alternatively, write the final column for such lines directly. See *Architecture findings*.
- Add tests for both anonymous-method members, with the literal moved and aligned. Pin the boundary with an anchor column equal to the shift next to one column less (FC-003).
- The block-lambda members are pre-existing. One fix of the mechanism closes them too, and their pass-1 output becomes the pass-2 output that FMT-INDENT-028 already demands. So no output is undecided.
- A narrower option for anonymous methods only would leave the lambda members failing. If the lead chooses it, the lambda members are a follow-up candidate.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| An anonymous method is aligned in a hole (FMT-INDENT-031) | The hole registered for the opening brace's line, then the ancestors of the anonymous method up to that hole, stopping at the first `BlockSyntax` | `LayoutComputer.RegisterRawStringHoleLines` (innermost multi-line raw hole around the first token of a line); `LayoutModel.AcceptsWrite` (contributor span inside the registered hole) | Same source. The brace is a descendant of the anonymous method, so a hole registered for a line the brace starts is always an ancestor of it. The `Span.Contains` check only matters when the brace does not start its line, which FMT-BREAK-002 makes unreachable (12). Reference equality `ancestor == hole` holds because the registration and the contributors walk the same root, and the model holds the hole strongly. | ok |
| Block exception now walks through nested strings | `BlockSyntax` between the anonymous method and the registered hole | Spec exception paragraph ("between the anonymous method and the hole, whether or not a nested … string lies between") | agree; tests with a regular and a single-line raw string inside a lambda body, and inside an outer anonymous method's body | ok |
| A nested multi-line raw literal ends the hole | Registration overwrites outer holes with inner ones (pre-order walk) | Spec "A nested multi-line interpolated raw string literal does end it" | agree (`rawInRegular` fixture: multi-line raw in a regular string in the hole, aligned and stable) | ok |
| Block column when the anchor's line does not move with the literal | `ShiftRange` clamps the pre-move column at 0; `IndentationRewriter` writes no whitespace for a column ≤ 0; the move then adds the shift | FMT-INDENT-031 / FMT-INDENT-030: the anchor is measured after the move | Disagree when anchor column < rightward shift | finding #2 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which multi-line raw hole owns a line | `rg -n "InterpolationSyntax\|InterpolatedMultiLineRawStringStartToken\|IsMultiLineRawStringLiteral\(\|TryGetRawStringHole" src/Reihitsu.Formatter`. Statement: `LayoutComputer.RegisterRawStringHoleLines` (line 351). Storage and reads: `LayoutModel.RegisterRawStringHoleLine`, `TryGetRawStringHole`, `AcceptsWrite`, `IsUnplacedRawStringHoleLine`. Reader: `LambdaAlignmentContributor.IsAlignedInRawStringHole`. `RawStringLineShifts.IsMultiLineRawStringLiteral` / `IsNonRawStringText` / `IsHoleOpeningLine` decide the literal moves, a different decision. `ListElementInteriorUtilities`, `BraceBlankLinePolicy`, and `SingleSpaceSpacingRule` only test the node kind. | one owner; closes the architecture finding of 12 |
| Floor of a placed hole line's column | `LayoutModel.ShiftRange` (clamps the pre-move column at 0), `LayoutModel.Set`/`SetUnlessPlaced` (subtract the shift, no clamp), `IndentationRewriter` (writes whitespace only for `Column > 0`), `RawStringLineShifts.CollectEdits` (`Math.Max(0, indentation + shift)`, the final column) | Three places floor the column, two of them before the move. This is the cause of finding #2; see *Architecture findings*. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 13 new `…NestedInHoleOf{Moved,Aligned}Literal…` tests and the two spec-example tests | FMT-INDENT-031 for nested regular, verbatim, and single-line raw strings, `static`/`async`, and two levels of nesting, in both literal states | Block in column 13 or 0, or the body not one level deeper | `AssertRuleResult` (exact output, LF and CRLF, second pass, phase check, tree round-trip) | adequate; 14 records them failing before 89cd33d |
| `BlockLambdaInRegularStringNestedInHoleOf{Moved,Aligned}Literal…` | Counterpart parity (FMT-INDENT-028) | Lambda not under `(` | `AssertRuleResult` | adequate |
| Five scope-depth tests (single-line raw, regular, and verbatim strings that no multi-line hole encloses; nested string inside a lambda body; nested string inside an outer anonymous method body) | The exception and the "as before" sentence | Any shift | `AssertRuleResult` with input = expected | adequate |
| Missing | FMT-INDENT-031 with the anchor on a line that does not move with the literal | Finding #2 | — | missing (finding #2) |

## Failure classes dismissed

- **FC-001, FC-002, FC-015.** See *Guard scope* and *Policy ownership*. The guard now reads the registration.
- **FC-003.** The new tests cover both sides of the changed boundary: a nested string inside and outside a multi-line hole, inside and outside a block body, a moved and an aligned literal, and the `static`/`async` anchors. The anchor-column boundary of finding #2 is not covered (finding #2).
- **FC-004, FC-005, FC-019, FC-020, FC-021.** The repair only changes which anonymous methods are shifted. Comments behind `delegate`, a comment inside the body, and an end-of-line comment behind `delegate` in a nested string are kept and stable. A `/* */` comment ending in front of `delegate` fails on its column (finding #2), not on its text. A directive cannot stand in a hole.
- **FC-006, FC-023 to FC-026, FC-029, FC-036.** No syntax rewrite. Shifting hole lines leaves literal text alone: the verbatim continuation lines kept column 0 in every pass. Two anonymous methods on one line in one nested string are aligned each under its own `delegate` (FC-026).
- **FC-007, FC-008, FC-009, FC-018, FC-030.** No code fix changed. The RH5008 evidence of 12 is byte-identical for the members it ran on, and the repair only adds members that the code fixes do not format separately.
- **FC-011, FC-012.** No loop changed. The phase check runs in every new test and finds no reversal.
- **FC-013, FC-014.** No analyzer change. The new aligned shapes are the block-lambda shapes the analyzer sweep of 12 already covered.
- **FC-017.** See *Assertion adequacy*.
- **FC-028.** The node-level path is unchanged (12).
- **FC-031.** The class and method summaries of `LambdaAlignmentContributor` and the new `TryGetRawStringHole` summary describe the new code. No `TODO`, no tracker number.
- **FC-032.** 15 ran the formatter and analyzer projects after 89cd33d, the only compiled commit of the repair.
- **FC-033.** Every new test runs under LF and CRLF. Finding #2 was reproduced under both.
- **FC-034, FC-035.** The expected values in 14 come from the spec examples and the anchor rule, and 14 records them failing before the implementation.
- **FC-037.** Not applicable.
- **Tab matrix.** Tabs in whitespace are replaced first. The repair adds no measurement of whitespace. The literal-content tab of hint 3 in 12 stays a follow-up candidate (5).

## Architecture findings

**Components.** Two places floor a hole line's column before the move (`LayoutModel.ShiftRange` and `IndentationRewriter`), and `RawStringLineShifts.CollectEdits` then moves the line.

**Shared decision.** The final column of a placed hole line of a multi-line raw literal.

**Structural cause.** The layout predicts the later move: `Set` and `SetUnlessPlaced` subtract the line's shift, and `ShiftRange` works in pre-move columns. That representation cannot hold the prediction when the target lies left of the shift, which happens exactly when the target's anchor stands on a line the move does not carry. `ShiftRange` and the rewriter clamp at 0 before the move, while `CollectEdits` clamps the final column. The three statements of the floor disagree. The input is finding #2's counterexample.

The structure is pre-existing; block lambdas fail on `main`. The run newly exposes it for anonymous methods. `LayoutModel` is a component the run changes, and the rewriter and `RawStringLineShifts` are its counterparts. The lead decides under `squad/README.md`, *Architecture findings*.

## Hints

1. The previous hints of 12 are unchanged; they are follow-up candidates 3, 4, and 5 in `00-lead.md`. Hint 1 (`}), _ => 0` behind a multi-line switch arm) also shows inside a nested string in a hole (`switchArm` fixture), with the same cause.
2. If the lead fixes finding #2 for anonymous methods only, the block-lambda members are a pre-existing follow-up with the structural cause named above.
