Role: spec checker
Commit: c0d32d5
Verdict: consistent

Architecture-finding refusal (lead decision 8); the diff against `origin/main`, with emphasis on cbac9ac..c0d32d5 (handovers 21 to 23). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. Change doc: ok. No behavior change in this step; `Accepted`; the refusal stays a rare-class refusal with no visible output change (input does not compile, not reported, as on `main`), so no maintainer answer is owed. The change doc still says "any part, for a partial union", which the broader reading does not contradict.
2. Specs match the diff: ok. SHARED-DOC-019 (`element-documentation.md:1517`) says "a documentation comment of one of its declarations"; the code now reads every declaration of the union symbol whatever its modifiers, which is what that sentence demands. The later sentence "for a partial union declared in several parts, this holds for every part" is a special case of it, not a contradiction; decision 8 says no spec change, and none is needed. `IsUnjudgedUnion` summary matches. Spec names no mechanism.
3. IDs: ok. No spec or ledger file changed since cbac9ac.
4. Code owners: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. No header touched.
5. User documentation: ok. No rule page changed; none needs to.
6. Open questions: ok. 21 to 23 raise none; decision 8 is recorded in `00-lead.md`; finding #4 lapses as the decision states.
7. Coverage: ok. SHARED-DOC-019 is covered for RH8035, RH8036, RH8108, RH8203 by the 10 added tests, in Diagnose and Parse mode (5 of them failing before 22, passing after, per 22).
8. Tags: ok. All 10 added test methods carry `[Requirement("<rule>-001")]` and `[Requirement("SHARED-DOC-019")]`, as the table in 21 lists; no spec author examples were added in this step.
9. Leftovers: ok. `sdd/work/` holds only `union-naming-documentation`.
