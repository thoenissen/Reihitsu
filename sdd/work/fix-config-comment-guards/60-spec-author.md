Role: spec author
Commit: cb3d3ae2
Verdict: text written

The prompt named decision 55 and audit 8's #27 and #28; it carried no intended fix, suspected cause, or hoped-for verdict beyond what decision 55 records.

## Changed requirements

- RH2001-004 — The fix replaces the property with a private field — clarified: the coverage paragraph now excludes an `#if`, `#elif`, `#else`, or `#endif` in front of the type of a property without a modifier, also on the lines above it, and names RH2001-009 for it (#28). The two requirements are disjoint again.
- `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md`: the fourth *What* bullet no longer says "as on `main`"; it says `main` withholds the case only for a get-only property and fixes a read-write one. *Visible output changes* lists a1 (whole-member group above a read-write property) and a9 (read-write property inside an `#if !DEBUG` block), before (`main`) and after (no fix offered) (#27).

## Examples for the tests

a1 in the change doc (`#if DEBUG` / `private int _trace;` / `#endif` / `int Value { get; set; }`): no code fix is offered, LF and CRLF. The test implementer pins it in parallel (`61-test-implementer.md`).

## Output questions

_None._ Decision 55 keeps the refusal under the standing answer.

## Internal choices

- The `main` outputs for a1 and a9 are written from `57-auditor.md`'s description ("fixes it to `int _value;` with the group kept above", "fixes it correctly"); the auditor's fixtures were removed, so the exact text was not re-measured. The class wrapper and the `private int _x;`/`private int _trace;` lines are the auditor's fixture content as quoted. Alternative: re-run `scripts/apply-fix.sh` on `main`, outside this role's narrow task.
- The change doc's closing sentence for the new block states only what audit 8 measured for get-only properties (a3, below a whole-member group), not a claim about a get-only property inside a block.

## Follow-up candidates

_None._
