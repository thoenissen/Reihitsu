Role: spec checker
Commit: e42e7b2
Verdict: consistent

My prompt carried no intended fix, suspected cause, or verdict.

Repair round. Scope of this check: the repair diff `34ca7c3..HEAD` outside `sdd/work` (2 specs, 8 test files), against the auditor's findings and accepted hints in 07, the worklist in 00-lead.md, and the open questions filed on thoenissen/Reihitsu#1408. The first-round checks in 06 stand for everything the repair did not touch.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Observations (no action needed)

- **Auditor finding 1 resolved.** The kept-break MUST, its Why sentence and the example's closing clause are gone from SHARED-FIX-008. No file under `sdd/specs` or `src` still says a kept break "stays a lone `\r`" (grep). The RH5405 test no longer asserts `}` followed by a lone `\r`; it asserts the layout with every break read as `\n` plus the three created breaks as `\n`.
- **Auditor finding 2 resolved.** The contradiction between SHARED-FIX-008 and RH5011-004 / RH8304-004 is recorded in 08 among the contradictions left in place. RH5011-004 and RH8304-004 are unchanged. No test tagged SHARED-FIX-008, RH5011-004 or RH8304-004 pins an all-`\r` shape of RH5011 or RH8304. The lead filed it as Q-1380-1 on thoenissen/Reihitsu#1408, marked "Decided by principle P35 (#912)".
- **Question numbering in the handovers is swapped relative to the filed comment.** On #1408, Q-1380-1 is the RH5011/RH8304 P35 instance and Q-1380-2 is the kept-break question. 09 calls the kept-break question Q-1380-1 and the RH5011/RH8304 shapes Q-1380-2. No spec, test or source file cites either number (grep), so nothing in the repository is wrong; only 09's prose is reversed. The PR text should use the numbers as filed.
- **SHARED-FIX-008 text.** The decided P35 sentences are intact: a fix writes the line ending FMT-PIPE-007 detects, a lone `\r` counts as `\n`, and every break a fix creates in an all-`\r` document is `\n`. The remaining sentences (CRLF and LF "creates or keeps", the content exemption under SHARED-FIX-003, the CRLF example) are the pre-PR requirement. The kept-break behavior in an all-`\r` document is deliberately unspecified and is Q-1380-2 on #1408. `git diff f52df45 HEAD` of `shared/code-fixes.md` touches only SHARED-FIX-008; `Uses` is still `FMT-PIPE-007`, as before.
- **FMT-PIPE-007 example.** The new code example (two lone `\r` and one `\r\n`, all breaks `\n` afterwards, tie variant in prose) is the example thoenissen/Reihitsu#1081 item 2 asks for. Requirement text and Why are unchanged.
- **Hints from 07 taken up.** CRLF variants of the RH5201, RH5301 and RH5303 SHARED-COMMENT-011 tests, a conforming RH5304 test (LF and CRLF), the RH5417 and RH5406 SHARED-FIX-008 shapes, and the FMT-PIPE-007 example and tie tests are in the diff and tagged. The auditor's remaining hints (the further #1382 instance, the RH5203-001 acceptance) were already settled in 00-lead.md before this round.

## Checked
1. Change doc exactly when required: ok. The repair only removes an over-claim and adds an example; no behavior is added, changed or retired. No change doc.
2. Specs match the diff: ok. SHARED-FIX-008 now states what the code does for created breaks (verified by the auditor and the spec author's scratch runs, and by the passing tests below); no requirement contradicts the repair diff other than the recorded RH5011-004 / RH8304-004 contradiction. Format is intact (normative sentence first, Why, examples).
3. IDs: ok. `requirement-ids.txt` is unchanged since 34ca7c3; no ID minted, moved, renumbered or retired.
4. Code owners: ok. No owner changed; `SpecConsistencyTests` passes within the architecture run.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. Both questions from 08 are filed on thoenissen/Reihitsu#1408 (`SDD open questions: sdd/specs/shared/code-fixes.md`) as Q-1380-1 and Q-1380-2; neither lives only in the PR.
7. Requirements covered and specs consistent: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. The amended SHARED-FIX-008 is covered by the tagged RH5405, RH5406 and RH5417 all-`\r` tests (pinning created breaks only) plus the existing CRLF and LF tests. The new FMT-PIPE-007 example is covered by two tagged `FormatSyntaxTree` tests (2:1 gives `\n`, 2:2 gives `\r\n`). Amended SHARED-COMMENT-011 is covered for RH5201, RH5301, RH5303 (LF and CRLF) and RH5304 (LF and CRLF). I re-ran the repair's tests: analyzer, `Name~FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns|Name~BehindMisplacedBlockCommentIsDetected`: `Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13`; formatter, `TestCategory=FMT-PIPE-007`: `Passed!  - Failed:     0, Passed:    19, Skipped:     0, Total:    19`. No tagged test contradicts the amended wording.
8. Nothing left behind: ok. `sdd/work/` holds only `principles-p32-p37`, which the lead deletes before publishing.
Scope of the repair: ok. The repair diff outside `sdd/work` is exactly `formatter/pipeline.md`, `shared/code-fixes.md` (SHARED-FIX-008 only) and eight test files; no production, tooling or project file, no ID file, no AGENTS.md. `scripts/lane-check.sh tooling`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
