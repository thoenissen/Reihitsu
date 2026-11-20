Role: implementer
Commit: 314433a
Verdict: tests pass

Issue thoenissen/Reihitsu#896, lane behavior change. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/FluentChain.cs` | `GetAnchorLink` returns the first invoked link in front of the first link that starts a line; otherwise the last link in front of it; otherwise (the first link starts a line) the chain's first link. The root fallback (`FirstLink.StartsLine && IsFirstLinkBlocked`) is unchanged. XML summary rewritten to the new rule (FC-031). | SHARED-CHAIN-007 |
| `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` | Layout only: `scripts/format.sh --force src` realigned a wrapped chain under its last prefix link. | self-hosting |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs` | Layout only, same cause. | self-hosting |
| `src/Reihitsu.ArchitectureTests/General/TestScriptProjectSelectionTests.cs` | Layout only, same cause; no assertion or input changed. | self-hosting |
| `src/Reihitsu.Analyzer.Test/SelfHosting/RuleDocumentationExampleTests.cs` | Layout only, same cause; no assertion or input changed. | self-hosting |

The four reformatted files are exactly the ones `01-spec-author.md` listed; `git diff -w` over them is empty. `scripts/format.sh --force src` formatted 4 of 2011 files and nothing else.

`MethodChainAlignmentContributor` and `RH5201MethodChainsShouldBeAlignedAnalyzer` are unchanged: both read `GetAnchorLink`. Their summaries (FC-031) name `GetAnchorLink` without restating its rule, and `GetChainAnchorColumn`'s summary already describes an anchor that is a later link on the first dot's line, so they stay accurate.

## Policy owners
- `rg -n "GetAnchorLink" src --glob '!*Test*'`: owner `FluentChain.GetAnchorLink`; readers `MethodChainAlignmentContributor.GetAnchorColumn` and `RH5201MethodChainsShouldBeAlignedAnalyzer.GetAnchorColumn`.
- `rg -n "FirstInvokedLink" src --glob '!*Test*'`: `ChainLineBreakRewriter` (which links break, not the anchor) and RH5201's shared-line check; neither selects an anchor. No private copy of the anchor rule found.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (after this commit): 3 files changed, 12 insertions(+), 7 deletions(-); of these, `FluentChain.cs` is the only behavioral change (8+/3-), the other two are layout only. No new type or public member.

## Deleted special cases
_N/A._

## Tests run
Focused (Debug):
- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~FluentChainTests"`: Passed 56, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~PrefixChainAnchorTests|FullyQualifiedName~ChainLinkLayoutTests|FullyQualifiedName~ConditionalAccessWrapBetweenOperatorTokensTests"`: Passed 145, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5201|FullyQualifiedName~RH5112|FullyQualifiedName~RH5111"`: Passed 382, Failed 0.

Whole projects (Debug, after reformatting):
- core: Passed 771; formatter: Passed 6420; analyzer: Passed 11424; cli: Passed 407; architecture: Passed 198. No failures.
- selfhosting (`scripts/test.sh --configuration Debug --project selfhosting`): Failed 1, Passed 6081. The one failure is `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles`, which fails because `sdd/work/prefix-chain-anchor/` exists during the run; it clears when the lead deletes `sdd/work/` before publishing. Every analyzer row and every formatter idempotency/stability row passes.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
