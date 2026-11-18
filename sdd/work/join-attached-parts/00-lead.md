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
| 1 | Spec author | Widen FMT-BREAK-020 or add a new requirement for the join? | New FMT-BREAK-040 owns the join; FMT-BREAK-020 keeps member-name rejoin only | One owner per gap; spec author's recommendation | Spec structure only | e13cde4 |
| 2 | Spec author | Follow the issue's owner hint (`JoinConditionalElementAccesses`, `NormalizeChain`)? | No; no code owner is prescribed | Agent-filed proposal; the implementer picks the owner | Internal | e13cde4 |

## Follow-up candidates

- FMT-BREAK-015 names FMT-BREAK-016..020 as owners of postfix operator wraps, but none states `++`/`--` (spec wording; output not checked) — 01-spec-author.
- Analyzer counterpart (report a line break in front of an attached part): would be a new rule → not taken in; an issue already exists per #895's text, so the issue manager checks duplicates — 01-spec-author.

## Worklist / next step

- Claimed (11cc1d2), draft PR #1870, SDK prepared.
- Spec author done (e13cde4): change doc `sdd/changes/2026-11/attached-part-below-element.md` (Draft, 5 visible output changes), FMT-BREAK-040 new; one output question (column of a part held below by a comment). Lane check confirmed.
- Next: skeptic (02), then output question batch.
