Role: implementer
Commit: 859ed25
Verdict: tests pass

Repair round 1. The prompt named the issue, slug, lane, handovers, and the two decisions entries; it carried no intended fix or verdict. The auditor's suggested changes were treated as suggestions. The run has one issue, thoenissen/Reihitsu#1403, so this handover is not split by issue.

No tests were handed over; the diff is Markdown only, so `tests pass` means no test was affected.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/README.md` | *Several issues in one run*: new bullet *The roles are the union of the lanes* — the run runs every role of the claimed lane and of every issue's own lane, in the usual order; each issue receives its own lane's duties (with the rule-page text fix plus `scripts/` example and the reproduce-first example); the spec author, skeptic, test implementer, and implementer have no duty for an issue whose own lane does not run them; the cleaner, spec checker, auditor, and validator check the finished tree whole, as the claimed lane asks. *One agent per role* bullet: a role-file duty keyed to a lane means the issue's own lane. *One section per issue* bullet: an issue the role has no duty for gets one line saying so and naming its lane. | Auditor finding 1; `squad/decisions/2026-10-22-1403-batch-roles-and-change-doc.md` answer 1 |
| `squad/lead.md` | Section 1 step 5: the run's roles are the claimed lane's plus every issue's own lane's (points to the README). Section 3: `00-lead.md` records, for several issues, the roles of the run and which issues each serves. Section 4: "each role of the run" for several issues. *Never*: a role an issue's own lane requires may not be skipped unrecorded. Section 1 step 1: several issues may be separated by spaces or commas (`#123 #124`, `#123, #124`). | Auditor findings 1 and 2 |
| `.claude/commands/squad.md` | Line 3 plural-neutral ("the GitHub issues named in $ARGUMENTS — usually one"). Line 7 defers to `squad/lead.md` section 1 step 1 for the accepted forms and asks only when no issue can be extracted. | Auditor finding 2 |

Single-issue runs: every changed step keeps its single-issue text and adds the several-issue form in a parenthesis or a "for several issues" clause; the union of one issue's lane with itself is that lane, so a single-issue run spawns the same roles as before. The command now accepts `issue 123`, which `lead.md` already accepted; that is the one single-issue difference, and it removes a needless question.

`sdd/changes/2026-10/establish-squad.md` is unchanged, as the maintainer decided (answer 2).

Judgment calls for the auditor to challenge:

- **Tree-checking roles.** The decision says each issue receives its own lane's duties. I read that as applying to the roles that do per-issue work (spec author, skeptic, test implementer, implementer). The cleaner, spec checker, auditor, and validator check the whole tree as the claimed lane asks, so a docs issue inside a tooling run is audited with the rest of the tree. The union can never add a tree-checking role the claimed lane lacks: along both widening orders the narrower lanes run no tree role that the wider one does not (the spec checker runs in every lane; the docs lane's conditional validator and missing cleaner and auditor are narrower than every wider lane).
- **General rule instead of role-file edits.** Rather than editing every role file's *When you run* list, the README states once that a lane-keyed duty means the issue's own lane, next to the existing "the issue" convention. This keeps the role files in their single-issue form.
- **Comma separators.** Finding 2 named comma-separated lists; I made the owner (`lead.md` step 1) state them instead of leaving the forms open.

Not changed: the auditor's hints (decisions-entry escalation number, `batch-<a>` branch visibility, `#<a>` in the `Verdict` example) were not findings; the decisions entry is the lead's file.

## Policy owners
- Roles of a run of several issues: owner `squad/README.md` *Several issues in one run*, bullet *The roles are the union of the lanes*; `squad/lead.md` sections 1, 3, 4, and *Never* point to it. Checked with `rg -n "roles? (of|the) (the )?lane|each role of|own lane" squad .claude CLAUDE.md AGENTS.md`; no other file states which roles a run spawns.
- Accepted prompt forms: owner `squad/lead.md` section 1 step 1; `.claude/commands/squad.md` now defers to it. Checked with `rg -n "issue number|ARGUMENTS|issue 123" squad .claude CLAUDE.md AGENTS.md`.

## Tests run
None — Markdown only. `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Disputed tests or scope
_None._ Three Markdown files changed, all already in the run's diff.

## Follow-up candidates
- Pre-existing, from the auditor: the XML summary of `LaneClassifier.Covers` (`src/Reihitsu.Tooling/LaneClassifier.cs`) says a claimed lane "runs every role the required lane runs", which the lane table contradicts for tooling against docs and tests.
