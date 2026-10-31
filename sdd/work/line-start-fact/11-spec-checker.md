Role: spec checker
Commit: a339287
Verdict: consistent

The `fixed` addition after publication (a681e8e..a339287): RH6001-001 lists `fixed`, and the formatter spaces `fixed` (FMT-SPACE-012). This pass also compares against every spec that RH6001 and FMT-SPACE-012 name or that name them. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-10/fixed-keyword-spacing.md` is required, because the formatter output changes: FMT-SPACE-012 now includes `fixed`. It is `Accepted` (accepted in advance, recorded in `squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md`) and has no task list. The RH6001-001 keyword addition is a clarification and needs none.
2. Specs match the diff, including the cross-spec comparison: ok.
   - **RH6001.md:** `Uses` is FMT-SPACE-012 and SHARED-BLANK-008. Its *Why* ("formatted code raises no RH6001") now holds, because `KeywordSpacingRule` gained `FixedKeyword`, and `fixed/* c */(` becomes `fixed/* c */ (` (FMT-SPACE-013).
   - **FMT-SPACE-012:** lists `fixed` with the new example, and no exception clause applies to it. The code case list matches the spec list.
   - **Specs that name FMT-SPACE-012:** RH6005, RH6022, RH6021, RH5402, RH6016 and token-gap-fixes refer to `operator`, `new[]` or the requirement in general, and none enumerates the keyword list.
   - **Other keyword lists:** the lists in indentation (FMT-INDENT-004), blank-lines, structural-transforms, comment-placement and statement-blocks already contain `fixed` and are unaffected.
   - **Fixed-size buffer:** `fixed int _buffer[4];` keeps one space, which the change doc states is unchanged output. FMT-SPACE-012's "exactly one space" and FMT-SPACE-002's collapse give the same result, and the tests pin it.
   - **Diagnostics:** the change doc's SHARED-DIAG-010 reference is consistent.
3. IDs: ok. None added or changed.
4. Code owners: ok. Both specs name only existing types.
5. User documentation: ok. `documentation/rules/RH6001.md` lists `fixed`, matching the spec.
6. Open questions: ok. The `fixed` question was answered (follow-ups decision), and the spacing question was decided (fixed-spacing decision). None open.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 89, Total: 89`.
8. Tags: ok.
   - **FMT-SPACE-012:**
     - `OneSpaceAfterFixedKeyword`;
     - `OneSpaceAfterEachOfNestedFixedKeywords`;
     - `SeveralSpacesAfterFixedKeywordBecomeOne`;
     - `SpacedFixedKeywordIsKept`;
     - `FixedSizeBufferKeepsOneSpaceAfterFixedKeyword`;
     - `SeveralSpacesInFixedSizeBufferBecomeOne`;
     - `OneSpaceAfterFixedKeywordBehindBlockComment`, which also carries FMT-SPACE-013.
   - **RH6001:** the formatter test `VerifyFormatterFixesGluedFixedKeyword` carries RH6001-001. The analyzer test for the glued keyword carries RH6001-001/-003/-004, and the stray-`///` `fixed` test carries RH6001-002.
   - These match `07-test-implementer.md` and `09-test-implementer.md`, and the nested test carries no FMT-INDENT-004 tag because indentation is not what it verifies.
9. Nothing left behind: ok. `sdd/work/` holds only `line-start-fact/`.
