Role: implementer
Commit: 898f702
Verdict: tests pass

Structural change after the second audit. I worked from `17-auditor.md` finding #4, `18-test-implementer.md`, `19-spec-author.md`, `00-lead.md`, and `squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md` (B). The prompt carried no intended fix. The auditor's suggested route was taken as a suggestion and re-derived below.

## Changes

`src/Reihitsu.Analyzer/Rules/Spacing/RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs` (RH6001-002, SHARED-BLANK-008):

- **New skip condition.** A keyword followed by `(` is skipped when its trailing trivia holds whitespace, or when `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(keyword, openParen)` holds. That second condition covers the line break that ends a `///` comment behind the keyword, which Roslyn files in the leading trivia of `(`.
- **Removed copy.** The private `EndOfLineTrivia` check on the keyword's trailing trivia is gone.

Why this is the right change:

- **RH6001-002 as amended.** A line break before the `(` excludes the keyword "when the `(` starts its line (SHARED-BLANK-008)". The shared gap fact is the decision the formatter makes for the same gap (FMT-SPACE-001), so the two now agree.
- **The fix.** The RH6001 code fix has no line-break check of its own (`rg EndOfLine src/Reihitsu.Analyzer.CodeFixes/Rules/Spacing/RH6001*`: no hit). It is not offered once the analyzer stops reporting.
- **What still reports.** RH6001-001's reported set is unchanged:
  - a `/* */` comment directly between the keyword and `(`, with no line break, still reports, because `AreSeparatedByEndOfLine` excludes multi-line comment interiors and ordinary comments without a line break;
  - `if/* c */// x` / `(c)` is now not reported, which RH6001-002 also excludes.

## Policy owners

- **RH6001, RH6002, RH6005, RH6021.** `rg -n "SyntaxKind\.EndOfLineTrivia" src/Reihitsu.Analyzer/Rules/Spacing` finds no line-break check on a token gap in any of them. All four decide the gap with `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`.
- **The other members of the enumeration in `18-test-implementer.md`.** These are the holding and unreachable sites in Core, Analyzer, and CodeFixes, plus the already-decided follow-ups. I did not change them, and their tests run green in the full analyzer, core, and formatter runs below.

## Tests run

All on 898f702 in Debug. `scripts/build.sh --configuration Debug` gave 0 warnings and 0 errors.

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH6001"`: `Passed: 29, Failed: 0`. The ten red rows are green, and the nine `//` controls stay green.

| Project | Passed | Failed |
|---|---|---|
| formatter | 3260 | 0 |
| analyzer | 6298 | 0 |
| core | 535 | 0 |
| cli | 251 | 0 |
| architecture | 89 | 0 |
| tooling | 125 | 0 |

## Disputed tests or scope

_None._ The `fixed` mismatch that `18-test-implementer.md` reports predates the run. RH6001-001 does not list `fixed`, while the analyzer checks it. It belongs to the spec author or the maintainer. I left the analyzer's keyword set unchanged.

## Architecture findings

_None._

## Follow-up candidates

- RH6001-001 and the analyzer disagree on `fixed`, as `18-test-implementer.md` reports.
- The earlier candidates stand:
  - `AttributeTargetUtilities.StartsLine` and the analyzer-side terminator sets (decision 3A);
  - the join's tight-pair table (decision `…-join-spacing-pairs`);
  - `Data/RootListPosition` → `TokenGapNormalizer.HasOwnLinePlacementOwner`.
