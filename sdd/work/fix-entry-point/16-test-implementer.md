Role: test implementer
Commit: see the commit that adds this file (parent 683b7bba)
Verdict: tests written

Repair round 1, lead decisions 21 and 22 (test part): auditor findings 1, 2, 3 and 5 (`14-auditor.md`), with the examples of `15-spec-author.md`. My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched.

Expected values come from the FMT-STRUCT-026 and FMT-PIPE-029 text and examples as changed in 683b7bba. I also checked every document-level and context expected value against `main`. To do that I built a scratch worktree of `df4f828d` (removed since) and ran its `scripts/format.sh` over the same inputs. `main`'s output equals each expected value: members `a`, `b`, `c`, `d` (`/* a⏎ b */ /* c */`), `e` (`/* a⏎ b */ // c`), `h`, the FMT-STRUCT-026 three-declarator example, and the inputs `p`, `q`, `pq` and `m`. None of the values comes from a patched copy of the code (FC-035). The fix's expected output is the formatter's layout, by answer 1a–1f A.

## Tests

`src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/StructuralTransformSeparatorTests.cs`. These are phase-output tests: `AssertSplitFields`, LF and CRLF, whole text, and `AssertSameTree`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| BlockCommentThatSpansLinesBehindACommaGetsALineBreakBehindIt (spec: `_x, /* a⏎ b */ _y;`) | FMT-STRUCT-026 | `AssertSplitFields` | fails (`b */private int _y;`) |
| BlockCommentOnOneLineBehindACommaAndASpaceGetsNoLineBreakBehindIt (spec contrast: `_x, /* a */ _y;`) | FMT-STRUCT-026 | same | passes (other side of the condition) |
| LastOfTwoCommentsBehindACommaThatSpansLinesGetsALineBreakBehindIt (`/* a */ /* b⏎ c */`) | FMT-STRUCT-026 | same | fails |
| OneLineCommentBehindACommentThatSpansLinesBehindACommaGetsALineBreakBehindIt (`/* a⏎ b */ /* c */`) | FMT-STRUCT-026 | same | fails |
| SingleLineCommentBehindACommentThatSpansLinesBehindACommaGetsOneLineBreakBehindIt (`/* a⏎ b */ // c`, `_y` on the next line) | FMT-STRUCT-026 | same | passes (exactly one line break) |
| EveryFieldBehindACommentThatSpansLinesStartsItsOwnLine (spec example, three declarators) | FMT-STRUCT-026 | same | fails |

`src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs`. These are document-level tests that pin `main`'s output: `AssertRuleResult` with LF, CRLF, a second pass, and phase stability.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| BlockCommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField (member `a`) | FMT-STRUCT-026; FC-016, FC-021 | `AssertRuleResult` | fails |
| BlockCommentThatSpansLinesBehindASeparatorWithTheDeclaratorOnTheNextLineEndsTheLineOfTheField (member `b`) | FMT-STRUCT-026; FC-016, FC-021 | same | fails |
| LastOfTwoCommentsBehindASeparatorThatSpansLinesEndsTheLineOfTheField (member `c`) | FMT-STRUCT-026 | same | fails |
| OneLineCommentBehindACommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField (`/* a⏎ b */ /* c */`) | FMT-STRUCT-026 | same | fails |
| SingleLineCommentBehindACommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField (`/* a⏎ b */ // c`, auditor's holding member) | FMT-STRUCT-026 | same | passes, must stay |
| CommentThatSpansLinesBehindTheFirstOfThreeDeclaratorsEndsTheLineOfTheFirstField (member `h`) | FMT-STRUCT-026 | same | fails |
| EveryFieldBehindACommentThatSpansLinesStartsItsOwnLine (spec example) | FMT-STRUCT-026 | same | fails |

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterFieldSplitTests.cs`. These run the context entry point through `AssertSplitsTarget`: LF and CRLF, and a second pass.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| MembersBehindACommentThatSpansLinesInTheGapsKeepTheirPlace (spec example `pq`) | FMT-PIPE-029, FMT-PIPE-010; FC-002 | `AssertSplitsTarget` | fails (moves `_x` and `_q`) |
| FirstFieldBehindACommentThatSpansLinesInTheGapInFrontKeepsItsPlace (`p`, front side only) | FMT-PIPE-029, FMT-PIPE-010; FC-002 | same | fails |
| MemberBehindACommentThatSpansLinesInTheGapBehindKeepsItsPlace (`q`, back side only) | FMT-PIPE-029, FMT-PIPE-010; FC-002 | same | fails |
| LaterFieldBehindACommentThatSpansLinesBehindTheSeparatorStartsItsOwnLine (spec example `m`) | FMT-PIPE-029 | same | fails (first field at column 11, `b */private int _y;`) |
| LaterFieldBehindACommentThatSpansLinesStandsAtTheMemberColumn (member `a`, no other member) | FMT-PIPE-029 | same | fails |

The other side of the "shares the line" condition is already covered by the existing tests. A whitespace-only gap: `FirstFieldOfATargetBehindAMemberOnItsLineStartsTheNextLine` and `MemberBehindTheTargetOnItsLineStartsTheNextLine`. A one-line comment gap: `CommentsInTheGapsStayBehindTheTokenInFrontOfThem`. I did not change them.

`src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`. Each test uses `Verify` (one pass, re-analysis), `AssertSingleFixUnderBothLineEndings` (exactly one offered action, LF and CRLF), and `AssertFormattingChangesNothing` (the fix result is the formatter's stable layout).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FixLeavesTheMembersBehindACommentThatSpansLinesInTheGapsWhereTheyAre (`pq`) | RH7101-004; FC-013 | `Verify` + both line endings + formatter stability | fails |
| FixLeavesTheFirstFieldBehindACommentThatSpansLinesInTheGapInFront (`p`) | RH7101-004; FC-013 | same | fails |
| FixLeavesTheMemberBehindACommentThatSpansLinesInTheGapBehind (`q`) | RH7101-004; FC-013 | same | fails |
| FixStartsTheFieldBehindACommentThatSpansLinesOnItsOwnLineBehindAMemberInFront (`m`) | RH7101-004; FC-013 | same | fails |
| FixStartsTheFieldBehindACommentThatSpansLinesBehindTheSeparatorOnItsOwnLine (`a`) | RH7101-004; FC-013 | same | fails |

The same file has one comment fix (finding 5). The `GetFixAllScenario` comment now describes the sequential Fix All provider instead of "the batch fixer".

`src/Reihitsu.ArchitectureTests/Analyzer/CodeFixFormatterAccessTests.cs` (finding 3)

The banned set is now computed by reachability instead of by type kind:

- **Seeds:** every phase class, `FormattingPipeline`, and every `CSharpSyntaxRewriter` of the formatter.
- **Closure:** every formatter class that a banned class depends on directly (ArchUnitNET dependencies, so calls inside method bodies count), followed transitively. This is how a phase runs its rewriters, the static ones included.
- **Not banned:** classes named `…Helpers`, `…Utilities`, or `…Extensions`, and types of `Reihitsu.Formatter.Data`. These are the helper and utility classes FMT-PIPE-009 keeps callable, plus the data the phases read. They also do not pass the ban on: a utility's own dependencies are not followed. That keeps `ExpressionBodiedTransformUtilities` and `ReihitsuFormatterHelpers` callable even though they touch transform or rewriter types.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| CodeFixesDoNotDependOnFormatterPhasesTransformsOrThePipeline (unchanged, over the wider set) | FMT-PIPE-009; FC-017 | ArchUnitNET `NotDependOnAny` | passes (no code fix uses a banned type today) |
| BannedTypesIncludeThePhasesTheTransformsAndThePipeline (4 old rows plus `MemberLineStartRewriter`, `BlankLineCollapser`, `IndentationRewriter`, `RegionNamingRewriter`, `BracePlacer`, `TokenGapNormalizer`, `BlankLineEditor`, `LayoutComputer`, `DocCommentElementNormalizer`, `ExpressionBodyToBlockConverter`, `CommaSpacingRule`, `ArgumentAlignmentContributor`) | FMT-PIPE-009; FC-017 | reflection | passes; the 10 rows from `IndentationRewriter` on would fail on the old set |
| BannedTypesExcludeHelpersAndUtilityClasses (3 old rows plus `TriviaListUtilities`, `LineBreakTriviaUtilities`, `FormattingContext`, each used by a phase) | FMT-PIPE-009 | reflection | passes |

I printed the computed set once with a temporary probe, deleted since. It holds the 12 phases, the pipeline, every rewriter and transform, the spacing rules, the layout contributors, and the phase machinery: `SpacingPolicy`, `LayoutModel`, `BracePlacer`, `TokenLocator`, `UsingGrouping` and the like. It also holds `Reihitsu.Formatter.Utilities.EndOfLineWhitespace`, which `CleanupPhase` uses and whose name is not `…Utilities`. No code fix uses it today. If the lead counts it as a helper, rename it or add it to the exclusion. I left it banned.

## Watched failures

These runs were in Debug on 683b7bba plus these tests.

- **Formatter** (`StructuralTransformSeparatorTests`, `FieldDeclarationSplitTests`, `ReihitsuFormatterFieldSplitTests`): 15 failed, 139 passed. The 15 failures are exactly the new rows marked "fails". Every diff is one of three things:
  - `b */private int _y;` (finding 1);
  - `_x` or `_q` moved off the `*/` line (finding 2);
  - for `m`, the first field at column 11 (finding 1, member-in-front case).
- **Analyzer** (`RH7101DoNotCombineFieldsAnalyzerTests`): 5 failed, 59 passed. The 5 failures are exactly the new tests, with the same three diffs.
- **Architecture**, Release: 115 passed, built without warnings.

## Not covered, and why

- **Phase output for member `b` (the line break behind `*/` written by the author).** FMT-STRUCT-026 says "a line break behind the last of those comments". It does not say whether the author's line break is carried trivia or the separator, and decision 17 keeps phase output internal. The document-level test pins `b`, and the phase test for `/* a⏎ b */ // c` asserts exactly one line break where the comment already ends its line.
- **The `i` member** (initializers and a method behind). Its boundary is the same as `a`'s. The comment-kind decision does not look at initializers, and the shared-line decision is covered by `p`, `q` and `m`.
- **Directive in the gap.** A directive cannot stand in a shared-line gap. A directive inside the declaration withholds the split (FMT-STRUCT-023), which the existing tests cover.
- **Fix All over these shapes.** Each input has one diagnostic. Fix All over two declarations is covered by `FixAllSplitsTwoDeclarationsOnOneLine` and the scenario.
- **Finding 4** (`GetColumn` copy) is production code with no observable change. The existing tests pin it.
- **Failure classes FC-004 to FC-012, FC-014, FC-018, FC-022 to FC-030, FC-032 to FC-037.** The auditor dismissed them for this change (`14-auditor.md`), and the repair adds no surface they reach.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~StructuralTransformSeparatorTests|FullyQualifiedName~FieldDeclarationSplitTests|FullyQualifiedName~ReihitsuFormatterFieldSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~CodeFixFormatterAccessTests|FullyQualifiedName~Sdd"
```
