Role: implementer
Commit: c119a8a
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1642

No production change (decision 2). The five `MultiLineCommentGapSpacingTests` characterization tests pass after c119a8a. They are part of the full formatter run below.

## thoenissen/Reihitsu#1635

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | `CollapseSeparatorsToPreviousParameterLine` is generalized to `CollapseSeparatorsToPreviousElementLine<TNode, TElement>`, which reads the elements through an accessor. `VisitArgumentList` and `VisitAttributeArgumentList` now call it too, so a leading comma moves behind the element in front of it, and a comment behind the comma moves with it. A held comma (`WouldJoinAcrossUnjoinableTrivia`) is skipped, as before. | FMT-BREAK-012, FMT-BREAK-011, FMT-BREAK-001 |
| same | `EnsureSeparatorsHaveEndOfLine` counts a line break in front of a separator as a line break that separates two elements. It leaves that separator as written, without appending a line break behind it. Before, a held comma was split off onto its own line, and a list wrapped only in front of commas was never split. | FMT-BREAK-012 (held comma stays at the start of the next element's line; elements decide) |
| same | The indexer-only `isWrappedAfterOpener` signal is removed from `VisitBracketedParameterList`, `EnsureParametersOnSeparateLines`, and `EnsureSeparatorsHaveEndOfLine`. An indexer list wrapped only behind `[` is now joined like every other list (decision 9). | FMT-BREAK-012 (entry 6) |

XML summaries and the inline comment of `VisitBracketedParameterList` are updated to describe the new code (FC-031).

### Policy owners

- `rg -n "isWrappedAfterOpener|CollapseSeparatorsToPrevious|EnsureSeparatorsHaveEndOfLine|HasElementSplitSignal" src`: every hit is in `LineBreakListRewriter`. The comma-move policy now has a single owner for all four list kinds.
- `rg -ln "GetSeparator\(" src/Reihitsu.Formatter`: `GenericConstraintLineBreakRewriter` joins the commas of a constraint clause (FMT-BREAK-037). That is a different construct with its own layout, and it uses the same guard, `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`. It is not a copy of this policy. The other hits (`ListElementAligner`, and the structural transforms that remove trailing commas and split fields) do not decide where a comma's line breaks go.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 43 insertions(+), 44 deletions(-). No new type and no new public member.

### Deleted special cases

_N/A._ No shape is declared unsupported. The indexer-only opener signal was removed under decision 9.

### Tests run

```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ListWrapByElementsTests|FullyQualifiedName~IndexerParameterListLayoutTests|FullyQualifiedName~LineBreakRequirementTests"
  before: Failed: 26, Passed: 35, Total: 61 (all for the reason the test handover gives)
  after:  Failed: 2, Passed: 59, Total: 61 (the two disputed tests below)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests|FullyQualifiedName~RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests"
  Passed! Failed: 0, Passed: 55, Total: 55
scripts/test.sh --configuration Debug --project formatter
  Failed: 3, Passed: 4968, Total: 4971 (the three disputed tests below)
scripts/test.sh --configuration Debug --project analyzer      Passed! Failed: 0, Passed: 9186
scripts/test.sh --configuration Debug --project core          Passed! Failed: 0, Passed: 651
scripts/test.sh --configuration Debug --project cli           Passed! Failed: 0, Passed: 279
scripts/test.sh --configuration Debug --project architecture  Passed! Failed: 0, Passed: 118
scripts/test.sh --configuration Debug --project tooling       Passed! Failed: 0, Passed: 126
```

## Disputed tests or scope

### 1. The directive-held comma case is disabled text in the build the test parses

Two tests contain this case:
- `ListWrapByElementsTests.DirectiveHeldArgumentCommaStaysWhileArgumentsInFrontAreSplit`
- the `#if X` list in `ListWrapByElementsTests.SpecificationExampleHeldCommasStayAndCommentsMoveWithTheirComma`

The case comes from FMT-BREAK-012's third example and change doc entry 8:

```csharp
Foo(1, 2
#if X
    , 3
#endif
    );
```

**What the tests assert:** `Foo(1,⏎2⏎#if X⏎, 3⏎#endif⏎);`.

**Why that disagrees with the spec's own definition:** both tests parse without preprocessor symbols. With `X` undefined, `, 3` is disabled text in the leading trivia of `)`. The parsed list is then `Foo(1, 2 <directive trivia> )`: two arguments that start on one line and a line break only in front of the closing parenthesis. By FMT-BREAK-012's definition ("decided by its elements alone"), that list is single-line. A line break in front of the closing parenthesis of an argument list stays (Non-goals). So the specified output is the input unchanged. The comma the example calls "held" is not a separator of the parsed list at all.

**What the expected output would need:** reading the comma out of disabled text, or reasoning about the `X` build. That evaluates another build configuration, which `AGENTS.md`, *Special cases do not grow the source*, rules out. So I did not build it.

**What I checked:** with a temporary test, since deleted, I parsed the same input with `WithPreprocessorSymbols("X")`. The expected output above comes out exactly, under LF, CRLF, a second pass, and the phase stability check. Without `X`, the input comes out unchanged. The behavior the example describes is therefore delivered in the build where the comma exists.

**Suggested resolutions, for the lead to route:**
- The test implementer passes `CSharpParseOptions.Default.WithPreprocessorSymbols("X")` in both tests. Alternatively, the spec author changes entry 8 and the example to `#if !X`, or to an active `#if` branch.
- The spec author states in entry 8 and the example which build the comma is active in.

A cheap alternative also exists: a directive anywhere in the list interior would make the list multi-line. It is not in FMT-BREAK-012's definition, and it would also split `Foo(1, 2⏎#if X⏎#endif⏎)`. I did not implement it. It would be a spec change, and the case is rare.

### 2. An existing indexer test is stale under the changed FMT-BREAK-012

`ParameterContinuationLineAlignmentTests.IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter` has no `Requirement` attribute. It asserts that this input is already formatted:

```csharp
internal int this[int first, int second // keep
                  , int third] => first;
```

The changed FMT-BREAK-012 and change doc entry 7 (`O`, the parenthesized twin) require the elements on the held comma's line and in front of it to be split. The same shape is pinned by `ListWrapByElementsTests.CommentHeldIndexerParameterCommaStaysWhileFreeCommaMoves` and `CommentHeldParameterCommaStaysWhileParametersSharingALineAreSplit`, which now pass. The output after c119a8a is:

```csharp
internal int this[int first,
                  int second // keep
                  , int third] => first;
```

The test's own subject is the comma's alignment with the first parameter, and that alignment still holds in this output. The test implementer should update the expectation to this output, or to whatever the spec author confirms.

The two tests the test handover flagged (`CommaAloneOnItsLineStaysAligned`, `CommaAfterDirectiveHeldFirstParameterStaysAligned`; decision 14) pass unchanged.

## Architecture findings

_None._ Decision 9's opener signal is removed. The comma policy now has one owner across the four list visitors.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
