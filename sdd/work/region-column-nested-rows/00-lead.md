# Lead — region-column-nested-rows

- Issue: thoenissen/Reihitsu#1399 (`agent-filed`: goals are proposals)
- Slug: region-column-nested-rows
- Branch: claude/epic-rubin-2swis1 (session-designated); draft PR thoenissen/Reihitsu#1995
- Lane: behavior change — the issue requests changed formatter output (region directive column inside initializers, collection expressions, switch expressions, argument lists, patterns; nested array-initializer rows laid out like complex element initializers) that FMT-INDENT-010 / FMT-INDENT-023 do not state today, so a change doc is required.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Issue context (summarized): the issue says the two decisions were taken in thoenissen/Reihitsu#912 (round 9 items 20, 21, both option b). Two later comments add instances: list patterns and parenthesized patterns wrapped over several lines (item 20 shape), and a region inside a switch *section* where `#endregion` stands four columns left of `#region` (not checked whether FMT-INDENT-010 decides it).

## Maintainer answers

- Batch 1 (after skeptic), answered:
  - Q1–Q5 (region directive columns inside initializers, patterns, lists, switch sections): a region inside a member is to be declared **unsupported** (escalation 10, answered: declare). Scope of "inside a member" and whether it is declared in this run are asked back (batch 1b).
  - Q6 (row opening behind `}, {`): neither option fits. Rule given instead: each initializer is either single-line or multi-line, judged per initializer when nested; a multi-line initializer puts its `{` and `}` on their own lines. Its reach (rows only vs every array initializer) is asked back (batch 1b).
  - Batch 1b answered: 1A (every region directive from a member's first to its last token is unsupported — bodies, accessors, lambdas, switch sections, field/property initializers, parameter lists, the member's attribute arguments; a nested type is not a member, regions between its members stay supported), 2A (declared in this run), 3A (every array initializer — outer, rows, `new[] { … }` — follows the single-/multi-line rule; multi-line puts `{` and `}` on their own lines). Addition: the `{` on its own line should start under the `=` and not be wrapped (meaning asked back in batch 1c: placement for `new[]` and what "not wrapped" refers to).
  - Batch 1c answered (brace placement of a multi-line array initializer): when the initializer stands directly behind `=` (`int[,] b = { … }`), its `{` stays on the `=` line; with `new[]` / `new T[…]`, the `{` goes on its own line under `new` (as collection initializers do today). Rows inside and the `}` follow the multi-line rule (own lines).
  - Batch 2 answered (numbering of the lead's message; 03 Q1–Q5 = 1–5, 04 Q1–Q2 = 6–7): 1A (a region in a type's header — base list, primary-constructor parameters — is unsupported); 2B (confirmed by follow-up: in a file with top-level statements, every region is unsupported, also one between the statements — all top-level statements count as one member); 3B (a comment in front of a multi-line row's `{`: the `{` moves onto its own line below the comment, row laid out from the `{`'s own line); 4A (a line break inside a string literal makes the initializer multi-line); 5A (`stackalloc T[] { … }` and `stackalloc[] { … }` follow the `new` rule, `{` under `stackalloc`); 6A (judged on the lines the other line-break rules leave); 7A (regions between an extension block's members stay supported, like a nested type; a region in `extension(...)` header is refused as in 1).
  - Q7, Q8: not answered; Q8 becomes moot under the declaration (fixes withheld, SHARED-FIX-010). Q7 (comment-led row brace) is a rare class: lead leaves it to local refusal / asks again only if needed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

- F1, F2: moot after the declaration (03-spec-author) — dropped.
- F4 (03): RH7303 does not report regions the CLI now refuses in initializers, parameter lists, attribute args, headers.
- F5 (03, low): no analyzer reports the FMT-BREAK-046 brace layout (P11 allows it).
- F3 narrowed (03) to the RH7304 `IsWithinElementBody` spec gap.
- (old) F1 (01-spec-author): blank line inserted after an opening `[`/`(` before a `#region` and before a closing `]`/`)` after an `#endregion`, but not for `{`/`}` — blank-line phase.
- F2 (01-spec-author): a region inside a wrapped method chain, binary expression, or query takes the statement column; spec leaves it unspecified.
- F3 (02-skeptic): RH7304 vs FMT-INDENT-010 for regions crossing a construct's end (attribute list `)]`) or in a base list; RH7304's `IsWithinElementBody` treats initializers/parameter lists/attribute args/base lists as between declarations, which SHARED-TYPEREGION-005 does not state.

## State

- Last step finished: spec author (01, c9404e4; change doc Draft; open output question 1 + several change-doc entries needing answers).
- Skeptic (02, c1f75f2): verdict output question; premise holds, reachable via CLI; no architecture findings; escalation 10 not an option (deletes no special case).
- Skeptic 04 done (3d7023b): spec states answers correctly; 2 new output questions; internal choices for spec author (FMT-INDENT-010 sentence defers to FMT-PIPE-018; cite FMT-BREAK-016; AGENTS table summary completeness; FMT-UNSUP-023 input = context for WithContext; stackalloc[] implicit form). Batch 2 answered. Next: fresh spec author (05) applies batch 2 + skeptic 04 internal choices, sets change doc Accepted; then test implementer (06).
- Spec author 03 done (2956db2): FMT-UNSUP-023 (region inside a member), FMT-BREAK-046 (array-initializer braces); 5 open output questions in 03; scope warning ~217 test literals hold a region inside a member. Next: skeptic 04, then batch 2.
- Batches 1, 1b, 1c answered. Earlier plan: fresh spec author (03) rewrites change doc + specs per answers; then fresh skeptic (04) on the amended diff; then any new output questions; then test implementer.
- Old: batch 1b (scope of the declaration, timing, reach of the initializer line rule).
- Old: output-question batch 1 (Q1 scope A/B, Q2 switch-section #endregion, Q3 anonymous object, Q4 patterns, Q5 other aligned lists, Q6 entry 9, Q7 entry 10, Q8 code-fix results). Next after answers: route to spec author, then test implementer.

## Confirmed examples (maintainer: "looks good")

Array initializers (statements at column 8, fields at 4):
1. `int[,] b = { {⏎1, 2⏎}, { 3, 4 } };` → outer `{` stays behind `=` (col 19), `}` own line at 19; multi-line row: `{` own line at 23, `1, 2` at 27, `},` at 23; single-line row `{ 3, 4 }` own line at 23.
2. `var e = new[] {⏎1, 2⏎};` → `var e = new[]` / `{` at 16 / `1, 2` at 20 / `};` at 16.
3. `var f = new int[,] { { 1, 2 }, {⏎3, 4⏎} };` → `var f = new int[,]` / `{` at 16 / `{ 1, 2 },` at 20 / `{` at 20 / `3, 4` at 24 / `}` at 20 / `};` at 16.
4. `int[,] g = { { 1,⏎2 }, { 3, 4 } };` → row is multi-line: `{` at 23, `1,` and `2` each at 27 (element line breaks kept as written), `},` at 23, `{ 3, 4 }` at 23, `};` at 19.
5. Fully single-line initializers (`int[,] a = { { 1, 2 }, { 3, 4 } };`, `var h = new[] { 1, 2 };`) stay as written.
6. Field `int[] F = { 1,⏎2 };` → `int[] F = {` / `1,` at 18 / `2` at 18 / `};` at 14.
Regions:
7. Regions between members, also between a nested type's members, stay supported.
8. Unsupported: a region in a method body, a field initializer, a parameter list, a member's attribute arguments; whole file left unchanged, CLI reports `error: <file>(line,col): unsupported syntax: <reason>. The file was left unchanged.`; code fixes that format such a member are not offered on it.
