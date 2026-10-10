# Introduce Spec Driven Development

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected specs | none yet — this change creates `sdd/` |

## What

Reihitsu adopts a lightweight form of Spec Driven Development (SDD). The intended behavior of every shipped surface — analyzer rules, formatter, CLI, Playground, and the policies they share — is written down in specs under `sdd/specs/`. Decisions that change intended behavior are recorded as change docs under `sdd/changes/`.

The approach borrows from OpenSpec (specs plus change proposals), Spec Kit, and Squad, but keeps only what pays off in this repository: no checked-in plans or task lists, no tooling dependency.

## Why

- **Navigation.** Today the intended formatter behavior has to be reverse-engineered from complex code. A spec says what a phase does and names the types that implement it, so an agent or a person finds the owner without searching.
- **A reference independent of the code.** A bug is a difference between the code and the spec. Without a spec, every "is this a bug?" question is reopened from scratch, and the Behavior Contract of each run is thrown away after the PR.
- **Traceable decisions.** Why a behavior is the way it is gets lost in PR threads. Change docs keep the decision next to the spec it changed.

## Decisions

### Repository layout

```
sdd/
  specs/
    README.md            index: spec → ID prefix → code owners
    analyzer/RH####.md   one spec per rule
    formatter/
      pipeline.md        phase order and cross-phase invariants
      <phase>.md         one spec per pipeline phase
    shared/<policy>.md   policies analyzer and formatter must agree on
    cli/…
    playground/…
  changes/
    yyyy-mm/<slug>.md    checked-in change docs
  work/
    <slug>/…             temporary implementation docs, never merged
```

`Reihitsu.Tooling` (the fixture runner) is internal and gets no spec. `Reihitsu.Core` gets no per-project spec: specs follow behavior, not project boundaries. The policies Core hosts that analyzer and formatter must share (for example blank-line spacing, ordering move safety, using-directive ordering safety) are specified once under `shared/` and referenced by ID from analyzer and formatter specs instead of being restated.

### Specs and user documentation are different documents

`documentation/rules/RH####.md` stays end-user documentation: what is reported, why, how to fix it, one example, no implementation detail. Specs are the technical source of truth and may name types, corner cases, and code-fix boundaries. User documentation is derived from the spec; when both change, the spec changes first.

### Spec format

Specs are optimized for agent work: grep-able, unambiguous, small enough to load only what a task needs.

Every spec file starts with a short header:

```markdown
# Formatter — Blank lines

| | |
|---|---|
| ID prefix | FMT-BLANK |
| Status | Active |
| Code owners | `BlankLinePhase`, `BlankLineEditor`, `TokenGapAnalysis` |
| Uses | SHARED-BLANK-003 |

## Non-goals
- …
```

Every requirement is one heading carrying its ID:

~~~~markdown
### FMT-BLANK-007 — Blank line before a comment block

The formatter MUST keep exactly one blank line before …

Why: …

Example:
```csharp
// before
```
```csharp
// after
```
~~~~

Rules:

- The normative sentence comes first and uses MUST / MUST NOT / SHOULD.
- `Why` is short but present — it is what lets an agent decide an edge case the requirement does not spell out.
- Examples are concrete before/after C# blocks; they are the seed for tests.
- `Non-goals` list what the spec deliberately does not cover, so an agent does not extend behavior on its own.
- Code owners are type names only, never file paths or line numbers.

### Requirement IDs

- Format: `<PREFIX>-<NNN>`, for example `RH3202-001`, `FMT-BLANK-007`, `SHARED-ORDER-002`, `CLI-004`.
- IDs are immutable and never reused. A dropped requirement stays in its file with status `Retired`, so every link keeps resolving.
- Specs, change docs, tests, and PR descriptions link requirements by ID.

### Change docs

A change doc records **what** changes in intended behavior and **why**, plus the decisions taken and the alternatives rejected. It is written before implementation and checked in with the PR.

A change doc is required only when intended behavior changes. Not required:

- a PR that touches no spec;
- a bug fix — code is brought back to the spec;
- a clarification — the spec gains a requirement that writes down behavior that already exists (for example a gap a bug fix exposed).

When in doubt, the spec-check role decides and escalates to the maintainer.

Content: header (status, date, affected requirement IDs), What, Why, Decisions, Alternatives considered. No task lists, no implementation plan — those are noise once the change is merged.

File: `sdd/changes/yyyy-mm/<slug>.md`, by the month the change is merged.

### Temporary implementation docs

Plans, task lists, and Behavior Contract working notes live under `sdd/work/<slug>/` on the PR branch. Sessions run in disposable containers and later steps may run in a different session, so these docs are committed to the branch rather than kept local. They are deleted before the PR is published as ready; a PR that still contains `sdd/work/` is not mergeable.

### Keeping specs true

- **Now:** a spec-check role in the squad verifies that every PR updates the affected specs, that a change doc exists exactly when one is required, that user documentation still matches, and that no `sdd/work/` remains.
- **Script:** a check that every code owner named in a spec still exists as a type.
- **Later:** tests reference requirement IDs, and an architecture test reports requirements without a test.

### Initial coverage

- Formatter: the pipeline spec plus one spec per phase — complete from the start, because the formatter is the main reason for SDD and has a fixed size.
- Shared policies used by analyzer and formatter.
- CLI and Playground.
- Analyzer: one example rule per category (RH0 through RH8), preferably rules with a code fix, so every category has a template. All other rules get a spec when they are next touched, or in batches when time allows.

## Rollout

Step by step, each step its own PR, tracked as one parent issue with sub-issues:

1. Move the source projects under `src/` — mechanical only, first, so specs name current locations from the start.
2. SDD skeleton: layout, templates, ID rules, index, this change doc.
3. Formatter specs: pipeline plus all phases.
4. Shared, CLI, and Playground specs, plus the nine analyzer examples.
5. Rewrite the agent documentation: remove Codex, make `AGENTS.md` agent-neutral, and let the Claude files link to it.
6. Establish the squad: lead, roles, escalations, spec-check role.
7. Script that checks the code owners named in specs.
8. Test enforcement of requirement IDs.

Steps 5 and 6 are specified in a separate change doc once the squad concept is settled.

## Alternatives considered

- **Full OpenSpec** with checked-in proposals, designs, and task lists — rejected: task lists are noise after merge, and the tool adds a dependency.
- **A change doc for every PR** — rejected: most PRs fix code against an unchanged intent.
- **Using `documentation/rules/` as the spec** — rejected: user documentation must stay free of technical detail.
- **Formatter specs by user-visible topic instead of by phase** — rejected for now: specs per phase match the code and make navigation direct; cross-phase behavior goes into the pipeline spec.
- **Working docs kept local or only in the PR description** — rejected: later sessions cannot rely on local files, and a PR description is not versioned with the code.
