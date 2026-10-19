Role: spec author
Commit: 502673c
Verdict: spec written

Repair round on the skeptic's findings (`02-skeptic.md`, verdict "proceed narrower"). I re-ran every finding myself against the code before changing anything: analyzer cases with my scratch harness outside the repository (net10, `LanguageVersion.Preview`, shipped `Reihitsu.Analyzer`), fix cases with `scripts/apply-fix.sh` (LF and CRLF). All ten numbered findings and the smaller points reproduce; I agree with each and found no point to dispute. No requirement ID was added, removed, or renumbered; no change doc (all requirements still state current behavior). This handover supersedes the open-question and follow-up lists of `01-spec-author.md`; it is self-contained.

# Findings and what changed

| # | Finding (re-derived) | Change |
|---|---|---|
| 1 | `items.ToImmutableDictionary(x => x, x => 0, null, valueComparer)` (`IEqualityComparer<int>` for the value) is not reported by RH1002; without the comparers it is. `HasExplicitEqualityComparerArgument` accepts any `IEqualityComparer<>` parameter. | SHARED-EQUAL-002 gains a coverage sentence: only a parameter `IEqualityComparer<T>` whose `T` is the compared type. New question struct-equality Q3. |
| 2 | `static partial class Helpers { }` + `partial class Helpers { }` nested in a class: RH2101 reports the second part (`IsStaticType` reads one part's modifiers). | RH2101-002 coverage sentence: parts all `static` or all not. New issue RH2101 Q1. |
| 3 | `static N IAdd<N>.operator +(…)` and `static explicit IAdd<N>.operator int(…)` are reported by RH2004; the fix adds `private` (`private static N IAdd<N>.operator +` — not valid, CS0106). | RH2004-001 coverage sentence excludes explicit interface implementations of operators and conversion operators. RH2004 Q2 (reporting, ordinary) and Q3 (fix, P1). |
| 4 | Each part of a partial property, indexer, and event is reported; of a partial constructor only the implementing part. `apply-fix.sh` on a partial property converges in 2 iterations, one part per application, so one application leaves one part `private` and the other without a modifier (CS8799). | RH2004-001 coverage sentence excludes partial constructors; RH2004-006 coverage excludes `partial` declarations. RH2004 Q4 (fix, P1) and Q5 (unreported defining part, ordinary). |
| 5 | `private readonly int Value { get; }` in a struct → `private readonly readonly int _value;` (CS1004). | RH2001-004 coverage sentence: property without `readonly`. RH2001 Q6 (P1). |
| 6 | A target-typed `new()` that is the operand of a cast to the collection type (also through parentheses, `!`, conditional branches, `switch` arms) is not reported; the cast type is. Explicit creations initializing a declaration are reported in addition to the declared type. Also verified: `=> new()` and `d = new();` are reported on `new`; a property initializer `= new();` only on the type. | RH1001-002 text and Why corrected; example extended (explicit creation in a declaration, cast with conditional, assignment, arrow body). RH1001-003: several declarators all count (`Variables.All`), example extended. |
| 7 | RH2109 also reports `@code {` inside a C# `/* */` comment, inside a C# string, and inside a raw string of a code block. P5/SHARED-DIAG-003 is scoped to C# comments; Razor and HTML comments need a decision. | RH2109 Q1 is now an ordinary question with P5 as precedent, covering Razor, HTML and C# comments and C# literals. RH2109-001 coverage excludes C# comments and literals too. |
| 8 | `Debug.Assert(c, f ? "" : null)` and `Debug.Fail(f ? " " : "")` are not reported. `s ?? ""` is not reported either (may have text; not questioned). | SHARED-DEBUG-001 coverage now keeps out exactly the filed inputs: `string.Empty`, and conditional or `switch` expressions whose every result is a constant without text. SHARED-DEBUG Q1 widened. |
| 9 | Verified: `LeftJoin`, `RightJoin` (.NET 10), `ImmutableHashSet.Create<S>()`, `ImmutableHashSet.CreateRange(items)`, `ImmutableDictionary.CreateRange(pairs)` assigned to `var` — reported by neither RH1002 nor RH1001. | Added to RH1002 Q1. No spec change (RH1002-001 lists names exhaustively). |
| 10 | Get-only `private Counter Current { get; }` with `Current.Increment()` in the constructor: the fix gives `private readonly Counter _current;`; in a constructor the readonly field is a variable, so behavior changes too. | RH2001 Q3 parenthetical corrected. |

Smaller points:

- `documentation/rules/RH1002.md`: "Only the `System.Linq.Enumerable` methods are checked" was wrong (the `ToImmutable…`/`ToFrozen…` methods live on other classes and are checked). Corrected to "`Contains` and `SequenceEqual` are checked only as `System.Linq.Enumerable` methods. Calls of them that bind to other types … are not reported."
- RH2004-006: members of a C# 14 extension block are reported and get `private`, which is their default (verified: `1.Twice()` from another class fails with CS1061). Added to the `private` bullet and to the fix example (verified with `apply-fix.sh`).
- RH0002-002: prose example added for the comments-only file (empty span after the final line break) and the empty file (empty span at its start), as observed.
- SHARED-EQUAL-001 coverage now leaves out exactly generic structs that have their own equality members but a type argument lacking them (`S?`, `(int, S)`); `KeyValuePair<S, int>` stays covered.
- SHARED-EQUAL-002 example gained the missing forms: `switch` on both sides, null-conditional access with `null` receiver, non-trailing named argument (`Enumerable.Distinct(source: items, comparer)`).
- Non-goals that restated requirements removed: RH2006 and RH2007 detail message, RH2002 return types, SHARED-NESTED containing-type sentence.
- RH2006/RH2007 parallel wording kept: they are requirements about two different methods, and SHARED-DEBUG's non-goal deliberately leaves "which method, how the argument is found" to each rule. The lead may decide otherwise.
- `documentation/rules/RH2004.md` sentence from round 1 stays.

Checks after the changes: every example block of the changed specs re-run through the harness (all match the markup, no compile errors); RH2004-006 before/after re-run with `apply-fix.sh` (output equals the after-block). `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentationConventionTests"`: 26 passed. `RequirementCoverageTests` still fails only for the 60 untagged new IDs.

## Changed requirements

All 60 requirements from round 1 stay (list in `01-spec-author.md`). Changed in this round:

- SHARED-EQUAL-001 — A struct without its own equality members — clarified (coverage sentence narrowed)
- SHARED-EQUAL-002 — A custom comparer argument replaces the equality members — clarified (coverage sentence, example)
- SHARED-DEBUG-001 — A constant without text is no message — clarified (coverage sentence)
- RH0002-002 — The diagnostic is on the first token of the file — clarified (example)
- RH1001-002 — Every use of the collection type is reported — changed (corrected: cast operand, explicit creation in a declaration; Why; example)
- RH1001-003 — A creation with a custom comparer is not reported — clarified (several declarators; example)
- RH2001-004 — The fix replaces the property with a private field — clarified (coverage sentence)
- RH2004-001 — A declaration without an accessibility modifier is reported — clarified (coverage sentence)
- RH2004-006 — The fix adds the default accessibility — clarified (extension block; coverage sentence; example)
- RH2101-002 — A nested static class is not reported — clarified (coverage sentence)
- RH2109-001 — A `@code` block in a `.razor` file is reported — clarified (coverage sentence)

## Examples for the tests

Every requirement's example seeds tests; notes from round 1 still hold. New in this round:

- SHARED-EQUAL-002: the added `switch`, null-conditional, and non-trailing named-argument lines (RH1002).
- RH1001-002: `both`, `cast`, `declared = new()`, `Create() => new()`; RH1001-003: `l, m` (not reported) and `n, o` (type reported once).
- RH0002-002: comments-only and empty file (prose example; no existing test covers them).
- RH2004-006: the extension-block member (needs C# 14).
- Do not pin as intended any input listed in the questions below; at most characterization tests.

## Open questions to file

None blocks this run. One issue per spec path, each a sub-issue of thoenissen/Reihitsu#912, titled exactly as the heading says. None exists yet. Every body starts with the preamble below, with `<spec path>` and `<prefix/rules>` filled in as given in each heading's first line.

Preamble (verbatim, per issue):

> Part of #912 — see there for how questions are decided.
>
> While `<spec path>` (<prefix/rules>) was written from the analyzers' current behavior, the cases below looked wrong. They were deliberately left out of the requirements. Each needs a maintainer decision: **bug** (fix the code and add the case to the spec), **intended** (add the case to the spec as it is), or **change** (change doc first). Record the decision as a comment and tick the box. A question marked "Decided by principle Pn (#912)" is already decided by that principle and needs no question round; it is listed so it becomes a bug fix.

### Issue `SDD open questions: sdd/specs/shared/struct-equality.md`

Preamble with `sdd/specs/shared/struct-equality.md` (SHARED-EQUAL, used by RH1001 and RH1002).

> ## Checklist
>
> - [ ] Q1 — A struct that implements `IEquatable<T>` without overriding `GetHashCode()` counts as having equality members
> - [ ] Q2 — A generic struct with its own equality members counts as having them even when its type argument lacks them (`S?`, `(int, S)`)
> - [ ] Q3 — A comparer for another type argument counts as a custom comparer for the compared type
>
> ## Questions
>
> ### Q1 — A struct that implements `IEquatable<T>` without overriding `GetHashCode()` counts as having equality members
>
> Input:
>
> ```csharp
> using System;
> using System.Collections.Generic;
>
> struct S : IEquatable<S>
> {
>     public int Value;
>
>     public bool Equals(S other) => Value == other.Value;
> }
>
> class C
> {
>     HashSet<S> _set = new HashSet<S>();
> }
> ```
>
> Output: RH1001 reports nothing (likewise RH1002 for `items.Distinct()` on `IEnumerable<S>`).
>
> Why it looks wrong: hash-based collections and operations call `GetHashCode()` through `EqualityComparer<S>.Default`, which for this struct is `ValueType.GetHashCode` — the boxing, reflection-based fallback the rules exist to report. `IEquatable<T>` removes only the cost of `Equals`. The rule pages say "Implement `IEquatable<T>`, or override both `Equals(object)` and `GetHashCode()`", and existing tests pin the current behavior, so the question is whether that "or" is intended. Touches SHARED-EQUAL-001 (its coverage sentence leaves this case out), RH1001-001, RH1002-001.
>
> ### Q2 — A generic struct with its own equality members counts as having them even when its type argument lacks them
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
>
> struct S
> {
>     public int Value;
> }
>
> class C
> {
>     Dictionary<S?, int> a;
>     Dictionary<(int, S), int> b;
>     Dictionary<S, int> c;
> }
> ```
>
> Output: RH1001 reports only `c`.
>
> Why it looks wrong: `Nullable<S>` overrides `Equals`/`GetHashCode` and `ValueTuple<int, S>` implements `IEquatable<T>`, but both delegate to `EqualityComparer<S>.Default`, which falls back to `ValueType.Equals`/`GetHashCode` for `S`. The cost reported for `c` is the same for `a` and `b`; only the outer type is inspected. Touches SHARED-EQUAL-001 (its coverage sentence leaves this case out), RH1001-001, RH1002-001.
>
> ### Q3 — A comparer for another type argument counts as a custom comparer for the compared type
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Collections.Immutable;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items, IEqualityComparer<int> valueComparer)
>     {
>         items.ToImmutableDictionary(x => x, x => 0, null, valueComparer);
>         items.ToImmutableDictionary(x => x, x => 0);
>     }
> }
> ```
>
> Output: RH1002 reports only the second call.
>
> Why it looks wrong: the first call passes `null` as key comparer and a comparer for the `int` values; the keys of type `S` are still hashed by `EqualityComparer<S>.Default`. Any argument passed to an `IEqualityComparer<>` parameter is accepted as a custom comparer, whatever its type argument. The `ToImmutableDictionary` overloads with `keyComparer, valueComparer` are the shapes that reach this. Touches SHARED-EQUAL-002 (its coverage sentence leaves this case out), RH1002-001.

### Issue `SDD open questions: sdd/specs/analyzer/RH1002.md`

Preamble with `sdd/specs/analyzer/RH1002.md` (RH1002).

> ## Checklist
>
> - [ ] Q1 — Equality-based methods and factories outside the rule's fixed list are not checked
>
> ## Questions
>
> ### Q1 — Equality-based methods and factories outside the rule's fixed list are not checked
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Collections.Immutable;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items, IEnumerable<KeyValuePair<S, int>> pairs)
>     {
>         items.CountBy(x => x);
>         items.AggregateBy(x => x, 0, (count, x) => count + 1);
>         items.LeftJoin(items, x => x, x => x, (a, b) => a);
>         items.RightJoin(items, x => x, x => x, (a, b) => a);
>         var set = ImmutableHashSet.Create<S>();
>         var range = ImmutableHashSet.CreateRange(items);
>         var map = ImmutableDictionary.CreateRange(pairs);
>         items.GroupBy(x => x);
>     }
> }
> ```
>
> Output: RH1002 reports only `GroupBy`; RH1001 reports nothing (the factory results are typed with `var`).
>
> Why it looks wrong: `CountBy` and `AggregateBy` (.NET 9) and `LeftJoin` and `RightJoin` (.NET 10) hash the selected key and take an optional `IEqualityComparer<TKey>`, exactly like `GroupBy` and `Join`, which the rule checks. `ImmutableHashSet.Create`/`CreateRange` and `ImmutableDictionary.CreateRange` build the hash-based collections that `ToImmutableHashSet`/`ToImmutableDictionary` build, which the rule checks. All are missing from the rule's fixed list of method names. Touches RH1002-001.

### Issue `SDD open questions: sdd/specs/shared/debug-message-text.md`

Preamble with `sdd/specs/shared/debug-message-text.md` (SHARED-DEBUG, used by RH2006 and RH2007).

> ## Checklist
>
> - [ ] Q1 — `string.Empty`, and a conditional without text on every path, count as message text
>
> ## Questions
>
> ### Q1 — `string.Empty`, and a conditional without text on every path, count as message text
>
> Input:
>
> ```csharp
> using System.Diagnostics;
>
> class C
> {
>     void M(bool condition, bool flag)
>     {
>         Debug.Assert(condition, string.Empty);
>         Debug.Assert(condition, flag ? "" : null);
>         Debug.Assert(condition, "");
>         Debug.Fail(string.Empty);
>         Debug.Fail(flag ? " " : "");
>     }
> }
> ```
>
> Output: RH2006 reports only `Debug.Assert(condition, "")`; RH2007 reports nothing.
>
> Why it looks wrong: `string.Empty` is the framework's spelling of the empty string, and a conditional (or `switch`) expression whose every result is `null`, empty, or white space gives no text on any path; neither gives the failure more text than `""`. They escape because only compile-time constants are checked (`string.Empty` is a `static readonly` field; a conditional with a non-constant condition has no constant value). The rule pages say "Constant `null`, empty, and whitespace-only messages are treated as missing text", which matches the code; SHARED-EQUAL-002 looks through conditionals in a comparable situation. Touches SHARED-DEBUG-001 (its coverage sentence leaves exactly these cases out), RH2006-001, RH2007-001.

### Issue `SDD open questions: sdd/specs/analyzer/RH2001.md`

Preamble with `sdd/specs/analyzer/RH2001.md` (RH2001).

> ## Checklist
>
> - [ ] Q1 — The fix drops comments inside the property (Decided by principle P1 (#912))
> - [ ] Q2 — The fix is offered although code excluded by `#if` refers to the property (Decided by principle P1 (#912))
> - [ ] Q3 — The fix changes run-time behavior for a property of a mutable struct type (Decided by principle P1 (#912))
> - [ ] Q4 — A property whose accessors use `field` with logic of their own is reported
> - [ ] Q5 — A property with a `field:`-targeted or accessor attribute is reported
> - [ ] Q6 — The fix on a `readonly` property writes `readonly` twice (Decided by principle P1 (#912))
>
> ## Questions
>
> ### Q1 — The fix drops comments inside the property (Decided by principle P1 (#912))
>
> Input:
>
> ```csharp
> public class C
> {
>     private int Value /* name */ { get; /* inner */ set; } // trailing
>
>     public int Get() => Value;
> }
> ```
>
> Output (`scripts/apply-fix.sh RH2001`):
>
> ```csharp
> public class C
> {
>     private int _value; // trailing
>
>     public int Get() => _value;
> }
> ```
>
> Why it is wrong: the comments `/* name */` and `/* inner */` are removed. SHARED-FIX-003 (third condition) withholds a fix that changes the text of a comment the user wrote. Touches RH2001-004, SHARED-FIX-003.
>
> ### Q2 — The fix is offered although code excluded by `#if` refers to the property (Decided by principle P1 (#912))
>
> Input:
>
> ```csharp
> public class C
> {
>     private int Value { get; set; }
>
>     public int Get()
>     {
> #if NEVER
>         return Value;
> #else
>         return 0;
> #endif
>     }
> }
> ```
>
> Output: the property becomes `private int _value;`; the excluded `return Value;` is left as it is and no longer compiles when `NEVER` is defined.
>
> Why it is wrong: SHARED-FIX-003 (fourth condition) withholds a fix that leaves code excluded by `#if` referring to a name the fix changed. Touches RH2001-004, SHARED-FIX-003.
>
> ### Q3 — The fix changes run-time behavior for a property of a mutable struct type (Decided by principle P1 (#912))
>
> Input:
>
> ```csharp
> public struct Counter
> {
>     public int Count;
>
>     public void Increment() => Count++;
> }
>
> public class C
> {
>     private Counter Current { get; set; }
>
>     public int Run()
>     {
>         Current.Increment();
>
>         return Current.Count;
>     }
> }
> ```
>
> Output: `private Counter _current;` with `_current.Increment(); return _current.Count;`.
>
> Why it is wrong: before the fix, `Current.Increment()` mutates a copy returned by the getter and `Run()` returns 0; after it, the field itself is mutated and `Run()` returns 1. SHARED-FIX-003 (second condition) withholds a fix that changes what the program does at run time. A get-only property is affected in a constructor as well: there the `readonly` field the fix creates is a variable, so `Current.Increment()` in the constructor mutates the field afterwards instead of a copy. Touches RH2001-004, SHARED-FIX-003.
>
> ### Q4 — A property whose accessors use `field` with logic of their own is reported
>
> Input:
>
> ```csharp
> public class C
> {
>     private int Value { get => field; set => field = value * 2; }
> }
> ```
>
> Output: RH2001 reports `Value`; no code fix is offered.
>
> Why it looks wrong: the rule is about auto-implemented properties, whose accessors only read and write the backing field. The analyzer decides "auto-implemented" by whether the compiler created a backing field, which it also does for a property using the C# 14 `field` keyword, so a property with setter logic is reported although a field cannot replace it. (`{ get => field; }` without logic is reported too; that one looks consistent with the rule.) Touches RH2001-001 (its coverage sentence leaves accessors with bodies out), RH2001-002.
>
> ### Q5 — A property with a `field:`-targeted or accessor attribute is reported
>
> Input:
>
> ```csharp
> using System;
> using System.Diagnostics.CodeAnalysis;
>
> public class C
> {
>     [field: NonSerialized]
>     private int Cache { get; set; }
>
>     private int Value { [ExcludeFromCodeCoverage] get; set; }
> }
> ```
>
> Output: RH2001 reports both properties; no code fix is offered for either.
>
> Why it looks wrong: the rule page says "Properties decorated with attributes are exempt", and a property with `[Obsolete]` is not reported, but the analyzer only looks at attributes that apply to the property itself. Two readings: the exemption is for any attribute written on the declaration (then neither should be reported), or only for attributes that need property syntax (then both are rightly reported, the page needs a correction, and a fix could move `[field: X]` to the field as `[X]`). Touches RH2001-001 (its coverage sentence leaves this case out), RH2001-002.
>
> ### Q6 — The fix on a `readonly` property writes `readonly` twice (Decided by principle P1 (#912))
>
> Input:
>
> ```csharp
> public struct S
> {
>     private readonly int Value { get; }
>
>     public S(int value)
>     {
>         Value = value;
>     }
>
>     public int Get() => Value;
> }
> ```
>
> Output (`scripts/apply-fix.sh RH2001`): `private readonly readonly int _value;`, which fails with CS1004 (duplicate `readonly` modifier).
>
> Why it is wrong: the fix adds `readonly` for a get-only property without checking whether the property already carries it. SHARED-FIX-003 (first condition) withholds a fix whose result has a compile error the code did not have. Touches RH2001-004 (its coverage sentence leaves this case out), SHARED-FIX-003.

### Issue `SDD open questions: sdd/specs/analyzer/RH2002.md`

Preamble with `sdd/specs/analyzer/RH2002.md` (RH2002).

> ## Checklist
>
> - [ ] Q1 — An async void explicit interface implementation is not reported
> - [ ] Q2 — An async void partial method is reported twice, once on its non-async definition
>
> ## Questions
>
> ### Q1 — An async void explicit interface implementation is not reported
>
> Input:
>
> ```csharp
> using System.Threading.Tasks;
>
> interface I
> {
>     void M();
>     void N();
> }
>
> class C : I
> {
>     async void I.M() { await Task.Delay(1); }
>
>     public async void N() { await Task.Delay(1); }
> }
> ```
>
> Output: RH2002 reports only `N`.
>
> Why it looks wrong: both implement a `void` interface member and carry the same async void hazards. The implicit implementation and an `override async void` are reported; only the explicit form is skipped, because the analyzer checks only ordinary methods. Touches RH2002-001 (its coverage sentence leaves this case out).
>
> ### Q2 — An async void partial method is reported twice, once on its non-async definition
>
> Input:
>
> ```csharp
> using System.Threading.Tasks;
>
> partial class P
> {
>     partial void Q();
>
>     async partial void Q() { await Task.Delay(1); }
> }
> ```
>
> Output: RH2002 reports `Q` twice — on the defining declaration `partial void Q();`, which has no `async`, and on the implementing declaration.
>
> Why it looks wrong: it is one method with one async void body; every other function is reported once (RH2002-005), and the defining declaration carries nothing the user could change. Touches RH2002-001 (its coverage sentence leaves partial methods out), RH2002-005.

### Issue `SDD open questions: sdd/specs/analyzer/RH2003.md`

Preamble with `sdd/specs/analyzer/RH2003.md` (RH2003).

> ## Checklist
>
> - [ ] Q1 — A creation through a `using` alias is not reported
>
> ## Questions
>
> ### Q1 — A creation through a `using` alias is not reported
>
> Input:
>
> ```csharp
> using NIE = System.NotImplementedException;
>
> class C
> {
>     void M() => throw new NIE();
> }
> ```
>
> Output: RH2003 reports nothing; `throw new System.NotImplementedException()` is reported.
>
> Why it looks wrong: the created type is `System.NotImplementedException` in both cases. A syntactic pre-filter compares the written simple name with `NotImplementedException` before the semantic check, so an alias name never reaches it. Not covered by SHARED-DIAG-002: the alias is a different name, not a different spelling of the same name. Touches RH2003-001.

### Issue `SDD open questions: sdd/specs/analyzer/RH2004.md`

Preamble with `sdd/specs/analyzer/RH2004.md` (RH2004).

> ## Checklist
>
> - [ ] Q1 — The fix adds `private` to an operator, which must be `public`
> - [ ] Q2 — Explicit interface implementations of operators and conversion operators are reported
> - [ ] Q3 — The fix adds `private` to an explicit interface implementation of an operator (Decided by principle P1 (#912))
> - [ ] Q4 — The fix changes one part of a partial property, indexer, event, or constructor (Decided by principle P1 (#912))
> - [ ] Q5 — The defining part of a partial constructor is not reported
>
> ## Questions
>
> ### Q1 — The fix adds `private` to an operator, which must be `public`
>
> Input:
>
> ```csharp
> public struct S
> {
>     static S operator -(S a, S b) => a;
> }
> ```
>
> Output (`scripts/apply-fix.sh RH2004`): `private static S operator -(S a, S b) => a;`
>
> Why it looks wrong: user-defined operators and conversion operators must be `public static` (CS0558). The input already does not compile, so this is not a SHARED-FIX-003 case, but the fix replaces it with code that still does not compile while removing the RH2004 diagnostic. Either the fix should add `public` for operators, their only valid accessibility, or it should not be offered for them. Touches RH2004-001, RH2004-006 (its coverage sentence leaves operators out).
>
> ### Q2 — Explicit interface implementations of operators and conversion operators are reported
>
> Input:
>
> ```csharp
> interface IAdd<T>
>     where T : IAdd<T>
> {
>     static abstract T operator +(T a, T b);
>
>     static abstract explicit operator int(T value);
> }
>
> public struct N : IAdd<N>
> {
>     static N IAdd<N>.operator +(N a, N b) => a;
>
>     static explicit IAdd<N>.operator int(N value) => 0;
> }
> ```
>
> Output: the code compiles, and RH2004 reports both implementations, on `+` and on `operator` (besides the interface `IAdd`).
>
> Why it looks wrong: an explicit interface implementation cannot carry an accessibility modifier; RH2004-003 exempts explicit implementations of methods, properties, indexers, and events for exactly that reason, but not of operators and conversion operators. Touches RH2004-001 (its coverage sentence leaves this case out), RH2004-003.
>
> ### Q3 — The fix adds `private` to an explicit interface implementation of an operator (Decided by principle P1 (#912))
>
> Input: as in Q2.
>
> Output (`scripts/apply-fix.sh RH2004`): `private static N IAdd<N>.operator +(N a, N b) => a;`, which fails with CS0106 (the modifier `private` is not valid for this item).
>
> Why it is wrong: SHARED-FIX-003 (first condition) withholds a fix whose result has a compile error the code did not have. Independent of how Q2 is decided while the diagnostic exists. Touches RH2004-006 (its coverage sentence leaves operators out), SHARED-FIX-003.
>
> ### Q4 — The fix changes one part of a partial property, indexer, event, or constructor (Decided by principle P1 (#912))
>
> Input:
>
> ```csharp
> public partial class P
> {
>     partial int Prop { get; set; }
>
>     partial int Prop { get => 1; set { } }
> }
> ```
>
> Output: both parts are reported (likewise both parts of a partial indexer and of a partial event). One application of the fix gives `private partial int Prop { get; set; }` next to `partial int Prop { get => 1; set { } }`, which fails with CS8799 (both partial member declarations must have identical accessibility modifiers); `apply-fix.sh` needs a second application for the other part. For a partial constructor (`partial P(int x);` and `partial P(int x) { }`), only the implementing part is reported (Q5), so after its fix the CS8799 error stays.
>
> Why it is wrong: SHARED-FIX-003 (first condition) withholds a fix whose single application leaves a compile error the code did not have; SHARED-FIX-001 requires one application to correct a diagnostic. RH2004-002 exempts partial types and partial methods, but not these partial members. Touches RH2004-006 (its coverage sentence leaves `partial` declarations out), SHARED-FIX-003.
>
> ### Q5 — The defining part of a partial constructor is not reported
>
> Input:
>
> ```csharp
> public partial class P
> {
>     partial P(int x);
>
>     partial P(int x)
>     {
>     }
> }
> ```
>
> Output: RH2004 reports only the implementing part.
>
> Why it looks wrong: both parts lack an accessibility modifier, and for a partial property, indexer, or event both parts are reported. Either both parts of a partial constructor should be reported, or partial members should be exempt like partial methods (RH2004-002). Touches RH2004-001 (its coverage sentence leaves partial constructors out), RH2004-002.

### Issue `SDD open questions: sdd/specs/analyzer/RH2101.md`

Preamble with `sdd/specs/analyzer/RH2101.md` (RH2101).

> ## Checklist
>
> - [ ] Q1 — The part without `static` of a nested static partial class is reported
>
> ## Questions
>
> ### Q1 — The part without `static` of a nested static partial class is reported
>
> Input:
>
> ```csharp
> public class Outer
> {
>     static partial class Helpers
>     {
>     }
>
>     partial class Helpers
>     {
>     }
> }
> ```
>
> Output: the code compiles (the class is static), and RH2101 reports the second `Helpers`.
>
> Why it looks wrong: nested static classes are exempt because they hold no instance state (RH2101-002); this class is static, but the analyzer reads the `static` modifier of each declaration part on its own. Touches RH2101-002 (its coverage sentence leaves this case out), SHARED-NESTED-002.

### Issue `SDD open questions: sdd/specs/analyzer/RH2109.md`

Preamble with `sdd/specs/analyzer/RH2109.md` (RH2109).

> ## Checklist
>
> - [ ] Q1 — `@code {` inside a comment or a C# string is reported
> - [ ] Q2 — An escaped `@@code {` and email-like text `user@code{` are reported
> - [ ] Q3 — `@functions` blocks are not reported
>
> ## Questions
>
> ### Q1 — `@code {` inside a comment or a C# string is reported
>
> Input — `Counter.razor`:
>
> ```razor
> <!-- @code { -->
> @* @code { *@
> @code {
>     /*
>     @code {
>     */
>     private string s = "@code{";
> }
> ```
>
> Output: RH2109 reports every `@code` in the file — in the HTML comment, the Razor comment, the C# block comment, the C# string, and the real directive. A raw string literal containing `@code {` on its own line is reported too.
>
> Why it looks wrong: the analyzer matches the regular expression `@code\s*\{` against the raw file text, so comment text and literal text are checked as if they were directives. Precedent: P5 (#912) and SHARED-DIAG-003 make the text of a C# comment or literal content that a rule about code does not report; whether that extends to Razor and HTML comments in a `.razor` file is for the maintainer to decide. Touches RH2109-001 (its coverage sentence leaves these cases out).
>
> ### Q2 — An escaped `@@code {` and email-like text `user@code{` are reported
>
> Input — `Counter.razor`:
>
> ```razor
> <p>@@code { }</p>
> <p>user@code{</p>
> ```
>
> Output: RH2109 reports both lines.
>
> Why it looks wrong: in Razor, `@@` is an escaped `@` that renders the literal text `@code { }`, and an `@` preceded by a letter or digit is treated as email-like markup text; neither starts a code block. Touches RH2109-001 (its coverage sentence leaves these cases out).
>
> ### Q3 — `@functions` blocks are not reported
>
> Input — `Counter.razor`:
>
> ```razor
> @functions {
>     private int CurrentCount;
> }
> ```
>
> Output: RH2109 reports nothing.
>
> Why it looks wrong: in a Razor component, `@functions` is the older equivalent of `@code` and puts the same logic into the markup file; the rule's title speaks of "Razor code blocks". The rule page names only `@code`. Touches RH2109-001.

## Further instances of existing follow-up issues

For the lead to comment on; not new questions, not written into any spec.

- thoenissen/Reihitsu#971 (leading acronym run): RH2001's fix names the field `_hTTPClient` for `private int HTTPClient { get; set; }` (verified with `scripts/apply-fix.sh RH2001`; `URL` → `_url` and `first_name` → `_firstName` are correct). SHARED-CASING-006's coverage already leaves this input out, so RH2001-004 does not specify it.
- thoenissen/Reihitsu#967 (`DeclarationModifierUtilities` loses indentation): RH2004's fix on a member whose attribute list is on its own line, with no modifier on the member,

  ```csharp
  public class C
  {
      [Obsolete]
      void M()
      {
      }
  }
  ```

  produces `[Obsolete]` / `private     void M()` at column 0: the inserted modifier (through `AddAccessibilityModifiers(SyntaxTokenList)`) takes no leading trivia, and the token that started the line keeps its indentation behind it — the mechanism of #967 case 1 (the skeptic confirmed it). #996 says not to file the shared defect again. RH2004-006's coverage sentence leaves declarations with attribute lists out.

## New-rule candidates under P11

None. No finding is an analyzer/formatter disagreement. RH1002 Q1 and RH2109 Q3 are ordinary questions: P11 concerns what the formatter does beyond the analyzers.

## Decisions needed

None (no blocking question).
