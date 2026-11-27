Role: skeptic
Commit: 1c7e70e
Verdict: proceed

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

## Premise

Real for both rules, observed by running them. The caller is the analyzer itself, run by the compiler on user code (build and IDE); no formatter or code-fix path is involved.

`scripts/apply-fix.sh` refuses both IDs ("no code fix provider declares 'RH4124' as fixable"), so I ran the five single-letter analyzers (RH4122 to RH4126) through `CompilationWithAnalyzers` in a throwaway file-based app outside the repository (`#:project src/Reihitsu.Analyzer/Reihitsu.Analyzer.csproj`, net11, `LanguageVersion.Preview`, trusted-platform references; it prints compiler errors first, checked with a deliberately broken input). No input below has a compiler error.

Fixture 1 — the change doc's RH4124 example plus the lambda form:

```csharp
using System.Linq;

class C
{
    void M(int[] values, int[] others)
    {
        var doubled = from x in values
                      let y = x * 2
                      join o in others on y equals o into g
                      select g.Count() into k
                      select k;
    }
}

class C2
{
    void M(int[] values)
    {
        var doubled = values.Select(x => x);
    }
}
```

Observed: only `RH4124 Lambda.cs:7 'x'`. None of `x`, `y`, `o`, `g`, `k` is reported by any of RH4122 to RH4126.

Fixture 2 — RH4122:

```csharp
public static class E
{
    extension(string t)
    {
        public bool IsBlank => t.Length == 0;
    }

    public static int Square(this int s) => s * s;
}

public static class E2
{
    extension(int)
    {
        public static int Zero => 0;
    }
}
```

Observed: only `RH4122 Block.cs:8 's'`. `GetDeclaredSymbol` on the receiver `string t` returns a parameter whose `ContainingSymbol` is a `NamedType` (the extension block's type); on `int` it returns a parameter with the empty name.

## Mechanism and candidates

- **RH4124**: the analyzer registers only `SyntaxKind.Parameter` and keeps parameters whose containing method is an `AnonymousFunction`; a range variable is declared by a query clause, not by a `ParameterSyntax`, so no registration reaches it. Candidates, from the Roslyn syntax types that carry a range-variable identifier: `FromClauseSyntax` (also `QueryExpressionSyntax.FromClause`, also with an explicit type), `LetClauseSyntax`, `JoinClauseSyntax`, `JoinIntoClauseSyntax`, `QueryContinuationSyntax`. `SelectClause`, `WhereClause`, `OrderByClause`, and `GroupClause` declare nothing. RH4124-003 names exactly these five; nothing is missing.
- **RH4122**: `IsSupportedParameter` accepts a parameter only when its `ContainingSymbol` is a method (not a local function or anonymous function) or an indexer; an extension-block receiver's `ContainingSymbol` is the extension `INamedTypeSymbol` (observed), so it falls through. Partition of `ParameterSyntax` owners by containing symbol across RH4122/RH4123/RH4124: method kinds and indexers (RH4122), local functions (RH4123), anonymous functions (RH4124), the extension type (nobody — the gap). Delegates (`Invoke`), primary constructors, operators, and conversions are methods; function-pointer parameters have no identifier; the setter's `value` has no syntax. The receiver is the only member that falls through, so the partition is complete after RH4122-004.

## Breaking inputs

Failure classes that reach the change: FC-002, FC-003, FC-016, FC-023, FC-024, FC-027, FC-034. The formatter classes (FC-013, FC-014) do not apply: names are not something a formatter phase judges. FC-036 does not apply: neither rule has a code fix.

All inputs below were compiled and run through the probe on commit 1c7e70e; none is reported today (so each positive one fails today, as the tests must show). No existing test pins any of them: `RH4122…Tests.cs` and `RH4124…Tests.cs` contain no `extension(` and no query expression; only `RH4105MethodParameterCasingAnalyzerTests.cs` uses extension blocks, for casing.

RH4124 — every clause kind × enclosing scope × identifier form:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

class C
{
    static IEnumerable<int> F = from f in new[] { 1 } select f;           // field initializer

    IEnumerable<int> P => from p in new[] { 1 } select p;                  // expression-bodied property

    void M(int[] values, int[] others, object[] objects)
    {
        var a1 = from int t in objects select t;                           // explicit type
        var a2 = from _ in values select 0;                                // _ : not reported
        var a3 = from v in values let _ = v select v;                      // let _ : not reported
        var a4 = from v in values join _ in others on v equals _ select v; // join _ : not reported
        var a5 = from v in values join w in others on v equals w into _ select v; // into _ : not reported
        var a6 = from v in values select v into _ select 0;                // continuation _ : not reported
        var a7 = from @a in values select @a;                              // verbatim
        var a8 = from é in values select é;                                // non-ASCII letter
        var a9 = from X in values select X;                                // uppercase
        var b1 = from v in values from u in others select v + u;           // second from
        var b2 = from v in values select (from z in others where z == v select z).Count(); // nested query
        Func<int[], IEnumerable<int>> b3 = arr => from q in arr select q;  // query in lambda
        int Local(int[] arr) => (from r in arr select r).Sum();            // query in local function
        var b4 = from v in values orderby v group v by v into h select h.Key; // group continuation
        var b5 = from ab in values select ab;                              // two letters: not reported
    }
}
```

Expected under RH4124-003: `f`, `p`, `t`, every single-letter `v`/`u`/`w`, `@a`, `é`, `X`, `z`, `q`, `r`, `h`; not `_` (five forms, all of which compile as range variables), not `ab`, and no use (`select v`, `equals w`) reported. The test implementer may split this into focused tests; the members that matter: each of the five clause kinds (FC-016), `_` in each clause kind and a two-letter name (FC-003, FC-023), a nested query and a query inside a lambda, a local function, and a field initializer (FC-027), and the issue's own shape `from x in values let y = x * 2 select y` (FC-034, already in RH4124-003's example).

RH4122 — receiver forms:

```csharp
using System.Diagnostics.CodeAnalysis;

public static class E
{
    extension(string t)
    {
        public void M(int a) { }
        public static void N(int b) { }
        public int this[int i] => i;
    }

    extension<T>(T[] a) { public int Size => a.Length; }
    extension(ref int r) { public void Inc() => r++; }
    extension(in long l) { public long Twice => l * 2; }
    extension([NotNull] string? s) { public bool Has => s is not null; }
    extension(object @o) { public object Self => @o; }
    extension(char é) { public char Same => é; }
    extension(double D) { public double Half => D / 2; }
    extension(decimal value) { public decimal Neg => -value; }
}

public static partial class G { extension(byte x) { public byte Id => x; } }
public static partial class G { extension(byte y) { public byte Id2 => y; } }

public static class H { extension(string _) { public static int Zero => 0; } }
```

Observed today: only the member parameters `a`, `b`, `i` (RH4122-001). Expected under RH4122-004 in addition: `t`, `a` (generic), `r`, `l`, `s`, `@o`, `é`, `D`, `x`, `y` — one each; not `value`, not `_`, not `extension(int)`. Members: modifiers and attributes on the receiver (FC-016), `_`/verbatim/non-ASCII/uppercase (FC-023), blocks in both parts of a partial static class (FC-024), unnamed receiver and long name (FC-002, FC-003, in the spec examples). Add one block with several members, like `extension(string t)` above, so "one diagnostic per extension block" is pinned (the spec examples have one member per block); a symbol-based implementation that also visits the compiler's synthesized implementation methods would report the receiver once per member.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4124 analyzer | Must change: report the five clause kinds |
| RH4122 analyzer | Must change: accept a receiver whose container is an extension type, named only |
| RH4123, RH4125, RH4126 analyzers | Must stay: none reports a range variable or a receiver today (observed), and none should |
| RH4105 | Must stay: already reports receiver casing (`extension(string Text)` → RH4105, observed) |
| RH3007 | Must stay: still reports every query on its first `from` (observed on the casing fixture) |
| Code fixes, Fix All | Not involved: neither rule has a fix, and none is added (SHARED-SINGLE non-goals) |
| Formatter, CLI, `Reihitsu.Core` | Not involved |
| `SingleLetterIdentifierUtilities` | Must stay; `HasSingleLetterName` already decides the name |
| Rule pages | Must change: *Description* of RH4124 and RH4122 (spec author did); H1 only if output question 1 picks B |
| `AnalyzerResources.resx`, package README row | Only if output question 1 picks B; `AnalyzerPackageMetadataTests` keeps resource title, rule-page H1, and README row equal |
| Tests | Must change: RH4124 and RH4122 test classes |
| Self-hosting | Not affected: a Roslyn scan of `src/`, `tests/`, `scripts/` finds no real (non-literal) query clause with a one-character identifier and no extension block with a one-character receiver, so neither the `binaries/` bump nor the self-hosting rows will report new findings |

## Refuse option

For an analyzer gap, refusing means not reporting the class: the change is not made. That contradicts the maintainer's recorded decision on #1089, and neither class is rare in the sense of *Code-fix coverage*: extension blocks are the C# 14 form of extension declarations, and query syntax is ordinary C# (though RH3007 already asks to avoid it). The requested change is small: one more registration (or a handler over five syntax kinds) in RH4124 and one branch in RH4122's container check, no new type, no new mechanism. Declaring a shape unsupported is not an option: no formatter phase or code fix is involved. No simpler option.

## Output questions

The spec author's question 1 (RH4124 title and message, A keep / B widen) stands; I agree with B. Reported on `from x`, the current text "Single-letter lambda and anonymous method parameter names should not be used" names a construct that is not written there; B costs one resource value, the rule-page H1, the README row, and the spec title, all held equal by `AnalyzerPackageMetadataTests`. No further output question: every other visible change follows the maintainer's recorded decisions.

## Internal choices

- RH4124-003's *Why* says the compiler's lambdas "take it as their parameter". That holds for the clause that introduces a variable, but a variable introduced by `let` or a second `from` reaches later lambdas through a transparent identifier, not as their parameter. Recommend: "in method syntax the same value is named by a lambda parameter, as `values.Select(x => x)` shows". Spec wording only; no output.
- The change doc could name RH3007 in *Why*: with RH3007 on (the default), a query with single-letter range variables now also gets one RH4124 per variable, and rewriting it to method syntax by hand keeps those reports on the lambda parameters. This is consistent, not a conflict; one sentence would pre-empt a "duplicate warnings" reading. No output.
- The "Done when" clause "fixed where the rule's fix applies" is empty for both rules (no fix); state that in the PR rather than adding anything.

## Issue goals

The issue's two decisions and its "Done when" are kept; nothing to drop. The spec states the RH4124 boundary by cause (every clause that declares a range variable) instead of the four keywords; that covers the first `from`, an explicitly typed `from`, and both kinds of `into` — the same set the decision means, with no visible difference from it.

Severity: the RH4124 half is real but narrow. RH3007 (Warning, on by default) already reports every query expression, so the new RH4124 reports add to an existing warning for most users and stand alone only where RH3007 is disabled. The RH4122 half closes a gap on the current extension syntax. Neither changes the decision.

## Architecture findings

_None._ The per-rule partition by containing symbol had one member without an owner (the receiver), which RH4122-004 assigns; no shared decision is duplicated.

## Follow-up candidates

1. **Casing of query range variables is unchecked.** RH4105 reports a non-camelCase lambda parameter, but no rule reports a non-camelCase range variable — the same gap as this issue's RH4124 half, for casing. Observed with all analyzers of the assembly: `values.Select(Value => Value)` → `RH4105 'Value'`; `from Value in values let Doubled = Value * 2 select Doubled` → only `RH3007 'from'`. Out of scope (the issue is about single-letter names). Output question for the follow-up issue (check for a duplicate first; it may have been raised with the origin issues):

   **RH4105 · Method parameter names should be in camelCase — query range variable**

   Input:
   ```csharp
   var doubled = from Value in values
                 let Doubled = Value * 2
                 select Doubled;
   ```

   Before (`main`): not reported.

   After, A (recommended): `Value` and `Doubled` reported by RH4105, with its rename fix, like the lambda parameter of `values.Select(Value => Value)`.

   After, B: not reported.

   Why A: the range variable names the same value as the lambda parameter in method syntax, which this run already uses to place range variables under the parameter rules.
