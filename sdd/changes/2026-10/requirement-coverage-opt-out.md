# Enforce requirement coverage for every spec

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | none — process change |
| Depends on | [Introduce Spec Driven Development](introduce-sdd.md) |

## What

Requirement coverage becomes opt-out. `RequirementCoverageTests` enforces every spec by default: every active requirement of every spec is covered by at least one test tagged `[Requirement("<ID>")]`. A spec is left out of the enforcement only by an entry on an exemption list that states its reason, and the goal is an empty list. Until now the check was opt-in: it enforced only the spec paths it listed and reported every other spec without failing.

Tests stay as they are: a test without a `[Requirement]` tag remains allowed. What changes for tests is the process that writes them. Every test a run derives from a requirement carries that requirement's tag, and the spec checker checks that the tests a run adds for a requirement carry its tag.

## Why

- **A spec cannot fall out of the check by being forgotten.** An opt-in list enforces a new spec area only when someone remembers to add it. Now that every active requirement has a tagged test, the list no longer protects work in progress; it only lets new specs and new `shared/` specs go unenforced by default. Opt-out turns that default around: leaving a spec unenforced is a visible entry with a reason.
- **New tests are tagged from the start.** The rulebook already asks every PR to tag the tests it adds, but no role checks it. A test derived from a requirement and left untagged does not fail the coverage check when an older test already covers that requirement, so the gap goes unnoticed. Naming the duty in the test implementer's role and the check in the spec checker's closes it where the tests are written.

## Decisions

### Every spec is enforced unless an exemption names it

The check enforces every spec under `sdd/specs/`. A spec is exempted only by an entry in `RequirementCoverageTests` that names the spec and states why it is exempted; the list is meant to be empty and is empty when this change is merged. The check fails when an entry states no reason, so an exemption without one cannot be merged. For an exempted spec, the check writes the uncovered requirements to the test output without failing, as it did for every spec outside the old list.

A requirement of an exempted `shared/` spec stays enforced when a spec that is not exempted lists it under `Uses`, so exempting a shared spec never releases a requirement that an enforced spec relies on.

### Untagged tests stay allowed

Nothing requires a test to carry a `[Requirement]` tag. A test that verifies a requirement only incidentally does not tag it, and a test that verifies no requirement — a failure class, a repository convention, a tool's behavior — carries none. The architecture tests and the tooling tests therefore need no special handling: they are neither tagged against a conventions spec nor exempted as a class.

### Tests derived from a requirement carry its tag

Every test derived from a requirement — from its example, its normative sentence, or a condition it states — carries that requirement's tag, one attribute per ID. A bug's reproduction is derived from the requirement the bug violates and tags it. The test-implementer role states this duty, and the spec checker checks, for every test a run adds, that the tests listed against a requirement carry its tag. The spec checker owns the check because it already compares what the run wrote down — the handovers and the specs — with the diff, and because it runs in every lane, including the tests lane, where the auditor does not run.

## Alternatives considered

- **Require every test method to carry a tag or a visible exemption** — rejected: about 3,050 tests of the shipped-surface test projects carry no tag, many of them verifying a requirement only incidentally or none at all. An exemption attribute on each would only repeat the test's documentation summary, and tagging them with the nearest requirement would make the tags claim coverage the tests do not give.
- **Tag architecture and tooling tests against a new repository-conventions spec** — rejected: those tests check repository conventions and internal tools, which `sdd/README.md` keeps out of the specs (`Reihitsu.Tooling` gets no spec). With untagged tests allowed, they need no spec.
- **Exempt architecture and tooling tests as a class** — rejected: with untagged tests allowed, there is nothing to exempt them from.
- **Keep the opt-in list and add the remaining spec paths to it** — rejected: it enforces today's specs but leaves the next new spec unenforced until someone adds it.
- **Let the auditor check the tags of new tests** — rejected: the auditor does not run in the tests lane, where most tagged tests are written, and its test check is about assertion strength, not about what the handovers name.
