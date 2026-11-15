Role: spec author
Commit: 2fc94f7
Verdict: spec written

## Changed requirements

- FMT-USING-009 — The header above the first directive stays at the top — clarified. The normative paragraph now says what a blank line is for the header split, by reference to SHARED-BLANK-001: a line that holds nothing but whitespace before its line break; an empty line and a line of only spaces or tabs both end the header, in every line ending; a line that holds a comment does not. A new example (a line of four spaces below `// Header`) sits between the "comment after the header's blank line moves" example and the "comment directly above the first directive stays" example, so the qualifier has an example on each side (FC-002).
- `sdd/specs/formatter/using-directives.md` header: `Uses` gains SHARED-BLANK-001 and FMT-CLEAN-001 (the new example names both).
- No new requirement ID, no change doc: intended behavior does not change. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` passes (51/51).

How the reading was established — every statement of "blank line" in the specs already counts a whitespace-only line:

- SHARED-BLANK-001 (`sdd/specs/shared/blank-line-spacing.md`): the formatter counts as blank a line that holds nothing but whitespace, "whether it is empty or holds spaces"; RH5011, RH5020, RH8303, statement-blank-lines and FMT-BLANK already cite it as the definition.
- SHARED-USING-008 and SHARED-USING-011 (`sdd/specs/shared/using-directive-ordering.md`) count line breaks in front of the first comment, so whitespace between them is irrelevant.
- SHARED-FIX-007, *Moved code* bullet: "a blank line ... — a line that holds only whitespace".
- The same phase already reads it this way for FMT-USING-010: `UsingDirectiveOrderingUtilities.HasBlankLineAbove` counts `EndOfLineTrivia` and ignores whitespace between them. Only `UsingLeadingTriviaBuilder.GetHeaderSplitIndex` requires two adjacent `EndOfLineTrivia`.

No spec reading supports "empty line only", so the issue's case is decided by the spec as clarified and is a bug, not an output question. The issue's premise "the spec does not say" is overstated: SHARED-BLANK-001 said it; FMT-USING-009 only did not cite it.

The issue's "Required output" is not adopted: it puts `// About Linq` directly under `using System;` without a blank line. FMT-USING-010 (unchanged) states that the blank line ending the header counts as standing above the first comment that moves with the directive, and its existing example shows a blank line there. The spec's new example follows FMT-USING-010.

## Examples for the tests

- FMT-USING-009 — the new whitespace-only example (formatter, whole document). Expected output per the spec, for LF and CRLF:

```csharp
// Header

using System;

// About Linq
using System.Linq;

class C
{
    int x;
}
```

- For the RH7203 code fix, which takes the phase's output as its layout (FMT-PIPE-030): the header lines stay as written (FMT-USING-011, "its own lines stay as written"), so the four spaces on the header's blank line remain in the fix's result; the blank line above `// About Linq` is the one FMT-USING-010 places. Expected fix result for the issue's input:

```csharp
// Header
    
using System;

// About Linq
using System.Linq;
```

(line 2 holds four spaces, as in the input).

Class members for the skeptic and the test implementer to enumerate (each follows from the clarified wording; none is a new requirement): a tab-only line; several whitespace-only lines; a whitespace-only line between two header comments ("the last blank line among them"); a whitespace-only line directly above the directive with no comment after it; LF and CRLF; the same inside a namespace with a body (FMT-USING-011). SHARED-BLANK-001 also counts the line below a `///` documentation comment as blank; `GetHeaderSplitIndex` would miss that too, because the `///` trivia carries its own line break. Whether a caller reaches a `///` comment above the first using is for the skeptic to establish.

## Output questions

_None._ The clarified FMT-USING-009, together with the unchanged SHARED-BLANK-001 and FMT-USING-010, decides the visible output of the issue's input for both the formatter and the RH7201–RH7207 fixes.

## Internal choices

- Clarify by citing SHARED-BLANK-001 in FMT-USING-009 instead of restating the definition, or adding a requirement; alternative: a new FMT-USING requirement defining "blank line" for the whole spec. Rejected because the shared definition exists and the rulebook says to reference it.
- Limit the citation to FMT-USING-009; FMT-USING-002, FMT-USING-004 and FMT-USING-010 are not changed, since their code already counts whitespace-only lines (`HasBlankLineAbove`) and the issue does not concern them. Alternative: cite SHARED-BLANK-001 in each.
- Show the whitespace-only line in the spec example with literal trailing spaces, as `sdd/specs/analyzer/RH3004.md` and `RH7302.md` already do.
- No rule page changes: nothing a user reads about RH7201–RH7207 changes.
- Prompt carried no intended fix or verdict; the issue's "Cause" paragraph and "Done when" items were treated as proposals.

## Follow-up candidates

_None._
