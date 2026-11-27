Role: skeptic
Commit: 2c400d6
Verdict: #1568: output question (proceed on the maintainer's answer; change doc to be amended); #1698: output question (proceed; one more input for the question); #1567: proceed narrower (as the spec author narrowed it)

All outputs below are real outputs at 2c400d6 (production code identical to fe7b8f8), taken with `scripts/format.sh <dir>` run twice (every fixture was stable on the second pass) or `scripts/apply-fix.sh <RH####> <dir>` (LF and CRLF agree). Fixtures were written to the session scratchpad and are quoted inline. My prompt carried no intended fix, cause, or hoped-for verdict.

## thoenissen/Reihitsu#1568

### Premise

Real. Caller: whole-document formatting (CLI) and every code fix that formats a node holding an anonymous method. The scope-depth layout outside holes comes from `LambdaAlignmentContributor.Contribute` (`src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs`), whose `AnonymousMethodExpressionSyntax` case aligns only `when IsAlignedInRawStringHole(...)`; every other anonymous method keeps the scope depth. The spec author's before-outputs reproduce.

Today's layout is worse than the issue and the change doc show. In positions where the block lambda aligns to its construct, the anonymous method's block falls back to the statement's scope depth, left of the construct that holds it (fixture `A1.cs`, `A3.cs`, `format.sh`, stable):

```csharp
        var d1 = new D
                 {
                     F = delegate
        {
            return 1;
        }
                 };
        Func<int> c1 = y > 0
                           ? delegate
        {
            return 1;
        }
                           : null;
        var a = k switch
                {
                    0 => M(delegate
        {
            return 1;
        }),
                    _ => M(() =>
                           {
                               return 1;
                           }),
                };
        Func<int>[] arr = [
                              delegate
        {
            return 1;
        },
                              () =>
                              {
                                  return 2;
                              }
                          ];
```

Inputs: `var d1 = new D { F = delegate { return 1; } };`, `Func<int> c1 = y > 0 ? delegate { return 1; } : null;`, `var a = k switch { 0 => M(delegate { return 1; }), _ => M(() => { return 1; }), };`, `Func<int>[] arr = [delegate { return 1; }, () => { return 2; }];` (with `class D { public Func<int> F { get; set; } }`, `int M(Func<int> f, int k = 0)`).

The code-fix path gives yet other columns today (`apply-fix.sh`):
- RH5101 on `return M(⏎            delegate { return 1; }, 2);` → `return M(delegate` with the block in column 16 and `2);` in 17 (confirms the spec author).
- RH5111 on `var d1 = new D { F =⏎            delegate { return 1; } };` → `var d1 = new D { F = delegate` with the block in column 25 (under `F`), `} };` closing it.

So the same anonymous method gets the scope depth (8), 16, 25, or its anchor depending on position and entry point. No analyzer reports any of these layouts (RH5203, RH5204, RH5205 run over the formatted fixtures: no diagnostic).

### Mechanism and candidates

_N/A_ (behavior change). Dispatch: `LambdaAlignmentContributor.Contribute` switches on `SimpleLambdaExpressionSyntax`, `ParenthesizedLambdaExpressionSyntax`, `AnonymousMethodExpressionSyntax`; only the last is position-limited.

### Breaking inputs

Failure classes reaching the change: FC-027 (enclosing scopes: argument, equals-value, assignment, return, initializer, conditional branch, switch arm, collection element, chain, nested anonymous method), FC-022 (verbatim/raw literals inside the shifted block), FC-020 (disabled text inside the block), FC-028 (whole document vs node-level entry), FC-013 (checked: no layout rule reports the new layout), FC-010/FC-017 (two passes, LF and CRLF), the tab-width matrix cell "node-level target with the tab in the whitespace in front of it" with anchoring "the block aligned to `delegate`".

Candidate inputs, each with today's output and the existing test that pins it:

| Input | Today | Pinned by |
|---|---|---|
| argument, argument + second argument, `static`, `async`, parameter list, declaration value, assignment, return | scope depth | FMT-INDENT-031's new example (spec); existing tests asserting scope depth will need updating (spec author lists `AnonymousMethodHoleAlignmentTests`, `RawStringInterpolationHoleTests`) |
| `E += delegate { Console.WriteLine(); };` (compound assignment; lambda counterpart `E += (s, e) => { … };` aligns under `(`) | scope depth | none found |
| field initializer `Func<int> _f = delegate { return 1; };` (lambda counterpart aligns under `(` in column 19) | scope depth (column 4) | none found |
| object-initializer member, conditional branch, switch-expression arm, collection-expression element (above) | statement column, left of the construct | none found |
| chain `new List<int>().Select(delegate (int a) { return a; }).Where(delegate (int a) { return a > 0; })` (lambda counterpart aligns each block under its `(`) | scope depth | none found |
| nested `M(delegate { return (int)M(delegate { return 2; }); })` | scope depth relative to the outer block | none found |
| empty body `M(delegate { })` → today `delegate⏎        {⏎        });` (the brace starts a line, so it shifts under B) | scope depth | none found |
| wrapped parameter list `delegate (int a,⏎ int b) { … }` → today parameters aligned, block at 8 | scope depth | none found |
| block holding a verbatim multi-line string, a multi-line raw string, `#if NEVER` disabled text, and a comment in front of the last statement (fixture `A2.cs`; the lambda counterpart today shifts the statements and the raw literal, leaves `line2";` and `disabled text` in their columns, and keeps `#if` in column 0) | scope depth | none found for anonymous methods |
| RH5101 / RH5102 / RH5111 / RH5205 fix results (spec author's list, plus RH5111 in an object initializer above) | 16 / 16 / 8 / scope depth; 25 | none found |
| C# 10, anonymous method in a regular string's interpolation stays on one line | unchanged | FMT-INDENT-031's new example |

Hand-built option-B layout of all the positions above (fixture `B1.cs`: field, `+=`, initializer, conditional branch, chain, nested, argument + second argument, wrapped parameter list, return) gives no diagnostic for RH5201, RH5102, RH5203, RH5204, RH5205 (`apply-fix.sh`, LF and CRLF).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, indentation phase (`LambdaAlignmentContributor`) | must change under B; must stay under A |
| Other formatter phases | not involved (line breaks of the anonymous method stay FMT-BREAK-002/006) |
| Code fixes RH5101, RH5102, RH5111, RH5205 | results change through the formatter under B; no fix code involved |
| Analyzers RH5203, RH5204, RH5205 | must stay; none reports either layout (checked) |
| Rule pages | not involved |
| `Reihitsu.Core` | not involved |
| Tests | existing tests asserting the scope-depth layout outside holes, and inside a lambda's body in a hole, change their expected output |

### Refuse option

Leave the construct as written = the issue's option A: no production change, the spec keeps FMT-INDENT-031's hole limit. Its cost is visible, not in code: the initializer, conditional-branch, switch-arm, and collection-element layouts above stay specified, and the code-fix columns 16 and 25 stay a disagreement with whole-document formatting (follow-up). B needs no new mechanism: the block-lambda alignment already handles the construct, and the requested behavior removes the hole-only condition rather than adding one. The input class is not rare in older code (anonymous methods as event handlers and callbacks), which is why the visible blast radius of B is wide: every anonymous method whose block starts a line moves. Declaring a shape unsupported: not an option — no comment, directive, or disabled-text shape is involved, and nothing would be deleted.

### Output questions

The spec author's question stands. It should show the maintainer the positions where today's layout is clearly broken, because they are the strongest argument for B and they also move under B. Add to its *Before* / *After, B*:

Input:
```csharp
using System;

class D
{
    public Func<int> F { get; set; }
}

class C
{
    int M(Func<int> f, int k = 0)
    {
        var d1 = new D { F = delegate { return 1; } };
        Func<int> c1 = k > 0 ? delegate { return 1; } : null;

        return M(delegate { return 1; });
    }
}
```

Before (`main`):
```csharp
        var d1 = new D
                 {
                     F = delegate
        {
            return 1;
        }
                 };
        Func<int> c1 = k > 0
                           ? delegate
        {
            return 1;
        }
                           : null;
```

After, B (hand-built, as the lambda counterpart lays out the same positions today):
```csharp
        var d1 = new D
                 {
                     F = delegate
                         {
                             return 1;
                         }
                 };
        Func<int> c1 = k > 0
                           ? delegate
                             {
                                 return 1;
                             }
                           : null;
```

My recommendation: B — one layout for the construct in every position, and it removes the broken layouts above; A keeps them specified.

Change-doc amendment for the spec author (routing is the lead's): `anonymous-method-aligns-everywhere.md` *Visible output changes* lists four entries; the initializer, conditional-branch, switch-arm, collection-element, field-initializer, compound-assignment, chain, and nested positions also change under B, and the RH5111 initializer result (column 25 → under `delegate`) is a code-fix result that changes. FMT-INDENT-031's "wherever the anonymous method stands" covers them; an example with at least the initializer and the conditional branch would make the spec show the positions the maintainer is asked about.

### Internal choices

FMT-INDENT-031 changed in place vs retired and replaced: changes nothing visible; I agree with in place.

### Issue goals

- Goals and option B serve the issue; nothing to drop.
- "Affected Surface: … the scope-depth indentation of anonymous-method blocks" is a prescribed location, not a goal; ignore it.
- The issue's "Additional Context" `2);` case is covered (entry 2).

### Architecture findings

_None_ within the run under B. Under A, the code-fix path (RH5101 column 16, RH5111 column 25) disagreeing with whole-document formatting (column 8) is a pre-existing FC-028 divergence; it stays the spec author's follow-up candidate.

### Follow-up candidates

The spec author's two stand. Add to the A-branch one: the RH5111 initializer result (column 25) belongs to the same divergence.

## thoenissen/Reihitsu#1698

### Premise

Real, but the issue's *Before* is stale, as the spec author found: today `// e`, `a`, and `}` stand in column 12 (FMT-INDENT-034), and only `// f` (column 16) is off. Caller: whole-document formatting; code fixes that format a node holding such a hole (RH5205, per the spec author). Cause: `CommentIndentationContributor.AlignCommentsBeforeToken` adds one indent level in front of every `CloseBraceToken`, the interpolation's included.

Neighbours checked (fixture `H1.cs`, `format.sh`, stable), each with `// f` four columns right of the brace today:

```csharp
        var al = $"{
            a,
                5

                // f
            }";
        var bc = $"{
            a

                /* f */
            }";
        var two = $"{
            a

                // f
                // g
            }x{
                a

                    // h
                }";
        var nest = $"{
            $"{
                a

                    // f
                }"

                // g
            }";
        var rawInRegular = $"{
            $$"""
              {{
                  a

                  // f
              }}
              """

                // g
            }";
```

Inputs: the same with every comment and content line written at column 12 or 16 (`al` = `$"{⏎a,⏎5⏎// f⏎}"`, `bc` = `/* f */`, `two` = two comments, then a second hole, `nest` = regular in regular, `rawInRegular` = a multi-line raw literal in a regular string's hole).

The content and the brace do not always share a column (fixture `H3.cs`):

```csharp
        var t = $"{a
                   + b

                // f
            }";
        var u = $"x{M(a,
                      b)

                // f
            }";
```

Inputs: `var t = $"{a +⏎            b⏎            // f⏎            }";` and `var u = $"x{M(a,⏎                      b)⏎            // f⏎            }";`. The hole's content line stands in column 19 or 22, its `}` in 12.

### Mechanism and candidates

_N/A_ (behavior change). Dispatch: the one-level-deeper branch keys on `SyntaxKind.CloseBraceToken`, whose parents are blocks, type and namespace bodies, accessor lists, initializers, anonymous objects, property patterns, switch blocks and expressions, extension blocks, and `InterpolationSyntax` (regular, verbatim, single-line raw, multi-line raw). Only `InterpolationSyntax` of a non-multi-line-raw string is in question.

### Breaking inputs

Failure classes: FC-021 (comment before the closing delimiter; line and delimited comment), FC-027 (nesting: regular in regular, regular in raw hole, raw in regular hole, hole inside a lambda's shifted block), FC-022 (verbatim, single-line raw, multi-line raw), FC-003 (both sides of the boundary: multi-line raw's own hole unchanged), FC-010/FC-017, FC-031 (the summary of `AlignCommentsBeforeToken` states the one-level-deeper set).

Candidate inputs: the spec's new FMT-INDENT-006 example (t, v, s, n, r), plus `al`, `bc`, `two` (two comments; second hole), `nest`, `rawInRegular`, `t`/`u` from `H3.cs` (content column ≠ brace column), and a hole inside a lambda's block body (RH5205 path, spec author). Existing tests: the spec author names `CommentBeforeBraceBlankLineTests`; none found for the other shapes.

Analyzer check on the hand-built after-layout (`H2.cs`: `t` and `nest` with every comment in the brace's column and the blank line above it): RH5204, RH5020, RH5104, RH5024 report nothing.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, indentation phase (comment column) | must change |
| Blank-line phase | must stay (FMT-BLANK-011 blank line above the comment; FMT-BLANK-020 already names the interpolation's closing brace) |
| Code fixes (RH5205 result) | change through the formatter; no fix code involved |
| Analyzers RH5204, RH5020, RH5104, RH5024 | not involved (checked) |
| Rule pages, `Reihitsu.Core` | not involved |

### Refuse option

Leave it as today (the issue's B): no code, the spec states the one-level-deeper column for this brace. The input class is rare: a comment on its own line in front of the closing brace of a multi-line hole in a non-raw interpolated string (C# 11 or later). The requested change costs one cheap syntactic condition (the kind of the interpolated string the brace's interpolation belongs to) and one clause in FMT-INDENT-006; no new mechanism. Both options are non-destructive.

Declaring a shape unsupported (named as required; I recommend against it): span — a comment on its own line in the leading trivia of the closing brace of an `InterpolationSyntax` whose interpolated string is not a multi-line raw string literal; nothing excepted. It would delete no special case existing today; it would only avoid the exception option A adds to FMT-INDENT-006. It refuses no common code but would refuse whole files for a harmless comment the formatter already places non-destructively. A shape that deletes nothing existing should not be declared (`AGENTS.md`).

### Output questions

The spec author's question (A brace column with blank line / B unchanged / C no blank line) stands, and it should show one more input, because the issue's option A says "the column of the hole's other content lines", the spec says "the column of the brace", and the two differ when the hole's content is aligned:

Input:
```csharp
class C
{
    string M(int a, int b)
    {
        var t = $"{a +
            b
            // f
            }";

        return t;
    }
}
```

Before (`main`):
```csharp
        var t = $"{a
                   + b

                // f
            }";
```

After, A (recommended; the spec as written — the brace's column):
```csharp
        var t = $"{a
                   + b

            // f
            }";
```

After, A′ (the column of the hole's content line):
```csharp
        var t = $"{a
                   + b

                   // f
            }";
```

Why A: the comment takes the column of the token it precedes, which is FMT-INDENT-006's general rule for every other delimiter that closes no deeper content; A′ would need a notion of "the hole's content column" the formatter does not have.

### Internal choices

The boundary keyed on the kind of interpolated string (spec author) vs on where the string stands: they give different output for a regular string nested in a multi-line raw hole, so it is part of the output question, not internal; the spec author's question already names that case (change doc entry 2). No other internal choice.

### Issue goals

- The issue's *Before* is stale (columns 8 → 12 since FMT-INDENT-034); drop it, no visible effect.
- "Only the regular interpolated string is affected" is wrong: verbatim and single-line raw are affected the same way; the spec covers them.
- The issue's option A drops the blank line; keeping it is a departure with a visible effect, already put to the maintainer as option C by the spec author.

### Architecture findings

_None._ The comment set of `AlignCommentsBeforeToken` names the blank-line phase as reading "the same predicate"; the blank-line side (`BraceBlankLinePolicy.RemovesBlankLinesDirectlyBefore`, FMT-BLANK-020) keeps the interpolation's closing brace on purpose, so after the change the two sets differ legitimately (one decides blank lines, the other a column). The test implementer should pin the blank line next to the new column so a drift shows.

### Follow-up candidates

- Out of scope, seen in passing: an alignment clause on its own line in a hole, `$"{⏎a,⏎5⏎}"`, puts `5` in column 16, four right of `a` (fixture `H1.cs`, `al`). Rare; no spec checked. Lead to decide whether it is worth a follow-up.

## thoenissen/Reihitsu#1567

### Premise

Narrowed, as the spec author found. Fixture `L1.cs` (`format.sh`, stable):

- Not reproduced (FMT-BREAK-041 joins): `Func<int, int> f = static⏎            (int x) => x;` → `Func<int, int> f = static (int x) => x;`; `async⏎(int x) => await Task.Yield()` → joined; `b ? static⏎(int x) => x : null` → `b ? static (int x) => x : null`.
- Reproduced (kept, `(` a continuation line in column 12, not the statement's column as the issue says): `int⏎(int x) => x`, `List<int>⏎(int x) => null`, `(int, int)⏎(int x) => (x, x)`, `int[]⏎(int x) => null`, `int?⏎(int x) => x`, `ref int⏎(ref int x) => ref x`, `async Task<int>⏎(int x) => x`.
- The identical gap inside an argument is joined today: `M2(int⏎            (int x) => x)` → `M2(int (int x) => x)` (fixture `L2.cs`). So the same lambda is laid out differently as an argument and as a value.

Caller: whole-document formatting; RH5111's fix (spec author).

### Mechanism and candidates

_N/A_ (behavior change). The gap is `ParenthesizedLambdaExpressionSyntax.ReturnType` last token → `ParameterList.OpenParenToken`; return-type kinds seen: `PredefinedTypeSyntax`, `GenericNameSyntax`, `TupleTypeSyntax`, `ArrayTypeSyntax`, `NullableTypeSyntax`, `RefTypeSyntax`.

### Breaking inputs

Failure classes: FC-004, FC-019, FC-020, FC-021 (comment, directive, disabled text in the joined gap), FC-027 (positions: declaration value, assignment, return, conditional branch, expression body, nested lambda body, list element), FC-010/FC-017, FC-038 (a join must not carry a comment into a declared span: no comment moves in this join, but the guard decides that).

Candidate inputs and today's output (`L1.cs`, `L2.cs`):

| Input | Today | Pinned by |
|---|---|---|
| return-type kinds above (predefined, generic, tuple, array, nullable, `ref`), `async Task<int>` | kept | none found |
| `int⏎#if DEBUG⏎(int x) => x;⏎#else⏎(int x) => x + 1;⏎#endif` | kept | none found |
| `int /* c */⏎(int x) => x` | kept | none found |
| `int⏎// c⏎(int x) => x` | kept, with a blank line inserted above `// c` (FMT-BLANK-011) | none found |
| `int // c⏎(int x) => x` | kept | FMT-BREAK-045 negative example |
| `static int⏎⏎(int x) => x` (blank line) | kept | FMT-BREAK-045 example |
| block body after the join, `int (int x) =>⏎{…}` → block under `(` in column 31 today when already joined; `static int (int x) =>` → under `(` in 38 | aligned (FMT-INDENT-028) | none found |
| lambda as a lambda's expression body `a => int⏎(int x) => x`; expression-bodied property `Func<int, int> P => int⏎(int x) => x;` | not run | none found |
| `[A] int (int x) => x` → today `[A]⏎            int (int x) => x;` and `[A] (int x) => x` → `[A]⏎            (int x) => x;` (the lambda's attribute list goes on its own line); must stay — the attribute gap is not FMT-BREAK-045's | attribute on its own line | none found for lambdas |
| `ref⏎int (ref int x) => ref x` (line break inside the `ref` return type) | kept; not FMT-BREAK-045's gap | none found |

Analyzers on the joined layout (`int (int x)`, `List<int> (int x)`, `int[] (int x)`, `int? (int x)`, `(int, int) (int x)`): RH6006, RH6008, RH6012, RH6015, RH5105, RH5108 report nothing. On the unjoined `int⏎(int x) => x`: RH5105, RH5108, RH5111 report nothing.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, line-break phase | must change under A |
| Indentation, horizontal spacing | must stay (FMT-INDENT-028 aligns the block under the joined `(` today; FMT-SPACE-026 gives one space) |
| Precheck | not involved (no shape declared) |
| Code fix RH5111 | result changes through the formatter under A |
| Analyzers RH5105, RH5108, RH6006 | not involved (checked) |
| Rule pages, `Reihitsu.Core` | not involved |

### Refuse option

Leave it as written (today; the issue's B and C are today's output now). Cost of A: one more joined gap plus a comment/directive/disabled-text guard. The rewriter that joins the neighbouring modifier gap (`ContinuationJoinRewriter`) has no guard of its own and relies on the precheck (FMT-UNSUP-011, FMT-UNSUP-017); the list-element join (FMT-BREAK-015) has one. So FMT-BREAK-045's "a comment keeps the line break" needs a guard the modifier gaps do not have, or a declaration. The input class is rare (C# 10 explicit lambda return type with a line break in front of `(`).

Declaring a shape unsupported: span — a comment, directive, or disabled text between the last token of a parenthesized lambda's return type and the `(` of its parameter list. It would delete no special case existing today (the FMT-BREAK-015 guard serves every list-element gap and stays); it would only spare the guard the new join needs. I agree with the spec author: a shape that deletes nothing existing is not declared (`AGENTS.md`), so it is not an option to escalate.

### Output questions

The spec author's question stands as written. My recommendation: A, weakly — the decisive fact is the argument/value inconsistency above (`M2(int⏎(int x) => x)` joins, `f = int⏎(int x) => x` does not); B is a legitimate, cost-free answer for a rare shape.

### Internal choices

A requirement of its own (FMT-BREAK-045) vs an entry of FMT-BREAK-041: changes nothing visible; agree with the spec author (FMT-BREAK-041's gaps are the precheck-guarded ones).

### Issue goals

- The issue's main examples (`static`, `async`, conditional branch) no longer reproduce; drop them as goals, no visible effect. `Closes` remains right under A because the remaining case is one the issue names ("Applies the same way to a lambda with an explicit return type").
- "The `(` falls into the statement's column" is stale: it is a continuation line one level deeper.
- The issue's option B (continuation indented one level) describes today's output; it is no longer a separate option.

### Architecture findings

_None._

### Follow-up candidates

- The spec author's RH5105 candidate stands.
- Seen in passing, both rare and outside the issue: a line break inside a `ref` return type (`ref⏎int (ref int x) => ref x`) stays; a lambda's attribute list is put on its own line, which leaves the `(` of `[A] (int x) => x` starting a line (`[A]⏎(int x) => x`). The second is formatter output, not kept input; whether that layout is specified for lambdas I did not settle — the lead decides whether either is worth a follow-up.

## Whole run

- No build or test run beyond the formatter and code-fix runners above.
- Fixtures: scratchpad directories `s1` to `s9`; they are not part of the repository.
