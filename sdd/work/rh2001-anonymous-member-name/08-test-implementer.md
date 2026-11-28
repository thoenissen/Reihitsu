Role: test implementer
Commit: 5d2f6f4 (the head the tests ran on; this handover and the test change are one commit on top of it)
Verdict: tests written

Repair round 1 (lead decision 11): auditor findings 1 and 2 from `07-auditor.md`. The prompt carried no intended fix, cause, or hoped-for verdict. I treated the auditor's suggested change as a suggestion and derived the test from the code path it names (`DocumentOrderFixAllProvider.ApplySolutionFixAllAsync`, which re-analyzes a document whose text an earlier document's fixes changed) and from SHARED-FIX-002. No production code was touched.

Changed file: `tests/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests.cs`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixAllFixesAPropertyInAnotherPartialPartWhoseDeclarationTheFixOfTheFirstPartMoves` (new; finding 1) | RH2001-012, SHARED-FIX-002 (FC-008, FC-017) | `Verify` with a second document `/0/Test1.cs`; `NumberOfFixAllInProjectIterations = 1`, `NumberOfFixAllIterations = 1` (solution), `NumberOfFixAllInDocumentIterations = 2` | green on the head |
| `VerifyFixWritesTheNameOfAMemberNamedByTheBareReference`, `VerifyFixKeepsACommentOnTheLineAboveAMemberWhoseNameItWrites` (finding 2) | RH2001-012 (unchanged) | `Verify` | green; they now call the inherited `ConvertEndOfLine`, and the private `WithEndOfLine` and its `Methods` region are deleted |

Shape of the new test: document 0 holds `partial class Sample { private int Value { get; set; } }`. Document 1 holds three `object Xxx() => new { Value };` members, followed by `private int Name { get; set; }`. In document 1, the fix of document 0 writes `Value = _value` three times, which adds 27 characters in front of `Name`'s declaration. That is more than the 16 characters of the declaration's line in front of the identifier, so a diagnostic span taken from the starting text lands in `Third`'s line, outside `Name`'s declaration. Expected values: `private int _value;`, the three `new { Value = _value }` from RH2001-012's examples, and `private int _name;` from RH2001's field example.

The document-scope iteration count is 2 by design. Fix All in document fixes only the triggering document per pass, so two documents with one diagnostic each take two passes, and the new branch is not reached in that scope. Project and solution scope are pinned to one pass, which is what SHARED-FIX-002 asks for.

Mutation check (finding 1's falsifier): I changed `if (currentText.ContentEquals(startingText) == false)` in `DocumentOrderFixAllProvider.cs` temporarily to `... == false && false` and ran the RH2001 filter. Result: 1 failed out of 341, which was exactly the new test, with "Context: Fix all in project / Expected '1' iterations but found '2' iterations". The mutation is reverted (`git status` shows only the test file).

## Scenarios (bug fix)
_N/A_ (behavior change, repair round).

## Not covered, and why
- Lead decision 10 (a re-analyzed document uses every diagnostic on its current code, not only those offered on the starting code): accepted as a rare class without a guard, and the auditor could not construct an RH2001 input for it. No test.
- CRLF variant of the new test: the change is a span shift across documents, so line endings do not matter. The new branch compares texts and re-analyzes regardless of separators, and the existing LF and CRLF tests already cover the written separator.
- The auditor's hints (Fix All title, mode flag, unused `WithChanges`, cost, elastic-marker comment) were not routed to me (decision 11).

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests.VerifyFixAllFixesAPropertyInAnotherPartialPartWhoseDeclarationTheFixOfTheFirstPartMoves"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsed"
scripts/test.sh --project architecture
```

On this head, in Debug: the RH2001 filter gives 341 passed and 0 failed, and the architecture tests give 221 passed. `scripts/format.sh` ran over the test file; it re-indented the configuration lambda.

## Follow-up candidates
_None._
