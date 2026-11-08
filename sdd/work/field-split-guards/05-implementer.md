Role: implementer
Commit: 1ac2927
Verdict: test disputed — #963: one test disputed (all other tests pass); #1384: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision 5 (`00-lead.md`), which is a decision of the run and was carried out.

Before the change, the focused formatter filter failed 50 cases (48 tests, data rows counted separately) and the focused analyzer filter failed 11, as `04-test-implementer.md` states.

## thoenissen/Reihitsu#963

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `Splits` gains `CarriesCommentInRepeatedPart`: walks the declaration's tokens from its first token up to the first declarator's first token and returns true for a comment in any of their trailing trivia, or in the leading trivia of any but the first token. The declaration's leading trivia and the first declarator's own leading trivia (own line in front of the first name, maintainer answer A) are outside the walk. `Splits` summary updated. | FMT-STRUCT-030, FMT-PIPE-031 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `SplitsFieldDeclaration` summary names the two new conditions; body unchanged (delegates). | FMT-PIPE-031 |

### Policy owners
- `rg -n "SplitsFieldDeclaration\(|FieldDeclarationSplitTransform.Splits|Splits\(fieldDeclaration\)" src --glob '!*Test*'` — the one owner is `FieldDeclarationSplitTransform.Splits`; callers are `SplitFields` (twice), `ReihitsuFormatter.SplitsFieldDeclaration` (delegates), `FormatNodeInDocumentWithContextAsync`, and the RH7101 fix.
- `rg -n "Variables.Count > 1" src --glob '!*Test*'` — only `Splits`.
- Comment test reused from `Reihitsu.Core.SyntaxTriviaUtilities.IsCommentTrivia` (includes documentation comment kinds, so `///` behind an attribute list blocks in every documentation mode).

### Tests run
See *Whole run*.

### Disputed tests or scope
- `RH7101DoNotCombineFieldsAnalyzerTests.FixAllSplitsOnlyTheDeclarationWithoutACommentInThePartEveryFieldRepeats` (RH7101-007, RH7101-004). It asserts that Fix All splits `_c, _d` and `_e, _f` and leaves `private int /* c */ _a, {|RH7101:_b|};` with its diagnostic — which is what RH7101-007 demands and what the code now produces. It fails with `Context: Diagnostics of fixed state … expected "0" actual "1"` at (3,29), the `_b` marked in the fixed source: the test calls `Verify(source, fixedSource)` without setting `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` to `MarkupMode.Allow`, so the `{|RH7101:_b|}` markup in the expected source is not read as an expected diagnostic. The repository's pattern for a remaining diagnostic in a fixed source sets both (e.g. `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests`, around line 1287). Established by adding exactly that `onConfigure` locally: the test then passes; the edit was reverted and is not committed. The asserted output is not in question; the test's harness configuration is. Route: test implementer.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1384

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `Splits` gains `IsDocumented`: a documentation comment trivia (`SyntaxTriviaUtilities.IsDocumentationCommentTrivia`) in the declaration's leading trivia refuses the split. The trivia kind carries the documentation mode: in `None` a `///`/`/** */` is an ordinary comment and does not refuse it. A `///` behind the previous member's code lands in this leading trivia too (Roslyn), so that form is covered. | FMT-STRUCT-030, FMT-PIPE-031 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7101DoNotCombineFieldsCodeFixProvider.cs` | Decision 5: `TryGetFixableFieldDeclaration` drops its private documentation-comment condition and asks only `ReihitsuFormatter.SplitsFieldDeclaration`; its summary now states that the formatter alone decides and names the kept-whole cases (directive, documentation comment, comment in the repeated part). The now unused `using Reihitsu.Core;` is removed. | FMT-PIPE-031, RH7101-005/006/007 |

### Policy owners
- `rg -n "IsDocumentationCommentTrivia" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Analyzer/Rules/Organization` — no remaining copy of the field-split documentation condition; the two hits (RH8201, RH7309 fixes) are unrelated decisions.
- The split's documentation guard has one owner, `FieldDeclarationSplitTransform.IsDocumented`, reached by the RH7101 fix only through `SplitsFieldDeclaration`.

### Tests run
See *Whole run*. The RH7101-006 tests (`DocumentedDeclarationsAreReportedWithoutAFix`, `…InDocumentationModeDiagnose`, `DocumentationCommentInFrontOfTheAttributesOfTheDeclarationWithholdsTheFix`, the mode-None fix tests) stay green after the private copy was removed, and both RH7103 regression tests pass.

### Disputed tests or scope
_None._

### Architecture findings
_None._ (The skeptic's FC-015 finding is resolved by decision 5 in this commit.)

### Follow-up candidates
_None._

## Whole run

Production paths: the two the handovers name (`Splits`, the RH7101 fix) plus one summary in `ReihitsuFormatter.SplitsFieldDeclaration`. No new public member or dependency.

Focused runs (Debug):

```shell
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterFieldSplitTests|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.StructuralTransforms.StructuralTransformSeparatorTests|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.StructuralTransformRequirementTests"
```
`Passed!  - Failed: 0, Passed: 244, Skipped: 0, Total: 244`

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter.Formatting.RH7101DoNotCombineFieldsFormatterTests"
```
`Failed!  - Failed: 1, Passed: 158, Skipped: 0, Total: 159` — the one is the disputed Fix All test.

Full projects (Debug):

- formatter: `Passed! - Failed: 0, Passed: 4778, Total: 4778`
- analyzer: `Failed! - Failed: 1, Passed: 7367, Total: 7368` — the disputed Fix All test only
- architecture: `Passed! - Failed: 0, Passed: 115, Total: 115`
- cli: `Passed! - Failed: 0, Passed: 251, Total: 251`

`scripts/build.sh --configuration Debug`: 0 errors. `scripts/format.sh` on the three changed files: formatted 0 of 3.
