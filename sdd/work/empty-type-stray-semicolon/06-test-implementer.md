Role: test implementer
Commit: <see git log: "Test implementer: carry out lead decisions 5 and 6">
Verdict: tests written

Correction round for the lead's decisions 5 and 6 (`00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5404 `VerifyNestedEmptyTypeFollowedByASemicolonConvergesToSemicolonDeclaration` (LF, CRLF). Input now starts with `namespace N;` and a blank line, so the source carries the row's line ending; the asserted output keeps its shape (`class H⏎{⏎    class I;⏎}`) behind the same two lines. Decision 5 | RH5404-003; FC-030 | `VerifyCodeFixWithEndOfLine` | pass (both rows) |
| RH5412 `VerifyTwoEmptyClassesWithSemicolonsOnTheirOwnLinesAreFixed`, renamed from `VerifyTwoEmptyClassesWithSemicolonsOnOneLineAreFixed`. Input is `class A { };⏎class B { };`, each declaration on its own line; Fix All still corrects two diagnostics in one document in one iteration. Decision 6 | SHARED-EMPTY-005 | `Verify` (fix one, iterative, Fix All) | pass |

The rename follows the input: the old name described two declarations on one line, which the test no longer covers.

## Scenarios (bug fix)
_N/A — behavior change._

## Not covered, and why
- Two reported declarations on one line under Fix All: needs two iterations, with or without `;`, on the shared-line helper under `BatchFixer`. Per decision 6 this is follow-up candidate 3 in `00-lead.md` and is deliberately not tested here. The iterative and single fix of that layout are not re-pinned either, as the lead asked for the one-line case to be left to the follow-up.

## Commands
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~VerifyNestedEmptyTypeFollowedByASemicolonConvergesToSemicolonDeclaration|FullyQualifiedName~VerifyTwoEmptyClassesWithSemicolonsOnTheirOwnLinesAreFixed"` — Passed 3.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5404|FullyQualifiedName~RH5412"` — Passed 105.

## Follow-up candidates
_None_ beyond candidate 3 already recorded in `00-lead.md`.
