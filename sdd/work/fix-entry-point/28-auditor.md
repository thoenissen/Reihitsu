Role: auditor
Commit: 02126b5fedafbb09298ac1ecf564662fdae1b322
Verdict: findings

My prompt named the audit's scope and carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audits the structural change of `00-lead.md` rows 24 and 25: `git diff f1192956 02126b5f`, against finding 6 and the architecture finding of `20-auditor.md`, with handovers 21–27. It is a delta audit, not a full one.

- Head `02126b5f` is checked out, and the tree is clean.
- Base `origin/main` is `df4f828d`, as my prompt states (`git ls-remote`). The branch contains it (`git merge-base --is-ancestor`).
- The diff holds what 21–27 describe:
  - the FMT-PIPE-029 amendments and the change-doc case 7 amendment;
  - 16 RH7101 tests and 16 `ReihitsuFormatterFieldSplitTests`;
  - in `ReihitsuFormatter.cs`, `FormatSplitFieldInContext` (the precondition, the read of the layout, and the second layout), `TryGetSplitFields`, and `StartsLineInLayout`.
- No other production file changed. The scope did not grow.
- I relied on the cleaner's Release build and the roles' focused results. I re-ran tests only for one mutation, under *Assertion adequacy*.

How I checked:

- I ran `scripts/apply-fix.sh RH7101 --max-iterations 1` on the head over 29 fixtures, under LF and CRLF. All arms gave the same diff under both line endings.
  - 27 fixtures ended `fixed` with one action and one iteration.
  - f13 holds three combined declarations, so it needs three iterations. With `--max-iterations 5` it ended `fixed` in 3 iterations, and the result is the formatter's layout without its blank lines.
- I formatted every fixture and every fix result with `scripts/format.sh`. For all 22 fixtures of the class sweep, formatting the source and formatting the fix result gave the same text. A second formatter pass over the fix results changed 0 of 22.
- I built a mutation in a scratch worktree, described under *Assertion adequacy*.
- The fixtures and the worktree were in the scratchpad and are removed.

Status of earlier findings:

| # | Status | Evidence |
|---|---|---|
| 1 | closed (unchanged) | `FieldDeclarationSplitTransform.BuildTrailingTrivia` is not in this diff, and 20's evidence still holds. f03 and f11 (a spanning `/* */` in both gaps) give the formatter's layout |
| 2 | closed (unchanged) | `p`, `q`, and `pq` stay green. f03 and f11 hold |
| 3 | closed (unchanged) | The architecture test is not in this diff. `ReihitsuFormatter` is the formatter's own type, and no code-fix file changed |
| 4 | closed (unchanged) | `ReihitsuFormatter.cs:526` still uses `SyntaxTokenPositionUtilities.GetColumn`, and no private copy came back |
| 5 | closed (unchanged) | Not in the diff |
| 6 | closed | All members 20 listed as failing now give the formatter's layout without the blank line. The behind form of g2 does too. Tests in 22 cover d1/d2 (A), d5/d6 (B), d4/g1 (C), and g2/g3 (D), each in front, behind, and in both gaps. My fixtures f05 (A), f16 (C), and f07/f08/f09 (doc-comment combinations 20 did not list) hold as well. The members 20 listed as holding still hold: d3/d7 (f04), f1/f4, d8–d10 (f17, f19), and `p`/`q`/`pq` |
| Architecture finding of 20 | closed | The entry point no longer restates a phase's rule. "Shares the line" is read from the formatted context (`StartsLineInLayout`), so the documentation-comment phase and the line-break phase each decide once, inside the pipeline. The only test the entry point still makes on the input is the precondition (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine`). FMT-PIPE-029 states that precondition as the FMT-PIPE-018/010 boundary, and it does not predict any phase |

### Class of finding 6, enumerated again

The decision: whether the first field, or the member behind the target, starts a new line, as document-level formatting decides it. The axes, and every member that the RH7101 fix reaches:

- **Side.** The gap in front of the target, the gap behind it, and both.
- **Gap content.** Fixtures for each:
  - none (f01);
  - a one-line `/* */` (f02);
  - a spanning `/* */` (f03);
  - a one-line `/** */` (f04);
  - a spanning `/** */` (f05);
  - a one-line `/** */` then a one-line `/* */` (f06);
  - a one-line `/** */` then a spanning `/* */` (f07);
  - a spanning `/** */` then a one-line `/* */` (f08);
  - two documentation comments, the second spanning (f09);
  - a one-line `/* */` then a spanning `/** */` (f16);
  - a spanning `/** */` holding XML (f21);
  - the holding side, where something in the gap starts a line: a spanning `/** */` then an end of line (f17), `///` (f19), and an end of line then `/** x */` in front of the member (f18, row 25).
  - The B and D shapes are covered by the tests of 22.
  - A directive or disabled text always carries an end of line, so the precondition keeps that trivia, and a directive inside the declaration withholds the split (FMT-STRUCT-023).
- **Which phase starts the line in document-level formatting.** None; the line-break phase (`MemberLineStartRewriter`); the documentation-comment phase (relocation); the attribute placement of the line-break phase (f10, f11); the split of a neighbouring combined field (f13).
- **Neighbour kind.** A field; a combined field (f13); a method in front (f12); a property behind (f12); a member with an attribute list behind (f10, f11); the target with an attribute list (f10, f11).
- **Container.** A class; a nested struct with an initializer (f15); a record (f16).
- **Others.**
  - Indentation by spaces, and by tabs (f14).
  - A target whose later declarator is on a later line (f20).
  - A member in front that carries `///` (f22).
  - LF and CRLF for every fixture.

Every member holds. In each one, the fix moves exactly the members that document-level formatting moves. Outside the blank lines, the fix result differs from the formatter's output only in code the fix does not change:

- the body of a neighbouring method (f10, f11, f12);
- the `///` normalization of `_p`'s documentation comment (f22);
- a tab in an untouched line (f14);
- the holding side f17 and f19, which FMT-PIPE-018 and FMT-PIPE-010 decide.

I also traced why the read of the layout covers the whole class:

- In the gaps the precondition admits, the lexer never puts a non-documentation comment in front of a documentation comment in the member's leading trivia. A documentation comment ends the token's trailing trivia, and everything else up to it is trailing trivia of the token in front. So the first comment in that leading trivia is a documentation comment whenever the trivia holds one. That is the comment `IsMovableDelimitedDocumentation` lets the phase move.
- The context is the declaring type (`RH7101DoNotCombineFieldsCodeFixProvider.cs:37`). The facts in which `FormatContext` differs from document-level formatting cannot change a line start between two members:
  - `TargetFirstTokenColumn` is read only by `LayoutComputer.cs:376`;
  - the disabled `AccessorExpressionBody` transform;
  - no region formatting;
  - `PreserveRootDocumentationBoundary`, which concerns only the root's first token.

No new finding repeats the class of finding 6 or of any earlier audit finding. See `Repeats` below.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | medium | `src/Reihitsu.Formatter/ReihitsuFormatter.cs:505-520` (the second `FormatContext` call in `FormatSplitFieldInContext`); tests `ReihitsuFormatterFieldSplitTests`, `RH7101DoNotCombineFieldsAnalyzerTests` | FC-017, FC-003 | – | new mechanism | **No test covers the second layout.** The first layout runs without the document column. When it shows that the first field keeps the target's line, the entry point lays the context out again with the document column (FMT-PIPE-017, answer 3a). **Mutation.** I made the condition of that second layout always false in a scratch worktree. All 45 `ReihitsuFormatterFieldSplitTests` and all 81 RH7101 tests still passed. **The output it decides (g02).** Input `class C⏎{⏎    private int _p; /* a⏎   b */      private int _x = 1 +⏎        2, _y;⏎}`. The target's first token stands in document column 13. The head writes `   b */      private int _x = 1` / `+ 2` at column 30, under `1`. That is FMT-PIPE-017's layout from the document column, and the same as the formatter's alignment relative to the token. Without the second layout, `+ 2` lands at column 25, aligned to a column the written-back token does not have. g01, g03, g05, and g07 show no difference: there the token is one space behind `*/`, so both columns are the same. Since the comment in front of the gap spans lines, the line-break phase leaves the first field behind `*/`. So only an input like this one, with more than one space or a tab in front of the target's first token and a first field that spans lines, exercises the branch | Add an entry-point test and an RH7101 fix test for g02, with exact text under LF and CRLF and the expected value from FMT-PIPE-017/029 (the first field keeps its column, and its wrapped line is laid out from it). A variant with a tab in front of `private` would also cover answer 3b on this path |
| 8 | low | `src/Reihitsu.Formatter/ReihitsuFormatter.cs:631-632` (`StartsLineInLayout`) | FC-015 | – | PR-introduced | **The copy.** `StartsLineInLayout` tests `SingleLineDocumentationCommentTrivia \|\| MultiLineDocumentationCommentTrivia` inline. `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:82`) states exactly that predicate, and its summary says callers share it so that copies cannot drift. The two agree today, so nothing is corrupted. **Not a repeat.** #4 was also a private copy of a Core helper in `ReihitsuFormatter`, but for a different decision (the token column). Under `squad/README.md`, *Architecture findings*, that makes it a different class | Call `SyntaxTriviaUtilities.IsDocumentationCommentTrivia(leadingTrivia[triviaIndex])` |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the shared-line question arises (`mayShareLineWithPrecedingMember` / `…FollowingMember`) | `AreSeparatedByEndOfLine`. True when the trailing trivia of the token in front of the gap, or the leading trivia of the token behind it, holds an end of line, a `///`, a directive, or disabled text. A line break inside a `/* */` or `/** */` does not count | FMT-PIPE-029's precondition: the line breaks SHARED-BLANK-008 counts. `FormatNodeInDocumentAsync` (`ReihitsuFormatter.cs:186`) uses the same helper for its own trivia write-back | Same helper and same span (the whole gap). Both sides are tested: c5/c6 and the row-25 pair hold, and A–D move | holds |
| Whether the first field shares the line (`StartsLineInLayout(formattedFields[0].GetFirstToken())`) | In the formatted context: the token itself, then each documentation comment in its leading trivia (`StartsLineAtLeadingTriviaIndex`, which counts no line break inside comment text) | Document-level formatting: `DocumentationCommentFormattingPhase` (`IsWrittenBehindSource`, `IsMovableDelimitedDocumentation`) and `MemberLineStartRewriter:96` | Read from their result, not restated. Where the precondition holds, a non-documentation comment cannot stand first in the member's leading trivia (lexer, see *Scope*) | holds |
| Whether the member behind shares the line (`StartsLineInLayout(last.GetLastToken().GetNextToken())`) | Same, on the first token of the member behind, in the final layout | Same | The final layout differs from the first only in what `LayoutComputer` reads from `TargetFirstTokenColumn`, which is a column and never a line start | holds |
| Which layout is final (`documentColumn` or `null`) | The front-gap answer | FMT-PIPE-017 (the first field keeps its column only on the target's line) | Agrees. Untested: #7 | finding #7 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a member starts a new line in document-level output | `DocumentationCommentFormattingPhase` and `MemberLineStartRewriter`, plus attribute placement and the field split as further phases that start lines. The entry point reads their result in `StartsLineInLayout`. `rg "HasLineBreakBetween\|TokenGapUtilities" ReihitsuFormatter.cs` finds nothing | one owner per phase; the second owner of 20 is gone |
| The gap holds a line break (inside comments included) | `TokenGapUtilities.HasLineBreakBetween` / `HasLineBreak`, used by `MemberLineStartRewriter`, the other line-break rewriters, and `FieldDeclarationSplitTransform.BuildTrailingTrivia`. Its summary names no consumer it has lost | one statement |
| A line break separates two tokens (SHARED-BLANK-008) | `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` / `EndsLine` (Core), used by `ReihitsuFormatter.cs:186` and `:475/477` | one statement |
| A trivia or token starts its line | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` and `SyntaxTokenPositionUtilities.StartsLine` (Core), reused. `AttributeTargetUtilities.StartsLine` is a pre-existing private method and not in this diff | no new copy |
| A trivia is a documentation comment | `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (Core), plus an inline copy in `StartsLineInLayout` | finding #8 |
| The fields the split makes in the formatted context | `TryGetSplitFields`, which replaces the inline lookup of f1192956 and is used by both layouts | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterFieldSplitTests`: 12 tests for A–D in front, behind, and both (FMT-PIPE-029) | The documentation comment and its member start the next line; the comment in front stays behind its token; no blank line | A member left behind `_p;`, `*/`, or `_y;`, or a blank line written | `AssertSplitsTarget`: whole text, LF and CRLF, second pass | adequate. 22 watched all 12 fail before the change |
| Same file: c5, c6, and the row-25 pair (FMT-PIPE-029, -010, -018) | Where something in the gap starts a line, the trivia stays, and no second line break is written | A line break written between `/** x */` and `private`, or a documentation comment moved | `AssertSplitsTarget` | adequate. They pin both sides of the precondition (FC-002, FC-003) |
| `RH7101DoNotCombineFieldsAnalyzerTests`: 12 "moves" tests (RH7101-004) | Exact fix output, exactly one action, and the result is the formatter's layout without its blank lines | Any other text, a second action, or a formatter change beyond blank lines | `Verify`, `AssertSingleFixUnderBothLineEndings`, `AssertFormattingAddsOnlyBlankLines` | adequate. The helper's third assertion removes every blank line, but none of these inputs holds a blank line of its own, so it cannot hide one |
| Same file: 4 holding tests (c5, c6, row 25) | Exact fix output, exactly one action | A moved documentation comment or a doubled line break | `Verify`, `AssertSingleFixUnderBothLineEndings` | adequate. Formatter equality does not apply, because FMT-PIPE-018/010 keep that trivia |
| The second layout (no test) | A first field that keeps the target's line is laid out from the target's document column | g02's wrapped `+ 2` at column 25 instead of 30 | – | missing: finding #7. The mutation that drops it passes every focused test |

## Failure classes dismissed

- **FC-001, FC-002, FC-003**: The precondition and the read of the layout are checked under *Guard scope*. Both sides of the precondition are tested. The one gap is the branch that picks the layout, which is #7.
- **FC-004, FC-019, FC-020, FC-021**:
  - The write-back trivia is unchanged from f1192956: the first field gets the target's leading trivia, the last field the target's trailing trivia, and the member in front and the member behind keep their comments.
  - No comment was dropped or moved off its token in the 29 fixtures, which include one-line and spanning `/* */` and `/** */`, `///`, and documentation comments holding XML.
  - A directive in a gap carries an end of line, so the precondition keeps that trivia. The split guard for a directive inside the declaration is unchanged.
- **FC-005**: No directive guard changed.
- **FC-006, FC-022–FC-027, FC-029, FC-036**: The diff adds no new rewrite semantics, identifiers, or key/value layout. A raw string inside the first field (g07) gives the formatter's layout.
- **FC-007, FC-008, FC-009**: Every single-declaration fixture converged in one iteration. Three combined declarations converged in three (f13). Fix All and the offer guard are not in this diff.
- **FC-010, FC-011, FC-012**: A second formatter pass over the 22 fix results changed nothing. The second layout runs at most once and has no loop.
- **FC-013**: Covered by the class enumeration. The fix moves exactly the members that document-level formatting moves.
- **FC-014, FC-018, FC-028, FC-030**: Not reached. FC-030 is unchanged because the fix still writes back only the split fields and the two gaps.
- **FC-015**: The only new copy is #8.
- **FC-016**: The class of #6 is enumerated above, and every member holds. The sibling `FormatNodeInDocumentAsync` (`:186`) is the pre-existing hint of 14 and 20, and this diff does not touch it.
- **FC-017**: See #7.
- **FC-031**:
  - The remarks of `FormatSplitFieldInContext`, the inline comments at the precondition and at `documentColumn`, and the summaries of `TryGetSplitFields` and `StartsLineInLayout` describe the new code.
  - The `documentColumn` comment says "laid out again for the other". The code re-lays out only from `null` to the document column, but that is the only direction that can be needed: without the question, the first layout already uses the document column.
- **FC-032**: The `apply-fix.sh` and `format.sh` runs used tools built from the head tree, and the mutation runs used tools built from the worktree.
- **FC-033, FC-034, FC-035**:
  - The tests run under LF and CRLF.
  - Their expected values come from the FMT-PIPE-029 examples, which 22 cross-checked against the formatter, and not from patched code.
  - The tests use the auditor's own shapes (d1, d2, d4, d5, d6, g1, g2, g3), not simplified cousins.
- **FC-037**: The change is not text-only.
- **Matrix *Tab width across formatter phases***: Its decision, the column of a raw-string hole line, is not reached. A tab in the type's indentation (f14) holds as decided by FMT-PIPE-017 and answer 3b.

**Visible output (check 10).** The new expected values are covered as follows:

- The "moves" outputs: answer 1a–1f A (the fix follows the formatter's layout) and the unchanged SHARED-FIX-006 (no blank line in the gap).
- The holding outputs: FMT-PIPE-018/010 as answered for the run (answer 3a), and `main`'s behavior, which 21 measured for c5/c6.

No rule page changed, and nothing is undecided.

## Architecture findings

_None._ The structure of row 24 holds across the class. The entry point reads the shared-line decision from the phases' result, and the only test it still makes on the input is the FMT-PIPE-018/010 boundary that the spec states.

## Hints

- The remarks of `FormatSplitFieldInContext` run one line past the width the surrounding lines keep (`ReihitsuFormatter.cs:452`). It is cosmetic, and the formatter does not reflow comments.
- The hint of 14 and 20 about `FormatNodeInDocumentAsync`'s `keepsLeadingTrivia` (`ReihitsuFormatter.cs:186`) is unchanged and still not verified for an input a caller reaches.
