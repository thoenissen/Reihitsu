Role: spec checker
Commit: 4a721ff
Verdict: consistent

The prompt carried no intended fix or hoped-for verdict; I checked the diff `origin/main...HEAD` against the specs, the change doc, and the handovers.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| none | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/context-target-first-line.md` is needed (FMT-PIPE-017 and FMT-PIPE-024 change behavior, FMT-PIPE-018 is restated), is `Accepted`, has no task list, has `Visible output changes` 1-18, and `Decisions` records the maintainer's answer (A on all 18, `00-lead.md`).
2. Specs match the diff: ok. FMT-PIPE-017/018/024, SHARED-FIX-007, and RH5111-004 state the code's behavior: the target-position fact is two plain values (annotation on the first token, document column) read by `LayoutComputer.UseTargetPosition` alone, and the write-back of the leading trivia stays. The RH5111-004 examples agree with change docs 1 and 4. RH3208-003 and the fluent-chain fixes need no change (change doc: output unchanged).
3. IDs: ok. No requirement added, retired, or renumbered; `requirement-ids.txt` is untouched.
4. Code owners: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92` (includes `SpecConsistencyTests`). No type was added to or removed from a spec header.
5. User documentation: ok. No `documentation/rules/` page is in the diff and none is affected; the RH5111 and RH3208 fixes keep their documented Correction. `AGENTS.md`'s changed sentence (no recorded exception; the write-back keeps the first token in its document column and the indentation phase lays out the other lines from the fact) matches FMT-PIPE-017/018/024 and the code.
6. Open questions: ok. The 18 output questions of `03-spec-author.md` are answered under `Maintainer answers` in `00-lead.md`; `04-spec-author.md` and `07-spec-author.md` raise none; every handover's `Follow-up candidates` is `None`. The implementer's unconfirmed probe (decision 9, RH3208 local function with a `where` clause behind a label) is routed to the auditor, not written into a spec.
7. Coverage: ok. FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-024, SHARED-FIX-007, RH5111-004, and RH3208-003 each have `[Requirement]` tests in the diff; the architecture project passes (above).
8. Test tags: ok. Every RH5111, RH3208, and formatter test the diff adds carries the tags its `Tests` row in `05-test-implementer.md` lists (spot-checked all rows against `git diff`, e.g. change 4 and 11 and 12 carry FMT-PIPE-024, the tab and comment-trivia cases carry FMT-PIPE-018). The RH5203/RH5301 "after" tests and the three comment-only edits are untagged, as the table says. Every requirement under the spec author's examples (FMT-PIPE-017, 018, 024, changes 1-18) is tagged on a test the diff adds.
9. Nothing left behind: ok. `sdd/work/` holds only `context-target-first-line`, which the lead deletes before publishing.

Note, not a finding: `08-cleaner.md` records "Commit: see git log"; the lead can ignore it since the directory is deleted before publishing.
