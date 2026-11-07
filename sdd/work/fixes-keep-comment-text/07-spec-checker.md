Role: spec checker
Commit: 7bb4156
Verdict: findings

The prompt carried no intended fix or hoped-for verdict. Checked against `git diff origin/main...HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/formatter/pipeline.md` FMT-PIPE-010; `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (`FormatNodeInDocumentWithContextAsync`, line ~331) | The clarified sentence says "Both" entry points MUST write each comment in the target's trailing trivia once. Only `FormatNodeInDocumentAsync` does (`WriteBackTrailingTrivia`); `FormatNodeInDocumentWithContextAsync` still writes `originalLastToken.TrailingTrivia` unchanged (05-implementer: no caller reaches the duplication there). The requirement therefore contradicts the code for the WithContext entry point. Suggested fix: scope the new "write each comment once" clause to `FormatNodeInDocumentAsync` (AGENTS.md, Supported inputs: no caller reaches it for the other), or have the implementer cover it. No tests exist for the WithContext case either, consistent with 04-test-implementer. | spec author |

## thoenissen/Reihitsu#1075
- RH5011-007, RH5025-006, RH5030-003 are new requirements for existing behavior decided by unchanged requirements and the maintainer's #1075 choices (clarification, no change doc; decision 3). They follow the format (one heading, normative sentence, `Why:`, examples, `{|RH####:...|}` markup). RH5027-001 gains examples only; its `Uses` gains RH5024-001 and SHARED-DIAG-003.
- Output questions: Q1 (RH5023 hole) answered B in `00-lead.md`, RH5023-002 unchanged, listed under Follow-up candidates. None written into a spec.
- IDs: RH5011-007, RH5025-006, RH5030-003 are the next free numbers, inserted in sorted position in `requirement-ids.txt`; nothing reused or removed.
- Rule pages: unchanged. RH5011/RH5025/RH5027/RH5030 pages describe the normal case; the new behavior is corner cases/withheld fix, which pages must not carry. Ok.
- Tags: every new RH5011/RH5025/RH5027/RH5030 test carries the requirement the `04-test-implementer.md` table lists; all of 03-spec-author's "Examples for the tests" requirements (RH5011-007, RH5027-001, RH5025-006, RH5030-003) are tagged on at least one new test.

## thoenissen/Reihitsu#1261
- SHARED-BRACE-002 reverted to main's text plus one comment-once sentence and one example; FMT-PIPE-010 clarified with a second example (see finding 1 for the "Both" scope). `Uses` gains SHARED-FIX-003 in both spec headers and in the `pipeline.md` Uses row; no new IDs needed. Clarification, no change doc (triage on #1261, decision 3).
- Decision 8/9/10 respected: no `else if` test, no SHARED-BRACE-002 alignment, owner stated as output not mechanism.
- Tags: all RH5405/RH5406/RH5407 tests carry SHARED-BRACE-002; formatter entry-point tests carry FMT-PIPE-010; the RH5412 characterization test carries SHARED-FIX-003 (listed in the table). Spec examples are tagged on at least one test (SHARED-BRACE-002, FMT-PIPE-010).
- Region-text instance not reproduced (decision 7); follow-ups are in `00-lead.md`.

## Checked
1. Change doc exactly when required: ok (none added, none required; no `sdd/changes` in the diff).
2. Specs match the diff: finding 1 (otherwise ok).
3. IDs: ok.
4. Code owners: ok; no owner added/renamed/removed, `sdd/specs/README.md` untouched and unaffected.
5. User documentation: ok (no rule page affected; helpLink pages unchanged).
6. Open questions routed: ok.
7. Coverage/consistency: `scripts/test.sh --project architecture` -> `Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. Every new requirement (RH5011-007, RH5025-006, RH5030-003) has a covering `[Requirement]` test.
8. Test tags: ok.
9. Nothing left behind: ok; only `sdd/work/fixes-keep-comment-text/` exists, for the lead to delete before publishing.
