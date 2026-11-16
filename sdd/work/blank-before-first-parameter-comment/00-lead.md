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

## Follow-up candidates

- RH8303 reports `///` behind an argument-list `(`, collection-expression `[`, and type-argument `<`, which RH8303-001 says it does not cover (02-skeptic.md); likely resolved by the run under C.
- Operator-wrap member of the alignment candidate: `return a <⏎// c⏎b;` (02-skeptic.md).
- First element behind an opener of a type parameter list, statement header, cast, attribute list, array size, or parenthesized pattern lands in the enclosing line's column instead of aligned (01-spec-author.md). Rarity to be judged before the issue manager.

## State

- Last step: maintainer answered 1C.
- Next: route answer to spec author (with skeptic internal choices 1-5), then test implementer.
