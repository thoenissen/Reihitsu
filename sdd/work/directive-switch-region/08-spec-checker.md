Role: spec checker
Commit: de6ca65
Verdict: consistent

Inputs read: `00-lead.md`, handovers 01-07, the diff `origin/main...HEAD`. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked

### thoenissen/Reihitsu#1385
1. ok — `sdd/changes/2026-11/directive-before-switch-section.md` is required (FMT-BLANK-006 gains switch sections), has `Visible output changes` (two inputs with before/after), no task list, `Accepted`, and `Decisions` records answer 1A.
2. ok — FMT-BLANK-004, -006 (renamed heading, definition of "directly preceded"), -016 (coverage list, clarification) match the diff in `BlankLineEditor`; each has `Why:` and examples; the section examples match the change doc.
3. ok — no new ID; `requirement-ids.txt` untouched (heading rename keeps ID FMT-BLANK-006).
4. ok — `BlankLineEditor` is a code owner of `blank-lines.md`; `RH5011-002` added to `Uses` and exists in the ledger (line 548); `SpecConsistencyTests` pass as part of the architecture run.
5. ok — no `documentation/rules/` page changed or affected.
6. ok — OQ1 has its answer under `Maintainer answers`; no question is written into a spec.
7. ok — `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`. Covering `[Requirement]` tests exist for FMT-BLANK-004/-006/-016 (`RequiredBlankLineBehindDirectiveTests`, node tests, the RH5011 formatter parity test).
8. ok — every added test in 04's table carries the tags the table lists; the examples the spec author named are tagged on at least one added test. `RegionFixKeepsWhitespaceBetweenNumberSignAndEndRegionKeyword` is an untagged characterization test (verifies no requirement), not a finding.

### thoenissen/Reihitsu#1398
1. ok — clarification (FMT-SWITCH-006 precedence in FMT-SWITCH-004), no change doc required.
2. ok — FMT-SWITCH-004 text and two examples match the `SwitchCaseBraceRewriter` change.
3. ok — no new ID.
4. ok — `SwitchCaseBraceRewriter` is a code owner of `switch-case-braces.md`.
5. ok — no rule page affected.
6. ok — the two follow-up questions (`return`/`throw`-only section, parenthesized layout) are under `Follow-up candidates`, not in the spec.
7. ok — architecture result above; `SwitchCaseBraceRequirementTests` and `SwitchCaseBraceRewriterTests` cover FMT-SWITCH-004/-006.
8. ok — tags match the table; both spec examples are tagged.

### thoenissen/Reihitsu#1239
1. ok — recorded as a clarification of FMT-REGION-007 (Decision 4), so no change doc; the visible output change is the maintainer's answer 2A, recorded in `00-lead.md`.
2. ok — FMT-REGION-002, -004, -007 and the intro match the `RegionNamingRewriter` change; examples (described pair, tab, disabled branch, undescribed pair) present with `Why:`.
3. ok — no new ID.
4. ok — `RegionNamingRewriter` and `RegionDirectiveUtilities` are code owners; `RegionDirectiveUtilities` itself is not changed (Decision 2).
5. ok — no rule page changed; the RH7301 fix output is unchanged and pinned by a characterization test.
6. ok — OQ2 answered; the other-directives question is under `Follow-up candidates`.
7. ok — architecture result above; `RegionLineFormTests` covers FMT-REGION-002/-004/-007.
8. ok — tags match; the three FMT-REGION-007 examples and the FMT-REGION-004 example are tagged.

### Run-wide
9. ok — `git ls-tree` shows no `sdd/work/` outside `directive-switch-region/`; the lead deletes this run's directory before publishing.
