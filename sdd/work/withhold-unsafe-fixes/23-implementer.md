Role: implementer
Commit: 22893378
Verdict: tests pass

Repair round 3 (the last), lead rows 36–39, auditor findings 10–13, the spec diff of 7a955713, and the tests of `22-test-implementer.md`. My prompt restated the decided rows and the auditor's suggested repairs. It carried no other intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside. No output questions raised (standing instruction).

A previous implementer of this round was lost before writing a handover. It left unverified working-tree changes in `stash@{0}`. I reviewed them line by line against RH3004-006/-007 and the row 36/37 text, applied them, and verified them. I added one change of my own: a local function's expression body is now bound with its statement (see below). Then I dropped the stash.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RewriteBindingUtilities.cs` `BindsAsBefore`, `CountErrors` | Errors are counted only on the path from the replaced expression to the bound node. The operation walk skips every child operation whose syntax is a statement that is not on the path, such as later statements of a block or of an anonymous function's body, and everything inside it. The conversion check and the symbol comparison on the path are unchanged. | Row 36; RH3106-003 (finding 10) |
| same file, `GetBindingRoot` | The bound node is the smallest one that can decide: the innermost statement other than a block, or an initializer or expression body outside every statement, or a constructor initializer or primary-constructor base type. It is taken around the outermost anonymous function that encloses the expression, or around the expression itself when no anonymous function encloses it. A local's initializer is bound with its declaration statement, which gives `var` its type. My own addition, beyond the stash: a local function's expression body is bound with its `LocalFunctionStatement`, not as an `ArrowExpressionClauseSyntax` inside a method body. The old outermost search never bound it that way, and I did not want the round to depend on how the speculative arrow-clause API handles local functions. | Row 37 (finding 11) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `RegisterCodeFixesAsync`, new `GetExpressionBody` | The multi-line raw-string guard applies only when the fix keeps a block body. `GetExpressionBody` is the one decision on whether the fix writes an expression body, used by both registration and `ApplyCodeFixAsync`. | RH3004-007 (row 38, finding 12) |
| same file, `MoveStatementsAsync`, `GetFirstStatementLinePosition`, `GetMovingLineColumns`, new `GetLineElements`/`AddLineElements`/`IsCode`/`IsMovingLine`; removed `StaysInColumnZero`, `KeepsColumns`, `IsDisabledStatementLine` | One rule replaces the per-kind special cases. The block's elements are its tokens, its comments, its directives, and the tokens and comments of its disabled text, parsed in place. `IsMovingLine` gives every line one outcome, decided on these elements: a line on which code starts moves; a line that starts inside a token begun earlier stays (verbatim literal); a column-0 non-region directive stays; a code-free line with its first comment in column 0 stays; a code-free continuation of a comment follows the line on which the comment starts. The shift is measured on the first line on which code (active or disabled) starts, from that line's first character. The leftmost-line rules count only moving lines. | RH3004-006 (row 39, finding 13) |

## Policy owners

- Rebinding a rewrite or a probe: `rg -n "TryGetSpeculativeSemanticModel\(" src` (production) finds only `RewriteBindingUtilities`. One owner; both RH3106 consumers decide through it.
- Lines that move or stay in RH3004's moved code: `rg -n "StaysInColumnZero|KeepsColumns|IsMovingLine|IsDisabledStatementLine" src` finds only `IsMovingLine` and its three callers in the RH3004 provider. The three earlier per-kind predicates are gone.
- Raw-string lines: `rg -ln MultiLineRawStringLiteralToken src` (production) finds the formatter's `RawStringLineShifts`, RH5202, RH5601, and RH3004. RH3004 only checks for presence on the block-body path. The expression-body path leaves the raw string to the formatter.

## Row 37 measurements

The auditor's fixture is `scratchpad/perf/perf.cs`: `Run(delegate () { Console.WriteLine(i); });` × n with `Run(Action)` and `Run(Action<int>)`, analyzer only, through `CompilationWithAnalyzers`. I ran it five times per shape in one process on 5ace63e2 (before) and on this change (after), in clean worktrees. Each cell gives the range of the five runs and the minimum, in ms:

| n | before, unwrapped | before, in one `try` | after, unwrapped | after, in one `try` |
|---|---|---|---|---|
| 100 | 80–132 (80) | 354–1366 (354) | 81–139 (81) | 96–141 (96) |
| 200 | 23–27 (23) | 1277–1386 (1277) | 92–171 (92) | 61–78 (61) |
| 400 | 36–46 (36) | 5633–6065 (5633) | 99–144 (99) | 67–156 (67) |

The quadratic growth inside `try` is gone. The wrapped and unwrapped costs are now the same.

The lower "before, unwrapped" numbers at n = 200/400 come from JIT warm-up. The wrapped runs of the same process spend seconds in the same code first. I ran a direct micro-benchmark of `BindsAsBefore` over the 400 unwrapped nodes, unwrapped only, eight repetitions. Both trees converge to the same cost:
- before: 933 → 109–130 ms;
- after: 790 → 96–118 ms.

Swapping the old whole-tree error count into the new root also changed nothing, so the path walk adds no measurable cost.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
#   Passed! - Failed: 0, Passed: 141, Total: 141   (the 16 analyzer tests of 22-test-implementer.md that failed now pass)
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~RewriteBindingUtilitiesTests"
#   Passed! - Failed: 0, Passed: 25, Total: 25     (the 3 Core tests that failed now pass)
scripts/test.sh --configuration Debug --project analyzer      # Passed: 6923, Failed: 0
scripts/test.sh --configuration Debug --project core          # Passed: 646, Failed: 0
scripts/test.sh --configuration Debug --project architecture  # Passed: 115, Failed: 0
scripts/build.sh                                              # Release: 0 Warning(s), 0 Error(s)
scripts/format.sh <both changed files>                        # Formatted 0 of 2 file(s)
```

I changed no test. Extra probes with `scripts/apply-fix.sh`, LF and CRLF, fixtures in the scratchpad:
- RH3004, the disabled-text variant the test implementer left open: `/* note */ Console.WriteLine(x);` in column 0 inside `#if NEVER`. Fixed at 28/40, the same as the active-code example.
- RH3106, `void L() => Run(delegate () { });` and `Action P => delegate () { };`: reported and fixed.
- RH3106, `var a = delegate () { }; a();` in a local-function body, and `var b = …; Action c = b;` in an `if`: not reported.

## Disputed tests or scope

_None._ Both changed files are the ones rows 36–39 name. No new public member: `GetExpressionBody` and the line-element helpers are private.

## Architecture findings

_None new._ Rows 36 and 39 are carried out as decided:
- The binding decision stays in one place, with its scope corrected to the path.
- The RH3004 layout has one outcome per line, and the measured line is by construction a moving line: it is the first line on which code starts, and such a line always moves.

## Follow-up candidates

- The auditor's earlier candidate on a shared raw-string helper stands. Nothing changed for it.
- A line that starts inside an interpolation hole of a multi-line interpolated verbatim string moves, because code starts on it. A line that starts in the string's text stays. This matches the `KeepsColumns` behavior before this round, and whitespace in a hole does not change the value. It is noted only because RH3004-006 words the case as "starts inside a verbatim string literal".
