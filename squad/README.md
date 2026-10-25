# The squad

The squad is how agents work in this repository. The maintainer talks only to the **lead**; the lead delegates every stage to a role agent with a narrow job and stops to ask whenever a decision belongs to the maintainer. [Introduce Spec Driven Development](../sdd/changes/2026-10/introduce-sdd.md) records why it is built this way.

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
| [Spec checker](roles/spec-checker.md) | Verifies specs, change doc, user documentation, and the requirement tags of new tests against the diff | Change anything |
| [Auditor](roles/auditor.md) | Final quality check of the finished tree against specs and the failure-class catalog | Change anything |
| [Validator](roles/validator.md) | Builds the solution and runs all tests | Diagnose or fix |

Roles are strictly separated. A role that has to re-read everything was given a poor handover; a lead that knows everything has done too much itself.

## Lanes

The lead claims a lane at the start. The diff's paths decide whether the claim holds, not the lead's judgment: `scripts/lane-check.sh <lane>` classifies every changed path and names the narrowest lane that admits them. Every skipped role is recorded with its reason in the PR's run report.

| Lane | Spec author | Skeptic | Test impl. | Implementer | Cleaner | Spec checker | Auditor | Validator |
|---|---|---|---|---|---|---|---|---|
| Docs / text only (proven) | ✓ writes the text | – | – | – | – | ✓ | – | only if a compiled file changed |
| Specs and tests for existing behavior | ✓ writes the spec, when the run adds one | ✓ spec against the code | ✓ | – | ✓ | ✓ | – | ✓ |
| Bug fix | only for a clarification | ✓ premise check | ✓ reproduction | ✓ | ✓ | ✓ | ✓ | ✓ |
| Behavior change / new rule | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tooling | – | – | ✓ when the tool has tests | ✓ | ✓ | ✓ | ✓ | ✓ |

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator.

What the lane check derives from the paths:

- **Docs** — `sdd/`, `documentation/`, Markdown, issue and PR templates, and C# files when `scripts/verify-text-only.sh` proves the whole diff text-only.
- **Tests** — files of test projects, besides text. This is also the lane of a run that writes the spec of an existing rule: the spec's requirements need their covering tests in the same PR (`sdd/README.md`).
- **Bug fix** — shipped source under `src/` without a change doc.
- **Behavior change** — shipped source with a change doc under `sdd/changes/`. A claim of this lane without a change doc fails.
- **Tooling** — `scripts/`, `.github/`, `.claude/`, `squad/`, `AGENTS.md`, `CLAUDE.md`, `Reihitsu.Tooling`, project files, `Directory.Build.props`, rulesets, `.editorconfig`, the solution.

A claim may be wider than the diff — a bug that does not reproduce ends in the bug-fix lane with only a test. A narrower claim fails, and so does a diff no lane admits (an unclassified path, or shipped source next to tooling without a change doc). Lanes widen along two orders: docs → tests → bug fix → behavior change, and docs → tests → tooling → behavior change. A docs-lane diff cannot add a requirement to an enforced spec, because the requirement needs a covering test; that run widens to tests.

## Handovers are files

Every role writes its result to `sdd/work/<slug>/<nn>-<role>.md`, where `<slug>` names the run and `<nn>` counts up from `01`. The next role reads that file plus the specs, nothing else from the conversation. The lead keeps only verdicts and summaries.

A handover starts with three lines — `Role`, `Commit` (the commit the role's statements were derived from), `Verdict` — followed by the sections its role file prescribes. It:

- carries facts: inputs, outputs, paths, test names, commands, and results;
- never carries an intended fix, a suspected cause, or what the next role should find — a role that receives one ignores it and says so;
- states what the role established and how, so the next role can try to break the claim instead of redoing the work;
- refers to issues and PRs by number (`thoenissen/Reihitsu#<N>`), never by a pasted copy;
- summarizes the maintainer's clarifications in English when they matter, never quoting them verbatim; they override the issue text.

A run of several issues splits every handover by issue ([below](#several-issues-in-one-run)).

`sdd/work/` is committed to the branch, so a run can continue in a later session, and it is deleted before the PR is published.

## Escalations

The lead stops and asks the maintainer when:

1. the spec author has an open question about intended behavior: a blocking one stops the run at once; the ones found in passing are asked together before the auditor (see [`roles/spec-author.md`](roles/spec-author.md));
2. a change doc is required — the maintainer approves What and Why before tests are written;
3. the skeptic says the change should not be made;
4. a test turns out to be wrong, i.e. the spec would have to change mid-run;
5. scope grows beyond the change doc: new rule, public API, dependency, a change to a shared helper with other consumers, or more than double the intended production paths;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the spec checker or the auditor still blocks after the third repair round, or a repair would need a choice the maintainer has not made — narrowing a requirement or dropping its claim, a design change, more scope;
8. a bug does not reproduce, or the report carries no scenario concrete enough to test;
9. a failure also exists on `main`, independent of the change;
10. no lane admits the diff, or the lane must widen after the implementer has started — or, before the claim, the issues a prompt names for one run do not share a lane;
11. the prompt names more than eight issues for one run;
12. a role reports an architecture finding, or an audit finding repeats the class of an earlier audit finding of the same run ([below](#architecture-findings)) — asked at once, before the next role starts.

Every answer is recorded in `squad/decisions/` (see [`decisions/README.md`](decisions/README.md)). When the same kind of question keeps getting the same answer, the maintainer turns it into a rule and the escalation goes away. When something slipped through that should have reached the maintainer, a new escalation is added. The squad never tunes its own escalations, and a new rule is added only together with a decisions entry naming the failure it prevents.

## Architecture findings

A defect whose cause is structural rather than local is an **architecture finding**:

- two components own the same decision — two pipeline phases, an analyzer and its code fix, a shared helper and a private copy of it;
- a component predicts or copies another's behavior — a formatter phase computes the column a later phase will produce, or replays a rule that another component applies;
- one rule is stated in more than one place, so the statements can drift apart.

A disagreement between an analyzer and the formatter (FC-013) is not by itself an architecture finding: [P11](../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11) and [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) define how the two surfaces relate. It stays an ordinary finding and reaches escalation 12 only when it repeats the class of an earlier audit finding.

The escalation covers the defects that reach the run: a defect of one of the run's issues, a defect the change introduces, and a pre-existing defect the change newly depends on or exposes — the defects an audit finding may block on ([`roles/auditor.md`](roles/auditor.md), *Severity*). A structural defect unrelated to the run is not an architecture finding of the run; it is a follow-up candidate that names its structural cause. This section is the one statement of what an architecture finding is; the role files point here.

A role that finds one reports it in its handover under `## Architecture findings`: the components, the decision they share, and the input that shows them disagreeing or the places that state the rule. It finishes its own job without changing the structure. The lead asks the maintainer at once, before the next role starts (escalation 12), with three options: fix narrowly in this run and file the structural change as a follow-up issue; change the structure first and continue the run on top of it; or stop the run. Whether a structural defect gets a narrow fix is the maintainer's choice, never a role's.

**Failure classes, not instances.** When the skeptic or the auditor finds a defect, it enumerates the defect's class: the axes along which the failing shape can vary, and every member they span — for a column that two phases compute, the direction of the shift, what the varying character stands in front of, and whether another line is aligned to it. It traces or constructs every member and reports all failing members together in one finding, naming the members that hold. Where [`failure-classes.md`](failure-classes.md) carries a matrix for the class, the enumeration starts from it.

**A repeated class is an architecture signal.** Two findings are of the same class when they share their mechanism — the same decision, failing between the same components — and differ only in the input shape; a shared catalog ID alone does not make two findings one class. An audit finding of the same class as a finding of an earlier audit of the same run is escalation 12 instead of another repair round: one enumeration already missed a member, and repairing one member per round only finds the next.

## Repairs

The lead routes each finding — from the spec checker, the auditor, the validator, or CI — to the role that owns it: code to the implementer, a missing or weak test to the test implementer, a spec gap to the spec author, formatting or warnings to the cleaner. All findings of one verdict form one worklist and one repair round. A block after a repair round starts another round in the same way when every finding is `same requirement` or `PR-introduced` and none repeats the class of an earlier audit finding (that is escalation 12, [above](#architecture-findings)): the owners repair, then the cleaner, the spec checker, the auditor on the repair only, and the validator run. A run has at most three repair rounds; a structural change the maintainer chose under escalation 12 is not one of them. A block after the third is escalation 7.

## Several issues in one run

A run implements the issues the maintainer names in the prompt — usually one, or several (`/squad #a #b #c`) that become one run with one branch, one `sdd/work/<slug>/`, and one pull request. A single-issue run is the case the rest of this file and the role files describe; a run of several issues follows them too, with these additions:

- **The prompt sets the batch.** The issues of a run are the ones the prompt's issue references select ([`lead.md`](lead.md), section 1, step 1); other text in the prompt is context. The lead never adds an issue to a run on its own; a follow-up candidate or a related issue it finds stays outside the run. More than eight issues is escalation 11, asked before the claim.
- **One agent per role for the whole run**, never one per issue. Wherever a role file says "the issue", it means each issue of the run; wherever it keys a duty to a lane, it means the run's lane, which every issue of the run shares.
- **One section per issue in every handover.** After the three header lines, the handover has one `## thoenissen/Reihitsu#<N>` section per issue, in the order the prompt names them, each holding the sections the role file prescribes one heading level down; an issue the role has nothing to do for gets one line saying so and why. A result that belongs to the whole tree rather than to one issue — a build, a full test run, a lane check — is written once, in a final `## Whole run` section. When the issues' verdicts differ, the `Verdict` line names one per issue (`#<a>: reproduced; #<b>: not reproduced`).
- **One lane.** Every issue of a run has the same lane: the lane it would claim on its own ([`lead.md`](lead.md), *Claim the lane*). The run claims that lane, runs that lane's roles, and widens as a whole exactly like a single-issue run: a widening gives every issue the new lane and runs the roles it adds, and once the implementer has started it is escalation 10. When the issues a prompt names need different lanes — a rule-page text fix and a `scripts/` change, say — the lead asks the maintainer to split them into runs of one lane each, before the claim (escalation 10).
- **Change docs.** One change doc may cover several issues of the run that share a cause; otherwise each issue that requires one gets its own, and escalation 2 applies to each.
- **An escalation stops the whole run.** An escalation for any one issue halts role work for every issue until the maintainer answers; issues do not drop out on their own. The answer decides that issue's outcome, and the run then resumes for all of them.
- **Scope is measured against the batch.** Escalation 5 compares the scope of the whole run with what its handovers and change docs name together, not each issue with its own share.
- **One link per issue.** The pull request carries one `Closes #<N>` line per issue, or `Refs #<N>` for one the run does not close — an issue that did not reproduce, or one the maintainer's answer left open.

## Rules for every role

- Commit your own files and your handover with a subject ending in `[skip ci]`; stage only the files you wrote, never `git add -A`. Only the lead pushes and touches GitHub.
- Use the repository scripts (`scripts/README.md`), not hand-written `dotnet` commands; use focused `--filter` runs while working.
- Never silence, skip, or `[Ignore]` a test, and never change `TargetFramework` or add a `global.json`.
- Edit nothing outside the run's scope; an out-of-scope find goes into your handover as a follow-up candidate.
- Source carries no `TODO` and no reference to this repository's issue or PR numbers (`AGENTS.md`).
- Work frugally: `rg` instead of browsing, batch independent reads, read a file once, keep build and test output minimal, send large output to a file and quote the actionable tail, do not narrate unchanged state. Never trim a failing result.
- English only.
