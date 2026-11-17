# Lead — expression-body-arrow-unsupported

- Issue: thoenissen/Reihitsu#1851 — `agent-filed` (goals are proposals; the decision to declare the shape is the maintainer's, see below)
- Slug: `expression-body-arrow-unsupported`
- Branch: `claude/charming-carson-n5mazp` (session-designated)
- Draft PR: thoenissen/Reihitsu#1856
- Lane: behavior change — the issue declares a new unsupported shape (new FMT-UNSUP requirement, formatter refusal, CLI report, withheld code fixes, possibly RH5401 reporting), which needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing when follow-up candidates exist. None skipped.

## Maintainer clarifications (summarized)

- The maintainer already answered escalation 10 for this shape: declare it unsupported, in a change of its own; this issue is that change (issue body and triage comments). The decision to declare stands; the run builds it.
- Prerequisite runs (#1847, #1848, merged through #1853) are on `main`; the sibling shape behind the arrow (FMT-UNSUP-012, #1839) is on `main`.
- Open in the issue, decided in the change: whether RH5401 stops reporting a member whose arrow line follows a comment, or keeps reporting with no fix (issue recommends the first). This is a visible output → output question.

## Maintainer answers

- Escalation 10 (declare the shape "comment, directive, or disabled text between an expression body's `=>` and the code in front of it"): declare it, in a change of its own — this run. (Given before the run, in #1851.)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim, lane, prepare done.
- Next: spawn spec author.
