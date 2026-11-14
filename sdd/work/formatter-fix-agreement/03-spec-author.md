Role: spec author
Commit: fc96006
Verdict: #1529: spec written, change doc written; #1543: spec written, change doc written

Follow-up stage after the maintainer's answers (1B, 2A in `00-lead.md`). My prompt carried no intended fix or hoped-for verdict beyond those answers. Outputs re-measured at fc96006 with `scripts/format.sh` and `scripts/apply-fix.sh` (LF and CRLF) on scratch fixtures; the *after* outputs of #1543 are derived from today's output by removing the blank line behind each comment, since nothing is implemented yet.

Both change docs stay `Status: Draft`; the lead sets `Accepted` (`sdd/README.md`, *Change docs*).

`Reihitsu.ArchitectureTests.Sdd`: 50 of 51 pass; `EveryRequirementOfAnEnforcedSpecIsCovered` fails only for RH5111-006 ("no test covers it"), which the test implementer's `[Requirement("RH5111-006")]` tests close.

## thoenissen/Reihitsu#1529

### Changed requirements

- RH5111-006 — The fix lays out an assignment inside a conditional access as the document does — added (`sdd/specs/analyzer/RH5111.md`; ledger updated; `Uses` gains SHARED-CHAIN-003, SHARED-CHAIN-007, SHARED-CHAIN-008).

Change doc: `sdd/changes/2026-11/conditional-access-assignment-fix-layout.md` — Affected requirements, five visible output changes (the column-9 input added as change 2; two wrapped links and `a.B?.B` as change 5), Decisions (maintainer's answer B; supersession of the `node-root-detached.md` repair-round-2 choice; D and E not chosen, the shape not declared unsupported; requirement placed in RH5111), Alternatives A, C, D, E.

### Examples for the tests

RH5111-006: the first example (six reported assignments: `a?.B` col 10 and col 9, `a?[0]`, `a?.B?.B`, two wrapped links, `a.B?.B`) gives the fix tests; the second (`a?.B.C =⏎ 5`, `x.B⏎ .C =⏎ 5`) the boundary on each side of the qualifiers "a link of the left side starts a line" and "behind a conditional access's `?`". Markup: the reported span starts behind the last `?` (`a?{|RH5111:.B`, `a?.B?{|RH5111:.B`), as RH5111-003 and the existing tests have it. Each fix result should also equal whole-document formatting of the same input (measured: identical at fc96006) and be silent under RH5201 and RH5112 (skeptic). The three existing tests at `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs` ~2040–2200 change their expected output.

### Output questions

_None._ Answered: 1B.

### Internal choices

- **`node-root-detached.md` is not edited.** The lead asked to update it where the new answer supersedes its decision "as the SDD rules require". The rules require the opposite: that doc is merged into `main` (via #1530), and `sdd/README.md`, *Change docs*: "Change docs are history. Once merged, they are not edited to track later changes; a later change gets its own doc." The repository's precedent (`blank-line-sole-owner.md`, "This supersedes two earlier decisions"; `using-comment-blank-line.md`; `withhold-unsafe-fixes.md`) records supersession in the new doc only. The new doc's *Decisions* names the superseded section and states what of it still holds (the member keeps its line break; `main`'s join stays rejected; the RH6023 part is unaffected). Alternative: add a forward pointer to the old doc — rejected under that rule. If the lead still wants the old doc touched, that is a rule exception the lead records.
- Skeptic: column-9 input added as a visible output change — agreed, done (change 2), plus c08 and c12 as change 5.
- Skeptic: drop RH6023 from *Affected* — it was never listed there; *What* keeps one sentence saying its half has no output in question, which is the issue's own claim and worth answering in the doc.
- Skeptic: whole-document parity test for c01/c05/c06 — no formatter requirement changes, so no FMT example; the test implementer can take the "equal to whole-document formatting" check as stated above. Recommend keeping it, as the skeptic does.
- Requirement placed in RH5111 and referencing SHARED-CHAIN-003/007/008 rather than restating the anchor rule; alternative an example under SHARED-FIX-007.
- The skeptic's two implementation shapes to watch (an entry-point fact predicting the anchor column; widening the fix's target beyond the assignment, FC-030) are not spec matters; RH5111-006 states that the code in front of the assignment is not changed and its first token keeps its place, which excludes the second.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1543

### Changed requirements

- FMT-BLANK-020 — Behind a comment on its own line, no blank line before a brace, closing bracket, or chained keyword — changed (`sdd/specs/formatter/blank-lines.md`): title; the token list now names every brace except the opening brace of a block statement that follows another statement (FMT-BLANK-017), plus `else`, `catch`, `finally`, do-`while`, and states that these are the tokens of FMT-BLANK-007/008/019; the "construct decides" sentence now says only the token kind and the block-statement condition decide; *Why* mirrors RH5024–RH5027 and drops "not decided by this requirement"; one example added.

Change doc: `sdd/changes/2026-11/own-line-comment-before-every-brace.md` — *Why* corrected, entry 1 extended, entry 5 switched to a raw interpolated string, Decisions (maintainer's answer A; one token set; the issue's input refused by FMT-UNSUP-004), Alternatives (B; declaring the keyword shape unsupported, not proposed).

### Examples for the tests

FMT-BLANK-020: the new example (braces of an array initializer in assignment and field form, a nested array element `{`, an extension block, a property pattern `{`, an assigned collection initializer `}`, a raw interpolation's `}`, `else`, `catch`, `finally`, do-`while`) gives the formatter regression tests; the existing examples, the FMT-BLANK-017 one included ("left unchanged"), stay and must keep passing. Change-doc entries 1–6 give the same cases one by one. Per `00-lead.md` decision 3, round trips extend to RH5026 and RH5027.

### Output questions

_None._ Answered: 2A.

### Internal choices

- Skeptic: correct the oscillation claim — agreed; *Why* now says the fix's removal is kept by formatting and the cost is lasting diagnostics on formatted code.
- Skeptic: declaration-form array initializer `}` and nested array-initializer element `{` — agreed; added to entry 1 and to the spec example (measured before outputs). `new[] { }` and `stackalloc int[] { }` named in the text, not shown.
- Skeptic: entry 5 in the raw form — agreed; switched, with one sentence that the regular-string comment column is not part of this change.
- Skeptic: RH5026/RH5027 round trips — agreed; a test matter, already lead decision 3.
- The issue's own input moved from *Why* to a *Decisions* entry, so it is stated once.

### Follow-up candidates

Unchanged from `01-spec-author.md`/`02-skeptic.md` (comment in front of a multi-line interpolation hole's `}` in a regular interpolated string goes to column 12); already in `00-lead.md`.

## Whole run

Files: `sdd/specs/analyzer/RH5111.md`, `sdd/specs/formatter/blank-lines.md`, `sdd/specs/requirement-ids.txt`, `sdd/changes/2026-11/conditional-access-assignment-fix-layout.md`, `sdd/changes/2026-11/own-line-comment-before-every-brace.md`, this handover. No rule page changes: RH5111's page describes the normal case, unchanged; RH5024–RH5027 are unchanged.
