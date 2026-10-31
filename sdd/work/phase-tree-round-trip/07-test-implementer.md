Role: test implementer
Commit: 3cfdde8
Verdict: tests written (repair round: disputed tests re-derived, amendment tests added)

Inputs:
- the follow-up routed by the lead;
- `05-implementer.md` (*Disputed tests or scope*) and `06-spec-author.md`;
- FMT-STRUCT-026 and FMT-BREAK-029 as amended at c4d4b21, and the amended change doc;
- the decisions `squad/decisions/2026-10-31-phase-tree-round-trip-{accessor-arrow,elastic-trivia,break-residue-reach}.md`.

No intended fix or hoped-for verdict was in my prompt to ignore. I changed no production code.

## Disputed tests (implementer's item 2)

All 26 were re-derived from FMT-PIPE-021 and FMT-STRUCT-026 as amended.

- **24 tests expected a fused `return`.** FMT-STRUCT-026's first separator requires one space where `return` and the next token would read as one, and FMT-PIPE-021 forbids text that parses to another tree. In each expected phase output I inserted that space and changed nothing else: `return42` became `return 42`, `returna + b` became `return a + b`, and so on. The tests are:
  - `ExpressionBodiedTransformTests` (14)
  - `ExpressionBodiedLocalFunctionTransformTests` (7)
  - `PhaseCombinationTests.StructuralTransformThenBlankLine`
  - `StructuralTransformPhaseIntegrationTests.ConvertsExpressionBodiedMethodToBlockBody`
  - `StructuralTransformPhaseIntegrationTests.ConvertsExpressionBodiedLocalFunctionToBlockBody`

  The rest of each expected text is still the test's own earlier expectation. For example, the expression's indentation is dropped, which FMT-STRUCT-026 allows ("MAY drop ... the whitespace in front of an expression that has no comment in front of it"). All 24 pass at 3cfdde8.
- **2 get-only tests expected ` => `:** `ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty` and `ConvertsGetOnlyIndexerToExpressionBodiedIndexer`. The amended FMT-STRUCT-026 has the get-only transform write one space on each side of the created `=>` (`int P => _value;`). Their expected values already say that, so I left them unchanged and tagged both `FMT-STRUCT-026`. Both fail at 3cfdde8 because the phase writes `Value=>_value` and `this[int i]=>_data[i]`.

## Accessor final-output tests (implementer's item 1)

Confirmed. The amended FMT-STRUCT-026 writes one space on each side of a created `=>`, and FMT-STRUCT-009 and FMT-STRUCT-014 show `get => …`. That is what the 31 tests expect, so none of them needed a change.

- At 3cfdde8 all 31 fail on exactly that gap. Every message shows `get=>`, `set=>`, or `init=>` where `get =>` is expected, and nothing else differs.
- I checked every failure message. The only exceptions are the tests listed below for other reasons.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `StructuralTransformSeparatorTests.AccessorTransformWritesOneSpaceOnEachSideOfTheCreatedArrow` (replaces `…WritesNoSpaceAroundTheCreatedArrow`) | FMT-STRUCT-026 | phase output, gap | fail (gaps `""`) |
| `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheCreatedArrow` (replaces `…WritesNoSpaceAroundTheCreatedArrow`) | FMT-STRUCT-026 | phase output, gap | fail (gaps `""`) |
| `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheArrowBehindAnIndexer` | FMT-STRUCT-026 | phase output, gap (indexer `]` before `=>`) | fail (gaps `""`) |
| `StructuralTransformSeparatorTests.TokensCreatedForConvertedAccessorsCarryNoElasticTrivia` | FMT-STRUCT-026 | phase output, `SyntaxAnnotation.ElasticAnnotation` on any trivia | fail (`=>` both sides, created `;` leading) |
| `StructuralTransformSeparatorTests.TokensCreatedForBracedBodiesCarryNoElasticTrivia` | FMT-STRUCT-026 | same | fail (inserted `{`/`}`) |
| `StructuralTransformSeparatorTests.TokensCreatedForConvertedExpressionBodiesCarryNoElasticTrivia` | FMT-STRUCT-026 | same | fail (`return` leading, created `;` trailing, created `}` leading) |
| `StructuralTransformSeparatorFinalOutputTests.HandWrittenArrowKeepsItsGapsWhileAConvertedAccessorGetsSpacesAroundItsArrow` | FMT-STRUCT-026 | `AssertRuleResult` | fail (`set=> _s = value;`) |
| `LineBreakResidueWhitespaceTests.FinalOutputHoldsNoWhitespaceInFrontOfANormalizedBraceGap` | FMT-BREAK-029 | `AssertRuleResult` | pass |
| `ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty` (tag added) | FMT-STRUCT-026 | `ApplyPhase` | fail (`Value=>_value`) |
| `ExpressionBodiedTransformTests.ConvertsGetOnlyIndexerToExpressionBodiedIndexer` (tag added) | FMT-STRUCT-026 | `ApplyPhase` | fail (`]=>_data[i]`) |
| 24 re-derived `return` tests listed above | none (untagged, as before) | `ApplyPhase` / phase combination | pass |

How the new tests were derived:

- **Elastic trivia.** The tests check the whole structural-phase output. The parsed input carries no elastic trivia, so any elastic trivia in the output stands on a token the phase created or carried over. Either way FMT-STRUCT-026 forbids it ("its trivia is exactly what this requirement allows"). They cover the three shapes the spec author named (converted accessor, braced `if` body, converted expression body), with and without comments.
- **Hand-written arrow.** The final-output test is the example in FMT-STRUCT-026's Why. Its run confirms the premise: the hand-written `get=>"";` already stays as written today, and only the converted setter differs.
- **FMT-BREAK-029's reach.** The amended text is a permission, so only its stated consequence can be tested: with whitespace in front of a `}` the phase places behind `M();`, the final output holds none. As the spec author says, nothing is asserted on the phase output for that case.

## Failing at 3cfdde8, with the reason

The formatter project has 3332 tests and 40 failures:

- **31 accessor final-output tests:** the `=>` behind an accessor keyword has no space. The tests are:
  - `ExpressionBodiedAccessorTests` (11)
  - `ThrowOperandPrecedenceExpressionBodyTests` (9)
  - `ReihitsuFormatterTests` (2)
  - `AccessorListAccessorLineBreakTests` (2)
  - one each in `GetOnlyMemberExpressionBodyTests`, `StructuralTransformRequirementTests`, `EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests`, `StraddlingRegionPairTests`, `AccessorListDeclarationBraceTests`, `BlankLineBeforeIfDirectiveAfterStatementTests`
  - `StructuralTransformSeparatorFinalOutputTests.CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput`
- **2 get-only phase tests:** no space around the created `=>`.
- **3 created-arrow phase tests:** gaps `""` instead of `" "`.
- **3 elastic-trivia phase tests:** the created tokens named in the table carry elastic markers.
- **1 hand-written-versus-converted final-output test:** `set=> _s = value;`.

`scripts/test.sh --project architecture`: 89/89 pass.

## Not covered, and why

- **Elastic trivia written by later phases.** The amended FMT-STRUCT-026 states that it does not decide this, and the elastic-trivia decision makes it a follow-up.
- **The phase-level side of FMT-BREAK-029's reach** (`TokenGapNormalizer`, `LineBreakInitializerRewriter`). The requirement permits either outcome there; the spec author's handover says no test is needed.
- **The field split's elastic trivia.** The field split lies outside FMT-STRUCT-026 as a whole.

## Commands

```shell
scripts/test.sh --project formatter
scripts/test.sh --project formatter --filter "FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~ExpressionBodiedTransformTests|FullyQualifiedName~ExpressionBodiedLocalFunctionTransformTests|FullyQualifiedName~LineBreakResidueWhitespaceTests"
scripts/test.sh --project formatter --filter "TestCategory=FMT-STRUCT-026|TestCategory=FMT-BREAK-029"
scripts/test.sh --project architecture
```
