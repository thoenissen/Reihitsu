Role: implementer
Commit: 5226975
Verdict: tests pass

No tests were handed over: the lead skipped the test implementer because the change is to Markdown process files that no test covers. `Tests pass` here means no test was affected; nothing compiled changed.

The prompt carried no intended fix or verdict; it named the issue, slug, lane, handovers, and the decisions entry, which I followed.

## Changes

Source of every behavior: thoenissen/Reihitsu#1403 and the maintainer's decisions in `squad/decisions/2026-10-22-1403-multi-issue-run-design.md` (cited below as D1–D4). The decisions override the issue's recommendation 2 (an issue dropping out alone), which is not implemented.

| File | What changed | Requirement |
|---|---|---|
| `squad/README.md` | New section *Several issues in one run*: the prompt sets the batch and the lead never adds an issue; one agent per role for the whole run; "the issue" in a role file means each issue; handover layout (one `## thoenissen/Reihitsu#<N>` section per issue holding the role's sections one level down, a final `## Whole run` section for tree-wide results, per-issue `Verdict` when they differ); one lane, the narrowest that admits all, widening along the orders, escalation 10 before the claim when none does; change docs per shared cause; any escalation stops the whole run; escalation 5 measured against the batch; one `Closes`/`Refs` line per issue. A pointer from *Handovers are files*. Escalation 10 extended to "no lane admits all issues of a run" before the claim; new escalation 11 "more than eight issues". | Issue (one run, one PR, one `sdd/work/`, per-issue sections, rule rewording); D1, D2, D3, D4 |
| `squad/lead.md` | Intro: issues the prompt names, one PR, steps apply to every issue. Start: several issue numbers, read every issue, claim check for every issue, specs every issue touches, new step 5 picks each issue's lane and the common lane before the claim. Claim: branch `claude/issues-<a>-<b>-<c>-<slug>` for two or three issues, `claude/batch-<a>-<slug>` for more; claim commit, draft-PR title, and one `Closes` line per issue. Lane: `00-lead.md` lists the issues and each one's lane. Roles: one agent per role for the whole run. Not reproduced: only that issue becomes `Refs` and skips remaining roles. Publish: `Ready for CI (#<a>, #<b>, #<c>)`, one link line per issue. Escalating: stops every issue. Follow-ups: the "two separable concerns" sentence reworded so a follow-up never joins the run. Never: no added issue, no per-issue role agent. Single-issue wording is kept for every step. | Issue (mechanical parts); D1–D4 |
| `squad/roles/auditor.md`, `skeptic.md`, `test-implementer.md`, `spec-author.md` | Inputs read "every issue of the run"; auditor checks every issue's requirements; test implementer's bug-fix line is per reported bug; spec author: issues touch the specs, non-goals per the issues, a blocking question is when "an issue's" behavior depends on it, and one change doc may cover issues that share a cause. `implementer.md`, `cleaner.md`, `validator.md`, `spec-checker.md` do not mention the issue and are unchanged. | Issue (role files); D1, D4 |
| `.claude/agents/*.md` (8 wrappers) | "Your prompt names the issue (every issue, in a run of several), …". | Issue ("the issue" wording) |
| `.claude/agents/README.md` | `/squad <issue> [<issue> ...]`. | Issue |
| `.claude/commands/squad.md` | Several issue numbers make one run with one PR (points to the README section); "claim the issues"; asks when the arguments contain anything other than issue numbers or URLs. | Issue |
| `CLAUDE.md` | `/squad <issue> <issue> ...` in the squad paragraph; command table row "one or more GitHub issues". | Issue |
| `squad/decisions/README.md` | Template: escalation range `1–11`; `Run` row allows `issues #<a>, #<b>`. | Follows from escalation 11 |

`AGENTS.md` describes the squad but not the invocation, so it is unchanged (the issue says to adapt it only if it does).

Judgment calls the issue left open, for the auditor to challenge:

- **Branch threshold.** The issue says `claude/batch-<a>-<slug>` "when long" without a bound; I set it at more than three issues, with `<a>` the first issue the prompt names.
- **Escalation 11.** D4 makes more than eight issues need the maintainer's consent. I recorded it as a new numbered escalation (11) rather than folding it into 5, because it is asked before the claim and is not scope growth. `squad/README.md` allows a new escalation only with a decisions entry naming it; `2026-10-22-1403-multi-issue-run-design.md` is that entry.
- **"No lane" example.** The README section names a bug fix plus a tooling issue as a case where the lanes meet only in behavior change, which the lane check rejects without a change doc. This follows the existing lane-check rule ("shipped source next to tooling without a change doc" fails).
- **Handover layout.** D1 says one section per issue. I chose per-issue top-level sections, with the role's own sections one level down, plus a `## Whole run` section for results that are not per issue (cleaner build, validator run, lane check).

Single-issue behavior: every changed step keeps its single-issue form and adds the several-issue form in parentheses or in a sentence that starts "For several issues" / "In a run of several issues". So a single-issue prompt reads the same steps, branch, commit subjects, PR title, and link as before.

## Policy owners

Owner of the batch rules: `squad/README.md` *Several issues in one run*. `lead.md`, the role files, and the command point to it rather than restating it. Checked with `rg -n -i "the issue|issue #<N>|Closes #<N>|one fresh agent per role|/squad <issue>" squad .claude CLAUDE.md AGENTS.md`. Every remaining "the issue" in `squad/roles/` (skeptic Q1 and Q5, test-implementer reproduction steps) is covered by the README convention that "the issue" in a role file means each issue of the run.

## Tests run

None. The diff touches only Markdown. `rg` found no test reading these files besides `PhaseOwnershipDocumentationTests`, which reads only the phase-ownership map in `AGENTS.md`, and `AGENTS.md` is unchanged. `LaneClassifierTests` classifies `squad/lead.md` as tooling; that classification is unchanged.

## Disputed tests or scope

_None._ The diff covers 18 Markdown files under `squad/`, `.claude/`, and `CLAUDE.md`, all named by the issue or holding its "the issue" wording. It adds no public API, no dependency, and no compiled change.

## Follow-up candidates

_None._
