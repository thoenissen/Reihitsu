Role: spec author
Commit: a178f95
Verdict: spec written

Repair of Finding 1 of `14-spec-checker.md`, after the structural change of audit round 1 (lead decisions 13 and 16). The wording is re-derived from decision 16, `12-implementer.md` (`GetRenameCascadeAsync`, `HasExcludedCodeOccurrenceAsync`), and the tests `11-test-implementer.md` tags `SHARED-CASING-007`. I did not take the checker's suggested phrase as given; the bullet now also says what the cascade is and where the region ends. Clarification of existing behavior, so no change doc and no new ID.

## Changed requirements
- SHARED-CASING-007 — The fix renames the declaration and every reference — clarified (ID kept, `sdd/specs/shared/casing.md`):
  - The excluded-code bullet now bounds the region by "a document where the symbol, or a member of its rename cascade, can be referred to". It defines the rename cascade as a member it overrides, a member that overrides it, an interface member it implements, and an implementation of it, each with its own cascade in turn, also in another document or project of the solution. A document where none of these can be referred to is outside the region, even when its excluded code names the old or new name (FC-002: both sides stated).
  - New example "a name in excluded code reached through the rename cascade, for RH4103, in two documents" (withheld side). It is the same input as `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToAMethodReferencedInExcludedCode`. Its prose names the private nested override, interface implementation, and cross-project variants, and the offered counterpart (`…FixRenamesAFileLocalOverrideTogetherWithTheOverriddenMethodWhenNoExcludedCodeNamesIt`).
  - New example "excluded code of an unrelated type, for RH4106, in two documents" (offered side). It is the same input and output as `RH4106…FixIsOfferedWhenOnlyAnExcludedBranchOfAnUnrelatedTypeInAnotherFileNamesTheField`.

## Examples for the tests
SHARED-CASING-007: both new examples are already pinned by the tests named above, which carry the tag. No new test is needed.

## Output questions
_None._

## Internal choices
- I listed the cascade's members exhaustively, matching what `GetRenameCascadeAsync` closes over, instead of SHARED-CASING-009's "such as" list. Alternative: the checker's shorter "or a member renamed with it". I rejected it because it leaves the region's extent to the reader, and the narrowing side (unrelated type, decision 16) would not be stated. I did not edit 009's wording.

## Follow-up candidates
_None._
