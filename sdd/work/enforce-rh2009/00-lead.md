# Lead — enforce-rh2009

| | |
|---|---|
| Issue | thoenissen/Reihitsu#1915 (`agent-filed`: goals are proposals) |
| Slug | enforce-rh2009 |
| Branch | claude/gifted-hawking-3q8b4p (session-designated) |
| PR | #1917 (draft) |
| Base at claim | ec07467 |
| Lane | behavior-change |

## Lane and reason

The issue labels itself `Lane: tooling`, but the work refactors production methods anywhere in `src/` (production paths) next to `Release.ruleset`/`Debug.ruleset`, the self-hosting test utility, and possibly the root `reihitsu.json` (tooling). The lane check admits production next to tooling only with a change doc, which makes it `behavior-change`; a change of the shipped default limit would also be a visible output change that a change doc lists. All roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator.

## Maintainer clarifications (summarized)

- Triage comment on #1915: the issue belongs in 1.5.0 like #1865 did for RH2008/RH2111; the default limit of RH2009 is tried on this repository's own code before 1.5.0 ships it. If the default looks poorly chosen, the run brings the evidence to the maintainer as an output question with a proposed new default, instead of working around it. The run starts alone among the feature runs because refactors may touch source files anywhere.

## Context for roles

- Precedent: `sdd/changes/2026-11/enable-line-count-rules.md` and `squad/notes/2026-11-19-enable-line-count-rules.md` (enabling RH2008/RH2111 for the repository). Its note warns that `binaries/` predating a rule makes a ruleset change inert; `binaries/Reihitsu.Analyzer.dll` currently contains no RH2009.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane: issue says tooling | behavior-change | Production refactors next to ruleset/tooling paths need a change doc | Lane choice is internal | f1a9c2b |
| 2 | 01 | Root `reihitsu.json` complexity limit | None set | Spec author's recommendation; repository uses the default | Repository config, not shipped | 2645615 |
| 3 | 01 | `Debug.ruleset` RH2009 entry | No entry (like RH2008/RH2111), pinned by a test | Consistent with precedent; deliberate absence meets the goal | Repository build config only | 2645615 |
| 5 | 02 | Ask output question 1 (default 15/20/10)? | Not asked; default stays 15 | Maintainer's triage comment asks only if the default is poorly chosen; scan: 0.73 % of production members over 15, p99 = 14, matches RH2008 roll-out share; skeptic re-ran and confirmed 27 members | RH2009-001 unchanged, no output changes | (change-doc amendment) |
| 6 | 02 | Who restructures the 8 test helpers | Test implementer (results unchanged); implementer takes the 19 production members | Test code is the test implementer's | Internal | |
| 7 | 02 | Test names | Fold RH2009 into `ReleaseRulesetReportsRuleAsError`, delete `ReleaseRulesetSwitchesOffComplexityRule`; add a separate `DebugRulesetHasNoEntryForComplexityRule` instead of renaming the line-count test, so `scripts/README.md` stays valid | Avoids doc churn | Internal | |
| 8 | 03 | Change doc names test methods? | No; states only what is checked | Change doc carries no task list | Internal | efea239 |
| 9 | 04 | Test implementer removed `_ignoredDiagnosticIds` itself | Accepted | Test-project code; it is what makes the scan check RH2009 | Internal | 3ebf2d4 |
| 4 | 01 | Remove empty `_ignoredDiagnosticIds` and its test | Remove | Dead code once RH2009 leaves it | Test utility, internal | 2645615 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: 01 spec author (change doc Draft; output question 1 on default limit, recommended A=15).
- Done: 02 skeptic (proceed; no output question, no architecture finding, no follow-up).
- Done: 03 spec author (change doc Accepted, efea239).
- Done: 04 test implementer (3ebf2d4, 0b0f705): 8 test helpers restructured; failing: Release RH2009 row, RH2009 scan row (19 production members).
- Done: 05 implementer (37813ca, 469a7d1, c861527; handover 3cb070c): all tests pass; full self-hosting only fails `WorkingDocumentsDirectoryContainsNoFiles` (expected until sdd/work is deleted).
- Next: 06 cleaner, then 07 spec checker.
