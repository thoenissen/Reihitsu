# Lead — blank-before-first-parameter-comment

- Issue: thoenissen/Reihitsu#1781 (`agent-filed`: goals are proposals) — FMT-BLANK-011 puts a blank line between `(` and a comment that is the first content of a parameter list or a union's case list.
- Slug: `blank-before-first-parameter-comment`
- Branch: `claude/amazing-ritchie-ywdsex` (session-designated); draft PR #1812.
- Lane: **behavior change** — the issue requests changed formatter output (option A exempts the first content of a parameter list from FMT-BLANK-011); the visible output change is the maintainer's to choose, so a change doc is expected.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.
- Maintainer clarifications: none beyond the issue text.

## Maintainer answers

- Q1 (comment first behind an opener): **C** — no blank line between any opening `(`, `[`, or generic `<` and a comment that is its first content; RH5020 and RH8303 stop reporting exactly there. A comment behind a comma keeps its blank line; an author's blank line behind the opener stays; `/** */`/`///` follow the same answer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Is drafted question 2 (RH5020/RH8303 follow the formatter) open? | Folded into question 1: the rules follow whatever the formatter keeps | SHARED-DIAG-010 and RH5020's mirror of FMT-BLANK-011 decide it | An unchanged requirement demands it | — |
| 2 | Skeptic | Output-question form | Keep labels A/B/C as drafted; drop path and internal references from *Why*; add the analyzer line to each *After* | lead.md section 5, AGENTS.md *Output questions* | Question form only | — |
| 3 | Skeptic | Architecture finding: the 'first content behind an opener' rule exists in three copies (BlockStartUtilities.IsFirstInBlock, RH5020 IsFirstCommentInBlock, RH8303 IsFirstInBlock) | Answer C:  give the rule one owner in an existing Reihitsu.Core utility used by all three, keeping the doc-phase `/** */` move narrow; escalation 9 if it needs a new type or >100 lines | A narrow fix would widen three copies identically, which is the drift the finding names | Internal structure | — |
| 4 | Implementer | BlankLineTokenCleanupRewriter removed an author's blank line before a comment ahead of `]` once `[` was exempt | Keep the implementer's cheap condition in this run | Fallout of the run's change; covered by the existing empty-collection test | Restores FMT-BLANK-011/019 output that `main` already produces | a730de0 |
| 5 | Implementer | Own Core.Test case for `SyntaxTriviaUtilities.IsContentOpeningToken` | Not added | The formatter, RH5020 and RH8303 matrices cover generic and operator `<` | Tests only | — |
| 6 | Repair 1 | Trim change-doc entry 3 (`void M<`, `new int[` columns)? | Leave it | A change doc records real output, not a requirement; the spec no longer pins the column | The output equals `main` apart from the decided blank line | 37af885 |

## Follow-up candidates

- FMT-BLANK-019 wording contradicts itself for an empty `[⏎⏎]` without a comment (one sentence removes the blank line before the closing token, another keeps it behind the opener); code removes it (10-test-implementer.md).
- S1125 warnings at `src/Reihitsu.Cli/FormatCommandHandler.cs` lines 479, 490, 501 in the Debug CLI build during `scripts/format.sh` (06-cleaner.md); pre-existing on `main`.
- A comment first inside a parenthesized pattern makes the formatter put `(` and `)` on own lines and lands comment and pattern in the statement's column (04-test-implementer.md).
- RH8303 reports `///` behind an argument-list `(`, collection-expression `[`, and type-argument `<`, which RH8303-001 says it does not cover (02-skeptic.md); likely resolved by the run under C.
- Operator-wrap member of the alignment candidate: `return a <⏎// c⏎b;` (02-skeptic.md).
- First element behind an opener of a type parameter list, statement header, cast, attribute list, array size, or parenthesized pattern lands in the enclosing line's column instead of aligned (01-spec-author.md). Rarity to be judged before the issue manager.

## State

- Audit 2 (repair 1 of 4ebe340): pass; audited SHA de1f1a5. Repair rounds used: 1.
- Next: validator, lane check, issue manager (follow-up candidates), note, publish.
