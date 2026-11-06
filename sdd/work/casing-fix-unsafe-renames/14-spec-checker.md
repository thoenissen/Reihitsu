Role: spec checker
Commit: 5505a58 (head before this handover)
Verdict: findings

Re-check after the structural change of audit round 1 (lead decisions 13-16). Verified against `git diff 98349b9..HEAD` (production: `CasingCodeFixProviderBase{T}.cs`; tests: RH4103, RH4105, RH4106, RH4121; no `sdd/specs/` file changed) and `git diff origin/main...HEAD`. No prompt-supplied fix or verdict was followed.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/shared/casing.md`, SHARED-CASING-007, second coverage bullet | The bullet says neither name occurs in excluded code "in a document where the symbol can be referred to". The code (decision 16) scans the union of the reference scopes of every symbol of the rename cascade (overridden/overriding members, implemented/implementing interface members, also in another project). Nine new tests tagged `SHARED-CASING-007` rely on this wider region (file-local or private nested override, nested implementation, base in a referenced project). "The symbol" reads as the declared symbol alone, so the spec does not state the behavior the tests pin, and the narrower reading would let the fix be offered. SHARED-CASING-009 and its Why already speak of "the symbol's rename cascade"; 007 should say "where the symbol, or a member renamed with it, can be referred to". This is a clarification of existing behavior, so no change doc. | spec author |

Not findings, noted for the lead: the native-entry-point rule (SHARED-FIX-003 run-time bullet, RH4104-002) says "renaming an extern method ... counts"; a method renamed through the cascade is still a renamed extern method, so the text covers it, and no edit is needed. The method-body compile-error widening is covered by 009's "wherever the rename puts it".

## Checked
- 1 Change doc: ok. No requirement was added, changed, or retired since 98349b9, and none is needed for a bug fix. No change doc exists.
- 2 Specs match the diff: finding 1. Compile-error scope (all errors of the affected projects) and native entry points over the cascade agree with SHARED-CASING-009 and SHARED-FIX-003. Format of the specs is unchanged since the last check.
- 3 IDs: ok, `requirement-ids.txt` untouched.
- 4 Code owners: ok, no owner changed; architecture suite passes.
- 5 User documentation: ok, no rule page touched.
- 6 Open questions: ok. Decisions 15 (run-time rebinding) is a follow-up candidate in `00-lead.md`; none is written into a spec.
- 7 Coverage: `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. No new requirement was added.
- 8 Tags: ok. All 25 test methods added since 98349b9 carry the tags that `11-test-implementer.md` lists (native-entry-point tests: `SHARED-FIX-003`; cascade and scope tests: `SHARED-CASING-007` and, where withheld, `SHARED-FIX-003`; compile-error tests: `SHARED-CASING-009` and `SHARED-FIX-003` where withheld; the three offered-side tests carry only `SHARED-CASING-007`/`-009`/`-FIX-003` as listed). The `Examples for the tests` of the spec author are still tagged on tests of the earlier rounds.
- 9 Nothing left behind: only this run's `sdd/work/casing-fix-unsafe-renames/`, which the lead deletes.
