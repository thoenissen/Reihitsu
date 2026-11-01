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

Pre-auditor output question (2026-11-01), from Decisions taken row 16 and 09-spec-checker.md finding 1: the same directive-before-`#endregion` gap before a collection expression's `]` (and through the same rule the `)` of a parenthesized pattern). The maintainer chose A: no blank line is inserted above the directive, as for a closing brace.

Pre-auditor-repair output questions (2026-11-01), from 14-auditor.md findings 1 and 3:
1. Comment glued to the `{` of a complex element initializer — A: every complex element `{` is treated like the other initializer braces (no blank line above the glued comment; an author's blank line there is removed), independent of whether the element spans lines.
2. Own-line comment plus blank line above the `{` of a collection initializer assigned to a member — A: the blank line is removed, as for an assigned object initializer.

Maintainer clarification of the guidance (2026-11-01, supersedes its first wording): unwanted are comments that share a line with code, and comments around the `=` of an assignment (between `=` and the assigned value, e.g. `L =⏎// c⏎{`). Comments belong only where they do not affect indentation, and analyzer rules should check that. For such unwanted comments the formatter must not be made more complex to give them a particular layout. The first wording follows for reference: comments glued to braces are unwanted code; the formatter must not be made more complex to give them a particular layout. Where a rule or exception exists only to keep a specific output for such comments, prefer the simplest uniform rule, and an output change for glued comments that follows from the simpler rule is covered by this guidance (list it in the change doc).

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
| 9 | Implementer | D1: `PlacesBlankLineBelowDirectiveAndKeepsAuthorBlankLineBelowDirective` formats a disabled `#if DEBUG` branch | Test wrong; route to test implementer (define the symbol or use an active branch), expected output unchanged | The test's intended input is an active branch; disabled text is never formatted | The asserted output is the one the maintainer answered (question 6) | 6f38281 |
| 10 | Implementer | D2: `SeparatesBlockStatementsThatFollowAStatementInsideSwitchSection` expects a multi-line switch section without braces | Test wrong; route to test implementer to expect the braces the unchanged FMT-SWITCH requirements add | An unchanged requirement decides the braces; only the blank lines are this run's | Unchanged switch-brace requirement demands the braces | 6f38281 |
| 11 | Implementer | D4: FMT-BLANK-018 token list differs from today's output in three shapes (multi-line complex element `{`, assigned collection initializer `}`, documentation comment on the glued line) | Amend the spec to today's output, as implemented | Decision 3 and the change doc intend today's output | Today's output is kept | 1d6970e |
| 12 | Implementer | D5: FMT-BLANK-011's exemption should name the block-statement brace | Amend the spec | Removes a literal reading that contradicts FMT-BLANK-017 and the answered question 7/3 | Output follows the maintainer's answers | 1d6970e |
| 13 | Implementer | Code owners: `BraceBlankLinePolicy` new owner of FMT-BLANK; new public Core member `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` | Accept the scope growth; spec author adds the code owner; test implementer adds a direct Core test | One owner for the exemption shared by RH5025 and the formatter | Internal | 1d6970e, 6f38281 |
| 14 | Implementer | Stale XML comment about `TokenGapNormalizer` in `BlankLineBeforeIfDirectiveAfterStatementTests.cs` | Adjacent fix: the cleaner corrects it | The run's change made it stale; a comment-only edit | Comment only | a98b099 |
| 15 | Spec author (06) | Owner of `BlankLineSpacingPolicy.IsBlockStatementAfterStatement` | Keep it under SHARED-BLANK's existing ownership of `BlankLineSpacingPolicy`; no new requirement | A type owns one spec (`sdd/README.md`); RH5025-005 and FMT-BLANK-017 tests cover it, plus a direct Core test | Internal | 1d6970e |
| 16 | Spec author (06) | D3 also occurs before a collection expression's `]` | Open output question for the pre-auditor batch; tests do not pin that case until answered | The maintainer's answer named a closing brace only | Asked; maintainer answered A (see Maintainer answers) | repair round 1 |
| 17 | Auditor (14) | Architecture finding: `BraceBlankLinePolicy` copies the line-break phase's multi-line complex-element condition (finding 1) | Change the structure in this run: drop the layout qualifier, per maintainer answer 1A; no blank-line decision reads a layout a later phase changes | Run-introduced copy inside the run's components; the purpose of the issue is one owner per decision | Answered by the maintainer (1A) | 1e9c3d6 |
| 18 | Auditor (14) | Finding 2: FMT-BLANK-020 trigger narrower than `main` (two comments on the comment line) | Repair: widen to a line holding only ordinary comments, in the simplest form | PR-introduced; restores `main`'s output; RH5024/RH5025 would report formatter output otherwise | Restores `main`'s output | 1e9c3d6 |
| 19 | Auditor (14) | Finding 4: two private copies of the "glued comment" predicate | Repair: one predicate in `BraceBlankLinePolicy` (or `TokenGapAnalysis`), both rewriters read it | One owner; simplifies, as the maintainer's guidance asks | Internal | 1e9c3d6 |
| 20 | Auditor (14) | Finding 5: stale `RootConstruct` summaries | Repair: name the blank-line phase reader | Comment accuracy | Comment only | 1e9c3d6 |
| 21 | Maintainer guidance | Other glued-comment special cases from D4 (assigned collection initializer `}` exclusion, documentation-comment condition) | Spec author and implementer drop each special case that exists only to keep a specific glued-comment output, in favor of the simplest uniform rule; the change doc lists the resulting glued-comment output changes | Maintainer guidance: no formatter complexity for unwanted glued comments | Covered by the maintainer's guidance; listed in the change doc and the PR | 60a9050 |
| 22 | Spec author (15) | Change-doc entry 18: own-line comment + blank line before a one-line complex element `{` loses the blank line | Covered by maintainer answer 1A ("every complex element `{` treated like the other initializer braces", which includes FMT-BLANK-020); change doc back to Accepted, entry 18 marked decided by 1A; named in the PR | 1A's option text stated the alignment with the other initializer braces; RH5025 reports the blank line B would keep | Covered by answer 1A | f97fbbf |
| 23 | Spec author (15) | FMT-BLANK-020's remaining exclusions (array, extension-block, interpolation braces, property-pattern `{`, assigned collection initializer `}`) | Keep them in this run; they concern own-line comments, outside the guidance; joined to the RH5024/RH5025 follow-up candidate | Not covered by the guidance; changing them is an unasked output change | Today's output kept | 60a9050 |
| 24 | Test implementer (16) | Conflict A: entry 13 / FMT-BLANK-020 example show `L =⏎// c⏎{` without the FMT-BLANK-011 blank line above `// c`; today's output comes from a phase reversal | FMT-BLANK-011 applies uniformly: `L =⏎⏎// c⏎{`; correct entry 13 and the example; no exemption for a comment after `=` | Maintainer guidance: comments around `=` are unwanted and get no extra formatter logic; matches the assigned object initializer (`Child =⏎⏎// note`); keeps FMT-BREAK-030 | Covered by the maintainer's guidance; answer 2A (blank line between comment and `{` removed) still holds | e985d4d |
| 25 | Test implementer (16) | Conflict B: FMT-SWITCH-007 example shows a blank line above `/* b1 */ /* b2 */ break;`, which FMT-BLANK-018 now decides as glued to `break` | Update the FMT-SWITCH-007 example to FMT-BLANK-018's output and list the change in the change doc | Comment on a line with code: covered by the guidance; one rule, not an exception | Covered by the maintainer's guidance | e985d4d |
| 26 | Test implementer (16) | FMT-BLANK-020 names the parenthesized-pattern `)`, changing `// c⏎⏎)` from `main`'s output (kept), which no change-doc entry or answer lists | Spec author checks; unless an answered change-doc entry already covers it, `)` keeps `main`'s output and the four tests follow | An own-line comment, outside the guidance; no unasked visible change | `main`'s output kept (spec author 17: FMT-BLANK-020 restores it; no spec change) | e985d4d |
| 27 | Implementer (19) | D1: `RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringArgumentOnCommentLedLine` expects a blank line above `/* c */ """`, a comment glued to a raw-string argument | Test wrong under FMT-BLANK-018 (entry 17); test implementer removes that one blank line | Comment on a line with code; covered by the guidance | Covered by the maintainer's guidance (change-doc entry 17) | d744bbb |
| 28 | Implementer (19) | D2: `switch-case-braces.md` mentions FMT-BLANK-018 without a Uses entry; two readings of FMT-BLANK-018 (multi-line comment ending on the token's line is not glued; end-of-file token has no glued comment) | Spec author fixes the Uses row and writes both readings down | Spec defect; the readings state today's behavior | Output unchanged | 2c3fe1c |
| 29 | Implementer (19) | FMT-BLANK-008 cleanup now applies only to a do statement's `while` | Accept; corrected after audit 24 (#7): `#pragma …⏎⏎while (…)` now keeps its blank line where `main` removed it, as the unchanged FMT-BLANK-008/009 require; tests added and the change doc lists it | Matches the spec | An unchanged requirement demands the output | 1e9c3d6 |
| 30 | Auditor (24) | #6 (repeats #2's class → architecture finding): the comment-position predicates of FMT-BLANK-011/018/020 do not cover every comment shape; a delimited comment that starts on an earlier line and ends on the token's line is neither glued nor own-line | Change the structure in this run: one exhaustive classification of a comment's position relative to the following token, read by FMT-BLANK-011, -018 and -020, so every comment shape falls into exactly one class. A comment any part of which shares the token's line is on a line with code, i.e. glued (reverses the second reading of row 28) | Repeated class in an audit of this run; the guidance says such comments are unwanted and get no extra formatter logic, and treating them as glued gives `main`'s output before braces, brackets and pattern parentheses | Before those tokens it restores `main`'s output; elsewhere the change is a comment on a line with code, covered by the maintainer's guidance; listed in the change doc | 1c0d67d |
| 31 | Auditor (24) | #8: the assigned collection initializer's `{` guard keeps the blank line in `L =⏎⏎#pragma …⏎{` where `main` removed it | Accept the output (FMT-BLANK-009 and FMT-BREAK-030 require it); add a test and list it in the change doc | An unchanged requirement decides it | An unchanged requirement demands the output | 1c0d67d |
| 32 | Auditor (24) | #9: `TokenGapNormalizer.FindLineStartIndex`/`IsOrdinaryComment` duplicate helpers now public in `BraceBlankLinePolicy` | Repair: one shared trivia utility, both phases read it | One owner | Internal | pending |
| 33 | Auditor (24) | Hint: the change doc's sentence that RH5020 keeps reporting a comment glued to `{` | Spec author widens it to every glued comment | Accuracy | Change-doc text only | 1c0d67d |
| 34 | Auditor (24) | Round accounting | Rows 30–33 and #7 form repair round 3, the last; the structural change of row 30 does not use up a round | `squad/README.md`, Repairs and Architecture findings | — | — |
| 35 | Spec author (25) | Chain link behind a multi-line `*/` (`.C()⏎/* c⏎ */ .D()`): the literal row-30 reading is not idempotent because FMT-BREAK-019 moves the link | Accept the spec author's exception: where FMT-BREAK-019 breaks the line in front of the token, that line's comments count as on their own line. Condition: the decision is ONE predicate, owned where the chain rewriter decides the break and read by both phases, computed from syntax facts no phase in between changes — no copy and no prediction of the line-break output | Keeps `main`'s output and idempotency; a shared fact with one owner is not a second owner | `main`'s output kept | pending |

## Follow-up candidates

- Parenthesized pattern content loses its indentation (`if (o is (⏎1 or 2⏎))`) — 01-spec-author.md; the positional pattern `o is (1,⏎2⏎)` likewise — 02-skeptic.md. One issue.
- RH5026 reports the formatter's output for `// f⏎⏎finally` and `else` (FC-013) — 02-skeptic.md.
- Region directives inside collection expressions, patterns and initializers end up at the statement's column; no requirement decides the column — 02-skeptic.md.
- The blank line between an own-line comment and an array-initializer, extension-block or property-pattern `{` is kept today and RH5024/RH5025 report the formatter's output — 03-spec-author.md (may join the RH5026 candidate).
- `AGENTS.md`'s formatter phase-ownership table does not mention `BraceBlankLinePolicy` or FMT-BREAK-030 (tooling lane) — 05-implementer.md.
- Analyzer coverage for unwanted comment placement: comments that share a line with code, and comments around the `=` of an assignment, should be reported by analyzer rules so that comments only stand where they do not affect indentation (maintainer guidance, 2026-11-01). A new rule is outside this run; the issue manager checks existing rules (e.g. RH5020) for gaps — 00-lead.md.
