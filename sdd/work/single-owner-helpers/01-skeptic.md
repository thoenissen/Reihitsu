Role: skeptic
Commit: 33d3ce8
Verdict: #1956: proceed; #1958: proceed; #1963: proceed wider (two more sibling copies, see the issue's section)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' prescribed solutions (`Reihitsu.Core` as owner, "route through the one helper") are proposals and are checked below.

## thoenissen/Reihitsu#1956

### Premise

Real. The predicate is stated twice with identical bodies (eleven arms each, same order, same `CollectionExpressionSyntax` arm `token != collection.OpenBracketToken`, same `_ => SyntaxIndentationUtilities.IsIndentingScope(node)` default):

- `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs:341` — `internal static bool IsOwningConstruct(SyntaxNode, SyntaxToken)`, whose summary (line 333-336) says it "must stay in step with" the formatter's copy.
- `src/Reihitsu.Formatter/Pipeline/Core/Utilities/ListElementInteriorUtilities.cs:95` — `private static bool IsOwningConstruct(SyntaxNode, SyntaxToken)`.

`rg -U "AnonymousObjectCreationExpressionSyntax\s*\n\s*or SwitchExpressionSyntax" src` finds only these two; no third copy.

Callers reach both copies:

- Analyzer: `CommentPositionUtilities.IsOwnedByNestedConstruct` ← `CommentRegionAnalyzerBase:79`, the base of RH5035, RH5038, RH5039, RH5043, RH5044, RH5046, RH5047 (analyzer run on any document).
- Formatter: `FindOwningConstruct` ← `ListElementInteriorJoinRewriter:202` (line-break phase) and `ListElementAligner:108` (indentation phase), both reached by whole-document formatting and the CLI; `LiesInListElement` ← `ReihitsuFormatterHelpers:124` (node-level entry point used by code fixes).

The issue's owner proposal holds: `Reihitsu.Analyzer.csproj` and `Reihitsu.Formatter.csproj` both reference `Reihitsu.Core` and not each other (the analyzer has no formatter reference), and `Reihitsu.Core` ships in the package (`Reihitsu.Analyzer.Package.csproj:47`). `Reihitsu.Core` has `InternalsVisibleTo` only for `Reihitsu.Core.Test`, so the shared predicate must be `public`, like the other Core utilities. `SyntaxIndentationUtilities.IsIndentingScope` is already `public` in `Reihitsu.Core` (`src/Reihitsu.Core/SyntaxIndentationUtilities.cs:134`); the issue's "must be reachable from it, or move with it" is already satisfied and nothing has to move with it.

The intended behavior is SHARED-COMMENT-006 (`sdd/specs/shared/comment-placement.md`), whose construct list matches the predicate's arms (brace bodies and accessor list via `IsIndentingScope`, initializer, list and parenthesized pattern, interpolation, collection expression after its own `[`, query body, anonymous object, switch expression, recursive pattern). No spec changes; the run moves code only.

Baseline on `33d3ce8` (`scripts/build.sh`: 0 warnings, 0 errors):

```
scripts/test.sh --project analyzer --filter "FullyQualifiedName~CommentPositionUtilitiesTests|FullyQualifiedName~RH7004UsingDeclarations|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.NoCodeFixWhenACrossedMethodBodyHoldsAGroupWithAnElifBranch"
Passed! - Failed: 0, Passed: 126
```

### Mechanism and candidates

Mechanism: one decision (does a node on the path from a token to its region or list element lay out that token's line itself) is stated in two assemblies, so a change to one copy does not reach the other (FC-015). The candidate set is the predicate's switch arms: `CollectionExpressionSyntax` (two members: the token is its `[`, or any other token), `InitializerExpressionSyntax`, `AnonymousObjectCreationExpressionSyntax`, `SwitchExpressionSyntax`, `RecursivePatternSyntax`, `ListPatternSyntax`, `ParenthesizedPatternSyntax`, `AccessorListSyntax`, `InterpolationSyntax`, `QueryBodySyntax`, and the default arm (`IsIndentingScope`: namespace, type, block, switch statement, accessor list, non-empty switch section, per `GetIndentingScopeRange`).

### Breaking inputs

FC-015 (the issue's own class), FC-031 (stale summaries; see below). No input changes output; the move is correct when every arm keeps its result. Existing coverage per arm:

| Arm | Pinned by |
|---|---|
| anonymous object, initializer, switch expression, recursive pattern, parenthesized pattern, list pattern, collection (not `[`), collection (`[`), block via default arm, query body, interpolation, and the non-owning cases (lambda expression body, `from … in`, binary operator, chain link) | `CommentPositionUtilitiesTests.IsOwnedByNestedConstructMatchesFormatterOwnership` (15 rows; asserts the analyzer's and the formatter's answer agree) and `IsOwnedByNestedConstructReturnsFalseBeforeElement` |
| `IsIndentingScope` itself | `SyntaxIndentationUtilitiesTests.IsIndentingScopeRecognizesBraceScopesOnly`, `…NonEmptySwitchSection`, `…ReturnsFalseForEmptySwitchSection` |
| `AccessorListSyntax` explicit arm | none found. No caller reaches it as a nested node: an accessor list cannot occur inside a list element, a statement header, a base list, or an attribute list, and RH5047's region is the accessor list itself, which the walk in `IsOwnedByNestedConstruct` stops at. It is also covered by the default arm (`IsIndentingScope(accessorList)` is `true`, pinned above). No test is needed for it (`AGENTS.md`, *Supported inputs*) |

The parity test stays meaningful after the move: it still compares two different walks (`IsOwnedByNestedConstruct`, with its element-boundary rule, against `FindOwningConstruct`).

Documentation facts the implementer and auditor need (FC-031):

- The analyzer summary at `CommentPositionUtilities.cs:333-336` names the formatter's private copy and says it must stay in step; it becomes false with the move (the issue's Done-when item).
- `IsOwnedByNestedConstruct`'s summary has `<see cref="IsOwningConstruct"/>` (`CommentPositionUtilities.cs:294`), which must point at the new owner or the cref breaks.
- The construct lists in the summaries of both copies and of `FindOwningConstruct` (`ListElementInteriorUtilities.cs:66-69`) omit the accessor list, which the body includes. The prose lists at `ListElementInteriorJoinRewriter.cs:36` and `ListElementAligner.cs:84` name `FindOwningConstruct`, which no issue removes.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Core` | Must change: gains the predicate (public) |
| `Reihitsu.Analyzer` (`CommentPositionUtilities`) | Must change: calls the Core predicate; private copy and "stay in step" note deleted |
| `Reihitsu.Formatter` (`ListElementInteriorUtilities`) | Must change: calls the Core predicate; private copy deleted |
| Diagnostics of RH5035, RH5038, RH5039, RH5043, RH5044, RH5046, RH5047 | Must stay |
| Formatter output (list-element interior joins, list-element alignment, node-level `LiesInListElement` fact) | Must stay |
| Code fixes, Fix All | Not involved (reach the formatter only through `ReihitsuFormatter`; output must stay) |
| Specs (SHARED-COMMENT-006, FMT-BREAK, FMT-INDENT) and their code-owner lists | Not involved: no listed type is removed |
| Rule docs | Not involved |
| Tests | Existing tests must pass unchanged |

### Refuse option

No input class is at stake, so neither withholding a fix nor leaving a construct as written applies. Declaring a shape unsupported is not an option: there is no shape and no special case. The refuse option is "no change": the two copies stay, guarded only by the parity test, which covers list-element gaps and not the analyzer's other regions. The change costs about one public method in Core and two deletions; it adds no type if the predicate goes into an existing Core class such as `SyntaxIndentationUtilities` (next to `IsIndentingScope`); a new Core class would be a new type and so escalation 9. Recommend making the change.

### Output questions

_None._

### Internal choices

1. Which Core class owns the predicate. `SyntaxIndentationUtilities` already holds the default arm (`IsIndentingScope`) and is a FMT-INDENT code owner; a new class adds a type (escalation 9). Recommend an existing class.
2. Keep the explicit `AccessorListSyntax` arm or drop it as redundant with the default arm. Dropping changes no reachable result but is a semantic edit the issues do not ask for. Recommend moving the body verbatim.

### Issue goals

All three Done-when items stand. "`SyntaxIndentationUtilities.IsIndentingScope` must be reachable from it, or move with it" is already true today and needs no work.

### Architecture findings

The issue is itself one: the analyzer and the formatter own the same decision (FC-015). The structural change the issue asks for is the fix; within the components the run changes, no new type needed, far below 100 lines.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1958

### Premise

Real. The set {`#if`, `#elif`, `#else`, `#endif`} is stated three times:

- Owner: `SyntaxTriviaUtilities.IsConditionalDirective(SyntaxTrivia)` (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:127`), public, by `SyntaxKind`; used by RH2001's fix and inside `SyntaxTriviaUtilities` (lines 456, 490).
- `ConditionalGroupUnitUtilities.IsConditionalDirective(DirectiveTriviaSyntax)` (`src/Reihitsu.Core/ConditionalGroupUnitUtilities.cs:23`), internal, by type: `ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax`. Callers: `GetConditionalDirectives` (same file, line 40) and `OrderingDeclarationUtilities.DeclarationHoldsConditionalDirective` (`OrderingDeclarationUtilities.cs:585`).
- `RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.WouldSeparateDirective` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.cs:197`), inline, by the same three types.

Note the issue places `ConditionalGroupUnitUtilities` correctly in `src/Reihitsu.Core/`, so it is in the owner's assembly and needs no visibility change.

Callers: RH7004's fix (code-fix registration, line 239); the ordering fixes of RH7102–RH7109 and the ordering move safety through `OrderingDeclarationUtilities`/`OrderingMoveSafety`.

The type-based and kind-based forms decide the same set: Roslyn's documentation (`Microsoft.CodeAnalysis.CSharp.xml`, 5.12.0, lines 32819-32847) gives `IfDirectiveTriviaSyntax`, `ElifDirectiveTriviaSyntax`, `ElseDirectiveTriviaSyntax`, `EndIfDirectiveTriviaSyntax` exactly one kind each (`IfDirectiveTrivia`, `ElifDirectiveTrivia`, `ElseDirectiveTrivia`, `EndIfDirectiveTrivia`). That `ConditionalDirectiveTriviaSyntax` covers exactly `If` and `Elif` I did not confirm by running anything; the existing tests below are what will hold the equivalence.

The remaining uses of these kinds decide something else and are rightly left alone (the issue's own exclusion): `SyntaxTriviaUtilities.ContainsUnbalancedDirectives` (If/EndIf nesting plus detached Elif/Else, lines 360, 673), `DirectivePairSplitScan.Splits` (a per-kind switch), `ReihitsuFormatterHelpers.GetOutermostConditionalGroups` (If/EndIf depth), `TrailingCommaCodeFixHelper:43` and `RH8201…CodeFixProvider:53` (`#if` only), `OrderingDeclarationUtilities:605` (`#endif` only).

### Mechanism and candidates

Mechanism: the membership test "is this directive one of the four conditional-compilation kinds" is written in three places, so a change to the set reaches only one (FC-015). Candidates: the four kinds × the two call sites (RH7004 `WouldSeparateDirective`, `ConditionalGroupUnitUtilities.IsConditionalDirective`), plus the negative members (`#region`, `#pragma`, `#nullable`, `#define`, …).

### Breaking inputs

FC-015. No input changes output if the four members keep their result. Existing coverage:

| Call site × member | Pinned by |
|---|---|
| RH7004, `#if` and `#endif` in the closing brace's leading trivia, group after the declaration | `RH7004…Tests.NoCodeFixWhenTheNewBlockWouldEndInFrontOfAConditionalPairWithTheSymbolUndefined` |
| RH7004, `#else`/`#endif` in the closing brace's leading trivia, `#if` before the declaration (fix offered) | `FixIsOfferedInsideTheActiveBranchOfAConditionalPairWhoseElseBranchEndsInFrontOfTheClosingBrace`, `FixIsOfferedInsideAConditionalPairThatEndsInFrontOfTheClosingBrace` |
| RH7004, negative kinds (`#pragma`, `#nullable`, `#region`) | `NoCodeFixWhenTheNewBlockWouldSplitAPragmaPair`, `…ANullablePair`, `…ARegionPair`, `FixIsOfferedForAPragmaPairAroundTheDeclarationAndEveryFollowingStatement` (these are decided by `SplitsDirectivePair` too, so they pin the result, not this check alone) |
| RH7004, `#elif` | none found; I did not settle whether any reachable input lets the `#elif` member decide alone (an `#elif` in front of the closing brace has its `#if` either in the same trivia, where the `#if` member decides, or earlier in the block, where `SplitsDirectivePair` may decide) |
| `ConditionalGroupUnitUtilities`, all four via ordering fixes | RH7103 `NoCodeFixWhenACrossedMethodBodyHoldsAGroupWithAnElifBranch`, `StaticMemberWhoseStaticModifierAnIfSplitsIsReportedWithoutFix`; RH7105 `MisorderedModifiersSplitByAConditionalDirectiveAreReportedWithoutFix`; RH7106 `MisorderedCompoundAccessModifiersSplitByAConditionalDirectiveAreReportedWithoutFix`; RH7109 `FixMovesAFieldOfATypeThatAGroupWrapsWholeAtTheEndOfTheDocument` |
| Direct unit test of either predicate | none found (`rg "IsConditionalDirective\|ConditionalGroupUnitUtilities" tests` is empty) |

All RH7004 tests (part of the 126 above) pass at `33d3ce8`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `SyntaxTriviaUtilities.IsConditionalDirective` | Owner; must stay (may gain a node-form overload, see internal choices) |
| `ConditionalGroupUnitUtilities` | Must change: no own statement of the set |
| RH7004 code fix (`WouldSeparateDirective`) | Must change: no own statement of the set |
| RH7004 fix result and its withholding | Must stay |
| Ordering fixes RH7102–RH7109, `OrderingMoveSafety` | Must stay |
| RH2001 fix | Not involved (already uses the owner) |
| Formatter, analyzer diagnostics, rule docs, specs | Not involved |

### Refuse option

No input class is at stake; withholding or leaving as written does not apply, and no shape could be declared unsupported. The refuse option is "no change", which keeps three statements of a four-member set. The change is a few lines. Recommend making it.

### Output questions

_None._

### Internal choices

1. Node form. Either `ConditionalGroupUnitUtilities.IsConditionalDirective` delegates to the trivia-form owner, or `SyntaxTriviaUtilities` gains a `DirectiveTriviaSyntax` overload that delegates to (or states with) the trivia form, or the callers switch to the trivia they already hold and the node form is deleted. None changes output; the requirement is that the four kinds are written once. No recommendation among them beyond that.

### Issue goals

All Done-when items stand. The request to name the left-alone uses in the PR is met by the list under *Premise*.

### Architecture findings

The issue is itself one: one rule stated in three places (FC-015); the fix is the structural change, a few lines, no new type.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1963

### Premise

Real, and wider than reported. In `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`, the read "first comment, directive, or disabled text in the trailing trivia of a token, else in the leading trivia of the first token of the node behind it" (`FindFirstUnsupportedOperatorTrivia(token.TrailingTrivia) ?? FindFirstUnsupportedOperatorTrivia(node.GetFirstToken().LeadingTrivia)`) is written five times, not three:

| Place | Gap | Named by the issue |
|---|---|---|
| `FindTriviaBehindToken` (757), 5 call sites (709-733) | assignment operator, `=` of equals-value, switch arm `=>`, attribute `=`, using-alias `=` (FMT-UNSUP-020/021/022) | yes (the owner) |
| `FindTriviaBehindBinaryOperator` (465) | operator → right operand (FMT-UNSUP-011) | yes |
| `FindTriviaBehindExpressionBodyArrow` (479) | `=>` → expression (FMT-UNSUP-012) | yes |
| `FindTriviaBehindConditionalOperator` (449) | `?` → `WhenTrue` and `:` → `WhenFalse`, two gaps of the same form (FMT-UNSUP-008) | no |
| `FindTriviaBetweenLinkDotAndName` (592) | `.` → name (FMT-UNSUP-016) | no |

Four more places read the same two-sided gap between two adjacent tokens rather than a token and a node: `FindTriviaInFrontOfExpressionBodyArrow` (506), `FindTriviaInAttachedPartGap` (538), `FindTriviaInsideLinkOperator` (575), `FindTriviaInJoinedGap` (650), each `FindFirstUnsupportedOperatorTrivia(previous.TrailingTrivia) ?? FindFirstUnsupportedOperatorTrivia(token.LeadingTrivia)`.

The issue's own wording ("no span differs, none is a defect") holds for every row: the bodies are identical. The precheck is reached by every formatting entry point (whole document, CLI, node-level entry points of code fixes).

Baseline at `33d3ce8`: `scripts/test.sh --project formatter --filter "FullyQualifiedName~Unit.Precheck"` → Passed 1861, Failed 0.

### Mechanism and candidates

Mechanism: the precheck states "the trivia in the gap behind a token" once per shape instead of once, so a change to how a gap is read (which trivia kinds count, which side is read first) has to be repeated per shape (FC-015, FC-016 for the unnamed siblings). Dispatch: the `switch (descendant)` in `PrecheckPhase` (lines ~206-245 and ~697-735) and the token walks of the four token-pair helpers. Candidates: the rows of the table above.

### Breaking inputs

FC-015, FC-016. The candidate result of each place must stay identical, including which trivia is returned first (trailing of the token before leading of the next), because the reported location and the reason's precedence depend on it. Existing coverage, by requirement tag:

| Gap | Pinned by |
|---|---|
| FMT-UNSUP-011 (binary) | `UnsupportedBinaryOperatorCommentTests` (spec examples, `FirstShapeInTheTextIsReported`, `CommentBehindAConditionalOperatorInFrontOfABinaryExpressionKeepsTheReasonOfAConditional`, node-level and document entry points); `DelimitedDocumentationInExpressionTests`; `LoneOperatorJoinBlankLineTests.LoneOperatorFollowedByCommentIsUnsupported` |
| FMT-UNSUP-012 (arrow clause) | `UnsupportedExpressionBodyArrowCommentTests`; `ExpressionBodyDirectiveGuardTests`; `ExpressionBodiedMethodTests.CommentAfterArrowIsUnsupported`; `ExpressionBodiedIndexerTests`; `GetOnlyMemberExpressionBodyTests`; `ExpressionBodyNextLineExpressionTests` |
| FMT-UNSUP-008 (conditional) | `UnsupportedConditionalOperatorCommentTests`; `ConditionalExpressionFullPipelineTests`; `ConditionalOperatorSpacingTests` |
| FMT-UNSUP-016 (dot to name) | `UnsupportedLinkDotNameCommentTests` |
| FMT-UNSUP-020, -021, -022, -013, -014, -015, -017 | `UnsupportedAssignmentCommentTests`, `UnsupportedSwitchArmExpressionCommentTests`, `UnsupportedNameEqualsCommentTests`, `UnsupportedExpressionBodyArrowLeadingCommentTests`, `UnsupportedAttachedPartCommentTests`, `UnsupportedLinkOperatorCommentTests`, `UnsupportedJoinedGapCommentTests` (all in the 1861 above) |

No new test is needed for a refactor of identical bodies; the existing tests are the characterization.

Documentation facts (FC-031): the summaries of `FindTriviaBehindBinaryOperator` and `FindTriviaBehindExpressionBodyArrow` carry scope statements that `FindTriviaBehindToken`'s summary does not: which nodes count as a binary expression (`is` type test counts; pattern, assignment, range do not) and what an arrow clause is (not a lambda's or switch arm's `=>`). `FindTriviaBehindToken`'s summary cites FMT-UNSUP-020 to -022 only.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` | Must change (internal) |
| Precheck result (refused or not, reported location, reason) for every entry point | Must stay |
| Other formatter phases, analyzers, code fixes, CLI, rule docs, specs | Not involved |

### Refuse option

No input class or shape is at stake; refusing and declaring do not apply. The refuse option is "no change". The change removes code. Recommend making it.

### Output questions

_None._

### Internal choices

1. Scope of the routing.
   - **A** — the issue's two methods only. Leaves the conditional (two gaps) and the dot-to-name gap as written copies of the same read, so "one method states how the trivia behind a token is read" (the issue's Done-when) is not true.
   - **B** — A plus `FindTriviaBehindConditionalOperator` and `FindTriviaBetweenLinkDotAndName`: every token-to-node gap goes through one helper.
   - **C** — one helper for the two-sided read between two adjacent tokens, used by all nine places (the token-to-node form passes `node.GetFirstToken()`).

   Recommend **B** as the minimum that meets the issue's purpose; C is the full single statement and also stays small (one expression per place) and in the same file, so the lead may take it as an adjacent cleanup under *Scope*. None changes output.
2. Whether the named wrappers stay as one-line methods or the call sites call the shared helper directly. Either way, the scope facts listed under *Breaking inputs* must survive somewhere the reader of the dispatch finds them. No recommendation.

### Issue goals

"One method states how the trivia behind a token is read, the other two callers use it" — change "the other two callers" to every caller with that read (option B or C above); otherwise the goal's first half is not met. No visible output is involved.

### Architecture findings

The issue is itself one: one rule stated in several places inside one component (FC-015), with two unnamed siblings (FC-016). Fix within `PrecheckPhase`; no new type.

### Follow-up candidates

_None._

## Whole run

- **Nothing to reproduce.** None of the three issues reports a behavior a test can fail on: each reports a structure (a duplicated statement) and states that no output changes. Every place is reached by a caller (named per issue), and every duplicate agrees with its owner today. The existing tests named above are the characterization; a reproduction test cannot be written. Whether that is escalation 4 is the lead's call; the maintainer's comment on #1956 frames the batch as maintenance with no output change, which this analysis confirms.
- **Build and focused baseline** at `33d3ce8`: `scripts/build.sh` succeeded with 0 warnings; the analyzer filter above passed 126, `Unit.Precheck` passed 1861.
- **No visible output is in question** in any issue, and no shape is a candidate for declaring unsupported.
- **Size.** All three together stay well under 100 production lines; only a new Core class for #1956 would add a type (escalation 9), which an existing class avoids.
