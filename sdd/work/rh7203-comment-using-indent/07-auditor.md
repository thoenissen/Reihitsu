Role: auditor
Commit: 1c383897ec19d845dd9e0466845ac79546c60bde
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `1c38389` (the checkout matches it). Base `origin/main` is `6ab3d75`, the same as `git ls-remote origin refs/heads/main`, and the branch contains it. The diff is what the handovers describe: 2 production files (+30/−2), 9 test files, the FMT-USING-011 spec text, RH7201–RH7207 wording, `requirement-ids.txt`, and the work files. `scripts/lane-check.sh bug-fix` reports "confirmed — claimed bug-fix, diff needs bug-fix".

Checked against the specs:
- **FMT-USING-011** holds for every example and for every input class in the 01-skeptic matrix:
  - fixed: a, c, c1, c2, c4, c5, b3, f, g, nb, h, b1, b2, bl, t1, s1, u1;
  - holding: b, c3, e, d, s2.
- **SHARED-FIX-007** (unchanged) decides the block-namespace members. Maintainer answers 1A and 2A in `00-lead.md` decide s1, u1 and t1.
- **Issue #1746.** Its required output is met, and every other using-ordering fix that reproduced has a test. Two "Done when" items differ, and both differences are recorded as lead decisions 3 and 5:
  - The other rules are tested under LF only. All seven providers share one route, `OrganizeScopeUsingsAsync`, and the CRLF dimension is covered on RH7203 and on the entry point.
  - "the same text as whole-document formatting" is narrowed to the lines of the using block.

I constructed members beyond the tests and ran them with `scripts/apply-fix.sh RH7203` (fixtures under the scratchpad, LF and CRLF, 20 of 20 fixed). Every member holds; the moved and the new first directive each keep the original first directive's line whitespace:
- mixed `\t  ` indentation;
- the new first directive sharing its line with the original first (`using System.Linq; using System;`);
- a whitespace-only line between header and directive;
- a header in column 0 above directives in column 4;
- a header with no blank line plus a comment that leads the new first directive;
- c3 below a blank-line header;
- `/* Header */` followed by trailing spaces;
- a two-line `///` header;
- an outer scope whose nested namespace is also misordered, where the rewriter sees a rewritten node;
- a new first directive with `/* x */` on its own line.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Header split: trailing line whitespace stays with the original first directive (`UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia`, lines 117–129) | Walks back from the FMT-USING-009 split index over `WhitespaceTrivia` only. It never goes below `firstSignificantTriviaIndex`. It then tests the trivia in front of that whitespace with `SyntaxTriviaUtilities.EndsLine` | The rewriter's `SyntaxTriviaUtilities.EndsLine(header.Last())` (`UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`, line 110) | Agree. When the split moves, `header.Last()` is exactly `leadingTrivia[lineStartIndex - 1]`. On the blank-line side the split already ends at an EOL, so both are true. In c3 (`/* c */ using`) the header ends in whitespace, so both are false. The header is never empty, because `lineStartIndex > firstSignificantTriviaIndex` | ok |
| The new first directive's prefix gets `scopeIndentation` (rewriter, lines 110–113) | `header.Last()` | The remainder's whitespace, which the moved directive keeps through `GetIndentationTrivia` / `GetIndentationTriviaBefore` | Agree. When the header ends its line, the directive's line starts after that line break. So `GetLineIndentation` (the whitespace in front of the token's line) equals the whitespace trivia left in the remainder. The tab and mixed fixtures show this byte for byte | ok |
| Column-0 scopes are unchanged | — | — | `scopeIndentation` is empty, and no whitespace follows the header's line break. Split, prefix and remainder are therefore identical to `main` (d and s2 tests, plus tracing) | ok |
| FC-003 boundary of the walk-back | — | — | Both sides are tested: whitespace after a line-ending header (a, the tab tests), no whitespace (d, s2), and a header that does not end its line (c3) | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Header split of the original first directive | `rg -n SplitOriginalFirstHeaderTrivia src`: one owner (`UsingLeadingTriviaBuilder`) and one production caller (`UsingDirectiveOrderingRewriter.OrganizeUsingDirectives`). `rg -il header` finds no other using-directive owner in Core, Analyzer or CodeFixes | one owner |
| Whether a trivia ends its line | `SyntaxTriviaUtilities.EndsLine` in Core, reused in both places. No private copy. The `///` structured line break is covered, which the implementer's first attempt with `EndOfLineTrivia` missed | one owner |
| Indentation of the original first directive's line | `UsingDirectiveOrderingUtilities.GetLineIndentation` (Core). It is used by the rewriter's `GetScopeIndentation` and by the RH5114 fix, which already used it before this run. `SyntaxTriviaUtilities.GetLineIndentationTrivia` and `BlankLineCodeFixUtilities.GetLineIndentation` serve other decisions | one owner for this decision |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 22 `ReihitsuFormatterUsingDirectivesTests` FMT-USING-011 tests (fix entry point) | The exact document text after `OrganizeUsingDirectivesInDocumentAsync` | Any line at a column other than the original first directive's whitespace, or any other change to the text | `AssertOrganizes`: exact string under LF and CRLF, through the fixes' own entry point (avoids FC-033) | adequate |
| Tab tests (formatter `IndentWithTabs`, RH7203 `Replace("    ", "\t")`) | The tab is kept as written (answer 2A) | Spaces or column 0 in front of a line | Same exact-text helpers | adequate |
| RH7203 matrix (11 tests × LF/CRLF) and the RH7201/7202/7204/7205/7206/7207 tests | The fix output equals the expected text in one application. Re-analysis finds no diagnostic (FC-007) | A column-0 line, or a second iteration | `VerifyCodeFixWithEndOfLine` / `Verify` (iterative fix and re-analysis) | adequate |
| `FixAllKeepsNamespaceIndentationOfTwoBlocksBelowHeaders` | Fix All gives the same result on both sides of the split in two scopes (FC-008) | One block left at column 0, or a stale span | `Verify` with the batch fixer | adequate |
| `UsingDirectiveRequirementTests` whole-document guards (a, h) | CLI output stays at column 4 and is stable | A changed column or blank line, or a non-idempotent second pass | `AssertRuleResult` (LF/CRLF, second pass, phase stability) | adequate |
| Hold-side tests (b, c3, e, d, s2) | Shapes that worked on `main` are unchanged | Any change | `AssertOrganizes` | adequate |

## Visible output decided
- **Block-namespace expectations** (spaces): demanded by SHARED-FIX-007, which this run does not change.
- **s1/u1** (indented file-scoped namespace and compilation unit): covered by maintainer answer 1A.
- **t1** (tab): covered by maintainer answer 2A.

Both answers are recorded under `Maintainer answers` in `00-lead.md`. No rule page changed.

## Failure classes dismissed
- **FC-004, FC-020, FC-005.** No comment is dropped or relocated: the change moves only whitespace trivia between header and remainder. Blocks containing directives are refused by `UsingDirectiveOrderingSafety.CanSafelyReorder` and never reach the split.
- **FC-006, FC-022–FC-026, FC-029, FC-036.** No syntax rewrite, literal, identifier or rename is involved.
- **FC-009, FC-018, FC-030.** The fixes' routing and action registration are unchanged, and the entry point still formats only the scope's using block.
- **FC-011, FC-012.** No loop or neighbouring-phase interplay was added. In whole-document formatting the indentation phase still owns the column, and the guard tests show no change.
- **FC-013.** The fix output now sits at the formatter's column, so no layout rule can report the edited lines. 01-skeptic found that RH5204 did not report even the column-0 line.
- **FC-014.** No new rule.
- **FC-028.** `FormatNode` / `FormatNodeInDocumentAsync` are not the fixes' path.
- **FC-038–FC-043.** No shape is declared unsupported and no precheck changed. The skeptic correctly found that declaring the shape would delete nothing.
- **FC-040** (documentation mode) as a general concern: `///` is handled whether it is parsed as structured trivia (`EndsWithLineBreak`) or as a plain comment followed by an `EndOfLineTrivia`. Both satisfy `EndsLine`.
- **FC-032.** The implementer ran the full formatter, analyzer and architecture suites after the compiled change, and the cleaner rebuilt in Release.
- **FC-035.** The expected values come from the spec examples and from the same block without the comment, not from patched code. They were watched failing on the pre-fix head (03-test-implementer).
- **FC-037.** No comment-only change was claimed.
- **FC-031.** Checked: the new inline comment, the `SplitOriginalFirstHeaderTrivia` summary and the `firstLeadingTriviaPrefix` parameter doc describe the new code. The `GetHeaderSplitIndex` summary still describes the pre-adjustment index, which is correct. No `TODO`, and no tracker references.
- **FC-001, FC-002, FC-003, FC-015, FC-016, FC-017, FC-019, FC-021, FC-027, FC-033, FC-034, FC-007, FC-008, FC-010.** Traced or constructed above. All hold.

## Architecture findings
_None._ One component, the using phase, decides the directive indentation. The two values it uses, the remainder's whitespace and `scopeIndentation`, read the same line by construction, so neither copies nor predicts the other. FMT-PIPE-030's design is unchanged.

## Hints
- **Confirmed, pre-existing, unrelated to this run.** A line holding only whitespace between a header and a comment that leads the original first directive is not taken as FMT-USING-009's blank line. The cause is that `UsingLeadingTriviaBuilder.GetHeaderSplitIndex` requires two adjacent `EndOfLineTrivia`, while the line is `[EOL][Whitespace][EOL]`. As a result, `// About Linq` stays at the top instead of moving with its directive. Probe: `scripts/apply-fix.sh RH7203` on the input below, LF and CRLF. The output keeps `// About Linq` above `using System;`.

  ```csharp
  // Header
      
  // About Linq
  using System.Linq;
  using System;
  ```

  SHARED-FIX-007 already treats a line holding only whitespace as a blank line. Whether FMT-USING-009 means the same is a follow-up candidate for the lead. It is not a finding of this run.
- **Spec wording.** FMT-USING-011 says "the whitespace that stands at the start of the line on which the block's original first directive stands". When that line starts inside a multi-line comment (`/* a⏎   b */ using X;`), `GetLineIndentation` follows the comment back to its first line, which is the sensible column. The wording does not say so. That shape needs a blank-line header to reach the new code, and it holds in practice. No action needed unless the spec author wants the wording exact.
