Role: spec checker
Commit: 7385d7b30e0df624e31f9f357a1e894b944cdd09
Verdict: consistent

Repair round 1 check of the whole diff `origin/main...HEAD` (base b673099), focused on `5a1e702..HEAD`. The prompt carried no intended fix or hoped-for verdict.

## thoenissen/Reihitsu#1597

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1 ok (SHARED-CASING-012 is clarified again; it withholds fixes for a rare class, decisions 13, 16, 17; no requirement added, changed in intent or retired; no change doc in the diff, none required; no output question). 2 ok: the normative text (inspected lists: cascade-declaring list and extension-block member lists; exemption only for a single-identifier `nameof`, old name of a cascade parameter of the same list, C# 11 or later) matches `HasCallerArgumentExpressionNaming` (both overloads), `GetParameters`, and `IsExemptNameofArgument`: exempt names exclude the new name and are grouped per owner, extension members get an empty exempt set, the receiver's own list keeps the exemption. The *Why* now states a conservative refusal and no longer asserts C# 10 binding (decision 17); `grep` finds no other sentence on pre-C# 11 `nameof` binding. Examples (a), (b), (c), (d) and the C# 11 counterpart are in the spec and each has a test. The SHARED-FIX-003 *Run time* sentence, the RH4105-003 and RH4120-003 wording match. 3 ok (no new ID this round; SHARED-CASING-011 and -012 are in `requirement-ids.txt` after -010). 4 ok (`sdd/specs/README.md` row for RH4102 matches the deleted type; architecture suite passes). 5 ok (no rule page and no `documentation/` file in the diff). 6 ok (no output questions in handovers 11-15; decisions 13-17 are in `00-lead.md`). 7 ok: `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`. 8 ok: the seven new RH4105 tests carry `SHARED-CASING-012` and `RH4105-003` as the 12 table lists, and every example (a)-(d) is tagged on at least one test. 9 ok.

## thoenissen/Reihitsu#1598

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
1-9 ok. The repair changed only the linked-document assertion in `ApplyFixInMultiTargetSolutionAsync`; no spec or tag changed, the five tests keep SHARED-CASING-007 and SHARED-FIX-003.

## thoenissen/Reihitsu#1016

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
Unchanged since 09-spec-checker.md; 1-9 ok.

## thoenissen/Reihitsu#1015

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
Unchanged since 09-spec-checker.md; 1-9 ok.

## thoenissen/Reihitsu#1084

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

### Checked
Unchanged since 09-spec-checker.md; 1-9 ok.

## Whole run

- `sdd/work/` holds only `casing-fix-safety/`; the lead deletes it before publishing. No `documentation/` or `sdd/changes/` file is in the diff.
- Observation, not a finding: the receiver list of an extension block keeps the cascade exemption (implementer's handover 13 notes no test covers a receiver that carries `CallerArgumentExpression` itself); the spec's first bullet includes that list, so spec and code agree.
