Role: spec author
Commit: 7e6b3ea
Verdict: #951: spec written; #1133: spec written; #1146: spec written

Inputs: thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146 and their comments (read through the GitHub MCP server), `00-lead.md` with its two "Escalations" entries, `02-skeptic.md`, and the specs named below. The #951 body names a suspected cause and an intended mechanism, and the #1146 body proposes RH5104 rewordings for Q3, Q5, and Q6. I did not use either: the specs state observable output only, and Q3, Q5, and Q6 are out of scope (thoenissen/Reihitsu#1433). `sdd/specs/shared/code-fixes.md` is referenced and was not edited. No change doc.

Check run on the current tree: `scripts/test.sh --project architecture` filtered to `SpecConsistencyTests`, `SpecStructureTests`, `RuleDocumentationConventionTests`, and `RequirementCoverageTests`. 50 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for the four new IDs FMT-RAW-007, FMT-RAW-008, FMT-RAW-009, and RH5104-006. The test implementer adds those tests.

## Revision history

- **5572ea1.** First spec diff.
- **2178115.** Escalation 1, A (narrow): RH5601-001 and RH5602-001 are back to their `9501a4b` text; the RH5601-002 and RH5602-002 clarifications are kept.
- **3e6dd08.** Escalation 2 (Q1 1B, Q2 2A, Q3 3A, Q4 4B) and the skeptic's findings that needed no maintainer decision. I re-derived each finding; each one is listed below under its issue, with how I checked it.
- **f649b88.** Escalation 3, answer A: a comment that starts a hole line takes the FMT-INDENT-006 column of the token it precedes, measured after FMT-RAW-007. FMT-RAW-007 gains this as a third condition, with an example, and the case is removed from its coverage sentence (see #951).

- **8d5a762.** The skeptic's re-check (`02-skeptic.md`, "Re-check", 12b09f9) listed repairs R1 to R4 in the spec. None needed a maintainer decision (`00-lead.md`, "Lead notes"). I re-derived each one:
  - **R1, confirmed.** I traced a literal inside a lambda body on the current tree, a misaligned literal over passes 1 to 3. Pass 1 puts `name` and `}` at 27 and 27. Pass 2 puts them back at 14, the block column inside the body. That is case 1's double move.
    - Under the old wording "directly or through the construct around it", this input reads two ways. Only the bounded reading is idempotent, and it matches 1B's "measured after the literal's shift", which implies that the anchor moves with the literal.
    - FMT-RAW-007's alignment condition now requires both an anchor inside the literal (in the same hole or on a content line) and an aligned construct inside the hole.
    - The other conditions of FMT-RAW-007 now cover every other token-started hole line, whatever column the indentation phase would give it. That includes scope-depth columns and columns that a construct outside the hole sets.
    - A new example shows the lambda-body input. The surrounding output is the trace's output, with the input's blank line before `M(` that BlankLinePhase would otherwise insert.
    - FMT-INDENT-011 now defers to FMT-RAW-007. The FMT-INDENT non-goal says "no alignment inside the hole".
    - FMT-INDENT-013's list of constructs that lay out their own lines now names the interpolation hole. The skeptic reports that `ListElementInteriorUtilities.IsOwningConstruct` includes `InterpolationSyntax`. I did not re-read that code; the spec now matches the skeptic's observed traces.
    - FMT-RAW-007's title is now "Hole lines move only with the literal unless aligned within it".
  - **R2, confirmed.** In an aligned literal, the comment condition moves `// c`, FMT-CLEAN-002 replaces tabs, and FMT-SPACE spaces `1+2` inside holes. The old FMT-RAW-006, "MUST NOT change", contradicted all three. FMT-RAW-006 now forbids moving only what the raw-string phase moves: the content lines, the closing marker, and the hole lines that FMT-RAW-007 moves only with the literal. Its explicit exceptions are gone; nested raw literals and nested text need none.
  - **R3, confirmed by reading.** FMT-INDENT-003 (switch sections) and FMT-INDENT-004 (unbraced embedded statements) also give scope-depth columns. The last condition of FMT-RAW-007 names FMT-INDENT-002 to FMT-INDENT-004, and FMT-INDENT-004 is added to `Uses`.
  - **Coverage sentence.** FMT-RAW-007 no longer leaves lines broken inside a hole unspecified as a whole; the last condition now places them. Only the column that the earlier phase gives such a line stays unspecified. Hole lines indented less than a leftward shift stay unspecified, as before.
  - **R4, confirmed.** In `$@"a⏎{x}b"`, the second line begins at the hole's `{`. That position is neither in text nor in a hole that began earlier, yet the example says the fix is withheld.
    - RH5104-006 now tests whether the line begins inside a literal or a comment that began on an earlier line, whatever starts the line. An interpolated string counts with its text, its holes, and the brace that opens a hole.
    - The example gains the `@$"` twin.
    - RH5104-004's condition now reads "inside a literal, an interpolated string included, or a comment (RH5104-006)".
    - The bullet on the rule page `documentation/rules/RH5104.md` now says "a multi-line string literal, interpolated strings included, or a multi-line comment".

- **193b3a5 (repair round 1).** Auditor finding 6 (`07-auditor.md`) and escalation 4, answer A (`00-lead.md`): the RH5104 fix is withheld for a reported comment that has another comment before it on the line where it starts.
  - **New RH5104-007**, "The fix is withheld behind another comment on the comment's line" (SHARED-FIX-003). Its example holds both shapes:
    - `M(); /* a */ // b`: only `/* a */` is offered a fix. Once that fix has run, `// b` is offered its fix, and the order stays `/* a */`, `// b`.
    - `/* c */ /* d⏎   e */ M();`: `/* d … e */` stays reported without a fix, because `/* c */` is not reported and so nothing moves it off the line.
  - **RH5104-004's condition** now also says that no other comment stands before the comment on its start line (RH5104-007).
  - **RH5104-007 is in the ledger**, in sorted position.
  - **The rule page is unchanged.** Per AGENTS.md, "Code-fix coverage": a gap caused only by a comment is the general limitation stated in `general-notes.md`, and it is not repeated on the rule page.
  - **Checks.** The architecture spec checks give 50 passed and 1 failed. The failure is only the expected missing coverage for RH5104-007; the test implementer adds that test.

- **67a46d3 (repair round 1, code owners).** The implementer's repair round 1 (`04-implementer.md`, 78766ed) adds `RawStringLineShifts` and `RawStringLiteral` under `Reihitsu.Formatter.Pipeline.RawStringAlignment.Utilities`. I checked them against `sdd/README.md` ("Code owners"):
  - **They belong to FMT-RAW.** `RawStringLineShifts` is the single owner of the per-line shift of FMT-RAW-001, FMT-RAW-007, FMT-RAW-008, and FMT-RAW-009, including the lines that FMT-RAW-009 keeps. `RawStringLiteral` is the record that describes one literal for it. Under `sdd/README.md`, a type whose behavior a spec specifies belongs to that spec even when code outside it calls the type. Both are added to the `Code owners` row of `sdd/specs/formatter/raw-string-alignment.md` and to its row in `sdd/specs/README.md`.
  - **`indentation.md` needs no change.** `LayoutComputer` and `LayoutModel` call `RawStringLineShifts`, so a type can be an owner of only one spec and FMT-INDENT does not take it. Under `sdd/README.md`, the caller's spec lists the requirement IDs it relies on under `Uses`. `indentation.md` already lists FMT-RAW-001 and FMT-RAW-007, which are the only FMT-RAW IDs its text references. `SpecStructureTests` requires `Uses` to match the references exactly, so I added nothing.
  - **Checks.** The architecture spec checks (`SpecConsistencyTests`, `SpecStructureTests`, `RequirementCoverageTests`) now give 51 passed and 0 failed. Every new requirement is covered, RH5104-007 included.

- **This revision (merge of `origin/main` at 3973631, rule-page policy of thoenissen/Reihitsu#1462).** I re-read AGENTS.md ("Code-fix coverage", "Key conventions"), `documentation/rules/general-notes.md`, `.claude/commands/create-rule-doc.md`, and `squad/roles/spec-author.md`. Rule pages now carry no withheld fixes, exceptions, or corner cases. The page changes only when what is reported, or how to fix it, changes for the user in the normal case.
  - **`documentation/rules/RH5104.md`.** The `## Exceptions` bullet is removed, because it described a withheld fix. The page is back to `origin/main`'s text. The decisions stay in the spec: RH5104-006 (line begins inside a literal, an interpolated string, or a comment) and RH5104-007 (another comment before the comment on its line). Neither J1 (RH5104-005) nor the Q4 clarification (RH5104-002) changes the page's normal-case description or its Violation and Correction blocks.
  - **`documentation/rules/RH5601.md`.** The added clause is removed, because it narrowed the report in a corner case: whitespace after a raw string's opening quotes and the indentation of its lines. The page is back to `origin/main`'s text. Its existing sentence still covers the user's normal case: "Tabs inside literal content are treated as semantic content and are left untouched." The decision stays in RH5601-002.
  - **Result.** The run's diff against `origin/main` under `documentation/` is empty: `git diff origin/main --stat -- documentation/` prints nothing.

## thoenissen/Reihitsu#951

### What changed in this revision

- **Escalation 3 (A), latest revision.** FMT-RAW-007 has a third condition: a comment that starts a hole line takes the column FMT-INDENT-006 gives it, which is the column of the token it precedes, or one level deeper than a closing brace it precedes. That column is measured at the token's position after FMT-RAW-007 placed it.
  - The fourth example is the question's input: `// c` and `name` both end at column 21, and `}` at 17.
  - The coverage sentence no longer names comment lines.
  - FMT-INDENT-006 is added to `Uses`.
- **1B.** FMT-RAW-007 is rewritten, under the new title "Hole lines at the block column move only with the literal". It has two conditions, each with examples:
  - A line inside a hole that the indentation phase would give the block column of FMT-INDENT-002 is not given that column. It moves only by the closing marker's shift, and stays where it is in an aligned literal. Named starters: an expression token that no alignment requirement places, the `,` of an alignment clause, the `:` of a format clause, and the hole's closing `}`.
  - A line that an alignment requirement places relative to a token (FMT-INDENT-012 to FMT-INDENT-029, directly or through the construct around it) keeps that alignment, measured after the shift.
  - The new third example is a wrapped `string.Concat("a",⏎"b")` in a hole. I traced it on the current tree: `"b"` ends at column 38 in pass 1, and pass 2 is stable. So the expected output is today's output, as 1B requires.
- **Skeptic: FMT-RAW-007 vs FMT-RAW-008.** Confirmed. The old "stay as written" clause contradicted FMT-RAW-008's second example. FMT-RAW-007 now hands nested raw literals to FMT-RAW-008, and nested non-raw text and comments to FMT-RAW-009.
- **Skeptic: line starters.** The `,` and `:` clause starters are added. The skeptic observed that they flip like `name`, which means the indentation phase gives them the block column. Comment lines are now specified (escalation 3, above).
- **Skeptic: conflict with FMT-CLEAN-002.** Confirmed: a tab in front of a hole line is whitespace trivia. FMT-RAW-007 now states that the tab replacement of FMT-CLEAN-002 still applies to that whitespace, and the old "MUST NOT move it in any other way" is gone.
- **2A.** New FMT-RAW-009, "Text nested in a hole keeps its lines". A line that starts inside the text of a nested non-raw literal (`@"…"`, `$@"…"`), or inside a nested `/* */` comment, that began on an earlier line in a hole stays as written, also when the outer literal moves. I traced both examples on the current tree:
  - `b"}` moves from column 0 to 13 today, which changes the string's value;
  - the comment's `  b */` line moves from column 2 to 15 today.
- **FMT-RAW-006.** The exception now also names hole lines that an alignment requirement places (FMT-RAW-007). An aligned literal whose anchored hole line is out of alignment is realigned today, and that would otherwise contradict FMT-RAW-006.
- **FMT-INDENT-011 and the FMT-INDENT non-goal.** Narrowed to 1B. FMT-INDENT does not re-indent content and closing lines, nor hole lines that would take the FMT-INDENT-002 block column. Hole lines that an alignment requirement places keep that alignment.
- **`Uses` of `raw-string-alignment.md`.** Now: FMT-PIPE-001, FMT-PIPE-002, FMT-PIPE-004, FMT-INDENT-002, FMT-INDENT-007, FMT-INDENT-011, FMT-INDENT-012, FMT-INDENT-029, FMT-CLEAN-002. A range names only its ends.
- **Ledger.** FMT-RAW-009 is inserted in sorted position.

### Changed requirements (cumulative)

- FMT-RAW-006 — Aligned and single-line literals are left alone — clarified (limited to what the raw-string phase moves; R2).
- FMT-RAW-007 — Hole lines move only with the literal unless aligned within it — added (bounded by R1, R3).
- FMT-RAW-008 — A raw string nested in a hole is aligned in the same pass — added, unchanged in this revision.
- FMT-RAW-009 — Text nested in a hole keeps its lines — added.
- FMT-INDENT-011 — Disabled text and literal content are never re-indented — clarified (defers to FMT-RAW-007).
- FMT-INDENT-013 — A line inside a list element aligns with the element — clarified (the interpolation hole lays out its own lines; R1).
- Non-goals:
  - FMT-RAW: the non-goal on holes and nested literals is removed; the tab non-goal now points to thoenissen/Reihitsu#1432.
  - FMT-INDENT: the raw-string non-goal now excludes the hole lines that an alignment requirement places.

The coverage sentence of FMT-RAW-007 leaves two cases unspecified:

- lines that an earlier phase breaks inside a hole and that no alignment requirement places, such as the braces of `delegate { return 1; }` — the 1B consequence the skeptic named and the maintainer accepted;
- hole lines indented less than a leftward shift.

### Examples for the tests

All of these are formatter tests over two passes, under LF and CRLF.

- **FMT-RAW-007.**
  - The five spec examples: case 1, the aligned literal with `name` at column 25 (fails today), the wrapped `string.Concat` (a characterization test), the literal inside a lambda body (fails today), and the comment line `// c` above `name` (fails today).
  - A comment in front of the hole's `}`: it ends one level deeper than that `}`.
  - Also from the skeptic's inputs:
    - `$$"""` with `{{⏎name⏎}}}`;
    - `Hello {⏎name}`;
    - `{x⏎,10⏎:N2}`;
    - a leftward shift;
    - an inner `"""` that opens a hole line (combined with FMT-RAW-008).
- **FMT-RAW-008.** The two spec examples, plus depth 3, which needs two passes today.
- **FMT-RAW-009.** The two spec examples (both fail today), plus a `$@"…"` continuation line.
- **FMT-INDENT-011.** The inputs of FMT-RAW-007's first two examples.

### Open questions for the maintainer

_None._ The comment-line question is answered by escalation 3 (A) and carried out in FMT-RAW-007.

Note, not a question: today `RawStringAlignmentPhase` clamps a line that would move left of column 0 to column 0. FMT-RAW-007 leaves that case unspecified, through its coverage sentence.

## thoenissen/Reihitsu#1133

### What changed in this revision

- **3A.** RH5601-002 now also names the whitespace after the opening quotes of a multi-line raw string, interpolated or not, as the literal's text. The example gains `string y = """→` and `string z = $"""→`. The `$"""→` form is reported and fixed today (skeptic); the plain form is not reported.
- **Rule page.** `documentation/rules/RH5601.md` now says that the whitespace after a raw string's opening quotes, and the indentation of its lines, belong to the literal.

### Changed requirements (cumulative)

- RH5601-001 — Every tab in the code is reported — its `9501a4b` wording is kept (escalation 1, A). The example gains the hole-line tab `→+ 2}`, which is reported today and under that wording.
- RH5601-002 — Tabs in literals, comments, and disabled text are not reported — clarified. The whitespace after the opening quotes, the content-line indentation, and the closing-line indentation of a multi-line raw string, interpolated or not, are the literal's text.
- RH5602-001 — unchanged.
- RH5602-002 — Whitespace inside a multi-line string literal is not reported — clarified to "part of the text of" the literal; the example gains `{1}··`. No RH5602 behavior changes.
- Rule pages: `RH5601.md` has the clause above; `RH5602.md` is net unchanged against `9501a4b`.

### Examples for the tests

- **RH5601-002.**
  - `string x`: the issue's reproduction. It is reported today, and the fix breaks compilation.
  - `string z` (`$"""→`): reported today, so a reproduction for 3A.
  - `string w` and `string y`: characterization tests.
  - Also from the skeptic's inputs: the closing line after a hole line (`→→{x}` / `→→""";`), the same under `$$"""`, and mixed indentation `→··a` / `→··"""`.
- **RH5601-001.**
  - The hole-line tab `→+ 2}`: a characterization test.
  - Also these four, each reported and fixed today and kept so: `"""→;` after the end token of an interpolated raw string, `$"a"→;`, a `→$"""` continuation line, and the hole-line tab.
- **RH5602-002.** `{1}··`: a characterization test.

### Open questions for the maintainer

_None._

### Follow-up candidate (text for the maintainer before creation)

RH5601-001 and RH5602-001 still differ from SHARED-DIAG-003, which says that the whitespace before and after a literal is code, and so is an interpolation hole's expression.

- RH5601 does not report a tab directly in front of or behind a string or character literal, such as `string s =→"a"→;`, or `→"b";` on a continuation line.
- RH5602 does not report trailing whitespace on any line a string literal touches, such as `string s = "a";··` or `b";··` at the end of a verbatim literal. It also skips the code lines inside an interpolated string's holes, such as `{1··`.

Bringing them in line means that RH5602 can no longer skip whole lines through `FormattingTextAnalysisUtilities.GetStringLineIndices`, a helper RH5204 shares. Candidate title: "RH5601/RH5602: report whitespace next to literals and in interpolation holes (SHARED-DIAG-003)".

## thoenissen/Reihitsu#1146

### What changed in this revision

- **4B.** RH5104-006 (new title: "The fix is withheld when the line starts inside a literal, a comment, or a hole") now also covers a line that begins inside an interpolation hole, of `$"…"` or `$@"…"`, that spans several lines.
- **RH5104-004.** Its condition now reads "does not begin inside a literal, a comment, or an interpolation hole (RH5104-006)". Without that, RH5104-004 would still require the fix for the hole case.
- **Skeptic: RH5104-006's list of kinds.** Confirmed. The main clause already covered the missing kinds, but the list named only four. It now lists:
  - verbatim `@"…"` and `$@"…"`;
  - raw strings: plain, UTF-8, and interpolated;
  - `/* */` and `/** */`;
  - multi-line holes.

  The example gains `"""u8.Length + /* c */ x`, `$@"a⏎{x}b" + /* c */ x`, and `$"{⏎x}" + /* c */ x`. The plain `/* a⏎   b */ M(); // c` is not in the spec example, because `/* a…b */` is itself reported there and its fix is offered. It is for the test implementer, as the skeptic lists it.
- **Rule page.** `documentation/rules/RH5104.md`'s Exceptions bullet now also names the braces of an interpolation that spans several lines.

### Changed requirements (cumulative)

- RH5104-002 — Comments on lines without code, of directives, and in multi-line interpolations are not reported — clarified (Q4).
- RH5104-004 — The fix moves the comment above its line — clarified (condition tied to RH5104-006).
- RH5104-005 — No blank line is inserted above the moved comment — reworded (J1, SHARED-FIX-006).
- RH5104-006 — The fix is withheld when the line starts inside a literal or a comment — added (interpolated strings included, R4).
- RH5104-007 — The fix is withheld behind another comment on the comment's line — added (escalation 4, A).
- `Uses`: SHARED-COMMENT-002, SHARED-FIX-003, SHARED-FIX-006.

### Examples for the tests

- **RH5104-002.**
  - The spec's `/* a */ /* b */` and `/* c */ // d`.
  - Also from the skeptic's inputs:
    - `/* x⏎   y */ /* b */`;
    - `/** a */ /* b */` and `/** a */ // b` above a member;
    - a comment-only last line without a line break.
  - Still reported: `/* a */ M(); // b` and `/* a */ /* b */ M();`.
- **RH5104-005.** The spec's full before/after. The existing blank-line tests, such as `VerifyRelocatedCommentGetsPrecedingBlankLineWhenPreviousLineIsCode`, are updated to the new text.
- **RH5104-006.**
  - Each of the seven withheld forms in the spec example.
  - Also `/* a⏎   b */ M(); // c`, where the fix for `// c` is withheld.
  - The other side: `var a = /* c */ @"a⏎b" + x;`, which is fixed.

### Open questions for the maintainer

_None._

### Follow-up candidates

- The comment-reordering candidate is resolved in this run by RH5104-007 (escalation 4).
- **thoenissen/Reihitsu#1433 Q6 is superseded by J1** (from the skeptic's handover).

## Whole run

Files written, all under `/home/user/Reihitsu`:

- `sdd/specs/formatter/raw-string-alignment.md`
- `sdd/specs/README.md`
- `sdd/specs/formatter/indentation.md`
- `sdd/specs/analyzer/RH5601.md`
- `sdd/specs/analyzer/RH5602.md`
- `sdd/specs/analyzer/RH5104.md`
- `sdd/specs/requirement-ids.txt` (sorted inserts FMT-RAW-007, FMT-RAW-008, FMT-RAW-009, RH5104-006, RH5104-007)

`documentation/rules/RH5104.md`, `RH5601.md`, and `RH5602.md` are net unchanged against `origin/main`. `sdd/specs/README.md` gains the two new FMT-RAW code owners (`RawStringLineShifts`, `RawStringLiteral`). None of the files owned by concurrent runs were touched.

The traces in this revision ran with `bash scripts/trace.sh <fixture> --passes 3 --no-install` over fixtures in the session scratchpad, outside the repository: a wrapped argument in a hole, a nested verbatim string, a comment starting a hole line, and a comment continuation line.
