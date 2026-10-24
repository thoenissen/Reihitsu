Role: spec author
Commit: 14faca6
Verdict: #951: spec written; #1133: spec written; #1146: spec written

Inputs: thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146 and their comments (read through the GitHub MCP server), `00-lead.md`, the specs named below. The #951 body names a suspected cause and an intended mechanism (the indentation phase skipping tokens, `ReplaceNodes` dropping the inner literal); the #1146 body proposes RH5104 rewordings for Q3, Q5, and Q6. I did not use either: the specs state observable output only, and Q3, Q5, and Q6 are out of scope (thoenissen/Reihitsu#1433). `sdd/specs/shared/code-fixes.md` is referenced and was not edited. No change doc.

Check run (repeated after the revision, same result): `scripts/test.sh --project architecture` filtered to `SpecConsistencyTests`, `SpecStructureTests`, `RuleDocumentationConventionTests`, and `RequirementCoverageTests`. 50 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for the three new IDs FMT-RAW-007, FMT-RAW-008, and RH5104-006. The test implementer adds those tests.

## thoenissen/Reihitsu#951

### Changed requirements

- FMT-RAW-007: the lines of an interpolation hole move with the literal. Added. In a multi-line interpolated raw string, a line that starts inside a hole moves by the closing marker's delta and is not moved any other way. In an aligned literal it stays as written. A coverage sentence leaves out hole lines indented less than a leftward shift.
- FMT-RAW-008: a raw string nested in a hole is aligned in the same pass. Added. This holds at every depth.
- FMT-RAW-006: aligned and single-line literals are left alone. Clarified with an exception for a literal nested in a hole, which is aligned on its own (FMT-RAW-008). Without it, FMT-RAW-008's second example would contradict FMT-RAW-006.
- FMT-INDENT-011: disabled text and literal content are never re-indented. Clarified. The indentation phase must not re-indent any line after the opening line of a multi-line raw string, hole lines included (FMT-RAW-007). FMT-RAW-007 was added to `Uses`.
- Non-goals:
  - The FMT-RAW non-goal on holes and nested literals is removed; FMT-RAW-007 and FMT-RAW-008 now specify that behavior.
  - The FMT-RAW non-goal on tab indentation now points to thoenissen/Reihitsu#1432 (out of scope; no tab normalization).
  - The FMT-INDENT non-goal on raw strings now lists hole lines as FMT-RAW's.
  - The intro sentence of `raw-string-alignment.md` names hole lines.

### Examples for the tests

- FMT-RAW-007: the issue's case 1 (the hole moves 13 columns), plus an aligned literal with a hole line at an unusual column, which is left unchanged. Both should be checked over two passes (FMT-PIPE-004), as the maintainer asked.
- FMT-RAW-008: the issue's case 2 (both literals misaligned, one pass), plus an aligned outer literal around a misaligned inner one.
- FMT-RAW-006: the nested exception, covered by FMT-RAW-008's second example.
- FMT-INDENT-011: hole lines are not re-indented, using the same inputs as FMT-RAW-007.

### Open questions for the maintainer

_None._

Note, not a question: today `RawStringAlignmentPhase` clamps a line that would move left of column 0 to column 0. FMT-RAW-007 leaves that case unspecified on purpose, through its coverage sentence.

## thoenissen/Reihitsu#1133

Revised after escalation 1 (`00-lead.md`, "Escalations"). The maintainer chose A, narrow: only whitespace that is part of a literal's text is exempt. Whitespace next to a literal and in a hole's expression keeps today's behavior, and there are no new diagnostics there. My first revision (5572ea1) applied SHARED-DIAG-003 in full; that is withdrawn.

### Changed requirements

- RH5601-001: every tab in the code is reported. Normative text and `Why:` are restored to their `9501a4b` wording, so a tab next to a string or character literal stays exempt. The example gains one line: a tab that indents a hole line (`→+ 2}`) in an interpolated raw string. That tab is reported today and by the restored wording, because no code token precedes it and the next code token, `+`, is not a literal.
- RH5601-002: tabs in literals, comments, and disabled text are not reported. Clarified. The indentation of a multi-line raw string's content lines and closing line, interpolated or not, is the literal's text. This covers the issue's input (`$"""`, `→→a`, `→→"""`); the non-interpolated twin is in the same example. `Uses` keeps SHARED-DIAG-003, which RH5601-002 references.
- RH5602-001: unchanged; it is restored to its `9501a4b` text.
- RH5602-002: whitespace inside a multi-line string literal is not reported. Clarified to "part of the text of" a multi-line literal, for an interpolated string the text between and around its holes. The example gains `{1}··` in an interpolated raw string. RH5602-001's touched-line rule already exempts that today, so no behavior changes for RH5602. `Uses` has its `9501a4b` content again (SHARED-DIAG-003 removed, since no RH5602 text references it now).
- Rule pages:
  - `documentation/rules/RH5601.md` keeps one added clause: the indentation of a raw string's lines, the closing line included, belongs to the literal.
  - `documentation/rules/RH5602.md` is restored to its `9501a4b` text.

### Examples for the tests

- RH5601-002:
  - `string x`, the tab-indented interpolated raw literal. This is the issue's reproduction: reported today, and the fix breaks compilation.
  - `string w`, the non-interpolated twin. Not reported today; a characterization test.
- RH5601-001: the hole-line tab `→+ 2}` is reported. A characterization test of today's behavior.
- RH5602-002: `{1}··` inside an interpolated raw string is not reported. A characterization test.

### Open questions for the maintainer

_None._

### Follow-up candidate (text for the maintainer before creation)

RH5601-001 and RH5602-001 still differ from SHARED-DIAG-003, which says that the whitespace before and after a literal is code, and so is an interpolation hole's expression. Where they differ:

- RH5601 does not report a tab directly in front of or behind a string or character literal, such as `string s =→"a"→;`, or `→"b";` on a continuation line.
- RH5602 does not report trailing whitespace on any line a string literal touches, such as `string s = "a";··` or `b";··` at the end of a verbatim literal. It also skips the code lines inside an interpolated string's holes, such as `{1··`.

Bringing them in line means that RH5602 can no longer skip whole lines through `FormattingTextAnalysisUtilities.GetStringLineIndices`, a helper RH5204 shares. A candidate title: "RH5601/RH5602: report whitespace next to literals and in interpolation holes (SHARED-DIAG-003)".

## thoenissen/Reihitsu#1146

### Changed requirements

- RH5104-002 (heading now "Comments on lines without code, of directives, and in multi-line interpolations are not reported"): clarified for Q4. A comment that shares none of its lines with a token (only whitespace or other comments around it) is not reported. RH5104-001 already implied this. Examples: `/* a */ /* b */` and `/* c */ // d`, each on a line of its own above `M();`.
- RH5104-005 (title now "No blank line is inserted above the moved comment"): reworded per J1. The fix inserts no blank line and keeps the existing blank lines above the line (SHARED-FIX-006). The old insertion and exemption list is gone.
- RH5104-006 (the fix is withheld when the line starts inside a literal or a comment): added for Q1.
  - The fix is withheld when the comment's starting line begins inside the text of a literal or inside a multi-line comment: verbatim, raw, or interpolated raw string, or a `/** */` documentation comment (SHARED-FIX-003).
  - When that line begins outside them, the fix is offered as RH5104-004 states, even if such a literal starts on that line.
  - SHARED-FIX-003 and SHARED-FIX-006 were added to `Uses`.
- Rule page: `documentation/rules/RH5104.md` gains an `## Exceptions` bullet. No fix is offered when the comment's line begins inside a multi-line string literal or a multi-line comment. The `### Correction` block is unchanged, because the J1 change does not affect it: every blank line in it was already in the violation.

### Examples for the tests

- RH5104-002: `/* a */ /* b */` and `/* c */ // d` on code-free lines are not reported (Q4; both are reported today).
- RH5104-005: the full before/after. `// second` gets no blank line, and `// fourth` keeps the blank line written above its line.
- RH5104-006:
  - Each of the four withheld forms is reported with no code fix offered: `/** … */ void M() { } // c`, a verbatim string, a raw string, and an interpolated raw string, each with `+ /* c */ x` on its last line.
  - The other side of the condition: `var a = /* c */ @"a` followed by `b" + x;` is fixed to `/* c */` above `var a = @"a`.

### Open questions for the maintainer

_None._

Follow-up candidate, not run and derived only from the wording of RH5104-004: two inline comments on one code line, as in `M(); /* a */ // b`. Fixing `// b` first and then `/* a */` would give `// b` above `/* a */`, a reordering. This is outside Q1 and Q4. It is for the skeptic or the test implementer to check before anyone raises it.

## Whole run

Files written, all under `/home/user/Reihitsu`:

- `sdd/specs/formatter/raw-string-alignment.md`
- `sdd/specs/formatter/indentation.md`
- `sdd/specs/analyzer/RH5601.md`
- `sdd/specs/analyzer/RH5602.md`
- `sdd/specs/analyzer/RH5104.md`
- `sdd/specs/requirement-ids.txt` (sorted inserts FMT-RAW-007, FMT-RAW-008, RH5104-006)
- `documentation/rules/RH5601.md`
- `documentation/rules/RH5602.md` (net unchanged against `9501a4b` after the revision)
- `documentation/rules/RH5104.md`

No code owners changed, so `sdd/specs/README.md` is untouched. None of the files owned by concurrent runs were touched.
