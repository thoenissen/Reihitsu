Role: implementer
Commit: 970fe19 (production change; this handover is amended in the commit after it)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` | New private `RemoveWhitespaceThatEndsText`: removes the whitespace trivia at the end of a list that ends the text (through the existing owner `TriviaListUtilities.RemoveTrailingWhitespace`), keeping a byte order mark when the list starts the text and all of it was removed. Applied (1) to the `EndOfFileToken`'s leading trivia before `RemoveTrailingEndOfLineTrivia`, and (2) to the trailing trivia of the file's last real token when the next token is the `EndOfFileToken` without significant leading trivia. The detached-root arm (`nextToken` is `None`) is split off and still removes only line breaks (FMT-CLEAN-008 unchanged, lead decision 5). Summaries of the class, `CleanLeadingTrivia`, and `CleanTrailingTrivia` updated (FC-031). | FMT-CLEAN-005, FMT-CLEAN-009 |

Order is whitespace first, then line breaks: `CleanWhitespaceBeforeEndOfLine` has already removed every whitespace trivia in front of a line break, so after the trailing whitespace goes, only line breaks can remain at the end (e.g. `[comment, EOL, ws]` → `[comment, EOL]` → `[comment]`). Directive and disabled-text trivia are not `WhitespaceTrivia`, so `#pragma …\n···` keeps the directive's line break and `#endif···` is unchanged (characterization test still passes).

The byte order mark guard is load-bearing: disabling it fails the existing `ByteOrderMarkTests.ByteOrderMarkAloneBeforeLineBreakIsKept` (a file whose end-of-file leading trivia is just the byte order mark). A file holding only a byte order mark and spaces gives the same output as on `main` (byte order mark only), checked with `scripts/format.sh`.

`EndOfLineWhitespace.StripBeforeFollowingLineBreak` is not changed, so its node-level consumer (`ReihitsuFormatter.cs`) keeps its behavior (`GapInFrontOfATargetOnTheSameLineIsKept` passes).

## Policy owners
- `rg "static.*RemoveTrailingWhitespace" src`: `TriviaListUtilities.RemoveTrailingWhitespace` (Pipeline/Core/Utilities, reused here) and `LineBreakTriviaUtilities.RemoveTrailingWhitespace`, which only forwards to it. One owner.
- `rg "RemoveTrailingEndOfLineTrivia" src/Reihitsu.Formatter`: `CleanupPhase` (private, reused) and `LineBreakTriviaUtilities` (line-break phase's own); unchanged.
- Byte order mark: `ByteOrderMarkUtilities.StartsWithByteOrderMark` / `CreateByteOrderMarkTrivia`, reused.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 39 insertions(+), 7 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Cleanup"` — Passed: 61, Failed: 0 (all 16 previously failing tests in `EndOfFileWhitespaceTests` pass; the 8 guard tests still pass).
- `scripts/test.sh --configuration Debug --project formatter` — Passed: 5059, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 9506, Failed: 0.
- `scripts/test.sh --configuration Debug --project cli` — Passed: 279, Failed: 0.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
