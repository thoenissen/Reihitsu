Role: implementer
Commit: 9ee01ab
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every handed-over test passes except two, both disputed below. Neither failure comes from the production change. One expects a line ending the source does not carry. The other expects one Fix All iteration, and the same layout without semicolons already needs two on `main`'s code path.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.cs` | `ShouldReport`: the "has a semicolon → not reported" check is replaced by "has no open brace token → not reported". This still excludes `class C;`, where `OpenBraceToken` is `default`, kind `None`, not missing. A brace body with `;` behind it is now reported. `HasMeaningfulBodyTrivia`: when a `;` follows the body, a non-whitespace, non-line-break trivia in the close brace's trailing trivia or the semicolon's leading trivia blocks the conversion. Summary updated. | SHARED-EMPTY-001, SHARED-EMPTY-004, SHARED-EMPTY-005, FMT-STRUCT-018 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs` | `CreateSemicolonToken` takes the declaration. Its new `;` keeps the trailing trivia of the existing `;` when one follows the braces, otherwise the trailing trivia of the close brace as before. `WithSemicolonToken` replaces the old `;`, so the whitespace and line breaks between `}` and `;` go with the braces. Summaries updated. | FMT-STRUCT-017 |

The code fixes needed no change. They call `CanConvertSafely` and format the declaration, which spans the `;`, through `FormatMemberOnLinesOfItsOwnAsync`, so the transform above writes their result.

## Policy owners
- `rg -n "EmptyTypeDeclarationSemicolonAnalysisUtilities\." src`: the analyzer base, the code-fix base, and the five `Visit*Declaration` methods of the transform. All of them go through `ShouldReport`/`CanConvertSafely`, the single owner.
- `rg -n "SemicolonToken.IsKind\(SyntaxKind.SemicolonToken\)" src/Reihitsu.Core src/Reihitsu.Formatter/Pipeline/StructuralTransforms`: only the two new uses.
- The RH5404 analyzer has its own has-semicolon check (`RH5404ElementMustNotBeOnSingleLineAnalyzer.cs:52`). It belongs to follow-up candidate 1 in `00-lead.md` and is not touched.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 23 insertions(+), 14 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5412|…RH5413|…RH5414|…RH5415|…RH5416|…RH5418|…RH5404"`: Failed 2, Passed 179. The two failures are the disputed tests below.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~EmptyTypeDeclaration|FullyQualifiedName~StructuralTransformRequirementTests|FullyQualifiedName~UnionDeclarationStructuralTests"`: Passed 75.
- Full projects, Debug:
  - analyzer: Failed 2, Passed 11939 (the same two)
  - core: Passed 887
  - formatter: Passed 7302
  - architecture: Passed 198
  - cli: Passed 441, Skipped 1

## Disputed tests or scope

1. **RH5404 `VerifyNestedEmptyTypeFollowedByASemicolonConvergesToSemicolonDeclaration`, data row CRLF.**
   - What it asserts: the input `class {|RH5404:H|} { class I { }; }` holds no line break, yet with the CRLF row the expected output is written with CRLF.
   - What the spec says: FMT-PIPE (`sdd/specs/formatter/pipeline.md:262`) falls back to `Environment.NewLine` when the input holds no line break. On Linux that is LF. The actual output is `class H⏎{⏎    class I;⏎}` with LF, exactly the expected text in LF. The LF row passes.
   - Why they disagree: the test can pass only on Windows. The CRLF row adds no coverage for this input; it would need a line break in the source, such as a second line. Route to the test implementer.

2. **RH5412 `VerifyTwoEmptyClassesWithSemicolonsOnOneLineAreFixed`.**
   - What it asserts: `class A { }; class B { };` → `class A;⏎class B;`. The iterative fix passes. Fix All in document fails with "Expected '1' iterations but found '2' iterations".
   - Where the second iteration comes from: the same layout without semicolons needs two iterations too. I added a throwaway probe test, `class {|#0:A|} { } class {|#1:B|} { }` → `class A;⏎class B;` with the default `Verify`, and it failed the same way. The probe is removed and not committed. The no-semicolon path runs no code this change touches. Both fixes break the same gap between `A` and `B` through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (SHARED-FIX-007), and the batch fixer drops one of the overlapping text changes. So the iteration count is a property of the shared-line helper under `BatchFixer`, not of SHARED-EMPTY-005.
   - Options for the lead:
     - (a) The test implementer sets the Fix All iteration count to 2 for this test. The result text is already right.
     - (b) The test implementer moves `class B` to its own line, so the test pins SHARED-EMPTY-005 without the shared-line interaction.
     - (c) Treat the two-iteration Fix All on a shared line as a follow-up candidate (below). Making it converge in one iteration would mean changing how the shared-line break is written, which is outside this run.

## Architecture findings
_None._

## Follow-up candidates
- Fix All of RH5412 to RH5418 needs two iterations when two reported declarations share one line, with or without a `;` behind them. Both fixes break the same gap via `FormatMemberOnLinesOfItsOwnAsync`, and `BatchFixer` drops one of the two overlapping edits. Seen with the probe described above. Size unknown: it concerns every caller of that helper.
