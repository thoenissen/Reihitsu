Role: spec checker
Commit: ad58197b (base; this handover's commit follows it)
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1087

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/range-variable-casing.md` *Decisions*, "Every casing fix keeps the name of an anonymous object member" | Records answer 3B only. The maintainer's answer 4A (batch 1b, 2026-11-27: SHARED-CASING-015 applies to every fix using the shared rename, RH4101, RH4102, RH4107 to RH4114, RH4120 too) is in `00-lead.md` and drives case 4 of the doc's *Visible output changes*, but the *Decisions* section does not record it. | spec author |

### Checked
- 1: change doc `local-casing-rule-split.md` is Accepted, has *Visible output changes* and no task list; its *Decisions* record the 2026-10-18 answers, 2A, and 1B. Required: requirements are added and changed (RH4115-004/-005, RH4117-003). See finding 1 for the other doc.
- 2: RH4115-002/-004/-005 and RH4117-002/-003 match the diff (analyzer and tests); every new requirement has `Why:` (RH4115 5/5, RH4117 3/3).
- 3: RH4115-004, -005 and RH4117-003 are in `requirement-ids.txt` at sorted positions; nothing reused or removed.
- 4: owners unchanged for RH4115/RH4117; architecture suite below.
- 5: `documentation/rules/RH4115.md` and `RH4117.md` match the specs, no formatter mention, footer intact (RuleDocumentationConventionTests passes).
- 6, 7, 8: see Whole run.

## thoenissen/Reihitsu#1088

### Findings
_None._

### Checked
- 1: RH4128 is a new rule; covered by `local-casing-rule-split.md`; the RH4128 ID decision is in its *Decisions* and in `00-lead.md` decision 1; answer 2A is recorded.
- 2: `sdd/specs/analyzer/RH4128.md` (3 requirements, 3 `Why:`, markup `{|RH4128:..|}`) matches the analyzer and fix; neighbouring specs RH4110, RH4116, RH4118, RH4124, RH4125 updated to point at RH4128.
- 3: RH4128-001 to -003 inserted in sorted position.
- 4: spec header owners `RH4128LocalConstantCasingAnalyzer` and `RH4128LocalConstantCasingCodeFixProvider` exist; `sdd/specs/README.md` has the row; `Reihitsu.sln`, `Release.ruleset`, and the package README list RH4128 (ID in the Naming range).
- 5: `documentation/rules/RH4128.md` is short, has the footer, no formatter mention, no corner-case notes; `### Correction` (`const int maxRetries = 3;`) equals the fix output; the `helpLinkUri` page exists; the resources carry title and message.

## thoenissen/Reihitsu#1978

### Findings
_None beyond finding 1, which sits in this issue's change doc and is listed under #1087._

### Checked
- 1: `range-variable-casing.md` is Accepted with *Visible output changes*; the 1978 option A answer is under *Decisions*. Title and message widening is recorded.
- 2: RH4105-005/-006 and SHARED-CASING-015 match the diff; each has `Why:` (RH4105 6/6, casing 15/15) and examples.
- 3: RH4105-005/-006 and SHARED-CASING-015 inserted at sorted positions.
- 4: the RH4105 spec header owners exist.
- 5: `documentation/rules/RH4105.md` title, description, and fix text cover range variables, match the spec and the resources, mention no formatter and no corner cases; the package README row matches the new title.

## Whole run

### Findings
_No further findings._ Finding 1 above is the only one.

### Checked
- 6: the spec author's and the skeptic's output questions (1B, 2A, 3B, 4A) all have answers under *Maintainer answers* in `00-lead.md`; none is written into the specs; the RH2001-011 candidate is under *Follow-up candidates*; none is recorded only in the PR.
- 7: `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:   198, Skipped:     0, Total:   198`. Every new requirement of an enforced spec (RH4105-005/-006, RH4115-004/-005, RH4117-003, RH4128-001 to -003, SHARED-CASING-015) has a covering `[Requirement]` test in the diff.
- 8: all 67 test methods the diff adds carry `[Requirement]` tags (28 of them SHARED-CASING-015); every requirement under the spec author's *Examples for the tests* (RH4115-002, -004, -005, RH4117-001 to -003, RH4128-001 to -003, RH4105-002, -005, -006, SHARED-CASING-015) is tagged on at least one added test; the `04-test-implementer.md` table names the requirements the tests carry (spot checks of the RH4115, RH4117, and RH4105 rows match the tags).
- 9: `sdd/work/` holds only `local-casing-rules`, which the lead deletes before publishing.
