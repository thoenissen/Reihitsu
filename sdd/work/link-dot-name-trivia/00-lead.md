# Lead — link-dot-name-trivia

- Issue: thoenissen/Reihitsu#1888 — `agent-filed` (goals are proposals).
- Slug: `link-dot-name-trivia`
- Branch: `claude/loving-pasteur-qjid3h` (session-designated)
- Draft PR: #1903
- Lane: **behavior change** — the issue asks to declare a new unsupported shape (new FMT-UNSUP requirement, visible formatter/CLI/code-fix output change) with a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer clarifications

- Escalation 10 for this shape was already answered by the maintainer: declare it unsupported, in a change of its own. Recorded in `sdd/changes/2026-11/unsupported-link-operator-trivia.md`, *Decisions*, "The gap between a link's `.` and its member name is declared in a change of its own (maintainer, batch 1, answer 2A)". Confirmed by the lead at claim time. Triage comment on the issue places it in 1.5.0 after the runs for #896, #892, #1877/#1878.

## Maintainer answers

- (pre-run) Declare the dot-to-name shape unsupported, in a change of its own — see above.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- Using directive: a comment between a qualified name's `.` and its name moves the continuation to column 1 (`using System. // c1` / `Text;`). Outside #1888. Open question (indent vs leave as written) + escalation 10 option (declare unsupported; spec author recommends against). Source: 01-spec-author.md. Escalation 10 to be asked with batch 1.

## Open output questions (batch 1)

- Reason text for FMT-UNSUP-016 (01-spec-author.md): A `a comment or directive between a . and its member name` (rec.), B `a comment or directive between a member access's . and its name`.
- Escalation 10: using-directive qualified-name dot gap (above).

## State

- Audited SHA: —
- Last step: spec author done (a5328b71), change doc Draft.
- Next step: skeptic (02), then batch 1 output questions.
