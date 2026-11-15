Role: auditor
Commit: 0696846bb47bdbab35ee2ce4b463cab96293019f
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: 0696846. `git ls-remote origin refs/heads/main` returns 4838e9a03f0736d9e4593286d0f251f3a92fb704, which matches the prompt and is an ancestor of the head (merged in eb15964).
- Full audit of `4838e9a...0696846`. Production changes: `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (+141/−7). Test changes: `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`. Spec changes: `RH2001.md`, `requirement-ids.txt`, and the change doc `rh2001-rename-binding.md`. The diff matches handovers 04–07.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence: `scripts/apply-fix.sh RH2001` on fixtures in the scratchpad, run on the head and on a temporary `4838e9a` worktree (since removed), LF and CRLF identical. `scripts/apply-fix.sh RH4106` was run for the parity check. One `dotnet build` of the fix output confirmed the compile errors. Fixtures live under `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/fx1` … `fx4`.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` `RegisterCodeFixesCoreAsync` / `RenameChangesBindingAsync` (no check for this class) | FC-006, FC-036, FC-015 | – | same requirement (SHARED-FIX-003, the principle of #1757); pre-existing on `main` 4838e9a | A reference to the property used as an **anonymous-object member without a name** or a **tuple element with an inferred name** gets renamed. That renames the anonymous or tuple member, so the code that reads it no longer compiles, or prints a different name at run time. The guard only compares identifiers spelled like the *field* name, so it cannot see a reference spelled like the *old* name whose target disappears. **Failing members** (fix offered, head and `main`): (a) `var x = new { Value }; … x.Value` → `new { _value }`, CS1061; (b) `new { this.Value }` → `new { this._value }`, CS1061 on `x.Value`; (c) `new { other.Value }` → `new { other._value }`, CS1061; (d) `items.Select(item => new { Value, item }).Select(pair => pair.Value + pair.item)` → CS1061 (built: `error CS1061: '<anonymous type: int _value, int item>' does not contain a definition for 'Value'`); (e) `var t = (Value, 1); … t.Value` → `(_value, 1)`, CS1061 (built); (f) `new { Value }.ToString()` compiles but returns `{ _value = 0 }` instead of `{ Value = 0 }` (run-time change). **Holding members**: explicit names `new { Value = Value }` → `new { Value = _value }` and `(Value: Value, 1)` → `(Value: _value, 1)`. **Parity**: RH4106 on the same input with a field `Value` (`new { Value }`, `x.Value`) withholds its fix (`no-fix-offered`), through its compile-error check. | Refuse this class: withhold the fix when a reference to the property is the expression of an `AnonymousObjectMemberDeclaratorSyntax` without `NameEquals`, or a tuple `ArgumentSyntax` without `NameColon`. This is a cheap syntactic check over the references the rename already finds. Add reproductions for (a), (e), and (f), and an offered characterization for the explicit-name members. The lead decides whether this belongs to #1757's scope or to a follow-up. LINQ projections of a private auto-property are common code, so withholding here may need escalation 9 (refusal for commonly written code). |
| 2 | medium | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` `GetOtherSymbolReferencesAsync`, the `qualifier` switch (`memberAccess.Expression.ToString()`) | FC-001, FC-002 | – | PR-introduced | The description of another symbol's reference includes the receiver's *text*. When the receiver contains a reference to the property, the rename changes that text, and the comparison reports a binding change that did not happen. This contradicts RH2001-011's third paragraph ("every reference keeps both its binding and its text, apart from the references to the property, which the fix renames") and the change doc's "The rename itself is unchanged". It also withholds a fix that `main` offers. **Failing member, measured on both**: `class Outer { private int _value; class Inner { private Outer Value { get; set; } int Get() { return Value._value; } } }`. `main` offers the fix and gives the correct `return _value._value;`. The head reports `no-fix-offered`. **Withheld on the head, same mechanism, not run on `main`**: `Value!._value` (r1), `this.Value._value` (r2), `Value._value()` on an outer method (r3), `((Outer)Value)._value` (r4), and an `internal int _value` of another type read as `Value._value` (r5, which also breaks RH4108). **Holding member**: `Value?._value`, whose qualifier is the constant `"?"`, stays offered (`_value?._value`). The two shapes of the same access therefore get different outcomes. The class is rare: a member named like the derived field must be reachable through the property's own value. | Refuse this class: record it in RH2001-011 as withheld, a receiver that holds a reference to the property in front of a member named like the field, so that the spec matches the code, and pin one member with a test. Alternatively, describe the receiver by what it binds to, with the property mapped to one marker, if that stays a one-line change. The lead chooses. |
| 3 | medium | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` `GetOtherSymbolReferencesAsync`, `root.DescendantNodes()` (does not descend into structured trivia) | FC-001 | – | same requirement (RH2001-011, first paragraph, added by this run) | A `cref` in a documentation comment that names an inherited member like the field is left as written, and after the rename it binds to the new field. RH2001-011 paragraph 1 withholds a reference to another symbol that is "left as written … and then binds to the field", in any document. The scan never sees crefs, so the fix is offered. Input: `class Base { protected int _value; } class Sample : Base { private int Value { get; set; } /// <summary>Reads <see cref="_value"/></summary> public int Get() { return Value; } }`. Head and `main` both give `private int _value;` with the cref unchanged. The cref then resolves to `Sample._value` instead of `Base._value`: traced by C# cref lookup, not executed. There is no compile error and no run-time change. The class is rare. | Refuse this class: scan with `DescendantNodes(descendIntoTrivia: true)` so crefs are compared like other references, and add one test. Alternatively, state in RH2001-011 that a cref is outside it. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the rename keeps every other reference's binding (`RenameChangesBindingAsync`) | `IdentifierNameSyntax` nodes spelled like the field name, outside structured trivia, in the documents of the containing type's parts (`GetReferringDocuments`). Each is described by its bound symbols or candidates, the candidate reason, and the receiver *text* | `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`: conflict annotations, `RewritesOtherCodeAsync` (tree diff), reference-location count, and new compile errors | Disagree. There is no compile-error check, so references spelled like the old name are invisible (#1). Crefs are excluded (#3). The receiver text includes the renamed property (#2). | findings #1–#3 |
| `GenericNameSyntax` excluded from the scan | Same as the row above | Same | Traced and run: `this._value<int>()` on an extension method and `_value<int>()` on an inherited generic method are left as written by the renamer (fixtures `a_generic_ext`, `b_generic_inherited`). Member lookup with type arguments excludes the non-generic field, so the binding holds. | holds |
| Multiset comparison (sorted descriptions) | A lost and a gained description could cancel out | – | A cancelling swap needs a renamed property reference that binds to symbol X with the same qualifier text, while an X reference with that qualifier rebinds to the property in the same document. The derived member hides the inherited one everywhere in the type, and inner-scope symbols keep their own references, so no reachable shape was found. | holds |
| Placement in `RegisterCodeFixesCoreAsync` | Last in the `&&` chain, after the cheap guards | `ApplyCodeFixAsync` does not re-check. Fix All (`BatchFixer`) only applies registered actions | – | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a rename may be applied (SHARED-CASING-009 binding cases) | `rg -n "Renamer.RenameSymbolAsync\|RewritesOtherCodeAsync\|CountReferenceLocationsAsync\|HasConflictAnnotationsAsync" src --glob '!*Test*'` finds `CasingCodeFixProviderBase{T}.cs` (`RenameWithoutConflictAsync`:721, `RewritesOtherCodeAsync`:1050, `CountReferenceLocationsAsync`:1096, `HasConflictAnnotationsAsync`:1167) and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (`RenamePropertyToFieldNameAsync`, `RenameChangesBindingAsync`, `GetOtherSymbolReferencesAsync`) | Two owners (FC-015), accepted by lead decisions 3 and 9, with the unification follow-up recorded. The difference is justified in 04, but parity is not proven: #1 is an input where the casing owner withholds and the RH2001 owner ships non-compiling code. Add it to the follow-up. |
| Referring documents of a private member | `ExcludedCodeUtilities.GetReferringDocuments`, reused. No copy. | holds |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 26 `VerifyNoFixIsOffered…` single-document tests (f02, f05/t4, t4, f05, e2, f08, f07 delegate, s16, e1, block-body lambdas f09/s01/s12, local declaration, assignment, constructor, explicit block lambda, query range, s06, t1, s13) | Diagnostic reported, no action registered | An action is registered (`Assert.IsEmpty(actions)`) | `VerifyNoCodeFixIsOffered`: asserts the markup diagnostics and an empty action list, so FC-009 is covered | holds. 03, 05, and 07 recorded each as red on `main` |
| `VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadInAPartialPartInAnotherDocument`, `…ALambdaParameterInAPartialPartInAnotherDocument…` | No change in either document | The fix renames `Value` in either document | `Verify(source, source, …)` with a second document. It asserts an unchanged result, not an empty action list, which is weaker for a hypothetical no-op action. The guard cannot register a no-op action, and both tests were red on `main` | holds |
| `VerifyFixAllLeavesAPropertyWhoseFieldNameIsBoundAndReplacesTheOthers` | Fix All skips the withheld property and fixes the others | `Value` fixed, or `Count`/`Total` left | `Verify` (iterative and Fix All) | holds |
| The offered characterizations (f01 LF/CRLF, f06, f10, f03, f04, s02–s05, t2, t3, s07–s11, s14, the expression-body f09/s01/s12, the `int` f07, the expression-body partial) | Exact `main` output, with references qualified | Any other output, or a withheld fix | `Verify` with exact expected text and re-analysis | holds. Expected values match `main` (05, 07), and their source is not a patched copy (FC-035) |
| Missing | — | — | — | No test covers the members of #1, #2, or #3 (see Findings) |

## Visible output

Every expected value the diff adds is decided:

- The withheld run-time members (f05, t4, e2, f08, the block-body lambdas, the query range variable, s06, t1, s13, and the partial t4/f09): unchanged SHARED-FIX-003.
- The withheld rewrite members (f02, the delegate-typed f07, s16, e1): maintainer answer B in `00-lead.md` and in the change doc's `Decisions`.
- The offered tests: `main`'s output.

The rule page is unchanged. #2 is a visible change from `main` (offered → withheld) that no test and no answer covers. Withholding for a rare class puts nothing in question, but the spec the run adds says the opposite.

## Failure classes dismissed

- FC-003: both sides are pinned for the lambda (block vs. expression body), the extension method (delegate vs. `int`), the inherited member (named vs. unnamed), and `out` (same vs. earlier statement).
- FC-004, FC-005, FC-019, FC-020, FC-021: no trivia handling changed. `TryCreateFieldDeclaration` and the guards of RH2001-007/009/010 are untouched.
- FC-007: the offered tests re-analyze after the iterative fix.
- FC-008: the guard is evaluated per diagnostic against the original solution. Two renames interact only when they derive one field name, which lead decision 4 dropped (it already breaks RH4111). Covered by the Fix All test.
- FC-009: covered by the helper, as the table above shows.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter phase, no precheck, and no formatter call changed. `FindUnsupportedSyntax(fieldDeclaration)` is unchanged.
- FC-016: block-body siblings were added in 07. Receiver and cref siblings: #2 and #3.
- FC-017: exact-output assertions and empty-action assertions are used.
- FC-018: the fix uses `Renamer.RenameSymbolAsync`, unchanged.
- FC-022: no literal is touched.
- FC-023: a verbatim `@_value` has `ValueText` `_value` and is matched by the scan.
- FC-024: partial parts are compared per referring document and covered by three cross-document tests.
- FC-025, FC-026: no expression-body or token-position logic changed.
- FC-027: block vs. expression body is enumerated (05, 07). Anonymous and tuple projection is the enclosing scope that fails: #1.
- FC-029: no key/value layout is involved.
- FC-030: the fix still formats only the field declaration.
- FC-031: the summaries and remarks of `RenameChangesBindingAsync`, `GetOtherSymbolReferencesAsync`, and `GetAnnotatedPropertySymbolAsync` describe the new code. There is no `TODO` and no tracker number.
- FC-032: no project-wide result was reused across the compiled change. The cleaner's Release build is at ec646e3, and the merge eb15964 touched only `requirement-ids.txt` (00-lead decision 14).
- FC-033: f01 and f02 run under LF and CRLF, and apply-fix arms were identical for both.
- FC-034: the reproductions are the failing shapes themselves (05 replaced the cousins).
- FC-035: expected values were measured on `main`.
- FC-036: `nameof` is decided by RH2001-004's existing example (`nameof(_name)`). Serialization or reflection of an anonymous projection is #1(f). Public API cannot be reached because the property is private.
- FC-037: no text-only claim is made.

## Architecture findings

FC-015, an extension of the finding in 01-skeptic that the lead decided under decisions 3 and 9:

- **Components:** `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and `RH2001…CodeFixProvider.RenameChangesBindingAsync`.
- **Shared decision:** may this rename be applied.
- **Input where they disagree:** #1. On `private int Value; … new { Value } … x.Value`, RH4106 withholds its fix through the compile-error check. RH2001 offers its fix on the same projection of `private int Value { get; set; }` and produces CS1061.

No new structural cause beyond the recorded follow-up. The lead decides under *Architecture findings*.

## Hints

- #1 is pre-existing on `main` 4838e9a with identical output. If the lead puts it outside #1757's scope, it is a follow-up candidate: it ships non-compiling code for commonly written LINQ projections, and it shares the structural cause recorded above.
- The guard runs a full rename and builds two semantic models per referring document at registration, once per diagnostic. This is the same cost profile as the casing fixes, so it is not a finding.
