Role: Lead
Commit: c63f24c
Verdict: lane claimed — tests

# Run

- Issue: thoenissen/Reihitsu#979 — analyzer specs for RH4111–RH4118 and RH4120–RH4127 (16 rules), one batch of the backfill under thoenissen/Reihitsu#899.
- Slug: `naming-specs-part-two`
- Branch: `claude/loving-keller-f3x25f` (the session's designated branch; the session environment forbids pushing to another branch name, so the playbook's `claude/issue-<N>-<slug>` name is not used).
- Claim PR: thoenissen/Reihitsu#1018 (draft).

# Lane

**Specs and tests for existing behavior.** The issue asks for specs of existing rules and for tagging their tests, adding a focused test only where a requirement has no covering test. No production code changes.

Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

# Maintainer clarifications (summarized)

- Read the issue, `AGENTS.md`, `sdd/README.md`, `sdd/templates/analyzer-spec.md`.
- Reuse `sdd/specs/shared/casing.md`; use `sdd/specs/analyzer/RH4106.md` and `RH4107.md` as models for casing rules.
- The open questions on the shared casing spec are already filed in thoenissen/Reihitsu#1009 and undecided: do not file them again, and leave those cases out of the new specs. The same holds for thoenissen/Reihitsu#971.
- RH4122–RH4127 (single-letter names) get a shared spec only if more than one spec relies on the same requirement.
- The analyzers of RH4116 and RH4118 share a class name: establish what each rule reports and specify both precisely; a naming defect there is filed as an open question, not fixed.
- Done means: one spec per listed rule (16), existing tests tagged with `[Requirement]`, a focused test only where a requirement has no covering test, every example verified against a test or by running it, open questions filed per `sdd/README.md`, CI green, PR based on `main` without `sdd/work/`.
- Must not: write current-but-defective behavior into a spec; change an existing requirement of `shared/casing.md` or any other spec without asking the maintainer. Adding a new shared casing requirement for behavior these rules share is allowed.
- Open questions may be filed as the rulebook says (sub-issues of thoenissen/Reihitsu#912). Spec author: list open questions with their exact input and real output in your handover; the lead files them. No other issue is created without the maintainer's approval.

# Open questions filed (lead)

From `03-spec-author.md`, all as sub-issues of thoenissen/Reihitsu#912:

- Q1–Q4 → thoenissen/Reihitsu#1019 (RH4115)
- Q5 → thoenissen/Reihitsu#1020 (RH4121)
- Q6 → thoenissen/Reihitsu#1021 (RH4118)
- Q7 → thoenissen/Reihitsu#1022 (RH4124)
- Q8 → thoenissen/Reihitsu#1025 (RH4114), thoenissen/Reihitsu#1026 (RH4112), thoenissen/Reihitsu#1027 (RH4113)
- Q9 → new question Q5 in thoenissen/Reihitsu#1009, added as a comment (shared casing)
- Q10 → thoenissen/Reihitsu#1023 (RH4116)
- Q11 → thoenissen/Reihitsu#1024 (RH4122)
