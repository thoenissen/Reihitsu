Role: spec checker
Commit: 140b8f3
Verdict: findings

Run diff `c2866ea..HEAD`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

`scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117` (includes `SpecConsistencyTests`, the `Uses` consistency test, and `EveryRequirementOfAnEnforcedSpecIsCovered`).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 (change doc) | whole run: `sdd/specs/shared/statement-braces.md` (SHARED-BRACE-003), `sdd/specs/analyzer/RH7309.md` (RH7309-007), `sdd/specs/shared/attribute-targets.md` (SHARED-ATTR-022); no `sdd/changes/` file in the diff | Three requirements are added that withhold a fix which `main` offers, and maintainer answers 1 (C: RH5406 offers no fix for a multi-line body on the `if`/`else` line), 2 (`b++;` moves to its own line), 3, 4, 5, 6, and 7 (B: no fix for RH5518/RH5528) change code-fix results an end user sees. No change doc exists, so none of these answers sits in a `Decisions` section; they are only in `00-lead.md`. The lead decided "bug-fix lane, no change doc" from the maintainer's triage comments (decisions 2, 7, 8, 9, 14). That holds for the line-ending, whitespace and column parts (they bring code to SHARED-FIX-006/007/008). Whether the withholds of answers 1 and 7 count as "a requirement added with new behavior" (`sdd/README.md`, *When a change doc is required*) or as local rare-class withholds (`AGENTS.md`, Code-fix coverage) is the lead's call and should be stated in the PR; I cannot verify it from the files. | lead (spec author if a change doc is wanted) |

No other finding.

## Checked
1. Change doc exactly when required: finding 1 (decision for the lead). No change doc is in the diff; the three added requirements are SHARED-BRACE-003, RH7309-007, SHARED-ATTR-022; the rest of the spec diff is clarifications (SHARED-BRACE-002, RH5011-004, RH8304-004, SHARED-ATTR-017, the seven split-rule `-004`/`-006` requirements, SHARED-FIX-007/008 wording).
2. Specs match the diff: ok. SHARED-BRACE-002 and -003 follow the maintainer answers 1 to 6 (header's-last-line reading of decision 13, body keeps its column, `b++;` on its own line, trailing whitespace stays on the body line, directive/disabled text withheld). Each new requirement has `Why:`, examples, and `{|RH####:…|}` markup. RH5011-004 matches decision 11 (`break;\n\r`). SHARED-ATTR-022 matches decision 14. The `Uses` of `code-fixes.md` and RH7309 list the added references; the architecture test confirms `Uses` equals the referenced IDs.
3. IDs: ok. `requirement-ids.txt` gains exactly RH7309-007, SHARED-ATTR-022, SHARED-BRACE-003, each as the next free number at its sorted position; nothing removed or renumbered.
4. Code owners exist: ok. No owner changed; no spec was added, so `sdd/specs/README.md` is unchanged and still lists every spec. `SpecConsistencyTests` passes.
5. User documentation: ok. No rule page is in the diff. The `### Correction` of RH5405, RH5406, RH5407, RH5011, RH8304, RH7309 equals the unchanged fix output for their violation examples (body below the `if` keeps its lines; RH5406 correction keeps `Other(1,⏎ 2);`). No page carries a note about a withheld fix; the withholds are covered by `general-notes.md`. The help-link pages exist and the ID ranges fit their categories.
6. Open questions routed: ok. Every output question of 01, 02, 03, 04 has an answer under `Maintainer answers` in `00-lead.md` (answers 1 to 7); 07 raised none; no question is written into a spec; the two follow-up candidates (formatter write-back, RH8402 line break) are under `Follow-up candidates` and are outside the run's issues.
7. Requirements covered: ok. Architecture run quoted above; SHARED-BRACE-003 (36 tag lines in the diff), RH7309-007 (5), SHARED-ATTR-022 (10) are tagged by tests the diff adds.
8. Tests carry their requirement's tag: ok. All 107 test methods the diff adds that I could attribute carry a `[Requirement]` (the one my scan flagged, RH5406 `NoFixIsOfferedWhenAMultiLineChildStartsOnTheLineOfItsIf`, carries SHARED-BRACE-003, RH5406-001, RH5406-002 as 05's table lists). Every requirement named under `Examples for the tests` (SHARED-BRACE-002/-003, RH5011-004, RH8304-004, RH7309-007, SHARED-ATTR-017/-022) is tagged on a test the diff adds or newly tags. RH7004-004 is not newly tagged; the RH7004 characterization test is tagged RH7004-007, as 05 lists.
9. Nothing left behind: ok for now. `sdd/work/` holds only this run's directory; the lead deletes it before publishing.
