Role: spec author
Commit: 81fc5cf
Verdict: spec written

Spec part of lead decision 19 (architecture finding and finding 8 of `23-auditor.md`). Clarification in the bug-fix lane: no change doc, every requirement keeps its ID, `sdd/specs/requirement-ids.txt` unchanged. The prompt carried the change the lead decided and nothing beyond it; I derived the wording from decision 19 and the auditor's evidence.

## Changed requirements
- SHARED-CASING-007 — The fix renames the declaration and every reference — clarified (`sdd/specs/shared/casing.md`):
  - **Excluded-code bullet.** It now reads "neither the old name of a symbol of the rename cascade nor the new name". The old names are those of every cascade symbol that source can refer to by its name, not only the reported symbol's. A differently spelled cascade symbol, such as the other part's parameter of a partial method, adds its own spelling. FC-002, both sides: a symbol whose name source never writes (a property's accessor, a type's constructor) adds no old name, and a symbol outside the cascade adds none either (a differently spelled type parameter of the other part of a partial method, which the renamer leaves alone). The region sentence is unchanged apart from "an old name".
  - **Cascade paragraph, examples made accurate.** "the other part of a partial method together with its type parameter or parameter at the same position" now reads: the other part of a partial method or partial indexer with its parameter at the same position, however that part spells it, and the other part of a partial method with its type parameter *of the same name*. The auditor's probe `r4/ptp` shows the renamer does not cascade a differently spelled type parameter, so the old wording overstated it. It is still an example list; the renamer's determination stays the definition.
  - **New example** "an old name of a differently spelled part in excluded code, for RH4105, in two documents": finding 8's counterexample (`Arg` / `other`), withheld. In prose: the reverse direction (`Other` reported, `nameof(Arg)` excluded in the first part, withheld), the partial indexer (`Index` / `position`, withheld), the offered side (no branch names `Arg`, `other`, or `arg`: both parts renamed to `arg`), and the negative side for type parameters (`Run<Key>` / `Run<TItem>`, excluded `TItem` does not withhold; only the first part becomes `TKey`).

## Examples for the tests
SHARED-CASING-007, the new RH4105 example. Tests from it:
- defining to implementing part, withheld (single fix and Fix All);
- implementing to defining part (`Other`, `nameof(Arg)`), withheld for `Other`;
- partial indexer, withheld;
- the offered side, both parts renamed to `arg`;
- the type-parameter negative side (RH4121, `Key` / `TItem`, excluded `TItem`): offered, only the first part renamed. This pins that the implementer takes old names from the symbols the renamer renames. `23-auditor.md` notes that the reference search returns a superset of the renamed declarations; if it also returns the other part's `TItem`, taking names from every returned definition would withhold this fix, which the spec does not allow. `main` and the current head offer this fix (only `Key` and `TKey` are searched), and the auditor's `r4/ptp` run shows only the first part renamed; excluded `TItem` itself was not run.

## Output questions
_None._ The restated bullet can only withhold more fixes, and only where excluded code names the spelling of a differently spelled cascade member. That spelling no longer exists after the rename, so SHARED-FIX-003's build-configuration condition already decides each case (P1, decisions 17 and 19). The type-parameter negative case keeps today's offered fix and today's output. Offered fixes keep `main`'s renamer output. No rule page changes: every change is a withheld fix, which `general-notes.md` covers.

## Internal choices
- **"Refer to by its name" stated in plain words, not as `CanBeReferencedByName`.** The spec names no API; the implementer chooses the mechanism. Alternative: name the Roslyn property.
- **The type-parameter negative side written as an example.** It is the boundary of "symbol of the cascade" on the old-name axis and guards against over-withholding. Alternative: leave it out, which leaves FC-002's other side without an example.
- **The cascade paragraph's partial example corrected in place**, rather than left as an inaccurate example under "for example". Alternative: leave it, since the renamer's determination is the definition anyway.

## Follow-up candidates
_None._ The partial-constructor parameter belongs to the same class but RH4105 reported no diagnostic for it in the auditor's harness; I did not name it in the spec, since no test of this run can pin it.

Checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"` passes, 37 tests.
