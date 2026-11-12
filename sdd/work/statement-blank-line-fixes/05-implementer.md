Role: implementer
Commit: see `git log -1 -- sdd/work/statement-blank-line-fixes/05-implementer.md` (the commit that adds this file)
Verdict: #946: tests pass; #972: tests pass; #1096: tests pass (one formatter test disputed, see #1076); #1076: test disputed (5 formatter-parity tests); #1626: tests pass; #1625: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond what `00-lead.md` decides (decisions #8, #9, #10, #11, #13, answer 3A). Built and tested in Debug. No test file was changed.

Of the 94 tests 04-test-implementer handed over as failing, 89 pass now. The other 5 are formatter-parity tests (`VerifyFormatter(source)`) whose analyzer half passes now but whose input the formatter does not leave unchanged; see *Disputed tests or scope* under #1076.

**Owner of the blank line in front of a statement (#1626):** `Reihitsu.Analyzer.CodeFixes.Core.BlankLineCodeFixUtilities.InsertBlankLineBefore(SyntaxNode root, SyntaxToken token, bool behindLeadingDirectives)` in `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs`. It is an existing type, not a new one. The *Code owners* rows can name `BlankLineCodeFixUtilities` for SHARED-STMT-004, SHARED-AFTERBRACE-001 and SHARED-AFTERBRACE-002. `StatementShouldBePrecededByABlankLineCodeFixProviderBase` keeps only the choice of the token: the statement's first token.

## thoenissen/Reihitsu#946

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` | `ApplyCodeFixAsync` derives its token from the statement (`FirstAncestorOrSelf<StatementSyntax>().GetFirstToken()`), not from the diagnostic location. For `await using`/`await foreach` that is `await`, the token whose gap the analyzer measures. The diagnostic location is unchanged. | RH5005-002, RH5006-002, SHARED-STMT-004 |

### Policy owners
`rg -n "GetPreviousToken|GetLocation" src/Reihitsu.Analyzer/Rules/Layout/RH50*.cs`: every rule on the base measures the gap in front of its statement's first token. Now the fix inserts there too, so analyzer and fix agree for all 20 rules without a per-rule override.

### Disputed tests or scope
_None._

### Architecture findings
_None_. The skeptic's small finding (analyzer and fix each choose the token) is resolved: the fix derives the token from the statement.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#972

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `InsertBlankLineBefore` rebuilds the gap (two line breaks plus indentation) only when no line break outside a comment separates the statement from the token before it (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine`). Before, the base rebuilt whenever the gap's last line held content (`TokenGapAnalysis.HasTerminalLineBreak == false`), so a comment that starts the statement's line got two blank lines. Otherwise it inserts one line break at the start of the leading trivia, which puts the blank line above the line where the comment starts. | SHARED-STMT-004 (bullets 2 and 3) |

### Policy owners
`rg -n "AreSeparatedByEndOfLine" src -g '!*Test*'` names the same predicate RH5030's `CanRegisterCodeFix` uses for RH5030-003. Insert and withhold therefore now answer "is the statement on a later line" in one way.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1096

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs` | Blank lines are counted with `TokenGapAnalysis.Between(previousToken, firstToken).BlankLineCount`, not `SyntaxTriviaUtilities.ContainsBlankLine` (decision #8). | SHARED-BLANK-001 |
| `src/Reihitsu.Analyzer/Base/StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs` | Same, with `TokenGapAnalysis.Between(statement.GetLastToken(), nextToken)` (decision #8). | SHARED-BLANK-001, RH5011-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `GetIndentation` moved here from the base. New `GetSwitchSectionIndentation` / `IsWithinLabelContinuation` (decisions #9, #13). Inside an initializer, when the anchor line starts behind a label's first token, on a continuation line of a multi-line label, the column is read from another statement of the section that starts its own line (the section's `break;`). Without such a statement, the continuation line's whitespace is used. Every other anchor line still goes through `SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation`, unchanged. | SHARED-STMT-004 (column inside an initializer, answer 2A) |

### Policy owners
- Blank-line measure: `rg -n "ContainsBlankLine\(" src -g '*.cs' -g '!*Test*'` finds only the definition `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:315`, with no production caller. `rg -n "TokenGapAnalysis.Between" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` finds the two analyzer bases. The formatter's `BlankLineEditor` already used `TokenGapAnalysis`. SHARED-BLANK-001's Why (decision #14) stays correct; no revert is needed.
- Label compensation: `rg -n "ComputeAnchorDerivedIndentation" src -g '!*Test*'` finds the Core helper, `BlankLineCodeFixUtilities` and `RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider`. I left the shared Core helper unchanged, so RH5103 keeps its documented over-indent; that is the follow-up already in `00-lead.md`. The continuation check is in the code-fix owner only.

### Disputed tests or scope
- `RH5002…FormatterTests.VerifyFormatterKeepsBlankLineBelowEndIfDirectiveAndAnalyzerReportsNothing` passes.
- `SyntaxTriviaUtilities.ContainsBlankLine` has no production caller any more. I kept it because `src/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs:420-468` tests it, and removing those tests is not mine to do. Recommendation: the cleaner deletes the method and its five tests, so the second measure cannot come back (FC-015).

### Architecture findings
_None new_. Decision #8 is done within budget (about 15 production lines in the two bases). Decision #9's structural cause (the fix predicts a column) remains the follow-up already recorded.

### Follow-up candidates
_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#1076

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/WhitespaceSpanRemovalCodeFixProviderBase.cs` | New `private protected virtual GetRemovalSpan(SourceText, TextSpan)` hook. By default it returns the reported span. `ApplyCodeFixAsync` is an instance method now. | SHARED-WS-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/BlankLineSpanRemovalCodeFixProviderBase.cs` | New constructor parameter `removesRun`. When it is set, the removed span widens to every blank line directly above and below the reported one that ends with a line break. | RH5022-004, RH5024-005, RH5025-004, RH5026-004, RH5027-004, SHARED-FIX-001 |
| `RH5022`/`RH5024`/`RH5025`/`RH5026`/`RH5027` code-fix providers | Pass `removesRun: true`. | same |
| `RH5023`/`RH5028` code-fix providers | Pass `removesRun: false`, so the behavior is unchanged: RH5023 still deletes one line per diagnostic, as its existing `OneApplicationPerDiagnosticCorrectsTheRun*` tests require. | RH5023-004 |
| `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` | `EnumerateConsecutiveBlankLines` (only caller: RH5023) skips a line without a line break. The end of the file is not a line. | RH5023-001 |
| `src/Reihitsu.Analyzer/Base/StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs` | `ContinuesEnclosingStatement`: a next token `else`, or `while` whose parent is a `DoStatementSyntax`, is not reported. RH5011 is the only rule on this base. | RH5011-008 |
| `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs` | New shared cheap check `FollowsRegionDirectiveRequiringBlankLine(root, sourceText, targetLineIndex)` (decision #10). The nearest non-blank line above the target starts with an active `#region`, or with an active `#endregion` when the target line does not begin with `}`. | SHARED-BLANK-006, RH5024-001, RH5025-007, RH5026-005, RH5027-005 |
| RH5024, RH5025, RH5026 analyzers; RH5027 analyzer | Skip a blank line for which the check holds. | same |

### Policy owners
- `rg -n "FollowsRegionDirectiveRequiringBlankLine|IsMissingRequiredBlankLineAfter" src -g '!*Test*'`: both live in `RegionDirectiveBlankLineUtilities`. RH5032 (`IsMissingRequiredBlankLineAfter`) builds its `#endregion`-before-`}` exemption from token line indices (`closeBraceStartLineIndices`); the new check reads the target line's first character. That is two statements of SHARED-BLANK-006's exemption in one class. They agree on every input where the target line begins with a token, which is the only kind the four rules pass. Unifying them would mean giving the new check a token-index set. That is cheap, but I did not do it: no test needs it, and the four analyzers do not compute that set today.
- `rg -n "removesRun" src`: the five deleting rules and the two per-line rules.

### Disputed tests or scope
Five formatter-parity tests fail, and I cannot fix them without changing the formatter, which is outside this run. Each calls `VerifyFormatter(source)`, which asserts first that the analyzer reports nothing (passes now) and then that the full formatter pipeline returns `source` unchanged (line 148 of `FormatterTestsBase{TAnalyzer}.cs`). For each input, the formatter changes it, for reasons unrelated to the rule under test:

| Test | Formatter change (actual vs expected) | Cause |
|---|---|---|
| `RH5011…FormatterTests.VerifyFormatterKeepsBreakDirectlyAboveElseAndAnalyzerReportsNothing` | `if (a > 1)⏎    break;⏎else⏎    a--;` becomes braced (`{ break; }` / `{ a--; }`) | the formatter braces an `if`/`else` chain whose bodies are unbraced |
| `RH5024…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | inserts a blank line between `    }` and `    #endregion // R` | SHARED-BLANK-005: a blank line above a region directive |
| `RH5025…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | re-anchors the dictionary initializer to the `new()` column, adds blank lines above `#region A`/`#endregion // A`, drops the trailing comma | initializer alignment, SHARED-BLANK-005, trailing comma |
| `RH5026…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | blank line above `#region R` (after `}`) and above `#endregion // R` | SHARED-BLANK-005 |
| `RH5027…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | blank line above `#region R` and above `#endregion // R` | SHARED-BLANK-005 |

What each test is for (FC-013: the formatter keeps the blank line below the region directive or `break` above `else`, and the analyzer reports nothing) holds in every actual output above: the blank line below `#region`, and the `break` directly above `else` (now inside a block), are kept. Only the inputs are not formatter-stable. Recommendation: the test implementer makes each input formatter-stable, or passes the formatted text as the expected output (`VerifyFormatter(source, fixedSource)`). I changed no assertion.

### Architecture findings
_None new_ (decision #10 implemented as one shared check).

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1626

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | Now the one owner: the separated case inserts one line break, and the same-line case rebuilds the gap: it trims the previous line's trailing whitespace, inserts two line breaks and the indentation, and keeps the rest of the leading trivia. `GetIndentation`, `CreateBlankLineLeadingTrivia`, `TrimTrailingWhitespace` and `GetPrecedingSiblingStatement` moved here from the base. | SHARED-STMT-004, SHARED-AFTERBRACE-001, SHARED-AFTERBRACE-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` | Delegates to the owner with `behindLeadingDirectives: false`. | SHARED-STMT-004 |
| `RH3208…CodeFixProvider.cs`, `RH5030…CodeFixProvider.cs` | Delegate with `behindLeadingDirectives: true`. RH5030's `CanRegisterCodeFix` still withholds when no line break outside a comment separates the tokens, so RH5030 reaches only the insert path (RH5030-003 unchanged). | SHARED-AFTERBRACE-001, RH5030-003 |

### Policy owners
`rg -n "InsertBlankLineBefore|ReplaceFormattingOnlyGap|CreateBlankLineLeadingTrivia|InsertLeadingLineBreak" src -g '!*Test*'` finds only `BlankLineCodeFixUtilities` and its three callers. The formatter's `BlankLineEditor.InsertBlankLineBeforeToken` is the formatter's own phase and not a code-fix owner.

### Disputed tests or scope
- **`behindLeadingDirectives` keeps two placements.** In the separated case, the base inserts at trivia index 0, so the blank line goes above a directive in the gap. SHARED-AFTERBRACE-001 puts it behind the last directive. Making them one placement would change the base's output for `a++;⏎#if X⏎#endif⏎// note⏎if (…)` (reported, since the last trivia is a comment): today the fix puts the blank line above `#if X`, and with the AFTERBRACE placement it would go between `#endif` and `// note`. That is a visible change no spec decides, so I kept both behind the flag. SHARED-STMT-004's first bullet ("above the first of [the comments]") reads closer to the AFTERBRACE placement. This is a candidate for the lead, possibly an output question; see *Follow-up candidates*.
- RH5011's fix stays a third owner of the same gap (decision #11, follow-up).

### Architecture findings
_None new_. The structural change of #1626 adds no type. It moves about 150 lines from the base into the existing `BlankLineCodeFixUtilities` and adds about 25 (the separated/rebuild branch and the directive flag). The #1096 narrow fix adds about 45 more in the same file. Escalation 9 is not reached.

### Follow-up candidates
- **Placement of the statement blank line when a directive is in the gap.** The statement blank-line base puts the blank line above a directive that stands above the statement's comments (`a++;⏎#endif⏎// note⏎if`). This lands inside the `#if` block, which SHARED-AFTERBRACE-001's Why rejects for RH5030/RH3208. To unify, the flag would be dropped and `FindIndexAfterLeadingDirectives` used for all callers. That is a visible output change for the base rules (RH5001–RH5019, RH5021, RH5029).

## thoenissen/Reihitsu#1625

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | Through #1626's owner: for `} /* a⏎    b */ L();` no line break outside a comment separates `}` and `L`, so the gap is rebuilt. The trailing space behind `b */` is removed, a blank line follows, and `L();` gets its nesting column. | SHARED-AFTERBRACE-002, RH3208-002 (answer 3A) |

### Policy owners
See #1626.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## Whole run

### Production diff size
`git diff --shortstat -- src ':(exclude,glob)src/*Test*/**'` against `212a268` (before this commit): `21 files changed, 405 insertions(+), 226 deletions(-)`. About 150 of the insertions in `BlankLineCodeFixUtilities.cs` were moved from `StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` (183 deletions there). No new type. New members:
- `BlankLineCodeFixUtilities.InsertBlankLineBefore(root, token, behindLeadingDirectives)`, an internal signature change;
- `WhitespaceSpanRemovalCodeFixProviderBase.GetRemovalSpan`, private protected virtual;
- the `BlankLineSpanRemovalCodeFixProviderBase` constructor parameter `removesRun`;
- `RegionDirectiveBlankLineUtilities.FollowsRegionDirectiveRequiringBlankLine`, a public Core member;
- the private `ContinuesEnclosingStatement`, `GetSwitchSectionIndentation` and `IsWithinLabelContinuation`.

### Deleted special cases
_N/A._ No shape declared unsupported.

### Comments (FC-031)
- The stale `GetIndentation` summary ("documented limitation") went with the move. Its replacement in `BlankLineCodeFixUtilities` describes the continuation-line case as implemented.
- The summaries of `ApplyCodeFixAsync` (base), both analyzer bases' `OnStatement`, `EnumerateConsecutiveBlankLines`, the RH5024–RH5027 analyzer methods, and `WhitespaceSpanRemovalCodeFixProviderBase.ApplyCodeFixAsync` are updated.
- `SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion`'s remark about the RH5103 limitation stays true for RH5103, its remaining unguarded caller.

### Tests run
```shell
scripts/build.sh --configuration Debug                       # Build succeeded, 0 warnings
scripts/format.sh <21 changed production files>              # 1 file reformatted, then stable
scripts/test.sh --configuration Debug --project analyzer --filter "<the 23 classes changed in cfdf0b1>"
#   Failed: 5, Passed: 400, Total: 405   (the 5 disputed formatter-parity tests)
scripts/test.sh --configuration Debug --project analyzer
#   Failed: 5, Passed: 8206, Total: 8211 (same 5)
scripts/test.sh --configuration Debug --project core          # Passed: 660
scripts/test.sh --configuration Debug --project architecture  # Passed: 117
scripts/test.sh --configuration Debug --project formatter     # Passed: 4837
```
The baseline on `212a268` with the same 23-class filter had 94 failures.
