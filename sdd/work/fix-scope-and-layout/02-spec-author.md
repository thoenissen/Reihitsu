Role: spec author
Commit: 1be5f1b
Verdict: #1140: spec written; #1141: spec written; #1144: spec written; #1145: spec written; #1094: spec written; #1393: nothing to write; #1511: spec written (with #1140); #1561: nothing to write

Bug-fix lane, no change doc (lead's lane claim; triage on #1140 and #1393). Every change below is a clarification, or follows a maintainer answer recorded in `00-lead.md` (answers 1A, 2A) or a withheld rare class from *Decisions taken* (7, 8, 9). The prompt carried no intended fix, cause, or hoped-for verdict beyond the lead's recorded decisions. No code, test, or rule page was touched. `SpecStructureTests` and `SpecConsistencyTests` pass on the working tree (26/26); `RequirementCoverageTests` will fail for RH5103-005, RH5205-004, and SHARED-ATTR-021 until the test implementer adds their tests.

## thoenissen/Reihitsu#1140

Narrowed per decision 3: only RH5204, RH6023/RH6024, RH5405–RH5407/RH5403, and RH5417/RH3202 are aligned. RH5110-003, RH5206-003, SHARED-CHAIN-009, RH5101-003/RH5102-003, RH5111-004, RH5303–RH5308, RH5401, RH5404, RH5408, SHARED-EMPTY-003 (RH5412–RH5416) are left as they are.

### Changed requirements

- RH5204-006 — The fix re-indents only the reported line — changed (maintainer answer 1A): for every reported token, comment, or region directive, only the whitespace in front of the line's first element is replaced with the expected number of spaces; nothing else changes; a continuation line keeps its column. Title changed, ID kept. Intro sentence of `RH5204.md` and `Uses` (SHARED-FIX-006) updated.
- RH5403-003 — The fix lays out the reported block — changed (alignment to SHARED-FIX-006): only the reported block and the gap in front of its `{`; not the header, another block of the statement, or the blank lines in front; an `else if` block takes the column of `else` (SHARED-FIX-007). Intro sentence and `Uses` (SHARED-FIX-006, SHARED-FIX-007) updated.
- SHARED-BRACE-002 — The fix wraps the body in braces — changed (alignment to SHARED-FIX-006): wraps the reported body; MUST NOT change the condition, another body of the chain, or the blank lines in front. The column sentence ("When that `if` statement starts its line …") is unchanged, because its wording belongs to #1416 (decision 10). `Uses` gains SHARED-FIX-006.
- SHARED-OPSPACE-004 — see #1141.
- RH5417, RH3202, SHARED-BODY-003: no change. RH5417-003 restates no scope wider than SHARED-FIX-006; the RH5417 region instances (blank lines around a `#region` above or inside the member) and the shared-line instances are decided by SHARED-FIX-006 (*Blank lines*) and SHARED-FIX-007 (*Shared line*) as they stand. RH5417-004's `Why:` ("the fix lays out the member that owns the reported body") is rationale for an unchanged withheld class and stays.

### Examples for the tests

RH5204-006 (four examples: comment and declaration, body left as written, first type behind `///`, wrapped argument keeps its column; plus the region directive), RH5403-003 (second example: `int  x`, unreported `else if` block, `else if` block column, no blank line), SHARED-BRACE-002 (last example: unreported `else` block and no blank line above the `if`).

Existing tests tagged RH5204-006, RH5403-003, SHARED-BRACE-002, and the RH5405/RH5412 blank-line tests the skeptic lists (`VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration` and siblings) assert the old scope and have to change; the RH5412 one is outside the narrowed run and stays.

### Output questions

_None._

### Internal choices

- RH5204-006, continuation lines: I read answer 1A ("only the reported line moves") as deciding that a later line onto which the reported line's code continues — a wrapped argument, the second line of a multi-line comment — keeps its column (RH5203 then reports a wrapped argument on its own). Alternative: shift such lines with the reported line. The two give different output for `      return M(1,⏎               2);`, so if the lead does not read 1A as covering it, it is an output question and the last sentence of RH5204-006's normative paragraph and the *wrapped argument* example come out until answered. Recommendation: treat it as decided; shifting needs the fix to know which lines are anchored to the moved one, which is the indentation phase's knowledge.
- SHARED-BRACE-002's column sentence left untouched (decision 10, #1416); the `else if` column for the brace fixes comes from SHARED-FIX-007, as decided. Alternative: add an `else if` sentence there now, which would pre-empt #1416.
- Process flag: RH5204-006 and RH5203-003 change their normative text (sdd/README: a changed requirement needs a change doc). Both follow a recorded maintainer answer and the lead's lane decision says no change doc; the lead decides whether to record them in one.

### Follow-up candidates

_None_ beyond those already in `00-lead.md`.

## thoenissen/Reihitsu#1141

### Changed requirements

- SHARED-OPSPACE-004 — The fix puts one space on both sides — clarified: also MUST NOT change anything else (SHARED-FIX-006): a gap with a line break or comment stays as written, the operator stays on its line, operands (lambda, initializer, argument list) keep their layout. New example. `Uses` gains SHARED-FIX-006.
- RH6023-002, RH6024-003: no change (they refer to SHARED-OPSPACE-004 and restate no wider scope).

### Examples for the tests

SHARED-OPSPACE-004 (new example: `a+⏎b` → `a +⏎b`, a lambda and an array initializer left as written). The `/* c */` characterization input (decision 5) needs no requirement of its own; SHARED-OPSPACE-002 already says it is not reported.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1144

### Changed requirements

- RH5203-003 — The fix moves the argument into the alignment column — changed (maintainer answer 2A): a comment that starts its own line between the argument and the comma or `(` in front of it is moved into the same column; nothing else changes (blank lines, comment text, later lines of a multi-line comment, a trailing comment behind the comma). Coverage sentence: directive and disabled-text lines in front of the argument are left unspecified. New example; `Uses` gains SHARED-BLANK-002.
- RH5205-004 — No fix for a lambda with an attribute list — added (decision 7).
- RH5403-003 and SHARED-BRACE-002 — see #1140; together with SHARED-FIX-007 they decide the `else if` column (decision 10).
- No change for the shared-line instances (RH5417, RH3202: SHARED-FIX-007 *Shared line* already has the RH5417 example), RH5409–RH5411, RH3102, RH3201 Q3 (SHARED-FIX-007 first sentence), and FMT-PIPE-010 (decision 6; FMT-PIPE-017 states it).

### Examples for the tests

RH5203-003 (new example: `// two` at column 1), RH5205-004, RH5403-003 (second example).

### Output questions

_None._

### Internal choices

- RH5203-003 leaves directive and disabled-text lines in front of the argument unspecified rather than deciding them: the issue's instance is a comment, and the run has no measurement of a directive there. Alternative: state that they stay as written.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1145

### Changed requirements

- RH5103-003 — The fix moves the statement onto its own line — clarified: the preceding statement may be any statement, also one that ends with an embedded statement or a block, a labeled statement, or a local function (Q2, the guard-scope bug); in a block, when the preceding statement does not start its line and RH5103-005 does not apply, the indentation is that of the line on which it starts (the third of three statements on one line). Example extended with `M(); M(); M();`, `if (x > 0) M(); M();`, `foreach (…) { } M();`, `L: M(); M();`.
- RH5103-005 — No fix in a block whose opening brace is on the line — added (decision 8): in a block, no fix when the block's opening brace stands on the line on which the preceding statement starts. `Uses` gains FMT-INDENT-028.

### Examples for the tests

RH5103-003 (the four new rows), RH5103-005.

### Output questions

_None._

### Internal choices

- RH5103-005's class is "the block's opening brace is on the preceding statement's line", not the skeptic's "the preceding statement does not start its line": the latter would also withhold the fix for the third statement of `M(); M(); M();`, which today gets the line's indentation correctly and is common. The chosen condition holds exactly the Q4 inputs (`void N() { M(); M(); }`, `() => { M(); M(); }`) and other one-line blocks, all reported by RH5417 or RH5403, so it stays a rare class.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1094

### Changed requirements

- SHARED-ATTR-021 — The separate-line fix is withheld when the enclosing brace is off its depth column — added (decision 9, Q3). Recorded in the shared spec because `TargetAttributePlacementCodeFixProviderBase` implements the fix for every placement rule; SHARED-ATTR-008's coverage sentence points to it. `Uses` of `attribute-targets.md` gains FMT-INDENT-028.
- RH5501-004, RH5503-004, RH5505-004, RH5507-004, RH5509-004, RH5511-004, RH5513-005, RH5515-004, RH5517-004, RH5519-004, RH5521-004, RH5525-004, RH5527-004, RH5530-005 — clarified: each withheld-fix requirement now names SHARED-ATTR-021 next to SHARED-ATTR-011 and SHARED-ATTR-012 (title, sentence, `Why:`, `Uses`). Rule pages unchanged (withheld fix).
- Q2 (shared line) needs no spec change: SHARED-FIX-007 *Shared line* decides it. Q4 is a characterization test (decision 9).

### Examples for the tests

SHARED-ATTR-021 (RH5513, local function in a lambda block aligned to `(` in a wrapped argument).

### Output questions

_None._

### Internal choices

- SHARED-ATTR-021 measures the indentation of the line the brace stands on, so a brace that ends its owner's line (`void M() {`) on a line at its depth stays covered as today. Alternative: the brace's own column, which would withhold the fix for every K&R-style body.
- One shared requirement referenced by all fourteen placement rules, rather than a RH5513-only requirement: the guard lives in the shared base, so every rule using SHARED-ATTR-008 withholds alike.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1393

Nothing to write: RH5104-005 already states the case and its example has this shape (skeptic); answer 3A adds only a characterization test, which tags RH5104-005.

## thoenissen/Reihitsu#1511

Covered by RH5204-006 (#1140 section): the fix replaces only the reported line's indentation, so a first type behind a `///` comment, a directive, a `//` comment, or nothing makes progress and is not otherwise formatted. The example *first type of a file, behind a documentation comment* seeds the test; the skeptic's other rows (directive, no trivia, `//` header, namespace, misindented first comment) are the same requirement.

## thoenissen/Reihitsu#1561

Nothing to write: RH5204-001 already judges a line by its first token as SHARED-BLANK-008 defines it, which a token behind the closing line of a multi-line comment is not (decision 11, answer 4A); the fix is in the analyzer. Its tests tag RH5204-001.

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests"`: 26 passed.
- New IDs inserted in `sdd/specs/requirement-ids.txt`: RH5103-005, RH5205-004, SHARED-ATTR-021. No ID retired or renumbered; no code owner changed, so `sdd/specs/README.md` is unchanged.
