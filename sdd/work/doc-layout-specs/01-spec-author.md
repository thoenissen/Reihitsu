Role: spec author
Commit: 3c792b90b0ef0c161fc6d0edc4e943a6ff5cdfff
Verdict: spec written

Run: thoenissen/Reihitsu#985, slug `doc-layout-specs`, lane "Specs and tests for existing behavior". No change doc: every requirement writes down current behavior. My prompt carried no intended fix or verdict.

## What I did and how it was established

- Twelve new specs under `sdd/specs/analyzer/`: RH8301–RH8309, RH8401, RH8402, RH8501. 49 IDs went into `sdd/specs/requirement-ids.txt` after `RH8204-003`, still in ordinal order. Twelve rows went into `sdd/specs/README.md` after RH8204.
- Code owners are each rule's analyzer and code-fix provider. RH8309 also claims `XmlDocumentationElementOrderingUtilities`, which holds the canonical order. `DirectDocumentationSyntaxChecker` (SHARED-DOC) calls only its `GetTagName`, which RH8309 does not specify, so `shared/element-documentation.md` is unchanged. These types are not claimed: `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationCommentCodeFixUtilities`, `CopyrightHeaderTemplateResolver`, `FormattingTextAnalysisUtilities`, and `DocumentationCommentUtilities`.
- Uses:
  - SHARED-DOC-004 and SHARED-DOC-008 (most specs);
  - SHARED-BLANK-001 (RH8302, RH8303);
  - FMT-DOC-005 and FMT-DOC-006 (RH8301);
  - FMT-DOC-008, FMT-DOC-009, and FMT-DOC-011 (RH8305, RH8307);
  - RH0001-001 and RH0001-006 (RH8402: rule-to-rule, as the lead instructed).
- No reserved ID was minted, and `shared/element-documentation.md` was not edited.
- For #1242: the RH8304 fix calls `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync`, and the RH8308 fix calls its `IsHorizontalWhitespace`. Both specs state their own fix output and do not use RH8107-006 or RH8107-007, which specify element removal, a different operation.
- Examples were verified mechanically. A scratch harness ran each rule's analyzer, its first code action, and Fix All on every `csharp` block of every spec, using DocumentationMode `Diagnose` and LanguageVersion `Preview`. It compared the reported spans and the Fix All result with the blocks. All blocks match. RH8402 was run with the JSON configuration from its spec and the file name `C.cs`. The RH8402-001 block was checked without a configuration (0 diagnostics).
- The prose claims were checked with separate fixtures: RH8301 two spaces/tab, RH8402 CRLF, RH8304 line break of the line, and RH8303 inserted line ending, which the existing test `VerifyInsertedBlankLineUsesDetectedCarriageReturnLineFeedEndOfLine` already covers.
- Checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` passed 26/26. `scripts/test.sh --project analyzer --filter FullyQualifiedName~RuleDocumentationConventionTests` passed 3/3. `RequirementCoverageTests` will fail until the tests are tagged, which is the test implementer's step.
- Rule pages corrected:
  - `documentation/rules/RH8309.md` said `<typeparam>`/`<param>` must be "in declaration order", but the rule does not check that. The fix keeps the written order (RH8309-002, RH8309-004).
  - `documentation/rules/RH8501.md` said "must not be an empty string", but a value that is only white space or `null` is reported too (RH8501-002).
  - No other page was wrong.
- Coverage sentences ("This requirement covers …") leave every open-question shape out of the specs. Those shapes stay pinned only by untagged characterization tests where tests exist. These existing tests touch shapes that are left out and must stay untagged for the excluded requirement:
  - `RH8401…VerifyNoDiagnosticsWhenDocumentationModeIsNone` (RH8401 Q2);
  - `RH8303…VerifyDocumentationAfterSwitchLabelProducesDiagnostic` (RH8303 Q1).

## Changed requirements

- RH8301-001 to RH8301-004 — added
- RH8302-001 to RH8302-004 — added
- RH8303-001 to RH8303-005 — added
- RH8304-001 to RH8304-004 — added
- RH8305-001 to RH8305-004 — added
- RH8306-001 to RH8306-004 — added
- RH8307-001 to RH8307-004 — added
- RH8308-001 to RH8308-004 — added
- RH8309-001 to RH8309-004 — added
- RH8401-001 to RH8401-004 — added
- RH8402-001 to RH8402-005 — added
- RH8501-001 to RH8501-003 — added

## Examples for the tests

Every requirement above has an example (a location requirement points to the one of an earlier requirement). Existing test classes:
- `Reihitsu.Analyzer.Test/Formatting/RH8301…`, `RH8302…`, `RH8303…AnalyzerTests`;
- `Reihitsu.Analyzer.Test/Documentation/RH8304…` to `RH8309…`, `RH8401…`, `RH8402…AnalyzerTests`;
- `Reihitsu.Analyzer.Test/Design/RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzerTests`, which has a single test.

Gaps I saw while reading tests (not exhaustive):
- RH8306-003 (lowercase verb, `Getset` not reported);
- RH8501-001 (alias, qualified name, assembly target, attribute in another namespace) and RH8501-002 (white space, `null`, empty constant, concatenation);
- RH8402-001 (configuration with a problem), RH8402-002 (CRLF), and RH8402-003 (leading blank line);
- RH8309-002 (custom and nested elements);
- RH8304-002, RH8305-002, RH8308-002 (`/** */`).

`XmlDocumentationElementOrderingUtilitiesTests` in `Reihitsu.Core.Test` tests the order list behind RH8309-001.

## Open questions to file

One issue per spec, titled `SDD open questions: <spec path>`, as a sub-issue of #912. #912 has reached the 100-sub-issue limit, so list them in a comment there as earlier batches did. None of these specs had an issue before (searched). The items marked "Decided by principle" are filed with that mark. All inputs were run on the commit above.

### `SDD open questions: sdd/specs/analyzer/RH8301.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: a `///` comment that documents nothing, such as `void M() { ///InBody` on a line of its own inside a method body.
  - Real output: RH8301 reports it, and the fix writes `/// InBody`.
  - Why it looks wrong: under P13, a `///` that documents nothing is an ordinary comment, so the documentation-line rule should not apply.
  - Touches: RH8301-001 (left out by its coverage sentence).

### `SDD open questions: sdd/specs/analyzer/RH8302.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `/// inbody`, a blank line, then `int a = 0;` inside a method body; or `/// stranded`, a blank line, then the type's closing `}`.
  - Real output: both are reported, and the fix removes the blank line.
  - Why it looks wrong: neither comment documents anything, so under P13 each is an ordinary comment.
  - Touches: RH8302-001.

### `SDD open questions: sdd/specs/analyzer/RH8303.md`
- **Q1 — Decided by principle P13 (#912).** Also an instance of #1148 (the RH8303 part of the narrowed FMT-BLANK-011), and in the switch case an analyzer/formatter disagreement (P4).
  - Inputs and real output: RH8303 reports each of these, and the fix inserts a blank line:
    - `int a = 0;⏎/// inbody⏎a++;` in a method body;
    - `case 1:⏎/// incase⏎break;`, where the formatter does not insert a blank line, because FMT-BLANK-011 exempts the first comment of a switch section;
    - `N(1,⏎/// arg⏎2);`, inside an argument list;
    - `[Obsolete]⏎/// <summary>G</summary>⏎int G;`, after an attribute list, where the compiler ignores the comment.
  - Why it looks wrong: none of these comments documents a declaration (SHARED-DOC-004). Under P13 each is an ordinary comment, which RH5020 judges and which RH5020 exempts after a case label.
  - Touches: RH8303-001. The existing test `VerifyDocumentationAfterSwitchLabelProducesDiagnostic` pins the case-label shape.

### `SDD open questions: sdd/specs/analyzer/RH8304.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `/// <a/><b/>` on a line of its own inside a method body.
  - Real output: `<b/>` is reported, and the fix splits the line.
  - Why it looks wrong: the comment documents nothing, so under P13 it is an ordinary comment.
  - Touches: RH8304-001.

### `SDD open questions: sdd/specs/analyzer/RH8305.md`
- **Q1 — Decided by principle P8 (#912)** (tag names are case-sensitive and namespace-free, #1037 Q5 / #1195 Q3).
  - Input: `/// <x:summary xmlns:x="urn:x">H</x:summary>` on `int H;`.
  - Real output: reported, and the fix expands it to three lines.
  - Why it looks wrong: under P8, `<x:summary>` is not a summary.
  - Touches: RH8305-001.
- **Q2 — Decided by principle P20 (#912).**
  - Input: one `///` comment holding a three-line `<summary>J1</summary>` followed by `/// <summary>J2</summary>` on `int J;`.
  - Real output: the one-line second summary is not reported, because only the first top-level `<summary>` of a comment is checked.
  - Why it looks wrong: P20 says a rule checks every element of its kind.
  - Touches: RH8305-001.
- **Q3 — Decided by principle P9 (#912)** (SHARED-FIX-006 planned in #1138, P14d).
  - Input: `/// <summary>A</summary>` on `void M(){int a=1;⏎a++;}`, with two blank lines above the next member.
  - Real output: the fix reformats the whole method body to `void M()⏎{⏎int a = 1;⏎⏎a++;⏎}`. In another member, it also turns `///Text` into `/// Text` and removes a blank line between the declaration's two documentation comments.
  - Why it looks wrong: P9 says a fix changes only the code its diagnostic is about.
  - Touches: RH8305-004, whose coverage requires the rest of the declaration to be laid out already.
- **Q4 — Decided by principle P7 (#912).** An instance of #1150: add it there.
  - Input: `/// <summary>A</summary>⏎int A;` in a file that also contains `void N( }`.
  - Real output: the fix is offered and returns the document unchanged.
  - Why it looks wrong: SHARED-FIX-005 says an offered fix must change the document.
  - Touches: RH8305-004.
- **Q5 — Decided by principle P13 (#912).**
  - Input: `/// <summary>K</summary>` inside a method body.
  - Real output: reported and expanded.
  - Touches: RH8305-001.

### `SDD open questions: sdd/specs/analyzer/RH8306.md`
- **Q1 — Decided by principle P8 (#912)** (case-sensitive tag names).
  - Input: `/// <Summary>Gets the value</Summary>` on a property.
  - Real output: reported, because the tag is matched case-insensitively.
  - Touches: RH8306-001.
- **Q2 — Decided by principles P8 and P20 (#912)** (#1195 Q1: the documentation is the declaration's merged comments, and every top-level element is checked).
  - Inputs and real output:
    - `/// <summary>Gets the value</summary>` after `/// <remarks>R</remarks>` and a blank line: not reported, because only the first documentation comment is read;
    - `/// <summary>Value</summary>⏎/// <summary>Gets the value</summary>`: not reported, because only the first top-level summary is read.
  - Touches: RH8306-001.
- **Q3 (open).** The text that is judged ignores what is inside nested elements and does not decode character references.
  - Inputs and real output, none of them reported:
    - `<summary><para>Gets value</para></summary>`;
    - `<summary>Returns&#32;value</summary>`, where the first word is read as `Returns&#32;value`;
    - `<summary>Value&#46;</summary>`, a period written as a reference.
  - Why it looks wrong: the reader sees "Gets value", "Returns value" and "Value.".
  - Readings: (a) judge the rendered text, with nested element content included and references decoded; (b) keep today's raw top-level text.
  - Touches: RH8306-001, RH8306-002, RH8306-003.
- **Q4 (open).**
  - Input: `/// <summary>Value.</summary>⏎public int this[int i] => 0;`.
  - Real output: not reported, because only property declarations are checked.
  - Why it looks wrong: an indexer is a property of its type, and RH8202 treats properties and indexers alike. P11 may decide this as the rule's own gap or as a different construct.
  - Touches: RH8306-001.

### `SDD open questions: sdd/specs/analyzer/RH8307.md`
- **Q1 — Decided by principle P7 (#912).** An instance of #1150.
  - Inputs and real output: the fix is offered and returns the document unchanged for both:
    - `/** <summary>F⏎ * more</summary> */` on `int F;`: reported, but FMT-DOC-012 makes the formatter leave delimited comments alone;
    - `/// <param name="v">The value⏎/// </param>` in a file that also contains `void N( }`.
  - Why it looks wrong: SHARED-FIX-005 says an offered fix must change the document.
  - Touches: RH8307-004.
- **Q2 — Decided by principle P9 (#912).**
  - Input: `/// <param name="v">The value⏎/// </param>` on `void M(int v){int a=1;}`.
  - Real output: the fix also reformats the body to `void M(int v)⏎{⏎int a = 1;⏎}`.
  - Touches: RH8307-004.
- **Q3 — Decided by principle P13 (#912).**
  - Input: `/// <remarks>In⏎/// body</remarks>` inside a method body.
  - Real output: reported and rebuilt.
  - Touches: RH8307-001.

### `SDD open questions: sdd/specs/analyzer/RH8308.md`
- **Q1 — Decided by principle P7 (#912)** (SHARED-FIX-001, one application corrects).
  - Input: `/// <summary>C</summary> a &amp; b` on `int C1;`.
  - Real output: the diagnostic covers only `a `. The fix leaves `/// <summary>C</summary>&amp; b`, which is reported again, so a second application is needed.
  - Touches: RH8308-001, RH8308-003, RH8308-004.
- **Q2 (open).**
  - Input: `/// <summary>E</summary><!-- c --> text`.
  - Real output: not reported, because an XML comment between the end tag and the text stops the check.
  - Why it looks wrong: the text is still outside every element, on the end tag's line.
  - Readings: (a) report the text; (b) an XML comment ends the "after a closing tag" position.
  - Touches: RH8308-001.
- **Q3 — Decided by principle P13 (#912).**
  - Input: `/// <a/> body` inside a method body.
  - Real output: reported, and the fix removes `body`.
  - Touches: RH8308-001.

### `SDD open questions: sdd/specs/analyzer/RH8309.md`
- **Q1 — Decided by principle P8 (#912)** (case-sensitive, namespace-free tag names).
  - Inputs: `/// <Remarks>R</Remarks>⏎/// <summary>…` and `/// <x:remarks xmlns:x="u">R</x:remarks>⏎/// <typeparam …>`.
  - Real output: both are ranked as `<remarks>`, reported, and sorted.
  - Touches: RH8309-001.
- **Q2 (open).**
  - Input: `/** <remarks>R</remarks>⏎ * <summary>E</summary> */`.
  - Real output: not reported, because only `///` comments are checked.
  - Why it looks wrong: element order is not layout. FMT-DOC-012 explains why the layout rules skip delimited comments, but it does not explain why order would not matter there.
  - Touches: RH8309-001.
- **Q3 — Decided by principle P8 (#912)** (#1195 Q1, merged documentation).
  - Input: `/// <remarks>R</remarks>`, a blank line, then `/// <summary>H</summary>` on `int H;`.
  - Real output: not reported, because each comment is ordered on its own. The compiler joins the comments into one documentation whose order is wrong.
  - Touches: RH8309-001.
- **Q4 — Decided by principle P7 (#912).**
  - Inputs and real output:
    - `/// <summary>…</summary>`, a blank line, then `/// <remarks>R</remarks>⏎/// <example>E</example>`: `<example>` is reported. The fix reorders only the declaration's first comment, but its formatting joins the two comments, so the diagnostic remains and a second application is needed (SHARED-FIX-001).
    - `/// <remarks>R</remarks>⏎/// <summary>S</summary>` inside a method body: reported, and the fix is offered but returns the document unchanged (SHARED-FIX-005).
  - Touches: RH8309-004.
- **Q5 — Decided by principle P9 (#912).**
  - Input: `/// <returns>R</returns>⏎/// <summary>…</summary>⏎/// <param …>` on `int A(int a) => a;`.
  - Real output: the fix converts the method to a block body. `void I<T>() { }` is expanded onto several lines.
  - Touches: RH8309-004.
- **Q6 (open).** The fix moves only the listed elements. Top-level text, XML comments, and custom elements keep their positions, so they end up next to other elements.
  - Inputs and real output:
    - `/// <remarks>R</remarks>⏎/// Text between⏎/// <summary>S</summary>` becomes `<summary>…</summary>⏎/// Text between⏎/// <remarks>R</remarks>`;
    - the same with `<!-- c -->` in place of the text;
    - `<summary>`, `<remarks>`, `<custom>`, `<param name="b">`, `<param name="a">`, `<returns>` becomes `<summary>`, `<param b>`, `<custom>`, `<param a>`, `<returns>`, `<remarks>`, so the two `<param>` elements end up on either side of `<custom>`.
  - Why it looks wrong: text and comments the user placed after one element now follow another. AGENTS.md (Code-fix coverage) treats relocating comments as unsafe.
  - Readings: (a) withhold the fix when anything but white space stands between top-level elements; (b) move text and comments together with the element before them; (c) keep today's behavior.
  - Touches: RH8309-004.
- **Q7 — Decided by principle P13 (#912).**
  - Input: `/// <remarks>R</remarks>⏎/// <summary>S</summary>` inside a method body.
  - Real output: reported.
  - Touches: RH8309-001.

### `SDD open questions: sdd/specs/analyzer/RH8401.md`
- **Q1 — Decided by principle P13 (#912).**
  - Input: `/// <summary>S</summary>` on a line of its own inside a method body.
  - Real output: not reported, because the comment holds XML.
  - Why it looks wrong: under P13, a `///` that documents nothing is an ordinary comment that uses documentation slashes, which is exactly RH8401's subject.
  - Touches: RH8401-002 (left out by its coverage sentence).
- **Q2 (open).** SHARED-DOC-008 and P13 conflict here.
  - Input: `/// workaround` in a method body, with DocumentationMode `None`.
  - Real output: not reported, because the rule does not run in `None` mode.
  - Why it looks wrong: P13 says such a comment is an ordinary comment "in every documentation mode", and RH8401 forbids ordinary comments written with `///`. SHARED-DOC-008 says documentation rules report nothing in `None` mode.
  - Readings: (a) RH8401 reports `///` comments in every mode; (b) RH8401 follows SHARED-DOC-008.
  - Touches: RH8401-001, whose coverage is `Parse` or `Diagnose` only. `VerifyNoDiagnosticsWhenDocumentationModeIsNone` pins today's behavior.
- **Q3 — Decided by principle P7 (#912)** (SHARED-FIX-001).
  - Input: `/// first⏎﻿        /// second` in a method body.
  - Real output: one application converts only the first line. The second line stays `///` and is reported again; a second application converts it.
  - Note: `documentation/rules/RH8401.md` describes this as left to manual correction, but the fix is offered and needs two applications.
  - Touches: RH8401-004, whose coverage requires nothing but white space before each `///`.

### `SDD open questions: sdd/specs/analyzer/RH8402.md`
Configuration: `copyrightText` of three `//` lines with `{fileName}` and `{company}`.
- **Q1 — Decided by principle P1 (#912)** (SHARED-FIX-003, comment text), with P9 for the blank line.
  - Inputs and real output: the fix replaces the whole leading comment block whenever any comment in it contains "copyright" in any casing:
    - `// Copyright (c) Old Corp.⏎// File note⏎⏎namespace N;` becomes the header followed directly by `namespace N;`, so `// File note` and the blank line are deleted;
    - `// copyright note⏎#if DEBUG …` deletes `// copyright note`;
    - `// Copyright (c) Old Corp.⏎⏎namespace N;` loses the blank line below the header.
  - Touches: RH8402-005, whose coverage sentence excludes these shapes.
- **Q2 (open).**
  - Input: the file starts with the exact header but its last line continues: `// </copyright> extra`.
  - Real output: not reported, because the comparison checks only that the file starts with the header text.
  - Why it looks wrong: the file's header is not the configured one.
  - Readings: (a) require a line break or the end of the file after the header; (b) keep the prefix comparison.
  - Touches: RH8402-003.
- **Q3 (open).**
  - Input: any file without the header, with DocumentationMode `None`.
  - Real output: not reported.
  - Why it looks wrong: a copyright header is an ordinary comment and does not depend on documentation parsing.
  - Readings: (a) check in every mode; (b) keep the documentation-mode gate.
  - Touches: RH8402-003, whose coverage is `Parse` or `Diagnose` only.

### `SDD open questions: sdd/specs/analyzer/RH8501.md`
- **Q1 (open).**
  - Input: `[UnconditionalSuppressMessage("C", "CA1")]`.
  - Real output: not reported.
  - Why it looks wrong: it is a code analysis suppression just like `SuppressMessage` and takes the same `Justification`.
  - Readings: (a) RH8501 reports it as well, as the rule's own gap; (b) a separate rule (see the new-rule candidates).
  - Touches: RH8501-001.
- **Q2 (open).**
  - Input: `[SuppressMessage("C", "CA1", Justification = "<Pending>")]`.
  - Real output: not reported.
  - Why it looks wrong: `<Pending>` is the placeholder the IDE inserts when it generates a suppression. It states no reason.
  - Readings: (a) report it; (b) any text counts.
  - Touches: RH8501-002.

## New-rule candidates

- A `#pragma warning disable` without a justification comment on its line. It is the other in-source way to suppress a warning, which RH8501 does not cover.
- `[UnconditionalSuppressMessage]` without `Justification`, if RH8501 Q1 is answered with reading (b).

## Decisions needed

_None._
