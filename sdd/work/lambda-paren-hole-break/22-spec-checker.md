Role: spec checker
Commit: 90e07d0
Verdict: findings

Repair round 2. I am a fresh agent. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1, 2 | `sdd/changes/2026-11/lambda-paren-hole-break.md`, `### Unchanged` bullet 2 ("A block lambda in a hole is broken and aligned as before") | Decision 12 changes block-lambda output: a block lambda whose anchor stands on a hole line that does not move (end line of a `/* */` comment, verbatim continuation line) with a column below the literal's shift used to land in the shift's column (13) on pass 1 and was not idempotent. It now lands under its `(`. The change doc does not list this under `Visible output changes` and still says block lambdas are unchanged. The requirement is unchanged (FMT-INDENT-028/030/031 already demand the new output), so no new requirement or maintainer question is needed, but the sentence is no longer accurate. Add a short entry (or qualify the bullet) naming the block-lambda case as covered by FMT-INDENT-028 and lead decision 12, with the pass-2 fixed point as the example. | spec author |

## Checked
1. Change doc: finding 1. It stays required (the run changes behavior otherwise), stays `Accepted`, has no task list. Decision 13 (floor moved after the raw-string move) and decision 12 add no new visible output beyond finding 1.
2. Specs match the diff: finding 1 only. FMT-INDENT-030 and FMT-INDENT-031 ("measured after the literal moved") and FMT-RAW-007 bullet 1 ("at the token's column after the literal moved") already state the post-move anchor column, which is what the moved floor implements. No spec states a pre-move floor or a negative-column rule, so none contradicts the diff. The 0 floor of the final column is not stated by any requirement (neither before nor after this round); it is not behavior this round introduces, so it is a note, not a finding. The spec diff against the previous round is empty (`git diff 21ecce6 90e07d0 --stat -- sdd/specs sdd/changes` shows no file).
3. IDs: ok. `requirement-ids.txt` untouched this round; no ID added.
4. Code owners: ok. `LayoutModel` and `IndentationRewriter` are in the indentation header; `RawStringLineShifts` is in the raw-string header. The members changed (`FloorColumn`, `GetColumnAfterRawStringShift`, `MoveLines` signature) are not owners. No type added, renamed, or removed. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. None new; follow-ups remain under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. Architecture result quoted in 4.
8. Requirement tags: ok. All 16 tests in `HoleAnchorOnUnmovedLineAlignmentTests` carry `[Requirement("FMT-RAW-007")]` and `FMT-INDENT-030`; 10 carry `FMT-INDENT-031` and 6 carry `FMT-INDENT-028` (matching the anonymous-method and block-lambda halves); one carries `FMT-SPACE-013`, as 19-test-implementer.md's table lists.
9. Nothing left behind: ok. Only this run's `sdd/work/lambda-paren-hole-break/` exists; the lead deletes it before publishing.
