Role: spec checker
Commit: a353709
Verdict: consistent

Architecture-finding refusal (lead decision 6); the diff against `origin/main`, with emphasis on 46cf659..a353709 (handovers 15 to 18). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. Change doc: ok. `Status` stays `Accepted`; the What bullet and the Decisions section "A union whose documentation holds a parameter element is not judged" carry any letter case and the failed expansion; input, before (not reported on `main`) and after (not reported) are stated, so no visible output change and no maintainer answer is owed; a rejected alternative is listed; no task list.
2. Specs match the diff: ok. SHARED-DOC-019 (a) defines the parameter element as `param`/`paramref` in any letter case, prefix ignored, which matches `HasParameterElement` using `GetTagsIncludingNested`; (b) (no exception, nothing reported when the expansion fails, every part of a partial union) matches `IsUnjudgedUnion` plus `XmlDocumentationExpander.CanExpandDocumentation`, which catches all but `OperationCanceledException`. The new example follows the format. RH8035-001, RH8036-001, RH8108-001, RH8203-001 carry the widened clause; `Uses` rows unchanged and still correct (the helper is called by exactly those four rules). The spec names no mechanism.
3. IDs: ok. No new ID in this step; `requirement-ids.txt` untouched since 46cf659; nothing reused or removed.
4. Code owners: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. The renamed helper is internal and named in no spec header; `sdd/specs/README.md` unchanged and still matches.
5. User documentation: ok. No rule page changed in this step and none needs to: the refusal is a corner case that stays out of the pages (decision 6).
6. Open questions: ok. 15 has no output question; decision 6 and 7 are recorded in `00-lead.md`; the unverified `include` route was tested by 16 and is no new member of the class; follow-up candidates unchanged.
7. Coverage: ok. SHARED-DOC-019 (a) and (b) are covered by tagged tests for RH8035, RH8036, RH8108, and RH8203, in Diagnose and Parse mode and for partial unions. The spec records that (b) has no falsifying input today (16, "Not covered").
8. Tags: ok. All 20 test methods added since 46cf659 carry `[Requirement("<rule>-001")]` and `[Requirement("SHARED-DOC-019")]`, as the `Tests` table in 16 states; every example under `Examples for the tests` in 15 (the three RH8035 spellings, RH8036, the RH8203 counterexample, RH8108, prefixed tag, include) is tagged on at least one added test.
9. Leftovers: ok. `sdd/work/` holds only `union-naming-documentation`.
