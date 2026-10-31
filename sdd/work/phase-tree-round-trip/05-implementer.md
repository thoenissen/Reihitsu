Role: implementer
Commit: fd77b4c
Verdict: test disputed

Inputs: thoenissen/Reihitsu#1488, `00-lead.md` through `04-test-implementer.md`, `sdd/changes/2026-11/phase-tree-round-trip.md`, the spec diff against `origin/main`, and the decisions `squad/decisions/2026-10-31-phase-tree-round-trip-*.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Every new or changed test of the run passes except `CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput`. The formatter project has 57 failures. All of them are existing or new tests that assert behavior the spec diff forbids, or that the spec diff together with FMT-SPACE-002 cannot produce. See *Disputed tests or scope*. I changed no test.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | The braced statement keeps its own leading trivia from the line of its first comment on, and its trailing trivia unchanged (line break included). A trailing `//` without a line break (end of file) gets one. The inserted `{` gets a line break only when a comment leads the statement. The inserted `}` gets one only when a comment, directive, or disabled text starts the line of the next token. That token is read from the original tree. | FMT-STRUCT-026, FMT-PIPE-021, FMT-PIPE-013 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | `CreateReturnStatement` moves the expression's leading comments behind `return` with the author's whitespace, and drops whitespace-only indentation. It writes one space only when nothing separates `return` from the expression and the two would read as one token. Comment lines moved in front of the statement keep their indentation. | FMT-STRUCT-026, FMT-PIPE-021 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/TokenGapUtilities.cs`, `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | The private `WouldMergeWhenTight` moved, unchanged, from the list join into `TokenGapUtilities`, so the join and the `return` separator share one fusion policy. | FMT-STRUCT-026 (FC-015) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AccessorExpressionBodyTransform.cs`, `GetOnlyMemberExpressionBodyTransform.cs` | No space around the created `=>`. The accessor keyword's trailing layout trivia is dropped. | FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/TrailingCommaRemovalUtilities.cs`, `EnumTrailingCommaRemovalTransform.cs`, `InitializerTrailingCommaRemovalTransform.cs` | `PreserveSeparatorTrivia` replaces `GetTriviaToPreserve`/`PreserveTrailingTrivia` for callers. When a line break already ends the last element's line, the removed comma's trivia leads the closing brace. Otherwise it trails the last element, as before. | FMT-STRUCT-027, FMT-PIPE-021 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `MoveLines` edits the tokens that hold a moved line's indentation and no longer parses a new literal. It changes the text of the plain/UTF-8 literal token, the interpolated text token, or the end token. In a hole it changes the whitespace trivia, or rebuilds a `///` or disabled-text trivia whose inside a line starts in. Every other node and token is kept, and the edited token keeps its annotations. Parse options are no longer read. | FMT-PIPE-022, FMT-PIPE-017 (third case) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | `MoveTokenToNewLine` no longer trims the previous token's trailing whitespace, inside or outside the node. | FMT-BREAK-029 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs`, `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/BracePlacer.cs` | Member-body braces have one path, `DeclarationBraceLineBreakRewriter`. `VisitBlock` skips brace placement for the bodies of methods, constructors, destructors, operators, conversions, and local functions, and `VisitAccessorList` skips event accessor lists. `BracePlacer.NormalizeOwnedBraces` places the open brace with `NormalizeGapBeforeOwnedTokenPreservingPreviousTrivia`, so the whitespace in front of it stays. | member-body-braces decision (B), FMT-BREAK-029 |

`FieldDeclarationSplitTransform` and `EmptyTypeDeclarationSemicolonTransform` are unchanged. No public API, no dependency, and no new type. Code owners are unchanged.

## Policy owners

- Token fusion: `rg -n "WouldMergeWhenTight|IsIdentifierPartCharacter" src --glob '!*Test*'` finds one owner, `TokenGapUtilities.WouldMergeWhenTight`, with two callers (`ListElementInteriorJoinRewriter`, `ExpressionBodyToBlockConverter`). The unary-operator half stays in `Reihitsu.Core/UnaryOperatorSpacingUtilities`, which it calls.
- Member-body braces: `rg -n "NormalizeOwnedBraces|EnsureBraceOnOwnLine" src/Reihitsu.Formatter/Pipeline`. Member bodies and event accessor lists are now placed only through `NormalizeOwnedBraces` in `DeclarationBraceLineBreakRewriter`. Property and indexer accessor lists are still placed by both `LineBreakBlockRewriter.VisitAccessorList` and `AccessorListLayout` (see *Follow-up candidates*).
- Reparse inside the phases: `rg -n "ParseExpression|ParseLeadingTrivia|ParseToken" src/Reihitsu.Formatter/Pipeline` finds the documentation phase (two, out of FMT-PIPE-022's scope by the annotation-scope decision) and `RawStringLineShifts.MoveLineInsideTrivia` (a `///` trivia inside a hole, also structured trivia).
- Whitespace in front of a line break in the line-break phase: `rg -n "RemoveTrailingWhitespace\(" src/Reihitsu.Formatter/Pipeline/LineBreaks` finds 16 call sites (see *Disputed tests or scope*, item 3).

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter
# Failed!  - Failed:    57, Passed:  3269, Skipped:     0, Total:  3326   (baseline 1d8778f: Failed: 103)
scripts/test.sh --configuration Debug --project formatter --filter "TestCategory=FMT-PIPE-021|TestCategory=FMT-PIPE-022|TestCategory=FMT-STRUCT-026|TestCategory=FMT-STRUCT-027|TestCategory=FMT-BREAK-029|TestCategory=FMT-PIPE-017|TestCategory=FMT-PIPE-013"
# Failed!  - Failed:     1, Passed:    78   (CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput)
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~PhaseTreeRoundTripTests|FullyQualifiedName~PhaseAnnotationTests|FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~TrailingCommaCommentOwnerTests|FullyQualifiedName~LineBreakResidueWhitespaceTests|FullyQualifiedName~ControlFlowBraceTransformTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
# Failed!  - Failed:     1, Passed:    94   (the same test)
scripts/test.sh --configuration Debug --project analyzer      # Passed!  - Failed: 0, Passed: 6307
scripts/test.sh --configuration Debug --project architecture  # Passed!  - Failed: 0, Passed: 89
scripts/test.sh --configuration Debug --project cli           # Passed!  - Failed: 0, Passed: 251
scripts/test.sh --configuration Debug --project core          # Passed!  - Failed: 0, Passed: 535
scripts/build.sh --configuration Debug                        # 0 Error(s); no warning in a changed file
```

The reparse hook in `FormatterTestsBase.ApplyRule` no longer fails any fixture. None of the 57 failures is a FMT-PIPE-021 difference.

## Disputed tests or scope

### 1. Accessor arrow: FMT-STRUCT-026 plus FMT-SPACE-002 change the final output of 31 tests (escalation 4)

FMT-STRUCT-026 forbids the accessor transform to write a space around the `=>` it creates. `AccessorTransformWritesNoSpaceAroundTheCreatedArrow` asserts the gaps `""` on the phase output, and it passes. No later phase writes a space between the accessor keyword and `=>`. No spacing rule covers that pair (`horizontal-spacing.md` non-goals: the arrow is uncovered, thoenissen/Reihitsu#958), so FMT-SPACE-002 keeps the zero gap. The change doc says the final output does not change for these inputs, and `CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput` asserts that. Its expected value was taken from `scripts/format.sh` at 4c9e416 (`04-test-implementer.md`), before the transform stopped writing the space.

Input `string P{get{return"";}set{_s=value;}}` (`Regression/Structural/StructuralTransformSeparatorFinalOutputTests.cs:348`):

```
expected:  get => "";     set => _s = value;
actual:    get=> "";      set=> _s = value;
```

The get-only property `string Q => ""` keeps its output, because `PropertyLayoutLineBreakRewriter.CollapseExpressionBody` writes ` => ` for member-level arrows.

The same difference fails 30 existing final-output tests:

- `ReihitsuFormatterTests` (2): `FormatSyntaxTree…`/`FormatDocumentAsyncConvertsSingleStatementAccessorToExpressionBody`
- `ExpressionBodiedAccessorTests` (11)
- `ThrowOperandPrecedenceExpressionBodyTests` (9)
- `GetOnlyMemberExpressionBodyTests.BlockGetterPropertyWithInitializerKeepsAccessorList`
- `StructuralTransformRequirementTests.AccessorConversionIsAppliedOnlyWhereEachCommentStaysNextToItsCode`
- `EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests.EmptyClassStaysBracedBelowCSharp12AfterAccessorExpressionBodyTransform`
- `StraddlingRegionPairTests.RegionStraddlingAnAccessorListIsPreservedAsAPair`
- `AccessorListAccessorLineBreakTests` (2)
- `AccessorListDeclarationBraceTests.CommentBeforeAccessorListOpenBraceIsPreserved`
- `BlankLineBeforeIfDirectiveAfterStatementTests.TrailingDirectiveAfterConvertedAccessorKeepsItsBlankLine`

Every one shows `get=>`/`set=>`/`init=>` where `get =>` is expected.

The spec decides two things that cannot both hold under today's spacing rules: no space written by the transform, and an unchanged final output. Choosing between them (a spacing rule for the accessor arrow, a recorded final-output change, or a separator exception) is the maintainer's call. I wrote none of them.

### 2. Existing phase-level tests assert the fused text FMT-PIPE-021 forbids (26 tests)

These tests run the structural-transform phase (or a phase combination) and compare its text.

- **24 tests expect a fused `return`:** `int M(){return42;}`, `{// why\nreturna + b;}`, `returnnew C();`. The phase now writes `return 42`, `return a + b`, and `return new C()`, which FMT-STRUCT-026's first separator and FMT-PIPE-021's example require.
  - `Unit/StructuralTransforms/ExpressionBodiedTransformTests.cs` (14)
  - `Unit/StructuralTransforms/ExpressionBodiedLocalFunctionTransformTests.cs` (7)
  - `Integration/PhaseCombinationTests.StructuralTransformThenBlankLine`
  - `Integration/StructuralTransformPhaseIntegrationTests.ConvertsExpressionBodiedMethodToBlockBody`, `…LocalFunctionToBlockBody`
- **2 tests expect ` => ` from the get-only transform:** `ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty` and `ConvertsGetOnlyIndexerToExpressionBodiedIndexer` (`int Value => _value;`). The phase now writes `int Value=>_value;`, which the new `GetOnlyMemberTransformWritesNoSpaceAroundTheCreatedArrow` requires.

These tests assert the opposite of the new requirements. Per the test implementer's role they are theirs to change; the 24 `return` cases are the old form of the test implementer's own new tests.

### 3. FMT-BREAK-029's reach beyond the conditional trim (scope, escalation 5 if it is in)

I removed the trim the conditional-trim decision names (`MoveTokenToNewLine`), and the one the member-body path took on the open brace. The phase still removes whitespace in front of a line break it writes in these places. Each one rewrites the previous token's trailing trivia rather than leaving it in place, so whether FMT-BREAK-029 reaches it depends on how that phrase is read.

- `TokenGapNormalizer.NormalizeGapBeforeToken` and `NormalizeGapBeforeOwnedToken`: `RemoveTrailingWhitespace(RemoveTrailingEndOfLineTrivia(...))`. This also covers a previous token that had no line break, as in `M(); }`, and the `}` of a member body.
- `LineBreakInitializerRewriter.CleanupTrailingWhitespaceBeforeToken`.
- The list, chain, and property-layout rewriters, which move a line break out of the trivia (outside the requirement).

`TokenGapNormalizer` is a shared helper with about 25 call sites in the phase. Changing it is escalation 5, so I left it. The final output is unaffected either way, because FMT-CLEAN-001 removes the whitespace. The spec author or the lead decides whether FMT-BREAK-029 should say so.

## Architecture findings

**The space behind a created `=>` is decided by the transform's zero-width trivia and by the spacing phase.**

- `SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)` carries elastic markers, which are zero-width whitespace trivia.
- The spacing phase's collapse (`TrailingWhitespaceWriter.CollapseMultipleTrailingSpaces`, the uncovered-pair path of FMT-SPACE-002) turns the trailing marker into one space.
- So the phase's tree formats to `get=> ""`, while the same text read back from the file (`get=>"";`, the trace's next pass, `scripts/format.sh` on the phase output) stays `get=>""`. The tree and its text are formatted differently, which is the outcome FMT-PIPE-021's *Why* rules out. The structural comparison does not see it, because whitespace is not compared.
- Before this run, the transform wrote an explicit space there. Now the marker does the same job silently.
- Input: `string P{get{return"";}}` with a second accessor, as in disputed item 1. Hand-written `get=>"";` stays `get=>""` (traced).
- The same mechanism applies to every created token that keeps its default elastic trivia: `{`/`}` from `SyntaxFactory.Block`, and `return` when no separator is written.

I made no structural change.

## Follow-up candidates

- `TokenGapUtilities.WouldMergeWhenTight` counts `@` as merging. So `int Get()=>@x;` becomes `return @x`, although `return@x` reads as two tokens (checked by lexing). The policy is unchanged from the list join it came from. FMT-STRUCT-026's "no space where they read as two tokens" is not met for that one pair. The final output is the same, because the spacing phase writes the space after `return`.
- `ExpressionBodyToBlockConverter.ExtractSignificantTrivia` still strips the whitespace in front of the arrow's trailing comment (moved behind `{`) and in front of the semicolon's leading comment (moved in front of the new `;`). FMT-STRUCT-026's list of carried comments names neither, so I left them. The final output is unchanged.
- Nested unbraced `if`: the inner inserted `}` decides its line break from the token behind the inner statement in the original tree. When that token is led by a comment, the inner `}` also gets a line break, although the outer inserted `}` follows it. The phase output is still the same tree, and the final output is unaffected.
- A moved UTF-8 multi-line raw literal token now carries its value text as a string value instead of the byte array the parser gives. The text is the same, and no phase reads the value.
- Property and indexer accessor-list braces are still placed by `LineBreakBlockRewriter.VisitAccessorList` and again by `AccessorListLayout` through `PropertyLayoutLineBreakRewriter`/`DeclarationBraceLineBreakRewriter`. That is the same two-path shape the member-body decision removed for bodies, but it is outside that decision.
- The skeptic's two candidates in `02-skeptic.md` are untouched.
