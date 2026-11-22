# Lead — thoenissen/Reihitsu#1926

- Issue: thoenissen/Reihitsu#1926 — declare a comment, directive, or disabled text behind an assignment `=` (property initializer included, outside the switch-arm/anonymous-member cases FMT-UNSUP-019 already refuses) or behind a switch arm's `=>` unsupported. Labels: enhancement, formatter, `agent-filed`.
- Slug: `unsupported-assignment-arm-comment`
- Branch: `claude/squad-1926-97asq4` (session-designated). Draft PR: #1944.
- Lane: **behavior-change** — declaring a new unsupported shape changes visible formatter, CLI, and code-fix output (new FMT-UNSUP requirement, precheck change), so it needs a change doc and production code.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Prerequisite: the maintainer ordered this run after PR #1933 (#1633 #1634 #1647 #1685 #1542 #1929) merged; it has merged into `main` (652e67b).

## Maintainer answers

- Escalation 10 for this shape (asked in the #953 run, PR #1920, re-confirmed in an issue comment by the maintainer on 2026-11-22): **declare it unsupported** — a comment, directive, or disabled text behind the `=` of an assignment or a property initializer, or behind the `=>` of a switch arm. The file stays unchanged, the CLI reports it with a reason and a location, and code fixes whose formatted node holds the shape are withheld. Declared in a change of its own (this run). The question is not to be asked again.
- The change doc lists the special cases the declaration deletes under *Special cases*; if there are none (the continuation indent those lines get today is the general rule), it states the shape is declared on the maintainer's explicit answer, as FMT-UNSUP-019 was.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step: claim and lane.
- Next: spec author.
