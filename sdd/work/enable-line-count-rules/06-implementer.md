Role: implementer
Commit: be29e3c8
Verdict: #1865: tests pass for production code (remaining failures are test code and the test decision 13 makes wrong, both for the next test-implementer stage); #1866: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It continues `05-implementer.md`; scope by decisions 12 and 13.

## thoenissen/Reihitsu#1865

### Changes

| File | What changed | Requirement |
|---|---|---|
| `Directory.Build.props` | Decision 13: the `AdditionalFiles` item of the root `reihitsu.json` removed; the comment above the package import says why (the analyzers in `binaries/` predate the `design` section) and that the self-hosting scan reads the file | decision 13 |
| `src/Reihitsu.Formatter/Utilities/TargetWriteBackUtilities.cs` (new internal static type) | `ReihitsuFormatter` (530 lines of code, over the repository's 500 after the 05 extractions) loses its write-back helpers, moved verbatim: `TryGetSplitFields`, `StartsLineInLayout`, `EndTargetLineAsFormatted` (now internal), `WriteBackSplitFields`, `ReplaceTargetByFields`, `TrimLeadingWhitespace` (private); new entry `ReplaceTargetBySplitFields`, which computes the member indentation and line break and calls the two write-back steps | 2A |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | Callers use the new type; `FormatSplitFieldInContext` (61) calls `ReplaceTargetBySplitFields` instead of building indentation, line break, and the two steps itself | 2A, RH2008 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs` | `OrganizeUsingDirectives` (63): leading trivia of a later directive built in new private `CreateLeadingTriviaOfLaterDirective` (indentation, blank line above comment, kept line break; comments moved with the code) | RH2008 |

Every changed file ran through `scripts/format.sh` (no change). FC-031: the summaries of `FormatSplitFieldInContext` and `OrganizeUsingDirectives` still describe what they do (behavior unchanged, steps delegated); the new members have their own summaries. I re-read the summaries of the 05 extractions in `FormatCommandHandler`, `FluentChain`, `ModifierOrderingUtilities`, `PrecheckPhase`, `ReihitsuFormatter`; they match their bodies. The four large moved types (`CasingRenameSafetyUtilities`, `ConditionalGroupUnitUtilities`, `DirectivePairSplitScan`, `UnnecessaryParenthesesTypeArgumentListUtilities`) were moved verbatim by 05 and not re-read line by line.

### State after this stage (05's "What is left")

1. Re-measured with the full self-hosting run (`DisplayName~` does not select DynamicData rows: "No test matches"). At 514bdc8d the scan listed 2 production methods (`UsingDirectiveOrderingRewriter.cs:63` 63, `ReihitsuFormatter.cs:705` 61) and 1 production type (`ReihitsuFormatter` 530) — the 05 estimates had missed them. All three are fixed; at be29e3c8 the RH2008 row lists 9 and the RH2111 row 11 violations, **all in test projects** (decision 12):
   - RH2008 (> 120): `RH8032…AnalyzerTests.cs:794` (190), `RH8033…AnalyzerTests.cs:563` (190), `RH5201…AnalyzerTests.cs:175` (270), `BlankLineRegionTests.cs:221` (148), `CommentBeforeBraceBlankLineTests.cs:189` (121), `:1327` (220), `CollectionExpressionAlignmentTests.cs:569` (141), `ObjectInitializerAlignmentTests.cs:351` (123), `ElseKeywordClosingBraceAuditFanOutTests.cs:26` (169).
   - RH2111 (> 3000): `RH3002…AnalyzerTests` 6946, `RH3004…` 4416, `RH2001…` 5269, `RH8201…` 3932, `RH5111…` 4492, `RH5202…` 3115, `RH7101…` 3053, `RH7309…` 4453, `RH4103…` 3308, `RH7109…` 5435, `HoleAnchorInStatementColumnAlignmentTests` 3522.
2. Every other analyzer row and every formatter self-hosting row passes over the refactored code (full run below).
3. Decision 13 carried out: the Debug build no longer reports RH0001 for `reihitsu.json`. The Release build (`scripts/build.sh`) fails only with `MSTEST0049` at `src/Reihitsu.SelfHosting.Test/Analyzers/LineCountSelfHostingTests.cs:173` (test code, error under `TreatWarningsAsErrors`); every production project builds in Release without a diagnostic.
4. FC-031: above.

### Policy owners

- `rg -n "EndTargetLineAsFormatted|TrimLeadingWhitespace|StartsLineInLayout" src -g '*.cs'` — one owner each, now `TargetWriteBackUtilities`; no other copy.
- `rg -n "HasLineBreakBeforeSignificantTrivia|CreateLeadingTrivia\(" src/Reihitsu.Formatter` — `UsingLeadingTriviaBuilder` stays the owner; the new helper only calls it.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 392 files changed, 3258 insertions(+), 6593 deletions(-). This stage: 3 files, +338 / −247 (moved code counts on both sides). New type: `TargetWriteBackUtilities` (internal, Formatter), a fifth type of the same kind as the four of decision 15.

### Deleted special cases

_N/A._

### Tests run (Debug, at be29e3c8's tree)

- `scripts/test.sh --configuration Debug --project formatter`: Passed! Failed 0, Passed 6115.
- `--project architecture`: Passed! Failed 0, Passed 198.
- `--project selfhosting`: Failed 4, Passed 5990 — `AnalyzerReportsNoViolationsOnOwnSourceCode` RH2008 and RH2111 (test code only, list above), `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` (decision 13), `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles` (`sdd/work/` exists).
- At 514bdc8d with only the build change: `--project selfhosting` Failed 4, Passed 5987 (same four tests).
- Analyzer, core and CLI projects not re-run: this stage changes no file they compile (05 ran them on the refactored tree: all passed).

### Disputed tests or scope

- `src/Reihitsu.SelfHosting.Test/Repository/BuildConfigurationTests.cs:102` `RepositoryBuildPassesRootConfigurationToTheAnalyzers` asserts one `AdditionalFiles` item ending in `/reihitsu.json` in the root build files; decision 13 removes it, so the test fails ("The root build files must add the root reihitsu.json as one AdditionalFiles item."). For the next test-implementer stage, not changed here.
- `MSTEST0049` at `LineCountSelfHostingTests.cs:173` breaks the Release build (test code: test implementer or cleaner).
- `scripts/lane-check.sh behavior-change` reports "no lane": the root `reihitsu.json` (added in 261a706b) is classified `Unknown`. The lead decides whether the lane rules learn the file or how it is argued.
- New production type `TargetWriteBackUtilities` (above).

### Architecture findings

_None._

### Follow-up candidates

_None_ beyond the one the lead already holds (pass the root `reihitsu.json` to the build once `binaries/` are rebuilt).

## thoenissen/Reihitsu#1866

### Changes

None in this stage; done in 261a706b (`Debug.ruleset`: `RH08402` → `RH8402`).

### Tests run

`RulesetTests` within the full `--project selfhosting` run above: passed.

### Disputed tests or scope

_None._
