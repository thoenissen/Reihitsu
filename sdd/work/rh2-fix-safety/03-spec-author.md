Role: spec author
Commit: 2543b5d
Verdict: #967: spec written; #1286: spec written; #1287: spec written; #1288: spec written (unchanged since 01)

Amendment round. The specs now carry the maintainer's answers Q1 = A, Q2 = C, and Q3 = B, and `00-lead.md` decisions 4, 6, 7, and 8. Decision 5 is a code-structure decision, and no spec names where the excluded-`#if` check lives, so nothing changed for it. Everything is a clarification in the bug-fix lane, with no change doc. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore.

I compiled every new C# example in a scratch net10.0 / C# 14 project with the markup stripped. All of them build. The only warnings were unused-event and unassigned-field warnings in the RH74xx and RH2001 fixtures.

`SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails, as expected, on five IDs that have no tagged test yet: RH2001-007, RH2001-008, RH2005-004, RH7002-004, and SHARED-DIAG-012.

## thoenissen/Reihitsu#967

### Changed requirements

- RH2005-003 — The fix replaces `public` with `private` — clarified (decision 4). The normative text now says `private` stands where the first token after the attribute lists stood: on that token's line, in its column, and after every comment and directive line in front of it. Changes to the examples:
  - The `static public` example without attributes stays. It passes on `main` and pins the behavior.
  - New reproduction example: `[Obsolete]` on its own line, then `static public int Counter;`, becomes `private static int Counter;` on the field's line.
  - New example with a `// note` line and a `#pragma warning disable CS0414` line between the attribute list and the field. Both lines stay in front of `private`.
- RH2005-004 — The fix is withheld for a field used outside its type — changed in wording:
  - "any field the declaration declares" (decision 6).
  - Q3 = B: every reference outside the bodies counts, including one that would still compile, such as `nameof` in the type's own attribute or a `cref` in another type's documentation comment.
  - The Why now gives the reason for one rule for every reference, not CS0122 alone.
  - New examples: `public int A, B;` with only `B` referenced outside, and the Q3 input. Both are reported with no code fix.
- RH2004-006 — The fix adds the default accessibility — clarified with a new example: `[Serializable]` then `class D` gives `internal class D`, and `[Obsolete]`, `// note`, `#pragma`, then `static void Run()` gives `private static void Run()`. Comment and directive lines stay in front. Also covered by decision 4.

### Examples for the tests

- RH2005-003: all three examples. The two attribute examples are the reproductions.
- RH2005-004: all five examples. Split the Q3 example into two tests, one for `nameof` in the attribute and one for the `cref`. Each reference alone withholds the fix.
- RH2004-006: the existing `[Obsolete]`/`void M()` example and the new type plus comment/directive example. Testing the comment and the `#pragma` separately is worthwhile (FC-005).

### Output questions

_None._

### Internal choices

- Rule pages `RH2005.md` and `RH2004.md` are unchanged. A withheld fix is covered by `general-notes.md`.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1286

### Changed requirements

- RH2001-004 — The fix replaces the property with a private field — coverage clarified. It now also covers a `Nullable<T>` whose `T` is an enum or a struct declared `readonly`. The wording is "struct declared `readonly`", with a pointer to RH2001-008 for what counts. New example: `private int? Cache { get; set; }` on .NET 9 becomes `private int? _cache;` with the references renamed.
- RH2001-007 — now "The fix is withheld for a comment or directive inside the property" — widened (decision 7). It covers a comment or directive between the name and the end of the declaration, except inside the initializer. That means between the name and `{`, inside the accessor list, or between `}` and the initializer's `=`. A comment or directive in front of the name, or after the initializer's `=`, is outside this requirement. Four examples: the issue's input, `} /* init */ = 5;`, a comment line in a multi-line accessor list, and `#if DEBUG set; #endif` in the accessor list.
- RH2001-008 — now "The fix is withheld for a settable property of a struct type not declared `readonly`" — changed per Q2 = C:
  - The fix is withheld for every property with a `set` accessor whose type is a struct not declared `readonly`, whether or not a use changes the value.
  - "Declared `readonly`" is read from source or from the referenced assembly's metadata, so the result depends on the target framework. The maintainer accepted that `int` and `bool` lose the fix on netstandard2.0 and .NET Framework.
  - `Nullable<T>` counts as `readonly` when `T` is an enum or a `readonly` struct.
  - Examples: the `Counter.Increment()` input, a `Point` and a `Point?` with no changing use, and a netstandard2.0 `int` and `int?`. All are reported with no fix.

### Examples for the tests

- RH2001-004: the `int?` example. Analyzer tests reference .NET 9. Confirm that `Int32` is declared `readonly` there. The skeptic verified net10.0 only.
- RH2001-007: all four examples, with the directive tested separately from the comments.
- RH2001-008: all three examples. The netstandard2.0 example needs `ReferenceAssemblies.NetStandard.NetStandard20`, or the equivalent, for that one test.

### Output questions

**RH2001 · Private auto-implemented properties should not be used — a get-only property of a struct not declared `readonly`, and a property typed by a type parameter**

This question does not block the run. Q2 as recorded says "a *settable* property whose type is a struct". Two neighbouring cases are left out of RH2001-004's coverage and stay unspecified until this is answered:

- **A get-only property.** It becomes a `readonly` field. Outside a constructor, a call on a `readonly` field runs on a copy, as through the property. Inside a constructor, it runs on the field.
- **A property typed by a type parameter** that is not constrained to a reference type. Its type argument may be a mutable struct.

Input:
```csharp
struct Counter
{
    private int _count;

    public void Increment() => _count++;
}

class C<T>
{
    private Counter Current { get; }

    private T Item { get; set; }

    C()
    {
        Current.Increment();
    }
}
```

Before (`main`): fix offered for both: `private readonly Counter _current;` and `private T _item;`. Inside the constructor, `_current.Increment()` now changes the field.

After, A (recommended): reported, no code fix offered for either property. The withholding then applies to every property whose type is a struct not declared `readonly`, or a type parameter not constrained to a reference type, with or without a setter.

After, B: fix offered for both, as on `main`.

Why A: it is one type check with no setter or constructor case to tell apart. This is the "keep it simple" reading of the option the maintainer chose.

### Internal choices

- RH2001-008 keeps its ID and is rewritten, not retired. No test was tagged with its narrower text yet. Alternative: retire it and add a new ID.
- RH2001-008 calls an enum "not a struct in the sense of this requirement", so enums keep the fix under RH2001-004.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1287

### Changed requirements

- SHARED-DIAG-012 — Where the one diagnostic of a partial member stands — added (Q1 = A). It applies to a rule that reports a partial method, property, indexer, event, or constructor at most once. The diagnostic stands at the rule's usual position, on the first part found by these steps:
  1. Generated parts are never chosen. When every part is generated, nothing is reported.
  2. Otherwise, a part the rule would report when judging that part alone by what it writes, the defining part before the implementing part.
  3. When no part qualifies, the defining part, or the implementing part when the defining part is generated.

  File order and position do not matter. "Defining part" is defined. Examples for RH2002, RH2004 (implementing part first in the file), and RH8306 (only the implementing part's summary is a sentence, which folds in the earlier candidate 2).
- RH2002-001 — coverage clarified. A partial method is judged as one declaration and reported once, on the implementing part, or on the defining part when the implementing part is generated. The old exclusion "nor a part of a partial method" is removed. Example: the issue's input. Uses: SHARED-DIAG-004, SHARED-DIAG-012.
- RH2004-001 — coverage clarified (decision 8; this replaces the issue's stale "RH2004-002 is updated" goal). "A constructor only when it is not `partial`" is replaced: a partial property, indexer, event, or constructor is reported once, on the part SHARED-DIAG-012 selects, with no code fix (SHARED-DIAG-004). Example: all four kinds, each reported on its defining part, with the property's implementing part placed first. The example also covers the partial constructor case from Q5. Uses: SHARED-DIAG-004, SHARED-DIAG-012.
- RH8306-001 — coverage clarified. Partial properties are now covered: each part's summary is checked, and the property is reported once on the part SHARED-DIAG-012 selects. Example: both parts carry a sentence summary, and the defining part is reported. Indexers stay out of coverage. Uses: SHARED-DIAG-012.
- RH7401-001 to RH7412-001 — coverage clarified. The "is not `partial`" clause is removed. A partial member is one member and is reported at most once, on the part SHARED-DIAG-012 selects. Coverage is limited to a partial member whose parts would all be reported. Whether one is reported when only some parts would be is "not covered", because #1334 (P25) owns that. Each spec gets one example of its own member kind:
  - RH7401: the issue's two-region input.
  - RH7402–RH7404 and RH7409–RH7412: both parts in a non-matching region.
  - RH7405–RH7408: a member that overrides nothing, with both parts in an override region.

  Uses: SHARED-DIAG-004, SHARED-DIAG-012.
- From 01, still standing: RH2101-002 and RH7002-001 (clarified), and RH7002-004 (added).
- `sdd/specs/requirement-ids.txt`: +SHARED-DIAG-012.

### Examples for the tests

- SHARED-DIAG-012: its three examples.
- RH2002-001: the partial example. It replaces the characterization test `VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations`.
- RH2004-001: the partial example. One test per member kind is advisable. It replaces the characterization test near `RH2004AccessModifierMustBeDeclaredAnalyzerTests.cs:881`. The withheld fix tags SHARED-DIAG-004.
- RH8306-001: the partial example.
- RH7401-001 to RH7412-001: one partial example each. RH7401 replaces `VerifyPartialOverrideMethodIsReportedOncePerPart`.
- Generated-part cases: the defining part generated, the implementing part generated, and both generated. These need multi-document tests and are tagged SHARED-DIAG-012.
- Cross-file case: the parts in two documents. The choice must not depend on document order.
- From 01: RH2101-002 and RH7002-004.

### Output questions

_None._ Q1 is answered.

### Internal choices

- The location rule is one shared requirement, SHARED-DIAG-012. The rule specs only point to it and carry their own example. Alternative: state the order in each rule spec.
- SHARED-DIAG-012 step 3 applies to RH2002 when the implementing part is generated: the user's defining part `partial void Q();` then carries the diagnostic. That is the literal reading of Q1's "when no part qualifies by what it writes, the defining part". Alternative: report nothing.
- For RH7401–RH7412, the case where only some parts lie outside a matching region is left out of coverage, not specified, because #1334 decides it. Today it reports per part, and the swapped placement in RH7409 reports nothing.

### Follow-up candidates

1. SHARED-DIAG-004 wording. This extends the existing candidate in `00-lead.md`:
   - its exclusion list should name SHARED-NESTED-002's per-part reports;
   - its second paragraph should not appear to cover RH8306's summaries;
   - its last paragraph ("the members and regions that one part holds") should not read as covering the region a part of a partial *member* lies in, which RH7401–RH7412 now judge as one member under SHARED-DIAG-004.

   Wording only. No visible output differs.

## thoenissen/Reihitsu#1288

### Changed requirements

None in this round. RH2004-003 and the RH2004-001 coverage from 01 stand. The RH2004-001 coverage sentence was rewritten for #1287 and no longer mentions explicit interface operators.

### Examples for the tests

RH2004-003 (from 01).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- Files:
  - `sdd/specs/analyzer/RH2001.md`, `RH2002.md`, `RH2004.md`, `RH2005.md`, `RH8306.md`, `RH7401.md` to `RH7412.md`
  - `sdd/specs/shared/diagnostics.md`
  - `sdd/specs/requirement-ids.txt`
- `sdd/specs/README.md`: no change, because no code owners changed.
- Rule pages: none changed. The report changes only in corner cases: a partial member reported once, and fixes withheld.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 50 passed, 1 failed (the coverage of the five untested IDs listed above).

## Addendum — lead decision 10 (RH2001 boundary)

The lead decided the open RH2001 question as A, and corrected answer 2C in `00-lead.md`: it has no "settable" restriction.

- **RH2001-008:** retitled "The fix is withheld for a property of a struct type not declared `readonly`" and changed. The fix is now withheld for a property whose type is a struct not declared `readonly`, or a type parameter not constrained to a reference type. This holds with or without a `set` accessor. The Why now covers the constructor case of a get-only property and the type-parameter case. Two new examples:
  - a get-only `Counter Current { get; }` changed in a constructor: no fix;
  - `class C<T, TRef> where TRef : class`: `T Item` gets no fix, and `TRef Reference` becomes `_reference`.
- **RH2001-004:** its coverage now names "a type parameter constrained to a reference type" next to reference types.
- **Examples for the tests:** the two new RH2001-008 examples. They compile in a scratch net10.0 project.
- **Spec tests:** `SpecConsistencyTests` and `SpecStructureTests` pass. `RequirementCoverageTests` fails on the same five untested IDs as before.
- **Output questions:** _None_ remain.
