# Complexity rule for methods

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | RH0001-007, SHARED-LINES-002; new specs: RH2009 (RH2009-001 to RH2009-006), SHARED-LIMIT (SHARED-LIMIT-001, SHARED-LIMIT-002) |

## What

A new Design rule, **RH2009 — Methods should not exceed the complexity limit** (Design, Warning, enabled, no fix), reports a member whose body has a cognitive complexity above its limit (thoenissen/Reihitsu#352). A new user page, `documentation/cognitive-complexity.md`, explains the scoring of every construct with worked examples; the rule page stays short and links to it.

- **Members judged** — the ones RH2008 judges for their length (RH2008-002, RH2008-004), each by its block body or the expression of its expression body (RH2009-002).
- **Cognitive complexity** (RH2009-003), starting from 0:
  - `if`, `?:`, `switch` statement and `switch` expression (once for the whole switch), `for`, `foreach`, `while`, `do`, and `catch` add 1 **plus their nesting level**: the same `if` scores 1 at nesting level 0, 2 at level 1, 3 at level 2;
  - `else if`, `else`, and a `?:` that is the false branch of another `?:` (also in parentheses) add 1, without a nesting increment;
  - each sequence of like binary logical operators adds 1 — `&&`, `||`, and `&`, `|`, `^` on `bool` or `bool?` — with a new sequence at each change of operator and after a `!`; a compound assignment (`&=`, `|=`, `^=`) adds nothing; the `and` and `or` combinators of an `is` pattern count the same way, those of a case label or switch arm score nothing; each `goto` adds 1; a method or local function that invokes itself adds 1 once;
  - the nesting level is raised by the branches of an `if` chain and of a `?:` chain, a switch's sections or arms, a loop body, a `catch` block, and the body of a lambda or anonymous method; `try`, `finally`, `using`, `lock`, plain blocks, query clauses, `??`, `?.`, `return`, `throw`, `break`, `continue`, case labels, and conditional directives add nothing and do not nest.
- **No-nesting exception** (RH2009-004) — a member in which no structure lies in the nested part of another structure is never reported, whatever its complexity. Nesting that comes only from a lambda or an anonymous method does not count for the exception, though it counts for the score.
- **Lambdas and local functions** (RH2009-005) — the body of a lambda or anonymous method counts toward the member that holds it, one level deeper; a lambda is never judged on its own. A local function is judged only on its own and adds nothing to its holder.
- **Limit** (RH2009-001) — `design.maximumMethodComplexity` in `reihitsu.json`, default 15, one limit for production and test code. A member is reported when its complexity is greater than the limit (SHARED-LIMIT-002). RH0001 validates the setting like the line limits (RH0001-007), and a setting without a valid value keeps the default (SHARED-LIMIT-001).
- **Diagnostic** (RH2009-006) — one per member, on the token RH2008 reports on (RH2008-005): "This member has a cognitive complexity of {0}, which exceeds the limit of {1}."

The default-on-invalid and greater-than-limit statements that SHARED-LINES-002 held for the line counts move to a new shared spec, `shared/configured-limits.md` (SHARED-LIMIT), which RH2008, RH2111 (through SHARED-LINES-002), and RH2009 use; SHARED-LINES-002 keeps only that test code and production code have separate limits. RH2008's and RH2111's behavior does not change.

This repository's own code is not held to the rule in this change: the self-hosting scan ignores RH2009, and `Release.ruleset` lists it with `Action="None"`. The violations it reports there, and a review of the default, are a follow-up.

## Why

Nothing in the analyzer set makes a method's complexity visible; RH2008 catches long methods, not short, deeply nested ones. Cognitive complexity measures how hard a body is to follow: it charges for nesting, which the reader has to hold in mind, and charges a switch once instead of once per case, so it tracks readability rather than the number of paths.

The no-nesting exception keeps the rule on what the measure is for: a body whose decisions stand one after another — a validation sequence, a dispatch on an `if … else if` chain — reads one decision at a time; it is long rather than complex, which RH2008 judges.

The limit lives in `reihitsu.json`, the one place where Reihitsu's own settings live and where RH0001 reports a typo or an invalid value, as for the line limits. The issue proposed an `.editorconfig` key, which no Reihitsu rule reads.

## Visible output changes

Every example is production code without a `reihitsu.json`, unless it says otherwise.

**1. A new diagnostic on a nested body above the limit.** The issue's example has a cognitive complexity of 16:

```csharp
// input, and before: not reported
class C
{
    public int Score(int[] values)
    {
        int total = 0;

        foreach (var value in values)                     // +1
        {
            if (value > 0)                                // +2 (nesting 1)
            {
                if (value % 2 == 0)                       // +3 (nesting 2)
                {
                    for (int i = 0; i < value; i++)       // +4 (nesting 3)
                    {
                        if (i % 3 == 0)                   // +5 (nesting 4)
                        {
                            total += i;
                        }
                        else if (i % 5 == 0)              // +1
                        {
                            total -= i;
                        }
                    }
                }
            }
        }

        return total;
    }                                                     // total 16
}
```

After: RH2009 on the identifier `Score`, "This member has a cognitive complexity of 16, which exceeds the limit of 15."

The issue's compliant example — `Score` 1, `ScoreValue` 3, `AdjustForIndex` 2 — is not reported, before and after.

**2. Deeper nesting costs more.** With `{ "design": { "maximumMethodComplexity": 5 } }`:

```csharp
// before: not reported (RH0001 reports "Unknown configuration setting 'design.maximumMethodComplexity'.")
// after: RH2009 on `M` (total 6); `N` (total 3) is not reported
class C
{
    void {|RH2009:M|}(int a)
    {
        if (a > 0)              // +1
        {
            if (a > 1)          // +2 (nesting 1)
            {
                if (a > 2)      // +3 (nesting 2)
                {
                    a--;
                }
            }
        }
    }

    void N(int a)
    {
        if (a > 0)              // +1
        {
            a--;
        }

        if (a > 1)              // +1
        {
            a--;
        }

        if (a > 2)              // +1
        {
            a--;
        }
    }
}
```

**3. No nesting, no report.** With `{ "design": { "maximumMethodComplexity": 1 } }`, before and after: not reported, whatever the complexity:

```csharp
class C
{
    int M(int value)
    {
        if (value == 1)                               // +1
        {
            return 10;
        }
        else if (value == 2 && value > 0)             // +1, +1 for &&
        {
            return 20;
        }
        else                                          // +1
        {
            value++;
        }

        for (var i = 0; i < value; i++)               // +1
        {
            value--;
        }

        value = value > 5 || value < 0 ? 0 : 1;       // +1, +1 for ||

        switch (value)                                // +1
        {
            case 3:
                return 30;

            default:
                return value;
        }
    }                                                 // total 8, nothing nested
}
```

**4. Lambdas count, but do not lift the exception alone.** With `{ "design": { "maximumMethodComplexity": 3 } }`:

```csharp
// before: not reported
// after: RH2009 on `M`, "This member has a cognitive complexity of 4, which exceeds the limit of 3."; `N` is not reported
using System;
using System.Collections.Generic;
using System.Linq;

class C
{
    void {|RH2009:M|}(List<int> values, bool enabled)
    {
        if (enabled)                                  // +1
        {
            values.ForEach(value =>
            {
                if (value > 0)                        // +3 (nesting 2)
                {
                    Console.WriteLine(value);
                }
            });
        }
    }                                                 // total 4

    int[] N(int[] values, int mode)
    {
        if (mode == 1 || mode == 2)                   // +1, +1 for ||
        {
            return values;
        }

        return values.Select(value => value > 0 ? value : 0).ToArray();   // +2 (nesting 1)
    }                                                 // total 4, nesting only through the lambda
}
```

**5. A local function is judged on its own.** With `{ "design": { "maximumMethodComplexity": 2 } }`:

```csharp
// before: not reported
// after: RH2009 on `Local` (total 3); `M` (total 1) is not reported
class C
{
    int M(int value)
    {
        if (value > 0)                                // +1 for M
        {
            return Local(value);
        }

        return 0;

        int {|RH2009:Local|}(int input)
        {
            foreach (var digit in input.ToString())   // +1 for Local
            {
                if (digit == '0')                     // +2 (nesting 1) for Local
                {
                    return 0;
                }
            }

            return input;
        }
    }
}
```

**6. Chained conditionals, recursion, and switch patterns.** With `{ "design": { "maximumMethodComplexity": 3 } }`:

```csharp
// before: not reported
// after: RH2009 on `Fib` (total 4) and `Kind` (total 4); `Name` (total 3, nothing nested) is not reported
class C
{
    string Name(int code) => code == 1 ? "one" : code == 2 ? "two" : (code == 3 ? "three" : "other");   // +1, +1, +1

    int {|RH2009:Fib|}(int n)
    {
        if (n > 1)                                    // +1
        {
            if (n > 2)                                // +2 (nesting 1)
            {
                return Fib(n - 1) + Fib(n - 2);       // +1, once for calling itself
            }
        }

        return n;
    }

    int {|RH2009:Kind|}(char c, bool upper)
    {
        if (upper)                                    // +1
        {
            return c switch                           // +2 (nesting 1)
            {
                'A' or 'E' or 'I' => 1,               // +0 in a switch arm
                _ when c > 'X' && c < 'Z' => 2,        // +1 for &&
                _ => 0,
            };
        }

        return 0;
    }
}
```

**7. RH0001 accepts the new setting.**

```json
{ "design": { "maximumMethodComplexity": 20 } }
```

Before: "The reihitsu.json configuration is invalid: Unknown configuration setting 'design.maximumMethodComplexity'." After: valid. `{ "design": { "maximumMethodComplexity": 0 } }` reports, after, "The reihitsu.json configuration is invalid: The 'design.maximumMethodComplexity' setting must be a whole number from 1 to 2147483647.", and RH2009 keeps the default of 15. `maximumTestMethodComplexity` stays an unknown setting.

**8. New pages:** the rule page `documentation/rules/RH2009.md` and the scoring guide `documentation/cognitive-complexity.md`.

## Decisions

### Cognitive complexity, default 15, configurable in `reihitsu.json`

The maintainer decided the metric (cognitive, as the issue proposes, after first naming cyclomatic), the default of 15 as a starting point to be tried out on this repository, configuration through `reihitsu.json` like the line limits, and one setting for production and test code.

### A member without nesting is never reported; deeper nesting costs more

The maintainer decided that a method that never nests one structure inside another is not reported, whatever its complexity, and that a structure's increment grows with its nesting depth. RH2009-004 states the exception by the nested parts of RH2009-003, so the exception and the score share one notion of nesting — except for lambdas and anonymous methods, whose nesting counts for the score but not for the exception (maintainer: "lambdas are ok").

### Lambdas count; local functions stand alone

The maintainer decided that the body of a lambda or anonymous method counts toward the member that holds it and raises the nesting level by one, since it is read as part of that member, while nesting that comes only from a lambda does not lift the no-nesting exception ("lambdas are ok"). A lambda in a field, property, or event initializer is not judged in this change. A local function is judged only on its own and adds nothing to its holder: it has a name and a signature and is read as a method of its own.

### What scores, and how

The maintainer decided each of these scoring points:

- `&`, `|`, and `^` on `bool` count like `&&` and `||`, as sequences of binary logical operators.
- **`&`, `|`, and `^` on `bool?` count like on `bool`**: it is the same logical operator on the same conditions; only the null case differs.
- **`&=`, `|=`, and `^=` score nothing**, also on `bool`: a compound assignment accumulates a result and is read as one statement, not as a condition.
- A member that calls itself scores 1 once, however many calls it holds; recursion through several members scores nothing.
- **Only an invocation `M(…)` of the member itself counts as recursion**: a constructor that creates its own type with `new`, an accessor that uses its own property, and an operator that uses itself score nothing. The invocation is the form a reader recognizes as recursion.
- **A `?:` in a switch `when` clause is nested in the switch**: the case label and its `when` clause are read inside the switch, as part of the decision the switch opened.
- A `?:` in the false branch of another `?:`, also in parentheses, scores 1 without a nesting increment, like `else if`.
- `and` and `or` in an `is` pattern count like `&&` and `||`; in a case label or switch arm they score nothing, because a switch is charged once however many cases it lists.
- Only active code is scored; conditional directives score nothing.

### Scoring guide on its own page

The maintainer decided that the full scoring — every construct, with worked examples and per-line points — lives on `documentation/cognitive-complexity.md`, and the rule page stays short and links to it.

### This repository's own violations are a follow-up

The maintainer decided that the violations RH2009 reports in this repository's own code are ignored in this change, through the self-hosting scan's ignore list; their handling and a review of the default come in a follow-up.

### One shared statement of a configured limit

When a setting without a valid value keeps its default, and that a measure equal to the limit is accepted, is stated once, in SHARED-LIMIT, for the line limits and the complexity limit. The members RH2009 judges and where it reports are RH2008's (RH2008-002, RH2008-004, RH2008-005), referenced rather than restated, so the two rules cannot drift apart.

## Alternatives considered

- **Cyclomatic complexity** — rejected by the maintainer: it counts paths and charges nesting nothing, so a deeply nested body scores like a flat one.
- **An `.editorconfig` key** (`dotnet_diagnostic.RH2009.<option>`), as the issue proposed — rejected: no Reihitsu rule reads one, a mistyped key is silently ignored, and the line limits already live in `reihitsu.json`, where RH0001 validates them.
- **A separate limit for test code** (`maximumTestMethodComplexity`, as RH2008 has) — rejected by the maintainer: a test is not more complex than production code by nature.
- **Every `?:` nests, also one that continues a chain** — rejected: a chained conditional is the expression form of an `if … else if` chain, which scores flat, so the same dispatch should not score more, or be reported, only because it is written as an expression.
- **Local functions counting toward the member that holds them** — rejected: a local function is read as a method of its own; extracting a nested block into one should make its holder simpler.
- **Lambdas judged on their own**, as the issue proposed — rejected: a lambda has no name to report on and is read as part of the member. A lambda in a field, property, or event initializer, which no member holds, is not judged in this change; judging it is a follow-up.
- **Lambdas left out of the score** — an earlier answer, superseded: the maintainer decided that a lambda's body is read as part of the member and counts.
- **`&`, `|`, `^` on `bool` scoring nothing** — an earlier answer, superseded: the maintainer decided that a logical operator on conditions is read like `&&` and `||`, whether it short-circuits or not.
- **Only `bool` operands counting for `&`, `|`, `^`** — rejected by the maintainer: `bool?` operands combine the same conditions.
- **`&=`, `|=`, `^=` counting as a sequence of their own** — rejected by the maintainer: a compound assignment is an assignment, not a condition.
- **Recursion through several members** (`A` calls `B`, `B` calls `A`) — rejected: finding it needs a call graph over the whole compilation, and a member's score would change when another member, perhaps in another file, changes.
- **Every use of the member itself counting as recursion** (`new` of its own type in a constructor, its own property in an accessor, its own operator in an operator) — rejected by the maintainer: a property or operator that uses itself is nearly always endless recursion, and a recursive constructor is rare.
- **A `when` clause not nesting**, like a condition — rejected by the maintainer: the clause is read inside the switch.
- **`#if` scoring like `if`** — rejected: a build reads one branch, which is how every Reihitsu rule treats conditional code (SHARED-DIAG-008).
- **Pattern combinators counting also in case labels and switch arms, or nowhere** — rejected: `x is 1 or 2` is the same condition as `x == 1 || x == 2`, while the cases of a switch are one decision.
- **Query clauses nesting like the lambdas they compile to** — rejected: `where` and `select` clauses would cost more than the statements they stand in for.
- **Totals per type, file, or project** — rejected: the rule judges members.
- **Counting `??`, `?.`, and `try`** — rejected: they do not make the reader track a decision.
