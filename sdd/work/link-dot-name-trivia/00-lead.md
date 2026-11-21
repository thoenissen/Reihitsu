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
- Batch 1, Q1 (reason text): **A** — `a comment or directive between a . and its member name`.
- Batch 1, Q2 (one-line form, e.g. `t. /* c */ Length`): **A** — refused too; the shape is a place on any line, declared in this change.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Ask escalation 10 for the qualified-name dot gap? | No; file it as a follow-up with options A/B/C | Declaring it would delete no special case (no phase handles it) and leave the no-comment cells broken; AGENTS.md: a shape that deletes nothing is not declared | Follow-up only; nothing in this run changes that output | — |
| 2 | Skeptic | Own requirement FMT-UNSUP-016 vs widening FMT-UNSUP-015; defined by syntax kind (every member access/binding) | Accept the spec author's choices | Matches the earlier answer (a place in the tree), cheap check | Internal structure; the refused inputs are those the maintainer's answer names | a5328b71 |

## Follow-up candidates

- Qualified name split at its `.` loses its column (with or without a comment): `using static System.`/`Math;` to column 1; `typeof`, `new`, member-type continuations to statement/member column; with comments also using aliases, namespaces, attribute names, `default`, casts. No phase joins a `QualifiedNameSyntax` at its dot. Outside #1888. Carries an open question with options: A join the name back onto its dot, B indent the continuation one level, C leave as written. Sources: 01-spec-author.md, 02-skeptic.md (wider class).

## Answered output questions (batch 1)

- Reason text for FMT-UNSUP-016 (01-spec-author.md): A `a comment or directive between a . and its member name` (rec.), B `a comment or directive between a member access's . and its name`.
- One-line form `t. /* c */ Length` refused (02-skeptic.md) — asked as a neighbour to confirm the earlier answer covers it.

## State

- Audited SHA: —
- Last step: test implementer done (3c7d2da1): 302 formatter + 17 analyzer + 4 CLI tests red as expected.
- Next step: implementer (05).
