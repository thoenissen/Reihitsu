Role: implementer
Commit: dd2f609
Verdict: test disputed

This round adds the `fixed` keyword spacing to the published line-start-fact run (PR thoenissen/Reihitsu#1510). Inputs: `00-lead.md`, `05-auditor.md` finding #7, `06-spec-author.md`, `07-test-implementer.md`, the change doc `sdd/changes/2026-10/fixed-keyword-spacing.md` (Accepted), FMT-SPACE-012, and `squad/decisions/2026-10-31-line-start-fact-fixed-spacing.md` (A). The prompt carried no intended fix.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/KeywordSpacingRule.cs` | `IsSpacedKeyword` now includes `SyntaxKind.FixedKeyword`. The rule requires exactly one space after `fixed`, as after the other statement keywords. Behind a block comment (`fixed/* c */(…)`), the existing two-sided gap handling of FMT-SPACE-013 applies to it as it does to `if`. In a fixed-size buffer, `fixed int` gets one space, which FMT-SPACE-002's collapse already produced. | FMT-SPACE-012 (RH6001-001, SHARED-DIAG-010) |

## Policy owners

`rg -n "FixedKeyword" src/Reihitsu.Formatter src/Reihitsu.Core --glob '*.cs'` finds `KeywordSpacingRule` as the only spacing owner for the keyword. The other hit is `SyntaxIndentationUtilities.GetEmbeddedStatement`, which is not a spacing decision. The analyzer side is `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer`, and its keyword set already includes `fixed`.

## Tests run

All runs on dd2f609, Debug. `scripts/build.sh --configuration Debug` gives 0 warnings and 0 errors.

Focused runs:

| Command | Result |
|---|---|
| `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~HorizontalSpacingRequirementTests"` | `Failed: 1, Passed: 21` |
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH6001KeywordsMustBeSpacedCorrectlyFormatterTests"` | `Passed: 4` |

- In the formatter run, `OneSpaceAfterFixedKeyword` and `OneSpaceAfterFixedKeywordBehindBlockComment` are now green.
- The green rows listed in `07-test-implementer.md` stay green: `SeveralSpacesAfterFixedKeywordBecomeOne`, `SpacedFixedKeywordIsKept`, `FixedSizeBufferKeepsOneSpaceAfterFixedKeyword`, and `SeveralSpacesInFixedSizeBufferBecomeOne`.
- `OneSpaceAfterEachOfNestedFixedKeywords` fails; see below.
- In the analyzer run, the parity test `VerifyFormatterFixesGluedFixedKeyword` is green.

Full runs:

| Project | Passed | Failed |
|---|---|---|
| formatter | 3266 | 1 (`OneSpaceAfterEachOfNestedFixedKeywords`, see below) |
| analyzer | 6307 | 0 |
| core | 535 | 0 |
| cli | 251 | 0 |
| architecture | 89 | 0 |
| tooling | 125 | 0 |

`scripts/format.sh --check src` reports `0 of 1753 file(s) need formatting`.

## Disputed tests or scope

**`src/Reihitsu.Formatter.Test/Regression/Spacing/HorizontalSpacingRequirementTests.OneSpaceAfterEachOfNestedFixedKeywords`** (tagged FMT-SPACE-012).

- **What it asserts.** Input is `fixed(int* p = items)` / `fixed(int* q = others)` / `{` / `}`, both statements at column 8. The test expects `fixed (…)` / `fixed (…)` / `{` / `}` with every line still at column 8.
- **What the formatter gives.** The spacing part holds: both keywords get their space. But the second `fixed`, and the block it owns, go to column 12.
- **What the spec says.** FMT-INDENT-004: "The formatter MUST indent the unbraced embedded statement of … `using`, `lock`, and `fixed` one level deeper than its owner." The second `fixed` statement is the unbraced embedded statement of the first, so column 12 is the specified output.
- **This predates the run.** I traced the same input with both keywords already spaced, with `scripts/trace.sh`. The indentation phase moves the inner `fixed` and its block from 8 to 12, and it does the same to stacked `using (a)` / `using (b)` / `{ }`. The input with spacing already applied is therefore not stable, independent of the spacing change.
- **Why they disagree.** The test expects a stacked layout that no requirement grants. FMT-SPACE-012 is satisfied. Only the test's indentation expectation contradicts FMT-INDENT-004.
- **What the lead needs to route.** Either the expectation changes to column 12 for the nested statement and its braces (a test-implementer change), or stacked resource statements become a spec question for the maintainer.

## Architecture findings

_None._

## Follow-up candidates

_None new._
