Role: skeptic
Commit: c317972
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's "Probable owners" note is a suspected cause and was not used. Every observation below was taken with `scripts/format.sh` on c317972 (no production change since `main`), fixtures in the scratchpad, formatted twice (all stable), deleted afterwards.

## Premise

Real. The issue's reproduction matches the spec author's (01) and my own runs: every line that starts with a constraint, a comma between constraints, or a token inside a constraint of a `where` clause falls back to the block column of the enclosing scope, one level left of `where`; partly wrapped clauses are kept. No requirement owns those lines today (FMT-BREAK-023 and FMT-INDENT-017 cover only the `where` line), so the lane `behavior change` holds.

Callers that reach it:
- CLI and whole-document formatting: any file with a wrapped clause, in every declaration kind that can carry one — class, struct, interface, record, record struct, method (with a body, without one in an interface), local function, delegate (change doc entry 7), and extension block (`extension<T>(T value)⏎    where T : class,⏎    new()` → `new()` at column 4).
- RH5110's code fix: `RH5110…CodeFixProvider.ApplyCodeFixAsync` formats the clause's parent declaration with `ReihitsuFormatter.FormatNodeInDocumentAsync`, so its result inherits the formatter's layout (change doc entry 8).

Severity is as the issue says: a layout defect with no effect on meaning, on every wrapped clause. The reasoning generalizes further than the issue states — see *Breaking inputs*: the same fallback hits a wrap between the type parameter and the colon, wraps inside one constraint, and directives in the clause.

Analyzer side: no conflict (FC-013). RH5110 judges only the `where` keyword (RH5110-001) and excludes line breaks inside a clause by its non-goal. RH6002-001 exempts a comma that starts its line, so the comment-blocked `, new()` that the spec keeps at a line start is not reported. RH5107 checks parameter lists only. No existing test asserts today's wrong column (searched `src/*Test*` for wrapped clauses; the one hit, `WrappedAngleBracketListTests.WrappedGenericConstraintTypeJoins`, has a single constraint and stays valid). P11: wrapped constraint layout is formatter-only; I classify it as a new-rule candidate, not RH5110's gap, because RH5110's spec excludes line breaks inside a clause deliberately and the constraint list is a list construct of its own, as parameter lists have their own rules (RH5105–RH5107) apart from the declaration's position rules. Follow-up candidate 6.

## Mechanism and candidates

_N/A_ as a bug (behavior change). Dispatch facts, for the candidate set:
- Line breaks: `GenericConstraintLineBreakRewriter.GetConstraintClauseAdapter` switches over seven owner types — `ClassDeclarationSyntax`, `StructDeclarationSyntax`, `InterfaceDeclarationSyntax`, `RecordDeclarationSyntax` (record and record struct), `MethodDeclarationSyntax`, `DelegateDeclarationSyntax`, `LocalFunctionStatementSyntax`. `ExtensionBlockDeclarationSyntax` is not among them; consequently FMT-BREAK-023 is not applied to extension blocks today (`extension<T>(T value) where T : class` stays on one line, observed).
- Indentation: `GenericConstraintContributor` dispatches on `TypeParameterConstraintClauseSyntax` itself, so it reaches every owner, extension blocks included; it places only the `where` keyword.
- Constraint kinds (`TypeParameterConstraintSyntax` subtypes): `ClassOrStructConstraintSyntax` (`class`, `struct`, `class?`), `ConstructorConstraintSyntax` (`new()`), `TypeConstraintSyntax` (types, `notnull`, `unmanaged`), `DefaultConstraintSyntax`, `AllowsConstraintClauseSyntax` (`allows ref struct`). All are reachable; each can contain a line break of its own (observed: `new(⏎)`, `allows ref⏎struct`, `class⏎?`, `System.⏎IDisposable`, `global::⏎System.IDisposable`, `IComparable<T,⏎T>`).

## Breaking inputs

Failure classes that reach the change: FC-002 (qualifiers "starts a line", "directly behind the colon", "wrapped"), FC-003 (one vs. several constraints; wrapped vs. not), FC-004/FC-019/FC-020/FC-021 and FC-005 (comments and directives in the colon gap and the comma gaps), FC-010/FC-033 (CRLF), FC-016 (sibling owner types, sibling constraint kinds), FC-026 (several clauses and commas in one declaration), FC-027 (enclosing scopes), FC-028/FC-030 (RH5110 fix through `FormatNodeInDocumentAsync`), FC-032 (a formatter change flips code-fix results).

Class: a line inside a `where` clause, other than the `where` line, whose column no requirement owns. Axes and members, observed on c317972 (all fail = land at the block column of the enclosing scope, unless noted):

| Axis | Members |
|---|---|
| What starts the line | a later constraint (fails); a comma in front of a constraint, kept there by a comment or a directive (fails; d01 below); the first constraint, pushed down by a comment behind the colon (fails) or by a directive behind the colon (fails, see amendment 2); the colon itself after `where T⏎` (fails; output question S2); a later type argument inside a constraint (holds today: FMT-INDENT-015 aligns `string>` under `int`); any other token inside a constraint — `)` of `new(⏎)`, `struct` of `allows ref⏎struct`, `?` of `class⏎?`, a qualified name's right part, the first type argument after `<` (fails; output question S1 and follow-up 3) |
| Declaration kind | class, struct, interface, record, record struct, method with body, method without body, local function, delegate, extension block (all fail) |
| Position of `where` | starts its line (every kind but extension blocks); shares the declaration's line (extension blocks only, since FMT-BREAK-023 does not reach them) |
| Shift of the `where` line by FMT-INDENT-017 | none; right (written at column 4, placed at 8); left (written at 16, placed at 8) — the first constraint's column must be measured after that move |
| Enclosing scope | block namespace (issue), file-scoped namespace, nested type, local function inside a block, a lambda body, or another local function |
| Clauses per declaration | one; two wrapped clauses whose type parameter names differ in width (`TKey`, `TValue`), so each clause has its own column |
| Type parameter name | `T`, a long name, a verbatim identifier `@class` |
| Line ending | LF, CRLF |
| Surface | whole-document formatting; RH5110's fix (`scripts/apply-fix.sh RH5110` runs LF and CRLF) |

Chained anchor: when a multi-line constraint moves onto its own line (change doc entry 6), its inner line aligned by FMT-INDENT-015 must follow the moved constraint (`string>` at 30 = 18 + 12, not at its input column).

Directive input, reachable in multi-targeting code (observed, both lines land at column 4):

```csharp
void M<T>()
    where T : IDisposable
#if !X
        , allows ref struct
#endif
{
}

void N<T>()
    where T :
#if !X
        class,
#endif
        new()
{
}
```

Further candidates for the test implementer, each decided by the spec as written:
- a clause whose only wrap is behind the colon with a comment, and whose constraints are on one line: `where T : // c⏎ class, new()` → wrapped, so `class,` and `new()` each start a line (FMT-BREAK-037 first sentence);
- a comma moved to the previous line with a comment behind it: `where T : class⏎ , // c⏎ new()` → `where T : class, // c` / `new()` (the gap between `class` and `,` holds no comment, so the join is allowed; the comment stays behind the comma);
- a blank line in a comma gap (`class,⏎⏎ new()`): kept, FMT-BREAK-030; a blank line in a gap that is joined goes with the join;
- a one-line clause with a trailing comment (`where T : class, new() // c`) and with an inline block comment between constraints (`class, /* c */ new()`): not wrapped, unchanged;
- a member without a body (`new();`) and an empty type that FMT-STRUCT turns into a semicolon declaration (`struct S<T> where T : class,⏎ new() { }` → `…new();`) — the issue's side observation;
- `allows ref struct` as the last constraint of a wrapped clause.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Formatter, line-break phase (constraint clause gaps) | must change |
| Formatter, indentation phase (constraint and comma lines) | must change |
| Formatter, horizontal spacing (`, ` and ` : ` inside a clause) | must stay |
| Formatter, blank-line phase | must stay (FMT-BREAK-030 hands it the blank lines unchanged) |
| Formatter, structural transforms (empty type → `;`, expression body → block) | must stay; interacts (a wrapped clause in front of `new();` or a converted body) |
| Node-level entry points (`FormatNodeInDocumentAsync`) | must stay; no prediction added (question 10: nothing in the change doc moves a decision into an entry point) |
| RH5110 analyzer | must stay (diagnostic set and location unchanged) |
| RH5110 code fix and Fix All | code must stay; output changes through the formatter (change doc entry 8) |
| RH6002, RH5107 | not involved (no conflict, see *Premise*) |
| `Reihitsu.Core` | not involved |
| Rule page RH5110.md | must stay (its examples hold one-line clauses only) |
| CLI | not involved beyond formatter output |
| Specs | FMT-BREAK-037 and FMT-INDENT-032 need the amendments below |
| Tests | new formatter tests for both requirements; an RH5110 fix test for entry 8; analyzer tests whose fix output contains a wrapped clause must be rerun (FC-032) |

## Simpler option

None that meets the issue's purpose. "Align only, keep the author's line breaks" (the base-list model, FMT-INDENT-016) is the narrower change; it needs no line-break work and fixes the column, which is the issue's main symptom. It leaves partly wrapped clauses, against the issue's first rule; that is exactly question 1 of 01, option B, so the maintainer's answer to it decides. No other existing helper covers constraint lists.

## Output questions

The spec author's five questions (01) stand. Two more, within thoenissen/Reihitsu#1631. Columns as in 01: `where` at 8, the first constraint behind `where T : ` at 18.

### S1 · Formatter — a line break inside one constraint, with no comment holding it

The spec text counts any line break inside a constraint as wrapping the clause; the change doc's *Decisions* says a multi-line constraint is "possible only where a comment or a directive keeps a type argument list wrapped". The second statement is false on c317972: only type argument lists are joined (FMT-BREAK-014); wraps in a qualified name, `global::`, `new(⏎)`, `allows ref⏎struct`, `class⏎?` are kept, and their second line lands at column 4. The readings give different output.

Input:
```csharp
using System.Collections.Generic;

class C
{
    void M<T>()
        where T : class, IDictionary<int,
            string>, new()
    {
    }

    void N<T>()
        where T : class, System.
            IDisposable, new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, string>, new()
    {
    }

    void N<T>()
        where T : class, System.
    IDisposable, new()
    {
    }
}
```

After, A (recommended) — a wrap inside a constraint that no comment or directive holds is joined, and the clause is judged after the join:
```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, string>, new()
    {
    }

    void N<T>()
        where T : class, System.IDisposable, new()
    {
    }
}
```

After, B — type argument lists are joined first (as today); any other wrap inside a constraint stays and wraps the clause; the inner line keeps the block column, since FMT-INDENT-032 places no line inside a constraint:
```csharp
class C
{
    void M<T>()
        where T : class, IDictionary<int, string>, new()
    {
    }

    void N<T>()
        where T : class,
                  System.
    IDisposable,
                  new()
    {
    }
}
```

After, C — the spec text read on the input: every line break inside a constraint wraps the clause, also one that a join removes:
```csharp
class C
{
    void M<T>()
        where T : class,
                  IDictionary<int, string>,
                  new()
    {
    }

    void N<T>()
        where T : class,
                  System.
    IDisposable,
                  new()
    {
    }
}
```

Why A: parameter and argument lists already join wraps inside an element and judge the list after the join (FMT-BREAK-015: `void M(int a, System.⏎IDisposable d)` → `System.IDisposable`, and `Dictionary<int,⏎string>` keeps its list on one line, observed), and A is the only reading that leaves no line at the block column.

### S2 · Formatter — wrap between the type parameter and the colon (01's follow-up candidate 1)

01 excluded this gap from FMT-BREAK-037 and listed it as a follow-up. It is within the issue: the clause is wrapped and, after this run's change as specified, its constraints still sit left of `where`, which is the issue's reported symptom. Whether it joins the run is the lead's scope decision; if it does, this is the question.

Input:
```csharp
class C
{
    void M<T>()
        where T
            : class,
            new()
    {
    }
}
```

Before (`main`):
```csharp
class C
{
    void M<T>()
        where T
    : class,
    new()
    {
    }
}
```

After, A (recommended) — the colon joins the type parameter's line; the clause is then laid out by FMT-BREAK-037 and FMT-INDENT-032:
```csharp
class C
{
    void M<T>()
        where T : class,
                  new()
    {
    }
}
```

After, B — the colon keeps its line, one level deeper than `where`; the constraints align under the first:
```csharp
class C
{
    void M<T>()
        where T
            : class,
              new()
    {
    }
}
```

After, C — the run as specified (follow-up instead):
```csharp
class C
{
    void M<T>()
        where T
    : class,
      new()
    {
    }
}
```

Why A: the gap structures nothing (as FMT-BREAK-015 reasons for wraps inside an element) and leaves one shape for every clause; B is the fallback where a comment (`where T // c`) blocks the join, which the answer should cover too.

Note for 01's question 3: its answer should state that it also applies when a directive, not a comment, keeps the first constraint below the colon (input `N` under *Breaking inputs*); the spec says "comment" only.

## Internal choices

Spec amendments that change no output beyond the questions above (for the spec author):
1. FMT-BREAK-037, last sentence: "a comma that a comment keeps off the previous line" — FMT-BREAK-001 blocks the join for a directive and for disabled text too; name them (FC-005). Same for the examples' caption "comments block a join".
2. FMT-INDENT-032, second sentence: "because a comment behind the colon keeps FMT-BREAK-037 from joining it" — name directives and disabled text as well, or the column of `new()` in input `N` is undecided (FC-005). Recommend widening the wording; the column follows the answer to 01 question 3.
3. FMT-BREAK-037: "A clause that is not wrapped MUST be left as it is" reads as forbidding FMT-BREAK-014's join inside it; say that this requirement changes nothing in such a clause. Together with S1, state whether the clause is judged before or after the joins inside its constraints.
4. Change doc, *Decisions* — "A multi-line constraint splits its clause": correct the claim that a multi-line constraint is possible only through a comment or directive, per S1's answer.
5. Coverage: state that FMT-BREAK-037 and FMT-INDENT-032 apply to extension blocks (every owner of a `where` clause), since FMT-BREAK-023's list ("type, method, local function, or delegate") and the line-break rewriter's owner list do not name them. Recommend stating it; the output for extension blocks then follows the same layout.

The split into two requirements (one per phase) and leaving FMT-INDENT-017 unchanged are sound; I agree with 01's recommendations there.

## Issue goals

- "A clause that fits on one line stays on one line" — change to "a clause written on one line stays on one line": the formatter has no line-length limit (line-breaks.md non-goal). The spec already says so; no visible change.
- The sketch "one level under `where`" for a first constraint wrapped below the colon — a proposal; 01 question 2 asks it as an output question. Nothing to add.
- "A regression test comes first, as for every formatter bug fix" — the lane is behavior change; tests come before the implementation anyway. No change.
- "Probable owners" — suspected cause; drop.
- The side observation (`, new();` on an interface) — covered by change doc entry 4; no separate goal.

## Architecture findings

None of the run's defects shows two owners of one decision; the defect is a missing owner. One fact for the lead: which declarations own `where` clauses is enumerated in two places that already differ from the syntax — `GenericConstraintLineBreakRewriter.GetConstraintClauseAdapter` (seven types, no `ExtensionBlockDeclarationSyntax`) and RH5110-001's list — while the indentation side dispatches on the clause node. FMT-BREAK-037 applies to every clause; if the run's line-break work goes through that enumeration, extension blocks are missed (FC-016). A test on an extension block exposes it either way.

## Follow-up candidates

01's candidates 2 (base list wrapped behind its colon), 3 (first type argument pushed down by a comment), and 4 (`Foo(1⏎ , 2, 3)`) stand; 1 is S2 above unless the lead keeps it out of the run. Further:

5. **Extension blocks and FMT-BREAK-023 / RH5110.** `extension<T>(T value) where T : class` keeps `where` on the declaration line (observed); RH5110-001 does not list extension blocks either, so both surfaces are silent. Output question for the issue: A — `where` starts its own line one level deeper than `extension` (as for types); B — unchanged. Recommendation A. RH5110 would need a behavior change (P11: the rule's own gap, since the construct is the one RH5110 names).
6. **New-rule candidate (P11): layout of the constraints inside a wrapped clause** — one constraint per line, aligned under the first, comma at the end of the line; the counterpart of RH5101/RH5102/RH5107 for constraint lists. Not RH5110's gap: its spec excludes line breaks inside a clause by a non-goal.
7. **A line inside a constraint that a comment keeps wrapped and no requirement places** (`where T : System. // c⏎ IDisposable`, `IComparable< // c⏎ T>`) — lands at the block column; FMT-INDENT-032 deliberately places no line inside a constraint. Merge with 01's candidate 3 if S1 is answered A (then only comment-held wraps remain).
