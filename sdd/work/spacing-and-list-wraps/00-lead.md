# Lead — spacing-and-list-wraps

- Issues (prompt order): thoenissen/Reihitsu#1642 (`agent-filed`), thoenissen/Reihitsu#1635 (`agent-filed`). Goals of both are proposals.
- Slug: `spacing-and-list-wraps`
- Branch: `claude/keen-goldberg-e84ipm` (session-mandated branch, see decision 1)
- Draft PR: thoenissen/Reihitsu#1733
- Lane: **behavior change** (widened from bug fix after the maintainer's answers, decision 6)
  - #1642: reported observed-versus-expected difference in the formatter's horizontal spacing (space behind a multi-line `*/` removed before some tokens).
  - #1635: reported difference against FMT-BREAK-012 in the line-break phase (argument list wrapped in front of a comma is not laid out one element per line).
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author: only for a clarification (skipped unless a role reports a spec gap).

## Maintainer clarifications

- #1635 triage comment (maintainer): stays a bug against FMT-BREAK-012 for 1.2.2; related to the vNext issue about a leading comma in an argument list (thoenissen/Reihitsu#956) but not folded into it.
- #1635 second comment (agent-written): two related observations — a list wrapped only after `(` is joined to one line (`M(⏎a, b)` → `M(a, b)`), and a trailing line comment that pins a leading comma keeps two elements on one line. Both proposed to be settled together with the issue's input; proposals, not binding.

## Maintainer answers

- Batch 1 (before test implementer), #1635:
  - Q1 `Foo(1⏎, 2, 3)` → **C**: one argument per line with trailing commas: `Foo(1,⏎    2,⏎    3)`. Applies to attribute argument lists too.
  - Q2 `Foo(1⏎, 2,⏎3)` → **like 1C**: `Foo(1,⏎    2,⏎    3)`.
  - Clarification (summarized): a list is multi-line when its elements are on different lines and single-line when all elements are on the same line. Consequence: a list wrapped only behind `(` with all elements on one line (`M(⏎a, b)`) is single-line, so today's join to `M(a, b)` is the intended output — this changes FMT-BREAK-012's "wrapped after its opening parenthesis" clause.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's mandated branch `claude/keen-goldberg-e84ipm` instead of `claude/issues-1642-1635-<slug>` | The environment allows pushing only to that branch | Branch name is internal | claim commit |
| 2 | Skeptic | #1642: make the change? | Do not change behavior. Add only a characterization test for FMT-SPACE-001's sentence on multi-line comments; drop the issue's expected output; link `Refs #1642` | FMT-SPACE-001 and FMT-SPACE-013 require today's output, and the issue's premise that a single-line comment keeps the space is false (skeptic 01). The issue reproduces as reported, so it is not escalation 4 | The output stays as on `main`, which unchanged requirements demand | – |
| 3 | Skeptic | #1635: fix the wrap-only-behind-`(` case (W1) and the comment-pinned leading comma (W2c) in this run? | W1: yes — unchanged FMT-BREAK-012 names it, same mechanism. W2c: only if it falls out of the same mechanism with no extra guard; otherwise left as written (rare class, RH5035/RH5038 report it) | FC-016; *Special cases do not grow the source* | W1 is required by unchanged FMT-BREAK-012; W2c is a rare class either way | – |
| 4 | Skeptic | Architecture finding: the "wrapped behind the opening delimiter" signal exists only in the indexer visitor | Change the structure in this run: one shared signal for all four list visitors in `LineBreakListRewriter`, within the component the run changes | The class includes common code (W1), and per-visitor copies are the defect | Internal structure | – |
| 6 | Answers | Lane after the answers | Widen to behavior change: change doc + FMT-BREAK-012 diff (multi-line decided by elements on different lines; leading commas become trailing in multi-line argument/attribute argument lists) by the spec author, then a fresh skeptic on the spec diff | Maintainer's answers change visible output beyond the unchanged spec | Decided by the maintainer | – |
| 7 | Answers | Decision 3's W1 | Superseded: W1 (`M(⏎a, b)`) stays joined to one line per the maintainer's clarification; W2c (comment-pinned leading comma) is a rare class: rewrite only when the comma move is safe with a cheap check, otherwise leave as written | Maintainer answer; *Special cases do not grow the source* | Maintainer answer / rare class | – |
| 8 | Skeptic 03 | Change doc entry 6 (indexer wrapped only behind `[` now joined) — ask? | No question: covered by the maintainer's clarification (elements decide multi-line); the old split was a lead decision of #1644 | Maintainer answer covers it | Covered by maintainer answer | – |
| 9 | Skeptic 03 | Decision 4 (shared opener-wrap signal) | Moot: the new definition needs no opener signal; delete the indexer-only one | Skeptic 03 | Internal | – |
| 10 | Skeptic 03 | Refusal scope for a comma held by a comment/directive | Narrow to the held gap, as FMT-BREAK-037 does; the rest of the list follows answer C; FMT-BREAK-011 clause and change doc updated to match, with entries for the three argument-list inputs | Whole-list form conflicts with SHARED-DIAG-010 (RH5102 reports it), gives up safe ground, and drifts from FMT-BREAK-037 (FC-016) | Rare class; the rest follows the maintainer's answer C | – |
| 11 | Skeptic 03 | Comment behind a leading comma that moves (`Foo(1⏎, /* c */ 2, 3)`) | Comment moves with its comma: `Foo(1, /* c */⏎2,⏎3)`, as parameter lists on `main` and FMT-BREAK-037 already do (FMT-PIPE-014) | Existing precedent decides it | Decided by existing requirements | – |
| 12 | Skeptic 03 | Line counting in the exception paragraph | Count lines as SHARED-BLANK-008 does, like the first paragraph | Consistency | Internal reading | – |
| 13 | Skeptic 03 | Change doc completeness | Add RH5101's fix result change (`Foo(⏎1⏎, 2, 3)` → `Foo(1,⏎2,⏎3)`) and RH5102's convergence | Missing visible entries; follow from answer C | Follow from answer C | – |
| 14 | Test impl. 04 | A held comma alone on its line (`int first // keep⏎,⏎int second`): join to `, int second` or leave in place? | Leave in place: the held comma and the line break behind it stay as written; existing tests `CommaAloneOnItsLineStaysAligned` and `CommaAfterDirectiveHeldFirstParameterStaysAligned` stand; spec author states the case in FMT-BREAK-012 | Rare class (RH5035/RH5038 report the comment); refusing needs no extra machinery | Rare class: leaving a construct as written | – |
| 15 | Test impl. 04 | Architecture test `RepositoryFilesReferenceNoSquadNote` fails on `03-skeptic.md` line 24 | Spec author rewords that line (no squad-note path) | Keeps the validator green before `sdd/work/` is deleted | Internal text | – |
| 16 | Implementer 05 | Directive-held comma tests parse without `X`, so `, 3` is disabled text and the list is single-line | The tests are wrong: they pass the preprocessor symbol `X`; the FMT-BREAK-012 example and change doc entry 8 state that the branch is active (X defined). Without X, the list stays unchanged by the unchanged elements-decide definition | Reading another build is ruled out (*Special cases do not grow the source*) | Decided by the spec's own definition; the asserted output only adds the build condition | – |
| 17 | Implementer 05 | `IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter` pins the old output | Update to `int first,⏎int second // keep⏎, int third` (change doc entry 7 `O`) | Decided by the changed FMT-BREAK-012 and entry 7 | Covered by entry 7 / decision 10 | – |
| 5 | Skeptic | Refuse leading-comma argument lists instead? | No | Common code; maintainer triage keeps it a bug for 1.2.2; declaring unsupported deletes nothing | – | – |

## Follow-up candidates

- `Foo(1, 2, 3⏎)`: RH5102 reports it, its fix makes no progress, the formatter keeps it; #956 decides the target — extend thoenissen/Reihitsu#956 with this note (01-skeptic).
- thoenissen/Reihitsu#956: this run moves leading commas behind the previous element in multi-line argument and attribute argument lists (maintainer answer 1C); extend #956 so it reflects what remains (lead).
- RH5109 reports `void N(⏎int a, int b)` and its fix splits it, while FMT-BREAK-012 joins it; RH5109-003's example contradicts FMT-BREAK-012, which it cites (02-spec-author, 03-skeptic).
- RH5107's fix and the formatter disagree on a comment behind a leading comma in a parameter list: `void P(int a⏎, /* c */ int b)` → formatter `int a, /* c */⏎int b`, fix `int a,⏎/* c */ int b` (03-skeptic).
- New-rule candidate: argument-list counterpart of RH5107 (leading comma); check #956 first (03-skeptic).

## Worklist / next step

- Done: skeptic (01).
- Answers received (see above).
- Done: spec author (02), skeptic on spec diff (03).
- Done: spec author amendment (aae1cae); change doc accepted by the lead.
- Done: test implementer (04, 0aeb480).
- Done: spec author amendment (a8ba6f3).
- Done: implementer (05, c119a8a): 3 disputed tests.
- Done: spec author (8c01778), test implementer (06, 32a99e5); all focused and full formatter tests green.
- Done: cleaner (07, 48ccd41): clean.
- Done: spec checker (08, 99fa02d): two change-doc findings (status Draft; stale "code not written yet").
- Repair round 1: spec author fixes both (status Accepted on lead acceptance) → spec checker (10) → auditor.
