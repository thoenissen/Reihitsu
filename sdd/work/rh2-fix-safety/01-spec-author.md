Role: spec author
Commit: 162e7e6
Verdict: #967: spec written; #1286: spec written; #1287: open question; #1288: spec written

All changes are clarifications in the bug-fix lane: they write down behavior the maintainer decided in the issues and their comments (#930, #912 rounds 7a, 7b, 8), and widen coverage sentences. No change doc. The prompt carried no intended fix or verdict to ignore.

`SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails only on the four new IDs (RH2001-007, RH2001-008, RH2005-004, RH7002-004). Those tests come from the test implementer.

## thoenissen/Reihitsu#967

### Changed requirements

- RH2005-003 — The fix replaces `public` with `private` — clarified: new example `static public int Counter;` → `private static int Counter;`, where `private` takes the place of the first modifier, on its line and in its column. The requirement already says "put `private` first", and SHARED-FIX-007 already covers the layout, so case 1 is a bug against both. No new requirement.
- RH2005-004 — The fix is withheld for a field used outside its type — added. The fix is withheld when any code in the solution outside the containing type references the field. The containing type's body includes its nested types and every part of a partial type. A derived type counts as outside. Excluded `#if` code is left to SHARED-FIX-003. `Uses` gains SHARED-FIX-003.
- RH2004-006 — The fix adds the default accessibility — clarified for the issue comment's RH2004 instance. The coverage sentence no longer excludes attribute lists. The added modifier goes after the attribute lists, in front of the first token that follows them, on that token's line and in its column. New example: `[Obsolete]` on its own line, then `void M()` → `private void M()` indented.

The issue says to remove "both open-question lines" from the RH2005 non-goals. Nothing like that is in the non-goals on `main`, so nothing was removed.

### Examples for the tests

RH2005-003 (second example), RH2005-004 (three examples: unrelated type, derived type, partial part plus nested type), RH2004-006 (attribute example).

### Output questions

_None._

### Internal choices

- `documentation/rules/RH2005.md` is unchanged, although the issue asks for a sentence about the public surface. A withheld fix is an exception, not the normal case. `general-notes.md` already covers "a diagnostic without a code fix", and the page already tells users to use a property when the value belongs to the public surface. Alternative: add the sentence the issue asks for.
- Case 1 is recorded as an example under RH2005-003 rather than as a new requirement, because the existing requirement already decides it. Alternative: a separate requirement for the modifier's position.
- Fix All withholds the fix per field. That is SHARED-FIX-002 and is not restated.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1286

### Changed requirements

- RH2001-004 — The fix replaces the property with a private field — clarified:
  - a property that already carries `readonly` keeps exactly one `readonly`. New example: a `readonly` struct property becomes `private readonly int _value;`. The coverage clause "does not carry the `readonly` modifier" is removed.
  - the coverage sentence now points to SHARED-CASING-007 / SHARED-FIX-003 for a property whose old or new name occurs in excluded `#if` / `#elif` / `#else` code. New negative example covers both forms from the issue comment: an inactive reference to `Value`, and an inactive declaration of `_value`.
  - `Uses` gains SHARED-FIX-003.
- RH2001-007 — The fix is withheld for a comment inside the property — added. Covers a comment between the name and the accessor list's `{`, or inside the accessor list. Example: `Value /* name */ { get; /* inner */ set; } // trailing`. A comment before the first token or after the last token does not block the fix.
- RH2001-008 — The fix is withheld when a mutable struct would be changed in place — added, for the decided part only. The property has a `set` accessor, its type is a non-`readonly` struct, and a reference calls a non-`readonly` instance method of it (`Current.Increment()`). A coverage sentence leaves every other property of a non-`readonly` struct type unspecified until output question 2 is answered.

### Examples for the tests

- RH2001-004: the `readonly` struct example, and the `#if` example. Tag the `#if` example SHARED-CASING-007 and SHARED-FIX-003, not RH2001-004, because its case is outside RH2001-004's coverage.
- RH2001-007.
- RH2001-008.

### Output questions

**2 · RH2001 · Private auto-implemented properties should not be used — a property whose type is a struct not declared `readonly`, with no call that changes it**

The issue decides `Counter Current { get; set; }` with `Current.Increment()` → no fix. It leaves open where the "mutable struct" boundary lies. Three cases fall on it:

- `int?`: `Nullable<T>` is not declared `readonly`, but nothing can change it in place.
- a user struct that is not `readonly` but is never changed through the property.
- a get-only property. It becomes a `readonly` field, which copies the value like the property does, except inside a constructor.

Requirements: RH2001-004 (its coverage names reference types, enums, and `readonly` structs), RH2001-008.

Input:
```csharp
class C
{
    private int? Cache { get; set; }

    int Read() => Cache.HasValue ? Cache.Value : 0;
}
```

Before (`main`): presumably fix offered → `private int? _cache;` with references renamed. Not verified by a test; RH2001-004's coverage leaves it out.

After, A — withhold for every property whose type is a struct not declared `readonly`, or a type parameter not constrained to a reference type. `int?` and every user struct that is not `readonly` lose the fix, with or without a call that changes the value:
```csharp
class C
{
    private int? {|RH2001:Cache|} { get; set; } // reported, no code fix offered
}
```

After, B — withhold only when a reference to the property calls a non-`readonly` instance member on its value. This includes members a language construct calls, such as `GetEnumerator`, `GetAwaiter`, `Deconstruct`, and `Dispose`. The input above gets the fix:
```csharp
class C
{
    private int? _cache;

    int Read() => _cache.HasValue ? _cache.Value : 0;
}
```

After, C (recommended) — like A, but `Nullable<T>` of an enum or of a `readonly` struct counts as `readonly`. The input above gets the fix, as in B. A user struct that is not `readonly` gets no fix, as in A, even when nothing calls a member that changes it.

Why C: it keeps the fix for the common `int?` / `DateTime?` property, whose value no member can change in place. For other structs it uses one type-level check rather than B's usage analysis over every construct that calls a member implicitly. That analysis is easy to get wrong, and a wrong check gives an unsafe fix. This question does not block the run: RH2001-008 specifies the decided case, and only the boundary waits.

### Internal choices

- The `#if` instance gets no RH2001 requirement. SHARED-CASING-007 (already in `Uses`) and SHARED-FIX-003 decide it; RH2001-004 only points to them and carries the example.
- The comment instance is a rule requirement (RH2001-007), because the issue decides which positions withhold the fix. SHARED-FIX-003 alone would allow either keeping the comment or withholding the fix. Alternative: only a pointer in RH2001-004's coverage sentence.
- Comments in other positions inside the declaration stay outside RH2001-004's coverage. Examples are between modifiers, or between `}` and `=` of an initializer. The issue does not decide them.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1287

### Changed requirements

- RH2101-002 — A nested static class is not reported — clarified. A nested partial class is `static` when any part carries `static` (SHARED-DIAG-004), and then no part is reported. New example: `static partial class Helpers` plus `partial class Helpers` → not reported. The old coverage sentence (all parts static or none) is replaced. `Uses` gains SHARED-DIAG-004.
- RH7002-001 — Every top-level type after the first is reported — clarified. It now covers declarations with or without `partial`, except a further part of a type declared earlier in the same file.
- RH7002-004 — Further parts of one partial type are not reported — added. "Same type" means same name, same number of type parameters, same namespace. Examples on both sides: `partial class C` twice → nothing reported; `C` in another namespace and `C<T>` → their first parts are reported, and the further part of `C<T>` is not. `Uses` gains SHARED-DIAG-004.

Not written yet, waiting for output question 1:

- RH2002 — the partial async void method reported once.
- RH2004 — a partial property, indexer, event, or constructor reported once. Also Q5: the defining part of a partial constructor is judged.
- RH8306 — a partial property whose two parts both carry a sentence summary, reported once.
- RH7401 (and RH7402–RH7412 where the same double report occurs) — a partial member reported once.

That the RH2004 fix is withheld on a partial member needs no rule requirement: the first paragraph of SHARED-DIAG-004 states it, and so does its RH2004 example. Tests tag SHARED-DIAG-004. Once Q1 is answered, I would add these:

- a shared requirement under `shared/diagnostics.md` (next free ID SHARED-DIAG-012) stating where the one diagnostic stands;
- coverage changes in RH2002-001, RH2004-001 (drop "a constructor only when it is not `partial`"), RH8306-001 (drop "partial properties are not covered", for parts whose summaries both read as a sentence), and the RH7401–RH7412 "is not `partial`" clauses, limited to the double report;
- examples with the location marked.

### Examples for the tests

- RH2101-002 (second example).
- RH7002-004 (both examples).
- SHARED-DIAG-004's existing RH2004 partial-property example, for the withheld fix.

### Output questions

**1 · RH2002 / RH2004 / RH8306 / RH7401 · partial members — where the one diagnostic stands**

The maintainer decided P15: a partial declaration is reported once (SHARED-DIAG-004). Which part carries that diagnostic is not decided anywhere. Today these rules report every part. The location is visible output, and the tests need it. This blocks the remaining #1287 instances.

Input 1 — RH2002 · Async void methods should not be used:
```csharp
using System.Threading.Tasks;

partial class P
{
    partial void Q();

    async partial void Q()
    {
        await Task.Delay(1);
    }
}
```
Before (`main`): reported twice, on both `Q`.

Input 2 — RH2004 · Access modifier must be declared:
```csharp
public partial class P
{
    partial int Prop { get; set; }

    partial int Prop { get => 1; set { } }
}
```
Before (`main`): reported twice, and one fix application adds `private` to one part only (CS8799). The same happens for partial indexers and events. For a partial constructor, only the implementing part is reported today.

Input 3 — RH8306 · Property summary must use a noun phrase instead of a sentence: `public partial int P { get; }` and `public partial int P => 0;`, each with `/// <summary>Gets the value.</summary>`. Before (`main`): reported twice.

After, A (recommended) — on the first part, in document order, that the rule would report judged by what that part writes; when no part qualifies, on the first part:
```csharp
partial void Q();
async partial void {|RH2002:Q|}() { await Task.Delay(1); }   // 1: the part that writes async

partial int {|RH2004:Prop|} { get; set; }                    // 2: first part, no code fix offered
partial int Prop { get => 1; set { } }

/// {|RH8306:<summary>Gets the value.</summary>|}           // 3: first part whose summary is a sentence
public partial int P { get; }
/// <summary>Gets the value.</summary>
public partial int P => 0;
```

After, B — always on the first part in document order. 1 lands on the defining `partial void {|RH2002:Q|}();`, which does not write `async`. 2 and 3 are as in A.

After, C — always on the implementing part, the one with the body or accessor bodies. 1 is as in A. 2 lands on `partial int {|RH2004:Prop|} { get => 1; set { } }`. 3 lands on the summary of `public partial int P => 0;`.

Why A: the diagnostic lands where the code the rule complains about is written, such as `async` in input 1. #1252 Q2 objected that the defining part "carries nothing the user could change". A also matches the maintainer's P25 decision for RH7401 (#1334: "reported once, at the first part that does not" lie in a matching region). So RH7401's partial override with both parts in non-matching regions is reported on its first part. RH7002 needs no answer: under RH7002-004, a further part of a type is never reported.

### Internal choices

- RH7002: a `partial` type that follows another type is reported on its first part in the file, and further parts are not. I wrote this into RH7002-001 now and did not wait for Q1. It is the only reading compatible with the C1 decision ("the second part is not reported; both parts are one type") and with the rule reporting every type after the first. Alternative: leave partial declarations of other types unspecified.
- `SHARED-NESTED-002` (each part of a nested partial type is reported) is left as it stands. The issue's RH2101 instance concerns only `static`. RH2101-002 refers to SHARED-NESTED-002 without restating it. See the follow-up candidate.
- RH7104, RH7105, RH7106 are untouched (P28, issue comment). Which part's region decides for RH7401–RH7412 stays with #1334.

### Follow-up candidates

1. **RH2101 · Nested classes should not be used — non-static nested partial class is reported on each part.** Input:
   ```csharp
   class Outer
   {
       partial class Part
       {
       }

       partial class Part
       {
       }
   }
   ```
   Real output: `Part` is reported twice. SHARED-NESTED-002 and its example require this. Why it may look wrong: SHARED-DIAG-004 (P15) says a rule that judges what a declaration is reports it at most once. Its exclusions list names, parameters, members, and regions, but not where a part is declared. Requirements: SHARED-NESTED-002, SHARED-DIAG-004.
   - Reading A: both parts are reported, as the spec says today, because each part is a declaration to move.
   - Reading B: reported once (`partial class {|RH2101:Part|}` on one part only).

   Recommendation: A. Each part has to be moved on its own, like the per-part text of SHARED-DIAG-004's second paragraph. If so, SHARED-DIAG-004's exclusion list should name it. This concerns RH2104–RH2108 equally.
2. **RH8306 · Property summary must use a noun phrase instead of a sentence — partial property whose parts' summaries disagree.** Input: `/// <summary>Value</summary> public partial int P { get; }` and `/// <summary>Gets the value.</summary> public partial int P => 0;`. Real output: the second part is reported. Requirements: RH8306-001, SHARED-DIAG-004.
   - Reading A: reported, judged per part.
   - Reading B: judged only by the summary the compiler uses for the symbol.

   Recommendation: A. The rule reads the words written in each comment. This is outside #1287's instance, which has both summaries as sentences.

## thoenissen/Reihitsu#1288

### Changed requirements

- RH2004-003 — Static constructors and explicit interface implementations are not reported — clarified, per maintainer round 7b option A (#1254 Q2). The exemption now also covers explicit interface implementations of operators and conversion operators. New example: `static N IAdd<N>.operator +` and `static explicit IAdd<N>.operator int` → not reported. The fix withholding (#1254 Q3) is then moot, as the issue's comments say.
- RH2004-001 — A declaration without an accessibility modifier is reported — coverage sentence simplified. It drops the clause on explicit interface operators, which RH2004-003 now decides, and keeps the partial-constructor clause (see #1287).

### Examples for the tests

RH2004-003. The characterization tests in `RH2004AccessModifierMustBeDeclaredAnalyzerTests` that pin today's report and fix on `IAdd<N>.operator +` turn into "not reported" tests.

### Output questions

_None._

### Internal choices

- `documentation/rules/RH2004.md` is unchanged. Its description already limits the rule to "every type and member that supports accessibility", and an explicit implementation does not support one.
- The ordinary-operator `public` part (#1254 Q1) is moved to #1435 and is not written here. RH2004-006's coverage still excludes operators.

### Follow-up candidates

_None._

## Whole run

- Files: `sdd/specs/analyzer/RH2001.md`, `RH2004.md`, `RH2005.md`, `RH2101.md`, `RH7002.md`, `sdd/specs/requirement-ids.txt` (+RH2001-007, RH2001-008, RH2005-004, RH7002-004). `sdd/specs/README.md` needed no change; no code owners changed.
- `scripts/test.sh --project architecture` filtered to the three spec test classes: 50 passed, 1 failed (coverage of the four new IDs, expected until tests exist).
- No rule page changed.
