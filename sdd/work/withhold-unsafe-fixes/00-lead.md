Role: Lead
Commit: 735d0db
Verdict: lane claimed — bug fix

# Run

- Issues, in prompt order: thoenissen/Reihitsu#1136, #1134, #1135, #1262, #1332, #1336, #1340.
- Slug: `withhold-unsafe-fixes`.
- Branch: `claude/sweet-johnson-msb5hu`. The session environment fixes this branch name and forbids pushing to any other branch, so the run does not use the `claude/batch-1136-<slug>` name of `squad/lead.md`, section 2.
- Draft PR: thoenissen/Reihitsu#1481.
- Blockers named by the issues are closed: #1138 (PR #1357), #1260 (PR #1359), #1415 (PR #1418).

# Lane: bug fix

- #1136 — spec clarification of SHARED-FIX-003 (relocating or reordering comments and directives; dropping, editing, or relocating disabled text) with covering tests. On its own it would be the tests lane; the maintainer's triage places it as a clarification without change doc in the same run as #1332 and #1336, and the tests lane widens to bug fix.
- #1134 — RH7109 fix reorders fields of sequential-layout types: observed versus expected, bug fix.
- #1135 — RH7309 fix changes static initializer execution order: bug fix.
- #1262 — region fixes (RH7303, RH7304, RH7306, RH7309) that break code or separate a declaration from its documentation: bug fix; triage states no change doc, RH7309-004 aligned as a clarification.
- #1332 — RH7501 fix withheld when it breaks code, moves directives, or rearranges shared lines: bug fix; RH7501-004 gains the "withheld otherwise" sentence as a clarification.
- #1336 — RH7004, RH7101, RH7105, RH7106 fixes: bug fix; RH7101-004 changes as a clarification.
- #1340 — RH7003 reports a namespace containing a namespace: triage states the behavior is already required by RH7003-001 on main, so bug fix without change doc; the rule's own spec text and page are aligned as a clarification.

Roles run: spec author (clarifications of SHARED-FIX-003, RH7309-004, RH7501-004, RH7101-004, RH7003-001/-002, and the rule page of RH7003), skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator. Roles skipped: none.

# Maintainer clarifications (summarized)

- #1136: runs as a clarification without change doc; fixes that relocate comments or directives are bugs, fixed in this run for the cases #1332 and #1336 name. A directive pair split across a brace (one directive moved, its partner left behind) counts as reordering a directive.
- #1262: bug-fix lane, no change doc; RH7309-004 is aligned to SHARED-FIX-003 as a clarification. Additional instances from later comments belong to the issue: RH7304 analyzer pairing like the compiler for a pair with a directive in executable code; RH7306 fix withheld when nothing remains of the name; RH7309 fix withheld when a comment stands directly above a region block; RH7309 fix withheld in documentation mode `Parse`/`Diagnose` when it would separate a declaration from its `///` (three shapes listed on the issue), unchanged in mode `None`.
- #1332: the blank-line part (former Q10) moved to #1437 and is not part of this run. The empty block that remains after the fix is kept (Q9).
- #1336: an additional instance — RH7101 in mode `Parse` on a multi-declarator field with a `///` or `/** */` comment: fix withheld, diagnostic stays; RH7101-004 changes with this issue.
- #1340: bug-fix lane, no change doc; where the rule's spec text disagrees, the same PR aligns it.

# Lane widened: behavior change

After the spec author's repair round, `scripts/lane-check.sh bug-fix` failed because the run's decisions entry lies under `squad/` (tooling). The maintainer reclassified the run as a behavior change and allowed change docs (`squad/decisions/2026-10-27-withhold-unsafe-fixes-lane.md`), before the implementer started. The run now claims the behavior-change lane; the spec author writes the change doc, and the maintainer approves its What and Why before the test implementer starts (escalation 2). Roles run: all eight, as before. The maintainer asked the lead to pause after every role because of the run's budget.

# Issue status

All seven issues carry the `agent-filed` label (checked 2026-11-05): their goals are proposals, not binding (`squad/README.md`, *Issues are hypotheses*). The lead decides a goal where nothing visible is in question; the maintainer answers below stay binding.

# Maintainer answers

The process on `main` moved escalation answers from `squad/decisions/` into this file; the two decisions entries this run had written are removed and their content is kept here.

1. After the skeptic (2026-10-27), the lead's recommendation on every question:
   - RH7306 analyzer and fix judge different text (architecture finding): fix narrowly — the fix judges the description through one shared predicate, no private copy.
   - RH7309, layout-sequential struct: backing fields of auto-properties (also `field`-backed) and of instance field-like events count as instance fields; the fix is withheld.
   - RH7303, mirror of the straddling region (`#region` in a body, `#endregion` at type level): the fix is withheld like the decided shape.
   - RH7309, regions that `RegionDirectiveUtilities.GetTopLevelRegions` counts although they belong to a nested enum or an expression: out of this run; follow-up for the helper and the SHARED-TYPEREGION-005 gap.
   - RH7309, comment above a block that moves, with blank lines before its `#region`: fix withheld.
   - RH7309, comment directly above a block that keeps its position: fix offered.
   - RH7306, the `#endregion` partner would be left without description: fix withheld.
   - RH7105/RH7106, comment between the last modifier and the next token: fix withheld, as for directives.
   The maintainer also asked the lead to pause after every role because of the run's budget.
2. Lane (2026-10-27): the maintainer reclassified the run as a behavior change and allowed change docs, overriding the triage notes on #1262 and #1340 that ruled one out. The run note under `squad/notes/` is classified as tooling, so the behavior-change lane stays required.
3. Change docs (2026-11-05): the maintainer approved the visible output changes of `withhold-unsafe-fixes` and `silence-uncorrectable-namespaces-and-regions` as of commit c2e1f67, after `main` was merged and the plan re-checked.
4. RH7101 output question (2026-11-05, 05-spec-author Q1): a `/** */` or `///` comment that starts behind code on the line and stands in the leading trivia of a combined field declaration documents the declaration; in documentation mode `Parse`/`Diagnose` the fix is withheld (option A). The four tests of main's accepted case 7 change their expectation or move to mode `None`.
5. Pausing (2026-11-06): the maintainer withdrew the request to pause after every role; the run continues without stops except for output questions and escalations.

# Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | `claude/sweet-johnson-msb5hu` instead of `claude/batch-1136-<slug>` | The session environment allows pushes to this branch only | Branch names are internal | d72bbdc |
| 2 | Skeptic | SHARED-FIX-003 brace-split sentence wider than the maintainer's clarification | Narrowed by the spec author | The maintainer's clarification already decides it | It restores fixes the maintainer did not ask to withhold | c37e8b7 |
| 3 | After skeptic | Second skeptic pass over the change docs | Skipped | The change docs restate the spec diff the skeptic already checked; budget | No spec text changed | c2e1f67 |
| 4 | Test implementer | #1336 B3 (`namespace N { int a, b; }`, fix changes nothing) does not reproduce on bd9283a: main's RH7101 change already withholds the fix | Keep the tests as SHARED-FIX-005 characterization; #1336 stays `Closes`, since every other instance reproduced; the PR states that B3 was already fixed on `main` | The issue's other instances are fixed by this run | The output asserted is today's output on `main` | 0e8d0a7 |
| 5 | Implementer | New public Core helpers (`SyntaxTriviaUtilities.SplitsDirectivePair`, `OrderingDeclarationUtilities.FieldOrderSetsLayout`/`IsLayoutField`/`ChangesLayoutOrder`, `HasInitializer` made public) and a new `RegionBlockLayout` | Accepted as in scope: 15 production files against 13 named, under double; each helper gives one owner to a decision RH7109 and RH7309 share, as the spec author's note asked | Avoids a private copy per fix (FC-015) | Internal structure only; the visible changes are the accepted change docs' | 1d19e02 |
| 6 | Audit 1 | Architecture finding: initializer-order (and layout-order) decision owned by both `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder`/`ChangesLayoutOrder` and `RegionBlockLayout.ChangesMemberOrder`, disagreeing on members in disabled text | Change the structure in this run: one owner of the member-order decision that counts members of disabled text, used by the single move (RH7103, RH7109) and the block permutation (RH7309); RH7103 is taken in as an adjacent fix with a test | Both components are the run's own; a narrow fix would keep two owners (FC-015) | The run's accepted SHARED-FIX-003 *Build configurations* bullet demands the withheld fix for RH7103 and RH7109 alike | (repair 1) |
| 7 | Audit 1 | Findings #5 and #6: comment positions in the RH7004 fix not pinned | Restore `main`'s output for a comment that does not end its line (#5); pin the full output for the two comment inputs (#6); no output question | #5: `main`'s output; #6: SHARED-FIX-003 condition 3 demands the kept comment, and its layout is the formatter's | Both outputs are `main`'s or demanded by an accepted requirement | (repair 1) |
| 8 | Audit 1 | Auditor hint: `HasBackingField` counts an identifier named `field` as the keyword before C# 14 | Taken into the repair (same requirement as #2) | Same guard, over-withholding | Restores fixes no requirement withholds | (repair 1) |
| 9 | Repair 1 tests | The new SHARED-ORDER-007 positive example keeps an expression-bodied `Log` unchanged, which no RH7103/RH7109 fix output meets because both format the whole type (FC-030, #969) | The spec author rewrites the example with a block-bodied member so a test can mirror it; the whole-type formatting stays with #969 | A spec example must be an output the code produces | The guard's verdict in the example is unchanged; only the incidental member shape changes | (repair 1) |
| 10 | Repair 1 implementer | Candidate: `SplitsDirectivePair` pairs two `disable X` (or two `restore X`) directives | Dropped, not a follow-up: the implementer states that no caller input reaches it today (`AGENTS.md`, *Supported inputs*) | No caller reaches the case | Nothing changes | 0684485 |
| 11 | Repair 1 spec checker | The change doc does not list the RH7103/RH7109 fix withheld when a move crosses an initialized member of excluded code | No new output question: the approved SHARED-FIX-003 *Build configurations* bullet demands the withheld fix; the spec author adds the case to the change doc (affected requirements SHARED-ORDER-007, RH7103-007; rows and a before/after) and records why it is covered | The maintainer approved the bullet that decides it | Covered by the maintainer's approval of the bullet (Maintainer answers 3) | (repair 1) |
| 12 | Audit 2 | Finding #9 repeats #4's class (architecture signal): each caller of `ChangesMemberOrder` still decides whether the layout check applies; RH7103 passes `false` | Structural change, not a repair round: the shared owner derives the layout check from the type symbol itself, so no caller can opt out; RH7103 gets a test and the change doc lists the newly withheld fix | Repairing one member per round would only find the next; one owner closes the class | The withheld fix is demanded by the approved SHARED-FIX-003 *Build configurations* bullet (as decision 11) | (structural change) |
| 13 | Audit 2 | Findings #10 (excluded members in the moved member's leading trivia cross what it crosses; SHARED-ORDER-007 and change-doc case 10 say otherwise) and #11 (pairing tests) | Repair round 2: spec author corrects SHARED-ORDER-007 and case 10; test implementer adds the tests; no output question | #10's withheld fix is demanded by the approved *Build configurations* bullet | Covered by Maintainer answers 3, as decision 11 | (repair 2) |
| 14 | Repair 2 spec author | Where the excluded-code layout counting lives; SHARED-ORDER-007's title; case numbering | Accepted as proposed: inside SHARED-ORDER-007 (a new requirement would restate SHARED-FIX-003), title kept, case 10 extended | Fewer statements of one rule | Spec structure only | bf155df |

# Follow-up candidates

- `RegionDirectiveUtilities.GetTopLevelRegions` counts regions of a nested enum and regions inside expressions as the type's own; SHARED-TYPEREGION-005 does not classify an equals-value clause, an arrow expression, an attribute argument, or a parameter default (02-skeptic, answer 4C).
- Audit every fix outside this run against the new SHARED-FIX-003 placement condition (02-skeptic).
- RH7105/RH7106 fix formats the whole member, against SHARED-FIX-006 (02-skeptic).
- RH7003 reports a nested namespace that sits only in an excluded `#if` branch (02-skeptic).
- RH7003 rule page carries an "Exceptions" section the rule-page conventions do not allow (01-spec-author).
- RH7004 fix adds blank lines around an `#endregion` that stands in front of the using declaration, against SHARED-FIX-006 (06-test-implementer).
- RH7309 fix is offered and changes nothing when the first member of the reported block is a nested type (FC-009, SHARED-FIX-005) (06-test-implementer).
- RH7309 fix is offered for an order that is already correct and changes nothing (SHARED-FIX-005) (07-implementer).
- RH7309 parses member-level disabled text on its own, so a member an `#if` splits between active and disabled text is not counted (07-implementer).
- RH7304 still reports a directive whose region's first token does not start its line, a report RH7304-001 does not claim (07-implementer).
- On `main`, the RH7004 fix for a `#nullable enable` among the following statements inserts a blank line and drifts line endings under CRLF (10-auditor hint).
- RH7102 has no member-order guard: a `const` written after `#if DEBUG` instance fields, or after initialized fields, reorders them in a `DEBUG` build; same on `main` (18-auditor hint).
