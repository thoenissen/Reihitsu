Role: implementer
Commit: e615ba1
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, `02-skeptic.md`, `03-spec-author.md` to `06-test-implementer.md`, `sdd/changes/2026-11/region-directive-separator.md`, `git diff f8dc7ef..HEAD -- sdd/specs`.

On `e615ba1` the focused run failed 39 of 83 tests, the set the test-implementer handovers name. After the change all handed-over tests pass. Four older tests fail. They assert the trailing whitespace that FMT-REGION-007 now removes (see *Disputed tests or scope*).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/RegionFormatting/Utilities/RegionNamingRewriter.cs` | Every paired directive is now written in canonical form and replaced when its text differs. A described `#region` becomes keyword, one space, capitalized trimmed description, with or without a capitalization change. An undescribed `#region` becomes the bare keyword. Its `#endregion` becomes the bare keyword, or the keyword, one space, and the trimmed text as written. Inactive directives are covered as before, because the pairing already includes them. The new private helpers are `BuildUndescribedEndRegionTrivia` and `AddReplacementIfChanged`. `BuildRegionTrivia` now accepts an empty name. | FMT-REGION-001, -004, -007 |
| `src/Reihitsu.Formatter/Pipeline/RegionFormatting/RegionFormattingPhase.cs` | Returns the root unchanged when `context.FormatsRegionDirectives` is false. | FMT-PIPE-028 |
| `src/Reihitsu.Formatter/Data/FormattingContext.cs` | New plain flag `FormatsRegionDirectives`, default `true`. | FMT-PIPE-028, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync` pass `formatsRegionDirectives: false`. Their remarks now say that region directive text is kept. | FMT-PIPE-028 |
| `src/Reihitsu.Core/RegionDirectiveUtilities.cs` | New `GetEndRegionText`, which returns the trimmed text behind `endregion`. `GetEndRegionDescription` now calls it, and its behavior is unchanged. This gives the read one owner instead of a copy in the rewriter. | FMT-REGION-004 |

## Policy owners
- `rg -n "RegionKeyword|EndRegionKeyword" src`: the only writers of region directive text are `RegionNamingRewriter` and `RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia`. The latter is used by the rewriter and the RH7301 fix, and it is unchanged.
- `rg -n "CreateCanonicalEndRegionTrivia|PreprocessingMessage\(|WithRegionKeyword|WithEndRegionKeyword" src`: the RH7302 fix writes its own message text. It keeps the separator as written under RH7302-004, so it is unchanged and not a copy of FMT-REGION-007.
- The text behind `#endregion` was read only in `GetEndRegionDescription`. That read is now `GetEndRegionText`, reused by the rewriter.
- The node-level skip has one owner, the context flag, which `RegionFormattingPhase` reads. It sits beside `DisabledStructuralTransforms` (FMT-PIPE-011).

## Tests run
All runs below are on the working tree with the change, in Debug.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~RegionLineFormTests|FullyQualifiedName~ReihitsuFormatterNodeRegionTests|FullyQualifiedName~TabReplacementTests|FullyQualifiedName~ReihitsuFormatterTests.FormatNode|FullyQualifiedName~KeepsBlankLineAbovePropertyHoldingARegion|FullyQualifiedName~RegionFormatting"`: Failed 2, Passed 124. The two failures are the disputed tests.
- `scripts/test.sh --configuration Debug --project formatter`: Failed 2, Passed 4407, Total 4409. The two failures are disputed tests 1 and 2.
- `scripts/test.sh --configuration Debug --project analyzer`: Failed 2, Passed 6413, Total 6415. The two failures are disputed tests 3 and 4. The RH7103, RH7104, and RH7105 tests from `04-test-implementer.md` pass.
- `scripts/test.sh --configuration Debug --project core`: Passed 607.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 92.
- `scripts/test.sh --configuration Debug --project cli`: Passed 251. This was run before the `GetEndRegionText` extraction, which does not change behavior.

## Disputed tests or scope
Four existing tests assert that whole-document formatting keeps trailing whitespace behind a `#region` description already in the one-space form (`#region Fields···`):

1. `Reihitsu.Formatter.Test.Regression.Regions.RegionFormattingTests.TrailingOpeningWhitespaceWithCanonicalCloseIsStable` (`src/Reihitsu.Formatter.Test/Regression/Regions/RegionFormattingTests.cs:326`). It expects the input unchanged.
2. `Reihitsu.Formatter.Test.Regression.Regions.RegionFormattingTests.MismatchedCloseUsesNormalizedOpeningDescription` (`:347`). It expects `#region Fields···` to be kept while the close becomes `// Fields`.
3. `Reihitsu.Analyzer.Test.Formatter.Formatting.RH7301RegionsShouldMatchFormatterTests.VerifyFormatterPreservesTrailingOpeningWhitespaceWhenCloseIsCanonical` (`src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH7301RegionsShouldMatchFormatterTests.cs:55`). It runs the formatter over the document and expects the input unchanged.
4. `...RH7301RegionsShouldMatchFormatterTests.VerifyFormatterUsesNormalizedOpeningDescriptionForMismatchedClose` (`:78`). It expects `#region Fields···` to be kept.

FMT-REGION-007 says whitespace behind the description is removed, and its example turns `#region Baz···` into `#region Baz` (maintainer answer 5, option C). The test implementer's `TrailingWhitespaceBehindDescriptionAfterOneSpaceIsRemoved` asserts the same and passes. These four tests pin `main`'s output, which the change doc lists as changing, so their expected `#region Fields···` has to become `#region Fields`. Each test's name or summary also states the old behavior. Each still has a valid point, which is that the close is canonical, or is derived from the trimmed description. None of these tests appears in `04-test-implementer.md`'s list of changed expectations.

Scope: the change adds one public member to Core, `RegionDirectiveUtilities.GetEndRegionText`. It is a small extraction, and `GetEndRegionDescription` is its other consumer. No other production paths are touched beyond the four formatter files.

## Architecture findings
_None._ Only `RegionNamingRewriter` writes the region line form, and the node-level skip is a single context flag that the phase reads. The skeptic's follow-up 1 (a code fix writes half of a region pair) is resolved by the skip. `FormatNodeInDocumentAsyncKeepsBothHalvesOfRegionOpenedInFrontOfTopLevelType` and `RH7104…FixOfTopLevelPartialTypeKeepsRegionOpenedInFrontOfTheType` pass.

## Follow-up candidates
- `00-lead.md` follow-up 1 is absorbed (see above).
- Follow-ups 2 and 3 in `00-lead.md` are unchanged: trailing whitespace behind other directives, and whitespace between `#` and `region`.
