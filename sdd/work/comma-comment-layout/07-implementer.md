Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/comma-comment-layout/07-implementer.md`)
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointers to the handovers and specs. All of the handed-over tests pass except one, `RH5107…VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt`. Its assertion is right, but its test configuration is missing a setting (see *Disputed tests or scope*).

## thoenissen/Reihitsu#1737

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.cs` | `RegisterCodeFixesCoreAsync` registers no fix when the reported comma's trailing trivia holds a comment (`token.TrailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)`). That covers `, /* c */ int b`, `, /* c */`, `, // c`, several comments, and a multi-line comment that starts on the comma's line. A comment that starts a later line is leading trivia of the next parameter, so that input keeps its fix. The class summary was updated. Fix All uses the batch fixer over the registered actions, so a commented comma's diagnostic stays. | RH5107-004 |

## thoenissen/Reihitsu#1738

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | New home of `CollapseSeparatorsToPreviousElementLine`, moved here from `LineBreakListRewriter` (private static there) so that the initializer rewriter reuses the one owner. It now also handles a trailing comma behind the last element, where the token behind the comma is the closing delimiter (`separator.GetNextToken()`). The comment-guard refusal was deleted (see *Whole run*). | FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-038 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | Calls the collapse after each child visit. The calls cover initializer expressions of every kind, array, complex element, object, and collection initializers included (before the array early return); anonymous object creations; collection expressions; list patterns; and a recursive pattern's property pattern clause. The class summary was updated. | FMT-BREAK-038, FMT-BREAK-021, FMT-BREAK-022 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | The four visitors (argument, attribute argument, parameter, and bracketed parameter list) call the moved helper. | FMT-BREAK-011, FMT-BREAK-012 |

## Whole run

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New `ListSeparatorCommentReason` and `FindTriviaInFrontOfComma`, plus `EnumerateGapTrivia`. The check covers every `CommaToken` among the node's descendant tokens whose previous token with text lies inside the node. The gap is that token's trailing trivia, the trivia of any zero-width tokens in between (the omitted type argument), and the comma's leading trivia. The input is refused when the gap holds a comment (`IsCommentTrivia`, documentation comments included), a directive, or disabled text, and any trivia text in the gap holds `\n` or `\r`. That is a text-line test, so the line break inside a multi-line comment counts. The first such trivia is reported. `FindUnsupportedSyntax` now picks the first of three shapes. On a tie at one comment, FMT-UNSUP-004 wins for a declarator comma (`comma.Parent is VariableDeclarationSyntax`) and FMT-UNSUP-009 wins for every other comma. | FMT-UNSUP-009, FMT-UNSUP-001, FMT-UNSUP-003, FMT-UNSUP-004, FMT-UNSUP-007 |
| `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` (moved) | Deleted the `WouldJoinAcrossUnjoinableTrivia(previousToken, separator)` refusal and its documentation sentence. | FMT-UNSUP-009, FMT-BREAK-012 |
| `LineBreakListRewriter.EnsureSeparatorsHaveEndOfLine` | Deleted the `HasLeadingEndOfLine(separator)` operand and the documentation sentences about a held separator. | FMT-UNSUP-009, FMT-BREAK-012 |
| `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` | Deleted the `AreSeparatedByEndOfLine(constraint.GetLastToken(), separator)` operand and the held-comma documentation sentence. | FMT-UNSUP-009, FMT-BREAK-037 |
| `GenericConstraintContributor.AlignConstraints` | Deleted the `ListElementAligner.AlignSeparators(constraint.Constraints, …)` call. The summaries no longer mention comma lines. | FMT-UNSUP-009, FMT-INDENT-032 |
| `TrailingCommaRemovalUtilities` | Deleted `GetTriviaToPreserve`'s branch for the comma's leading trivia, together with `ContainsNonFormattingTrivia`. `PreserveSeparatorTrivia` now carries only `separator.TrailingTrivia`. `GetTriviaToPreserve` reduced to that one expression and was inlined, which also removed the unused `Microsoft.CodeAnalysis.CSharp` using. The summary was updated. | FMT-UNSUP-009, FMT-STRUCT-027 |

## Policy owners
- Moving a comma that starts a line behind its element: `rg -n "CollapseSeparatorsToPreviousElementLine|HasLineBreakBetween\([^)]*, *(separator|comma)" src --glob '*.cs' --glob '!*Test*/**'`. It finds one owner, `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine`, called from the four `LineBreakListRewriter` visitors and the five `LineBreakInitializerRewriter` call sites. RH5107's code fix is a separate text edit outside the formatter; it is already a lead follow-up candidate ("RH5107 fix delegates its layout to the formatter").
- Comma alignment: `rg -n "AlignSeparators\(" src --glob '*.cs' --glob '!*Test*/**'` finds `ListElementAligner.AlignToFirstElement` and `ArgumentAlignmentContributor` (two calls). All three are kept, as the change doc lists under *Checked and kept*. The constraint call was deleted.
- Unsupported shapes: `PrecheckPhase.FindUnsupportedSyntax` is the only statement of them, and `ReihitsuFormatter.FindUnsupportedSyntax` asks it (FMT-UNSUP-007).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 8 files changed, 217 insertions(+), 120 deletions(-). This run added no new type. It added one new non-private member: `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine`, which is internal through the internal class and was moved from a private member.

## Deleted special cases
Every entry under *Special cases → Deleted* in `sdd/changes/2026-11/comma-behind-comment-unsupported.md` was deleted:
- `CollapseSeparatorsToPreviousElementLine`'s `WouldJoinAcrossUnjoinableTrivia` refusal and its documentation sentence. The method now lives in `LineBreakTriviaUtilities`.
- `EnsureSeparatorsHaveEndOfLine`'s `HasLeadingEndOfLine(separator)` operand and its documentation sentences.
- `PlaceOneConstraintPerLine`'s `AreSeparatedByEndOfLine(constraint last token, separator)` operand and its documentation sentence.
- `GenericConstraintContributor`'s `AlignSeparators(constraint.Constraints, …)` call.
- `TrailingCommaRemovalUtilities.GetTriviaToPreserve`'s leading-trivia branch and `ContainsNonFormattingTrivia`.
- Never written: the held-comma line rule for initializers and FMT-INDENT-033's separator alignment. No separator alignment was added to `ObjectInitializerContributor`, `AnonymousObjectContributor`, `CollectionExpressionContributor`, `ListPatternContributor`, or `RecursivePatternContributor`.

Every entry under *Checked and kept* was left unchanged. I found no further special case, and I kept nothing that the doc lists as deleted. No throw site was added.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter`: Passed, 0 failed, 5097 passed.
- `scripts/test.sh --configuration Debug --project analyzer`: 1 failed, 9527 passed. The failure is `VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt` (disputed below).
- `scripts/test.sh --configuration Debug --project cli`: Passed, 281.
- `scripts/test.sh --configuration Debug --project architecture`: Passed, 118.
- `scripts/test.sh --configuration Debug --project core`: Passed, 682. `--project tooling`: Passed, 126.
- `scripts/format.sh` was run on the eight changed files; it reformatted `PrecheckPhase.cs` (collection initializer trailing commas). All projects were re-run after that.

## Disputed tests or scope
- **`RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests.VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt`** (RH5107-004).
  - What it asserts: Fix All moves `, int b` and leaves `{|RH5107:,|} /* c */ int c` with its diagnostic. That is what RH5107-004 requires.
  - Why it fails: the failure is `Diagnostics of fixed state: expected "0" actual "1"`, with the remaining diagnostic at (5,12). The test passes explicit `expected` diagnostics to `Verify` and does not set `FixedState.MarkupHandling` or `BatchFixedState.MarkupHandling` to `MarkupMode.Allow`. Without that setting the `{|RH5107:,|}` markup in the fixed source is not counted as an expected diagnostic. The repository's other tests with a withheld diagnostic in the fixed source set both, for example `RH5030…AnalyzerTests` around line 1343.
  - Evidence: as a local probe I added `test => { test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow; }` to the `Verify` call. The test then passed against this code. I reverted the probe, and the test file is unchanged.
  - Routing: back to the test implementer for that configuration. No assertion or expected text changes.
- **Precheck tie rule** (decided by the tests, not stated in the spec). For a comment in front of a non-declarator comma inside a variable declaration, the tests expect the FMT-UNSUP-009 reason: the tuple-type row `(int A // c⏎, int B) t = default;` and the function-pointer field row `delegate*<int // c⏎, void> _f;`. That comment also lies in FMT-UNSUP-004's span. FMT-UNSUP-009 assigns FMT-UNSUP-004's reason only to commas between declarators. FMT-UNSUP-001 ("the shape that starts first") does not decide a tie at one comment. I implemented what the tests assert. The spec author may want FMT-UNSUP-009's paragraph on declarator commas to say that every other comma inside a declaration reports FMT-UNSUP-009's reason.
- **Shared helper (scope)**: `CollapseSeparatorsToPreviousElementLine` moved from `LineBreakListRewriter` (private) to `LineBreakTriviaUtilities` (internal static), so the initializer rewriter reuses the one owner instead of a copy. It now has two consumers.

## Architecture findings
_None._ RH5107's fix and the line-break phase both placing a comma is already recorded in `00-lead.md` (decision 6, follow-up).

## Follow-up candidates
_None._
