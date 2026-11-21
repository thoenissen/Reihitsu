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
| 4 | 01 | Remove empty `_ignoredDiagnosticIds` and its test | Remove | Dead code once RH2009 leaves it | Test utility, internal | 2645615 |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: 01 spec author (change doc Draft; output question 1 on default limit, recommended A=15).
- Next: 02 skeptic, then output-question batch before test implementer.
