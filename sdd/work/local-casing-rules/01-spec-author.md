Role: spec author
Commit: 4771b09
Verdict: #1087: change doc written; #1088: change doc written; #1978: change doc written

The prompt carried no intended fix or suspected cause. The maintainer's answers in `00-lead.md` and in the issues were applied as binding; where they replace an issue's text (camelCase for local constants in thoenissen/Reihitsu#1088), the answer wins.

## thoenissen/Reihitsu#1087

Change doc: `sdd/changes/2026-11/local-casing-rule-split.md` (shared with #1088, same cause: the split of local names between RH4115, RH4117, and RH4128). Rule pages: `documentation/rules/RH4115.md` (local constants → RH4128), `documentation/rules/RH4117.md` (every deconstruction form, patterns included).

### Changed requirements

- RH4115-001 — Local variable names are in camelCase — changed (its coverage sentence now points at the exclusions RH4115-002 and RH4115-004; the list of declarations is unchanged).
- RH4115-002 — Variables of a deconstruction are not checked — changed (all forms that RH4117-001 and RH4117-003 name, not only `var (…)`; with a negative example for the pattern variables that stay with RH4115).
- RH4115-004 — A local named `_` is not reported — added.
- RH4115-005 — Local constants are not checked — added.
- RH4117-001 — Variables of a deconstruction declaration are in camelCase — changed (adds the declaration expressions inside a tuple on the left of a deconstruction or in front of `in` of a `foreach`).
- RH4117-002 — The fix renames the variable to camelCase — clarified (second example for pattern variables; the normative text is unchanged).
- RH4117-003 — Variables of a deconstruction pattern are in camelCase — added.
- Non-goal pointers kept in sync (no requirement changed): RH4116, RH4118 ("Variables declared by a deconstruction — RH4117").

### Examples for the tests

RH4115-002 (both blocks), RH4115-004, RH4115-005, RH4117-001, RH4117-002 (second example), RH4117-003 (both blocks).

Existing tests that pin behavior this change reverses (found while checking coverage; the test implementer owns them):
- `RH4115LocalVariableCasingAnalyzerTests.NoCodeFixForLetterlessLocalVariable` uses `int _ = 42;` and expects a diagnostic without a fix; after RH4115-004 there is no diagnostic.
- `RH4115LocalVariableCasingAnalyzerTests.VerifyNoDiagnosticsForDeconstructionVariables` and `VariablesOfVarDeconstructionsAreNotChecked` (RH4115-002) — check they still match the widened RH4115-002.
- Any RH4115 test with a local `const` that is not camelCase; I found none in `RH4115LocalVariableCasingAnalyzerTests.cs` by search.

### Output questions

**1 · RH4117 · Deconstruction variable names should be in camelCase — the subpatterns of a positional pattern**

The maintainer's answer says RH4117 owns "every variable a deconstruction declares, in assignments and in patterns alike", and names `(var A, var B) = p`, `var (A, B) = p`, `p is var (A, B)`, and `case var (L, R)`. It does not name the pattern counterpart of `(var A, var B) = p`, which is the positional pattern `p is (var A, var B)`. Today RH4115 reports those variables (RH4115-001 lists the `var` and declaration patterns). Requirements touched: RH4115-002, RH4117-003.

Input:
```csharp
class C
{
    int M((object, object) pair)
    {
        if (pair is (var Third, int Fourth))
        {
            return (int)Third + Fourth;
        }

        return 0;
    }
}
```

Before (`main`): RH4115 is reported on `Third` and `Fourth`; the fix renames them to `third` and `fourth`.

After, A (recommended): RH4117 is reported on `Third` and `Fourth`; RH4115 is not. The fix result is unchanged (`third`, `fourth`).

After, B: unchanged — RH4115 is reported on `Third` and `Fourth`; RH4117 checks only `var (…)` patterns.

Why A: a positional pattern deconstructs the tested value as `(var a, var b) = p` does, so A gives the assignment and the pattern spelling the same rule, which is what "in assignments and patterns alike" asks for.

The spec is written on A. Under A, the boundary is where the variable's designation stands: the `var` or declaration pattern that is itself a subpattern of a positional pattern, also of a nested positional pattern, goes to RH4117. These stay with RH4115 (RH4117-003, second example): the variable that names a whole pattern (`pair is (1, 2) Both`), a property-pattern variable, also inside a positional pattern (`({ Length: var Size }, _)`), a declaration pattern inside a combinator or parentheses (`(int Number and > 0, _)`), and a simple pattern variable (`is int Number`). If the maintainer picks B: RH4117-003's second bullet and its examples with `(var Head, _)`, `(int Count, (var Inner, var outer))` are removed, RH4115-002 drops "a variable declared by a subpattern of a positional pattern" and its `objects is (var Item, int Number)` line, RH4117-002's second example drops `Head`, and case 6 of the change doc moves to *Unchanged*.

### Internal choices

- RH4115-002 and RH4117-001/003 state the boundary by the position of the designation in the syntax tree (inside a parenthesized variable designation; a declaration expression that is an element of a tuple expression; the pattern of a positional pattern's subpattern), not as a list of spellings. Alternative: list the spellings the maintainer named — rejected, it misses nesting.
- A declaration expression inside a tuple expression is a deconstruction variable without checking the tuple's parent: C# allows `(var a, var b)` only as a deconstruction target or a `foreach` variable (CS8185 elsewhere), so the position alone decides.

### Follow-up candidates

_None for this issue._ (See *Whole run* for the anonymous-object rename gap that this issue's RH4115 and RH4117 fixes share.)

## thoenissen/Reihitsu#1088

Change doc: shared with #1087 (`sdd/changes/2026-11/local-casing-rule-split.md`). New spec `sdd/specs/analyzer/RH4128.md`, new rule page `documentation/rules/RH4128.md`, index row in `sdd/specs/README.md`, package README row in `src/Reihitsu.Analyzer.Package/README.MD`. Non-goal pointers updated: RH4110 ("Local constants — RH4128"), RH4125 (casing of local constant names — RH4128).

### Changed requirements

- RH4128-001 — Local constant names are in camelCase — added.
- RH4128-002 — A local constant named `_` is not reported — added.
- RH4128-003 — The fix renames the constant to camelCase — added.

### Examples for the tests

RH4128-001, RH4128-002, RH4128-003. The RH4128-003 example was run through today's RH4115 fix (which today renames local constants) with `scripts/apply-fix.sh RH4115`: its output matches the spec's after block byte for byte, so the rename the new rule reuses already produces it.

### Output questions

_None._ The rule, its camelCase style, and coordination with #1087 are the maintainer's decisions (2026-10-18, 2026-11-21). The change doc lists the visible changes (cases 2, 3, and the new rule page) for the lead's batch. One sub-decision is mine and visible, listed as case 3 of the change doc: a local constant named `_` is not reported (RH4128-002), for the same reason the maintainer exempted a local `_` from RH4115.

### Internal choices

- **Rule ID RH4128, not RH4119.** RH4119 is the gap in the RH41xx range, but it is not free: it shipped in `v1.0.0-beta5` and `v1.0.0-beta6` as the single-letter-identifier rule (commits `4a91fb2c`, `afe79c15`) and was retired in `5578c425` when RH4122 to RH4127 replaced it (`git tag --contains 4a91fb2c`). Reusing it would let an old suppression of RH4119 silence the new rule. The maintainer's answer says "the next free number in RH41xx", which RH4128 is. Alternative: RH4119 — rejected for that reason. The lead may still want to confirm it with the maintainer, since the ID is visible.
- Names of the new types in the spec's code owners: `RH4128LocalConstantCasingAnalyzer`, `RH4128LocalConstantCasingCodeFixProvider` (pattern of `RH4115LocalVariableCasingAnalyzer`). The implementer may rename them; the spec, `sdd/specs/README.md`, and the requirement IDs then follow.
- Title for the resources, rule page, and package README row: *Local constant names should be in camelCase*; message equal to the title (no placeholder). The resource entries of the new rule are the implementer's (role file); the rule page and the package README row are written. `Reihitsu.sln` (solution items) and `Release.ruleset` (`RH4128` as `Error`) are not text paths I own; the implementer adds them.
- Code fix: Fix All, the shared casing rename. `AGENTS.md` allows a new rule's fix only when comprehensive; the shared rename already handles local constants today under RH4115 (verified above), so it is.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1978

Change doc: `sdd/changes/2026-11/range-variable-casing.md`. Spec `sdd/specs/analyzer/RH4105.md` (title now *Parameter and query range variable casing*). Rule page `documentation/rules/RH4105.md` (new heading, description and why mention range variables). Resource texts changed: `RH4105Title` and `RH4105MessageFormat` in `src/Reihitsu.Analyzer/AnalyzerResources.resx` → *Parameter and query range variable names should be in camelCase*. Package README row changed to the same title. Non-goal pointer: RH4124 ("The casing of parameter and range variable names — RH4105").

The `/// RH4105: Method parameter names should be in camelCase` doc comment of `RH4105MethodParameterCasingAnalyzer` still quotes the old title; it is code and the implementer's. No test quotes the old title text (searched `tests/`); `AnalyzerPackageMetadataTests` pins title ↔ page heading ↔ README row ↔ message, and all four were changed together.

### Changed requirements

- RH4105-002 — Discards and record parameters are not checked — changed (a range variable named `_` is not reported either).
- RH4105-005 — Query range variable names are in camelCase — added.
- RH4105-006 — The fix renames the range variable to camelCase — added (including the withheld case below).

### Examples for the tests

RH4105-002 (the `N` method with `from _`), RH4105-005 (both blocks), RH4105-006 (both examples).

Evidence that the fix is comprehensive (the maintainer's condition): a scratch program using the Roslyn renamer (`Microsoft.CodeAnalysis.CSharp.Workspaces` 5.12.0, the repository's version) renamed range variables declared by `from`, `let`, `join`, `join … into`, and a query continuation `into`, with every reference across the continuation, and the result compiled. Both fix examples (RH4105-006, change doc case 1) were produced exactly by that renamer.

The one unsafe result found: the renamer also renamed the anonymous object members in `select new { Value, Doubled }` to `value` and `doubled` — it compiles, but `ToString()`, serialization, and reflection see other names, which SHARED-FIX-003 forbids. RH4105-006 therefore withholds the fix when a reference to the range variable is the whole expression of an anonymous object member without a name of its own (cheap check inside the declaring query expression; a range variable has no references outside it). This is a refusal under SHARED-FIX-003, not a new visible decision, and the maintainer's answer allows withholding; it is still listed as case 2 of the change doc.

### Output questions

_None._ Option A, the widened title and message, and the fix condition are the maintainer's answer of 2026-11-27. The change doc lists the visible changes (reports, fix result, withheld fix, `_`, title and message, rule page) for the lead's batch.

### Internal choices

- The fix's guard is stated in RH4105-006 for range variables only, because the run's issue is about range variables. Alternative: state it once in SHARED-CASING for every casing fix — rejected for this run, see the follow-up candidate in *Whole run*.
- The clause list in RH4105-005 is the one RH4124-003 states, so both rules check the same declarations; RH4105 lists RH4124-003 under `Uses`.

### Follow-up candidates

See *Whole run*.

## Whole run

### Checks

- Every C# block of the four changed or new specs and of both change docs (42 blocks, markup stripped) compiles with the C# preview language version and `allowUnsafe` (scratch Roslyn compilation), after one fix to the RH4105-002 example (`from _` in the same method as a parameter `_` gave CS1931).
- `scripts/apply-fix.sh RH4115` and `RH4117` on the #1019 inputs confirmed every *Before* of `local-casing-rule-split.md` (cases 1, 2, 4, 5, 6: diagnostics, fix results, no-fix for `_`).
- `scripts/test.sh --project architecture`: 195 passed, 3 failed, all expected until the test implementer and implementer run — `AnalyzerSpecHeaderMatchesItsRule` (RH4128 is not an existing rule yet), `EveryCodeOwnerNamesAnExistingType` (the two RH4128 types), `EveryRequirementOfAnEnforcedSpecIsCovered` (RH4105-005, RH4105-006, RH4115-004, RH4115-005, RH4117-003, RH4128-001 to RH4128-003 have no tests yet). The `Uses`, ledger order, and index checks pass.
- Ledger: RH4105-005, RH4105-006, RH4115-004, RH4115-005, RH4117-003, RH4128-001 to RH4128-003 inserted in ordinal order (checked).

### Visible output changes (all issues)

1. RH4115 no longer reports a local named `_` (`int _`, `foreach (var _ …)`, `catch (Exception _)`, `using var _`).
2. A local constant not in camelCase is reported by RH4128 instead of RH4115; fix result unchanged.
3. A local constant named `_` is no longer reported (RH4128-002; before: RH4115 without fix).
4. `(var A, var B) = p`, `(int A, var (B, C)) = …`, `foreach ((var K, var V) in …)`: RH4117 instead of RH4115; fix result unchanged.
5. `p is var (A, B)`, `case var (L, R)`: newly reported by RH4117, with fix.
6. `p is (var A, int B)`: RH4117 instead of RH4115 (output question 1 under #1087).
7. Query range variables not in camelCase: newly reported by RH4105, with fix; fix withheld when the range variable names an anonymous object member (`new { Value }`); `from _` not reported.
8. RH4105 title and message: *Parameter and query range variable names should be in camelCase*.
9. Rule pages: RH4105, RH4115, RH4117 changed; RH4128 new. Package README: RH4105 row changed, RH4128 row added.

### Follow-up candidates

**Casing fixes rename an anonymous object member that is named after the renamed variable (RH4105 for parameters, RH4115, RH4117, and RH4128).** The shared casing rename (SHARED-CASING-007) renames a reference that is the whole expression of an anonymous object member without a name of its own, and with it the member. Run on `main` with `scripts/apply-fix.sh`:

```csharp
Func<int, string> f = Value => new { Value }.ToString();
```

RH4105's fix gives `value => new { value }.ToString()`, and

```csharp
var Total = values.Sum();
var o = new { Total };
return o.ToString();
```

RH4115's fix gives `var total …; var o = new { total };`. Both compile, but `ToString()` now returns `{ value = 1 }` instead of `{ Value = 1 }`, and a serializer or reflection sees the new name — a run-time change SHARED-FIX-003 forbids. (When the member is read, as in `o.Total`, the fix is already withheld.) This is a defect against the unchanged SHARED-FIX-003, outside the run's issues as far as parameters and locals go; the caller is each casing code fix, applied by the user in the IDE. Since the run touches the casing rules, the lead may weigh it as an adjacent fix (bug fix, no visible output in question) or file it; a shared guard would also replace the range-variable-only guard of RH4105-006.

### Architecture findings

_None._
