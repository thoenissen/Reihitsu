Role: auditor
Commit: 6db7bcc
Verdict: pass, text cleanup

My prompt named the issue, the slug, the lane, the branch and head, the `origin/main` SHA, the handovers, and the probe. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head 6db7bcc0cba5bcdaa8cd1cb42c4a7adf6d1f785d; `origin/main` and `git ls-remote origin refs/heads/main` are both 6d574e1159eef2628a716c472a569e37d9676532, and it is an ancestor of the head. The diff (`6d574e1...HEAD`) is the one the handovers describe: `TokenRewriter` (new), `IndentationRewriter`, `CleanupPhase`, `TabReplacementPhase`, five test files, and the run's handovers. The merge commit brought in only `FluentChainCodeFixHelper.cs` and a note from `main`, outside the formatter. The Release build in `src/Reihitsu.Cli/bin/Release` is newer than the head commit.

**Equivalence evidence (differential, executed).** I built `main` (6d574e1) in a temporary worktree and ran both CLIs with `--force` over the same perturbed copies of every `*.cs` file under `src/` (1956 files). There were six variants:

- every line's indentation stripped, LF and CRLF;
- indentation converted to tabs, plus trailing ` \t` on every fifth line, LF and CRLF;
- trailing `  \t ` on every third line, LF;
- every line shifted right by 1 or 3 spaces, LF.

Of the 11736 files, each build formatted 8612, skipped the same 3046 for syntax errors, and left the same 78 alone. `diff -r` of the two output trees is empty, so the output is byte-identical on every formatted file. Main took 2:57, the head 1:47, on 4 cores. The worktree is removed.

**Static argument.** Roslyn's `SyntaxReplacer` visits trivia only when the replacement set holds trivia or something inside structured trivia. Neither holds for `DescendantTokens()` of a non-structured root, or for the tab site's filtered subset. So the `rewritten` argument was always the unvisited original token, as `03-implementer.md` says. `TokenRewriter` (`visitIntoStructuredTrivia: false`, `VisitToken` without `base`) hands each callback that same original token, in the same order. It visits the same set, missing and zero-width tokens and the `EndOfFileToken` included, and it returns the input node when nothing changes, as `ReplaceTokens` does. In `ApplyIndentation`, `triviaItem.FullSpan.Start` is read before `triviaItem` is reassigned, so it equals the old `originalTrivia[triviaIndex]`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Formatter/Pipeline/TabReplacement/TabReplacementPhase.cs:24-26` (summary of `Execute`) | FC-031 | – | PR-introduced | The new text says "Only the tokens and directives that hold a tab in their whitespace are rebuilt, in one walk of the tree (`TokenRewriter`), so the cost grows with the number of tokens". The directives are not rebuilt in that walk. `ReplaceDirectiveTriviaTabs` rebuilds them afterwards with a separate `ReplaceTrivia` call whose set is the directives that hold a tab. The summary attributes both passes to the one walk. Comment only: `pass, text cleanup` applies, because it reaches no compiled behavior, diagnostic, or rule metadata | Attribute the one walk to the tokens only, and say that directives holding a tab are rebuilt afterwards by their own replacement |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which tokens the tab pass rebuilds (`HasWhitespaceTab` in the `TokenRewriter` callback) | the token's own leading and trailing trivia, judged on the input token | the shortcut `root.DescendantTokens().Any(HasWhitespaceTab)`, and the old filter `.Where(HasWhitespaceTab)` | same predicate on the same tokens of the input tree (`DescendantTokens()` and the rewriter visit the same set) | holds |
| Whether a token is skipped by the indentation phase (`token.IsMissing`) | the token of the input tree | old `original.IsMissing` | identical, since `original` was the same token | holds |
| Which tokens are visited at all | tokens of the code, not inside structured trivia | `ReplaceTokens(root.DescendantTokens(), …)` (`descendIntoTrivia: false`) | `visitIntoStructuredTrivia: false`, and `VisitToken` does not descend into trivia | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rewriting every token of a tree through a callback | `rg -n "override SyntaxToken VisitToken" src` shows `TokenRewriter` plus the phase-specific rewriters `HorizontalSpacingRewriter`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineCollapser`, `BlankLineRegionDirectiveRewriter` (each owns its own phase's decision, not a generic callback walk). `rg -n "ReplaceTokens\(.*DescendantTokens\(\)" src` leaves only `TabReplacementPhase.cs:150` (one directive's structure, bounded) | one owner |
| Whitespace-tab replacement | `TabReplacementPhase.ReplaceWhitespaceTabs` (unchanged; still "the one statement of the rule") | unchanged |
| Line-ending normalization (sibling site, not changed) | `LineEndingNormalizationPhase.Execute` and a private copy in `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync` | pre-existing and untouched by the run; see *Follow-up candidates* |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `IndentationRewriterWholeFileTests` (7) | every line of a large file, and the lines around directives, regions, disabled text, delimited and documentation comments, raw strings, a missing token, and the first and last token, gets today's column | any dropped, misplaced, or extra indentation edit changes `ToFullString()` | `LayoutComputer.Compute` + `IndentationRewriter.Apply`, exact `Assert.AreEqual`, LF and CRLF | adequate |
| `CleanupPhaseWholeFileTests` (6) | whitespace in front of every line break, at the end of the file, and at the start of the file is removed; disabled text and literals are kept; a detached root loses only its line breaks; cancellation throws | the same, plus a missing `OperationCanceledException` | `CleanupPhase.Execute`, exact output, LF and CRLF | adequate |
| `TabReplacementPhaseWholeFileTests` (9) | tabs in code whitespace and between directive tokens are replaced, tabs in text that keeps them stay, both sides of the filter and of the shortcut are exercised, a detached root, cancellation | exact output; the test implementer's three mutations each failed tests | `TabReplacementPhase.Execute`, exact output, LF and CRLF | adequate |
| `LargeFileFullPipelineTests` (3) | the whole pipeline, `FormatNode`, and `FormatNodeInDocumentAsync` indent and clean a large input | exact output; `AssertRuleResult` adds a second pass and phase stability | `AssertRuleResult`, `ReihitsuFormatter` entry points | adequate |
| `LargeTabIndentedFileTests` (2) | a tab-indented file or node formats like its space-indented twin | exact output, plus equality with the space twin | `AssertRuleResult`, `FormatNode` | adequate |

No test asserts time (decision row 2). The scaling evidence is the measurements in `03-implementer.md`, and the equivalence evidence is these tests plus the differential under *Scope*.

## Failure classes dismissed
- FC-001, FC-002, FC-003: the only predicate the run touches is `HasWhitespaceTab`. It is unchanged and is applied to the same tokens, and both sides of it are tested (`ReplacesTheTabsOfEveryOtherMember…`).
- FC-004, FC-005, FC-019, FC-020, FC-021: no line is joined and no token is moved. The visited set excludes structured trivia, exactly as before. Directives and disabled text are covered by the whole-file tests and by the differential (repository sources with `#if`, `#region`, and `#pragma`).
- FC-006, FC-022 to FC-027, FC-029: no syntax-specific decision changes. Raw strings and `placedColumns` are covered by `MovesRawStringLinesUnderTheOpeningQuotes…` and by the differential, since the repository's tests hold many raw strings.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-036, FC-043: no code fix changes. Code fixes reach the changed phases only through the node-level entry points, whose callbacks receive the same token as before. The implementer reports the analyzer suite green (10932).
- FC-010: line-ending stability is unchanged. CRLF is covered by every new test and by two differential variants.
- FC-011, FC-012: no loop and no neighbouring-phase interaction changes.
- FC-013, FC-014: no analyzer change.
- FC-015, FC-016: one owner of the rewrite walk. The sibling sites are handled under decision row 3 (*Hints*).
- FC-017, FC-033, FC-034, FC-035: the expected values are generated or written from the requirements and pinned on unchanged code. The tests passed before the change (`02-test-implementer.md`, `04-test-implementer.md`). The self-hosting weakness the skeptic named is covered by tests where the phases actually rewrite, and by the differential.
- FC-028: the callback receives the token of its input tree for any root, so detached and document-scoped targets behave as before. Both are tested (`LargeFileFullPipelineTests`).
- FC-031: checked. Every changed summary and parameter doc describes the new call shape, except finding #1. The unchanged private helpers (`CleanLeadingTrivia(…, original)`, `StripTrailingWhitespaceAtEndOfLine(…, originalToken)`) still describe the token correctly: it is the token of the input tree.
- FC-032: no earlier project result is reused. I relied on the implementer's suite runs after 06d658a and on my differential at 6db7bcc.
- FC-037: not a text-only change.
- FC-038 to FC-042: no shape is declared unsupported, and the precheck is untouched.
- FC-040: the documentation mode changes only whether `///` is structured trivia. Neither walk visits structured trivia. Covered by the `DocumentationMode.None` tests.
- FC-044: nothing is modelled.

Checks 9 to 12: no edit falls outside the run's scope. No visible expected value is added or changed: every new test pins output that `main` already produces, and no rule page changes. No special case or rare-class guard is added. No architecture finding.

## Architecture findings
_None._ The cause is the cost profile of a Roslyn API applied to a set that grows with the input. No two components own one decision.

## Hints
- **Sibling line-ending site, the PR statement.** `03-implementer.md` states that the `LineEndingNormalizationPhase.cs:31`/`:45` cost is "about 4 % of the total before this fix". That share is now larger. I re-ran the probe (`mixed` variant, 40 % foreign line endings) on the head, best of 3:
  - the line-endings sub-phase took 25.8 → 30.7–36.8 → 118.7–119.0 ms for N = 400 → 800 → 1600;
  - the total at N = 1600 was 821–839 ms, so the sub-phase is about 14 % of a run.

  The reason it can stay is that the input is rare: mixed line endings are what RH5604 reports, so per `AGENTS.md` (*Special cases do not grow the source*) they are rare by definition. Its set also grows only with the foreign line endings. The PR should state that reason and the post-fix share, not the pre-fix 4 %.
- The issue's "Done when" also lists `TabReplacementPhase.cs:151` (now `:150`). It replaces one directive's own tokens, a bounded set; the PR should say so in its per-site statement.

## Follow-up candidates
- **Line-ending normalization has two owners (FC-015, pre-existing).** `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.ApplyCodeFixAsync` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs:41-66`) is a private copy of `LineEndingNormalizationPhase.Execute` (`src/Reihitsu.Formatter/Pipeline/LineEndings/LineEndingNormalizationPhase.cs:24-45`). It has the same two replacement calls and the same quadratic set. Structural cause: one rule stated in two places. It is unrelated to this run: the run changes neither file.
