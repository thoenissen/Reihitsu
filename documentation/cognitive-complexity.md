# Cognitive complexity

Rule [RH2009](rules/RH2009.md) reports a method whose body is harder to follow than its limit allows. It measures how hard a body is to follow as its *cognitive complexity*: a score that adds points for every break in the straight flow of the code, and more points the deeper that break is nested. This page explains how the score is computed, construct by construct, with examples.

## What is scored

Every method is scored on its own, and so are constructors, finalizers, operators, conversion operators, local functions, the accessors of properties, indexers, and events, and properties and indexers written with `=>`. The score of a property with `get` and `set` accessors is not a sum: each accessor is scored and reported on its own. Scores are not added up per type, file, or project.

The score starts at 0 for every body. The declaration itself — its name, parameters, attributes, and a constructor's `: base(…)` or `: this(…)` call — adds nothing.

Lambdas and anonymous methods are part of the body that contains them. A lambda in a field, property, or event initializer belongs to no method and is not scored.

## Three kinds of points

- **Structural points** — a structure that breaks the flow, such as `if` or a loop, adds 1, **plus its nesting level**: 1 at the top of the body, 2 inside one other structure, 3 inside two, and so on.
- **Continuation points** — `else if`, `else`, and a chained `?:` add 1, with no nesting points, because the reader already pays for the decision at its `if`.
- **Flat points** — a sequence of logical operators, a `goto`, and recursion add 1 wherever they stand.

The **nesting level** of a piece of code is the number of structures it lies inside. A structure raises the nesting level only for the code it controls — the branches of an `if`, the body of a loop — and not for its condition.

```csharp
class Nesting
{
    void M(int a)
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
    }                           // total 6

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
    }                           // total 3
}
```

## Every construct

| Construct | Points | Nesting points | Raises the nesting level of |
|---|---|---|---|
| `if` | 1 | yes | its branch and the branches of its `else if` and `else` |
| `else if` | 1 | no | — (its branch is part of the `if`'s chain) |
| `else` | 1 | no | — (its branch is part of the `if`'s chain) |
| `?:` | 1 | yes | its two branches |
| `?:` as the false branch of another `?:`, also in parentheses | 1 | no | — (its branches are part of the chain) |
| `switch` statement, `switch` expression | 1 for the whole switch | yes | its sections or arms, with their case labels, patterns, and `when` clauses |
| `for`, `foreach`, `await foreach`, `while`, `do` | 1 | yes | its body |
| `catch` | 1 | yes | its block, not its `when` filter |
| a sequence of logical operators: `&&`, `\|\|`, and `&`, `\|`, `^` on `bool` or `bool?` | 1 per sequence | no | — |
| a sequence of `and` / `or` in an `is` pattern | 1 per sequence | no | — |
| `goto`, `goto case`, `goto default` | 1 | no | — |
| a method that calls itself | 1, once | no | — |
| lambda, anonymous method (`delegate { … }`) | 0 | — | its body |
| local function | 0 — scored on its own | — | — |
| `try`, `finally`, `using`, `lock`, `fixed`, `checked`, `unchecked`, `unsafe`, a plain block | 0 | — | nothing |
| `return`, `throw`, `break`, `continue`, `yield` | 0 | — | — |
| `??`, `??=`, `?.`, `?[]` | 0 | — | — |
| case labels, `default`, `when` clauses, `catch` filters | 0 themselves; the operators in them count | — | — |
| `and` / `or` in a case label or a switch arm | 0 | — | — |
| `not`, and a pattern without `and` or `or` | 0 | — | — |
| `&`, `\|`, `^` on numbers and enums | 0 | — | — |
| `&=`, `\|=`, `^=` | 0 | — | — |
| query clauses (`from`, `where`, `select`, …) | 0 | — | nothing |
| `#if`, `#elif`, `#else`, `#endif` | 0 | — | nothing |

## `if`, `else if`, and `else`

An `if` adds 1 plus its nesting level. `else if` and `else` add 1 each, whatever the nesting level, and their branches lie inside the `if`'s chain. An `if` written inside the block of an `else` is a new structure and nests; written as `else if`, it continues the chain.

```csharp
class Chains
{
    int Flat(int value)
    {
        if (value == 1)          // +1
        {
            return 10;
        }
        else if (value == 2)     // +1
        {
            return 20;
        }
        else                     // +1
        {
            return 0;
        }
    }                            // total 3

    int Nested(int value)
    {
        if (value == 1)          // +1
        {
            return 10;
        }
        else                     // +1
        {
            if (value == 2)      // +2 (nesting 1)
            {
                return 20;
            }

            return 0;
        }
    }                            // total 4
}
```

## Logical operators

A sequence of like logical operators adds 1. In one condition, the first operator adds 1, and every operator that differs from the one before it adds 1 more. Parentheses do not end a sequence; a `!` in front of a parenthesized condition does, and so does any other expression between the operators — a call, a comparison, a `?:`, a `??`.

`&`, `|`, and `^` count like `&&` and `||` when they combine `bool` values (also `bool?`). On numbers and enums they are bit operations and add nothing. A compound assignment such as `ok &= valid` adds nothing.

```csharp
class Conditions
{
    bool _a, _b, _c, _d;

    void M(int flags)
    {
        var x = _a && _b && _c;              // +1
        var y = _a && _b || _c;              // +2: && then ||
        var z = _a && (_b || _c) && _d;      // +3: &&, ||, then && again
        var u = !(_a && _b) && _c;           // +2: the ! starts a new sequence
        var v = _a & _b | _c ^ _d;           // +3: &, |, then ^ on bool
        var w = (flags & 4) != 0 && _a;      // +1: & on a number adds nothing
    }                                        // total 12, but nothing is nested
}
```

The pattern combinators `and` and `or` of an `is` expression count in the same way, in a sequence of their own; a `not` pattern starts a new sequence. The `&&` in front of an `is` and the `or` inside it are two sequences.

```csharp
class Patterns
{
    void M(int x, bool a)
    {
        var p = x is 1 or 2 or 3;                // +1
        var q = x is (1 or 2) and not 3;         // +2: or, then and
        var r = x is not (> 0 and < 10) and 20;  // +2: the not starts a new sequence
        var s = a && x is 1 or 2;                // +2: && and or are two sequences
    }                                            // total 7, but nothing is nested
}
```

## Conditional expressions `?:`

A `?:` adds 1 plus its nesting level, and its two branches lie one level deeper. A `?:` that is the false branch of another `?:` — also when it is written in parentheses — continues a chain, as `else if` does: it adds 1 and no nesting points. A `?:` in a true branch nests.

```csharp
class Conditionals
{
    string Name(int code) => code == 1 ? "one"                // +1
                             : code == 2 ? "two"               // +1, continues the chain
                             : (code == 3 ? "three" : "other"); // +1, continues the chain
                                                               // total 3

    string Pick(int code, bool upper) => code == 1
                                             ? (upper ? "ONE" : "one") // +1 for the outer ?:, +2 (nesting 1) for the inner ?:
                                             : "other";                // total 3
}
```

## `switch`

A `switch` statement and a `switch` expression add 1 plus their nesting level, once for the whole switch, however many cases it has. Everything in its sections or arms lies one level deeper, also a `when` clause: a `?:` in a `when` clause adds its nesting points like one in the section's statements. `and` and `or` in a case label or an arm add nothing, because the switch already counts as one decision; `&&` and `||` in a `when` clause count.

```csharp
class Switches
{
    int M(int value, char letter)
    {
        switch (value)                                       // +1
        {
            case 1 or 2:                                     // +0
                return letter switch                         // +2 (nesting 1)
                {
                    'a' or 'e' => 1,                         // +0
                    _ when value > 0 && letter > 'x' => 2,   // +1 for &&
                    _ => 3,
                };

            case 3:
                return 0;

            default:
                goto case 3;                                 // +1
        }
    }                                                        // total 5
}
```

## Loops, `catch`, and `goto`

`for`, `foreach`, `while`, and `do` add 1 plus their nesting level, and their body lies one level deeper; their condition does not. A `catch` adds 1 plus its nesting level, and its block lies one level deeper; `try` and `finally` add nothing and do not nest. Every `goto` adds 1.

```csharp
using System;

class Loops
{
    void M(int[] values)
    {
        try
        {
            foreach (var value in values)               // +1
            {
                while (Check(value))                     // +2 (nesting 1)
                {
                    Console.WriteLine(value);
                }
            }
        }
        catch (InvalidOperationException e)              // +1
            when (e.Message.Length > 0 && values.Length > 0)  // +1 for &&
        {
            if (values.Length > 1)                       // +2 (nesting 1)
            {
                goto done;                               // +1
            }
        }
        finally
        {
            Console.WriteLine();
        }

        done:
        Console.WriteLine("done");
    }                                                    // total 8

    bool Check(int value) => value > 0;
}
```

## Lambdas and anonymous methods

A lambda or an anonymous method adds nothing itself, but its body is part of the method that contains it and lies one level deeper.

```csharp
using System;
using System.Collections.Generic;

class Lambdas
{
    void M(List<int> values, bool enabled)
    {
        if (enabled)                                // +1
        {
            values.ForEach(value =>
            {
                if (value > 0)                      // +3 (nesting 2: the if and the lambda)
                {
                    Console.WriteLine(value);
                }
            });

            values.ForEach(delegate (int value)
            {
                Console.WriteLine(value > 0 ? value : 0);  // +3 (nesting 2: the if and the anonymous method)
            });
        }
    }                                               // total 7
}
```

## Local functions

A local function is scored on its own, as a method, starting at 0. It adds nothing to the method that declares it.

```csharp
class LocalFunctions
{
    int M(int value)
    {
        if (value > 0)                                  // +1 for M
        {
            return Count(value);
        }

        return 0;

        int Count(int input)
        {
            var count = 0;

            foreach (var digit in input.ToString())     // +1 for Count
            {
                if (digit == '0')                       // +2 (nesting 1) for Count
                {
                    count++;
                }
            }

            return count;
        }                                               // Count: total 3
    }                                                   // M: total 1
}
```

## Recursion

A method or local function that calls itself adds 1, once, however many calls of itself it holds. Every call of the method itself counts, on any instance: `M(…)`, `this.M(…)`, `C.M(…)`, and a call on another object of the same type, such as `Left.Depth()` in a tree node. A call of another overload, a call through a method group or a delegate, and a cycle through several methods (`A` calls `B`, `B` calls `A`) add nothing, and so do a constructor that creates an instance of its own type with `new`, an accessor that uses its own property or indexer, and an operator that uses itself.

```csharp
class Recursion
{
    int Fib(int n)
    {
        if (n < 2)                         // +1
        {
            return n;
        }

        return Fib(n - 1) + Fib(n - 2);    // +1, once for both calls
    }                                      // total 2

    bool IsEven(int n) => n == 0 || IsOdd(n - 1);   // +1 for ||; total 1
    bool IsOdd(int n) => n != 0 && IsEven(n - 1);   // +1 for &&; total 1
}

class Node
{
    public Node Left;

    public Node Right;

    int Depth()
    {
        if (Left == null || Right == null)                   // +1, +1 for ||
        {
            return 1;
        }

        return 1 + System.Math.Max(Left.Depth(), Right.Depth());   // +1, once for both calls
    }                                                        // total 3
}
```

## Query expressions

The clauses of a query expression — `from`, `where`, `orderby`, `select`, and the others — add nothing and do not nest. The operators and `?:` inside them count at the nesting level where the query stands.

```csharp
using System.Linq;

class Queries
{
    int[] M(int[] values, bool enabled)
    {
        if (enabled)                                       // +1
        {
            return (from value in values
                    where value > 0 && value < 10          // +1 for &&
                    select value > 5 ? value : 0)          // +2 (nesting 1)
                   .ToArray();
        }

        return values;
    }                                                      // total 4
}
```

## Conditional compilation

`#if`, `#elif`, `#else`, and `#endif` add nothing and do not nest. Only the code that the current build compiles is scored; code that an `#if` leaves out counts as if it were not there.

```csharp
class Directives
{
    int M(int value)
    {
        if (value > 0)              // +1
        {
#if DEBUG
            if (value > 100)        // +2 (nesting 1), only in a build that defines DEBUG
            {
                return 100;
            }
#endif
            return value;
        }

        return 0;
    }                               // total 3 with DEBUG, 1 without
}
```

## When a method is reported

A method is reported when its cognitive complexity is **greater than the limit** — 15 by default, set with `design.maximumMethodComplexity` in `reihitsu.json` (see [RH2009](rules/RH2009.md)). A method whose score equals the limit is accepted. The limit is the same for production code and test code.

A method is **never reported when nothing in it is nested**: when no `if`, `?:`, `switch`, loop, or `catch` lies inside the branches, body, or block of another one. Such a method makes its decisions one after another, and the reader can follow them one at a time, however many there are. Nesting that comes only from a lambda or an anonymous method does not count for this: a `?:` inside a lambda in an otherwise flat method scores its nesting points, but it does not make the method reportable.

Once one structure is nested inside another, the whole score counts, the points of the structures at the top of the body included.

```csharp
using System.Linq;

class Reporting
{
    int[] Flat(int[] values, int mode)
    {
        if (mode == 1)                                            // +1
        {
            return values;
        }

        if (mode == 2 || mode == 3)                               // +1, +1 for ||
        {
            return values.Reverse().ToArray();
        }

        return values.Select(value => value > 0 ? value : 0)      // +2 (nesting 1: the lambda)
                     .ToArray();
    }                                                             // total 5; never reported: only the lambda nests

    int[] Nested(int[] values, int mode)
    {
        if (mode == 1)                                            // +1
        {
            if (values.Length > 0)                                // +2 (nesting 1)
            {
                return values;
            }
        }

        return values;
    }                                                             // total 3; compared with the limit
}
```

## Worked examples

A method that nests four levels deep, reported at the default limit:

```csharp
class Worked1
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
    }                                                     // total 16: reported, the limit is 15
}
```

The same logic split into three methods, none of them reported:

```csharp
class Worked2
{
    public int Score(int[] values)
    {
        int total = 0;

        foreach (var value in values)                  // +1
        {
            total += ScoreValue(value);
        }

        return total;
    }                                                  // total 1

    private int ScoreValue(int value)
    {
        if (value <= 0 || value % 2 != 0)              // +1, +1 for ||
        {
            return 0;
        }

        int score = 0;

        for (int i = 0; i < value; i++)                // +1
        {
            score += AdjustForIndex(i);
        }

        return score;
    }                                                  // total 3

    private int AdjustForIndex(int index)
    {
        if (index % 3 == 0)                            // +1
        {
            return index;
        }

        if (index % 5 == 0)                            // +1
        {
            return -index;
        }

        return 0;
    }                                                  // total 2
}
```

A long validation whose decisions stand one after another: its score is above the default limit, but nothing is nested, so it is not reported:

```csharp
class Worked3
{
    string Validate(string name, int age, string email, string country, string zip, string phone)
    {
        if (name == null || name.Length == 0)                         // +1, +1 for ||
        {
            return "name";
        }

        if (age < 0 || age > 150)                                     // +1, +1 for ||
        {
            return "age";
        }

        if (age < 18 & country == "US")                               // +1, +1 for & on bool
        {
            return "minor";
        }

        if (email != null && !email.Contains('@'))                    // +1, +1 for &&
        {
            return "email";
        }

        if (country is not ("DE" or "AT" or "CH"))                    // +1, +1 for or
        {
            return "country";
        }

        if (zip.Length != 5 && country == "DE")                       // +1, +1 for &&
        {
            return "zip";
        }

        if (phone != null && phone.Length < 6 || phone == "")         // +1, +2 for && then ||
        {
            return "phone";
        }

        return email == null ? "ok"                                   // +1
               : email.EndsWith(".test") ? "test"                     // +1, continues the chain
               : "ok";
    }                                                                 // total 17; never reported: nothing is nested
}
```

A method that uses most constructs at once:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class Worked4
{
    IEnumerable<string> Tokenize(string text, bool strict)
    {
        if (string.IsNullOrEmpty(text))                                     // +1
        {
            return Array.Empty<string>();
        }

        var parts = text.Split(' ')
                        .Where(part => part.Length > 0 && part != "-")      // +1 for &&
                        .Select(part => part.StartsWith('#')                // +2 (nesting 1: the lambda)
                                            ? part.Substring(1)
                                            : part.ToLowerInvariant())
                        .ToList();

        foreach (var part in parts)                                         // +1
        {
            switch (part)                                                   // +2 (nesting 1)
            {
                case "if" or "else":                                        // +0
                    continue;

                default:
                    if (strict && IsReserved(part))                         // +3 (nesting 2), +1 for &&
                    {
                        throw new FormatException(part);
                    }

                    break;
            }
        }

        return parts.Count > 1 ? parts : Tokenize(text.Trim(), false);      // +1 for ?:, +1 for calling itself
    }                                                                       // total 13: not reported

    bool IsReserved(string part) => part is "class" or "struct";            // +1 for or; total 1
}
```

A method with an anonymous method and a local function:

```csharp
using System.Collections.Generic;

class Worked5
{
    void Run(List<Job> jobs, bool parallel)
    {
        if (parallel)                                  // +1
        {
            jobs.ForEach(delegate (Job job)
            {
                if (job.Ready)                         // +3 (nesting 2: the if and the anonymous method)
                {
                    Start(job);
                }
            });
        }
        else                                           // +1
        {
            StartAll();
        }

        void StartAll()
        {
            foreach (var job in jobs)                  // +1 for StartAll
            {
                if (job.Ready)                         // +2 (nesting 1) for StartAll
                {
                    Start(job);
                }
            }
        }                                              // StartAll: total 3
    }                                                  // Run: total 5

    void Start(Job job)
    {
    }
}

class Job
{
    public bool Ready;
}
```
