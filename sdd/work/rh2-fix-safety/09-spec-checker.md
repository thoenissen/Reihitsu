Role: spec checker
Commit: 6357087
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Diff checked: `162e7e6..HEAD`. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. Run-wide results (all four issues share the specs, so the sections below point to the shared rows).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 6 | `00-lead.md` decision 10; `03-spec-author.md` RH2001 output question | The spec author's output question (get-only property of a non-readonly struct, and a type parameter not constrained to a reference type) was answered by the lead under `Decisions taken`, not under `Maintainer answers`. The get-only half follows the literal answer 2C ("every struct not declared readonly"). The type-parameter half (RH2001-008, RH2001-004 coverage, and its `C<T, TRef>` example) goes beyond "struct", and a type-parameter property loses a fix it has on `main` (visible). Nothing records the maintainer's answer for it. Either the lead batches that half to the maintainer before the auditor, or states in the PR that it is a decision of the lead under "keep it simple". | lead (not a role fix) |

No other findings.

## thoenissen/Reihitsu#967
- RH2005-003, RH2004-006 (placement behind attribute lists), RH2005-004 (withheld outside use): specs match the diff (`DeclarationModifierUtilities`, RH2005 fix provider, `ExcludedCodeUtilities`); IDs RH2005-004 in the ledger at its sorted position; tests tagged RH2005-003, RH2005-004, RH2004-006.
- Rule pages `RH2004.md`, `RH2005.md` unchanged; their `### Correction` still equals the fix output (the fix output for the pages' own examples did not change).

## thoenissen/Reihitsu#1286
- RH2001-004 (coverage, `readonly` once, `Nullable<T>`), RH2001-007, RH2001-008 match the diff and the formatted examples; IDs RH2001-007/-008 in the ledger. Tests tagged RH2001-004/-007/-008, SHARED-CASING-007, SHARED-FIX-003.
- `ExcludedCodeUtilities` is a code owner of `shared/casing.md` and listed in `sdd/specs/README.md`; `CasingCodeFixProviderBase<T>` stays. RH2001 `Uses` gains SHARED-FIX-003. See finding 1 for the type-parameter half of RH2001-008.

## thoenissen/Reihitsu#1287
- SHARED-DIAG-012 added (ledger ok, sorted), `PartialMemberUtilities` owner in `shared/diagnostics.md` and the index; RH2002-001, RH2004-001, RH8306-001, RH7401–RH7412-001, RH2101-002, RH7002-001/-004 match the diff; `Uses` lists SHARED-DIAG-004/-012 where the rules rely on them. Tests tagged SHARED-DIAG-012 and each rule's -001 (all of RH7401–RH7412), RH2101-002, RH7002-004.
- Judgment call, not a finding: SHARED-DIAG-012 is a new requirement, recorded as a clarification of SHARED-DIAG-004 under the maintainer's answer Q1 = A and the lane decision in `00-lead.md` (decision 2); no change doc, consistent with the lead's decision.
- Follow-up candidates 1 (SHARED-DIAG-004 wording), 2 (generated-code unification), 3 (#1334 note) are under `Follow-up candidates` in `00-lead.md`.

## thoenissen/Reihitsu#1288
- RH2004-003 now names operators and conversion operators; diff (`ShouldSkip`) matches; example added; test tagged RH2004-003. Maintainer's round 7b answer is recorded in the issue and `00-lead.md`.

## Checked
1. Change doc: none present, none required for a bug fix plus clarifications (see judgment call under #1287); ok.
2. Specs match the diff: ok. Format (one heading per requirement, normative sentence, `Why:`, markup) ok.
3. IDs: ok. RH2001-007, RH2001-008, RH2005-004, RH7002-004, SHARED-DIAG-012 added at sorted positions; nothing renumbered or deleted.
4. Code owners: ok. `SpecConsistencyTests` pass; `PartialMemberUtilities` and `ExcludedCodeUtilities` listed in specs and `sdd/specs/README.md`.
5. User documentation: ok. No rule page changed or needed.
6. Open questions: finding 1. Q1–Q3 answered under `Maintainer answers`; follow-ups outside the run are under `Follow-up candidates`; none in a spec.
7. Coverage: ok. Architecture project 115 passed; every new active requirement has a tagged test.
8. Tags: ok. Every added test method carries a `[Requirement]` tag except `VerifyFixKeepsUppercaseRunOfLeadingAcronymInFieldName`, a pre-existing characterization test whose declaration only appears moved in the diff (verifies no requirement, not a finding). Every requirement under `Examples for the tests` is tagged on an added test.
9. Nothing left behind: ok. `sdd/work/` holds only `rh2-fix-safety`, which the lead deletes before publishing.
