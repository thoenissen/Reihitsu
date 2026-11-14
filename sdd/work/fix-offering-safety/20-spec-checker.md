Role: spec checker
Commit: 37bbff90
Verdict: consistent

Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (slug `fix-offering-safety`, lane behavior change). Re-check of the repair ea52344 against Finding 1 of `18-spec-checker.md`. My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc exactly when required: ok. The repair adds one sentence to the existing code-fix bullet of `sdd/changes/2026-11/fix-offering-safety.md`; no Decision, no new visible output change, no task list, `Status: Accepted` kept.
2. Specs match the diff: ok. Finding 1 is resolved. SHARED-FIX-011 and SHARED-FIX-012 now each state that a Fix All, at any scope, leaves such a document unchanged whether it applies registered fixes one by one or applies fixes without registering them, and fixes the other documents in its scope. `ReihitsuCodeFixProviderBase.CreateFixAllProvider` does this: it asks `MayOfferFixesAsync` (hiding carrier; syntax error when `FormatsThroughFormatter`) per document before the delegate runs and returns the document unchanged otherwise. The batch path goes through `RegisterCodeFixesAsync`, which asks the same check. The change doc sentence matches both requirements, and its "a document with a syntax error and a fix whose result the formatter lays out" matches the `FormatsThroughFormatter` condition. Clarifications only, nothing contradicts the diff, requirement format (normative sentence first, `Why:` present) unchanged.
3. IDs: ok. The repair touches no ID and not `requirement-ids.txt`.
4. Code owners exist: ok. No spec header changed. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`.
5. User documentation: ok. No rule page and no `general-notes.md` affected.
6. Open questions routed: ok. The repair raises none (19 lists none); decision 25 is in `00-lead.md`.
7. Requirements covered: ok. No requirement added; the tests that carry SHARED-FIX-012 for RH3002 and RH7101 (project and solution scope) and `FixAllProviderCreationTests` already cover the new sentences. Architecture project quoted above.
8. Test tags: ok. The repair changes no test; the tags checked in 18 are unchanged.
9. Nothing left behind: ok. `sdd/work/` holds only `fix-offering-safety`, which the lead deletes before publishing.
