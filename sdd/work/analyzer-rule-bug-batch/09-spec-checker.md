Role: spec checker
Commit: a24062a
Verdict: #1074: consistent; #1079: consistent; #1276: consistent; #1613: consistent; #1708: consistent

## thoenissen/Reihitsu#1074

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: no change doc; RH5601-001/RH5602-001/RH6009-001/RH6010-001 are rewritten in place as a clarification to SHARED-DIAG-003 (00-lead.md decision 4, triage comment).
2. ok: RH5601/RH5602/RH6009/RH6010 specs match the diff (`IsInsideLiteralText`, `IsInterpolationBrace`); format of the changed requirements holds.
3. ok: no new IDs.
4. ok: no owner renamed/added/removed; the architecture run below passes.
5. ok: no rule page changed; the spec author's note that the pages stay true is consistent.
6. ok: no output questions in handovers 01/02.
7. ok: no new requirement; changed requirements are covered by tagged tests.
8. ok: every added test carries its requirements; the 03 table matches; the examples named under `Examples for the tests` are tagged (`VerifyEveryTabOfTheRequirementExampleIsReported`, `VerifyEveryRunOfTheRequirementExampleIsReported`, `VerifyEveryBraceOfTheRequirementExampleIsReported`, `VerifyTheRequirementFixExampleIsFixed`).
9. see Whole run.

## thoenissen/Reihitsu#1079

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: clarification of RH5603-001/-003/-004 on the maintainer's recorded decision; no change doc needed.
2. ok: spec matches `RH5603FileMustNotEndWithANewlineAnalyzer` (tail starts at the first line break after the last non-whitespace character; `}··⏎` keeps its spaces); `·` marker declared; `Uses` gains SHARED-FIX-001.
3. ok: no new IDs.
4. ok. 5. ok: rule page already says "and any blank lines".
6. ok: the RH5603 line-break-behind-directive question is under `Follow-up candidates` in 00-lead.md (candidate 1), not in the spec.
7. ok: RH5603-001/-003/-004 tagged tests exist (analyzer and formatter).
8. ok: the two untagged RH5603 tests (lone `\r`, `}⏎···` without final break) are listed in 03 as untagged characterizations of non-spec behavior; not findings.

## thoenissen/Reihitsu#1276

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: clarification; withholding the fix is decision 3 and matches the existing RH5404 page.
2. ok: RH5404-001/-002/-003 describe the diff (keyword location, no fix for an extension block); `Uses` lists SHARED-DIAG-007 and SHARED-FIX-001, which `UsesListsExactlyTheReferencedRequirements` accepts.
3. ok. 4. ok.
5. ok: `documentation/rules/RH5404.md` ("except for extension blocks, which have to be expanded by hand") agrees; no formatter mention added.
6. ok: no output question.
7. ok. 8. ok: RH5404-001/-002/-003 tags on the added tests, including the requirement-example test.

## thoenissen/Reihitsu#1613

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: clarification of RH5202-001/-003. 2. ok: lone-`\r` examples match the shared `RawStringLiteralUtilities` closing-line decision used by analyzer and fix. 3. ok. 4. ok: `RawStringLiteralUtilities` is an existing Core type already owned by formatter/raw-string-alignment.md; the RH5202 owners need no change. 5. ok: page unchanged, states alignment only. 6. ok. 7. ok. 8. ok: aligned tests carry RH5202-001, fix tests RH5202-003 (and -001 where they also verify reporting), as the skeptic asked.

## thoenissen/Reihitsu#1708

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
1. ok: RH4105-004 is a clarification of SHARED-DIAG-007 behavior (decided by the lead; no change doc). 2. ok: requirement has a normative sentence, `Why:`, example. 3. ok: `RH4105-004` is the next free number and sits sorted after `RH4105-003` in `requirement-ids.txt`; nothing else in the ledger changed. 4. ok. 5. ok: no rule page change. 6. ok. 7. ok: RH4105-004 is covered by three `[Requirement("RH4105-004")]` tests. 8. ok: the dropped `ref` test (decision 12) is gone; no spec text references it.

## Whole run

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

### Checked
- 1: no change doc is in the diff (`sdd/changes/` untouched); none is required for a bug-fix lane run with clarifications only.
- 7: `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:   117, Skipped:     0, Total:   117`.
- 9: the branch's only `sdd/work/` is `sdd/work/analyzer-rule-bug-batch/`; the lead deletes it before publishing.
- Note for the lead (not a finding): the `Worklist / next step` in `00-lead.md` still says the ref-case drop and the cleaner are next; both are done (07, 08).
