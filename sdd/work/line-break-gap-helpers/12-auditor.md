Role: auditor
Commit: fa5ee92143355d318e591361a320662924c4951e
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `eadb4807c1e6b31ac8c88a8a902d21c91d227e44` (audited in `07-auditor.md`). Head `fa5ee92143355d318e591361a320662924c4951e` is checked out with a clean tree. Base `origin/main` is `074661f9dccd0c1edf192d30e032b15856774a9b`; it matches `git ls-remote origin refs/heads/main` and is an ancestor of head. `scripts/lane-check.sh bug-fix` confirms the lane.

The repair diff (`2ea11cf..HEAD`) matches handovers 08 to 11:
- One production hunk in `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` (6 lines added, 9 removed, summary included).
- One tag removed in `LineBreakTriviaUtilitiesTests`.
- One tag replaced in `ChainGapJoinTests`.
- `sdd/work/` handovers.

The repair touches no spec, change doc, rule page, or documentation, and it did not grow into another surface. Evidence from `07-auditor.md` for files the repair did not touch is reused unchanged. I trusted the implementer's suite results (formatter 5781/0, analyzer 10639/0) and the cleaner's build (0 warnings). No finding turns on a re-run.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | `CollapseSeparatorsToPreviousElementLine` now writes `EndLineInFrontOf(newSeparator, nextToken, endOfLine)` (`LineBreakTriviaUtilities.cs:187`), and the private check and write are gone. `rg -n "AppendEndOfLine\("` now finds only the owner `EndLineInFrontOf` (`:61`) and the chain's comment-tail branch on the leading side (`ChainLineBreakRewriter.cs:270`), which is a different decision kept by decision 3 |
| 2 | closed | `EndLineInFrontOfAppendsLineBreakBehindWhitespace` no longer carries `FMT-BREAK-029`, and its assertion is unchanged as a characterization of decision 4. The test implementer also found the same misreading in `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace`, a member #2 did not name, and retagged it `FMT-CLEAN-001` (traced below). After the repair, no test of the run carries `FMT-BREAK-029`. The requirement keeps its coverage in `LineBreakResidueWhitespaceTests` (3 tests, outside the diff) |

## thoenissen/Reihitsu#1640

The repair touches nothing of this issue. `ChainGapJoinTests` changed only in a tag, which is covered under #1651 because it belongs to finding #2's class. `07-auditor.md`'s results for this issue stand unchanged.

### Findings
_N/A — the repair does not touch this issue's code._

### Guard scope
_N/A — no guard of this issue changed in the repair._

### Policy ownership
_N/A — unchanged since `07-auditor.md`._

### Assertion adequacy
_N/A — no assertion of this issue changed. The one retag is covered under #1651._

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

## thoenissen/Reihitsu#1651

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | | | | | | _None._ | |

**Parity of the migrated guard.** I re-derived it independently.
- **Old guard.** It appended the line break iff `HasTrailingEndOfLine(newSeparator) == false && (nextToken == default || HasLeadingEndOfLine(nextToken) == false)`.
- **Second conjunct.** `HasLeadingEndOfLine(next)` is `IsFirstOnLine(next)` (`LineBreakTriviaUtilities.cs:292-301`). Its `None`-previous branch cannot be reached because the comma stands in front of `next`. `IsFirstOnLine(next)` is `AreSeparatedByEndOfLine(next.GetPreviousToken(), next)` (`src/Reihitsu.Core/SyntaxTokenPositionUtilities.cs:48-63`). Its `Span.IsEmpty` exit cannot be reached: `GetFirstToken()` and `GetNextToken()` skip zero-width tokens, and the formatter refuses input with syntax errors.
- **Which comma.** `next` is read from the current `node`. Its previous token is the original comma, and that comma is not rewritten by an earlier iteration, which changes only comma `i-1` and the last token of element `i-1`. `newSeparator` keeps the comma's trailing trivia unchanged, because `RemoveLeadingEndOfLineAndWhitespace` touches only leading trivia.
- **Combined.** The two conjuncts are the two disjuncts of `AreSeparatedByEndOfLine(newSeparator, next)` (`src/Reihitsu.Core/SyntaxTriviaUtilities.cs:37-41`), and both use the same `EndsLine` predicate. For `nextToken == default`, the leading trivia is empty, so the helper appends unless the comma already ends its line, which is the old behavior.
- **What changes.** Only the write: the whitespace at the end of the comma's trailing trivia now stays in the line-break phase's output. FMT-BREAK-029 ¶2 permits this for a rewrite of the previous token's trailing trivia (`sdd/specs/formatter/line-breaks.md:1729`), and FMT-CLEAN-001 removes it in the final output.
- **Entry points.** Cleanup runs for every entry point, the node-level ones included (`07-auditor.md`, FC-007). Every list `CollapseSeparatorsToPreviousElementLine` receives is interior to the formatted node, so cleanup reaches the comma.
- **Final output is pinned.** `InitializerLeadingCommaTests` pins the final output of the three shapes the rewrite now writes differently: the append branch behind whitespace (`X = 3\n    , };`, `[1\n    , 2]`), the append branch behind a block comment (`, /* c */ Y = 4`), and the keep branch behind an end-of-line comment (`, // d`). It runs through `AssertRuleResult` with exact text, a second pass, and phase stability, and it stayed green.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Moved comma: a line break already separates it from the next token | `EndLineInFrontOf`: `AreSeparatedByEndOfLine(newSeparator, nextToken)` = `newSeparator.TrailingTrivia.Any(EndsLine) \|\| nextToken.LeadingTrivia.Any(EndsLine)` | Removed private `HasTrailingEndOfLine(newSeparator) \|\| HasLeadingEndOfLine(nextToken)` | Same span and the same `EndsLine` predicate, traced above. The `default` next token is handled the same way | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| End the line in front of a token by appending to the previous token's trailing trivia, keeping an existing line break | `rg -n "AppendEndOfLine\(" src/Reihitsu.Formatter`: `LineBreakTriviaUtilities.EndLineInFrontOf` (`:61`), called from binary, ternary, chain, list, constraint, and now `CollapseSeparatorsToPreviousElementLine` (`:187`). `ChainLineBreakRewriter.cs:270` writes the leading side behind a comment tail, a different decision (decision 3) | one owner |
| Whitespace in front of a line break | `rg -n "RemoveTrailingWhitespace\("`: owner `CleanupPhase.cs:197`. The other hits in the line-break phase are joins or gap normalizations that rewrite the trailing trivia, which FMT-BREAK-029 ¶2 permits. None of them is a site this run changed or newly depends on | unchanged; not a finding |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfAppendsLineBreakBehindWhitespace` (tag removed) | The append branch keeps the whitespace in front of the appended line break (decision 4, a characterization with no requirement) | The whitespace removed, or no line break appended | exact trivia comparison | adequate. Its summary matches what it asserts |
| `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace` (`FMT-BREAK-029` → `FMT-CLEAN-001`, `FMT-BREAK-019` kept) | Final output: `.C()` starts its own line behind `/* c */` and behind `a.B()   `, and no line ends in whitespace | Whitespace left at the end of `a.B()` (the input has three spaces there), the comment moved, or a missing break | `AssertRuleResult`: exact text, LF/CRLF, second pass | adequate. FMT-CLEAN-001 ("no whitespace before a line break") demands the asserted value |
| `InitializerLeadingCommaTests` (pre-existing, unchanged) | Final output of moved commas in every list kind of FMT-BREAK-038 | A doubled line break, whitespace left, or a comment moved off the comma's line | `AssertRuleResult` | adequate for the repaired write |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

## Whole run

### Visible output is decided
No expected value of a visible surface changed in the repair: only two `[Requirement]` attributes changed. Final formatter output is unchanged; the implementer's formatter and analyzer suites passed with no assertion touched. Rule pages are untouched.

### Cost of special cases
The repair removes a private copy and adds no production code for a rare class. No shape is declared, so FC-042 does not apply.

### Comments
The rewritten summary of `CollapseSeparatorsToPreviousElementLine` describes the new code (FC-031): the append through `EndLineInFrontOf`, the comment that stays in front of the line break, and the whitespace left for cleanup (FMT-CLEAN-001). The repair's added lines contain no `TODO`, no tracker number, and no commented-out code.

### Failure classes dismissed
The repair changes one write and two tags. Each class below names why it cannot reach that change. Every other class of the catalog was dismissed in `07-auditor.md` for code the repair does not touch, and that evidence stands.
- FC-001, FC-002, FC-003: the guard is equivalent over the same span (traced above), and both branches are pinned (`InitializerLeadingCommaTests`, and the `EndLineInFrontOf` unit tests).
- FC-004, FC-019, FC-020, FC-021: a comment behind the comma stays in front of the appended line break. The gap in front of the comma holds no comment, because the precheck refuses one there (FMT-UNSUP-009); this is unchanged.
- FC-010, FC-011, FC-012: the phase's output now keeps whitespace in front of the line break, as binary, ternary, chain, list, and constraint already did after the first round. A second run of the phase sees the line break and writes nothing. No later phase reads trailing trivia by index, apart from the gap analysis that `07-auditor.md` already traced.
- FC-015, FC-016: the policy now has one owner (finding #1 closed).
- FC-017: the retags make the tags evidence of the requirements they name (finding #2 closed).
- FC-007, FC-008, FC-009, FC-018, FC-028: no entry-point code is touched, and cleanup runs for every entry point.
- FC-026: the loop re-reads the current node each iteration, as on `main`. No token is found again by text or position.
- FC-031: covered under *Comments*.
- FC-037: no text-only claim.
- FC-038 to FC-042: no shape declared.
- FC-044: no guard models a computed result.

### Architecture findings
_None._

### Hints
The two hints of `07-auditor.md` are not addressed, and neither blocks:
- `PlaceOneConstraintPerLine` records a replacement for every separator.
- `JoinGaps`'s `edits` parameter has no direct unit test.

### Follow-up candidates
_None_ beyond those in `00-lead.md`.
