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

_None yet._

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

## Follow-up candidates

- Parenthesized pattern content loses its indentation (`if (o is (⏎1 or 2⏎))`) — 01-spec-author.md; the positional pattern `o is (1,⏎2⏎)` likewise — 02-skeptic.md. One issue.
- RH5026 reports the formatter's output for `// f⏎⏎finally` and `else` (FC-013) — 02-skeptic.md.
- Region directives inside collection expressions, patterns and initializers end up at the statement's column; no requirement decides the column — 02-skeptic.md.
- The blank line between an own-line comment and an array-initializer, extension-block or property-pattern `{` is kept today and RH5024/RH5025 report the formatter's output — 03-spec-author.md (may join the RH5026 candidate).
