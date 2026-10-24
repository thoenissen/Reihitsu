# Introduce Spec Driven Development

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | none — process change |

## What

Reihitsu works by a lightweight form of Spec Driven Development (SDD), carried out by a squad of agent roles.

- The intended behavior of every shipped surface — analyzer rules, formatter, CLI, Playground, and the policies they share — is written down as specs under `sdd/specs/`. A bug is a difference between the code and a spec.
- Every requirement has an immutable ID, recorded in a ledger. Tests name the requirements they cover, and the architecture tests fail the build when an active requirement has no covering test.
- Behavior that looks wrong while a spec is written is not specified and not silently fixed: it is escalated to the maintainer as an open question, and only the outcome of the decision is recorded.
- A decision that changes intended behavior is recorded as a change doc under `sdd/changes/`. Plans and handovers of a run are temporary working docs under `sdd/work/`, never merged.
- Agents implement issues as a squad: the maintainer talks only to a lead, and the lead delegates every stage to a role with a narrow job, along a lane that the diff's paths confirm.
- The decisions that recur across many rules are recorded once as principles, written as requirements of the shared specs and as sections of `sdd/README.md`.

`sdd/README.md` is the rulebook for specs, IDs, tests, open questions, and change docs; `squad/README.md` and `squad/lead.md` are the rulebook for the squad. This doc explains why they are built as they are.

## Why

- **Navigation.** Without specs, intended formatter and analyzer behavior has to be reverse-engineered from complex code. A spec says what a phase or rule does and names the types that implement it, so an agent or a person finds the owner without searching.
- **A reference independent of the code.** Without a spec, every "is this a bug?" question is reopened from scratch, and whatever a run worked out about intended behavior is lost after its PR. A spec makes the answer durable and makes a bug a decidable difference.
- **Traceable decisions.** Why a behavior is the way it is gets lost in PR threads. A `Why:` on every requirement and a change doc for every change of intent keep the reason next to the rule.
- **A smaller, sturdier agent workflow.** The earlier agent skills grew to thousands of lines because every run had to reconstruct intended behavior. With specs, that knowledge is written once, and the workflow shrinks to roles with narrow jobs.
- **Structural independence.** Tests are written by a role that did not write the code; the premise is challenged by a role that did not write the spec; the final check is made by a role that changed nothing.
- **One source of instructions.** Agent-neutral instructions let any agent work in the repository without duplicated or drifting copies.

## Decisions

### Specs follow behavior, not projects

```
sdd/
  README.md              the rulebook
  templates/             a spec, an analyzer spec, a change doc
  specs/
    README.md            index: spec → ID prefix → code owners
    requirement-ids.txt  ledger of every requirement ID ever issued
    analyzer/RH####.md   one spec per rule
    formatter/
      pipeline.md        phase order and cross-phase invariants
      <phase>.md         one spec per pipeline phase
    shared/<behavior>.md behavior specified once and used by more than one spec
    cli/                 CLI specs
    playground/          Playground specs
  changes/
    yyyy-mm/<slug>.md    change docs, by the month the change is merged
  work/
    <slug>/…             temporary working docs; never merged
```

`Reihitsu.Core` gets no spec of its own, and the internal `Reihitsu.Tooling` gets none. Formatter specs follow the pipeline phases, because that matches the code and makes navigation direct; cross-phase behavior goes into the pipeline spec. Behavior that more than one spec relies on — analyzer and formatter, analyzer and code fixes, every code fix, or several rules of one kind — is specified once under `shared/` and referenced by ID. `shared/diagnostics.md` and `shared/code-fixes.md` apply to every rule and every code fix without being listed. A spec specifies behavior, never an implementation: a base class that implements shared behavior is a code owner of the shared spec, but no requirement demands that a rule derive from it. A policy a formatter phase owns and a rule checks may stay in the phase's spec and be referenced from the rule's.

### Specs and user documentation are different documents

`documentation/rules/RH####.md` stays end-user documentation: what is reported, why, how to fix it, one example, no implementation detail, no mention of the formatter. A spec is the technical source of truth and may name types, corner cases, and code-fix boundaries. The rule page is derived from the spec; when both change, the spec changes first.

### Spec format

Specs are written for agent work: grep-able, unambiguous, and small enough to load only what a task needs.

- A header table names the `ID prefix`, `Status`, `Code owners`, and `Uses`. An analyzer spec adds `Category`, `Severity`, `Enabled by default`, and `Code fix`, which must match the rule's descriptor and code-fix provider.
- Code owners are type names only, never paths or line numbers, and a type owns at most one spec. What decides is the behavior a type implements, not who calls it.
- `Uses` lists exactly the requirement IDs of other specs the spec relies on, so a reader sees the dependencies and the checks can enforce them.
- `Non-goals` list only deliberate exclusions — what the spec does not cover by decision, never a case whose intended behavior is undecided or a known defect — so extending behavior into one is a behavior change that needs a change doc.
- One requirement per heading, `### <ID> — <short title>`: one decision with its own reason. The normative sentence comes first (MUST, MUST NOT, SHOULD), then `Why:`, which decides the edge cases the requirement does not spell out, then concrete before/after examples, which seed the tests. Analyzer examples mark reported spans with the verifier markup `{|RH####:…|}`.
- A requirement claims only what is verified. It states the inputs it decides, and claims that no other input behaves otherwise only when the code that dispatches on the input shows the list is complete. Where the set of inputs is open-ended, it names the decided ones and states its reach in a coverage sentence; a form left out that looks wrong is an open question for the maintainer. An exhaustive or negative claim that the code does not bear out is broken again by every review round.
- An analyzer spec states only what is the rule's own — what is reported, what is not, where, what the fix produces, and when it is withheld — and references the shared behavior every rule or fix has.
- A rule already stated by a shared spec is referenced by ID, never restated. The diagnostic message and the code-fix title belong to the resources and the rule page, not to requirements.

### Requirement IDs and the ledger

IDs have the form `<PREFIX>-<NNN>`, for example `RH3202-001`, `FMT-BLANK-007`, `SHARED-ORDER-002`, `CLI-004`. They are immutable and never reused, so specs, change docs, tests, and PR descriptions can refer to a requirement by ID and the reference stays valid. A dropped requirement is retired in place with its reason. `sdd/specs/requirement-ids.txt` lists every ID ever issued, in ordinal order; it is append-only, and a CI step fails a pull request that removes a line, so a requirement cannot disappear together with its ledger entry.

### Tests name requirements, and coverage is enforced for every spec

- A test names each requirement it covers with `[Requirement("<ID>")]`, one attribute per ID. The attribute is also a test category, so `--filter "TestCategory=<ID>"` runs every test of one requirement.
- A test derived from a requirement — from its example, its normative sentence, or a condition it states — carries that requirement's tag from the start, and a bug's reproduction tags the requirement the bug violates. A test that verifies a requirement only incidentally does not tag it.
- Untagged tests are allowed. A test that verifies no requirement — a failure class, a repository convention, a tool — carries no tag and needs no exemption, so the architecture and tooling tests need no special handling.
- Coverage is opt-out. `RequirementCoverageTests` enforces every active requirement of every spec unless an entry in its exemption list names the spec and states why. The goal is an empty list, and it is empty. A requirement of an exempted `shared/` spec stays enforced when an enforced spec lists it under `Uses`. A PR that adds a requirement to an enforced spec adds its covering test in the same PR.
- The shared Fix All requirements are carried once by the code-fix test base; a rule's test class overrides that test only for a Fix All requirement of its own.
- The spec checker checks that the tests a run derives from a requirement carry its tag; the coverage check cannot see an untagged test when an older test already covers the requirement.

### Mechanical checks

The rules that can be checked mechanically are checked by the architecture tests in CI, so they do not depend on a reviewer's attention:

- `SpecConsistencyTests`: every code owner is a type declared in a production project; every requirement ID heads exactly one requirement; ledger and specs agree.
- `SpecStructureTests`: IDs carry their spec's prefix; the index lists every spec with its prefix and owners; `Uses` is exact; the ledger is sorted; a type owns at most one spec; analyzer spec headers match the rule's descriptor and code-fix provider.
- `RequirementCoverageTests`: every tag names an active requirement as one string literal on a test method; every enforced requirement is covered; every exemption states its reason.

### Open questions

A spec describes current behavior. When the real output looks wrong while a spec is written or extended, the intended behavior is an open question that only the maintainer answers, and it is an escalation to the maintainer, never a GitHub issue:

- A question that blocks the run — an issue's own behavior depends on the answer, or two readings give different output for the same input — stops the run at once.
- A question found in passing does not stop the run: the spec author leaves the case out of the spec and lists the question in its handover, and the lead asks all questions of the run in one chat message before the auditor runs, then waits.
- Each question stands on its own: input, real output, why it looks wrong, the requirement IDs it touches, the competing readings, and a recommendation.
- The answer decides: a bug fix or a clarification within the run's issues is done in this run, a clarification adding the current behavior as a requirement; a different intended behavior needs a change doc first; a case outside the run's issues becomes a follow-up issue whose text the maintainer approves; "leave it unspecified" records nothing.
- While a question is undecided, the case never goes into a spec — no requirement, no non-goal, no note — and a question is never recorded only in a PR description, which nobody reads after the merge.
- A rule that stays silent where the formatter rewrites is not an open question about the formatter; the rulebook's section *The formatter may do more than the analyzers (P11)* says what it is instead.

The maintainer decides in the chat, and only the outcome is recorded — a spec change, a change doc, or an approved follow-up issue. A tracker of questions that nobody reads after the decision would only duplicate the escalation path.

### Principles

Where many open questions share one pattern, the maintainer decides the pattern once, as a principle. A principle is written into the shared specs as requirements, or into `sdd/README.md` as a section, and from then on a shape that does not follow it is a difference between the code and a spec rather than a new question. Specs cite a principle by its requirement ID; two of them also carry a label in the rulebook, P11 and P31.

| Principle | Where it is stated |
|---|---|
| A fix that would add a compile error, change run-time behavior, change comment, directive, or literal text the user wrote, leave code an `#if` branch excludes referring to a name it changed, or change which declarations a documentation comment documents is withheld; withholding a fix never removes the report | SHARED-FIX-003; `AGENTS.md`, *Code-fix coverage* |
| A name is compared by its value: no `@`, no whitespace or comments inside a qualified name, an alias resolved to the symbol it names | SHARED-DIAG-002 |
| Only the text of a literal or a comment is content; the code around a literal and inside an interpolated string is checked like any other code | SHARED-DIAG-003 |
| Only ordinary comments are checked, every kind alike and in every documentation mode; a `///` or `/** */` comment that documents nothing is an ordinary comment, and the formatter leaves such a stranded comment where it was written | SHARED-COMMENT-001; FMT-DOC-003 |
| A fix corrects its diagnostic in one application, the fixes of two rules do not undo each other, and an offered fix changes the document | SHARED-FIX-001, SHARED-FIX-004, SHARED-FIX-005 |
| A fix changes only the code its diagnostic is about: it inserts no blank line before the node it changes and formats only that node | SHARED-FIX-006 |
| The code a fix changes is laid out as the formatter lays it out: a changed member gets its own line, and a blank line that a removed directive line leaves at the edge of a body or doubled is removed | SHARED-FIX-007 |
| Code a fix moves or wraps keeps its own layout; only its indentation follows the new nesting | SHARED-FIX-007 |
| The formatter may do more than the analyzers; a gap in the rule's own construct is the rule's, a different construct is a new-rule candidate | `sdd/README.md`, *The formatter may do more than the analyzers (P11)* |
| A layout rule whose fix lays code out as the formatter does not report a shape the formatter deliberately keeps (P31) | SHARED-DIAG-010; `sdd/README.md`, *The formatter may do more than the analyzers (P11)* |
| For a rule that checks where a line starts, a comment that starts the line is the line's first element; a misplaced comment-led line is still reported | SHARED-COMMENT-011 |
| A delimited comment that spans several lines belongs to the line it starts on | SHARED-COMMENT-012 |
| A partial declaration is judged as one declaration, and text that one part writes independently is judged per part | SHARED-DIAG-004 |
| Parentheses, `!`, and a `throw` branch do not change a value | SHARED-DIAG-005 |
| A region belongs to the innermost type-like declaration whose braces enclose it, and a member is judged by its innermost region where the rule reads the region's description | SHARED-TYPEREGION-002, SHARED-TYPEREGION-006 |
| Member-ordering rules check each top-level region on its own | SHARED-ORDER-004 |
| An element of an ordered sequence is compared with every earlier element of its group, whatever lies between | SHARED-USING-007 |
| An end tag with another name is not an end tag of its own | SHARED-DOC-017 |
| Every element of the rule's kind is checked, with one diagnostic per reported declarator | SHARED-DIAG-006 |
| A diagnostic has a source location; a declaration without a name, such as an extension block, is reported at a token that marks it | SHARED-DIAG-007 |
| A conditional directive or excluded code in the judged span means the construct is not reported; a region or pragma directive there keeps the report and withholds a fix that would rewrite it | SHARED-DIAG-008 |
| A verdict does not depend on base-list order, on which partial part holds a declaration, or on a member of another kind | SHARED-DIAG-009 |
| A rule does not report where the change it asks for cannot be written at that position | SHARED-DIAG-011 |
| A `///` comment behind code documents the next declaration and is judged as if it started its line; a fix that cannot carry it with the declaration it moves is withheld; in documentation mode `None` it is an ordinary comment | SHARED-DOC-004 |
| A rule whose subject is a documentation comment reports nothing when documentation is not parsed; the other documentation rules check every mode | SHARED-DOC-008 |
| A fix writes the line ending the formatter detects, a lone `\r` counting as `\n`, on every line break in the span it changes | SHARED-FIX-008; FMT-PIPE-007 |
| Wrapping code in a block keeps the comments' layout | FMT-PIPE-013 |
| One treatment per syntax family, within the per-construct layouts the formatter specs name | FMT-PIPE-014 |
| The formatter does not apply a structural rewrite it cannot complete safely | FMT-STRUCT-024 |
| A blank line the formatter inserts goes after directives, never directly in front of one | FMT-BLANK-016 |

### Change docs

A change doc is required only when intended behavior changes: a requirement is added with new behavior, changed, or retired, or a rule, phase, or surface is added or removed. A PR that touches no spec, a bug fix, and a clarification need none. A change doc is written before implementation, records what changes and why, the decisions taken, and the alternatives rejected, and is checked in with the PR that implements it, by the month it is merged. It is `Accepted` once the maintainer approved it, before the PR is ready for review. It carries no task list and no implementation plan, which are noise once the change is merged. Change docs are history: a later change gets its own doc.

### Temporary working docs

Plans, task lists, handovers, and working notes of a run live under `sdd/work/<slug>/` on the PR branch. They are committed, because sessions run in disposable containers and a later step may run in a different session. They are deleted before the PR is marked ready; a PR that still contains `sdd/work/` is not mergeable.

### Agent-neutral instructions

`AGENTS.md` holds the repository instructions for every agent and contributor. `CLAUDE.md` imports it and adds only what is specific to Claude Code. The squad's process and roles live under `squad/` and are agent-neutral; `.claude/agents/<role>.md` is a thin wrapper per role that sets only model, effort, and tools and points at the role file, and `/squad` makes the session agent the lead. Agent-specific files of other agents and their duplicated instructions are not kept. The repository scripts under `scripts/` are the canonical entry points, in Bash only. `create-rule-doc` and `add-resource-texts` stay as reference commands that the roles cite.

### The squad: roles

The maintainer talks only to the lead. The lead claims the lane, spawns one fresh agent per role, routes findings, escalates, publishes the PR, and watches its CI; it never writes specs, tests, or production code, and never performs a role itself — when roles cannot be spawned, it stops and reports.

| Role | Job | Must not |
|---|---|---|
| Spec author | Writes the change doc (when required), the spec diff, and the documentation; every text change of the docs lane; lists open questions found in passing | Touch code or tests, beyond comment-only edits in the docs lane |
| Skeptic | Challenges the premise: is the issue real, is the spec wrong instead, does it conflict, which input breaks it, is there a simpler option, is the severity right | Review code |
| Test implementer | Derives tests from the spec examples and the failure-class catalog; for a bug, the first test is the reproduction | Touch production code |
| Implementer | Makes the tests pass | Change test assertions |
| Cleaner | Formats with `reihitsu-format`, makes the Release build warning-free | Change behavior, add suppressions or ruleset changes |
| Spec checker | Verifies specs, change doc, user documentation, and the tags of new tests against the diff | Change anything |
| Auditor | Final independent check of the finished tree against the specs and the failure-class catalog | Change anything |
| Validator | Builds the solution and runs every test project | Diagnose or fix |

Strict separation is the point: a role that has to re-read everything was given a poor handover, and a lead that knows everything has done too much itself. The skeptic works before anything is built, where it is most valuable; reviewing the finished code is the auditor's job.

### Lanes

A run claims one lane, and the diff's paths decide whether the claim holds, not the lead's judgment: `scripts/lane-check.sh` classifies every changed path and names the narrowest lane that admits them.

| Lane | Admitted by | Roles |
|---|---|---|
| Docs / text only | `sdd/`, `documentation/`, Markdown, templates, and C# that `scripts/verify-text-only.sh` proves text-only | spec author, spec checker; validator only if a compiled file changed |
| Specs and tests for existing behavior | test projects, with or without the spec of an existing rule | spec author when a spec is written, skeptic, test implementer, cleaner, spec checker, validator |
| Bug fix | shipped source without a change doc | all but the spec author, who runs only for a clarification |
| Behavior change / new rule | shipped source with a change doc | all |
| Tooling | scripts, CI, build settings, `Reihitsu.Tooling`, agent instructions | all but spec author and skeptic; test implementer when the tool has tests |

Order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. A claim may be wider than the diff — a bug that does not reproduce ends in the bug-fix lane with only a test — but not narrower. Lanes widen along docs → tests → bug fix → behavior change and docs → tests → tooling → behavior change; before the implementer starts, the lead widens and runs the roles the wider lane adds, afterwards a needed widening is an escalation. Every skipped role is recorded with its reason in the PR's run report. A text-only proof is syntax-aware, because a line-based `grep` cannot tell a comment from a directive or from comment-looking text in a literal.

### Handovers are files

Every role writes its result to `sdd/work/<slug>/<nn>-<role>.md`, starting with `Role`, `Commit`, and `Verdict`, followed by the sections its role file prescribes. The next role reads that file and the specs, nothing else from the conversation; the lead keeps only verdicts and summaries. A handover carries facts — inputs, outputs, paths, test names, commands, results — and never an intended fix, a suspected cause, or what the next role should find, so each role can try to break the previous claim instead of confirming it. Each writing role commits its own files and handover with `[skip ci]`; only the lead pushes and touches GitHub. Because handovers are committed, a run can continue in a later session.

### Escalations and the decisions log

The lead stops all role work and asks the maintainer when a decision belongs to the maintainer:

1. the spec author has an open question about intended behavior: a blocking one stops the run at once, the ones found in passing are asked together before the auditor;
2. a change doc is required — the maintainer approves What and Why before tests are written;
3. the skeptic says the change should not be made;
4. a test turns out to be wrong, so the spec would have to change mid-run;
5. scope grows beyond the change doc: new rule, public API, dependency, a shared helper with other consumers, or more than double the intended production paths;
6. a warning can only be resolved by a suppression or a ruleset change;
7. the spec checker or the auditor still blocks after the third repair round, or a repair would need a choice the maintainer has not made — narrowing a requirement or dropping its claim, a design change, more scope;
8. a bug does not reproduce, or the report has no scenario concrete enough to test;
9. a failure also exists on `main`;
10. no lane admits the diff, the lane must widen after the implementer started, or the issues named for one run do not share a lane;
11. the prompt names more than eight issues for one run.

The question is asked in the chat in the format `AGENTS.md` prescribes — context, one question, options with an example each, a recommendation — and the lead waits; a recommendation is not an answer. Every answer is recorded in `squad/decisions/`, one file per answered escalation so parallel PRs do not conflict, written before the run continues and committed with the PR so it survives the deletion of `sdd/work/`. The log exists to tune the squad: when the same kind of question keeps getting the same answer, the maintainer turns it into a rule — in a spec, the failure-class catalog, or the playbook — and the escalation goes away; an entry whose content becomes a rule is deleted in the PR that adds the rule, and that PR names the entries it folds in. When something slipped through that should have reached the maintainer, an escalation is added. The squad never tunes its own escalations.

### Repairs

The lead routes each finding of the spec checker, the auditor, the validator, or CI to the role that owns it: code to the implementer, a missing or weak test to the test implementer, a spec gap to the spec author, formatting and warnings to the cleaner. All findings of one verdict form one worklist and one repair round. A block after a repair round starts another round in the same way when every finding concerns the same requirement or was introduced by the PR, up to three rounds in a run, so a run cannot loop; a block after the third, or a repair that needs a choice the maintainer has not made, is an escalation. When the auditor passes with findings confined to comments or documentation, a fix proven text-only by `verify-text-only.sh --strict-docs` needs no second audit.

### Issue intake

Issues are often generated from a source audit rather than observed by running the tool, so every issue is a hypothesis. A bug is confirmed only by a test that reproduces it; a report's reasoning and severity are checked, not only its example; a framing the repository's policy contradicts — such as a diagnostic without a fix being a defect in itself — is rejected. A bug that does not reproduce keeps its passing test as a characterization test, the PR links the issue with `Refs` instead of `Closes`, and the maintainer decides. Before reporting a non-reproduction, the test implementer runs the example on both line endings, on the counterpart surface, on the code-fix path, and on the nearest sibling shape; no second confirmation is needed, because the maintainer sees every non-reproduction.

### Several issues in one run

A run implements exactly the issues the prompt names — usually one, or several that become one run with one branch, one `sdd/work/<slug>/`, and one pull request. The lead never adds an issue on its own; a follow-up candidate is presented to the maintainer and created only after the maintainer approved its text. In a run of several issues:

- one agent per role serves the whole run, never one per issue;
- every handover has one section per issue, plus a section for results that belong to the whole tree;
- every issue shares the run's one lane, and the run widens as a whole; issues that need different lanes are split into separate runs before the claim;
- one change doc may cover issues that share a cause;
- an escalation for one issue stops the whole run until it is answered;
- scope is measured against the whole batch;
- the PR carries one `Closes` or `Refs` line per issue.

### Claiming, publishing, and CI

The lead claims the issues with an empty commit and a draft PR, so parallel work sees the claim. Every commit of the run carries `[skip ci]` until a single trigger commit at the end, so CI runs once on the finished tree. The PR is published ready for review, with the run report in its description, once `sdd/work/` is deleted and the validator is green: a PR the squad publishes is finished. When `main` moves after a passing audit and overlaps the PR only in files to which both sides add independent entries — the requirement-ID ledger, or a test file to which both sides added tests — the lead merges, the role that owns the file keeps both sides, the cleaner runs, and CI stands in for a second audit and a local validator run; the run report names the checks that did not run locally. When the new commits change code, spec text, or a test the PR also changes, the maintainer decides between a re-audit and relying on CI. The lead then watches CI until it is green; a failure the PR caused is a repair finding.

### The failure-class catalog

`squad/failure-classes.md` lists the known ways a change in this repository goes wrong — guards narrower than their decision, lost trivia and directives, meaning-changing rewrites, non-converging fixes, formatter instability, analyzer/formatter disagreement, and weak evidence — one line per class with its origin. The skeptic names the classes that can reach a change, the test implementer covers them, and the auditor checks every one. IDs are immutable. Knowledge that holds for more than one spec moves into `shared/` specs; the catalog keeps what is about how changes fail rather than about intended behavior.

### Keeping it lean

Every rule of the earlier agent workflow was sorted into knowledge about the code (moved into specs and the failure-class catalog), process mechanics (moved into the lead playbook and role files), or obsolete (removed). Source carries no `TODO` and no reference to this repository's tracker numbers: unfinished work is an approved follow-up issue linked from the PR. Role files stay short; content beyond a role's job belongs in specs or the catalog.

## Alternatives considered

- **Full OpenSpec with checked-in proposals, designs, and task lists** — rejected: task lists are noise after merge, and the tool adds a dependency.
- **A change doc for every PR** — rejected: most PRs fix code against an unchanged intent.
- **Using `documentation/rules/` as the spec** — rejected: user documentation must stay free of technical detail.
- **Formatter specs by user-visible topic instead of by phase** — rejected: specs per phase match the code; cross-phase behavior goes into the pipeline spec.
- **Working docs kept local or only in the PR description** — rejected: later sessions cannot rely on local files, and a PR description is not versioned with the code.
- **Open questions recorded in PR descriptions or written into specs as non-goals** — rejected: nobody reads a PR description after the merge, and a non-goal would turn a suspected defect into specified behavior.
- **One GitHub issue per spec that collects its open questions** — rejected: such an issue duplicates the escalation path, collects questions nobody reads once they are decided, and splits one decision across an issue and a run; the maintainer decides in the chat, and only the outcome — a spec change, a change doc, or an approved follow-up issue — is recorded.
- **Opt-in coverage, enforcing only listed spec paths** — rejected: a new spec would stay unenforced until someone remembered to add it; an exemption with a reason makes leaving a spec out visible.
- **Requiring every test to carry a tag or an exemption** — rejected: many tests verify a requirement only incidentally or none at all; tagging them with the nearest requirement would claim coverage they do not give.
- **Tagging architecture and tooling tests against a repository-conventions spec** — rejected: conventions and internal tools have no spec, and untagged tests are allowed.
- **Letting the auditor check the tags of new tests** — rejected: the auditor does not run in the tests lane, and its test check is about assertion strength.
- **The lead implements directly, or performs a role when subagents are unavailable** — rejected: it breaks the role separation the squad exists for.
- **A separate reproducer role** — rejected: the test implementer's first test for a bug is the reproduction.
- **The skeptic also reviews the finished code** — rejected: that is the auditor's job.
- **The lead judging the lane** — rejected: the diff's paths decide, so a claim can be checked mechanically.
- **Automatic tuning of escalations** — rejected: the squad would decide on its own when it needs the maintainer.
- **Separate review rounds after publishing, or publishing the PR as a draft** — rejected: a PR the squad publishes is finished; the squad watches its CI instead.
- **A higher-tier confirmation of every non-reproduction** — rejected: the maintainer sees every non-reproduction anyway.
- **One run per issue only** — rejected: issues of one lane that the maintainer names together are cheaper as one run with one PR, as long as each keeps its own outcome and link.
- **Dropping the earlier workflow rules wholesale** — rejected: they encode real failures; they were sorted and condensed instead.
