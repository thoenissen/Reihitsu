Role: spec author
Commit: 7436a13ece40f69a9c61179e6a89a935f2358015
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. No change doc: every requirement writes down current behavior (lane: specs and tests for existing behavior). No rule page under `documentation/rules/` was wrong, so none was changed.

## Changed requirements

New spec `sdd/specs/shared/member-region-grouping.md` (code owners `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase`, `OverrideMemberUtilities`, `InterfaceImplementationUtilities`; uses SHARED-REGDESC-001, SHARED-TYPEREGION-006):

- SHARED-REGGROUP-001 — Class, struct, and record declarations are checked — added
- SHARED-REGGROUP-002 — The region a member lies in — added
- SHARED-REGGROUP-003 — A region name matches the type's simple name — added
- SHARED-REGGROUP-004 — An override comes from the type that introduced the member — added
- SHARED-REGGROUP-005 — An implementation comes from the interface that declares the member — added
- SHARED-REGGROUP-006 — The diagnostic is on the member's name — added
- SHARED-REGGROUP-007 — An override region — added

New analyzer specs, two requirements each (`-001` what is reported, `-002` what is not):

- RH7401, RH7402, RH7403, RH7404 (override members; use SHARED-REGGROUP-001 to -004 and -006) — added
- RH7405, RH7406, RH7407, RH7408 (non-override members in override regions; use SHARED-REGGROUP-001, -002, -006, -007) — added
- RH7409, RH7410, RH7411, RH7412 (interface implementations; use SHARED-REGGROUP-001, -002, -003, -005, -006) — added

New analyzer spec `sdd/specs/analyzer/RH7501.md` (uses FMT-SWITCH-002):

- RH7501-001 — A `break` that ends a section's block is reported — added
- RH7501-002 — Other statements that end a section's block are not reported — added
- RH7501-003 — The diagnostic is on the `break` keyword — added
- RH7501-004 — The fix moves the `break` behind the block — added

All 35 IDs are in `sdd/specs/requirement-ids.txt`, all 14 specs in `sdd/specs/README.md`.

## How the statements were established

- Every example of the 14 specs (49 C# blocks) was run through every RH74xx analyzer and RH7501 with a scratch harness outside the repository (Roslyn compilation over each block, `#:project` reference to `Reihitsu.Analyzer`): each block compiles without errors, and each rule's diagnostics equal the block's `{|RH####:…|}` markup exactly, position and text. Other rules of the family also fire in a few blocks (for example RH7401 on the override outside a region in the SHARED-REGGROUP-007 example); the markup marks only the rule the requirement is about.
- The RH7501-004 example was run with `scripts/apply-fix.sh RH7501`: `fixed`, LF and CRLF, output identical to the after-block.
- `scripts/test.sh --project architecture --filter` on `SpecConsistencyTests|SpecStructureTests|RequirementCoverageTests|RuleDocumentation…`: 38 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, listing exactly the 35 new IDs (no tags yet).
- Code facts the requirements rest on: both bases register on class, struct, record, and record struct declarations (nested included, interfaces not) and read the top-level regions of the declaration; the region name is compared ordinally with the simple type name; override names come from the start of the override chain; interface names from the explicit qualifier or the declaring interface, overrides excluded.
- `InterfaceImplementationUtilities.GetImplementedInterfaceName` is also called by `DocumentationAnalysisUtilities`, only to test whether a member implements any interface member. The lead's clarification allows claiming the type; the documentation specs do not reference SHARED-REGGROUP IDs.

Deliberately left out of the requirements (they are the open questions and principle instances below): a member in a region nested in a top-level region; a member that implements members of two interfaces; a field-like event with several declarators; a region whose base type only an override of another member kind comes from; and, for RH7501, a closing brace that is not first on its line, a `break` that is its block's only statement, and directives in front of the `break`. RH7501-004 states its two conditions (another statement in front of the `break`, closing brace first on its line) for that reason.

## Examples for the tests

SHARED-REGGROUP-001 to SHARED-REGGROUP-007, RH7401-001 to RH7412-002 (each `-001` and `-002`), RH7501-001 to RH7501-004. Existing tests already cover most `-001` cases and several `-002` cases; RH7501's Fix All is shared behavior (SHARED-FIX-002) and needs no rule requirement.

## Open questions to file

New issue needed: `SDD open questions: sdd/specs/shared/member-region-grouping.md` (sub-issue of #912, which is at its sub-issue limit — list it there instead).

**Q1 — An override region depends on the member kind of the overrides.** Not decided by a principle.

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

Real output: nothing is reported. RH7405 counts only override methods when deciding which regions are override regions, and RH7406 checks only properties. Add `public override string ToString() => "";` from `Object` and nothing changes. Add any override method of `B` and RH7405 reports `N`. Why it looks wrong: every rule page (RH7405–RH7408) explains override regions as the place for "inherited behavior by the type that introduced it". The verdict on `N` depends on whether an unrelated override method exists, not on the region `N` sits in. Readings: (A) a region is an override region when any override of the type, of any member kind, comes from the region's type, so `N` is reported; (B) only overrides of the rule's own member kind count (today's behavior, which the rule pages describe). Reading A also changes the RH7405–RH7408 pages. Touches SHARED-REGGROUP-007, RH7405-001 to RH7408-001, RH7405-002 to RH7408-002.

**Q2 — A member that implements members of two interfaces is judged against one of them, chosen by the order of the base list.** Not decided by a principle.

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

Real output: RH7409 reports `M` and asks for region `IA`. With `class C : IB, IA` and the same body, nothing is reported. Why it looks wrong: `M` implements `IB.M` as much as `IA.M`, and the verdict changes with the order of the base list, not with the code. Readings: (A) a region named after any interface whose member the method implements is accepted, so nothing is reported in either order; (B) only the first interface in base-list order is accepted (today). Touches SHARED-REGGROUP-005, RH7409-001 to RH7412-001.

**Q3 — Only the first declarator of a field-like event is judged.** Decided by principle P20 (#912). It violates SHARED-DIAG-006, which #1260 is adding. Until that is merged, the case stays out of the specs.

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

Real output: nothing is reported. `X` comes from `B2` but lies in region `B1`. RH7403 reads only `A`. The same holds for RH7411 (`class C : IA, IB { #region IA  public event EventHandler A, X; #endregion }` with `X` declared by `IB`: nothing reported). It also holds for RH7407: with `public override event EventHandler A, X;` outside any region and `#region B2  public event EventHandler F;  #endregion`, `F` is not reported, because only `A` names an override region (`B1`). Expected under P20: every declarator is judged. Touches SHARED-REGGROUP-006, SHARED-REGGROUP-007, RH7403-001, RH7407-001, RH7411-001.

New issue needed: `SDD open questions: sdd/specs/analyzer/RH7501.md` (sub-issue of #912, or listed there).

**Q4 — The fix moves `#if` branches and directives in front of the `break` out of the block.** The `#if` input is decided by principle P1 (#912) and violates SHARED-FIX-003. The `#pragma` input is a relocated directive. AGENTS.md (Code-fix coverage) says such a rewrite withholds itself, but P1's four conditions do not name relocation literally, so the maintainer confirms.

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

Real output (DEBUG undefined): the fix is offered. It writes `}` and then `#if DEBUG` / `A(y);` / `#endif` / `break;` after the block, so a DEBUG build fails with CS0103, because `y` is out of scope.

Second input: the same section with `#pragma warning disable CS0162` in front of `break;` and `#pragma warning restore CS0162` behind it inside the block. Real output: the `disable` moves behind the block and the `restore` stays inside it, so the restore now comes first and the suppression runs to the end of the file. The moved `break` also keeps its old indentation (20 columns instead of the brace's 16). Touches RH7501-004, SHARED-FIX-003.

**Q5 — The fix breaks the code when the block's closing brace is not first on its line.** Input 1 is decided by principle P1 (#912) and violates SHARED-FIX-003. Input 2 is decided by principle P9 (#912), which #1138 is adding as SHARED-FIX-006.

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

Real output: the fix produces source that no longer parses. The text in front of the brace on its line (`case 1: { A(); break; `) is written in as the `break`'s indentation (`apply-fix.sh`: `invalid-result`).

Input 2: a section whose block ends `                    break; }` (break and brace on one line). Real output: the result holds a second `break;` that the user did not write, the block's `}` moves to column 0, and the switch's `}` joins the `break` line (`break; break;         }`). Touches RH7501-004.

**Q6 — The fix leaves an empty block.** Not decided by a principle. It is an analyzer/formatter disagreement, so P4 applies: decided case by case.

```csharp
class C
{
    void M(int x)
    {
        switch (x)
        {
            case 1:
                {
                    {|RH7501:break|};
                }
        }
    }
}
```

Real output: the fix produces `case 1:` / `{` / `}` / `break;`. That is the shape FMT-SWITCH-006 calls noise ("`case 3: { } break;` would add noise without structure"), and the formatter never produces it. Readings: (A) report and fix as today, leaving the empty block; (B) report, and the fix removes the block that becomes empty, leaving `case 1: break;`; (C) do not report a `break` that is its block's only statement. Touches RH7501-001, RH7501-004.

Principle instances the lead comments on existing issues (not new questions):

- #1264 (P17, innermost region judges a member). The RH74xx bases judge a member by the top-level region around it, also when it lies in a nested region. Here RH7401 reports `N`, not `M`; under P17 `M` (in `Methods`) would be reported and `N` (in `B`) would not:

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

  The same holds for RH7405: a helper in `#region B` > `#region Helpers` is reported, while one in `#region Helpers2` > `#region B` is not. SHARED-REGGROUP-002 states only the case of a member in no nested region.

- #1240 (region directives in inactive `#if` branches). I found no input where such a directive changes an RH74xx verdict. A pair whose directives straddle an excluded branch does not compile (CS1038, CS1027), and a pair inside an excluded branch encloses no member. RH7501 reads no region directives.

## New-rule candidates (P11)

- A member that implements no interface member, placed in a top-level region named after an interface the type implements, such as a helper method in `#region IDisposable`. It is the counterpart of RH7405–RH7408 for interface regions. No rule reports it today: RH7409–RH7412 only ask implementations to be in their region, and RH7405–RH7408 only look at override regions.

## Follow-up candidates

_None._
