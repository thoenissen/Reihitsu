Role: auditor
Commit: 1287283bf82baf357fa85e624a1c723c5a1d9c4d
Verdict: pass, text cleanup

## Scope

Repair delta against 41d705395bdaa3bb93722bc8df41bf17a495de0b (audit 11), plus the merge of `origin/main`. Head 1287283 as named in my prompt; the checkout is c6f9c16, whose only change on top of 1287283 is `00-lead.md` (run state), so the audited tree is 1287283's. `origin/main` and `git ls-remote origin refs/heads/main` are both 1102aa52fe204b2c5b423d14ccd697a433466e13, an ancestor of head (merged in 773d38a). `scripts/lane-check.sh behavior-change`: confirmed. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The repair is what 12–16 describe: `git diff 41d7053 e23344a` without `sdd/work/` touches only `sdd/specs/formatter/line-breaks.md` (FMT-BREAK-039 second bullet, *Why*, one new example), `sdd/changes/2026-11/argument-list-below-name.md` (*What*, change 3, new change 7, *Decisions*, *Alternatives*), `ArgumentListLineStartTests.cs` (7 tests), and the RH5302 analyzer tests (1 test). No production code changed in the repair, and the scope did not grow.

Merge: the run's diff is line-for-line the same before and after the merge (every file of `git diff eb89003 e23344a` has the same added and removed lines as in `git diff 1102aa5 1287283`), so the conflict resolution in 14 kept both sides' tests and changed none. Main's new commits (#1809 field split behind a region and directives in multi-declarator fields, #1810/#1811 tooling and trace, #1812 comment behind an opening token, #1814 test.sh, #1815 parallel CLI) touch none of the run's production files (`ArgumentListLineStartRewriter`, `LineBreakPhase`, `ChainLineBreakRewriter`, `ListElementInteriorJoinRewriter`) and none of the shared predicates the join reads; checked interactions are under *Failure classes dismissed*.

Evidence: `scripts/build.sh` on the branch (Release, succeeded) and in a worktree of 1102aa5 in the scratchpad; both CLIs over a generated corpus of 190 fixtures in two variants each — **A** with a line break in front of the argument list's `(`, **B** the same input with that line break removed — outputs compared; a second branch pass over every A output (`Formatted 0 of 190`), and the 44 invocation fixtures again under CRLF (identical to LF after stripping `\r`, second pass `Formatted 0 of 44`). Corpus axes: outer list (invocation first/second argument, object creation, implicit object creation, element access, tuple, nested invocation, chain invocation, constructor initializer, attribute argument list, parameter default value) × nested construct (interpolation hole: regular, verbatim, raw, with format, with alignment, nested hole, conditional access, object creation, element access, switch expression; query clause: first, `where`, `select`, `orderby`, `let`, `group … by`, `join … on`; array initializer implicit and typed, implicit-object array element, `stackalloc`; object, collection, dictionary, anonymous-object initializers; `with` expression; lambda expression and block bodies; switch expression; conditional; parenthesized; nested argument list; element access; cast; `checked`; binary; `throw`; `await`; `is` pattern; directly in the element; collection expression) × chain/non-chain.

## Previous findings

| # | Status | Why |
|---|---|---|
| 1 | closed | The undecided output was asked as OQ 5 and answered 5A (`00-lead.md`); change 7 and the *Decisions* entry record it; the false "does not change" statements and FMT-BREAK-039's precedent sentence are corrected (remaining residue is finding 2, text only). Every member finding 1 listed now has a decided output and is locked by a test: chain and non-chain hole (`RequirementExample…`, `ChainArgumentListInInterpolationHoleOfLaterArgument…`), query clause (`ChainArgumentListInQueryClause…`, example), array initializer (`ArgumentListInArrayInitializer…`, example), `stackalloc` (`ChainArgumentListInStackallocInitializer…`), second argument, and the code-fix consequence (RH5302). The change doc's *before* of change 7 still matches main 1102aa5's output |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `sdd/specs/formatter/line-breaks.md` FMT-BREAK-039 *Why*, the sentence "A list is judged so wherever the argument list stands in an element, so whether a joined argument list was written below its name never decides the list's layout, also where it stands inside an interpolation hole, a query clause, or an initializer of the element."; `sdd/changes/2026-11/argument-list-below-name.md` line 13 ("…inside an interpolation hole, a query clause, or an array…") and the heading of change 7 ("…a query clause, or an array initializer…") | FC-031 (text describing behavior that does not hold) | – | same requirement | The *Why* claims an invariant the requirement's own first bullet and the run's test `CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedArgumentList` contradict: `Baz([Bar⏎(1)], 2);` gives `Baz([⏎        Bar(1)⏎    ],⏎    2);` while `Baz([Bar(1)], 2);` stays one line, so where the argument list was written does decide the list's layout through a collection expression nested in the element (the only such member in the corpus; every other member of the 190 has identical A and B output). The change doc says "a query clause" where FMT-BREAK-039 and the change doc's line 9 correctly say "a query clause after the first": an argument list in the first clause was already joined before the list on `main` (`Baz(from w in list.Skip⏎(1) select w, 2)` → one line on `main` and branch), so it is not part of change 7. Text only: neither reaches compiled behavior, diagnostics, or rule metadata, and the bullet the tests follow is correct | In the *Why*, bound "never decides" to the list itself (for example: "…the line break the join removes never counts for the list itself; it reaches the list only through a construct of the first bullet nested in the element"). In the change doc, write "a query clause after the first" in line 13 and in change 7's heading |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption; the merge changed none of the guards audit 11 traced (`ArgumentListLineStartRewriter.CanJoin`, `IsArgumentListOpenParenthesis`, `ListElementInteriorJoinRewriter.IsOwnedGap`, `ChainLineBreakRewriter.NormalizeChain`), and the shared blocking predicate `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` is unchanged by main (main only added `IsContentOpeningToken` beside it). Audit 11's table stands byte-identical._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Join of the gap in front of an argument list's `(` | Unchanged since audit 11: `ArgumentListLineStartRewriter` owns it; `ChainLineBreakRewriter.NormalizeChain` and `ListElementInteriorJoinRewriter.IsOwnedGap` defer. Main's merged code adds no owner (`git diff eb89003 1102aa5 -- src/Reihitsu.Formatter src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes`: precheck, field split, blank-line rewriters, `PrecedingTokenFacts`, `BlockStartUtilities`, `SyntaxTriviaUtilities.IsContentOpeningToken`, RH7101 fix — none touches that gap) | one owner |
| Whether a comment behind an opening token gets a blank line (main #1812) | `SyntaxTriviaUtilities.IsContentOpeningToken` → `PrecedingTokenFacts.OpensContent` → `BlockStartUtilities.IsFirstBehindOpeningToken`, `BlankLineTokenCleanupRewriter` | not this run's policy; it reads the token in front of a comment behind `(`, never the gap in front of `(`, and no argument list's `(` has an opening token in front of it |
| Statement of the list layout around a joined argument list in the specs | FMT-BREAK-039 second bullet (owner); change doc *What*, change 7, *Decisions* restate it for the change | one statement; its *Why* overstates it (finding 2) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RequirementExampleJudgesListAsTheLineItBecomesAroundArgumentListInNestedConstruct` | FMT-BREAK-039's new example verbatim: chain and non-chain hole, query clause, array initializer each one line | A list keeping one argument per line, a kept gap, or a second pass changing it | `AssertRuleResult` (exact, LF and CRLF, second pass, per-phase stability) | holds; fails on `main` per 13 |
| `ChainArgumentListInInterpolationHoleOfLaterArgumentLeavesListOnOneLine`, `ChainArgumentListInQueryClauseLeavesListOnOneLine`, `ArgumentListInArrayInitializerLeavesListOnOneLine`, `ChainArgumentListInStackallocInitializerLeavesListOnOneLine` | One line for the member each names (element position, construct, chain arm) | As above | `AssertRuleResult` | holds; each matches my corpus cell and `main`'s output for the input written without the gap |
| `ListWithLineBreakBetweenArgumentsKeepsOneArgumentPerLineAroundJoinedInterpolationHole` | The other side of "only line break" (FC-003): an author's break between arguments still gives one argument per line | The list collapsing to one line | `AssertRuleResult` | holds |
| `CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedArgumentList` | The bullet's stated other side (FC-002): a nested collection expression is judged as written | The collection or the list becoming one line | `AssertRuleResult` | holds. 13 says every such expected value is `main`'s output for the input without the gap; for this test that derivation gives one line, so it cannot be how the value was derived. The value is still independent of the patched code: `main` 1102aa5 gives exactly the expected output for `Baz([Bar(1),⏎2], 3);` (a collection spanning lines by an author's break), which FMT-BREAK-039's first bullet (answer 1A) makes the target |
| RH5302 `VerifyFixJoinsArgumentListInInterpolationHoleAndKeepsOperandListOnOneLine` | Change 7's code-fix example: the fix's formatted node carries the join and the one-line operand list | `Has($"{x.ToString()}",⏎2)` or a kept gap | `VerifyCodeFixWithEndOfLine` (LF, CRLF; iterative) | holds |

Visible output (check 10): every expected value the repair adds is answer 5A's one-line list, answer 1A's collection expression, or `FMT-BREAK-012`'s unchanged one-argument-per-line list. In the corpus, every branch output differs from `main` only where answers 1A and 5A decide it: for all members but the nested collection expression, the branch output of A equals `main`'s output of B (the same input written without the gap), and the collection expression follows answer 1A. No undecided output.

## Failure classes dismissed

Audit 11's dismissals stand for the unchanged production code. Delta for the repair and the merge:

- FC-002, FC-003 — both sides of the repaired bullet are tested (between-argument break; nested collection expression), and the corpus confirms the boundary: only the collection-expression cells differ between A and B.
- FC-027 — the class of finding 1 enumerated afresh along outer list × nested construct × chain arm (190 cells, *Scope*); every cell holds against FMT-BREAK-039 as repaired, under LF and CRLF, stable on a second pass. No member fails that finding 1's enumeration missed.
- FC-005, FC-038 to FC-043 — main #1809 declares a directive inside a multi-declarator field unsupported (FMT-UNSUP-010). The join never moves a directive (shared blocking predicate), so it cannot produce the shape (FMT-UNSUP-005); `int f = Bar⏎#if DEBUG⏎(1), g = 2;⏎#else …` is refused identically by both CLIs, and `int f = Bar⏎(1), g = 2;` behind a `#region` is split and joined on the branch (`int f = Bar(1);⏎int g = 2;`), the join being answer 1A.
- FC-012, FC-015 — main #1812 decides blank lines in front of a comment behind `(`, a gap the join does not touch: `Baz(Bar⏎(// c⏎1), 2)`, `Baz(Bar⏎(⏎⏎// c⏎1), 2)`, `Baz(Bar⏎⏎(⏎// c⏎1), 2)`, `Baz(⏎// c⏎Bar⏎(1), 2)` give the same output on `main` and branch, stable on a second pass.
- Concurrency (main #1815, parallel formatting) — `LineBreakPhase.Execute` builds its rewriters per call; `ArgumentListLineStartRewriter` holds only a readonly cancellation token and static pure methods.
- FC-035 — expected values match `main`'s output for an independent input (see *Assertion adequacy*), not the patched code.
- FC-031 — the new test summaries describe their tests; no TODO, tracker number, or commented-out code in the repair diff. Finding 2 is spec and change-doc text.
- FC-007, FC-009, FC-030 — code fixes unchanged; RH5302 test covers the fix path iteratively under LF and CRLF.
- FC-013, FC-014 — no rule reports the shape (12, RH5102 checked with `apply-fix.sh`).

## Architecture findings

_None._

## Hints

- `13-test-implementer.md`'s FC-035 derivation sentence does not hold for `CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedArgumentList` (see *Assertion adequacy*); the value is correct and independently derivable, so this is a handover-text inaccuracy, not a test defect.

## Follow-up candidates

_None_ beyond F1–F3 and F5 in `00-lead.md`.
