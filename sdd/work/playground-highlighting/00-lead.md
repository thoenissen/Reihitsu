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
8. Escalation 9 (incremental highlighting, +534 production lines, two new types): answer A — keep the full incremental fix since it works.
6. Delivery: issue filed (#1941, milestone 1.6.0), run through the squad in this session.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | tests (04) | Does PLAY-010 "each category has its own colour" also require a colour different from the normal text colour? | Yes; keep the `AreNotEqual(normal, …)` assert | A category in the normal colour would be indistinguishable from uncoloured text, which defeats the requirement | Answer 1 asked for distinct colours per category; the exact colours are styling, a spec non-goal | df0d437 |
| 2 | implementer (05) | A mark that starts inside another and ends past it | Cut at the outer mark's end, one element per mark | Keeps markup well-formed; no probed input produces it | Rare overlapping-error class; the mark still starts at the reported position | f209936 |
| 3 | implementer (05) | `PlaygroundHighlighter` missing from the spec's Code owners | Route to spec author (spec header and `sdd/specs/README.md`) | Code-owner lists are spec metadata | Internal spec metadata | 0f674fc |
| 4 | audit 1 (09) | Repair round 1 worklist | F1 (empty-span widening splits surrogate pairs; Unicode line breaks marked) → tests + implementer; F2 weak stylesheet tests → test implementer; F3 two bare ParseText calls → implementer, one shared parse entry point; F4 output `<pre>` loses label and focus → test + implementer; F5 double BOM in Home.razor.cs → implementer; F6 `Errors` doc vs null → implementer | All PR-introduced or same requirement; small | F1 restores PLAY-014/PLAY-010 as specified; F4 restores the label PLAY-002 already names; the rest are internal | 9bf2670 |
| 6 | repair 1 (11) | Keep the implementer's extra `:focus-visible` outline on the output pane? | Keep | A keyboard focus stop without a visible focus indicator is an accessibility defect of finding 4's own repair | Styling, a spec non-goal; it mirrors the input's existing outline | 9bf2670 |
| 7 | repair 1 (13) | Spec gap: PLAY-012 does not state the keyboard reachability the output test asserts | Spec author adds one sentence to PLAY-012 (no new ID) | Test tag must be backed by spec text | On `main` the output is a focusable text area; the sentence keeps that existing reachability | 7bead9d |
| 8 | audit 2 (16) | Finding 7 (output `pre` named via `aria-labelledby` without a nameable role) repeats finding 4's class | Handled as an architecture finding by its cause, not as a repair round: the test helper decides the name by the ARIA rule that a role-less `pre`/`div`/`span` cannot be named, and the output pane gets a role that may be named; also correct the `Parse` summary (hint) | The cause is the element's role, not the one attribute; fixing the helper's rule stops the next member | Restores the name PLAY-002 already gives the pane; no new visible output | 9dbe708 |
| 9 | validation (21) | Repair round 2 worklist | V1: panes grow to their content width instead of scrolling inside (violates PLAY-012; caret off screen) → test implementer (a test that fails on it, at the strongest level the harness allows) + implementer. V2: pasting/typing large input is far too slow (300-line fill 27 s, 1000 lines > 120 s) → implementer finds the cause and fixes it; acceptance bar set by the lead: pasting a 1000-line file re-colours in about 2 s or less and a keystroke at 1000 lines stays under about 100 ms in Release, measured by the validator; test implementer adds a deterministic test only if one can pin the fix without timing | Both PR-introduced; main's text areas neither grow nor re-render | V1 restores PLAY-012 as written; V2 is responsiveness, not output | 7791652 |
| 10 | audit 4 (28) | Repair round 3 worklist | F8: NUL becomes U+FFFD through the markup path → refuse by cause: text that holds NUL never goes through the markup path (text frames; the implementer picks the cheapest variant that keeps every character), correct the `AppendEncoded` summary, page test with a pasted NUL; F9: lone CR at the end of `app.css` → implementer | Rare input class; refusal is a cheap check, not modelling | Restores PLAY-010's 'no character changed'; colours on such a line may be lost, a rare-class refusal | pending |
| 11 | repair 3 (29) | New `#pragma warning disable BL0006` in `PlaygroundBrowserDocument.cs` (test helper reading the render tree) | Not accepted as is: the repository carries no suppressions; a fresh test implementer removes it by not referencing the analyzer-flagged API at compile time, keeping the tests' strength; escalation 6 only if no such way exists | Suppressions are the maintainer's (escalation 6); first establish whether one is needed | Test-only | 04dbbfa |
| 5 | audit 1 (09) | Pre-existing `@media (max-width: 700px)` pane height overridden by the later base rule in `app.css` | Adjacent fix in this run, with a stylesheet test | A few lines in a file the run already rewrites; restores the narrow-screen height the stylesheet already intends | Restores the stylesheet's own stated layout; styling is a spec non-goal | 9bf2670 |

## Follow-up candidates

- Playground: process only the latest of several queued input events, and keep the output pane out of renders caused only by typing (from 23-implementer.md).

## Audited SHA

0605942 (audit 4, repair 2; findings 8, 9)
b63ec36 (audit 3, pass; final audited tree)
b38dac5 (audit 2, repair 1; finding 7 open)
fcb82346886ea93149e5229451aa87a43b7438fb (audit 1, base main 1788f94)

## Next step

Repair round 3 of 3 (last): test implementer (29) → implementer (30) → cleaner (31) → spec checker (32) → auditor repair-only (33) → validator (34) with browser re-measure. A block after this round is resolved by narrowing.
