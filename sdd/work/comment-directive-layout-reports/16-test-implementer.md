Role: test implementer
Commit: 98aeef9 (tests), written on 43b04d9
Verdict: tests written

Repair round 3, for audit 14 findings #1–#3 as decisions 17–19, the amended SHARED-COMMENT-003 and SHARED-INIT-001, and 15-spec-author's "Examples for the tests" state them. My prompt carried no suspected cause or hoped-for verdict beyond those recorded decisions.

Per issue: #1147 — RH5201-002 counterexample (finding #1) and SHARED-INIT-001 earlier-line comments (finding #3). #1386 — SHARED-COMMENT-003 per-line `///` count (finding #2). #1152, #1387, #1594, #1655, #1736 — nothing to do.

All 38 new test runs (26 methods, LF/CRLF rows included) are red on 43b04d9, each for the reason the finding describes; none was green before.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5038…AnalyzerTests.VerifyEveryLineOfTripleSlashRunIsReportedOnItsOwnInEveryDocumentationMode` (LF, CRLF) | SHARED-COMMENT-003, SHARED-COMMENT-001, RH5038-001 | `Verify` with Parse and None transforms, exact markup spans | red: Parse mode reports 2 (one for the two-line `///` run), expected 3 |
| `RH5048…AnalyzerTests.VerifyEveryLineOfTripleSlashRunBeforeFirstCallOfWrappedChainIsReportedOnItsOwnInEveryDocumentationMode` (LF, CRLF) | SHARED-COMMENT-003, SHARED-COMMENT-001, RH5048-001 | `Verify` Parse/None | red: Parse reports 1, expected 2 |
| `RH5201…AnalyzerTests.VerifyLinkInAnchorColumnBehindMisalignedCommentTailIsReported` (LF, CRLF) | RH5201-002 | `Verify(source, fixed, 1 iteration)` | red: not reported (expected 1) |
| `RH5201…FormatterTests.VerifyFormatterBreaksLinkInAnchorColumnBehindMisalignedCommentTail` | RH5201-002 | `VerifyFormatter(input, fixed, (8,18)-(8,19))` | red: not reported |
| `RH5301…AnalyzerTests.VerifyMemberBelowLineOfTwoCommentsInExpectedColumnIsNotReported` (LF, CRLF) | SHARED-INIT-001, RH5301-002 | `Verify` | red: reported at (11,21) |
| `RH5301…AnalyzerTests.VerifyMemberBehindFurtherCommentEndingOnItsLineIsNotReported` (LF, CRLF) | SHARED-INIT-001, RH5301-002 | `Verify` | red: reported at (12,16) |
| `RH5301…AnalyzerTests.VerifyMemberBelowLineOfTwoCommentsOutsideExpectedColumnIsReported` (LF, CRLF) | SHARED-INIT-001, RH5301-002 | `VerifyCodeFixIsWithheldOrProducesOneOf(…, true, …)` | red: the fix's output (the aligned shape) is still reported |
| `RH5301…AnalyzerTests.VerifyMemberBehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsReported` (LF, CRLF) | SHARED-INIT-001, RH5301-002 | `VerifyCodeFixIsWithheldOrProducesOneOf` | red: fixed state still reported |
| `RH5301…FormatterTests.VerifyFurtherCommentsBehindCommentsThatStartEarlierLinesAreNotChecked` (spec example) | SHARED-INIT-001, RH5301-002 | `VerifyFormatter(source)` | red: 2 diagnostics (12,21), (14,16) |
| `RH5301…FormatterTests.VerifyMemberBelowLineOfTwoMisplacedCommentsIsFixed` | SHARED-INIT-001, RH5301-002 | `VerifyFormatter(input, fixed, diag)` | red: formatter output reported |
| `RH5301…FormatterTests.VerifyMemberBehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsFixed` | SHARED-INIT-001, RH5301-002 | `VerifyFormatter(input, fixed, diag)` | red: formatter output reported |
| `RH5303…AnalyzerTests` — the same four methods, `Element…` names (LF, CRLF each) | SHARED-INIT-001, RH5303-002 | as RH5301 | red: aligned shapes reported at (10,26) / (11,17) |
| `RH5303…FormatterTests.VerifyElementBelowLineOfTwoMisplacedCommentsIsFixed`, `…BehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsFixed` | SHARED-INIT-001, RH5303-002 | `VerifyFormatter(input, fixed, diag)` | red: formatter output reported |
| `RH5304…AnalyzerTests` — the same four methods, `NestedElement…` names (LF, CRLF each) | SHARED-INIT-001, RH5304-002 | as RH5301 | red: aligned shapes reported at (11,21) |
| `RH5304…FormatterTests.VerifyNestedElementBelowLineOfTwoMisplacedCommentsIsFixed`, `…BehindFurtherCommentWhoseLineStartsOutsideExpectedColumnIsFixed` | SHARED-INIT-001, RH5304-002 | `VerifyFormatter(input, fixed, diag)` | red: formatter output reported |

Notes on the expected values:
- SHARED-COMMENT-003: the RH5038 test is the spec's new example verbatim (two `///` lines, one two-line `/** */`), three diagnostics in both modes, each span ending at the end of its text (markup excludes the line break).
- RH5201-002: the auditor's counterexample. In the analyzer test a blank line stands in front of `/* x` so the fix output follows the shape the existing `VerifyFixBreaksLinkBehindMultiLineCommentTailInOnePass` already asserts (SHARED-CHAIN-008 unchanged: the line breaks in front of `.D()`, which lands in the anchor column). The formatter test uses the auditor's input without the blank line; its fixed text is what the unchanged formatter writes (`scripts/format.sh` on a scratch copy; the formatter is not changed in this run).
- SHARED-INIT-001: each misaligned counterpart's accepted fix output, and its formatter output, is exactly the corresponding not-reported shape (the formatter moves only the line's first comment into the expected column, the tail `*/` stays; checked with `scripts/format.sh`, formatter unchanged). So the misaligned tests are red now only because that output is still reported; they also guard both sides of the new condition: the line's first comment is checked, the further comment is not.

## Not covered, and why
- RH5201 reverse case (`*/` in the anchor column, `.` elsewhere): decision 17 drops it with the SHARED-COMMENT-012 vs RH5201-002 tension; no test asserts either outcome.
- SHARED-INIT-001 earlier-line further comment in front of a closing brace (one level deeper): same `IsAlignedAt` path as the member case with an offset; not asked by the audit or the spec's examples. The existing closing-brace two-comment test (`VerifyClosingBraceBehindTwoLeadingCommentsInBraceColumnIsNotReported`) covers the token's own line.
- SHARED-COMMENT-003 on the other 16 rules of `CommentPositionAnalyzerBase`: RH5038 runs through the shared base (`CommentRegionAnalyzerBase` → `CommentPositionAnalyzerBase.OnSyntaxTree`), where the count is decided once; RH5048 is the separate `FluentChainAnalyzerBase` path the audit names. One rule per path.
- `/** */` run in RH5048: decision 18 concerns `///` runs; the `/** */` single-diagnostic case is already covered by RH5038's example and by the existing `VerifyDelimitedDocumentationStyle…` tests.
- FC dimensions: no code fix changes (RH5201 fix, initializer fixes unchanged), so Fix All is covered by the existing batch scenarios; LF/CRLF on every new test; comment cases are the subject of every test; no directive variant is in scope (the decisions concern comments only).

## Commands
```shell
scripts/test.sh --project analyzer --filter "Name~EveryLineOfTripleSlashRun|Name~AnchorColumnBehindMisalignedCommentTail|Name~LineOfTwoComments|Name~FurtherCommentEndingOnItsLine|Name~FurtherCommentWhoseLineStartsOutside|Name~LineOfTwoMisplacedComments|Name~FurtherCommentsBehindCommentsThatStartEarlierLines"
```
