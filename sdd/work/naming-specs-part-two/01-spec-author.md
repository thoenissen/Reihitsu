Role: spec author
Commit: cc63994
Verdict: spec written

No change doc: the run specifies existing behavior; no requirement changes intended behavior. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## What was written

- 16 analyzer specs: `sdd/specs/analyzer/RH4111.md` to `RH4118.md`, `RH4120.md` to `RH4127.md`.
- One new shared spec: `sdd/specs/shared/single-letter-names.md` (prefix `SHARED-SINGLE`, code owner `SingleLetterIdentifierUtilities`). It exists because more than one spec relies on the same requirement: SHARED-SINGLE-001 (what a single-letter name is) is used by all six of RH4122 to RH4127, and SHARED-SINGLE-002 (which variables are `foreach` iteration variables) is used by both RH4125 and RH4126.
- `sdd/specs/shared/casing.md` is reused and not changed. RH4121 defines its own style and derivation, because the casing spec names type parameters as its non-goal; it uses SHARED-CASING-001, -006, -007, -009 and -010.
- 36 new IDs in `sdd/specs/requirement-ids.txt`, in ordinal order. 17 new rows in `sdd/specs/README.md`.
- No rule page changed: the documented behavior does not change.

How it was established: I read each analyzer, code-fix provider, `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `CasingUtilities` and `SingleLetterIdentifierUtilities`. Then I ran every example through the shipped analyzers with a scratch probe (`/tmp/claude-0/-home-user-Reihitsu/153ec12d-0883-5726-b799-f5a8330ef8f9/scratchpad/probe`, driven by `check.py` next to it), using the Debug build of `cc63994`:
- Every marked block reports exactly its marked spans, with no compiler errors.
- Every "is not reported" block reports nothing.
- Every before/after pair gives the after block when the single fixes are applied one after another.
- The RH4121-002 and RH4121-004 blocks match what the "no fix offered" observations say: in RH4121-002, `Key`, `tKey` and `t1` collide with `TKey` and `T1` in the same list.

`scripts/test.sh --project architecture --configuration Debug --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists all 36 new IDs as uncovered. That is expected until the tests are tagged.

About the issue's premise on RH4116 and RH4118: their analyzer types are `RH4116TupleElementCasingAnalyzer` (element names of a tuple *type*) and `RH4118TupleElementCasingAnalyzer` (names written in a tuple *expression*). They share the suffix, not the full name. Both are specified separately (RH4116-001, RH4118-001). The naming defect is Q6 below.

## Changed requirements

- SHARED-SINGLE-001 — A single-letter name is one character other than `_` — added
- SHARED-SINGLE-002 — The iteration variables of a `foreach` statement — added
- RH4111-001 — Private property names are in PascalCase — added
- RH4111-002 — The fix renames the property to PascalCase — added
- RH4112-001 — Protected property names are in PascalCase — added
- RH4112-002 — The fix renames the property to PascalCase — added
- RH4113-001 — Internal property names are in PascalCase — added
- RH4113-002 — The fix renames the property to PascalCase — added
- RH4114-001 — Public property names are in PascalCase — added
- RH4114-002 — The fix renames the property to PascalCase — added
- RH4115-001 — Local variable names are in camelCase — added
- RH4115-002 — Variables of a `var (…)` deconstruction are not checked — added
- RH4115-003 — The fix renames the variable to camelCase — added
- RH4116-001 — Tuple type element names are in PascalCase — added
- RH4117-001 — Variables of a `var (…)` deconstruction are in camelCase — added
- RH4117-002 — The fix renames the variable to camelCase — added
- RH4118-001 — Element names written in a tuple expression are in PascalCase — added
- RH4120-001 — Record primary constructor parameter names are in PascalCase — added
- RH4120-002 — Discards and other parameters are not checked — added
- RH4120-003 — The fix renames the parameter and its property to PascalCase — added
- RH4121-001 — Type parameters of every generic declaration are checked — added
- RH4121-002 — A type parameter name is `T`, `T` and a PascalCase name, or `T` and digits — added
- RH4121-003 — The fix adds or corrects the `T` prefix — added
- RH4121-004 — The fix is withheld when no usable name can be derived — added
- RH4122-001 — Single-letter parameter names are reported — added
- RH4122-002 — Overrides and explicit interface implementations are not checked — added
- RH4122-003 — Record, local function, lambda, and anonymous method parameters are not checked — added
- RH4123-001 — Single-letter local function parameter names are reported — added
- RH4123-002 — Parameters of other members are not checked — added
- RH4124-001 — Single-letter lambda and anonymous method parameter names are reported — added
- RH4124-002 — Parameters of other members are not checked — added
- RH4125-001 — Single-letter local variable names are reported — added
- RH4125-002 — Iteration variables, catch variables, and fields are not checked — added
- RH4126-001 — Single-letter iteration variable names are reported — added
- RH4126-002 — Variables that are not iteration variables are not checked — added
- RH4127-001 — Single-letter exception variable names are reported — added

## Examples for the tests

Every requirement above has its own example. All 36 IDs are seeds: SHARED-SINGLE-001, SHARED-SINGLE-002, RH4111-001 to RH4127-001.

Fences on scope, which the tests must respect:
- RH4115-001 covers names other than `_`, and only local declarations that are not `const` (Q1 and Q2).
- RH4115-001 covers pattern designations only of declaration patterns and of `var` patterns that declare a single variable (Q4).
- RH4115-002 and RH4117-001 cover only the `var (…)` form of a deconstruction (Q3 and Q4).
- RH4121-003 does not cover an old name that starts with `t` or `T` followed by `_` (Q5).
- The camelCase fixes of RH4115, RH4117 and RH4121 inherit the coverage limits of SHARED-CASING-006 and SHARED-CASING-007. That keeps thoenissen/Reihitsu#971 (`HTTPClient` → `hTTPClient`) and thoenissen/Reihitsu#1009 Q2 (an `@` escape for keywords) out, as the maintainer asked.

## Open questions to file

None of these blocks the run: each case is left out of the specs. Issues that already exist were checked through the sub-issues of thoenissen/Reihitsu#912. None exists for RH4115, RH4118, RH4121, or RH4124, so each of the issues named below is new. Every input below was run against the shipped analyzer and code fix.

### Q1 — RH4115 reports a local named `_` and offers no fix

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`.
- **Touches:** RH4115-001, which covers names other than `_`.
- **Input:**

  ```csharp
  class C { void M() { int _ = 42; } }
  ```

- **Real output:** RH4115 is reported on `_`. No code fix is offered, because `_` has no letter or digit (SHARED-CASING-008).
- **Why it looks wrong:** every neighbouring rule treats `_` as the conventional name of an unused value:
  - RH4105-002 exempts a parameter `_`, and RH4120-002 a record parameter `_`.
  - SHARED-CASING-003 accepts a field `_`.
  - SHARED-SINGLE-001 exempts `_` in all single-letter rules, RH4125 included.

  A local `_` is the only such name that is reported, and the user can only resolve it by hand.
- **Readings:**
  - A: do not report a local named `_`.
  - B: keep the report and specify it.

### Q2 — RH4115 requires camelCase for local constants

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`.
- **Touches:** RH4115-001, which covers local declarations that are not `const`; RH4115-003.
- **Input:**

  ```csharp
  class C { int M() { const int MaxValue = 3; return MaxValue; } }
  ```

- **Real output:** RH4115 is reported on `MaxValue`. The fix renames it, and its use, to `maxValue`.
- **Why it looks wrong:** RH4110 requires PascalCase for constant fields, and the .NET naming conventions use PascalCase for constants, local constants included. The same constant would have to change its style when it moves between a field and a local.
- **Readings:**
  - A: do not check local constants under RH4115, or check them for PascalCase.
  - B: keep camelCase and specify it.

### Q3 — A deconstruction written as `(var a, var b)` is reported by RH4115, not RH4117

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`. It also touches RH4117.
- **Touches:** RH4115-001, RH4115-002, RH4117-001. Each now covers only the `var (…)` form.
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
  - `var (First, Second) = pair;` and `foreach (var (Key, Value) in list)` are reported by RH4117 only.
- **Why it looks wrong:** both forms declare the variables of one deconstruction. A user who turns off RH4117 still gets RH4115 for the parenthesized form. Rule page `documentation/rules/RH4115.md` says "Variables declared by a deconstruction are handled by RH4117 instead", which the parenthesized form contradicts.
- **Readings:**
  - A: RH4117 reports every variable declared in a deconstruction, whichever form it uses, and RH4115 none of them.
  - B: keep the split by syntax form and correct the RH4115 rule page.

### Q4 — The variables of a `var (…)` pattern are checked by neither RH4115 nor RH4117

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4115.md`. It also touches RH4117.
- **Touches:** RH4115-001, which covers declaration patterns and `var` patterns that declare a single variable; RH4117-001, which covers deconstruction declarations only.
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
  - `First`, `Second`, `Left` and `Right` are not reported by RH4115 or RH4117.
  - `Third` and `Fourth` are reported by RH4115.
  - RH4125 does report single-letter names in the same positions (`is var (a, b)`).
- **Why it looks wrong:** these are ordinary pattern locals, but no casing rule checks them. RH4115 skips them because they sit in a parenthesized designation. RH4117 only looks at deconstruction declarations.
- **Readings:**
  - A: RH4115 reports them, like every other pattern variable.
  - B: RH4117 reports them, as deconstruction variables.
  - C: neither, as now.

### Q5 — RH4121's fix doubles the prefix when it is followed by `_`

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4121.md`.
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
- **Why it looks wrong:** the author already wrote the prefix and separated it from the name with `_`. The fix treats the `T` or `t` as the first word of the body and puts a second `T` in front. A `t` or `T` followed by an uppercase letter or a digit (`tKey` → `TKey`, `t1` → `T1`) gets only its case corrected.
- **Readings:**
  - A: treat `T_`/`t_` as the prefix, giving `TKey`, `TKey` and `T1`.
  - B: keep the current output and specify it.

  `Type` → `TType` (tested) is the same mechanism for a `T` followed by a lowercase letter, and is not part of this question.

### Q6 — RH4118's analyzer type is named after tuple elements, not after its rule

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4118.md`. This is the naming defect that thoenissen/Reihitsu#979 asks to be filed, not fixed.
- **Touches:** the code owners of RH4116 and RH4118.
- **Observation:**
  - The rule title of RH4118 is "Named tuple argument names should be in PascalCase", and the rule checks names written in a tuple expression.
  - Its type is `RH4118TupleElementCasingAnalyzer`, and its test class is `RH4118TupleElementCasingAnalyzerTests`.
  - RH4116's type, `RH4116TupleElementCasingAnalyzer`, checks the element names of a tuple *type*, which matches its title "Named tuple elements should be in PascalCase".
  - The two types differ only in their ID prefix, so the type name alone does not tell which syntax each rule checks.
- **Readings:**
  - A: rename RH4118's analyzer and test class to match its title, for example `RH4118TupleArgumentNameCasingAnalyzer`, and update the spec's code owners and the index row.
  - B: keep the names.

### Q7 — Query range variables are checked by no naming rule, while the equivalent lambda parameter is

- **Spec issue:** new `SDD open questions: sdd/specs/analyzer/RH4124.md`. It also touches RH4115 and RH4125.
- **Touches:** RH4124-001, RH4125-001, RH4115-001. None of them covers range variables.
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
- **Why it looks wrong:** a range variable is the query-syntax form of a lambda parameter, and `let` introduces a value like a local. The same name is reported in method syntax and silently accepted in query syntax.
- **Readings:**
  - A: report range variables under the lambda-parameter rules (RH4124, RH4105) or the local rules (RH4125, RH4115).
  - B: leave them unchecked, and record that as a non-goal once decided.

## Follow-up candidates (out of scope)

- `CodeFixResources` declares `RH4116Title` and `RH4118Title` ("Fix casing"), but neither rule has a code-fix provider. Their only reference is the generated `CodeFixResources.cs`. These are unused resource strings.
