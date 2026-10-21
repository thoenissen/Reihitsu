Role: spec author (repair round after the skeptic)
Commit: e78bec1
Verdict: open question

This round repairs `sdd/specs/shared/code-fixes.md` against `02-skeptic.md` (F1–F9). I re-derived each finding from the code, the specs and the cited decision comments instead of taking the suggested wording. I filed no issue; the open questions the skeptic lists are the lead's to file. One clause is not written because I cannot cite a decision for it (see Decisions needed); everything else is written and does not depend on it.

Verification: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` gives 38 passed and 1 failed. The only failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-FIX-006, -007 and -008, as before.

## Changed requirements

- SHARED-FIX-002 — clarified (wording, F7 and F9).
- SHARED-FIX-006 — changed (F2).
- SHARED-FIX-007 — changed (F1, F3, F5, F6).
- SHARED-FIX-008 — changed (F4).
- `shared/code-fixes.md` header — `Uses` changes from `—` to `FMT-PIPE-007`, as `SpecStructureTests` requires.

## Per finding

**F1 — fixed.** I re-derived the finding. `scripts/format.sh` on the RH7303 example's result (`var x = 1;⏎x++;⏎x--;`) inserts a blank line after `var x = 1;`, so the sentence "formatting the result changes nothing in that code" contradicted the spec's own example. SHARED-FIX-007 gains a **Blank lines** bullet: the blank lines between the changed node and its neighbours follow SHARED-FIX-006 and the removed-directive bullet, also where formatting the result would insert or remove one. The RH7303 example now says that formatting would insert a blank line after `var x = 1;` and that the fix inserts none. The precedence comes from these maintainer decisions:
- thoenissen/Reihitsu#1209, decision comment 6297851407, Q3. Reading B was "P10: the formatter's layout of the changed statement includes the blank line in front of it". The maintainer decided P14a: no blank line is inserted.
- thoenissen/Reihitsu#1220, decision comment 6297866626, Q2. The maintainer decided P14a: no blank line is inserted before the changed statement.
- thoenissen/Reihitsu#1263, comment 6298667024 (round 7b, option A): "P14a forbids inserting the blank line the formatter would want; P14c only cleans blank lines the removal leaves".
- For "remove": the decided P14d example on thoenissen/Reihitsu#1138 and thoenissen/Reihitsu#1284. Two blank lines above a get-only method stay two, although document formatting collapses them to one.

**F2 — fixed.** SHARED-FIX-006 gains a **Shared line** bullet that points to SHARED-FIX-007 for a member that shares its line with the changed member after the fix. This mirrors the existing pointer for removed directive lines.

**F3 — fixed.** I restored the dropped clause of P14b, "the lines the fix edits are laid out as the formatter lays them out", without the self-referencing parenthetical. I did not pick between the skeptic's two readings of it.

**F4 — partly fixed; one clause not written.** I re-derived the conflicts:
- FMT-PIPE-007 counts end-of-line trivia and documentation-comment newlines. It keeps the original line ending inside literals, inside other comments, and inside disabled text.
- RH5604-001 counts the same set of line breaks.
- SHARED-FIX-003 makes a literal's value and a comment's text content, explicitly "including the whitespace and line breaks inside it". The comment part was decided in `squad/decisions/2026-10-18-shared-fix-003-layout-fields-endregion.md`, answer A.

SHARED-FIX-008 now says: a line break inside a literal, or inside the text of a comment other than a documentation comment, is content under SHARED-FIX-003. It stays as it is and does not count when deciding which line ending the document uses, as FMT-PIPE-007 does not count it.

I did not write the disabled-text exemption. No maintainer decision covers it: SHARED-FIX-003 does not name disabled text, and the FMT-EOL non-goals record that normalizing line breaks in disabled text is undecided even for the formatter. See Decisions needed. I kept the non-goal on documents with no line break or mixed line endings unchanged.

**F5 — fixed.** I rewrote SHARED-FIX-007's `Why:` so it no longer claims the fix wrote every line around the shapes or that no author layout is lost. It now says:
- a shared line, or a blank line that a removal leaves at the edge of a body or next to another blank line, is a shape the formatter would change, "whoever wrote the lines around them";
- the blank lines between the changed node and its neighbours stay the author's (F1).

**F6 — fixed.** The decided clause "the changed member gets its own line" already decides the mirror shape. I added one sentence: when the other member comes first on the line, the changed member starts on the next line. I also added `int _y; void P() { }` to the RH5417 example, with the result `int _y;` followed by `P` on the next line. This shape does not conform today; it is a follow-up candidate for thoenissen/Reihitsu#1144, as the skeptic notes.

**F7 — fixed.** The SHARED-FIX-002 sentence now reads "in one Fix All application to a document, a project, or a solution, every reported diagnostic whose fix is offered on the code the application starts from", so "whose" attaches to the diagnostic.

**F8 — rejected.** The bound on the `///` exterior is still stated in SHARED-FIX-003. A new example needs a rule whose fix changes the exterior only inside its own reported node. No decided or existing-test example of that is at hand, and inventing one would put an unverified claim into the spec. The bound stays without an example, as it is outside this run's decided texts.

**F9 — applied.** SHARED-FIX-002's RH7103 example no longer repeats the code blocks. It points to the example "another run-time change" under SHARED-FIX-003, which has the same input and result, and explains the Fix All behavior on it. Tests can seed from that SHARED-FIX-003 example.

Skeptic notes I did not act on, because they are outside the findings or outside scope:
- **P23 against SHARED-DIAG-001 (generated code).** P23 is decided text and is kept verbatim. The test implementer should not read it as a requirement about generated code.
- **Questions 1–4 and the follow-up candidates.** They are for the lead to file.

## Examples for the tests

- SHARED-FIX-002: the RH7103 example under SHARED-FIX-003.
- SHARED-FIX-006 and SHARED-FIX-008: unchanged.
- SHARED-FIX-007: the RH7303 example is now consistent with the requirement. For the RH7303 and RH7310 examples, "formatting the result changes nothing" holds for the changed code except the blank lines the **Blank lines** bullet governs.
- The RH5417 example gains the `P` mirror case, which does not conform today. Do not tag it.

## Open questions to file

_None new._ The P14b question from `01-spec-author.md` still stands. The lead files it together with the skeptic's questions.

## Decisions needed

**Is a line break inside disabled text exempt from SHARED-FIX-008?** This touches SHARED-FIX-008 and SHARED-FIX-003.
- Input: a document whose line breaks between tokens are all CRLF. It contains `#if NEVER⏎    int a;⏎#endif` where the disabled line breaks are LF. A fix whose rewritten node keeps that excluded branch is applied, for example RH4106 on a field in the SHARED-FIX-003 `#if` example.
- What the spec says now: SHARED-FIX-008 reads literally as requiring every line break the fix keeps, those in disabled text included, to be CRLF. FMT-PIPE-007 keeps the original line ending in disabled text, and the FMT-EOL non-goals record that normalizing it is undecided.
- Readings:
  - (a) Disabled text is exempt, like literals and comment text. The LF line breaks stay. Output: the branch keeps `\n`.
  - (b) It is not exempt. Output: the branch's line breaks become `\r\n`.
- Recommendation: (a). It is consistent with FMT-PIPE-007 and with the "preserve disabled text" rule in `AGENTS.md`.
- This blocks only that clause. No test in this run involves disabled text, and every other part of this round is written.
