Role: spec author
Commit: 9e85bab
Verdict: spec written

Repair round 2, finding 1 of `12-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict; the spec states lead decisions 7 and 9 as the code does at 9e85bab (`IsEmptyTextLine` in the RH5202 fix, and the tests named below).

## Changed requirements

- RH5202-005 — retitled "The fix leaves an empty line of literal text empty" (was "…an empty content line empty") — clarified. It now covers a content line of the literal's own text and a content line of a multi-line raw string nested in a hole, at any depth (decision 9). It states both qualifiers with a case on each side (FC-002): **Empty** (a whitespace-only line shifts) and **A line of literal text** (an empty line of hole code is not a content line and shifts as RH5202-003 states: rightward it receives the spaces, leftward it has nothing to remove and stays empty, decision 7). An empty line inside a nested verbatim string or comment stays under RH5202-004. New examples: a nested raw string with an empty content line (13 to the right), an empty hole-code line shifting right (written with `·` markers, the convention `region-formatting.md` uses), and the same line shifting left. The two earlier examples are kept.
- RH5202-004 — clarified: the "A literal other than a raw string" bullet now says that an empty content line of the nested raw string is the exception, which RH5202-005 leaves empty.

No new IDs. `Uses` is unchanged; every cited ID is already listed. No change doc: decisions 7 and 9 restore `main`'s output or follow the formatter under the unchanged SHARED-FIX-007/FMT-RAW-002. The rule page is unchanged.

`scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 115, Total: 115`.

## Examples for the tests

Tags the affected tests should carry:

- `VerifyFixShiftsEmptyLineInsideHole`, `VerifyFixShiftsEmptyLineInsideHoleWithCrLf`: RH5202-003 and RH5202-005. They are RH5202-005's empty-hole-code example (the other side of "a line of literal text"), and the shift is RH5202-003's. Their current tags are correct.
- `VerifyFixLeavesEmptyLineInsideHoleEmptyWhenShiftingLeft`: RH5202-005, the leftward hole-code example. Keep RH5202-003 if the test implementer wants it, because it is the shift with nothing to remove. Its current tags (003, 005) already match. The checker's concern was that 005 excluded hole lines, and the requirement now includes this case explicitly.
- `VerifyFixLeavesEmptyContentLineOfRawStringNestedInHoleEmpty…` (4 tests): add RH5202-005, the nested-raw example. Keep RH5202-004 (nested raw lines shift, inner literal stays reported) and SHARED-FIX-007.
- `VerifyFixShiftsWhitespaceOnlyLineInsideHole`: RH5202-003. RH5202-005 may be added, because it states the "Empty" qualifier's other side.

## Output questions

_None._ Decisions 7 and 9 settle both outputs.

## Internal choices

- The empty-hole-line behavior is stated inside RH5202-005, as the other side of its "line of literal text" qualifier, and not as its own requirement. Alternative: a new RH5202-006 "An empty line of hole code shifts". I chose the qualifier form because the checker asked for it there and FC-002 wants both sides in one place.
- The `Why` for the hole-code line states only that the fix shifts a hole as one block and does not lay out hole code. It does not claim that the trailing spaces are desirable. The formatter keeps such a line empty, so the fix's output differs from the formatter's on this line. Decision 7 accepted that difference as `main`'s output under RH5202-003.

## Follow-up candidates

_None_ beyond `00-lead.md`.
