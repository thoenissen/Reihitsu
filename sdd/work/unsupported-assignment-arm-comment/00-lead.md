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
- Batch 1 (2026-11-22), answer given as "1b 2a 3a":
  - Q1 compound assignment operators → **B**: every assignment operator is refused, a property initializer's `=` included; the existing arm/anonymous-member assignment shape (FMT-UNSUP-019) is folded in, and its reason changes to the new one. Declared in this work.
  - Q2 deconstruction declaration `var (a, b) = // c⏎t;` → **A**: refused. Declared in this work.
  - Q3 (escalation 10) the remaining `=` gaps — a comment/directive/disabled text behind an enum member's `=`, a parameter default's `=`, and a directive/disabled text behind a variable declarator's `=` → **A**: declared unsupported **in this work** (not in a change of its own).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | One requirement or two (assignment `=` vs. switch-arm `=>`) | Two: FMT-UNSUP-020 and FMT-UNSUP-021, each with its own reason | Separate gaps, separate reasons for the CLI report | The maintainer's answer covers both shapes; requirement split is internal | 4046a07 |
| 3 | skeptic | Initializer members (object/`with`/index) as assignments | Count them (as the spec author wrote) | They are assignment expressions, as FMT-UNSUP-019 already counts them | Covered by the maintainer's answer ("the `=` of an assignment") | 4046a07 |
| 4 | skeptic | Issue goals "a CLI test per input" / "a code-fix test per input" | One precheck test per input (CLI path already pinned by CLI-019 end-to-end tests); one test per affected fix plus the must-stay-offered cases | Coverage first | Tests only | — |
| 2 | spec author | Order of the new shapes in the precheck | Listed last, so an overlapping earlier shape (FMT-UNSUP-010/019) keeps its own reason | Keeps today's reasons for already-refused input | Reasons of already-refused input stay as they are | 4046a07 |

## Follow-up candidates

- Open question outside the issue (from `02-skeptic.md`): `int P { get; } /** c */ = 1;` — `main` moves the `/** */` onto its own line behind a blank line, leaving `/** c */ = 1;`; stable but looks wrong. Follow-up carrying the output question.
- ~~Escalation 10 neighbouring `=` gaps~~ — answered 3A, joins this run; no longer a follow-up.

## State

- Batch 1 answered (1B, 2A, 3A).
- Now: one spec-author amendment round (fresh agent) with the answers plus the skeptic's spec gaps (`02-skeptic.md`: FMT-BLANK-018/020 and FMT-INDENT-023 examples, FMT-BREAK-016 wording, change doc completeness incl. RH5304 and directive cases, using-alias `=` on the other side), then status Accepted, then test implementer.
- Last step: skeptic (02).
