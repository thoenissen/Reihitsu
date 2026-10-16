# Spec Driven Development

This file is the current rulebook for Spec Driven Development (SDD) in Reihitsu. It applies to agents and humans alike. Change docs under `changes/` explain why a rule exists; this file states the rule. When a change doc changes a rule, update this file in the same PR.

Specs under `specs/` describe the intended behavior of every shipped surface — analyzer rules, formatter, CLI, Playground, and the policies they share. A bug is a difference between the code and a spec.

## Layout

```
sdd/
  README.md              this rulebook
  templates/             templates for a spec, an analyzer spec, and a change doc
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

- Specs follow behavior, not projects. `Reihitsu.Core` gets no spec of its own.
- Behavior that more than one spec relies on is specified once under `shared/` and referenced by ID, whichever surfaces share it: analyzer and formatter, analyzer and code fixes, every code fix, or several rules of one kind. A shared spec states the behavior in terms every user of it shares ("the statement", "the rule's naming style"); each spec that uses it names the requirement IDs under `Uses` and states only what is its own.
- A spec specifies behavior, never an implementation. A base class or helper that implements shared behavior is a code owner of the shared spec; no requirement demands that a rule derive from it, and a rule may stop using it without a spec change.
- A policy that a formatter phase owns and an analyzer rule checks may stay in that phase's spec; the analyzer spec references its `FMT-…` ID instead of restating it.
- `Reihitsu.Tooling` is internal and gets no spec.
- Every spec file is listed in `specs/README.md`. Add the row when you add the spec.
- Analyzer rules without a spec get one in planned batches, grouped by the behavior they share, in the squad's *specs and tests for existing behavior* lane. A bug fix on a rule without a spec does not have to write it.

## Specs and user documentation

- `documentation/rules/RH####.md` is end-user documentation: what is reported, why, how to fix it, one example. It carries no implementation detail.
- A spec is the technical source of truth. It may name types, corner cases, and code-fix boundaries.
- User documentation is derived from the spec. When both change, change the spec first; a PR that changes one keeps the other consistent.

## Spec format

Start from [`templates/spec.md`](templates/spec.md), or from [`templates/analyzer-spec.md`](templates/analyzer-spec.md) for an analyzer rule.

- **Header table:** `ID prefix`, `Status` (`Active`), `Code owners`, and `Uses` — exactly the requirement IDs of other specs that its requirements reference, `—` when none. A pointer in a non-goal is not a use.
- **Code owners** are type names only — never file paths or line numbers. Every named type must exist. A type is a code owner of at most one spec: a rule's analyzer and code-fix provider belong to the rule's spec, a type that implements shared behavior to that shared spec, and a general-purpose utility to none.
- **Non-goals** list what the spec deliberately does not cover. Do not extend behavior into a non-goal without a change doc. An open question is never written into a spec, neither as a requirement nor as a non-goal (see [Open questions](#open-questions)).
- **One requirement per heading:** `### <ID> — <short title>`. A requirement is one decision with its own `Why:`. Two decisions with different reasons are two requirements; the qualifiers of one decision stay in it, each with an example on either side.
- The **normative sentence comes first** and uses MUST, MUST NOT, or SHOULD.
- **`Why:`** follows — short, but always present. It is what decides an edge case the requirement does not spell out.
- **`Example:`** gives concrete before/after C# blocks where behavior is visible. Examples are the seed for tests.
- A rule that a shared policy already states is referenced by its ID, never restated.
- **Examples** are minimal and neutral: `class C`, `void M()`, no namespaces or names taken from test projects. When one block mixes diagnostics that get a fix with diagnostics that do not, the second block shows the result.

### Analyzer specs

- The header adds `Category`, `Severity`, `Enabled by default`, and `Code fix` (`Fix All`, `Without Fix All`, or `None`), which state the rule's descriptor and code-fix provider.
- The requirements state only what is specific to the rule, in this order: what is reported, what is not, where the diagnostic is, what the fix produces, and when the fix is withheld. Behavior every rule or every code fix shares — generated and inactive code, comments and directives that block a fix, one-pass convergence, Fix All, the line endings of inserted text — is specified under `shared/` and only referenced.
- A Fix All requirement exists only where the rule's Fix All differs from the shared behavior, for example when it skips colliding renames. No requirement states that no code fix is offered; the header says so.
- The diagnostic message and the code-fix title are not written into requirements; the resources and the rule page own them.
- Examples mark each reported span as `{|RH####:…|}`, as the verifier markup does. A requirement about what is reported has one block; a requirement about the fix has the code before and after it. A single block followed by "is not reported" or "no code fix is offered" states a negative case. An input that is not C#, such as `reihitsu.json`, states the reported problem in prose after the block.

## Requirement IDs

- Format: `<PREFIX>-<NNN>`, for example `RH3202-001`, `FMT-BLANK-007`, `SHARED-ORDER-002`, `CLI-004`. Analyzer specs use the rule ID as prefix.
- IDs are immutable and never reused. New requirements take the next free number in their spec.
- Every ID ever issued is listed in [`specs/requirement-ids.txt`](specs/requirement-ids.txt), one per line, in ordinal sort order. Insert the ID of a new requirement at its sorted position in the same PR; never remove or move a line, retired IDs included.
- A dropped requirement is retired, not deleted: it keeps its heading and ID, gets a `Status: Retired` line under the heading with the reason, and stays in its file so every link keeps resolving.
- Specs, change docs, tests, and PR descriptions refer to requirements by ID.

## Tests and requirements

- A test names each requirement it covers with `[Requirement("<ID>")]` on its test method, one attribute per ID, the ID written as a string literal. A test that verifies a requirement only incidentally does not tag it.
- The attribute is declared in `Reihitsu.Analyzer.Test` and `Reihitsu.Cli.Test`; another test project declares it when its tests are first tagged.
- The attribute is a test category, so `scripts/test.sh --project <project> --filter "TestCategory=<ID>"` runs every test of one requirement in that project.
- A test that covers a Fix All requirement overrides `FixAllCorrectsEveryDiagnostic` from `BatchCodeFixTestsBase` only to carry the tag.
- Every active requirement of an **enforced** spec directory is covered by at least one test. Enforced today: `analyzer/` and `cli/`. Every other spec is reported but not enforced. A PR that adds a requirement to an enforced spec adds its covering test in the same PR.
- A retired requirement is never tagged; the tests that covered it tag the requirement that replaced it, or no requirement.

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
- Header: `Status`, `Date`, `Affected requirements` (IDs, the specs a new surface creates, or `none — process change`), and `Depends on` when it builds on another change doc.
- Status is `Draft` while under discussion and `Accepted` once the maintainer has approved it. Set `Accepted` before the PR is marked ready for review.
- File: `changes/yyyy-mm/<slug>.md`, by the month the change is merged.
- No task lists and no implementation plan — they are noise once the change is merged.
- Change docs are history. Once merged, they are not edited to track later changes; a later change gets its own doc. While SDD itself is being set up, the change docs that introduce SDD and the squad are amended in place instead.

## Open questions

A spec describes current behavior. When the real output looks wrong while a spec is written or extended, leave the case out of the spec — no requirement, no non-goal, no note — and do not change the behavior in the same PR. File it as an open question:

- Each spec with open questions gets one GitHub issue, titled `SDD open questions: <spec path>`. A question about shared behavior belongs to the shared spec's issue, not to the rule where it was found. Add later questions to the existing issue for that spec; open one only when none exists.
- Filing them is part of the run that writes or extends the spec. In the squad workflow, the squad files them.
- Each question stands on its own: input, real output, why it looks wrong, and the requirement IDs it touches.
- The maintainer decides each question: a bug fix brings the code in line with the spec, a clarification adds the current behavior as a requirement, and a different intended behavior needs a change doc first (see [When a change doc is required](#when-a-change-doc-is-required)).
- A question is never recorded only in a PR description, which nobody reads after the merge. The PR may link the issue.

## Temporary working docs

- Plans, task lists, handovers, and working notes for a run live under `work/<slug>/` on the PR branch.
- Commit them to the branch: sessions are disposable, and a later step may run in a different session.
- Delete `work/<slug>/` before the PR is marked ready. A PR that still contains `sdd/work/` is not mergeable.

## Checks

`SpecConsistencyTests` in `Reihitsu.ArchitectureTests` enforces the mechanical part of these rules and runs in CI with the other architecture tests:

- every type named under `Code owners` is declared in a production project under `src/` (a Razor component counts by its file name);
- every requirement ID heads exactly one requirement across all specs;
- every ID in the ledger still heads a requirement, active or retired, and every requirement ID is in the ledger exactly once.

The CI workflow also fails a pull request that removes a line from the ledger, so a requirement cannot be deleted together with its ledger entry.

`RequirementCoverageTests` in the same project reads the `[Requirement]` attributes from the sources of every test project and fails the build when:

- an attribute names an ID that heads no requirement, names a retired requirement, has an argument that is not one string literal, or sits on a method that is not a `[TestMethod]`;
- an active requirement of an enforced spec directory has no covering test.

For the specs that are not enforced yet, it writes the uncovered requirements per spec to the test output without failing.

## Keeping specs true

Every PR, in the same PR:

- updates each spec whose behavior or code owners it changes, and keeps `specs/README.md` in sync;
- tags the tests it adds with the requirements they cover;
- adds a change doc exactly when one is required;
- keeps the user documentation consistent with the spec;
- leaves no `sdd/work/` behind.
