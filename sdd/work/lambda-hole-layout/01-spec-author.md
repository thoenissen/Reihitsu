Role: spec author
Commit: 25c5670
Verdict: #1568: change doc written; #1698: change doc written; #1567: change doc written

All "before" outputs below are real outputs of `scripts/format.sh` / `scripts/apply-fix.sh` at 25c5670 (code identical to fe7b8f8). All "after" outputs are hand-built, since nothing is implemented yet; each spec example was run through today's formatter and differs from today's output only in the lines the change moves (checked by diff).

## thoenissen/Reihitsu#1568

Change doc: `sdd/changes/2026-11/anonymous-method-aligns-everywhere.md` (Draft), written on the issue's recommended option B. This issue is the follow-up named in `lambda-paren-hole-break.md` (*Alternatives considered*, last item) after the maintainer's statement there that a hole has no layout rules of its own.

### Changed requirements

- FMT-INDENT-031 — An anonymous method's block aligns like a block lambda — changed (scope grows from multi-line raw-string holes to every position; anchor unchanged: `async`, else `delegate`, also behind `static`). Its hole examples stay; the three examples that said "left unchanged" (single-line raw string outside a raw hole, anonymous method in a lambda's block body in a hole and outside one, the same with a regular string between) now show the aligned output; two examples added (positions outside holes, and C# 10 where the brace does not start a line).
- `Uses` of `formatter/indentation.md` gains FMT-BREAK-044.

### Examples for the tests

FMT-INDENT-031 (every example; the first is new and covers argument, argument followed by another argument, `static`, `async`, parameter list, declaration value, assignment, return; the last is the negative case under C# 10).

### Output questions

**1568 · Formatter · anonymous method's block outside a raw-string hole**

Input:
```csharp
using System;
using System.Threading.Tasks;

class C
{
    object M(Func<int> f, int y = 0)
    {
        var x = M(delegate { return 1; }, 2);
        Func<int> s = static delegate { return 1; };
        Func<Task<int>> t = async delegate { return 1; };

        return M(delegate { return 1; });
    }
}
```

Before (real, 25c5670):
```csharp
        var x = M(delegate
        {
            return 1;
        },
                  2);
        Func<int> s = static delegate
        {
            return 1;
        };
        Func<Task<int>> t = async delegate
        {
            return 1;
        };

        return M(delegate
        {
            return 1;
        });
```

A (issue's A): unchanged; spec states the scope depth outside holes (today's FMT-INDENT-031 already does).

B (issue's B, recommended):
```csharp
        var x = M(delegate
                  {
                      return 1;
                  },
                  2);
        Func<int> s = static delegate
                             {
                                 return 1;
                             };
        Func<Task<int>> t = async delegate
                            {
                                return 1;
                            };

        return M(delegate
                 {
                     return 1;
                 });
```

Also changes under B (neighbours the maintainer should see in the same answer): an anonymous method inside a block lambda's body, in a raw-string hole and outside one, aligns under its own `delegate` instead of the scope depth relative to the lambda's brace (change doc entry 4); an assignment `g = delegate { … };` puts the block under `delegate` (column 12) instead of the statement column.

Why it looks wrong today: the same anonymous method takes two layouts depending only on whether it stands in a multi-line raw-string hole, against the maintainer's earlier "a hole has no layout rules of its own"; the block lambda in the same position aligns everywhere (FMT-INDENT-028). Requirement IDs: FMT-INDENT-031, FMT-INDENT-028, FMT-INDENT-002.

Recommendation: B — one layout for the construct in every position, matching its block-lambda counterpart.

Code-fix results that change under B (real "before" from `scripts/apply-fix.sh`):
- RH5101 on `return M(⏎    delegate { return 1; }, 2);` → today block in column 16 (one left of `delegate`, neither scope depth 8 nor anchor 17), `2);` in 17; under B block in 17.
- RH5102 on `return M(delegate { return 1; },⏎    2, 3);` → today block in 16; under B 17.
- RH5111 on `g =⏎    delegate { return 1; };` → today `g = delegate` with block in column 8; under B column 12.
- RH5205 on a misaligned block lambda whose body holds `return M(delegate { return 1; });` → today the anonymous method's block at scope depth inside the lambda body; under B under `delegate`.
RH5204 and RH5205 report neither layout (checked with `apply-fix.sh` on B's output: no diagnostic).

### Internal choices

- FMT-INDENT-031 is changed in place rather than retired and replaced: its hole behavior still holds, and the ~30 tests tagged with it keep a valid tag. Alternative: retire it and add FMT-INDENT-039.
- No rule page changes (RH5205's non-goal excludes anonymous methods; no page describes the anonymous-method column).

### Follow-up candidates

- Only if the maintainer picks A: the RH5101 and RH5102 fixes place an anonymous method's block in column 16 for `return M(⏎    delegate { return 1; }, 2);`, while whole-document formatting gives column 8 (FMT-INDENT-002 / FMT-INDENT-031 as today). Input and outputs as above. A code-fix layout that differs from the formatter's; under B the column the fix should give is 17, and a test should pin it.
- Only if the maintainer picks B: RH5205 (statement lambda opening braces should be aligned) leaves anonymous methods out by a non-goal, while the formatter then aligns them like lambdas. Under P11 (`sdd/README.md`) the anonymous method is a different construct from RH5205's lambdas, so a new-rule candidate, not a gap of RH5205.

## thoenissen/Reihitsu#1698

Change doc: `sdd/changes/2026-11/interpolation-close-brace-comment-column.md` (Draft), written on the issue's recommended option A, with the blank line kept.

Premise check: the issue's "Before" no longer matches. At 25c5670 `// e`, `a`, and the closing `}` stand in column 12 (FMT-INDENT-034 continuation lines), not 8; `// f` in column 16, with a blank line in front of it. The cause is FMT-INDENT-006's sentence "a comment that precedes a closing brace MUST be indented one level deeper than the brace" applied to an interpolation's `}` (implemented in `CommentIndentationContributor.AlignCommentsBeforeToken`, which adds one level for every `CloseBraceToken`). The same happens for verbatim (`$@"`) and single-line raw (`$"""{…}"""`) interpolated strings, and for a regular string nested in a multi-line raw literal's hole (`a` in 21, `// f` in 25). A multi-line raw literal's own hole puts `// f` one level deeper than `}}`, in its content's usual column (also when written at the brace column).

The blank line is FMT-BLANK-011's (one blank line above every comment that starts its line behind code); FMT-BLANK-020's example already shows it in front of an interpolation's closing brace in a raw literal. It is specified, so the change keeps it; the issue's option A omitted it.

### Changed requirements

- FMT-INDENT-006 — A comment on its own line takes the column of the code that follows it — changed: the closing brace of an interpolation (`InterpolationSyntax`) of a regular, verbatim, or single-line raw interpolated string is excluded from the one-level-deeper rule, and a comment in front of it takes the brace's column; the interpolation of a multi-line raw string literal keeps the rule. The string the interpolation belongs to decides, not where that string stands. One example added (regular, verbatim, single-line raw, regular nested in a raw hole, raw's own hole unchanged); `Why:` gains one sentence.
- `Uses` of `formatter/indentation.md` gains FMT-BLANK-011.

### Examples for the tests

FMT-INDENT-006 (the new example; its `r` statement is the unchanged negative case).

### Output questions

**1698 · Formatter · comment in front of a regular interpolated string hole's closing brace**

Input:
```csharp
class C
{
    void M(int a)
    {
        var t = $"{
            // e
            a
            // f
            }";
    }
}
```

Before (real, 25c5670; the same when `// f` is written in column 8 or 12):
```csharp
        var t = $"{
            // e
            a

                // f
            }";
```

After, A (recommended):
```csharp
        var t = $"{
            // e
            a

            // f
            }";
```

After, B: unchanged; the spec states the one-level-deeper column for this brace.

After, C (the issue's A literally — column and no blank line; needs an exception to FMT-BLANK-011 for interpolations):
```csharp
        var t = $"{
            // e
            a
            // f
            }";
```

Same rule for `$@"…"`, `$"""{…}"""`, and a regular string nested in a multi-line raw hole (change doc entry 2). Not changed in any option: a multi-line raw literal's own hole (`{{`, `a` and `// f` one level deeper, `}}`).

Why it looks wrong today: the hole's content lines and its `}` share one column (FMT-INDENT-034), and only the comment in front of `}` stands four columns further right. Requirement IDs: FMT-INDENT-006, FMT-INDENT-034, FMT-BLANK-011.

Recommendation: A — one column for every line of the hole, and the blank line follows the rule every other comment behind code follows.

Code-fix results: RH5205 on a misaligned block lambda whose body holds `return $"{⏎f⏎// f⏎}";` puts `// f` four columns right of `}` today (real) and in `}`'s column under A. RH5101 and RH5111 withhold their fix for an argument or value holding the comment (real), before and after. RH5204 reports neither layout (checked).

### Internal choices

- The boundary is the kind of interpolated string (multi-line raw vs. every other), stated by the syntax (`InterpolationSyntax` of an `InterpolatedStringExpressionSyntax` whose start token is or is not the multi-line raw start). Alternative: apply the brace column to multi-line raw holes too — rejected, it would put the comment left of the hole's usual content and change raw output the issue says is consistent.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1567

Change doc: `sdd/changes/2026-11/lambda-return-type-join.md` (Draft), written on the issue's recommended option A, narrowed to what still reproduces.

Premise check: the issue's own examples (`static⏎(int x) => x` as a declaration's value, `async`, and `: static⏎(int x) => x` in a conditional branch) no longer reproduce at 25c5670: FMT-BREAK-041 already joins the gap behind a lambda's modifier, and the formatter outputs `Func<int, int> f = static (int x) => x;` (real). The "explicit return type" case the issue says applies the same way does reproduce: `int⏎    (int x) => x` keeps its line break and blank lines (FMT-BREAK-030), the `(` a continuation line one level deeper than the statement (FMT-INDENT-034), not in the statement's column as the issue says. Inside a list element the gap is already joined (FMT-BREAK-015; `N(int⏎(int x) => x)` → `N(int (int x) => x);`, real).

### Changed requirements

- FMT-BREAK-045 — A lambda's explicit return type joins its parameter list — added (ledger updated). A comment, directive, or disabled text in the gap keeps the line break (FMT-BREAK-001); one space is FMT-SPACE-026's.
- FMT-BREAK-011 — Parameter lists hug their declaration — changed (one sentence naming FMT-BREAK-041 and FMT-BREAK-045 as owners of the gap in front of the `(` behind a modifier or a return type).
- `Uses` of `formatter/line-breaks.md` gains FMT-INDENT-028 and FMT-SPACE-026.

### Examples for the tests

FMT-BREAK-045 (both examples: declaration value, modifier plus return type across a blank line, conditional branch, block body aligned under the joined `(`, expression body; negative: comment in the gap, lambda without return type).

### Output questions

**1567 · Formatter · parenthesized lambda whose explicit return type stands on the line above its `(`**

Input:
```csharp
using System;

class C
{
    void M(bool b)
    {
        Func<int, int> i = int
            (int x) => x;
        Func<int, int> m = int
            (int x) =>
            {
                return x;
            };
    }
}
```

Before (real, 25c5670): unchanged — the input is today's output.

After, A (recommended):
```csharp
        Func<int, int> i = int (int x) => x;
        Func<int, int> m = int (int x) =>
                               {
                                   return x;
                               };
```

After, B: unchanged; the spec states the kept line break (the issue's options B and C both describe today's output now).

Why it looks wrong today: the wrap structures nothing and leaves `(int x) => x` reading like a new expression; the same gap is joined behind a modifier (FMT-BREAK-041) and inside a list element (FMT-BREAK-015). Requirement IDs: FMT-BREAK-030, FMT-BREAK-011, FMT-BREAK-041, FMT-BREAK-015, FMT-INDENT-034.

Recommendation: A.

Code-fix results: RH5111 on `f =⏎    int⏎    (int x) => x;` gives `f = int⏎    (int x) => x;` today (real) and `f = int (int x) => x;` under A. RH5101/RH5102 unchanged (list elements already joined).

### Internal choices

- A requirement of its own (FMT-BREAK-045) rather than an entry of FMT-BREAK-041, whose gaps mirror FMT-UNSUP-017's refused gaps. Declaring a comment between a lambda's return type and its `(` unsupported was considered and not proposed: FMT-BREAK-001 already keeps the line break there, so the shape would delete no special case, and a shape that deletes nothing is not declared (`AGENTS.md`).
- Scope limited to the gap return type → `(`; the modifier → return type gap stays FMT-BREAK-041's.

### Follow-up candidates

- Only if the maintainer picks A: RH5105 (opening parenthesis must be on the declaration line) leaves a parenthesized lambda's `(` unreported by decision ("a lambda has no name in front of its parameter list"); with an explicit return type the `(` then has a token in front of it that the formatter joins it to. Under P11 the lead decides whether this is RH5105's own gap (its construct is the opening parenthesis of a parameter list) or stays excluded; input: `Func<int, int> i = int⏎    (int x) => x;` — not reported today.

## Whole run

- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`: `FMT-BREAK-045: no test covers it (formatter/line-breaks.md:4572)`. Expected until the test implementer tags a test with FMT-BREAK-045. `SpecStructureTests` passes after the `Uses` additions.
- No rule page, resource text, code owner, or `sdd/specs/README.md` row changes.
- Three change docs, one per issue: the issues share an area but not a cause (indentation of anonymous-method blocks; comment column in a hole; a line-break join).
- Existing tests whose expectations the recommended options change (for the test implementer to find with a full formatter run): tests asserting the scope-depth anonymous-method block outside raw holes or inside a lambda's body in a hole (e.g. in `AnonymousMethodHoleAlignmentTests`, `RawStringInterpolationHoleTests`), tests of a comment in front of a regular hole's `}` (e.g. `CommentBeforeBraceBlankLineTests`), and tests keeping a lambda's return-type line break (e.g. `ParenthesizedLambdaLineStartTests`). Not checked one by one.
