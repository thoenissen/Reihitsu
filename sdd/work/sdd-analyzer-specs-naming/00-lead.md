Role: Lead
Commit: 4efe6aea3f2db03ee4b91ed03edf56c958a8afe3
Verdict: lane claimed — tests

# Run: thoenissen/Reihitsu#978

- Issue: thoenissen/Reihitsu#978, "SDD analyzer specs: RH4001–RH4110 (naming, part 1)", part of thoenissen/Reihitsu#899.
- Slug: `sdd-analyzer-specs-naming`
- Branch: `claude/sleepy-thompson-gikv37` (the session's designated branch, used instead of `claude/issue-978-<slug>`; based on `origin/main` at 4efe6ae).
- Claim PR: thoenissen/Reihitsu#1007 (draft).
- Lane: **Specs and tests for existing behavior** (`tests`). Reason: the issue asks for specs of existing rules together with `[Requirement]` tags on their tests and focused tests where a requirement has no covering test; no production code changes.
- Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
- Roles skipped: implementer (no production change in this lane), auditor (not part of the tests lane).
- Open-question issues for the touched specs: none open (thoenissen/Reihitsu#928 for RH4106 is closed).

## Rules in scope

RH4001–RH4011, RH4101–RH4105, RH4107–RH4110 (20 rules), plus the shared casing spec under `sdd/specs/shared/`. RH4001 and RH4009 are not casing rules and get standalone specs.

## Maintainer's clarifications (verbatim)

> - Issue #978 is one batch of the planned analyzer-spec backfill under #899. Read #978, then AGENTS.md, sdd/README.md, sdd/templates/analyzer-spec.md, squad/README.md, squad/lead.md.
> - Read sdd/specs/analyzer/RH4106.md (the only specified casing rule today) and, as a model for a shared spec with member rules, sdd/specs/shared/statement-blank-lines.md together with sdd/specs/analyzer/RH5001.md.
> - Lane: "specs and tests for existing behavior". No production code changes.
> - This batch creates the shared casing spec under sdd/specs/shared/. Move `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `CasingUtilities` from RH4106's code owners to the shared spec, and let RH4106 reference the shared IDs under `Uses`. If an existing RH4106 requirement states shared behavior, moving it would retire its ID: stop and ask me before doing that.
> - RH4001 and RH4009 are not casing rules and get standalone specs.
> - Done means: one spec per listed rule (20 rules), the shared casing spec, the existing tests tagged with [Requirement], a focused test added only where a requirement has no covering test, every example verified against a test or by running it, open questions filed per sdd/README.md, CI green, PR based on main with no sdd/work/.
> - Must not: write current-but-defective behavior into a spec; re-file #971 (leading uppercase run such as `_hTTPClient`) — leave that case out, #971 adds its requirement itself; change any existing requirement of another spec without asking me.
> - You may file open questions as the rulebook says (sub-issues of #912) without asking. Do not create any other issue without my approval. Opening the claim PR and publishing it per squad/lead.md is approved.

## Escalation 1 — answered

The spec author's handover `01-spec-author.md` blocked on whether RH4106's shared requirements are retired. The answer is recorded in `squad/decisions/2026-10-17-rh4106-shared-casing-move.md`. In short: retire no RH4106 ID. Rewrite RH4106 freely so that the shared casing spec and RH4106 together are defined optimally, state the shared behavior once in the shared spec, and update the test tags to match. Remove the acronym (`HTTPClient`) non-goal. `CasingUtilities` becomes a code owner of the shared casing spec.

Maintainer's answer (verbatim):

> Wir sind grade noch dabei die Specs zu definieren. Daher können auch Specs umgeschrieben oder geändert werden. Wir sollten nichts retiren, wenn nichtmal alles da ist. Einfach ändern. Wenn der Unit-Tests fehlschlägt ist das egal. Wir müssen es so machen, dass die specs optimal definiert sind

## Lane widened: tests → tooling

`scripts/lane-check.sh tests --head worktree` reports `mismatch — claimed tests, diff needs tooling`. The only path that causes it is the lead's decisions entry `squad/decisions/2026-10-17-rh4106-shared-casing-move.md`, which `squad/README.md` requires to ship with the PR. The implementer had not started, so the lane widens along docs → tests → tooling, as `squad/README.md` allows. The roles it adds are the implementer (run at its stage; this run has no production change to make) and the auditor. Follow-up candidate: the lane check classifies `squad/decisions/` entries as tooling.

## Open questions filed (sub-issues of thoenissen/Reihitsu#912, from `06-spec-author.md`)

- thoenissen/Reihitsu#1009 — `sdd/specs/shared/casing.md` (Q1–Q4)
- thoenissen/Reihitsu#1010 — `sdd/specs/analyzer/RH4005.md`
- thoenissen/Reihitsu#1011 — `sdd/specs/analyzer/RH4001.md` (Q1–Q3)
- thoenissen/Reihitsu#1012 — `sdd/specs/analyzer/RH4009.md` (Q1–Q2)
- thoenissen/Reihitsu#1013 — `sdd/specs/analyzer/RH4103.md` (Q1–Q2)
- thoenissen/Reihitsu#1014 — `sdd/specs/analyzer/RH4102.md`

Follow-up candidate awaiting the maintainer's approval: RH4102's Fix All corrects only the event kind it was invoked from (it breaks SHARED-FIX-002).

Maintainer approved filing bug issues ("bug issues kannst du anlegen"). Filed: thoenissen/Reihitsu#1015 (RH4102 Fix All corrects only the event kind it was invoked from). The namespace-type fix (#1009 Q1) and the `Leaf` fixes (#1013 Q1, #1014 Q1) stay open questions; no earlier spec covered them, so they are not bugs against a spec.

## Implementer skipped

`07-test-implementer.md` reports every test green on the unchanged production code, and the issue rules out production changes. The tooling lane adds the implementer only because of the decisions entry under `squad/`, which needs no implementation. With no failing test to make pass, the implementer is skipped.
