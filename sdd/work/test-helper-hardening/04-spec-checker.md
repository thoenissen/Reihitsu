Role: spec checker
Commit: eebeb5d
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The diff against `origin/main` holds 11 C# test files under `src/Reihitsu.Formatter.Test` and `src/Reihitsu.Analyzer.Test` plus this run's `sdd/work/test-helper-hardening/` files. It touches no production file, no `sdd/specs/`, no `sdd/changes/`, no `requirement-ids.txt`, and no `documentation/rules/` page. No `[Requirement(...)]` line is added or removed anywhere in it.

## thoenissen/Reihitsu#1804

### Findings
_None._

### Checked
Test-helper consolidation only; see *Whole run* for the per-check results.

## thoenissen/Reihitsu#1838

### Findings
_None._

### Checked
Test-helper assertion only; the new `FormatterTestsBaseTests` tests verify a test helper and carry no tag (nothing in a spec decides them).

## Whole run

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| none | | | | |

## Checked
1. Change doc: ok, none required (no intended behavior changes; no Visible output changes).
2. Specs match the diff: ok, no behavior changed, no spec touched.
3. IDs: ok, none added; ledger untouched.
4. Code owners: ok, no spec header touched, so `SpecConsistencyTests` is not affected.
5. User documentation: ok, no rule page or diagnostic affected.
6. Open questions: ok. Neither the skeptic's handover nor the test implementer's holds an output question (the lead's `Maintainer answers` are "none yet", consistent). Both follow-up candidates (Formatter.Test `ApplyRule` refusal check; cross-project `VerifyFormatterFixCore` copy) are under `Follow-up candidates` in `00-lead.md`, and written into no spec.
7. Coverage: ok, not applicable (the diff does not touch `sdd/specs/`, so the architecture run is not required and no new requirement needs a covering test).
8. Tags: ok. The test implementer's `Tests` tables list failure classes (FC-015, FC-010, FC-016, FC-003) for the new or rewritten tests and state that no tag was added or removed; the diff confirms it. Existing tags (FMT-STRUCT-003, FMT-BREAK-001, FMT-BREAK-027) are unchanged, including on merged tests. No `Examples for the tests` exists (no spec author).
9. Leftovers: ok. `sdd/work/` holds only `test-helper-hardening/`, this run's; the lead deletes it before publishing.
