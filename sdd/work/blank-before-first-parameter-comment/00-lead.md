# Lead — blank-before-first-parameter-comment

- Issue: thoenissen/Reihitsu#1781 (`agent-filed`: goals are proposals) — FMT-BLANK-011 puts a blank line between `(` and a comment that is the first content of a parameter list or a union's case list.
- Slug: `blank-before-first-parameter-comment`
- Branch: `claude/amazing-ritchie-ywdsex` (session-designated); draft PR #1812.
- Lane: **behavior change** — the issue requests changed formatter output (option A exempts the first content of a parameter list from FMT-BLANK-011); the visible output change is the maintainer's to choose, so a change doc is expected.
- Roles: spec author → skeptic → (output questions) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist.
- Maintainer clarifications: none beyond the issue text.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- First element behind an opener of a type parameter list, statement header, cast, attribute list, array size, or parenthesized pattern lands in the enclosing line's column instead of aligned (01-spec-author.md). Rarity to be judged before the issue manager.

## State

- Last step: spec author (cfb7d41), change doc Draft, output questions 1 (A/B/C) and 2 (A/B) drafted in 01.
- Next: skeptic, then batch output questions.
