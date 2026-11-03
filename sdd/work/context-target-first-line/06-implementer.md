Role: implementer
Commit: beb3376
Verdict: tests pass

The prompt carried a maintainer-guidance note on proportionality and the requirement IDs to read; it carried no intended fix, suspected cause, or hoped-for verdict. The change follows the spec diff and the tests.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | Target-position fact: `TargetAnnotation` (annotation on the target's first token in the root) and `TargetFirstTokenColumn` (its document column). Both default to `null`, so every entry point but the context one is unchanged | FMT-PIPE-024 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNodeInDocumentWithContextAsync` passes the fact: the annotation it already put on the target's first token, and `ComputeDocumentColumn(firstToken, countsOwnWhitespaceTabsAsReplaced: false)` (each character one column, BOM zero). XML summary/remarks and two inline comments no longer describe the closed gap | FMT-PIPE-017, FMT-PIPE-018, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | `UseTargetFirstToken` / `TryGetTargetFirstToken` hold the marked token and its column | FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs` | `UseTargetPosition` finds the marked token in the root; `GetColumnBeforeRawStringShift` measures every token on the target's first line, from the target's first token on, as `fact column + distance to that token − raw-string shift of the line`, so its final column (`GetAdjustedColumn`) is the fact column plus the distance. Every contributor that aligns to an anchor on that line reads it through this method, so aligned lines follow; lines placed by depth are untouched (decision 3) | FMT-PIPE-017, FMT-PIPE-024 |

Whole-document formatting, `FormatSyntaxTree`, `FormatNode`, and `FormatNodeInDocumentAsync` never set the fact, so their code path is unchanged. Production diff: 4 files, +141/−13, almost all of it doc comments.

## Policy owners
- `rg -n "ComputeDocumentColumn|TargetFirstToken|TargetAnnotation|RootFirstTokenColumn" src --glob '!**/*.Test/**'`: the document column has one owner, `ReihitsuFormatterHelpers.ComputeDocumentColumn`, which is reused. The context's own offset stays with `LayoutComputer.ShiftToRootFirstTokenColumn`. The target's column is decided only in `LayoutComputer.TryGetTargetLineColumn`, and it reaches the alignments through the existing single measuring point `GetColumnBeforeRawStringShift`/`GetAdjustedColumn`. No private copy exists.
- `rg -n "LayoutComputer.GetColumn\(|\.GetColumn\(" src/Reihitsu.Formatter/Pipeline/Indentation`: there are no direct column reads of an anchor in the contributors, apart from `MethodChainAlignmentContributor:80`, which takes a distance within one line, and `PatternAnchor`, which goes through the model.

## Tests run
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests"`: Passed 44, Failed 0 (all 8 handed-over failures pass).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests|FullyQualifiedName~RH5112|FullyQualifiedName~RH5201"`: Passed 319, Failed 0 (all 24 handed-over failures pass, RH5112/RH5201 green).
- `scripts/test.sh --configuration Debug` (every project except self-hosting): Analyzer 6448, Formatter 4425, Core 607, Cli 251, Architecture 92, Tooling 125, Playground 17 passed, 0 failed.
- `scripts/format.sh` on the four changed files: formatted 0 of 4.

No test, assertion, input, or expected output was changed.

## Disputed tests or scope
- **The spec states a mechanism the code does not implement.** FMT-PIPE-024's bullets say that the line-break phase keeps the target's first token on its line, neither breaking nor joining in front of it, and that the documentation, line-break, and blank-line phases keep the target's leading trivia. FMT-PIPE-018's new paragraph says no phase adds or removes a blank line in that trivia, moves a comment or directive in it, or moves the first token to another line. The change does not touch those three phases. They still break, join, re-space, and relocate in front of the target within the root, as on `main`. None of that reaches the result. The entry point discards it, because it writes back the target's leading trivia from the document and nothing in front of the target. The indentation phase also measures the target's first line from the fact, wherever that line ended up in the root. So the result-level statements hold: FMT-PIPE-017's first paragraph about the context target, its "is not written back and decides no column" sentence, FMT-PIPE-018's first paragraph, and every test.
  - Implementing the stated mechanism would mean adding target guards to three more phases. Those guards would decide nothing visible. That cost runs against the maintainer's proportionality guidance.
  - Recommendation: the spec author restates those FMT-PIPE-024 bullets and the FMT-PIPE-018 "phases MUST keep that trivia" paragraph as the result, along these lines: the trivia and the code in front of the target are written back from the document, and the indentation phase lays out the target's other lines from the fact's column. The alternative is to route guard work back to me. This is the lead's call. No visible output differs between the two.
- **A new fact appears in no code-owner list.** The fact is two `FormattingContext` properties, not a new type, so FMT-PIPE's code-owner list needs no new entry.

## Architecture findings
_None._ The target's column is decided once, in the indentation phase, from a fact captured before the phases run. No phase predicts a later phase's column, and no step after the last phase moves a line.

## Follow-up candidates
- `GenericConstraintContributor` and `ConstructorInitializerContributor` place their lines relative to the placed column of a parent line (`model.TryGetPlacedColumn`), not to a token. If a context target's first line held such a parent, the `where` line would follow the context's line start, not the target's fact column. The only reachable shape is an RH3208 local function with a `where` clause behind a label and several spaces. I did not test it, and I have not shown that its output is wrong: the block of the same function follows the depth (RH3208-003), and a constraint line may legitimately follow it too. This is a candidate for the auditor to probe, not a confirmed defect.
