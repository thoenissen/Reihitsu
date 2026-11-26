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
- Batch 2 (2026-11-26): first reply "unsupported", asked back; then: both cases (attribute-argument `=` and using-alias `=`) are unsupported, and the lead decides the message, possibly two different messages.
  - Q2 → **B**: a comment, directive, or disabled text behind the `=` of an attribute argument or of a using alias is declared unsupported, in this work (on the maintainer's explicit answer; it deletes no special case).
  - Q1 → delegated to the lead: see decision 7.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | One requirement or two (assignment `=` vs. switch-arm `=>`) | Two: FMT-UNSUP-020 and FMT-UNSUP-021, each with its own reason | Separate gaps, separate reasons for the CLI report | The maintainer's answer covers both shapes; requirement split is internal | 4046a07 |
| 3 | skeptic | Initializer members (object/`with`/index) as assignments | Count them (as the spec author wrote) | They are assignment expressions, as FMT-UNSUP-019 already counts them | Covered by the maintainer's answer ("the `=` of an assignment") | 4046a07 |
| 4 | skeptic | Issue goals "a CLI test per input" / "a code-fix test per input" | One precheck test per input (CLI path already pinned by CLI-019 end-to-end tests); one test per affected fix plus the must-stay-offered cases | Coverage first | Tests only | — |
| 5 | spec author (03) | Does an equals-value clause formatted on its own hold the whole gap (refused) | Yes, pending the skeptic's check of the amendment: no caller formats a bare clause (RH5111 formats the declaration/list/enum, RH5303 the object creation), and it lets both equals-value refusals go | Deletes refusals; direct-call examples retargeted | No caller reaches a bare clause (AGENTS.md, Supported inputs) | 0106e32 |
| 6 | lead | Re-run the skeptic on the amended spec | Yes, fresh skeptic on 03's amendment before batch 2 | Answer 3A added shapes and a FMT-UNSUP-003 reading no skeptic has checked; its questions join batch 2 so the run stops once | Process only | — |
| 7 | batch 2 | CLI reason wording (delegated by the maintainer) | Two reasons: `a comment or directive behind an assignment operator or the = of a declaration` (assignment operators, property/declarator/enum-member/parameter `=`), and `a comment or directive behind the = of an attribute argument or a using alias` (name-equals `=`) | Each message names what the user wrote; the maintainer allowed two messages | Maintainer delegated the choice explicitly | — |
| 8 | test implementer (06) | When to repair the FMT-BREAK-030 / FMT-INDENT-030 examples and their pinned test | After the implementer, in one spec-author + test-implementer pass before the cleaner | Keeps the shared tree free of parallel edits during the implementer's builds | Spec examples and a phase-alone test; outputs follow the answered shape | — |
| 9 | pre-audit merge | Run the cleaner after the merge conflict resolution | Skip it | The conflicts and their resolution touched only two spec Markdown files (`Uses` rows); no C# file was resolved by hand, and main's C# arrives as merged on main | No compiled or formatted file changed by the resolution | 71519cb |
| 10 | audit | The auditor's helper-duplication hint | Follow-up, not taken into this run | A compiled change after the audit would need a re-audit; no output changes | Internal refactoring | — |
| 2 | spec author | Order of the new shapes in the precheck | Listed last, so an overlapping earlier shape (FMT-UNSUP-010/019) keeps its own reason | Keeps today's reasons for already-refused input | Reasons of already-refused input stay as they are | 4046a07 |

## Follow-up candidates

- From `13-auditor.md` (hint): `PrecheckPhase.FindTriviaBehindToken` duplicates `FindTriviaBehindBinaryOperator` and `FindTriviaBehindExpressionBodyArrow`; route those through one helper (internal cleanup, no output change).
- Open question outside the issue (from `02-skeptic.md`): `int P { get; } /** c */ = 1;` — `main` moves the `/** */` onto its own line behind a blank line, leaving `/** c */ = 1;`; stable but looks wrong. Follow-up carrying the output question.
- ~~Escalation 10 neighbouring `=` gaps~~ — answered 3A, joins this run; no longer a follow-up.

## State

- Batch 1 answered (1B, 2A, 3A).
- Done: spec-author amendment (03, commits 0106e32, 4f7624a). Open output questions from 03: reason wording; attribute-argument / using-alias `=`.
- Done: skeptic on the amendment (04, f182d14): proceed; decision 5 confirmed; one spec wording fix (FMT-UNSUP-020 equals-value bullet: the gap belongs to the shape for every owner) and RH5305 in the change doc's still-offered list; extra existing tests that break are listed in 04.
- Batch 2 answered (Q2 B; Q1 delegated → decision 7).
- Done: spec-author second amendment (05, 577e60a, 81f474b): FMT-UNSUP-022 added, change doc Accepted.
- Done: test implementer (06, 4f0282c, 2205e54): 181 formatter + 33 analyzer tests red for the intended reason; architecture green.
- Open worklist after the implementer (same requirement, PR-introduced): spec author — FMT-BREAK-030 example `Child =⏎⏎/* c */ {` (now refused; use the FMT-BLANK-018 carrier) and FMT-INDENT-030's mention of the nested-initializer anchor; test implementer — move `LineBreakPhaseBlankLineTests.KeepsBlankLinesAboveBlockStatementAndGluedComment` with the example. Decision 8.
- Done: implementer (07, 0f0adb0, 7c3f1c9): all tests green in Debug; 019 walk and five join refusals deleted.
- Done: decision-8 worklist — spec author (08, 25ef89b), test implementer (09, 8f6368d).
- Done: cleaner (10, 8e64333): clean, Release 0 warnings.
- Done: spec checker (11, ed3177f): consistent, no findings.
- Done: merged origin/main eed7195 (71519cb); conflicts only in `Uses` rows, resolved by spec author (12, 0a728ea).
- Done: build green; auditor (13, 0c1d326) passed at audited SHA **c5374c2** with one comment-only finding (ObjectInitializerContributor.cs:122-123).
- Done: cleaner (14, 72ef94a): comment fixed; text-only proof PASS vs c5374c2 (no second audit).
- Now: validator (15), then issue manager, note, publish.
- (superseded:) fresh skeptic on the amendment (04), then batch 2 with 03's and 04's questions, then status Accepted, then test implementer.
- (previous plan:) one spec-author amendment round (fresh agent) with the answers plus the skeptic's spec gaps (`02-skeptic.md`: FMT-BLANK-018/020 and FMT-INDENT-023 examples, FMT-BREAK-016 wording, change doc completeness incl. RH5304 and directive cases, using-alias `=` on the other side), then status Accepted, then test implementer.
- Last step: skeptic (02).
