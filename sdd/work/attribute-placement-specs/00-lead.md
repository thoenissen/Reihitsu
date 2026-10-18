Role: Lead
Commit: c1483939e2a6fde3da5b02653c9da62ece924138
Verdict: lane claimed

# Run: thoenissen/Reihitsu#980

- Issue: thoenissen/Reihitsu#980 — SDD analyzer specs for RH5501, RH5503, RH5505, RH5507, RH5509, RH5511, RH5513, RH5515, RH5517, RH5519, RH5521, RH5523, RH5525, RH5527, RH5530 (attribute placement), one batch of the backfill under thoenissen/Reihitsu#899.
- Slug: `attribute-placement-specs`
- Branch: `claude/laughing-feynman-pq99uy` (the session's designated branch; the session environment does not allow pushing a `claude/issue-980-…` branch). Draft claim PR: thoenissen/Reihitsu#1028.
- Base: `origin/main` at c1483939e2a6fde3da5b02653c9da62ece924138.

## Lane

**Specs and tests for existing behavior.** The issue asks for specs of existing rules and the covering tests; no behavior is requested to change, and the maintainer explicitly excludes production code changes.

Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production code in this lane), auditor (the lane table does not run it).

## Maintainer clarifications (summarized)

- Read thoenissen/Reihitsu#980, `AGENTS.md`, `sdd/README.md`, `sdd/templates/analyzer-spec.md`. Model for a shared spec with member rules: `sdd/specs/shared/casing.md` with `sdd/specs/analyzer/RH4106.md`, and `sdd/specs/shared/statement-blank-lines.md` with `sdd/specs/analyzer/RH5001.md`. The formatter's attribute line-break requirements are in `sdd/specs/formatter/line-breaks.md` (FMT-BREAK IDs).
- Exactly one new shared spec under `sdd/specs/shared/` for the attribute-target rules, covering the shared target behavior and the placement behavior. Its code owners are `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, and `TargetAttributePlacementCodeFixProviderBase`. Its name and structure must let thoenissen/Reihitsu#981 (the list-shape rules) extend the same spec later. Where the formatter owns the layout, reference the FMT-BREAK requirement ID instead of restating it.
- Done means: one spec per listed rule (15), the shared spec, existing tests tagged with `[Requirement]`, a focused test added only where a requirement has no covering test, every example verified against a test or by running it, open questions filed per `sdd/README.md`, CI green, PR based on `main` with no `sdd/work/`.
- Must not: write current-but-defective behavior into a spec; specify the list-shape rules (thoenissen/Reihitsu#981); change an existing requirement of another spec without asking the maintainer.
- Open questions may be filed as the rulebook says (sub-issues of thoenissen/Reihitsu#912) without asking. No other issue may be created without approval.
- A parallel run on thoenissen/Reihitsu#979 also edits `sdd/specs/README.md` and `sdd/specs/requirement-ids.txt`; when `main` moves, merge it before the final checks and keep both sides' rows.

## Open-question issues on touched specs

None open for `sdd/specs/formatter/line-breaks.md` (thoenissen/Reihitsu#918 is closed); the shared spec and the 15 analyzer specs are new.
