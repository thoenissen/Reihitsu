Role: implementer
Commit: see the commit that adds this file
Verdict: test disputed

My prompt carried the change to make (make the RH5401 tests pass under the amended RH5401-003/-004, replace the blank-line write-back), which is the maintainer's escalation 9 answer B recorded in `00-lead.md`; it carried no suspected cause or hoped-for verdict.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs` | The blank-line write-back is removed: `FormatKeepingAttributeBlankLinesAsync`, `TryKeepBlankLines`, and `GetSlots` are deleted, and `ApplyCodeFixAsync` again calls `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, propertyDeclaration, cancellationToken)` as on `main`. A new guard, `KeepsBlankLinesBehindAttributeLists`, compares for each attribute list as written the blank-line count of the gap from its `]` to the next token with the same gap in the `FormatNode` result that `CanCollapse` already computes; the formatted gap is the one behind the formatted list holding the list's last attribute, so the gaps FMT-STRUCT-028 creates by splitting a list are not compared (example `L`). `CanCollapse` withholds the fix when a count differs. Blank lines are counted with `TokenGapAnalysis.Between(...).BlankLineCount` (Reihitsu.Core), so comments, directives, and disabled text in the gap are handled by the shared owner. Doc comments of `ApplyCodeFixAsync` and `CanCollapse` updated (FC-031) | RH5401-003, RH5401-004 |

## Policy owners

- Counting blank lines in a token gap: `rg -n "BlankLineCount" src --glob '!*Test*/**'` — `TokenGapAnalysis` (Reihitsu.Core) is the owner; the formatter's `BlankLineCollapser`, `BlankLineTriviaBoundaryRewriter`, and `BlankLineSpacingPolicy` use it. The RH5401 fix now uses it too; its private slot counting is gone.
- Whether formatting would change those blank lines: the fix does not predict the blank-line phase; it compares the input with the formatter's actual `FormatNode` result, which `CanCollapse` already produced for the one-line check.
- RH5307 is unchanged in this round (`IsIndexerArgumentList`, `CanSafelyCollapseToSingleLine` as in 06).

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 3 files changed, 264 insertions(+), 52 deletions(-). After this round, against `origin/main`: 3 files changed, 92 insertions(+), 49 deletions(-). Delta of the round, RH5401 fix only: 33 insertions(+), 202 deletions(-). No new type.

## Deleted special cases

_N/A._ The run declares no shape unsupported. (The write-back removed here was this run's own code, replaced as the maintainer decided.)

## Tests run

`scripts/format.sh` over the changed file: formatted 0 of 1.

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
# Failed!  - Failed: 2, Passed: 76, Skipped: 0, Total: 78   (the two rows below)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests"
# Passed!  - Failed: 0, Passed: 55, Skipped: 0, Total: 55
scripts/test.sh --configuration Debug --project analyzer
# Failed!  - Failed: 2, Passed: 11019, Skipped: 0, Total: 11021   (the same two rows)
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198
```

All 13 rows that 08 listed as failing for the new condition now pass, except the Fix All test below.

## Disputed tests or scope

- **`VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange` (both rows), `src/Reihitsu.Analyzer.Test/Formatting/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.cs`.** The test puts the withheld diagnostic of `D` as `{|RH5401:...|}` markup into the fixed code but calls `Verify` without enabling markup for the fixed states, so the testing library expects no diagnostic there. It fails with `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"` on the span of `D` (10,5)–(13,14). That is exactly the result the spec asks for: `E` and `A` fixed, `D` left as written and reported. Other tests in the repository that keep a withheld diagnostic in the fixed source set `test.FixedState.MarkupHandling = MarkupMode.Allow;` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow;` (for example `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.cs` around line 1288). I checked this on a temporary copy of the test with those two settings added: both rows pass with this commit's code. The copy was restored; the test file is unchanged. This is the test-harness defect 08 anticipated under *Not covered* ("that is my defect"); route it to the test implementer.

## Architecture findings

_None._ The architecture finding of 06 (fix and blank-line phase stating one rule twice) is resolved by this round: the fix no longer edits the formatter's blank lines; it only compares counts and withholds.

## Follow-up candidates

_None._
