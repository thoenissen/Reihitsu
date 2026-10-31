Role: test implementer
Commit: 621b90e
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's scope answers; nothing was ignored.

Expected values come from the spec examples (FMT-PIPE-012, FMT-PIPE-023, FMT-PIPE-024, FMT-CLEAN-008), the change doc, and the zero-offset decision (A). Where the spec says a node-in-document output stays "today's output while the target holds nothing an earlier phase changes", the expected value is the unchanged code's output for the space variant, checked by a scratch probe against `main`'s formatter (kept outside the repository) and against the shape of the input; each such test also runs with a tab at the same place, which is what fails today.

## Tests

New class `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterDetachedRootTests.cs`. Every input marks one place inside the target with `⇥`; each fixture runs with a space and with a tab there, under LF and CRLF, with a second pass (node-in-document: the document again; `FormatNode`: the result again at its depth).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatNodeOnStatementBehindBlankLineReturnsItWithoutLeadingLineBreak` | FMT-PIPE-023 | `AssertFormatNode` | fails (space variant: leading line break) |
| `FormatNodeInDocumentOnStatementBehindBlankLineKeepsTheBlankLine` | FMT-PIPE-023 | `AssertFormatsTarget` | passes |
| `ElseIfTargetKeepsTheLineCommentBehindItsCondition` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant: `// note` lost) |
| `ElseIfTargetKeepsTheBlockCommentBehindItsCondition` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant: `/* explanation */` lost) |
| `ElseIfTargetWithContextKeepsTheLineCommentBehindItsCondition` | FMT-PIPE-023 | `AssertFormatsTarget` with context | passes |
| `ChainPrefixTargetKeepsTheLayoutOfTheLongerChain` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant re-lays the prefix) |
| `AccessorListTargetOfOneLinePropertyKeepsTheAccessorAttributeOnThePropertyLine` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant splits) |
| `AccessorTargetOfOneLinePropertyKeepsItsAttributeOnThePropertyLine` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant splits) |
| `ObjectCreationTargetInsideArgumentGetsItsInteriorWrapJoined` | FMT-PIPE-023, FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant keeps `new⏎Example()`) |
| `ParameterTargetGetsItsInteriorWrapJoined` | FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant keeps `int⏎x = 1`) |
| `ElementAccessArgumentListTargetGetsItsArgumentsCollapsed` | FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant keeps `[1,⏎2]`) |
| `CaseLabelPatternTargetKeepsItsOpeningBraceOnTheLabelLine` | FMT-PIPE-024 | `AssertFormatsTarget` | fails (tab variant moves `{` off the label line) |
| `CaseLabelPatternTargetBehindNotKeepsItsOpeningBraceOnTheLabelLine` | FMT-PIPE-024 | `AssertFormatsTarget` | fails (same, through `not`) |
| `AssignedCollectionInitializerTargetKeepsItsOpeningBraceOnTheAssignmentLine` | FMT-PIPE-024 | tab = space, `L = {` line kept, second pass | passes (see *Findings*) |
| `StatementTargetBehindLocalDeclarationGetsTheBlankLineOfItsListPosition` | FMT-PIPE-024 | `AssertFormatsTarget` | passes |
| `FormatNodeOnStatementBehindLocalDeclarationAddsNoBlankLine` | FMT-PIPE-024 | `AssertFormatNode` | passes |
| `EmbeddedIfTargetBehindMultiLineCommentTakesTheIndentationPhaseDepth` | FMT-PIPE-024 | ends-with assertion, second pass | fails (`{`/`X();`/`}` at 8/12/8, expected 12/16/12) |
| `InitializerTargetBehindMultiLineCommentTakesTheIndentationPhaseDepth` | FMT-PIPE-024 | `AssertFormatsTarget` | fails (`/* a` in column 12, expected 8) |
| `FormattingContextFactsHoldOnlyPlainValues` | FMT-PIPE-024 | reflection over `FormattingContext` and every formatter type its fields hold | fails (`PrecedingTokenFacts.TrailingTrivia`, `RootListPosition.PreviousStatement`, `RootListPosition.PreviousSection`) |
| `FormatNodeOnEmbeddedStatementOfIfPlacesItOneLevelDeeper` | FMT-PIPE-012 | `AssertFormatNode` | fails (column 8, expected 12) |
| `FormatNodeOnEmbeddedStatementOfElsePlacesItOneLevelDeeper` | FMT-PIPE-012 | `AssertFormatNode` | fails |
| `FormatNodeOnEmbeddedStatementOfLoopPlacesItOneLevelDeeper` | FMT-PIPE-012 | `AssertFormatNode` | fails |
| `FormatNodeOnIfOfElseClauseAddsNoLevel` | FMT-PIPE-012 | `AssertFormatNode` | passes |
| `FormatNodeOnEmbeddedStatementTakesTheExplicitIndentLevel` | FMT-PIPE-012 | `AssertFormatNode` (`indentLevel` 0 and 3) | passes |
| `FormatNodeOnObjectInitializerMemberAddsNoLevelForTheInitializer` | FMT-PIPE-012 | `AssertFormatNode` | fails (column 12, expected 8; `2` in 17) |
| `FormatNodeOnAnonymousObjectMemberAddsNoLevelForTheAnonymousObject` | FMT-PIPE-012 | `AssertFormatNode` | fails |
| `FormatNodeOnElseIfWithTrailingCommentGetsTheResultOfTheNodeWithoutParent` | FMT-PIPE-012 | `AssertFormatNode` + equality with a parentless copy at depth 2 | fails (space variant keeps `// note` behind the condition) |
| `FormatNodeOnChainPrefixGetsTheResultOfTheNodeWithoutParent` | FMT-PIPE-012 | `AssertFormatNode` + parentless copy | fails |
| `FormatNodeOnAccessorListOfOneLinePropertyGetsTheResultOfTheNodeWithoutParent` | FMT-PIPE-012 | parentless copy at depth 1 | fails |
| `FormatNodeOnObjectCreationInsideArgumentGetsTheResultOfTheNodeWithoutParent` | FMT-PIPE-012 | parentless copy at depth 2 | fails |
| `FormatNodeOnStatementPartOfTreeReturnsItWithoutTrailingLineBreak` | FMT-CLEAN-008 | `AssertFormatNode` | passes |

Changed test: `ReihitsuFormatterTests.FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation` — expectation changed to the FMT-PIPE-012 example (no leading line break), now tagged FMT-PIPE-012; fails today. The change doc names this test as the one expectation that changes.

Whole formatter project at this commit: 24 failed (the 23 new failing tests above plus the changed one), 3409 passed. Architecture project: 89 passed (`RequirementCoverageTests` now finds tests for FMT-PIPE-023 and FMT-PIPE-024).

The "parentless copy" helper builds the copy with `CSharpSyntaxTree.Create(node).GetRoot()` and formats it with the explicit `indentLevel` equal to the depth FMT-PIPE-012 gives the node, so it checks both "the result the same node without a parent gets" and the depth.

## Findings

- **Assigned collection initializer: no discriminating input found.** For `L = {` and `L =⏎{` targets, with or without a tab, the unchanged code gives the same output, so the test pins only tab = space, the brace staying on `L = {`, and stability; it passes today. Observation, no diagnosis: for that target the node-in-document output places the elements and `}` in the column of `{` (`L = {` with `{` in 16, `1,` and `2` in 16, `}` in 16), while document formatting places the elements four columns right of `{` and `}` under it. The output is the same before the change and, per the spec, after it; whether it is a defect of FMT-PIPE-017 is for the lead to route (follow-up candidate, end-user visible only if a code fix formats such a target).
- **Zero-offset embedded `if`: only the braces and body are asserted.** Today the output also inserts a blank line after `if (a)` and moves `/* note` from column 12 to 8; the blank line is the unspecified case the skeptic listed as a follow-up, and the spec gives the comment's first line no column I can derive with certainty (the depth says 12; FMT-PIPE-024 also says leading comments are aligned from the preceding token). The test asserts the text from `*/ if (b)` to the end and a stable second pass. If the lead wants the comment column pinned, that is a spec question, not a test change.
- No test or spec found wrong; no scope growth; no architecture finding.

## Not covered, and why

- FC-028 (FormatNode vs node-in-document disagree): the disagreements in these tests are the specified ones (FMT-PIPE-012 vs FMT-PIPE-024); both surfaces are tested on the same inputs (issue example, `else if`, chain prefix, accessor list, object creation).
- FC-007/FC-008/FC-030/FC-032 (code fixes): no code-fix output is specified to change; the RH5405 comment loss is out of scope (thoenissen/Reihitsu#1527). The analyzer project is run by the validator after the implementation; no new analyzer test.
- FC-019/FC-020/FC-021: comments in the touched gap are covered by the two `else if` tests (line and block comment on the header line). Directives in front of a target are covered by existing `ReihitsuFormatterNodeBlankLineTests` (the list-position and preceding-token facts read them); no directive sits in a gap a new flag decides.
- List-element flag members: only object creation in an argument and a parameter root are tested. The lambda `static` modifier, the attribute target `:`, a lambda default parameter value, and an attribute-list root were not: they reach `ListElementInteriorJoinRewriter` through the same flag but not provably one call site (`FindInnermostListElement` vs `ListElementInteriorUtilities.FindOwningConstruct` vs `IsListElement`). The "owning construct between" side of the flag (a target inside a brace scope inside an argument) has no test.
- Case-label pattern through `and`/`or`: `or` was probed and gives no tab difference with the brace on its own line; only the direct and `not` forms with the brace on the label's line are tested.
- `FormatNodeInDocumentWithContextAsync` depth for a zero-offset context: not tested; no output of it is specified to change.
- FC-010 CRLF/idempotency: every test runs LF and CRLF with a second pass. FC-022/023/024/025/026/029/036: no rewrite of literals, identifiers, or member kinds is involved.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterDetachedRootTests|FullyQualifiedName~FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation"
scripts/test.sh --project architecture
```
