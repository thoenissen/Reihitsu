# Establish the squad

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected specs | none — process change |
| Depends on | [Introduce Spec Driven Development](introduce-sdd.md) |

## What

The agent workflow is rebuilt around a squad: the maintainer talks only to a **lead**, and the lead delegates every stage to a role agent with a narrow job. The role definitions are agent-neutral; agent-specific files only link to them. Codex support is removed. The review flows (`gh-review`, `gh-rereview`, `gh-apply-review`) are retired: a PR the squad publishes is finished and needs no further review round.

## Why

- The current skills grew to roughly 2,500 lines because every run had to reconstruct the intended behavior from scratch. With specs, that knowledge is written once; the workflow can shrink without losing quality.
- Strict roles make independence structural: tests are written by someone who did not write the code, the premise is challenged by someone who did not write the spec.
- Agent-neutral documentation lets other agents be added later without duplicating instructions.

## Decisions

### Agent-neutral documentation

```
AGENTS.md                 repository instructions for any agent
CLAUDE.md                 contains only "@AGENTS.md" plus Claude-specific notes
squad/
  README.md               process: lanes, handovers, escalations
  lead.md                 lead playbook
  roles/<role>.md         one file per role
  failure-classes.md      catalog of known failure classes
  decisions/<date>-<slug>.md   one file per escalation decision
.claude/agents/<role>.md  thin wrapper: model, effort, tools, link to squad/roles/<role>.md
```

Claude Code reads `CLAUDE.md`, not `AGENTS.md`; the `@AGENTS.md` import makes `AGENTS.md` the single source. `.codex/` and the Codex-specific content of `AGENTS.md` are removed. Model tier and effort stay in the agent-specific wrapper files, because they are agent-specific.

### Roles

| Role | Job | Must not |
|---|---|---|
| Lead | Talks to the maintainer, picks the lane, spawns roles, routes findings, escalates, publishes the PR | Write specs, tests, or production code itself |
| Spec author | Writes the change doc (when required) and the spec diff | Touch code or tests |
| Skeptic | Challenges the premise: is the issue real, is the spec wrong instead, does it conflict with other specs or `shared/`, which input breaks it, is there a simpler option | Review code |
| Test implementer | Derives tests from the spec examples and the failure-class catalog; for a bug, the first test is the reproduction | Touch production code |
| Implementer | Makes the tests pass; builds in Debug | Change test assertions — a test it believes is wrong goes back to the lead |
| Cleaner | Runs `reihitsu-format`, makes the Release build warning-free | Change behavior, add suppressions or ruleset changes, change assertions |
| Spec checker | Verifies that specs, change doc, and user documentation match the diff, that a change doc exists exactly when required, and that named code owners exist | Change anything |
| Auditor | Final quality check of the finished tree against specs and the failure-class catalog | Change anything |
| Validator | Builds the solution and runs all tests | Diagnose or fix |

Roles are strictly separated. A role that has to re-read everything was given a poor handover; a lead that knows everything has done too much itself.

### Handovers are files

Every role writes its result to `sdd/work/<slug>/<nn>-<role>.md`. The next role reads that file plus the specs, nothing else from the conversation. The lead keeps only verdicts and summaries. Because handovers are committed to the branch, a run can continue in a later session. `sdd/work/` is deleted before the PR is published.

### Lanes

The lead claims a lane at the start; a mechanical check against the final diff confirms it before publishing, as `verify-text-only.sh` does today. The diff's paths decide, not the lead's judgment. Every skipped role is recorded with its reason in the PR's run report.

| Lane | Spec author | Skeptic | Test impl. | Implementer | Cleaner | Spec checker | Auditor | Validator |
|---|---|---|---|---|---|---|---|---|
| Docs / text only (proven) | – | – | – | – | – | ✓ | – | only if a compiled file changed |
| Tests for existing behavior | – | – | ✓ | – | ✓ | ✓ | – | ✓ |
| Bug fix | only for a clarification | ✓ premise check | ✓ reproduction | ✓ | ✓ | ✓ | ✓ | ✓ |
| Behavior change / new rule | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator.

A bug the test implementer cannot reproduce ends the run: the passing test stays as a characterization test, and the maintainer decides how to proceed.

### Repairs

The lead routes each auditor or spec-checker finding to the role that owns it: code to the implementer, a missing test to the test implementer, a spec gap to the spec author. One repair round is allowed; a second block is an escalation.

### Escalations

The lead stops and asks the maintainer when:

1. the spec author has an open question about intended behavior;
2. a change doc is required — the maintainer approves What and Why before tests are written;
3. the skeptic says the change should not be made;
4. a test turns out to be wrong, i.e. the spec would have to change mid-run;
5. scope grows beyond the change doc: new rule, public API, dependency;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the auditor still blocks after one repair round;
8. a bug does not reproduce.

### Tuning over time

Every escalation is recorded in `squad/decisions/`: question, answer, short reason — one file per entry, so parallel PRs do not conflict. When the same kind of question gets the same answer repeatedly, the maintainer turns it into a rule and the escalation goes away. When something slipped through that should have reached the maintainer, a new escalation is added. The squad never tunes its own escalations.

### Keeping it lean

- Every existing rule in the current skills is sorted into one of three buckets: **knowledge about the code** (moves into `shared/` and pipeline specs and into `failure-classes.md`), **process mechanics** (moves into the lead playbook), or **obsolete** (Behavior Contract, admission artifact, review flows — removed). An agent produces the sorting as a table; the maintainer decides per row.
- `failure-classes.md` holds one line per known failure class plus its origin. The test implementer works through it for matching inputs; the auditor checks the same list.
- Each role file has a budget of about 150 lines. Content beyond that belongs in specs or the catalog.
- A new rule is added only together with a decisions entry naming the failure it prevents.

## Alternatives considered

- **The lead implements directly** to save context loading — rejected: it blurs responsibility, and a good handover makes the cost small.
- **A separate reproducer role** — rejected: the test implementer's first test for a bug is the reproduction.
- **The skeptic also reviews the finished code** — rejected: that is the auditor's job; the skeptic is most valuable before anything is built.
- **Starting lean and dropping the existing rules** — rejected: the rules encode real failures. They are sorted and condensed instead.
- **Automatic tuning of escalations** — rejected: the squad would decide on its own when it needs the maintainer.
- **Keeping the review flows** — rejected: the preflight already made them unnecessary in practice.
