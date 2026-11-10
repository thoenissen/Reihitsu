Role: spec author
Commit: 7b9f7b6f5f987c5fb86453e54d72aa19fee53e71
Verdict: #1269: spec written; #1342: spec written; #1343: spec written, text written; #1459: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The work follows `00-lead.md` (*Maintainer answers* Q1 = A, *Decisions taken* 3–11) and the *Surfaces* tables of `01-skeptic.md`. No change doc was written; see *Whole run → Internal choices*.

## thoenissen/Reihitsu#1269

### Changed requirements

- RH8201-005 — The fix replaces the comment with `<inheritdoc/>` — clarified: `/// <inheritdoc/>` starts in the column of the replaced comment; the comments, directives, attribute lists, and blank lines around it keep their lines and columns, and the member's lines stay; a member whose first token followed `*/` goes on the next line in the column of `/**` (decision 4). Coverage widened from LF / four spaces per level / directives at column 0 to `\n` or `\r\n`, any indentation, and `#if`/`#elif`/`#else`/`#endif`/`#region`/`#endregion`/`#pragma`/`#nullable` (with excluded code) in any column. New example. `Uses` gains SHARED-FIX-006, SHARED-FIX-008.
- RH8202-001 — A `<value>` element on a property or indexer is reported — clarified: with several elements, the reported one is the first top-level one, or, when there is none, the first nested one (`DirectDocumentationSyntaxChecker.GetFirstDocumentingTag`: top-level matches first). Coverage widened from "at most one `<value>`" to any number. Still one diagnostic per declaration (decision 3).
- RH8202-003 — The fix removes the element — clarified: the fix removes every `<value>` element that RH8202-001 reports, top-level and nested, in one application; one inside `<code>`/`<example>` stays (SHARED-FIX-001). New example. `Uses` gains SHARED-FIX-001.
- RH8204-003 — The fix replaces the element with its content — clarified: the fix removes only the start and end tags and keeps the content character for character (references, `>`, nested-element quotes and attributes and empty-element form, white-space-only content, line endings). Coverage widened to placeholders with attributes and any content, on several lines under `\n` or `\r\n`; content that holds another `<placeholder>` is not covered. New example. `Uses` gains SHARED-FIX-006, SHARED-FIX-008.

### Examples for the tests

RH8201-005 (both examples, plus a CRLF variant of each), RH8202-003 (the "several elements" example), RH8204-003 (the "content kept as written" example, and the CRLF variant of the first example). The skeptic's breaking-input matrices name further cells under the widened coverage (`#nullable`, `#else`, disabled text, `/* */` next to the comment; `&quot;`, CDATA, XML comment in placeholder content).

### Output questions

_None._

### Internal choices

- RH8201-005: `/// <inheritdoc/>` starts "in the column in which the comment's first `///` or `/**` starts". That matches today's output for standard indentation; the alternative ("indented like the member") differs only where the comment and member are at different columns, which the indent.cs case shows the author wrote deliberately.
- RH8202 with a second `<value>` lacking an end tag stays outside coverage (RH8202-001 already admits only the reported element without an end tag), so RH8202-004 is unchanged.
- RH8204: white-space-only content is now kept (`the<placeholder> </placeholder>customer` → `the customer`, today `thecustomer`). I read it as RH8204-003's "replace with its content", with no visible behavior in question. The lead should confirm.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1342

### Changed requirements

- RH8305-004 — The fix lays the summary out on three lines — clarified: the fix changes only the reported summary (SHARED-FIX-006). The rest of the comment, the other documentation comments and the blank lines between them, and the declaration's code stay as written, wherever the declaration stands. The coverage clause "in a declaration whose code … is already laid out as the formatter lays it out" is removed. Two new examples. `Uses` gains SHARED-FIX-006, FMT-DOC-001.
- RH8305-005 — The fix is withheld for a comment written behind code — added (maintainer answer Q1 = A).
- RH8307-004 — The fix lays the element out as the formatter does — clarified, the same way as RH8305-004. New example. `Uses` gains SHARED-FIX-006, FMT-DOC-001.
- RH8307-005 — The fix is withheld for a comment written behind code — added (Q1 = A).
- RH8309-004 — The fix sorts the elements into the prescribed order — clarified: the fix changes only the order of the elements, each element keeps its lines, and the declaration's code stays. The "already laid out" clause is removed. Coverage now says "the only documentation comment of its declaration" and points to -006/-007. New example. `Uses` gains SHARED-FIX-006, FMT-DOC-001.
- RH8309-006 — The fix is withheld for a comment written behind code — added (Q1 = A).
- RH8402-005 — The fix writes the header at the start of the file — clarified: the coverage sentence now points to RH8402-006.
- RH8402-006 — The fix replaces an old header and keeps the lines behind it — added. It writes down the existing replace path (a leading run of comments with `copyright` in any casing is replaced, from the start of the file through the run's last comment). It also states the bug-fix target: the line break ending that line and every line after it, blank lines included, stay. Coverage is a run in which every comment contains `copyright`, and a `copyrightText` that does not end with a line break. `Uses` gains SHARED-FIX-006.

Withholding for the behind-code shape follows SHARED-DOC-004, which already requires a fix that moves neither the declaration nor the code in front to leave the comment behind that code. The "starts behind code" qualifier has its other side in each -004 example (a comment whose first `///` starts its own line).

### Examples for the tests

RH8305-004 (three examples), RH8305-005, RH8307-004 (two examples), RH8307-005, RH8309-004 (two examples), RH8309-006, RH8402-006 (two examples). The skeptic's #1342 matrix lists further failing cells: misformatted body, `{ }` body, nested members of a documented type, `///Text` in the same leading trivia, expression-bodied nested member. They fall under the widened -004 sentences.

### Output questions

_None._ Q1 is answered (A) and recorded as RH8305-005, RH8307-005, RH8309-006.

### Internal choices

- RH8402-006 is a new requirement, not a widened RH8402-005. The replace path is a separate decision with its own `Why:`.
- RH8402-006 coverage excludes a run that mixes a copyright comment with a comment without the word (see *Follow-up candidates*), so the spec does not write down today's removal of such a note.

### Follow-up candidates

- **RH8402 · Files must start with configured copyright header — a note in the same comment run as an old header.** Input: `// Copyright (c) Old Corp.⏎// Orders and invoices⏎namespace N;`. Today the fix replaces the whole run, so `// Orders and invoices` is deleted. That looks wrong next to RH8402-005's *Why* ("A comment at the start of a file is the author's note and stays below the new header"). Reading A: replace the whole run (today); output is the header then `namespace N;`. Reading B: replace only the comments that contain `copyright`; output is the header, then `// Orders and invoices`, then `namespace N;`. Recommendation: B. Not run; outside the issues, which are about the blank line.

## thoenissen/Reihitsu#1343

### Changed requirements

- RH8308-001 — Text after a top-level element on its line is reported — clarified: coverage widened from "text without character references" to text with character and entity references, unless the text's first non-white character is written as a reference that stands for white space (`&#32;`). `Uses` gains SHARED-FIX-001.
- RH8308-003 — The diagnostic covers the text up to the end of the line — clarified: a reference is part of the text, so the span runs through every reference, at the start, in the middle, and at the end (decision 10). New example.
- RH8308-004 — The fix removes the text — new example (text with `&amp;`, one application).
- RH8309-001 — An element out of the prescribed order is reported — clarified: with several `///` comments, an element out of order within one comment is reported, in a later comment as well (verified: the analyzer judges each `SingleLineDocumentationCommentTrivia` on its own). Coverage no longer requires one documentation comment. Order across comments is explicitly not covered (see *Follow-up candidates*).
- RH8309-007 — The fix is withheld for a declaration with several documentation comments — added (decision 6). Applies to `///` or `/** */`, whichever comment is reported and whatever separates them.
- RH8401-004 — The fix turns the whole comment into a `//` comment — clarified: the coverage sentence points to RH8401-005.
- RH8401-005 — The fix is withheld for a byte order mark or substitute character in front of `///` — added (decision 7: refuse the class first; `DocumentationCommentUtilities` stays unchanged).

Rule page: `documentation/rules/RH8401.md` no longer has the *Code fix behavior* section, which listed a corner case and implementation detail (decision 8). *How to fix it* now says, as the format requires, whether the code fix does the work: "The code fix does this for you, for every line of the comment at once." The example and the footer are unchanged.

### Examples for the tests

RH8308-003 (reference example), RH8308-004 (one-application example; the skeptic's matrix rows `a &amp;`, `&amp; b`, `a &#32; b`, `a &#x41; b`, `a &lt; &gt; b`), RH8309-001 (a later comment reported), RH8309-007 (both members; also the skeptic's ordinary-comment and `/** */` separators), RH8401-005 (U+FEFF on line 2, U+001A on line 2, U+FEFF on line 3 of 4).

### Output questions

_None._

### Internal choices

- RH8309-007 withholds for every declaration with more than one documentation comment. That includes one whose first comment is the out-of-order one, which today's fix corrects (decision 6 wording). The alternative was to withhold only when the reported comment is not the first.
- RH8401-005 withholds for the whole comment, not only for the affected line: converting the other lines would leave a half-converted comment, which RH8401-004's *Why* rules out.

### Follow-up candidates

- **RH8309 · XML documentation elements must follow a prescribed order — elements out of order across two documentation comments of one declaration.** Input: `/// <remarks>R</remarks>⏎⏎/// <summary>⏎/// S⏎/// </summary>⏎int A;`. Today it is not reported, because each comment is judged on its own. The compiler joins both comments, so the documentation is out of order. Reading A: not reported (today). Reading B: `<summary>` is reported, with no fix (RH8309-007). Recommendation: B, since the rule's *Why* is about the documentation a reader and a tool see. Not run beyond the analyzer reading; outside the issues.

## thoenissen/Reihitsu#1459

### Changed requirements

Covered by the RH8305-004 and RH8307-004 clarifications under #1342. Their new sentence names "directly in the compilation unit" as a place the declaration may stand, and their new examples are a file-level `public class C { … }`. No separate requirement. The formatter specs (FMT-PIPE-010) are unchanged (decision 9).

### Examples for the tests

RH8305-004 and RH8307-004 file-level examples. The skeptic's #1459 matrix covers struct, interface, enum, delegate, record, second type, after `using`, and after a file header.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

### Internal choices

- **No change doc.** RH8305-005, RH8307-005, RH8309-006, RH8309-007, and RH8401-005 add withheld-fix behavior. Each withheld class lies outside the coverage of the fix requirement as it stood:
  - RH8305-004, RH8307-004, and RH8309-004 covered only "a `///` comment that starts its own line";
  - RH8309-001 covered only "one documentation comment";
  - RH8401-004 covered only "white space" in front of `///`, which U+FEFF and U+001A are not to the fix.

  So no specified behavior changes. `AGENTS.md` (*Code-fix coverage*) makes withholding for a rare class a rule-local decision recorded in the rule's spec, and the one visible decision (Q1) has the maintainer's answer in `00-lead.md`. RH8402-006 writes down existing behavior plus a bug-fix target demanded by SHARED-FIX-006. The alternative is one change doc for the five withheld cases, which would widen the run to the behavior-change lane (`sdd/README.md`, *When a change doc is required*, "a requirement is added with new behavior"). This is the lead's decision. My recommendation is no change doc.
- New requirement IDs, inserted in `sdd/specs/requirement-ids.txt`: RH8305-005, RH8307-005, RH8309-006, RH8309-007, RH8401-005, RH8402-006. Each needs a covering test in this PR (`RequirementCoverageTests` fails until the test implementer adds them).
- `scripts/test.sh --project architecture --filter "SpecStructureTests|SpecConsistencyTests|RuleDocumentation"`: 26 passed (`Uses` rows, ledger order, code owners).
