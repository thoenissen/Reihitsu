# Lead — comma-comment-layout

- Issues: thoenissen/Reihitsu#1737 (`agent-filed`), thoenissen/Reihitsu#1738 (`agent-filed`). Goals of both are proposals.
- Slug: `comma-comment-layout`
- Branch: `claude/adoring-shannon-yomd27` (the session's designated branch; the harness allows pushes only there, so the `claude/issues-1737-1738-...` name of `lead.md` is not used)
- PR: thoenissen/Reihitsu#1749 (draft claim)
- Lane: **behavior change**
  - #1737: the fix and the formatter disagree on where a comment behind a leading comma goes; one of them changes visible output and the spec has to state the placement → behavior change (change doc).
  - #1738: no spec requirement names a comma starting a line in an object initializer; deciding and specifying its layout is new specified behavior → behavior change.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer answers

- Batch 1, Q1 (#1738, comma that starts a line inside initializer / collection expression / pattern): **A**, with one change — in an array initializer (case `e`, `new int[] { 1⏎, 2 }`) the comma also moves behind the previous element (`1,` ⏎ `2`), as in the other constructs. A comma held off the previous line by a `//` comment (case `c`) stays alone on its line at the elements' column, element on the next line, as in A.
- Batch 1, Q2 (escalation 10, declare "comma separated from the previous token by a line break" unsupported): **not declared (B)**; the comma keeps moving up behind the previous element as today.

- Batch 1b (asked at once, blocks tests): array initializer with a comma held off the previous line by a comment (`1 // k⏎, 2`) — answered **A** (keep `, 2`, column only). The maintainer is considering declaring unsupported the shape "a comment between a list element and its following comma" ("no comment belongs there") and asked which special cases it would delete → escalation 10, investigated (04-skeptic, 485162a): only a shape covering comments AND directives/disabled text deletes code. Escalation 10 answered: **Q4 A — declare** the shape (comment, directive, or disabled text between a comma and the token in front of it, comma on a later text line; every comma token; definition per 04-skeptic §1), **Q5 A — in this run**. Batch 1b answer (3A) becomes moot.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch | Harness restricts pushes to it | Internal | claim commit |
| 2 | Spec author | One change doc per issue or one shared | One per issue | Causes differ (fix vs. formatter layout) | Doc structure is internal | 81bfa87 |
| 3 | Spec author | #1738 scope: object initializers only or the whole FMT-BREAK-021/022 family | Whole family, pending skeptic | Same mechanism; one class, not instances | The family's outputs are asked as output questions | 81bfa87 |
| 5 | Skeptic | #1737: fix follows formatter (A), formatter follows fix (B), or withhold | Withhold RH5107's fix when the reported comma's trailing trivia holds a comment; T (comment only on the next line) keeps its fix. Spec RH5107 records the withheld class; change doc reworded accordingly. Issue goal reworded to "no fix result disagrees with the formatter"; still `Closes #1737` | Class is rare by definition (RH5035 reports every comment in a parameter list); B would ship a SHARED-FIX-003 violation for Q/R; A copies FMT-BREAK-012 into a hand-written text edit (architecture finding → refuse first) | Withholding a fix for a rare class is not in question (README, What reaches the maintainer) | pending |
| 6 | Skeptic | #1737 architecture finding (fix and line-break phase own comma placement) | Refuse the class (decision 5); structural change (fix delegates to formatter) as follow-up | README, Architecture findings: refuse first | Internal | — |
| 7 | Skeptic | #1738 question shape | Use the skeptic's single reworded question (spec author's Q2 option A conflicted with RH5301/5303/5304; Q3 folded in) | Avoids a permanent diagnostic (FC-013) | Asked as output question | — |
| 8 | Spec author 05 | Declaration in its own change doc; FMT-INDENT-033 retired; FMT-UNSUP-004 reason wins where both shapes hold; RH5110-003 fix withheld via SHARED-FIX-010; AGENTS.md precheck row updated | Accepted as proposed | Follow the answered declaration; same pattern as FMT-UNSUP-008 | The visible part (refusal, withheld fixes) is the maintainer's 4A/5A answer; rest is spec bookkeeping | 57771bb |
| 9 | Test implementer | Delete 40 tests whose input FMT-UNSUP-009 now refuses (shapes moved to precheck rows); expected values for uncovered #1738 cases = main's output of the comma-last form | Accepted | Follows the declaration and answer 1A | Covered by maintainer answers 1A/4A | cf2ad90 |
| 10 | Implementer | Disputed Fix All test lacks MarkupHandling.Allow | Route to fresh test implementer (configuration only, no assertion change) | Implementer's probe shows assertion holds | Test config is internal | 481c314 |
| 11 | Implementer | Spec gap: which reason wins when FMT-UNSUP-004 and -009 refuse the same comment at a non-declarator comma | Route to fresh spec author: state FMT-UNSUP-009 wins outside declarator commas (as implemented and tested) | One statement of the rule | Reason text was not asked; both reasons are already user-visible texts — choice follows the tested behavior and the declaration's answer | dd4f25b |
| 12 | Implementer | Comma collapse moved to shared `LineBreakTriviaUtilities` with two consumers | Accepted | One owner of the policy | Internal | d3ae8ef |
| 13 | Before auditor | origin/main moved (#1750, #1752) | Merged before the audit; ledger auto-merged | lead.md §6.1 | Internal | merge commit |
| 14 | Audit 1, repair round 1 | Finding 1: trailing-comma removal drops a doc comment in the comma's leading trivia on the same line (Parse/Diagnose modes) | Restore the leading-trivia branch of `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia`; move it to *Checked and kept*; correct FMT-UNSUP-009 `Why:`; tests per removing transform | PR-introduced safety-rail defect, kept guard serves input outside the shape | Restores main's output (FMT-PIPE-002) | df4eb34 |
| 15 | Audit 1, repair round 1 | Finding 2a: RH5107 fix applied when a doc comment behind the comma sits in the next token's leading trivia | Withhold the fix for that input too (RH5107-004 applies regardless of which token carries the comment) | Rare class (RH5035); cheap trivia check | Withholding for a rare class is not in question | df4eb34 |
| 16 | Audit 1, repair round 1 | Finding 2b: FMT-BREAK-038 (and pre-existing FMT-BREAK-012) leave a `/** */`/`///` written behind a moved comma with the next element | Bring the code to the unchanged requirement: a comment behind the comma on its line moves with it whichever token's trivia holds it; fixed in the shared collapse helper, so argument/parameter lists (FMT-BREAK-012, pre-existing on main) are fixed too as an adjacent fix. If it needs more than a cheap trivia check, refuse the class instead and report | Requirement already states the placement; same helper | An unchanged requirement demands the output | superseded by 18 |
| 17 | Audit 1 hints | Dead zero-width loop in `PrecheckPhase.EnumerateGapTrivia`; FMT-BREAK-038 line-count wording | Implementer removes the loop if proven unreachable; spec author aligns wording to text lines | Cleanup in touched code | No visible change | loop kept: reachable for broken documents through FindUnsupportedSyntax in code fixes (16-implementer) |
| 18 | Repair round 1 (14-spec-author Q1) | Decision 16's target is unstable: the documentation-comment phase (owner of off-position `///`/`/** */` relocation) moves such a comment onto its own line, for comma-last code too | Option A: comma-first formats exactly like comma-last; FMT-BREAK-012/038's "comment behind the comma moves with it" covers ordinary comments (and every comment in None mode); documentation comments are placed by the documentation-comment phase. No src change for 16; doc-comment rows are characterization tests | Phase ownership (AGENTS.md table): the doc phase owns the relocation; B would make the line-break phase override it and change main's comma-last output | Output equals main's output for the comma-last form, which the doc phase already produces; rare class (doc comment inside a list) | 53c3d03 |
| 19 | Before audit 2 | origin/main moved (#1753), touching RH5301/5303/5304/5203 tests | Merged before the repair audit; test implementer resolved 4 conflicts keeping both sides (RH5203 keeps this run's deletion of a now-refused input); analyzer/formatter/architecture green | lead.md §6.1 | Internal | 569da61, 64dcc16 |
| 4 | Spec author | New requirement IDs vs. extending existing ones | New requirements (FMT-BREAK-038, FMT-INDENT-033) | Keeps existing requirements stable | Wording/IDs internal | pending |

## Follow-up candidates

- RH5107 fix delegates its layout to the formatter instead of a hand-written text edit (02-skeptic, #1737 Follow-up candidates).
- Comma that starts a line between switch-expression arms falls to the statement column — carries an output question (02-skeptic, #1738).
- Comma that starts a line in a positional pattern — same (02-skeptic, #1738).
- Comma that starts a line in a base list — same (02-skeptic, #1738).
- FMT-DOC-001 should state where a documentation comment behind a comma in an argument, parameter or initializer list ends up (on its own line); code does it, no requirement says so (14-spec-author).

## Audited SHA

- Audit 1: d1741804bd97da6e977138d52b67f995bc997266 (base main 3b3639e) → findings (13-auditor).

## Open worklist / next step

- Main merged (569da61). Next: auditor repair-only (21) on HEAD, then validator.
