Role: implementer
Commit: see the commit that adds this file (subject "Implementer: one glued-comment predicate, FMT-BLANK-020 by syntax and widened trigger, no comment pull behind = [skip ci]")
Verdict: test disputed

Repair round 2 after the audit, issue thoenissen/Reihitsu#1496, lane behavior change. Inputs: `00-lead.md` (maintainer answers and clarified guidance, rows 17–26), `05-implementer.md`, `14-auditor.md` findings 1, 2, 4, 5 and the architecture finding, `15`–`18`, the spec and the change doc on 9bbdc43. The prompt named the round's work items, no suspected cause and no hoped-for verdict. No test assertion, input or expected output was changed.

Result: formatter 3663 passed, 1 failed (D1 below); analyzer 6351/6351; core 544/544; CLI 251/251; architecture 91 passed, 1 failed (D2 below, a spec file). Debug build: 0 warnings. `scripts/format.sh --check src`: 0 of 1785 files need formatting.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` | Owns three predicates now. `RemovesBlankLinesDirectlyBefore` is the FMT-BLANK-007/008/019 token set: every brace except the `{` of a block statement after a statement, `else`/`catch`/`finally`, the do-statement `while`, and the FMT-BLANK-019 closers. `RemovesBlankLinesBehindOwnLineComment` is the FMT-BLANK-020 set, decided by syntax alone: every complex-element `{` is in it, and the `SpansMultipleLines` layout read is gone (row 17, finding 1, answer 1A). `FindGluedCommentIndex` is the one FMT-BLANK-018 glued predicate (finding 4): the first content on the token's line is an ordinary comment, only whitespace and comments (ordinary or documentation) follow it up to the token, none of them holds a line break, the comment starts its line, and the token has text. `FindFirstContentIndexOnLine` and `IsOrdinaryComment` moved here from the cleanup rewriter. `KeepsGluedCommentWithoutBlankLine` is removed. The class summary is updated. | FMT-BLANK-017–020 |
| `.../BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | The FMT-BLANK-007/008/009/019 branch reads `RemovesBlankLinesDirectlyBefore`. The FMT-BLANK-018 removal now runs for every token in that set and reads `FindGluedCommentIndex`. The FMT-BLANK-020 removal runs for the FMT-BLANK-020 set, and its trigger is widened (finding 2): the last content is an ordinary comment that ends its line, and the first content of that comment's line starts a line. What stands in front of the comment on its line no longer matters. Private copies `FindFirstContentIndexOnTokenLine`, `HoldsOnlyOrdinaryCommentsFrom` and `IsOrdinaryComment` are removed. The summaries of both removal methods are updated. | FMT-BLANK-018, FMT-BLANK-020 |
| `.../BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | FMT-BLANK-011 inserts no blank line when the first comment is the glued one (`FindGluedCommentIndex`), whatever the token is. The token check, the `skipsGluedCommentBoundary` parameter and the private copy `IsOrdinaryCommentGluedToToken` are removed (finding 4). The remarks are updated. | FMT-BLANK-011, FMT-BLANK-018 |
| `.../LineBreaks/Rewriter/LineBreakInitializerRewriter.cs` | The `{` of an assigned collection initializer is no longer pulled up (its leading line breaks and whitespace stripped) when its leading trivia holds a comment, directive or disabled text (`SyntaxTriviaUtilities.ContainsUnjoinableTrivia`). That strip removed the FMT-BLANK-011 blank line above `// c` behind `L =`, which was the phase reversal of row 24. | FMT-BREAK-030, FMT-BLANK-011 (row 24, entry 13) |
| `src/Reihitsu.Formatter/Data/Enumerations/RootConstruct.cs`, `Data/FormattingContext.cs` | The summaries name the blank-line phase as a reader of `AssignedCollectionInitializer` (finding 5). | FC-031 |

The three FMT-SWITCH-007 tests (rows 25, entry 19) pass with no switch-phase change. The blank-line phase no longer inserts the line above `/* b */ break;`, because that comment is glued, and the switch-case brace phase carries the leading trivia of the `break` as it finds it.

One behavior difference beyond the tests: the cleanup branch used to apply FMT-BLANK-008 to every `while` keyword. It now applies only to the do-statement `while`, as FMT-BLANK-008 says. All test projects pass, and on 9bbdc43 a blank line in front of a `while` statement was already kept in the output (probed: `int a = 0;⏎⏎while (x)` was unchanged).

## Policy owners

- `rg -n "FindGluedCommentIndex|IsOrdinaryCommentGluedToToken|HoldsOnlyOrdinaryCommentsFrom|FindFirstContentIndexOnTokenLine|KeepsGluedCommentWithoutBlankLine" src --glob '*.cs' --glob '!*Test*'` finds only `BraceBlankLinePolicy.FindGluedCommentIndex`, which both rewriters read. The old copies are gone.
- `rg -n "RemovesBlankLinesDirectlyBefore|RemovesBlankLinesBehindOwnLineComment" src --glob '!*Test*'` finds `BraceBlankLinePolicy` only, read by the cleanup rewriter.
- `rg -n "GetLineSpan|SpansMultipleLines" src/Reihitsu.Formatter/Pipeline/BlankLines` finds nothing. No blank-line decision reads a layout now (row 17). The one fact taken from the line-break side is `LineBreakInitializerRewriter.IsAssignedCollectionInitializer`, which is a syntax fact (parent kind or root flag).
- `rg -n "static bool IsOrdinaryComment" --glob '*.cs'` finds three methods. One is `BraceBlankLinePolicy`. Two pre-date the run: `TokenGapNormalizer` (exists on `main`) and the analyzer's `CommentPositionUtilities`, which sits in another assembly. This is a two-line kind check; I did not merge them.

## Tests run

```shell
scripts/build.sh --configuration Debug                     # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project formatter  # Failed: 1, Passed: 3663, Total: 3664
scripts/test.sh --configuration Debug --project analyzer   # Passed: 6351
scripts/test.sh --configuration Debug --project core       # Passed: 544
scripts/test.sh --configuration Debug --project cli        # Passed: 251
scripts/test.sh --configuration Debug --project architecture  # Failed: 1, Passed: 91, Total: 92
scripts/format.sh --check src                              # 0 of 1785 file(s) need formatting
```

## Disputed tests or scope

### D1 — `RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringArgumentOnCommentLedLine` (test wrong under FMT-BLANK-018)

Input: `var s = Call(1,⏎    /* c */ """⏎a⏎""".Trim()⏎.ToUpper());`. The expected output has a blank line between `Call(1,` and `/* c */ """`. `/* c */` starts its line and shares it with the raw-string token, the first token of an argument. Under FMT-BLANK-018 it is glued, and FMT-BLANK-018's third bullet applies: "above any other token … none is inserted and the author's are kept". The input has no blank line, so the output should have none. That blank line came from FMT-BLANK-011 on `main`. The test is tagged FMT-INDENT-030 and asserts raw-string alignment; the blank line is incidental. The test implementer's scan (16) matched lines that start with a delimited comment followed by code, and it likely missed this one because the code there is `"""`. Everything else in the actual output equals the expected output. Route to the test implementer: remove the blank line from the expected output. This is change-doc entry 17 (a comment glued to an argument), so no new visible change.

### D2 — architecture test `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` (spec file)

`formatter/switch-case-braces.md: FMT-BLANK-018 is referenced but missing from Uses`. FMT-SWITCH-007's new sentence (17-spec-author, e985d4d) names FMT-BLANK-018, and the spec's Uses row was not extended. This is independent of production code. It is the spec author's: add FMT-BLANK-018 to that row.

### Interpretations of FMT-BLANK-018 I had to make (spec author may want to state them)

- **A comment that starts on an earlier line and ends on the token's line is not glued.** Example: `/* x⏎ */ .D()`. It does not share the line it starts on with the token. This keeps `ChainLinkBehindMultiLineCommentTests.ReportedLinkBehindMultiLineCommentTailStartsItsOwnLine` (FMT-BLANK-011 blank line above `/* x`). The predicate also rejects a line break inside any comment between the glued comment and the token.
- **A token without text (the end-of-file token) has no glued comment.** Example: `}⏎// trailing comment` at the end of a file keeps its FMT-BLANK-011 blank line (`TrailingCommentWithNoBlankLineGainsOneAtEndOfFile`). The rule follows the line-start fact the line-break utilities already use: a token without text never starts a line.

### Scope

Production paths: the same files as round 1's blank-line components, plus a guard in `LineBreakInitializerRewriter` and two summaries. There is no new public member outside the formatter's internal `BraceBlankLinePolicy`. Net production diff: +159 / −161 lines.

## Architecture findings

_None._ The architecture finding of 14-auditor.md is repaired: the blank-line phase no longer copies the line-break phase's multi-line complex-element condition. The second owner from row 24 is also removed: the line-break phase's strip in front of an assigned collection initializer `{` deleted a blank line, and it no longer does when a comment stands there.

## Follow-up candidates

_None new._
