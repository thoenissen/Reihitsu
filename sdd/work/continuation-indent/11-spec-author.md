Role: spec author
Commit: bcd74c2
Verdict: spec written

Amendment round for the lead's decision rows 16, 18, 20, 21, 23, and 24 (`00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those rows, which this round carries out. This handover adds to `06-spec-author.md` and `08-spec-author.md`; it supersedes neither.

Checked on the head's code (`scripts/format.sh`, `scripts/apply-fix.sh`) and on `origin/main` in a temporary worktree: every new example's *before* and *after* (fixtures in `/tmp/claude-0/-home-user-Reihitsu/067a7317-225f-510e-8f9b-6cda6989da93/scratchpad/sa11/`). The existing FMT-BREAK-041 examples still match the head's output exactly (final newline aside). `scripts/test.sh --project architecture`: 198 passed. One finding on the way: a type may be a code owner of at most one spec (`EveryTypeIsCodeOwnerOfAtMostOneSpec`), so `JoinedGap` and `JoinedGapUtilities` are owned by `line-breaks.md` only, following `AttachedPartGapUtilities`, which the precheck also reads.

## Changed requirements

- FMT-BREAK-041 — clarified (row 16): the join removes only the gap's line breaks; the construct is then laid out by the requirements that lay it out on one line — the initializer behind `with` starts its own line (FMT-BREAK-021), an anonymous method's block spans several lines (FMT-BREAK-002, FMT-BREAK-006), an empty type becomes a semicolon declaration (FMT-STRUCT-017). New example with all three. `Uses` gains FMT-STRUCT-017.
- FMT-SPACE-010 — changed (row 20): one space on both sides also for the `=` of a `let` clause, a binary pattern's combinator (`and`, `or`), and the `is` of an `is` pattern expression; `Why:` explains the join's minimal separator; new example on one line (`o is(1)` → `o is (1)`, `arr is[1 or(2), _]` → `arr is [1 or (2), _]`, `(o)is"s"` → `(o) is "s"`, `o is(1)and(2)` → `o is (1) and (2)`, `let y=x` → `let y = x`). `Uses` gains FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041.
- FMT-INDENT-034 — clarified (row 23): a node that a node-level entry point formats as a root of its own counts as the statement that holds a continuation line when no statement, member declaration, or using directive inside it holds the token (the RH5302 `&&` chain); it stands in for the statement or member that holds it in the document.
- Code owners (row 21): `line-breaks.md` gains `ContinuationJoinRewriter`, `JoinedGap`, `JoinedGapUtilities`, `PatternLineStartUtilities`; `indentation.md` gains `ContinuationLineContributor`, `QueryClauseContributor`; `sdd/specs/README.md` kept in sync.
- `AGENTS.md` phase table (row 21): the precheck row names `JoinedGapUtilities.GetGap` as the one statement of FMT-UNSUP-017's gaps; the line-break row names `ContinuationJoinRewriter` with `JoinedGapUtilities.GetGap`/`IsModifier` and `BinaryOperatorLineBreakRewriter` with `PatternLineStartUtilities.StartsOwnLine`; the indentation row names `ContinuationLineContributor` with `LayoutModel.SetContinuation` and `QueryClauseContributor`, and its decisions cell names the alignment of query clauses under the query's first token.
- Change doc `sdd/changes/2026-11/continuation-join.md`:
  - Affected requirements gain FMT-SPACE-010 and RH5111-004.
  - Section 1 (row 16): a new input with *before* (`main`) and *after* for the three reshaped joins.
  - Section 4 (row 18): an RH5111 bullet. `public⏎string Property⏎{ get; set; }⏎= "test";` gave `… } = "test";` with `public` alone on its line on `main`, and gives `public string Property { get; set; } = "test";` now (RH5111-004 already requires the formatter's layout, so the RH5111 spec does not change).
  - New section 5 (row 20): the spacing change on one-line code, with input and output.
  - *Special cases* (row 24): `CreateReturnStatement`'s condition is recorded as deleted and reshaped. The arrow's trailing trivia without its whitespace becomes the `return`'s, as one filter for every conversion, and the `return42` check now also requires that no line break is left.
- Change doc `sdd/changes/2026-11/omitted-type-argument-comment-unsupported.md` (row 24): the `ListElementAligner` guard and skip are recorded as reshaped. Both methods now take the first element's own token with zero-width tokens included, because the guard also kept a one-line `typeof(Dictionary<,>)` away from a column lookup that fails on the empty token.

### What else the spacing change alters on one-line code (row 20)

Compared on `main` and the head with the same one-line input. Changed: every `is` of an `is` pattern expression and every `and`/`or` combinator that touches the token next to it with no whitespace between them, on either side: `is(`, `is[`, `is"s"`, `(o)is`, `)or(`, `)and(`, `or(` inside a list pattern, `(1)or(2)` in a switch arm and in a `case` label; and `let y=x`. Unchanged, because `main` already spaced them: `is not(1)` (already `is not (1)`), `is{ }`, `is var(…)`, the type-test `is`, and gaps that already held whitespace (a run became one space on `main` already). Not changed: the gap behind `case` (`case(1) or (2):` keeps `case(`). No analyzer reports these gaps (RH6001 checks other keywords; RH6006 checks only the space behind `(`), so no diagnostic changes. Any code fix that formats a node holding such a token writes the new spaces, and no fix test pins one: the only test input with such a gap, `RH3001…NegationWhoseOperandTouchesIsBehindItIsReportedWithoutACodeFix` (`!(a)is true`), offers no fix.

## Examples for the tests

Tests whose expected value these changes affect:

- `ContinuationJoinTests.EachGapIsJoined`, row *With expression*: expected `var w = r with` followed by `{` and `};`, each in column 18, as the FMT-BREAK-041 example shows (`r with { };` on one line is wrong under FMT-BREAK-021).
- `ContinuationJoinTests.EachGapIsJoined`, row *Modifier of an anonymous method*: expected `Func<int> g = static delegate`, then `{`, `return 1;`, `};` on lines of their own (FMT-BREAK-041 example).
- `ContinuationJoinTests.ModifierGapsOfOtherDeclarationsAreJoined`: expected `public class N;` instead of `public class N⏎{⏎}` (FMT-STRUCT-017).
- `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyCodeFixConvergesForMultilinePropertyInitializer`: expected `public string Property { get; set; } = "test";` (change doc section 4).

New tests the examples seed:

- FMT-BREAK-041: the reshaping example (whole document).
- FMT-SPACE-010: the second example (one line), each case on its own; also `case(1)or(2):` → `case(1) or (2):` and the switch-arm `(1)or(2)` → `(1) or (2)` from change doc section 5.
- FMT-INDENT-034: nothing new. The node-level root clause is what `VerifyFixIndentsAContinuationOnTheRootsFirstLineFromTheDocumentsLineStart` (RH5302, RH5308) already checks; the test implementer may add the FMT-INDENT-034 tag where it is missing.

Rows 23 and 24 change no expected value.

## Output questions

_None._ Every case is covered by rows 16, 18, 20, 21, 23, and 24.

## Internal choices

- `JoinedGap` and `JoinedGapUtilities` are owned by `line-breaks.md` only (the spec-structure test allows one spec per type), as `AttachedPartGapUtilities` is, and the precheck reads them the same way. Alternative: `unsupported-syntax.md`, since FMT-BREAK-041 states that its gaps are those FMT-UNSUP-017 names.
- The spacing change is listed in `continuation-join.md` (section 5) rather than in a change doc of its own, because the joins of that change doc are what need it. Alternative: a separate change doc.
- FMT-SPACE-010 keeps one requirement for all of these operators rather than adding a new ID.

## Follow-up candidates

- `case(1)` without a space between `case` and a parenthesized pattern stays as written, on `main` and now: no spacing rule covers `case` followed by `(`, while `switch (`, `is (`, and the combinators get one space. Input `case(1)or(2):`, output on the head `case(1) or (2):`, expected by analogy `case (1) or (2):`. Outside the run's issues.
