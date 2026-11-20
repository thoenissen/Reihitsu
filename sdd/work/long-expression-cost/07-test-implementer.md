Role: test implementer
Commit: facfa67 (base of this repair; the tests are committed on top of it)
Verdict: tests written

Repair round 1, worklist: audit findings 1 and 2 of 06-auditor.md. The prompt carried no intended fix, suspected cause, or hoped-for verdict; the auditor's suggested changes were treated as suggestions and re-derived from measurements below. Finding 3 (BOM) is the cleaner's; I did not touch the BOM of `LongExpressionCostTests.cs`. The new Core test file is written with a BOM, as `.editorconfig` asks.

## Finding 1 — time arm of `LongExpressionCostTests`

**Measured on the head before the change** (Release, `--logger "console;verbosity=detailed"`): idle, three runs, time ratios 5.1 to 14.6 at a bound of 16, base samples of 5 to 13 ms. Under six busy loops on the 4-core sandbox, two runs: one spurious failure (`ArgumentListGrowsLinearly`, CRLF, 9 ms vs 225 ms, ratio 23.9) and several ratios of 14 to 15.6. Finding 1 reproduces.

**What the harness does now** (`AssertCostGrowsLinearly`, `Measure`; sizes, factor 8, and bound 16 unchanged, so the reproduction is still at the reported scales):

1. **Samples of about equal length.** A base sample formats the base input `GrowthFactor` (8) times in a row and reports the cost per formatting; a large sample formats the large input once. With linear cost both samples last about as long (about 40 to 250 ms), so a scheduler pause or a garbage collection weighs on both alike instead of dominating a 6 ms base run.
2. **Alternating rounds, equal counts.** A round takes one base sample, then one large sample; the cheapest of each size counts. At least 3 rounds, at most 9; after 3, the rounds stop once memory and time are within the bound. A slow period of the machine lands on both sizes.
3. **Two clocks.** Each sample records the process's processor time (`Environment.CpuUsage.TotalTime`) and the wall-clock time; time counts as grown only when **both** ratios exceed the bound (`Math.Min(processorTimeRatio, elapsedRatio)`). Processor time ignores time other processes take from the machine; wall-clock time ignores work the runtime does on other threads (measured: idle, the processor-time ratio alone spiked to 15.4 while wall-clock stayed 7.9 — background work on other threads). A quadratic cost raises both (below).
4. Allocation arm unchanged in substance (per-formatting allocation of the cheapest sample; `GC.GetAllocatedBytesForCurrentThread`).

Not used: `GC.Collect` before each sample (blocked by Sonar S1215 as an error, and no suppression convention exists in the repository); a larger base input (the chain tests cannot grow the large depth past 800 without risking the deep-spine stack overflow already recorded as a follow-up, and a test-host stack overflow kills the whole run); an absolute budget (lead decision 3).

**Measured after the change**, asserted ratio = min(processor, wall):

| Condition | Runs | Highest asserted time ratio | Failures |
|---|---|---|---|
| idle | 1 (24 arms) | 9.8 | 0 |
| 8 busy loops on 4 cores | 3 (72 arms) | 14.5 (one arm; next highest 11.1) | 0 |
| concurrently with the full analyzer suite (`scripts/test.sh --project analyzer`) | 2 (48 arms) | 11.4 | 0 |
| whole formatter project (`--project formatter`, 6136 tests) | 1 | – | 0 |

Allocation ratios unchanged: 7.8 to 9.6. The class takes about 14 s idle (was about 6 s), about 34 s under heavy load.

**Still fails on the unchanged code for the right reason**: with the production files of 3e94566 reverted to 3e94566^ (temporarily, restored with `git checkout HEAD`), `ConditionalChainInTrueBranchGrowsLinearly` fails on allocation (10.1 MB vs 454.9 MB, ratio 45.2; time 40.4 / 40.1) and `ArrayInitializerOnOneLineGrowsLinearly` fails on time (processor 73.4 ms vs 4395.8 ms, ratio 59.9; wall ratio 60.1; allocation 7.8).

## Finding 2 — direct test of `SyntaxTokenNavigationUtilities`

New class `src/Reihitsu.Core.Test/SyntaxTokenNavigationUtilitiesTests.cs`. Every test parses (or builds) a root and asserts, for every token of `DescendantTokens(descendIntoTrivia: true)` — directive, disabled-text, and documentation-comment tokens included — that `GetPreviousToken` and `GetNextToken` of the helper equal Roslyn's default `token.GetPreviousToken()` / `token.GetNextToken()`. Some tests add explicit assertions on the tokens at a fallback.

Mutation check (helper changed temporarily, restored from a copy, `git status` clean afterwards):

| Mutation of the helper | Tests that fail |
|---|---|
| zero-width candidate with trivia returned as is (no fallback) | `FindsNoTokenBeyondTheEndsOfTheText`, `AgreesWithRoslynAroundDirectivesAndDisabledCode`, `AgreesWithRoslynAroundDocumentationComments` |
| no ancestor reaches past the token: return `default` instead of Roslyn's call | `AgreesWithRoslynAroundDirectivesAndDisabledCode`, `AgreesWithRoslynAroundDocumentationComments`, `AgreesWithRoslynOnMissingAndSkippedTokens` |
| non-strict full-span comparison (`<=` / `>=`) | all 9 |

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LongExpressionCostTests` (all 12, harness changed only) | test reliability (audit finding 1); defect A and B reproductions as in 02 | `AssertCostGrowsLinearly` | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynOnLongLists` | FC-017 (audit finding 2) | `AssertAgreesWithRoslyn` | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynUnderCrlfLineEndings` | FC-017 | same | pass |
| `SyntaxTokenNavigationUtilitiesTests.SkipsZeroWidthTokensAsRoslynDoes` | FC-017 (omitted array sizes, omitted type arguments) | same, plus explicit asserts | pass |
| `SyntaxTokenNavigationUtilitiesTests.FindsNoTokenBeyondTheEndsOfTheText` | FC-017 (end of file behind a final comment; first and last token) | same, plus explicit asserts | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynAroundDirectivesAndDisabledCode` | FC-017 (`#define`, `#region`, `#pragma`, `#if`/`#elif`/`#else` with disabled text) | same | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynAroundDocumentationComments` | FC-017 (`///` and `/** */`, `cref`) | same | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynOnMissingAndSkippedTokens` | FC-017 (missing and skipped tokens, asserted present) | same | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynOnEmptyAndCommentOnlyText` | FC-017 | same | pass |
| `SyntaxTokenNavigationUtilitiesTests.AgreesWithRoslynOnDetachedAndRewrittenNodes` | FC-017, FC-028/FC-041 (detached expression, node replaced in a tree, subtree root) | same, plus explicit asserts | pass |

No requirement tags: no spec requirement states either the cost or the helper's contract.

## Scenarios (bug fix)

Not applicable in this repair round; the reproduction of 02 is unchanged in shape and size and still fails on the unchanged code (above).

## Not covered, and why

- **Existing tests whose output changes**: none; no production code and no expected output changed.
- **Finding 3 (BOM)**: routed to the cleaner by decision 6.
- **An operation count instead of time for defect B**: still none exists (02); the time arm stays the only signal for defect B, now with two clocks.
- **Code fix, Fix All, idempotency, token gaps, unsupported shapes**: nothing in this round touches them.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.FullPipeline.LongExpressionCostTests"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.SyntaxTokenNavigationUtilitiesTests"
```

Add `--logger "console;verbosity=detailed"` (without `--`) to the first one to see the per-arm memory, processor-time, and wall-clock ratios. Runs on this head: Core project 747 passed; formatter project 6136 passed.

## Follow-up candidates

_None._
