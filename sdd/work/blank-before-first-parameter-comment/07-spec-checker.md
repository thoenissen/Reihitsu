Role: spec checker
Commit: 8e2745c
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/first-comment-behind-opening-token.md` is `Accepted`, has `Visible output changes` and no task list; the maintainer's answer 1C is under `Decisions`.
2. Specs match the diff: ok. FMT-BLANK-011, RH5020-002, RH8303-002 are changed with `Why:` and examples; the rule-page and spec token lists agree.
3. IDs: ok. No ID added, removed, or renumbered; `requirement-ids.txt` is untouched.
4. Code owners: ok. No spec header or `sdd/specs/README.md` entry changed; SpecConsistencyTests pass inside the architecture run.
5. User documentation: ok. RH5020 and RH8303 pages widen the exemption bullet in short prose, with no formatter mention and no withheld-fix note; no code fix output changed, so `### Correction` is unaffected.
6. Open questions: ok. The one output question is answered under `Maintainer answers`; the follow-up candidates are under `Follow-up candidates`; none is in a spec.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 153, Skipped: 0, Total: 153`. Every changed requirement has covering `[Requirement]` tests.
8. Tags: ok. Every added test method carries a `[Requirement]` tag, the tags match the `Tests` table in `04-test-implementer.md`, and the requirements under `Examples for the tests` (FMT-BLANK-011, RH5020-002, RH8303-002) are tagged on new tests.
9. Leftovers: ok. `sdd/work/` holds only this run's directory.
