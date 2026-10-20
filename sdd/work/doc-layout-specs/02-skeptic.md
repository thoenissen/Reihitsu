Role: skeptic
Commit: 5278e6ecdad6d1ad4e2a224820eb305c4a5e64c1
Verdict: proceed narrower

Run: thoenissen/Reihitsu#985, slug `doc-layout-specs`, lane "Specs and tests for existing behavior". My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The run writes specs for rules that exist; the premise is real. I checked every requirement of the twelve specs against the analyzer's reporting condition and the code fix, ran `scripts/apply-fix.sh` (LF and CRLF arms; parse options `LanguageVersion.Latest`, DocumentationMode `Parse`, no preprocessor symbols) on fixtures outside the repository, and ran `scripts/format.sh` on the same inputs to compare with the formatter. RH8306, RH8402, and RH8501 have no entry point that runs them alone (no fix provider, or a `reihitsu.json` the runner cannot pass), so those claims were checked against the code only.

Most claims hold. Below, findings F1 to F15 are cases the spec states wrongly or leaves out; the spec author has to narrow coverage sentences, change one example, and file more questions. No finding needs a maintainer decision before the run can continue: each open item goes into the per-spec issue.

Claims that held, so the next role can try to break them instead of re-deriving them:

- RH8301: the separator check is `DocumentationCommentUtilities.NormalizeExteriorSuffix` over each line that starts with `///` but not `////`. Lines inside string literals (regular, verbatim, raw, UTF-8, interpolated), `/* */`, `/** */`, and disabled text are skipped (`FormattingTextAnalysisUtilities.GetNonFormattableLineIndices`).
- RH8302: one span per run of blank lines, from the run's first line through its last line break. A `/** */` comment counts only when nothing but white space follows its `*/` on that line.
- RH8303: the exemptions are read from trivia: the previous token is `{`, or the nearest trivia before the comment that is not white space or a line break is a comment, a documentation comment, or a directive. Line 0 is never reported.
- RH8304 and RH8308: each element or text is judged against the top-level node directly before it. Text, XML comments, and CDATA reset the previous element.
- RH8305: the first top-level `XmlElementSyntax` named `summary` (case-sensitive `LocalName`) of each `///` comment is checked.
- RH8306: the summary text is built by `GetTextContent`; the verbs are compared with `OrdinalIgnoreCase`; the `?` exemption applies to `bool` and `bool?`.
- RH8309: the order list is `XmlDocumentationElementOrderingUtilities.CanonicalElementOrder`. The fix sorts with a stable `OrderBy` and keeps unknown elements in place.
- RH8401: a comment is reported when `Content.All(XmlText)` holds, once per exterior, on that exterior's last three characters.
- RH8402: the header is compared as a prefix with `StartsWith`. Line breaks come from the first line break of the file. The fix replaces the span that `GetLeadingHeaderSpan` returns.
- RH8501: the attribute matches by the display name of the constructor's containing type, including candidate symbols. A justification counts when `GetConstantValue` is a string with text.
- The formatter inserts the blank line at every place the RH8303-001 example reports. It agrees with RH8302-001 everywhere except `#region` (F2).
- The edits to the rule pages `documentation/rules/RH8309.md` and `RH8501.md` match the code.

## Mechanism and candidates

_N/A_ (not a bug fix). The candidate set comes from the dispatch:

- RH8301–RH8303 scan the text of every line.
- RH8304, RH8305, RH8308, RH8309, and RH8401 run on every `SingleLineDocumentationCommentTrivia`.
- RH8307 runs on every `XmlElement`, nested ones and those in `/** */` comments included.
- RH8306 runs on `PropertyDeclaration`, RH8501 on `Attribute`, and RH8402 on each syntax tree once the compilation starts.

Roslyn never puts a `///` or `/**` comment in trailing trivia. A documentation comment written behind code is therefore leading trivia of the next token. That makes these positions candidates for every trivia-driven rule:

- before a declaration;
- behind code;
- after an attribute list;
- before an accessor;
- in a method body;
- before `}`;
- in an argument list;
- in a switch section.

## Breaking inputs

### A. The spec states behavior that should be an open question

**F1 — RH8307-001 covers `/** */` comments.** The input is `/** <summary>Total⏎ * sum</summary> */` on `int D;`:

- RH8307 reports it.
- The fix ends with `no-progress (1 iteration)`.
- `format.sh` leaves the comment unchanged (FMT-DOC-012).

So the formatter's output is reported and stays reported: an analyzer/formatter disagreement (FC-013). P4 says such cases are decided one by one, not by a principle. RH8304, RH8305, RH8308, RH8309, and FMT-DOC-012 all exclude delimited comments. Remove `/** */` from RH8307-001's coverage sentence and example, and file it as an open question in the RH8307 issue. The no-op part stays Q1 (P7).

**F2 — RH8302-001's "a preprocessor directive" includes `#region`, and there RH8302 and RH5031 undo each other.** Input: `/// <summary>Sixth</summary>⏎⏎#region R⏎⏎    int F;⏎…`.

- RH8302 reports the blank line, and its fix removes it.
- On the result, RH5031 reports `#region R` (SHARED-BLANK-005), and `apply-fix.sh RH5031` inserts the same blank line again.

This is SHARED-FIX-004: **Decided by principle P7 (#912)**. The formatter keeps an existing blank line there, but does not insert one (`format.sh` output `/// </summary>⏎⏎    #region R`), so each rule also flags one of two formatter-stable layouts. `#if`, `#pragma`, and `#nullable` agree: the formatter removes the blank line before them. Exclude a blank line followed by `#region` or `#endregion` from RH8302-001's coverage. File it in the RH8302 issue with RH5031, FMT-BLANK-010, and FMT-BLANK-013 under *Touches*.

**F3 — The "Fifth" case in the RH8302-001 example is a shape the requirement's own coverage excludes.** No preprocessor symbols are defined in the verifier (`AnalyzerTestsBase`) or in `apply-fix`, so `#if DEBUG⏎    int E;⏎#endif` makes `int E;` disabled text. `/// <summary>Fifth</summary>` is then leading trivia of the class's `}`: a comment in front of a closing brace. That is the P13 shape of RH8302 Q1. A test tagged RH8302-001 from this block would pin a decided bug as intended. Use `#if !NEVER` instead (`format.sh` removes the blank line there, so analyzer and formatter agree) or `#pragma warning disable`.

**F4 — A documentation comment that starts behind code.** It is leading trivia of the next declaration (FMT-DOC-001 relocates it), so SHARED-DOC-004 counts it as a documentation comment of that declaration. Each spec's coverage sentence therefore includes it.

- **RH8304: decided by principle P1 (#912)** (SHARED-FIX-003, compile error). The fix inserts everything in front of `///` on the line, not only the indentation (`DocumentationCommentUtilities.GetContinuationPrefix`, called from `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync`, see #1242):
  - `int A; /// <summary>S</summary><remarks>R</remarks>⏎    int B;` becomes `int A; /// <summary>S</summary>⏎    int A; /// <remarks>R</remarks>`. The field is duplicated (CS0102).
  - `class C⏎{ /// <summary>S</summary><remarks>R</remarks>` becomes a second `{ /// <remarks>R</remarks>` line, which `apply-fix` reports as `invalid-result`.

  RH8304-004 ("the indentation of that line") is false for this input. Exclude it from RH8304-004's coverage, for example "a comment whose lines have nothing but white space in front of `///`", and file it.
- **RH8303: open.**
  - Input: `int A; /// <summary>⏎    /// B</summary>⏎    int B;`.
  - The diagnostic is on the second line's `///`. The documentation trivia is at index 0 of the leading trivia, so neither exemption applies.
  - The fix inserts a blank line inside the comment, so the compiler sees two documentation comments. RH8302 cannot see this blank line, because it scans text.
  - This contradicts RH8303-001 ("starts its line") and RH8303-004 ("the `///` of its first line"). The SHARED-FIX-003 layout exemption allows line breaks next to a tag, so no principle decides it.
- **RH8302: open.**
  - Input: `int Z; /// <summary>D</summary>⏎⏎    int D;`.
  - It is not reported, because the text scan finds no line that starts with `///`. Yet RH8302-001's coverage includes it.
  - `format.sh` moves the comment and removes the blank line, so code without diagnostics changes when formatted (FC-013).
- **RH8301: open.**
  - Input: `int a; ///Behind`.
  - It is not reported, and RH8301-002 specifies that as intended. Its Why says "Only a line that a `///` comment starts carries the documentation exterior". That is false: Roslyn gives that comment a `DocumentationCommentExteriorTrivia`.
  - In RH8301-002's own example, `///Behind` documents field `s`, because the blank line and the `/* */` comment do not separate them (SHARED-DOC-004).
  - FMT-DOC-001 and FMT-DOC-004 relocate the comment and normalize its separator.
  - Whether the rule's own gap (P11) includes this line is for the maintainer to decide. Remove the shape and that Why sentence from RH8301-002, and exclude the shape in RH8301-001's coverage.
- **RH8305, RH8307, and RH8309:** each fix relocates the comment and edits the previous member's line. These are P9 instances of Q3, Q2, and Q5.
- RH8308 and RH8401 handle the shape correctly.

**F5 — Elements without an end tag: the fixes destroy code.**

- RH8305: `/// <summary>S⏎    int A;` is reported, because the missing end tag is placed on the next line. The fix produces `/// <summary>⏎    /// S⏎    ///     int A;`, which comments the field out.
- RH8307: `/// <remarks>Text⏎    /// more⏎    int A;` after a complete summary also ends in `///     int A;`.
- RH8309: `/// <returns>R</returns>⏎    /// <summary>S⏎    int A() => 0;` gives `invalid-result`.

These are **decided by principle P1 (#912)**, with P19 for withholding the fix. The coverage sentences of RH8305-001 and RH8307-001 exclude these inputs ("closed by its own end tag"), but no question was filed. RH8309-001 and RH8309-004 do not exclude them at all, and RH8309-004's coverage sentence admits this input.

An end tag with another name (`<summary>S</returns>`, `<remarks>Text⏎/// more</returns>`) is reported. The fix keeps `</returns>` and is harmless. That shape is P19 by analogy; #1241 names only RH8107, RH8202, and RH8204.

The cause is in the formatter: `format.sh` on these inputs also writes `///     int A;`. See *Follow-up candidates*.

**F6 — RH8307 and inline elements.** RH8307-001 covers every nested element. Input: `/// Text <b>bold⏎/// still bold</b>` inside `<remarks>`.

- RH8307 reports it.
- The fix produces `Text <b>⏎/// bold⏎/// still bold⏎/// </b>`. #961, decided on #915 Q3, calls exactly that output a formatter bug.
- RH8307-004 says the fix lays the element out "as FMT-DOC-009 specifies". FMT-DOC-009 itself says it "does not yet decide inline formatting elements such as `<b>` or `<i>`".

Exclude inline formatting elements from the coverage of RH8307-001 and RH8307-004, as FMT-DOC-009 does, and file the shape as open (P4, #961).

### B. Claims that break inside their coverage

**F7 — RH8304-002 says a character reference stops the report, but one that decodes to white space does not.**

- `/// <summary>S</summary>&#32;<remarks>R</remarks>` and the same with `&#160;` are both reported. The fix leaves `&#32;` at the end of the summary line, where RH8308 then reports it.
- `&amp;` is not reported.
- The cause: `ContainsMeaningfulText` reads `ValueText`.

Qualify RH8304-002 ("a character reference that does not stand for white space"), or exclude the shape and file it.

**F8 — The RH8307 non-goal "A summary that spans fewer than three lines — RH8305" is false.** `/// <summary>Stops⏎/// </summary>` is reported by RH8307 too, because the content starts on the start-tag line and the end tag is on a later line. The fix turns it into three lines. Reword the non-goal, for example to "how many lines a summary spans — RH8305".

### C. Coverage exclusions without a filed question

**F9 — Tag names compared with the formatter (affects how RH8305 Q1 is classified).**

- `/// <Summary>S</Summary>` is not reported by RH8305, because `LocalName` is compared case-sensitively. The formatter expands it, because FMT-DOC-008 is "matched case-insensitively". Code without diagnostics changes when formatted (FC-013).
- `<summary xmlns="urn:x">S</summary>` is reported and fixed. RH8305-001's coverage excludes "in no XML namespace", but Q1 lists only `<x:summary>`.

P8 decides which documentation counts. FMT-DOC-008, which RH8305 uses, is a merged requirement that matches without regard to case. A P8 fix of RH8305 alone would widen the disagreement with the formatter.

Recommendation: file Q1 as open (P4) with all three shapes and FMT-DOC-008 under *Touches*. If it stays "Decided by principle P8", it must still add `<Summary>`, the `xmlns` shape, and FMT-DOC-008.

The same `xmlns="…"` exclusion is in the coverage sentences of RH8306-001 and RH8309-001, but not in their Q1. For example, `/// <remarks xmlns="u">R</remarks>⏎/// <summary>S</summary>` is reported by RH8309 and sorted. Add it to each Q1.

**F10 — Further exclusions that no filed question covers:**

- **RH8305-004** excludes "content that is only white space". Inputs `<summary></summary>` and `<summary> </summary>` both become `<summary>⏎/// ⏎/// </summary>` in one application. Either cover this output or file why it looks wrong.
- **RH8306-001** coverage also excludes XML comments, CDATA, and processing instructions in the summary, which each count as one space. Q3 does not mention them.
- **RH8306 partial properties** (from the code; no entry point runs RH8306 alone). Each part is a `PropertyDeclarationSyntax` with its own leading trivia, so two documented parts with sentence summaries give two diagnostics. RH8306-004's "one diagnostic per property" then fails. This is **decided by principle P15 (#912)** (SHARED-DIAG-004 in #1260). File it, or exclude the shape.
- **RH8309 Q4 and Q7** need two more shapes. Both are P13 (the comment documents nothing, CS1587) and P7/P9.
  - Input after an attribute list: `/// <summary>S</summary>⏎/// <remarks>R</remarks>⏎[Obsolete]⏎/// <returns>R</returns>⏎/// <summary>T</summary>⏎int M() => 0;`. The comment after the attribute is reported. The fix reorders the comment in front of the attribute instead, and also formats the method:
    - the expression body becomes a block;
    - the summaries are expanded;
    - a blank line is inserted after `[Obsolete]`.

    The diagnostic stays (`no-progress`).
  - Input on an accessor: an in-order property comment, and `/// <remarks>R</remarks>⏎/// <summary>G</summary>` on `get => 0;`. The accessor comment is reported, the fix edits the property's comment, and the result is `no-progress`.

### D. Classification of the questions to file

**F11 — RH8501: the non-goal answers Q2.** The non-goal "Whether a justification is a good reason — only its presence is checked" settles Q2 (`<Pending>`) as reading (b). At the same time, RH8501-002 excludes `<Pending>` and Q2 asks the maintainer. `sdd/README.md` forbids writing an open question as a non-goal. Change one of the two: drop Q2 and cover `<Pending>` as not reported, or narrow the non-goal so it does not decide `<Pending>`.

**F12 — RH8402: Q1's second bullet contradicts RH8402-005.**

- The input `// copyright note⏎#if DEBUG …` is filed as P1, because the comment is deleted.
- That input matches RH8402-005's second bullet ("a comment that contains that word, the header replaces it") and its coverage sentence (the only comment, followed by a directive on the next line). It behaves exactly like the spec's own example "an old header is replaced".

Either the comment is the old header, and the bullet leaves Q1; or replacing any comment that contains "copyright" is the question, and RH8402-005 cannot state the replacement. The doubt is real: SHARED-FIX-003 allows "text that a diagnostic of the fix's own rule reports", and the RH8402 diagnostic is an empty span at the start of the file.

The coverage sentence of RH8402-005 also reads as if it limited the whole requirement to files that have a copyright comment, although bullets 1 and 3 describe files without one. Scope it to the second bullet.

**F13 — RH8309 Q6: two of its shapes look decided by P1.** AGENTS.md (*Code-fix coverage*) says a rewrite that would "reorder, or relocate user-authored comments" must withhold itself, and a fix that violates SHARED-FIX-003 is "a bug against that spec, not an open question". In the `<!-- c -->` and free-text shapes, the fix moves the elements around that content, so the comment and the text end up next to other elements. Reading (a), withhold, is what that policy prescribes. Consider marking those two shapes **Decided by principle P1 (#912)**. The custom-element shape (`<param b>`, `<custom>`, `<param a>`) changes only element order and can stay open.

**F14 — RH8306 Q3 should cite P8c.** P8c (#1153: "only summary content that renders text counts as text; `<see cref>` … count as text") defines the summary text that RH8306 judges. At least the nested-element and decoded-reference parts of Q3 look decided by P8c. Cite P8c in the question; the lead decides whether to mark it decided.

**F15 — Every other filed question is classified correctly:** RH8301–RH8304 Q1 (P13), RH8305 Q2–Q5, RH8306 Q1/Q2/Q4, RH8307 Q1–Q3, RH8308 Q1–Q3, RH8309 Q1–Q5/Q7, RH8401 Q1–Q3, RH8402 Q2/Q3, and RH8501 Q1. F1, F2, F4, F5, F6, F9, and F10 add shapes or parts to them.

### E. Wording

- **RH8301-004:** "everything after the first character of the content stay as they are" leaves out the first character, which stays too. Write "the content, from its first character on, stays as it is".
- **RH8306-001** coverage "content is text without character references and empty elements such as `<see cref="…"/>`" reads as excluding empty elements. Its own example reports `Value <see cref="C"/>.`. Write "text without character references, with or without empty elements such as …".

### Failure classes for the test implementer

- **FC-022:** `///`-looking lines inside raw, interpolated raw, verbatim, and UTF-8 literals, for RH8301–RH8303, which scan text.
- **FC-020 and FC-021:** `#if` and `#else` with a documentation comment in each branch, `#if false`, `#region`, and `#pragma` after a documentation comment (RH8302, RH8303). A comment behind code, or behind `{`, above a documentation comment (RH8303).
- **FC-017 and FC-010:** a CRLF variant of every fix test: RH8302's removed run, RH8303's inserted line, RH8304's inserted line break, and RH8402's header and separator.
- **FC-008 and FC-026:** Fix All with several diagnostics on one line (RH8304; RH8308 `a <c>x</c> b`) and in one comment (RH8401 lines, which each replace the same comment).
- **FC-004:** RH8309 with text or XML comments between elements; RH8402 with a leading comment block.
- **FC-007:** RH8308 with a character reference (Q1), RH8309 with two comments (Q4), and RH8401 with U+FEFF (Q3). These are characterization tests only, untagged.
- **FC-009 and FC-030:** the RH8305, RH8307, and RH8309 fixes on a file with a syntax error and on `/** */` (no-op), and on an unformatted declaration (formats more than the comment).
- **FC-024:** RH8306 on a partial property and on an explicit interface implementation.
- **FC-006:** the inputs of F4 (RH8304) and F5. They must not be tagged with the requirement whose coverage the spec author narrows.

Two examples must not become tagged tests before the spec changes: the "Fifth" block of RH8302-001 (F3), and the `/** */` case of RH8307-001 (F1).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH8301–RH8501 | Must stay (lane: no production change) |
| Code fixes | Must stay |
| `XmlDocumentationElementOrderingUtilities`, `DocumentationCommentCodeFixUtilities`, `CopyrightHeaderTemplateResolver` | Must stay |
| Formatter | Not involved in this run; the F5 defect is a follow-up candidate |
| Specs RH8301, RH8302, RH8303, RH8304, RH8305, RH8306, RH8307, RH8309, RH8402, RH8501 | Must change (coverage, examples, non-goals, wording per F1–F14) |
| Specs RH8308, RH8401 | Must stay |
| Rule pages | Must stay (the RH8309 and RH8501 edits are correct) |
| Tests | Must change (tags and new tests; F3/F1 examples excluded until fixed) |
| Open-question issues | Must change: add the shapes and reclassifications of F1, F2, F4–F6, F9–F14 |

## Simpler option

_None._ Narrowing the coverage sentences and filing the shapes is already the smallest correct spec. No requirement needs to be split or added.

## Questions for the maintainer

_None blocking._ The competing readings in F1, F4 (RH8301, RH8302, RH8303), F6, F9, F11, and F12 go into the per-spec open-question issues, as `sdd/README.md` (*Open questions*) prescribes.

## Follow-up candidates (out of scope)

- **The formatter comments out a declaration after a documentation element without an end tag.**
  - Inputs and output: `scripts/format.sh` on `/// <summary>S⏎    int A;` writes `///     int A;`. The same happens after an unclosed `<remarks>Text⏎/// more`.
  - Why it is a defect: the field disappears from the program, which violates the non-destructive rule in `AGENTS.md`.
  - Coverage elsewhere: I searched #961 and #1241, and neither covers it.
  - Next step: the issue text needs the maintainer's approval.
