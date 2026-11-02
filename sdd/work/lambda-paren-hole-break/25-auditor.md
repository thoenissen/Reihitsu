Role: auditor
Commit: 4a31be5
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It pointed to the `///` path that 20-implementer.md left without a test, and that path is enumerated below.

## Scope

- Repair round 2, delta audit against 21ecce6 (`18-auditor.md`). Fresh agent.
- State:
  - Head 4a31be5ac34cc31f0a5f98500e4267fa102b79ff is checked out.
  - `git ls-remote origin refs/heads/main` gives 932761907ae10ef593a4616d09bc5ba141c8bc84, as the prompt says, and that commit is an ancestor of head.
  - `scripts/lane-check.sh behavior-change` confirms the lane.
- `git diff 21ecce6 4a31be5` matches what handovers 19–24 describe:
  - production changes in `LayoutModel` (`FloorColumn`, `GetColumnAfterRawStringShift`, `ToLayoutBeforeRawStringShift`, and the new `ShiftRange`), in `IndentationRewriter` (`placedColumns`, plus the `///` continuation record), and in `RawStringLineShifts.MoveLines`/`CollectEdits` (`getColumnAfterMove`);
  - 16 tests in `HoleAnchorOnUnmovedLineAlignmentTests`;
  - change doc entry 9 and the *Unchanged* bullet;
  - `sdd/work/`.
- The repair stays within `LayoutModel`, its counterparts named by decision 13, and the change doc. Scope did not grow. No spec requirement changed.
- Evidence:
  - I built the CLI at head, at 21ecce6, and at `main`, and ran it twice per fixture. 21ecce6 and `main` were built from git worktrees in the session scratchpad, which are removed now.
  - Corpora:
    - 198 fixtures: three unmoved-anchor forms × 22 hole bodies × three literal states (moved right 13, aligned, moved left 8);
    - a sweep of anchor columns 3–22 (240 fixtures);
    - a construct sweep at columns 7/8/9 (96 fixtures, CRLF included);
    - a moved-line anchor sweep in nested contexts (168 fixtures);
    - a CRLF `///` fixture.
  - One mutation run, in a scratch worktree.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 2 | closed | Every member that #2 listed now lands under its anchor on pass 1 and is stable: the comment end line directly in the hole (column 6), `$@` continuation (4, 10, 12), and `@$` continuation (4). This holds for anonymous methods and block lambdas alike, and the new tests pin these members. The column sweep with the literal moved right holds for anchor columns 3–7 and 9–22 in both forms and for both constructs. The aligned and left-moving literals hold at every column except 8. Column 8 is finding #3, a member that #2 did not name. |
| Architecture (18) | closed | Lead decision 13 is carried out. `LayoutModel.FloorColumn` is the one floor of a line's final column, and it applies after the move; see *Policy ownership*. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` (`if (delta == 0) return;` in `Contribute`); `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` (`ShiftRange` returns on `delta == 0` before `RecordPlacement`; `ReleaseUnplacedRawStringHoleLines`) | FC-001, FC-003, FC-010, FC-016 | #2 | same requirement (FMT-INDENT-031; pre-existing mechanism the run newly depends on) | See *Finding 3*. | See *Finding 3*. |
| 4 | medium | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` (the loop that records `placedColumns` for the later lines of a `///` comment) | FC-017, FC-021 | – | PR-introduced | The branch is reachable and load-bearing, but no test covers it. See *Finding 4*. | Add a test with a multi-line `///` comment inside the block of an anonymous method and of a block lambda whose anchor stands on an unmoved hole line left of the shift, the literal moved right. One more case puts the `///` comment between `delegate` and `{`. Unequal input indentation of the `///` lines makes the assertion stronger. |
| 5 | low | `sdd/changes/2026-11/lambda-paren-hole-break.md`, entry 9 and the intro to *Visible output changes* | FC-031 | – | PR-introduced | See *Finding 5*. | Widen entry 9 to every alignment anchored on such a line, or add one sentence naming the others. The claim about the statement's column depends on how #3 is decided. |

### Finding 3

**What goes wrong.** Take a block lambda or an anonymous method whose anchor stands on a hole line that does not move, and whose anchor column equals the column that block indentation gives the opening brace. That column is the column of the enclosing statement: 8 in a method body, 12 one block deeper.

`LambdaAlignmentContributor` computes `delta == 0` and writes nothing. `ShiftRange` would also return before `RecordPlacement`. So no construct inside the hole places the block's lines. `ReleaseUnplacedRawStringHoleLines` then drops their layouts, and the lines keep the whitespace they have in the input and only move with the literal. The braces do not land under the anchor, and the body is not one level deeper. This is the layout of `main` that the issue reports for anonymous methods, and pass 2 keeps it.

Counterexample (LF and CRLF alike), with the anchor `delegate` in column 8:

```csharp
class C
{
    int F()
    {
        var h = $"""
    K {M(/* one
aaaaa */delegate { return 1; })}
    """;

        return 0;
    }
}
```

Head, pass 1 and pass 2 (stable):

```csharp
        var h = $"""
                 K {M(/* one
aaaaa */delegate
             {
             return 1;
             })}
                 """;
```

FMT-INDENT-031 demands `{` and `})}` in column 8 and `return 1;` in column 12.

With the literal already aligned, or moving left, the same input gives all three lines in column 0. For example, the verbatim form `sssss{M(() => { return 1; })} end"}` in an aligned literal gives `{`, `return 1;`, and `})} end"}` in column 0.

Enumeration. The axes:
- the construct: anonymous method, `async` anonymous method, parenthesized lambda, `async` lambda, simple lambda;
- the unmoved line: `/* */` end line, `/** */` end line, `$@` continuation, `@$` continuation;
- the literal: moved right 13, aligned, moved left 8;
- the anchor column: 3–22;
- the statement column: 8 or 12;
- the line ending: LF or CRLF.

Failing members (head, all stable unless noted):

| Anchor column | Construct | Unmoved line | Literal | Head output |
|---|---|---|---|---|
| = statement column (8; 12 when the statement stands one block deeper) | all five constructs | all four kinds | moved right | block lines in the shift's column 13, body not deeper |
| = statement column | all five | all four | aligned, moved left | block lines in column 0 |
| = statement column, with a construct inside the block that aligns to a token of the block (chain `q⏎.A()⏎.B()`, argument list, nested multi-line raw literal, nested block lambda) | anonymous method and block lambda | `/* */` end line | moved left | not idempotent: `cm_anonchain_lf`, `cm_innerraw_lf`, `cm_lambdaargs_lf`, `cm_nestedlambda_lf` move `.B()`, `2);`, and the inner literal's lines on pass 2 |

Members that hold:
- anchor columns 3–7 and 9–22, in every literal state, for both constructs and both forms;
- input whose block lines already stand under the anchor (`preformatted08`: stays as written);
- anchors on lines that move, also nested in object initializers, collection initializers, argument lists, and switch arms (168 fixtures, pads 0–13: all aligned and stable). Such an anchor cannot reach the statement's column, because a moved hole line stands right of the literal's opening quotes;
- the same block outside a hole (plain code): block indentation places it.

**Origin.**
- Block lambdas: on `main` they give the same output for every member (pre-existing).
- Anonymous methods: on `main` they are unaligned at every column, which is the issue's defect. Since the run, they align at every column except this one.
- At 21ecce6 this member already failed in the same way; #2's enumeration did not name it. The members #2 listed as holding (aligned literal, literal moving left) fail at this column.

**Repeats #2.** It is a member of #2's input space (an anchor on a hole line that does not move, left of the shift) that #2's enumeration did not name. The mechanism differs, though: here the placement of hole lines fails, not the floor of their column. The two components that decide whether a construct inside the hole placed the block's lines disagree. See *Architecture findings*.

**Reachability.** Whole-document formatting (CLI, `FormatDocumentAsync`) of C# 11+ source with a multi-line `/* */` or `/** */` comment, or a `$@`/`@$` string, in a hole of a multi-line raw literal. The anchor must stand in the statement's column on the line where that text ends. These are the same callers as for #2.

**Suggested change.**
- Placement of the block's hole lines should not depend on a non-zero delta. For example, the contributor could record the placement of the lines from `{` to `}` even when the delta is 0, or `ShiftRange` could record placement before its `delta == 0` return.
- Pin the boundary with tests: the anchor in the statement's column next to one column on either side. Cover an anonymous method and a block lambda, a moved and an aligned literal, and one test with the statement one block deeper.
- Under `squad/README.md`, *Architecture findings*, the lead decides whether this is changed structurally.

### Finding 4

The repair added the loop in `ApplyIndentation` that records the first line's column before the move for every later line of a placed `///` comment. The reachable input: a multi-line `///` comment inside the block of an anonymous method or block lambda whose anchor stands on an unmoved hole line left of the shift. `CSharpParseOptions.Default` parses such a comment as `SingleLineDocumentationCommentTrivia` in the CLI.

Head output for that input is correct and stable, in LF and in CRLF, also with unequal input indentation of the `///` lines (`vb_doccomment_mv`, `vb0_doccomment_mv`, `vb_docbrace_mv`, `crlfdoc`).

Mutation: I disabled the loop in a scratch worktree at head. The fixture then gives the later `///` lines in column 13 instead of 9, while the first line stays in 9. The formatter test project still passes against that mutant: `scripts/test.sh --no-install --project formatter` reports Failed 0, Passed 4179. Nothing pins the branch.

### Finding 5

Change doc entry 9 has two inaccuracies:
- **Other alignments changed too.** Entry 9 names only blocks. But the repair also changes the first pass of every other alignment whose anchor stands on an unmoved hole line left of the shift: wrapped arguments, object-initializer members, collection-expression elements, member-access chains, switch arms, and ternaries. Before, those lines landed in the shift's column (13), and on `main` that output is not idempotent. Now head's pass 1 equals `main`'s pass 2: 28 fixtures, such as `cm_args_mv` (`2,`/`3))}` from column 13 to 8). FMT-INDENT-030 demands this output, so nothing visible is undecided. Decision 14's reasoning covers it, but the change doc does not list it.
- **The statement's column fails.** Entry 9's sentence "They now land under the anchor on the first pass" does not hold when the anchor stands in the statement's column (#3).

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Floor of a line's final column | `FloorColumn(columnBeforeShift + GetRawStringShift(line))`, in `GetColumnAfterRawStringShift` and `ToLayoutBeforeRawStringShift` | `RawStringLineShifts.CollectEdits` now asks `getColumnAfterMove` and no longer floors anything itself; `IndentationRewriter.WriteLineStart` writes whitespace only for a positive column, but records the layout's column for the move | Agree. A placed line moves from its recorded layout column, not from the whitespace written, so the rewriter's `> 0` is not a floor of the result. A line whose shift is 0 can only store a column ≥ 0 (`FloorColumn(after) - 0`), so dropping `shift == 0` from `CollectEdits`' skip changes no line that does not move: placed equals written there, and the `newSpaces == indentation` skip catches it. | ok |
| Which lines move from the recorded column | `placedColumns`: lines that `WriteLineStart` placed, plus the later lines of a placed `///` comment (`Span.End - 1` of the original trivia) | `RawStringLineShifts` constructor (`StartsInsideNestedText`: lines that start in nested non-raw text or in a `/* */`/`/** */` comment get shift 0) | Agree. The later lines of a `///` comment are moving lines and receive the first line's column. Lines inside a nested `/* */` comment are never placed and keep shift 0 (`vb_blockcomment_mv`: `   b */` stays in column 3). | ok; untested branch, finding #4 |
| `ShiftRange` stores `FloorColumn(old + delta + shift) - shift` | the hole lines with a layout in the range, accepted by `AcceptsWrite` | `TryGetLayoutColumn` reads `FloorColumn(layout + shift)`, and the contributor's delta is computed from that | Agree: a stored column satisfies `stored + shift ≥ 0` at store time, so the floored read equals the unfloored sum, and the delta adds to the same base. | ok |
| A construct inside the hole placed the block's lines | `LambdaAlignmentContributor` returns on `delta == 0`; `ShiftRange` returns on `delta == 0`; placement is recorded only by `RecordPlacement` inside a write | `ReleaseUnplacedRawStringHoleLines` drops every unrecorded hole line's layout | Disagree when the anchor's column equals the block-indentation column of `{` | finding #3 |
| `IsHoleOpeningLine` precondition ("only lines that move reach it") | Lines past the `newSpaces == indentation` skip | `getColumnAfterMove` | Holds: a line that does not move gets `newSpaces == indentation`, as argued in the first row | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Floor of a line's final column | `rg -n "Math\.Max\(0\|FloorColumn\|Column > 0\|< 0\)" src/Reihitsu.Formatter/Pipeline/Indentation src/Reihitsu.Core/DocumentationCommentUtilities.cs`. Owner: `LayoutModel.FloorColumn` (called from `GetColumnAfterRawStringShift` and `ToLayoutBeforeRawStringShift`). Not floors of the result: `IndentationRewriter.WriteLineStart` (`layout.Column > 0`, writes only), and `DocumentationCommentUtilities.AlignContinuationExteriorMarkers` (`Math.Max(0, column)`, writes the markers before the move; the rewriter then records the first line's column for the later lines, so the move overrides it). `ShiftContinuationExteriorMarkers` has no formatter caller. | one owner; the Core write limit is compensated in the rewriter, and that compensation has no test (#4) |
| Line move | `rg -n "MoveLines\|GetRawStringShift\|GetColumnAfterRawStringShift" src --glob '!**/obj/**'`: `GetRawStringShift` is the one statement of a line's shift. `GetColumnAfterRawStringShift` is the one conversion to the final column; it is used by `TryGetPlacedColumn`, `TryGetLayoutColumn`, and the `MoveLines` delegate in `IndentationRewriter.Apply`. `LayoutComputer.GetAdjustedColumn` adds the shift unfloored, which equals the floored value for every stored column (see *Guard scope*). | one owner |
| Placement of a hole line by a construct inside the hole | `LayoutModel.RecordPlacement` (from `Set`, `SetUnlessPlaced`, and `ShiftRange`), `ReleaseUnplacedRawStringHoleLines`; the `delta == 0` returns in `LambdaAlignmentContributor.Contribute` and `LayoutModel.ShiftRange` | Placement is inferred from a write. Two skips of the write decide it implicitly: finding #3, and see *Architecture findings*. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 12 `…OneColumnLeftOfShift…`, `…InShiftColumn…`, `…InAlignedLiteral…` tests (anonymous method and block lambda; `$@` and comment end line) | FMT-INDENT-031/028 with the anchor measured after the move (FMT-INDENT-030), on both sides of the old boundary | Block in column 13 or not one level deeper; any second-pass change | `AssertRuleResult` (exact output, LF and CRLF, second pass, phase check, tree round-trip) | adequate; 19 records 4 of them failing before 1589b0a |
| `AnonymousMethodOnInterpolatedVerbatimContinuationLineLeftOfShift…`, `AnonymousMethodBehindSpacedCommentEndLineLeftOfShift…` | #2's `@$` member and its exact counterexample | Same | `AssertRuleResult` | adequate |
| `…RightOfShift…`, `…OfLiteralMovedLeft…` | Holding members stay | Any shift | `AssertRuleResult` | adequate |
| Missing | Anchor in the block-indentation column of `{` (the statement's column) | Finding #3 | — | missing (#3) |
| Missing | Later `///` lines of a comment on a line whose column is below 0 before the move | Finding #4 (mutant passes 4179/4179) | — | missing (#4) |

Visible output (check 10): every new expected value is demanded by unchanged FMT-INDENT-028/030/031 and FMT-RAW-007 (lead decision 14), and the anonymous-method layout is covered by maintainer answers 1, 4, and 6. No expected value is undecided.

## Failure classes dismissed

- **FC-002, FC-015.** See *Guard scope* and *Policy ownership*. The floor now has one owner. The placement decision is finding #3.
- **FC-004, FC-005, FC-019, FC-020, FC-021.**
  - Comments are kept and moved with their lines: `//`, `/* */`, and `///` inside the block, `///` and `//` between `delegate` and `{`, and a `/* */` comment whose later line does not move (`vb_blockcomment_mv`).
  - The `///` continuation path is correct but untested (#4).
  - A directive cannot stand in a hole.
- **FC-006, FC-023 to FC-026, FC-029, FC-036.** No syntax rewrite. Literal text is untouched, and the nested verbatim continuation lines keep column 0. A nested multi-line raw literal inside a block whose line is below 0 before the move gets its own distance from that column and lands under its quotes (`vb_innerraw_mv`).
- **FC-007, FC-008, FC-009, FC-018, FC-030.** No code fix changed.
- **FC-028.** The node-level root shift (`ShiftRange(0, ∞, …, "RootFirstTokenColumn")`) now floors after the move. Its result differs from 21ecce6 only for a hole line whose column before the move falls below 0 under the shift, where 21ecce6 put it in the literal's shift column, the #2 defect. The analyzer project (6397 tests, in 20) passed on the repair commit.
- **FC-011, FC-012.** No loop changed. The phase check runs in every new test.
- **FC-013, FC-014.** No analyzer change.
- **FC-017.** See *Assertion adequacy*.
- **FC-031.**
  - The summaries of `LayoutModel` (class, `Set`, `ShiftRange`, `TryGetLayout`, `TryGetPlacedColumnBeforeRawStringShift`, and the new members), of `IndentationRewriter.Apply`/`WriteLineStart`, and of `RawStringLineShifts.MoveLines`/`CollectEdits` describe the new code.
  - No `TODO`, no tracker number.
  - The change doc gap is finding #5.
- **FC-032.** 20 ran the formatter, analyzer, and CLI projects on 1589b0a, the only compiled commit of the repair. 21 confirms a warning-free Release build.
- **FC-033.** CRLF: the new tests run under both line endings. My #3 and `///` fixtures ran under CRLF too, with the same results.
- **FC-034, FC-035.** 19 derived the expected values from the requirements and recorded the failures before the implementation.
- **FC-037.** Not applicable.
- **Tab matrix.** No new measurement of whitespace. Follow-up candidate 5 is unchanged.

## Architecture findings

- **Components.** `LambdaAlignmentContributor` and `LayoutModel` (`ShiftRange`, `RecordPlacement`, `ReleaseUnplacedRawStringHoleLines`).
- **Shared decision.** Whether a construct inside the hole placed the lines of a block in an interpolation hole of a multi-line raw literal.
- **Structural cause.** The model infers placement from a write, and `RecordPlacement` runs only inside `Set`, `SetUnlessPlaced`, and `ShiftRange`. The contributor decides that the block already stands where it belongs (`delta == 0`) and writes nothing; `ShiftRange` makes the same skip. So the contributor's decision "placed, nothing to move" reaches the model as "not placed", and the model releases the lines to their input whitespace.
- **Input.** Finding #3's counterexample.
- **Scope.** The structure is pre-existing (block lambdas fail on `main`), and the run newly depends on it for FMT-INDENT-031. Both components are ones the run changes. The finding repeats the class of #2 by input space. The lead decides under `squad/README.md`, *Architecture findings*.

## Hints

1. The previous hints are unchanged; they are follow-up candidates 3, 4, and 5 in `00-lead.md`.
2. In `cm_*_lf` fixtures with the anchor in column 8, the instability comes from finding #3's mechanism. A fix of #3 should be checked against them: a construct inside the block that aligns to a token of the unplaced block, with the literal moving left.
