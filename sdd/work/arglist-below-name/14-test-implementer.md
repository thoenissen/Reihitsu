Role: test implementer
Commit: 773d38a
Verdict: tests written

## Merge of origin/main (1102aa5)

Resolved the two test-file conflicts by keeping both sides' tests unchanged; no assertion was edited.

- `src/Reihitsu.Analyzer.Test/Formatting/RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs` — kept this run's `VerifyFixJoinsElementArgumentListBelowItsName` (FMT-BREAK-039) and main's `FixIsOfferedForAnInitializerInsideACombinedFieldWithADirectiveBetweenItsDeclarators` (FMT-UNSUP-003, FMT-UNSUP-010).
- `src/Reihitsu.Analyzer.Test/Formatting/RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs` — kept this run's `VerifyFixJoinsWithElementArgumentListBelowWith` and `VerifyFixJoinsWithElementArgumentListsOfTwoCollectionExpressions` (FMT-BREAK-039) and main's `VerifyFixLeavesNoBlankLineAboveTheFirstCommentOfTheCollectionExpression` (RH5305-004).

Merge commit: `773d38a Merge origin/main [skip ci]`.

## Test runs on the merge commit

| Command | Result |
|---|---|
| `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5303CollectionInitializer\|FullyQualifiedName~RH5305CollectionExpressions"` | 123 passed, 0 failed |
| `scripts/test.sh --project formatter --filter "FullyQualifiedName~ArgumentListLineStartTests"` | 50 passed, 0 failed |

No test merged from `main` fails with this run's change.
