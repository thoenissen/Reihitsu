Role: auditor
Commit: 9167b0f01026a789403da6909a22493335d651af
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from handovers 00 to 11, the change doc, the eight `line-start-fact` decisions, the specs, and the diff.

## Scope

- Head: 9167b0f (checked out, clean tree).
- Base: `origin/main` = 802afbf, fetched again; it is an ancestor of the head.
- Full audit. This is the first audit of the run.
- The diff matches handovers 06 and 09: Core facts, `LineStartIndex`, the rewritten `IndentationRewriter`, `SetUnlessPlaced`, the zero-width join, the helper moves, and the `ReihitsuFormatterHelpers` split.
- `scripts/lane-check.sh behavior-change`: confirmed.
- I trusted the cleaner's build and the roles' test runs. I ran no tests.
- Probes:
  - Tools: `scripts/trace.sh`, `scripts/format.sh`, and `scripts/apply-fix.sh`, on fixtures in the session scratchpad, under LF and CRLF.
  - Stranded `///` inputs: a hole of a multi-line raw string, an `#region` behind the comment, an argument with a wrapped operator, and the end of the file. Each one matched its `//` counterpart and was idempotent.
  - Zero-width joins: a comment behind `>`, a chain behind the list, and lists inside a collection initializer. All held, except finding #1.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs:107-133` (`CollapseTokenToSameLine`, the bypass for tokens without text) | FC-010, FC-003, FC-001 | – | PR-introduced | **The join of an omitted type-argument list takes two formatter passes when a blank line stands in the gap.** The bypass runs only when `AreSeparatedByEndOfLine(previousToken, token)` is false for the zero-width predecessor. That predicate also reads `token.LeadingTrivia`. With a blank line, the token's leading trivia holds the second line break, so the bypass is skipped. The collapse then strips only the token's leading EOL and whitespace, and `HasTrailingEndOfLine(omitted)` is false. The line break in the trailing trivia of the token with text stays.<br><br>Input: `typeof(Dictionary<,` / blank line / `>);`. Trace, under LF and CRLF:<br>- pass 1, `LineBreakPhase`: the blank line is removed and `>` goes to column 0;<br>- pass 1, `IndentationPhase`: `>` goes to column 8;<br>- pass 2, `LineBreakPhase`: `typeof(Dictionary<,>);`.<br><br>Failing members:<br>- a blank line before `>`;<br>- a blank line before the first `,` (`Func<` / blank / `,>`);<br>- blank lines before several separators (`Func<` / blank / `,` / blank / `,>`);<br>- LF and CRLF alike.<br><br>Holding members:<br>- the line break only in the trailing trivia of the token with text (the seven join tests);<br>- an own-line or trailing comment in the gap (stays wrapped, `>` at 8, stable);<br>- a comment behind the closing `>` (joined);<br>- `int[,` / blank / `] r` (not joined, stable). | Decide the bypass on whether the directly preceding token has no text, not on where the line break lives. Then join the whole gap from the token with text to the token: its trailing trivia and the token's leading trivia. Add blank-line rows (`>`, first `,`, several `,`) to the FMT-BREAK-014 join tests in `ZeroWidthTokenLineStartTests`. |
| 2 | high | `src/Reihitsu.Core/AdjacentTokenSpacingUtilities.cs:31-34` (`DetermineLineBreakTolerantSpacing`, used by RH6021 analyzer and fix); `src/Reihitsu.Analyzer/Rules/Spacing/RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.cs:72` | FC-015, FC-016, FC-013 | – | same requirement | **Two more private copies of the decision "a line break separates two tokens" look only for `EndOfLineTrivia`.** Neither sees the line break of a `///` comment. Each reports formatter output that its own requirement excludes.<br><br>RH6021, against RH6021-002: the formatter output (stable over 2 passes) is `public C()` / `        :/// x` / `    base(1)`. `apply-fix.sh RH6021` reports it under LF and CRLF, `no-fix-offered`. The `//` counterpart `:// x` gives `no-diagnostic`.<br><br>RH6005, against RH6005-001: the formatter output (`format.sh --check`: 0 need formatting) is `public static C operator/// x` / `    +(C a, C b)`. `apply-fix.sh RH6005` reports it and fixes it in 1 iteration, which rewrites formatter output. The `//` counterpart gives `no-diagnostic`.<br><br>The issue's *Done when* requires every "starts its line" predicate to use the shared facts. Neither copy is named in decision 3A, which deferred only `AttributeTargetUtilities.StartsLine` and the analyzer-side terminator sets.<br><br>Members:<br>- RH6021, trailing side without a space before `///`: fails.<br>- RH6021, leading side: holds on formatter output, because the indentation space precedes `:`.<br>- RH6005, `operator` directly followed by `///`: fails.<br>- RH6016 (`DetermineSameLineAdjacentSpacing`, line table): holds.<br>- RH6001: `nameof/// x` is relocated by the documentation-comment phase, so formatter output cannot reach it.<br>- RH6002: fixed by this run, holds. | Route both through `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` / `EndsLine`, with an analyzer test per member. These are bugs against the existing RH6021-002 and RH6005-001, so no change-doc amendment is needed. The alternative, deferring them like 3A, is the maintainer's choice (see *Architecture findings*). |
| 3 | high | `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:93` (`ComputeTokenColumn`); its only shipped caller is `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs:111` | – (unaddressed issue requirement) | – | same requirement | **A code-fix-only member is still in `ReihitsuFormatterHelpers`.** The issue's *Done when* says `ReihitsuFormatterHelpers` holds entry-point code only. Its *Proposed Behavior* splits it into entry-point guards, a neutral facade, and the code-fix API. `ComputeTokenColumn` is code-fix API: no entry point calls it, and the class summary says "Code fixes … apply the same guards and computations through it".<br><br>`06-implementer.md` (*Scope*) kept it "because the tests name it". No decision records dropping this item; `00-lead.md` counts the split in the scope baseline. The change carries no behavior. | Move `ComputeTokenColumn` to a code-fix API type outside the entry-point helpers, and retarget `ReihitsuFormatterHelpersTests`. Alternatively, put dropping the item to the maintainer. Then align the class summary. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join across tokens without text (`CollapseTokenToSameLine` bypass) | The zero-width predecessor's trailing trivia plus the token's leading trivia (`AreSeparatedByEndOfLine(omitted, token)`) | Where the line break to be joined lives: the trailing trivia of the token with text in front (`token.GetPreviousToken()`) plus the token's leading trivia | No. The bypass is skipped when the token's own leading trivia holds a line break, and the trailing EOL of the token with text is then left | finding #1 |
| Token first on its line (`SyntaxTokenPositionUtilities.IsFirstOnLine`) | Empty span → false. Otherwise `AreSeparatedByEndOfLine` against `GetPreviousToken()`, which skips zero-width tokens | `StartsLine` / `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (text scan of every trivia) | The notion of a line break is the same: EOL, `///`, directive, disabled text; a multi-line comment's interior is excluded. They differ only in that `IsFirstOnLine` allows a comment in front of the token, which is the documented intent (RH5204 then applies SHARED-COMMENT-011) | holds |
| Trivia ends its line (`SyntaxTriviaUtilities.EndsWithLineBreak`) | Directive, disabled text, `///`: text ends with any of the five terminators | The deleted `BlankLineTriviaUtilities.EndsWithLineBreak` (also `/** */`, `\r`/`\n` only); `TokenGapAnalysis` text scan | Equivalent: `/** */` ends in `*/`. Wider only for U+0085/U+2028/U+2029 at the end of disabled text, which is now correct | holds |
| End-of-file line placed (`LayoutComputer.IsAloneOnEndOfFileLine`) | EOF token separated from its predecessor, or no predecessor | `LineStartIndex` trivia starts in the EOF leading trivia (SHARED-BLANK-002) | Agree. `#endregion`, a comment, and trailing whitespace without a final newline were all traced | holds |
| Rewriter's EOF whitespace branch | Empty-span token, pending whitespace, `StartsLineAtLeadingTriviaIndex(original, count - pending)`, layout present | `LineStartIndex` | Agree. Omitted tokens carry no trivia | holds |
| `HasLeadingEndOfLine` fallback for detached subtrees | Token with text, leading trivia that ends a line or is whitespace | Old: leading EOL or whitespace | Equivalent, plus `///` and directives | holds |
| `SetUnlessPlaced` | Refused once any `Set` placed the line in any sweep | `Set` (last writer wins) | Result does not depend on order (the inner `Set` wins either way). `format.sh --check src` was unchanged (09) | holds |
| Same-line gap in `HorizontalSpacingRewriter` | `AreSeparatedByEndOfLine` | RH6002 (same predicate); RH6021, RH6005 (EOL-kind copies) | Agrees with RH6002. Disagrees with RH6021 and RH6005 | finding #2 |
| Interior join (`ListElementInteriorJoinRewriter.CanJoin`), chain gap (`ChainLineBreakRewriter.HasLineBreak`) | `AreSeparatedByEndOfLine` | `IsFirstOnLine` | Agree | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A line break separates two tokens | `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (owner); `AttributeTargetUtilities.StartsLine:350` (follow-up, decision 3A); `AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing:31-34`; `RH6005…Analyzer.cs:72`, `RH6001…Analyzer.cs:68` (trailing-trivia EOL checks). Searched with `rg 'IsKind\(SyntaxKind.EndOfLineTrivia\)\)\|Any\(SyntaxKind.EndOfLineTrivia\)'` | Finding #2. RH6001 cannot be reached on formatter output |
| Token starts its line (strict) | `SyntaxTokenPositionUtilities.StartsLine` → `TokenGapAnalysis` | one owner |
| Trivia starts its line | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (`LineStartIndex`, `CommentIndentationContributor`, RH5204) | one owner |
| Line terminator | `LineEndingUtilities.IsLineTerminator` / `GetLineTerminatorLength`. Its remaining copies are analyzer-side only (decision 3A). `FormattingTextAnalysisUtilities.IsOperatorGapViolation:383` names `\n`/`\r` but returns false for any non-space/tab character, so it is not a second definition | one owner in the formatter and Core |
| Where a line's text starts (column) | `LineStartIndex` (`LayoutComputer.TryGetLineTextColumn`, `IndentationRewriter`) | one owner |
| Spacing of a joined gap | `SpacingPolicy` and `ListElementInteriorJoinRewriter.IsTightBefore`/`IsTightAfter` | Two owners, recorded and deferred by decision `…-join-spacing-pairs` |
| Byte order mark, token containment, trailing-whitespace removal | `ByteOrderMarkUtilities`, `TokenContainmentUtilities`, `TriviaListUtilities`. `LineBreakTriviaUtilities.RemoveTrailingWhitespace` is a forwarder | one owner each |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `StrandedDocumentationLineStartTests` (all) | The line behind a stranded `///` is placed as behind `//`; the comment stays | A different column, a moved comment, a second-pass change, or a phase reversal | `AssertRuleResult`: LF and CRLF, second pass, phase stability | adequate |
| `ZeroWidthTokenLineStartTests` joins (`WrappedUnboundGenericIsJoined`, …) | FMT-BREAK-014 joins omitted lists in one pass | A list still wrapped after pass 1 | `AssertRuleResult` | Adequate as written, but no row has a blank line in the gap, which is the failing cell of finding #1 (FC-003) |
| `ZeroWidthTokenLineStartTests` kept-wrapped, array-size, and EOF rows | Block column; end-of-file trivia placed with and without a final line break | Column 16, or `#endregion` kept at 4 | `AssertRuleResult` | adequate |
| `CommentLineTerminatorIndentationTests` | FMT-INDENT-006 for every terminator | The comment keeps column 12, or the terminator is rewritten | Own `AssertFormats`: LF and CRLF, exact text, second pass. `NormalizeLineEndings` rewrites only `\n`, so a lone `\r` survives under CRLF (FC-033 checked) | Adequate. No phase-level check, but the defect class is one phase |
| `ReihitsuFormatterNodeColumnTests` (two new) | FC-028 parity for a brace behind `///` | Detached and document results differ | `FormatNodeInDocumentAsync` + `AssertRuleResult`; `FormatNode` under LF and CRLF, second pass | adequate |
| RH5204/RH5203/RH6002/RH5201/RH5112/RH5048 additions | Diagnostics follow SHARED-BLANK-008; fixes converge | A missing or extra diagnostic, a fix needing two passes, a moved comment | `Verify` (iterative and Fix All); RH5048 uses its own LF+CRLF diagnostics check | adequate |
| `LayoutContributorOwnershipTests` | At most one alignment contributor places each line | Two contributor types recorded for one line | `LayoutModel.GetAlignmentContributors` | Adequate for the property as the issue states it. See *Hints* for the per-type granularity |
| `TokenContainmentUtilitiesTests`, `TriviaListUtilitiesTests` | Moved helpers keep their behavior | A different result | Direct call | adequate |
| `StrandedDocumentationTokenSpacingTests`, `HorizontalSpacingRewriterTests` retags | FMT-SPACE-001 with the `///` line break, both sides | The gap rewritten | `AssertRuleResult` / `AssertHorizontalSpacing` | adequate |

## Failure classes dismissed
- **FC-002:** checked through the guard-scope rows. The qualifiers "with text" and "whether or not the file ends with a line break" are explicit conditions (`Span.IsEmpty`; EOF tests with and without a final newline).
- **FC-004, FC-019, FC-020, FC-021:**
  - Every join checks `WouldJoinAcrossUnjoinableTrivia` over the gap it joins.
  - Omitted tokens carry no trivia.
  - Traced: a comment behind the stranded `///`, a directive, disabled text, an own-line comment, and a comment in or behind an omitted list. All are kept.
- **FC-005:** directives are part of `EndsWithLineBreak`, `ContainsUnjoinableTrivia`, and the `…AndDirective…` test.
- **FC-006:** the only syntax change is the join to `Dictionary<,>`, which is valid. No meaning changes.
- **FC-007, FC-008:** the fix tests use `Verify` (iterative and Fix All). Batch bases are unchanged.
- **FC-009:** RH6002 stops reporting. No fix becomes offered without an effect.
- **FC-011:** the alignment sweeps stay bounded by `MaxAlignmentPasses`, and `SetUnlessPlaced` adds no loop.
- **FC-012:** the zero-width ping-pong is gone. Every probe was stable at the phase level, except finding #1, which is a pass-level change, not a reversal.
- **FC-013:** examined. RH6021 and RH6005 are finding #2. RH5204, RH5203, RH6002, RH5201, and RH5112 are covered by tests.
- **FC-014, FC-018, FC-036:** no new rule, and no rename.
- **FC-017:** see *Assertion adequacy*.
- **FC-022:** a stranded `///` inside a hole of a multi-line raw string matches its `//` counterpart and is idempotent.
- **FC-023 to FC-026, FC-029:** no identifier, member-kind, or paired-layout logic changed.
- **FC-027:** probed. A nested argument with a wrapped operator behind `///` and omitted lists inside collection initializers and chains hold.
- **FC-028:** covered by `ReihitsuFormatterNodeColumnTests`. In `FormatNodeInDocumentAsync`, `keepsLeadingTrivia` is equivalent to the deleted `StartsOnNewLineIncludingDocumentation`.
- **FC-030:** no code fix changed its formatting scope. RH3208's predicate swap is equivalent, because a labeled statement always has a preceding `:`.
- **FC-031:** the summaries of `IsFirstOnLine`, `StartsLine`, `AreSeparatedByEndOfLine`, `EndsLine`, `CollapseTokenToSameLine`, `HasLeadingEndOfLine`, `ApplyIndentation`, `WriteLineStart`, `SetTokenIndentation`, `TryGetLineTextColumn`, `SetUnlessPlaced`, `CreateSeparator`, and `TrimGapBeforeToken` describe the new code. The `ReihitsuFormatterHelpers` summary matches its contents, which is finding #3.
- **FC-032:** no stale results were reused; my probes ran on the head.
- **FC-033:** LF, CRLF, and lone-CR fixtures keep their terminator in the tests.
- **FC-034:** the issue's three examples are the tests, with example 2 as amended.
- **FC-035:** the expected values come from the `//` counterpart, whose code path is unchanged.
- **FC-037:** no text-only claim was made.
- **Tab matrix:** no tab is counted. `LineStartIndex` measures columns after the preparation phase has replaced every tab.

## Architecture findings
- **Components:**
  - `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (the shared fact, SHARED-BLANK-008);
  - `AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing`, a private copy in `Reihitsu.Core`, used by RH6021 analyzer and fix;
  - the trailing-trivia line-break check in `RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer`.
- **Shared decision:** whether a line break separates two tokens.
- **Input showing the disagreement:** finding #2. `:/// x` / `base(1)` and `operator/// x` / `+(C a, C b)` are formatter output. The formatter keeps the gap (FMT-SPACE-001), and RH6021 and RH6005 report it.
- These are members of the class the skeptic escalated (decision `…-architecture-finding`, option B). Neither the skeptic's enumeration nor decision 3A names them, so this is escalation 12 before the repair.

## Hints
- `LayoutModel` records alignment contributors per line as a `HashSet<Type>`. One contributor class running on two nested nodes that both `Set` the same line is not detected. That matches the issue's wording ("one alignment contributor"), but it is weaker than per-node ownership. The per-line sets are also allocated in production only so that the tests can observe them. `_alignmentContributors.ContainsKey` is the only load-bearing use.
- `DocCommentElementNormalizer` now splits element content at U+0085, U+2028, and U+2029 too. After the preparation phase normalizes XML newline tokens, such a character should no longer appear inside a documentation comment. I derived this from code only and did not verify it.
- `typeof(Tuple<,,>).ToString(` / `)` keeps its empty argument list wrapped. This is unrelated to the run and not investigated.
