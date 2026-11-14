Role: spec author
Commit: 8451138
Verdict: spec written

Repair round 1, finding 1 of `06-spec-checker.md` (7151d2c). The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

I re-derived the change from the diff, not from the finding:

- `ExpressionBodyToBlockCodeFixProviderBase<TNode>` delegates the conversion to `ExpressionBodyCodeFixUtilities.ConvertBodyAsync`.
- `ExpressionBodyCodeFixUtilities` formats the member through `ReihitsuFormatter.FormatNodeInDocumentAsync` and writes back only the block body. That write-back is the SHARED-BODY-003 behavior.
- The type exists as `internal static class Reihitsu.Analyzer.CodeFixes.Core.ExpressionBodyCodeFixUtilities`.

The other three listed owners still exist. RH3202 also calls the new overload of `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`. It does so through `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider`, which the RH3202 spec already lists, and SHARED-STMT already lists `BlankLineCodeFixUtilities`. So no other listing is missing.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"`: Passed 26, Failed 0.

## Changed requirements
None. Only the code-owner listing changed: `ExpressionBodyCodeFixUtilities` was added to the `Code owners` row of `sdd/specs/shared/expression-body-conversion.md` and to the SHARED-BODY row of `sdd/specs/README.md`.

## Examples for the tests
_None._ No requirement text changed.

## Output questions
_None._

## Internal choices
- I listed the type in the same place as the other owners, between the provider base and `ExpressionBodyRewriteUtilities`. The alternative was to list it only in the spec. I did not take it because the README row has to stay in sync.

## Follow-up candidates
_None._
