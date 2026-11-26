Role: test implementer
Commit: 1df7506
Verdict: #1956: tests written; #1958: tests written; #1963: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

None of the three issues reports an observed output, so there is nothing to reproduce (lead decision 1). The existing tests listed in `01-skeptic.md` are the characterization. I added tests only where the skeptic's coverage table showed a value that no test pins independently of the code being moved. Every new test passes on `1df7506`, as a test for existing behavior must, and pins a value that a faulty move would change.

## thoenissen/Reihitsu#1956

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Analyzer.Test.Core.CommentPositionUtilitiesTests.IsOwnedByNestedConstructReturnsTheOwnershipOfEachConstruct` (15 rows) | SHARED-COMMENT-006; FC-015 | direct call of `CommentPositionUtilities.IsOwnedByNestedConstruct` and `ListElementInteriorUtilities.FindOwningConstruct` | pass |

Why a new test is needed: `IsOwnedByNestedConstructMatchesFormatterOwnership` only asserts that the analyzer and the formatter give the same answer, never what the answer is. After the move, both walks call the same Core predicate. If an arm were dropped or changed during the move, both would agree on the wrong answer and the parity test would still pass. The new test uses the same 15 inputs and asserts the expected value for each one, for both walks: owned for anonymous object, initializer, switch expression, property pattern, parenthesized pattern, list pattern, collection expression after its `[`, block (default arm), query body, and interpolation. Not owned for the `[` of a collection expression, a lambda expression body, `from … in`, a binary operator, and a chain link. Each explicit arm of the predicate is the only reason its row is owned (none of those nodes is an `IsIndentingScope` scope), so dropping any arm flips its row.

The existing parity test and `IsOwnedByNestedConstructReturnsFalseBeforeElement` stay unchanged.

### Not covered, and why
- `AccessorListSyntax` explicit arm: no test, as in `01-skeptic.md`. No caller reaches an accessor list as a nested node inside a list element or a comment region, and the default arm (`IsIndentingScope`, pinned by `SyntaxIndentationUtilitiesTests.IsIndentingScopeRecognizesBraceScopesOnly`) gives the same answer for it.
- No direct Core test of the moved predicate. The lead left the choice of owning class and name to the implementer (decision 2), and the new analyzer test pins every arm through both public callers, whatever the predicate is called.
- Behavior of RH5035/RH5038/RH5039/RH5043/RH5044/RH5046/RH5047 and of the list-element join and alignment: pinned by their existing rule and formatter tests. The run changes no output, so nothing is added.

## thoenissen/Reihitsu#1958

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Core.Test.SyntaxTriviaUtilitiesTests.IsConditionalDirectiveRecognizesExactlyTheConditionalDirectives` (13 rows: `#if`, `#elif`, `#else`, `#endif` true; `#define`, `#undef`, `#region`, `#endregion`, `#pragma warning`, `#nullable`, `#line`, `#warning`, `#error` false) | FC-015 | direct call of the owner | pass |
| `Reihitsu.Core.Test.ConditionalGroupUnitUtilitiesTests.GetConditionalDirectivesReturnsExactlyTheConditionalDirectives` (new file) | FC-015 | direct call (`InternalsVisibleTo`) | pass |

Why: according to the skeptic, `rg "IsConditionalDirective|ConditionalGroupUnitUtilities" tests` was empty, so neither the owner nor the node-form call site had a direct test of the four-kind set. The first test pins the owner every copy will route to, including the negative kinds. The second test pins the node-form call site. Its input mixes all four conditional kinds with `#region`, `#pragma`, and `#nullable`, and `GetConditionalDirectives` stays whichever node-form option the implementer picks (skeptic, *Internal choices* 1), so it binds no choice. It does not call the node-form predicate itself, which the implementer may delete.

Neither test carries a requirement tag: no spec requirement states this helper set.

### Not covered, and why
- **RH7004 `#elif` member of `WouldSeparateDirective`**: no test, because the member cannot decide alone. Static proof: an `#elif` in the closing brace's leading trivia must have its `#endif` in the same trivia. Otherwise the closing brace would sit between the `#elif` and its `#endif`, inside the group, which leaves the block without its brace in some build (syntax-broken input). That `#endif` is a member of the same check, and `GetRelatedDirectives` returns the same group for both, so it finds the same `#if` and gives the same result. The `#if`, `#else`, and `#endif` results of RH7004 are pinned by the RH7004 tests the skeptic lists (`NoCodeFixWhenTheNewBlockWouldEndInFrontOfAConditionalPairWithTheSymbolUndefined`, `FixIsOfferedInsideTheActiveBranchOfAConditionalPairWhoseElseBranchEndsInFrontOfTheClosingBrace`, `FixIsOfferedInsideAConditionalPairThatEndsInFrontOfTheClosingBrace`).
- Ordering fixes RH7102–RH7109: pinned by the ordering tests the skeptic lists. Their output does not change.

## thoenissen/Reihitsu#1963

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedAssignmentCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheRightOperand` | FMT-UNSUP-020; FC-015 | `AssertUnsupportedFor` (LF and CRLF) | pass |
| `…UnsupportedBinaryOperatorCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheOperand` | FMT-UNSUP-011; FC-015 | `AssertUnsupportedFor` | pass |
| `…UnsupportedConditionalOperatorCommentTests.CommentInTheTrailingTriviaOfTheOperatorIsReportedBeforeOneInFrontOfTheBranch` (`?` and `:` gaps) | FMT-UNSUP-008; FC-015, FC-016 | `AssertUnsupportedFor` | pass |
| `…UnsupportedLinkDotNameCommentTests.CommentInTheTrailingTriviaOfTheDotIsReportedBeforeOneInFrontOfTheName` | FMT-UNSUP-016; FC-015, FC-016 | `AssertUnsupportedFor` | pass |

Why: the only thing the shared helper can get wrong while still giving the same yes-or-no answer is the order of its two reads. If it read the next node's leading trivia before the token's trailing trivia, a gap holding a comment on each side would report the second comment. Each requirement states that the location is "the span of the first comment, directive, or disabled text in that place". `rg '(=|\?|:|&&|\+|=>|\.) // [a-z0-9]+\\n\s*/[/*]' tests/Reihitsu.Formatter.Test/Unit/Precheck` found this case pinned only for the arrow clause (`UnsupportedExpressionBodyArrowCommentTests`, row "Line comment behind the arrow and a comment line"). The new tests put `// first` in the token's trailing trivia and `// second` on the next line, in the leading trivia of the node's first token, and assert that `// first` is reported under LF and CRLF. There is one test for each place that states the read today: `FindTriviaBehindToken`, `FindTriviaBehindBinaryOperator`, both gaps of `FindTriviaBehindConditionalOperator`, and `FindTriviaBetweenLinkDotAndName`. `FindTriviaBehindExpressionBodyArrow` is already pinned.

### Not covered, and why
- FMT-UNSUP-021 (switch arm `=>`) and FMT-UNSUP-022 (attribute `=`, using alias `=`): one call site, no test of their own. `rg -n "FindTriviaBehindToken\(" src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` shows the five call sites (assignment, equals-value, switch arm, attribute argument, using alias). All five reach the one method `FindTriviaBehindToken(SyntaxToken, SyntaxNode)` and differ only in the token and node they pass, which the existing FMT-UNSUP-020/-021/-022 tests already pin. The FMT-UNSUP-020 test above covers the read order of that shared method.
- The four token-to-token readers (`FindTriviaInFrontOfExpressionBodyArrow`, `FindTriviaInAttachedPartGap`, `FindTriviaInsideLinkOperator`, `FindTriviaInJoinedGap`): out of scope (lead decision 3, option B), so not touched.
- Yes-or-no refusal, reason, entry points, closure, and CRLF of every shape: pinned by the 1861 precheck tests the skeptic ran.

## Whole run

### Failure classes
FC-015 and FC-016 apply (one decision stated in several places) and are the purpose of the tests above. FC-031 (stale summaries) is a documentation fact for the cleaner and auditor, not something a test can pin. The other classes concern output, layout, code-fix convergence, or new shapes. This run changes none of them: no output, no fix, no shape, no line-ending handling. So they cannot reach the changed code beyond what the existing suites already pin.

### Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~CommentPositionUtilitiesTests"
scripts/test.sh --project core --filter "FullyQualifiedName~ConditionalGroupUnitUtilitiesTests|FullyQualifiedName~SyntaxTriviaUtilitiesTests.IsConditionalDirective"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Unit.Precheck"
```
Results on `1df7506` plus these tests: analyzer 33 passed; core 14 passed; formatter precheck 1865 passed (the 1861 baseline plus 4); 0 failed.

## Follow-up candidates
_None._
