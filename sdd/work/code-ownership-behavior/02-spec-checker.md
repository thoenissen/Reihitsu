Role: spec checker
Commit: 2532a68
Verdict: consistent

No intended-fix, suspected-cause, or hoped-for-verdict text was present in the inputs (`00-lead.md`, `01-spec-author.md`, task prompt); nothing to ignore.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None | – |

## Checked
Diff: `git diff origin/main...HEAD` (base f6e897c) touches `sdd/README.md` (one bullet, *Spec format -> Code owners*) and the two `sdd/work/code-ownership-behavior/` handovers only. `scripts/lane-check.sh docs`: "LANE CHECK: confirmed - claimed docs, diff needs docs"; text-only proof PASS (3 non-compiled files).

1. Change doc exactly when required: ok. No intended behavior changes (rulebook wording clarified, no requirement, rule, phase, or surface added or removed), so none is required and none was added. `sdd/changes/2026-10/introduce-sdd.md` line 92 states only "A type is a code owner of at most one spec." and does not draw the shared-spec / general-purpose-utility distinction, so the issue's amend clause does not apply; the file is unchanged against `origin/main` and stays consistent with the clarified bullet.
2. Specs match the diff: ok. No spec changed. The added text matches the sentences prescribed in thoenissen/Reihitsu#1008 in meaning: behavior decides, not callers; a spec-specified type belongs to that spec even when called from outside; callers list relied-on IDs under `Uses`; a general-purpose utility is a type whose behavior no spec specifies (syntax or trivia helper). Polish only (first two sentences joined with a colon, "the spec of each such caller"). The new text agrees with the existing `Uses` definition in the same bullet list.
3. IDs: ok. No requirement ID added, changed, or removed; `requirement-ids.txt` untouched.
4. Code owners exist: ok. No spec header and no `sdd/specs/README.md` row changed; no code-owner row moves (the issue excludes it). The architecture `SpecConsistencyTests` were not run because nothing under `sdd/specs/` changed.
5. User documentation: ok. No rule page, code fix, or diagnostic is affected.
6. Open questions: ok. The spec author's handover lists none.
7. Requirements covered and specs consistent: ok (not applicable). The diff does not touch `sdd/specs/`, so `scripts/test.sh --project architecture` was not required and not run; no new requirement exists to cover.
8. Nothing left behind: ok for this check's scope. `sdd/work/` holds only this run's directory `code-ownership-behavior`; the lead deletes it before publishing (the issue's done-when requires a PR with no `sdd/work/`).

Follow-up candidates: none.
