# Lead — field-split-guards

- Issues: thoenissen/Reihitsu#963 (`agent-filed`), thoenissen/Reihitsu#1384 (`agent-filed`) — goals are proposals.
- Slug: `field-split-guards`
- Branch: `claude/magical-albattani-jl4e5j` (session-designated branch; see decision 1). Draft PR: thoenissen/Reihitsu#1660.
- Lane: **bug fix**.
  - #963: after triage (issue comment of 2026-10-24) the issue keeps only the field-splitting part: a comment between the modifiers/type (also attribute list, per the 2026-10-21 comment) and the first declarator must block the split instead of being copied onto every field. FMT-STRUCT-024 (P36) already requires it; the `{// why` expression-body part moved to #1439 and is out of scope.
  - #1384: a documented field declaration (`///` or `/** */`) must not be split. Triage: already required by FMT-STRUCT-024 on main → bug fix without change doc; FMT-STRUCT-022's contradicting example is aligned as a clarification. The RH7101 fix offer follows `SplitsFieldDeclaration` (FMT-PIPE-031), so RH7101 tests change with it. A code-fix regression test (e.g. RH7103 on a documented `public int X, Y;`) was requested in an issue comment.
- Roles: spec author (clarification of FMT-STRUCT-022 and any spec text that disagrees with FMT-STRUCT-024) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager only if follow-up candidates exist.

## Maintainer answers

- (from issue thread, before the run) #1384: FMT-STRUCT-024 wins over FMT-STRUCT-022's example — a documented field declaration is not split; FMT-STRUCT-022's example is aligned in the same bug run, and the RH7101 tests change with it.

- Output question 1 (#963, comment on its own line in front of the first declarator, i.e. in the first declarator's leading trivia): **A** — split as today; the comment stays in front of the first field only; the RH7101 fix is offered. FMT-STRUCT-030 and RH7101-007 must name this slot explicitly on the split side.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/magical-albattani-jl4e5j` instead of `claude/issues-963-1384-<slug>` | The session's environment forbids pushing to any other branch | Branch names are internal | claim commit |
| 3 | Spec author | Own-line comment between attribute list and modifiers (`[Obsolete]⏎// why⏎private int _a, _b;`) blocks the split | Accept the spec author's reading; no output question | FMT-STRUCT-024 (P36) forbids a structural rewrite that moves or duplicates a comment; today's output copies the comment onto every field, which that unchanged requirement rules out, and #963's comment of 2026-10-21 lists the attribute-list twin | An unchanged requirement decides the output | 41f8fc4 |
| 4 | Spec author | Rule page RH7101.md not changed for the withheld fix (RH7101-007) | Accept | `general-notes.md` covers diagnostics without a fix (AGENTS.md, Code-fix coverage) | Rule page unchanged | 41f8fc4 |
| 5 | Skeptic | Architecture finding (FC-015): RH7101 fix's `TryGetFixableFieldDeclaration` keeps its own documentation-comment check once `Splits` holds the guard | Change the structure in this run: the fix relies on `SplitsFieldDeclaration` alone; remove the copy and correct its summary | A narrow fix would leave a second statement of the rule; the change stays in the counterpart of the changed component, and FMT-PIPE-031's Why already demands it | RH7101 fix offer is unchanged for documented declarations (still withheld, now via the formatter predicate) | — |
| 6 | Skeptic | Follow-up candidate: RH7103 and other type-formatting fixes reformat untouched members | Drop | RH7103's spec lists that layout as a non-goal; no defect against a spec | Nothing changes | — |
| 7 | Skeptic | Issue goals: #963 expression-body goals; #1384 change-doc goal | Drop both (moved to #1439; triage put #1384 in the bug-fix lane) | Triage decisions on the issues | Nothing visible dropped in this run | — |
| 8 | Test implementer | #1384 expects the `///` instance "unchanged"; tests expect it unsplit with the one-line `<summary>` expanded | Accept | The expansion is the documentation-comment phase's existing output with or without a split (FMT-STRUCT-030's first example); the issue's purpose is the refused split | The expansion is today's unchanged output for an unsplit declaration | eb5c766 |
| 9 | Test implementer | 9 FMT-PIPE-029 tests with a documented target deleted | Accept | No caller reaches a documented target once the split is refused (AGENTS.md, Supported inputs) | The entry point's caller (RH7101 fix) is withheld for those inputs | eb5c766 |
| 10 | Implementer | Disputed test `FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` fails on markup handling, not on asserted output | Route to a fresh test implementer to fix the test's markup configuration; expected output stays | Test-harness configuration is internal; the asserted output follows RH7101-007 | Asserted output unchanged | — |
| 2 | Lane | Lane for both issues | Bug fix; spec author runs for a clarification only | FMT-STRUCT-024 already demands both guards; triage recorded the lane | Unchanged requirement decides the output | — |

## Follow-up candidates

- Line break between the type and the first declarator: a split gives `private int⏎_second;` with no continuation indent (`private int⏎        _g, _h;` → `private int⏎    _g;⏎    private int⏎    _h;`); whether that layout is intended is a formatter question. Source: 02-skeptic.md (#963, Follow-up candidates).

## Worklist / next step

- Spec author done (01-spec-author.md, 41f8fc4): FMT-STRUCT-030, RH7101-007 added; FMT-STRUCT-022/024, FMT-PIPE-029/031, RH7101-004/006 aligned. No output questions.
- Skeptic done (02-skeptic.md, bd5c57c): both reproduced and reachable. Output question 1 (comment on own line in front of first declarator) asked; answered A.
- Spec author amendment done (03-spec-author.md, 5af2226).
- Test implementer done (04-test-implementer.md, eb5c766): both reproduced, 59 failing tests.
- Implementer done (05-implementer.md, 1ac2927 production, 74e8fb1): all pass except one disputed test (markup handling).
- Next: fresh test implementer (06) for the disputed test; then cleaner.
