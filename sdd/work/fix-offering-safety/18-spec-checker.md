Role: spec checker
Commit: 07746674
Verdict: findings

Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (slug `fix-offering-safety`, lane behavior change). Re-check after repair round 2 (417800b, 66a181f, 079709c, 5517687; the diff is `bc39767c..HEAD`). My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/shared/code-fixes.md` SHARED-FIX-012 (and SHARED-FIX-011); `sdd/changes/2026-11/fix-offering-safety.md` What, line 15 | The round makes two providers' custom Fix All (RH3002, RH7101; the only `FixAllProvider.Create(delegate)` users) return a document with a hiding carrier, or (RH7101) a syntax error, unchanged through the new `ReihitsuCodeFixProviderBase.CreateFixAllProvider`. SHARED-FIX-012 says only "The fix MUST decide it when it registers, before it offers an action", and the change doc says "Every fix learns this when it registers". A Fix All that applies fixes without registering them is not stated, so the four new SHARED-FIX-012 tests (RH3002, RH7101, project and solution scope) and the `FixAllProviderCreationTests` architecture test (tagged SHARED-FIX-011 and SHARED-FIX-012) rest on an inference. Neither document says that a Fix All, whatever path it takes, leaves such a document unchanged. SHARED-FIX-011 names no Fix All either. It is a clarification of existing behavior (no new visible output; the Fix All of a document with a carrier already must not edit it per "wherever its edit lies"), so it needs a sentence in each requirement and the change doc's What bullet, not a new requirement or Decision. | spec author |

## Checked
1. Change doc exactly when required: ok apart from finding 1. No new visible output change; the round fixes the Fix All paths against SHARED-FIX-012's existing "every code fix", and the word-boundary rows pin FMT-PIPE-033's existing text. Change doc still `Accepted`, no task list.
2. Specs match the diff: finding 1. Everything else agrees: FMT-PIPE-033 (word-boundary rows `#else// c`, `#elif(Y)` are lines that end the group; `#elsewhere`, `#endif2`, `#else_1`, `#elifx Y` are not), CLI-021 ("by the definition of FMT-PIPE-033", unchanged behavior through the new `ISourceFormatter.HoldsHidingCarrier`), SHARED-FIX-011 (RH7101 sets `FormatsThroughFormatter`, RH3002 does not, as the spec's "formats through the formatter" condition needs).
3. IDs: ok. No spec file and no `requirement-ids.txt` change in the round.
4. Code owners exist: ok. No spec header changed; `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118` (the 118th is the new test). No owner entry is required for the additions: `shared/code-fixes.md` lists `Code owners | —` and applies to all providers, so `ReihitsuCodeFixProviderBase` and its new `CreateFixAllProvider` need no entry; `ISourceFormatter` is an internal interface (the CLI spec lists `DefaultSourceFormatter`, which still exists and implements the new member); `FixAllProviderCreationTests` is a test type. `sdd/specs/README.md` unaffected.
5. User documentation: ok. No rule page or `general-notes.md` is affected; a Fix All that leaves a document unchanged is behavior `general-notes.md` already covers under the shared conditions.
6. Open questions routed: ok. 14 to 17 raise none; decisions 21 to 24 are in `00-lead.md`.
7. Requirements covered: ok. No requirement added. The architecture project passed (118/118, quoted above).
8. Test tags: ok. The five new Fix All tests (RH3002 x2, RH7101 x2, RH3102 x1) carry `[Requirement("SHARED-FIX-012")]`; the architecture test carries SHARED-FIX-011 and SHARED-FIX-012; both match the `Tests` tables of 14 and 16. The new DataRow rows extend existing FMT-PIPE-033-tagged tests, which already carry the tag.
9. Nothing left behind: ok. `sdd/work/` holds only this run's `fix-offering-safety` (the lead deletes it before publishing).
