Role: spec author
Commit: 4ae1d43
Verdict: spec written

Spec part of lead decision 17 (architecture finding of `17-auditor.md`, findings 6 and 7). This is a clarification of existing behavior in the bug-fix lane. There is no change doc, and every requirement keeps its ID; `sdd/specs/requirement-ids.txt` is unchanged. I re-derived the wording from decision 17 and the auditor's evidence. I did not take finding 7's suggested phrase as given. The prompt carried no intended fix or hoped-for verdict beyond the decision itself.

## Changed requirements
- SHARED-CASING-007 — The fix renames the declaration and every reference — clarified (`sdd/specs/shared/casing.md`):
  - **The one statement of the cascade.** A new paragraph after the normative sentence defines the *rename cascade*. It is the reported symbol itself and every symbol the renamer renames with it, in any document or project of the solution. Which symbols these are is the renamer's own determination, not a list of relations.
  - **Kinds as examples only.** The paragraph names override, overridden, implemented interface member, implementation, and the other part of a partial method together with its type parameter or parameter at the same position. Each of these brings in turn the symbols the renamer renames with it.
  - **Referenced assemblies.** A member declared in a referenced assembly is not renamed and is not part of the cascade.
  - **The paragraph is the only statement.** It says that every other statement of the spec about the cascade refers to it.
  - **The excluded-code bullet.** It no longer restates the cascade, which removes the closed list of finding 7. Its region is now "a document where a symbol of the rename cascade can be referred to". The negative side stays: a document where no symbol of the cascade can be referred to is outside the region.
  - **The unrelated-type example.** The prose now reads "the rename cascade of the private field `C.Value` is the field alone", replacing "it has no rename cascade", to match the definition.
  - **New example** "a name in excluded code reached through the other part of a partial method, for RH4121, in two documents". It is finding 6's counterexample:
    - Withheld for both reported `Key`s.
    - The reverse direction is in prose: `#if NEVER` around `[System.Obsolete(nameof(Key))]` on the declaration without a body.
    - The offered side is in prose: no excluded branch names `Key` or `TKey`, so both parts are renamed to `TKey`.
- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified:
  - The third bullet now reads "in every other type that declares a symbol of the rename cascade (SHARED-CASING-007)". This replaces its own "such as" list.
  - `Why:` refers to the same statement. It also says that a member declared in a referenced assembly keeps its name, which matches the definition's exclusion. The metadata example is unchanged and still agrees with it.

## Examples for the tests
SHARED-CASING-007, the new RH4121 partial-method type parameter example. Three tests should come from it:
- definition to implementation (withheld, both diagnostics);
- implementation to definition (the `nameof(Key)` prose variant, withheld);
- the offered side (`TKey` in both parts).

Decision 17 also asks for Fix All. The example's statement "no code fix is offered for either" covers it through `Verify`'s Fix All scopes. The existing 007 and 009 examples are unchanged in input and output.

## Output questions
_None._ The definition moves from a hand-written list to whatever the renamer renames. That can only withhold more fixes, and only where excluded code in a document of a further cascade symbol names the old or new name. SHARED-FIX-003's build-configuration condition already decides each such case (P1, lead decision 17). Offered fixes keep `main`'s renamer output. No rule page changes; every change is a withheld fix, which `general-notes.md` covers.

## Internal choices
- **Where the cascade is stated.** It sits in SHARED-CASING-007's opening section, not inside the excluded-code bullet. 009's bullet and `Why:` need it as well, and the auditor named 007 as the owner. The alternative was a separate new requirement, but that needs a new ID, which a clarification does not justify.
- **Definition by the renamer, not by mechanism.** The cascade is defined by what the renamer renames, not by the reference engine's definitions (`FindReferencesAsync`). How the code reads that set is the implementer's choice. A spec that names the API would bind the code owner to one mechanism. The alternative was to name `SymbolFinder` in the spec.
- **Metadata members left out.** A metadata member is explicitly outside the cascade because the renamer cannot rename it. This keeps 009's metadata example ("`MetaBase.process` keeps its name") consistent with the definition. The alternative was to include it and let it contribute no documents.

## Follow-up candidates
_None._ The record positional-parameter cascade is a member of the renamer's own set, and the auditor checked that it holds. I did not list it as an example, because no test in this run pins the renamer's behavior for it.

Checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"` passes, 37 tests.
