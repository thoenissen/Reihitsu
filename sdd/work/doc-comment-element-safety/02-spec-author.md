Role: spec author
Commit: e29c7bf
Verdict: #1329: spec written; #1389: spec written; #1241: spec written; #1344: spec written; #1242: spec written

This handover replaces `01-spec-author.md`. It carries the maintainer's answers (the section "Maintainer answers to the spec author's questions" in `00-lead.md`: 1C, 2(ii), 3A, 4A, 5B, 6B) into the specs. Every answer is written as a clarification of a decided principle: FMT-PIPE-002 and the stranded-comment principle (FMT-DOC-003 / SHARED-COMMENT-001) for the formatter, and SHARED-FIX-003 and P19 (SHARED-DOC-017) for the fixes. None of them needed a spec change I would call a new intended behavior, so no change doc and no open question remain. No rule reports more inputs than today. `shared/code-fixes.md`, `formatter/pipeline.md`, `sdd/README.md`, `squad/`, and the files the parallel runs own are untouched.

The reproduction facts are those in `01-spec-author.md`. I re-ran nothing for this round, apart from the architecture checks under *Whole run*. `⏎` is a line break.

## thoenissen/Reihitsu#1329

### Changed requirements

- FMT-DOC-013 — Documentation with an element without an end tag is left as written — **added** (answer 1C).
  - When any XML element of a `///` comment, at the top level or nested, has no end tag, the formatter leaves every line of that comment as written. FMT-DOC-005 to FMT-DOC-011 do not apply to it, not even to its complete elements, and the code that follows stays code.
  - Coverage: a comment that starts its own line. An element closed by an end tag of another name (`<summary>S</returns>`) counts as having an end tag here, so FMT-DOC-008 still rebuilds it, as it does today. A comment behind code that FMT-DOC-001 moves is not covered.
- FMT-DOC-005, FMT-DOC-006, FMT-DOC-008, FMT-DOC-009 — **clarified**: each now states that it does not apply to a comment that FMT-DOC-003 or FMT-DOC-013 leaves as written.

### Examples for the tests

FMT-DOC-013. Its example holds both shapes of the issue in one class, unchanged, the one-line `<summary>` of `B` included; FMT-PIPE-002 and FMT-PIPE-003 cover the declaration staying code. Today's output is in `01-spec-author.md`.

## thoenissen/Reihitsu#1389

### Changed requirements

- FMT-DOC-003 — Stranded documentation stays as it was written — **changed** (answer 2(ii); heading changed from "where" to "as").
  - A stranded comment (followed by `;`, `)`, `]`, or `}`) is not moved, and the text of every line is left as written: FMT-DOC-005 to FMT-DOC-011 do not apply.
  - Why: the compiler does not read such a comment as documentation, so it is an ordinary comment whose text FMT-PIPE-002 keeps.
  - New examples: `int A; /// <summary>end</summary>` before `}`, and `int A; ///<remarks>a⏎/// b</remarks>` before `}`, both unchanged.
  - This also stops the FMT-DOC-005 separator that today turns `int A; ///<remarks>a</remarks>` before `}` into `/// <remarks>a</remarks>`. The maintainer named that consequence in answer 2.
- FMT-CLEAN-009 — unchanged; the BOM instance is a plain bug.

### Examples for the tests

- FMT-DOC-003: both new examples, plus the Q-D1 shapes from thoenissen/Reihitsu#1367, all unchanged:
  - `var x = 1; /// <summary>stray</summary>` before `}`;
  - `int A /// <summary>stray</summary>` with `;` on the next line;
  - `M(1 /// <remarks>a⏎/// b</remarks>⏎);`.
- FMT-CLEAN-009: `U+FEFF⏎class C{int x;}` keeps the BOM.
- FMT-PIPE-002 may be tagged alongside both.
- Watch for existing formatter tests that may assert the old separator on a stranded comment. A quick search found comments behind code in `TrailingDocumentationCommentPreservationTests`, `ReihitsuFormatterTests`, `OneLineClassBodyMembersFullPipelineTests`, and `BlankLineRequirementTests`. I did not check whether a member follows each of them, which would make it a relocated comment rather than a stranded one.

## thoenissen/Reihitsu#1241

### Changed requirements

- SHARED-DOC-017 — **clarified**, as in `01-spec-author.md`, with one change for answer 6B.
  - The RH8309 fix is now withheld when **any** top-level element of the reported comment has no end tag of its own, whether or not the fix would move it.
  - The RH8309 coverage sentence from round 1 is removed, and the Why says why.
  - The RH8309 example has a third comment: an unclosed `<seealso>` that keeps its position.
- RH8107-007 and RH8202-004 — **clarified** to "no end tag of its own" (SHARED-DOC-017), as in round 1. Answer 4A confirms that the end-tag-of-another-name instances are in this run: today `/// <remarks><returns>Nothing</remarks>` and `/// <remarks><value>The size</remarks>` are fixed to `/// <remarks>`.
- RH8204-002 (location of an element without an end tag of its own), and the coverage sentences of RH8204-003, RH8305-004, RH8307-004, and RH8309-004 — as in round 1. The RH8309-004 sentence now reads "any top-level element".
- Rule pages: `documentation/rules/RH8107.md`, `RH8202.md`, `RH8204.md`, `RH8305.md`, `RH8307.md` as in round 1. The `RH8309.md` note now says "one of the elements".

### Examples for the tests

- SHARED-DOC-017, one test per rule example:
  - RH8107: `<remarks><returns>Nothing</remarks>`;
  - RH8202: the analogue `<remarks><value>The size</remarks>`, plus the existing `<value>The size` example;
  - RH8204, RH8305, RH8307: both shapes each;
  - RH8309: all three comments.
- RH8107-007 and RH8202-004: the end-tag-of-another-name inputs.
- RH8204-002: its second example (locations).
- Positive cases that must keep their fix: SHARED-DOC-017's `<returns/>` example, and the RH8204-003, RH8305-004, RH8307-004, and RH8309-004 examples.

## thoenissen/Reihitsu#1344

### Changed requirements

- RH8304-004 — The fix moves the element onto a new `///` line — **clarified** (answer 3A).
  - The inserted indentation is the white space at the start of the line. With code in front of `///` (a field declaration, an opening brace), the fix inserts only that white space and never repeats the code.
  - Coverage widened to a comment whose first line has code in front of its `///`.
  - New example: `int A; /// <summary>S</summary><remarks>R</remarks>` → `    int A; /// <summary>S</summary>⏎    /// <remarks>R</remarks>`, and `{ /// <summary>S</summary><remarks>R</remarks>` → `{ /// <summary>S</summary>⏎/// <remarks>R</remarks>`.
  - No withhold, and no rule-page note.
- RH8309-005 — The fix is withheld for text or an XML comment beside the elements — **added** in round 1, **scope widened** before any merge (answer 5B).
  - Text with a non-white-space character, or an XML comment, at the top level of the reported comment withholds the fix: between two elements, before the first, or after the last.
  - A CDATA section or a processing instruction there is not covered.
  - The example now has four comments: text between, an XML comment between, `/// Intro` before the first element, and `/// Outro` after the last.
- `documentation/rules/RH8309.md` — the note now says "when the comment holds text outside the elements".

### Examples for the tests

- RH8304-004: the new example, both classes. Today it produces CS0102 and `invalid-result`.
- RH8309-005: each of its four comments, reported and without a fix. All four are reported and fixed today.

## thoenissen/Reihitsu#1242

### Changed requirements

Unchanged from `01-spec-author.md`:
- `DocumentationCommentCodeFixUtilities` is a code owner of `shared/element-documentation.md`.
- SHARED-DOC-018 is added.
- RH8107-006 references SHARED-DOC-018.
- RH8202 `Uses` lists SHARED-DOC-017 and SHARED-DOC-018.
- The index in `sdd/specs/README.md` is updated.

### Examples for the tests

SHARED-DOC-018: tag the existing tests of RH8107-006's example and of RH8202-003. No assertion changes.

## Open questions for the maintainer

Found in passing, not blocking:

1. **A comment behind code with an element without an end tag, when FMT-DOC-001 moves the comment.**
   - Input: `class C⏎{⏎    int A; /// <summary>S⏎    int B;⏎}`.
   - Real output: not run in this round.
   - Why it is open: FMT-DOC-013 covers only a comment that starts its own line, and FMT-DOC-004 says a relocated comment is normalized. So whether the moved comment is left as written (beyond the move) is not decided, and deciding it would be the formatter's relocation behavior, which this run must not extend.
   - Touches FMT-DOC-001, FMT-DOC-004, FMT-DOC-013.
   - Readings: (a) relocate it and leave its text as written, in the spirit of answer 1C; (b) leave the whole comment where it is, behind `int A;`.
   - Recommendation: (a). The comment still documents `B`, and answer 1C leaves only its text alone.

## Whole run

- Files changed in this round:
  - `sdd/specs/formatter/documentation-comments.md`;
  - `sdd/specs/analyzer/RH8304.md`, `RH8309.md`;
  - `sdd/specs/shared/element-documentation.md`;
  - `sdd/specs/requirement-ids.txt` (FMT-DOC-013 inserted at its sorted position);
  - `documentation/rules/RH8309.md`;
  - this handover.
- Files changed in round 1 and kept: the RH8107, RH8202, RH8204, RH8305, and RH8307 specs and rule pages, and `sdd/specs/README.md`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage|FullyQualifiedName~RuleDocumentation"`: 50 passed, 1 failed.
  - `EveryRequirementOfAnEnforcedSpecIsCovered` fails with `RH8309-005`, `FMT-DOC-013`, and `SHARED-DOC-018`: "no test covers it".
  - The test implementer closes all three.
- Follow-up candidate for the lead: `squad/decisions/2026-10-19-rh8202-uses-rh8107-fix-requirements.md` is superseded by the RH8202 `Uses` change; `squad/` is off-limits here.
