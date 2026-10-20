Role: spec author
Commit: 1c7aff4 (repair round after `02-skeptic.md`; first round derived from 7436a13ece40f69a9c61179e6a89a935f2358015)
Verdict: spec written

The prompts carried no intended fix, suspected cause, or hoped-for verdict. In the repair round, the coordinator relayed the skeptic's findings S1–S7. I re-derived each finding from the code and from fresh runs (below) instead of copying the suggested text. No change doc: every requirement writes down current behavior (lane: specs and tests for existing behavior). No rule page under `documentation/rules/` was wrong, so none was changed.

## Changed requirements

`sdd/specs/shared/member-region-grouping.md` (code owners `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`, `OverrideMemberUtilities`, `InterfaceImplementationUtilities`; uses SHARED-REGDESC-001, SHARED-TYPEREGION-006):

- SHARED-REGGROUP-001 — Class, struct, and record declarations are checked — added
- SHARED-REGGROUP-002 — A region name matches the type's simple name — added
- SHARED-REGGROUP-003 — An override comes from the type that introduced the member — added
- SHARED-REGGROUP-004 — An implementation comes from the interface that declares the member — added
- SHARED-REGGROUP-005 — The diagnostic is on the member's name — added
- SHARED-REGGROUP-006 — An override region — added

The first round had seven IDs. Its SHARED-REGGROUP-002 ("The region a member lies in") restated SHARED-TYPEREGION-006 (S1), so I dropped it and renumbered the rest of this batch's own IDs, none of which have reached `main`. The ledger holds SHARED-REGGROUP-001 to -006.

Analyzer specs, two requirements each (`-001` what is reported, `-002` what is not):

- RH7401–RH7404 (override members) — added. They use SHARED-REGGROUP-001, -002, -003, -005 and SHARED-TYPEREGION-006.
- RH7405–RH7408 (non-override members in override regions) — added. They use SHARED-REGGROUP-001, -005, -006 and SHARED-TYPEREGION-006.
- RH7409–RH7412 (interface implementations) — added. They use SHARED-REGGROUP-001, -002, -004, -005 and SHARED-TYPEREGION-006.

`sdd/specs/analyzer/RH7501.md` (uses FMT-SWITCH-002):

- RH7501-001 — A `break` that ends a section's block is reported — added
- RH7501-002 — Other statements that end a section's block are not reported — added (now includes `yield break`, with an example)
- RH7501-003 — The diagnostic is on the `break` keyword — added
- RH7501-004 — The fix moves the `break` behind the block — added

All 34 IDs are in `sdd/specs/requirement-ids.txt`, and all 14 specs are in `sdd/specs/README.md`.

### What the repair round changed, and why

- **S1.** I dropped the duplicate requirement. Each rule's `-001` now says "lies in a top-level region (SHARED-TYPEREGION-006)". It also scopes itself to members that lie in no region nested in a top-level region, as RH7308-001 does. A member in a nested region is a P17 instance (#1264), so it stays out of the RH74xx text. The non-override `-002` requirements carry the same scope.
- **S2.** SHARED-REGGROUP-005 now covers a field-like event with one declarator only. RH7403, RH7407, and RH7411 `-001` say "a field-like event with one declarator". RH7407 says the same for the overrides that make a region an override region. The count and location for several declarators were added to Q3 (P20).
- **S3/S4.** RH7501-004 now holds only when all of these are true:
  - the block holds a statement in front of the `break`, with no blank line between them;
  - the `break` is the first statement on its line;
  - the closing brace is the first token on its line;
  - no directive and no excluded code lies between the statement in front of the `break` and the closing brace.

  I checked the edges with `apply-fix.sh`, and the fix is correct for all of them: `/* c */ break;` on a line of its own, `} // end`, `case 2: {` with the brace on the label line, and a `//` comment on its own line between the `break` and `}`. The wrong outputs are Q7 and Q8. The blank-line exclusion keeps Q10 out of the spec.
- **S5 and S7** are new questions Q4 and Q5. SHARED-REGGROUP-006's negative sentence now reads "matches no type that an override in any declaration of the type comes from", so the partial case stays undecided in the text. SHARED-REGGROUP-002 now covers only type names that do not start with a character that changes under invariant upper-casing.
- **While re-deriving the skeptic's FC-024 and FC-023 candidates** I ran them:
  - A partial override is reported once for each part. A partial interface implementation is judged only at its defining part. Both are now Q6. Every `-001` now says "is not `partial`" (RH7405–RH7408: "and is not `partial`").
  - `class @Base` is matched by `#region Base`, and `#region @Base` does not match. This agrees with P3 (a name is compared by its value), so SHARED-REGGROUP-002 now states it with an example.
  - Two cases match the spec as written and need no change. A user-written `Equals(R? other)` in a record is reported by RH7409 with `IEquatable`. An operator that implements a `static abstract` interface operator is not reported, because it is not a method declaration (RH7409-002).
- **S6.** The code-owner claim on `InterfaceImplementationUtilities` is kept. The coordinator relays the coupling to RH8201 and RH8203 to the maintainer.

## How the statements were established

- I extracted every example block of the 14 specs (49 blocks) and ran it through the RH74xx analyzers and RH7501 with a scratch harness outside the repository. The harness compiles each block with Roslyn and references `Reihitsu.Analyzer` through `#:project`. Each block compiles without errors, and each rule's diagnostics equal the block's `{|RH####:…|}` markup exactly, position and text. Blocks marked "is not reported" produce no diagnostic of the named rule.
- `scripts/apply-fix.sh RH7501` on the RH7501-004 before-block gives `fixed` under LF and CRLF, and the output equals the after-block line for line.
- `scripts/test.sh --project architecture --filter` on `SpecConsistencyTests|SpecStructureTests|RequirementCoverageTests|RuleDocumentation…`: 38 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, and it lists exactly the 34 new IDs, which carry no tags yet.

## Examples for the tests

SHARED-REGGROUP-001 to SHARED-REGGROUP-006; RH7401-001 to RH7412-002 (each `-001` and `-002`); RH7501-001 to RH7501-004. Existing tests already cover most `-001` cases and several `-002` cases. RH7501's Fix All is shared behavior (SHARED-FIX-002) and needs no rule requirement. Inputs that the specs leave out (the questions below) are characterization tests only, with no requirement tag.

## Open questions to file

Every question below stands on its own and can be filed verbatim. The two issues do not exist yet. #912 is at its sub-issue limit, so list them in a comment there.

### Issue `SDD open questions: sdd/specs/shared/member-region-grouping.md`

**Q1 — Whether a region is an override region depends on the member kind of the overrides.** Not decided by a principle.

```csharp
class B
{
    public virtual int P => 0;
}

class C : B
{
    #region B

    public override int P => 1;

    public void N()
    {
    }

    #endregion
}
```

Real output: nothing is reported. RH7405 counts only override methods when it decides which regions are override regions, and RH7406 checks only properties. Add any override method of `B` to `C`, and RH7405 reports `N`.

Why it looks wrong: the rule pages of RH7405–RH7408 explain override regions as the place for "inherited behavior by the type that originally introduced it". Here the verdict on `N` depends on whether an unrelated override method exists, not on the region `N` lies in.

Readings:
- (A) A region is an override region when any override of the type, of any member kind, comes from the region's type, so `N` is reported. This also changes the RH7405–RH7408 pages, which describe (B).
- (B) Only overrides of the rule's own member kind count (today).

Touches SHARED-REGGROUP-006, RH7405-001 to RH7408-001, and RH7405-002 to RH7408-002.

**Q2 — A member that implements members of two interfaces is judged against only one of them, chosen by the order of the base list.** Not decided by a principle.

```csharp
interface IA
{
    void M();
}

interface IB
{
    void M();
}

class C : IA, IB
{
    #region IB

    public void M()
    {
    }

    #endregion
}
```

Real output: RH7409 reports `M` and asks for the region `IA`. With `class C : IB, IA` and the same body, nothing is reported.

Why it looks wrong: `M` implements `IB.M` as much as `IA.M`, yet the verdict changes with the order of the base list, not with the code.

Readings:
- (A) A region named after any interface whose member the method implements is accepted, so nothing is reported in either order.
- (B) Only the first interface in base-list order is accepted (today).

Touches SHARED-REGGROUP-004 and RH7409-001 to RH7412-001.

**Q3 — A field-like event with several declarators is judged and reported only by its first declarator.** Decided by principle P20 (#912). It violates SHARED-DIAG-006, which #1260 adds. Until that is merged, the case stays out of the specs.

Input 1 (RH7403):

```csharp
using System;

class B1
{
    public virtual event EventHandler A;
}

class B2 : B1
{
    public virtual event EventHandler X;
}

class D : B2
{
    #region B1

    public override event EventHandler A, X;

    #endregion
}
```

Real output: nothing is reported. `X` comes from `B2` but lies in the region `B1`.

Input 2 (RH7411): `interface IA { event EventHandler A; }`, `interface IB { event EventHandler X; }`, and `class C : IA, IB { #region IA  public event EventHandler A, X;  #endregion }`. Real output: nothing is reported, although `X` comes from `IB`.

Input 3 (RH7407): `B1` and `B2` as in input 1, and `class D : B2 { public override event EventHandler A, X;  #region B2  public event EventHandler F;  #endregion }`. Real output: `F` is not reported. Only `A` names an override region (`B1`), although `X` comes from `B2`.

Input 4 (RH7407, diagnostic count): an override region `B` (from `public override event EventHandler E;`) that also holds `public event EventHandler F, G;`. Real output: one diagnostic, at `F`.

Input 5 (RH7411, diagnostic count): `class C : IA { public event EventHandler A, X; }` with both events declared by `IA` and no region. Real output: one diagnostic, at `A`.

Expected under P20: every declarator is judged. The number and location of the diagnostics for one declaration with several declarators follow SHARED-DIAG-006.

Touches SHARED-REGGROUP-005, SHARED-REGGROUP-006, RH7403-001, RH7407-001, and RH7411-001.

**Q4 — An override declared in another part of a partial type does not make a region an override region.** Not decided by a principle. P15 ("a partial declaration is one declaration", #1260) is close, but it judges the partial element itself, and `N` below is not partial.

```csharp
class B
{
    public virtual void M()
    {
    }
}

partial class C : B
{
    public override void M()
    {
    }
}

partial class C
{
    #region B

    public void N()
    {
    }

    #endregion
}
```

Real output: RH7405 does not report `N`. It does report `N` when `M` is declared in the same part as the region.

Why it looks wrong: the verdict on `N` depends on which file holds the override, not on the region `N` lies in. This is the same objection as Q1.

Readings:
- (A) A region is an override region when any declaration of the type declares an override that comes from the region's type, so `N` is reported.
- (B) Only the analyzed declaration counts (today).

Touches SHARED-REGGROUP-001, SHARED-REGGROUP-006, RH7405-001 to RH7408-001, and RH7405-002 to RH7408-002.

**Q5 — No region name satisfies both these rules and the region-name rules for a type whose name starts with a lower-case letter.** P4 (analyzer/formatter disagreement), decided case by case.

```csharp
class myBase
{
    public virtual void M()
    {
    }
}

class C : myBase
{
    #region myBase

    public override void M()
    {
    }

    #endregion
}
```

Real output: RH7401 accepts the region, but RH7302 reports `#region myBase`. The formatter (FMT-REGION-001) rewrites it to `#region MyBase` and `#endregion // MyBase`, and then RH7401 reports `M`, because the comparison is ordinal.

Why it looks wrong: no layout is both formatter-stable and clean for RH7302 and RH7401. Names like this arise only for types that the naming rules do not report, such as types from metadata or with casing rules suppressed, so the severity is low.

Readings:
- (A) Keep the exact ordinal match (today).
- (B) Match the name after FMT-REGION-001's capitalization, so `#region MyBase` is accepted and nothing is reported after formatting.

Touches SHARED-REGGROUP-002, RH7401-001 to RH7404-001, RH7409-001 to RH7412-001, and, through the override region's name, RH7405-001 to RH7408-001.

**Q6 — A partial member is judged once per part, or only at one part.** The double report is decided by principle P15 (#912), SHARED-DIAG-004, which #1260 adds. Which part's region decides is not decided by a principle.

Input 1 (RH7401):

```csharp
class B
{
    public virtual void M()
    {
    }
}

partial class C : B
{
    #region Methods

    public override partial void M();

    #endregion

    #region Other

    public override partial void M()
    {
    }

    #endregion
}
```

Real output: RH7401 reports `M` twice, once for each part. P15 says one member is reported once.

Input 2 (RH7409, RH7410):

```csharp
interface I
{
    void M();

    int P { get; }
}

partial class C : I
{
    #region Methods

    public partial void M();

    public partial int P { get; }

    #endregion

    #region I

    public partial void M()
    {
    }

    public partial int P { get => 0; }

    #endregion
}
```

Real output: `M` and `P` are reported once each, at the defining parts in `Methods`. The implementing parts in `I` are not judged. Moving the defining parts into `I` and the implementing parts into `Methods` reports nothing.

Why it looks wrong: one member's verdict depends on which of its two parts lies in the matching region.

Readings:
- (A) Both parts have to lie in a matching region, and the member is reported once.
- (B) The defining part decides (today, for implementations).
- (C) The implementing part decides.

Touches SHARED-REGGROUP-003, SHARED-REGGROUP-004, SHARED-REGGROUP-005, RH7401-001 to RH7412-001, and RH7405-002 to RH7408-002.

### Issue `SDD open questions: sdd/specs/analyzer/RH7501.md`

**Q7 — The fix moves directives and excluded code across the block's closing brace.** Inputs 1 to 3 are decided by principle P1 (#912) and violate SHARED-FIX-003. Input 4 relocates a directive. AGENTS.md (Code-fix coverage) says such a rewrite withholds itself, but P1's four conditions do not name relocation, so the maintainer confirms.

Input 1 (DEBUG undefined):

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    var y = 1;
#if DEBUG
                    A(y);
#endif
                    break;
                }
        }
    }

    void A(int y)
    {
    }
}
```

Real output: the fix is offered. It writes `}`, then `#if DEBUG` / `A(y);` / `#endif` / `break;` behind the block, so a DEBUG build fails with CS0103, because `y` is out of scope.

Input 2: block `{ A(); #region Exit  break;  #endregion }`, each on its own line. Real output (`apply-fix.sh`: `invalid-result`): `#endregion` stays inside the block, and `#region Exit` and `break;` land behind it, so the result does not compile.

Input 3: block `{ A(); break;  #if DEBUG  A();  #endif }`, each on its own line. Real output (`fixed`): the `#if` branch stays inside and `break;` moves behind the `}`. In a DEBUG build, `A()` now runs a second time, a run-time change.

Input 4: `#pragma warning disable CS0162` in front of `break;` and `#pragma warning restore CS0162` behind it, inside the block. Real output: the `disable` moves behind the block and the `restore` stays inside, so the restore now comes first and the suppression runs to the end of the file. The moved `break` keeps its old indentation (20 columns instead of the brace's 16).

Touches RH7501-004 and SHARED-FIX-003.

**Q8 — The fix breaks or rearranges code when the `break` or the closing brace shares its line.** Input 1 is decided by principle P1 (#912) and violates SHARED-FIX-003. Inputs 2 and 3 are decided by principle P9 (#912), SHARED-FIX-006, which #1138 adds.

Input 1:

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1: { A(); break; }
            default: { A(); break; }
        }
    }

    void A()
    {
    }
}
```

Real output (`apply-fix.sh`: `invalid-result`): the text in front of the brace on its line (`case 1: { A(); break; `) is written in as the `break`'s indentation, so the result does not parse.

Input 2: a block that ends with `                    break; }` (the `break` and the brace on one line). Real output: the result holds a second `break;` the user did not write. The block's `}` moves to column 0, and the switch's `}` joins the `break` line (`break; break;         }`).

Input 3: a block whose last line is `                    A(); break;`, with `}` on the next line. The same happens with `A(); /* first */ break;`. Real output: the block's `}` is pulled up onto `A()`'s line (`A();                 }`), and `break;` follows on its own line.

Touches RH7501-004.

**Q9 — The fix leaves an empty block.** P4 (analyzer/formatter disagreement), decided case by case.

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    break;
                }
        }
    }
}
```

Real output: RH7501 reports the `break`, and the fix produces `case 1:` / `{` / `}` / `break;`.

Why it looks wrong: FMT-SWITCH-006 calls this shape noise ("`case 3: { } break;` would add noise without structure"), and the formatter never produces it.

Readings:
- (A) Report and fix as today, leaving the empty block.
- (B) Report, and have the fix remove the block that becomes empty, giving `case 1: break;`.
- (C) Do not report a `break` that is its block's only statement.

Touches RH7501-001 and RH7501-004.

**Q10 — The blank line in front of the `break` moves with it.** My view: P14a (#1138: "a fix MUST NOT insert a blank line before the node it changes, and MUST NOT change the blank lines between that node and its neighbours") supports today's output. The fix inserts no blank line, and it keeps the one blank line between the `break` and the node in front of it. P10 agrees, because `scripts/format.sh` keeps that blank line. But P14a does not decide the case literally: the neighbour in front of the `break` changes from the last statement to the closing brace, and the result differs from the FMT-SWITCH-002 shape. So file it for confirmation, marked "supported by P14a (#1138)", not as a bug.

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    A();

                    break;
                }
        }
    }

    void A()
    {
    }
}
```

Real output: `{` / `A();` / `}`, a blank line, then `break;`. The blank line moves with the `break`. Input like this is common: RH5010 asks for a blank line in front of a `break` inside a block (SHARED-BLANK-004).

Readings:
- (A) The blank line moves with the `break` (today).
- (B) The `break` lands directly under `}`, in the FMT-SWITCH-002 shape.

Touches RH7501-004.

### Principle instances for comments on existing issues (not new questions)

- **#1264 (P17, the innermost region judges a member).** The RH74xx rules judge a member by the top-level region around it, also when it lies in a nested region. In the input below, RH7401 reports `N`, not `M`. Under P17, `M` (innermost region `Methods`) would be reported and `N` (innermost region `B`) would not. RH7405 behaves the same way: a helper in `#region B` > `#region Helpers` is reported, while one in `#region Helpers2` > `#region B` is not. The RH74xx specs leave members in nested regions out.

  ```csharp
  class B { public virtual void M() { } public virtual void N() { } }
  class D : B
  {
      #region B
      #region Methods
      public override void M() { }
      #endregion
      #endregion
      #region Overrides
      #region B
      public override void N() { }
      #endregion
      #endregion
  }
  ```

- **#1240 (region directives in inactive `#if` branches).** No input changes an RH74xx verdict. A pair that crosses an excluded branch does not compile (CS1038, CS1027), and a pair inside an excluded branch encloses no member. RH7501 reads no region directives.

## New-rule candidates (P11)

- A member that implements no interface member, placed in a top-level region named after an interface the type implements, such as a helper method in `#region IDisposable`. This is the interface-region counterpart of RH7405–RH7408. No rule reports it today.

## Follow-up candidates

_None._ The ownership coupling (S6: `InterfaceImplementationUtilities` also decides RH8201's and RH8203's "implements an interface member") goes to the maintainer through the coordinator.
