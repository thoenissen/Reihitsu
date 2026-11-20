Role: test implementer
Commit: af1c227
Verdict: tests written

Repair round 1 (auditor finding 1, maintainer answer A). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Tags only: no assertion, fixture, or data row changed, no production code touched.

## Tests
All in `src/Reihitsu.Analyzer.Test/Formatting/RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` (8 rows) | RH5604-001, RH5604-005 | `Verify` | pass (tags added) |
| `VerifyForeignTerminatorAtTheEndOfADirectiveIsFixedToThePredominantLineEnding` (4 rows) | RH5604-001, RH5604-005 | `Verify` | pass (tags added) |
| `VerifyForeignTerminatorInsideADocumentationCommentIsFixedToThePredominantLineEnding` (4 rows) | RH5604-001, RH5604-005 | `Verify` | pass (tags added) |
| `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf` (pre-existing on `main`, untagged until now) | RH5604-001, RH5604-005 | `Verify` | pass (tags added) |

The fourth test is not in 08's list: it pins a lone `\r` inside a `///` comment of an LF file, reported and fixed to `\n` — the third site of the new RH5604-001 example — so after the amendment it is derived from RH5604-001 and RH5604-005 as much as the other three. Its assertion is unchanged.

## Spec examples against tests
- RH5604-001, lone `\r` example (between tokens, directive end, inside `///`, LF file): one file with all three sites in the spec; covered site by site by the three `VerifyForeignTerminator…` methods (`\r`, `\n` rows) and `VerifyLoneCarriageReturnInsideXmlDocumentationIsNormalizedToLf`. The sentence for U+0085/U+2028/U+2029: the between-tokens method covers all three, the directive and documentation-comment methods U+2028. No new test.
- RH5604-005, lone `\r` example fixed to `\n`: `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` row (`\r`, `\n`) is that example. No new test.

## Scenarios (bug fix)
_None_ — repair round on tags; no reproduction asked.

## Not covered, and why
- A combined test with all three lone-`\r` sites in one file: each site is pinned separately with the same helper, analyzer, and fix; the analyzer collects the three carriers in one pass and the fix rewrites them in one call (`LineEndingUtilities.NormalizeLineEndings`), so a combined fixture adds no arm.
- U+0085 and U+2029 at a directive's end and inside `///`: same carrier kinds and same rewrite call as the U+2028 rows; not added, as the spec sentence states them together and 08 asked for no new test.
- Decisions 6 and 7 of `00-lead.md`: no test for an all-lone-`\r` file or a `\r`-majority file without `\n`.
- Failure classes: unchanged from 02 and 07; no code or assertion changed.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604"   # 47/47
scripts/test.sh --project architecture                                       # 198/198
```

## Follow-up candidates
_None._
