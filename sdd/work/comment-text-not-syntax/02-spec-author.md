Role: spec author
Commit: eeafb94
Verdict: #966: spec written (clarification, no change doc); #1289: change doc written

My prompt carried no intended fix or hoped-for verdict beyond pointing at decisions 2 and 3 and the maintainer's answer to Q1 in `00-lead.md`; I worked from those, the skeptic handover, the specs, and the shipped code.

## thoenissen/Reihitsu#966

### Changed requirements

- SHARED-USING-009 — Blocks with preprocessor directives are not reordered — clarified:
  - "anywhere in the leading or trailing trivia of its directives" now reads "anywhere in the trivia of its directives: in the leading trivia of its first directive, in the leading or trailing trivia of any of its directives, and between the tokens of a directive" (decision 2, token-wise reading; skeptic matrix B stays unchanged and unreported).
  - New sentence: only a preprocessor directive makes the block unsafe; a line of a `/* */` or `/** */` comment that starts with `#` is the comment's text (SHARED-DIAG-003), not a directive.
  - Two examples added: B1 (a `#pragma` between the tokens of `using System … .Text;`) is left unchanged and not reported; a `/* # Header */` file header before `using System.Linq; using System;` is reported by RH7203 on `System` and reordered, with the header kept at the top (consistent with FMT-USING-009).
  - `Uses` of `sdd/specs/shared/using-directive-ordering.md` gains SHARED-DIAG-003.

Change doc: **not required.** Both sentences state what the unchanged requirement already demanded: the token-wise reading is today's behavior (the regex blocks matrix B), and "only a preprocessor directive" was already the requirement's wording; the code's silence on `#`-led comment lines is the bug the run fixes (bug fix + clarification, `sdd/README.md`, *When a change doc is required*).

### Examples for the tests

- SHARED-USING-009: the new B1 example (unchanged / not reported) and the `# Header` example (RH7203 reported on `System`, fixed and formatted to `using System; using System.Linq;` with the header on top). The skeptic's matrices A (A1–A8) and B (B1–B3) remain the full test list, each under LF and CRLF.

### Output questions

_None._

### Internal choices

- Added the comment-text sentence and its example to SHARED-USING-009 in addition to the token-wise sentence decision 2 routed, so the qualifier "preprocessor directive" has an example on each side (FC-002). Alternative: the token-wise sentence alone, leaving the comment case implicit in SHARED-DIAG-003.

## thoenissen/Reihitsu#1289

### Changed requirements

- RH2109-001 — A `@code` block in a `.razor` file is reported — changed: RH2109 reads a `.razor` file as markup without lexing its C#. A line inside a multi-line C# comment or literal (`/* */`, `/** */`, verbatim, raw, interpolated verbatim, interpolated raw) is read like markup: a line-starting `@code` + `{` there MUST be reported; a line-starting `@*` or `<!--` there opens a Razor/HTML comment for the rule, so a `@code` up to the next `*@`/`-->` (or the end of the file) MUST NOT be reported, also a real directive. The first paragraph now says that for such lines the new paragraphs decide instead of SHARED-DIAG-003. Why and two examples added.
- SHARED-DIAG-003 — one sentence added to its scope paragraph pointing to RH2109-001 for how RH2109 reads a Razor file's C#; the requirement's demands for every other rule are unchanged.

Change doc: **required** and written as `sdd/changes/2026-11/rh2109-csharp-lines-read-as-markup.md` (`Status: Draft`, for the lead to set `Accepted`). RH2109-001 previously demanded "not reported" for this class through SHARED-DIAG-003; it now demands "reported" — a changed requirement is a change of intended behavior (`sdd/README.md`: "A different intended behavior needs a change doc, whose `Decisions` section records the answer"). Its `Visible output changes` says none: the shipped analyzer already behaves this way. `Decisions` records the maintainer's answer A, converse miss included.

Rule page `documentation/rules/RH2109.md`: not changed — a corner case, not the normal report.

### Examples for the tests

- RH2109-001, new examples: `@code` reported on lines 1, 3, 7 (block comment and raw string inside a `@code` host); converse: only line 1 reported when a line-starting `@*` inside a C# block comment hides the later `@code {`. The existing characterization tests listed in `01-skeptic.md` (`VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported` and the eight `…IsReported` members) become `Requirement("RH2109-001")` tests of the decided result; the converse has no test yet and needs one.

### Output questions

_None._ (Q1 answered: A.)

### Internal choices

- Recorded the exception in RH2109-001 and added only a pointer to SHARED-DIAG-003, rather than carving the exception out in the shared requirement. Alternative: a Razor exception bullet inside SHARED-DIAG-003.

## Follow-up candidates

**RH2109 · Razor @code blocks should not be used — real HTML or Razor comment opened behind other text on its line** (outside both issues; same "opener must start a line" decision, not asked in this run)

Input (`Counter.razor`):
```razor
<p>Counter</p><!--
@code {
}
-->
```

Real output (`main`): reported on line 2 — the scanner treats only a line-starting `<!--`/`@*` as an opener (tests such as `VerifyHtmlCommentOpenerInAttributeValueDoesNotHideDirective` tag this under RH2109-001 for openers that are not real comments).

Why it looks wrong: SHARED-DIAG-003 makes the text of a real HTML comment comment text, and RH2109-001 says such a `@code` MUST NOT be reported; no requirement states that a mid-line opener opens nothing. Requirement IDs: RH2109-001, SHARED-DIAG-003.

Readings: A — reported on line 2 (today; the spec records that only a line-starting opener counts, since a mid-line one may sit in an attribute value, C#, or script). B — not reported (needs telling a real mid-line comment from one inside an attribute or string: a parser of text). Recommendation: A, recorded in RH2109-001, for the same reason as #1289.
