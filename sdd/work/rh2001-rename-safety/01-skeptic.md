Role: skeptic
Commit: 52ae88c13ebfd2ecc5c8599e9d3d729abd4b0ebd
Verdict: output question

The prompt carried no intended fix or hoped-for verdict. The issue's *Affected* line suggests reusing the casing fixes' rename-safety check, and its "Done when" says to withhold the fix. Both are proposed solutions, so I tested them as claims and did not act on them.

## Premise

**The issue's own candidate input does not show a defect.** `scripts/apply-fix.sh RH2001` on the issue's example (`Base` with `protected int _value`, and `Get(int _value) { return Value + _value; }`) gives `return this._value + _value;` under both LF and CRLF. Qualifying a reference to the renamed symbol with `this.` is an allowed rename (SHARED-CASING-009, third paragraph). Every reference keeps its binding.

**The defect class is still real, and a caller reaches it.** The caller is the RH2001 code fix, offered by `RegisterCodeFixesCoreAsync` on the reported property and applied singly or through `WellKnownFixAllProviders.BatchFixer`. The fix calls `Renamer.RenameSymbolAsync` (`RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:100`) and ships whatever the renamer returns. Its only binding guard is `HasFieldNameCollision` (`:162`), which checks `ContainingType.GetMembers(fieldName)`: members the type declares itself, not inherited ones and not names in an inner scope.

Method of evidence:

- 32 fixtures run through `scripts/apply-fix.sh RH2001`, LF and CRLF. All 64 arms reported "fixed", with the same diff under both line endings.
- To show a run-time change, I compiled each before/after pair with the .NET 11 SDK and ran it. Each value below is the result of the same call on the code before and after the fix.

Run-time changes. SHARED-FIX-003, condition 2, forbids each of these:

| # | Shape (other symbol named `_value`) | Fix output (the changed line) | Run: before → after |
|---|---|---|---|
| f05 | inherited `protected int _value`, read as `other._value` with `other` typed `Sample` | `return _value + other._value;` (`other._value` now binds to the new private field) | 11 → 2 |
| t4 | inherited `protected int _value`, read as `this._value` | `return _value + this._value;` | same mechanism as f05: the text is unchanged and the reference now binds to `Sample._value` |
| e2 | inherited `protected int _value { get; set; }` (property), read as `this._value` | `return _value + this._value;` | same as t4 |
| f08 | inherited `protected internal int _value`, read as `sample._value` in a nested type | `return sample._value;` (unchanged text, rebinds) | 10 → 1 |
| f09 | simple lambda `_value => Value + _value` | `_value => _value + _value` | 101 → 200 |
| s01 | parenthesized untyped lambda `(_value) => Value + _value` | `(_value) => _value + _value` | 101 → 200 |
| s12 | static property, simple lambda in a static method | `_value => _value + _value` | 101 → 200 |
| s06 | `int.TryParse(input, out var _value) ? Value + _value : 0` | `... ? _value + _value : 0` | 101 → 200 |
| t1 | `out int _value` in the same conditional expression | `... ? _value + _value : 0` | same as s06 |
| s13 | class primary-constructor parameter `_value` | `return _value + _value;` | 101 → 2 |

Rewrites of a reference to another symbol. These compile, and running them shows no run-time change:

| # | Shape | Fix output | Run |
|---|---|---|---|
| f02 | inherited `protected int _value`, read unqualified | `return _value + base._value;` | 11 → 11 |
| f07 | extension method `_value(this Sample)`, called as `this._value()` | `return Extensions._value(this) + _value();` | 11 → 11 |
| s16 | `using static Holder;` with a public static field `_value` | `return _value + Holder._value;` | compiles |
| e1 | static `_value` of the enclosing type, used from a nested type | `return _value + Outer._value;` | not compiled |

Correct today: the renamed reference is qualified with `this.`, and every other reference keeps its text and its binding:

- f01: the issue's input;
- f03: a local declared by a statement;
- f04: a method parameter;
- s02: an explicitly typed lambda parameter;
- s03: an anonymous-method parameter;
- s04: a local-function parameter;
- s05 and t2: pattern variables (`is int _value`, `is var _value`);
- t3: an `out var` declared in an expression statement before the use;
- s07: a `foreach` variable;
- s08: a query range variable;
- s09: a catch variable;
- s10: a local function named `_value` (`this._value + _value()`);
- s11: an outer local captured in a lambda;
- s14: a member named `_value` in a nested type;
- s15: a deconstruction variable.

Two inputs are correct but add a warning:

- f06: an inherited method `_value()`. The output is `_value + _value()`; the invocation still binds to the method, and the run gives 11 → 11.
- f10: an inherited `_value` with no reference to it. The fix is offered.

Both results compile with the new warning CS0108 ("hides inherited member"). The same warning appears in f02, f05, and t4. SHARED-FIX-003 counts only compile errors. The casing fix RH4106 also offers f10 (below), so the existing policy already accepts this warning. I raise no question for it.

## Mechanism and candidates

**Mechanism.** The fix applies the renamer's result without checking that every reference keeps its binding. Wherever a symbol other than the property already answers to the derived `_camelCase` name at some reference, the renamer does one of three things, and the fix ships the result in every case:

1. It qualifies the property's reference (allowed).
2. It rewrites a reference to the other symbol (`base.`, a type qualifier, or a static extension call).
3. It leaves a reference unqualified or unchanged, so that the reference binds to a different symbol after the rename.

A member is decidable on the result: after the rename, a reference either binds to a different symbol than before (a run-time change), or a reference to a symbol other than the property has changed text.

**Where the candidates come from.** The candidate set is the set of symbol kinds that C# name lookup can return for the name `_value` at a reference inside the property's containing type. That is the lookup chain from an inner scope outward:

- `ILocalSymbol`: declared by a statement, by `out var` or `out T`, by a pattern designation, by deconstruction, by `foreach`, by `catch`, or by `using`;
- `IParameterSymbol`: method, local function, explicitly typed lambda, implicitly typed simple or parenthesized lambda, static lambda, anonymous method, primary constructor;
- `IRangeVariableSymbol`;
- a local function (`IMethodSymbol`, `MethodKind.LocalFunction`);
- a member declared by the type itself, which `HasFieldNameCollision` already covers;
- inherited members: field, property, method;
- a member of an enclosing type;
- a statically imported member;
- an extension method.

The second axis is how the other symbol is reached: unqualified, through `this.`, through a receiver of the type or of a type derived from it, or invoked. All members are listed in the two tables above.

## Breaking inputs

- **FC-006, rewrite changes meaning.** The run-time table above: f05, t4, e2, f08, f09, s01, s12, s06, t1, s13.
- **FC-027, another enclosing scope behaves differently.** Lambda parameters fail only when their type is inferred (f09, s01, s12); the explicitly typed form s02 holds. `out var` and `out int` fail when the use is in the same expression (s06, t1) and hold when the use is in a later statement (t3). The reproduction has to be the failing scope itself, not a sibling that holds (FC-034).
- **FC-015, a policy with more than one owner.** See *Architecture findings*.
- **FC-016, sibling shapes.** Every member of both tables needs a test. A fix for the issue's "inherited member" must not cover only the unqualified read (f02) and miss `this._value` (t4) and `other._value` (f05).
- **FC-024, partial types.** The property can be referenced from another `partial` part in another document. The same mechanism applies there. The test implementer should include one cross-document case (for example f09's lambda in the other part). I did not run this case.
- **FC-008, Fix All.** `BatchFixer` computes every diagnostic's fix against the original solution. I did not settle the case of two reported properties that derive the same field name, for example `Value` and `value`; `apply-fix` has no Fix All mode. That case is listed under *Follow-up candidates*.
- **FC-033.** Every case above gives the same diff under LF and CRLF, so line endings do not hide any of them.

Which cases are rare. Every run-time-changing member except f05 and t4 needs a name that another Reihitsu rule already reports. `apply-fix` reported each diagnostic below on the fixture named:

- RH4105 reports the lambda and primary-constructor parameters `_value` (f09, s01, s12, s13);
- RH4115 reports the `out var` local (s06);
- RH4108 reports the `protected internal` field (f08);
- e2's protected property `_value` breaks RH4112's PascalCase requirement (taken from the rule's title; I did not run RH4112).

f05 and t4 do not break another rule: RH4107 requires a protected field in `_camelCase`, so the base class follows the rules. These two inputs are uncommon (a derived type with both a private auto-property `Value` and an access to an inherited `_value`), but they are not rare by that definition.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH2001 analyzer (reporting) | must stay; every case is still reported |
| RH2001 code fix (`RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`) | must change: it is withheld for the run-time-changing class (demanded by SHARED-FIX-003, which is unchanged); the rewrite class depends on output question 1 |
| Fix All (`BatchFixer`) | must stay; it inherits whatever the single fix decides |
| `CasingCodeFixProviderBase<T>` / SHARED-CASING-009 | not involved unless the lead decides the architecture finding by reuse; RH4106 withholds the six shapes run against it (*Architecture findings*) |
| `Reihitsu.Core`, formatter | not involved |
| Rule page `documentation/rules/RH2001.md` | must stay; `general-notes.md` already covers a withheld fix |
| Spec `sdd/specs/analyzer/RH2001.md` | the withheld run-time class is already demanded by SHARED-FIX-003. Recording the gap under RH2001-004's coverage is optional and is the spec author's call (bug-fix lane, clarification only). If output question 1 is answered B, the spec needs a statement (RH2001 starts using SHARED-CASING-009, or a requirement of its own), which needs a change doc and widens the lane to behavior change |
| Tests (`RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests`) | must change: reproductions for the run-time table, characterization tests for the inputs that hold |

## Refuse option

**Withhold the fix** whenever a symbol other than the property already answers to the derived field name, at a reference to the property or at a reference that binds to that other symbol. Run-time-changing results are already forbidden, so withholding them is the minimum. It is a withheld fix for a class that is rare apart from f05 and t4.

Cost compared with the alternatives:

- **Check the declared field name against what is visible to the type and against identifiers in its declarations.** This withholds the run-time class and the rewrite class together. It also withholds the safe `this.`-qualified cases (f01, f03, f04, ...). Each of those uses a local or parameter named `_value`, which RH4105 or RH4115 reports, so they are rare. This is a cheap syntactic and symbol check, of the same kind as the existing `HasExcludedCodeOccurrenceAsync`, which already searches excluded code for these names.
- **Reuse the casing fixes' complete check** (`RenameWithoutConflictAsync`, private in the 1485-line `CasingCodeFixProviderBase<T>`). Its result on these inputs equals option B of question 1: f02, f05, f07, f09, s06, and t4 are withheld, and f10 is offered. Reusing it means extracting it out of the generic base. That is a structural change, and possibly a new type, which would be escalation 9 if it exceeds about 100 lines.
- **Repair the renamer's misses**, for example by adding the missing `this.` or `base.`. This is machinery for a rare class and is excluded by *Special cases do not grow the source*.

**Declaring a shape unsupported** is not an option. The class is decided by semantic binding, not by a formatter-input shape the precheck could recognize, and no formatter phase is involved.

## Output questions

**1 · RH2001 · Private auto-implemented properties should not be used — the derived field name is already used by an inherited member**

Input:
```csharp
class Base
{
    protected int _value;
}

class Sample : Base
{
    private int Value { get; set; }

    int Get()
    {
        return Value + _value;
    }
}
```

Before (`main`):
```csharp
class Sample : Base
{
    private int _value;

    int Get()
    {
        return _value + base._value;
    }
}
```

After, A: unchanged from `main`. The fix is offered and rewrites the read of the inherited field to `base._value`.

After, B (recommended): no fix offered; the diagnostic stays.

Why B: the fix would otherwise edit a reference the diagnostic does not name. The casing fixes, such as RH4106 on the same input with a field `Value`, already withhold it (SHARED-CASING-009). Using the same rule for RH2001 keeps one rename policy across the analyzers.

The same question covers the rare variants: an extension method named `_value` (rewritten to `Extensions._value(this)`), a static-imported member (`Holder._value`), and a member of an enclosing type (`Outer._value`). Withholding those needs no question because each already breaks another rule. They are listed here so that one answer decides the whole rewrite class.

## Internal choices

- **What the reproduction covers.** I recommend one reproduction per row of the run-time table, at least f05, t4, f09, s01, s06, and s13, plus characterization tests for f01, f03, f04, s02, s05, t3, s10, and f06/f10. These change no visible output beyond what SHARED-FIX-003 already demands.
- **The CS0108 warning in f10.** Keep the fix offered. SHARED-FIX-003 counts only compile errors, and RH4106 offers the same result. Nothing needs to change.

## Issue goals

- **"A test reproduces or refutes the behavior for a local or parameter, an inherited member, and an extension-style call":** change it. For a local or parameter, the axis is too coarse. Statement locals and method, local-function, and explicitly typed lambda parameters are refuted (they are qualified correctly). Implicitly typed lambda parameters, `out` variables used in the same expression, and primary-constructor parameters reproduce. The inherited-member axis reproduces, and its worst members are `this._value` and `other._value`, not the issue's example. The extension call is a rewrite, not a run-time change. No visible output changes from this goal itself.
- **The issue's candidate input:** drop it as the reproduction. It shows no defect, and the result `this._value + _value` is correct. Keep it as a characterization test.
- **"If it reproduces, the fix is withheld for these inputs":** keep it for the run-time class. For the rewrite class it depends on output question 1.
- **"Possibly the shared rename-safety check of the casing fixes can be reused":** this is a proposed solution. The lead decides it under *Architecture findings*.
- **"Today's output: Unknown":** now known (the tables above). The issue's question of whether `Value` becomes `this._value` is answered: it does, and that is allowed.

## Architecture findings

**FC-015: the decision "may this rename be applied" has two owners.**

- `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` (private) applies SHARED-CASING-009 in full: conflict annotations, rewritten references, a reference-count check for references left as written, compile errors, and more.
- `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` keeps its own weaker copy: `HasFieldNameCollision`, `HasExcludedCodeOccurrenceAsync`, and `HasHidingCarrierInReferringDocumentAsync`.

Inputs that show them disagreeing: I ran f02, f05, f07, f09, s06, and t4 with the property replaced by a private field `Value` through `scripts/apply-fix.sh RH4106`. All six are `no-fix-offered`, and f10 is fixed. RH2001 offers all seven. The defect is one of the run's issues, so this is an architecture finding of the run. Refusing the class locally needs no structural change.

## Follow-up candidates

- **RH2001 Fix All with two properties that derive the same field name** (`Value` and `value`, or `FooBar` and `Foo_Bar`). `BatchFixer` would apply both renames computed against the original solution, which could produce two `_value` members. A single fix withholds the second through `HasFieldNameCollision`. I did not settle it: `apply-fix` has no Fix All mode. The case is rare, because the second name breaks RH4111. The candidate is to check it with a Fix All test and, if it fails, to withhold the group as SHARED-CASING-010 does for the casing fixes.
- **The renamer misses** (implicitly typed lambda parameters, `out` variables in the same expression, and primary-constructor parameters are not qualified, and `this.`/receiver accesses to inherited members are left as written). These are Roslyn renamer behavior, not this repository's. RH4106 withholds them, so no other Reihitsu fix is known to ship them. No follow-up is needed unless the lead wants an upstream report.
