Role: spec author
Commit: f857eda
Verdict: spec written

This is the repair round after `02-skeptic.md`, which ended in "proceed narrower". I re-ran every finding myself with the scratch probes: `/tmp/claude-0/-home-user-Reihitsu/153ec12d-0883-5726-b799-f5a8330ef8f9/scratchpad/probe`, and the skeptic's `skeptic/probe2`, which compiles a referenced assembly through `LIB=<file>`. All of A–F and the partial-member finding reproduce as the skeptic describes. I used the skeptic's suggested wording only where my own derivation arrived at the same scope.

- No existing requirement of `shared/casing.md` or of any other existing spec changed, and none had to: every repair is a coverage limit or a clarification inside this run's new specs.
- No change doc: the specs still describe current behavior only.
- Rule pages are unchanged.

## Changed requirements

- **RH4111-001 — clarified.** One diagnostic per property declaration, so each part of a partial property gets its own diagnostic. Partial example added.
- **RH4112-001, RH4113-001, RH4114-001 — clarified and narrowed.**
  - Partial wording and example, as for RH4111-001.
  - Virtual and abstract properties are checked, and so are overrides (and, for RH4114, implicit interface implementations) whose original definition is declared in source.
  - A new coverage sentence, the same as RH4102-001 and RH4103-001: the property's chain of overridden and implicitly implemented properties never leads into a referenced assembly (finding A).
- **RH4111-002, RH4112-002, RH4113-002, RH4114-002 — narrowed.** They cover only a property whose name is not also a parameter name of its record's primary constructor (finding B).
- **RH4112-002, RH4113-002, RH4114-002 — clarified.**
  - These three also cover only the properties that their -001 covers (finding A).
  - They state the cascade in source: a rename also renames the overridden and overriding properties, and for RH4114 the interface property and its implicit implementation together.
  - A cascade example was added to each (finding D).
- **RH4115-001 — clarified.** It adds the designations of property and positional patterns and of list patterns, with examples `Text`, `Both` and `Items` (finding C).
- **RH4116-001 — narrowed.** It covers only a tuple type that is not part of the signature of a member that overrides or implements a member declared in a referenced assembly (finding E).
- **RH4120-001 — narrowed.** It covers a parameter that the compiler turns into a property, which it does when no member of that name is declared in the record or inherited from its base type. The `Why:` is corrected to match (finding B).
- **RH4120-003 — narrowed.** It covers the parameters that RH4120-001 covers (finding B).
- **RH4121-001 — clarified.** One diagnostic per declared type parameter, so each part of a partial type or partial method gets its own diagnostic. Partial example added.
- **RH4122-001 — clarified.**
  - One diagnostic per declared parameter, so each part of a partial method gets its own diagnostic. Partial example added.
  - The `Why:` explains why implicit interface implementations are checked.

No requirement was added or retired. `requirement-ids.txt` and `sdd/specs/README.md` are unchanged.

### Checks

- **Examples.** Every example of the 16 specs and the shared spec was re-run with `check.py`:
  - every marked block reports exactly its spans, with no compiler errors;
  - every "is not reported" block reports nothing;
  - every before/after pair gives the after block when the single fixes are applied one after another, including the three new cascade examples.
- **Architecture tests.** `scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: 26 passed. `RequirementCoverageTests` still lists the 36 IDs until the tests are tagged.

## Examples for the tests

All 36 IDs of `01-spec-author.md` are seeds. These examples are new in this round:
- RH4111-001 to RH4114-001: partial property.
- RH4112-002, RH4113-002, RH4114-002: cascade in source.
- RH4115-001: `Text`, `Both`, `Items`.
- RH4121-001: partial type.
- RH4122-001: partial method.

The tests must not pin any case of the questions below.

## Open questions to file

This is the final list; the lead can file from it alone. Every input was run against the shipped analyzer and code fix at `f857eda`. None blocks the run, and each case is left out of the specs.

Existing issues, checked through the sub-issues of thoenissen/Reihitsu#912 and a search:
- thoenissen/Reihitsu#1009 — `shared/casing.md`
- thoenissen/Reihitsu#1013 — RH4103
- thoenissen/Reihitsu#1014 — RH4102

No issue exists for RH4112, RH4113, RH4114, RH4115, RH4116, RH4118, RH4121, RH4122 or RH4124, so every issue named "new" below has to be opened.

### Q1 — RH4115 reports a local named `_` and offers no fix

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`.
- **Touches:** RH4115-001, which covers names other than `_`.
- **Input:**

  ```csharp
  using System;
  class C
  {
      void M(int[] values)
      {
          int _ = 42;
      }
      void N(int[] values)
      {
          foreach (var _ in values) { }
          try { } catch (Exception _) { }
      }
      void O()
      {
          using var _ = (IDisposable)null;
      }
  }
  ```

- **Real output:** RH4115 is reported on each `_`. No code fix is offered, because `_` has no letter or digit (SHARED-CASING-008).
- **Why it looks wrong:** every neighbouring rule treats `_` as the conventional name of an unused value:
  - RH4105-002 and RH4120-002 exempt a parameter `_`.
  - SHARED-CASING-003 accepts a field `_`.
  - SHARED-SINGLE-001 exempts `_` from all single-letter rules, RH4125 to RH4127 included.

  A local `_` is the only such name that is reported, and the user can only resolve it by hand.
- **Readings:**
  - A: do not report a local named `_`.
  - B: keep the report and specify it.

### Q2 — RH4115 requires camelCase for local constants

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`.
- **Touches:** RH4115-001, which covers local declarations that are not `const`; RH4115-003.
- **Input:**

  ```csharp
  class C { int M() { const int MaxValue = 3; return MaxValue; } }
  ```

- **Real output:** RH4115 is reported on `MaxValue`. The fix renames the constant and its use to `maxValue`.
- **Why it looks wrong:** RH4110 requires PascalCase for constant fields, and the .NET naming conventions use PascalCase for constants, local constants included. A constant would have to change its style when it moves between a field and a local.
- **Readings:**
  - A: do not check local constants under RH4115, or check them for PascalCase.
  - B: keep camelCase and specify it.

### Q3 — A deconstruction written as `(var a, var b)` is reported by RH4115, not RH4117

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`. It also touches RH4117.
- **Touches:** RH4115-001, RH4115-002 and RH4117-001, which cover only the `var (…)` form.
- **Input:**

  ```csharp
  using System.Collections.Generic;
  class C
  {
      void M((int, int) pair, List<(int, int)> list)
      {
          (var Fifth, var Sixth) = pair;
          (int Seventh, var (Eighth, Ninth)) = (1, pair);
          foreach ((var Key2, var Value2) in list) { }
      }
  }
  ```

- **Real output:**
  - RH4115 reports `Fifth`, `Sixth`, `Seventh`, `Key2` and `Value2`.
  - RH4117 reports only `Eighth` and `Ninth`.
  - `var (First, Second) = pair;` and `foreach (var (Key, Value) in list)` are reported only by RH4117.
- **Why it looks wrong:** both forms declare the variables of one deconstruction. A user who turns off RH4117 still gets RH4115 for the parenthesized form. Rule page `documentation/rules/RH4115.md` says "Variables declared by a deconstruction are handled by RH4117 instead", which this form contradicts.
- **Readings:**
  - A: RH4117 reports every variable declared in a deconstruction, in either form, and RH4115 none of them.
  - B: keep the split by syntax form and correct the RH4115 rule page.

### Q4 — The variables of a `var (…)` pattern are checked by neither RH4115 nor RH4117

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`. It also touches RH4117.
- **Touches:** RH4115-001, which lists the pattern forms it covers; RH4117-001, which covers deconstruction declarations only.
- **Input:**

  ```csharp
  class C
  {
      void M((int, int) pair)
      {
          if (pair is var (First, Second)) { }
          switch (pair) { case var (Left, Right): break; }
          if (pair is (var Third, var Fourth)) { }
      }
  }
  ```

- **Real output:**
  - `First`, `Second`, `Left` and `Right` are reported by neither RH4115 nor RH4117.
  - `Third` and `Fourth` are reported by RH4115.
  - RH4125 does report single-letter names in the same positions (`is var (a, b)`).
- **Why it looks wrong:** these are ordinary pattern locals, but no casing rule checks them. RH4115 skips any designation inside a parenthesized designation. RH4117 only looks at deconstruction declarations.
- **Readings:**
  - A: RH4115 reports them, like every other pattern variable.
  - B: RH4117 reports them, as deconstruction variables.
  - C: neither, as now.

### Q5 — RH4121's fix doubles the prefix when it is followed by `_`

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4121.md`.
- **Touches:** RH4121-003, which does not cover an old name starting with `t` or `T` followed by `_`.
- **Input:** one fixture each:

  ```csharp
  class C<T_Key> { }
  ```

  ```csharp
  class C<t_key> { }
  ```

  ```csharp
  class C<T_1> { }
  ```

- **Real output:** the fix renames them to `TTKey`, `TTKey` and `TT1`.
- **Why it looks wrong:** the author already wrote the prefix and separated it with `_`. The fix treats it as the first word of the body and adds a second `T`. A `t` or `T` followed by an uppercase letter or a digit gets only its case corrected (`tKey` → `TKey`, `t1` → `T1`).
- **Readings:**
  - A: treat `T_`/`t_` as the prefix, giving `TKey`, `TKey` and `T1`.
  - B: keep the current output.

  `Type` → `TType` and `tkey` → `TTkey` follow the stated rule for a `T` followed by a lowercase letter, and are not part of this question.

### Q6 — RH4118's analyzer type is named after tuple elements, not after its rule

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4118.md`. thoenissen/Reihitsu#979 asks for this naming defect to be filed, not fixed.
- **Touches:** the code owners of RH4116 and RH4118.
- **Observation:**
  - RH4118's title is "Named tuple argument names should be in PascalCase", and the rule checks names written in a tuple expression.
  - Its type is `RH4118TupleElementCasingAnalyzer`, and its test class is `RH4118TupleElementCasingAnalyzerTests`.
  - `RH4116TupleElementCasingAnalyzer` checks the element names of a tuple type, which matches RH4116's title "Named tuple elements should be in PascalCase".
  - The two type names differ only in their ID prefix.
- **Readings:**
  - A: rename RH4118's analyzer and test class to match its title, for example `RH4118TupleArgumentNameCasingAnalyzer`, and update the code owners and the index row.
  - B: keep the names.

### Q7 — Query range variables are checked by no naming rule, while the equivalent lambda parameter is

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4124.md`. It also touches RH4105, RH4115 and RH4125.
- **Touches:** RH4124-001, RH4125-001 and RH4115-001, none of which covers range variables.
- **Input:**

  ```csharp
  using System.Linq;
  class C
  {
      void M(int[] values)
      {
          var a = from x in values let y = x * 2 select y;
          var b = from Row in values select Row;
          var c = values.Select(x => x);
      }
  }
  ```

- **Real output:**
  - RH4124 reports the lambda parameter `x` in `c`.
  - No rule reports the range variables `x`, `y` or `Row`. That includes RH4105, RH4115, RH4124 and RH4125.
- **Why it looks wrong:** a range variable is the query-syntax form of a lambda parameter, and `let` introduces a value like a local. The same name is reported in method syntax and accepted in query syntax.
- **Readings:**
  - A: report range variables under the lambda-parameter rules (RH4124, RH4105) or the local rules (RH4125, RH4115).
  - B: leave them unchecked, and record that as a non-goal once decided.

### Q8 — A property whose original definition is in a referenced assembly is reported, and the fix is missing or breaks the build

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4112.md`, `…/RH4113.md` and `…/RH4114.md`. The lead may file one question and link it from each of the three issues.
- **Same as:** thoenissen/Reihitsu#1013 Q1 (RH4103) and thoenissen/Reihitsu#1014 Q1 (RH4102).
- **Touches:** RH4112-001/-002, RH4113-001/-002, RH4114-001/-002, which now cover only properties whose original definition is declared in source. RH4111 cannot reach this, because a private property cannot override or implicitly implement.
- **Input:** a referenced assembly declares:

  ```csharp
  public class Base { public virtual int count { get; set; } protected virtual int size { get; set; } }
  public interface ILib { int total { get; } }
  ```

  For RH4113, a second referenced assembly with `[assembly: InternalsVisibleTo("P")]` declares `public class Base { internal virtual int size { get; set; } }`.

  The source declares:

  ```csharp
  public class Impl : ILib { public int total => 0; }
  public class Middle : Base { public override int count { get; set; } }
  public class Leaf : Middle { public override int count { get; set; } }
  ```

  It also declares the same `Middle`/`Leaf` chain with `protected override int size` (RH4112) and with `internal override int size` (RH4113).
- **Real output:**
  - Every property is reported: RH4114 for `total` and `count`, RH4112 and RH4113 for `size`.
  - `Impl.total` and `Middle.*` get no code fix.
  - `Leaf.*` gets a fix that renames `Leaf` and `Middle` together to `Count` or `Size`. The result fails with CS0115 ("'Middle.Count': no suitable method found to override").
- **Why it looks wrong:** the name is dictated by a type the author does not control, and the fix for `Leaf` produces code that does not compile. SHARED-CASING-009 does not withhold the fix, because the added error is in `Middle`, which does not contain the renamed `Leaf.count`.
- **Readings:**
  - A: do not report such properties, and withhold the fix that renames a source chain of a metadata property.
  - B: keep the current output.

### Q9 — Renaming a record's positional parameter or its same-named member separates the two and adds a public property

- **Issue:** existing thoenissen/Reihitsu#1009 (`shared/casing.md`), as a new question there. Every rule that reaches it shares the rename of SHARED-CASING-007, and none of the questions already in #1009 covers it.
- **Touches:**
  - RH4111-002, RH4112-002, RH4113-002 and RH4114-002, which now cover only a property whose name is not also a positional parameter name.
  - RH4120-001 and RH4120-003, which now cover only a parameter that the compiler turns into a property.
  - SHARED-CASING-007. That requirement is not changed here.
  - The field rules RH4106 to RH4110 of thoenissen/Reihitsu#978. A follow-up for those specs, not changed in this run.
- **Input:** one fixture each:

  ```csharp
  public record R(int count)
  {
      public int count { get; init; } = count;
  }
  ```

  ```csharp
  public record R(int count)
  {
      private int count { get; init; } = count;
  }
  ```

  ```csharp
  public record R(int count)
  {
      public int count = count;
  }
  ```

- **Real output:**
  - **First fixture:** RH4120 and RH4114 both report `count`.
    - RH4120's fix gives `public record R(int Count) { public int count { get; init; } = Count; }`. The compiler now synthesizes a new public property `Count` next to `count`: `r.Count` compiles afterwards and failed with CS1061 before.
    - RH4114's fix gives `public int Count { get; init; } = count;`. `r.count` still compiles afterwards, now as a synthesized public property.
  - **Second fixture:** RH4111 reports the private property. Its fix gives `private int Count { get; init; } = count;`, which adds a public synthesized `count`.
  - **Third fixture** (from the skeptic's run): RH4109's fix gives `public int Count = count;`.
- **Why it looks wrong:** each fix silently adds a public property. That changes the record's equality, `ToString`, `Deconstruct` and public API, while the fix claims only to rename (FC-006, FC-036).
- **Readings:**
  - A: withhold the fix when the rename would separate a positional parameter from its member of the same name.
  - B: rename the parameter and the member together.
  - C: keep the current output.

### Q10 — RH4116 reports tuple element names that a member of a referenced assembly dictates

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4116.md`.
- **Touches:** RH4116-001, which now covers only tuple types that are not part of the signature of a member overriding or implementing a member of a referenced assembly.
- **Input:** a referenced assembly declares:

  ```csharp
  public class Base { public virtual (int first, int Second) Pair() => default; }
  ```

  The source declares:

  ```csharp
  public class Middle : Base { public override (int first, int Second) Pair() => default; }
  ```

- **Real output:** RH4116 reports `first` in the override. The rule has no code fix.
- **Why it looks wrong:**
  - Changing the name in the override is a compile error (CS8139).
  - For an implementation of an interface member, the same change gives CS8141.
  - The base cannot be edited.

  So the diagnostic can only be suppressed.
- **Readings:**
  - A: do not report element names that the compiler requires to match a member of a referenced assembly.
  - B: keep reporting them.

### Q11 — RH4122 does not report the receiver parameter of a C# 14 extension block

- **Issue:** new `SDD open questions: sdd/specs/analyzer/RH4122.md`. No spec change: RH4122-001 does not list the receiver.
- **Touches:** RH4122-001.
- **Input:**

  ```csharp
  public static class E
  {
      public static int Ext(this int s) => s;
      extension(string t)
      {
          public int Size => t.Length;
      }
  }
  ```

- **Real output:**
  - RH4122 reports `s` and does not report `t`.
  - RH4105 does report an extension-block receiver named `Text`.
- **Why it looks wrong:** the receiver of a classic extension method is reported, but the receiver of an extension block is not, and the casing rule RH4105 checks the same parameter. The receiver's containing symbol is the extension type, not a method, so the rule's parameter filter skips it.
- **Readings:**
  - A: report it, like the `this` parameter of an extension method.
  - B: leave it unchecked, and record that as a non-goal once decided.

## Follow-up candidates (out of scope)

- `CodeFixResources.RH4116Title` and `RH4118Title` ("Fix casing") are unused; neither rule has a code-fix provider.
- Once Q9 is decided, the field specs RH4106 to RH4110 need the same coverage review for a field that shares its name with a positional parameter.
