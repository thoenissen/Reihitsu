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

# Escalations 12 and 1 after audit 1 — answered 2026-10-25

On the auditor's findings (`17-auditor.md`), the maintainer chose 1B and 2A. 1B: change the structure first — the node-level formatting path must stop deciding a line's final column after the pipeline, so that the indentation phase is the only owner of that column for node-level targets too; this widens the change doc (to be amended) and may make RH5303-006/RH5304-005 unnecessary. 2A: the new output for a comment line above a comment-led line inside an interpolation hole (aligned to the leading comment, as on ordinary lines) is intended; FMT-RAW-007, the change doc, and a test follow it. The remaining audit findings 2 and 4–7 are repaired in the same round (repair round 2 of 3). Standing instruction from the maintainer for the rest of the run: continue autonomously, take the lead's recommended option for every further question, commit a checkpoint before acting on each decision, record each in `squad/decisions/`, and present all decisions at the end. Checkpoint before this decision: 724ce92. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-audit-1.md`.

# Third amendment and spec author's Q1/Q2 — decided 2026-10-25 under the standing instruction

The lead took its recommended options: the change doc as of `18-spec-author.md` is approved (set `Accepted`); Q1 (BOM-led raw string): B — restore the earlier output, the BOM counts as one column in the raw-string move; Q2 (tab in front of a node-level target): A — four columns, as FMT-PIPE-017 states. Checkpoint before: b40c2fb. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-third-amendment.md`.

# Skeptic round 20 — decided 2026-10-25 under the standing instruction

C1 (tab in front of a node-level target): B — for whatever lies outside the target, the node-level column counts characters (a whitespace tab outside the target is one column), so the fixes keep converging under SHARED-FIX-001/005; SHARED-FIX-007 is clarified not to reach whitespace tabs outside the code the fix changes. This reverses the earlier Q2 A. C2–C7 are spec wording with a single reading, for the spec author. Checkpoint before: 0e22387. Recorded in `squad/decisions/2026-10-25-phase-prediction-removal-tab-before-node-target.md`.

# Audit 2 — decided 2026-10-26 under the standing instruction

On `28-auditor.md`: finding #8 (repeats #1, escalation 12): option A — the context entry point restores the target's original leading trivia only where FMT-PIPE-010 keeps it, so no step after the last phase moves a line (FMT-PIPE-017 and the `AGENTS.md` sentence then hold as written). Finding #9: the dead line-offset mapping and attached-input clone are repaired (removed or made reachable and tested). Finding #10: keep `ShiftContinuationExteriorMarkers`; its removal is a follow-up candidate (public Core API). Repair round 3 of 3. Checkpoint before: 504310f. Recorded in `squad/decisions/2026-10-26-phase-prediction-removal-audit-2.md`.
