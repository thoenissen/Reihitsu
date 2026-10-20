Role: spec author
Commit: 0925b47
Verdict: spec written

Repair of the remaining finding of `08-spec-checker.md`, as decided in `squad/decisions/2026-10-20-996-rh2001-002-second-spec-check.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decided task. No requirement ID was added, removed, or renumbered; no change doc; no test or rule page changed. This handover supersedes the open-question list of `07-spec-author.md`; the list below is the full, final set of issue texts.

# What I established and how

- **08 finding 1 holds.** `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer` reports a private property without attributes or explicit interface implementation when `IsAutoProperty()` is true, which asks whether the containing type has a field whose `AssociatedSymbol` is the property; the compiler creates one for every property that uses `field`, expression-bodied or not. `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` refuses a property with an `ExpressionBody` or an accessor with a body. Reproduced with a scratch test (added to the RH2001 test class, run, reverted; `git status` clean): `class C { private int {|RH2001:Value|} => field; private int {|RH2001:Other|} { get => field; } }` with `VerifyNoCodeFixIsOffered` passed — both reported, no code fix offered.
- **RH2109 Q1 raw-string input verified, kept.** Scratch test (added to the RH2109 test class, run, reverted) with the exact Q1 `Counter.razor` content passed with exactly four diagnostics at 1:1 (the directive), 3:5 (block comment), 5:25 (string literal), and 7:1 (raw string literal), as the Q1 text states. Command for both: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Scratch"` → "Passed!  - Failed:     0, Passed:     2".

# What changed

- RH2001-002: the expression-bodied clause now carries the same qualifier as the accessor clause — "a property without a backing field — an expression-bodied property whose expression does not use the `field` keyword, or one whose accessors have bodies that do not use it —". The example (`private int Computed => 1;`, accessor body `return 1;`) stays on the not-reported side.
- RH2001-001: the coverage sentence now also says "that is not expression-bodied", so the reported side of the qualifier leaves `=> field` out as explicitly as it leaves accessor bodies out. No tagged RH2001-001 test uses an expression-bodied property.
- RH2001 Q4: retitled and extended to name `private int Plain => field;` and `{ get => field; }`, with the three readings; "Touches" updated.
- RH2109 Q1: unchanged.

Checks: `scripts/test.sh --project architecture` — "Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72" (`SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`, `RuleDocumentationConventionTests` included). The rule page `documentation/rules/RH2001.md` needs no change: it says "auto-implemented getters and setters that provide no additional logic", which the open question Q4 decides.

## Changed requirements

- RH2001-001 — A private auto-implemented property is reported — clarified (coverage sentence leaves expression-bodied properties out)
- RH2001-002 — Other properties are not reported — clarified (expression-bodied clause qualified by the `field` keyword)

All other requirements are as in `07-spec-author.md`.

## Examples for the tests

No example changed. Characterization input the test implementer must add (untagged, as the decisions entry says), pinning RH2001 Q4:

```csharp
class C
{
    private int {|RH2001:Value|} => field;
}
```

reported, no code fix offered (`VerifyNoCodeFixIsOffered`). Q4 also names `private int Other { get => field; }`, which no test pins today; it behaves the same (verified above) and belongs in the same characterization test. The RH2109 Q1 raw-string input is not pinned by any test either; adding it to `VerifyCodeBlockTextInCommentsAndStringsIsReported` (or a sibling characterization test) would pin every input the Q1 text names — expected spans as listed above.

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
> - [ ] Q4 — A property that uses `field` in an expression body or in accessor bodies is reported, also with logic of its own
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
> Why it is wrong: the comments `/* name */` and `/* inner */` are removed. SHARED-FIX-003 (third condition) withholds a fix that changes the text of a comment the user wrote. Touches RH2001-004 (its coverage sentence leaves this case out), SHARED-FIX-003.
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
> Why it is wrong: SHARED-FIX-003 (fourth condition) withholds a fix that leaves code excluded by `#if` referring to a name the fix changed. Touches RH2001-004 (its coverage sentence leaves this case out), SHARED-FIX-003.
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
> Why it is wrong: before the fix, `Current.Increment()` mutates a copy returned by the getter and `Run()` returns 0; after it, the field itself is mutated and `Run()` returns 1. SHARED-FIX-003 (second condition) withholds a fix that changes what the program does at run time. A get-only property is affected in a constructor as well: there the `readonly` field the fix creates is a variable, so `Current.Increment()` in the constructor mutates the field instead of a copy. Touches RH2001-004 (its coverage sentence leaves properties whose type is a struct that is not `readonly` out), SHARED-FIX-003.
>
> ### Q4 — A property that uses `field` in an expression body or in accessor bodies is reported, also with logic of its own
>
> Input:
>
> ```csharp
> public class C
> {
>     private int Value { get => field; set => field = value * 2; }
>
>     private int Other { get => field; }
>
>     private int Plain => field;
> }
> ```
>
> Output: RH2001 reports `Value`, `Other`, and `Plain`; no code fix is offered for any of them.
>
> Why it looks wrong: the rule is about auto-implemented properties, whose accessors only read and write the backing field. The analyzer decides "auto-implemented" by whether the compiler created a backing field, which it does for every property that uses the C# 14 `field` keyword, in accessor bodies as well as in an expression body. So `Value` is reported although its setter has logic of its own that a field cannot replace. `Other` and the expression-bodied `Plain` add no logic, and their being reported looks consistent with the rule, but the same check reports them, so the decision covers them too. Three readings: a property is auto-implemented when the compiler creates its backing field (all three reported, today's behavior); only when every accessor is implemented by the compiler (none reported); or when its accessors and expression body add no logic to reading and writing `field` (`Other` and `Plain` reported, `Value` not). Touches RH2001-001 (its coverage sentence leaves expression-bodied properties and accessors with bodies out), RH2001-002 (which states only properties whose expression body or accessor bodies do not use `field`).
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
> Why it looks wrong: the created type is `System.NotImplementedException` in both cases. A syntactic pre-filter compares the written simple name with `NotImplementedException` before the semantic check, so an alias name never reaches it. Not covered by SHARED-DIAG-002: the alias is a different name, not a different spelling of the same name. Touches RH2003-001 (its coverage sentence leaves this case out).

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
> - [ ] Q1 — `@code {` inside a C# comment or a C# string literal is reported (Decided by principle P5 (#912))
> - [ ] Q2 — `@code {` inside a Razor or HTML comment is reported
> - [ ] Q3 — An escaped `@@code {` and email-like text `user@code{` are reported
> - [ ] Q4 — `@functions` blocks are not reported
>
> ## Questions
>
> ### Q1 — `@code {` inside a C# comment or a C# string literal is reported (Decided by principle P5 (#912))
>
> Input — `Counter.razor`:
>
> ```razor
> @code {
>     /*
>     @code {
>     */
>     private string s = "@code{";
>     private string t = """
> @code {
> """;
> }
> ```
>
> Output: RH2109 reports four `@code`: the directive on the first line, the one in the block comment, the one in the string literal, and the one in the raw string literal.
>
> Why it is wrong: the analyzer matches the regular expression `@code\s*\{` against the raw file text, so the text of a C# comment and of a C# literal inside a code block is checked as if it were a directive. SHARED-DIAG-003 (P5) says an analyzer rule MUST NOT report anything inside the text of a comment or a literal; only the directive on the first line should be reported. Touches RH2109-001 (its coverage sentence leaves these cases out), SHARED-DIAG-003.
>
> ### Q2 — `@code {` inside a Razor or HTML comment is reported
>
> Input — `Counter.razor`:
>
> ```razor
> <!-- @code { -->
> @* @code { *@
> ```
>
> Output: RH2109 reports both `@code`.
>
> Why it looks wrong: the analyzer matches the regular expression against the raw file text, so the text of a Razor `@* *@` or HTML `<!-- -->` comment is checked as if it were a directive. P5 (#912) and SHARED-DIAG-003 make the text of a comment content, but they name only C# comment kinds; whether that extends to Razor and HTML comments in a `.razor` file is for the maintainer to decide (Q1 is the C# part, which P5 decides). Touches RH2109-001 (its coverage sentence leaves these cases out).
>
> ### Q3 — An escaped `@@code {` and email-like text `user@code{` are reported
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
> ### Q4 — `@functions` blocks are not reported
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

None. No finding is an analyzer/formatter disagreement. RH1002 Q1 and RH2109 Q4 are ordinary questions: P11 concerns what the formatter does beyond the analyzers.

## Decisions needed

None (no blocking question).
