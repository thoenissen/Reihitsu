Role: test implementer
Commit: 7b49f3aa
Verdict: tests written

Follow-up round for the maintainer's answers Q7 and Q8 (RH5201-002 `*/` example, cases `f` and `g`). My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the RH5201-002 `*/` example and its fix block in `sdd/specs/analyzer/RH5201.md`. Production code already exists on this branch, so every new test passed on its first run; none was seen red. Each test pins the diagnostic location with `{|#0:.|}` markup and the exact count, so a missing or a misplaced diagnostic fails it.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5201MethodChainsShouldBeAlignedFirstCallTests.VerifyLinkBehindSecondCommentBehindOwnLineCommentTailInAnchorColumnIsReportedAndFixed` — case `f` (`/* x` ⏎ `*/ /* y */ .Self()`), LF and CRLF; one diagnostic, fix `*/ /* y */` ⏎ `.Self();` in one application and by Fix All | RH5201-002, RH5201-005 | `Verify` + fix (`VerifyFix`, 1 diagnostic, 1 iteration) | pass |
| `RH5201MethodChainsShouldBeAlignedFirstCallTests.VerifyLinkBehindTailOfCommentStartingBehindOwnLineCommentInAnchorColumnIsReportedAndFixed` — case `g` (`/* w */ /* x` ⏎ `*/ .Self()`), LF and CRLF; one diagnostic, fix `*/` ⏎ `.Self();` | RH5201-002, RH5201-005 | `Verify` + fix | pass |
| `RH5201MethodChainsShouldBeAlignedFormatterTests.VerifyFormatterBreaksLinkBehindSecondCommentBehindOwnLineCommentTailInAnchorColumn` — case `f` through the formatter: reported before, formatter output exact, analyzer-clean and stable on a second pass, LF and CRLF (FC-013) | RH5201-002 | `VerifyFormatter` | pass |
| `RH5201MethodChainsShouldBeAlignedFormatterTests.VerifyFormatterBreaksLinkBehindTailOfCommentStartingBehindOwnLineCommentInAnchorColumn` — case `g` through the formatter, same checks | RH5201-002 | `VerifyFormatter` | pass |

Other side of the widened condition (FC-003): case `e` (comment starts behind code on the line of the token in front of the link, not reported) stays covered by the existing `VerifyLinksBehindOwnLineCommentTailInAnchorColumnAreReportedAndFixed` and `RH5201MethodChainsShouldBeAlignedAnalyzerTests.VerifyLinkBehindCommentThatStartsBehindCodeIsTheAnchor`. I added `f` and `g` as separate tests rather than into that example test, so the test the implementer worked against stays unchanged.

Other spec changes since `04-test-implementer.md`, checked:
- RH5201-006 examples 2 and `g` now use `C Next` + `C? Opt`; the tests already did. Nothing changed.
- SHARED-CHAIN-005 exception wording narrowed to comments below the token in front of the link; no output change (07 pass), covered by `e` above and by `f`/`g`. Nothing added.
- Existing tests asserting an output the change alters: none. `rg` for `\*/ /\*` and `/\* w \*/` under `src/Reihitsu.Analyzer.Test` and `src/Reihitsu.Formatter.Test` found no RH5201 or chain test with either shape before this round.

## Not covered, and why

- Seeing the new tests red: production code for Q7/Q8 predates them (`05-implementer.md`); the base of the run (`d63eab4`) did not report links behind `*/` in the anchor column at all, which `04-test-implementer.md` recorded as red for the sibling cases `a`, `b`, `d`.
- Comment/directive-in-gap and Fix All across chains: covered by the existing tests of this class (`GetFixAllScenario`, F08); `f` and `g` take the same `IsBehindMultiLineCommentTail` path in `SyntaxTokenPositionUtilities` as `a`.
- Failure classes otherwise as in `04-test-implementer.md`; this round adds no rewrite and changes no formatter output.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201"
scripts/test.sh --project architecture
```

On this head: analyzer RH5201 filter 187 passed (181 before + 6 new runs); architecture 198 passed.

## Follow-up candidates

_None._
