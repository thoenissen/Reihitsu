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
