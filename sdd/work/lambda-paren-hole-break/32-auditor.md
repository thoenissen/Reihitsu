Role: auditor
Commit: 2a265da
Verdict: pass, text cleanup

The prompt named the inputs and the previous audit. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Delta audit of the structural change for lead decisions 15–17 and its repair, against 4a31be5 (`25-auditor.md`). Fresh agent.
- State:
  - Head 2a265daf1dda6efb8ff5f044a24c5ad9db9c665f is checked out.
  - `git ls-remote origin refs/heads/main` gives 932761907ae10ef593a4616d09bc5ba141c8bc84, as the prompt says, and that commit is an ancestor of head.
  - `scripts/lane-check.sh behavior-change` confirms the lane.
- `git diff 4a31be5 2a265da` matches handovers 26–31:
  - `LayoutModel.ShiftRange` places every accepted line also for a delta of 0, and writes a layout only for a non-zero delta;
  - `LayoutModel.CaptureColumns`/`MatchesColumns` capture through the new private `GetCapturedColumn`;
  - `LambdaAlignmentContributor.Contribute` no longer returns on a delta of 0;
  - 88 tests in the new `HoleAnchorInStatementColumnAlignmentTests`, 2 `///` tests in `HoleAnchorOnUnmovedLineAlignmentTests`, 5 unit tests in `LayoutModelTests`;
  - change-doc entry 9, its intro sentence, and *Unchanged* bullet 2;
  - `sdd/work/`.
- Scope did not grow. Both production files are named by decision 15. No spec requirement changed.
- Evidence:
  - I built the Debug CLI at head, at 4a31be5, and at `main` (worktrees in the session scratchpad, removed now) and ran every fixture twice per version.
  - Corpus 1, 4032 fixtures: 16 block constructs (anonymous method, `async`, `static`, empty; parenthesized, `async`, `static`, typed, simple, empty lambda; blocks holding a chain, wrapped arguments, a nested lambda, a comment, several statements; a block already broken in the input) × anchor behind a `/* */` end line or on a `$@` continuation line (anchor columns 5–14) or on a moved line × literal moved right 13, aligned, moved left 8 × statement in column 8 or 12 × with or without an outer chain `.A()`⏎`.B()`.
  - Corpus 2, 4860 fixtures: wrapped arguments, chain, conditional, creation initializer, collection expression, switch expression, binary, and lambdas inside arguments, initializers, and chains, × the same unmoved kinds (anchor columns 3–15) and literal states, × input indentation of the continuation lines.
  - Corpus 3: a nested multi-line raw literal with its own block lambda inside the block of a statement-column lambda (9 fixtures); anchors on moved lines in the shifted statement column (9 fixtures).
  - 826 CRLF copies of corpus 1 and 2 members (all head-vs-4a31be5 differences plus every 25th fixture).

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 3 | closed | Every member #3 listed is aligned on pass 1 and stable at head: all five constructs (plus `static` forms, empty blocks, and blocks already broken in the input) × `/* */` end line and `$@` continuation × moved right, aligned, moved left × statement column 8 and 12, with and without an outer chain (corpus 1: all 360 head-vs-4a31be5 differences are exactly these members, each with `{`/`}` in the anchor's column and the body one level deeper). The four non-idempotent `cm_*` members (chain, inner raw literal, wrapped arguments, nested lambda, literal moved left) are stable. Families A–D in `HoleAnchorInStatementColumnAlignmentTests` pin them. Head is stable on every fixture of the corpora (about 8900 LF and 826 CRLF), and CRLF output equals LF output. |
| 4 | closed | `DocumentationCommentInBlockOf{AnonymousMethod,BlockLambda}LeftOfShiftKeepsEveryLineInTheStatementColumn` reach the `///` loop in `IndentationRewriter.ApplyIndentation`; 27 records both failing with the loop disabled. |
| 5 | closed | Entry 9 now names the other FMT-INDENT-030 alignments and splits off the statement's column. Corpus 2 confirms the claim "Only blocks reach this case": arguments, chain, conditional, initializer, collection expression, switch expression, and binary with the anchor in column 8 or 12 give the same output at head and at 4a31be5, and are stable. One member of the second case is still not described; see #6. |
| Architecture (25) | closed | Placement no longer depends on whether a write changes a column: `ShiftRange` records placement before its delta check, and the contributor always calls it. The sweep's convergence check now compares the column contributors read; see *Guard scope*. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | low | `sdd/changes/2026-11/lambda-paren-hole-break.md`, entry 9, second case (*An anchor in the column that block indentation gives the opening brace*) | FC-031 | – | PR-introduced | The second case describes only an anchor that stands on a hole line that does not move. The structural change also changes the output when the anchor stands on a hole line that **moves** but is aligned into the statement's column by an alignment whose own anchor stands on an unmoved line. See *Finding 6*. | Add one sentence and, optionally, the example below to the second case: the same holds when the anchor stands on a line that an alignment places in the statement's column, such as a lambda argument `N(1,`⏎`() =>` behind a comment end line, or an initializer member `F = () =>`. |

`pass, text cleanup`: #6 is confined to the change doc. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. The output it describes is the one unchanged FMT-INDENT-028 demands, and head produces it.

### Finding 6

Input (corpus 2, `largs_cm_Al_d8_p6_i0`; the anchor `(` of `() =>` stands on a hole line that moves, aligned under `1`, which stands on the line where the comment ends):

```csharp
class C
{
    int N(params object[] f) => 0;
    void T(bool b, int i)
    {
        var a = $"""
                 K {M(/* one
aaa */N(1,
() =>
{
return 1;
}))}
                 """;
    }
}
```

`main` and 4a31be5: `() =>` in column 8, `{`/`return 1;`/`}))}` in column 0, body not deeper, stable. With the literal moved right, 4a31be5 gives those lines in column 13, stable, and `main` is not idempotent. Head, in every literal state: `{` and `}))}` in column 8, `return 1;` in column 12, stable.

Members that change this way (corpus 2, all correct and stable at head):
- a lambda argument after a wrapped first argument (`largs`), and two such lambdas (`twolam`), with `(` aligned into column 8 or 12;
- an initializer member `F = () =>` whose `(` lands in column 12 (`linit`, statement one block deeper);
- each behind a `/* */` end line and on a `$@` continuation line, in the aligned and the left-moving literal, where `main` is stable. With the literal moved right `main` is not idempotent there, which the first case already covers.

The change doc's closing sentence ("…and in neither case in the statement's column") excludes these members from "keeps its output", but no case says what they change to. `25-auditor.md` had dismissed them ("such an anchor cannot reach the statement's column, because a moved hole line stands right of the literal's opening quotes"); that holds only for a line no alignment pulls left of the quotes.

No test pins these members. They run through the same code path as families A–D (the contributor's delta of 0 and `ShiftRange`'s placement), so a mutation that brings back either `delta == 0` skip fails those families. A regression test for the `largs` input is cheap if the lead wants the member pinned; see *Hints*.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A construct inside the hole placed the lines of a block | `LambdaAlignmentContributor.Contribute`: every block lambda, and an anonymous method for which `IsAlignedInRawStringHole` holds, calls `ShiftRange(line of {, line of }, delta)` for any delta | `LayoutModel.ShiftRange`: every key in the range that `AcceptsWrite` accepts is placed (`RecordPlacement`); `ReleaseUnplacedRawStringHoleLines` drops only unplaced lines | Agree. Placement no longer depends on the delta. `AcceptsWrite` still keeps a lambda outside the hole from placing hole lines, and keeps an outer-hole lambda from placing lines of a nested multi-line raw literal's hole (corpus 3: the inner block aligns under its own anchor, stable). With `Contributor == null` (the root shift of `ShiftToRootFirstTokenColumn`, which now also reaches the loop with a delta of 0) `RecordPlacement` records nothing, and the layouts are untouched. | ok |
| A delta of 0 keeps the layout | `ShiftRange`: `continue` after `RecordPlacement` | `LayoutModelTests.ShiftRangeWithZeroDeltaDoesNotModify` (column and source label unchanged) | Agree | ok |
| The sweeps have converged | `CaptureColumns`/`MatchesColumns` over every `_layouts` entry, through `GetCapturedColumn`: the registered column of an unplaced hole line, else the layout's column | The readers: `TryGetPlacedColumnBeforeRawStringShift` (behind `GetAdjustedColumn`, `GetColumnBeforeRawStringShift`, and the raw-string shift's `getColumnBeforeRawStringShift`), and `TryGetLayoutColumn`, which only `LambdaAlignmentContributor` reads, for the `{` line | Agree for every reader. `GetCapturedColumn` mirrors `TryGetPlacedColumnBeforeRawStringShift`. The layout of an unplaced hole line, which the snapshot no longer holds, cannot change during the sweeps: every write there has a non-null `Contributor`, so a write is either rejected or places the line. A line that becomes placed while its layout changes to its registered column changes only the `TryGetLayoutColumn` read; that read is the `{` line of a lambda, and every writer of such a line (an ancestor's argument alignment, an outer lambda's range) runs before the lambda in the pre-order sweep, so the lambda reads the final value in the same sweep. A registered line without a layout that becomes placed adds an entry, and the count check catches it. | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Placement of a hole line by a construct inside the hole | `rg -n "RecordPlacement\(\|_placedRawStringHoleLines" src/Reihitsu.Formatter --glob '!**/obj/**'`: `LayoutModel.RecordPlacement`, called from `Set`, `SetUnlessPlaced`, `ShiftRange`; read by `IsUnplacedRawStringHoleLine` and `ReleaseUnplacedRawStringHoleLines`. `rg -n "delta == 0\|delta != 0" src/Reihitsu.Formatter/Pipeline/Indentation`: only `ShiftRange`'s layout-write skip remains | one owner; no write is skipped on an unchanged column any more |
| Column a contributor reads for a line | `TryGetPlacedColumnBeforeRawStringShift` and `GetCapturedColumn`, both on `IsUnplacedRawStringHoleLine` | two statements of the same choice in one class, five lines apart, both behind the one predicate; parity holds by construction. Not a finding. |
| Floor and move of a line's column | unchanged since 4a31be5 (`FloorColumn`, `GetColumnAfterRawStringShift`, `GetRawStringShift`) | one owner (as in 25) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Family A (60): every construct × every unmoved kind × every literal state, anchor in column 8 | FMT-INDENT-028/031 with the anchor measured after the move (FMT-INDENT-030, FMT-RAW-007 bullet 1) | Block lines in column 13 or 0, body not deeper | `AssertRuleResult` (exact output, LF and CRLF, second pass, phase check, tree round-trip) | adequate; 27 records all 60 failing before acb2ea0 |
| Family B (8): anchor in column 7 and 9 | Boundary on both sides (FC-003) | Any shift of the block | `AssertRuleResult` | adequate |
| Family C (4): statement one block deeper | Statement column 12 | Block in column 13 or 0 | `AssertRuleResult` | adequate |
| Family D (8): inner chain, wrapped arguments, nested raw literal, nested lambda, literal moved left | Inner alignments measured from the placed block, idempotent | Inner line moves on pass 2 | `AssertRuleResult` (second pass) | adequate |
| `…ClosingLineInInputColumn{6,8,14,16}…AlignsTheChainUnderItsAnchorLink` (8) | The outer chain reads `.A` on the placed `}` line (convergence check) | `.B()` in column 8 | `AssertRuleResult` | adequate; 29's mutation of `GetCapturedColumn` fails the four column-6/14 tests and passes the controls |
| `LayoutModelTests` `CaptureColumns…`, `MatchesColumnsAfter…` (5) | Snapshot holds the read column; placement by a zero shift or a `Set` at the stand-in column is a change; placement in the registered column and a rejected write are not | Wrong captured column or wrong match result | unit | adequate; both sides pinned |
| `DocumentationCommentInBlockOf…` (2) | Later `///` lines move from the first line's recorded column | `/// b`, `/// c` in column 13 | `AssertRuleResult` | adequate (27's mutation) |
| Missing (optional) | Anchor on a moved line that an alignment places in the statement's column | — | — | not required; same code path as families A–D (#6, *Hints*) |

Visible output (check 10): every new expected value is demanded by unchanged FMT-INDENT-028/031 (block under the anchor, body one level deeper), FMT-INDENT-030 and FMT-RAW-007 (anchor measured after the move), FMT-INDENT-008/012/029 and FMT-RAW-001 for the inner lines, and the anonymous-method layout by maintainer answers 1, 4, and 6; lead decision 15 records it. No expected value is undecided. No rule page changed.

## Failure classes dismissed

- **FC-001, FC-002, FC-003.** See *Guard scope*. The boundary at the statement's column is pinned on both sides (families A and B), and at depth 12 (family C).
- **FC-010, FC-011, FC-016.** Head is stable on every fixture of the corpora. The loop's bound (`MaxAlignmentPasses`) is unchanged; the new snapshot can only report a change where a read changed. Sibling shapes of #3 (other constructs, unmoved kinds, literal states, inner constructs, nested literal, moved-line anchors) all hold; the moved-line anchors are #6 for the change doc only.
- **FC-012.** Only the indentation phase changed.
- **FC-015.** See *Policy ownership*.
- **FC-004, FC-005, FC-019, FC-020, FC-021.** No token or trivia is moved; only line columns change. Comments inside the block (`//`, `///`) and on the anchor line keep their text; a directive cannot stand in a hole.
- **FC-006, FC-022 to FC-027, FC-029, FC-036.** No syntax rewrite. Literal text is untouched: nested `$@` continuation lines and comment end lines keep column 0, nested raw literals land under their quotes.
- **FC-007, FC-008, FC-009, FC-018, FC-030.** No code fix changed.
- **FC-028.** The node-level root shift reaches `ShiftRange` with `Contributor == null`, so a delta of 0 still changes nothing there.
- **FC-013, FC-014.** No analyzer change.
- **FC-017.** See *Assertion adequacy*.
- **FC-031.** The summaries of `CaptureColumns`, `MatchesColumns`, `ShiftRange`, `GetCapturedColumn`, and the inline comment in `LambdaAlignmentContributor.Contribute` describe the new code. No `TODO`, no tracker number (`git diff 4a31be5 2a265da -- src`). The change doc gap is #6.
- **FC-032.** 28 ran formatter, analyzer, and CLI projects on acb2ea0, the only production commit; 56c5d86 and later add tests and handovers only. 30 confirms a warning-free Release build.
- **FC-033.** Every regression test runs LF and CRLF; my CRLF copies give the LF output with CRLF only.
- **FC-034, FC-035.** 27 and 29 derived expected values from the requirements and recorded the failures before acb2ea0.
- **FC-037.** Not applicable.
- **Tab matrix.** No whitespace measurement changed.

## Architecture findings

_None._ The architecture finding of 25 is resolved: placement is recorded for every accepted write, whatever the delta, and the convergence check compares what contributors read.

## Hints

1. #6's `largs` input is a cheap regression test for the moved-line member (aligned literal, `/* */` end line, statement column 8), if the lead wants it pinned beside families A–D.
2. `ShiftRange` now scans every layout key also for a delta of 0, so each block lambda costs one scan per sweep, not only in the sweep that moves it. Not measured as a problem; the first sweep already scanned for almost every lambda.
3. The previous hints are unchanged; they are follow-up candidates 3, 4, and 5 in `00-lead.md`.
