Role: auditor
Commit: 21befc2a5211b543c5c906489bfd13ef80cb0e7d
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `12-auditor.md`, handovers 13 to 16, decision `squad/decisions/2026-10-30-line-start-fact-audit-1-copies.md`, the specs, and the repair diff.

## Scope

- Head: 21befc2 (checked out, clean tree).
- Base: `origin/main` = 802afbf, confirmed with `git ls-remote`. It is an ancestor of the head.
- Repair delta against 9167b0f (repair round 1). The repair stays inside the three first-audit findings and changes no spec, so this is a delta audit, not a full one.
- The diff matches handovers 13 and 14:
  - `CollapseTokenToSameLine` redirects whenever the preceding token has no text;
  - `DetermineLineBreakTolerantSpacing` and RH6005 use `AreSeparatedByEndOfLine`;
  - `ComputeTokenColumn` is gone, and its caller uses `SyntaxTokenPositionUtilities.GetColumn`;
  - the tests are as listed.
- `scripts/lane-check.sh behavior-change`: confirmed.
- I trusted the implementer's and the cleaner's build and test results, and ran no test project.
- Probes, on fixtures in the session scratchpad:
  - **Omitted-list joins, through `scripts/format.sh` twice, LF and CRLF:**
    - trailing whitespace on the `<,` line before a blank line;
    - a blank line in every gap (`Func<` / blank / `,` / blank / `,` / blank / `>`);
    - an omitted list in a chain;
    - an omitted list in an attribute argument;
    - two omitted lists in an array initializer;
    - `#if`/`#else` in the gap.
  - **RH6001 on formatter output:** the 11 checked keywords plus the `while` of `do`, each with `///` and with `//` behind the keyword. Run through `scripts/format.sh` twice, then `scripts/apply-fix.sh RH6001`.
  - **Raw-string check:** `dotnet fsi` against the built Roslyn. A whitespace-only content line keeps the whitespace beyond the closing indentation, so `WrappedListWithWhitespaceOnlyLineBeforeClosingAngleBracketIsJoinedInOnePass` really has 12 spaces on that line.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | The redirect no longer depends on where the line break stands. All seven listed members and my six extra shapes join in one pass, and the second pass makes no change, under LF and CRLF. The kept-wrapped `#if` shape is unchanged. |
| 2 | closed | Both listed failing members now hold through the shared fact: RH6021 trailing side, and RH6005 `operator` followed by `///`. Tests cover both, plus the RH6021 leading side, the base list, and the conversion operator. The RH6001 member that #2 listed as holding fails; see #4. |
| 3 | closed | `ComputeTokenColumn` is deleted, and no source or spec names it any more (only historical decision and change docs do). The remaining `ReihitsuFormatterHelpers` members each have an entry-point caller (`ReihitsuFormatter`, CLI). The class summary matches. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | high | `src/Reihitsu.Analyzer/Rules/Spacing/RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs:66-68` | FC-013, FC-015, FC-016 | #2 | same requirement | **RH6001 has one more private copy of "a line break separates two tokens", and it is blind to the line break of a `///` comment.** It checks only the keyword's trailing trivia, for `WhitespaceTrivia` or `EndOfLineTrivia`. A `///` behind the keyword is lexed into the leading trivia of `(` and carries its own line break, so RH6001 reports formatter output. Its `//` counterpart is not reported.<br><br>Example of formatter output, stable over 2 passes, LF and CRLF: `if/// x` / `(c)`.<br>- `apply-fix.sh RH6001` reports it and fixes it to `if /// x`, which rewrites formatter output.<br>- This is against RH6001-002 read with SHARED-BLANK-008: `(` starts its line, so a line break stands before it.<br>- It is also against RH6001-001's *Why*: "formatted code raises no RH6001".<br><br>Failing members (formatter keeps `keyword/// x`; reported and fixed under LF and CRLF):<br>- `if`, `while`, the `while` of `do`;<br>- `for`, `foreach`, `fixed`;<br>- `lock`, `switch`, `using`.<br><br>Holding members:<br>- `return`, `throw`, `catch`: the documentation-comment phase moves the comment onto its own line behind a blank line, so the output is `no-diagnostic`;<br>- every `//` counterpart: `no-diagnostic`.<br><br>Why it repeats #2: it is the same decision, failing between the same components. A spacing analyzer's private end-of-line check disagrees with the shared fact that the formatter uses.<br>- #2 listed RH6001 as holding, but its probe was `nameof/// x`. RH6001-002 says `nameof` is not checked, so that probe was not a member of RH6001 at all.<br>- `14-implementer.md` carried the same "unreachable on formatter output" claim into its follow-up candidates.<br><br>Other spacing rules hold:<br>- RH6002, RH6005, RH6021: shared fact;<br>- RH6003, RH6006, RH6007, RH6011 to RH6014, RH6018 to RH6020, RH6022: text scan of the adjacent character or same-line whitespace;<br>- RH6008, RH6016, RH6017: line table;<br>- RH6009, RH6010: character in front;<br>- RH6023, RH6024: `IsOperatorGapViolation` returns false for any character that is not a space or a tab. | Escalation 12 before another repair. If the maintainer chooses the narrow route: decide the line-break side with `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(token, nextToken)`, as RH6005 now does, and keep the whitespace check. Add an analyzer test per statement shape (the nine failing keywords, with `//` controls) and a `VerifyFormatter` test for one kept shape. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join across tokens without text (`CollapseTokenToSameLine` redirect) | The previous token in the node has an empty span → the gap from `token.GetPreviousToken()` (the token with text) to the token: its trailing trivia plus the token's leading trivia | Where the line breaks of the gap stand (both sides); `WouldJoinAcrossUnjoinableTrivia` over the same pair; `RemoveLeadingEndOfLineAndWhitespace` and `RemoveTrailingEndOfLineTrivia` over the same two lists | Yes. The span decided equals the span rewritten. The only callers with an empty previous token are the angle-bracket lists (omitted type arguments), whose `<` is always inside the node, so the new `ContainsToken` refusal cannot trigger there. | holds |
| RH6021 sides spaced (`DetermineLineBreakTolerantSpacing`) | `AreSeparatedByEndOfLine(previous, token)` and `(token, next)` | The RH6021 analyzer and fix call the same method. Formatter: FMT-SPACE-001 same-line gap via the same fact | Agree. A default previous token has empty trailing trivia, as before. | holds |
| RH6005 keyword spaced | Keyword trailing whitespace, or `AreSeparatedByEndOfLine(keyword, next)` | Formatter same-line gap (shared fact). RH6005 fix inserts behind a reported keyword only | Agree. Wider than before only by the next token's leading `///`, directive, or disabled text, each of which ends a line. A `/** */` in front of the next token is still reported, which is unchanged and outside RH6005-001's MUST-report set. | holds |
| RH6001 keyword spaced | Keyword trailing trivia only, `EndOfLineTrivia` kind | The same shared fact as RH6005 | No. A `///` in the leading trivia of `(` is not seen | finding #4 |
| Code-fix attribute column (`GetColumn` replacing `ComputeTokenColumn`) | `token.GetLocation().GetLineSpan()` vs `token.SyntaxTree.GetLineSpan(token.Span)` | — | Equivalent for a token in a tree, which `attributeList.Parent.GetFirstToken()` always is | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A line break separates two tokens | Found with `rg -n "EndOfLineTrivia" src/Reihitsu.Analyzer src/Reihitsu.Core`, then by reading each spacing rule:<br>- `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`: the owner;<br>- `AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing` and RH6005: now delegate to it;<br>- `AttributeTargetUtilities.StartsLine:355`: deferred by decision 3A;<br>- `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs:68`: a private copy.<br><br>The other hits count blank lines or line endings, which is a different policy: RH8303, RH5020, RH7207, RH5604, and `SyntaxTriviaUtilities` internals. | Finding #4 |
| Joining across tokens without text | `LineBreakTriviaUtilities.CollapseTokenToSameLine` only (`rg CollapseTokenToSameLine`) | one owner |
| A token's column in its tree | `SyntaxTokenPositionUtilities.GetColumn`. `ReihitsuFormatterHelpers.ComputeDocumentColumn` is a different decision: the document column of a formatting root, with the BOM handling of FMT-PIPE-017 | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ZeroWidthTokenLineStartTests`, seven `…IsJoinedInOnePass` rows | FMT-BREAK-014 joins an omitted list in one pass, wherever the blank or whitespace-only line stands | Still wrapped after pass 1, a changed second pass, or a phase reversal | `AssertRuleResult`: exact output, LF and CRLF, second pass, `PhaseStabilityCheck` | Adequate. Both sides of the old boundary are covered: line break in the trailing trivia only (the earlier seven rows), and also in the leading trivia (the new rows) (FC-003). |
| `WrappedTypeArgumentListWithTextAndBlankLineIsJoinedInOnePass` | Control for arguments with text | Same | same | adequate |
| RH6021 `…StrayDocumentationComment…IsNotReported` (4) and `…SingleLineComment…` controls (4) | RH6021-002 with the `///` line break on either side and on both colon kinds | An unexpected diagnostic | `Verify(source)` (no diagnostics, LF and CRLF through the base) | adequate |
| `VerifyOnlyLeadingSideIsFixedWhenStrayDocumentationCommentEndsTrailingSide` | RH6021-001 and RH6021-004: the reported tight side is fixed, the `///` side is untouched | The trailing gap rewritten, or no fix | `Verify(source, fixed, …)`: iterative and Fix All | adequate |
| RH6005 `…StrayDocumentationComment…IsNotReported` (2) and `//` controls (2) | RH6005-001 excludes a line break behind the keyword | An unexpected diagnostic | `Verify(source)` | adequate |
| RH6021 and RH6005 `VerifyFormatterOutputWith…IsNotReported` | FC-013: formatter output stays unchanged and is not reported | The formatter changes it, or the analyzer reports it | `VerifyFormatter(source)`, LF and CRLF | adequate |
| `SyntaxTokenPositionUtilitiesTests.GetColumn…` (2) | The column read for the moved caller | A different column | Direct call | Adequate. They are the deleted `ComputeTokenColumn` assertions retargeted. |
| Missing | RH6001-002 with `///` behind a checked keyword | — | — | Covered by finding #4 |

## Failure classes dismissed

- **FC-001, FC-002:** see *Guard scope*. The redirect's qualifier ("tokens directly in front without text") is the explicit `Span.IsEmpty` condition, and the joined span equals the inspected span.
- **FC-003:** both sides of the changed redirect condition are tested (see *Assertion adequacy*).
- **FC-004, FC-005, FC-019, FC-020, FC-021:**
  - `CanSafelyCollapseInteriorList` refuses an interior comment or directive before the collapse runs.
  - `WouldJoinAcrossUnjoinableTrivia` now inspects the wider pair, which is stricter, not looser.
  - The `#if`/`#else` probe stays wrapped and stable.
  - The RH6021 fix row keeps the `///` on its side.
- **FC-006:** the join yields `Dictionary<,>`, which is valid. The RH6005 and RH6021 changes only suppress reports.
- **FC-007, FC-008:** the RH6021 fix row uses `Verify` (iterative and Fix All). No fix provider changed.
- **FC-009:** RH6021 offers no action on a side it no longer reports, because the fix derives the sides from the same helper.
- **FC-010, FC-011, FC-012:**
  - every join probe was stable on the second `format.sh` pass under LF and CRLF;
  - the tests run `PhaseStabilityCheck`;
  - no loop changed.
- **FC-013:** examined. RH6021 and RH6005 hold. RH6001 is finding #4.
- **FC-014, FC-018, FC-023 to FC-026, FC-029, FC-036:** there is no new rule, rename, identifier logic, member-kind logic, or paired layout in the repair.
- **FC-015, FC-016:** see *Policy ownership*; finding #4.
- **FC-017:** see *Assertion adequacy*.
- **FC-022:** the whitespace-only raw-string line was verified to keep its whitespace (see *Scope*). No literal is rewritten.
- **FC-027:** omitted lists in a chain, an attribute argument, and an array initializer were probed and hold.
- **FC-028:** `FormatNode` and `FormatNodeInDocumentAsync` are untouched by the repair. The redirect is pipeline-internal.
- **FC-030:** the code-fix change swaps one column read for an equivalent one, and the formatting scope is unchanged.
- **FC-031:** the summaries and comments of `CollapseTokenToSameLine`, `DetermineLineBreakTolerantSpacing`, and the RH6005 guard describe the new code. The `ReihitsuFormatterHelpers` class summary matches its members.
- **FC-032:** my probes ran on the head build that `format.sh` and `apply-fix.sh` produced.
- **FC-033:**
  - `NormalizeLineEndings` and `AlternateLineEndings` drive the tests under both endings;
  - the probes were run as separate LF and CRLF fixtures;
  - `apply-fix.sh` runs both arms.
- **FC-034:** the join rows use the first audit's exact shapes (`Dictionary<,` / blank / `>`, `Func<` / blank / `,>`). The RH6021 and RH6005 rows use the exact formatter output from #2.
- **FC-035:** the expected values come from FMT-BREAK-014 (one joined line) and from the `//` counterparts, whose code path is unchanged.
- **FC-037:** no text-only claim was made.
- **Tab matrix:** no column is computed in the repair, apart from the equivalent `GetColumn` read, and no tab is counted.

## Architecture findings

- **Components:**
  - `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, the shared fact (SHARED-BLANK-008), used by the formatter's same-line gap and now by RH6002, RH6005, and RH6021;
  - the private trailing-trivia `EndOfLineTrivia` check in `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.OnSyntaxTree` (line 68).
- **Shared decision:** whether a line break separates a keyword from the token behind it.
- **Input showing the disagreement:** `if/// x` / `(c)`, which the formatter keeps (2 passes, LF and CRLF) and RH6001 reports and rewrites. The `//` counterpart is not reported.
- This repeats the class of #2, whose enumeration listed RH6001 with a probe (`nameof`) that RH6001 does not check. This is escalation 12, not another repair round.

## Hints

- Six test files the run added carry no UTF-8 BOM, although `.editorconfig` sets `charset = utf-8-bom`: `ZeroWidthTokenLineStartTests.cs`, `StrandedDocumentationLineStartTests.cs`, `CommentLineTerminatorIndentationTests.cs`, `LayoutContributorOwnershipTests.cs`, `TokenContainmentUtilitiesTests.cs`, and `TriviaListUtilitiesTests.cs`.
  - The repair added a BOM to `AdjacentTokenSpacingUtilities.cs`, but not to `ZeroWidthTokenLineStartTests.cs`, which it also edited.
  - About 228 tracked `.cs` files already lack one, so the convention is not enforced. This is not a finding.
- The `return`, `throw`, and `catch` shapes are relocated by the documentation-comment phase to `keyword` / blank line / `/// x` / `(…)`. That is pre-existing, outside this repair, and was not investigated further.
- The first-audit hints still stand unchanged: per-type contributor sets, `DocCommentElementNormalizer` and U+0085/U+2028/U+2029, and `typeof(Tuple<,,>).ToString(` / `)`.
