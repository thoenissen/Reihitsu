Role: skeptic
Commit: 9326571
Verdict: output question — proceed with the spec diff once the change doc is amended as below and the batch carries the amended and added questions

The prompt carried no intended fix or hoped-for verdict. It did frame the review ("the spec author's answers depart from the issue … widen the run"); that framing was treated as a claim to check, not as a finding. Every "today" output below was produced on this commit with `scripts/format.sh` (CLI, whole document) or `scripts/apply-fix.sh` on fixtures in the scratchpad, deleted afterwards. Outputs "under 1B" are derived, not run: FMT-BREAK-031 writes no space where `TokenSeparationUtilities.WouldMergeIntoOneToken` says the pair reads back as written, and no rule of the spec diff decides the pair, so FMT-SPACE-002 keeps the zero gap.

## Premise

Real, and reachable.

- `int P =>1;` → `int P => 1;` while `y =>y` and `1 =>1` stay; `[return :Obsolete]` → `[return:Obsolete]` while `[field :NonSerialized]` and `[method : Obsolete]` keep their space; `N(a:⏎1, b :⏎2)` → `N(a: 1, b : 2)` while `M(b:b, c :1)` stays (probe). Callers: whole-document formatting and the CLI; the property arrow also through the RH5401 fix, the accessor list through the RH5408 fix (both `FormatNodeInDocumentAsync` on the property).
- The one defect with more than cosmetic weight is the fused accessor modifier (change doc entry 10). It is wider than the entry shows and it is reached by a code fix: `scripts/apply-fix.sh RH5408` on `get;⏎protected⏎internal set;` gives `public int P { get; protectedinternal set; }` under LF and CRLF, status `invalid-result`. Whole-document formatting gives `internalprotected set;` for `internal⏎protected set;` and `protectedprivate set;` for `protected⏎private set;` (probe). Both 1A and 1B fix it; it does not depend on any output question.
- The issue's mechanism statements hold as the spec author checked them, with one more writer the inventory names correctly as row 11 (`AttributeTargetUtilities.MergeAttributeListGroup`).

### The citations

| Claim in the change doc / handover | What the issues say | Verdict |
|---|---|---|
| Every arrow gets one space (Q2) — "decided by #958, #1514" | #920 Q2 (maintainer comment 6232450352): lambda `x => x`. Round 9 (#912 comment 6350039592, maintainer accepted all 30 recommendations): item 3 / P34 / #1373 LB-Q8 "every arrow's spacing belongs to FMT-SPACE", recorded on #958 with `get =>_p;` → `get => _p;`; item 25 / #1369 SP-Q2: `1=>2` → `1 => 2`. `sdd/changes/2026-10/phase-tree-round-trip.md`, *The structural transforms write separators only*: the expression-body arrow rule is "an approved follow-up" (#1514). | Holds for every arrow kind. Q2 is covered by maintainer answers; option A would undo them. |
| Attribute-target colon, one space behind (Q3) — "#1383 records" | The maintainer decided only that every target is treated alike (P34, round 9 item 3; SP-Q2 asked "collapse-only or add to #958", nothing about the side behind the colon). The form `[return: Obsolete]`, `[assembly: X]`, `[field: X]` "whatever the input spacing" is the agent-written text of #1383 (`agent-filed`). FMT-PIPE-014 says "spaced alike", which A and B both satisfy. | Overstated. The form behind the colon is undecided; Q3 is a genuine output question. The change doc's `Decisions` and `Why` must not call it decided. |
| Conditional `b ? 1 : 2` (Q4) — "#1440" | #920 Q2, maintainer. | Holds. |
| Name colon `b: b` (Q5) — "#1440" | #920 Q2 decides named arguments only. Named attribute arguments, tuple element names, and subpattern names are the spec author's extension by family. | Holds for arguments; the rest is open (as the spec author says). |
| Relational pattern `x is > 5` (Q9) — "#1440" | Round 9 item 25 (`x is >5` → `x is > 5`). `< 10`, `<= 10` follow. | Holds. |
| `Why` of the change doc: "the pairs it would space differently from today are the ones the rules above decide, most of which the maintainer already decided" | Q3's form, Q6, Q7, Q8's attributed list, Q10 and the members below are decided nowhere. | False as written; see *Breaking inputs*, class C. |
| Scope: completes #958 and #1514, parts of #1383 and #1440 | #958 holds only arrows (comment 6386273777) — fully covered by FMT-SPACE-017. #1514 fully covered. #1383 keeps indexer parameter lists, compound assignment, comment before `]`. #1440 keeps element access, invocation, `::`, initializer braces, explicit-interface dot, constraint colon, `let`, `delegate*`. Milestones differ: #1493 1.2.0, #958/#1383 1.2.1, #1440 vNext. | Scope decision for the lead; nothing visible beyond the questions. |

## Mechanism and candidates

Mechanism: the line-break phase decides the whitespace of a gap it joins (by token kind in `ListElementInteriorJoinRewriter.CreateSeparator`, by a written `SyntaxFactory.Space` in five rewriters, and not at all in `CollapseTokenToSameLine` and `BinaryOperatorLineBreakRewriter`), and FMT-SPACE-002 keeps that decision wherever the spacing phase has no rule, so the output depends on which owner last touched the gap.

Dispatch: `LineBreakPhase.CreateRewriters` and the join sites the inventory lists (rows 1–13, verified with `rg` for `SyntaxFactory.Space`, `RemoveTrailingWhitespace`, `CollapseTokenToSameLine`; no other writer of a same-line gap exists in `LineBreaks/`, `SwitchCaseBraces/`, `BlankLines/`). The list-element join's candidate pairs are every `(GetPreviousToken(), token)` pair inside an `IsListElement` node that `IsOwnedGap` does not exclude: not a separator, closer, operator wrap (`BinaryExpression`, `BinaryPattern`, `ConditionalExpression`, member access, conditional access, member binding, postfix, `IsPatternExpression.IsKeyword`, assignment), attribute-list boundary, non-lambda parameter-list `(`, multi-line literal, interpolation, or a token inside `FindOwningConstruct` (collection expression interior, initializer, anonymous object, switch expression, recursive/list/parenthesized pattern, accessor list, interpolation, query body, indenting scope). Today's separator is none for the `IsTightBefore`/`IsTightAfter` kinds and one space for every other pair; under 1B it is none for every pair that does not fuse.

## Breaking inputs

Failure classes that reach the change: FC-006, FC-022, FC-023 (fusing and literal/verbatim neighbours), FC-002/FC-003 (counter-sides of each new rule), FC-019/FC-021 (comments in the gaps the new rules decide), FC-025 (arrow kinds), FC-027 (nesting), FC-028 (node-level entry points), FC-010 (LF/CRLF idempotency), FC-013 (checked: no conflict), FC-015/FC-016 (one separator owner), FC-031 (stale comments), FC-032 (code-fix outputs change through the formatter).

### Class A — a join fuses two tokens (FC-006)

Axes: joiner × fusing pair. Every member fails today only where no later spacing rule separates the pair.

| Joiner | Fusing pair | Final output today | Status |
|---|---|---|---|
| `AccessorListLayout` via `CollapseTokenToSameLine` (CLI, whole document, RH5408 fix) | `protected⏎internal`, `internal⏎protected`, `private⏎protected`, `protected⏎private` before an accessor keyword | `protectedinternal set;`, `internalprotected set;`, `protectedprivate set;` (probed), `privateprotected set;` (probed) — does not compile | **fails**; change doc entry 10 names one of the four |
| `BinaryOperatorLineBreakRewriter` | `-`/`-b`, `+`/`+b`, `+`/`++b`, `&`/`&b`, `/`/`*p`, `as`/`string`, `is`/`int` | final output repaired by FMT-SPACE-010; phase text fused (FMT-PIPE-021) | phase-level only |
| `ListElementInteriorJoinRewriter` | `out`/`var`, `await`/`x`, `-`/`-x`, Unicode-escape identifier behind a keyword | already guarded | holds |
| Chain, ternary, `when`, assignment, property arrow, declaration `;`, list delimiters | `?`/`.5`, `1`/`.ToString` and the like | read back as written | holds |

Checked and dropped: two adjacent `>` that the parser (not the lexer) would combine into a shift operator. The helper lexes, so it would not see it, but no join produces `>` next to `>`/`>=` in an expression context — type-argument closers are joined in type context only, and binary-operator wraps are owned. No caller reaches it.

### Class B — a same-line gap the line-break phase rewrites although nothing is joined

| Writer | Today | Under the spec diff |
|---|---|---|
| `PropertyLayoutLineBreakRewriter` | `int P =>1;` → `int P => 1;` | FMT-SPACE-017 |
| `AccessorListLayout` | `int G { get;set; }` → `{ get; set; }` (probed) | FMT-SPACE-022 |
| `AttributeTargetUtilities.MergeAttributeListGroup` | writes one space behind the merged `]` and behind a token left touching a joined token — that is a join, so FMT-BREAK-031's first sentence demands no space there | see *Internal choices* |

Complete; no other writer found.

### Class C — a pair the join spaces today, that no rule of the spec diff decides, and that reads back without a space

Under 1B every member becomes tight. The change doc lists `static (`, `int (`, `in (`, `var (`, `stackalloc [` (entry 9) and says every other output stays. That is not true. The full set, derived from the candidate pairs above minus the tight kinds, the fusing pairs, the owned gaps, and FMT-SPACE-003/-009/-010/-012/-016 to -023:

| Family | Member (input wrapped at ⏎ inside an argument or parameter) | Today (probed) | Under 1B |
|---|---|---|---|
| Lambda modifier + `(` | `static⏎(int v) => v` | `static (int v) => v` | `static(int v) => v` (entry 9) |
| | `async⏎(x) => x` | `async (x) => x` | `async(x) => x` — **missing** |
| Argument modifier + `(` | `in⏎(a)`, `ref⏎(a)` | `in (a)`, `ref (a)` | `in(a)` (entry 9), `ref(a)` — **missing** |
| Argument modifier + prefix operator | `out⏎*p`, `ref⏎*p` | `out *p`, `ref *p` | `out*p`, `ref*p` — **missing** |
| Keyword + verbatim identifier | `await⏎@t`, `ref⏎@x`, `out⏎@x`, `in⏎@x`, `static⏎@x => …`, `async⏎@x => …` | `await @t` (probed), the others alike | `await@t`, `ref@x`, … — **missing** (FC-023) |
| `await` + `(` (async method) | `await⏎(t)` | `await (t)` | `await(t)` — **missing** |
| Designation | `var⏎(x, y)` | `var (x, y)` | `var(x, y)` (entry 9) |
| `stackalloc` + `[` | `stackalloc⏎[] { 1 }` | `stackalloc [] { 1 }` | `stackalloc[] { 1 }` (entry 9) |
| Pattern `not` + operand | `not⏎"x"`, `not⏎'c'`, `not⏎@o`, `not⏎-1`, `not⏎+1`, `not⏎~1`, `not⏎> 5`, `not⏎< 5`, `not⏎>= 5`, `not⏎<= 5` | `not "x"`, `not 'c'`, `not @o`, `not -1`, `not > 5` (probed), the others alike | `not"x"`, `not'c'`, `not@o`, `not-1`, `not> 5` — **missing** (FC-022, FC-023) |
| Query first clause | `from x in⏎(xs)`, `from x in⏎"abc"` | `in (xs)`, `in "abc"` | `in(xs)`, `in"abc"` — **missing** |
| Explicit lambda return type + `(` | `int⏎(x) => x`, `int[]⏎(x) => x`, `List<int>⏎(x) => x`, `int?⏎(x) => x`, `(int, int)⏎(x) => …` | `int (x)`, `int[] (x)`, `List<int> (x)` (probed) | `int(x)` (entry 9), `int[](x)`, `List<int>(x)`, `int?(x)`, `(int, int)(x)` — **missing** |
| Non-word token + `with` | `p.Q()⏎with { … }`, `xs[0]⏎with { … }`, `x!⏎with { … }`, `new C { }⏎with { … }` | `p.Q() with` (probed) | `p.Q()with`, `xs[0]with`, `x!with`, `}with` — **missing** |
| Function-pointer `*` + calling convention | `delegate*⏎unmanaged[Cdecl]<int, void>`, `delegate*⏎managed<…>` | `delegate* unmanaged[Cdecl]<int, void>` (probed) | `delegate*unmanaged[Cdecl]<int, void>` — **missing** |
| Type + range variable in a `from` clause | `from int?⏎x in xs` | `from int? x in xs` (probed) | `from int?x in xs` — **missing**; FMT-SPACE-021 does not list the `from` clause |

Not members (checked): positional pattern `C⏎(1)`, `not⏎(1 or 2)`, recursive and list patterns, `x⏎switch`, `x⏎is`, `x⏎as`, `x⏎with`, query clauses after the first `from`, `delegate⏎(`, `delegate⏎{`, `new⏎(`, `new⏎[]`, `throw⏎…`, `typeof`/`default`/`sizeof`/`checked`/`nameof`/`__arglist` `(`, a parameter's `=` (FMT-SPACE-010 covers `EqualsValueClause` and `NameEquals`), `[param:⏎Obsolete]` (the attribute starts its own list element and is not joined, probed). Each is owned, tight today, fused, or decided by a rule.

### Class D — rules keyed by a family that leave members of the same family out (P34, FMT-PIPE-014)

The new rules apply to one-line input too, so a family member the rule omits keeps the author's gap while its siblings are normalized:

- FMT-SPACE-021 (type and declared name): names parameters, tuple elements, variable declarations, declaration expressions, declaration patterns. Omits `foreach (int?x in xs)` (probed: unchanged today and under the diff), `catch (Exception?e)`, the `from`/`join` range variable (reachable through a join, class C). Member types (return type, property type) are left unspecified on purpose.
- FMT-SPACE-020 (`when`): names the `case` guard and the switch-expression arm. Omits the catch filter `catch (E e)when (x)`. Not reachable by a join; one-line only.

### Class E — comments in the gaps the new rules decide (FC-019, FC-021)

Every one of FMT-SPACE-016 to -023 needs both comment sides on one line: a comment the left token carries (FMT-SPACE-013) and a `/** */` the right token carries (FMT-SPACE-014). The rules with "no space before" (016 before the colon, 019 before the colon) hit FMT-SPACE-014's second sentence: `[return /** x */ :A]` must keep the gap in front of the comment, lose the gap between comment and `:`, and get one space behind `:`. The spec author asks this for three rules; it applies to all eight. Joins are refused with a comment in the gap (`WouldJoinAcrossUnjoinableTrivia`), so the joined side needs one test per joiner that the refusal still holds after the separator change.

### Candidate inputs for the test implementer

- Class A: all four accessor modifier pairs through whole-document formatting and through the RH5408 fix (`apply-fix.sh` reproduces `invalid-result` today), LF and CRLF; phase-level text of the binary-operator members.
- Class C: every row, joined, with the final output the maintainer's answer to OQ-1 gives, and the same pair written on one line next to it (the "alike" claim of FMT-SPACE-002).
- Class D: `foreach`, `catch`, `from` members per the answer to OQ-2; catch filter per OQ-3.
- FC-025: the arrow of every kind — method kept as expression by `RequiresExpressionBodyPreservation`, property, indexer, operator, conversion operator, constructor, finalizer, local function, accessors `get`/`set`/`init`/`add`/`remove` hand-written and created by FMT-STRUCT-009/-014, simple, parenthesized, attributed, `static`/`async` lambdas, lambda with explicit return type, switch arm with and without `when`.
- FC-027: each new rule inside an argument, an attribute argument, an initializer, an interpolation hole (`$"{(b?1:2)}"`), a nested argument list.
- FC-028: property arrow and accessor list through `FormatNode` and `FormatNodeInDocumentAsync` (RH5401, RH5408), and a list-element root (`RootConstruct.ListElement`).
- FC-002/FC-003 counter-sides as the spec author lists them, plus: nullable `?` directly before a conditional `?` is the parser's call (`x is int? ? 1 : 2` stays parsed as written), `int? ?`-free; `case` label colon after a property pattern (`case { A: 1 }:`).
- FC-032: the full analyzer test project, because every code fix formats through the formatter.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `HorizontalSpacing` rules (`SpacingPolicy`, new rule types, `KeywordSpacingRule`'s attribute-target branch) | must change |
| `LineBreaks` rewriters and utilities (rows 1–10, 12 of the inventory) | must change |
| `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform` (created `=>` spaces) | must change; no code fix runs them (`ReihitsuFormatter.CodeFixDisabledStructuralTransforms` disables them), so no fix output depends on their spaces |
| `Reihitsu.Core.TokenSeparationUtilities` | must stay; becomes the separator owner of every join |
| `Reihitsu.Core.AttributeTargetUtilities.MergeAttributeListGroup` (shared with the merge code fixes) | must stay (internal choice below) |
| Analyzers | not involved: no rule reports the pairs; RH6001 covers statement keywords only, RH6009/RH6010 agree with FMT-SPACE-022, RH6015 with `int? a`, RH6021 exempts name, conditional, attribute-target colons (FC-013 checked, no conflict) |
| Code fixes | not changed; outputs change through the formatter wherever the fixed node holds a changed pair; RH5408's `invalid-result` on split accessor modifiers is repaired |
| Rule pages | not involved |
| CLI | not involved (output through the formatter) |
| Specs | `horizontal-spacing.md`, `line-breaks.md`, `structural-transforms.md`; FMT-BREAK-024's "MUST stay as written" for a one-line attributed list now gets its spacing changed by FMT-SPACE-022 — reword to "stays on its line" (internal) |
| Comments | `NoSpaceSpacingRule` remark (spec author, item 8), `LineBreakAssignmentRewriter`'s comment, `CreateSeparator`'s summary, `KeywordSpacingRule`'s attribute-target comment (FC-031) |
| `AGENTS.md` phase-ownership rows | must change with the guard names (spec author's internal choice) |

## Simpler option

None that meets the issue's purpose. The narrowest defensible change is the lexer guard on every join (fixes class A, entry 10, including RH5408) plus FMT-SPACE-016/-017 (the two FMT-PIPE-014 families the issue names); it leaves the join's per-kind table, which is the issue's architecture finding. The guard alone is a bug fix against FMT-PIPE-003 and needs no output question.

## Output questions

Amendments to the spec author's batch:

- **Q10 is incomplete.** Replace its input and outputs with OQ-1 below; the change doc's entry 9, its `Unchanged` paragraph, its `Why` ("the pairs it would space differently … are the ones the rules above decide"), and the handover's "Why B" of Q1 must list or stop denying the members of class C.
- **Q3 is not decided by the maintainer** in the form B shows; keep the question, drop "the form #1383 records" as a decision.
- **Q2, Q4, Q5 (arguments), Q9 are covered by maintainer answers** (#920 Q2; #912 round 9 items 3 and 25; the phase-tree-round-trip change doc for #1514). Whether to ask them for confirmation or record them as decided is the lead's call; option A of Q2 contradicts those answers.
- **Entry 10 is incomplete** (not a question — a compile fix demanded by FMT-PIPE-003): list the four modifier pairs and the RH5408 path.

**OQ-1 · Formatter — a joined keyword, lambda return type, `with`, or function-pointer `*` (with 1B)**

Input:
```csharp
using System.Linq;
using System.Threading.Tasks;

class C
{
    async Task M(int a, int[] xs, object o, Task t, P p)
    {
        N(async
          (x) => x);
        N(await
          (t));
        N(ref
          (a));
        N(o is not
          "x");
        N(o is not
          > 5);
        N(from x in
          (xs) select x);
        N(List<int>
          (x) => x);
        N(p.Q()
          with { X = 1 });
    }

    unsafe void F(delegate*
                  unmanaged[Cdecl]<int, void> f, int* p)
    {
        N(out
          *p);
    }
}
```

Before (`main`):
```csharp
        N(async (x) => x);
        N(await (t));
        N(ref (a));
        N(o is not "x");
        N(o is not > 5);
        N(from x in (xs) select x);
        N(List<int> (x) => x);
        N(p.Q() with ⏎ { … });   // `with` joined, initializer laid out below
    unsafe void F(delegate* unmanaged[Cdecl]<int, void> f, int* p)
        N(out *p);
```

After, A — no rule; the join's minimal separator stays:
```csharp
        N(async(x) => x);
        N(await(t));
        N(ref(a));
        N(o is not"x");
        N(o is not> 5);
        N(from x in(xs) select x);
        N(List<int>(x) => x);
        N(p.Q()with ⏎ { … });
    unsafe void F(delegate*unmanaged[Cdecl]<int, void> f, int* p)
        N(out*p);
```
The same holds for `static`/`in`/`var`/`stackalloc` (entry 9), `await @t`, `not 'c'`, `not -1`, `not @o`, `in "abc"`, `int[] (x)`, `int? (x)`, `xs[0] with`, `x! with`.

After, B (recommended) — one space behind the keywords `static`, `async`, `ref`, `out`, `in`, `await`, `var` (designation), `stackalloc`, `not` (pattern), and the `in` of a `from` clause; one space on each side of `with`; one space between an explicit lambda return type and its `(`; one space behind a function pointer's `*` in front of `managed`/`unmanaged`: today's joined output is kept for every member, and the same pairs written on one line get the space too (`await(t)` → `await (t)`, `o is not"x"` → `o is not "x"`).

Why B: A writes forms such as `not"x"`, `not> 5`, `p.Q()with`, and `out*p` that nobody decided and that read as a different construct, and B moves the per-kind choice the join makes today into the spacing phase instead of dropping it, which is the issue's purpose; the pick changes to A if the maintainer prefers fewer rules for inputs this rare.

**OQ-2 · Formatter — a type and the name it declares, beyond the declarations FMT-SPACE-021 lists (with 7B)**

Input:
```csharp
using System;
using System.Linq;

class C
{
    void M(int?[] xs)
    {
        foreach (int?x in xs)
        {
        }

        try
        {
        }
        catch (Exception?e)
        {
        }

        N(from int?
          y in xs select y);
    }
}
```

Before (`main`): `foreach (int?x in xs)`, `catch (Exception?e)`, `N(from int? y in xs select y);`.

After, A — FMT-SPACE-021 as written: `foreach (int?x in xs)`, `catch (Exception?e)`, `N(from int?y in xs select y);`.

After, B (recommended): `foreach (int? x in xs)`, `catch (Exception? e)`, `N(from int? y in xs select y);`.

Why B: the `foreach` variable, the catch variable, and the range variable are declared names like the others the rule lists, and under A the joined `from` clause loses its space.

**OQ-3 · Formatter — the `when` of a catch filter (with 6B)**

Input:
```csharp
try
{
}
catch (System.Exception e)when (e != null)
{
}
```

Before (`main`): unchanged.

After, A — FMT-SPACE-020 as written: unchanged.

After, B (recommended): `catch (System.Exception e) when (e != null)`.

Why B: every `when` guard then reads alike; the space behind `when` stays undecided in both.

## Internal choices

- **FMT-BREAK-031 and the attribute-list merge.** `MergeAttributeListGroup` writes a space behind the merged `]` where the line-break phase joins the merged list with the declaration's next token. That is a join, so FMT-BREAK-031's first sentence demands no space; the spec author's reading ("a gap that a merge creates by removing tokens") does not hold for this gap. Output is identical either way (FMT-SPACE-003). Recommend naming the merge as a recorded exception in FMT-BREAK-031, pointing at thoenissen/Reihitsu#1492, as FMT-STRUCT-026 does for the field split; the alternative, stripping the space, changes a helper the merge code fixes share.
- **FMT-BREAK-024 wording** "MUST stay as written" for a one-line attributed accessor list: reword to "stays on its line", since FMT-SPACE-022 now changes its spacing.
- **The issue's "Done when" item 1** ("No line-break rewriter writes a space") is met by the lexer-decided separator only if the merge exception above is recorded.
- **Ask or record Q2/Q4/Q5-arguments/Q9** — see *Output questions*; recommend listing them in the batch as decided, with the citations, so one answer confirms them.

## Issue goals

- "The line-break phase joins with one neutral separator" — the separator is the open Q1; the issue's own `Blocked by` names B3's minimal separators as the contract it follows, which supports 1B. Changes visible output either way.
- "Every arrow keeps the author's spacing until #958" — drop: maintainer answers decide one space for every arrow (citations above). Visible; Q2.
- "Every attribute-target colon follows the `return:` rule (no space before)" — keep; the added "one space behind" is the open Q3. Visible.
- Desired output `[field:NonSerialized]`, `int P =>1;` — superseded by Q2/Q3. Visible.
- Done when "`SpacingPolicy` is consulted only by the spacing phase" — drop: already true on `main` (spec author's row 7). Not visible.
- Done when "regression tests show … each arrow keeping its author's spacing" — change to the answer of Q2. Visible through Q2.
- Done when "`NoSpaceSpacingRule`'s remark … since the chain rewriter no longer closes any gap" — keep as a comment fix; the premise is wrong (the chain rewriter still closes gaps that hold a line break), as the spec author states. Not visible.

## Architecture findings

- The issue itself: the line-break phase (`ListElementInteriorJoinRewriter`, `LineBreakAssignmentRewriter`, `TernaryLineBreakRewriter`, `SwitchCaseWhenLineBreakRewriter`, `PropertyLayoutLineBreakRewriter`, `AccessorListLayout`) and `HorizontalSpacingPhase` own the whitespace of the same gaps; input `N(name:⏎"x")` vs `N(name:"x")`.
- Within the run's change: the "two tokens read as one" decision has one owner (`TokenSeparationUtilities`) for the list join and the transforms, but `CollapseTokenToSameLine` and `BinaryOperatorLineBreakRewriter` join without asking it (FC-015); input `get;⏎protected⏎internal set;` → `protectedinternal` (CLI and RH5408). FMT-BREAK-031 as written removes this second path.
- `AttributeTargetUtilities.MergeAttributeListGroup` is a third writer of a joined gap, shared with code fixes; tracked by thoenissen/Reihitsu#1492 (see *Internal choices*).

## Follow-up candidates

- **Wrapped `with` expression loses its column** — `var w = new C()⏎        with { };` gives `with` at the statement's column, the initializer below it (probed). Same class as thoenissen/Reihitsu#953 (continuation lines no requirement owns); no new issue, a comment there at most.
- The spec author's two candidates stand: `[return⏎:Obsolete]` in a declaration's attribute list is not joined (outside the run: line breaks), and wrapped pattern parts lose their column (thoenissen/Reihitsu#1542 covers parenthesized and positional patterns; the property-pattern member `{ X:⏎1 }` belongs to the same issue or to #953).
- The space behind `when` (`when(x)`) and behind a catch filter's `when` stay undecided in every option; #1369 SP-Q2's input carried `catch (Exception ex) when(ex != null)` and round 9 did not decide it. Candidate for #1440's list:

  **Formatter — the space behind `when`.** Input `case int i when(i > 0):` and `catch (E e) when(e != null)`. Before: unchanged. A: unchanged. B (recommended): `when (i > 0)`, `when (e != null)`, like `if (`. Why B: `when` introduces a condition like `if` and `while`.
