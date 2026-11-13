Role: spec author
Commit: c7556d1
Verdict: spec written

Three text corrections from lead decisions 15, 16, 17; no new requirement IDs. SDD architecture tests (`Reihitsu.ArchitectureTests.Sdd`): 51 passed.

## Changed requirements
- SHARED-FIX-003 — fix withholding — clarified: the RH7004 example "a pair that lies wholly in the moved statements" now says the fix is offered with `DEBUG` defined and withheld without it, because `s.Flush();` is then disabled text among the moved statements (RH7004-007). Decision 15.
- RH7302-005 — the fix renames the `#endregion` that repeats the name — clarified: the trailing-whitespace example marks `{|RH7302:#region members  |}`, the span running to the end of the line as RH7302-003 states; the lead-in sentence names the two trailing spaces as part of the span. Decision 16.
- SHARED-FIX-007 — *Moved code* bullet — clarified: the column-0 comment rule holds for every line of the moved code, a moved member's own leading trivia included; such a comment travels (SHARED-ORDER-002) and keeps column 0. Decision 17.
- SHARED-ORDER-002 — separators stay, attached trivia travels — clarified: one sentence pointing to SHARED-FIX-007 for the column of each travelling line, column-0 comments keeping column 0. Decision 17.

## Examples for the tests
None new. The RH7004 `DEBUG`-undefined case and the RH7302 trailing-whitespace span already match the tests of 05-test-implementer.md; decision 17 adds no test (rare class).

## Output questions
_None._

## Internal choices
- Decision 17 is stated in SHARED-FIX-007 (owner of moved-code layout) with a one-sentence pointer in SHARED-ORDER-002, rather than an exception written only in SHARED-ORDER-002.

## Follow-up candidates
_None._
