Role: skeptic
Commit: 36f14e9
Verdict: #1385: proceed (output question 1 open for the section gap); #1398: proceed; #1239: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every "today" output below was produced at 36f14e9 (= `main` 9e9735c plus spec text only) with `scripts/format.sh --no-install <scratch dir>` on fixtures outside the repository (deleted afterwards); RH7302 facts with `scripts/apply-fix.sh --no-install RH7302 <dir>`.

## thoenissen/Reihitsu#1385

### Premise

Real, reached by whole-document formatting (CLI `reihitsu-format`, `FormatDocumentAsync`) on any method body. The issue's two instances reproduce; so do the further instances its comment names, and several more (table under *Breaking inputs*). Code region: `BlankLineEditor` (`src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BlankLineEditor.cs`):

- `EnsureBlankLineBeforeStatement` (l. 244) — skips only when the gap's last significant trivia is a directive (`SyntaxTriviaUtilities.IsPrecededByDirective`), otherwise inserts at leading-trivia index 0, i.e. in front of every directive of the gap. Called for FMT-BLANK-001/-002/-003/-004 (`BlankLineStatementSpacingRewriter.ApplyStatementSpacing`, `BlankLineBreakSpacingRewriter.EnsureBlankLineAfterBreak`).
- `EnsureBlankLineBeforeSection` (l. 268) — FMT-BLANK-004 section gap; inserts at index 0 with no directive check at all.
- `EnsureBlankLineAfterClosingBrace` (l. 289) — FMT-BLANK-005; already inserts behind the last directive (`FindIndexAfterLeadingDirectives`). Fixture 09 below confirms it is correct today.

Severity: the misplaced blank line lands inside the conditional region the directive above opens/closes or separates a `#pragma` from the code it governs; no compile or meaning change, analyzer-clean either way (RH5011-002 exempts a `break` followed by a directive; statement rules count any blank line in the gap per SHARED-BLANK-001). It is a layout bug against FMT-BLANK-016, as triage says, not more.

### Mechanism and candidates

Mechanism: when a blank line that FMT-BLANK-001 to -004 require is inserted into a gap holding a directive whose last significant trivia is not that directive (a comment follows it, or the statement in front belongs to a conditional group that the gap closes), or into any switch-section gap, the editor inserts it at index 0 of the next token's leading trivia — in front of the first directive — instead of behind the last one.

Dispatch: the blank-line phase's rewriter list (`BlankLinePhase.CreateRewriters`): `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter` (FMT-BLANK-011 comment boundary; already directive-aware via `IsPrecededByDirective(trivia.Take(commentIndex))`), `BlankLineRegionDirectiveRewriter` (FMT-BLANK-013, out of scope), `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`. The inserting paths that reach a non-region directive are the three `BlankLineEditor` methods above; two are defective, one is correct.

Candidates by asking requirement (defining type of the gap's owner: `StatementSyntax` first token, or `SwitchSectionSyntax` first token):
FMT-BLANK-001 (compound/jump), -002 (local declaration, statement behind one), -003 (`yield`), -004 (blank line *before* a non-terminal `break`; statement behind `break`; section behind a `break`-ending section), -005 (behind `}`), -017 (block statement after a statement, through -004/-005).

### Breaking inputs

Failure classes: FC-016 (sibling shapes: two of three editor paths), FC-019/FC-020 (directives in the gap, incl. `#if NEVER` disabled groups), FC-003 (directive directly in front vs. comment behind directive — both sides of the FMT-BLANK-006 boundary), FC-010/FC-012 (second pass must keep the blank behind the directive; the cleanup rewriter removes blank lines behind a trailing directive in front of braces — FMT-BLANK-017 excludes a block statement's `{` from that, so cover fixture 31 for two passes), FC-028 not settled (no caller found that formats a statement node whose own leading trivia carries a directive; node-level entry points keep the target's leading trivia, FMT-PIPE-010/-018).

Class matrix — axes: asking requirement × gap shape {directive then comment; conditional group whose branch is disabled; group whose active branch holds the previous statement; two directives with comments between; comment, then directive, then comment; directive directly in front} × list {block, switch section}. Each fixture's today result (`blank` marks where the formatter puts the line):

| # | Input (in a method body) | Today | Status |
|---|---|---|---|
| 01 | `a++;⏎#if NEVER⏎a--;⏎#endif⏎// why⏎if (a > 0) {}` (FMT-BLANK-001) | blank in front of `#if` | fails |
| 02 | same with `#if !NEVER` (active; `if` follows `a--;`) | blank in front of `#endif` | fails |
| 03 | `a++;⏎#pragma warning disable CS0168⏎// why⏎return;` | in front of `#pragma` | fails |
| 04 | `a++;⏎#pragma …⏎// why⏎var b = a;` (FMT-BLANK-002) | in front of `#pragma` | fails |
| 05 | `var b = a;⏎#pragma …⏎// why⏎a++;` (FMT-BLANK-002, behind decl.) | in front of `#pragma` | fails |
| 06 | `a++;⏎#pragma …⏎// why⏎yield return a;` (FMT-BLANK-003) | in front of `#pragma` | fails |
| 07 | loop: `break;⏎#pragma …⏎// why⏎a++;` (FMT-BLANK-004 behind) | in front of `#pragma` | fails |
| 08 | loop: `a++;⏎#pragma …⏎// why⏎break;` (FMT-BLANK-004 before `break`) | in front of `#pragma` | fails |
| 09 | `if (a > 0) {}⏎#pragma …⏎// why⏎a++;` (FMT-BLANK-005) | behind `#pragma` | holds |
| 11 | switch: `break;⏎#pragma …⏎// two⏎case 2:` | in front of `#pragma` | fails (both readings of OQ1) |
| 16 | `a++; // tail⏎#if !NEVER⏎// why⏎if …` | in front of `#if` | fails |
| 21 | `a++;⏎#if !NEVER⏎// c1⏎#pragma …⏎// c2⏎if …` | in front of `#if` | fails (expected behind `#pragma`, in front of `// c2`) |
| 23 | loop: `break;⏎#if !NEVER⏎// scope⏎{ a++; }⏎#endif` (FMT-BLANK-017) | in front of `#if` | fails |
| 25 | switch: `break;⏎#if NEVER⏎case 3: break;⏎#endif⏎// two⏎case 2:` | in front of `#if` | fails (both readings) |
| 10 | switch: `break;⏎#pragma …⏎case 2:` | in front of `#pragma` | OQ1 |
| 12 | switch: `break;⏎#if NEVER⏎case 3: break;⏎#endif⏎case 2:` | in front of `#if` | OQ1 (directive directly in front of the section) |

Members that hold today and must stay (negative tests):

- 20: author-written blank in front of the directive, `a++;⏎⏎#pragma …⏎// why⏎if …` — unchanged; no second blank line (FMT-BLANK-016 last sentence, FMT-BLANK-001 "kept, not doubled").
- 15: `a++;⏎// lead⏎#pragma …⏎// why⏎if …` — today one blank line in front of `// lead` (FMT-BLANK-011) and none behind `#pragma`; FMT-BLANK-001 counts that blank line (SHARED-BLANK-001, "not doubled"), so this stays. The fix must not add a second one.
- 22: `a++;⏎// c⏎#pragma …⏎if …` and 30: loop `break;⏎#if !NEVER⏎{ … }⏎#endif` — directive directly in front: no statement blank line (FMT-BLANK-006); 22 keeps the comment's blank line.
- 13/14: `#region` in the gap — FMT-BLANK-013 frames it (blank above and below) and the statement rule adds nothing; stays.
- 24: section gap behind a `return`-ending section with `#pragma`/comment — no blank line (FMT-BLANK-004 asks none); stays.
- 31/32: the spec's expected shapes (blank behind `#if !NEVER`/`#endif`, in front of the comment) are already fixed points today — a second pass after the fix must leave them (FC-010).

Existing tests that pin the wrong output: `ReihitsuFormatterNodeBlankLineTests.InsertsBlankLineAboveDirectiveOfCommentedBlockStatementAfterBreak` (l. 3008; member 23) and `InsertsBlankLineAboveDirectiveOfSwitchSectionAfterBreakSection` (l. 2706; section gap, rewritten after OQ1).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `BlankLinePhase` (`BlankLineEditor` statement and section paths) | must change |
| `BlankLineEditor.EnsureBlankLineAfterClosingBrace`, region rewriters, comment boundary (FMT-BLANK-011) | must stay |
| RH5002/RH5008 (and other statement-rule) code fixes, SHARED-STMT-004 | not involved (moved to #1447 by triage) |
| Analyzers (RH5011, statement rules) | not involved; both outputs analyzer-clean |
| `Reihitsu.Core` (`SyntaxTriviaUtilities`) | not involved unless the implementer reuses a helper; behavior must stay |
| Rule docs | not involved |
| Spec `blank-lines.md` FMT-BLANK-016 | wording amendment (internal, below) and the section-gap sentence after OQ1 |

### Simpler option

_None._ The fix is local to `BlankLineEditor`; `FindIndexAfterLeadingDirectives` already exists for the correct sibling path.

### Output questions

OQ1 of `01-spec-author.md` (directive directly in front of a section behind a `break`-ending section) is a genuine output question: FMT-BLANK-006 names statements only, so the written spec reads B, the issue asks A. I confirm the facts and the recommendation **A**: it matches the statement path (FMT-BLANK-006), RH5011-002's reasoning, and the issue; B adds a blank line inside the region the directive opens. Member 12 (a whole disabled `#if NEVER … #endif` group directly in front of the section) falls under the same question — under A no blank line, under B one behind `#endif` — and should be shown to the maintainer as a second input of OQ1 so the answer covers it explicitly. Members 11 and 25 (comment between the directive and the section) get the blank line behind the last directive under both readings.

### Internal choices

- FMT-BLANK-016's new coverage list omits the blank line *before* a `break` (FMT-BLANK-004's first clause; member 08 fails today). FMT-BLANK-001's list has no `break`, so the enumeration reads as incomplete. Recommend the spec author add "a `break` statement and" to the FMT-BLANK-004 item. No visible output: the first sentence of FMT-BLANK-016 already demands it.

### Issue goals

- "A change doc records the decision" — drop (triage: bug against FMT-BLANK-016, no change doc). No visible effect. If OQ1 is answered A, the lead decides whether the FMT-BLANK-006 widening needs one (the spec author flagged it).
- "SHARED-STMT-004 states it" and "regression test … for the RH5002/RH5008 fixes" — drop, moved to #1447 (triage). No visible effect in this run.
- "FMT-BLANK-006 … state it" — only on OQ1 = A.
- The issue's "Lane: behavior change" line is stale (triage).

### Architecture findings

_None._ All placement decisions live in one component (`BlankLineEditor`); the defect is FC-016 (two sibling paths miss the rule one sibling applies). The RH5002/RH5008 fixes placing the blank line independently of the formatter is pre-existing, tracked in #1447; after this run their output (blank in front of the directive) is kept by the formatter as an author-written blank line (FMT-BLANK-016), so nothing becomes unstable.

### Follow-up candidates

_None_ beyond #1447.

## thoenissen/Reihitsu#1398

### Premise

Real, reached by whole-document formatting. Fixtures (`switch (o)` with a `break`-only section labeled by a multi-line delimited pattern, the other sections single-line):

- property pattern `case Point⏎{⏎X: 1,⏎Y: 2⏎}:⏎break;` + `case string s: B(); break;` → today `B();` is wrapped in `{ }` (switch braced);
- the same with list pattern `case [⏎1,⏎2⏎]:` (switch over `int[]`), with a parenthesized pattern `case (int⏎or long):`, with two `break;` statements, and with stacked labels `case int:⏎case Point{…}:` → all braced today.
- with another section genuinely multi-line (`A(); B();`) → braced, `break`-only section bare — holds today (issue's second instance; spec author's second example).

Code: `SwitchCaseBraceRewriter` (`src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs`). Severity is low: code a previous formatter run already braced stays braced after the fix, because its blocks span lines and FMT-SWITCH-008 makes such a switch braced — no churn on formatted code, only unbraced input changes.

### Mechanism and candidates

Mechanism: `HasAnyMultiLineSection` asks `IsMultiLineSection` of every non-fall-through section, and `IsMultiLineSection` returns true from `LabelContainsMultiLinePattern` before looking at the statements, so a section whose statements are all `break` (`IsEmptySection`, which `BuildSection` uses to leave it bare) still makes the switch multi-line.

Dispatch: `IsMultiLineSection`'s evidence list — label pattern (FMT-SWITCH-004), non-terminal statements (FMT-SWITCH-003; none in a `break`-only section), `ContainsMultiLineSwitchExpression` (FMT-SWITCH-005; a `break` holds none). So the label is the only path by which a `break`-only section reaches the switch decision. Defining types: `RecursivePatternSyntax`, `ListPatternSyntax`, `ParenthesizedPatternSyntax` anywhere under the section's labels, including a `when` clause.

### Breaking inputs

FC-002 (the qualifier "only `break` statements" is missing from the switch-level predicate), FC-016 (`BuildSection` honors it, `HasAnyMultiLineSection` does not), FC-027 (stacked labels, `when` clause), FC-003 (one `break` vs. several; break-only vs. another multi-line section).

Matrix: pattern kind {recursive, list, parenthesized} × place {case pattern, `when` clause} × `break` count {1, 2} × labels {single, stacked with a single-line label} × other sections {all single-line → must become unbraced (fails today), one multi-line → braced with the break-only section bare (holds)}. Members run: recursive/case/1/single (fails), recursive/case/2 (fails), recursive/stacked (fails), list/case/1 (fails), parenthesized/case/1 (fails), recursive + other multi-line (holds). Not run: `when` clause (`case int x when o is Point⏎{⏎X: 1⏎}:`) — same predicate, the test implementer should cover it.

Neighbour, not a member: a `return`-only (or `throw`-only) section with a multi-line label braces the switch and becomes `{⏎return;⏎}` today. FMT-SWITCH-006 exempts `break` only, so the spec decides this as intended; see follow-up 2.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `SwitchCaseBracePhase` (switch-level multi-line decision) | must change |
| `BuildSection`/`AddBraces`/`RemoveBraces`, directive and goto/local guards | must stay |
| Analyzers, code fixes, `Reihitsu.Core`, rule docs | not involved |
| Spec FMT-SWITCH-004 | clarified (done in 9ee6cfb) |

### Simpler option

_None._

### Output questions

_None._ FMT-SWITCH-006 ("MUST NOT make the switch multi-line") is an unchanged requirement that demands the fixed output.

### Internal choices

_None._ The spec author's choice (precedence sentence inside FMT-SWITCH-004 rather than a new requirement) is fine.

### Issue goals

- "A change doc records the decision" — drop (triage: FMT-SWITCH-006 already requires it). No visible effect.
- "FMT-SWITCH-004/-006 state the precedence" — met by the FMT-SWITCH-004 clarification; -006 already states it.
- "Lane: behavior change" — stale (triage).

### Architecture findings

_None._ The two disagreeing predicates are in one class (FC-016), not two components.

### Follow-up candidates

1. **Formatter layout of a multi-line parenthesized pattern** — pre-existing, outside the run's issues, visible output, reached by whole-document formatting:

   Input:
   ```csharp
   bool M(object o)
   {
       var x = o is (int
                     or long);
       return x;
   }
   ```
   Today (stable over two passes):
   ```csharp
   bool M(object o)
   {
       var x = o is
                 (
       int
       or long
                 );

       return x;
   }
   ```
   In a case label, `case (int⏎or long):` becomes `case⏎(⏎int⏎or long⏎):` with every line at the column of `case`. Looks wrong (contents left of their delimiters); no spec decision found. Candidate `agent-filed` bug/open question for the line-break and indentation phases.
2. **`return`-/`throw`-only section with a multi-line label** — FMT-SWITCH-004 braces the whole switch for it and writes `case Point⏎{ … }:⏎{⏎return;⏎}`, though such a section has no body to scope either (FMT-SWITCH-006's reasoning). Spec-decided today; low-priority open question whether the -006 exemption should extend to sections whose only statement is a terminal `return`/`throw`.

## thoenissen/Reihitsu#1239

### Premise

The behavior reproduces; whether it is a bug depends on OQ2. Reached by whole-document formatting only (the region phase is skipped by the node-level entry points). Code: `RegionNamingRewriter.BuildRegionTrivia`/`BuildUndescribedEndRegionTrivia` and `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia` rebuild only from the keyword on and never touch the `#` token's trivia, so the gap after `#` survives on every paired region line. Today:

| Input | Output |
|---|---|
| `# region fields` / `#endregion` | `# region Fields` / `#endregion // Fields` |
| `# region Fields` / `# endregion // Fields` | unchanged |
| `#→region Fields` (tab) | `#    region Fields` (FMT-TAB-002) |
| `# region` / `#  endregion  note` (undescribed) | `# region` / `#  endregion note` |
| in `#if false`: `# region fields` / `#  endregion` | `# region Fields` / `#  endregion // Fields` |
| `# pragma warning disable CS0168`, `# if DEBUG`, `# endif` | gap kept (moved to column 0) |

Stale parts of the issue: it cites FMT-REGION-001 (now FMT-REGION-007), and "a line the formatter does not rewrite, such as `#  region Fields`, is left unchanged" no longer distinguishes anything — FMT-REGION-007 writes every paired described line, and the gap is kept whether or not the name changes.

The RH7302 argument is wrong as a reason. `scripts/apply-fix.sh RH7302` reports `# region Fields` (no fix offered) and not `#region Fields` — but that is RH7302's own defect: it reads the description at a fixed offset (`ParentTrivia.ToString().AsSpan().Slice(7)` reads `n Fields`). The maintainer decided in #1174 that `# region Fields` is a region directive and is not reported; the work is tracked in #1444 (moved there from #1079), and the RH7302 spec scopes RH7302-001/-002 to lines without a gap after `#`. So FC-013 does not decide between A and B: after #1444 both outputs are analyzer-clean.

### Mechanism and candidates

_N/A_ until OQ2 is answered. If A: the region phase's builders keep the `#` token's trailing whitespace. Candidates: `#region` with description, `#region` without, `#endregion` behind a described region (FMT-REGION-002), `#endregion` behind an undescribed one (FMT-REGION-004, bare or with text).

### Breaking inputs

Only under A. FC-020 (disabled `#if false` branch, FMT-REGION-006), FC-003 (a line canonical except the gap must now be rewritten; a canonical line stays untouched), unpaired region → whole file unchanged (FMT-PIPE-005). Matrix: directive {`#region` described, `#region` bare, `#endregion` described pair, `#endregion` bare, `#endregion` with text} × gap {one space, several spaces, tab → four spaces} × branch {active, disabled}. The node-level test `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncReplacesTabBetweenHashAndRegionKeyword` (expects `#    region bar` kept) stays valid under A because node-level entry points skip the region phase.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter `RegionFormattingPhase` builders | must change on A; must stay on B |
| `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia` (Core, also called by the RH7301 code fix) | on A, a change there also changes the RH7301 fix's output — a visible surface; the lead should keep the change inside the formatter or include it in OQ2 |
| RH7302 analyzer | not involved (#1444) |
| Other directives (`# pragma`, `# if`) | not involved (follow-up) |
| Spec FMT-REGION-002/-004/-007 | clarified after OQ2 |

### Simpler option

On B, no code change: `Refs #1239`, a characterization test, and a sentence keeping the gap.

### Output questions

OQ2 of `01-spec-author.md` is a genuine output question (two readings of FMT-REGION-007 with different output). Two corrections to how it should be asked:

1. Its "Why A" must not cite RH7302 (see *Premise*). A sound reason for A: the region phase owns "the text of every paired `#region` and `#endregion`" (`AGENTS.md`, phase table) and FMT-REGION-007's own Why asks for one form for every region line; under B a described region line has two forms. A reason for B: every other directive keeps its gap after `#`, so B is consistent across directives. I still recommend **A**, with the first reason.
2. Show what A also changes, so the answer covers it: the undescribed case (`# region` → `#region`, `#  endregion  note` → `#endregion note`, which FMT-REGION-004's "bare keyword" reading leaves open too) and the tab case (`#→region Fields` → today `#    region Fields`, under A `#region Fields`).

### Internal choices

_None._

### Issue goals

- Expected output `#region Fields` — depends on OQ2; under B the goal is dropped and the issue gets `Refs #1239`.
- "The maintainer confirms whether FMT-REGION-007 covers the gap … or a general policy" — this is OQ2 itself; the general-policy reading widens the run to a behavior change and is not recommended in this run.

### Architecture findings

_None._

### Follow-up candidates

- (Only on OQ2 = A) whitespace between `#` and the keyword of other directives (`# pragma`, `# if`, `# endif`) — already listed by the spec author.

## Whole run

No build or test run beyond the format/apply-fix probes above.
