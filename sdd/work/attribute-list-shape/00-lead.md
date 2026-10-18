Role: Lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed — specs and tests for existing behavior

## Issue

thoenissen/Reihitsu#981 — SDD analyzer specs for the attribute list shape rules RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5514, RH5516, RH5518, RH5520, RH5522, RH5524, RH5526, RH5528, RH5529, RH5531 (16 rules). Parent: thoenissen/Reihitsu#899. Draft PR: thoenissen/Reihitsu#1102.

Slug: `attribute-list-shape`. Branch: `claude/sharp-cray-jghigr` (the session's designated branch, used instead of `claude/issue-981-<slug>`; the environment only permits pushing to it).

## Lane

Specs and tests for existing behavior: the issue asks for specs of existing rules plus their covering tests, with no production change.

Roles: spec author → skeptic → test implementer → cleaner → spec checker → validator. Implementer skipped (no production code). Auditor skipped (not part of this lane).

## Maintainer clarifications (summarized)

- Specs describe current behavior only. No production behavior change.
- Extend `sdd/specs/shared/attribute-targets.md` with the list-shape behavior where more than one spec relies on it. Code owners named by the issue: `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase`. Code ownership follows the behavior a type implements (`sdd/README.md`, Spec format).
- Already decided follow-ups on attributes: thoenissen/Reihitsu#1093 (attribute placement follows the attached declaration; the same layout gets the same verdict) and thoenissen/Reihitsu#1094 (RH5513 fix bugs). Their decided behavior is not written into any spec. A further instance of either found here is left out of the spec and reported in the handover as "instance of #1093" / "instance of #1094" — the lead adds a one-line comment to that issue; it is not filed as an open question.
- A case matching principle P1, P2, P3, P5, P6, P7 or P8 (thoenissen/Reihitsu#912, principle decisions comment) is filed in the spec's open-questions issue marked `Decided by principle Pn (#912)` with the requirement it violates, and left out of the spec. P4 (analyzer/formatter disagreement) stays a normal open question. Summary of the principles: P1 unsafe fixes are not offered (compile error, runtime change, changed comment/directive/literal text, `#if`-excluded code referring to a changed name); P2 externally dictated names are not reported; P3 names compared by value; P5 only literal and comment text is content; P6 a `/** */` documenting nothing is a block comment; P7 a rule's fix clears all its own diagnostics at the location in one application, fixes of two rules must not undo each other, an offered fix must change the document; P8 documented means a top-level `<summary>` or `<inheritdoc>`.
- Every requirement needs a covering test (`[Requirement("<ID>")]`); open questions are filed (the lead files them on GitHub from the handovers); the PR says `Closes #981`.
- Must not: change production behavior; retire an existing requirement without asking the maintainer; touch `sdd/work/coordinator/`; create issues other than open questions.
- Running in parallel, not touching attribute specs: #1068, #1069, #983, #987, #990.

## Open-question issues of touched specs

`SDD open questions: sdd/specs/shared/attribute-targets.md` (thoenissen/Reihitsu#1030) and `sdd/specs/analyzer/RH5530.md` (thoenissen/Reihitsu#1029) are closed and decided; no undecided questions block the run.

## Filed on GitHub

- Open questions on `shared/attribute-targets.md`: thoenissen/Reihitsu#1109 (Q1, split fix changes text outside the reported list; former Q2 folded in). New issue, sub-issue of #912, because #1030 was decided and closed.
- Open questions on `shared/code-fixes.md`: thoenissen/Reihitsu#1110 (Fix All of split rules needs two passes, violates SHARED-FIX-002). New issue, sub-issue of #912, because #1066 was decided and closed.
- One-line comment on thoenissen/Reihitsu#1093 with the further list-shape instances.

## Restart

The first spec-author agent was lost in a container restart during its second repair; a fresh spec author continued from the committed handovers (05-spec-author.md). This used the one allowed restart of the spec-author role.
