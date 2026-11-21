# Complexity rule for methods

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | RH0001-007; new spec: RH2009 (RH2009-001 to RH2009-006) |

## What

A new Design rule, **RH2009 — Methods should not exceed the complexity limit** (Design, Warning, enabled, no fix), reports a member whose body has a cognitive complexity above its limit (thoenissen/Reihitsu#352).

- **Members judged** — the ones RH2008 judges for their length: every method, constructor, finalizer, operator, conversion operator, local function, and accessor with a body, and every property and indexer with an expression body (RH2009-002).
- **Cognitive complexity** (RH2009-003), starting from 0:
  - `if`, `?:`, `switch` statement and `switch` expression (once for the whole switch), `for`, `foreach`, `while`, `do`, and `catch` add 1 **plus their nesting level** — deeper nesting costs more: the same `if` scores 1 at nesting level 0, 2 at level 1, 3 at level 2;
  - `else if` and `else` add 1, without a nesting increment;
  - each sequence of like logical operators (`&&`, `||`, and the pattern combinators `and`, `or`) adds 1, and each change of operator inside an expression 1 more; each `goto` adds 1;
  - nesting is opened by the branches of an `if` chain, of `?:`, of a switch, by a loop body, a `catch` block, and the body of a lambda, anonymous method, or local function; `try`, `finally`, `using`, `lock`, plain blocks, `??`, `?.`, `return`, `throw`, `break`, `continue`, case labels, and recursion add nothing and do not nest.
- **No-nesting exception** (RH2009-004) — a member whose complexity holds no nesting increment — every structure at nesting level 0 — is never reported, whatever its complexity. Once one structure has a nesting increment, the whole complexity is compared with the limit.
- **Nested functions** (RH2009-005) — the structures of lambdas, anonymous methods, and local functions count toward the member that holds them, one nesting level deeper; a local function is also judged on its own.
- **Limit** (RH2009-001) — `design.maximumMethodComplexity` in `reihitsu.json`, default 15, one limit for production and test code. A member is reported when its complexity is greater than the limit. RH0001 validates the setting like the line limits (RH0001-007), and a setting without a valid value keeps the default.
- **Diagnostic** (RH2009-006) — one per member, on the same token as RH2008: "This member has a cognitive complexity of {0}, which exceeds the limit of {1}."

This repository's own code is not held to the rule in this change; the violations it reports there, and a review of the default, are a follow-up (maintainer clarification).

## Why

Nothing in the analyzer set makes a method's complexity visible; RH2008 catches long methods, not short, deeply nested ones. Cognitive complexity measures how hard a body is to follow: it charges for nesting, which the reader has to hold in mind, and charges a switch once instead of once per case, so it tracks readability rather than the number of paths.

The no-nesting exception keeps the rule on what the measure is for: a body whose decisions stand one after another — a validation sequence, a dispatch on an `if … else if` chain — reads one decision at a time; it is long rather than complex, which RH2008 judges.

The limit lives in `reihitsu.json`, the one place where Reihitsu's own settings live and where RH0001 reports a typo or an invalid value, as for the line limits. The issue proposed an `.editorconfig` key, which no Reihitsu rule reads.

## Visible output changes

Every example is production code without a `reihitsu.json`, unless it says otherwise.

**1. A new diagnostic on a nested body above the limit.** The issue's example has a cognitive complexity of 16 — `foreach` 1, `if` 2, `if` 3, `for` 4, `if` 5, `else if` 1:

```csharp
// input, and before: not reported
class C
{
    public int Score(int[] values)
    {
        int total = 0;

        foreach (var value in values)
        {
            if (value > 0)
            {
                if (value % 2 == 0)
                {
                    for (int i = 0; i < value; i++)
                    {
                        if (i % 3 == 0)
                        {
                            total += i;
                        }
                        else if (i % 5 == 0)
                        {
                            total -= i;
                        }
                    }
                }
            }
        }

        return total;
    }
}
```

```csharp
// after: RH2009, "This member has a cognitive complexity of 16, which exceeds the limit of 15."
class C
{
    public int {|RH2009:Score|}(int[] values)
    {
        // body unchanged
    }
}
```

The issue's compliant example — `Score` 1, `ScoreValue` 3, `AdjustForIndex` 2 — is not reported, before and after.

**2. Deeper nesting costs more.** With `{ "design": { "maximumMethodComplexity": 5 } }`:

```csharp
// before: not reported (RH0001 reports "Unknown configuration setting 'design.maximumMethodComplexity'.")
// after: RH2009 on `M` (1 + 2 + 3 = 6); `N` (1 + 1 + 1 = 3) is not reported
class C
{
    void {|RH2009:M|}(int a)
    {
        if (a > 0)
        {
            if (a > 1)
            {
                if (a > 2)
                {
                    a--;
                }
            }
        }
    }

    void N(int a)
    {
        if (a > 0)
        {
            a--;
        }

        if (a > 1)
        {
            a--;
        }

        if (a > 2)
        {
            a--;
        }
    }
}
```

**3. No nesting, no report.** With `{ "design": { "maximumMethodComplexity": 1 } }`, a body whose structures all stand at nesting level 0 is not reported, before and after, whatever its complexity — here 9:

```csharp
class C
{
    int M(int value)
    {
        if (value == 1)
        {
            return 10;
        }
        else if (value == 2 && value > 0)
        {
            return 20;
        }
        else
        {
            value++;
        }

        for (var i = 0; i < value; i++)
        {
            value--;
        }

        value = value > 5 || value < 0 ? 0 : 1;

        if (value > 4)
        {
            value--;
        }

        switch (value)
        {
            case 3:
                return 30;

            default:
                return value;
        }
    }
}
```

**4. Nesting through a lambda or a local function counts.** With `{ "design": { "maximumMethodComplexity": 1 } }`:

```csharp
// before: not reported
// after: RH2009 on `M`, "This member has a cognitive complexity of 2, which exceeds the limit of 1."
using System.Linq;

class C
{
    int {|RH2009:M|}(int[] values) => values.Count(value =>
    {
        if (value > 0)
        {
            return true;
        }

        return false;
    });
}
```

**5. RH0001 accepts the new setting.**

```json
{ "design": { "maximumMethodComplexity": 20 } }
```

Before: "The reihitsu.json configuration is invalid: Unknown configuration setting 'design.maximumMethodComplexity'." After: valid. `{ "design": { "maximumMethodComplexity": 0 } }` reports, after, "The reihitsu.json configuration is invalid: The 'design.maximumMethodComplexity' setting must be a whole number from 1 to 2147483647.", and RH2009 keeps the default of 15.

**6. A new rule page,** `documentation/rules/RH2009.md`.

## Decisions

### Cognitive complexity, default 15, configurable in `reihitsu.json`

The maintainer decided the metric (cognitive, as the issue proposes, after first naming cyclomatic), the default of 15 as a starting point to be tried out on this repository, and configuration through `reihitsu.json` like the line limits.

### A member without a nesting increment is never reported

The maintainer decided that a method that never nests one structure inside another is not reported, whatever its complexity, and that deeper nesting costs more points. RH2009-004 states the exception by the nesting increment of RH2009-003, so the exception and the score share one notion of nesting.

### This repository's own violations are a follow-up

The maintainer decided that the violations RH2009 reports in this repository's own code are ignored in this change; their handling and a review of the default come in a follow-up.

## Alternatives considered

- **Cyclomatic complexity** — rejected by the maintainer: it counts paths and charges nesting nothing, so a deeply nested body scores like a flat one.
- **An `.editorconfig` key** (`dotnet_diagnostic.RH2009.<option>`), as the issue proposed — rejected: no Reihitsu rule reads one, a mistyped key is silently ignored, and the line limits already live in `reihitsu.json`, where RH0001 validates them.
- **Lambdas judged on their own**, as the issue proposed — rejected: a lambda has no name to report on and is read as part of the member; its structures count toward that member, one level deeper.
- **Counting recursion**, which the published definition of cognitive complexity charges — rejected: the issue does not ask for it, and the rule stays a judgment of the body's structure; a recursive call is no harder to read than another call.
- **Counting `??`, `?.`, and `try`** — rejected: they do not make the reader track a decision, and the published definition does not count them.
