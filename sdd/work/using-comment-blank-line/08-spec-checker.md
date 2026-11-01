Role: spec checker
Commit: 5b0404c
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

_None._

## Checked
1. Change doc: ok. `sdd/changes/2026-11/using-comment-blank-line.md` exists, is `Accepted`, has no task list, has `Visible output changes` (entries 1-6), and its `Decisions` records maintainer answers 1-7, covering every entry.
2. Specs match the diff: ok. SHARED-USING-008 (exception), SHARED-USING-011 (new), FMT-USING-002 (changed), FMT-USING-010 (new), FMT-BLANK-011 (open case decided plus example), RH7207-001/-002/-004 and RH7201-004 to RH7206-004 describe the diff (RH7207 report, using phase and Core `HasBlankLineAbove`). Format (normative sentence, `Why:`, examples) holds. The FMT-USING Non-goals line stays true.
3. IDs: ok. FMT-USING-010 and SHARED-USING-011 are inserted at their sorted positions in `requirement-ids.txt`; nothing is reused or removed.
4. Code owners: ok. No owner was added, renamed, or removed (`UsingDirectiveOrderingUtilities` is already an owner of SHARED-USING); `sdd/specs/README.md` is unchanged and still matches. `Uses` lines are updated.
5. User documentation: ok. No rule page needs a change; `documentation/rules/RH7207.md` does not mention the blank line above comments, and the change adds no page note.
6. Open questions: ok. The skeptic's and spec author's output questions are answered under `Maintainer answers` in `00-lead.md`; follow-up candidates: none.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92`. FMT-USING-010 and SHARED-USING-011 have covering `[Requirement]` tests.
8. Test tags: ok. A scan of the added test methods found none without a `[Requirement]` tag; the test implementer's table names FMT-USING-010, SHARED-USING-011, FMT-BLANK-011, RH7207-001/-002/-004, RH7201-004 and RH7203-004 (and sibling fixes) matching the tags, and every requirement under `Examples for the tests` is tagged.
9. Nothing left behind: ok. `sdd/work/` holds only `using-comment-blank-line/` (the lead deletes it before publishing).
