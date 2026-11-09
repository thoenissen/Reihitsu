Role: spec checker
Commit: dc8823f
Verdict: consistent

## thoenissen/Reihitsu#1597

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (clarifications only, SHARED-CASING-011 and -012 withhold fixes for a rare class; no change doc, none in the diff). 2 ok (the diff's `IsCompilerLookupMethodRename` list equals the 21 names of SHARED-CASING-011, local functions excluded; `HasCallerArgumentExpressionNaming` matches SHARED-CASING-012 including the `nameof` exemption and the cascade scope; the SHARED-CASING-007 bullet and SHARED-FIX-003 sentence match). 3 ok (011 and 012 follow 010 in `requirement-ids.txt`, which is sorted). 4 ok. 5 ok (no rule page affected; no rule page changed). 6 ok (no output questions). 7 ok. 8 ok (all new SHARED-CASING-011/012 tests carry their tag; the decision-11 test is tagged SHARED-FIX-003 as the 05 table lists it). 9 ok.

## thoenissen/Reihitsu#1598

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (no spec or code change; five characterization tests only). 2 ok (SHARED-CASING-007 already states the region). 3-6 ok (not applicable). 7 ok. 8 ok (the five tests carry SHARED-CASING-007 and SHARED-FIX-003 as the 03 table lists). 9 ok.

## thoenissen/Reihitsu#1016

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (clarification of SHARED-CASING-008, withholding only). 2 ok (`_analyzer.IsValidName` in `TryGetFixedIdentifier` matches the new third condition; examples match the tests). 3 ok. 4 ok (`CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>` exist; SHARED-FIX-001 added to `Uses`). 5 ok (no rule page affected). 6 ok. 7 ok. 8 ok (RH4106, RH4002, RH4105, RH4005 tests tagged SHARED-CASING-008 and their rule requirements). 9 ok.

## thoenissen/Reihitsu#1015

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (SHARED-FIX-002 and RH4102-003 already required the behavior; bug fix). 2 ok. 3 ok. 4 ok (`RH4102CustomEventNameCasingCodeFixProvider` is removed from `sdd/specs/analyzer/RH4102.md` and `sdd/specs/README.md`; the type is deleted). 5 ok. 6 ok. 7 ok. 8 ok (the three RH4102 Fix All tests carry SHARED-FIX-002 or SHARED-CASING-010 plus RH4102-003). 9 ok.

## thoenissen/Reihitsu#1084

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (RH4005-002 clarified with an example, no new behavior). 2 ok (the `_`, `__`, `_2x`, `注文` example matches the tests). 3 ok. 4 ok. 5 ok. 6 ok. 7 ok. 8 ok (RH4005 tests tagged RH4005-002 and SHARED-CASING-008). 9 ok.

## Whole run

- `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.
- `sdd/work/` holds only `casing-fix-safety/`; the lead deletes it before publishing. No `documentation/` or `sdd/changes/` file is in the diff.
- Observation, not a finding: `RH4114...NoFixIsOfferedWhenTheRenameReachesAPositionalParameterThatACallerArgumentExpressionNames` (decision 11) exercises SHARED-CASING-012 but is tagged SHARED-FIX-003; the tag matches the 05 table, so it stays.
