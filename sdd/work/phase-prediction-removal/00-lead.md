Role: lead
Commit: c49431e
Verdict: lane claimed — behavior change

# Run

- Issue: thoenissen/Reihitsu#1463 — "Formatter: every phase does its own job, and no phase predicts a later one"
- Slug: `phase-prediction-removal`
- Branch: `claude/sleepy-einstein-o2j6zx` (the session's designated branch; the session environment only permits pushes to it, so the playbook's `claude/issue-<N>-<slug>` name is not used)
- Draft PR: thoenissen/Reihitsu#1469

# Lane

Behavior change. The issue requests changed formatter structure (moving whitespace-tab replacement to the start of the pipeline, a single owner of a line's final column across indentation and raw-string alignment) and spec changes to the pipeline and phase specs and the phase-ownership table in `AGENTS.md`; any changed output must be covered by a change doc. That is requested changed behavior, not a reported observed-versus-expected difference.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

# Maintainer clarifications

None so far. The issue has no comments.

# Escalation 2 — answered 2026-10-25

The maintainer approved the change doc's What and Why, chose a separate first `TabReplacementPhase`, chose folding raw-string alignment into `IndentationPhase`, and wants no follow-up for `RootListPosition`. On folding raw-string alignment into `IndentationPhase`, the maintainer asks whether the phase becomes too large and brings new problems of its own (complexity, new failure risks); this concern is open and the skeptic is to assess it. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-change-doc.md`. The change doc's status is still `Draft` and must become `Accepted` before the PR is marked ready.

# Escalations 1, 12 and 5 — answered 2026-10-25

On the skeptic's Q1 and architecture finding D2 (`02-skeptic.md`), the maintainer chose option B: fix D2 in this run, structure first, so that the layout model's column of a comment-led line matches the indented tree and the indentation phase moves a raw string's lines by the model's single shift. The wider scope (six currently stable outputs change; every alignment contributor reads the corrected column) is accepted. The change doc must list the changed outputs and be approved again (escalation 2). The skeptic's spec-precision points S1–S3 are to be applied. Follow-up candidates are discussed with the maintainer when the PR is ready; a follow-up to review (and possibly rewrite) `IndentationPhase` is to be drafted then. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-layout-model-column.md`.

# Escalation 2 (amended change doc) — answered 2026-10-25

The maintainer approved the amended change doc as of `03-spec-author.md`. Its status is to be set to `Accepted` by the spec author before the PR is marked ready. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-amended-change-doc.md`.

# Escalations 4 and 5 (first-on-line anchor) — answered 2026-10-25

On the implementer's disputed tests (`05-implementer.md`), the maintainer chose option A: an anchor on a comment-led line is always measured at its real column behind the comment, also when it is the first code on its line (for example a complex element initializer's opening brace led by `/* Keep element. */`). The two disputed RH5303/RH5304 formatter tests change to the new output, the RH5303 and RH5304 analyzers must agree with that anchor so they do not report formatter output, and the change doc is amended and approved again (escalation 2). Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-first-on-line-anchor.md`.

# Escalation 2 (second amendment) — answered 2026-10-25

The maintainer approved the change doc as of `06-spec-author.md`. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-second-amendment.md`. Status `Accepted` is still to be set by the spec author before the PR is marked ready.

# Escalations 5 and 12 (node-level shift) — answered 2026-10-25

On the implementer's stop in `08-implementer.md`, the maintainer chose option A: fix narrowly. The RH5303/RH5304 code fixes withhold themselves for a complex element whose opening brace's line starts inside a multi-line comment (the diagnostic stays without a fix, a supported end state per `AGENTS.md` *Code-fix coverage*); the gap is recorded as a decision in the RH5303/RH5304 specs; the tests that assert node-level output the change doc excludes are replaced by tests of the decided behavior; the node-level column steps (`ComputeTokenColumn` / `AdjustNodeIndentation`, together with the skeptic's follow-ups 1 and 2) become a follow-up candidate, presented with the other follow-ups when the PR is ready. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-node-level-shift.md`.
