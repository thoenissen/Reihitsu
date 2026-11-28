Role: spec checker
Commit: e99fc2b
Verdict: consistent

My prompt carried no intended fix or verdict; ignored as instructed. Repair round 1, diff `origin/main...HEAD` at e99fc2b.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Tooling only; no intended behavior of a spec-covered surface changes, so none is required.
2. Specs match the diff: ok. `sdd/specs/` is untouched and no spec covers the fixture runner. `scripts/README.md` (`invalid-result` row, `--lang-version` and exit-code paragraphs), `FixtureOutcome.InvalidResult`, the `RunAsync` and `CodeFixRunCommand` param docs, and the inline comment all describe decision 2 (per ID, distinct messages above the fixture's, a missing ID from zero). No stale "ID and message" or `IsSubsetOf` text remains in source, tests, or the README.
3. IDs: ok. No requirement added or changed.
4. Code owners: ok. No spec header touched; the architecture suite was not rerun (the implementer reports 198 passing).
5. User documentation: ok. No rule page is affected.
6. Open questions: ok. No output question; decisions 1 and 2 are in `00-lead.md`.
7. Requirement coverage: ok. No `sdd/specs/` change.
8. Test tags: ok. No requirement applies, so no `[Requirement]` tag is expected.
9. Nothing left behind: ok. `sdd/work/` holds only this run.
