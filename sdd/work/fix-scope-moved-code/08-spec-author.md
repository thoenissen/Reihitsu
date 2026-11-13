Role: spec author
Commit: a769844
Verdict: spec written

Decisions 19, 21, 22, 23 recorded in the specs as decisions (AGENTS.md, *Code-fix coverage*); change doc updated with the visible output changes they make. No new requirement IDs. SDD architecture tests (`--filter FullyQualifiedName~Sdd`): 51 passed. My prompt carried only the lead's decisions; nothing in it asked for a particular fix or verdict.

Before and after outputs measured with `scripts/apply-fix.sh` (LF and CRLF alike) on an `origin/main` worktree and on a769844, for: RH7004 code in front of the declaration, last statement on the `}` line, `/* x */ s.Flush();` in column 0; RH7103 `///` behind code documenting the moved member; RH7109 `///` behind code documenting the target, crossed fields sharing a line, left shift past column 0.

## Changed requirements
- SHARED-FIX-007 — *Moved code* bullet — changed (decision 19): the lines that keep column 0 are only **comment lines**, meaning lines that hold nothing but comments and whitespace and whose first comment starts in column 0 (`///` lines are decided on their own marker), plus non-region directive lines. A line that holds code moves, even when a comment in column 0 stands in front of it (`/* x */ Foo();`). The wording follows RH3004-006 and names it. New example: RH7004 with `/* x */ s.Flush();` in column 0, which moves four columns.
- SHARED-FIX-007 — *Moved code* bullet — changed (decision 23): for member/accessor moves and RH7004, a tab at the start of a moved line counts as one column. A right shift writes spaces and a left shift removes whitespace characters. If a line has fewer of them than a left shift removes, it cannot move, and SHARED-ORDER-010 withholds the fix (see *Internal choices* 1).
- SHARED-FIX-007 — *Removed directive line* bullet — clarified (decision 23): when, between two lines, a removal joins blank lines in front of a removed line with blank lines behind it, the blank lines in front stay as written and the ones behind go.
- SHARED-FIX-003 — RH7103 documentation example — changed: it used to point at the SHARED-DOC-004 carry examples. Now a `///` on the lines above the moved declaration travels with it, and a `///` behind code withholds the move.
- SHARED-ORDER-002 — clarified: "a comment that starts its line in column 0" became "a line that holds nothing but comments, the first of them in column 0".
- SHARED-ORDER-009 — changed:
  - Decisions 21 and 22: the leading trivia of the moved declaration and of the target must start their line. A `///` behind code (documentation mode `Parse`/`Diagnose`) withholds a move of the declaration it documents, and a move in front of it.
  - Decision 23: crossed declarations that share a line keep that line and take the first one's position as one block.
  - Two new examples: the RH7109 target comment, and RH7109 crossed `_a; _c`.
- SHARED-ORDER-010 — changed: the fix is withheld when a left shift would cut a moving line off at column 0 (tab counted as one). New RH7109 example (`// two columns` at column 2, shift -4).
- SHARED-DOC-004 — changed (decision 22): a member or accessor move does not carry a `///` comment behind code. The two RH7103 `Parse` examples (plain, and with an `#if DEBUG`/`#endif` pair) are now "is reported, and no code fix is offered". The `None` example is unchanged.
- RH7004-007 — changed (decision 21): retitled "The fix is withheld for lines it cannot change as whole lines". It now lists four line shapes that withhold the fix, the first already there and three new: a statement on the declaration's line, code in front of the declaration on its line, the last moved statement ending on the line of the block's `}`, and a comment or directive in the leading trivia of the declaration's `;`. Three new examples. "A comment in column 0" became "a comment line in column 0".
- RH3004-006 — clarified (decision 19): one paragraph saying its stays/moves rule is SHARED-FIX-007's line rule; only the shift amount is RH3004's own. RH3004's output does not change.
- `Uses` headers:
  - `code-fixes.md` gains RH3004-006 and RH3004-009;
  - `element-documentation.md` gains SHARED-ORDER-009;
  - `ordering-move-safety.md` gains SHARED-DOC-004.
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md`:
  - header and *What* updated;
  - six visible output changes added, each with before and after;
  - four decisions added.
  - Status stays Draft. The lead accepts the new visible changes.

## Examples for the tests
- SHARED-FIX-007: the RH7004 `/* x */ s.Flush();` example. On a769844 the fix keeps that line in column 0; the spec now moves it four columns. **Needs a code change** in `SyntaxIndentationUtilities.KeepsItsColumn`, which today keeps any line whose first trivia is a comment in column 0, and a test.
- SHARED-ORDER-009: the RH7109 target `///` example (no fix; already refused on a769844) and the crossed `_a; _c` example (passes on a769844).
- SHARED-DOC-004 / SHARED-ORDER-009: the RH7103 `Parse` examples, plain and with `#if DEBUG`/`#endif`, become "no code fix is offered". **Needs a code change** (decision 22): a769844 still carries the comment onto its own line. These two existing tests must change to no-fix tests:
  - `RH7103…Tests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration`
  - `FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration`

  `FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed` stays.
- SHARED-ORDER-010: the left-shift example. **Needs a code change**: a769844 offers the fix and cuts `// two columns` off at column 0. `main` offers no fix for this input.
- RH7004-007: the three new examples. Two need new tests: code in front of the declaration, and the last statement on the `}` line. Both are already refused on a769844. For the comment in front of `;`, the existing `NoFixIsOfferedForACommentInFrontOfTheSemicolon` needs its tag changed from SHARED-FIX-010 to RH7004-007 and its summary corrected (decision 21).
- SHARED-FIX-007 *Removed directive line*: blank lines in front of the first removed line stay and those behind go. A test with one blank line in front of `#region` and one behind it (whitespace on the two lines differing, so it is visible which one stayed) pins it, if none exists yet.

## Output questions
_None._ Each new refusal is a rare-class refusal the lead already decided (decisions 21, 22). The `/* x */ Foo();` column follows decision 19 and the maintainer's Q2 A answer (a plain shift of every moved line).

## Internal choices
1. **Left shift past column 0 (SHARED-ORDER-010):** I specified withholding the fix. The alternative is the implemented behavior, cutting the line off at column 0. Cutting off breaks SHARED-FIX-007's own rule that every moving line moves by the same number of columns. It is rare, since it needs a member indented deeper than its slot (RH5204), and `main` gives no fix for the spec example. The check is cheap: count the leading whitespace against the shift. If the lead prefers the cut-off, SHARED-ORDER-010's new sentence and example come out and SHARED-FIX-007 has to say the line ends in column 0.
2. **Where the RH7004 refusals live:** I extended RH7004-007, which is new in this run, and retitled it, instead of adding RH7004-008. The alternative is a new ID that needs `requirement-ids.txt`.
3. **Where the `///` refusal lives:** stated once in SHARED-ORDER-009, which owns the whole-line rule. SHARED-DOC-004 and SHARED-FIX-003 point to it. The alternative is a separate SHARED-ORDER requirement.
4. **Tab counting is scoped** in SHARED-FIX-007 to member/accessor moves and RH7004. RH3004's spec is silent on tabs, and I did not extend this to it.

## Follow-up candidates
_None._
