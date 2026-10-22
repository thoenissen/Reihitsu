Role: test implementer (auditor repair round)
Commit: 17fb03b
Verdict: tests written

Inputs:
- `08-auditor.md`: finding 2, and finding 3 as decided.
- `09-spec-author.md`.
- `squad/decisions/2026-10-22-1139-p12-scope-rh5304.md`.
- The revised SHARED-COMMENT-011: only the first token behind the comment is exempt, and a later token on that line is judged by the rule's own requirements.

My prompt carried no intended fix or verdict. No production file is touched, and every test below passes on the unchanged code.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/Formatting/`. Helper: `Verify` on `BatchCodeFixTestsBase<,>`.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| RH5201 `VerifyLaterLinkOnLineStartingWithBlockCommentIsDetectedAndFixed` (new) | SHARED-COMMENT-011 "only that first token is exempt", RH5201-001, RH5201-005; FC-003 (auditor finding 2) | pass: `.ToList()` reported on its `.`, `.Select` not reported; exact fixed output |
| RH5201 `…WithCrLf` (new) | the same; CRLF | pass |
| RH5303 `VerifyLaterElementOnLineStartingWithCommentIsDetectedAndFixed` (new) | SHARED-COMMENT-011 "only that first token is exempt", RH5303-002, RH5303-005; FC-003 | pass: `2` reported, `1` not; the fix puts `2` on its own line in the element column |
| RH5303 `…WithCrLf` (new) | the same; CRLF | pass |
| RH5304 `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements` (existing, tag added) | SHARED-COMMENT-011 (maintainer decision) | pass |
| RH5304 `VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace` (existing, tag added) | SHARED-COMMENT-011 (maintainer decision) | pass |

Inputs are the shapes from the second example block of SHARED-COMMENT-011, in the test files' own naming: `/* all */ .Select(value => value){|#0:.|}ToList();` and `/* a */ 1, {|#0:2|}`.

Each new test expects exactly one diagnostic on the later token. An implementation that exempts every token behind a line-leading comment would report none, and the test fails. With both new tests in place, that implementation no longer passes all tagged tests.

### Fixed output of the RH5201 test

The expected output has a blank line between `items.Where(value => value > 0)` and the `/* all */` line:

```
        var list = items.Where(value => value > 0)

                        /* all */ .Select(value => value)
                        .ToList();
```

My first expectation was the chain without that blank line. The run failed only on the missing blank line, and the rest of the layout matched.

I pinned the blank line because the spec text gives it:
- RH5201-005 says the fix lays out the chain as SHARED-CHAIN-008 specifies.
- SHARED-CHAIN-008 requires "the line breaks and columns that formatting the document gives it".
- FMT-BLANK-011 requires one blank line before a comment that starts its own line (SHARED-BLANK-002). That holds here, because the comment is not the first content of a block.

The auditor's hint saw the same blank line from `scripts/format.sh` on the spec example.

The fix converges in one pass; the verifier's iterative and Fix All checks pass. If the maintainer wants no blank line inside a chain, FMT-BLANK-011 is what changes, and this expectation changes with it.

## Not covered, and why

- Everything listed under "Not covered" in `05-test-implementer.md` still holds.
- RH5301 `/* a */ A = 1, B = 2`: the auditor found B reported, which conforms. I did not pin it. The requirement's other side is covered once per spec example (RH5201, RH5303), and the auditor suggested only those two. I have not proven that RH5301 and RH5303 share one call site, so this is a gap, not proof of coverage.
- RH5304, a later element on a comment-led line: not probed and not pinned. The decision names only the two existing tests.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5201|FullyQualifiedName~RH5303|FullyQualifiedName~RH5304|TestCategory=SHARED-COMMENT-011"
scripts/test.sh --project architecture
```

Results on this tree:
- The analyzer filter passes 162 of 162. 28 tests now carry the SHARED-COMMENT-011 tag.
- The architecture project passes 72 of 72.
- `scripts/format.sh` changed none of the 3 test files.
