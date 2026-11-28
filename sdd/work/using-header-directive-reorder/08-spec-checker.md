Role: spec checker
Commit: 2dae196
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

## Checked
1. Change doc: ok. `sdd/changes/2026-11/using-header-directive-reorder.md` exists, Status Accepted, has `Visible output changes` (cases 1-8), no task list. `Decisions` records the maintainer answers for every case (2-8 in the chat, case 1 in the issue).
2. Specs match the diff: ok. SHARED-USING-009 (changed), FMT-USING-012 (added), FMT-USING-004/009 and RH7201-RH7207 -001/-004 follow the narrowed predicate in `UsingDirectiveOrderingSafety` and the header split in `UsingLeadingTriviaBuilder`. Requirements have `Why:` and examples.
3. IDs: ok. FMT-USING-012 is the next free number and sits after FMT-USING-011 in `requirement-ids.txt`; nothing reused or removed.
4. Code owners: ok. No owner added, renamed, or removed; the spec owners are unchanged. Covered by `SpecConsistencyTests` inside the architecture run.
5. User documentation: ok. No rule page needs a change; `general-notes.md` states no case this change contradicts, and the RH7203/RH7207 pages mention no directive behavior.
6. Open questions: ok. All output questions are answered under `Maintainer answers` in `00-lead.md`; none is open or written into a spec; no follow-up candidates.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 221, Skipped: 0, Total: 221. FMT-USING-012 has 18 tagged tests.
8. Test tags: ok. The test implementer's table requirements (SHARED-USING-009, FMT-USING-002/003/004/011/012, FMT-PIPE-030, RH7201-RH7207 -004, RH7203-001, RH7207-001/-002) appear as `[Requirement]` tags in the diff; the spec author's examples for SHARED-USING-009, FMT-USING-004, FMT-USING-012, RH7201-RH7207 -004, and RH7207-001 are each tagged.
9. Nothing left behind: ok. `sdd/work/` holds only `using-header-directive-reorder`, which the lead deletes before publishing.
