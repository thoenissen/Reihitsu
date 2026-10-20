Role: spec author
Commit: 6f1bc7f (skeptic handover on top of 5278e6e)
Verdict: spec written

This is the repair round after the skeptic's `02-skeptic.md` (verdict: proceed narrower). I re-derived every finding against the code with my scratch harness (analyzer, first code action, Fix All; DocumentationMode `Diagnose`, no preprocessor symbols). I treated the skeptic's suggested changes as suggestions only. This file's **Open questions to file** replaces the section of the same name in `01-spec-author.md`.

## Findings and what I did

- **F1** — Agreed. RH8307 reports `/** <summary>Total⏎ * sum</summary> */`; the fix is a no-op, and FMT-DOC-012 leaves the comment alone. `/** */` is removed from RH8307-001's coverage and example and filed as open (P4). The no-op stays under P7.
- **F2** — Reproduced. RH8302's fix removes the blank line before `#region R`, RH5031 then reports `#region R`, and RH5031's fix inserts the blank line again. A run of blank lines followed by `#region` or `#endregion` is excluded from RH8302-001's coverage. Filed as P7 (SHARED-FIX-004).
- **F3** — Agreed: with no symbols defined, `#if DEBUG` turns `int E;` into disabled text. The example now uses `#if !NEVER`.
- **F4** — Reproduced every shape. The coverage sentences now exclude a documentation comment that starts behind code on its line: RH8301-001, RH8302-001, RH8303-001, RH8304-004, RH8305-004, RH8307-004, and RH8309-004.
  - RH8301-002 no longer specifies `///Behind`, and the wrong sentence in its Why ("Only a line that a `///` comment starts carries the documentation exterior") is gone.
  - Questions filed: RH8301, RH8302, and RH8303 open; RH8304 P1; RH8305, RH8307, and RH8309 P9.
- **F5** — Reproduced: `/// <summary>S⏎    int A;` becomes `/// int A;`, and the same happens for RH8307 and RH8309.
  - RH8305-001 and RH8307-001 already required "closed by its own end tag". RH8309-001 now requires "each top-level element empty or closed by its own end tag"; RH8309-004 inherits that.
  - Filed as P1. An end tag with another name (`<summary>S</returns>`, reported, fix keeps `</returns>`) is filed as P19.
- **F6** — Reproduced: `Text <b>bold⏎/// still bold</b>` becomes `<b>⏎/// bold⏎/// still bold⏎/// </b>`. Inline formatting elements are excluded from RH8307-001 and RH8307-004, as FMT-DOC-009 excludes them. Filed as open (P4, #961).
- **F7** — Reproduced: `&#32;` and `&#160;` between elements are reported, and `&amp;` is not.
  - The behavior is plausible (the reference stands for white space), so I specified it rather than filing it.
  - RH8304-001 now says a character reference that stands for white space counts as white space, with an example. RH8304-002 is qualified to match.
  - The leftover `&#32;` that RH8308 then reports is another rule's diagnostic left behind, which is a non-goal of SHARED-FIX.
- **F8** — The RH8307 non-goal now reads "How many lines a summary spans — RH8305".
- **F9** — Reproduced: RH8305 does not report `<Summary>S</Summary>`, while the formatter expands it (FMT-DOC-008 matches the name case-insensitively). `<summary xmlns="urn:x">` is reported.
  - RH8305 Q1 is reclassified as open, citing both P8 and P4, because the formatter's merged FMT-DOC-008 decides tag names the other way.
  - RH8306 Q1 and RH8309 Q1 stay P8, since no formatter requirement is involved. The `xmlns="…"` shape is added to each.
- **F10** — Each of the four points is handled:
  - RH8305 whitespace-only summary: the fix and the formatter both produce `<summary>⏎/// ⏎/// </summary>`, and the formatter is stable on it. That output seems to contradict FMT-DOC-011, which drops empty leading and trailing content lines, so I filed it as open.
  - RH8306: Q3 now includes XML comments, CDATA, and processing instructions.
  - RH8306 partial properties: reproduced (two diagnostics for one property). Excluded from coverage ("a property declared in one part") and filed as P15.
  - RH8309: Q4 and Q7 gain the shapes after an attribute list and on an accessor. The comment on an accessor is now named in RH8309-001's coverage.
- **F11** — Agreed. I removed the RH8501 non-goal "Whether a justification is a good reason". Q2 (`<Pending>`) stays open.
- **F12** — Agreed that RH8402-005 cannot state the replacement while the question is open.
  - RH8402-005 now specifies only insertion: in front of leading comments, or in place of leading white space. Its coverage is a file whose leading comments do not contain the word `copyright`.
  - The "old header is replaced" example is gone; a "leading blank line is replaced" example takes its place.
  - Replacing a comment that contains "copyright" is now open question Q1. Deleting other comments together with it is P1, and removing the blank line is P9.
  - The test `MismatchedConfiguredHeader` (and the block-comment variant) stays untagged as a characterization test.
- **F13** — Agreed for the free-text and `<!-- c -->` shapes of RH8309 Q6. The fix moves elements around content the user wrote, so that content ends up next to other elements, which AGENTS.md treats as unsafe. Those two shapes are marked P1; the custom-element shape stays open.
- **F14** — P8c defines the summary text: rendered text counts, markup, comments, CDATA, and PIs do not, and references are decoded. RH8306 judges that text, so I marked the nested-element and reference parts of Q3 as decided by P8c. The XML comment, CDATA, and PI part changes the verdict only where it splits or joins words, so it is marked P8c as well. The lead may downgrade the mark.
- **Wording** — RH8301-004 now reads "the content, from its first character on". RH8306-001 now reads "with or without empty elements such as `<see cref="…"/>`".

Checks:
- Every `csharp` block of every spec still matches the real analyzer output and Fix All result. RH8402 was run with its configuration and the file name `C.cs`; its -001 block was checked without a configuration.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` passes 26/26.
- No rule page needed another change.

Tests that must stay untagged for the requirement whose coverage excludes their input:
- RH8402 `MismatchedConfiguredHeader` and `MismatchedBlockCommentHeader` for RH8402-005;
- `RH8401…VerifyNoDiagnosticsWhenDocumentationModeIsNone`;
- `RH8303…VerifyDocumentationAfterSwitchLabelProducesDiagnostic`.

## Changed requirements

Clarified (text only, no new IDs): RH8301-001, RH8301-002, RH8301-004, RH8302-001, RH8303-001, RH8304-001, RH8304-002, RH8304-004, RH8305-004, RH8306-001, RH8307-001, RH8307-004, RH8309-001, RH8309-004, RH8402-005. Also changed: the RH8307 and RH8501 non-goals.

## Examples for the tests

Unchanged from `01-spec-author.md`. The RH8302-001 "Fifth" case now uses `#if !NEVER`, and RH8307-001 no longer has a `/** */` case.

## Open questions to file

One issue per spec, `SDD open questions: <spec path>`, listed in a comment on #912 (which is at its sub-issue limit). `⏎` is a line break.

### `SDD open questions: sdd/specs/analyzer/RH8301.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `///InBody` on its own line inside a method body.
  - Real output: reported, and the fix writes `/// InBody`.
  - Why it looks wrong: the comment documents nothing, so P13 makes it an ordinary comment.
  - Touches: RH8301-001.
- **Q2 (open).**
  - Input: `int a; ///Behind⏎    int b;`.
  - Real output: not reported. The `///` carries a documentation exterior and documents `b` (SHARED-DOC-004). FMT-DOC-001 and FMT-DOC-004 relocate it and normalize the separator, so code without diagnostics changes on formatting.
  - Readings: (a) report it as RH8301's own gap (P11); (b) leave lines that do not start with `///` alone.
  - Touches: RH8301-001, FMT-DOC-001, FMT-DOC-004.

### `SDD open questions: sdd/specs/analyzer/RH8302.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `/// inbody`, a blank line, `int a = 0;` in a method body; and `/// stranded`, a blank line, the type's `}`.
  - Real output: reported, and the fix removes the blank line.
  - Why it looks wrong: neither comment documents anything.
  - Touches: RH8302-001.
- **Q2 — Decided by principle P7 (#912)** (SHARED-FIX-004).
  - Input: `/// <summary>Sixth</summary>⏎⏎    #region R⏎⏎    int F;⏎…`.
  - Real output: the RH8302 fix removes the blank line. RH5031 then reports `#region R`, and its fix inserts the same blank line again. The formatter keeps an existing blank line there but does not insert one.
  - Touches: RH8302-001, RH5031, FMT-BLANK-010, FMT-BLANK-013.
- **Q3 (open).**
  - Input: `int Z; /// <summary>D</summary>⏎⏎    int D;`.
  - Real output: not reported, because no line starts with `///`. `format.sh` moves the comment and removes the blank line, so code without diagnostics changes on formatting.
  - Readings: (a) report it; (b) only comments that start their line.
  - Touches: RH8302-001, FMT-DOC-001, FMT-BLANK-010.

### `SDD open questions: sdd/specs/analyzer/RH8303.md`
- **Q1 — Decided by principle P13 (#912).** Also an instance of #1148, and of P4 in the switch case.
  - Inputs and real output: RH8303 reports each of these, and the fix inserts a blank line:
    - `int a = 0;⏎/// inbody⏎a++;` in a method body;
    - `case 1:⏎/// incase⏎break;`, where the formatter inserts no blank line;
    - `N(1,⏎/// arg⏎2);`;
    - `[Obsolete]⏎/// <summary>G</summary>⏎int G;`.
  - Why it looks wrong: none of these documents a declaration.
  - Touches: RH8303-001. `VerifyDocumentationAfterSwitchLabelProducesDiagnostic` pins the case-label shape.
- **Q2 (open).**
  - Input: `int A; /// <summary>⏎    /// B</summary>⏎    int B;`.
  - Real output: reported on the second line's `///`. The fix inserts a blank line inside the comment, so the compiler sees two documentation comments, and RH8302 cannot see that blank line.
  - Readings: (a) not reported, because the comment does not start its line; (b) reported on its first line, and the fix separates the comment from the code before it.
  - Touches: RH8303-001, RH8303-004, RH8303-005.

### `SDD open questions: sdd/specs/analyzer/RH8304.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `/// <a/><b/>` inside a method body.
  - Real output: reported, and the fix splits the line.
  - Touches: RH8304-001.
- **Q2 — Decided by principle P1 (#912)** (SHARED-FIX-003, compile error).
  - Inputs and real output:
    - `int A; /// <summary>S</summary><remarks>R</remarks>⏎    int B;` becomes `int A; /// <summary>S</summary>⏎    int A; /// <remarks>R</remarks>`, which declares `A` twice (CS0102);
    - `class C⏎{ /// <summary>S</summary><remarks>R</remarks>` gets a second `{` line, and the result does not parse.
  - The fix copies everything in front of `///` on the line, not only the indentation (`DocumentationCommentUtilities.GetContinuationPrefix`; relevant to #1242).
  - Touches: RH8304-004.

### `SDD open questions: sdd/specs/analyzer/RH8305.md`
- **Q1 (open; P8 for the analyzer, P4 against FMT-DOC-008).** Tag names, in three shapes:
  - `/// <x:summary xmlns:x="urn:x">H</x:summary>` is reported and expanded;
  - `/// <summary xmlns="urn:x">S</summary>` is reported and expanded;
  - `/// <Summary>S</Summary>` is not reported, but the formatter expands it, because FMT-DOC-008 matches the name case-insensitively.
  - P8 says tag names are case-sensitive and namespace-free. Applying that to RH8305 alone would widen its disagreement with the formatter.
  - Readings: (a) RH8305 and FMT-DOC-008 both follow P8; (b) both match `summary` case-insensitively and ignore the namespace.
  - Touches: RH8305-001, FMT-DOC-008.
- **Q2 — Decided by principle P20 (#912).**
  - Input: a three-line `<summary>J1</summary>` followed by `/// <summary>J2</summary>` in one comment.
  - Real output: the second summary is not reported.
  - Touches: RH8305-001.
- **Q3 — Decided by principle P9 (#912).**
  - Inputs and real output:
    - `/// <summary>A</summary>` on `void M(){int a=1;⏎a++;}` reformats the body. In another member, the fix also writes `///Text` as `/// Text` and removes a blank line between two documentation comments.
    - `int Z; /// <summary>D</summary>⏎    int D;` moves the comment off `Z`'s line and inserts a blank line.
  - Touches: RH8305-004.
- **Q4 — Decided by principle P7 (#912).** An instance of #1150.
  - Input: `/// <summary>A</summary>⏎int A;` in a file that also has `void N( }`.
  - Real output: the fix is offered and returns the document unchanged.
  - Touches: RH8305-004.
- **Q5 — Decided by principle P13 (#912).**
  - Input: `/// <summary>K</summary>` inside a method body.
  - Real output: reported and expanded.
  - Touches: RH8305-001.
- **Q6 — Decided by principle P1 (#912).**
  - Input: `/// <summary>S⏎    int A;` (no end tag).
  - Real output: reported, because the missing end tag is placed on the next line. The fix writes `/// <summary>⏎/// S⏎/// int A;`, which turns the field into a comment.
  - Touches: RH8305-001, RH8305-004.
  - Related: an end tag with another name, `/// <summary>S</returns>`, is reported and the fix keeps `</returns>`. That shape is **Decided by principle P19 (#912)**: such an element has no end tag of its own, so the fix is withheld.
- **Q7 (open).**
  - Input: `/// <summary></summary>` or `/// <summary> </summary>`.
  - Real output: the fix, like the formatter, writes `<summary>⏎/// ⏎/// </summary>`, an empty content line with one trailing space. The formatter is stable on it.
  - Why it looks wrong: FMT-DOC-011 drops empty leading and trailing content lines, yet one is created here.
  - Readings: (a) keep the empty middle line as room for the text; (b) leave an empty summary unchanged.
  - Touches: RH8305-004, FMT-DOC-008, FMT-DOC-011. It may belong in the issue for `formatter/documentation-comments.md` instead.

### `SDD open questions: sdd/specs/analyzer/RH8306.md`
- **Q1 — Decided by principle P8 (#912).**
  - Inputs: `/// <Summary>Gets the value</Summary>` and `/// <summary xmlns="u">Gets value</summary>` on a property.
  - Real output: both are reported, because the tag is matched case-insensitively and regardless of namespace.
  - Touches: RH8306-001.
- **Q2 — Decided by principles P8 and P20 (#912)** (#1195 Q1).
  - Inputs and real output, neither reported:
    - a summary in a later documentation comment, after `/// <remarks>R</remarks>` and a blank line;
    - a second top-level summary in the same comment.
  - Touches: RH8306-001.
- **Q3 — Decided by principle P8c (#912)** (#1153; the lead may downgrade the mark).
  - Inputs and real output, none reported:
    - `<summary><para>Gets value</para></summary>`;
    - `<summary>Returns&#32;value</summary>`;
    - `<summary>Value&#46;</summary>`.
  - Also: an XML comment, CDATA section, or processing instruction counts as one space, so `Gets<!-- c -->value` is reported, because its first word reads as `Gets`.
  - P8c says only content that renders text counts, `<see cref>` and the like count as text, references are decoded, and comments, CDATA, and PIs are no text.
  - Touches: RH8306-001, RH8306-002, RH8306-003.
- **Q4 (open; P11).**
  - Input: `/// <summary>Value.</summary>⏎public int this[int i] => 0;`.
  - Real output: not reported.
  - Readings: (a) indexers are the rule's own gap; (b) a different construct.
  - Touches: RH8306-001.
- **Q5 — Decided by principle P15 (#912)** (SHARED-DIAG-004 in #1260).
  - Input: `public partial int P { get; }` and `public partial int P => 0;`, each with `/// <summary>Gets the value.</summary>`.
  - Real output: two diagnostics for one property.
  - Touches: RH8306-001, RH8306-004.

### `SDD open questions: sdd/specs/analyzer/RH8307.md`
- **Q1 — Decided by principle P7 (#912).** An instance of #1150.
  - Inputs and real output: the fix is offered and returns the document unchanged for both:
    - `/** <summary>F⏎ * more</summary> */`;
    - `/// <param name="v">The value⏎/// </param>` in a file that also has `void N( }`.
  - Touches: RH8307-004.
- **Q2 — Decided by principle P9 (#912).**
  - Inputs and real output:
    - `/// <param name="v">The value⏎/// </param>` on `void M(int v){int a=1;}` also reformats the body;
    - `int Z; /// <remarks>R⏎    /// more</remarks>⏎    int D;` moves the comment off `Z`'s line and inserts a blank line.
  - Touches: RH8307-004.
- **Q3 — Decided by principle P13 (#912).**
  - Input: `/// <remarks>In⏎/// body</remarks>` inside a method body.
  - Real output: reported and rebuilt.
  - Touches: RH8307-001.
- **Q4 (open; P4).**
  - Input: `/** <summary>Total⏎ * sum</summary> */` on `int D;`.
  - Real output: reported, but the formatter leaves delimited comments alone (FMT-DOC-012), so formatted code stays reported. RH8304, RH8305, RH8308, and RH8309 do not check `/** */`.
  - Readings: (a) RH8307 skips `/** */` like the other layout rules; (b) both the formatter and the fix lay it out.
  - Touches: RH8307-001, FMT-DOC-012.
- **Q5 — Decided by principle P1 (#912).**
  - Input: `/// <remarks>Text⏎    /// more⏎    int A;` after a complete summary (no end tag).
  - Real output: the fix turns the field into a comment (`/// int A;`).
  - Related: an end tag with another name (`<remarks>Text⏎/// more</returns>`) is reported and the fix keeps `</returns>`. That shape is **Decided by principle P19 (#912)**.
  - Touches: RH8307-001, RH8307-004.
- **Q6 (open; P4, #961).**
  - Input: `/// Text <b>bold⏎/// still bold</b>` inside `<remarks>`.
  - Real output: reported, and the fix produces `Text <b>⏎/// bold⏎/// still bold⏎/// </b>`. #961, decided on #915 Q3, calls that output a formatter bug. FMT-DOC-009 does not decide inline formatting elements.
  - Touches: RH8307-001, RH8307-004, FMT-DOC-009.

### `SDD open questions: sdd/specs/analyzer/RH8308.md`
- **Q1 — Decided by principle P7 (#912).**
  - Input: `/// <summary>C</summary> a &amp; b`.
  - Real output: the diagnostic covers only `a `, and the fix leaves `&amp; b`, which is reported again.
  - Touches: RH8308-001, RH8308-003, RH8308-004.
- **Q2 (open).**
  - Input: `/// <summary>E</summary><!-- c --> text`.
  - Real output: not reported.
  - Readings: (a) report the text; (b) an XML comment ends the "after a closing tag" position.
  - Touches: RH8308-001.
- **Q3 — Decided by principle P13 (#912).**
  - Input: `/// <a/> body` inside a method body.
  - Real output: reported, and the fix removes `body`.
  - Touches: RH8308-001.

### `SDD open questions: sdd/specs/analyzer/RH8309.md`
- **Q1 — Decided by principle P8 (#912).**
  - Inputs: `<Remarks>`, `<x:remarks xmlns:x="u">`, and `<remarks xmlns="u">`, each before a `<summary>`.
  - Real output: all three are ranked as `<remarks>`, reported, and sorted.
  - Touches: RH8309-001.
- **Q2 (open).**
  - Input: `/** <remarks>R</remarks>⏎ * <summary>E</summary> */`.
  - Real output: not reported. Order is not layout, so FMT-DOC-012 is no reason to skip it.
  - Touches: RH8309-001.
- **Q3 — Decided by principle P8 (#912)** (#1195 Q1).
  - Input: `/// <remarks>R</remarks>`, a blank line, `/// <summary>H</summary>`.
  - Real output: not reported.
  - Touches: RH8309-001.
- **Q4 — Decided by principle P7 (#912).** The fix does not correct the reported comment in one application.
  - Inputs and real output:
    - out of order in the declaration's second comment: the fix edits the first comment, and formatting then joins the two, so a second application is needed;
    - inside a method body: no-op;
    - after an attribute list (`/// <summary>S</summary>⏎/// <remarks>R</remarks>⏎[Obsolete]⏎/// <returns>R</returns>⏎/// <summary>T</summary>⏎int M() => 0;`): the fix edits the comment in front of the attribute, converts the body, expands the summaries, inserts a blank line after `[Obsolete]`, and the diagnostic stays;
    - on an accessor (`/// <remarks>R</remarks>⏎/// <summary>G</summary>` on `get => 0;`): the fix edits the property's comment, and the diagnostic stays.
  - Touches: RH8309-004.
- **Q5 — Decided by principle P9 (#912).**
  - Inputs and real output:
    - `int A(int a) => a;` becomes a block body;
    - `void I<T>() { }` is expanded;
    - `int Z; /// <remarks>R</remarks>⏎    /// <summary>…</summary>⏎    int D;` moves the comment off `Z`'s line and inserts a blank line.
  - Touches: RH8309-004.
- **Q6 — free text and XML comments: Decided by principle P1 (#912)** (AGENTS.md, Code-fix coverage).
  - Inputs: `/// <remarks>R</remarks>⏎/// Text between⏎/// <summary>S</summary>`, and the same with `<!-- c -->` in place of the text.
  - Real output: the elements move around the text or comment, which then stands after `<summary>` instead of after `<remarks>`.
  - Touches: RH8309-004.
- **Q7 (open).**
  - Input: `<summary>`, `<remarks>`, `<custom>`, `<param name="b">`, `<param name="a">`, `<returns>`.
  - Real output: `<summary>`, `<param b>`, `<custom>`, `<param a>`, `<returns>`, `<remarks>`. The custom element keeps its position, so the two `<param>` elements end up on either side of it.
  - Readings: (a) keep custom elements in place; (b) move them with the element before them.
  - Touches: RH8309-004.
- **Q8 — Decided by principle P13 (#912).**
  - Inputs: `/// <remarks>R</remarks>⏎/// <summary>S</summary>` inside a method body, on an accessor, or after an attribute list.
  - Real output: reported.
  - Touches: RH8309-001.
- **Q9 — Decided by principle P1 (#912).**
  - Input: `/// <returns>R</returns>⏎/// <summary>S⏎int A() => 0;` (no end tag).
  - Real output: reported, and the fix produces a document that does not parse (`/// <returns>R</returns>    int A()`).
  - Touches: RH8309-001, RH8309-004.

### `SDD open questions: sdd/specs/analyzer/RH8401.md`
Unchanged from `01-spec-author.md`:
- **Q1** — Decided by principle P13: `/// <summary>S</summary>` in a method body is not reported. Touches RH8401-002.
- **Q2** — open: DocumentationMode `None`, where SHARED-DOC-008 and P13 conflict. Touches RH8401-001.
- **Q3** — Decided by principle P7: a line with U+FEFF in front of its `///` needs a second application. Touches RH8401-004.

### `SDD open questions: sdd/specs/analyzer/RH8402.md`
Configuration: `copyrightText` of three `//` lines with `{fileName}` and `{company}`.
- **Q1 (open).**
  - Inputs and real output: the fix replaces a leading comment that contains "copyright" in any casing:
    - `// Copyright (c) Old Company.⏎namespace N;` becomes the configured header;
    - `// copyright note⏎#if DEBUG …` deletes the note.
  - Why it looks wrong: the diagnostic is an empty span at the start of the file, so SHARED-FIX-003's exception for "text that a diagnostic of the fix's own rule reports" does not cover the old comment. A comment that mentions copyright need not be a header.
  - Readings: (a) replacing an old header is the correction itself, and the diagnostic should cover that comment; (b) the fix inserts in front of every comment and never deletes one.
  - Touches: RH8402-004, RH8402-005. The tests `MismatchedConfiguredHeader` and `MismatchedBlockCommentHeader` pin today's replacement and stay untagged.
- **Q2 — Decided by principle P1 (#912)** (SHARED-FIX-003, comment text), and **P9** for the blank line.
  - Inputs and real output:
    - `// Copyright (c) Old Corp.⏎// File note⏎⏎namespace N;` becomes the header followed directly by `namespace N;`, so `// File note` and the blank line are deleted;
    - `// Copyright (c) Old Corp.⏎⏎namespace N;` loses the blank line.
  - Touches: RH8402-005.
- **Q3 (open).**
  - Input: the exact header, but its last line continues: `// </copyright> extra`.
  - Real output: not reported (prefix comparison).
  - Readings: (a) require a line break or the end of the file after the header; (b) prefix.
  - Touches: RH8402-003.
- **Q4 (open).**
  - Input: a file without the header, with DocumentationMode `None`.
  - Real output: not reported.
  - Readings: (a) check in every mode; (b) keep the documentation-mode gate.
  - Touches: RH8402-003.

### `SDD open questions: sdd/specs/analyzer/RH8501.md`
- **Q1 (open).**
  - Input: `[UnconditionalSuppressMessage("C", "CA1")]`.
  - Real output: not reported.
  - Readings: (a) the rule's own gap; (b) a separate rule.
  - Touches: RH8501-001.
- **Q2 (open).**
  - Input: `Justification = "<Pending>"`, the placeholder the IDE inserts.
  - Real output: not reported.
  - Readings: (a) report it; (b) any text counts.
  - Touches: RH8501-002. The non-goal that decided it has been removed.

## New-rule candidates

Unchanged:
- `#pragma warning disable` without a justification comment.
- `[UnconditionalSuppressMessage]` without `Justification`, if RH8501 Q1 is answered with reading (b).

## Follow-up candidates (do not file; the maintainer must approve the text)

- **The formatter turns a declaration into a comment after a documentation element without an end tag.** Formatting `/// <summary>S⏎    int A;`, or an unclosed `<remarks>Text⏎/// more`, writes `///     int A;`, so the field disappears from the program. This violates the non-destructive rule for formatter transformations in `AGENTS.md`. The skeptic searched #961 and #1241 and found it in neither. The RH8305 Q6, RH8307 Q5, and RH8309 Q9 fixes inherit it.

## Decisions needed

_None._
