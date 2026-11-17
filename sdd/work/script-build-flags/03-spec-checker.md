Role: spec checker
Commit: b2ee9bc
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok — the diff drops build flags in three scripts, changing no intended behavior; none required, none added.
2. Specs match the diff: ok — the diff touches no `sdd/specs/` file, and no spec or documentation mentions the removed flags (`rg` over scripts, specs, documentation, .github, squad: only the historical note `squad/notes/2026-11-17-s1125-format-build.md:43`).
3. IDs: ok — no requirement added, changed, or retired.
4. Code owners: ok — no spec header or owner touched; the architecture suite was not run because the diff touches no spec.
5. User documentation: ok — no rule page or diagnostic affected.
6. Open questions: ok — no spec author or skeptic ran; `00-lead.md` has no open output question, and the implementer's follow-up (global `NoWarn` replacing project `NoWarn`) is moot and recorded as dropped in decision 2.
7. Requirements covered: ok — `sdd/specs/` untouched, so nothing to cover and no architecture run required.
8. Test tags: ok — the diff adds no test.
9. Nothing left behind: ok — `sdd/work/` holds only this run's `script-build-flags`, which the lead deletes before publishing.
