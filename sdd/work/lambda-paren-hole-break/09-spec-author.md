Role: spec author
Commit: df82ef3
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named three amendments, which I checked against the tree at df82ef3 (production code from 4d65391, tests from 4586769).

## Changed requirements

No requirement changed. The spec diff matches the implemented tree:

- **FMT-BREAK-011.** `LineBreakListRewriter.VisitParameterList` skips only `CollapseOpenParenToDeclarationLine` for a `ParenthesizedLambdaExpressionSyntax`. The first-element, comma, and closing-parenthesis collapses still run, as the requirement states.
- **FMT-INDENT-031.** `LambdaAlignmentContributor` anchors at `async` when present, otherwise at `delegate` (also behind `static`). It applies when the innermost `InterpolationSyntax` around the anonymous method belongs to a multi-line raw string and no `BlockSyntax` lies between them. Inside a hole's expression, a block can only be the body of a lambda or an anonymous method, so that matches the requirement's exception.
- **FMT-RAW-007.** Counts FMT-INDENT-031 among the hole alignments, as before.

**Code owners (amendment 2).** No edit was needed. The FMT-INDENT header in `sdd/specs/formatter/indentation.md` and its row in `sdd/specs/README.md` already list `LambdaAlignmentContributor`, which now implements FMT-INDENT-031. FMT-BREAK-011's owner, `LineBreakListRewriter`, was already listed for FMT-BREAK.

The statement that the owner "does not exist yet" appears only in my handover `03-spec-author.md` (line 18). No spec or change doc said it. That handover stays as written, since handovers are a record. This handover replaces the statement: FMT-INDENT-031 has no new type, and its owner is `LambdaAlignmentContributor`.

**Change doc** `sdd/changes/2026-11/lambda-paren-hole-break.md`:

- **Test expectations (amendment 1).** *The test expectation changes* now lists three tests:
  - `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass`
  - `ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration`
  - `ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift`

  For the two constraint-clause tests, the section quotes the new expected output: the block under `delegate`, in column 26. It also says that on `main` the same lines started in column 17. This is the layout of entries 1 and 2 applied to a block that was already broken. It is covered by maintainer answer 1 and lead decision 7, and it is not an output change of its own.
- **Status.** Changed from `Draft` to `Accepted`. Under `sdd/README.md`, a change doc is `Accepted` once every visible output change it lists has the maintainer's answer, and the lead sets it. Lead decision 4 recorded the acceptance, but the header was never updated. Entries 1–7 are covered by answers 1–6.
- **Rest of the doc.** Everything else still matches the tree:
  - What and Why
  - entries 1–7
  - *Unchanged*: `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` exists
  - the decisions and the alternatives

**Visible output.** My edits add no visible output change.

## Examples for the tests

Unchanged. FMT-BREAK-011 and FMT-INDENT-031, as in `05-spec-author.md`.

## Output questions

_None._

## Internal choices

- **"In an interpolation hole" left as written.** FMT-INDENT-031's first sentence says "lies in an interpolation hole of a multi-line interpolated raw string literal". The code tests the innermost hole around the anonymous method. The two readings differ only for an anonymous method in a hole of a regular or single-line interpolated string that is itself nested in a raw string's hole. I did not reword the requirement, because I did not run that input and so cannot state its output. The requirement's own exception paragraph already uses the innermost hole. Alternative: narrow the first sentence to "the innermost interpolation hole that contains it". That should be done only after a test shows the code's output for the nested case.

## Follow-up candidates

Unchanged from `03-spec-author.md`. The lead's list in `00-lead.md` is current.
