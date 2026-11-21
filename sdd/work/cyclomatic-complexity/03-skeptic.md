Role: skeptic
Commit: 601c1ad
Verdict: output question

Task: maintainer answer 12 — compare RH2009, as it stands after Maintainer answers 1–11, with the published definition, G. Ann Campbell, *Cognitive Complexity — a new way of measuring understandability*, SonarSource, version 1.7 (29 August 2023), local copy in the session scratchpad. Page numbers are the paper's. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

"RH2009" below means `sdd/specs/analyzer/RH2009.md` at ff78b28 **as amended by the answers**: the spec text still predates answers 3, 7, 8, 9, and 10 (it counts local functions and lambdas toward the holder, scores recursion 0, nests a chained `?:`, and counts combinators in case labels), so where the spec and an answer disagree, the answer is the one compared.

## Premise

The paper defines a score, not a rule: it sets no threshold and has no exception for bodies without nesting (both are the maintainer's: clarification 2 / answer 1, clarification 5). Its scoring has three parts (p5): structural increments (with a nesting increment), hybrid increments (`else if`, `else`: no nesting increment, but they raise the nesting level), and fundamental increments (no nesting increment). Appendix B (p16) is its "comprehensive listing".

Cross-check of the paper's own worked examples under RH2009's rules: p17 (`overriddenSymbolFrom`, 19), p18 (`addVersion`, 35 — `synchronized` neither scores nor nests, as `lock` in RH2009), and p19 (`toRegexp`, 20) give the paper's totals under RH2009 unchanged. p20 (YUI `save`, 20) gives 2 under RH2009 (the `if` and the `||` outside the callbacks), because answer 10 drops lambda bodies; p10 (`sumOfPrimes`, 7) has a labeled `continue`, which C# lacks.

## Comparison

| # | Paper rule | Paper (page) | RH2009 after the answers | Verdict |
|---|---|---|---|---|
| 1 | Method itself | "Cognitive Complexity does not increment for methods" (p6); "top-level methods are ignored" (p9) | Body starts from 0; the member adds nothing and does not nest | Match |
| 2 | Null-coalescing | "Cognitive Complexity ignores null-coalescing operators" (p6, example `a?.myObj`) | `??`, `??=`, `?.`, `?[]` score nothing, do not nest | Match |
| 3 | Loops | Structural: "`for, while, do while`" (p6); B1–B3 add `foreach` (p16) | `for`, `foreach`, `await foreach`, `while`, `do`: 1 + nesting level; body is nested | Match |
| 4 | `if`, `?:` | Structural: "ternary operators, `if`, `#if`, `#ifdef`" (p6); B3 nesting increment for "`if`, ternary operator" (p16) | `if` and `?:`: 1 + nesting level; branches nested, conditions not | Match, except a `?:` chained in a false branch (row 6) |
| 5 | `else if`, `else` | Hybrid: "No nesting increment is assessed for these structures because the mental cost has already been paid when reading the if" (p6); they "do increase the nesting count" (p5, B2 p16) | 1 each, no nesting increment; every branch of the chain is the chain's nested part; `else { if … }` nests | Match (the COBOL exception, p13, applies only to languages without `else if`) |
| 6 | Chained `?:` | Ternary is structural with a nesting increment and raises the nesting level (B2, B3, p16); no chain concept | A `?:` in another's false branch scores 1, no nesting increment (like `else if`) | **Departs — answer 8.** Open sub-case: a parenthesized false branch, question 5 |
| 7 | `#if` / `#ifdef` | Structural increment (p6); p9 example: "`#if DEBUG // +1 for if`", and it raises the nesting of what it holds. Appendix B (p16) does **not** list it | Directives score nothing; excluded code counts as absent (SHARED-DIAG-008) | **Departs — no answer.** Question 1 (the paper contradicts itself; Appendix B agrees with RH2009) |
| 8 | `switch` | "A `switch` and all its `case`s combined incurs a single structural increment" (p7); B2/B3 nest it (p16) | `switch` statement and `switch` expression: 1 + nesting level, once; sections/arms nested | Match (switch expression is C#-only, scored like the statement) |
| 9 | `catch` | "each `catch` clause results in a structural increment … only adds one point … no matter how many exception types are caught" (p7) | Each `catch` clause 1 + nesting level; its block nested; a `when` filter scores nothing itself | Match |
| 10 | `try`, `finally` | "`try` and `finally` blocks are ignored altogether" (p7); no nesting for `try` (p9) | Score nothing, do not nest | Match |
| 11 | Logical-operator sequences | "a fundamental increment for each sequence of binary logical operators" (p7); "increments for each new sequence of like operators"; `a && b && c \|\| d \|\| e && f` = 3, `a && !(b && c)` = 2 (p8); counted everywhere, "variable assignments, method invocations, and return statements" (p8) | `&&`/`\|\|`: first operator and each change, per tree; `!` (and `not`) starts a new tree; counted in every expression, `when` clauses included (decision 3) | Match (both paper examples give 3 and 2) |
| 12 | Non-short-circuit boolean operators | "only concerned with binary boolean operators" (change log 1.1, p21); "all sequences of binary boolean operators" (p8) | `&`, `\|`, `^` score nothing, also on `bool` operands | **Departs — no answer** (reading of "binary boolean operators"). Question 3 |
| 13 | Recursion | "a fundamental increment for each method in a recursion cycle, whether direct or indirect" (p8; B1 "each method in a recursion cycle", p16) | Answer 7: "each call of the member itself scores 1" | **Departs twice — answer 7 chose per call, but its option said this is what the paper does.** The paper charges once per method, and indirect cycles too. Question 2 |
| 14 | Jumps | "`goto` adds a fundamental increment, as do `break` or `continue` to a label …; no other jumps or early exits cause an increment" (p8) | Every `goto` form 1 (`goto label`, `goto case`, `goto default`); `break`, `continue`, `return`, `throw`, `yield` nothing; C# has no labeled `break`/`continue` | Match by the letter. Note: Java fall-through scores 0, and C# spells it `goto case`, which scores 1 — the spec's choice, consistent with "goto adds" |
| 15 | Nesting level | Nesting is raised by `if`, `else if`, `else`, ternary, `switch`, loops, `catch`, and "nested methods and method-like structures such as lambdas" (B2, p16) | Same list without lambdas and nested functions (rows 16–17); `lock`, `using`, `fixed`, `checked`, `unsafe`, plain blocks do not nest | Match for statements (p18: `synchronized` does not nest); departs for functions |
| 16 | Lambdas | "no structural increment for lambdas … such methods do increment the nesting level when nested inside other method-like structures" (p9; example `myMethod2` = 2) | Answer 10: a lambda body does not count toward the holder at all and is not judged on its own | **Departs — answer 10** (follow-up candidate already listed). Anonymous methods: the spec groups them with lambdas, answer 10 names lambdas only — question 4 |
| 17 | Nested methods | Same as lambdas (p9); Python example: a nested function's `if` "+1 structure, +1 nesting" counts toward the outer function (p15) | Answer 3: a local function is judged only on its own, nesting from 0, and adds nothing to the holder | **Departs — answer 3** |
| 18 | Compensating usages | COBOL `else … if`, JavaScript declarative outer functions, Python decorators (p12–p15) | None needed: C# has `else if`, classes, and no decorators | Not applicable |
| 19 | Threshold | None defined | Limit 15, `design.maximumMethodComplexity`, one limit for test code | Rule-level, not scoring — clarification 2 / answers 1, 6 |
| 20 | No-nesting exception | None: every method is scored the same way | A body with no nesting increment is never reported | Rule-level — clarification 5 |
| 21 | Class and application totals | Scores sum to class and application level (p10) | Per member only | Not addressed by the spec; out of scope for a member rule, no question |
| 22 | Structure inside a condition | Not addressed (examples score a `?:` in an argument by the nesting of where it stands, p20) | A structure in an `if`/loop/`?:` condition or a switch's governing expression is not in that construct's nested part | Paper silent; RH2009 decides it. Consistent with p19 (`a \|\| b ? 1 : 0` at nesting 0) |

C# constructs the paper does not cover, and how RH2009 treats them:

| Construct | RH2009 after the answers |
|---|---|
| `and` / `or` pattern combinators | Score like `&&`/`\|\|` (answer 5) in `is` patterns and `when` clauses; nothing in a case label's or switch arm's pattern (answer 9) |
| `not` pattern | Scores nothing; breaks a run of combinators like `!` (decision 3) |
| `switch` expression | Like a `switch` statement: one increment plus nesting; arms nested |
| `when` on a case label, switch arm, or `catch` | Scores nothing itself; its `&&`/`\|\|` and `is … or …` count (decision 3, answer 9) |
| `??`, `??=`, `?.`, `?[]`, `!` (null-forgiving) | Nothing (the paper's null-coalescing rule) |
| Query expressions (`from … where … select`) | Clauses neither score nor nest; operators and `?:` inside them count at the level where the query stands (decision 3). The same code as `.Where(x => …)` scores nothing at all now (answer 10) — the two spellings of one query differ |
| `using`, `lock`, `fixed`, `checked`, `unchecked`, `unsafe`, `using` declarations | Nothing, no nesting (the paper's `synchronized`, p18) |
| Type, property, list, relational patterns without combinators; `throw` expressions | Nothing |
| `await foreach`, `foreach` with deconstruction | Like `foreach` |
| Accessors, operators, expression-bodied members | Each judged as its own method (RH2009-002); the paper does not distinguish them |

## Mechanism and candidates

_N/A_ — no bug; a comparison of a specification with a published definition.

## Breaking inputs

_N/A_ for this comparison; the inputs of `02-skeptic.md`, *Breaking inputs*, stand, with these additions once the questions are answered: a member that calls itself twice (question 2), a pair of mutually recursive members (question 2), `&`/`|`/`^` on `bool` next to the same operator on `int` (question 3), an anonymous method inside a nested part (question 4), and a parenthesized `?:` as another `?:`'s false branch (question 5).

## Surfaces

Unchanged from `02-skeptic.md`. Question 2 option C would add a compilation-end analysis to the analyzer; question 3 option B needs the semantic model (operand type) in the score.

## Refuse option

Keeping every row as RH2009 has it today (the recommendations below except question 2) costs nothing beyond the spec rewrite already due for answers 3 and 7–10. The cheapest answer to question 2 is A (once per member, direct only); C is the one choice here that adds a mechanism.

## Output questions

Five departures or gaps that no answer covers. Every input compiles with the .NET 11 SDK (scratch library project, `dotnet build`: Build succeeded, 0 warnings). Scores are derived by hand from RH2009 as amended by the answers; "Before (`main`)" is always "not reported", because RH2009 does not exist on `main`.

**1 · RH2009 · Methods should not exceed the complexity limit — `#if` inside a body**

Input, with `{ "design": { "maximumMethodComplexity": 1 } }`, in a build that defines `DEBUG`:
```csharp
class C
{
    int M(int value)
    {
#if DEBUG
        if (value > 0)
        {
            value++;
        }
#endif

        return value;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — directives score nothing: `if` 1, no nesting increment:
```csharp
int M(int value)   // not reported
```

After, B — `#if` scores like `if` and nests what it holds (paper p6, p9): `#if` 1, `if` 2 (nesting 1) = 3:
```csharp
int {|RH2009:M|}(int value)   // "This member has a cognitive complexity of 3, which exceeds the limit of 1."
```

Why A: the paper's own comprehensive list (Appendix B) leaves `#if` out, and a build reads only one branch, which is how every Reihitsu rule already treats conditional code.

**2 · RH2009 · Methods should not exceed the complexity limit — recursion, per call or per member, direct or indirect**

Answer 7 chose "each call of the member itself scores 1"; the option presenting it said this is how the published definition charges recursion. The paper charges "each method in a recursion cycle, whether direct or indirect" (p8): once per method, not per call, and mutual recursion too.

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    int Fib(int n)
    {
        if (n > 1)
        {
            if (n > 2)
            {
                return Fib(n - 1) + Fib(n - 2);
            }
        }

        return n;
    }

    bool IsEven(int n)
    {
        if (n > 0)
        {
            if (n > 1)
            {
                return IsOdd(n - 1);
            }
        }

        return n == 0;
    }

    bool IsOdd(int n) => n != 0 && IsEven(n - 1);
}
```

Before (`main`): not reported.

After, A (recommended) — 1 once for a member that calls itself, direct recursion only (the paper, for direct recursion):
```csharp
int {|RH2009:Fib|}(int n)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
bool IsEven(int n)          // 3, not reported
bool IsOdd(int n)           // 1, no nesting, not reported
```

After, B — answer 7 as worded, 1 for each call of the member itself:
```csharp
int {|RH2009:Fib|}(int n)   // "This member has a cognitive complexity of 5, which exceeds the limit of 3."
bool IsEven(int n)          // 3, not reported
bool IsOdd(int n)           // 1, not reported
```

After, C — the paper exactly, 1 once for every member in a recursion cycle, direct or indirect:
```csharp
int {|RH2009:Fib|}(int n)       // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
bool {|RH2009:IsEven|}(int n)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
bool IsOdd(int n)               // 2, no nesting, not reported
```

Why A: the reader learns once that a member recurses, however many calls it has, and indirect cycles would need a call graph over the whole compilation, so a member's score would change when another member, perhaps in another file, is edited.

**3 · RH2009 · Methods should not exceed the complexity limit — `&`, `|`, `^` on `bool`**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    bool M(bool a, bool b, int flags)
    {
        if (a)
        {
            if (a & b)
            {
                return (flags & 4) != 0;
            }
        }

        return false;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — only `&&`, `||`, `and`, `or` count: `if` 1, `if` 2 = 3:
```csharp
bool M(bool a, bool b, int flags)   // not reported
```

After, B — `&`, `|`, `^` count like `&&`/`||` when their operands are `bool` (the paper's "binary boolean operators", p8, p21); `flags & 4` still scores nothing: 4:
```csharp
bool {|RH2009:M|}(bool a, bool b, int flags)   // "This member has a cognitive complexity of 4, which exceeds the limit of 3."
```

Why A: non-short-circuit operators on `bool` are rare and written deliberately, every example in the paper uses `&&`/`||`, and B would make the same `&` score or not depending on its operand type. What would change the pick: wanting the paper's wording taken literally.

**4 · RH2009 · Methods should not exceed the complexity limit — anonymous method (`delegate { … }`)**

Answer 10 leaves lambdas out; it does not name anonymous methods, which the spec has always treated together with lambdas.

Input, with `{ "design": { "maximumMethodComplexity": 1 } }`:
```csharp
using System;

class C
{
    Action<int> M(bool enabled)
    {
        if (enabled)
        {
            return delegate (int value)
            {
                if (value > 0)
                {
                    Console.WriteLine(value);
                }
            };
        }

        return null;
    }
}
```

Before (`main`): not reported.

After, A (recommended) — like a lambda: the body counts for nothing, the anonymous method is not judged; `M` = 1:
```csharp
Action<int> M(bool enabled)   // not reported
```

After, B — counted toward `M`, one level deeper: `if` 1, the inner `if` 3 (nesting 2) = 4:
```csharp
Action<int> {|RH2009:M|}(bool enabled)   // "This member has a cognitive complexity of 4, which exceeds the limit of 1."
```

Why A: an anonymous method is the older spelling of a lambda, and rewriting one into the other should not change the score.

**5 · RH2009 · Methods should not exceed the complexity limit — chained `?:` in parentheses**

Answer 8 scores a `?:` in another's false branch like `else if`. It does not say whether parentheses around that branch break the chain. RH3002 does not report these parentheses (`scripts/apply-fix.sh RH3002` on this input: `no-diagnostic` under LF and CRLF), so both spellings stay in code.

Input, with `{ "design": { "maximumMethodComplexity": 2 } }`:
```csharp
class C
{
    string Name(int code) => code == 1 ? "one" : (code == 2 ? "two" : "other");
}
```

Before (`main`): not reported.

After, A (recommended) — parentheses directly around the false branch do not break the chain: 1 + 1 = 2, no nesting increment:
```csharp
string Name(int code) => …   // not reported
```

After, B — only an unparenthesized false branch continues the chain: 1 + 2 (nesting 1) = 3:
```csharp
string {|RH2009:Name|}(int code) => …   // "This member has a cognitive complexity of 3, which exceeds the limit of 2."
```

Why A: the parentheses change neither the meaning nor the reading, and RH2009 already looks through parentheses when it counts logical-operator sequences.

## Internal choices

- **"A call of the member itself"** (any answer to question 2) — decide it by the symbol the call binds to, not by name: `M(1)` calling the overload `M(string)` is not recursion, and `this.M(…)`, `C.M(…)` (static), and a local function calling itself are. A self-call inside a lambda scores nothing (answer 10: the lambda body does not count). Recommend stating this in RH2009-003.
- **Change doc** — *Alternatives considered* should list each departure in the *Comparison* table that an answer chose (rows 6, 13, 16, 17, and 20), and the ones these questions decide, so that "cognitive complexity" in the rule page reads as "cognitive complexity, adapted" rather than as the paper's metric. The rule page names no paper, so it needs no change for this.

## Issue goals

The issue's "follow the cognitive-complexity definition" goal is now a definition with known departures. Each departure is either a maintainer answer (rows 6, 13, 16, 17, 19, 20) or one of questions 1–5; none is left undecided by this list.

## Architecture findings

_None._

## Follow-up candidates

- Already listed: lambdas (answer 10). Under answer 10, a query written as `from … where a && b` scores its `&&`, while the same query written as `.Where(x => a && b)` scores nothing; the follow-up that brings lambdas back closes that gap.
- Indirect recursion, if question 2 is answered A or B and the maintainer later wants the paper's full rule; it needs a compilation-end analysis.
