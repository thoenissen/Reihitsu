Role: spec author
Commit: ea5d52b
Verdict: spec written

Amendment follow-up to `02-skeptic.md` (*Internal choices* 1 and 2; `00-lead.md` decisions 4 and 5). No change doc is needed: both amendments describe today's behavior (a clarification). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

I checked both behaviors at ea5d52b with fixtures in the session scratchpad. `scripts/apply-fix.sh RH3004` gives the same status under LF and CRLF:
- a14 (`)` on a line after the comment's last line) is fixed to `second */⏎                            ) => x;`.
- a04 (comment between `int x` and a comma on a later line) gives `no-fix-offered`. `scripts/format.sh` on the built lambda reports `unsupported syntax: a comment or directive between a list element and the comma behind it`, which is FMT-UNSUP-009.
- I ran the two new spec examples literally, with the markup stripped. Example 1's expected block matches the fix output line for line. In example 2, `a` is fixed and `b` is left unchanged, as the spec states.

## Changed requirements

- RH3004-011: A comment over several lines in the parameter list keeps its later lines. Status: clarified.
  - The sentence "The expression body MUST follow on the comment's last line…" now reads: the closing parenthesis stands where the formatter places it (SHARED-FIX-007), and ` => ` and the body follow it on its line. That is the comment's last line when `)` stands there, and a later line when `)` stands on a line after the comment's last line.
  - Added one sentence: a comment between a parameter and the comma behind it, with the comma on a later line, makes the formatter refuse the built lambda (FMT-UNSUP-009), so no fix is offered (SHARED-FIX-010). A comment behind the comma does not withhold the fix.
  - Added two examples: the a14 shape (fixed), and a comment behind a comma (`a`, fixed) next to a comment in front of a comma (`b`, no fix offered).
- RH3004-003: The fix writes a lambda. Status: clarified. Its list of conditions that withhold the fix now names SHARED-FIX-010.
- `Uses` gains SHARED-FIX-010 and FMT-UNSUP-009. The spec-structure test requires FMT-UNSUP-009 because RH3004-011 now references it.

The rule page `documentation/rules/RH3004.md` is unchanged, because the amendments cover a withheld fix and a corner-case layout.

`scripts/test.sh --project architecture`: 197 pass. 1 fails, as expected: `EveryRequirementOfAnEnforcedSpecIsCovered`, `RH3004-011: no test covers it`. The test implementer's tagged test clears it.

## Examples for the tests

- RH3004-011, first example: `a` is a comment over two lines with `second */` in column 46, `c` is a comment on one line. Both are fixed.
- RH3004-011, second example: `)` on a line after the comment, fixed to `) => x;` on that line.
- RH3004-011, third example: `a` is fixed and `b` gets no fix. Tag the `b` test with SHARED-FIX-010 as well.
- RH3004-008: the example added in `01-spec-author.md`, unchanged.

## Output questions

_None._ Both amendments write down today's output, which SHARED-FIX-007, FMT-UNSUP-009, and SHARED-FIX-010 already decide.

## Internal choices

- The column of `)` in the a14 shape is not written as a number in the normative sentence. The sentence says "where the formatter places it (SHARED-FIX-007)", and the example shows the column. The alternative was to state column 28 (one column right of `(`) as a requirement. That would restate a formatter rule in RH3004.
- The a04 withholding is placed in RH3004-011 and in RH3004-003's list, as the skeptic recommended. I did not add it to RH3004-008, which covers spanning strings and comments on the fix's own terms rather than the formatter's refusal.

## Follow-up candidates

_None._
