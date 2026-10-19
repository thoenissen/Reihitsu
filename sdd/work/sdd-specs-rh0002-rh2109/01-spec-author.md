Role: spec author
Commit: 074b4f3
Verdict: spec written

Lane: specs and tests for existing behavior. No change doc: every requirement states behavior the code has today (clarifications of unspecified rules). My prompt carried no intended fix or hoped-for verdict.

# What was written

- 18 analyzer specs: `sdd/specs/analyzer/RH0002.md`, `RH1001.md`, `RH1002.md`, `RH2001.md`, `RH2002.md`, `RH2003.md`, `RH2004.md`, `RH2006.md`, `RH2007.md`, `RH2101.md` … `RH2109.md`.
- 3 shared specs, each used by more than one of the new specs and each holding the type that implements the shared behavior as its only code owner:
  - `sdd/specs/shared/struct-equality.md` (SHARED-EQUAL, owners `StructEqualityPerformanceAnalyzerBase`, `EqualityComparerArgumentUtilities`) — used by RH1001 and RH1002.
  - `sdd/specs/shared/nested-types.md` (SHARED-NESTED, owner `NestedTypeAnalyzerHelper`) — used by RH2101, RH2104–RH2108.
  - `sdd/specs/shared/debug-message-text.md` (SHARED-DEBUG, owner `DebugMessageTextAnalysisUtilities`) — used by RH2006 and RH2007.
- RH2102/RH2103 share no code and no requirement wording that needed a shared spec; each states its own construct.
- RH2001 lists SHARED-CASING-006 under `Uses` for the field name and does not restate the derivation (clarification 3). `DeclarationModifierUtilities` (RH2005) and `ModifierOrderingUtilities` are not claimed as owners (clarification 4).
- No SHARED-FIX-003 case is written as a non-goal (clarification 5). Cases that P1 decides but the code does not implement are left out of the specs entirely and listed below.
- Where an open question sits inside a requirement's construct, the requirement carries a coverage sentence ("This requirement covers …") that keeps the questionable inputs out, as `shared/casing.md` does: SHARED-EQUAL-001, SHARED-DEBUG-001, RH2001-001, RH2002-001, RH2004-006, RH2109-001.
- Ledger: 60 IDs inserted into `sdd/specs/requirement-ids.txt` (pure insertions, `LC_ALL=C sort -c` holds). Index: 21 rows added to `sdd/specs/README.md`.
- Rule page corrected: `documentation/rules/RH2004.md` claimed every member that supports accessibility is checked; members declared directly in an interface support modifiers but are exempt. One sentence added. No other page is wrong.

# How the statements were established

- Every analyzer example block of every new spec was extracted (markup stripped) and run through the shipped analyzers with a scratch harness outside the repository (net10 reference assemblies, `LanguageVersion.Preview`, generated-code analysis as configured by `DiagnosticAnalyzerBase`), comparing the reported spans with the `{|RH####:…|}` markup: all blocks match, none has a compile error. RH0002 blocks ran with documentation mode `None`; RH2109 ran with `.razor` additional files.
- The fix examples of RH2001-004/-005/-006 and RH2004-006 were run with `scripts/apply-fix.sh` (LF and CRLF); the after-blocks are the tool's output.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RuleDocumentationConventionTests"`: 26 passed. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only for the 60 new IDs (no tests tagged yet) — the test implementer's work.
- The analyzer test project uses `ReferenceAssemblies.Net.Net90`; the harness used net10, so the test implementer's runs are the final check of the examples. While verifying, `array.Contains(item)` for a struct without equality members turned out to bind to `Enumerable.Contains` and be reported (the span overloads require `IEquatable<T>`), so it is not written as a negative case in RH1002-003.

## Changed requirements

- SHARED-EQUAL-001 — A struct without its own equality members — added
- SHARED-EQUAL-002 — A custom comparer argument replaces the equality members — added
- SHARED-NESTED-001 — A nested declaration is declared directly in a type — added
- SHARED-NESTED-002 — One diagnostic per declaration, on its name — added
- SHARED-DEBUG-001 — A constant without text is no message — added
- RH0002-001 — A file parsed without documentation comments is reported — added
- RH0002-002 — The diagnostic is on the first token of the file — added
- RH1001-001 — Hash-based collections keyed by a struct without equality members — added
- RH1001-002 — Every use of the collection type is reported — added
- RH1001-003 — A creation with a custom comparer is not reported — added
- RH1001-004 — A use through an alias is reported on the alias — added
- RH1001-005 — Where the diagnostic is — added
- RH1002-001 — Equality-based operations on a struct without equality members are reported — added
- RH1002-002 — The compared type is the key — added
- RH1002-003 — Methods of other types are not reported — added
- RH1002-004 — `Contains` on a dictionary is not reported — added
- RH1002-005 — Where the diagnostic is — added
- RH2001-001 — A private auto-implemented property is reported — added
- RH2001-002 — Other properties are not reported — added
- RH2001-003 — The diagnostic is on the property name — added
- RH2001-004 — The fix replaces the property with a private field — added
- RH2001-005 — The fix is withheld for accessors a field cannot express — added
- RH2001-006 — The fix is withheld when the field name is taken — added
- RH2002-001 — An async void method is reported — added
- RH2002-002 — An async void local function is reported — added
- RH2002-003 — An async anonymous function bound to a void delegate is reported — added
- RH2002-004 — Async functions that return a task, and void functions that are not async, are not reported — added
- RH2002-005 — The diagnostic is on the name, or on `async` of an anonymous function — added
- RH2003-001 — An explicit creation is reported — added
- RH2003-002 — A target-typed creation is reported — added
- RH2003-003 — Other exception types are not reported — added
- RH2003-004 — The diagnostic is on the created type, or on `new` — added
- RH2004-001 — A declaration without an accessibility modifier is reported — added
- RH2004-002 — Partial types and partial methods are not reported — added
- RH2004-003 — Static constructors and explicit interface implementations are not reported — added
- RH2004-004 — Interface members are not reported, nested types in interfaces are — added
- RH2004-005 — Where the diagnostic is — added
- RH2004-006 — The fix adds the default accessibility — added
- RH2006-001 — An assertion without message text is reported — added
- RH2006-002 — Only the `message` argument is checked — added
- RH2006-003 — Only `System.Diagnostics.Debug.Assert` is checked — added
- RH2006-004 — The diagnostic covers the whole call — added
- RH2007-001 — A failure without message text is reported — added
- RH2007-002 — Only the `message` argument is checked — added
- RH2007-003 — Only `System.Diagnostics.Debug.Fail` is checked — added
- RH2007-004 — The diagnostic covers the whole call — added
- RH2101-001 — A nested class is reported — added
- RH2101-002 — A nested static class is not reported — added
- RH2102-001 — A class with primary-constructor parameters is reported — added
- RH2102-002 — The diagnostic is on the class name — added
- RH2103-001 — A struct with primary-constructor parameters is reported — added
- RH2103-002 — The diagnostic is on the struct name — added
- RH2104-001 — A nested struct is reported — added
- RH2105-001 — A nested interface is reported — added
- RH2106-001 — A nested record or record struct is reported — added
- RH2107-001 — A nested enum is reported — added
- RH2108-001 — A nested delegate is reported — added
- RH2109-001 — A `@code` block in a `.razor` file is reported — added
- RH2109-002 — Other files are not reported — added
- RH2109-003 — The diagnostic covers `@code` — added

## Examples for the tests

Every requirement above has an example; all of them seed tests. Notes for the test implementer:

- SHARED-EQUAL-001 example uses RH1001, SHARED-EQUAL-002 uses RH1002; SHARED-NESTED-001/-002 use RH2101; SHARED-DEBUG-001 uses RH2006. A shared ID is covered once the tagged test exists in any of its users' test classes.
- Location requirements (RH1001-005, RH2002-005, RH2003-004, RH2004-005, RH2006-004, RH2007-004, RH2101/…/RH2108 via SHARED-NESTED-002, RH2102-002, RH2103-002, RH2001-003, RH2109-003) point at the examples of the other requirements.
- RH0002 tests need documentation mode `None` (the existing test class already has the solution transform). RH2109 tests use `.razor` additional files; the `razor` blocks in RH2109-001/-002 are file contents, not C#.
- RH2001 and RH2004 are `Fix All` (BatchFixer); their test classes already derive from `BatchCodeFixTestsBase`.
- Existing tests already pin much of RH1001/RH1002 (comparer forms, aliases, dictionary `Contains`); tagging them should cover most of SHARED-EQUAL-002, RH1001-003/-004 and RH1002-004.

## Open questions to file

None blocks this run. One issue per spec path, each a sub-issue of thoenissen/Reihitsu#912, titled `SDD open questions: <spec path>`. None of these specs has such an issue yet. Proposed bodies follow (headings and checklist as in the existing open-question issues).

### Issue: `SDD open questions: sdd/specs/shared/struct-equality.md`

> Part of #912 — see there for how questions are decided.
>
> While `sdd/specs/shared/struct-equality.md` (SHARED-EQUAL), used by RH1001 and RH1002, was written from the analyzers' current behavior, the cases below looked wrong. They were deliberately left out of the requirements. Each needs a maintainer decision: **bug** (fix the code and add the case to the spec), **intended** (add the case to the spec as it is), or **change** (change doc first). Record the decision as a comment and tick the box.
>
> ## Checklist
>
> - [ ] Q1 — A struct that implements `IEquatable<T>` without overriding `GetHashCode()` counts as having equality members
> - [ ] Q2 — A generic struct whose type argument lacks equality members counts as having them (`S?`, `(int, S)`)
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
> Why it looks wrong: the hash-based collections and operations call `GetHashCode()` through `EqualityComparer<S>.Default`, which for this struct is `ValueType.GetHashCode` — the boxing, reflection-based fallback the rules exist to report. `IEquatable<T>` removes only the cost of `Equals`. The rule pages say "implement `IEquatable<T>`, or override both `Equals(object)` and `GetHashCode()`", and the existing tests pin the current behavior (`EquatableStruct` in the RH1001/RH1002 test data), so the question is whether the documented "or" is intended. Touches SHARED-EQUAL-001 (its coverage sentence leaves this case out), RH1001-001, RH1002-001.
>
> ### Q2 — A generic struct whose type argument lacks equality members counts as having them
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
> Why it looks wrong: `Nullable<S>` overrides `Equals`/`GetHashCode`, and `ValueTuple<int, S>` implements `IEquatable<T>`, but both delegate to `EqualityComparer<S>.Default`, which falls back to `ValueType.Equals`/`GetHashCode` for `S`. The cost the rule reports for `c` is the same for `a` and `b`; only the outer type is inspected. Touches SHARED-EQUAL-001 (its coverage sentence leaves generic structs with such type arguments out), RH1001-001, RH1002-001.

### Issue: `SDD open questions: sdd/specs/analyzer/RH1002.md`

> Part of #912 — see there for how questions are decided. (Same preamble as above, for `sdd/specs/analyzer/RH1002.md` (RH1002).)
>
> ## Checklist
>
> - [ ] Q1 — `CountBy` and `AggregateBy` hash a struct key but are not checked
>
> ### Q1 — `CountBy` and `AggregateBy` hash a struct key but are not checked
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
>         items.CountBy(x => x);
>         items.AggregateBy(x => x, 0, (count, x) => count + 1);
>         items.GroupBy(x => x);
>     }
> }
> ```
>
> Output: RH1002 reports only `GroupBy`.
>
> Why it looks wrong: `Enumerable.CountBy` and `Enumerable.AggregateBy` (.NET 9) group by key through a dictionary keyed by the selected key and take an optional `IEqualityComparer<TKey>`, exactly like `GroupBy`, `ToLookup`, and `DistinctBy`, which the rule checks. They are missing from the rule's fixed list of method names. Touches RH1002-001.

### Issue: `SDD open questions: sdd/specs/shared/debug-message-text.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/shared/debug-message-text.md` (SHARED-DEBUG), used by RH2006 and RH2007.)
>
> ## Checklist
>
> - [ ] Q1 — `string.Empty` counts as message text
>
> ### Q1 — `string.Empty` counts as message text
>
> Input:
>
> ```csharp
> using System.Diagnostics;
>
> class C
> {
>     void M(bool condition)
>     {
>         Debug.Assert(condition, string.Empty);
>         Debug.Assert(condition, "");
>         Debug.Fail(string.Empty);
>     }
> }
> ```
>
> Output: RH2006 reports only `Debug.Assert(condition, "")`; RH2007 reports nothing.
>
> Why it looks wrong: `string.Empty` is the framework's spelling of the empty string and gives the failure no more text than `""`, but it is a `static readonly` field, not a compile-time constant, so the constant-only check treats it as text. The rule pages say "Constant `null`, empty, and whitespace-only messages are treated as missing text", which matches the code; the question is whether `string.Empty` is meant to escape. Touches SHARED-DEBUG-001 (its coverage sentence leaves this case out), RH2006-001, RH2007-001.

### Issue: `SDD open questions: sdd/specs/analyzer/RH2001.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/analyzer/RH2001.md` (RH2001).)
>
> ## Checklist
>
> - [ ] Q1 — The fix drops comments inside the property (Decided by principle P1 (#912))
> - [ ] Q2 — The fix is offered although code excluded by `#if` refers to the property (Decided by principle P1 (#912))
> - [ ] Q3 — The fix changes run-time behavior for a property of a mutable struct type (Decided by principle P1 (#912))
> - [ ] Q4 — A property whose accessors use `field` with logic of their own is reported
> - [ ] Q5 — A property with a `field:`-targeted or accessor attribute is reported
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
> Why it is wrong: the comments `/* name */` and `/* inner */` are removed. SHARED-FIX-003 (third condition) withholds a fix that changes the text of a comment the user wrote; the fix should not be offered for a property with a comment inside its declaration (or should keep the comments). Touches RH2001-004, SHARED-FIX-003.
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
> Why it is wrong: before the fix, `Current.Increment()` mutates a copy returned by the getter and `Run()` returns 0; after it, the field itself is mutated and `Run()` returns 1. SHARED-FIX-003 (second condition) withholds a fix that changes what the program does at run time. A get-only property is not affected (it becomes a `readonly` field, which also mutates a copy). Touches RH2001-004, SHARED-FIX-003.
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
> Why it looks wrong: the rule is about auto-implemented properties, whose accessors do nothing but read and write the backing field. The analyzer decides "auto-implemented" by whether the compiler created a backing field, which it also does for a property using the C# 14 `field` keyword, so a property with setter logic is reported although a field cannot replace it. (`{ get => field; }` without logic is reported too; reporting that one looks consistent with the rule.) Touches RH2001-001 (its coverage sentence leaves accessors with bodies out), RH2001-002.
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
> Why it looks wrong: the rule page says "Properties decorated with attributes are exempt", and a property with `[Obsolete]` is not reported, but the analyzer only looks at attributes that apply to the property itself. Two readings: the exemption is for any attribute written on the declaration (then both should not be reported), or only for attributes that need property syntax (then both are rightly reported, the page needs a correction, and a fix could move `[field: X]` to the field as `[X]`). Touches RH2001-001 (its coverage sentence leaves this case out), RH2001-002.

### Issue: `SDD open questions: sdd/specs/analyzer/RH2002.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/analyzer/RH2002.md` (RH2002).)
>
> ## Checklist
>
> - [ ] Q1 — An async void explicit interface implementation is not reported
> - [ ] Q2 — An async void partial method is reported twice, once on its non-async definition
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
> Why it looks wrong: both implement a `void` interface member and carry the same async void hazards. The implicit implementation and an `override async void` are reported; only the explicit form is skipped, because the analyzer checks `MethodKind.Ordinary`. Touches RH2002-001 (its coverage sentence leaves this case out).
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

### Issue: `SDD open questions: sdd/specs/analyzer/RH2003.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/analyzer/RH2003.md` (RH2003).)
>
> ## Checklist
>
> - [ ] Q1 — A creation through a `using` alias is not reported
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
> Why it looks wrong: the created type is `System.NotImplementedException` in both cases. A syntactic pre-filter compares the written simple name with `NotImplementedException` before the semantic check, so an alias name never reaches it. (Not covered by SHARED-DIAG-002: the alias is a different name, not a different spelling of the same name.) Touches RH2003-001.

### Issue: `SDD open questions: sdd/specs/analyzer/RH2004.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/analyzer/RH2004.md` (RH2004).)
>
> ## Checklist
>
> - [ ] Q1 — The fix adds `private` to an operator, which must be `public`
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
> Why it looks wrong: user-defined operators and conversion operators must be `public static` (CS0558); the input already does not compile, and the fix replaces it with code that still does not compile while removing the RH2004 diagnostic. Either the fix should add `public` for operators (their only valid accessibility), or it should not be offered for them. Touches RH2004-001, RH2004-006 (its coverage sentence leaves operators out).

### Issue: `SDD open questions: sdd/specs/analyzer/RH2109.md`

> Part of #912 — see there for how questions are decided. (Same preamble, for `sdd/specs/analyzer/RH2109.md` (RH2109).)
>
> ## Checklist
>
> - [ ] Q1 — `@code {` inside a Razor or HTML comment is reported (Decided by principle P5 (#912))
> - [ ] Q2 — An escaped `@@code {` and email-like text `user@code{` are reported
> - [ ] Q3 — `@functions` blocks are not reported
>
> ### Q1 — `@code {` inside a Razor or HTML comment is reported (Decided by principle P5 (#912))
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
> Why it looks wrong: the analyzer matches the regular expression `@code\s*\{` against the raw file text, so the text of a comment is checked as if it were a directive. P5 makes only the text of a comment content that a rule about code does not report; SHARED-DIAG-003 states it for C# comments, and a Razor or HTML comment in a `.razor` file is the same kind of text. Proposed marking assumes P5 extends to Razor comments — the maintainer confirms. Touches RH2109-001 (its coverage sentence leaves comments out).
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

For the lead to comment on; not new questions and not written into any spec.

- thoenissen/Reihitsu#971 (leading acronym run): RH2001's fix names the field `_hTTPClient` for `private int HTTPClient { get; set; }` (verified with `scripts/apply-fix.sh RH2001`). `URL` → `_url` and `first_name` → `_firstName` are correct. SHARED-CASING-006's coverage already leaves this input out, so RH2001-004 does not specify it.
- thoenissen/Reihitsu#967 (`DeclarationModifierUtilities.AddAccessibilityModifiers` loses indentation): RH2004's fix on a member whose attribute list is on its own line, with no modifier on the member, produces `private     void M()` at column 0:

  ```csharp
  public class C
  {
      [Obsolete]
      void M()
      {
      }
  }
  ```

  becomes `[Obsolete]` / `private     void M()` (indentation moved behind the inserted modifier). Same utility and same defect class as #967 case 1 (the inserted modifier does not take over the leading trivia of the token that starts the line); a different input shape, since RH2004 only sees declarations without any accessibility modifier. #996 says not to file the shared defect again. If the lead judges it a new instance instead, it is decided by P10 (#912) and belongs in the RH2004 issue. RH2004-006's coverage sentence leaves declarations with attribute lists out.

## New-rule candidates under P11

None. No finding of this batch is an analyzer/formatter disagreement; `CountBy`/`AggregateBy` (RH1002) and `@functions` (RH2109) are filed as ordinary open questions above because P11 concerns what the formatter does beyond the analyzers, which does not apply to them.

## Decisions needed

None (no blocking question).
