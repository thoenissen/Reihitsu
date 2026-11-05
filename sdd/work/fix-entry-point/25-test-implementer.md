Role: test implementer
Commit: see the commit that adds this file (parent 24460b90)
Verdict: tests written

Behavior change, `00-lead.md` row 25, the test part: the new FMT-PIPE-029 example of `24-spec-author.md` (a documentation comment that starts its line in front of the target, with the target's first token behind it on that line, keeps its trivia). My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched.

Expected values are the spec example's output (FMT-PIPE-029, `sdd/specs/formatter/pipeline.md`), not a patched copy of the code (FC-035). The tests cover existing behavior and pass on the head, as the spec author measured. They are on the "trivia kept" side of the precondition: the reading row 25 rejected (asking the shared-line question because `private` does not start its line) would write a line break between `/** x */` and `private`, which these exact-text assertions refuse.

## Tests

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterFieldSplitTests.cs`, through `AssertSplitsTarget` (the context entry point, LF and CRLF, whole text, second pass):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| DocumentationCommentsThatStartTheirLinesInFrontOfMembersOnTheSameLineStayWhereTheyStand (spec example, both gaps) | FMT-PIPE-029, FMT-PIPE-010, FMT-PIPE-018; FC-002, FC-003 | `AssertSplitsTarget` | passes, must stay |
| FirstFieldBehindADocumentationCommentThatStartsItsLineInTheGapInFrontKeepsTheTargetsLine (gap in front only) | FMT-PIPE-029, FMT-PIPE-018; FC-003 | same | passes, must stay |

`src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`, through `Verify` (one pass, re-analysis) and `AssertSingleFixUnderBothLineEndings` (exactly one action, exact text under LF and CRLF):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FixLeavesDocumentationCommentsThatStartTheirLinesInFrontOfMembersOnTheSameLineWhereTheyStand (spec example) | RH7101-004; FC-002, FC-003 | `Verify` + both line endings | passes, must stay |
| FixLeavesTheFirstFieldBehindADocumentationCommentThatStartsItsLineInTheGapInFrontOnTheTargetsLine (gap in front only) | RH7101-004; FC-003 | same | passes, must stay |

## Not covered, and why

- **The gap behind the target alone** (`/** q */ private int _q;` starting its line behind the target): its first token's comment starts the line after an end-of-line, the shape the existing c5/c6 tests and the plain members-on-their-own-lines tests already cover through the same "something in the gap starts a line" branch. The new condition row 25 clarifies is the token behind the gap not starting its line, which only the gap in front exercises differently; the combined test covers the behind gap as well.
- **The formatter layout check** (`AssertFormattingAddsOnlyBlankLines`): document-level formatting moves `private` onto its own line here, and the fix by FMT-PIPE-018 does not, as for c5/c6, so that helper does not apply.
- **Fix All**: one diagnostic per input; Fix All is unchanged and covered elsewhere.
- **Failure classes**: no production change in this step; the classes `22-test-implementer.md` lists as dismissed stay dismissed. FC-002/FC-003 are the side of the condition these tests hold.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterFieldSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Sdd|FullyQualifiedName~Comment"
```

Results on 24460b90 plus these tests (Release): formatter 45 passed; analyzer 80 passed; architecture 62 passed.
