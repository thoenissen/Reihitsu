# The squad

The squad is how agents work in this repository. The maintainer talks only to the **lead**; the lead delegates every stage to a role agent with a narrow job, decides every internal question itself, and asks the maintainer only when an output an end user sees is in question ([below](#what-reaches-the-maintainer)). [Introduce Spec Driven Development](../sdd/changes/2026-10/introduce-sdd.md) records why it is built this way.

This file is the process every role follows. The lead's playbook is [`lead.md`](lead.md), each role's job is in [`roles/`](roles/), known failure classes are in [`failure-classes.md`](failure-classes.md), and run notes for improving the squad are in [`notes/`](notes/README.md). The intended behavior of the code is in [`sdd/specs/`](../sdd/specs/README.md), governed by [`sdd/README.md`](../sdd/README.md).

## Roles

| Role | Job | Must not |
|---|---|---|
| [Lead](lead.md) | Talks to the maintainer, claims the lane, spawns roles, routes findings, decides internal questions, asks output questions, manages the pull request and watches its CI | Write specs, tests, or production code, or write issues |
| [Spec author](roles/spec-author.md) | Writes the change doc (when required), the spec diff, and the documentation; writes every text change of the docs lane; lists the open questions it finds | Touch code or tests, beyond comment-only edits in the docs lane |
| [Skeptic](roles/skeptic.md) | Challenges the premise: is the issue real, is the spec wrong instead, does it conflict with other specs or `shared/`, which input breaks it, is there a simpler option | Review code |
| [Test implementer](roles/test-implementer.md) | Derives tests from the spec examples and the failure-class catalog; for a bug, the first test is the reproduction | Touch production code |
| [Implementer](roles/implementer.md) | Makes the tests pass; builds in Debug | Change test assertions |
| [Cleaner](roles/cleaner.md) | Runs `reihitsu-format`, makes the Release build warning-free | Change behavior, add suppressions or ruleset changes, change assertions |
| [Spec checker](roles/spec-checker.md) | Verifies specs, change doc, user documentation, and the requirement tags of new tests against the diff | Change anything |
| [Auditor](roles/auditor.md) | Final quality check of the finished tree against specs and the failure-class catalog | Change anything |
| [Validator](roles/validator.md) | Builds the solution and runs every test project except the self-hosting scans, which CI runs | Diagnose or fix |
| [Issue manager](roles/issue-manager.md) | Manages issues: searches duplicates, creates and extends follow-up issues, labels them `agent-filed`, attaches sub-issues | Touch the repository or the pull request |

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

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. In every lane, the issue manager runs once before the PR is published, when the run has follow-up candidates ([`lead.md`](lead.md), *Follow-ups*).

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
- summarizes the maintainer's clarifications and answers in English when they matter, never quoting them verbatim; they override the issue text.

A run of several issues splits every handover by issue ([below](#several-issues-in-one-run)).

`sdd/work/` is committed to the branch, so a run can continue in a later session, and it is deleted before the PR is published.

## Issues are hypotheses

Most issues are written by agents, not by the maintainer. An issue an agent writes carries the label `agent-filed`, meaning its text is not reviewed by the maintainer; only the maintainer removes the label, and only from an issue whose text they wrote or reviewed.

- For an `agent-filed` issue, the premise, the goals, the "Done when" items, the target behavior, and any prescribed solution are proposals. The skeptic challenges them, and any role reports a goal that turns out not to make sense once the work shows more than the issue knew. The lead drops or adjusts such a goal on its own when no visible output is in question, records it under *Decisions taken*, and the PR states how the result differs from the issue: `Closes #<N>` when the issue's purpose is met, `Refs #<N>` otherwise. A goal whose change would put a visible output in question is an output question.
- For an issue without the label, the premise is still checked (`AGENTS.md`, *Issue intake*), but its goals are binding: a departure from them is a question for the maintainer.
- For every issue, the premise includes whether one of the repository's own callers reaches the reported case (`AGENTS.md`, [*Supported inputs*](../AGENTS.md#supported-inputs)). The skeptic checks it before anything is built; an issue whose case no caller reaches is handled as that section states, and the lead reports it as the run's result ([`lead.md`](lead.md), section 4).

## What reaches the maintainer

The maintainer decides what an end user sees; the lead decides everything else. The visible surfaces are:

- the formatter's output;
- analyzer diagnostics — whether a diagnostic is reported, its location, message, and severity;
- code-fix results — whether a fix is offered or withheld, and what it writes;
- rule pages under `documentation/rules/`;
- CLI behavior — options, exit codes, and messages.

Everything else is internal: spec wording, requirement IDs and spec ownership, which input facts or which component carries a decision, structure, tests, tooling, scope, lanes, change-doc text, and the text of follow-up issues. No assembly of this repository is used as a library, so its API is internal too, and a component only has to work for the inputs the repository's own callers pass (`AGENTS.md`, [*Supported inputs*](../AGENTS.md#supported-inputs)): an output no such caller reaches is never in question.

A visible output is **in question** when its change is not decided yet: no unchanged spec requirement demands it, and no maintainer answer covers it. A bug fix that brings the code back to an unchanged requirement puts nothing in question. In question are:

- a visible output change that a change doc lists (`sdd/README.md`, *Change docs*);
- a case within the run's issues whose real output looks wrong and that no spec decides (`sdd/README.md`, *Open questions*);
- two readings of a spec that give different output for the same input;
- a test whose asserted visible output turns out to be wrong;
- a dropped or changed goal of an issue that would change visible output, and any departure from the goals of an issue without the `agent-filed` label.

Each of these is an **output question**, asked in the form `Output questions` in [`AGENTS.md`](../AGENTS.md#output-questions) describes. Output questions are batched: the lead asks every open one in one message at two points of the run — **before the test implementer starts**, after the spec author, the skeptic, and any amendment the skeptic caused, because tests lock outputs in; and **before the auditor**, for the questions found later. A question that blocks writing a test is asked at once. A case outside the run's issues is not asked: it becomes an `agent-filed` follow-up issue that carries the question in the same form.

The auditor checks that nothing visible slipped through (*Visible output is decided*, [`roles/auditor.md`](roles/auditor.md)): every changed expected value in a test of a visible surface is demanded by an unchanged requirement or covered by a maintainer answer.

## Escalations

The lead stops and asks the maintainer only when:

1. an output question is open ([above](#what-reaches-the-maintainer)) — at the two batching points, or at once when it blocks writing a test;
2. a safety rail is hit — the run's change, or an output the run changes, drops, relocates, or alters user code (comments, directives, disabled text, literals, executable meaning), produces code that does not compile, or breaks idempotency or a fix's convergence, and the run cannot repair it within its scope; the lead never ships, defers, or accepts such a defect on its own;
3. the skeptic says an issue without the `agent-filed` label should not be changed at all, or says so for an `agent-filed` issue whose goals the lead cannot drop without putting a visible output in question;
4. a bug does not reproduce, or the report carries no scenario concrete enough to test;
5. the run would need a new rule — a new diagnostic is visible output;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the narrowing after the third repair round would change an output the maintainer answered, or the run still blocks after it ([below](#repairs));
8. before the claim: an issue reference in the prompt is unclear, an existing claim exists, the prompt names more than eight issues, or its issues do not share a lane.

The maintainer's answer to an output question is recorded where it takes effect — the change doc's `Decisions` section or the spec — and in the run report. The answer to any other escalation is recorded in the run report.

## Decisions taken

Every other question the lead decides itself, on the recommendation of the role that raised it or its own, and never stops for it: whether a change doc is required and whether to accept it; spec wording and requirement scope; a test or a requirement that is wrong where nothing visible is in question; scope growth and adjacent fixes ([below](#scope)); architecture findings and repeated classes ([below](#architecture-findings)); a failure that also exists on `main`; widening the lane; merging `main` after an audit; the goals of `agent-filed` issues where nothing visible is in question; and every follow-up issue.

Each decision is a row of the `Decisions taken` table in `00-lead.md`, which the run report carries into the PR:

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

`Commit` names the commit that carries the decision out, so the maintainer can revert one row while reviewing. A row the maintainer overrules in review is carried out like a repair.

## Scope

The run implements the issues the prompt names, but its scope is not frozen. The lead may pull an adjacent fix into the run instead of filing an issue when all of these hold:

- it is a defect, or a cleanup the run's change touches anyway;
- it is small — as a rule of thumb, a few lines — and cheaper in this run than as a run of its own;
- it is in a component the run already changes, or in that component's counterpart;
- it gets a test;
- it puts no visible output in question;
- the lane check still admits the diff.

Every other find becomes a follow-up, unless no caller of the repository reaches it (`AGENTS.md`, *Supported inputs*); such a find is dropped. A role that sees the change grow — a shared helper with other consumers, more than double the production paths the handovers name, or an adjacent fix — reports it in its handover and keeps working; the lead decides and records it. Only a new rule stops the run (escalation 5).

## Notes

Before publishing, the lead writes one note per run to [`notes/`](notes/README.md): the output questions and their answers, the number of decisions taken, the stops and repair rounds, where the run got expensive, and suggestions for the process. Notes are input for improving the squad and are temporary. The squad never tunes its own process from them: the maintainer, or a session the maintainer starts for it, turns a pattern into a rule, records the reason in a process change doc under `sdd/changes/`, and deletes the notes it folds in.

## Architecture findings

A defect whose cause is structural rather than local is an **architecture finding**:

- two components own the same decision — two pipeline phases, an analyzer and its code fix, a shared helper and a private copy of it;
- a component predicts or copies another's behavior — a formatter phase computes the column a later phase will produce, or replays a rule that another component applies;
- one rule is stated in more than one place, so the statements can drift apart.

A disagreement between an analyzer and the formatter (FC-013) is not by itself an architecture finding: [P11](../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11) and [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) define how the two surfaces relate. It stays an ordinary finding and is handled as an architecture finding only when it repeats the class of an earlier audit finding.

The escalation covers the defects that reach the run: a defect of one of the run's issues, a defect the change introduces, and a pre-existing defect the change newly depends on or exposes — the defects an audit finding may block on ([`roles/auditor.md`](roles/auditor.md), *Severity*). A structural defect unrelated to the run is not an architecture finding of the run; it is a follow-up candidate that names its structural cause. This section is the one statement of what an architecture finding is; the role files point here.

A role that finds one reports it in its handover under `## Architecture findings`: the components, the decision they share, and the input that shows them disagreeing or the places that state the rule. It finishes its own job without changing the structure. The lead decides before the next role starts, and records the decision under *Decisions taken*:

- **Change the structure in this run** when the class has repeated in an audit of this run, or when the narrow fix would itself add a copy, a prediction, or a second statement of the rule — provided the structural change stays within the components the run changes and their counterparts. It is routed to its owner like a repair, from the first role of the lane it affects, and does not use up a repair round.
- **Otherwise, fix narrowly** and file the structural change as a follow-up.
- A structural change that would put a visible output in question is an output question; one that needs a new rule is escalation 5.

Whether a structural defect gets a narrow fix is the lead's decision, never a role's.

**Failure classes, not instances.** When the skeptic or the auditor finds a defect, it enumerates the defect's class: the axes along which the failing shape can vary, and every member they span that one of the repository's own callers reaches (`AGENTS.md`, *Supported inputs*) — for a column that two phases compute, the direction of the shift, what the varying character stands in front of, and whether another line is aligned to it. It traces or constructs every member and reports all failing members together in one finding, naming the members that hold. Where [`failure-classes.md`](failure-classes.md) carries a matrix for the class, the enumeration starts from it.

**A repeated class is an architecture signal.** Two findings are of the same class when they share their mechanism — the same decision, failing between the same components — and differ only in the input shape; a shared catalog ID alone does not make two findings one class. An audit finding of the same class as a finding of an earlier audit of the same run is handled as an architecture finding instead of another repair round: one enumeration already missed a member, and repairing one member per round only finds the next.

## Repairs

The lead routes each finding — from the spec checker, the auditor, the validator, or CI — to the role that owns it: code to the implementer, a missing or weak test to the test implementer, a spec gap to the spec author, formatting or warnings to the cleaner. All findings of one verdict form one worklist and one repair round. A block after a repair round starts another round in the same way when every finding is `same requirement` or `PR-introduced` and none repeats the class of an earlier audit finding (that is an architecture finding, [above](#architecture-findings)): the owners repair, then the cleaner, the spec checker, the auditor on the repair only, and the validator run. A repair asks the maintainer nothing unless it puts a visible output in question; a change-doc amendment is asked only for the visible output changes it adds or alters.

A run has at most three repair rounds; a structural change under *Architecture findings* is not one of them. A block after the third is resolved by **narrowing**: the failing part goes back to `main`'s behavior while the run keeps its other gains, an issue whose purpose is then no longer met is linked with `Refs #<N>`, and the narrowed part becomes a follow-up. The narrowing runs as one final round. When it would change an output the maintainer answered, or the run still blocks after it, that is escalation 7.

## Several issues in one run

A run implements the issues the maintainer names in the prompt — usually one, or several (`/squad #a #b #c`) that become one run with one branch, one `sdd/work/<slug>/`, and one pull request. A single-issue run is the case the rest of this file and the role files describe; a run of several issues follows them too, with these additions:

- **The prompt sets the batch.** The issues of a run are the ones the prompt's issue references select ([`lead.md`](lead.md), section 1, step 1); other text in the prompt is context. The lead never adds an issue to a run on its own; an adjacent fix may join the run ([Scope](#scope)), but a related issue stays outside it. More than eight issues is escalation 8, asked before the claim.
- **One agent per role for the whole run**, never one per issue. Wherever a role file says "the issue", it means each issue of the run; wherever it keys a duty to a lane, it means the run's lane, which every issue of the run shares.
- **One section per issue in every handover.** After the three header lines, the handover has one `## thoenissen/Reihitsu#<N>` section per issue, in the order the prompt names them, each holding the sections the role file prescribes one heading level down; an issue the role has nothing to do for gets one line saying so and why. A result that belongs to the whole tree rather than to one issue — a build, a full test run, a lane check — is written once, in a final `## Whole run` section. When the issues' verdicts differ, the `Verdict` line names one per issue (`#<a>: reproduced; #<b>: not reproduced`).
- **One lane.** Every issue of a run has the same lane: the lane it would claim on its own ([`lead.md`](lead.md), *Claim the lane*). The run claims that lane, runs that lane's roles, and widens as a whole exactly like a single-issue run: a widening gives every issue the new lane and runs the roles it adds, and once the implementer has started the lead decides the widening and records it. When the issues a prompt names need different lanes — a rule-page text fix and a `scripts/` change, say — the lead asks the maintainer to split them into runs of one lane each, before the claim (escalation 8).
- **Change docs.** One change doc may cover several issues of the run that share a cause; otherwise each issue that requires one gets its own, and the visible output changes of every one of them are asked together.
- **An escalation stops the whole run.** An escalation or an output question for any one issue halts role work for every issue until the maintainer answers; issues do not drop out on their own. The answer decides that issue's outcome, and the run then resumes for all of them.
- **Scope is measured against the batch.** [Scope](#scope) compares the scope of the whole run with what its handovers and change docs name together, not each issue with its own share.
- **One link per issue.** The pull request carries one `Closes #<N>` line per issue, or `Refs #<N>` for one the run does not close — an issue that did not reproduce, or one the maintainer's answer left open.

## Rules for every role

- Commit your own files and your handover with a subject ending in `[skip ci]`; stage only the files you wrote, never `git add -A`. Only the lead pushes and manages the pull request; only the issue manager writes issues.
- Use the repository scripts (`scripts/README.md`), not hand-written `dotnet` commands; use focused `--filter` runs while working.
- Never silence, skip, or `[Ignore]` a test, and never change `TargetFramework` or add a `global.json`.
- Edit nothing outside the run's scope; an out-of-scope find goes into your handover as a follow-up candidate, unless no caller of the repository reaches it (`AGENTS.md`, *Supported inputs*).
- Source carries no `TODO` and no reference to this repository's issue or PR numbers (`AGENTS.md`).
- Work frugally: `rg` instead of browsing, batch independent reads, read a file once, keep build and test output minimal, send large output to a file and quote the actionable tail, do not narrate unchanged state. Never trim a failing result.
- English only.
