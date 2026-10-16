# Establish the squad

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected specs | none — process change |
| Depends on | [Introduce Spec Driven Development](introduce-sdd.md) |

## What

The agent workflow is rebuilt around a squad: the maintainer talks only to a **lead**, and the lead delegates every stage to a role agent with a narrow job. The role definitions are agent-neutral; agent-specific files only link to them. Codex and GitHub Copilot support are removed. The review flows (`gh-review`, `gh-rereview`, `gh-apply-review`) are retired: a PR the squad publishes is finished and needs no further review round.

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
.claude/commands/squad.md `/squad <issue>`: the session agent becomes the lead
scripts/lane-check.*      confirms the claimed lane against the diff
```

Claude Code reads `CLAUDE.md`, not `AGENTS.md`; the `@AGENTS.md` import makes `AGENTS.md` the single source. `.codex/`, the Copilot files under `.github/` (`copilot-instructions.md`, `agents/`, `skills/`), and the Codex-specific content of `AGENTS.md` are removed. Model tier and effort stay in the agent-specific wrapper files, because they are agent-specific.

The previous workflow skills (`gh-implement`, `gh-preflight`, `gh-rubber-duck`), their agent definitions, and the task commands `create-analyzer-rule`, `fix-analyzer-rule`, `fix-formatter`, `extend-formatter`, and `draft-issue` are removed; the roles replace them. `create-rule-doc` and `add-resource-texts` stay as reference commands the roles cite. The repository scripts are Bash only; the PowerShell variants are removed, and with them `scripts/upload-issues.ps1`, which belonged to `draft-issue`: the lead drafts a follow-up issue from the repository's issue templates, and creates it only after the maintainer approved its text.

### Roles

| Role | Job | Must not |
|---|---|---|
| Lead | Talks to the maintainer, picks the lane, spawns roles, routes findings, escalates, publishes the PR and watches its CI | Write specs, tests, or production code itself, or perform a role itself |
| Spec author | Writes the change doc (when required), the spec diff, and the documentation; writes every text change of the docs lane | Touch code or tests, beyond comment-only edits in the docs lane |
| Skeptic | Challenges the premise: is the issue real, is the spec wrong instead, does it conflict with other specs or `shared/`, which input breaks it, is there a simpler option | Review code |
| Test implementer | Derives tests from the spec examples and the failure-class catalog; for a bug, the first test is the reproduction | Touch production code |
| Implementer | Makes the tests pass; builds in Debug (`scripts/build.sh --configuration Debug`) | Change test assertions — a test it believes is wrong goes back to the lead |
| Cleaner | Runs `reihitsu-format`, makes the Release build warning-free | Change behavior, add suppressions or ruleset changes, change assertions |
| Spec checker | Verifies that specs, change doc, and user documentation match the diff, that a change doc exists exactly when required, and that named code owners exist | Change anything |
| Auditor | Final quality check of the finished tree against specs and the failure-class catalog | Change anything |
| Validator | Builds the solution and runs all tests | Diagnose or fix |

Roles are strictly separated. A role that has to re-read everything was given a poor handover; a lead that knows everything has done too much itself. When roles cannot be spawned, the lead stops and reports rather than performing a role itself.

### Handovers are files

Every role writes its result to `sdd/work/<slug>/<nn>-<role>.md`. The next role reads that file plus the specs, nothing else from the conversation. The lead keeps only verdicts and summaries. Because handovers are committed to the branch, a run can continue in a later session. `sdd/work/` is deleted before the PR is published.

Each writing role commits its own files and its handover with `[skip ci]`; only the lead pushes and touches GitHub. Open questions the spec author finds in passing are therefore listed in its handover and filed by the lead.

### Lanes

The lead claims a lane at the start; `scripts/lane-check.sh` confirms it against the diff after each writing role and before publishing. The diff's paths decide, not the lead's judgment: shipped source with a change doc is a behavior change, shipped source without one a bug fix, test projects alone are tests, and text — including C# that `verify-text-only.sh` proves text-only — is docs. Every skipped role is recorded with its reason in the PR's run report.

| Lane | Spec author | Skeptic | Test impl. | Implementer | Cleaner | Spec checker | Auditor | Validator |
|---|---|---|---|---|---|---|---|---|
| Docs / text only (proven) | ✓ writes the text | – | – | – | – | ✓ | – | only if a compiled file changed |
| Tests for existing behavior | – | – | ✓ | – | ✓ | ✓ | – | ✓ |
| Bug fix | only for a clarification | ✓ premise check | ✓ reproduction | ✓ | ✓ | ✓ | ✓ | ✓ |
| Behavior change / new rule | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tooling | – | – | ✓ when the tool has tests | ✓ | ✓ | ✓ | ✓ | ✓ |

The tooling lane covers scripts, CI workflows, build settings, `Reihitsu.Tooling`, and the agent instructions (`.claude/`, `squad/`, `AGENTS.md`, `CLAUDE.md`), which none of the other lanes fits.

A claim may be wider than the diff; a narrower one fails. Lanes widen along two orders: docs → tests → bug fix → behavior change, and docs → tests → tooling → behavior change. Before the implementer has started, the lead widens the lane along them and runs the roles the wider lane adds; after that, or when no lane admits the diff, it escalates.

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator.

A bug the test implementer cannot reproduce ends the run: the passing test stays as a characterization test, the PR links the issue with `Refs` instead of `Closes`, and the maintainer decides how to proceed. Before reporting a non-reproduction, the test implementer runs the reported example on both line endings, on the counterpart surface, on the code-fix path, and on the nearest sibling shape; a second, higher-tier confirmation is not needed, because the maintainer sees every non-reproduction.

### Repairs

The lead routes each auditor, spec-checker, validator, or CI finding to the role that owns it: code to the implementer, a missing test to the test implementer, a spec gap to the spec author, formatting and warnings to the cleaner. One repair round is allowed; a second block is an escalation. An audit that passes with findings confined to comments or documentation needs no second audit when `verify-text-only.sh --strict-docs` proves the fix text-only.

### Publishing

The lead claims the issue at the start with a draft PR, keeps CI silent with `[skip ci]` until a single trigger commit at the end, and publishes the PR **ready for review** once `sdd/work/` is deleted and validation is green. It then watches CI until the PR is green; a failure the PR caused is a repair finding.

### Escalations

The lead stops and asks the maintainer when:

1. the spec author has an open question about intended behavior;
2. a change doc is required — the maintainer approves What and Why before tests are written;
3. the skeptic says the change should not be made;
4. a test turns out to be wrong, i.e. the spec would have to change mid-run;
5. scope grows beyond the change doc: new rule, public API, dependency, a change to a shared helper with other consumers, or more than double the intended production paths;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the auditor still blocks after one repair round;
8. a bug does not reproduce, or the report carries no scenario concrete enough to test;
9. a failure also exists on `main`, independent of the change;
10. no lane admits the diff, or the lane must widen after the implementer has started.

### Tuning over time

Every escalation is recorded in `squad/decisions/`: question, answer, short reason — one file per entry, so parallel PRs do not conflict. When the same kind of question gets the same answer repeatedly, the maintainer turns it into a rule and the escalation goes away. When something slipped through that should have reached the maintainer, a new escalation is added. The squad never tunes its own escalations.

### Keeping it lean

- Every existing rule in the current skills is sorted into one of three buckets: **knowledge about the code** (moves into `shared/` and pipeline specs and into `failure-classes.md`), **process mechanics** (moves into the lead playbook), or **obsolete** (Behavior Contract, admission artifact, review flows — removed). An agent produces the sorting as a table; the maintainer decides per row.
- `failure-classes.md` holds one line per known failure class plus its origin. The test implementer works through it for matching inputs; the auditor checks the same list.
- Rules that hold for every code fix — one-pass convergence, Fix All, no action that changes nothing — are code knowledge that no spec owns yet. They live in `failure-classes.md` until an analyzer-wide code-fix spec takes them over; they do not belong under `shared/`, which is reserved for policies the analyzer and the formatter must agree on.
- Source carries no `TODO`. Unfinished work becomes an approved follow-up issue linked from the PR, never from the code.
- Each role file has a budget of about 150 lines. Content beyond that belongs in specs or the catalog.
- A new rule is added only together with a decisions entry naming the failure it prevents.

## Alternatives considered

- **The lead implements directly** to save context loading — rejected: it blurs responsibility, and a good handover makes the cost small.
- **A separate reproducer role** — rejected: the test implementer's first test for a bug is the reproduction.
- **The skeptic also reviews the finished code** — rejected: that is the auditor's job; the skeptic is most valuable before anything is built.
- **Starting lean and dropping the existing rules** — rejected: the rules encode real failures. They are sorted and condensed instead.
- **Automatic tuning of escalations** — rejected: the squad would decide on its own when it needs the maintainer.
- **Keeping the review flows** — rejected: the preflight already made them unnecessary in practice.
- **Publishing the PR as a draft** — rejected: a PR the squad publishes is finished; the squad watches its CI instead.
- **A higher-tier confirmation of every non-reproduction** — rejected: the maintainer sees every non-reproduction anyway.
- **The lead performing a role when subagents are unavailable** — rejected: it breaks the role separation the squad exists for.
- **Keeping the task commands** — rejected: their workflows are what the roles now own; only the two reference commands stay.
