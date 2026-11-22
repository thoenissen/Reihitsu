Role: spec checker
Commit: 280a901
Verdict: consistent

Repair round 1, thoenissen/Reihitsu#1929 only (answer 9A, decision 8, audit finding #1). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Scope: `git diff 9caef70..HEAD` (specs, change doc, tests, work folder; no `src/` and no ledger change).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

## Checked
1. Change doc: ok. `pattern-anchor-behind-wrapper.md` stays `Accepted`; title and *What* widened to the 9A members; *Visible output changes* lists each member with before/after columns; the `Decisions` section records 9A ("The maintainer chose A for question 9") and the rejected alternative B. The change doc already existed, so none is newly required.
2. Specs match the diff: ok. FMT-INDENT-026 now names typed property patterns and parenthesized patterns behind `and`/`or`/`not` in arms, list elements, slices, and `case` labels. Its added example (input and output) is the pair the change doc and the tests carry; the earlier examples now compile (`P o`, `string value`) with columns unchanged. Format kept (normative first, `Why:` present, examples).
3. IDs: ok. No ID added, changed, or removed; `requirement-ids.txt` untouched.
4. Code owners: ok. No header or owner changed; architecture project passes.
5. User documentation: ok. No rule page affected. RH5206's spec needs no change (RH5206-001 and -003 state no column behind a wrapper); the change doc's *Code fixes* section covers the typed arm.
6. Open questions: ok. Question 9 has its answer (9A) under `Maintainer answers` in `00-lead.md`; none written into the spec or recorded only in the PR.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 198, Skipped 0, Total 198. FMT-INDENT-026 is covered by 14 new `[Requirement("FMT-INDENT-026")]` tests, one per member of the spec author's table in `12-spec-author.md`, plus the whole-example test.
8. Test tags: ok. All 15 new formatter tests carry `[Requirement("FMT-INDENT-026")]`; the new RH5206 test carries `RH5206-003` and `FMT-INDENT-026`, as the `Tests` table in `13-test-implementer.md` lists. Both spec-example tests (decision 8, `14-test-implementer.md`) keep their tag and match the corrected spec blocks per that handover. Every member under `Examples for the tests` is tagged on at least one test.
9. Nothing left behind: ok. Only this run's `sdd/work/list-pattern-columns/` exists; the lead deletes it before publishing.
