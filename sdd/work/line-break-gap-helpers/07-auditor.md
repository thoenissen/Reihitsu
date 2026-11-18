Role: auditor
Commit: eadb4807c1e6b31ac8c88a8a902d21c91d227e44
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `eadb4807c1e6b31ac8c88a8a902d21c91d227e44` is checked out with a clean tree. Base `origin/main` is `074661f9dccd0c1edf192d30e032b15856774a9b`, which matches `git ls-remote origin refs/heads/main` and is an ancestor of head. The diff matches the handovers: 4 production files under `src/Reihitsu.Formatter/Pipeline/LineBreaks/`, 2 new regression test files, 1 extended unit test file, 1 changed phase-level test, and `sdd/work/`. `scripts/lane-check.sh bug-fix` confirms the lane. No spec, change doc, rule page, or documentation is touched. I trusted the cleaner's build result and the roles' test results. I ran one experiment, described in finding #1.

## thoenissen/Reihitsu#1640

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | | | | | | _None for this issue._ | |

The requirements hold.
- `ChainLineBreakRewriter` keeps no join bookkeeping: `GetPendingToken` and `Replace` are deleted, and `CloseGap`/`CloseInnerGaps` only select gaps.
- Both chain paths join through `JoinGaps`: `NormalizeChain` passes its gaps and its line-break edits, and `JoinConditionalElementAccesses` passes its gaps only.
- Every layout decision reads the tree before the joins (`IsWrapped`, `StartsLine`, `HasInnerLineBreak`, `isRootLineIntact`, `HasLineBreak`, `IsBehindMultiLineCommentTail`). No token is found again by text or position (decision 1, FC-026).
- The `JoinGaps` summary now states the real precondition: order is free, and each token stands in front of at most one gap and behind at most one (decision 1, FC-031).

I checked the composition order independently. The old code applied the joins first and then the line-break edit on the pending token. The new code seeds the edits first and applies the joins on top. The two give the same tokens only if no (token, side) pair is written by both an edit and a join. I traced this against `src/Reihitsu.Core/FluentChain.cs:340-431`, `FluentChainLink.cs:51,56` and `FluentChainAttachedPart.cs`:
- **Edits write two sides only.** `EnsureLineBreak` writes `prev(op_i).TrailingTrivia`, or `op_i.LeadingTrivia` in the comment-tail branch, and only for i >= 1.
- **Join fronts.** They are: `DotToken`; the token in front of an attached part; attached-part tokens other than the last (attached-part `Tokens` end at `(`, `[` or a lone `!`); operator tokens other than the last; and `prev(op_0)`. None of them is `prev(op_i)` for i >= 1.
- **Join backs.** They are: a member name's first token; an attached part's first token; operator tokens with index >= 1; and `op_0`. None of them is `op_i[0]` for i >= 1.

So the sides are disjoint. Cells H (`C`: edit on its trailing side, join on its leading side) and I (`?`: edit on its leading side, join on its trailing side) hold both mixed cases. `ChainGapJoinTests` pins both in the full pipeline.

The goal "all formatter tests pass unchanged" is met for this issue. The one changed test (`SplitsMixedLineArgumentsUsingConfiguredLineFeed`) belongs to #1651's migration and asserts a single phase's internal output (decision 9).

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Chain gap selected for joining (`CloseGap`) | `previous.TrailingTrivia` + `next.LeadingTrivia` (`AreSeparatedByEndOfLine`, `WouldJoinAcrossUnjoinableTrivia`) | `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps` filter (`:147-149`) | Identical predicates over the same gap; unchanged from `main` | ok |
| `JoinGaps` `edits` precondition: an edit writes only a side that no gap writes | Per token: trailing trivia if the token is no gap front, leading trivia if it is no gap back | Chain edits (`EnsureLineBreak`, `:260-277`) | Disjoint, traced above | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Batch join of several gaps in one rewrite | `rg -n "JoinGaps\("`: owner `LineBreakTriviaUtilities.JoinGaps` (`:254`); callers `GenericConstraintLineBreakRewriter:147`, `ListElementInteriorJoinRewriter:282`, `ArgumentListLineStartRewriter:136`, `ChainLineBreakRewriter` (`JoinConditionalElementAccesses`, `NormalizeChain`). `rg -n "CreateJoinedTrailingTrivia\(\|RemoveLeadingEndOfLineAndWhitespace\("`: the remaining hits are single joins (`CollapseTokenToSameLine:124/133`, `CollapseSeparatorsToPreviousElementLine:183-184`, assignment, binary, ternary, switch `when`, initializer, the list parenthesis joins at `LineBreakListRewriter:101/176/213`). None keeps a pending-token map | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ChainGapJoinTests` (15 tests, cells G, H, I, J, K, L, B, comment/directive gaps, nested chain) | Exact final chain output | Any join or line-break edit lost, misplaced, or read from the joined tree (J, K) | `AssertRuleResult`: exact text under LF and CRLF, second pass, phase stability | adequate |
| `JoinGapsWithoutGapsReturnsSameNode` | No gap means the same node instance | A rewrite without gaps | `Assert.AreSame` | adequate |
| `JoinGapsJoinsGapWithMinimalSeparator`, `JoinGapsWritesSpaceWhereTokensWouldMerge` (CRLF) | FMT-BREAK-031 separator | Whitespace left, or tokens merged | exact `ToFullString` | adequate |
| `JoinGapsJoinsBothSidesOfOneToken` | A token at both ends of two gaps carries both joins | One join overwrites the other | exact text | adequate |
| `JoinGapsResultDoesNotDependOnGapOrder` | Order-free precondition | Reversed or mixed order gives different text | exact text, three orders | adequate |
| `JoinGapsLeavesOtherGapsUnchanged` | Only the given gaps join | Another line break removed | exact text | adequate |

The new `edits` parameter has no unit test of its own. Cells H and I falsify both composition directions through its only caller (see *Hints*).

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._ The issue's FC-015 copy is removed.

## thoenissen/Reihitsu#1651

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs:186-190` (`CollapseSeparatorsToPreviousElementLine`) | FC-015, FC-016 | – | same requirement | The issue names the second `LineBreakListRewriter` site, which is now `CollapseSeparatorsToPreviousElementLine` (moved by `c6d199c`). It still states the "end the line in front of the next token unless the gap already holds a line break" decision on its own: fact `HasTrailingEndOfLine(newSeparator) \|\| HasLeadingEndOfLine(nextToken)`, write `AppendEndOfLine(RemoveTrailingWhitespace(...))`. After this run, every other append site goes through `EndLineInFrontOf` and keeps the whitespace (decision 4), so the decision now has two statements with different whitespace handling inside the owner class. This is the drift the issue asks to remove. The fact is equivalent (`IsFirstOnLine` is `AreSeparatedByEndOfLine(previous, token)`, `src/Reihitsu.Core/SyntaxTokenPositionUtilities.cs:48-63`), and the final output is the same (FMT-CLEAN-001), so nothing is corrupted. But the diff proves no parity, and its stated reason for leaving the copy does not hold. `03-implementer.md` says migrating "would add phase-output churn to its nine callers' tests". I made the migration in a throwaway worktree: `nextToken != default` → `EndLineInFrontOf(newSeparator, nextToken, endOfLine)`, else append when `HasTrailingEndOfLine` is false. `scripts/test.sh --project formatter` then gave **5781 passed, 0 failed**: no test changes. Decision 3's "already shared" covers where the function lives, not who owns the decision. | Route the write at `:186-190` through `EndLineInFrontOf`, as at the chain, list and constraint sites, so the append family has one owner. Keep or fold the `nextToken == default` branch as the owner sees fit |
| 2 | low | `src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakTriviaUtilitiesTests.cs` `EndLineInFrontOfAppendsLineBreakBehindWhitespace` | FC-017 (tag as evidence) | – | PR-introduced | The test is tagged `[Requirement("FMT-BREAK-029")]` and asserts that whitespace is kept in front of a line break appended to the previous token's **trailing** trivia. FMT-BREAK-029 ¶1 (the MUST) covers only a line break written into the moved token's **leading** trivia. For a rewrite of the trailing trivia, ¶2 says the phase "may remove the whitespace" (`sdd/specs/formatter/line-breaks.md:1727-1729`). So the tag claims coverage of a requirement that does not demand the asserted value: it reports coverage that is not there, and it locks as "required" an internal choice the spec leaves open (decision 4). `06-spec-checker.md` makes the same misreading ("¶1 covers the chain, list and constraint sites"). | Drop the `FMT-BREAK-029` tag from this test, or replace it with a tag whose clause demands the value. The assertion stays as a characterization of decision 4 |

The other requirements hold.
- The chain, list and constraint sites write through `EndLineInFrontOf`.
- The chain's comment-tail branch stays chain-only and unchanged (decision 3; FMT-BREAK-019 ¶2, SHARED-COMMENT-012).
- The list's `hasExistingLineBreak` flag and its fact stay (decision 7).
- The constraint site's private check is gone. `EndLineInFrontOf` evaluates the same `AreSeparatedByEndOfLine(separator, next)` over the same span.
- Both helper branches have unit tests.
- Dropped as decided (decisions 3 and 5): the prepend family and the architecture-test idea.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Constraint: a later constraint already starts its line | `separator.TrailingTrivia` + `next.LeadingTrivia` (inside `EndLineInFrontOf`) | Removed private `AreSeparatedByEndOfLine(separator, next)` on `main` | Same predicate, same span | ok |
| List: the separator already ends its line | `HasTrailingEndOfLine(sep) \|\| HasLeadingEndOfLine(next)`, then the helper's `AreSeparatedByEndOfLine(sep, next)` | Each other | Equivalent: `next.GetPreviousToken()` is `sep`, and `GetFirstToken` skips zero-width tokens. The helper's check is always false on this path (decision 7) | ok |
| Chain: a line break is already in the gap | `HasLineBreak(prev, op)` = `AreSeparatedByEndOfLine`; then the helper repeats it | `EndLineInFrontOf` | Same predicate; the comment-tail branch is decided before the helper is reached | ok |
| Comment tail (`IsBehindMultiLineCommentTail`) | `op.LeadingTrivia` behind the last line-ending trivia | `EndLineInFrontOf` has no such branch | Intended difference (decisions 3, 6); unchanged from `main`; follow-up candidate 1 in `00-lead.md` | ok for this run |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| End the line in front of a token by appending to the previous token's trailing trivia, keeping an existing line break | `rg -n "AppendEndOfLine\("`: `EndLineInFrontOf` (`LineBreakTriviaUtilities.cs:61`, callers binary `:108`, ternary `:142/:180`, chain, list, constraint) **and** `CollapseSeparatorsToPreviousElementLine` (`:189`). `ChainLineBreakRewriter:270` is the comment-tail branch on the leading side, a different decision | **two owners**: finding #1 |
| Prepend family (`PrependEndOfLine`/`MoveTokenToNewLine`, `TokenGapNormalizer`) | `TokenGapNormalizer:140/216/289`, `LineBreakTriviaUtilities:28`, the rewriters the skeptic listed | Out of scope (decision 3); not audited further |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LineBreakInFrontOfTokenTests` (7 tests: cells A, M, N; block comment; already-broken list and constraint) | Exact final output at each migrated site, and the chain's comment-tail branch | A doubled line break (blank line), a missing break, a comment moved, or whitespace left at the end of a line | `AssertRuleResult` | adequate |
| `EndLineInFrontOf*` unit tests (7) | Both branches; CRLF; block comment; end-of-line comment; leading-trivia line break; multi-line comment (no comment-tail branch) | Wrong branch, wrong line ending, comment reordered | exact trivia comparison | adequate; tag issue in #2 |
| `LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed` (changed) | The inserted separator break is LF in an LF context, and `"b"` starts the next line | `\r` in front of `\n`, or no split | `Assert.MatchesRegex` with `[ ]*\n` plus `DoesNotContain("\r\n")` | adequate. Stronger than before (it now also checks the split). It asserts internal phase output only, so it is not a visible surface |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None new._ The comment-tail class (skeptic, decision 6) is unrelated to this run's change and is already a follow-up candidate. Finding #1 is a remaining second statement of the issue's own rule, reported as an ordinary finding.

## Whole run

### Visible output is decided
No test of a visible surface changes an expected value. Every new full-pipeline test pins `main`'s output; the regression files pass on `main`, per `02-test-implementer.md`. Rule pages are untouched. Two tests lock the member name behind a comment or directive between `.` and the name at the statement's column: `SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` and `DirectiveBetweenDotAndMemberNameKeepsItsLine` (decision 8). `sdd/specs/formatter/indentation.md:19` covers that column: "a continuation line that no requirement below owns ... currently take[s] the block column of the enclosing statement". So no asserted output is wrong against a spec, and no output question arises.

### Cost of special cases
No production code is added for a rare class. The comment-tail branch is kept by maintainer answer A (escalation 10, not declared), so FC-042 does not apply.

### Comments
The changed summaries describe the new code: `CloseGap`, `CloseInnerGaps`, `EnsureLineBreak`, `ApplyWrappedLayout`, the new inline comment in `NormalizeChain`, `JoinGaps`, `PlaceOneConstraintPerLine`, and `EnsureSeparatorsHaveEndOfLine`. The added source lines contain no `TODO`, no tracker number, and no commented-out code.

### Failure classes dismissed
- FC-001, FC-002, FC-003: guards traced above; both sides of each boundary are tested (unit tests for both helper branches; regression tests with and without a line break).
- FC-004, FC-019, FC-020, FC-021: no new join or move. The join filter is unchanged, and comment/directive gaps are pinned by `ChainGapJoinTests`. The block comment behind the previous token stays in front of the appended break (unit test and cell B).
- FC-005: the directive filter is unchanged (`WouldJoinAcrossUnjoinableTrivia`).
- FC-006, FC-022, FC-023, FC-024, FC-025, FC-029, FC-036: the change writes no tokens or literals, only trivia layout. Formatter output is unchanged.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-032, FC-043: no code-fix change. The code fixes that format chains or lists go through `ReihitsuFormatter`, whose final output is unchanged. `CleanupPhase` runs for every entry point (`FormattingPipeline.cs:159`), so node-level results are the same.
- FC-010, FC-011, FC-012: phase stability on final output is checked by `AssertRuleResult`. The line-break phase's own output now keeps whitespace in front of the line breaks it appends, which binary and ternary already did. A second run of the phase sees the line break and edits nothing. No caller detects change by reference identity (`rg ReferenceEquals` in `Pipeline/LineBreaks`: only `AttributeTargetFormattingRewriter:99`, unrelated). No later phase reads trailing-trivia shape by index (`rg "TrailingTrivia\.(Count|First|Last|\[)"`: only `BlankLineCollapser:157`, which uses gap analysis that ignores whitespace).
- FC-013, FC-014: analyzers are untouched and output is unchanged.
- FC-015, FC-016: finding #1.
- FC-017: tables above; finding #2.
- FC-026: no re-resolution. Edits and joins are keyed by original tokens and applied in one `ReplaceTokens`.
- FC-027: nested chains are unchanged (children are visited first), and `ChainInsideArgumentOfOuterChainIsJoinedOnItsOwn` pins it.
- FC-028: no entry-point-specific code.
- FC-031: summaries checked (see *Comments*).
- FC-033, FC-034, FC-035: no reproduction; expected values come from `scripts/format.sh` on the unpatched head (`02-test-implementer.md`).
- FC-037: no text-only claim.
- FC-038 to FC-042: no shape declared, and the precheck is unchanged.
- FC-044: no guard models a computed result.

### Architecture findings
_None._

### Hints
- `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` (`:229-234`) now records a replacement for every separator, also one whose trailing trivia comes back unchanged. So `ReplaceTokens` runs for every wrapped clause with two or more constraints, and the `replacements.Count == 0` exit now fires only for a single constraint. The output is identical and no caller compares by reference, so this is harmless. Filtering on `result != separator.TrailingTrivia`, or keeping the early `continue`, would restore the cheap path. Not a finding.
- `JoinGaps`'s `edits` parameter has no direct unit test. Its contract ("each join is applied on top of the edit given for the same token"; the edit-only return path when no gap is given) is falsified only through the chain's cells H and I. A unit test with an edit on each side of a gap token would pin the helper's contract independently of the chain.

### Follow-up candidates
_None_ beyond those in `00-lead.md`.
