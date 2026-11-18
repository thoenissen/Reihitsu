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

- Batch 1: 1A (join `[`/`!` outside chains), 2A (a conditional is judged as the line it becomes), 3A (a list is judged as the line it becomes, including `?[` and in-chain `!`), 4A (a collection expression keeps one element per line), 5A (RH5302 fix results follow).
- Q6 (a part kept below by a comment): the maintainer answered "unsupported", i.e. declare the shape unsupported (escalation 10). Details: 6.1A — declare in this run; 6.2A — the shape covers the gap in front of `[` (element access), `?[`, and a trailing `!` (the FMT-BREAK-040 gaps), and the gap in front of an argument list's `(` (FMT-BREAK-039); 6.3A — a comment, a directive, or disabled text in the gap. - Batch 2: 1A — a same-line comment in the gap is part of the shape (refused; FMT-SPACE-028's `[with /* size */ (4)]` example becomes a refused input); 2 "unsupported" = 2A — the gap between `?` and `[` of a conditional element access is declared too, in this run, as part of the same shape.
- (6 note) Lead's caveat to the maintainer: if the declaration deletes no special case, the lead comes back before anything is built.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Widen FMT-BREAK-020 or add a new requirement for the join? | New FMT-BREAK-040 owns the join; FMT-BREAK-020 keeps member-name rejoin only | One owner per gap; spec author's recommendation | Spec structure only | e13cde4 |
| 3 | Skeptic | Architecture finding: `ListElementInteriorJoinRewriter` (before list layout) and `ChainLineBreakRewriter` (after) both join attached parts | Change the structure in this run: FMT-BREAK-040 is the one owner and the join runs before list layout; no third partial join site | The narrow fix would add a third copy; within the line-break phase the run already changes | Output effects are change doc change 3, asked as an output question | — |
| 4 | Skeptic | FMT-BREAK-040 does not exclude the implicit element access (`["a"] = 1` in initializers), `new[]`, or `stackalloc[]` | Spec-author amendment: exclude them and add a keep-as-is example | They are not attached parts; read literally, the spec would join common code | The amendment keeps today's output | 28dda88 |
| 5 | Skeptic | The issue's goals: the root-column goal and the owner hint | Root column → output question 6; owner hint dropped | Agent-filed proposals | The root column is asked as an output question | — |
| 6 | Spec author (04) | The CLI prefix in the change doc examples (`Error:` vs `error:`) | Use what the CLI prints today | Examples must be real output | Doc text only; no CLI change | 476ec37 |
| 7 | Skeptic (05) | RH5112's fix also becomes withheld under FMT-UNSUP-014 (`return items // element⏎?[0]⏎.ToString();`) | Add it to change 7, with a test that locks the withheld fix | A consequence of the declaration answer 6 | Covered by answer 6 (the declaration withholds every fix whose formatted node holds the shape) | 20d383b |
| 2 | Spec author | Follow the issue's owner hint (`JoinConditionalElementAccesses`, `NormalizeChain`)? | No; no code owner is prescribed | Agent-filed proposal; the implementer picks the owner | Internal | e13cde4 |

## Follow-up candidates

- FMT-BREAK-015 names FMT-BREAK-016..020 as owners of postfix operator wraps, but none states `++`/`--` (spec wording; output not checked) — 01-spec-author.
- Postfix `++`/`--` below its operand: `i⏎++` stays at the statement's column; `Baz(i⏎++, 2)` gets one argument per line; no requirement decides it — 02-skeptic.
- `new⏎[]`, `new int⏎[3]`, `stackalloc⏎[]` leave `[` at the statement's column — 02-skeptic.
- A lambda expression body below `=>` lands at the statement's column; no requirement decides the column — 02-skeptic.
- A comment on its own line in front of a wrapped chain link inside a statement gets a blank line above it (`var d = s⏎// c⏎.Length;`) — 04-spec-author.
- Analyzer counterpart (report a line break in front of an attached part): would be a new rule → not taken in; an issue already exists per #895's text, so the issue manager checks duplicates — 01-spec-author.

## Worklist / next step

- Claimed (11cc1d2), draft PR #1870, SDK prepared.
- Spec author done (e13cde4): change doc `sdd/changes/2026-11/attached-part-below-element.md` (Draft, 5 visible output changes), FMT-BREAK-040 new; one output question (column of a part held below by a comment). Lane check confirmed.
- Skeptic done (6a73a04): proceed. Amendment (decision 4) sent to a fresh spec author (03).
- Spec amendment done (28dda88), commit for decision 4.
- Output question batch 1 (change doc changes 1–5 + the comment column) posted to the maintainer; fully answered.
- Spec author 04 done (476ec37): FMT-UNSUP-014, which deletes the `ArgumentListLineStartRewriter.CanJoin` trivia term and the `MethodChainAttachedPart` column. Open for the next batch: (a) a same-line comment in the gap (spec refuses it, recommendation A); (b) escalation 10: also declare the gap between `?` and `[` (would delete the remaining inner-gap refusal).
- Skeptic 05 done (720a2cf): the declaration is sound. Batch 2 asked: (a) same-line comment, incl. the FMT-SPACE-028 conflict `[with /* size */ (4)]`; (b) escalation 10 for the `?`–`[` gap. ANSWERED (1A, 2A).
- Spec author 06 done (20d383b): spec complete, no open questions.
- Next: test implementer (07).
- Old: spec author (06) applies (a), (b), RH5112 in change 7, and the FMT-UNSUP-014 wording; then the test implementer.
- Old: fresh skeptic (05), then ask (a) and (b) with any skeptic questions, then the test implementer.
- Old: spec author (04) writes the declaration and Decisions, and sets Status Accepted; then a fresh skeptic (05) on the declaration; then the test implementer.
