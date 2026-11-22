# Lead — playground-highlighting

- Issue: thoenissen/Reihitsu#1941 — Playground: syntax highlighting, no line wrapping, and visible error locations. Labeled `agent-filed` (the lead wrote it), but its plan items 1–4 were chosen by the maintainer in the chat before the issue was filed, so they are binding (see Maintainer answers).
- Milestone: 1.6.0
- Branch: `claude/beautiful-volta-05rdua` (designated by the session)
- PR: #1942 (draft)
- Slug: `playground-highlighting`
- Lane: behavior change — requested new user-visible Playground behavior (`src/Reihitsu.Playground`) that changes PLAY requirements in `sdd/specs/playground/playground.md`; needs a change doc.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer answers

1. Highlighting source (before the issue): colours come from Roslyn's syntax tree, which the page already loads; no editor library, no CDN.
2. Input colouring (before the issue): updates live while typing; the output still changes only on "Format" (PLAY-004 unchanged).
3. Syntax-error message (before the issue): keep today's sentence and list every syntax error below it with line, column, ID, and message (e.g. `line 1, column 26 — CS1002: ; expected`), plus a red squiggle under each error span in the input.
4. No auto-wrap in either pane; horizontal scrolling instead.
5. Errors shown in a clearly styled alert box; unsupported syntax gets the squiggle at its reported location; hidden code gets only the alert box.
6. Delivery: issue filed (#1941, milestone 1.6.0), run through the squad in this session.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

## Audited SHA

## Next step

Spec author (01) and skeptic (02) done. Waiting on output-question batch 1: Q1 marks after edit, Q2 var/nameof/dynamic/value colour, Q3 empty span at line end, Q4 doc-comment colouring. Then route answers to spec author (fresh or continue), plus skeptic wording gaps (comment trivia behind directives, aria-hidden overlay, shared line/column helper, stale change-doc line), then test implementer.
