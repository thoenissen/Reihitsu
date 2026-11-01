Role: implementer
Commit: see the commit that adds this file (subject "Implementer: one comment-position classification, chain-link break predicate, shared trivia helpers [skip ci]")
Verdict: test disputed

Repair round 3, the last, of thoenissen/Reihitsu#1496, lane behavior change. Inputs: `00-lead.md` (rows 30–35, the maintainer's guidance), `19-implementer.md`, `24-auditor.md` findings 6–9, `25-spec-author.md`, `26-test-implementer.md`, and FMT-BLANK-011/018/020 on 96d0384. The prompt named the work items: the classification of row 30, the chain-link case of row 35, and auditor #9. It carried no suspected cause and no hoped-for verdict. I changed no test assertion, input, or expected output.

Result: formatter 3710 passed, 1 failed (D1). Analyzer 6349 passed, 2 failed (D2, the LF and CRLF variants of one existing test). Core 544/544, CLI 251/251, architecture 92/92. Debug build: 0 warnings. `scripts/format.sh --check src`: 0 of 1787 files need formatting.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Utilities/BraceBlankLinePolicy.cs` | New `FindTokenLineStartIndex(token, context)` is the one classification of comment positions. It returns the index of the first content on the token's line. Every comment from that index on is on the token's line, and every comment in front of it is on its own line. A line break inside a delimited comment's text does not end a line. The method returns `-1` (every comment on its own line) in two cases: for a token without text, and for a token the chain layout moves off a multi-line `*/`. `FindGluedCommentIndex` now reads it. It also lost the `ContainsLineTerminator` condition, so a comment that starts on an earlier line and ends on the token's line is glued (row 30, which reverses the first reading of row 28). It takes the context as a third parameter. `FindFirstContentIndexOnLine` and `IsOrdinaryComment` moved out (#9). The summaries are updated. | FMT-BLANK-018 |
| `.../BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | The FMT-BLANK-020 trigger reads the classification (`FindTokenLineStartIndex(token, context) >= 0` bails) instead of its own scan of the token's line. Both removal methods take the context. The summaries are updated, including the auditor's hint on the `token` parameter. | FMT-BLANK-018, FMT-BLANK-020 |
| `.../BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | FMT-BLANK-011's skip passes the context to `FindGluedCommentIndex`. | FMT-BLANK-011 |
| `.../LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | New private `GetWrappedLayoutLineBreaks(chain)` is now the one per-link decision of the wrapped layout: whether the line in front of each link is broken. `ApplyWrappedLayout` applies it; this is the same decision as before, moved and not changed. New `internal static BreaksLineBehindMultiLineCommentTail(token, boundary)` is the row-35 predicate. It returns true when the token stands behind a multi-line comment's tail (the existing `IsBehindMultiLineCommentTail`), its gap holds a line break, and `GetWrappedLayoutLineBreaks` breaks in front of its link. It reads the token's chain from the syntax tree: the line breaks in front of and inside link operators, and the trivia in front of the first link. The blank-line phase changes none of these. `TryCloseGap`'s return value is no longer read, so it became `void CloseGap` (S3241). `NormalizeChain` no longer builds its own `wrappedLinks` array. | FMT-BREAK-019, FMT-BLANK-018 (row 35) |
| `.../Core/Utilities/TriviaListUtilities.cs` | Now holds `FindFirstContentIndexOnLine` and `IsOrdinaryComment`. Both phases read them (#9). | — |
| `.../LineBreaks/Utilities/TokenGapNormalizer.cs` | Its private `IsOrdinaryComment` and `FindLineStartIndex` are removed. It calls `TriviaListUtilities.IsOrdinaryComment` and `TriviaListUtilities.FindFirstContentIndexOnLine(trivia, lastContentIndex + 1)`, which equals the old `FindLineStartIndex(trivia, lastContentIndex)` for a content index. | — |

Net production diff: +225 / −174 in six files. Outside `BraceBlankLinePolicy`, the only new member visible beyond its class is the internal `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail`, which row 35 asks for.

## Policy owners

- Comment position on the token's line: `git grep -n "FindTokenLineStartIndex(" -- 'src/*.cs' ':!src/*Test*'` finds one definition in `BraceBlankLinePolicy`. Two places read it: `FindGluedCommentIndex`, and the FMT-BLANK-020 trigger in `BlankLineTokenCleanupRewriter`. `FindGluedCommentIndex` in turn is read by FMT-BLANK-011 (`BlankLineTriviaBoundaryRewriter`) and FMT-BLANK-018 (`BlankLineTokenCleanupRewriter`).
- Chain break behind a multi-line comment tail: `git grep -n "IsBehindMultiLineCommentTail(\|BreaksLineBehindMultiLineCommentTail(\|GetWrappedLayoutLineBreaks("` finds every match in `ChainLineBreakRewriter`. The only reader outside it is `BraceBlankLinePolicy.FindTokenLineStartIndex`. No other rewriter calls `IsBehindMultiLineCommentTail`.
- Trivia helpers: `git grep -n "static bool IsOrdinaryComment\|static int FindFirstContentIndexOnLine\|static int FindLineStartIndex" -- 'src/*.cs'` finds:
  - `TriviaListUtilities`, the formatter's one copy;
  - `CommentPositionUtilities.IsOrdinaryComment`, in the analyzer assembly and older than this run;
  - `BlankLineTriviaBoundaryRewriter.FindLineStartIndex`, which is a different helper: it walks back over whitespace to the comment's indentation, not to a line break. I left it alone.

## Tests run

```shell
scripts/build.sh --configuration Debug                        # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project formatter     # Failed: 1, Passed: 3710, Total: 3711
scripts/test.sh --configuration Debug --project analyzer      # Failed: 2, Passed: 6349, Total: 6351
scripts/test.sh --configuration Debug --project core          # Passed: 544
scripts/test.sh --configuration Debug --project cli           # Passed: 251
scripts/test.sh --configuration Debug --project architecture  # Passed: 92
scripts/format.sh --check src                                 # 0 of 1787 file(s) need formatting
```

Of the 32 tests in `26-test-implementer.md` that failed before, all now pass. So do the chain-link tests (`ChainLinkBehindMultiLineCommentTests` and the three new chain tests) and the not-glued tests.

## Disputed tests or scope

### D1 — `MultiLineGluedCommentBlankLineTests.InsertsBlankLineAfterClosingBraceAboveMultiLineCommentGluedToStatementOrMember`, member half

The input is a method `}` followed by the member `/* x⏎ */ int I;`. The test expects a blank line above `/* x`. The statement half (`}⏎/* z⏎ */ int i = 0;`, SHARED-BLANK-003) passes.

Under FMT-BLANK-018, `/* x` is glued to `int`, the first token of a member declaration. The spec's third bullet then applies: "above any other token — the first token of a member declaration … — no requirement decides the blank lines directly before the token, so none is inserted and the author's are kept". SHARED-BLANK-003 covers statements only ("followed by another statement in the same block or switch section"). The formatter has no member-after-`}` rule: probed on this branch, `}⏎int I;` with no comment stays without a blank line, and so does `}⏎/* x */ int I;`. Before this round, the expected blank line came from FMT-BLANK-011 under the old reading, which did not glue a multi-line comment.

The test should expect the input unchanged above `/* x`. That is change-doc entry 20 ("a comment glued to … a member: none inserted, the author's kept"), which the guidance covers.

The spec author's example 4 in `25-spec-author.md` and the change doc's sentence near line 1137 ("a member or statement after `}` (SHARED-BLANK-003) — it goes above the comment") need the same correction: SHARED-BLANK-003 has no member case. Route the test to the test implementer and the sentence to the spec author. The only way to make the test pass as written is a member-after-`}` blank-line rule that no requirement states, so I did not add one.

### D2 — `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` (LF and CRLF)

The input is `while (a)⏎    /* note⏎    */ if (b)`, an embedded `if` behind a multi-line comment. `EmbeddedIfBehindMultiLineCommentFixedData` expects a blank line between `while (a)` and `/* note`. The comment ends on the line of `if`, so under row 30 it is glued to the statement. FMT-BLANK-018's second bullet then applies: the statement requirements decide, and none inserts a blank line in front of an embedded statement, so the author's lines are kept and there were none. Before this round, the blank line came from FMT-BLANK-011, because the comment was not glued.

The fix's actual output differs from the expected output only by that one line. The columns and the braces match, and the test asserts the columns (RH5204-006). The expected output should drop line 52 of the test file (the blank line under `while (a)`). This is change-doc entry 20 (a comment glued to a statement). Route to the test implementer. The existing-test scan in `25-spec-author.md` did not list this file.

## Architecture findings

_None._

Row 30's structural change is in place. One function classifies every comment of the gap into exactly one of two positions, and FMT-BLANK-011, -018, and -020 read it.

Row 35's condition holds. The chain-link break is one predicate, owned in `ChainLineBreakRewriter`, and computed from the same per-link decision that the rewriter applies. The blank-line phase reads it and keeps no copy. The decision reads the chain's link line breaks and the trivia in front of its first link. The phases between the blank-line phase and the chain rewriter do not change those for the shapes in the tests.

One residual I checked: line-break rewriters that run before the chain rewriter inside the line-break phase could, in principle, join a chain's wrap. Probed with a chain inside an argument list (`F(a.B()⏎.C()⏎/* c⏎ */ .D(), 1)`): the output is stable in two passes and matches the top-level chain. I found no input where they disagree.

## Follow-up candidates

_None new._
