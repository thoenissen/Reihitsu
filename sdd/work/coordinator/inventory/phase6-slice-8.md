# Phase 6 slice 8 (issues #898–#1777)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 1710 | vNext | analyzer/RH8306 documentation | partial-member documentation | open | — | S | yes | — | yes | Question already written in the issue (A: report both parts); changes RH8306-001 and SHARED-DIAG-012. |
| 1711 | vNext | analyzer/RH8xxx documentation | partial-member documentation | open | — | M | yes | — | no | Unconfirmed (read from code); RH8032/8033/8101/8105/8108 still call `GetExpandedDocumentation`, only RH8203 uses `GetExpandedDocumentationOfPart`; reproduce first. |
| 1712 | vNext | analyzer/RH8307 documentation | new rule | open | — | M | yes | — | yes | RH8307-002 excludes nested elements by decision; scope (block vs inline elements) and new-rule-vs-extension are maintainer questions. |
| 1739 | vNext | analyzer/RH5xxx layout | new rule | open | — | M | yes | — | no | Maintainer triage 2026-11-15: separate rule, follow-up of #956; no such rule exists yet (RH5107 page: "commas of an argument list are not checked"). |
| 1741 | Maintenance | specs only | SHARED-DIAG-008 reversal | superseded by #1759 | #1759 reverses the reporting half; RH5101-004 ("diagnostic is still reported") then becomes correct, and the ~60-spec rewording is #1759's spec walk | M | no | 1759 | no | Fold any remaining RH5101-004 wording into the #1759 run. |
| 1758 | Maintenance | code fixes/RH5107 | formatter-delegating fixes | open | — | S | no | — | no | Fix still computes `anchorColumn` by hand (RH5107 provider:88–90); no known differing input. |
| 1759 | vNext | analyzer (shared)/SHARED-DIAG-008 | SHARED-DIAG-008 reversal | open | — | L | yes | — | no | Decided by maintainer (#1756); spec still states the old rule (diagnostics.md:401). |
| 1771 | Maintenance | code fixes (shared)/rename | single owner of a decision | open | — | M | yes | — | no | New shared type → escalation 9 before building; #1770 (merged after filing) added the RH2001 binding check the issue compares. |
| 1773 | vNext | code fixes (shared)/SHARED-FIX-007 | shared-line members | open | — | M | yes | — | yes | Question written in the issue; #1760 made whole-member fixes separate siblings, gap-only fixes undecided. |
| 1774 | vNext | code fixes/RH5101–RH5417 layout | partial write-back | open | — | L | yes | — | yes | Needs the shared write-back design (with #1140 remainder); design question A/B/C in the issue. |
| 1775 | Maintenance | code fixes (shared)/member line break | shared-line members | open | — | S | yes | — | yes | Two owners of the break (`FormatMemberOnLinesOfItsOwnAsync` vs `TargetAttributePlacementCodeFixProviderBase`); tab vs spaces output question. |
| 1777 | vNext | code fixes/RH5113 | fix withholding scope | open | — | S | yes | — | yes | Both guards still present (RH5113 provider:136, :162); answer B closes it. |
| 898 | Maintenance | squad/tooling (scripts) | squad/tooling | open | — | S | no | — | no | Still `100644` (`git ls-files -s scripts/trace.sh`); the only script without the bit. |
| 976 | vNext | Playground | Playground | open | — | S | yes | — | no | Decided in #934; playground.md:18 still lists it as tracked here. |
| 1054 | Maintenance | squad/tooling (LaneClassifier) | squad/tooling | done | `squad/decisions/` was removed on main in #1481 (escalation answers now live in the lead handover); no path left to classify | — | no | — | no | Premise obsolete; lane-rule cleanup remains in #1471. |
| 1427 | Maintenance | squad/tooling (roles) | squad/tooling | open | — | S | no | — | no | auditor, cleaner, test-implementer, validator templates still lack the section. |
| 1428 | Maintenance | tests/architecture (RequirementCoverageTests) | test infrastructure | open | — | S | no | — | no | `Uses` protection still limited to `shared/` (sdd/README.md:83). |
| 1470 | Maintenance | docs/scripts | single statement of a rule | open | — | S | no | — | no | Default-run coverage still restated in AGENTS.md, CONTRIBUTING.md, squad files, `.claude/agents/validator.md`. |
| 1471 | Maintenance | squad/tooling (LaneClassifier) | single statement of a rule | open | — | S | no | — | no | `CODE_OF_CONDUCT.md`/`SECURITY.md` still in neither root list (LaneClassifier.cs:25,30). |
| 1475 | Maintenance | Reihitsu.Core | internal refactor (dead code) | open | — | S | no | — | no | Only the Core test calls `ShiftContinuationExteriorMarkers`. |
| 1479 | Maintenance | squad/tooling (failure-classes) | squad/tooling | open | — | S | no | — | no | Section still names FMT-CLEAN-002 and `RawStringAlignmentPhase` (failure-classes.md:95). |
| 1507 | Maintenance | tests/architecture (phase-ownership) | phase-ownership table | open | — | S | no | — | no | `PhaseOwnershipDocumentationTests` still checks only phase order. |
| 1544 | Maintenance | docs/AGENTS.md (phase-ownership) | phase-ownership table | partly done | `LineBreakPhase` row now says "it keeps every blank line" | S | no | — | no | Remains: `BlankLinePhase` row does not name `BraceBlankLinePolicy`. |
| 1545 | Maintenance | squad/tooling (verify-text-only) | squad/tooling | open | — | S | no | — | no | `IsPubliclyVisible` (verify-text-only.cs:628) still ignores containing-type accessibility. |
| 1661 | Maintenance | specs only (SHARED-COMMENT-001) | comment spec clarification | open | — | S | no | — | no | Text test (comment-placement.md:27) still admits `/***`. |
| 1690 | Maintenance | specs only (SHARED-FIX-003, FMT-INDENT-007) | comment spec clarification | open | — | S | no | — | no | code-fixes.md:212 still lists "the indentation of its lines" as layout without exception. |
| 1693 | Maintenance | specs only (sdd/README.md) | spec clarification | open | — | S | no | — | no | sdd/README.md:62 still says "without being listed under `Uses`". |
| 1724 | Maintenance | CLI/trace (PipelineTraceCommand) | squad/tooling | open | — | S | no | — | no | Trace reports only syntax-error and generated skips; no hiding-carrier message. |
| 1725 | Maintenance | squad/tooling (README, AGENTS.md) | squad/tooling | open | — | S | no | — | no | Escalation 9 (squad/README.md:109,157) still reads as size/new-type only; maintainer confirms wording before merge. |
| 1740 | Maintenance | Reihitsu.Core | internal refactor (dead code) | open | — | S | no | — | no | `OrderUsings` called only from Core tests. |
| 1772 | Maintenance | specs only (sdd/changes) | spec clarification | open | — | S | no | — | no | Pointer to `sdd/work/comma-comment-layout/04-skeptic.md` still at comma-behind-comment-unsupported.md:102; the handover is deleted, so the facts come from that run's git history. |

## Closure candidates

- #1054 — done/obsolete: `squad/decisions/` was removed in #1481, so no run writes a decisions entry that widens the lane; residual lane wording is #1471.
- #1741 — superseded by #1759: reversing SHARED-DIAG-008's reporting half makes RH5101-004 read correctly and absorbs the analyzer-spec walk.
- #1544 — partly done: only the `BraceBlankLinePolicy` mention in the `BlankLinePhase` row remains (could ride with #1507).

## Overlaps

- #1759 + #1741 (one run; #1741 closes into it).
- #1710 + #1711 — partial-member documentation judged per part (RH8306 output question + RH8xxx reproduction).
- #1507 + #1544 — phase-ownership table: guard-name resolution test and the stale rows; land #1544's remaining row with or before #1507's test.
- #1773 + #1775 — SHARED-FIX-007 *Shared line*: which fixes it covers and who writes the break; one output-question round.
- #1774 + #1140 remainder (other slice) — shared partial write-back design.
- #1739 + #1758 (+ #956 in slice 1) — comma-position family: the new argument-list comma rule should reuse RH5107's fix shape, so make RH5107's fix formatter-delegating first or together.
- #1661 + #1690 — comment spec clarifications (SHARED-COMMENT-001, SHARED-FIX-003/FMT-INDENT-007), one docs-lane run.
- #1470 + #1471 + #1693 + #1725 — "state a rule once" docs/tooling cleanups.
- #1475 + #1740 — dead public members in Reihitsu.Core, one cleanup.
- #898 + #1724 — `scripts/trace.sh` fixes.
- #1777 relates to FMT-UNSUP-004 (declared unsupported shape) and SHARED-FIX-010 withholding scope.
