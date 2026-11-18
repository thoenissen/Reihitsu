Role: implementer
Commit: see the commit that adds this file (subject "Implementer: line-break gap helpers [skip ci]")
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

One existing phase-level test fails, as decision 4 anticipated: `LineBreakRewriterTests.SplitsMixedLineArgumentsUsingConfiguredLineFeed` locks the stripped text in the line-break phase's own output (see *Disputed tests or scope*). Every other test of the default run passes, and every handed-over test passes.

## thoenissen/Reihitsu#1640

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Utilities/LineBreakTriviaUtilities.cs` | `JoinGaps` gains an optional `edits` parameter (`IReadOnlyDictionary<SyntaxToken, SyntaxToken>`, default `null`): the caller's other token edits, computed from the original tokens and writing only a trivia side no gap writes. They seed the replacement map, the joins compose on top, and everything applies in one `ReplaceTokens`. An optional parameter rather than an overload, because an overload makes the existing `<see cref="LineBreakTriviaUtilities.JoinGaps"/>` crefs ambiguous (CS0419). The summary now states the real precondition (each gap closes two adjacent tokens, each token is in front of at most one gap and behind at most one, order is free) instead of "in text order" (decision 1, FC-031) | FMT-BREAK-031; FC-015, FC-026 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `GetPendingToken` and `Replace` deleted. `CloseGap`/`CloseInnerGaps` only select gaps (same filter: `HasLineBreak && !WouldJoinAcrossUnjoinableTrivia`) into a list. `NormalizeChain` collects the gaps and, separately, the wrapped layout's line-break edits computed from the original tokens; one `JoinGaps(node, gaps, lineBreakEdits)` applies both. `JoinConditionalElementAccesses` calls `JoinGaps(node, gaps)`. Every layout decision (`IsWrapped`, `StartsLine`, `HasInnerLineBreak`, `isRootLineIntact`, `HasLineBreak`, `IsBehindMultiLineCommentTail`) still reads the tree before the joins; no token is found again by text or position (decision 1) | FMT-BREAK-019, FMT-BREAK-020; FC-015, FC-026 |

Why the composition is safe (the skeptic's disjoint-sides argument, checked against the code): the joins write only `front.TrailingTrivia` and `back.LeadingTrivia`; the line-break edits write only `prev(op_i).TrailingTrivia` or `op_i.LeadingTrivia` for links `i >= 1`. `prev(op_i)` is the last token of the previous link or attached part, which is never the front of a selected gap, and `op_i` is never the back of one (only link 0's operator is). So the edit and the join on one token (cells H and I) touch different sides, and seeding the edits first gives the same tokens as the old "joins first, then compose on the pending token".

Decision 2's "Done when" holds: `ChainLineBreakRewriter` keeps no join bookkeeping (no replacement map of joins, no pending-token lookup); its joins go through `JoinGaps`; formatter output is unchanged (all full-pipeline tests pass).

## thoenissen/Reihitsu#1651

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ChainLineBreakRewriter.cs` | `EnsureLineBreak`'s gap-without-a-line-break branch writes `EndLineInFrontOf(previousToken, nextToken, eol)` instead of `AppendEndOfLine(RemoveTrailingWhitespace(...))`. The comment-tail branch stays chain-only and unchanged (decision 3) | FMT-BREAK-019, FMT-BREAK-029 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | `EnsureSeparatorsHaveEndOfLine` writes the separator's trailing trivia through `EndLineInFrontOf(separator, nextFirstToken, endOfLine)`; the `hasExistingLineBreak` flag and its fact stay (decision 3); the fact is evaluated twice (decision 7). Summary updated: whitespace in front of the break is kept for the cleanup phase | FMT-BREAK-012, FMT-BREAK-029 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs` | `PlaceOneConstraintPerLine` lets `EndLineInFrontOf` decide for every separator (its fact was exactly the helper's `AreSeparatedByEndOfLine`), so the private check is gone; a separator already followed by a line break gets its trailing trivia back unchanged. Summary names the helper | FMT-BREAK-037, FMT-BREAK-030 |

Decision 4 (option a): `EndLineInFrontOf` is unchanged; the migrated sites now keep the whitespace in front of the inserted line break in the phase's output, and the cleanup phase removes it (FMT-CLEAN-001). Final output is unchanged.

Not migrated: `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine:189` still writes `AppendEndOfLine(RemoveTrailingWhitespace(...))`. The skeptic marked it optional; it is already inside the shared class, and migrating it would add phase-output churn to its nine callers' tests for no owner change. The prepend family is out of scope (decision 3).

## Policy owners
- `rg -n "AppendEndOfLine\(LineBreakTriviaUtilities.RemoveTrailingWhitespace|AppendEndOfLine\(RemoveTrailingWhitespace" src/Reihitsu.Formatter` — one hit left: `LineBreakTriviaUtilities.cs:189` (`CollapseSeparatorsToPreviousElementLine`, see above).
- `rg -n "EndLineInFrontOf\(" src/Reihitsu.Formatter` — owner `LineBreakTriviaUtilities.cs:55`; callers binary (`:108`), ternary (`:142`, `:180`), chain (`:276`), list (`:518`), constraint (`:233`).
- `rg -n "JoinGaps\(" src/Reihitsu.Formatter` — owner `LineBreakTriviaUtilities`; callers `GenericConstraintLineBreakRewriter:147`, `ListElementInteriorJoinRewriter:282`, `ArgumentListLineStartRewriter:136`, `ChainLineBreakRewriter:246`, `:328`.
- `rg -n "CreateJoinedTrailingTrivia\(|RemoveLeadingEndOfLineAndWhitespace\(" src/Reihitsu.Formatter` — no pending-token join map outside `JoinGaps`; the remaining hits are single joins (binary, assignment, list parentheses, `CollapseTokenToSameLine`, `CollapseSeparatorsToPreviousElementLine`), which decision 2 leaves alone.

## Production diff size
`git diff --shortstat -- src` before commit: 4 files changed, 73 insertions(+), 89 deletions(-). No new type; no new public member beyond the optional parameter of the internal `JoinGaps`.

## Deleted special cases
_N/A._ (no shape declared; escalation 10 answer A)

## Tests run
- `scripts/build.sh --configuration Debug` — Build succeeded, 0 warnings.
- `scripts/test.sh --configuration Debug --project formatter` — Failed: 1, Passed: 5780 (the disputed test below; the handed-over `ChainGapJoinTests`, `LineBreakInFrontOfTokenTests`, `LineBreakTriviaUtilitiesTests` all pass).
- `--project analyzer` — Passed: 10639. `core` — 738. `cli` — 385. `architecture` — 198. `tooling` — 133. `playground` — 23.
- `--project selfhosting` — Failed: 1, Passed: 5854. The one failure is `WorkingDocumentsDirectoryContainsNoFiles`, which fails because this run's `sdd/work/line-break-gap-helpers/` exists; it is unrelated to the change and clears when the run's working docs are removed. Every formatter idempotency, stability and phase-alone stability row passes.
- `scripts/format.sh src/Reihitsu.Formatter/Pipeline/LineBreaks` — Formatted 0 of 25 file(s).

## Disputed tests or scope
1. **`src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakRewriterTests.cs:1206` `SplitsMixedLineArgumentsUsingConfiguredLineFeed`.** It runs the line-break phase alone on `Call("a", "b",⏎ "c");` and asserts `Assert.Contains("Call(\"a\",\n", result)`. The phase now writes `Call("a", \n"b",` — the space that stood behind the comma stays in front of the inserted LF, exactly decision 4 (option a, FMT-BREAK-029 ¶2: the cleanup phase is the single owner of whitespace at the end of a line). The test's purpose — the inserted break uses the context's LF, not CRLF — still holds (its second assertion, no `\r\n`, passes). The expectation would have to become e.g. `Call(\"a\", \n` or a check that ignores whitespace in front of the line break; the final formatter output is unchanged. Not changed by me; for the test implementer.
2. **Scope:** `JoinGaps` gains an optional parameter (internal method, three other callers unaffected). Within the decisions; reported for visibility.

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
