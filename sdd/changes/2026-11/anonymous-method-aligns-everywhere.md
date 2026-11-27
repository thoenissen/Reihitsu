# An anonymous method's block aligns under its first token everywhere

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-INDENT-031 (changed) |
| Depends on | [Parenthesized lambdas keep their line, and an anonymous method in a raw-string hole aligns like a block lambda](lambda-paren-hole-break.md) |

## What

The block of an anonymous method whose opening brace starts a line aligns under the anonymous method's anchor wherever the anonymous method stands, not only in an interpolation hole of a multi-line interpolated raw string literal. The anchor stays the one FMT-INDENT-031 takes in a hole: the `async` keyword of an async anonymous method, otherwise the `delegate` keyword, also behind `static`. The body stays one level deeper than the braces. Outside such a hole the block took the scope-depth column of the enclosing statement until now; inside the block body of a lambda or of another anonymous method in such a hole it took the scope depth relative to that body. Both cases now align like the hole case, as a block lambda in the same position aligns to the lambda (FMT-INDENT-028).

## Why

When the hole case was decided ([lambda-paren-hole-break](lambda-paren-hole-break.md), maintainer answer 1), the maintainer stated that a hole has no layout rules of its own. FMT-INDENT-031 nevertheless gave the anonymous method one layout inside a hole and another outside it, so the same code changed shape depending only on whether it stood in a raw-string hole. A block lambda in the same positions aligns to the lambda everywhere (FMT-INDENT-028); with this change the anonymous method, the older spelling of the same construct, does too. The scope-depth layout also leaves a later argument behind the anonymous method aligned under the argument list while the block stands far to the left of it (entry 2). In an object initializer, a conditional branch, a switch-expression arm, and a collection element the scope-depth block even stands left of the construct that holds the anonymous method (entry 5), and code fixes that format the same anonymous method give yet other columns (16 behind an argument list, 25 in an initializer; *Code-fix results*).

## Visible output changes

Each "before" is the output of `scripts/format.sh` on this branch's base (fe7b8f8; entry 5 on 2365510, whose production code is the same). Each "after" is the layout this change specifies; it is not built yet.

### 1. An anonymous method as an argument

```csharp
class C
{
    object M(System.Func<int> f)
    {
        return M(delegate { return 1; });
    }
}
```

Before:

```csharp
        return M(delegate
        {
            return 1;
        });
```

After:

```csharp
        return M(delegate
                 {
                     return 1;
                 });
```

### 2. An anonymous method followed by another argument

```csharp
class C
{
    object M(System.Func<int> f, int y)
    {
        var x = M(delegate { return 1; }, 2);

        return x;
    }
}
```

Before:

```csharp
        var x = M(delegate
        {
            return 1;
        },
                  2);
```

After:

```csharp
        var x = M(delegate
                  {
                      return 1;
                  },
                  2);
```

### 3. `static` and `async` anonymous methods, and an anonymous method as a declaration's value

```csharp
using System;
using System.Threading.Tasks;

class C
{
    void M()
    {
        Func<int> s = static delegate { return 1; };
        Func<Task<int>> t = async delegate { return 1; };
        Func<int, int> d = delegate (int a) { return a; };
    }
}
```

Before:

```csharp
        Func<int> s = static delegate
        {
            return 1;
        };
        Func<Task<int>> t = async delegate
        {
            return 1;
        };
        Func<int, int> d = delegate (int a)
        {
            return a;
        };
```

After — under `delegate` behind `static`, under `async`, and under `delegate`:

```csharp
        Func<int> s = static delegate
                             {
                                 return 1;
                             };
        Func<Task<int>> t = async delegate
                            {
                                return 1;
                            };
        Func<int, int> d = delegate (int a)
                           {
                               return a;
                           };
```

### 4. An anonymous method inside the block body of a lambda in a raw-string hole

```csharp
class C
{
    int M(System.Func<int> f)
    {
        var a = $"""
                 G {M(() =>
                      {
                          return M(delegate
                          {
                              return 1;
                          });
                      })}
                 """;

        return 0;
    }
}
```

Before: unchanged (the scope depth relative to the lambda's brace).

After:

```csharp
        var a = $"""
                 G {M(() =>
                      {
                          return M(delegate
                                   {
                                       return 1;
                                   });
                      })}
                 """;
```

### 5. A field's value, an object initializer, a conditional branch, a switch-expression arm, a collection element, `+=`, a chain, and a nested anonymous method

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class D
{
    public Func<int> F { get; set; }
}

class C
{
    private Func<int> _f = delegate { return 1; };

    event EventHandler E;

    int M(Func<int> f, int k = 0)
    {
        var d1 = new D { F = delegate { return 1; } };
        Func<int> c1 = k > 0 ? delegate { return 1; } : null;
        var a = k switch
        {
            0 => M(delegate { return 1; }),
            _ => 2,
        };
        Func<int>[] arr = [delegate { return 1; }, null];
        E += delegate { Console.WriteLine(); };
        var q = new List<int>().Select(delegate (int b) { return b; }).Where(delegate (int b) { return b > 0; });

        return M(delegate { return M(delegate { return 2; }); });
    }
}
```

Before — the block falls back to the scope depth of the enclosing member or statement; in an object initializer, a conditional branch, a switch-expression arm, and a collection element it stands left of the construct that holds it:

```csharp
    private Func<int> _f = delegate
    {
        return 1;
    };

    event EventHandler E;

    int M(Func<int> f, int k = 0)
    {
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
        var a = k switch
                {
                    0 => M(delegate
        {
            return 1;
        }),
                    _ => 2,
                };
        Func<int>[] arr = [
                              delegate
        {
            return 1;
        },
                              null
                          ];
        E += delegate
        {
            Console.WriteLine();
        };

        var q = new List<int>().Select(delegate (int b)
        {
            return b;
        }).Where(delegate (int b)
        {
            return b > 0;
        });

        return M(delegate
        {
            return M(delegate
            {
                return 2;
            });
        });
    }
```

After — each block under its `delegate`, in the column a block lambda in the same position takes today (FMT-INDENT-028):

```csharp
    private Func<int> _f = delegate
                           {
                               return 1;
                           };

    event EventHandler E;

    int M(Func<int> f, int k = 0)
    {
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
        var a = k switch
                {
                    0 => M(delegate
                           {
                               return 1;
                           }),
                    _ => 2,
                };
        Func<int>[] arr = [
                              delegate
                              {
                                  return 1;
                              },
                              null
                          ];
        E += delegate
             {
                 Console.WriteLine();
             };

        var q = new List<int>().Select(delegate (int b)
                                       {
                                           return b;
                                       }).Where(delegate (int b)
                                                {
                                                    return b > 0;
                                                });

        return M(delegate
                 {
                     return M(delegate
                              {
                                  return 2;
                              });
                 });
    }
```

### Code-fix results

Every code fix that formats a node holding an anonymous method whose block starts a line gives the new layout. Checked with `scripts/apply-fix.sh`:

- RH5101 on `return M(⏎    delegate { return 1; }, 2);` gives today `return M(delegate` with the block in column 16, one column left of `delegate` and neither the scope depth nor the anchor, and `2);` in column 17; after this change the block stands in column 17, under `delegate`.
- RH5102 on `return M(delegate { return 1; },⏎    2, 3);` gives the same block in column 16 today, and in column 17 after this change.
- RH5111 on `g =⏎    delegate { return 1; };` gives `g = delegate` with the block in the statement's column 8 today, and in column 12, under `delegate`, after this change.
- RH5111 on `var d1 = new D { F =⏎    delegate { return 1; } };` gives today `var d1 = new D { F = delegate` with the block in column 25, under `F`, closed by `} };`; after this change the block stands in column 29, under `delegate`.
- RH5205 on a misaligned block lambda whose body holds `return M(delegate { return 1; });` gives the anonymous method's block at the scope depth inside the lambda's body today, and under `delegate` after this change.

No rule page changes: RH5205, which reports misaligned lambda braces, leaves anonymous methods out by its non-goal, and RH5204 does not check the lines of an anonymous method's block; neither reports the new layout.

## Decisions

### Maintainer answer (2026-11-27)

Asked as an output question with the inputs of entries 1, 2, and 5 (argument followed by another argument, object initializer, conditional branch, return): **A** keep the scope-depth block outside raw-string holes, **B** align the block under the anonymous method's anchor everywhere. The maintainer chose **B**, as recommended.

### The anchor is the one of the hole case

The anchor does not change: `async` for an async anonymous method, otherwise `delegate`, also behind `static` (maintainer answers 4 and 6 of [lambda-paren-hole-break](lambda-paren-hole-break.md)). Only the positions in which the block aligns grow.

### FMT-INDENT-031 is changed, not retired

The requirement keeps its ID and its hole examples, which still hold; its scope grows from the hole to every position. The tests that tag it keep verifying it.

## Alternatives considered

- **Keep the scope-depth column outside holes** (the issue's option A) — the layout today; rejected by the maintainer's answer because it keeps a layout rule that depends on the hole, against the maintainer's statement that a hole has none.
- **Align under `static` for a `static` anonymous method** — rejected: the maintainer decided `delegate` for the hole case (answer 4), and the anchor is the same in every position.
