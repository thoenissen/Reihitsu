Role: spec checker
Commit: 473b7fb
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, the handovers 07, 09, 10, 11 (and 06), the change doc, the specs, `AGENTS.md`, and the diff `origin/main...HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. It exists (`sdd/changes/2026-11/region-column-nested-rows.md`), is Accepted, has `Visible output changes`, `Special cases`, and `Decisions`, has no task list; the behavior change (FMT-UNSUP-023, FMT-BREAK-046 added, others changed) requires it; the maintainer's batch answers are in `00-lead.md` and cited in its decisions.
2. Specs match the diff: ok. FMT-UNSUP-023 and FMT-BREAK-046 state the production changes (precheck refusal, `ArrayInitializerBraceLineBreakRewriter`, jagged-array clarification per decision 7); `AGENTS.md` phase table names both.
3. IDs: ok. FMT-BREAK-046 and FMT-UNSUP-023 are the next free numbers, inserted in sorted position in `requirement-ids.txt`; none reused, renumbered, or deleted.
4. Code owners: ok. `scripts/test.sh --project architecture` (includes `SpecConsistencyTests` and `PhaseOwnershipDocumentationTests`) passed. The new rewriter is named in the AGENTS phase table; the spec headers list only the phase and selected rewriters, as for other rewriters of the phase, so no owner list is stale.
5. User documentation: ok. No rule page or `documentation/` file is affected; the diff touches none.
6. Open questions: ok. 07, 09, 11 list none; the earlier ones have answers in `00-lead.md`; follow-ups F3 to F5 sit under `Follow-up candidates`.
7. Coverage: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 221, Skipped: 0, Total: 221`. FMT-BREAK-046 and FMT-UNSUP-023 each have covering `[Requirement]` tests.
8. Tags: ok. `ArrayInitializerBraceLineTests` (32 tests) carries FMT-BREAK-046 on 31 and FMT-INDENT-023 as additional tags; `UnsupportedRegionInsideMemberTests` carries FMT-UNSUP-023 and the cross-referenced FMT-UNSUP-002/-003/-005; the RH5206, RH5305, and CLI tests carry FMT-UNSUP-023 (and SHARED-FIX-010). The four jagged/row tests named in 11 sit under FMT-BREAK-046.
9. Nothing left behind: ok. `sdd/work/` holds only `region-column-nested-rows`, which the lead deletes before publishing.
