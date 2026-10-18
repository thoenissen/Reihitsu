Role: Lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed

# Run: thoenissen/Reihitsu#983 — specs for RH8020–RH8034

- Issue: thoenissen/Reihitsu#983 (SDD analyzer specs, element documentation part 2; parent thoenissen/Reihitsu#899).
- Slug: `element-docs-part-2`.
- Branch: `claude/great-franklin-s92k9s` (the session's designated branch, used instead of `claude/issue-983-…` because the session may push only there). Draft PR: thoenissen/Reihitsu#1101.
- Lane: specs and tests for existing behavior — the run writes specs for 15 existing rules and tags or adds tests of their current behavior; no production change.

## Roles

Run: spec author → skeptic → test implementer → cleaner → spec checker → validator.
Skipped: implementer (no production change in this lane), auditor (the lane does not run it).

## Rules in scope

RH8020–RH8029 (non-private/private methods, properties, indexers, fields, events must be documented), RH8030 (element documentation must have summary text), RH8031 (extension declarations must have summary text), RH8032 (extension declaration parameters must be documented), RH8033 (extension declaration type parameters must be documented), RH8034 (operators must be documented).

## Maintainer clarifications (summarized)

- Specs describe current behavior. Reuse `sdd/specs/shared/element-documentation.md`; extend it only for behavior more than one spec relies on. thoenissen/Reihitsu#984 and #985 extend the same shared spec later and are not running now.
- Principle P8 is decided (thoenissen/Reihitsu#912, implemented later by thoenissen/Reihitsu#1091): a declaration counts as documented exactly when the documentation the compiler produces for it has a top-level `<summary>` or `<inheritdoc>`. Where current behavior differs, the case is left out of the spec and filed in the spec's open-questions issue marked `Decided by principle P8 (#912)`.
- The private/non-private group cases of partial parts and static constructors are covered by thoenissen/Reihitsu#1092 and are left out of the spec; a further instance of that group goes to the lead, who adds a one-line comment on #1092 (not a new question).
- Principles P1, P2, P3, P5, P6, P7 (thoenissen/Reihitsu#912) get the same marking `Decided by principle Pn (#912)` with the violated requirement; P4 stays a normal open question.
- Open questions go to the `SDD open questions: <spec path>` issue (a sub-issue of #912); shared-behavior questions to the shared spec's issue. No other issue is created without the maintainer's approval.
- Must not: change production behavior, retire an existing requirement without asking the maintainer, touch `sdd/work/coordinator/`.
- Concurrent runs: thoenissen/Reihitsu#1068 (`shared/code-fixes.md`), #1069 (`shared/diagnostics.md`), #981, #987, #990.
- Done: every rule has its spec, every requirement a covering test, open questions filed, CI green, PR says `Closes #983`.

## Open-question issues

No open `SDD open questions` issue for `shared/element-documentation.md` exists at the start; the previous one (thoenissen/Reihitsu#1037) is decided and closed.

## Escalation and lane widening

- The spec author edited the text of SHARED-DOC-001, SHARED-DOC-002, and SHARED-DOC-005 (coverage of partial members; extension block as a private container). The maintainer kept all edits: `squad/decisions/2026-10-18-shared-doc-coverage-and-extension-block.md`.
- That decision record lives under `squad/`, a tooling path, so the lane widened from tests to tooling before the implementer stage (as in the run of thoenissen/Reihitsu#982). The auditor now runs. The implementer stays skipped unless a test fails against the unchanged analyzers, because the run changes no production code.
- The SHARED-DOC-002 extension-block edit answers thoenissen/Reihitsu#1055 once it has a covering test.
