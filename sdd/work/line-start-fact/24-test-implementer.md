Role: test implementer
Commit: 29a686a
Verdict: tests written

This is repair round 2. It answers `23-auditor.md` finding #5. The prompt carried no intended fix.

## Re-derivation

RH6001-002 requires that a keyword is not reported when a line break stands before its `(`, either directly behind the keyword or behind a `/* */` comment. A `///` comment ends its line (SHARED-BLANK-008), so each of these shapes has a line break in front of the `(` and must not be reported.

The shape axis is what stands between the keyword and the `///` comment, or between the `///` comment and the `(`:

- one `/* */` in front of the `///` (the auditor's member);
- two `/* */` in front of the `///`;
- a `/* */` behind the `///`, on the line of the `(`.

The keyword is not an axis. The guard is one condition shared by every keyword (`RH6001…Analyzer.cs`, one `if` over `token`/`nextToken`), and the nine keyword rows from `18-test-implementer.md` already show it for each keyword.

I formatted each shape with `scripts/format.sh --force`.
- The formatter keeps all three `///` shapes: `if/* c *//// x` / `(c)`, `if/* a *//* b *//// x` / `(c)`, and `if/// x` / `/* c */(c)`.
- The `//` counterpart of the third shape gets a blank line from the formatter (`if// x` / blank / `/* c */(c)`).
- `apply-fix.sh RH6001` on these outputs gives `no-diagnostic` for all six fixtures under LF and CRLF on the head.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Analyzer.Test/Formatting/RH6001…AnalyzerTests.VerifyKeywordFollowedByBlockCommentAndStrayDocumentationCommentIsNotReported` (`if/* c *//// x` / `(c)`) | RH6001-002 (FC-003, FC-034) | `Verify(source)` | green |
| `….VerifyKeywordFollowedByTwoBlockCommentsAndStrayDocumentationCommentIsNotReported` | RH6001-002 | `Verify(source)` | green |
| `….VerifyKeywordFollowedByStrayDocumentationCommentAndBlockCommentInFrontOfParenthesisIsNotReported` (`if/// x` / `/* c */(c)`) | RH6001-002 | `Verify(source)` | green |
| `….VerifyKeywordFollowedByBlockCommentAndSingleLineCommentIsNotReported`, `…TwoBlockCommentsAndSingleLineComment…`, `…SingleLineCommentAndBlockCommentInFrontOfParenthesis…` (the `//` controls) | — | `Verify(source)` | green |
| `Analyzer.Test/Formatter/Formatting/RH6001…FormatterTests.VerifyFormatterOutputWithBlockCommentAndStrayDocumentationCommentBehindKeywordIsNotReported` (`if/* c *//// x` / `(c)`) | RH6001-002 (FC-013) | `VerifyFormatter(source)` | green |

- **On the head:** `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"` gives `Passed: 36, Failed: 0`. All new tests pass, as expected.
- **On the old analyzer:** I temporarily replaced `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs` with its version from `802afbf`, ran the same filter, and restored the file with `git checkout`. Nothing of it is committed.
  - The three new `///` rows fail, and so does the new `VerifyFormatter` row. The `//` controls pass.
  - Failed 14, passed 22: these four, the nine keyword rows, and the earlier `VerifyFormatter` row.
  - So the new rows pin the member that the head closes.

## Not covered, and why

- **`if /* c */ /// x`**: whitespace in the keyword's trailing trivia. It is skipped before and after the change (auditor), so it adds no boundary.
- **`if/** d *//// x`**: a delimited documentation comment in the gap. RH6001-001 already excludes a gap that holds a documentation comment, so the `///` adds no decision. I did not probe whether the formatter keeps this shape.
- **A directive between `///` and `(`**: no diagnostic (auditor). The shared fact treats a directive like a line break.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"
```
