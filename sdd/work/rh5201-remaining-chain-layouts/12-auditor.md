Role: auditor
Commit: 2ca825e11596491f365a29b5ad901dd4afb720b2
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `2ca825e`, base `origin/main` `d63eab4` (contained in the head, checked with `git merge-base --is-ancestor`). Diff: `git diff d63eab4 HEAD`. It is what the handovers 01-11 describe: production changes in `FluentChain`, `SyntaxTokenPositionUtilities`, `ChainLineBreakRewriter`, and the RH5201 analyzer, plus specs, the change doc, the rule page, and tests. Issue thoenissen/Reihitsu#893 was read. Goal 1 is implemented. Goal 2 is dropped by decision 2: no caller reaches it, because the precheck refuses those inputs.

Evidence I gathered myself. I wrote 12 fixtures with the shared header used in `02-skeptic.md` and ran them under LF and CRLF:
- `scripts/apply-fix.sh RH5201`: 22 fixed in one action and one iteration, 2 no-diagnostic, 0 not-converged.
- `scripts/format.sh` twice: the second pass formatted 0 of 12.
- `apply-fix.sh RH5201` on the formatter output: 24 no-diagnostic.

The fixtures covered:
- the first link held by an own-line `/* x` ⏎ `*/` whose `*/` stands in the root column (x01);
- an element-access prefix (x02);
- a chain in a lambda argument (x03);
- several later calls (x04);
- an own-line `//` between prefix links (x05);
- `#pragma` in front of a prefix link (x06);
- a comment starting behind code, then a wrapped prefix (x07);
- an unheld first-link break plus a prefix behind an own-line `*/` (x08);
- a later link behind an own-line `*/` (x09);
- a line break in front of an attached part (x10, not reported, joined);
- a conditional chain (x11);
- a `/** */` tail (x12).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:95` (`linkIndex > 0 &&`), `src/Reihitsu.Core/FluentChain.cs:136-137` (`FirstLink.StartsLine && IsFirstLinkBlocked`); tests in `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedFirstCallTests.cs` | FC-003, FC-017 | – | PR-introduced | Two conditions the run adds to RH5201 are tested on one side only (details below the table) | Add analyzer tests (LF and CRLF) for both cells (details below the table) |

**Finding 1 in detail.**

What goes wrong:
- (a) The `linkIndex > 0` exclusion of the new `*/`-tail clause has no test. That is the first link behind an own-line multi-line comment whose `*/` line starts in the anchor (root) column, for example
  ```
  var a = c
          /* x
          */ .Next.Self();
  ```
  (fixture x01). Today only `.Self` is reported, and the fix converges in one action. Without the exclusion, `.Next` would be reported, and the fix could never clear it, because the formatter never breaks in front of the first link (`CloseGap` keeps the held gap), so the fix would not converge (FC-007). No test in `src/Reihitsu.Analyzer.Test` holds a first link behind an own-line `*/` (`rg '^\s*\*/ ?[.?!]'` finds only later and prefix links).
- (b) The new analyzer use of `FirstLink.StartsLine && IsFirstLinkBlocked` is tested with the first link held below the root (`b`, F01, F02, `#if`) and with a plain break (`e`). It is not tested with a comment that blocks the first link on the root's line without a line break, for example
  ```
  var a = c /* x */ .Next.Self()
           .Self();
  ```
  Here `.Self` must not be reported. A predicate reduced to `IsFirstLinkBlocked` passes every RH5201 test.

Suggested change: add the two analyzer tests:
- x01 shape: only `.Self` reported, and the fix result is the formatter's;
- the root-line block-comment shape: not reported for the first call.

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs:75-79` (`IsMisplaced` summary) | FC-031 | – | PR-introduced | The summary lists "a link behind the first that stands behind the closing line of a multi-line comment" as misplaced. The code reports it only when the link starts a line, that is, when the comment stands below the token in front of the link. A link behind the `*/` of a comment that starts behind code (RH5201-002 example `e`, F03) stands behind such a closing line and is not misplaced | Name the qualifier in the summary: a link behind the first whose comment starts below the token in front of it. Comment-only |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The root's line does not reach the first invoked link (`FluentChain.IsRootLineShortOfFirstInvokedLink`) | Trailing trivia of the root's last token plus leading trivia of link 0's operator (`IsFirstLinkBlocked`, gated by `FirstLink.StartsLine`); for links 1..FirstInvoked-1, the gap in front of the operator (`IsFirstOnLine`) and the gaps between operator tokens (`HasInnerLineBreak(true)`) | The formatter's former private `isRootLineIntact` in `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` (`HasLineBreak` and `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` at link 0; `StartsLine \|\| HasInnerLineBreak(false)` for prefix links). It is now replaced by the same property | `HasLineBreak` and `IsFirstOnLine` both reduce to `AreSeparatedByEndOfLine` on the same token pair. `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia` delegates to the Core one. The only difference, `HasInnerLineBreak(false)` against `(true)`, is a comment, directive, or disabled text between a prefix link's operator tokens, which FMT-UNSUP-015 refuses, and the maintainer's answer Q4B decided it for RH5201. Formatter output was traced unchanged | ok |
| A link behind the closing line of an own-line multi-line comment is misplaced (RH5201 `IsMisplaced`, `linkIndex > 0 && StartsLine && IsBehindMultiLineCommentTail`) | The operator's leading trivia, scanned backwards to the nearest line-ending trivia | `ChainLineBreakRewriter.EnsureLineBreak`, which calls the same Core helper for every link at index 1 and later whose `lineBreaks` is true. For index 1 and later, `lineBreaks` is true whenever the link starts a line (prefix: `isWrappedByUser`; first call and later links: true). Link 0 goes through `CloseGap`, which never breaks | Same span and boundaries. The `StartsLine` gate equals "a line break stands between the token in front and the comment", because a multi-line comment in leading trivia always follows the line break that ended the previous token's trailing trivia. Q7 (`*/ /* y */`) and Q8 (`/* w */ /* x`) take the same path on both sides | ok (boundary untested, finding 1) |
| The first call does not start a line (RH5201 `IsMisplaced`, `linkIndex == FirstInvokedLinkIndex` branch) | `IsFirstOnLine` of the first call's operator | Formatter `lineBreaks[first] = isWrappedByUser \|\| IsRootLineShortOfFirstInvokedLink` | For a first call that starts a line, the analyzer judges the column and the `*/` tail, and the formatter keeps or breaks the line accordingly. For one that does not, both read the shared property | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The root's line reaches the first invoked link | `rg -n "isRootLineIntact\|IsRootLineShortOfFirstInvokedLink\|FirstInvokedLinkIndex" src --glob '!*Test*'` finds one owner, `FluentChain.IsRootLineShortOfFirstInvokedLink`. Consumers: `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks` and `RH5201…Analyzer.IsMisplaced`. RH5112 and RH5048 use only `IsFirstLinkBlocked` and `FirstLink.StartsLine`, which are their own decisions | one owner |
| A token behind an own-line multi-line comment's closing line | `rg -n "IsBehindMultiLine\|CommentTail" src --glob '!*Test*'` finds one owner, `SyntaxTokenPositionUtilities.IsBehindMultiLineCommentTail` (moved unchanged from the formatter). `rg "ContainsLineTerminator\("` and `rg "MultiLineCommentTrivia"` turn up no copy of this decision. `RH5204…Analyzer:107` (`endsMultiLineCommentOnLine`) and `LineBreakBlockRewriter` decide other constructs (indentation, statements) | one owner for this decision |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5201MethodChainsShouldBeAlignedFirstCallTests` (all 18 methods and `GetFixAllScenario`) | Exact diagnostic locations and count; exact fix result after a fixed number of incremental iterations; Fix All in one iteration | A missing, extra, or misplaced diagnostic; a fix result that differs or needs another pass | `Verify` with markup and `Diagnostics(id, msg, n)`, `NumberOfIncrementalIterations` exact, `NumberOfFixAllIterations = 1`, LF and CRLF | adequate, except the two untested cells of finding 1 |
| `RH5201MethodChainsShouldBeAlignedFormatterTests` (5 new) | Reported before; formatter output exact; analyzer-clean and stable on a second pass | Formatter output that differs, a diagnostic left on it, or a second-pass change | `VerifyFormatter`, LF and CRLF | adequate |
| `RH5201CommentExemptChainWithNonInvokedPrefixDotTests…` and `RH5201FluentChainLinkFormatterTests.VerifyCommentKeptFirstLinkAlignsChainToRootColumn` (changed) | The new first-call diagnostic on a comment-held first link | Count or location mismatch | `VerifyFormatter` | adequate; covered by answer 2A |
| `ChainLinkLayoutTests.FirstCallTheRootLineDoesNotReachStartsItsOwnLine` | SHARED-CHAIN-005 example: formatter output exact | Output that differs on LF, on CRLF, on a second pass, or in a phase | `AssertRuleResult` | adequate (characterization; formatter unchanged) |

Visible output (check 10): every changed expected value is covered by an answer.
- The new diagnostics and fix results: Q1, Q2, Q3, Q6, Q7, Q8.
- Not reported for a held inner split: Q4B.
- Rule page: Q5.
- Every other new test asserts `main`'s output unchanged: F15, F03 shape, F12, and the formatter characterization.

## Failure classes dismissed
- FC-001, FC-002: traced under *Guard scope*. Every qualifier of RH5201-006 and of the RH5201-002 `*/` clause is an explicit condition: "between the first link and the first invoked link" is the loop bounds, "no comment … keeps it" is `HasInnerLineBreak(true)`, "keeps the first link on its own line" is `StartsLine && IsFirstLinkBlocked`, and "below the token in front" is the `StartsLine` gate.
- FC-004, FC-019, FC-020, FC-021: the change adds no rewrite; the fix output is the unchanged formatter's. Comment and directive gaps were exercised by F08, example 2 `b`, x05, x06, x07, x12, Q7, and Q8, under LF and CRLF.
- FC-005: `IsFirstLinkBlocked` and `HasInnerLineBreak(true)` use `WouldJoinAcrossUnjoinableTrivia`, which covers directives and disabled text. A directive ends the backward scan of `IsBehindMultiLineCommentTail` (`EndsLine`).
- FC-006, FC-018, FC-022 to FC-026, FC-029, FC-036: no rewrite, no rename, no literal handling. Diagnostics are located on operator tokens.
- FC-007, FC-008: tests pin incremental iterations and Fix All. Fixtures x01 to x12 converge in one action. The open member is finding 1(a).
- FC-009, FC-043: no fix registration changed; `FindUnsupportedSyntax` use unchanged.
- FC-010, FC-011, FC-012: formatter output unchanged (equivalence traced under *Guard scope*). The second format pass over the fixtures formatted 0 files.
- FC-013: formatter output of the 12 fixtures is RH5201-clean (24 no-diagnostic). Every input the formatter breaks is reported, through the shared predicates.
- FC-014: no new rule.
- FC-015, FC-016: one owner each (*Policy ownership*). Sibling shapes: prefix, call-less, and first-call links behind `*/`, conditional, `!.`, `!?.`, parenthesized root, element-access prefix, lambda argument.
- FC-027: nested chain in a lambda argument (x03) is reported and fixed. Each chain is its own `FluentChain`.
- FC-028: no entry point changed. The property reads only tokens inside the chain node, the same in a detached and in a document node.
- FC-030: fix formatting scope unchanged.
- FC-031: finding 2. `FluentChain.IsRootLineShortOfFirstInvokedLink`, `IsBehindMultiLineCommentTail`, and the `GetWrappedLayoutLineBreaks` summaries match the code.
- FC-032: tests were run after the last compiled change by the implementer and the cleaners. The validator runs the default set next.
- FC-033: CRLF is a `DataRow` on every new analyzer test; `VerifyFormatter` and `AssertRuleResult` run CRLF.
- FC-034: the issue's own shape (`declaration.Declaration` ⏎ `.Variables.Select(…)`) is tested.
- FC-035: the 04 tests were written red against unchanged production code. The 09 tests (Q7, Q8) were written after the code, but their locations come from the maintainer's answers, and their fix results equal the unchanged formatter's output.
- FC-037: no text-only claim.
- FC-038 to FC-042: no shape declared, and the precheck is unchanged.
- FC-040: `IsBehindMultiLineCommentTail` accepts `MultiLineCommentTrivia` and `MultiLineDocumentationCommentTrivia` alike (x12 `/** x` ⏎ `*/` is reported and fixed).
- FC-044: the guards read trivia kinds directly and model no computed result.

## Architecture findings
_None._ The skeptic's FC-015 risk was resolved by decision 3. The decision has one owner in `Reihitsu.Core`, read by both the formatter and RH5201.

## Hints
_None._

## Follow-up candidates
_None._
