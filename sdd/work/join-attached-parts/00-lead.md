# Lead — join-attached-parts

- Issue: thoenissen/Reihitsu#895 — "[FORMATTER FEATURE] Join attached parts outside member-access chains". Labels: enhancement, formatter, `agent-filed` (goals are proposals).
- Slug: `join-attached-parts`
- Branch: `claude/exciting-dirac-dujzha` (session-designated). Draft PR: thoenissen/Reihitsu#1870.
- Lane: **behavior change** — the issue requests new formatter output (joining a wrapped `[`, `(`, `?[`, trailing `!` onto the element in front of it outside member-access chains), so a change doc is expected.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Clarifications from the issue thread (summarized)

- A comment states the issue was decided as the target for further instances: an invocation's argument list, an element access, and primary-constructor base arguments written on the line below. The listed requirements are FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-020. Related: #956.
- A later status comment says an earlier formatter change already joins an argument list's `(` written below its name outside chains: invocations, object creations including implicit `new(...)`, constructor initializers, primary-constructor base types, and the collection-expression `with(...)` element. Per that comment, the element-access case (`arr⏎[0]`) is still open: on `main` it stays unchanged, with `[0]` at the statement column. The trivia guard applies: no join across a comment, directive, or disabled text.
- The comments did not say whether the trailing `!` and the `?[` cases are still open.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## Worklist / next step

- Claimed (11cc1d2), draft PR #1870, SDK prepared.
- Next: spec author (01).
