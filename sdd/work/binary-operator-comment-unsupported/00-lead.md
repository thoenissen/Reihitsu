# Lead — binary-operator-comment-unsupported

- Issue: thoenissen/Reihitsu#1726 — `agent-filed` (goals are proposals). Labels: enhancement, analyzer, formatter. Milestone 1.4.0.
- Slug: `binary-operator-comment-unsupported`
- Branch: `claude/admiring-heisenberg-tpv91a` (session-designated). Draft PR: #1831.
- Base at claim: `a971d8944cdc449278472f6ad85e403c9d48cd70`
- Lane: **behavior change** — the issue declares a new unsupported shape (new FMT-UNSUP requirement, precheck change, visible formatter/CLI/code-fix output change); this needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

- Escalation 10 (declare the shape unsupported), answered before this run and carried by the issue text: declare a comment or directive between a binary operator and its right operand unsupported, in a change of its own (this run). Comments above the operator's line stay supported; only the position behind the operator is refused. Same decision as for the conditional operator (FMT-UNSUP-008).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Spec author done (01, c920452): FMT-UNSUP-011, change doc Draft. Next: skeptic (02).
