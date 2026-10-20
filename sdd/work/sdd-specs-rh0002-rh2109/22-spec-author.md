Role: spec author
Commit: ed95c81
Verdict: spec written

Repair round after `21-auditor.md`, as decided in `squad/decisions/2026-10-20-996-shared-equal-002-bounded-list.md`: bound SHARED-EQUAL-002 and RH1001-003 instead of keeping exhaustive lists. My prompt carried the decided worklist and no suspected cause or hoped-for verdict beyond it; I re-derived every statement from the code. No requirement ID was added, removed, or renumbered. No change doc: both changes are clarifications of current behavior. Both requirement example blocks are byte-for-byte unchanged, so the existing tagged tests still contain them verbatim. No test, rule page, or production file changed. This handover supersedes the open-question lists of `15-spec-author.md` and `19-spec-author.md`; the list below is the full, final set of issue texts of this run.

# What I established and how

How I checked:

- I read `EqualityComparerArgumentUtilities` and `RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.HasExplicitEqualityComparerInitializer` at `ed95c81`. No file under `src/Reihitsu.Analyzer` or `src/Reihitsu.Core` has changed since 2026-10-17.
- Scratch harness: `net10.0` / `LangVersion preview` projects in the scratchpad, outside the repository and deleted afterwards, with the head's Release `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` as `<Analyzer>` items. I read the RH1001 and RH1002 lines from `dotnet build`. About 60 argument and initial-value forms, one per line, covering every item of every list in both requirements on both sides.
- Real verifier: a temporary test file `src/Reihitsu.Analyzer.Test/Performance/ScratchBoundedListTests.cs`, deleted afterwards (`git status` shows only my two spec files). It ran every new input of this handover (the two tagged-test inputs and the Q6 characterization input below, exactly as given), plus two `switch`-without-arms inputs that were later dropped from the handover through `Verify(source)` with `{|RH100x:…|}` markup. Command: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~ScratchBoundedList"`. Result: "Passed! - Failed: 0, Passed: 7". A mutation that removed one markup failed (1 of 7), so the markup is checked.
- `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72".

What I found, by auditor finding:

1. **Finding 1 holds (a constant with the value `null`).** `IsNullLikeExpression` asks `GetConstantValue` first, and `IsNullLikeOperation` checks `ConstantValue` at every level, so every compile-time constant is decided by its value. Reported, so not custom: `items.Distinct(NoComparer)` with `const IEqualityComparer<S> NoComparer = null;` as a member, the same through a local `const`, `Other.Nothing` (a `const` of another class), `(NoComparer)`, `NoComparer!`, `flag ? null : NoComparer`, `NoComparer ?? EqualityComparer<S>.Default`, `mode switch { 0 => NoComparer, _ => none }`, `NoComparer as IEqualityComparer<S>`, `checked((IEqualityComparer<S>)null)`, and `NullHolder?.Comparer` with `const Holder NullHolder = null;`. Not reported, so custom: a non-`const` local holding `null`, a `static readonly` field holding `null`, `NullHolder.Comparer` (a property read on the constant), `NoComparer ?? comparer`, and `flag ? comparer : NoComparer`. RH1001: `new Dictionary<S, int>(NoComparer)`, `Dictionary<S, int> a = new(NoComparer);`, `new(none)` with a `const` local, and `new HashSet<S>(comparer: NoComparer)` are reported; `new(notConst)` with a non-`const` local is not.
2. **Finding 2 holds (`default` of a type parameter).** `IsNullLikeDefaultType` treats every type with `IsValueType == false` as null-like. `default(TC)` with `where TC : IEqualityComparer<S>` is reported, and so are `default(TN)` with `notnull` and `default(TR)` with `class`; `default(TV)` with `struct` is not. RH1001 reports `Dictionary<S, int> map = new(default(TC));` and `new Dictionary<S, int>(default(TC))`, not the `TV` creation. Filed as struct-equality Q6. `TR` looks right (always `null`), but I left every type parameter out of the spec, so the question's answer decides them together and no reading is pre-empted.
3. **Finding 3 holds (`switch` without arms).** Both predicates require at least one arm. `items.Distinct(mode switch { })` is not reported (custom), `new(comparer: mode switch { })` is not reported, and `Dictionary<S, int> a = _mode switch { };` and `(Dictionary<S, int>)(_mode switch { })` are reported.

# The change

`sdd/specs/shared/struct-equality.md`, SHARED-EQUAL-002, text only:

- The "exactly when" claim and "Every other argument MUST count as custom" are gone. The requirement now has two bounded lists: the forms that MUST count as not custom, and the forms that MUST count as custom.
- Not-custom list: the first item reads "a constant with the value `null` — the literal `null`, a cast of it, or a `const` field or local initialized with `null` — or `default` of a reference type or of a nullable value type". The `switch` item requires at least one arm. The null-conditional item says "whose receiver has a form of the first item", so the existing `((Holder)null)?.Comparer`, a `const` receiver, and `default(Holder)?.Comparer` are all covered. That last one was already required under the old reading, as `21-auditor.md` notes.
- Custom list: everything the old "every other" sentence and the example rely on, stated as forms:
  - a variable, a field that is not a constant, a property other than `EqualityComparer<T>.Default`, a method call, or an object creation, also a non-`const` local initialized with `null`;
  - `default` of a non-nullable value type that implements the comparer interface;
  - a user-defined cast, whatever its operand;
  - wrappers (parentheses, `!`, a non-user-defined cast, `as`) around one of these forms;
  - a `??` whose left operand is custom;
  - a conditional with a custom branch when its condition is not a constant, or a custom chosen branch when it is;
  - a `switch` with a custom arm;
  - a null-conditional access such as `holder?.Comparer` whose receiver is a variable, a non-constant field, a property, or a method call.
- Coverage paragraph:
  - It now opens with the bounding sentence: the requirement specifies only the forms the two lists name and leaves every other form unspecified. It names three such forms: a `throw` branch or arm whose other branches or arms are all not custom (struct-equality Q4), a `switch` without arms, and `default` of a type parameter, whatever its constraints (Q6).
  - The Q3 sentence is unchanged.
  - The Q5 sentence now says "a form of the first list inside parentheses or behind `!`", because with two lists "a form" could otherwise be read to exclude `(comparer)`.
- Why: one sentence added: "A constant's value is known where it is used, so a constant holding `null` is the same as the literal."
- Lines checked against the code that the lists deliberately do not decide: `null ?? comparer` and `NoComparer ?? comparer` (custom in the code), and `(flag ? null : (Holder)null)?.Comparer` (not custom in the code). They are unlisted, so they are unspecified. None of them looks wrong.

`sdd/specs/analyzer/RH1001.md`, RH1001-003, text only:

- The `switch` item of the exempt list requires at least one arm.
- "Every other initial value MUST leave … reported, among them …" became a second bounded list of initial values that MUST leave the declared or cast type reported:
  - a creation without a custom comparer, a variable, `??`, `default`, a collection expression without a custom `with(…)`, or a user-defined cast;
  - wrappers around one of these;
  - a conditional with such a branch when its condition is not a constant, or with such a chosen branch when it is;
  - a `switch` with such an arm.
  - For several variables, it applies when any one initial value has such a form.
- The coverage sentence now bounds the requirement in the same way. It names a `throw` branch or arm whose other branches or arms are all exempt (RH1001 Q1), a `switch` without arms, and a collection expression with a custom `with(…)` (RH1001 Q2).
- A constant `null` reaches RH1001 only through SHARED-EQUAL-002: "a creation that passes no custom comparer" covers `new(NoComparer)`.
- Also verified on the reported side: `(_other!)`, `(Dictionary<S, int>)(_other)` (declared and cast type), `true ? new() : new(_comparer)`, a `switch` with a variable arm, and `(_mode == 0 ? _other : new(_comparer))!`.

## Changed requirements

- SHARED-EQUAL-002 — A custom comparer argument replaces the equality members — clarified. The exhaustive claim is replaced by two bounded lists and a bounding coverage sentence. A constant with the value `null` is not custom. A `switch` without arms and `default` of a type parameter are left unspecified.
- RH1001-003 — A creation with a custom comparer is not reported — clarified. The exhaustive "every other" claim is replaced by a second bounded list and a bounding coverage sentence. A `switch` without arms is left unspecified.

No rule page changed. Both pages say that passing `null` or `EqualityComparer<T>.Default` is not a custom comparer, which a constant holding `null` agrees with.

## Examples for the tests

The example blocks of SHARED-EQUAL-002 and RH1001-003 are unchanged. The rulebook allows one block per requirement, and the lead asked to keep the blocks, so the `const` side is pinned by separate tagged tests whose inputs are given here. The examples' existing `IEqualityComparer<S> none = null;` line stays the non-`const` side. All inputs below were run through the real verifier exactly as given and pass.

Tagged tests:

1. RH1002 test class, tag `SHARED-EQUAL-002`. A constant with the value `null` is not a custom comparer; a non-`const` local holding `null` is. 3 diagnostics:

   ```csharp
   using System.Collections.Generic;
   using System.Linq;

   struct S
   {
   }

   class C
   {
       const IEqualityComparer<S> NoComparer = null;

       void M(IEnumerable<S> items, bool flag)
       {
           const IEqualityComparer<S> none = null;
           IEqualityComparer<S> notConst = null;

           items.{|RH1002:Distinct|}(NoComparer);
           items.{|RH1002:Distinct|}(none);
           items.{|RH1002:Distinct|}(flag ? null : NoComparer);
           items.Distinct(notConst);
       }
   }
   ```

2. RH1001 test class, tags `RH1001-003` and `SHARED-EQUAL-002`. A creation passing a constant `null` is not exempt; one passing a non-`const` local is. 2 diagnostics:

   ```csharp
   using System.Collections.Generic;

   struct S
   {
   }

   class C
   {
       const IEqualityComparer<S> NoComparer = null;

       void M()
       {
           const IEqualityComparer<S> none = null;
           IEqualityComparer<S> notConst = null;

           var a = new Dictionary<{|RH1001:S|}, int>(NoComparer);
           Dictionary<{|RH1001:S|}, int> b = new(none);
           Dictionary<S, int> c = new(notConst);
       }
   }
   ```

Untagged characterization tests, each with the exact input of its issue text below and doc comments that say "Characterizes …":

3. Struct-equality Q6: the same input in both classes. RH1002: `{|RH1002:Distinct|}` on the `default(TC)`, `default(TN)`, and `default(TR)` calls, 3 diagnostics. RH1001: `Dictionary<{|RH1001:S|}, int> map`, 1 diagnostic.

The `switch` expression without arms gets no question and no test. As the lead decided after my first report, the maintainer's decision leaves it unspecified through the coverage sentences of SHARED-EQUAL-002 and RH1001-003 only.

## Not addressed

- `21-auditor.md` hint 2: the summary of the tagged test `RH1002 · VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` speaks of looking through wrappers in general. That is a test comment, and I do not touch tests. The spec text it relates to is now explicit (Q5 sentence).
- `14-auditor.md` hints 3 and 4 (RH2004's fix on `abstract void M();`; RH2001's fix and reflection) stay with the lead, as in `15-spec-author.md`.

## Open questions to file

None blocks this run. One issue per spec path, each a sub-issue of thoenissen/Reihitsu#912, titled exactly as the heading says. None exists yet. Every body starts with the preamble below, with `<spec path>` and `<prefix/rules>` filled in as given in each heading's first line. Compared with `15-spec-author.md`, only the struct-equality issue changed: Q5 from `19-spec-author.md` and the new Q6 are added to its checklist and questions. Every other entry, Q4 and all of RH1001 included, is copied unchanged.

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
> - [ ] Q4 — An argument that can only produce `null` or throw counts as a custom comparer
> - [ ] Q5 — An argument that can only produce `null` or the default comparer counts as a custom comparer when parentheses or `!` wrap the whole argument
> - [ ] Q6 — `default` of a type parameter that can be a struct comparer counts as no comparer
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
>
> ### Q4 — An argument that can only produce `null` or throw counts as a custom comparer
>
> Input:
>
> ```csharp
> using System;
> using System.Collections.Generic;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items, bool flag, int mode)
>     {
>         items.Distinct(flag ? null : throw new InvalidOperationException());
>         items.Distinct(mode switch { 0 => null, _ => throw new InvalidOperationException() });
>         items.Distinct(flag ? null : EqualityComparer<S>.Default);
>     }
> }
> ```
>
> Output: RH1002 reports only the third call. Likewise, RH1001 does not report `Dictionary<S, int> map = new(comparer: flag ? null : throw new InvalidOperationException());`, but it reports `Dictionary<S, int> other = new(comparer: flag ? null : EqualityComparer<S>.Default);` on its `S`.
>
> Why it looks wrong: in the first two calls, the only value the argument can produce is `null`. A `throw` produces no comparer. `null` falls back to `EqualityComparer<S>.Default`, exactly like the third call, which is reported. The rule treats a `throw` branch or arm as an unknown value and so counts the whole argument as a custom comparer. Two readings: a `throw` branch or arm produces no value and is ignored, so the first two calls are reported like the third, and so is RH1001's `map`; or a `throw` makes the argument custom, which is today's behavior. Touches SHARED-EQUAL-002 (its coverage sentence leaves this case out), RH1002-001, RH1001-003.
>
> ### Q5 — An argument that can only produce `null` or the default comparer counts as a custom comparer when parentheses or `!` wrap the whole argument
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items, bool flag)
>     {
>         items.Distinct(flag ? null : EqualityComparer<S>.Default);
>         items.Distinct((flag ? null : EqualityComparer<S>.Default));
>         items.Distinct((flag ? null : EqualityComparer<S>.Default)!);
>         items.Distinct(null as IEqualityComparer<S>);
>         items.Distinct((null as IEqualityComparer<S>));
>     }
> }
> ```
>
> Output: RH1002 reports the first and the fourth call only. Likewise, RH1001 reports `Dictionary<S, int> a = new(null as IEqualityComparer<S>);` on its `S`, but does not report `Dictionary<S, int> b = new((null as IEqualityComparer<S>));`.
>
> Why it looks wrong: parentheses and the null-forgiving operator `!` do not change the value. The wrapped calls pass exactly the comparer of the unwrapped ones, which falls back to `EqualityComparer<S>.Default`. The rule looks through these wrappers when the inner form is a constant `null`, such as `((IEqualityComparer<S>)null)!`, or always produces `EqualityComparer<S>.Default`, such as `(EqualityComparer<S>.Default ?? comparer)`. It does not look through them for the other forms it accepts: a conditional, a `switch` expression, a `??` expression, or a null-conditional access that can produce `null`; `default` of a nullable value type; and an `as` expression. It counts those as custom only when parentheses or `!` wrap the whole argument. Two readings:
> - Parentheses and `!` are always transparent, so the wrapped calls are reported like the unwrapped ones, and so is RH1001's `b`.
> - Today's behavior is intended.
>
> Touches SHARED-EQUAL-002 (its coverage sentence leaves this case out), RH1002-001, RH1001-003.
>
> ### Q6 — `default` of a type parameter that can be a struct comparer counts as no comparer
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M<TC, TN, TR, TV>(IEnumerable<S> items)
>         where TC : IEqualityComparer<S>
>         where TN : notnull, IEqualityComparer<S>
>         where TR : class, IEqualityComparer<S>
>         where TV : struct, IEqualityComparer<S>
>     {
>         items.Distinct(default(TC));
>         items.Distinct(default(TN));
>         items.Distinct(default(TR));
>         items.Distinct(default(TV));
>     }
>
>     void N<TC, TV>()
>         where TC : IEqualityComparer<S>
>         where TV : struct, IEqualityComparer<S>
>     {
>         Dictionary<S, int> map = new(default(TC));
>         Dictionary<S, int> other = new(default(TV));
>     }
> }
> ```
>
> Output: RH1002 reports the calls with `default(TC)`, `default(TN)`, and `default(TR)`, but not the call with `default(TV)`. RH1001 reports the `S` of the declared type of `map`, but not of `other`.
>
> Why it looks wrong: `TC` and `TN` are not constrained to a reference type, so they can be instantiated with a struct comparer. Then `default(TC)` is a real comparer, boxed when it is passed, and the operation compares through it, exactly as with `default(TV)`, which the rule counts as custom. The rule treats `default` of every type parameter that is not constrained to a value type as `null`. Elsewhere it decides by form and counts a value it cannot know at the call site, such as a variable, as custom. `TR` is constrained with `class`, so `default(TR)` is always `null`, and reporting it looks right under either reading. Two readings:
> - `default` of a type parameter that is not constrained to a reference type counts as custom, because it can be a comparer: the `TC` and `TN` calls are not reported, and neither is RH1001's `map`; the `TR` call stays reported.
> - `default` of a type parameter that is not constrained to a value type counts as no comparer, because it can be `null`: today's behavior.
>
> Touches SHARED-EQUAL-002 (its coverage sentence leaves `default` of a type parameter out), RH1002-001, RH1001-003.

### Issue `SDD open questions: sdd/specs/analyzer/RH1001.md`

Preamble with `sdd/specs/analyzer/RH1001.md` (RH1001).

> ## Checklist
>
> - [ ] Q1 — A declaration whose initial value picks between creations with a custom comparer and a `throw` is reported
> - [ ] Q2 — A collection expression that passes a custom comparer through `with(…)` is reported
> - [ ] Q3 — A collection expression that creates the collection is not reported where a target-typed `new()` is
>
> ## Questions
>
> ### Q1 — A declaration whose initial value picks between creations with a custom comparer and a `throw` is reported
>
> Input:
>
> ```csharp
> using System;
> using System.Collections.Generic;
>
> struct S
> {
> }
>
> class C
> {
>     IEqualityComparer<S> _comparer;
>     bool _flag;
>     int _mode;
>
>     void M()
>     {
>         Dictionary<S, int> a = _mode switch { 0 => new(_comparer), _ => throw new InvalidOperationException() };
>         Dictionary<S, int> b = _flag ? new(_comparer) : throw new InvalidOperationException();
>         Dictionary<S, int> c = _flag ? new(_comparer) : new(_comparer);
>     }
> }
> ```
>
> Output: RH1001 reports the `S` of the declared type of `a` and of `b`, but not of `c`.
>
> Why it looks wrong: every collection that `a` and `b` can hold is created with the custom comparer, and a `throw` produces no collection. The declaration is just as free of the key's equality members as `c`. The rule page says that for a conditional or `switch` expression "the exemption applies only if every one of those creations supplies a comparer", and here every creation does. The analyzer recognizes only creations in the branches and arms, so it treats a `throw` as a value without a comparer. Two readings: a `throw` branch or arm is ignored, so `a` and `b` are not reported; or a `throw` leaves the declared type reported, which is today's behavior. Touches RH1001-003 (its coverage sentence leaves this case out).
>
> ### Q2 — A collection expression that passes a custom comparer through `with(…)` is reported
>
> Input (collection expression arguments, `LangVersion` `preview`):
>
> ```csharp
> using System.Collections.Generic;
>
> struct S
> {
> }
>
> class C
> {
>     IEqualityComparer<S> _comparer;
>     S _item;
>
>     void M()
>     {
>         HashSet<S> set = [with(_comparer), _item];
>         HashSet<S> other = new(_comparer) { _item };
>     }
> }
> ```
>
> Output: RH1001 reports the `S` of the declared type of `set`, but not of `other`.
>
> Why it looks wrong: `with(_comparer)` passes the comparer to the `HashSet<S>` constructor, so the set hashes through it exactly as `other` does. The analyzer recognizes a comparer only in a creation written with `new`. Two readings: a collection expression whose `with(…)` passes a custom comparer is exempt like the matching `new(…)`, so `set` is not reported; or only `new` creations are exempt, which is today's behavior. Touches RH1001-003 (its coverage sentence leaves this case out).
>
> ### Q3 — A collection expression that creates the collection is not reported where a target-typed `new()` is
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
>
> struct S
> {
> }
>
> class C
> {
>     S _item;
>
>     void Take(HashSet<S> set)
>     {
>     }
>
>     void M()
>     {
>         Take([_item]);
>         Take(new());
>     }
> }
> ```
>
> Output: RH1001 reports the `S` of the parameter type and the `new` of `Take(new())`, but not `[_item]`.
>
> Why it looks wrong: both arguments create a `HashSet<S>` without a comparer, and neither names the type. RH1001-002 reports a target-typed `new()` that is not the initial value of a declaration or the operand of a cast of the same type, because that creation is a place where the collection hashes the struct. A collection expression is the same creation in another syntax. In a declaration (`HashSet<S> x = [_item];`) both forms are covered by the reported declared type. Three readings: report a collection expression like a target-typed `new()`, which then also needs a location under RH1001-005; report neither, because the parameter type is already reported; or keep today's behavior. Touches RH1001-002 (its coverage sentence leaves collection expressions out), RH1001-005.

### Issue `SDD open questions: sdd/specs/analyzer/RH1002.md`

Preamble with `sdd/specs/analyzer/RH1002.md` (RH1002).

> ## Checklist
>
> - [ ] Q1 — Equality-based methods and factories outside the rule's fixed list are not checked
> - [ ] Q2 — The `group … by`, `join`, and `join … into` clauses of a query expression are not checked
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
>
> ### Q2 — The `group … by`, `join`, and `join … into` clauses of a query expression are not checked
>
> Input:
>
> ```csharp
> using System.Collections.Generic;
> using System.Linq;
>
> struct S
> {
> }
>
> class C
> {
>     void M(IEnumerable<S> items)
>     {
>         var groups = from x in items group x by x;
>         var joined = from a in items join b in items on a equals b select a;
>         var grouped = from a in items join b in items on a equals b into g select g;
>         var method = items.GroupBy(x => x);
>     }
> }
> ```
>
> Output: RH1002 reports only `GroupBy` in the last line.
>
> Why it looks wrong: the compiler turns these clauses into calls of `Enumerable.GroupBy`, `Enumerable.Join`, and `Enumerable.GroupJoin`. Those calls hash `S` through `EqualityComparer<S>.Default`, exactly like the method call the rule reports. The analyzer looks only at invocation expressions. If the clauses should be reported, where the diagnostic goes is also open, for example on the `group` or `join` keyword, because RH1002-005 names only a method name or a null-conditional call. Touches RH1002-001 (its coverage sentence leaves query clauses out), RH1002-005.

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

None. No finding is an analyzer/formatter disagreement. RH1002 Q1 and Q2, RH1001 Q1–Q3, struct-equality Q4–Q6, and RH2109 Q4 are ordinary questions. P11 concerns what the formatter does beyond the analyzers.

## Decisions needed

None (no blocking question).
