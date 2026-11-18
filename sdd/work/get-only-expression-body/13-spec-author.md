Role: spec author
Commit: ee9d8a39e3d14b60023d5fd3ab295a94603e8038
Verdict: spec written

Repair round 1, auditor finding 3 (11-auditor.md). My prompt carried no intended fix or hoped-for verdict beyond the worklist; I worked from 00-lead.md (decisions 12, 13) and 11-auditor.md.

## Changed requirements

None. Header and index only:

- `sdd/specs/shared/get-only-expression-bodies.md`, `Code owners`: now `GetOnlyMemberExpressionBodyRewriteUtilities`, `GetOnlyMemberExpressionBodyAnalyzerBase`, `GetOnlyMemberExpressionBodyCodeFixProviderBase` (all three exist on the head). The introduction gains one sentence naming what the two bases own: the analyzer base reports and places the diagnostic (SHARED-GETONLY-001, -004), the code-fix base decides whether the fix is offered and writes its result (SHARED-GETONLY-003, -005, -006, -007).
- `sdd/specs/README.md`: the SHARED-GETONLY row lists the same three types.

`scripts/test.sh --project architecture`: 198 passed.

### Entries that depend on the implementer's coming repair (decision 12)

- `GetOnlyMemberExpressionBodyCodeFixProviderBase` stays an owner either way: it registers the fix and runs the SHARED-GETONLY-006 query.
- If the repair moves the signature-kept splice into a shared helper, that helper's type joins this spec's owners. If it is `ExpressionBodyCodeFixUtilities` (already a SHARED-BODY owner), add it to the SHARED-GETONLY header and row; if it is a new type, add it to both and, if it also serves RH3202's direction, to the SHARED-BODY row.
- `BlankLineCodeFixUtilities` (via `FormatMemberOnLinesOfItsOwnAsync`) is not added: it owns SHARED-FIX-007's shared-line bullet, which this spec already `Uses`, so the narrow wrapper repair changes no owner here.
- Whoever lands the repair re-checks this header; the spec checker should confirm it against the repaired head.

## Examples for the tests

None new.

## Output questions

_None._

## Internal choices

- Did not list `BlankLineCodeFixUtilities` as an owner: SHARED-FIX-007 owns the shared-line behavior and is referenced through `Uses` (alternative: list every type the fix calls, which the index does not do for other shared specs).

## Follow-up candidates

_None._
