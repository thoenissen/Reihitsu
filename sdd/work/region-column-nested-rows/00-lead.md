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
  - Batch 3 answered: A — a comment next to any brace of a multi-line array initializer (`new[] /* c */ {`, `{ /* first */ 1,`, `2 /* last */ }`) keeps its line; the brace or element next to it moves (the spec as written in b15784d).
  - Q7, Q8: not answered; Q8 becomes moot under the declaration (fixes withheld, SHARED-FIX-010). Q7 (comment-led row brace) is a rare class: lead leaves it to local refusal / asks again only if needed.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | 04 | Skeptic 04's internal spec choices (FMT-PIPE-018 deferral, cite FMT-BREAK-016, AGENTS table completeness, context input for WithContext, `stackalloc[]`) | Accepted, routed to spec author 05 | Consistency with unchanged requirements | Spec wording only | b15784d |
| 3 | 08 | New type `ArrayInitializerBraceLineBreakRewriter` | Accept; not escalation 9 | It is the code implementing the issue's own fix (README escalation 9 excludes it); it must run after the cast/list-element joins | Internal structure | 0fea711 |
| 4 | 08 | `ListElementInteriorJoinRewriter.IsOwnedGap` change outside named files | Accept as part of the fix | Narrowest fix for the stability test the change needs; also resolves F6 | Brings output to FMT-BREAK-046/FMT-PIPE-027 | 0fea711 |
| 5 | 08 | F6 (dropped space `{$"a"`) | Dropped — fixed by row 4 | Resolved in run | — | 0fea711 |
| 6 | 08 | Comment starting the line of a multi-line array's `}` (`2⏎/* c */ }`) leaves brace behind comment | Left as written, no follow-up | Rare input class (AGENTS.md Code-fix coverage) | Leaving a rare construct as written puts nothing in question | 0fea711 |
| 7 | 08 | Jagged array: element after a multi-line `new[] {…}` element not moved to its own line | Rows-only reading; spec author states it | The maintainer's answer leaves element line breaks as written; only rows were named | Covered by batch 1b answer 3A (element line breaks not decided) | 60f70f5 |
| 2 | 05 | 05 Q2: does 2B reach regions around usings and inside types in a top-level-statement file | Yes, literal reading | The maintainer confirmed 'any region in a file with top-level statements' | Covered by a maintainer answer | b15784d |

## Follow-up candidates

- F1, F2: moot after the declaration (03-spec-author) — dropped.
- (dropped, decision 5) F6 (06, conditional): interpolated string as first element of a multi-line array inside an argument list joins onto `{` with the space dropped (`{$"a",`) — only if the implementation leaves it.
- F4 (03): RH7303 does not report regions the CLI now refuses in initializers, parameter lists, attribute args, headers.
- F5 (03, low): no analyzer reports the FMT-BREAK-046 brace layout (P11 allows it).
- F3 narrowed (03) to the RH7304 `IsWithinElementBody` spec gap.
- (old) F1 (01-spec-author): blank line inserted after an opening `[`/`(` before a `#region` and before a closing `]`/`)` after an `#endregion`, but not for `{`/`}` — blank-line phase.
- F2 (01-spec-author): a region inside a wrapped method chain, binary expression, or query takes the statement column; spec leaves it unspecified.
- F3 (02-skeptic): RH7304 vs FMT-INDENT-010 for regions crossing a construct's end (attribute list `)]`) or in a base list; RH7304's `IsWithinElementBody` treats initializers/parameter lists/attribute args/base lists as between declarations, which SHARED-TYPEREGION-005 does not state.

## State

- Last step finished: spec author (01, c9404e4; change doc Draft; open output question 1 + several change-doc entries needing answers).
- Skeptic (02, c1f75f2): verdict output question; premise holds, reachable via CLI; no architecture findings; escalation 10 not an option (deletes no special case).
- Skeptic 04 done (3d7023b): spec states answers correctly; 2 new output questions; internal choices for spec author (FMT-INDENT-010 sentence defers to FMT-PIPE-018; cite FMT-BREAK-016; AGENTS table summary completeness; FMT-UNSUP-023 input = context for WithContext; stackalloc[] implicit form). Spec author 05 done (b15784d; change doc Accepted). 05 Q2 (reach of 2B: regions around usings/inside types in a top-level file) decided as covered by the maintainer's follow-up answer 'any region in a file with top-level statements … is unsupported' — spec's literal reading stands. 05 Q1 asked as batch 3, answered A (spec unchanged). Test implementer 06 done (377aa6c tests, d88cd61 handover): 103 formatter, 3 analyzer, 6 CLI failures, all the specified difference. Spec gaps routed to spec author 07 (FMT-BREAK-038, FMT-STRUCT-027 stale examples; FMT-BLANK-019, FMT-STRUCT-007/-013 region clauses now unreachable). Spec author 07 done (c984dd5): stale examples fixed, FMT-STRUCT-007/-013 region clauses kept for analyzers; architecture tests not run (spec checker/validator must). Implementer 08 done (0fea711 code, 83682f2 handover): all default suites green in Debug; 5 prod files +336/-6, one new type. Spec author 09 done (60f70f5): AGENTS.md rows, FMT-BREAK-046 jagged sentence; open worklist: a test for the jagged-array inline example (09 *Examples for the tests*) — route to test implementer with the spec checker's worklist. Cleaner 10 running; then spec checker, auditor.
- Earlier: fresh spec author (05) applies batch 2 + skeptic 04 internal choices, sets change doc Accepted; then test implementer (06).
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
