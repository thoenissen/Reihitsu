Role: test implementer
Commit: c1dc6a6
Verdict: #1152: tests written (repair); #1594: tests written (repair); #1147, #1387, #1386, #1655, #1736: nothing to do this round

This is a repair round for decision 16 in `00-lead.md` and the two disputed tests in `06-implementer.md`. The specs decide the expected values. Neither value comes from the patched code (FC-035). The lead's message named the outcome each requirement decides, and that matches my own reading of RH5204-005 and RH7304-004.

## thoenissen/Reihitsu#1152

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests.VerifyFormatterFixesDelimitedDocumentationCommentOnOwnLine` (existing). The expected span changes from (3,4)–(3,11) to (3,1)–(3,11): it starts at `/**` and ends at the end of `*/`, without the line break. The test is newly tagged | RH5204-005 | `VerifyFormatter(input, fixed, diagnostic)` | green |

## thoenissen/Reihitsu#1594

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7304RegionDirectivesMustUseConsistentIndentationAnalyzerTests.VerifyFixIndentsDirectivesToTheCommentThatLeadsTheFirstLineOfCode` (LF, CRLF) | RH7304-004 | `Verify(source, fixed)`, then `ApplyCodeFixAsync` | green |

What changed in this test:
- **Before:** it used `VerifyCodeFixWithEndOfLine`. That helper's single-action step expected the whole spec result.
- **Fix All:** unchanged. `Verify(source, fixed)` still checks the spec example: both regions are indented to column 4, with line endings converted.
- **Single action:** I kept the two-region input instead of switching to a single-region one, because it checks RH7304-004's "MUST change nothing else" directly. The action of the first diagnostic now has to produce `Fields` at column 4, with `Lines` still at column 2.
- **Usings:** `System.Collections.Immutable` and `Microsoft.CodeAnalysis.Text` were added.

## Not covered, and why
- Nothing new. A single-region single-action case already exists: `VerifyFixIndentsDirectivesToAMisplacedCommentThatLeadsTheFirstLineOfCode` checks one action on one region.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7304RegionDirectivesMustUseConsistentIndentationAnalyzerTests|FullyQualifiedName~RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests"
```
Result on this head: 41 passed, 0 failed.
