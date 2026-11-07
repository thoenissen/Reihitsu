Role: implementer
Commit: 317a6a5
Verdict: #1243: test disputed (3 tests); #1383: tests pass; #1149: tests pass; #1391: tests pass; #952: tests pass (no production change); #1611: tests pass. Whole run: 15 existing tests now conflict with the changed specs (routed below).

My prompt carried no intended fix or hoped-for verdict. I worked from `00-lead.md` (decisions 3 and 4 bind the structure), `02-skeptic.md`, `04-test-implementer.md`, `05-spec-author.md`, the change doc, and `git diff origin/main -- sdd/specs`. No test file was changed.

## thoenissen/Reihitsu#1243

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | New `EndLineInFrontOf(previousToken, token, endOfLine)`: appends a line break to the previous token's trailing trivia unless `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` already finds one in the gap. This is the one shared check from decision 4 | FMT-BREAK-030 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | `NormalizeBinaryOperatorPosition` uses the shared check instead of `AppendEndOfLine`; summary updated | FMT-BREAK-030, FMT-BREAK-017 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs` | `MoveQuestionTokenToNextLine` and `NormalizeColonTokenPosition` use the shared check; summaries updated | FMT-BREAK-030, FMT-BREAK-018 |

### Policy owners
`rg -n "AppendEndOfLine\(" src/Reihitsu.Formatter` lists the private "line break unless one exists" checks that decision 4 leaves for follow-up 9: `ChainLineBreakRewriter.EnsureLineBreak` (`HasLineBreak` → `AreSeparatedByEndOfLine`), `LineBreakListRewriter` (separator sites, lines 225 and 567), and `GenericConstraintLineBreakRewriter` (line 238). The new helper uses the same Core fact (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine`) as the chain rewriter.

### Tests run
Focused run (command in *Whole run*). `LoneOperatorJoinBlankLineTests`: 11 pass, 3 fail. The 3 failures are disputed below; each fails the same way on `d0e2102` without my change.

### Disputed tests or scope
- **`LoneCoalesceOperatorIsJoinedWithoutBlankLine`.** The blank line is gone. The test expects `?? "x"` at column 16, under `s`. The formatter puts it at column 20. FMT-INDENT (the binary-operator requirement, `sdd/specs/formatter/indentation.md:905`, unchanged) says: "A `??` operator MUST be indented one level deeper than that column." Formatting the already-joined input `var p = s⏎                ?? "x";` on main also gives column 20. The expected column contradicts an unchanged requirement.
- **`LoneOperatorBelowCommentLineIsJoinedWithoutBlankLine`** and **`LoneConditionalColonBelowCommentLineIsJoinedWithoutBlankLine`.** These tests expect no blank line above `// between` (or `// c`). That comment is on its own line and is not first in a block. FMT-BLANK-011 (unchanged) says the formatter "MUST ensure exactly one blank line before the first comment in a token's leading trivia that is not glued to the token". After my change the line-break phase writes no blank line there (checked with `scripts/trace.sh`), and `BlankLinePhase` inserts exactly one. It does the same for the already-joined input `var p = a⏎// between⏎&& b;`, which no line-break rewriter touches. The output has one blank line, as on main. The skeptic's table row "blank line between `a` and `// between`" treats this FMT-BLANK-011 blank line as a defect.

### Architecture findings
_None_ beyond decision 4, which is carried out: one shared check, three call sites.

### Follow-up candidates
_None new_ (follow-up 9 in `00-lead.md` stands).

## thoenissen/Reihitsu#1383

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | New `VisitBracketedParameterList`. Several helpers are made generic over `BaseParameterListSyntax` instead of being copied: `CollapseSeparatorsToPreviousParameterLine`, `CollapseCloseParenToParameterLine`, `EnsureParametersOnSeparateLines`, and `CollapseOwnedOpenParenToDeclarationLine`, which now takes `BaseParameterListSyntax`. New private `GetOpenToken`/`GetCloseToken`. `EnsureSeparatorsHaveEndOfLine` takes an optional `isWrappedAfterOpener` split signal. Only the indexer visit passes it; see *Disputed* | FMT-BREAK-011, FMT-BREAK-012, FMT-BREAK-013, FMT-PIPE-014 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/DeclarationBraceLineBreakRewriter.cs` | Indexer case: joins `[` to `this` through `CollapseOwnedOpenParenToDeclarationLine` before the accessor layout, the same path as methods | FMT-BREAK-011 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs` | The `SimpleAssignmentExpression` guard is removed. `NormalizeSimpleAssignment*Placement` is renamed to `NormalizeAssignmentOperatorPlacement`/`NormalizeAssignmentValuePlacement`, which now handle every assignment kind | FMT-BREAK-016, FMT-PIPE-014 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/CommentIndentationContributor.cs` | A comment in front of a `CloseBracketToken` whose parent is `CollectionExpressionSyntax` gets one level more than the bracket, as in front of `}`. List patterns, element access, and attribute lists are unchanged | FMT-INDENT-006, FMT-PIPE-014 |

### Policy owners
- `rg -n "VisitParameterList|VisitBracketedParameterList" src/Reihitsu.Formatter` → `LineBreakListRewriter` only. `ListElementInteriorJoinRewriter` (FMT-BREAK-015) and `ArgumentAlignmentContributor` (FMT-INDENT-012) already handled bracketed lists and are untouched.
- `rg -n "CloseBraceToken" src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/CommentIndentationContributor.cs` → the single "one level deeper" decision, which I extended in place.

### Tests run
`IndexerParameterListLayoutTests`, `CompoundAssignmentLayoutTests`, `CollectionExpressionClosingBracketCommentTests`: all pass.

### Disputed tests or scope
- **FMT-BREAK-012 "wrapped after its opening parenthesis" for method parameter lists and argument lists.** `FirstIndexerParameterJoinsOpeningBracket` requires `this[⏎int a, int b]` to become one parameter per line, and the indexer path now does that. On main and after this change, the method form `void M(⏎int a, int b)` and the call `M(⏎a, b)` both become one line, `M(int a, int b)` (observed with `scripts/format.sh`). That is against the unchanged text of FMT-BREAK-012. So in this one shape the indexer and the method now differ, and FMT-PIPE-014 says they should be laid out alike. The defect is on the method and argument side. Fixing it would change visible output for every method and call and touch the paths of RH5101/RH5102. I did not do it. Lead decision: follow-up, or leave it as a known gap.
- **Existing tests whose expected values contradict the changed FMT-BREAK-011/-012 and FMT-PIPE-014** (they pinned the old "indexer list is not laid out" output). Route to the test implementer:
  - `ParameterContinuationLineAlignmentTests.IndexerLeadingCommaStaysAligned`: expects `this[int first⏎, int second]` unchanged. FMT-BREAK-011 now moves the comma (`this[int first,⏎int second]`).
  - `ParameterContinuationLineAlignmentTests.ContinuationOfParameterAfterLeadingCommaIsAlignedWithItsFirstToken`: same leading-comma shape.
  - `ParameterContinuationLineAlignmentTests.IndexerParameterOnSharedLineKeepsContinuationUnderItsFirstToken`: expects `this[int first, [Obsolete] // keep⏎int second]` unchanged. The list is multi-line, so FMT-BREAK-012 gives one parameter per line.
  - `AttributeListStructuralFinalOutputTests.LineBreakBetweenMergedListsOfBracketedListsIsJoined`: expects `int this[[A, B] int i, int j]` on one line. The output is now `this[[A, B] int i,⏎int j]`, the same as the method form `void M([A]⏎[B] int i, int j)` gives on main (`M([A, B] int i,⏎int j)`, traced). `HasElementSplitSignal` deliberately keeps a list split when an element spanned lines as written.
- **Existing tests whose expected values contradict the changed FMT-INDENT-006** (comment in front of a collection expression's `]` stood in the `]` column). Route to the test implementer:
  - `CommentBeforeBraceBlankLineTests`: `RemovesBlankLineBetweenCommentAndBraceOrBracket`, `RemovesBlankLineBetweenCommentBehindDirectiveAndBraceOrBracket`, `RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket`, `RemovesBlankLineBehindLineOfDelimitedCommentAndLineComment`, `RemovesBlankLineBehindLineCommentThatFollowsEndOfMultiLineComment`, `RemovesBlankLineBehindLineOfTwoDelimitedComments`, `RemovesBlankLineBehindLineCommentThatFollowsDelimitedDocumentationComment`. Each differs only in the comment's column in front of `]` (now the elements' column). The blank lines these tests assert are unchanged.
  - `MultiLineGluedCommentBlankLineTests.InsertsNoBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket` and `RemovesAuthorBlankLineAboveMultiLineCommentGluedToCollectionExpressionBracket`: `/* x⏎ */ ];`. The comment's first line now takes the elements' column (18), as the brace analogue `/* x⏎ */ }` in the same file takes one level deeper than `}`. The blank-line assertions are unchanged.

### Architecture findings
_None._

### Follow-up candidates
- Method parameter lists and argument lists wrapped only after the opening parenthesis become one line, against FMT-BREAK-012 (above).

## thoenissen/Reihitsu#1149

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs` | New `VisitEnumMemberDeclaration` → `NormalizeEnumMemberValueOperatorPlacement`, which reuses `NormalizeOperatorPlacement`, the same path as declarators and properties. Value placement was already handled by `VisitEqualsValueClause`. Parameters are unchanged: FMT-BREAK-015 owns them (decision 5) | FMT-BREAK-016, RH5111-004 |

### Policy owners
`rg -n "NormalizeOperatorPlacement\(" src/Reihitsu.Formatter` → one owner, now called for assignments, declarators, properties, and enum members.

### Tests run
`EnumMemberValueLayoutTests`: all pass. Analyzer filter `RH5111|RH5202`: 199 passed, 0 failed (the RH5111 fix tests converge in one iteration through delegation).

### Disputed tests or scope
- `LineBreakRequirementTests.EnumMemberWrapBeforeEqualsIsKeptAndWrapAfterEqualsIsJoined` is an existing test that asserts `A⏎    = 1,` is kept. The clarified FMT-BREAK-016 now demands `A = 1,`, and so does the spec example the new test `SpecificationExampleIsLaidOutAsAssignments` covers. Route to the test implementer.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1391

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/ArgumentAlignmentContributor.cs` | `AlignToOpenToken` no longer returns early for an empty list. A closing token that starts a line goes to the column after the opener. The element, separator, and continuation loops are no-ops for an empty list. Summary updated | FMT-INDENT-012 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/BaseTypeListContributor.cs` | Calls `ListElementAligner.AlignContinuations(baseList.Types, model)` for every base list, single type and enum underlying type included. It runs after the later-type alignment (which still needs two or more types), so a line inside a later type reads that type's final column | FMT-INDENT-013, FMT-INDENT-016, Q1 |

### Policy owners
`rg -n "AlignContinuations\(" src/Reihitsu.Formatter` → the shared `ListElementAligner.AlignContinuations`, which the other list contributors already use. I added no new alignment code.

### Tests run
`BaseListContinuationAlignmentTests`, `EmptyWrappedListClosingDelimiterTests`, `RawStringOtherLineTerminatorAlignmentTests`, `ZeroWidthTokenLineStartTests.OmittedTypeArgumentBehindCommentTakesBlockColumn`: all pass.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#952

No production change: the regression tests already passed (decision 11). `DirectiveBehindDisabledTextTests` still pass after the change.

## thoenissen/Reihitsu#1611

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | The private spaces-only `GetLeadingSpaceCount` (public static before, no other caller) is removed. The closing-marker width and the content-line indentation now come from Core `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (decision 3), through a private `TextLine` wrapper. `ReplaceIndentation` now moves a line instead of rewriting its indentation as spaces: a right move inserts spaces in front, and a left move removes that many leading characters, at most all of them. Every other whitespace character stays. `GetIndentationLength` loses its unused `text` parameter. The summaries of `ApplyEdits`, `GetIndentationLength`, and `ReplaceIndentation` are updated. Hole-line whitespace trivia is still rewritten as a whole, as before | FMT-RAW-001, FMT-RAW-002, FMT-RAW-003, change doc Q2 |

### Policy owners
- `rg -n "GetLeadingWhitespaceLength\(" --type cs src` → the Core owner, called by the RH5202 analyzer (2 sites), the RH5202 fix (4 sites), and now the formatter (1 wrapper). No private counter remains.
- The **shift application** has two implementations: the RH5202 fix (`RH5202…CodeFixProvider.cs:184`, `Math.Min(-delta, GetLeadingWhitespaceLength(...))` characters removed, spaces inserted) and `RawStringLineShifts.ReplaceIndentation`. They agree on every cell of the tests. Decision 3 covered only the measure, so I did not merge them; see *Follow-up candidates*.

### Tests run
`RawStringClosingWhitespaceMeasureTests`: all pass. Analyzer `RH5202` formatter tests: pass (inside the 199 above).

### Disputed tests or scope
- `RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterKeepsTheClosingLineTabOfAnInterpolatedRawStringAndItIsNotReported` (analyzer project) is an existing test that expects `$"""` at column 15 with the closing line at 16 spaces + tab, which was aligned under the old spaces-only measure. Under Q2 the tab counts as one column, so the closing marker stands at column 17 and moves left by one: 15 spaces + tab. RH5202 reports the old expected value by its own measure. The expected value contradicts the changed FMT-RAW-001 (change doc, visible output change 1). Route to the test implementer.

### Architecture findings
_None new._ Decision 3 is carried out for the measure.

### Follow-up candidates
- The leftward and rightward shift of a raw string's lines is implemented twice, in the RH5202 fix and in `RawStringLineShifts.ReplaceIndentation`. A shared Core helper ("move a line of a raw string by n columns") would make it one statement. Structural cause: one rule stated in two places.

## Whole run

### Commands
```shell
scripts/build.sh --configuration Debug
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~LoneOperatorJoinBlankLineTests|FullyQualifiedName~IndexerParameterListLayoutTests|FullyQualifiedName~CompoundAssignmentLayoutTests|FullyQualifiedName~EnumMemberValueLayoutTests|FullyQualifiedName~CollectionExpressionClosingBracketCommentTests|FullyQualifiedName~BaseListContinuationAlignmentTests|FullyQualifiedName~EmptyWrappedListClosingDelimiterTests|FullyQualifiedName~RawStringOtherLineTerminatorAlignmentTests|FullyQualifiedName~DirectiveBehindDisabledTextTests|FullyQualifiedName~RawStringClosingWhitespaceMeasureTests|FullyQualifiedName~OmittedTypeArgumentBehindCommentTakesBlockColumn"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5111|FullyQualifiedName~RH5202"
scripts/test.sh --configuration Debug --project <formatter|analyzer|cli|architecture|core|tooling>
```

### Results (at 317a6a5)
- Focused formatter filter: `Failed: 3, Passed: 95, Total: 98` (the 3 disputed #1243 tests).
- Focused analyzer filter: `Failed: 0, Passed: 199, Total: 199`.
- Formatter project: `Failed: 17, Passed: 4673, Total: 4690`. These are the 3 disputed new tests plus the 14 existing tests listed above (4 indexer-layout, 7 + 2 collection-bracket comment column, 1 enum member). On `d0e2102` production code, the 14 existing tests pass and only the 3 disputed tests fail.
- Analyzer project: `Failed: 1, Passed: 7349` (the RH5601 tab test above).
- CLI 251/251, architecture 115/115, core 660/660, tooling 126/126 passed.
- `scripts/format.sh` ran over the 10 changed production files (it reformatted one XML doc comment in `LineBreakListRewriter.cs`).

### Scope
10 production files, all in the components the handovers named (`LineBreakTriviaUtilities`, binary and ternary rewriters, `LineBreakListRewriter`, `LineBreakAssignmentRewriter`, `CommentIndentationContributor`, `ArgumentAlignmentContributor`, `BaseTypeListContributor`, `RawStringLineShifts`). The one addition is `DeclarationBraceLineBreakRewriter`, where the indexer `[` join sits next to the method `(` join. No new public member and no new dependency. `CollapseOwnedOpenParenToDeclarationLine` (internal) now takes `BaseParameterListSyntax`, and its existing callers are unchanged.
