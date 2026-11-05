Role: test implementer
Commit: see the commit that adds this file (parent 11179496)
Verdict: tests written

Structural change of `00-lead.md` row 24, the test part: `20-auditor.md` finding 6, with the five FMT-PIPE-029 examples of `21-spec-author.md`. My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched.

Expected values come from the five new FMT-PIPE-029 examples (11179496). The per-gap tests take one gap of an example and leave the other gap out; their expected values follow the same FMT-PIPE-029 bullet. I cross-checked every "moves" expected value against the formatter: `scripts/format.sh` on the head over the 12 inputs gives each expected value with one blank line above each moved documentation comment, and over the 12 expected fix results gives the same text. That is the formatter's layout without the blank line, as answer 1a–1f A and SHARED-FIX-006 require. No value comes from a patched copy of the code (FC-035). The fixtures were in the scratchpad.

## Tests

Shapes, each used in both gaps (`both`), in the gap in front of the target only (`front`), and in the gap behind it only (`behind`):

- **A**: a `/** */` that spans lines (auditor d1, d2).
- **B**: a `/* */` that spans lines, then a one-line `/** */` (d5, d6).
- **C**: a one-line `/* */`, then a `/** */` that spans lines (d4, g1).
- **D**: a `/* */` that spans lines, then a `/** */` that spans lines (g2, g3).
- **c5 / c6** (holding side): the first token behind each gap already starts its line; a `/** */` that spans lines with a line break behind it (c5, spec example), and `///` (c6).

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterFieldSplitTests.cs`. Every test uses `AssertSplitsTarget`: the context entry point, LF and CRLF, whole text, and a second pass.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| MembersBehindADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine (A both, spec example) | FMT-PIPE-029 | `AssertSplitsTarget` | fails (members stay behind `_p;` and `_y;`) |
| FirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine (A front) | FMT-PIPE-029 | same | fails |
| MemberBehindADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine (A behind) | FMT-PIPE-029 | same | fails |
| MembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsStartTheNextLine (B both, spec example) | FMT-PIPE-029 | same | fails |
| FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentInTheGapInFrontStartsTheNextLine (B front) | FMT-PIPE-029 | same | fails |
| MemberBehindACommentThatSpansLinesAndADocumentationCommentInTheGapBehindStartsTheNextLine (B behind) | FMT-PIPE-029 | same | fails |
| MembersBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine (C both, spec example) | FMT-PIPE-029 | same | fails |
| FirstFieldBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine (C front) | FMT-PIPE-029 | same | fails |
| MemberBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine (C behind) | FMT-PIPE-029 | same | fails |
| MembersBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapsStartTheNextLine (D both, spec example) | FMT-PIPE-029 | same | fails |
| FirstFieldBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapInFrontStartsTheNextLine (D front) | FMT-PIPE-029 | same | fails |
| MemberBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapBehindStartsTheNextLine (D behind) | FMT-PIPE-029 | same | fails |
| DocumentationCommentsThatSpanLinesInFrontOfMembersThatStartTheirLinesStayWhereTheyStand (c5, spec example) | FMT-PIPE-029, FMT-PIPE-010, FMT-PIPE-018; FC-002 | same | passes, must stay |
| SingleLineDocumentationCommentsInFrontOfMembersThatStartTheirLinesStayWhereTheyStand (c6) | FMT-PIPE-029, FMT-PIPE-010, FMT-PIPE-018; FC-002 | same | passes, must stay |

`src/Reihitsu.Analyzer.Test/Formatting/RH7101DoNotCombineFieldsAnalyzerTests.cs`. Each "moves" test checks four things:

- `Verify`: one pass and re-analysis.
- `AssertSingleFixUnderBothLineEndings`: exactly one offered action, under LF and CRLF.
- The new private helper `AssertFormattingAddsOnlyBlankLines`. It asserts that formatting the source gives the formatted text and that formatting the fix result gives the same text.
- That helper also asserts that the fix result is the formatted text without its blank lines. `AssertFormattingChangesNothing` cannot be used here, because the formatter writes the blank line above the documentation comment (SHARED-FIX-006).

The two holding tests use `Verify` and `AssertSingleFixUnderBothLineEndings` only, because document-level formatting moves the documentation comment there and the fix does not, by FMT-PIPE-010 and FMT-PIPE-018.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FixStartsTheMembersBehindADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine (A both) | RH7101-004; FC-013 | `Verify` + both line endings + formatter layout | fails |
| FixStartsTheFirstFieldBehindADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine (A front) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMemberBehindADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine (A behind) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMembersBehindACommentThatSpansLinesAndADocumentationCommentInTheGapsOnTheNextLine (B both) | RH7101-004; FC-013 | same | fails |
| FixStartsTheFirstFieldBehindACommentThatSpansLinesAndADocumentationCommentInTheGapInFrontOnTheNextLine (B front) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMemberBehindACommentThatSpansLinesAndADocumentationCommentInTheGapBehindOnTheNextLine (B behind) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMembersBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine (C both) | RH7101-004; FC-013 | same | fails |
| FixStartsTheFirstFieldBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine (C front) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMemberBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine (C behind) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMembersBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapsOnTheNextLine (D both) | RH7101-004; FC-013 | same | fails |
| FixStartsTheFirstFieldBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapInFrontOnTheNextLine (D front) | RH7101-004; FC-013 | same | fails |
| FixStartsTheMemberBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine (D behind) | RH7101-004; FC-013 | same | fails |
| FixLeavesDocumentationCommentsThatSpanLinesInFrontOfMembersThatStartTheirLinesWhereTheyStand (c5) | RH7101-004; FC-002 | `Verify` + both line endings | passes, must stay |
| FixLeavesSingleLineDocumentationCommentsInFrontOfMembersThatStartTheirLinesWhereTheyStand (c6) | RH7101-004; FC-002 | same | passes, must stay |

## Watched failures

These runs were in Debug on 11179496 plus these tests.

- **Formatter** (`ReihitsuFormatterFieldSplitTests`): 12 failed, 31 passed. The 12 failures are exactly the new "fails" rows, each at "Unexpected first pass under LF". Every diff is finding 6: the documentation comment, and the member it documents, stay behind `_p;`, `b */`, `_y;` or `e */` instead of starting the next line. For example, actual `private int _p; /** a⏎       b */ private int _x;` against expected `private int _p;⏎    /** a⏎       b */ private int _x;`.
- **Analyzer** (`RH7101DoNotCombineFieldsAnalyzerTests`): 12 failed, 66 passed. The 12 failures are exactly the new "fails" rows, each in `Verify`'s fix comparison, with the same diff.
- **The formatter-layout half of the 12 RH7101 tests.** `Verify` fails first, so it is not reached today. I ran it alone once, with a temporary reordering that has been reverted, and all 12 passed: the formatter's output for each source and each fix result is the formatted text, and the fix result equals it minus the blank lines. So once the fix is right, these tests can only fail on the fix.
- **Architecture** (`Spec|Sdd|Comment`): 62 passed. The requirement tags resolve, and no tracker reference was added.

## Not covered, and why

- **The holding members that existing tests already cover**: `p`, `q`, `pq`, `m`, `a`, the one-line `/** X. */` gaps (`DocumentationCommentsInTheGapsGoWithTheMemberBehindThem`, `FixMovesADocumentationCommentInTheGap…`), and `CommentsInTheGapsStayBehindTheTokenInFrontOfThem`. I left them unchanged; they must stay green after the change. The auditor's f1 (a method in front behind a spanning comment) and f4 (a spanning `/* */` plus a one-line `/* */`) have no test of their own. They are on the "keeps its place" side, which `p`, `q` and `pq` cover through the same decision. A test only for f1/f4 would assert no different condition.
- **c5 and c6 per gap.** On the holding side, the front and behind gaps are kept by different requirements (FMT-PIPE-018 and FMT-PIPE-010). Both requirements are exercised in each combined test, and both sides must hold at once. I did not split them.
- **A directive or `#region` in the gap.** A directive cannot share a line with a member, and a directive inside the declaration withholds the split (FMT-STRUCT-023). Existing tests cover that.
- **Document-level formatting of these shapes.** The formatter's own output is unchanged by this change. It is pinned here only through `AssertFormattingAddsOnlyBlankLines`, which equals the head's output.
- **Fix All.** Each input has one diagnostic. Fix All is unchanged and covered by the scenario and by `FixAllSplitsTwoDeclarationsOnOneLine`.
- **Failure classes.** FC-004 to FC-012, FC-014, FC-018 to FC-030 and FC-032 to FC-037 were dismissed for this decision by the auditor (`20-auditor.md`), and the change adds no surface they reach. FC-001, FC-013, FC-016 and FC-021 are the classes of finding 6, and the "fails" rows cover them. FC-002 and FC-003 are covered by c5 and c6 on the other side of the condition.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterFieldSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Sdd|FullyQualifiedName~Comment"
```
