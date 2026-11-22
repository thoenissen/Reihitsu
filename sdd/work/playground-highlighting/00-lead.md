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
7. Batch 1 (answer: "as recommended" = 1A, 2A, 3A, 4A):
   - 1A: all error marks disappear at the first edit of the input and return on the next Format; the alert box stays until the next Format.
   - 2A: colour follows the parser — `var`, `nameof`, `dynamic`, `value` as identifiers when parsed as identifiers; contextual keywords parsed as keywords (`partial`, `async`, `await`, `get`, `set`, `where` in a constraint) as keywords.
   - 3A: an empty span at a line break is marked one character wide just behind the line's last character; in front of an ordinary character that character is marked; at end of text a mark behind the last character.
   - 4A: everything inside a doc comment is comment colour; directive text is directive colour; comment trivia behind a directive (`#endregion // Methods`) is comment colour.
6. Delivery: issue filed (#1941, milestone 1.6.0), run through the squad in this session.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | tests (04) | Does PLAY-010 "each category has its own colour" also require a colour different from the normal text colour? | Yes; keep the `AreNotEqual(normal, …)` assert | A category in the normal colour would be indistinguishable from uncoloured text, which defeats the requirement | Answer 1 asked for distinct colours per category; the exact colours are styling, a spec non-goal | df0d437 |
| 2 | implementer (05) | A mark that starts inside another and ends past it | Cut at the outer mark's end, one element per mark | Keeps markup well-formed; no probed input produces it | Rare overlapping-error class; the mark still starts at the reported position | f209936 |
| 3 | implementer (05) | `PlaygroundHighlighter` missing from the spec's Code owners | Route to spec author (spec header and `sdd/specs/README.md`) | Code-owner lists are spec metadata | Internal spec metadata | pending |

## Follow-up candidates

## Audited SHA

## Next step

Implementer (05) done at f209936, 80/80 Playground tests pass. Spawn spec author (code owners, 06) and cleaner (07) in parallel, then spec checker, auditor, validator (browser checks listed in 04/05).
