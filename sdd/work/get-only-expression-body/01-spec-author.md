Role: spec author
Commit: 18ba10b
Verdict: #849: change doc written; #850: change doc written

Both issues share one change doc, `sdd/changes/2026-11/get-only-expression-body-rules.md` (`Status: Draft`), and one shared spec, `sdd/specs/shared/get-only-expression-bodies.md` (prefix SHARED-GETONLY). Neither issue carries comments; no maintainer answer covers them. The maintainer's comment on thoenissen/Reihitsu#628 (indexers keep the expression-bodied form, no rule reports `this[int i] => e;`) agrees with #850's direction and is reflected as an RH3210 non-goal.

## thoenissen/Reihitsu#849

### Changed requirements

- RH3209-001 — Every get-only property the formatter converts is reported — added (new spec `sdd/specs/analyzer/RH3209.md`)
- SHARED-GETONLY-001 — A get-only member the formatter converts is reported — added
- SHARED-GETONLY-002 — A member whose shape the formatter keeps is not reported — added
- SHARED-GETONLY-003 — A member whose comments or directives the formatter keeps is not reported — added
- SHARED-GETONLY-004 — The diagnostic covers the accessor list — added
- SHARED-GETONLY-005 — The fix writes the formatter's expression body — added
- SHARED-GETONLY-006 — No fix for a member the formatter refuses — added

Rule page `documentation/rules/RH3209.md` is new (UTF-8 with BOM, like its neighbours).

### Examples for the tests

RH3209-001, SHARED-GETONLY-001 to SHARED-GETONLY-006 (the shared examples are written for RH3209).

### Output questions

The change doc lists six visible output changes; each is the maintainer's. One of them is a boundary the issue itself states two ways, so it is called out here:

**RH3209 · Get-only properties should use an expression body — comment where the formatter keeps the accessor list**

Input:

```csharp
class C
{
    private int _value;

    public int A // note
    {
        get => _value;
    }
}
```

Before (`main`): not reported (the rule does not exist; the formatter leaves the property as written, FMT-STRUCT-015).

After, A (recommended, written into SHARED-GETONLY-003): not reported.

After, B: reported on the accessor list; no code fix is offered.

Why A: the rule then reports exactly what the formatter converts, so formatting the document clears every diagnostic, and no diagnostic stays that no fix can clear. The issue's *Proposed Solution* asks for this ("match the formatter's collapse exactly, including its trivia and directive guards"); its code-fix section says such inputs "are reported without a fix", which is B. Precedent exists for both: SHARED-BODY-001 leaves alone the members the formatter keeps for a directive (A); SHARED-EMPTY-001 reports an empty type the formatter keeps for a comment (B). Requirement IDs touched: SHARED-GETONLY-003 (under B it would turn into a "reported, fix withheld" requirement). The same answer applies to RH3210.

The other visible changes in the change doc follow the issues' proposals: the diagnostic on the accessor list (`{` through `}`), the fix result, the not-reported shapes, the refused shapes reported without a fix (SHARED-GETONLY-006), and the two new rule pages.

### Internal choices

- Rule IDs RH3209 (properties) and RH3210 (indexers), the next free Clarity IDs after RH3208; RH3209 to RH3211 appear nowhere in the repository. Alternative: other free IDs.
- Type names `RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer`/`…CodeFixProvider` and `RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer`/`…CodeFixProvider`, from the titles. The implementer must use these names or the spec header changes with them.
- One shared spec for the two rules instead of repeating the requirements in each rule spec, as SHARED-BODY and SHARED-EMPTY do. Its `Code owners` is `—` until a shared type exists; a shared base or utility the implementer adds becomes its code owner (and the index row changes with it).
- The report condition is stated as "the accessor list FMT-STRUCT-014 replaces and FMT-STRUCT-015 does not keep", referencing the formatter requirements rather than restating their trivia seams.
- SHARED-GETONLY-005 keeps the member's signature as written (like SHARED-BODY-003), so the fix lays out only the arrow clause and semicolon. Alternative: format the whole declaration.
- SHARED-GETONLY-006 withholds the fix when the declaration as written holds a refused shape, in addition to SHARED-FIX-010's check of the node the fix builds: a comment between `get` and `=>` sits in a gap the conversion does not carry, so the built node alone would not show it.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#850

### Changed requirements

- RH3210-001 — Every get-only indexer the formatter converts is reported — added (new spec `sdd/specs/analyzer/RH3210.md`)
- SHARED-GETONLY-001 to SHARED-GETONLY-006 — shared with #849, listed there.

Rule page `documentation/rules/RH3210.md` is new.

### Examples for the tests

RH3210-001; SHARED-GETONLY-001 to SHARED-GETONLY-006 apply to indexers alike — the shared examples use RH3209, so indexer variants of them come from the issue's own shapes (the RH3210-001 example covers expression-bodied, block, throw, a `set` sibling, and an already expression-bodied indexer).

### Output questions

The comment boundary under #849 applies to RH3210 alike; no question of its own.

### Internal choices

As under #849.

### Follow-up candidates

_None._

## Whole run

Facts for the next roles, as found (no recommendation):

- `Reihitsu.Analyzer` references only `Reihitsu.Core`; `Reihitsu.Analyzer.CodeFixes` references the analyzer, Core, and the formatter. The formatter's conditions live in `GetOnlyMemberExpressionBodyTransform` and the `internal static` members of `AccessorExpressionBodyTransform` it calls. FMT-PIPE-009 bars code-fix code from calling a member of a transform; FMT-PIPE-011 / FMT-STRUCT-016 keep the node-level entry points from converting get-only members. Precedent for a predicate shared by analyzer and formatter: `EmptyTypeDeclarationSemicolonAnalysisUtilities` in `Reihitsu.Core` (SHARED-EMPTY).
- `Reihitsu.sln` lists every rule page as a solution item, and `Release.ruleset` lists every rule (RH3208 as `Error`); neither is changed here.
- Examples checked with `scripts/format.sh` on fixtures (today's formatter): every positive example of SHARED-GETONLY-001, SHARED-GETONLY-003, SHARED-GETONLY-005, RH3209-001, RH3210-001, and the change doc collapses to exactly the "after" shown; every negative example of SHARED-GETONLY-002 and SHARED-GETONLY-003 keeps its accessor list (the formatter only re-lays out `F { get => field; } = 5` over lines); both inputs of SHARED-GETONLY-006 are refused (`unsupported syntax: a comment or directive between the arrow of an expression body and the code in front of it`). The signature-spacing example of SHARED-GETONLY-005 is a fix result the formatter does not produce (it would also normalize `public  int  A`); it compiles. Language-version statements (C# 6 / C# 7 / C# 5) were not run.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed — `AnalyzerSpecHeaderMatchesItsRule` (RH3209/RH3210 not yet existing rules), `EveryCodeOwnerNamesAnExistingType` (the four rule types), `EveryRequirementOfAnEnforcedSpecIsCovered` (the eight new requirements). All three are expected until the rules and tests exist; the `Uses`, ledger, index, and ID-format checks pass.
