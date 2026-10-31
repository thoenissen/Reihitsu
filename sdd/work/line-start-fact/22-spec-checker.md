Role: spec checker
Commit: 347f39a
Verdict: findings

Repair 2 (21befc2..347f39a). The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 6 | `19-spec-author.md` (follow-up candidate on `fixed`), `20-implementer.md` | The analyzer checks `fixed` but RH6001-001 does not list it. Whether the list gains `fixed` or the analyzer drops it is a question about intended behavior. The spec author wrote "the maintainer decides" but filed it as a follow-up candidate, and no decision file or lead entry shows that it reached the maintainer. It is not a blocker for this run: the mismatch predates the run, and the spec is not changed for it. The lead must put it in the message to the maintainer, or in a follow-up issue the maintainer approves. | lead |

## Checked
1. Change doc: ok. The addition (RH6001 in the analyzer-consequences section, RH6001-002 in `Affected requirements`) is marked *(added 2026-10-31)*, covered by the note on provisional acceptance, and the status stays `Accepted`.
2. Specs match the diff: ok. RH6001-002 now states that the `(` starts its line, also behind a `///` written behind the keyword (SHARED-BLANK-008). The negative example `if/// stray` / `(a > 2)` is added and `Uses` adds SHARED-BLANK-008. RH6001-001 already reports only with no line break, so the spec and the code agree.
3. IDs: ok. No ID added or changed.
4. Code owners: ok. No owner renamed. `SpecConsistencyTests` pass.
5. User documentation: ok. No rule page is affected.
6. Open questions: finding 1.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 89, Total: 89`.
8. Tags: ok. All nine `Verify…KeywordFollowedByStrayDocumentationCommentIsNotReported` tests except `fixed`, plus the formatter test, carry `RH6001-002`. The `//` counterparts and the `fixed` test are untagged. Leaving `fixed` untagged is correct: the spec does not list `fixed`, so no requirement covers it.
9. Nothing left behind: ok. `sdd/work/` holds only `line-start-fact/`.
