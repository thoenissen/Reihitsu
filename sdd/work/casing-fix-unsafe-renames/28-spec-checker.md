Role: spec checker
Commit: 9162fc0 (head before this handover)
Verdict: consistent

Re-check after decisions 19 and 20 (`24-spec-author.md` to `27-cleaner.md`), against `git diff 20b2618 HEAD` and the whole diff to `origin/main`. No prompt-supplied fix or verdict was followed.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._ Decision 20 question: SHARED-CASING-007 agrees with the code. The cascade is defined as the renamer's own determination, and the old-name bullet defines old names as those of cascade symbols that source can refer to by name, with symbols the renamer leaves alone (the other part's differently spelled type parameter) adding none. `GetOldNames` reads the names the renamer's result no longer carries, so it implements that statement and predicts nothing. The reference search is a superset only for the documents scanned and the native entry points, which can only withhold more; the auditor's probe found no extra document. The spec's wording of that region ("a document where a symbol of the cascade can be referred to") is the intended set, and decision 20 records that. The partial-method type-parameter example ("of the same name") and the new negative example (`Run<Key>` / `Run<TItem>`) match the test that pins the offered fix.

## Checked
- 1 Change doc: ok. Clarification and a guard that only withholds under SHARED-FIX-003; no ID added, changed, or retired.
- 2 Specs match the diff: ok. Format kept; the new RH4105 example, its prose cases (reverse direction, indexer, offered side) and the RH4121 negative side match the six new tests and the code.
- 3 IDs: ok, `requirement-ids.txt` untouched since 20b2618.
- 4 Code owners: ok, no spec header touched.
- 5 User documentation: ok, no rule page touched.
- 6 Open questions: ok, none raised; the spec author lists none.
- 7 Coverage: `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. No requirement added.
- 8 Tags: ok. Five new RH4105 tests carry SHARED-CASING-007; the four withholding ones also SHARED-FIX-003, as `25-test-implementer.md` lists; the RH4121 test carries SHARED-CASING-007. Every example for the tests named by the spec author is tagged.
- 9 Nothing left behind: ok, only this run's `sdd/work/casing-fix-unsafe-renames/`, which the lead deletes.
