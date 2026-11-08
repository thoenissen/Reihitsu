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

- Output question 2 (#963, `///` or `/** */` behind the type on the type's line, Parse/Diagnose — Roslyn attaches it to the first declarator's leading trivia): **B** — split as on main; the comment stays in front of the first field only; the RH7101 fix is offered. FMT-STRUCT-030, FMT-PIPE-031 and RH7101-007 must put this case on the split side explicitly; the guard's code behavior stays.

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
| 10 | Implementer | Disputed test `FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` fails on markup handling, not on asserted output | Route to a fresh test implementer to fix the test's markup configuration; expected output stays | Test-harness configuration is internal; the asserted output follows RH7101-007 | Asserted output unchanged | 0a31af4 |
| 11 | Auditor 09 | Hint: stale comment in `CreateSplitField` | Route to the cleaner in repair round 1 | Comment the run's change made stale | Comment only | d093595 |
| 12 | Auditor 09 | Hints: wrapped declarator list continuation at member column; doc phase moving a doc-style comment that documents nothing onto its own line | Follow-up candidates (widen the existing line-break candidate; add the doc-phase one) | Pre-existing, outside both issues | Not changed in this run | — |
| 13 | Repair 1, spec author | Example output keeps the doc phase's blank line and moved comment (`private int⏎⏎    /** c */ _first;`) | Accept; FMT-STRUCT-030 states the placement is the documentation-comment phase's | The maintainer chose B from exactly this output; the placement is a follow-up candidate | Covered by answer B | 4672256 |
| 14 | Auditor 15 | Finding #2 (ordinary comment behind a `/** */` on the type's line is owned by the first declarator, split goes ahead against the amended spec) repeats #1's class | Architecture finding: change the structure in this run, not a repair round. The spec states the boundary once, in the form the code uses (trivia ownership: the part every field repeats is the trivia of the tokens before the first declarator, except the declaration's own leading trivia; the first declarator's leading trivia does not block, and the spec explains which comments that is — every comment from the first `///` or `/** */` behind the type in Parse/Diagnose, and every comment starting on a later line). FMT-PIPE-031, RH7101-007, FMT-STRUCT-022 refer to it instead of repeating the list; code comments point to it. Tests pin the new members on the split side. | One enumeration already missed a member; a further list would miss the next. The code's ownership boundary is idempotent, a literal comment-kind boundary is not | Output for the new members (`private int /** c */ /* d */ _first, _second;` etc.) is today's main output, unchanged by the run; it follows answer B's reasoning (comment attached to the first field, nothing copied). No output changes | — |
| 2 | Lane | Lane for both issues | Bug fix; spec author runs for a clarification only | FMT-STRUCT-024 already demands both guards; triage recorded the lane | Unchanged requirement decides the output | — |

## Follow-up candidates

- Line break between the type and the first declarator: a split gives `private int⏎_second;` with no continuation indent (`private int⏎        _g, _h;` → `private int⏎    _g;⏎    private int⏎    _h;`); whether that layout is intended is a formatter question. Source: 02-skeptic.md (#963, Follow-up candidates). Widened by 09-auditor.md: a wrapped declarator list that stays whole (now reached by documented and comment-guarded fields) puts its continuation line at the member column (`public int X,⏎    Y;`); locals do the same on main.
- The documentation-comment phase moves a doc-style comment (`///`, `/** */`) that documents nothing onto its own line with a blank line in front, even inside a declaration (`private int /** c */ _first, _second;` → `private int⏎⏎/** c */ …`). Source: 09-auditor.md (hints).

## Worklist / next step

- Spec author done (01-spec-author.md, 41f8fc4): FMT-STRUCT-030, RH7101-007 added; FMT-STRUCT-022/024, FMT-PIPE-029/031, RH7101-004/006 aligned. No output questions.
- Skeptic done (02-skeptic.md, bd5c57c): both reproduced and reachable. Output question 1 (comment on own line in front of first declarator) asked; answered A.
- Spec author amendment done (03-spec-author.md, 5af2226).
- Test implementer done (04-test-implementer.md, eb5c766): both reproduced, 59 failing tests.
- Implementer done (05-implementer.md, 1ac2927 production, 74e8fb1): all pass except one disputed test (markup handling).
- Test implementer 06 (0a31af4): disputed test corrected; RH7101 104/104 pass.
- Cleaner 07 (8472a86): clean, Release 0 warnings.
- Spec checker 08 (b6a5f22): consistent, no findings.
- Auditor 09 (b0ec5cf) audited 4ad21a7 against main 026fb64: 1 high finding (#963): `///`/`/** */` behind the type on its line (Parse/Diagnose) is owned by the first declarator's leading trivia, so the split goes ahead against FMT-STRUCT-030 bullet 2 / RH7101-007; literal repair is not idempotent. Output question 2 answered B.
- Spec author 10 (4672256) done. Test implementer 11 (7370231) done: split-side tests green, shown to fail under the literal-spec guard. Implementer 12 (9662f22): summaries only, text-only proven. Cleaner 13 (d093595): clean. Spec checker 14 (d1bb028): consistent.
- Auditor 15 (a00947d) audited f133f2f: #1 closed; #2 repeats #1's class → architecture finding (decision 14). Repair rounds used: 1.
- Structural change worklist: spec author 16 (single ownership statement), test implementer 17 (pin new members, formatter + RH7101), cleaner 18 (code comments point to the statement; format; build), spec checker 19, auditor 20, validator 21.
- Repair round 1 worklist: spec author (10) amends FMT-STRUCT-030 / FMT-PIPE-031 / RH7101-007 per answer B; test implementer (11) pins the case on the split side (formatter + RH7101 fix offered); implementer (12) aligns the summaries of `CarriesCommentInRepeatedPart` and `SplitsFieldDeclaration` with the boundary; cleaner (13) fixes the stale `CreateSplitField` comment (decision 11); then spec checker, auditor on the repair only, validator.
