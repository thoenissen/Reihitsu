Role: implementer
Commit: b64257f
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 3 in `00-lead.md`, which I carried out as the lead's recorded decision.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.cs` | `IsIndexerArgumentList` and `CanSafelyCollapseToSingleLine` are now `internal static`, so the code fix asks the analyzer's reporting condition (decision 3). Their doc comments say so. Behavior is unchanged | RH5307-001, RH5307-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider.cs` | `TryGetFormatTarget` now finds the list at the diagnostic's location: `root.FindNode(span, getInnermostNodeForTie: true)`, which is the element access (whose `ArgumentList` it takes) or the implicit access's bracketed list, and must have exactly the diagnostic's span. The old walk from the receiver's first token is removed. So are the fix's private copies of `IsIndexerArgumentList` and `CanSafelyCollapseToSingleLine`; the fix calls the analyzer's methods. `using Reihitsu.Core` is dropped | RH5307-003, RH5307-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs` | `CanCollapse` measures only the property's own code in the `FormatNode` result: from the first token behind the attribute lists through the `;`, without the trivia around it. `ApplyCodeFixAsync` passes a new formatting step, `FormatKeepingAttributeBlankLinesAsync`, to `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`. That step formats the property with `FormatNodeInDocumentAsync`, as before. Then, for each gap behind an attribute list, it writes back the blank lines the gap had as written (`TryKeepBlankLines`, `GetSlots`). A gap is split into slots at its comments, directives, and disabled text. Each slot gets its blank-line count as written. Only `EndOfLineTrivia` in the leading trivia of the token behind the gap is added or removed, plus whitespace that stood alone on a removed blank line. A gap whose slot count differs between the text as written and the formatted text keeps the formatter's blank lines. Original and formatted lists are matched by flat attribute index, so a list that FMT-STRUCT-028 splits maps to the formatted list that holds its last attribute | RH5401-003, RH5401-004 |

## Policy owners

- Which RH5307 list is reported, and whether it can be joined: `rg -n "IsIndexerArgumentList|CanSafelyCollapseToSingleLine" src --glob '!*Test*/**'` finds only the analyzer's declarations, the analyzer's own call, and the fix's call. The private copy in the fix is gone.
- RH5401's own-code measure: `CanCollapse` in the RH5401 fix is the only owner. `rg -n "IndexOf\('\\\\n'\)" src/Reihitsu.Analyzer.CodeFixes` shows the same one-line test in other fixes, but each one measures its own node.
- Blank lines behind an attribute list: `rg -n "BlankLineCount" src/Reihitsu.Analyzer.CodeFixes` finds only the new RH5401 code. `TokenGapAnalysis` (Reihitsu.Core) counts blank lines over a gap or a trivia range. It does not split a gap at comments into slots, and it gives no edit positions, so I did not reuse it. In the formatter, the blank-line phase (FMT-BLANK-011, `BlankLineCollapser`) owns these blank lines for document formatting. The RH5401 fix now deliberately states a different rule for its own output, as the maintainer decided (Q3, Q5). See *Architecture findings*.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 3 files changed, 264 insertions(+), 52 deletions(-). No new type. About 200 of the added lines, doc comments included, are the RH5401 blank-line write-back. The RH5307 change is net smaller than before.

## Deleted special cases

_N/A._ The run declares no shape unsupported.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests"
# Passed!  - Failed: 0, Passed: 55, Skipped: 0, Total: 55
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests"
# Passed!  - Failed: 0, Passed: 64, Skipped: 0, Total: 64
scripts/test.sh --configuration Debug --project analyzer
# Passed!  - Failed: 0, Passed: 11007, Skipped: 0, Total: 11007
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198
```

Before the tests, `scripts/format.sh` ran over the three changed files and reformatted one of them (the RH5401 fix). The Release build and its warnings are the cleaner's.

## Disputed tests or scope

- **Size of the RH5401 change.** The handovers name the RH5401 fix's guard as the change. The blank-line exception that RH5401-003 gained from the maintainer's Q3 and Q5 answers cannot come from the formatter. `FormatNodeInDocumentAsync` lays out the blank lines inside its target under FMT-BLANK-011 and collapses several into one, and no `FormattingContext` option turns that off for one gap. So the fix formats first and then writes the blank lines as written back into each gap behind an attribute list. That is about 150 code lines, no new type, inside the fix's own file. Only blank lines change, so no line's column moves. The lead decides whether this size is acceptable. The alternative is under *Architecture findings*.

## Architecture findings

- **RH5401 fix and the formatter's blank-line phase: one rule stated twice.** The formatter's blank-line phase owns the blank lines between a property's attribute lists and its own code (FMT-BLANK-011 and the collapse of several blank lines into one). The RH5401 fix now applies a second rule to the same gaps, its rule-local exception in RH5401-003: keep them as written. It does this by editing the formatter's output after the fact. The fix computes no column and predicts no layout. It reads both the text as written and the formatted text, and writes back only line breaks. Where the two layouts of a gap differ in anything but blank lines (a different number of comment, directive, or disabled-text slots), it keeps the formatter's result instead of guessing. The structural alternative is a formatter input, such as a context fact that tells the blank-line phase to keep the blank lines inside the target as written. The fix would then pass that instead of rewriting trivia. That alternative adds a formatter surface and a requirement in `sdd/specs/formatter/`, beyond this run's components. I made the narrow change. The inputs that show the two rules apply to the same gaps are the RH5401-003 examples `E`, `G`, `H`, `K` and the Q3 example `[Obsolete]⏎// note⏎int E⏎    => 1;`.

## Follow-up candidates

_None._
