Role: skeptic
Commit: a4cbb87
Verdict: proceed, with two spec corrections and one output question added to the batch

Inputs: thoenissen/Reihitsu#1786 (`agent-filed`), `00-lead.md`, `01-spec-author.md`, `sdd/changes/2026-11/argument-list-below-name.md`, `git diff origin/main -- sdd/specs`. My prompt carried no intended fix or hoped-for verdict. Every fixture below was formatted on `main`'s code at a4cbb87 (the branch changes only specs and docs) with `scripts/format.sh <dir>` or `scripts/apply-fix.sh <RH####> <dir>`, in a scratch directory outside the tree, since deleted.

## Premise

Real, and reached by a caller. Reproduced independently:

```csharp
// fixture fix5/W.cs, inside void M() of class C, with using System.Collections.Generic;
        HashSet<int> h = [with
        (4), 1, 2];
```

`scripts/apply-fix.sh RH5305` → `(4)` at column 25 (the `[`'s column); the CLI (spec author's run, and my `format.sh` runs of the same shape) puts `(4)` at column 8 (the statement's). The same column split exists for the conditional (RH5308 fix on `var x = a ? Get⏎(1) : 2;` puts `(1)` at column 16; the CLI at column 8). `Foo⏎(1,⏎ 2)`, `new List<int>⏎(4)`, `(f ?? g)⏎(1)`, `Get(1) + Get⏎(2)`, `a && Has⏎(1)` all keep their line break on `main`. Callers: the CLI and whole-document formatting for every shape; every code fix whose formatted node holds such a gap other than at its first token. The input is rarely written by hand, but the change is cheap and removes a meaningless column, so the premise justifies the change.

The issue says it was "read from code, not run"; its claimed columns hold. Its sketched target `var a = [with(4), 1, 2];` is not what the spec chose (Q1 A keeps one element per line); that is already posed as an output question.

## Mechanism and candidates

Not a bug; the candidate set is the owners of an `ArgumentListSyntax`. From Roslyn 5.12 (`Microsoft.CodeAnalysis.CSharp.xml`, members `*.ArgumentList`) plus `WithElementSyntax` (used in `WithSpacingRule.cs:35`): `InvocationExpressionSyntax`, `ObjectCreationExpressionSyntax`, `ImplicitObjectCreationExpressionSyntax`, `ConstructorInitializerSyntax`, `PrimaryConstructorBaseTypeSyntax`, `WithElementSyntax`. FMT-BREAK-039 names all six. Excluded by its coverage sentence: `BracketedArgumentListSyntax` owners (`ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`, `ImplicitElementAccessSyntax`) and `AttributeArgumentListSyntax`. Complete.

**Defect in the spec diff — `(f)⏎(1)` is not an argument list.** C# parses `(identifier)(expr)` as a cast. Compiled with the .NET 11 SDK (`dotnet run` of a three-line file):

```csharp
using System;
Func<int, int> f = x => x + 1;
var r = (f)(1);
```

→ `error CS0118: 'f' is a variable but is used like a type` at `(3,10)`: `(f)(1)` is a `CastExpressionSyntax` whose operand is `(1)`. So FMT-BREAK-039's normative text (`(f)⏎(1)` as "an invocation, whatever expression is invoked"), its first example (`var r = (f)⏎(1);` → `var r = (f)(1);`), the change doc's change 1, and the Q1 draft in `01-spec-author.md` all show a join that a requirement stated by the argument list does not produce, on code that does not compile. A real invocation of a parenthesized expression is `(f ?? g)⏎(1)` or `(a ? (Func<int, int>)Get : Get)⏎(1)`; both keep their line break on `main` (fixtures `order/Order.cs`, `after/G.cs`). `Make(1)⏎(2)` is a real invocation of an invocation. These must replace `(f)` before Q1 is asked; the cast itself is a different construct (follow-up candidates).

## Breaking inputs

**Containers whose layout depends on "spans several lines" (FMT-BREAK-018, FMT-BREAK-022).** The join removes a line break inside a container, and the line-break phase decides some containers before its joins and some after (`LineBreakPhase.CreateRewriters`: `ListElementInteriorJoinRewriter` and `LineBreakListRewriter` run before `ChainLineBreakRewriter`, which runs before `TernaryLineBreakRewriter`). On `main`, with the joins that exist today (FMT-BREAK-015 and FMT-BREAK-020) as the probe:

| Container (requirement) | After an FMT-BREAK-020 join on `main` | After an FMT-BREAK-015 join on `main` | Decided by FMT-BREAK-039? |
|---|---|---|---|
| Collection expression (022) | as written: `[v.Equals⏎(1).GetHashCode(), 2]` → one element per line | as written: `[Foo(out⏎var x), 2]` → one element per line | yes, paragraph 2 |
| Complex element initializer (022) | as written: `{ { v.Equals⏎(1).GetHashCode(), 2 } }` → inner braces one element per line | not probed | **no** — paragraph 2 names collection expressions only |
| List / property pattern (022) | not probed; reachable only via `nameof⏎(x)` in a pattern | — | no (rare) |
| Conditional expression (018) | after the join: `a ? v.Equals⏎(1).GetHashCode() : 2` → one line | after the join: `a ? Foo(out⏎var x) : 2` → one line | **no** — the outcome depends on where the join runs |
| Object, collection, anonymous, `with` initializer (021) | always several lines | — | not affected: FMT-BREAK-021 lays them out over several lines regardless |
| Array initializer (021) | author's layout; the join removes the only line break | — | not affected |
| Binary expression (017) | `a && v.Equals⏎(1).Equals(true) && k` → one line | — | not affected: 017 acts on operators at a line boundary only |
| Argument / parameter / tuple list holding the gap in an element (011–015) | — | FMT-BREAK-015 already joins `Foo(1, Get⏎(1))` on `main` | not affected (already joined) |
| Switch expression arms, lambda bodies, interpolation holes | joined, no layout decision (`$"{v.Equals⏎(1).GetHashCode()} x"` → one line) | — | not affected; still members to test |

The conditional row is an open visible output (Output questions, 1). The complex-element-initializer row follows the collection row: paragraph 2 should name every construct FMT-BREAK-022 decides, not only collection expressions (Output questions, 2).

**Failure classes that reach the change:**

- FC-004, FC-019, FC-020, FC-021 — comments, directives, disabled text in the gap: covered by the spec's third example (`//`, `/* */`, `#if`). Add a `#region`/`#pragma` in the gap and a comment on the line *after* `(` (`Foo⏎( // c⏎4)`), which is not in the gap and must not block the join.
- FC-040 — `/** */` in the gap (`Foo /** c */⏎(4)` and `Foo⏎/** c */ (4)`) in the documentation modes `None`, `Parse`, `Diagnose`: the documentation-comment phase runs earlier and may relocate it.
- FC-027 — nested and multi-line forms: `Foo⏎(Bar⏎(1))` (outer by 039, inner by 015), `Make(1)⏎(2)⏎(3)`, `Foo⏎(⏎    1,⏎    2)`, `Foo⏎(x =>⏎{ … })` (a gap inside a lambda block inside an argument, where FMT-BREAK-015 does not join), `new List<int>⏎(4)⏎{ 1 }`, generic `Foo<int>⏎(4)`, `f!⏎(1)`.
- FC-010, FC-017 — idempotency over two passes, under LF and CRLF, including a blank line in the gap (`Get⏎⏎(4)`).
- FC-028, FC-030 — the change doc's change 5 cases through `apply-fix.sh` (RH5305, RH5303, RH5301, RH5302, RH5308), plus RH5101/RH5102, whose formatted node starts at `(` and keeps the break (FMT-PIPE-017): that result is today's and stays.
- FC-038 — not reachable: the join only removes whitespace and line breaks in a gap with no comment, so it cannot carry a comment into FMT-UNSUP-004/008/009's spans.
- FC-013 — no rule reports `(` below an invoked name (RH5105 registers `SyntaxKind.ParameterList` only; RH5108 declarations only), and the join creates nothing RH5101, RH5102, RH5203, RH5305, or RH5308 reports. RH5308's fix on `a ? v.Equals⏎(1).GetHashCode() : 2` yields the CLI's one-line output on `main` (`apply-fix.sh RH5308`, 2 fixed), so the fix follows whatever the conditional reading becomes.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, line-break phase | Must change (the join) |
| Formatter, indentation phase | Must stay: the joined line no longer needs a column |
| Formatter, other phases, precheck | Not involved |
| Analyzers (RH5105, RH5108, RH5308, RH5305, …) | Must stay; no rule reports the shape (F3 is a new-rule candidate) |
| Code fixes | No source change; outputs change through the formatter (change doc, change 5) |
| Fix All | Not involved beyond the formatter consequence |
| `Reihitsu.Core` | Not involved |
| Rule pages | Must stay |
| Tests | Formatter regression and idempotency tests; existing tests whose expected output keeps a line break in front of an argument list's `(` outside a chain or list element would change — the test implementer should search for them |

## Refuse option

- **Leave the construct as written (option C, today's behavior)** — 0 production lines; keeps the meaningless column and the CLI/code-fix column split (the issue's second goal stays unmet; the split is #953's class, F2). The input is rare, but the join is one gap in one phase — a few dozen lines at most — so refusing buys little.
- **Declare the shape unsupported** — not an option: a line break in front of an argument list's `(` costs no special case anywhere, so declaring it deletes nothing (`AGENTS.md`: a shape that deletes nothing is not declared).
- **Narrower:** invocations and object creations only (the issue's scope), leaving constructor initializers and primary-constructor base types (Q2 B). Same mechanism, no saving; Q2 already asks it.

## Output questions

Both belong in the batch with the spec author's Q1 and Q2. Q1 must first replace `var r = (f)⏎(1);` with a real invocation, such as `var r = (f ?? g)⏎(1);` (before: unchanged on `main`; after A/B: `var r = (f ?? g)(1);`).

**1 · Formatter · Argument list written below its name inside a conditional expression**

Input:
```csharp
var x = a ? Get
(1) : 2;
```

Before (`main`), CLI:
```csharp
        var x = a
                    ? Get
        (1)
                    : 2;
```
(RH5308 fix on `main`: the same, with `(1)` at column 16.)

After, A (recommended) — the conditional is judged after the join, as for every join inside a conditional today:
```csharp
        var x = a ? Get(1) : 2;
```

After, B — the conditional is judged as written, as Q1 A judges a collection expression:
```csharp
        var x = a
                    ? Get(1)
                    : 2;
```

Why A: on `main` a conditional whose only line break was a split inside a branch already becomes one line (`a ? v.Equals⏎(1).GetHashCode() : 2`, `a ? Foo(out⏎var x) : 2`), so A keeps all joins alike inside a conditional.

**2 · Formatter · Argument list written below its name inside a dictionary's element initializer** (follows Q1; recommended: same answer as Q1)

Input:
```csharp
var e = new Dictionary<int, int> { { Get
    (1), 2 } };
```

Before (`main`):
```csharp
        var e = new Dictionary<int, int>
                {
                    {
                        Get
        (1),
                        2
                    }
                };
```

After, A (recommended, with Q1 A):
```csharp
        var e = new Dictionary<int, int>
                {
                    {
                        Get(1),
                        2
                    }
                };
```

After, B (with Q1 B):
```csharp
        var e = new Dictionary<int, int>
                {
                    { Get(1), 2 }
                };
```

Why A: FMT-BREAK-022 judges a complex element initializer as written after an FMT-BREAK-020 join today (`{ { v.Equals⏎(1).GetHashCode(), 2 } }` → one element per line), like a collection expression.

## Internal choices

- **FMT-BREAK-039 paragraph 2** should name the constructs FMT-BREAK-022 decides ("a construct of FMT-BREAK-022"), not only collection expressions, and a conditional should get a sentence of its own once Output question 1 is answered. Recommend: pin both in FMT-BREAK-039, because without them the outcome depends on where in the rewriter order the join runs.
- **The spec author's Why for Q1 A** ("as it does for every other join inside an element") is true for collection expressions only: FMT-BREAK-015's Why states the opposite for argument and parameter lists ("a list that was multi-line only because of such a wrap is laid out as the one-line list it becomes"), and conditionals follow that too. The Why should say "as a collection expression does today", so the maintainer sees that the precedent differs by container.
- **Overlap with FMT-BREAK-015 and FMT-BREAK-020** — see Architecture findings.

## Issue goals

- "`var a = [with(4), 1, 2];`" (one line): changed to one element per line by Q1 A — visible, already asked. (The issue's `var a = [ … ]` has no target type and does not compile; the spec's typed version is right.)
- "Regression tests … for an invocation, an object-creation-like `Foo(4)` element, and `with(4)`": keep; the spec's examples cover these and more.
- "The CLI and the RH5305 fix agree on the column for `[with⏎(4), …]`": met by the join, since the line disappears; keep.
- The `analyzer` label: no analyzer changes; the analyzer side is follow-up F3.

## Architecture findings

**One join stated in three requirements.** FMT-BREAK-039 paragraph 1 demands the join for every argument list, "whatever expression is invoked", with no carve-out. FMT-BREAK-020 already demands it for an argument list in a chain with a link, and FMT-BREAK-015 for one inside an argument, parameter, tuple, attribute-argument, or type-argument list element. The same gap is now stated in three places that can drift (their guards and exceptions already differ: FMT-BREAK-015 excludes "every line inside a nested braced, bracketed, or query construct"), and the implementation risks a third rewriter owning a gap that `ChainLineBreakRewriter` and `ListElementInteriorJoinRewriter` already join (FC-015). No input I found gives different output, so this is internal. Recommend: one requirement owns the argument-list gap. Either FMT-BREAK-039 owns it and FMT-BREAK-020/015 point to it for argument lists, or FMT-BREAK-039 covers only the gaps those two leave and says so. The lead decides.

## Follow-up candidates

- Agree with F1 (element access outside a chain), F2 (#953's unowned continuation-line class; RH5308's `(1)` at column 16 versus the CLI's column 8 is another member, removed here only for argument lists), and F3 (new-rule candidate).
- **F4 · Formatter · A cast's operand written on the line below the cast** (`(int)⏎(x + 1)`, and the `(f)⏎(1)` that looked like an invocation): a different construct, left as written on `main`; low value and rare, so the lead may drop it.
