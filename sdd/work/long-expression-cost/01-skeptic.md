Role: skeptic
Commit: d55885c
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's third comment lists candidate causes; they were treated as hypotheses and checked by measurement below, not taken as given.

## Premise

Partly real. After the fix for thoenissen/Reihitsu#1862 (a8ea6f3, an ancestor of d55885c), two of the issue's four shapes no longer grow super-linearly; two still do, and the list case is wider than the issue states.

**Reachability.** The CLI reaches every case: `reihitsu-format <file>` → `FormatCommandHandler.ReadFileAsync` → `ReihitsuFormatter.FormatSyntaxTree` → `FormattingPipeline.ExecutePhases` (frames seen in the sampled stacks below). Whole-document formatting is the same path. Code fixes format one node and are not the reported case.

**How measured.** Release build (`scripts/build.sh --no-install`), then the built CLI run directly in check mode, so nothing is written: `dotnet src/Reihitsu.Cli/bin/Release/net11.0/Reihitsu.Cli.dll --check <dir>/C.cs`. The times are wall-clock for one process, start-up included (an empty class takes 0.19 s). Every run printed `1 of 1 file(s) need formatting.` with exit 1, so the file was formatted and not refused. For comparison, the same CLI was built at `a8ea6f3^` (6d574e1, before #1862's fix) in a temporary worktree, which has since been removed.

| shape (generator kind) | n | before #1862 fix | d55885c | growth per doubling at d55885c |
|---|---|---|---|---|
| fluent chain in `return`, one link per line (`chain`) | 400 / 800 | 9.6 s / 60.6 s | 0.52 s / 0.83 s | linear: 1600 1.31 s, 3200 2.03 s |
| fluent chain in a local declaration (`chainlocal`) | 800 / 1600 / 3200 | – | 0.93 / 1.44 / 2.18 s | linear |
| right-nested `?:` in `return`, one branch per line (`ternary`) | 400 / 800 | 5.0 s / 41.2 s | 0.70 s / 1.15 s | **1600 2.87 s, 3200 9.26 s, 6400 33.08 s: about 3.5x** |
| right-nested `?:` in a field initializer (`ternfield`) | 800 / 1600 / 3200 | – | 1.40 / 3.07 / 9.44 s | **about 3x** |
| left-nested `+` concatenation in a local declaration (`concat`) | 2000 / 4000 | 23.1 s / 173 s | 0.57 s / 0.93 s | linear: 8000 1.46 s |
| concatenation in a field initializer (`concatfield`) | 800 / 1600 / 3200 | – | 0.35 / 0.47 / 0.86 s | linear |
| single-line array initializer `new[] { 0, 1, ... }` (`array`) | 4000 / 8000 | 4.9 s / 18.5 s | 3.32 s / 10.88 s | **16000 43.53 s: about 4x** |

So the issue's chain and concatenation rows were the #1862 cost and are fixed; their goals are met on `main` already. The ternary row and the array row still reproduce, as quadratic growth.

## Mechanism and candidates

Two defects with different mechanisms.

### A. Nested conditionals (line-break phase)

**Mechanism.** `TernaryLineBreakRewriter` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs`) visits every conditional expression bottom-up (`VisitConditionalExpression`, line 216), and at every conditional whose `WhenTrue` or `WhenFalse` is directly a `ConditionalExpressionSyntax` it calls `BreakOperatorsOntoOwnLines`, which recurses through the whole directly nested chain below (lines 89-104), rebuilding it with `With*` calls and querying `IsMultiLine` / `HasLeadingEndOfLine` on rewritten nodes. Each level therefore does work proportional to the depth below it, which is quadratic in the depth of a chain of directly nested conditionals.

**Evidence.** `dotnet-stack report` sampled during the `ternary` 6400 run (86 formatter samples, installed with `dotnet tool install dotnet-stack --tool-path <scratch>`): 82 of 86 samples lie under `TernaryLineBreakRewriter`, with innermost frames `BreakOperatorsOntoOwnLines` (23 BulkMove + 3), `NormalizeColonTokenPosition` (15), `HasLeadingEndOfLine` (14), and `GreenNode.WriteTo` reached from `NormalizeTernaryOperatorPosition` (18 + 3; text serialization behind `LineBreakDetection.IsMultiLine` on a rewritten node).

**Candidates (dispatch: `CSharpSyntaxRewriter.VisitConditionalExpression`; nesting axes are the two `ConditionalExpressionSyntax` slots the recursion follows).** All measured at d55885c:

| nesting | input layout | 800 | 1600 | 3200 | fails |
|---|---|---|---|---|---|
| in `WhenFalse`, right-nested | one branch per line | 1.15 s | 2.87 s | 9.26 s | yes |
| in `WhenFalse` | all on one line (`ternsingleline`) | 1.35 s | 2.82 s | 9.14 s | yes |
| in `WhenTrue` (`k > 0 ? k > 1 ? ... 0 : 0 : 1 ...`, `ternwhentrue`) | one line | 1.39 s | 3.03 s | 10.44 s | yes |
| in `WhenFalse` but parenthesized (`k == 0 ? 0 : (k == 1 ? 1 : (...))`, `ternparen`) | one line | 0.58 s | 0.88 s | (400: 0.42 s) | no, linear |
| statement position: `return` vs field initializer | one per line | see Premise table | | | yes, both |

The parenthesized member holds because a `ParenthesizedExpressionSyntax` stops the recursion; it is the boundary of the class, not a different mechanism.

### B. Long sibling lists (five phases, per-token navigation)

**Mechanism.** Several phases ask Roslyn, for every token (or every comment) of the tree, for its previous or next token (`SyntaxToken.GetPreviousToken` / `GetNextToken`, directly or through `SyntaxTokenPositionUtilities.IsFirstOnLine` / `StartsLine` and `PrecedingTokenFacts.From`), and the measured cost of each such call grows with the number of siblings in the token's parent, so a walk over a parent with N children costs quadratic time in N. It depends on the sibling count, not on line count or nesting.

**Evidence.** Sampled during `array` 16000 (105 formatter samples): by phase, indentation 40, blank lines 23, cleanup 21, line breaks 11, horizontal spacing 9, structural transforms 1; precheck 0. Innermost frame `SyntaxNavigator.GetPreviousToken` in 37 samples and `GetNextToken` in 9, more than any other. Their nearest Reihitsu callers: `CleanupPhase.CleanToken` (9), `BlankLineTokenCleanupRewriter.VisitToken` (7), `BlankLineTriviaBoundaryRewriter.VisitToken` (6), `LayoutComputer.ComputeBlockIndentation` (6), `LineStartIndex..ctor` (4, plus 2 via `SyntaxTokenPositionUtilities.StartsLine`), `CommentIndentationContributor.AlignCommentsBeforeToken` (4), `LayoutComputer.SetIfFirstOnLine` from `ObjectInitializerContributor.AlignInitializerToColumn` (3+1), `ListElementInteriorJoinRewriter.CanJoin` / `SelectJoinableGaps` (4), `HorizontalSpacingRewriter.VisitToken` (2). `PrecedingTokenFacts.From` and `ListElementInteriorJoinRewriter.CanJoin` also appear under `BulkMoveWithWriteBarrier`. The same frames lead the `enums` 8000 sample (54 samples; `GetPreviousToken` innermost in 19; `MemberLineStartRewriter`, `BlankLineCollapser` and `ComputeBlockIndentation` among the callers in `fields` 8000). The `LineBreakListRewriter` replacement set the third issue comment names did not appear in any sample. `rg -c "GetPreviousToken\(|GetNextToken\("` counts 86 call sites over 40 files in `src/Reihitsu.Formatter` and `src/Reihitsu.Core`; the samples implicate the dozen above.

**Candidates (dispatch: the parent kinds whose child list can be long; the axis is the parent's child count).** All at d55885c:

| parent with many children | defining type | 4000 | 8000 | fails |
|---|---|---|---|---|
| array initializer, one line (`array`) | `InitializerExpressionSyntax` | 3.32 s | 10.88 s (16000: 43.53 s) | yes |
| array initializer, 16 elements per line (`arraywrapped`) | `InitializerExpressionSyntax` | 3.57 s | 12.03 s | yes |
| collection expression `[0, 1, ...]` (`collexpr`) | `CollectionExpressionSyntax` | 3.56 s | 10.78 s | yes |
| argument list `M(0, 1, ...)` (`args`) | `ArgumentListSyntax` | 3.67 s | 13.40 s | yes |
| enum members, one per line (`enums`) | `EnumDeclarationSyntax` | 5.89 s | 23.64 s | yes |
| class with N fields, blank line between (`fields`) | `ClassDeclarationSyntax` members | 3.46 s | 10.73 s | yes |
| block with N statements (`stmts`) | `BlockSyntax` | 1.82 s | 4.51 s | milder (about 2.5x) |

For scale: an array of 1000 elements takes 0.78 s and of 2000 1.25 s, start-up included, so the cost only matters from several thousand siblings.

## Breaking inputs

No catalog class covers cost. The classes that reach a change in these code paths:

- **FC-016** — sibling shapes keep the hazard: defect B has a dozen call sites across five phases; fixing one leaves the curve quadratic (each phase alone is 9-40 % of the time). The test must cover the list kinds of the candidate table, not only the array.
- **FC-027** — only one nesting form considered: defect A's nesting in `WhenTrue`, in `WhenFalse`, on one line and one per line; the parenthesized form is the boundary.
- **FC-010 / FC-017** — output must stay byte-identical: check both passes, and under CRLF as well as LF, for every candidate shape at a small size, against `main`'s output.
- **FC-004 / FC-021** — any change to how the ternary rewriter or a navigating phase reads trivia must keep comments: end-of-line comments after a branch, comments on the `?`/`:` lines that are not in a span FMT-UNSUP-008 declares unsupported, comments between list elements, `#if` inside a list.
- **FC-033 / FC-034** — the reproduction must use the reported shape (a deep directly nested conditional; a long flat list), not a smaller cousin that never shows the growth.

**Fixture generators** (Python 3, run with `python3 -I gen.py <outdir> kind:n ...`; each writes `<outdir>/<kind><n>/C.cs`). Wrapper for every kind except `enums`: `"namespace N;\n\ninternal class C\n{\n" + body + "}\n"`.

```python
# ternary: return k == 0 ? 0 \n : k == 1 ? 1 ... \n : -1;  (one ": k == i ? i" per line, 15-space indent)
br = "\n".join(f"               : k == {i} ? {i}" for i in range(1, n))
body = f"    internal int M(int k)\n    {{\n        return k == 0 ? 0\n{br}\n               : -1;\n    }}\n"
# ternsingleline: return <e>;
e = " : ".join(f"k == {i} ? {i}" for i in range(n)) + " : -1"
# ternwhentrue: return <e>;
e = "".join(f"k > {i} ? " for i in range(n)) + "0" + "".join(f" : {i}" for i in range(n))
# ternparen (holds): return <e>;
e = "".join(f"k == {i} ? {i} : (" for i in range(n)) + "-1" + ")" * n
# array: one line
body = "    internal int[] M()\n    {\n        var a = new[] { " + ", ".join(map(str, range(n))) + " };\n        return a;\n    }\n"
# collexpr: int[] a = [0, 1, ...];   args: M(0, 1, ...); inside "internal void M(params int[] p)"
# enums: "namespace N;\n\ninternal enum E\n{\n" + ",\n".join(f"    V{i}" for i in range(n)) + "\n}\n"
# fields: "".join(f"    private int _f{i};\n\n" for i in range(n))
# chain (holds): return items \n .Where(x => x > i) per line \n .ToList();   concat (holds): var s = "x" \n + "p<i>" per line;
```

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, line-break phase, `TernaryLineBreakRewriter` | must change for defect A; output must stay identical |
| Formatter, indentation, blank-line, cleanup, line-break, horizontal-spacing phases, and `Reihitsu.Core` navigation helpers (`SyntaxTokenPositionUtilities`) | must change for defect B if it is fixed; output must stay identical |
| Precheck | not involved (no samples; its comma cost was fixed separately) |
| Analyzers | not involved; `Reihitsu.Core` helpers they share must keep their results |
| Code fixes, Fix All | not involved; must stay (they format one node) |
| CLI | not involved for this issue (see the follow-up below) |
| Specs | not involved: no spec states a cost requirement, so no requirement is added or changed (bug-fix lane without a change doc holds) |
| Rule docs | not involved |
| Tests | a reproduction per defect; the self-hosting run proves the "output unchanged on the repository's own sources" goal |

## Refuse option

- **Refuse = leave the cost as it is and record it**, which the issue itself allows ("a phase whose cost cannot be reduced with a cheap check stays as it is"). Both input classes are rare: thousands of directly nested conditionals, or one parent with thousands of children (data tables, generated code). Refusing costs CLI time on such files only: about 3 s at 4000 siblings, 11 s at 8000, 44 s at 16000; about 9 s for a 3200-deep conditional chain. No output is wrong.
- **Requested change, defect A:** confined to one rewriter in one phase, with a clear boundary (direct nesting). Cheap relative to refusing.
- **Requested change, defect B:** spread over about a dozen call sites in five phases and a `Reihitsu.Core` helper; removing the quadratic term needs every implicated site in all five phases, or the curve stays quadratic (FC-016). If it needs a new shared type or more than about 100 production lines, it is escalation 9, for size that only a rare class needs (`AGENTS.md`, *Special cases do not grow the source*). Refusing (recording) is the fallback.
- **Declaring a shape unsupported:** not an option. The shape would be "a list longer than N" or "a conditional nested deeper than N"; it deletes no special case (no guard serves these inputs today), it refuses code people write for data tables, and it is not a comment, directive, or trivia shape the precheck checks today. A shape that deletes nothing is not declared.
- **Simpler:** close the chain and concatenation rows as already fixed by #1862 (measured above); no change needed for them.

## Output questions

_None._ Every change must leave formatter output byte-identical; no visible output is in question.

## Internal choices

1. **Scope of defect B.** (a) fix every implicated navigation site in the five phases; (b) fix defect A only and record B's cost per phase with the reason it stays; (c) fix B at the sites with the largest shares (cleanup, blank lines, indentation) only. (c) leaves the growth quadratic, so it fails the issue's "linear" goal and is FC-016. Recommendation: (a) when it stays within escalation 9's bounds, else (b); the lead decides once the implementer has sized it. Decide (a) or (b) before the test implementer writes a reproduction for B.
2. **How to reproduce a cost defect without a flaky test.** An absolute wall-clock budget fails under CI contention (the same reasoning `AGENTS.md` gives for `Regex` timeouts). Options: a ratio assertion (time at 2n vs n under some bound), or an assertion on a deterministic count where one exists. Recommendation: the test implementer picks a form that cannot fail on a loaded machine and states why; byte-identical output for the candidate shapes is asserted separately.

## Issue goals

- **Chain and concatenation rows:** drop as already met; measured linear at d55885c (table above). No visible output change.
- **"The slowest phase for each input above is identified":** keep, restated per defect: line-break phase (`TernaryLineBreakRewriter`) for nested conditionals; for long lists no single phase, the cost is spread over indentation (about 38 %), blank lines (22 %), cleanup (20 %), line breaks (10 %), and horizontal spacing (9 %) through per-token navigation. No visible output change.
- **Widen the list row:** the issue names only the single-line array initializer; the same growth holds for every long sibling list (table above). Keep the goal for all of them or record all of them. No visible output change.
- **"Formatter output unchanged on the repository's own sources":** keep; the self-hosting test project proves it.
- **"Make it linear where a cheap change allows" / "or the remaining cost is stated per phase":** keep as written; it already contains the refuse option.

## Architecture findings

_None._ Defect B repeats one cost pattern across phases, but no two components own the same decision, nothing predicts another component's result, and no rule is stated twice; it is a performance pattern, not one of the structural causes.

## Follow-up candidates

1. **A deep spine crashes the whole CLI process (outside this issue).** `LayoutComputer.ComputeBlockIndentation` (`src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs`) recurses once per nesting level without an execution-stack guard (`rg EnsureSufficientExecutionStack src` finds none), so a deep enough spine overflows the stack, which .NET cannot catch. Observed at d55885c: `chain` 6400 links and `concat` 16000 terms abort with `Stack overflow.` and exit 134 (`Repeated 8004 times` / `Repeated 4923 times: at ...LayoutComputer.ComputeBlockIndentation`). In a run over a directory holding `A.cs` (a 100-link chain) and `Deep.cs` (the 6400-link chain), `--max-parallelism 1 --verbose` wrote `A.cs`, then aborted with exit 134, printed no summary, and would have left every later file unprocessed. A deeper input that overflows Roslyn's own guarded recursion (`concat` 32000) is instead reported per file, `Error processing <path>: Insufficient stack to continue executing the program safely...`, with `1 file(s) encountered errors.` and exit 2. Rare input; CLI behavior is visible, so the issue carries it as a question:

   **CLI · a file whose expression nesting overflows the formatter's stack**

   Input: a file holding a fluent chain of 6400 links (generator `chain:6400` above), in a run together with other files.

   Before (`main`): the process aborts with `Stack overflow.`, exit code 134, no summary; files after it are not processed.

   After, A (recommended): the file is reported as `Error processing <path>: ...`, the other files are processed, and the run ends with `1 file(s) encountered errors.` and exit code 2, as for the 32000-term concatenation today.

   After, B: unchanged.

   Why A: one rare file should not cost the user the whole run, and A matches how the CLI already reports a deeper input.
