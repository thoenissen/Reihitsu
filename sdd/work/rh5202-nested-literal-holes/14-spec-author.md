Role: spec author
Commit: 8b6674f
Verdict: spec written

Amendment in repair round 2 for lead decision 10, which reverses decision 7. This handover replaces the tag list in `13-spec-author.md`. The message carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- **RH5202-005** is retitled "The fix leaves an empty line empty" (clarified). Every empty line after the first stays empty. That covers the literal's own content lines, the content lines of a raw string nested in a hole at any depth, and lines of hole code outside every literal's text. Empty lines inside a nested verbatim string or comment stay under RH5202-004.
  - **Qualifier "Empty":** a whitespace-only content line shifts. It has a six-to-eighteen-space example.
  - **`Why`:** grounded in SHARED-FIX-007 and FMT-RAW-002. For a hole-code line it also cites SHARED-FIX-006, because the formatter removes such a blank line and a fix does not remove blank lines.
  - **Examples, one per kind of empty line:** plain content (12 to the right), whitespace-only content, interpolated content, nested raw content (13 to the right, inner literal stays reported), and empty hole code (13 to the right, now stays empty).
  - **Removed:** the decision-7 examples (hole line becomes 13 spaces, and the leftward variant).
- **RH5202-004:** the raw-string bullet now says "apart from an empty line, which RH5202-005 leaves empty".
- **RH5202-003:** unchanged. Its "apart from the lines RH5202-004 and RH5202-005 leave as written" already defers to RH5202-005.
- **Header `Uses`:** gains SHARED-FIX-006, which the new `Why` cites. No new IDs.

`scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 115, Total: 115`.

What the formatter does today, checked with `scripts/format.sh` on a scratch file (deleted afterwards):
- A 6-space content line, shifted 12 to the right, becomes 18 spaces. The fix and the formatter agree.
- An empty line of hole code before `}` is removed by the formatter.
- A whitespace-only line of hole code is also removed by the formatter.

## Examples for the tests

- **`VerifyFixShiftsEmptyLineInsideHole` and `…WithCrLf`:** the expected output changes to an empty line (decision 10). Tag RH5202-005 and RH5202-003. A rename such as `VerifyFixLeavesEmptyLineInsideHoleEmpty…` fits.
- **`VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingLeft`:** RH5202-005, plus RH5202-003.
- **The four `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmpty…` tests:** add RH5202-005, and keep RH5202-004 and SHARED-FIX-007.
- **The existing plain and interpolated empty-content-line tests:** keep RH5202-005.
- **A whitespace-only content line (the second example of RH5202-005):** RH5202-005 and RH5202-003. Add a test if none exists.
- **`VerifyFixShiftsWhitespaceOnlyLineInsideHole`:** RH5202-003 only. Do not tag it RH5202-005, because RH5202-005 does not state this line; see the output question below.

## Output questions

**RH5202 · Raw string literals should be formatted correctly: whitespace-only line of hole code.** This does not block the run: RH5202-003's general text covers today's behavior, and RH5202-005 leaves the line out.

Input:
```csharp
var a = $"""
    Hello {name
··
    }
    """;
```

Before (`main`, also at head): the fix shifts the line, which becomes 15 spaces (the test above).
```csharp
var a = $"""
                 Hello {name
···············
                 }
                 """;
```

Formatting either input or this output removes the line completely. So the fix's output is not the formatter's layout (SHARED-FIX-007), and the fix may not remove the line (SHARED-FIX-006). Decision 10 says "a whitespace-only line still shifts", but the SHARED-FIX-007 reason it gives for empty lines applies to this line too.

After, A (recommended): the fix treats the line like an empty hole-code line and writes it as empty.
```csharp
var a = $"""
                 Hello {name

                 }
                 """;
```

After, B: shift, as today (above).

Why A: the result is the closest the fix may come to the formatter's layout without removing a blank line. The line's whitespace is no part of any value.

If the lead picks A, RH5202-005 gains "a line of hole code that holds only whitespace is written empty", and the hole-code case moves to the other side of the "Empty" qualifier. If the lead picks B, RH5202-005 gains a statement and an example that this line shifts.

## Internal choices

- RH5202-005 does not state the whitespace-only hole-code line. The alternative was to specify today's shift (decision 10's wording). I did not, because with SHARED-FIX-007 as the grounding, the shift looks wrong (see above).
- I cited SHARED-FIX-006 for why the fix leaves an empty hole line empty instead of removing it as the formatter does. The alternative was to claim that the formatter keeps the line, but `format.sh` shows that it removes it.

## Follow-up candidates

_None_ beyond `00-lead.md`.
