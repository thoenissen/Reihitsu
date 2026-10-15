# Spec Driven Development

This file is the current rulebook for Spec Driven Development (SDD) in Reihitsu. It applies to agents and humans alike. Change docs under `changes/` explain why a rule exists; this file states the rule. When a change doc changes a rule, update this file in the same PR.

Specs under `specs/` describe the intended behavior of every shipped surface — analyzer rules, formatter, CLI, Playground, and the policies they share. A bug is a difference between the code and a spec.

## Layout

```
sdd/
  README.md              this rulebook
  templates/             templates for a spec and a change doc
  specs/
    README.md            index: spec → ID prefix → code owners
    analyzer/RH####.md   one spec per rule
    formatter/
      pipeline.md        phase order and cross-phase invariants
      <phase>.md         one spec per pipeline phase
    shared/<policy>.md   policies analyzer and formatter must agree on
    cli/                 CLI specs
    playground/          Playground specs
  changes/
    yyyy-mm/<slug>.md    change docs, by the month the change is merged
  work/
    <slug>/…             temporary working docs; never merged
```

- Specs follow behavior, not projects. `Reihitsu.Core` gets no spec of its own; a policy it hosts that analyzer and formatter share is specified once under `shared/` and referenced by ID.
- `Reihitsu.Tooling` is internal and gets no spec.
- Every spec file is listed in `specs/README.md`. Add the row when you add the spec.
- An analyzer rule without a spec gets one the next time its behavior is touched.

## Specs and user documentation

- `documentation/rules/RH####.md` is end-user documentation: what is reported, why, how to fix it, one example. It carries no implementation detail.
- A spec is the technical source of truth. It may name types, corner cases, and code-fix boundaries.
- User documentation is derived from the spec. When both change, change the spec first; a PR that changes one keeps the other consistent.

## Spec format

Start from [`templates/spec.md`](templates/spec.md).

- **Header table:** `ID prefix`, `Status` (`Active`), `Code owners`, and `Uses` (requirement IDs from other specs this one relies on; `—` when none).
- **Code owners** are type names only — never file paths or line numbers. Every named type must exist.
- **Non-goals** list what the spec deliberately does not cover. Do not extend behavior into a non-goal without a change doc.
- **One requirement per heading:** `### <ID> — <short title>`.
- The **normative sentence comes first** and uses MUST, MUST NOT, or SHOULD.
- **`Why:`** follows — short, but always present. It is what decides an edge case the requirement does not spell out.
- **`Example:`** gives concrete before/after C# blocks where behavior is visible. Examples are the seed for tests.
- In an analyzer spec, the first example block marks each reported span as `{|RH####:…|}`, as the verifier markup does; a second block, when present, is the code after the code fix. A single block followed by "is not reported" or "no code fix is offered" states a negative case.
- A rule that a shared policy already states is referenced by its ID, never restated.

## Requirement IDs

- Format: `<PREFIX>-<NNN>`, for example `RH3202-001`, `FMT-BLANK-007`, `SHARED-ORDER-002`, `CLI-004`. Analyzer specs use the rule ID as prefix.
- IDs are immutable and never reused. New requirements take the next free number in their spec.
- A dropped requirement is retired, not deleted: it keeps its heading and ID, gets a `Status: Retired` line under the heading with the reason, and stays in its file so every link keeps resolving.
- Specs, change docs, tests, and PR descriptions refer to requirements by ID.

## When a change doc is required

A change doc is required **only when intended behavior changes**: a requirement is added with new behavior, changed, or retired; or a rule, phase, or surface is added or removed.

Not required:

- a PR that touches no spec;
- a bug fix — the code is brought back to the spec;
- a clarification — the spec gains a requirement that writes down behavior that already exists, for example a gap a bug fix exposed. Add it in the same PR.

When in doubt, ask the maintainer.

## Change docs

Start from [`templates/change.md`](templates/change.md).

- Written **before** implementation and checked in with the PR that implements it.
- Records **what** changes in intended behavior and **why**, the decisions taken, and the alternatives rejected.
- Header: `Status`, `Date`, `Affected requirements` (IDs, or the specs a new surface creates), and `Depends on` when it builds on another change doc.
- Status is `Draft` while under discussion and `Accepted` once the maintainer has approved it. Set `Accepted` before the PR is merged.
- File: `changes/yyyy-mm/<slug>.md`, by the month the change is merged.
- No task lists and no implementation plan — they are noise once the change is merged.
- Change docs are history. Once merged, they are not edited to track later changes; a later change gets its own doc.

## Temporary working docs

- Plans, task lists, handovers, and working notes for a run live under `work/<slug>/` on the PR branch.
- Commit them to the branch: sessions are disposable, and a later step may run in a different session.
- Delete `work/<slug>/` before the PR is marked ready. A PR that still contains `sdd/work/` is not mergeable.

## Keeping specs true

Every PR, in the same PR:

- updates each spec whose behavior or code owners it changes, and keeps `specs/README.md` in sync;
- adds a change doc exactly when one is required;
- keeps the user documentation consistent with the spec;
- leaves no `sdd/work/` behind.
