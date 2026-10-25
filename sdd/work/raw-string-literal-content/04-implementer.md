Role: implementer
Commit: f50a0ba
Verdict: #951: tests pass; #1133: tests pass; #1146: test disputed

Inputs: `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-test-implementer.md`, and `git diff origin/main -- sdd/specs/ documentation/`. The prompt carried no intended fix. No test file was changed; every statement below was observed on `f50a0ba`. `Core/FormattingTextAnalysisUtilities.cs` was not touched, nor any file the lead lists as owned by a concurrent run. No public API, no dependency, and no shared helper with other consumers changed. `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken` gained a `position` parameter; its only consumers are the RH5601 analyzer and its code fix.

## thoenissen/Reihitsu#951

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | Registers the lines that a token starts inside a hole of a multi-line interpolated raw string, with the innermost such hole and the token's current column. While the alignment sweeps run, a write to such a line (`Set`, `ShiftRange`) is ignored unless the writing contributor's node lies inside that hole; an accepted write marks the line placed. `TryGetPlacedColumn` returns the current column for an unplaced hole line, else the layout column. `ReleaseUnplacedRawStringHoleLines` drops the layout of unplaced hole lines, so their whitespace is kept. Block indentation (pass 1) still writes to hole lines, so an in-hole lambda's `ShiftRange` keeps its relative body offsets. | FMT-RAW-007, FMT-INDENT-011, FMT-INDENT-013 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | `RegisterRawStringHoleLines` runs before pass 1. `RunAlignmentSweeps` sets `model.Contributor` to each node. `GetAdjustedColumn` reads `TryGetPlacedColumn`, so an anchor on an unplaced hole line is measured where it stays. Release runs after pass 3. | FMT-RAW-007 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/CommentIndentationContributor.cs` | Reads the following token's column through `TryGetPlacedColumn`, so a comment that starts a hole line takes the column of the token it precedes (+4 before a `}`), measured before the literal's shift, which then moves both. | FMT-RAW-007 (comment condition), FMT-INDENT-006 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | `ApplyIndentation` no longer returns early when the token's own line has no layout. A line without a layout still keeps its whitespace in `RebuildLeadingTrivia`, but a comment line in the leading trivia can now get its layout. Before this change, every first-on-line token had a layout, so only hole lines reach the new path. | FMT-RAW-007 (comment condition) |
| `src/Reihitsu.Formatter/Pipeline/RawStringAlignment/RawStringAlignmentPhase.cs` | Replaced the per-literal `AdjustContentLines` with a per-line shift table. Literals are visited outermost first. Each multi-line raw literal (plain, `u8`, interpolated) adds `opening quote column − closing column` to every later line of its span, both columns measured after the shifts of the enclosing literals. Lines that start inside the text of a non-raw literal, or inside a comment that began earlier, never shift. Only outermost literals are replaced; nested ones are rewritten inside them. | FMT-RAW-001, FMT-RAW-006, FMT-RAW-007, FMT-RAW-008, FMT-RAW-009 |

### Policy owners

- Raw-string target column: `rg -n "GetQuoteOffset|GetClosingColumn|AdjustContentLines" src --glob '!*Test*'`. Owners: `RawStringLiteralUtilities.GetQuoteOffset` (Core, reused). Other consumers: `MethodChainAlignmentContributor`, `RH5202…Analyzer`, `RH5202…CodeFixProvider`. All of them are unchanged. `AdjustContentLines`/`GetClosingColumnFromLastLine` were private to the phase and are gone.
- Multi-line interpolated raw string detection: `rg -n "InterpolatedRawStringEndToken|InterpolatedMultiLineRawStringStartToken" src --glob '!*Test*'`. Owners are each consumer's own one-line kind check (`MethodChainAlignmentContributor`, RH5202 analyzer and fix, RH5601, the phase, `LayoutComputer`). No shared predicate exists, and I added none.
- "Line starts inside nested text": `RawStringAlignmentPhase.StartsInsideNestedText` (new, private). This is a different policy from the RH5104 fix's `StartsInsideLiteralOrComment` (see #1146): raw text and hole code count as movable here, and as withheld there. Neither one matches `FormattingTextAnalysisUtilities.GetStringLineIndices`/`GetNonFormattableLineIndices`, which mark every line a literal or comment intersects, the first one included.

### Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment"
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~RawStringAlignment|FullyQualifiedName~SelfHostingTests|FullyQualifiedName~Indentation"
```

- `Passed! - Failed: 0, Passed: 43` (RawStringAlignment, all 27 `RawStringInterpolationHoleTests` included).
- `Passed! - Failed: 0, Passed: 660` (after the final comment edits).
- RH5202 formatter-parity tests: they pass in the analyzer filter under #1146 below.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._ The unspecified cases of FMT-RAW-007 are unchanged in kind. A line that an earlier phase breaks inside a hole, and that no in-hole construct places, keeps the column that phase gave it and moves with the literal. A hole line indented less than a leftward shift is clamped at column 0, as before.

## thoenissen/Reihitsu#1133

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5601UseTabsCorrectlyAnalyzer.cs` | `IsStringContentToken(token, position)` also treats `InterpolatedMultiLineRawStringStartToken` and `InterpolatedRawStringEndToken` as content, but only within their own text (`token.Span.Contains(position)`). Roslyn puts the whitespace after `$"""` and the closing line's indentation into those tokens' text. Their trivia, such as the tab in `"""→;` or `→$"""`, stays code, and so does the trivia of the other literal kinds. | RH5601-002, RH5601-001, SHARED-DIAG-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5601UseTabsCorrectlyCodeFixProvider.cs` | Passes the diagnostic position to the same predicate. | RH5601-002, SHARED-FIX-003 |

### Policy owners

`rg -n "IsStringContentToken" src`: one owner (the analyzer), used by the analyzer and the fix.

### Tests run

`scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5104|FullyQualifiedName~RH5601|FullyQualifiedName~RH5602|FullyQualifiedName~RH5202"`, result `Failed: 1, Passed: 124, Total: 125`. The one failure is the disputed RH5104 test below. Every RH5601, RH5602, and RH5202 test passes.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._ The RH5601/RH5602 gap against SHARED-DIAG-003 is already listed by the spec author.

## thoenissen/Reihitsu#1146

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5104CommentsMustBeOnTheirOwnLineAnalyzer.cs` | `IsInlineComment` now asks whether a non-empty token overlaps any line the comment spans. It no longer asks whether non-whitespace text surrounds the comment, so other comments on the line no longer count as code. | RH5104-002, RH5104-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.cs` | `RequiresBlankLineBeforeRelocatedComment` is removed, so the fix never inserts a blank line. The new `StartsInsideLiteralOrComment` withholds the action when the comment's start line begins inside any of: <br>- a non-whitespace trivia that began earlier (`/* */`, `/** */`); <br>- a token that began earlier (verbatim, raw, `u8` raw); <br>- an `InterpolatedStringExpressionSyntax` that began earlier (text, hole, or opening brace). <br>`RegisterCodeFixesAsync` became `async` to read the root and text. | RH5104-005, RH5104-006, RH5104-004, SHARED-FIX-003, SHARED-FIX-006 |

### Policy owners

- `rg -n "FindTrivia\(" src --glob '!*Test*'` and `rg -n "GetNonFormattableLineIndices|GetStringLineIndices|IsInsideCommentOrDisabledText" src --glob '!*Test*'`. No existing owner decides "the line begins inside a literal or comment that started on an earlier line". `SyntaxTriviaUtilities.IsInsideCommentOrDisabledText` tests a position for comment or disabled text only. The Core line-index sets include a literal's first line, which RH5104-006 says must stay fixable.
- The blank-line policy (RH5020) is no longer consumed by this fix. `BlankLineEditor.IsFirstInBlock` and `FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex` are unchanged and keep their other consumers.

### Tests run

Same analyzer command as #1133: 124 of the 125 RH5104/RH5601/RH5602/RH5202 tests pass. The 24 new and the 1 updated RH5104 tests pass, apart from the disputed one.

### Disputed tests or scope

- **`RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests.VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported`** (RH5104-006). It fails with `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"`, at `(6,6)-(6,13)`. That is `/* c */` inside the verbatim-string line, which stays reported, as the spec says.
  - **What the test asserts.** The fixed markup carries `{|RH5104:/* c */|}`, and the test calls `Verify(testData, fixedData)` with default fixed-state markup handling. For a diagnostic ID the provider lists as fixable, that default ignores markup in the fixed state, so the verifier expects 0 diagnostics there.
  - **What the spec says.** RH5104-006: the comment is still reported, and no fix is offered. The fixed state therefore has 1 diagnostic.
  - **Why they disagree.** The test's harness configuration contradicts the expected output it wrote itself. The repository's pattern for remaining fixable diagnostics in fixed code is `test.FixedState.MarkupHandling = MarkupMode.Allow`, plus `BatchFixedState` for Fix All (for example `RH4107ProtectedFieldCasingAnalyzerTests.cs:265-266` and `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.cs:355`).
  - **Probe.** With both lines added locally, the test passed: `Passed: 1`, fixed text and Fix All text included. The change was reverted immediately and is not committed.
  - **Routing.** To the test implementer. Assertions and expected text stay as they are; only the markup handling needs `Allow`.

### Follow-up candidates

_None_ beyond those the spec author and skeptic already listed (RH5104 reordering for `M(); /* a */ // b`).

## Whole run

- Full Debug runs on `f50a0ba`'s tree before the final comment-only edits (XML summaries of `LayoutModel.Set`/`ShiftRange`, `RunAlignmentSweeps`, the phase class):
  - analyzer: `Failed: 1, Passed: 6090, Total: 6091` (the disputed test);
  - formatter `2711/2711`, core `527/527`, cli `250/250`, architecture `84/84`, tooling `120/120`.
- After those edits, `scripts/format.sh` over the 9 changed files reported `Formatted 0 of 9`.
- The formatter filter `RawStringAlignment|SelfHostingTests|Indentation` gave `660/660`.
- The analyzer RH5104/RH5601/RH5602/RH5202 filter gave `124/125`, with the disputed test failing.
- The Debug build shows no warnings in the changed files after formatting. Release warnings are the cleaner's.

# Repair round 1

Role: implementer
Commit: f7ca92a
Verdict: #951: tests pass (findings 1–5, 7); #1133: nothing to repair; #1146: tests pass (finding 6 / RH5104-007)

Inputs:
- `07-auditor.md`, findings 1–7.
- The "Repair round 1" section of `00-lead.md`, with escalation 4, answer A.
- RH5104-007 and RH5104-004 in `sdd/specs/analyzer/RH5104.md`.
- The "Repair round 1 — auditor findings" section of `03-test-implementer.md` (c5cb834).

I re-derived each of the auditor's suggested changes instead of applying it as written; how is stated per finding. No test was changed. All statements were observed on `f7ca92a`.

## thoenissen/Reihitsu#951

### Mechanism

Findings 1, 2, 4, and 5 share one premise, and the auditor states it: the indentation phase measured columns before the raw-string phase moves the lines, and it assumed every line moves by the same amount. They are repaired by one owner of the per-line move. The two phases now share it, and nothing else was added for them.

### Changes

| File | What changed | Finding / requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/RawStringAlignment/Utilities/RawStringLineShifts.cs` (new) | The single owner of how many columns the raw-string phase moves each line. It collects every multi-line raw literal (plain, `u8`, interpolated) and the lines each one moves, with FMT-RAW-009 lines excluded. `GetShift(line, getColumnBeforeShift)` sums, outermost first, each literal's opening-quote column minus its closing column. The opening-quote column includes the shift of its own opening line (an earlier or enclosing literal). The closing column includes the shifts of the enclosing literals. The caller supplies the column a literal's first token has before any line moves. The raw-string phase passes the column in the text. The indentation phase passes its own column after indentation. The frozen-line predicate moved here from the phase, and now counts only `/* */` and `/** */` comments; `///` was dropped (finding 3). | 1, 3, 5; FMT-RAW-001, -007, -008, -009 |
| `.../RawStringAlignment/Utilities/RawStringLiteral.cs` (new) | Record describing one literal: node, opening and closing line, closing column, quote offset. | — |
| `.../RawStringAlignment/RawStringAlignmentPhase.cs` | `ComputeLineShifts`, `StartsInsideNestedText`, `IsNonRawStringText`, `GetOpeningQuoteColumn`, and `GetShift` were removed. The phase now asks `RawStringLineShifts` and keeps only the replacement building. | 1, 5 (FC-015) |
| `.../Indentation/Utilities/LayoutModel.cs` | **Columns that contributors read and write are columns after formatting.** `Set` stores `column − GetRawStringShift(line)`, the indentation the rewriter writes. Two new reads: `TryGetPlacedColumn` (now after the shift), `TryGetPlacedColumnBeforeRawStringShift`, and `TryGetLayoutColumn` (the stored layout, a stand-in included, after the shift). `TryGetLayout` stays the raw stored value, for the rewriter only. **Finding 7:** `AcceptsWrite` is now a pure check, and the new `RecordPlacement` marks a hole line placed. `Set` and `ShiftRange` call it after an accepted write. The `Contributor` property and the `Set`/`ShiftRange` summaries state the effect. `_alignedRawStringHoleLines` was renamed `_placedRawStringHoleLines`. | 1, 4, 7 |
| `.../Indentation/Utilities/LayoutComputer.cs` | `Compute` hands the model a `RawStringLineShifts`. `GetAdjustedColumn` is now `GetColumnBeforeRawStringShift(token) + GetRawStringShift(line)`, so an anchor on a nested literal's closing line (1a), on a frozen line (1b), or behind any raw literal's closing marker (5) is measured where it ends up. `RegisterRawStringHoleLines` registers `GetColumnAfterTabReplacement`, which counts each whitespace tab as `FormattingContext.IndentSize` (4) columns (finding 2). | 1, 2, 5 |
| `.../Indentation/Contributors/MethodChainAlignmentContributor.cs` | Removed the private prediction (`GetAdjustedColumn`, `IsMultiLineRawStringEnd`, `GetLeadingSpaceCountOfLastLine`). It now calls `LayoutComputer.GetAdjustedColumn`, which predicts the same shift for the single-literal case and the right one when the literal's opening line moves too. | 5 (FC-015, FC-031) |
| `.../Indentation/Contributors/GenericConstraintContributor.cs`, `ConstructorInitializerContributor.cs` | Read the declaration line through `TryGetPlacedColumn` instead of `TryGetLayout`. A constructor initializer cannot stand in a hole, so the constructor change only keeps one accessor for the same policy. Outside raw literals both reads return the same value. | 4 (FC-015) |
| `.../Indentation/Contributors/LambdaAlignmentContributor.cs` | Reads the brace line through `TryGetLayoutColumn`, so the relative `ShiftRange` base and the anchor are both after the shift. The stand-in block layout is still the base for a hole line, which keeps `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment`. | follows from 1 |

### Re-derivation of the suggested changes

- **Finding 1.** Confirmed (the two new tests were red). One shift owner, read through `GetAdjustedColumn`, instead of a contributor-side correction. The anchored line is stored as `final(anchor) − shift(own line)`.
- **Finding 2.** Confirmed. The auditor offered two options: measure after the tab replacement, or give the line a layout. A layout would rewrite the line's whitespace, which FMT-RAW-007 forbids beyond the FMT-CLEAN-002 replacement. So the registered column counts tabs as FMT-CLEAN-002 will expand them.
  - `CleanupPhase` owns that replacement (`"\t"` → four spaces) and belongs to a concurrent run, so I did not reuse or change it. The count is a second statement of the same rule: one `switch` in `GetColumnAfterTabReplacement`, which cites FMT-CLEAN-002.
- **Finding 3.** Confirmed. `///` was excluded from the frozen-line predicate. The spec needed no change, because FMT-RAW-009's examples and FMT-INDENT-007 name only `/* */`.
- **Finding 4.** Confirmed. Applied as suggested.
- **Finding 5.** The single-owner option was chosen. It needs no mechanism beyond what finding 1(a) already requires: the shift of a line on a raw literal's closing line, with the shift of the literal's opening line included. The test passes.
- **Finding 7.** Applied: the check and the side effect are split, and the effect is documented.

### Behavior change beyond the listed findings

**Auditor hint 1.** `GetAdjustedColumn` now predicts the shift for any anchor behind a raw literal's closing marker. A hole is not needed, so the hint-1 shape outside a hole is also fixed.
- Input: `return M("""⏎    a⏎    """ + string.Concat("x",⏎                        "y"));`
- Trace: `bash scripts/trace.sh <fixture> --passes 3` (scratchpad fixture). Pass 1 puts `"x"` and `"y"` both at 37, and the run reports "Stable after pass 2 (0 phase changes observed)". The auditor reported 24, then 37.
- Already aligned literals have a shift of 0, so formatted code is unaffected. The self-hosting test passes.
- No test pins this shape. It is a candidate for the test implementer (FMT-INDENT-012, FMT-PIPE-004).

### Policy owners

- `rg -n "GetShift|ComputeLineShifts|GetLeadingSpaceCountOfLastLine|IsMultiLineRawStringEnd|RealignContent" src --glob '!*Test*'`
  - **Raw-string line shift:** `RawStringLineShifts` is the only owner in the formatter.
  - **Remaining copy:** `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent` is unchanged, outside this repair, and already a maintainer-approved follow-up (thoenissen/Reihitsu#1458).
- `rg -n "TryGetLayout\(|TryGetPlacedColumn|TryGetLayoutColumn" src/Reihitsu.Formatter`
  - Contributors read `TryGetPlacedColumn` (comment, generic constraint, constructor initializer, `GetAdjustedColumn`) or `TryGetLayoutColumn` (lambda).
  - `TryGetLayout` remains in `IndentationRewriter`, which writes stored indentation, and in `LayoutModel` itself.
- **New code owners.** `RawStringLineShifts` and `RawStringLiteral` sit in `Reihitsu.Formatter.Pipeline.RawStringAlignment.Utilities`. `NamespaceTests.OtherTypesShouldNotBeInFormattingPhaseNamespace` requires that namespace. They are not yet listed under the Code owners of `sdd/specs/formatter/raw-string-alignment.md`. That is for the spec author; no architecture test requires it.

### Tests run

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment"
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~RawStringAlignment|FullyQualifiedName~SelfHostingTests"
```

- First command: `Passed: 51, Failed: 0` (the 8 new red tests included).
- Second command: `Passed: 67`, after the namespace move.

### Disputed tests or scope

_None._ Finding 5 needed no mechanism beyond finding 1(a).

### Follow-up candidates

- **Auditor hint 3 (FC-028), unverified and unchanged.** A fix that formats a node lying inside a hole registers only the holes inside that node.

## thoenissen/Reihitsu#1133

No finding; nothing changed.

## thoenissen/Reihitsu#1146

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.cs` | New `HasCommentBeforeOnLine(root, lineStart, commentStart)`: a comment trivia (`SyntaxTriviaUtilities.IsCommentTrivia`) that lies wholly between the start of the comment's start line and the comment. `RegisterCodeFixesAsync` withholds the action when it holds, next to `StartsInsideLiteralOrComment`. | RH5104-007, RH5104-004, SHARED-FIX-003 (maintainer answer A) |

Finding 6 also asks to "align the fix's notion of code with the analyzer's". I re-derived that and left `GetCommentRemoval` unchanged:
- With RH5104-007, a comment in front of the reported comment on its start line always withholds the fix. So `hasCodeBeforeComment` can no longer see another comment.
- The only divergence left is on the comment's last line. There, a following comment counts as code, which keeps that comment behind the line break the moved comment carried: `N(x); /* a⏎ b */ // c` becomes `/* a⏎ b */`, `N(x);`, `// c`. The order is kept, and nothing is joined.
- Switching to tokens there would join `// c` onto `N(x);`, which is a layout change no requirement asks for.

### Policy owners

`rg -n "IsCommentTrivia" src/Reihitsu.Core/SyntaxTriviaUtilities.cs` is the shared owner of what counts as a comment, and I reused it. "Another comment before it on the line" has no other owner (`rg -n "HasCommentBefore|CommentBeforeOnLine" src --glob '!*Test*'`).

### Tests run

`scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5104"`: `Passed: 51, Failed: 0`. This includes the 4 new tests and the exact iteration counts of `VerifyFixAllKeepsTheOrderOfCommentsThatShareALine`.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._

## Whole run

- Full Debug runs on the tree of `f7ca92a`, before the namespace move:
  - formatter `2719/2719`
  - analyzer `6095/6095`
  - core `527/527`
  - cli `250/250`
  - tooling `120/120`
  - architecture `83/84`. The failure was `OtherTypesShouldNotBeInFormattingPhaseNamespace`, for the two new types.
- After the move to `…RawStringAlignment.Utilities`:
  - architecture `84/84`
  - formatter `RawStringAlignment|SelfHostingTests` `67/67`
- `scripts/format.sh` over all 10 changed files: `Formatted 0 of 10`.
- The Debug build shows no warnings in the changed files.
