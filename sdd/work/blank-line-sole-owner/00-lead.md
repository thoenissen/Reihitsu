Role: lead
Commit: 741157c9ca6fd4e16b144da4cbd7be2f354aec8b
Verdict: lane claimed

# Run: blank-line-sole-owner

## Issue

- thoenissen/Reihitsu#1496 — "FMT-BLANK: the blank-line phase is the only owner of blank lines". Labeled `agent-filed`: its goals are proposals, except where the maintainer's revision comment (below) decided them.
- Package C3 of thoenissen/Reihitsu#1482. Blockers C1 (thoenissen/Reihitsu#1490, merged via #1535) and C2 (thoenissen/Reihitsu#1491, merged via #1532) are closed. The issue was written against `735d0dba`; every statement about today's code must be checked against current `main` (741157c) first, and a statement an earlier package made true or moot is noted and dropped.

## Slug

`blank-line-sole-owner`

## Lane

Behavior change. Reason: the issue requests changed formatter behavior (block statement after `}` gains the SHARED-BLANK-003 blank line) and changed analyzer behavior (RH5025 exemption for the `{` of a block statement), plus a rule-page change; the issue requires a change doc.

Branch: `claude/hopeful-gauss-tkf765` (the session's designated branch, used instead of `claude/issue-1496-<slug>`; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1541.

## Roles

All roles of the behavior-change lane run, in order: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Issue manager before publishing if there are follow-up candidates.

## Maintainer clarifications (summarized)

The maintainer revised the issue on 2026-10-28 (issue comment):

- FMT-BLANK-008 and RH5025 do not apply to the `{` of **any** block statement, not only after `}`. After a statement ending in `}` the SHARED-BLANK-003 blank line remains required; after any other statement the author's blank line is kept (none is required) and RH5025 does not report it.
- RH5020 keeps reporting a comment glued to `{` (no exemption); such a comment is fixed by hand.
- The change doc names the superseded decisions: thoenissen/Reihitsu#1396 (RH5030 exempting a block statement) and the standalone-block case of thoenissen/Reihitsu#1081 item 1.

These decisions are binding for the run.

## Maintainer answers

Output-question batch before the test implementer (2026-11-01): the maintainer chose the recommended option for all seven.

1. Block statement after `break;` (also `break;⏎/* c */ {`) — A: blank line after `break;`.
2. RH5025: only a block statement that follows another statement of its block or switch section is exempt — A: first statement of a section, top-level and labeled blocks unchanged.
3. Own-line comment then blank line then block statement — A: the blank line above `{` is kept.
4. Own-line comment in front of a comment glued to a brace — A: blank line above the own-line comment, none above the glued one.
5. Directive in front of a glued comment — A: no blank line inserted above the directive; output equals input.
6. Block statement after `}` behind a directive — A: blank line after the directive.
7. Comment glued to a block statement's brace after `}` — A: SHARED-BLANK-003 blank line above the comment.

Output question during the implementer stage (2026-11-01), from 05-implementer.md D3: a non-region directive starts and a region directive ends the gap before a closing brace, with a blank line between them (`int x;⏎#pragma warning restore …⏎⏎#endregion⏎}`; also in a method body and with `#if … #endif`). The maintainer chose A: no blank line is inserted above the directive; output equals input. The existing test `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` gets the new expected output.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/hopeful-gauss-tkf765` instead of `claude/issue-1496-blank-line-sole-owner` | The session environment requires all pushes to go to this branch | Branch names are internal | claim commit |
| 2 | Skeptic | Architecture finding: the line-break phase also decides blank lines between an own-line comment and a following brace/bracket (`TokenGapNormalizer.NormalizeLeadingGap`, skeptic M1) | Change the structure in this run: the blank-line phase owns these blank lines, stated in FMT-BLANK-007/008/019 (skeptic I3), with tests; amended (03-spec-author) as the new requirement FMT-BLANK-020 scoped to FMT-BLANK-018's token list instead of edits to FMT-BLANK-007/008/019 | It is inside the run's own structural change; a narrow fix would keep the second owner the issue removes | Today's outputs for these cases are kept by the spec; any case that does change is asked as an output question | 0e69e32 |
| 3 | Skeptic | FMT-BLANK-018 token set too wide (skeptic M2) | Narrow it to the tokens where today's output has no blank line (skeptic I1) | Keeps today's output, as the issue requires for glued comments | The amendment restores today's output | 0e69e32 |
| 4 | Skeptic | FMT-BLANK-018/019 conflict with SHARED-BLANK-006 after `#endregion` and with `#endif` (skeptic M3) | Exclude those lines and correct the change doc's Precedence section (skeptic I2) | An unchanged shared requirement decides it; otherwise RH5032 would report formatter output | SHARED-BLANK-006 already demands today's output | 0e69e32 |
| 5 | Spec author | RH5025 rule-page goal of the issue | Drop it; the page is unchanged | The page describes the declaration case; the exemption is a corner case that rule pages do not list (AGENTS.md) | The rule page keeps its text | 5aa08e3 |
| 6 | Spec author | Precedence stated as one ranking requirement or via narrowed scopes | Narrowed scopes of FMT-BLANK-008/011, precedence stated once in the change doc | Avoids a second statement of each rule | Internal spec structure | 5aa08e3 |
| 7 | Spec author (amendment) | FMT-BLANK-020 as a separate requirement vs. edits to FMT-BLANK-007/008/019 | Accept FMT-BLANK-020 | Avoids repeating the token exceptions in three requirements | Spec structure only; FMT-BLANK-020 states today's output | 0e69e32 |
| 8 | Spec author (amendment) | FMT-BLANK-009 extended to `]` and `)` behind a directive | Accept | Writes down today's output under its new owner | Output unchanged | 0e69e32 |
| 9 | Implementer | D1: `PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective` formats a disabled `#if DEBUG` branch | Test wrong; route to test implementer (define the symbol or use an active branch), expected output unchanged | The test's intended input is an active branch; disabled text is never formatted | The asserted output is the one the maintainer answered (question 6) | pending |
| 10 | Implementer | D2: `SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection` expects a multi-line switch section without braces | Test wrong; route to test implementer to expect the braces the unchanged FMT-SWITCH requirements add | An unchanged requirement decides the braces; only the blank lines are this run's | Unchanged switch-brace requirement demands the braces | pending |
| 11 | Implementer | D4: FMT-BLANK-018 token list differs from today's output in three shapes (multi-line complex element `{`, assigned collection initializer `}`, documentation comment on the glued line) | Amend the spec to today's output, as implemented | Decision 3 and the change doc intend today's output | Today's output is kept | 1d6970e |
| 12 | Implementer | D5: FMT-BLANK-011's exemption should name the block-statement brace | Amend the spec | Removes a literal reading that contradicts FMT-BLANK-017 and the answered question 7/3 | Output follows the maintainer's answers | 1d6970e |
| 13 | Implementer | Code owners: `BraceBlankLinePolicy` new owner of FMT-BLANK; new public Core member `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` | Accept the scope growth; spec author adds the code owner; test implementer adds a direct Core test | One owner for the exemption shared by RH5025 and the formatter | Internal | pending |
| 14 | Implementer | Stale XML comment about `TokenGapNormalizer` in `BlankLineBeforeIfDirectiveAfterStatementTests.cs` | Adjacent fix: the cleaner corrects it | The run's change made it stale; a comment-only edit | Comment only | a98b099 |
| 15 | Spec author (06) | Owner of `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` | Keep it under SHARED-BLANK's existing ownership of `BlankLineSpacingPolicy`; no new requirement | A type owns one spec (`sdd/README.md`); RH5025-005 and FMT-BLANK-017 tests cover it, plus a direct Core test | Internal | 1d6970e |
| 16 | Spec author (06) | D3 also occurs before a collection expression's `]` | Open output question for the pre-auditor batch; tests do not pin that case until answered | The maintainer's answer named a closing brace only | Asked, not decided by the lead | — |

## Follow-up candidates

- Parenthesized pattern content loses its indentation (`if (o is (⏎1 or 2⏎))`) — 01-spec-author.md; the positional pattern `o is (1,⏎2⏎)` likewise — 02-skeptic.md. One issue.
- RH5026 reports the formatter's output for `// f⏎⏎finally` and `else` (FC-013) — 02-skeptic.md.
- Region directives inside collection expressions, patterns and initializers end up at the statement's column; no requirement decides the column — 02-skeptic.md.
- The blank line between an own-line comment and an array-initializer, extension-block or property-pattern `{` is kept today and RH5024/RH5025 report the formatter's output — 03-spec-author.md (may join the RH5026 candidate).
- `AGENTS.md`'s formatter phase-ownership table does not mention `BraceBlankLinePolicy` or FMT-BREAK-030 (tooling lane) — 05-implementer.md.
