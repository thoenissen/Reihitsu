Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/fixes-keep-comment-text/05-implementer.md`)
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions. Every handed-over test passes except two, whose expected output my change produces exactly but whose harness setup cannot assert it (see *Disputed tests or scope*).

## thoenissen/Reihitsu#1075

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.cs` | New `IsOpeningBraceOfInterpolation`; the brace predicate skips the `{` of an `InterpolationSyntax` (covers `$"""`, `$$"""` with `{{`, `$@"`, `@$"`). A `{` inside the hole's code is still checked | RH5025-006 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.cs` | New `StartsWithFooter`: reports only when nothing but whitespace, or whitespace plus the block body's `}`, stands in front of `while` on its line (the qualifier RH5027-001 always had; FC-002) | RH5027-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs` | New `FindLineEndingTrivia`: the first trivia behind the semicolon (then in the next token's leading trivia) that ends its line, through `SyntaxTriviaUtilities.EndsLine`. The fix inserts the empty line at that trivia's end, with the line break of the line it ends (SHARED-FIX-008). `CanRegisterCodeFix` withholds when no such trivia exists and the next token is on a later line, i.e. every line break lies inside a comment. The same-line branch (RH5011-005/006) is unchanged | RH5011-004, RH5011-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5030BlankLineAfterClosingBraceCodeFixProvider.cs` | New `CanRegisterCodeFix`; `RegisterCodeFixesAsync` withholds when `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(closeBrace, nextToken)` is false. `BlankLineCodeFixUtilities` untouched, so RH3208 is unchanged | RH5030-003 |

### Policy owners

- "Does a line break outside a comment end the line": `rg -n "static bool AreSeparatedByEndOfLine|static bool EndsLine" src` → one owner each, `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:37,49`. RH5011 and RH5030 reuse them; no private copy written.
- Interpolation brace: `rg -n "InterpolationSyntax" src/Reihitsu.Analyzer/Rules/Layout src/Reihitsu.Core` → no existing "is this `{` a hole brace" predicate (RH5417 and `StringInterpolationUtilities` ask other questions). The new private predicate is RH5025's own.
- RH5027 footer line: no other owner; the requirement is RH5027's alone. It reuses `FormattingTextAnalysisUtilities.IsWhitespaceOnly`.
- `FormattingTextAnalysisUtilities.GetStringLineIndices` / `GetNonFormattableLineIndices`: not touched (RH5023, RH5204, RH5602, RH6004, RH8301–RH8303 unchanged).

## thoenissen/Reihitsu#1261

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `FormatNodeInDocumentAsync` marks the detached target's last token with an annotation (`DetachWithAnnotatedLastToken`, keeping the tree's parse options) and writes the trailing trivia back through `WriteBackTrailingTrivia`: when the marked token is still the result's last token, the original trailing trivia is written as before; when the phases left it inside the result (FMT-STRUCT-001 inserted `}` behind it), the comments that token still carries are not written again — `RemoveLeadingComments` drops that many comments, each with the whitespace in front of it, so only the line break is written behind `}`. The mark is removed from the result | FMT-PIPE-010, SHARED-BRACE-002 |

Decision 9: the owner of the body's trailing comment is `ControlFlowBraceTransform` (unchanged, it already keeps the comment on the body's last token, as document formatting does); the entry point no longer writes that comment a second time and writes only what nobody else writes, the line end behind the target. No change in `StatementBracesCodeFixProviderBase` (no third owner).

`FormatNodeInDocumentWithContextAsync` keeps its old write-back. None of its callers (RH5111, RH7101, RH3208, the chain helper) passes a target whose last token the pipeline moves inside the result (02-skeptic, 04-test-implementer), so no caller reaches the duplication there (AGENTS.md, *Supported inputs*). Giving it the same annotation would need a second annotation threaded through `FormatContext`; I did not widen the change for an unreachable input. The lead may want the spec checker to confirm FMT-PIPE-010's "Both" reads as satisfied on that basis.

### Policy owners

- `rg -n "WithTrailingTrivia\(originalLastToken.TrailingTrivia\)" src` → now one hit, `ReihitsuFormatter.cs:331` (the `WithContext` entry point, see above); the `FormatNodeInDocumentAsync` write-back goes through `WriteBackTrailingTrivia`. The split-field path (`FormatSplitFieldInContext`) writes its own trailing trivia and is untouched.
- Comment test: reuses `SyntaxTriviaUtilities.IsCommentTrivia`.

### Checks beyond the handed-over tests

`scripts/apply-fix.sh`, LF and CRLF, my branch against `main` (stashed):
- RH3208 on `int L() => 1; // trail⏎L();`, `int L() => 1; /* a⏎b */ L();`, `int L() => 1; /* x */⏎return L();`: output byte-identical to `main` (the follow-up candidate's wrong output is unchanged, as the lead required). RH3208's expression-body transform does not leave the marked token carrying the comment, so the write-back behaves as before.
- RH5405 on `if (a)⏎ M(..); // one⏎else if (c)⏎ M(..); // two⏎b++;`: `main` wrote `} // two`; now `}`. `// one` and `// two` each once.

## Tests run

```
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5011|FullyQualifiedName~RH5025|FullyQualifiedName~RH5027|FullyQualifiedName~RH5030|FullyQualifiedName~RH5405|FullyQualifiedName~RH5406|FullyQualifiedName~RH5407|FullyQualifiedName~RH5412"
  Failed: 2, Passed: 219, Total: 221   (the two disputed tests below)
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterEntryPointTests"
  Passed: 32, Total: 32
scripts/test.sh --configuration Debug --project analyzer      Failed: 2, Passed: 7332, Total: 7334 (same two)
scripts/test.sh --configuration Debug --project formatter     Passed: 4521
scripts/test.sh --configuration Debug --project architecture  Passed: 115
scripts/test.sh --configuration Debug --project core          Passed: 660
scripts/test.sh --configuration Debug --project cli           Passed: 251
```

`scripts/format.sh` over the five changed files: 0 of 5 changed.

## Disputed tests or scope

Two tests cannot pass with any implementation that meets the spec, because of their harness setup, not their expected text:

- `RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests.VerifyFixAllHandlesSpanningCommentNextToPlainBreakAndWithheldBreak`
- `RH5030BlankLineAfterClosingBraceAnalyzerTests.VerifyFixAllLeavesWithheldBraceReported`

What they assert: the fixed code keeps the withheld diagnostic, marked as `{|RH5011:break|}` / `{|RH5030:}|}` in the fixed markup, and call `Verify(testCode, fixedCode)` without configuring the fixed state. The testing library's default fixed-state markup handling ignores markup for diagnostic IDs the code fix provider lists as fixable, so the expected list of the fixed state is empty and the run fails with `Diagnostics of fixed state … expected "0" actual "1"` — the one actual diagnostic is exactly the withheld one the markup marks (RH5011 at (20,13), RH5030 at (21,9)). What the spec says: RH5011-007 and RH5030-003 require that diagnostic to stay reported with no fix, which is what the code does. The repository's other withheld-in-Fix-All tests (for example `RH8309…Tests` line 1288, `RH8204…`, `RH8107…`) pass `test => { test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow; }`. I applied exactly that configuration locally to both tests, without touching any input or expected text: both pass, so the produced text matches the expected text byte for byte. The edit was reverted; the tests are committed as handed over. Route to the test implementer: add that configuration to the two tests.

Scope: five production files, within the paths the handovers name. No new public member or dependency; the new members are private.

## Architecture findings

_None._ Decision 9's finding is resolved as described under #1261.

## Follow-up candidates

_None_ beyond those in `00-lead.md`.
