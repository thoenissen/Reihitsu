Role: skeptic
Commit: a26602c
Verdict: output question

The prompt held no intended fix, suspected cause, or hoped-for verdict. Every observation below comes from `scripts/trace.sh`, `scripts/format.sh`, and `scripts/apply-fix.sh` at a26602c, run on scratch fixtures outside the repository.

## Premise

**Reachability.** Whole-document formatting (the CLI, `FormatDocumentAsync`) reaches both parts. Any file with a parenthesized lambda that starts its own line, or with an anonymous method in a hole of a multi-line interpolated raw string, goes through `LineBreakPhase`, `BlankLinePhase`, and `IndentationPhase`.

**Parenthesized lambdas: real, but the issue understates it.**
- The strip happens in `LineBreakListRewriter.CollapseOpenParenToDeclarationLine` (`VisitParameterList`). The token in front of the `(` always lies outside the `ParameterListSyntax`, so this method only ever strips the `(` (`WithOpenParenToken(newOpenParen)`).
- For every other parameter-list owner, the owner's own visit then joins the gap with `CollapseOwnedOpenParenToDeclarationLine`: the anonymous method, class, struct, interface, record, extension block, and operator in `LineBreakListRewriter`, and the method, constructor, conversion operator, destructor, local function, and delegate in `DeclarationBraceLineBreakRewriter`.
- `ParenthesizedLambdaExpressionSyntax` has no such owner visit, so for a lambda the result is a bare strip.
- The issue says "Output is unchanged; only the strip-and-repaint goes". That is wrong whenever the stripped trivia holds a blank line, or the line has no layout in the indentation phase. A trace of pass 1 on a fixture with a blank line in front of a lambda in each gap shows the line-break phase removing every one of those blank lines. Controls with `(1)` in the same gaps keep them on `main`:

| Gap in front of `(` | Lambda on `main` | `(1)`/`(x)` control on `main` |
|---|---|---|
| after `,` in an argument list | blank line removed | kept (change doc entry 5) |
| between array-initializer elements | blank line removed | kept |
| between collection-expression elements | blank line removed | kept |
| after the `=>` of an expression lambda | blank line removed | kept |
| after `return` | blank line removed | kept |
| after `=` (FMT-BREAK-016) | joined | joined |
| after `(` as first argument (FMT-BREAK-012) | joined | joined |
| after a comment line (`// note`) | untouched (guard) | untouched |

The change doc lists only the first row as a visible output change (entry 5). The new FMT-BREAK-011 text ("where no requirement joins that gap, the phase leaves its line break and its blank lines as written") already decides every row, so the spec is right. The change doc and the output question for entry 5 need to cover the whole class, rows 1–5, and not one member. Entry 6 (a lambda that starts an unplaced hole line drops to column 0) is confirmed by reading `ReleaseUnplacedRawStringHoleLines`: an unplaced line keeps whatever whitespace the line-break phase left.

**Anonymous method in a raw-string hole: real.** I reproduced the column-13 output in a moved literal. `static delegate`, `async delegate`, and `delegate ()` give the same unplaced lines (`static`/`async`/`()` probe, fixture H1). Block lambdas in every position I tried are placed: in a conditional branch, as an initializer value, behind `??`, behind an outer `=>`, with `static`, as a switch-expression arm value, and in a hole's statement list (`if`, `switch`, local function). So are object initializers, `with` initializers, nested conditionals, and anonymous objects. The spec author's claim that the anonymous method is the only unplaced construct holds for every shape I probed. On `main`, the broken anonymous method also forces the list around it onto one element per line: `M(delegate { return 1; }, 2)` gives `},` then `2)` on its own line.

## Mechanism and candidates

Parenthesized lambdas: `VisitParameterList` strips the leading line break, blank lines, and whitespace of every parameter list's `(`. Only owners with a visit that joins the gap afterwards turn that strip into a join. `ParenthesizedLambdaExpressionSyntax` has none, so its `(` keeps only the strip.

- Dispatch types: the `ParameterListSyntax` parents in `ParameterListParentPolicy.IsClosingParenthesisCovered`. `ParenthesizedLambdaExpressionSyntax` is the only one without a joining owner visit.
- Candidate gaps in front of a lambda's `(`: every syntactic predecessor of an expression that can end a line. These are an argument separator, the first argument, an initializer or collection element separator, `=`, `=>` of a lambda, `=>` of a switch-expression arm, `return`, `?`/`:` of a conditional, a binary operator, and a modifier or return type of the lambda itself (`static`, `async`, `int`). An attribute list in front of a lambda, the MinimalApi entry in `PhaseReversalExceptionList`, is not probed here.

Anonymous method in a hole: _N/A_. This part is a behavior change, not a bug.

## Breaking inputs

Failure classes that reach the change:

- **FC-012 / FC-010.** The strip-and-repaint is this class. After the change, the line-break and indentation phases must each be a no-op on the issue's first input, on LF and CRLF. `PhaseReversalExceptionList` (`src/Reihitsu.Formatter.Test/Helpers/PhaseReversalExceptionList.cs`) holds five `LineStartIndentation` entries, and an entry whose reversal disappears fails its own test. The entries this change removes are found by running, not by reading.
- **FC-013.** Under the spec as written, formatter output is flagged by layout rules. See *Output questions* and *Architecture findings*. This is the main finding.
- **FC-002.** FMT-PIPE-025's predicate has qualifiers that must each be a condition: "innermost hole", "same hole", "block body" versus "expression body", "contains" versus "contained in", and "multi-line" raw literal. Cover each on both sides:
  - an anonymous method in a raw literal nested inside a block lambda that is in an outer hole: the innermost hole has no lambda, so the method is kept;
  - a block lambda inside the anonymous method: kept;
  - an anonymous method inside the block body of a lambda in the same hole: broken (spec example G);
  - an anonymous method inside an expression lambda: kept;
  - an anonymous method in a single-line raw literal: broken.
- **FC-027.**
  - Several anonymous methods in one hole.
  - The anonymous method as one element of a list of several. On `main` the list is wrapped; under FMT-PIPE-025 it stays on one line.
  - The anonymous method inside an object initializer in a hole.
  - `static` and `async` anonymous methods, with and without a parameter list.
  - A `$$"""` literal with `{{…}}` holes.
- **FC-019 / FC-021.** A block comment inside a kept anonymous method (`delegate { /* c */ return 1; }`). A line the author wrote inside one that ends in a `//` comment.
- **FC-022.** A regular `$"…"` and a verbatim `$@"…"` hole are deliberately left unspecified. A test must not lock either in by accident.
- **FC-017.** Two passes, LF and CRLF, on every case.
- **Class of the blank-line change.** Cover rows 1–5 of the table above, one test each. The rows that are joined, or guarded by a comment, are the unchanged side.

Unchanged inputs to pin:
- `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment`.
- A lambda argument on its own line without a blank line.
- The FMT-BREAK-011 example: I verified the after-output on `main`, and the comma and closing-parenthesis clauses are unchanged.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `LineBreakPhase`: parameter-list collapse for lambdas | must change |
| `LineBreakPhase` and `BlankLinePhase` inside a hole | must change (option A of question 1) or must stay (option B) |
| `IndentationPhase` (`ReleaseUnplacedRawStringHoleLines`, FMT-RAW-007) | must stay for A. B needs a new alignment requirement that places a broken anonymous method in a hole |
| Analyzers RH5001–RH5019 (SHARED-STMT) and RH5103 | must change under A to keep SHARED-DIAG-010; not involved under B. See question 1 |
| RH5417 | must stay. With A it reports every kept one-line anonymous method in a hole, with no fix (RH5417-004). SHARED-DIAG-010 does not cover it, but CLI users keep a diagnostic on formatted code |
| RH5105, RH5106, RH5107 | must stay (RH5105 already excludes lambdas) |
| Code fixes | not involved under B. Under A, the RH5002/RH5008 fixes insert lines into the kept anonymous method (`            return x; })}`), and the formatter then keeps those lines as author-written |
| Rule pages | not involved under B. Under A, the diagnostic changes of RH5001–RH5019 and RH5103 put their pages in scope for review |
| `Reihitsu.Core` | not involved |
| Tests | `RawStringInterpolationHoleTests.AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` changes. `PhaseStabilityCheckTests.CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` loses its example once the reversal is gone and needs another reversal or a synthetic phase. `PhaseReversalExceptionList` loses the entries the change removes |

## Simpler option

For the lambda part: none. The change already is the narrowest one, removing one strip.

For the hole part, option B of question 1 is simpler in the dimension that matters. It needs one alignment requirement, and it needs no predicate in two phases with five qualifiers. It changes no analyzer, and the output it gives is the shape every layout rule already accepts. I checked: `format.sh` keeps that shape unchanged on `main`, and RH5204, RH5205, RH5203, RH5202, RH5417, RH5008, and RH5103 report nothing on it.

## Output questions

**1 · Formatter · anonymous method in an interpolation hole of a multi-line raw string literal.** This replaces the spec author's question 1, which left out the analyzer consequences.

Input:
```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 Hello {M(delegate { int x = 1; return x; })}
                 """;

        return 0;
    }
}
```

Before (`main`): the anonymous method is broken onto unplaced lines in column 0, or in column 13 when the literal was moved:
```csharp
        var a = $"""
                 Hello {M(delegate
{
int x = 1;

return x;
})}
                 """;
```

After, A (the spec as written):
```csharp
        var a = $"""
                 Hello {M(delegate { int x = 1; return x; })}
                 """;
```
These are the diagnostics on that output, at a26602c, from `scripts/apply-fix.sh`:
- RH5008 reports `return`, and its fix offers `{ int x = 1;⏎⏎            return x; })}`.
- RH5103 reports `return x;`, with no fix.
- RH5417 reports the `{`, with no fix. It reports this on every one-line anonymous method in such a hole, `delegate { return 1; }` included.
- RH5002 likewise reports an `if` that follows a statement.

A therefore needs the SHARED-STMT rules and RH5103 to stop reporting inside such an anonymous method. SHARED-DIAG-010 requires this, and SHARED-STMT-002's Why argues the same way. That change is visible as well.

After, B (recommended):
```csharp
        var a = $"""
                 Hello {M(delegate
                          {
                              int x = 1;

                              return x;
                          })}
                 """;
```
Braces align under the anonymous method's first token, the way FMT-INDENT-028 aligns a block lambda in the same place, and every line moves with the literal. No rule reports this output.

Why B: it gives the same output wherever the literal stood, which is the issue's purpose. Formatted code then stays clean of RH5002–RH5019, RH5103, and RH5417 without any analyzer change, while A leaves diagnostics on formatted code or widens the run to about twenty analyzers.

**2 · Formatter · blank line in front of a parenthesized lambda that starts its line.** This widens the spec author's question 2 from entry 5 to the whole class.

Input:
```csharp
using System;
using System.Linq;

class C
{
    Func<int, int> M(int[] items)
    {
        var total = items.Aggregate(0,

                                    (sum, item) => sum + item);
        var l = new Func<int, int>[]
                {
                    (x) => x,

                    (x) => x
                };
        Func<int, int> h = x =>

            (y) => y;

        return

            (x) => x;
    }
}
```

Before (`main`): all four blank lines are removed. A `(1)` in the same places keeps its blank line.

After, A (recommended): all four blank lines are kept. Columns are unchanged.

Why A: a lambda is laid out like any other expression in the same gap, as FMT-BREAK-030 and the new FMT-BREAK-011 text already state.

The spec author's question 3 (entry 6, a hole line keeps its column) stands as written. I recommend A for it as well.

## Internal choices

- **One pipeline requirement or two phase requirements**, if question 1 is answered A. I agree with one pipeline requirement (FMT-PIPE-025): both phases write line breaks there.
- **The test example for the phase-stability check.** Once the lambda reversal is gone, `CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument` needs a reversal that still exists, or an injected phase. I recommend an injected phase, so the check's own test does not depend on a formatter defect surviving.

## Issue goals

- **"Output is unchanged; only the strip-and-repaint goes."** Drop it. The output changes for rows 1–5 of the table and for entry 6. This is visible, and it is question 2 and the spec author's question 3.
- **"The line-break phase does not break inside an interpolation hole … where no alignment rule will place the resulting lines; such a hole stays on one line."** This is a prescribed solution, and it conflicts with SHARED-DIAG-010 for the statement blank-line rules and RH5103 (question 1). If A stays, the goal widens to the blank-line phase (spec author) and to those analyzers.
- **Done when: "`AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` changes its expectation to the one-line hole."** This depends on question 1. Under B the expectation becomes the aligned shape.
- **Done when: "the matching phase-level idempotency exceptions are removed."** Keep it. `PhaseReversalExceptionList` enforces it.
- **Done when: the block-lambda test keeps today's output.** Keep it. Both options satisfy it.

## Architecture findings

**The formatter and the analyzers would own one decision with two answers (option A only).** Whether a statement inside an anonymous method in a raw-string hole starts its own line and gets a blank line in front of it would be decided twice:
- FMT-PIPE-025 says the formatter must not insert that line break;
- SHARED-STMT-001 (RH5001–RH5019), RH5103-001, and RH5417-001 report its absence.

The input is the question 1 input, and the conflicting diagnostics are listed there. This is FC-013 with SHARED-DIAG-010 applying, not a P11 case: here the rule asks for more than the formatter does, and the formatter's decision stands. Under B the decision has one answer.

## Follow-up candidates

1. **Formatter · anonymous method in a hole of a regular interpolated string below C# 11** (the spec author's candidate 1, `CS8967`). The caller path is still to be confirmed. Plausible: a formatter-backed code fix, for example the RH5417 fix on a method body, run in a project with `LangVersion` 10, whose formatted node contains such a string. I did not verify it. It is not affected by either option of question 1.
2. **Formatter · a parenthesized lambda behind a modifier or a return type on the next line** (the spec author's candidate 2). Another member with the same output: a conditional branch, `: static⏎(int x) => x` behind `: static` in column 31, puts `(int x)` in column 8. I agree with recommendation A (join), on the reasoning of FMT-BREAK-015.
3. **Formatter · a value continued on the line after `return` or after a lambda's `=>`** starts in the statement's column, for example `return⏎(1);` with `(1)` in column 8, and the same for `x =>⏎(y) => y`. This is not caused by this run, it holds for any expression, and I did not check it against FMT-INDENT. File it only if no FMT-INDENT requirement already decides that column.
