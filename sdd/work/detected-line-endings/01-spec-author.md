Role: spec author
Commit: d54b349cf9c1636064ea8822b4c8cf1d3449ff67
Verdict: #1695: spec written (line ending), open question (trailing whitespace); #1696: open question; #1707: no spec change needed; #1721: no spec change needed (already decided by FMT-PIPE-007 and FMT-EOL-004)

My prompt carried suspected causes and target outputs only as quoted from the issues; I worked from the issues' text and the specs, and none of it is taken as a verdict.

## thoenissen/Reihitsu#1695

### Changed requirements

- FMT-PIPE-010 — Node-level entry points format only their target — clarified: both node-level entry points write the line break that ends the target's written-back trailing trivia as the line ending FMT-PIPE-007 detects for the document (a lone `\r`, an LF in a CRLF document, a CRLF in an LF document); trailing trivia already in the detected ending stays as it stands; a line break inside a comment's text keeps its characters. New `Why:` sentence and a new example (mixed CRLF document with an LF behind the target; lone-`\r` document in prose). `SHARED-FIX-008` added to the spec's `Uses`.

This is a clarification, not a change of intended behavior: SHARED-FIX-008 (unchanged) already demands the detected ending for every line break in a fix's span, "its trailing trivia included", and every caller of the two entry points is a code fix (`AGENTS.md`, *Supported inputs*). FMT-PIPE-010's "keep the trailing trivia" was read by the code as "keep its characters"; the clarification resolves that reading in favor of SHARED-FIX-008. No change doc required for this part.

### Examples for the tests

- FMT-PIPE-010 — the new example (property `P`, CRLF document with `\n` behind `P`; and the lone-`\r` variant in its prose). Both entry points are covered: `FormatNodeInDocumentAsync` with appended tokens (the RH3202 conversion: the line break is written behind the new `}`) and without (property `P`), and `FormatNodeInDocumentWithContextAsync` (e.g. the RH5111 enum-member fix, whose target ends its line).

### Output questions

**1 · Node-level formatting (every formatter-delegating code fix) — whitespace behind the target's last token**

Two unchanged requirements give different output: FMT-PIPE-010 says both entry points "keep the target's trailing trivia" (keeps the spaces); SHARED-FIX-007 says the changed code is laid out so that formatting the result changes nothing in it, and FMT-CLEAN-001 removes whitespace before a line break (removes the spaces). SHARED-FIX-008 counts the trailing trivia as part of a fix's span. Requirement IDs: FMT-PIPE-010, SHARED-FIX-006, SHARED-FIX-007, FMT-CLEAN-001. The answer also decides two cases: whether the removal holds only where formatting appended tokens behind the target's original last token (the issue's "Required output"), or at the end of every target's last line (the issue's "Done when": "the same text as document formatting").

Case 1a — RH3202 · Expression-style methods should not be used (`·` is a space; the formatter appends `}` behind the original `;`):

```csharp
class C
{
    private int _value;

    public int GetValue() => _value;···
}
```

Before (`main`):

```csharp
    public int GetValue()
    {
        return _value;
    }···
```

Case 1b — a fix whose target keeps its last token, e.g. RH5404 · Element must not be on a single line, with the property as target:

```csharp
class C
{
    int P { get { return 1; } }···
}
```

Before (`main`):

```csharp
    int P
    {
        get
        {
            return 1;
        }
    }···
```

- **A** — remove the whitespace only where formatting appended tokens behind the target's original last token: 1a gives `}` with no trailing spaces, 1b keeps `}···`. SHARED-FIX-007 already demands it for 1a (the `}` and its line are code the fix created), so it is a clarification of FMT-PIPE-010, no change doc.
- **B** — remove the whitespace at the end of every target's last line, as document formatting does: 1a gives `}`, 1b gives `}`. For 1b, the spaces stood behind a token the fix keeps, so B changes what FMT-PIPE-010 states for that case: a change doc is required, and the lane widens to behavior change.
- **C** — keep the whitespace in both cases (today). The issue's whitespace goal is dropped; RH3202 and the other conversions leave trailing whitespace the layout rules report.

Recommendation: **A** — it is what the issue asks for, an unchanged requirement (SHARED-FIX-007) already demands it, and it keeps the run in the bug-fix lane; whitespace behind a token the fix did not move is the author's layout, which SHARED-FIX-006 leaves alone. What would change the pick: the maintainer wanting node-level output to equal document formatting at the end of the target's line (B).

Until the answer, nothing about the whitespace is written into FMT-PIPE-010. The whitespace regression test cannot be written without it; the line-ending tests can.

### Internal choices

- The line-ending clarification lives in FMT-PIPE-010, referencing SHARED-FIX-008, rather than as a new FMT-PIPE requirement — alternative: a new ID "the written-back trailing trivia ends in the detected line ending". FMT-PIPE-010 is where "keep the trailing trivia" is stated, so the qualifier belongs there.
- No change to SHARED-FIX-008, which already states the behavior.

### Follow-up candidates

- **Leading trivia written back with foreign line endings.** FMT-PIPE-018 (and FMT-PIPE-010 for `FormatNodeInDocumentAsync` when it keeps leading trivia) writes the target's leading trivia back "as it stands", explicitly restoring the line endings the preparation phase rewrote. SHARED-FIX-008's span is "the span the fix replaces, rewrites, or moves, its trailing trivia included", which names trailing trivia but not leading. In a CRLF document, an RH5111 enum-member fix whose target has a blank line above it ending in LF:

  ```csharp
  enum E␍
  {␍
      A,␍
  (blank line ending in \n)
      B =␍
      2␍
  }␍
  ```

  today keeps that `\n`. Reading 1 (leading trivia is not the fix's span; SHARED-FIX-007 keeps it as it stands): output keeps the LF. Reading 2 (it is part of the replaced node; SHARED-FIX-008): the LF becomes CRLF. Recommendation: reading 1 — the leading trivia is the author's layout that SHARED-FIX-007 and FMT-PIPE-018 explicitly keep; SHARED-FIX-008 could say so. Outside #1695, which is about trailing trivia only.

## thoenissen/Reihitsu#1696

### Changed requirements

_None._ The RH8402 spec states the behavior the issue calls a defect: RH8402-002 says "Every line break of the header MUST be written as the first line break of the file, or as `\n` when the file has none", and RH8402-005 says the separator behind the header is written "in the line ending of the file's first line, or `\n` when the file has only one line". The code (`CopyrightHeaderTemplateResolver.NormalizeLineEndings`, `GetPreferredLineEnding`) follows that spec. SHARED-FIX-008 (detected ending; `Environment.NewLine` without a line break) contradicts it, and RH8402's `Uses` does not list SHARED-FIX-008. Two unchanged requirements, two outputs: an output question. Implementing the issue changes RH8402-005 (and the fix's use of RH8402-002), so **a change doc is required and the lane widens to behavior change** if the answer is B.

### Examples for the tests

None until the answer. On B, the change doc's examples (all-lone-`\r`, mixed document whose first line ends in the minority ending, one-line file) are the test seeds.

### Output questions

**2 · RH8402 · Files must start with configured copyright header — line ending of the inserted header**

Configuration as in the RH8402 spec (`copyrightText` with three `//` lines), file `C.cs`. Input — a document whose first line ends in LF and whose other line breaks are CRLF (`␍` marks a `\r`; shown line breaks are only for display):

```csharp
namespace N;
class C␍
{␍
}␍
```

Before (`main`): the header's line breaks and the separator behind it are LF (the first line's ending):

```csharp
// <copyright file="C.cs" company="Example">
// Copyright (c) Example.
// </copyright>
namespace N;
class C␍
{␍
}␍
```

After, A: unchanged (RH8402-002/005 stand; the header follows the first line's ending, `\n` for a one-line file).

After, B (recommended): the header and the separator use the line ending the formatter detects for the document (CRLF here; `\n` in an all-lone-`\r` document, where A writes lone `\r`; `Environment.NewLine` in a file without a line break, where A writes `\n`):

```csharp
// <copyright file="C.cs" company="Example">␍
// Copyright (c) Example.␍
// </copyright>␍
namespace N;
class C␍
{␍
}␍
```

Why B: every other fix writes the detected line ending (SHARED-FIX-008), and the formatter would otherwise rewrite the header the fix just inserted. B keeps the analyzer consistent with the fix: after the fix, the file's first line break is the header's own, so RH8402-002's comparison in the first line's ending still matches. On B, whether RH8402-002's analyzer wording changes too is an internal choice (it need not; only the fix's write changes).

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1707

### Changed requirements

_None._ The case is decided by unchanged requirements: SHARED-FIX-006 ("A code fix MUST change only the code its diagnostic is about; the layout of the code around it MUST stay as it was"), RH3003-004 and RH3005-004 ("keep the trivia in front of and after" the literal / the comparison). The line break behind `+` in the RH3003 input is trailing trivia of `+`, outside the replaced literal, so turning its CRLF into LF changes code outside the fix: a bug against SHARED-FIX-006. SHARED-FIX-008 does not cover it (a line break outside the span), which is why SHARED-FIX-006 is the governing requirement. No clarification needed; no change doc.

Note for the skeptic and test implementer: RH3005-006 withholds the RH3005 fix when a comment stands inside the comparison, between its first and last token. The issue's RH3005 shape ("a comment on the line next to an operand") is only reachable where the comment stands in front of or behind the comparison, e.g. in a CRLF document:

```csharp
class C
{
    bool M(int a)
    {
        return
            /* x */ 0 {|RH3005:<|} a;
    }
}
```

A comment between `0` and `<`, or between `<` and `a`, withholds the fix and is not this issue's case.

### Examples for the tests

None new: RH3003-004 and RH3005-004 examples, run with CRLF line endings and a comment on the line next to the replaced node (outside the comparison for RH3005).

### Output questions

_None._

### Internal choices

- No explicit line-ending sentence added to SHARED-FIX-006 — alternative: name the line ending of the gaps next to the replaced node. "Change only the code its diagnostic is about" already covers it, and the earlier RH3006 repair of the same defect added none.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1721

### Changed requirements

_None._ The issue's second reading is the specified behavior: FMT-PIPE-007 states "A line break of a single character, such as a lone `\r`, MUST count as `\n`, so a file whose line breaks are all a lone `\r` gets `\n`" and "In a file whose line breaks are all a lone `\r`, every line break of the result is `\n`"; FMT-EOL-004 keeps every line break inside a literal (raw included), and FMT-PIPE-019 excludes literal line breaks from its normalization. So the output for the issue's input is `\n` outside the raw literal and lone `\r` inside it, by design. The issue's first reading ("the detected ending should be `\r`") contradicts FMT-PIPE-007, an unchanged requirement whose `Why:` gives the reason (a lone `\r` is not a line ending the formatter writes). No output question, no clarification, no change doc. A test pinning the observed bytes for the issue's input is a characterization test against FMT-PIPE-007 and FMT-EOL-004; if the observed output differs from that, it is a bug against those requirements.

### Examples for the tests

- FMT-PIPE-007 (the all-lone-`\r` sentence) together with FMT-EOL-004, on the issue's input.

### Output questions

_None._

### Internal choices

- No additional example combining a lone-`\r` file with a multi-line raw literal under FMT-PIPE-007 — alternative: add one. The two requirements already decide it in words, and this lane writes only what a case needs to be decided.

### Follow-up candidates

_None._

## Whole run

- Change docs: none written. A change doc becomes required, and the lane widens to behavior change for the whole run, if question 2 is answered B, or question 1 is answered B.
- Spec diff: `sdd/specs/formatter/pipeline.md` (FMT-PIPE-010 and `Uses`). No new requirement IDs, so `sdd/specs/requirement-ids.txt` is unchanged; no rule page changed.
