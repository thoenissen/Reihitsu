# The squad

The squad is how agents work in this repository. The maintainer talks only to the **lead**; the lead delegates every stage to a role agent with a narrow job and stops to ask whenever a decision belongs to the maintainer. [Establish the squad](../sdd/changes/2026-10/establish-squad.md) records why it is built this way.

This file is the process every role follows. The lead's playbook is [`lead.md`](lead.md), each role's job is in [`roles/`](roles/), known failure classes are in [`failure-classes.md`](failure-classes.md), and escalation answers are in [`decisions/`](decisions/). The intended behavior of the code is in [`sdd/specs/`](../sdd/specs/README.md), governed by [`sdd/README.md`](../sdd/README.md).

## Roles

| Role | Job | Must not |
|---|---|---|
| [Lead](lead.md) | Talks to the maintainer, claims the lane, spawns roles, routes findings, escalates, publishes the PR and watches its CI | Write specs, tests, or production code |
| [Spec author](roles/spec-author.md) | Writes the change doc (when required), the spec diff, and the documentation; writes every text change of the docs lane; lists open questions it finds in passing | Touch code or tests, beyond comment-only edits in the docs lane |
| [Skeptic](roles/skeptic.md) | Challenges the premise: is the issue real, is the spec wrong instead, does it conflict with other specs or `shared/`, which input breaks it, is there a simpler option | Review code |
| [Test implementer](roles/test-implementer.md) | Derives tests from the spec examples and the failure-class catalog; for a bug, the first test is the reproduction | Touch production code |
| [Implementer](roles/implementer.md) | Makes the tests pass; builds in Debug | Change test assertions |
| [Cleaner](roles/cleaner.md) | Runs `reihitsu-format`, makes the Release build warning-free | Change behavior, add suppressions or ruleset changes, change assertions |
| [Spec checker](roles/spec-checker.md) | Verifies specs, change doc, and user documentation against the diff | Change anything |
| [Auditor](roles/auditor.md) | Final quality check of the finished tree against specs and the failure-class catalog | Change anything |
| [Validator](roles/validator.md) | Builds the solution and runs all tests | Diagnose or fix |

Roles are strictly separated. A role that has to re-read everything was given a poor handover; a lead that knows everything has done too much itself.

## Lanes

The lead claims a lane at the start. The diff's paths decide whether the claim holds, not the lead's judgment: `scripts/lane-check.sh <lane>` classifies every changed path and names the narrowest lane that admits them. Every skipped role is recorded with its reason in the PR's run report.

| Lane | Spec author | Skeptic | Test impl. | Implementer | Cleaner | Spec checker | Auditor | Validator |
|---|---|---|---|---|---|---|---|---|
| Docs / text only (proven) | ✓ writes the text | – | – | – | – | ✓ | – | only if a compiled file changed |
| Tests for existing behavior | – | – | ✓ | – | ✓ | ✓ | – | ✓ |
| Bug fix | only for a clarification | ✓ premise check | ✓ reproduction | ✓ | ✓ | ✓ | ✓ | ✓ |
| Behavior change / new rule | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tooling | – | – | ✓ when the tool has tests | ✓ | ✓ | ✓ | ✓ | ✓ |

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator.

What the lane check derives from the paths:

- **Docs** — `sdd/`, `documentation/`, Markdown, issue and PR templates, and C# files when `scripts/verify-text-only.sh` proves the whole diff text-only.
- **Tests** — files of test projects, besides text.
- **Bug fix** — shipped source under `src/` without a change doc.
- **Behavior change** — shipped source with a change doc under `sdd/changes/`. A claim of this lane without a change doc fails.
- **Tooling** — `scripts/`, `.github/`, `.claude/`, `squad/`, `AGENTS.md`, `CLAUDE.md`, `Reihitsu.Tooling`, project files, `Directory.Build.props`, rulesets, `.editorconfig`, the solution.

A claim may be wider than the diff — a bug that does not reproduce ends in the bug-fix lane with only a test. A narrower claim fails, and so does a diff no lane admits (an unclassified path, or shipped source next to tooling without a change doc). Lanes widen along two orders: docs → tests → bug fix → behavior change, and docs → tests → tooling → behavior change.

## Handovers are files

Every role writes its result to `sdd/work/<slug>/<nn>-<role>.md`, where `<slug>` names the run and `<nn>` counts up from `01`. The next role reads that file plus the specs, nothing else from the conversation. The lead keeps only verdicts and summaries.

A handover starts with three lines — `Role`, `Commit` (the commit the role's statements were derived from), `Verdict` — followed by the sections its role file prescribes. It:

- carries facts: inputs, outputs, paths, test names, commands, and results;
- never carries an intended fix, a suspected cause, or what the next role should find — a role that receives one ignores it and says so;
- states what the role established and how, so the next role can try to break the claim instead of redoing the work;
- refers to issues and PRs by number (`thoenissen/Reihitsu#<N>`), never by a pasted copy;
- quotes the maintainer's clarifications verbatim when they matter; they override the issue text.

`sdd/work/` is committed to the branch, so a run can continue in a later session, and it is deleted before the PR is published.

## Escalations

The lead stops and asks the maintainer when:

1. the spec author has an open question about intended behavior that blocks the run (one found in passing is filed instead, see [`roles/spec-author.md`](roles/spec-author.md));
2. a change doc is required — the maintainer approves What and Why before tests are written;
3. the skeptic says the change should not be made;
4. a test turns out to be wrong, i.e. the spec would have to change mid-run;
5. scope grows beyond the change doc: new rule, public API, dependency, a change to a shared helper with other consumers, or more than double the intended production paths;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the auditor still blocks after one repair round;
8. a bug does not reproduce, or the report carries no scenario concrete enough to test;
9. a failure also exists on `main`, independent of the change;
10. no lane admits the diff, or the lane must widen after the implementer has started.

Every answer is recorded in `squad/decisions/` (see [`decisions/README.md`](decisions/README.md)). When the same kind of question keeps getting the same answer, the maintainer turns it into a rule and the escalation goes away. When something slipped through that should have reached the maintainer, a new escalation is added. The squad never tunes its own escalations, and a new rule is added only together with a decisions entry naming the failure it prevents.

## Repairs

The lead routes each finding — from the spec checker, the auditor, the validator, or CI — to the role that owns it: code to the implementer, a missing or weak test to the test implementer, a spec gap to the spec author, formatting or warnings to the cleaner. All findings of one verdict form one worklist and one repair round. A second block is escalation 7.

## Rules for every role

- Commit your own files and your handover with a subject ending in `[skip ci]`; stage only the files you wrote, never `git add -A`. Only the lead pushes and touches GitHub.
- Use the repository scripts (`scripts/README.md`), not hand-written `dotnet` commands; use focused `--filter` runs while working.
- Never silence, skip, or `[Ignore]` a test, and never change `TargetFramework` or add a `global.json`.
- Edit nothing outside the run's scope; an out-of-scope find goes into your handover as a follow-up candidate.
- Source carries no `TODO` and no reference to this repository's issue or PR numbers (`AGENTS.md`).
- Work frugally: `rg` instead of browsing, batch independent reads, read a file once, keep build and test output minimal, send large output to a file and quote the actionable tail, do not narrate unchanged state. Never trim a failing result.
- English only.
