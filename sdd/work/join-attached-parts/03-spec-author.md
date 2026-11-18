Role: spec author
Commit: 3497b33
Verdict: spec written

Amendment for decision 4 of `00-lead.md` (skeptic's internal choice on `[` tokens that are not attached parts). The prompt carried no intended fix or hoped-for verdict beyond the decision itself.

## Changed requirements

- FMT-BREAK-040 — An element access or a null-forgiving operator starts on the line of the code in front of it — changed (still Draft with the change doc): the boundary of `[` tokens is now stated by its cause. The joined `[` is the opening bracket of the bracketed argument list of an `ElementAccessExpressionSyntax`, or of the `ElementBindingExpressionSyntax` behind a conditional element access's `?`; every other `[` is not decided by this requirement. The list that follows is explanation only and now names the implicit element access (`ImplicitElementAccessSyntax`, `["a"] = 1` in an object or dictionary initializer), the implicit array creation (`new[]`), the implicit `stackalloc` array creation (`stackalloc []`), and array rank specifiers (`new int[3]`, `stackalloc int[3]`, `int[]`), besides the collection expression, list pattern, attribute list, indexer parameter list, and function-pointer calling-convention list it had.
- New third example of FMT-BREAK-040, kept as written: a dictionary initializer whose implicit element accesses start their own lines after `{` and `,`, plus one-line `new[] { 1, 2 }`, `new int[3]`, `stackalloc [] { 1, 2 }`, `stackalloc int[3]`. Checked with `scripts/format.sh` on the current code: "Formatted 0 of 1 file(s)".

Change doc `sdd/changes/2026-11/attached-part-below-element.md` (Status left at Draft): the decision "The join is stated by its cause" now states which `[` is joined and why the implicit element access and the array/stackalloc creations are not; *Visible output changes* gains a closing paragraph that these keep today's output.

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, FMT-BREAK-040 has no covering test (expected).

## Examples for the tests

- FMT-BREAK-040: the new kept-as-written example (dictionary initializer, `new[]`, `new int[3]`, `stackalloc []`, `stackalloc int[3]`). Also lock the dictionary initializer with a line break in front of each `["a"]` (already in the example) and the skeptic's sibling shape `["b"] = arr⏎[0]`, where only the `[0]` joins.
- The line-break-in-front forms `new⏎[]`, `new int⏎[3]`, `stackalloc⏎[]` are deliberately not in a spec example: today's output (the `[` at the statement's column) looks wrong and is a follow-up candidate (`00-lead.md`), so the spec does not write it down. A test may still assert that this change leaves them as on `main` (characterization), without a requirement tag for their column.

## Output questions

_None._

## Internal choices

- Stated the boundary positively by the owning node kinds (`ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`) and turned the list into explanation, rather than only extending the exclusion list (role rule 6). Alternative: append the three kinds to the existing list.
- Wrote `stackalloc []` with a space in the example and the requirement text, because FMT-SPACE (horizontal-spacing.md, the `stackalloc` keyword rule) specifies that spacing and the formatter produces it; `stackalloc[]` would not be kept as written.
- The kept-as-written example shows `new[]`, `new int[3]`, and `stackalloc` only on one line, so it does not specify the statement-column output of their split forms (see above).

## Follow-up candidates

_None new._ The split `new⏎[]` / `new int⏎[3]` / `stackalloc⏎[]` is already listed in `00-lead.md` (02-skeptic).
