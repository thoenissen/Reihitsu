Role: test implementer
Commit: 5b5a5f19b1c46e8674e99949c8e7fb4ae250a7ae (tests run on this head; production code unchanged since 31dd2dd)
Verdict: tests written

Repair round 2, decision 20, audit finding #3 (21-auditor). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All in `src/Reihitsu.Formatter.Test/Regression/Structural/TrailingCommaDocumentationCommentTests.cs`, rows `Parse` and `Diagnose`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DocumentationCommentInFrontOfTheEnumTrailingCommaStaysBehindTheLastMemberInFrontOfACommentLine` | FMT-STRUCT-019, FMT-PIPE-004; FC-017, FC-040 | `AssertRuleResult` (LF, CRLF, second pass, phase stability, FMT-PIPE-021 tree check) | pass |
| `DocumentationCommentInFrontOfTheArrayInitializerTrailingCommaStaysBehindTheLastElementInFrontOfACommentLine` | FMT-STRUCT-020, FMT-PIPE-004; FC-017, FC-040 | `AssertRuleResult` | pass |
| `DocumentationCommentInFrontOfTheCollectionInitializerTrailingCommaStaysBehindTheLastElementInFrontOfACommentLine` | FMT-STRUCT-020, FMT-PIPE-004; FC-017, FC-040 | `AssertRuleResult` | pass |

Input shape of each row: the last element `B /** doc B */,` or `2 /** two */,`, then a line `// note`, then `}`. Each input is already in its final layout apart from the comma.

Expected values follow the finding's argument. FMT-STRUCT-019/020 remove the comma and keep the comment behind the last element. FMT-PIPE-004 requires that one run gives the final shape. So the output is the input without the comma, with no blank line in front of `// note`. That text is a fixed point of the formatter: `AssertRuleResult`'s second-pass check covers it, and 21-auditor's comma-less fixtures under `Parse` (`scratchpad/b21/n.cs`, enum `E3` and array `_c`) confirm it. The expected values were not taken from patched code.

## Scenarios (falsification)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| 6 new rows on head 5b5a5f1 | LF, CRLF | formatter (`AssertRuleResult`) | pass | — |
| 6 new rows, test file copied onto `d1741804` (audit 1 head) | LF | formatter | fail 6/6 | first-pass string mismatch (that head drops the `/** */` comment, audit 1 finding #1) |
| 6 new rows, `src/Reihitsu.Formatter` and `src/Reihitsu.Core` from `main` 450f274 on that worktree | LF | formatter | fail 6/6 | FMT-PIPE-021 tree check: from the structural phase on, `/** doc B */` is `B`'s trailing trivia in the tree but not in the reparsed text. This is the trivia placement that, on `main`, produces the blank line on the first pass and removes it on the second (21-auditor #3). |

## Not covered, and why

- `DocumentationMode.None`: deliberately left out, per the finding and the auditor's hint. In `None` the comment is the last element's trailing trivia, not the comma's, so the run's branch does not fire. The blank-line phase then inserts a blank line in front of `// note` for the comma-less text too. That is pre-existing behavior outside the run (FC-040 hint in 21-auditor), and a `None` row would pin it as-is.
- A `///` comment in front of the comma: it ends its own line, so the comma would be on a later text line, which FMT-UNSUP-009 refuses. It is not this branch's input.
- Removing transforms other than enum and initializer: none exist (15-test-implementer, `rg "PreserveSeparatorTrivia"`).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~TrailingCommaDocumentationCommentTests"
```

Result on head: 18 passed (the 12 existing rows and the 6 new ones).
