# The lead chooses between continuing and spawning, and keeps its context small

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [The squad gives follow-up work to fresh agents](../2026-10/squad-fresh-agents-for-follow-ups.md) |

## What

The rule that every follow-up goes to a fresh agent is replaced by a decision table in the lead's playbook: the lead continues an agent for the maintainer's answer, for evidence for its own open question, or for a small correction while the agent is still small, and spawns a fresh agent for a large agent, another mechanism or area, or a repair round. Several findings for one role go to one agent. The skeptic, the spec checker, and the auditor stay fresh. A size threshold from `scripts/usage.sh` (responses and cache read per response) tells when an agent is large.

The lead keeps its own context small: `00-lead.md` is brought up to date at every stage boundary, `CLAUDE.md` carries compact instructions, a hint to the summarizer, that say what a compaction keeps, and the lead keeps `00-lead.md` current as it goes, since a run can go for hours without the maintainer and the harness compacts on its own.

Smaller changes from the same evaluation of the run notes: an output question that would make a fix move code also asks whether the fix may be withheld; the skeptic checks entry points for predicted phase decisions and no longer derives compiler results without compiling; parallel builds use separate worktrees; `squad/notes/` is text for the lane check instead of tooling.

## Why

The notes of the squad's runs show the cost of both extremes. When the spec author was continued nine times, it dominated the run. After the switch to fresh agents for everything, the run of six issues spawned 14 spec authors, 13 test implementers, and 10 implementers; each reads the role file, the handovers, the specs, and the code again. That run read 296 million cached tokens, of which the lead's own context accounted for 50 million over 182 responses, because the lead never dropped its history although its state was already in files.

One rule cannot be right for both a long-lived agent with a growing context and a small correction to a short one. The handovers keep a run coherent either way, so the choice is only about cost, and the lead can make it from numbers it already has.

## Visible output changes

None — no output an end user sees changes.

## Decisions

### Thresholds are guidance, not a gate

About 40 responses or about 100 thousand cache-read tokens per response are starting points taken from the runs' usage tables. The lead may depart from them and records why under *Decisions taken*, so the notes can show whether the numbers fit.

### The lead cannot compact itself, and nobody may be watching

A compaction is started by the harness or the maintainer, and a run can go for hours without input. The playbook therefore does not wait for or offer a compaction: it keeps the run's state in `00-lead.md` continuously and gives `CLAUDE.md` the compact instructions, so a compaction at any turn is safe.

### No leaving a batch

A rule that reverts one issue out of a batch was considered and dropped: the reverted work is rebuilt in the follow-up run, with the same repair rounds, so little is saved and work is lost.

## Alternatives considered

- **Keep fresh agents for every follow-up** — rejected: it repeats the orientation cost for corrections that need no new reading.
- **Continue by default** — rejected: it brings back the growing context that made the earlier rule necessary.
- **A table of task types** — rejected: size and area decide, not the kind of task.
