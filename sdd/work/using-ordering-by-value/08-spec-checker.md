Role: spec checker
Commit: c59d749
Verdict: findings

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/analyzer/RH7201.md`, RH7201-001, Unicode-escape example (`using Microsoft.Win32; using {\|RH7201:System.IO\|}; using System;`) | The example marks only `System.IO`. RH7201-001's own text reports every `System`-group directive that follows another root, so `using System;` below `Microsoft.Win32` must be marked `{\|RH7201:System\|}`. `main` already reports it, and the test `UnicodeEscapedSystemDirectiveIsReported` marks both (known: 04-test-implementer, `00-lead.md` decision 6). | spec author |

## Checked
1. Change doc: ok. All three issues are clarifications of existing requirements (SHARED-USING-004/007, RH7203/4/6/7201/7207-001/002); no requirement is added, changed in behavior, or retired, and no rule, phase, or surface is added or removed. No change doc is needed, and none is in the diff.
2. Specs match the diff: finding 1. Everything else matches: by-value names (SHARED-USING-004, with the Q1 answer), every-earlier-directive comparison (RH7203/4/6-001/-002 and SHARED-USING-007), non-name alias as last group (SHARED-USING-004, RH7207-001/-002/-004). Format (one per heading, `Why:`, markup) holds in the changed requirements.
3. IDs: ok. No ID is added, renumbered, or deleted; `requirement-ids.txt` is untouched.
4. Code owners: ok. No owner header changed except the `Uses` lines (RH7201 gains SHARED-USING-004, SHARED-USING-004's `Uses` gains SHARED-DIAG-002), and the architecture run is green.
5. User documentation: ok. No rule page is changed. The pages carry no "directly above" or neighbor wording that the amended requirements contradict, and no ID range or `helpLinkUri` is touched.
6. Open questions: ok. Q1 is answered under `Maintainer answers`. The SHARED-DIAG-002 wording item and the `OrderUsings` dead-code item are under `Follow-up candidates`. The spec author and skeptic left no unanswered output question.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. No requirement is new; every amended requirement has a `[Requirement]` test in the diff.
8. Tags: ok. Every added test carries `[Requirement]` for the requirements listed in the test implementer's `Tests` tables. Every requirement named under `Examples for the tests` is tagged on an added test (SHARED-USING-004, SHARED-USING-007, RH7201-001, RH7204-001, RH7206-001, RH7207-001, RH7207-002). The corrected pre-existing RH7203 test keeps its tags.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory.
