Role: test implementer
Commit: eb9a532 (the head the test was written and run on; this handover's commit adds it)
Verdict: tests written

This covers decision 21 in `00-lead.md` (auditor finding #5, `23-auditor.md`): one holding test for the false side of the "non-verbatim interpolation" condition in FMT-BREAK-044's refusal. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to ignore. I touched no production code and no spec, and staged only the test file and this handover.

The expected value comes from FMT-BREAK-044: only an interpolation of a non-verbatim string that contains the body refuses. Here the comment's interpolation is verbatim, so the body is braced. The braced text follows FMT-STRUCT-001/026 with decision 7's spacing, the same derivation as the sibling holding tests in `19-test-implementer.md` (for example `IfStatementInInterpolationGetsBracesWhenAnotherStringHoldsDocumentationCommentInCSharp10`). It matches the auditor's CLI observation at C# 10.

## Tests
File: `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| IfStatementInNonVerbatimStringNestedInVerbatimInterpolationGetsBracesWhenOnlyTheVerbatimInterpolationHoldsDocumentationCommentInCSharp10. Input `var s = $@"a {$"x {F(() => { if (b) return 1; return 2; })}" /** d */} b";` gives `… if (b) {return 1; }return 2; …" /** d */} b";` (`bool b`, C# 10) | FMT-BREAK-044 | `AssertResult` (LF and CRLF, exact output, second pass, phase stability) | pass |

**Falsification.** In a scratch worktree of eb9a532, since removed, I deleted the `&& StringInterpolationUtilities.IsInsideSingleLineInterpolation(interpolation, languageVersion)` condition in `ControlFlowBraceTransform.IsInSingleLineInterpolationWithDocumentationComment`. With the condition gone, the new test fails and the other 79 still pass. With the code unchanged, all 80 pass. So the test pins exactly the condition that finding #5 found untested.

## Scenarios (bug fix)
_None._ This is a holding test in a repair round, not a reproduction.

## Not covered, and why
- Further sides of the predicate are already pinned by tests that `19-test-implementer.md` and `23-auditor.md` (*Assertion adequacy*) list: the true side of the same condition (`…WithDocumentationCommentOutsideIt…`, `$"` outer), a comment in the inner `$"` hole of a `$@"` string, sibling interpolations, other strings, and C# 11.
- The auditor's other constructed boundaries (e02–e11) were not part of decision 21, and the auditor reports them as holding.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NonVerbatimInterpolationLineBreakTests"
```
Result on this head with the test added: 80 passed, 0 failed.

## Follow-up candidates
_None._
