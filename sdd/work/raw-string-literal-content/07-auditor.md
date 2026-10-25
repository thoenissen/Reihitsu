Role: auditor
Commit: ea03170
Verdict: #951: findings; #1133: pass; #1146: findings

Inputs: thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146 and their comments (GitHub MCP), `00-lead.md` through `06-spec-checker.md`, the spec and documentation diff, `sdd/specs/shared/code-fixes.md` (read only), and `git diff origin/main...HEAD`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. I changed no file except this handover.

`→` marks a tab and `⏎` a line break. Every fixture below was written to the session scratchpad, outside the repository. `trace` means `bash scripts/trace.sh <fixture> --passes 3 --no-install`. The `origin/main` comparisons ran in a temporary worktree of `f3d514d`, which I removed afterwards.

## thoenissen/Reihitsu#951

### Scope

Full audit of head `ea03170` against base `f3d514d`. The production files are `RawStringAlignmentPhase`, `LayoutModel`, `LayoutComputer`, `CommentIndentationContributor`, and `IndentationRewriter`. The tests are `RawStringInterpolationHoleTests`, `FormatterTestsBase.AssertSecondPassIsStable`, and the RH5202 formatter-parity tests.

Every spec example and every skeptic input passes, and so do the shapes I added that earlier phases break: a switch expression, a block lambda with split statements, and an array initializer in a hole of a misaligned literal, plus a CRLF case with a comment and a wrapped argument. The trace is stable after pass 1 for all of them.

The defects below share one premise that does not hold. The indentation phase measures an aligned hole line, or a comment, against a column, and it assumes that column moves with the literal by the same amount as the line itself. The comment condition and condition 1 of FMT-RAW-007, together with FMT-INDENT-030, require that the line and its anchor end up together.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | high | `LayoutComputer.GetAdjustedColumn` (`LayoutComputer.cs:202`), consumed through `ArgumentAlignmentContributor` and every other contributor except the method chain; against `RawStringAlignmentPhase.ComputeLineShifts` (`RawStringAlignmentPhase.cs:99`) | FC-012, FC-016, FC-022, FC-027 | same requirement (FMT-RAW-007 condition 1, FMT-RAW-008); PR-introduced for (b) | An anchored hole line is placed against its anchor's column before the raw-string phase moves the lines. The phase then moves the anchor's line and the anchored line by different amounts. **(a)** The anchor is on the closing line of a nested raw literal, which moves by the outer and the inner delta, while the anchored line moves by the outer delta only. **(b)** The anchor is on a line that FMT-RAW-009 freezes, which does not move, while the anchored line moves by the outer delta. In both cases the output of pass 1 is not a fixed point (FMT-PIPE-004). Counterexamples are below the table. | Make the placement of an anchored hole line account for the raw-string shift of the anchor's line relative to its own line. `MethodChainAlignmentContributor.GetAdjustedColumn` already predicts this shift for one case. Better still, let both phases share one owner of the per-line shift (FC-015, see finding 5). Add tests for (a) and (b). |
| 2 | high | `LayoutModel.TryGetPlacedColumn` (`LayoutModel.cs:89`) and `RegisterRawStringHoleLine` (`:77`), read by `CommentIndentationContributor` and `LayoutComputer.GetAdjustedColumn` | FC-012, FC-016, FC-003 | PR-introduced | A hole line that no construct places keeps its whitespace, tab included. Its column is registered as a character count, in which a tab counts as 1. `CleanupPhase` then expands the tab to four spaces (FMT-CLEAN-002, which FMT-RAW-007 says still applies). That moves the line's token 3 columns right of where every comment and every anchor were measured. Counterexamples are below the table. Both inputs were stable on `origin/main`, where the indentation phase re-indented the tab-indented line. `TabInFrontOfHoleLineOfAlignedLiteralBecomesFourSpaces` has neither a comment nor an anchor (FC-003). | Measure the kept column after the tab replacement that will apply to it, or give a tab-indented hole line a layout. Add both counterexamples as tests. |
| 3 | high | `RawStringAlignmentPhase.StartsInsideNestedText` (`RawStringAlignmentPhase.cs:253`, the `SyntaxTriviaUtilities.IsCommentTrivia` test at `:259`); counterpart `IndentationRewriter.HandleNonWhitespaceTrivia` (`AlignContinuationExteriorMarkers`) | FC-001, FC-012 | same requirement (FMT-RAW-007 comment condition, FMT-PIPE-004); new mechanism | `IsCommentTrivia` includes `SingleLineDocumentationCommentTrivia`. So the raw-string phase freezes the second line of a two-line `///` comment in a hole as "inside a comment that began on an earlier line". At the same time, the indentation phase aligns that line's `///` marker with the first line (FMT-INDENT-008), and the raw-string phase moves the first line. Counterexample below the table. FMT-RAW-009 cites FMT-INDENT-007 (`/* */` following lines are kept), which does not cover `///` continuation markers. Those are layout under FMT-INDENT-008 and SHARED-FIX-003. `/** */` in a hole is stable (checked). | Exclude single-line documentation comments from the frozen-line test, so the predicate matches FMT-INDENT-007's span. If the spec author finds FMT-RAW-009's "inside a comment" ambiguous, narrow it to `/* */` and `/** */`. Add a test. |
| 4 | high | `GenericConstraintContributor.Contribute` (`GenericConstraintContributor.cs:34`, `model.TryGetLayout(parentLine, …)`); `ConstructorInitializerContributor.cs:33` reads the same way but cannot reach a hole | FC-015, FC-016 | same requirement (FMT-RAW-007 condition 1 with FMT-INDENT-017, FMT-INDENT-030) | The diff routes line-column reads through `TryGetPlacedColumn` in `GetAdjustedColumn` and `CommentIndentationContributor`. `GenericConstraintContributor` still reads `TryGetLayout`, which for a hole line that no construct places is the pass-1 block column, a column the line never takes. A `where` clause of a local generic function in an anonymous-method body in a hole is therefore placed from the scope depth. Counterexample below the table. | Read the declaration line through `TryGetPlacedColumn`. Add a test. |
| 5 | medium | `MethodChainAlignmentContributor.GetAdjustedColumn` (`MethodChainAlignmentContributor.cs:95–129`, unchanged), a private prediction of the raw-string phase's shift | FC-015, FC-031 | PR-introduced divergence (no input of this run turns corrupt; the shape below was already not idempotent on `origin/main`) | The method predicts the shift of a token behind a raw literal's closing delimiter as `opening column − closing column` of that literal alone. After the PR, the phase also adds the shift that an earlier literal gives the opening line, and gives a frozen line no shift. The prediction and its XML summary ("predicted with the same shift") no longer match the owner. Counterexample: `return """⏎    a⏎    """ + """⏎    b⏎    """.Trim()⏎       .ToString();`. Pass 1 leaves `.ToString()` at column 13, and pass 2 moves it to 24 under `.Trim`. `origin/main` also needs 3 passes, for a different reason. | Let the chain contributor and finding 1's repair use one owner of the per-line shift, or justify the copy and prove parity. Update the summary. |

Counterexamples for finding 1, both with `var a = $"""` at column 8 and the literal misaligned by 13 columns:

- **(a)**, a nested literal's closing line carries the anchor:

  ```
      Outer {"""
          inner
          """ + string.Concat("a",
              "b")}
      """;
  ```

  Pass 1 puts `"a"` at 44 and `"b"` at 41. Pass 2 moves `"b"` to 44. On `origin/main` this input needed 3 passes.

- **(b)**, a frozen verbatim line carries the anchor:

  ```
      Hello {@"a
  b" + string.Concat("a",
      "b")}
      """;
  ```

  Pass 1 leaves `b"` at column 0, as FMT-RAW-009 requires, with `"a"` at 19, and moves `"b"` to 32. Pass 2 moves `"b"` back to 19. On `origin/main` the same input was stable but changed the verbatim string's value. The PR traded that value change for this idempotency break.

Counterexamples for finding 2, both an aligned literal at column 17:

- `Hello {` ⏎ `// c` (17 spaces) ⏎ `→name` (17 spaces and a tab) ⏎ `}`. Pass 1 puts `// c` at 18 and `name` at 21. Pass 2 moves `// c` to 21.
- `Hello {` ⏎ `→string.Concat("a",` (17 spaces and a tab) ⏎ `"b")}` (17 spaces). Pass 1 puts `"b"` at 32 and `"a"` at 35. Pass 2 moves `"b"` to 35.

Counterexample for finding 3, a misaligned literal with this hole:

```
    Hello {
        /// first
        /// second
        name
    }
```

Pass 1 moves `/// first` and `name` to 21 and leaves `/// second` at 8. Pass 2 moves `/// second` to 21.

Counterexample for finding 4, an anonymous-method body in a hole, with an aligned literal at 17:

```
                 Hello {M(delegate
                 {
                     void L<T>()
                         where T : class
                     {
                     }

                     return 1;
                 })}
```

- **Aligned literal.** `where` moves to 16. FMT-RAW-007 condition 1 and FMT-INDENT-017 put it at 25, one level deeper than `void` at 21. The output is stable but wrong.
- **The same body in a literal misaligned by 13.** Pass 1 puts `where` at 29, and pass 2 at 16.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A write to a hole line is accepted (`LayoutModel.AcceptsWrite`) | `holeLine.Hole.Span.Contains(Contributor.Span)`. The innermost `InterpolationSyntax` of a multi-line `$"""` string, against the span of the node whose contributors write | FMT-RAW-007 condition 1: the construct lies inside the hole, **and** its anchor is a token inside the literal, measured after the literal moved | "Construct inside the hole" is explicit. "Anchor inside the literal" holds for every reachable contributor, because they anchor to tokens of their own node. "Measured after the literal moved" does not hold when the anchor's line gets a different shift (finding 1) or is read through `TryGetLayout` (finding 4) | mismatch, findings 1 and 4 |
| Column of a hole line that no construct places (`TryGetPlacedColumn`) | The character column of the line's first token in the input of the indentation phase | The column that token has after `CleanupPhase` (FMT-CLEAN-002) and after `RawStringAlignmentPhase` | The two disagree for tab-indented lines (finding 2) and for lines the phase moves by a different amount than the anchored line (finding 1) | mismatch, findings 1 and 2 |
| A line keeps its start in the raw-string phase (`StartsInsideNestedText`) | `FindTrivia(lineStart)`: a comment trivia (`IsCommentTrivia`, documentation comments included) that straddles the line start. `FindToken(lineStart)`: a non-raw string token that straddles it. A token at the line start, with no leading trivia, directly behind such text (`$@"a⏎{x}`) | The indentation phase's own "this line is not re-indented" policy: FMT-INDENT-007 (`/* */`, `/** */`), FMT-INDENT-011 (verbatim continuation), and FMT-INDENT-008 (`///` continuation markers are re-indented) | They agree for `/* */`, `/** */`, `@"`, `$@"`, `@$"`, `u8` verbatim, and a raw literal's content lines, which move. They disagree for `///` | mismatch, finding 3 |
| Which lines are hole lines (`LayoutComputer.RegisterRawStringHoleLines`) | Every `IsFirstOnLine` token of every `InterpolationSyntax` of each multi-line `$"""` string, visited pre-order so the innermost hole wins | The lines that `ComputeLineShifts` moves: opening line + 1 to the closing line of each multi-line raw literal, minus frozen lines | They agree. A line that a token starts inside a raw literal's span is always a hole line, and a token-started line is never frozen (the `{` behind `$@"` text is not `IsFirstOnLine`) | agree |
| A literal is replaced on its own (`IsNestedInRawStringHole`) | Ancestors that are multi-line raw literals | `ComputeLineShifts` visits nested literals, and their lines fall inside the outermost span | They agree | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| How far the raw-string phase moves a line | `RawStringAlignmentPhase.ComputeLineShifts` (changed: per line and cumulative over nesting and shared lines, frozen lines excluded). `MethodChainAlignmentContributor.GetAdjustedColumn` (a private prediction, unchanged). `RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.RealignContent` (a private copy that shifts every line, unchanged). `LayoutComputer.GetAdjustedColumn` predicts no shift. Query: `rg -n "RealignContent\|GetLeadingSpaceCountOfLastLine\|GetQuoteOffset\(" src --glob '!*Test*'` | More than one owner, parity not proven: findings 1 and 5. The RH5202 copy is pre-existing (hint 2) |
| The column a line will have after indentation | `LayoutModel.TryGetPlacedColumn` (new), read by `LayoutComputer.GetAdjustedColumn` and `CommentIndentationContributor`. `LayoutModel.TryGetLayout`, read by `GenericConstraintContributor:34`, `ConstructorInitializerContributor:33`, `LambdaAlignmentContributor:91` (as the base of a relative `ShiftRange`, which is correct), and `IndentationRewriter` (after the release, which is correct). Query: `rg -n "TryGetLayout\|TryGetPlacedColumn" src/Reihitsu.Formatter` | Two accessors. `GenericConstraintContributor` is on the wrong one: finding 4 |
| A line starts inside text or a comment that began earlier | `RawStringAlignmentPhase.StartsInsideNestedText` (new). `RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.StartsInsideLiteralOrComment` (new). Core `FormattingTextAnalysisUtilities.GetStringLineIndices`/`GetNonFormattableLineIndices` and `SyntaxTriviaUtilities.IsInsideCommentOrDisabledText` | Different decisions: raw text and hole code move in the formatter but withhold the RH5104 fix. `04-implementer.md` justifies the difference, and I accept it. The comment kinds of the formatter predicate disagree with the indentation phase: finding 3 |
| Detection of a multi-line interpolated raw string | One-line kind checks in `RawStringAlignmentPhase.IsMultiLineRawStringLiteral`, `LayoutComputer.RegisterRawStringHoleLines`, `MethodChainAlignmentContributor.IsMultiLineRawStringEnd`, the RH5202 analyzer and fix, and RH5601. Query: `rg -n "InterpolatedMultiLineRawStringStartToken" src --glob '!*Test*'` | The same kind test everywhere, and it existed before the PR. Not a finding |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 25 `RawStringInterpolationHoleTests` that use `AssertRuleResult(input, expected)` (case 1, case 2, the `$$`, clause, leftward, argument, initializer, lambda-body, comment, nested, depth-3, `u8`, and FMT-RAW-009 tests) | Pass 1 equals the spec's output, and pass 2 equals it too, under LF and CRLF | A different column in either pass or under either line ending | `FormatterTestsBase.AssertRuleResult` (two passes, LF and CRLF) | Adequate for the shapes they hold. The run lacks tests for the shapes of findings 1 to 4 |
| `AlignedLiteralKeepsTheColumnsOfItsHoleLines` | An aligned literal is a fixed point | Any change in one pass | `AssertRuleResult(input)` (output equals input, so one pass suffices) | adequate |
| `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` | FMT-PIPE-004 for a column the spec leaves open | Pass 2 differs from pass 1 | `AssertSecondPassIsStable` (LF and CRLF) | Adequate. It asserts no column, as the coverage sentence requires |
| `TabInFrontOfHoleLineOfAlignedLiteralBecomesFourSpaces` | The tab in front of a hole line becomes four spaces, and the line is otherwise kept | A different column | `AssertRuleResult` | Adequate for its input, but it tests one side only (FC-003): without a comment or an anchor on the line. See finding 2 |
| `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` | Characterizes today's output, which 1B keeps | A different layout | `AssertRuleResult` | Adequate. It deliberately has no requirement tag |
| RH5202 `VerifyFormatterAlignsMultiLineHoleAndClearsTheDiagnostic`, `…NestedRawStringInOnePass…` | RH5202 reports the input. The formatter output equals the expected output, is stable on a second pass, and is not reported | A report on the output, or a second-pass change | `FormatterTestsBase<RH5202>.VerifyFormatter` (two passes, LF and CRLF) | adequate |

### Failure classes dismissed

Classes that reach this change are traced in the findings above: FC-001, FC-003, FC-012, FC-015, FC-016, FC-022, FC-027, and FC-031. I dismissed the rest:

- **FC-002.** "Inside the hole" is an explicit `Span.Contains` test. "Anchor inside the literal" is checked under Guard scope.
- **FC-004 and FC-021.** No line is joined. A `/* */` that ends in the middle of a hole line keeps its following line (`FollowingLineOfCommentInHoleStaysAsWritten`).
- **FC-005, FC-019, and FC-020.** A directive inside a hole does not parse, and the formatter skips files with syntax errors.
- **FC-006.** The content lines of a raw literal are never frozen, because `IsNonRawStringText` is false for raw tokens, so the compiler's indentation rule still holds. `ComputeReplacement` drops a reparse whose kind differs.
- **FC-007, FC-008, FC-009, FC-018, and FC-030.** #951 changes no code fix.
- **FC-010.** Every new formatter test runs under LF and CRLF, and I traced one CRLF hole case, which was stable. The failures above do not depend on the line ending.
- **FC-011.** The placed-line marking is monotone, the sweep keeps `MaxAlignmentPasses`, and the raw-string phase has no loop.
- **FC-013.** `scripts/apply-fix.sh` for RH5202, RH5203, RH5204, RH5205, RH5206, RH5104, RH5601, and RH5602 over four formatter outputs reported `no-diagnostic` in all 8 arms. The outputs were the aligned literal with `name` at 25, the lambda-body example, the comment example, and the nested aligned literal.
- **FC-014.** No new rule.
- **FC-017.** The helpers assert two passes under both line endings.
- **FC-023, FC-024, FC-025, FC-026, FC-029, and FC-036.** No identifier, partial, expression-body, repeated-token, key/value, or rename logic changed.
- **FC-028.** Both entry points run the same `LayoutComputer` and phase. See hint 3 for a case I did not verify.
- **FC-032.** 924eaa0 is a form-only S3267 change. The spec checker re-ran the focused suites after it, and ea03170 adds only a handover.
- **FC-033 and FC-034.** The issue's inputs are used verbatim.
- **FC-035.** The expected values are the spec's, and the tests were watched red on 8d5a762.
- **FC-037.** No text-only claim is made.

### Hints

1. **Pre-existing, outside the holes.** An anchor on the closing-delimiter line of a raw literal that is not in a hole has the mechanism of finding 1(a). The input `return M("""⏎    a⏎    """ + string.Concat("x",⏎                        "y"));` puts `"y"` at 24 in pass 1 and at 37 in pass 2. The code path for non-hole lines is unchanged by the diff. Follow-up candidate, unless the repair of finding 1 covers it.
2. **Pre-existing, already listed by the skeptic.** The RH5202 fix's `RealignContent` shifts every line, including FMT-RAW-009 lines, so it can change a nested verbatim string's value.
3. **Unverified, FC-028.** `RegisterRawStringHoleLines` sees only holes inside the node being formatted. So a fix that formats a node lying inside a hole (`FormatNodeInDocumentAsync`) gives that node's hole lines block and contributor columns, while document formatting keeps them.

## thoenissen/Reihitsu#1133

### Scope

Full audit, same SHAs. In scope: `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken(token, position)`, the fix's call, 13 analyzer tests, 1 formatter-parity test, and 1 RH5602 test.

### Findings

_None._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A tab is literal content (`IsStringContentToken`) | `InterpolatedMultiLineRawStringStartToken` and `InterpolatedRawStringEndToken`: `token.Span` only, so their trivia stays code. Other literal kinds: the token found by `FindToken`, trivia included, which is pre-existing | `CleanupPhase` (FMT-CLEAN-002/004) replaces tabs in whitespace trivia only. The RH5601 fix uses the same predicate with the diagnostic position | The start and end tokens agree with the formatter: `$"""→` and the closing-line tabs are kept and not reported, while `"""→;` and `→$"""` are replaced and reported. For the other kinds the trivia exemption is wider than the formatter's. That is pre-existing and is the follow-up candidate in `01-spec-author.md` | agree within scope |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A tab that RH5601 treats as content | `rg -n "IsStringContentToken" src`. One owner, the analyzer, with the analyzer and the fix as consumers | single owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 8 `VerifyNoDiagnosticsFor…` RH5601 tests (issue input under LF and CRLF, `$"""→`, after a hole line, `$$`, mixed indentation, and the plain twins) | No diagnostic | Any RH5601 report | `Verify(source)`, exact diagnostics | adequate |
| The 4 `Verify…IsDetectedAndFixed` RH5601 tests (hole-line tab, `"""→;`, `$"a"→;`, `→$"""`) | Exactly those diagnostics, fixed to the given text | A missing or extra report, or a different fix | `Verify(source, fixed, diag)`, incremental and Fix All | adequate (FC-003, both sides of the token-text boundary) |
| `VerifyCodeFixLeavesTheTabsOfAnInterpolatedRawStringUntouched` | The fix rewrites only the code tab | The closing line rewritten, which no longer compiles | `Verify(source, fixed, diag)` | adequate |
| `RH5601…FormatterTests.VerifyFormatterKeepsTheClosingLineTab…` | The formatter keeps the tab, and RH5601 does not report it | A report, or a formatter change | `VerifyFormatter(source)`, LF and CRLF | adequate |
| `RH5602…VerifyNoDiagnosticsForTrailingWhitespaceBehindAHole…` | `{1}··` is not reported | A report | `Verify(source)` | Adequate (characterization) |

### Failure classes dismissed

- **FC-001, FC-002, FC-003, and FC-016.** The token-text versus trivia boundary is explicit (`token.Span.Contains`), tested on both sides, and covers both the start and the end token and `$$`.
- **FC-006.** The fix no longer reaches the closing line (`VerifyCodeFixLeavesTheTabs…`).
- **FC-007.** The fix is single-pass and unchanged.
- **FC-008 and FC-009.** The Fix All provider is unchanged. A skipped diagnostic still registers no action, as before.
- **FC-010 and FC-033.** CRLF is tested on the issue input.
- **FC-013.** There is a formatter-parity test, and the apply-fix check above found nothing.
- **FC-031.** The summary describes the new position rule.
- **FC-034.** The issue input is used verbatim.
- Every other class has no path into a position predicate of an analyzer: formatter phases, renames, trivia moves, expression bodies, and layout pairs.

### Hints

_None._

## thoenissen/Reihitsu#1146

### Scope

Full audit, same SHAs. In scope:

- `RH5104CommentsMustBeOnTheirOwnLineAnalyzer.IsInlineComment`;
- `RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider`: the removed blank-line insertion, `StartsInsideLiteralOrComment`, and the async `RegisterCodeFixesAsync`;
- 23 new tests and 1 updated test, and 1 formatter-parity test;
- the RH5104 rule page.

The issue's triage comment keeps Q4 as "comments on code-free lines are not reported; no comment reordering".

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 6 | high | `RH5104CommentsMustBeOnTheirOwnLineAnalyzer.IsInlineComment` (`:81`) together with the unchanged `RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.GetCommentRemoval` (`:72–73`) and RH5104-004 | FC-004, FC-015, FC-016 | PR-introduced (same requirement: Q4 "no comment reordering") | The input is `/* a */ /* b⏎   c */ M();`. **On `origin/main`**, both comments were reported, and `scripts/apply-fix.sh RH5104` gave `/* a */`, then `/* b⏎ c */`, then `M();`: the order was kept. **On this branch**, only `/* b…c */` is reported, because its last line shares a line with `M();`. Every application moves it above `/* a */`, giving `/* b⏎ c */`, then `/* a */`, then `M();` under LF and CRLF, so the user's comments are reordered. The skeptic and the spec author name this shape as a follow-up candidate. No maintainer decision covers it, and the issue's own scope says "no comment reordering". The analyzer now counts only tokens as code. The fix still treats `/* a */` as code before the comment (`hasCodeBeforeComment` uses non-whitespace text), so the two owners of "what counts as code on this line" now disagree. | This needs a maintainer decision before a repair. Options: withhold the fix when another comment precedes the reported comment on its start line (the AGENTS.md code-fix coverage policy: a fix that reorders user comments withholds itself), or record the reordering as an accepted follow-up. Either way, align the fix's notion of code with the analyzer's, and add a test. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A comment shares a line with code (`IsInlineComment`) | Every line the comment spans. A token with a non-empty `Span` that overlaps the line's `Span`. Zero-width tokens (`EndOfFileToken`) and other comments do not count | The fix's `GetCommentRemoval`: `hasCodeBefore`/`hasCodeAfter` are any non-whitespace text on the start or end line, other comments included | They disagree whenever another comment shares the start or end line | mismatch, finding 6 |
| The fix is withheld (`StartsInsideLiteralOrComment`) | Three cases: a non-whitespace trivia that straddles the start of the comment's start line; a token that straddles it; or an `InterpolatedStringExpressionSyntax` ancestor of the token at the line start that began earlier | RH5104-006's list, RH5104-004's condition, and the analyzer's `IsInsideMultiLineInterpolatedStringHole` (comments inside holes are not reported) | They agree for every form in RH5104-006, including `$@"a⏎{x}b"` and `@$"`, where the line starts at `{` behind the text. The other side, a literal that starts on the comment's line, is fixed | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| "Code" on a comment's line | `IsInlineComment` in the analyzer (tokens) and `GetCommentRemoval` in the fix (non-whitespace text). Query: `rg -n "IsNullOrWhiteSpace" src/Reihitsu.Analyzer*/Rules/Layout/RH5104*.cs` | Two owners. The PR moved one, and they diverge: finding 6 |
| A line begins inside a literal or a comment | See #951. This private predicate differs deliberately from the formatter's | justified |
| A blank line before the moved comment | No longer consumed by the fix. `BlankLineEditor.IsFirstInBlock` and `FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex` are untouched and keep their other consumers | none in the diff |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 7 `VerifyNoDiagnosticsFor…` Q4 tests (`/* a */ /* b */`, `/* c */ // d`, the tail of a multi-line comment, `/** */` with `/* */` or `//`, and the last line without a line break, in two variants) | No diagnostic | Any report | `Verify(source)` | Adequate. They do not cover finding 6's shape |
| `VerifyCommentsAroundCodeAreReportedWhenAnotherCommentSharesTheLine` and `VerifyTwoBlockCommentsInFrontOfCodeAreReported` | Both comments are still reported when a token shares the line | One comment unreported | `Verify(markup)` | adequate (FC-003) |
| The 9 `VerifyNoCodeFixIsOffered…` tests and `…InsideABlockComment` | Reported, and no action registered, for each RH5104-006 form. For the block comment, an action exists for `/* a…b */` | One action, or a missing report | `VerifyNoCodeFixIsOffered` (checks the markup diagnostics, then zero actions per diagnostic), and `GetCodeFixActionsAsync` per location | adequate (FC-009) |
| `VerifyCommentOnTheLineWhereAVerbatimStringStartsIsMoved` and `…AHoleThatSpansLinesStartsIsMoved` | The other side of RH5104-006 is fixed | No fix, or a different text | `Verify(source, fixed)` | adequate (FC-003) |
| `VerifyFixInsertsNoBlankLineAboveTheMovedComment` and the renamed `VerifyRelocatedCommentGetsNoBlankLineWhenPreviousLineIsCode` | No inserted blank line, existing ones kept (RH5104-005) | An inserted or a removed blank line | `Verify(source, fixed)`, incremental and Fix All | adequate |
| `VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported` | Fix All moves `// d`, and the withheld `/* c */` stays reported and unmoved | `/* c */` moved into the string, or no longer reported | `Verify(…, FixedState.MarkupHandling = Allow)`. With no `BatchFixedCode`, the Fix All check uses the fixed state | Adequate. `03-test-implementer.md` shows it red on 493f956 |
| `RH5104…FormatterTests.VerifyFormatterKeepsTwoCommentsOnALineWithoutCode…` | The formatter keeps the line, and RH5104 does not report it | A report or a change | `VerifyFormatter(source)`, LF and CRLF | adequate |

### Failure classes dismissed

- **FC-002.** "Token" is explicit (`Length > 0`), and the end-of-file test covers it.
- **FC-005, FC-019, FC-020.** The directive exemption is unchanged and tested (`VerifyCommentBehindConditionalDirectiveDoesNotProduceDiagnostics`).
- **FC-006 and FC-022.** The Q1 forms are withheld. The literal kinds are listed in the tests.
- **FC-007.** The fix raising RH5020 is by design (J1, SHARED-FIX-006, RH5104-005 `Why`).
- **FC-008 and FC-009.** Tested, see above.
- **FC-010 and FC-033.** The analyzer and fix tests run LF only. Both predicates work on positions and token spans, never on line-break characters, and the CRLF insertion test is unchanged. I also ran `apply-fix` under CRLF for finding 6.
- **FC-013.** There is a formatter-parity test.
- **FC-017.** Exact text is asserted, incremental and Fix All.
- **FC-030.** The fix is a text edit and calls no formatter.
- **FC-031.** The summaries describe the new code. No blank-line wording remains.
- **FC-034.** The issue's Q1 and Q4 inputs are used.
- Every other class has nothing to reach in a comment-relocation fix: formatter phases, renames, identifiers, expression bodies, and layout pairs.

### Hints

_None._

## Whole run

### Scope

- The checkout is `ea03170` on `claude/sweet-knuth-bj050c`.
- `git ls-remote origin refs/heads/main` returns `f3d514d0b1cbd8b3e540f2b2c7243dcd21760929`, which matches the prompt, and `git merge-base --is-ancestor` confirms that the branch contains it.
- The diff holds 9 production files and 8 test files, plus specs, rule pages, and `sdd/work/`, exactly as the handovers describe.
- No `TODO` and no tracker reference appear in the added source lines. No file owned by a concurrent run is touched.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 7 | low | `LayoutModel.AcceptsWrite` (`LayoutModel.cs:247`) and `LayoutModel.Contributor` | — | PR-introduced | `AcceptsWrite` is named and documented as a query, but it also marks a line as placed. That marking decides which layouts `ReleaseUnplacedRawStringHoleLines` keeps. The mark is driven by ambient state that the sweep sets on the model. A reader of `ShiftRange` or `Set` cannot see that a successful check changes the outcome. | Split the marking from the check, or name and document the side effect. |

Findings 1 to 5 (#951) and 6 (#1146) are listed under their issues.

### Commands

```shell
bash scripts/trace.sh <fixture> --passes 3 --no-install         # 12 fixtures on ea03170; 7 on origin/main in a temporary worktree
bash scripts/apply-fix.sh RH5104 <dir> --no-install             # finding 6, on ea03170 and on origin/main
bash scripts/apply-fix.sh <RH5202|RH5203|RH5204|RH5205|RH5206|RH5104|RH5601|RH5602> <dir> --no-install   # FC-013, 4 fixtures, all no-diagnostic
```

I re-ran no test project. The build and the focused results of `05-cleaner.md` and `06-spec-checker.md` stand, because no finding turns on them.

# Repair round 1

Role: auditor
Commit: 3c390f4
Verdict: #951: findings; #1133: pass; #1146: pass

## Scope

This is a repair delta audit: `git diff feb5ecc 3c390f4`. The head is `3c390f4`. `git ls-remote origin refs/heads/main` returns `f3d514d0b1cbd8b3e540f2b2c7243dcd21760929`, the branch contains it, and the tree is clean. I read the "Repair round 1" sections of `00-lead.md`, `01-spec-author.md`, `03-test-implementer.md`, `04-implementer.md`, `05-cleaner.md`, and `06-spec-checker.md`. The prompt carried no intended fix or hoped-for verdict.

**Scope grew.** The repair changes more than the hole lines that findings 1 to 7 name:

- **One shared shift.** A new shared mechanism, `RawStringLineShifts` (`Pipeline/RawStringAlignment/Utilities/`), now feeds both the raw-string phase and the indentation phase.
- **A shared helper changed.** `LayoutComputer.GetAdjustedColumn` now adds the predicted raw-string shift for every anchor on a moved line, outside holes too. Every contributor calls it.
- **Stored columns.** `LayoutModel.Set` now stores a column with the shift subtracted.
- **One copy removed.** The private prediction in `MethodChainAlignmentContributor` is gone.

Formatted code is unaffected: an aligned literal has a shift of 0, and a tree without raw literals takes a null shift table. For misaligned literals, anchors behind a raw literal's closing quotes now converge in one pass (auditor hint 1, now pinned by a test). The lead may want to weigh escalation 5 (a shared helper with other consumers) for the run report.

Because of that growth, I audited the whole mechanism, not only the lines the findings name:

- every reader and writer of `LayoutModel` (`rg -n "TryGetLayout\(|TryGetPlacedColumn|TryGetLayoutColumn|\.Set\(|ShiftRange" src/Reihitsu.Formatter`);
- the recursion in `RawStringLineShifts.GetDistance`;
- the tab handling.

I re-ran no test project. The focused results of `05-cleaner.md` and `06-spec-checker.md` were taken on the final production code: no production file changed after `f7ca92a`.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Both counterexamples (a nested literal's closing line, and a frozen verbatim line carrying the anchor) are stable after pass 1. `ArgumentAnchoredOnTheClosingLineOfANestedLiteralKeepsItsAlignment` and `ArgumentAnchoredOnTheContinuationLineOfAVerbatimStringKeepsItsAlignment` pin the output. The anchor and the anchored line now both go through `RawStringLineShifts.GetShift`. |
| 2 | **open (narrowed)** | Closed when only whitespace stands in front of the token: both counterexamples are stable, and two tests pin them. Still open for a tab in front of a comment that precedes the token on the line. `LayoutComputer.GetColumnAfterTabReplacement` (`LayoutComputer.cs`, the `default:` branch) falls back to the character column at the first non-whitespace character. `CleanupPhase.NormalizeWhitespaceTabs` still expands that tab. Counterexample, an aligned literal: `Hello {` ⏎ 17 spaces `→/* c */ string.Concat("a",` ⏎ 17 spaces `"b")}`. Pass 1 puts `"b"` at 40, and pass 2 moves it to 43. FC-001: the predicate's span is the whitespace before the first non-whitespace character, while the counterpart replaces tabs in every whitespace trivia of the line. |
| 3 | closed | `StartsInsideNestedText` now freezes only `MultiLineCommentTrivia` and `MultiLineDocumentationCommentTrivia`. The `///` counterexample is stable, and `/** */` in a hole is still kept (traced). `DocumentationCommentLinesInHoleMoveWithTheTokenBelowThem` pins the output. |
| 4 | closed | `GenericConstraintContributor` reads `TryGetPlacedColumn`. Both constraint counterexamples are stable and pinned. |
| 5 | closed | The private prediction is removed, and `RawStringLineShifts` is the formatter's only owner of the shift. The shared-line chain counterexample is stable after pass 1 and pinned by `ChainLinkBehindSecondRawStringOnASharedLineAlignsAfterBothLiteralsMoved`. The stale XML summary went with the method. |
| 6 | closed | Escalation 4 has maintainer answer A. RH5104-007 and RH5104-004 are updated, and `HasCommentBeforeOnLine` implements the withholding. The finding's input now gets `no-fix-offered` from `apply-fix.sh RH5104`, so nothing is reordered. `M(); /* a */ // b` keeps its order over two iterations, and `/** x */ /* y */ M(); // z` offers no fix behind the documentation comment (traced). Four RH5104-007 tests pin this, with exact iteration counts. The fix's notion of code (`GetCommentRemoval`) is unchanged. I re-derived the implementer's argument and accept it: under RH5104-007 no comment precedes the moved one on its start line, so the remaining difference only keeps a following comment on its own line. |
| 7 | closed | `AcceptsWrite` is now a pure check. `RecordPlacement` holds the side effect, and the summaries of `Contributor`, `Set`, and `ShiftRange` state it. |

| Hint | Status | Evidence |
|---|---|---|
| 1 | closed | Fixed by the shared shift (the trace is stable after pass 1) and pinned by `WrappedArgumentAnchoredBehindTheClosingQuotesOfARawStringAlignsAfterTheLiteralMoved`. That test was watched red on `193b3a5`. |
| 2 | superseded | It is the maintainer-approved follow-up thoenissen/Reihitsu#1458. |
| 3 | closed: examined, no defect found | I used `scripts/apply-fix.sh` with fixes that format a node inside a hole: **RH5205** on a lambda whose brace is misaligned, in an aligned literal, and **RH3208** on an expression-bodied local function in an anonymous-method body, in an aligned literal and in a literal misaligned by 13. Each fix's output matches the document formatter's layout. Two fix outputs, traced, change only through `CleanupPhase`'s final-newline removal. The node entry points still register only holes inside the target, but no output I could construct differs. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 2 | high | `LayoutComputer.GetColumnAfterTabReplacement` | FC-001, FC-012 | same requirement (FMT-RAW-007, the FMT-CLEAN-002 sentence; FMT-PIPE-004) | See the table above: the count stops at the first non-whitespace character, so a tab in front of a comment on a hole line is counted as one column. | Count tabs the way `CleanupPhase` replaces them in every whitespace trivia in front of the token, not only up to the first non-whitespace character. Add the counterexample as a test. |
| 8 | high | `RawStringAlignmentPhase.CollectReplacements` (`getColumnBeforeShift`, the character column in the text) feeding `RawStringLineShifts.GetDistance`; it disagrees with the indentation side's `GetColumnBeforeRawStringShift`, which counts that line's tabs as four | FC-012, FC-015, FC-016 | PR-introduced (a sibling of finding 2 that the first audit did not construct; it fails the same way on `ea03170` and is stable on `origin/main`) | The case is a nested raw literal whose opening quotes start a hole line indented with a tab. The raw-string phase measures the opening quotes with the tab counted as one column, and aligns the inner literal there. `CleanupPhase` then expands the tab, which moves the opening quotes 3 columns right of the content. The indentation phase predicted the same literal with the tab counted as four, so the two owners of that column now disagree. Counterexample, an aligned outer literal at 17: `Outer {` ⏎ 17 spaces `→"""` ⏎ 21 spaces `inner` ⏎ 21 spaces `"""}`. Pass 1 puts `inner` and `"""}` at 18 while the quotes end at 21; pass 2 moves them to 21. The same happens with a misaligned outer literal. | Measure the opening column in the raw-string phase as `CleanupPhase` will leave it: one statement of the tab rule, shared by both phases (see hint A). Add a test for the aligned and the misaligned case. |
| 9 | low | `CommentIndentationContributor.AlignCommentsBeforeToken` summary ("the one it will have after indentation, which for a line of an interpolation hole that keeps its column is the column it already has"). `LayoutModel.TryGetPlacedColumn` summary ("the column the token already has") | FC-031 | PR-introduced | Both summaries describe the old value. `TryGetPlacedColumn` now returns the column after the raw-string phase moved the line, and for a kept hole line its column with tabs counted as four. Text only; it cannot reach compiled behavior. | Reword both to "the column after formatting: after the raw-string shift, with the tabs of a kept line counted as the cleanup phase replaces them". |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A line is frozen (`RawStringLineShifts.StartsInsideNestedText`) | `MultiLineCommentTrivia` or `MultiLineDocumentationCommentTrivia` straddling the line start. A non-raw string token straddling it, or a token directly behind such text | FMT-INDENT-007 keeps the following lines of `/* */` and `/** */`. FMT-INDENT-008 re-indents `///` markers | They agree | agree |
| Column of a kept hole line (`GetColumnAfterTabReplacement`) | From line start to the token: a tab counts 4 and a space 1. At any other character it falls back to the character column | `CleanupPhase.NormalizeWhitespaceTabs`: every whitespace trivia, `Replace("\t", "    ")` | They agree for whitespace-only prefixes and disagree when a comment precedes the token | mismatch, finding 2 |
| Opening-quote column of a literal (`getColumnBeforeShift`) | Raw-string phase: character column in the text. Indentation phase: `GetColumnBeforeRawStringShift`, tab-expanded for kept hole lines | The column after `CleanupPhase` | They disagree for a tab-indented hole line that starts with a nested literal | mismatch, finding 8 |
| Stored layout (`LayoutModel.Set`) | `layout.Column − GetRawStringShift(line)`, with the shift taken from the current model | The rewriter writes the stored value, and the raw-string phase adds its own shift | They agree once the sweep settles. A stale stored value changes the stored columns, so `MatchesColumns` forces another sweep. Stand-ins written in pass 1 are released or used only as the base of a relative `ShiftRange` | agree |
| The fix is withheld behind another comment (`HasCommentBeforeOnLine`) | Comment trivia (`IsCommentTrivia`, documentation comments included) lying wholly inside [line start, comment start) | RH5104-007. A comment that began on an earlier line is covered by RH5104-006 | They agree | agree |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Per-line raw-string shift | `RawStringLineShifts`, the only owner in the formatter (`rg -n "GetShift\|ComputeLineShifts\|GetLeadingSpaceCountOfLastLine\|IsMultiLineRawStringEnd" src --glob '!*Test*'`). The RH5202 fix's `RealignContent` is the approved follow-up thoenissen/Reihitsu#1458 | single owner. Its column input differs between the two callers: finding 8 |
| Tab replacement (FMT-CLEAN-002) | `CleanupPhase.NormalizeWhitespaceTabs` (`Replace("\t", "    ")`) and `LayoutComputer.GetColumnAfterTabReplacement` (`FormattingContext.IndentSize`) | Two statements of one rule. The diff justifies them: `CleanupPhase` belongs to a concurrent run. Parity holds for whitespace-only prefixes (checked by reading) and fails past a comment (finding 2). Hint A |
| Column of a line for contributors | `TryGetPlacedColumn` (comment, generic constraint, constructor initializer, `GetAdjustedColumn`) and `TryGetLayoutColumn` (lambda, the base of a relative shift). `TryGetLayout` is read only by `IndentationRewriter` | consistent |
| Another comment before the reported comment | `RH5104…CodeFixProvider.HasCommentBeforeOnLine` only (`rg -n "HasCommentBefore\|CommentBeforeOnLine" src --glob '!*Test*'`) | single owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 7 new `RawStringInterpolationHoleTests` and the 2 new `RawStringAlignmentPipelineTests` | The spec's output on pass 1 and pass 2, under LF and CRLF | Any column difference | `AssertRuleResult(input, expected)` | Adequate. The expected values come from the specs and were watched red before the code (FC-035). The tab tests cover only whitespace-only prefixes and no nested literal: findings 2 and 8 (FC-003, FC-016) |
| `VerifyCodeFixIsOfferedOnlyForTheFirstCommentOfALine` | All three comments are reported. Only `/* a */` has an action | An action for `// b` or `/* d…e */`, or none for `/* a */` | `Verify(markup)` plus `GetCodeFixActionsAsync` per diagnostic | adequate |
| `VerifyNoCodeFixIsOfferedForACommentBehindAnUnreportedCommentOnItsLine` | Reported, with no action | One action | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixAllKeepsTheOrderOfCommentsThatShareALine` | Order kept and the withheld comment still reported, after exactly 2 incremental iterations and 2 Fix All iterations | A reorder, a missing report, or another iteration count | `Verify(…, MarkupMode.Allow, exact iteration counts)` | adequate (FC-008, FC-017) |
| `VerifyCommentIsMovedOnceTheCommentBeforeItHasLeftItsLine` | The other side of RH5104-007 | No fix offered | `Verify(source, fixed)` | adequate (FC-003) |

## Failure classes dismissed (repair delta)

- **FC-011.** `GetDistance` recurses only into literals with a smaller index, either enclosing literals or earlier literals on a shared line, so it terminates. The sweep bound is unchanged.
- **FC-013.** `apply-fix.sh` for RH5202 to RH5206 over 7 formatted repair fixtures returned `no-diagnostic` in all 14 arms per rule. The fixtures were the hint-1 shape, a constraint clause, a nested anchor, a shared-line chain, a verbatim anchor, a three-literal chain with an argument, and a nested literal as an argument.
- **FC-028.** Hint 3 is examined above.
- **FC-032.** The cleaner's and the spec checker's runs are on the final production code.
- **FC-033.** CRLF is covered by `AssertRuleResult`, and the RH5104 predicates work on positions.
- **FC-035.** The test implementer derived the expected values from the specs and watched them red. The hint-1 test was watched red on `193b3a5`.
- The remaining classes are the same as in the first audit and the repair adds nothing for them. Six more shapes, each stable after pass 1: a three-literal chain on shared lines with a wrapped argument, a nested interpolated literal as a wrapped argument with a wrapped argument in its hole, a raw literal in a lambda in a hole of a raw literal inside a lambda, and the three earlier-phase shapes (switch expression, block lambda, array initializer).

## Hints

- **A.** The rule "a tab in code whitespace becomes four spaces" now has two statements: `CleanupPhase` and `GetColumnAfterTabReplacement`. The repair of findings 2 and 8 needs a third use of it. One shared helper in a place both owners may use is a follow-up candidate, outside this run's files if `CleanupPhase` stays off-limits.
- **B.** `RawStringLineShifts.GetDistance` is not memoized. Each nesting level evaluates the distances of the literals around it twice, once for the opening line and once for the closing line, so the cost grows with depth. That cost is harmless at the depths seen; `SelfHostingTests` passes.

## Commands

```shell
bash scripts/trace.sh <fixture> --passes 3 --no-install      # 12 first-round fixtures and 8 repair probes on 3c390f4; one probe also on ea03170 and on origin/main (temporary worktrees, removed)
bash scripts/apply-fix.sh RH5205|RH3208 <dir> --no-install   # hint 3
bash scripts/apply-fix.sh RH5104 <dir> --no-install          # finding 6
bash scripts/format.sh <dir> --no-install                    # scratch copies, then apply-fix RH5202 to RH5206 (FC-013)
```

# Repair round 2

Role: auditor
Commit: f8866f6
Verdict: #951: findings; #1133: pass; #1146: pass

## Scope

This is a repair delta audit: `git diff 34745f8 f8866f6 -- src/ sdd/specs/ documentation/`, at head `f8866f6`. `git ls-remote origin refs/heads/main` returns `3973631c2c222ae484dd6039c3d6c89311959653`, the branch contains it, and the tree is clean. I read the "Repair round 2" sections of `00-lead.md`, `03-test-implementer.md`, `04-implementer.md`, `05-cleaner.md`, and `06-spec-checker.md`, plus the spec checker's "Main merge" section and the latest revision of `01-spec-author.md`. The prompt carried no intended fix or hoped-for verdict.

The delta has three parts:

- **This run's repair, `b8cc3e0..22ecca4`.**
  - **Code.** `RawStringLineShifts.GetColumnAfterTabReplacement` is now shared and replaces the private copy in `LayoutComputer`. The raw-string phase uses it for every literal's first token.
  - **Summaries and tests.** XML summaries in `LayoutModel` and `CommentIndentationContributor`, and 3 tests.
- **The merge of `origin/main`, `4c8b98d`.** It is a clean merge: `git show --cc` has no resolution hunks. Main brings thoenissen/Reihitsu#1456 (byte-order mark handling in `CleanupPhase`, documentation-comment changes) and thoenissen/Reihitsu#1462 (the rule-page policy).
- **The spec author's rule-page revert, `6894c38`.** `git diff origin/main -- documentation AGENTS.md squad .claude` is now empty.

I audited main's changes only where they meet this run: `CleanupPhase` (the counterpart of the tab count) and the documentation-comment phase (the `///` lines in a hole, finding 3).

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 2 | closed | The residual counterexample (a tab in front of a comment on a hole line) is stable after pass 1, with `"b"` at 43 under `"a"`. `ArgumentAnchoredBehindATabIndentedCommentOnAHoleLineAlignsAfterTheTabReplacement` pins it. `GetColumnAfterTabReplacement` now counts a tab as four exactly when `FindTrivia` returns a `WhitespaceTrivia`, which is the set `CleanupPhase.NormalizeWhitespaceTabs` rewrites (whitespace in the leading and trailing trivia of every token). |
| 8 | closed | The aligned and the misaligned counterexample (nested opening quotes behind a tab) are stable after pass 1. Two tests pin them. Both phases now measure the opening column through the same method. |
| 9 | closed | The summaries of `TryGetPlacedColumn`, `TryGetPlacedColumnBeforeRawStringShift`, `RegisterRawStringHoleLine(column)`, and `AlignCommentsBeforeToken` describe the post-shift, tab-expanded column. |
| 1, 3–7 | closed (unchanged) | I re-traced the first-round counterexamples for findings 1, 3, and 4, and the hint-1 input, on `f8866f6`. All are stable after pass 1. |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 10 | high | `RawStringAlignmentPhase.ComputeReplacement` (`RawStringAlignmentPhase.cs:120–121`: `GetLeadingSpaceCount` then `Math.Max(0, currentSpaces + shift)`), against the measuring side `RawStringLineShifts.GetColumnAfterTabReplacement` | FC-012, FC-015, FC-016, FC-003 | PR-introduced (a sibling of findings 2 and 8 that no earlier round constructed: on `origin/main` the indentation phase re-indents such a line, and the tab is gone) | The phase moves a line only by changing the spaces in front of its first tab, while both phases now measure the line with that tab counted as four. When a literal moves left by more columns than the spaces in front of the tab, the move is clamped at the tab. The line then moves less than FMT-RAW-007 condition 3 requires: the line is indented more than the shift, so the requirement covers it. Anything aligned to it was measured at the predicted column. Counterexamples are below the table. Every existing tab test shifts right or not at all (FC-003). | Move a hole line's whole leading whitespace: write `tab-expanded width + shift` spaces in place of the spaces and tabs, so the phase writes what both phases measure. Content and closing lines stay space-counted: their leading whitespace is literal text, which is the scope of thoenissen/Reihitsu#1432. Add a leftward-shift test with a tab, with and without an anchored line. |

Counterexamples for finding 10, both with `var a = $"""` (quotes at 17) and the closing marker at 21, so the literal moves 4 columns left:

- **Plain.** The hole line is 2 spaces and a tab, then `name`, which puts `name` at column 6. The output, stable after pass 1, puts `name` at 4. FMT-RAW-007 requires 2.
- **Anchored.** The hole line is 2 spaces and a tab, then `string.Concat("a",`, and the next line is `"b")}`. Pass 1 puts `"b"` at 16, while `"a"` ends at 18 after cleanup. Pass 2 moves `"b"` to 18, so the result breaks FMT-PIPE-004.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A tab counts four columns (`GetColumnAfterTabReplacement`) | Every character from line start to the token. A tab counts 4 when `root.FindTrivia(position)` is `WhitespaceTrivia`. Everything else counts 1: comment text, literal text, a tab inside a comment, the BOM character | `CleanupPhase.NormalizeWhitespaceTabs`, applied to `WhitespaceTrivia` in the leading and trailing trivia of every token. Directive trivia is normalized separately and cannot stand on a raw literal's line | They agree | agree |
| How far a line actually moves (`ComputeReplacement`) | The leading spaces up to the first non-space character, clamped at 0 | The predicted shift, measured with tabs counted as four | They disagree when a leftward shift exceeds the spaces in front of a tab | mismatch, finding 10 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A code-whitespace tab is four columns | Measuring: `RawStringLineShifts.GetColumnAfterTabReplacement`, the only measuring statement, used by both phases. Rewriting: `CleanupPhase.NormalizeWhitespaceTabs`, owned by run R2, a follow-up per escalation 5. Writing a moved hole line: `RawStringAlignmentPhase.ComputeReplacement`, which ignores tabs (`rg -n "GetLeadingSpaceCount\|Math.Max\(0\|GetColumnAfterTabReplacement" src/Reihitsu.Formatter`) | The measuring statement has one owner. The writer disagrees with it: finding 10 |
| Per-line shift | `RawStringLineShifts` | single owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ArgumentAnchoredBehindATabIndentedCommentOnAHoleLineAlignsAfterTheTabReplacement` | `"b"` is aligned under `"a"` at its post-cleanup column, on pass 1 and pass 2, under LF and CRLF | A column difference | `AssertRuleResult(source with tabs, expected)` | adequate |
| `NestedLiteralOpeningATabIndentedHoleLineOf{Aligned,Misaligned}LiteralIsAlignedAfterTheTabReplacement` | The inner literal is aligned under its opening quotes after cleanup, on both passes | A column difference | `AssertRuleResult` | Adequate. Both shift right or not at all: finding 10 (FC-003) |

## Merge interaction

- **`CleanupPhase` (thoenissen/Reihitsu#1456).** The byte-order-mark change touches only whitespace that starts the text, which lies before line 0's first token. A hole line or a nested opening line is never line 0. `NormalizeWhitespaceTabs` is unchanged, so the parity under Guard scope holds.
- **The documentation-comment phase (thoenissen/Reihitsu#1456).** The `///` lines in a hole (finding 3) are still stable after pass 1 on `f8866f6`. `DocumentationCommentLinesInHoleMoveWithTheTokenBelowThem` passes in the spec checker's merged-tree run.
- **Rule pages (thoenissen/Reihitsu#1462).** `documentation/rules/RH5104.md` and `RH5601.md` equal `origin/main`. The withheld fixes live in RH5104-006, RH5104-007, and RH5601-002, as the new policy asks.

## Failure classes dismissed (repair delta)

- **FC-001.** The tab-count span now equals the cleanup span.
- **FC-011.** The round adds no loop.
- **FC-013.** The repair changes columns only for tab-indented hole lines. No layout analyzer reports hole lines (first-round apply-fix results).
- **FC-031.** Closed with finding 9.
- **FC-032.** The cleaner's and the spec checker's runs are on the final production code. The merge changed no file of this run's code.
- **FC-033.** `AssertRuleResult` covers CRLF.
- **FC-035.** The expected values follow from FMT-RAW-007, FMT-RAW-008, and FMT-CLEAN-002, and the tests were watched red on `d50b936`.
- The remaining classes are as in the earlier rounds and have nothing new to reach.

## Hints

- **A, unchanged.** It is the follow-up from escalation 5. The repair of finding 10 makes the raw-string phase a third place that has to apply the tab rule, which strengthens the case for one shared helper.

## Commands

```shell
bash scripts/trace.sh <fixture> --passes 3 --no-install   # 10 earlier counterexamples and 4 new probes on f8866f6
```

I re-ran no test project.
