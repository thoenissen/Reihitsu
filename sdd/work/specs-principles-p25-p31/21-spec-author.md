Role: spec author
Commit: 5a00aa5
Verdict: spec written

Repair of `20-spec-checker.md` finding 1 (third repair round). My prompt carried no intended fix or hoped-for verdict beyond routing the finding to me; I re-derived the change from the specs, the analyzers, and the architecture tests.

## Derivation

- SHARED-DIAG-004 paragraph 2 names RH7105 and RH7106 as rules that judge each part of a partial declaration on its own; only RH7104 listed SHARED-DIAG-004 under `Uses`.
- Adding the ID to `Uses` alone would be wrong: `sdd/README.md` requires `Uses` to list exactly the IDs referenced outside the header and the non-goals, and `SpecStructureTests` rejects an ID that is "in Uses but not referenced outside the non-goals". Each spec therefore gains a reference in its requirement body, in the same shape as RH7104-001, and the ID in `Uses`.
- The reference states current behavior: `RH7105DeclarationKeywordsMustFollowOrderAnalyzer` and `RH7106ProtectedMustComeBeforeInternalAnalyzer` register syntax-node actions per declaration and use no semantic model, so each part is judged by its own modifiers whatever the other parts write.
- No change doc: the change adds a cross-reference to behavior SHARED-DIAG-004 already states; no intended behavior changes. No new ID; `requirement-ids.txt` is untouched. No rule page under `documentation/rules/` describes per-part judgement, so none changes.

## Changed requirements

- RH7105-001 — Modifiers out of the canonical order are reported — clarified: each part of a `partial` type or member is judged by the modifiers that part carries, also when another part writes its modifiers in the canonical order (SHARED-DIAG-004). `Uses` gains SHARED-DIAG-004.
- RH7106-001 — A compound accessibility in the other order is reported — clarified: each part is judged by the modifiers it carries, also when another part writes the same accessibility in the canonical order (SHARED-DIAG-004). `Uses` gains SHARED-DIAG-004.

## Examples for the tests

- No new example. RH7106-001's new sentence is pinned by the existing `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.PartOfAPartialPropertyIsReportedAndFixedOnItsOwn` (tags `RH7106-001`, `RH7106-004`, `SHARED-DIAG-004`).
- RH7105-001's new sentence has no dedicated partial-declaration test where the other part is in the canonical order. Coverage is per ID and both IDs stay covered, so no test is required; if the lead wants the sentence pinned, the shape is the RH7106 one with `static public partial` on one part of a partial method and `public static partial` on the other.

`scripts/test.sh --project architecture` after the edit: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`.

## Open questions to file

_None._
