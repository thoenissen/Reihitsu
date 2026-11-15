Role: auditor
Commit: 7c095fe855603860e9b0ed3ef60ff821a503b874
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's "Cause" paragraph and "Required output" were read as proposals; the audit derives from the specs and the code.

## Scope

- Head 7c095fe (checkout verified), base `origin/main` c6d199c1fbb03a1d2a6731001c116c47073c5406 (`git ls-remote` agrees; it is an ancestor of HEAD). Full audit of `c6d199c..7c095fe`.
- The diff is what handovers 01–06 describe: one production file (`src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs`, +17/-5, `GetHeaderSplitIndex` and its summary), the FMT-USING-009 clarification and example in `sdd/specs/formatter/using-directives.md`, the new `src/Reihitsu.Formatter.Test/Regression/UsingDirectives/UsingDirectiveHeaderBlankLineTests.cs` (12 tests), 8 new RH7203 fix tests, and the run's `sdd/work/` files.
- `scripts/lane-check.sh bug-fix`: confirmed — claimed bug-fix, diff needs bug-fix.
- No test re-run: no finding turned on one. Build and test results of 04 and 05 are trusted.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

How the new predicate was established correct, statically:

- `GetHeaderSplitIndex` evaluates `TokenGapAnalysis.OfTriviaRange(leadingTrivia, firstSignificantTriviaIndex, i + 1).BlankLineCount` at each `EndOfLineTrivia` index `i` and splits behind the last one where the count grows.
- `TokenGapAnalysis.AnalyzeTriviaList` sets `lineHasContent = true` before it scans a comment's text, and `AnalyzeTriviaText` sets it to "text remains" after each internal break, so no blank line is ever counted inside a trivia's text. Every increment therefore happens at an `EndOfLineTrivia`, and the split always lands directly behind the line break that completes a blank line, never behind a content line.
- The range starts at a comment, so its first line always has content; a blank line needs a line break in front of it, as the old adjacency test did.
- New ⊇ old: two adjacent `EndOfLineTrivia` after a comment always count as one blank line in the shared measure, so every split the old code made is still found. The split moves only where the last blank line holds whitespace or sits below a `///` trivia whose text ends with its line break — exactly SHARED-BLANK-001's extra members. Lines inside block-comment text (`/* a⏎⏎ b */`) still do not count, as before and as SHARED-BLANK-001 demands.
- The loop bound changed from `Count - 1` to `Count`; a final `EndOfLineTrivia` that completes a blank line yields split `Count`, which equals the old "no split" result. The behavior doesn't change.
- `SplitOriginalFirstHeaderTrivia`'s back-over loop sees an `EndOfLineTrivia` at `splitIndex - 1` in every new case, so it leaves the split unchanged. `UsingDirectiveOrderingRewriter` adds the scope indentation (the header ends its line) and the FMT-USING-010 blank line (the remainder is led by a comment) as for an empty line.
- Directives, `#!`, and disabled text cannot reach the split: `UsingDirectiveOrderingSafety.CanSafelyReorder` refuses any block whose directives `ContainsDirectives`, the first directive's leading trivia included.

Class enumerated (axes: content of the last blank line — spaces, tab, several, empty, below `///`; position — header/comment boundary, between header comments, directly above the directive; other blank lines above or below; scope — compilation unit, namespace body; documentation mode; line ending; surface — whole document, RH7203 fix). Members m01–m13 of `02-skeptic.md` hold by the trace above. The tests cover m01–m12 on the formatter, and m01, m02, m04, m05, m07, m08, m10 (Parse and None) on the fix, all under LF and CRLF. Extra members traced and holding: block comment with an empty line inside its text followed directly by a comment (whole list stays the header, unchanged from `main`); `///` followed directly by `// About Linq` with no blank line (whole header, unchanged); file-scoped namespace (same trivia shape as the compilation unit).

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Last blank line among the comments of the original first directive's leading trivia (FMT-USING-009) | `leadingTrivia[firstSignificantTriviaIndex .. i]` for each `EndOfLineTrivia` index `i`; layout before the first comment is excluded and goes through `GetWhitespacePrefix`, as on `main` | `TokenGapAnalysis` (SHARED-BLANK-001's one measure), now called directly; `UsingDirectiveOrderingUtilities.HasBlankLineAbove` (FMT-USING-010, RH7207) for the blank line between directives | Same measure for the header. `HasBlankLineAbove` inspects the predecessor's trailing line break plus layout-only trivia (whitespace, line breaks) in front of the first comment. Over that span, counting line breaks equals the shared measure, a whitespace-only line included. For the moved comment, the rewriter takes the header's blank line as the one above it (FMT-USING-010), so both decisions agree on the same line | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| What counts as a blank line (SHARED-BLANK-001) in the using phase | `Reihitsu.Core/TokenGapAnalysis` (`OfTriviaRange`, `Between`, `IsBlankLine`), now used by `UsingLeadingTriviaBuilder.GetHeaderSplitIndex`. `Reihitsu.Core/UsingDirectiveOrderingUtilities.HasBlankLineAbove` counts `EndOfLineTrivia` over layout-only trivia and is shared by RH7207 and the rewriter. `rg` for an adjacent-`EndOfLineTrivia` test (`x[i + 1].IsKind(SyntaxKind.EndOfLineTrivia)`) over `src` outside tests: no hit remains. `rg EndOfLineTrivia` over `Pipeline/UsingDirectives`, `Core/UsingDirective*`, and the RH72xx analyzers and fixes: only line-break presence checks (`UsingTrailingTriviaBuilder`, `HasLineBreakBeforeSignificantTrivia`, `GetIndentationTriviaBefore`), no blank-line measure | One owner of the measure for the header split. `HasBlankLineAbove` is a pre-existing count over a span where it provably agrees with the shared measure, so it is not a finding (FC-015 dismissed for it) |
| Header split (FMT-USING-009) | `UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`/`GetHeaderSplitIndex`, sole caller `UsingDirectiveOrderingRewriter.OrganizeUsingDirectives:101`. `rg -i header` over the code fixes and Core: no other owner (RH8402's copyright header and RH8302/RH8303 documentation headers are other decisions) | single owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UsingDirectiveHeaderBlankLineTests.*` (12; m01–m12) | Exact whole-document output per FMT-USING-009/010/011, FMT-BLANK-010, FMT-CLEAN-001 | Any other first-pass text, e.g. `// About Linq` left at the top (today's stable fixed point) | `AssertRuleResult`: exact first pass under LF and CRLF, exact second pass, `PhaseStabilityCheck`; parse options passed for the `///` members | adequate. The fixed point is wrong but stable, so a check of stability alone would not catch it. The exact first-pass assertion avoids that weakness (FC-017, FC-033) |
| `CodeFixTakesLineOfSpacesAsEndOfHeader`, `…LineOfTab…`, `…KeepsLineOfSpacesBelowEmptyLine…`, `…EmptyLineBelowLineOfSpaces…`, `…KeepsHeaderCommentAboveLineOfSpaces…`, `…KeepsHeaderEndingInLineOfSpaces…` | Exact RH7203 fix result, header lines kept as written (FMT-USING-011), FMT-USING-010 blank line | Whitespace-only line dropped (m04 on `main`), comment not moved, line ending changed | `VerifyCodeFixWithEndOfLine`: SDK `Verify` (fix, Fix All, no remaining RH7203) plus exact `ApplyCodeFixAsync` text, per data row LF and CRLF | adequate |
| `CodeFixTakesEmptyLineBelowDocumentationCommentAsEndOfHeader` / `…UnparsedDocumentationComment…` | Same result in both documentation modes (FC-040) | Parse mode leaving `// About Linq` at the top (fails on `main`) | `Verify` with `ConvertEndOfLine` and `ApplyDocumentationModeParse/NoneToTestProject`; `CompilerDiagnostics.None` only silences CS1587 on `///` above a using | adequate. The None-mode test is a holding characterization |

## Visible output is decided

Every expected value is new; no existing expected value changed. No rule page changed.

- Whole document: FMT-USING-009 as clarified only cites SHARED-BLANK-001. That requirement is unchanged and binds the formatter: "whether it is empty or holds spaces", and the line below `///`. The other expected values are demanded by FMT-USING-010 (blank line above the moved comment), FMT-CLEAN-001 (spaces removed), and FMT-BLANK-010 (blank line below `///` removed). All of these are unchanged.
- Fix path: FMT-USING-011 (unchanged; "its own lines stay as written", so the spaces or tab stay) and FMT-USING-010 (unchanged).
- The issue's "Required output" (no blank line above `// About Linq`) is not asserted. FMT-USING-010 demands the blank line (lead decision 5). The PR should state the difference.

## Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021: the split only partitions the existing trivia list, and no trivia is dropped or reordered. On `main` the fix dropped the whitespace-only line (m04), and the change restores it. Directives and disabled text cannot reach the split (`CanSafelyReorder`). Comments in the gap are the subject of every test.
- FC-006, FC-022–FC-026, FC-029, FC-036: no rewrite of tokens, literals, identifiers, or members.
- FC-027: the scope axis (compilation unit, namespace body, file-scoped namespace) is traced, and the namespace body is tested.
- FC-028, FC-030: the using phase is reached only through document-scoped organization (`OrganizeUsingDirectivesInDocumentAsync`, whole document); no detached-node path reaches the split.
- FC-007, FC-008, FC-009: the fix tests run the SDK's iteration and Fix All checks. The action still changes the document, because it reorders.
- FC-010, FC-011, FC-012: `PhaseStabilityCheck` and the second pass on every formatter test; no loop-until-stable is added; the blank-line phase's collapse (m04) is consistent with the header split, with no insert-collapse cycle.
- FC-013, FC-014: no analyzer changed; RH7201–RH7207 do not judge header placement.
- FC-018: no naming fix.
- FC-031: the `GetHeaderSplitIndex` summary describes the new measure and split point. The summaries of `SplitOriginalFirstHeaderTrivia` and the rewriter's inline comments ("The header ends with a blank line …") remain true.
- FC-032: the analyzer test results in 04 were gathered after the production change (Debug, full analyzer project 9869/9869).
- FC-034: m01 is the issue's exact input on both surfaces.
- FC-035: expected values come from the spec examples and requirements (03), not from the patched code.
- FC-037: no text-only claim.
- FC-038, FC-039, FC-041, FC-042, FC-043: no unsupported shape declared or touched; the split does not move a comment into a declared shape's span (using directives hold no variable declaration, conditional, or list element).
- FC-001, FC-002, FC-003, FC-015, FC-016, FC-017, FC-033, FC-040: checked above. They hold.

## Architecture findings

_None._ The skeptic's FC-015 finding (a private adjacency test next to `TokenGapAnalysis`) is resolved by reuse in this run (lead decision 2). No new copy, prediction, or second statement of the rule was added.

## Hints

- `GetHeaderSplitIndex` re-runs the shared measure over a growing range at each line break, which is quadratic in the length of the original first directive's leading trivia. It runs once per reordered block, and the trivia is a file header of a few dozen lines at most, so it is not a hot path. The implementer recorded the trade-off (a linear form would need a new `TokenGapAnalysis` member or a private copy of its line state). No action needed.
- Spec wording: SHARED-BLANK-001 reads "MUST count as a blank line *only* a line … that holds nothing but whitespace". This puts an upper bound on "blank". The whitespace-only lower bound comes from "whether it is empty or holds spaces" and the *Why*. The run relies on that reading, which every handover shares. No visible output depends on another reading.
