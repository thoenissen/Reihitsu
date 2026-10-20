Role: test implementer
Commit: ecb84ce
Verdict: tests written

Characterization tests for struct-equality Q5, filed in `19-spec-author.md`. I followed the conventions of `16-test-implementer.md`: no `[Requirement]` tag, and a doc comment that starts with "Characterizes …" and says the case is an open question. No production code, spec, or rule page was touched. Both tests pass on the unchanged code. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

Both tests are new, in `src/Reihitsu.Analyzer.Test/Performance/`, at the end of their classes.

| Test (class prefix · method) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH1002 · VerifyWrappedArgumentThatCanOnlyProduceNullOrDefaultComparerIsNotReported | Char. struct-equality Q5 (RH1002 input) | `Verify` + `Diagnostics(…, 2)` | pass: only the first and the fourth call are reported |
| RH1001 · VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported | Char. struct-equality Q5 (RH1001 lines `a` and `b`) | `Verify` | pass: only the `S` of `a` is reported |

How the inputs match the issue text:
- RH1002: the Q5 input exactly as written. I checked this with a script. The script extracted the code block from `19-spec-author.md` and the test's raw string, removed the indentation and the `{|#n:…|}` markup, and compared them. They are identical.
- RH1001: the issue text gives only the two declarations `a` and `b`. As for Q4 in `16-test-implementer.md`, I put them unchanged into a minimal frame: `using System.Collections.Generic;`, `struct S`, `class C`, `void M()`.

Both tests can fail. I changed each test's expectation temporarily, then restored both files from backups. The final diff contains no mutation (checked with `git diff` and `grep`). Results of the two tests on the mutated tree: 2 failed, 0 passed.

| Test | Mutation | Failure |
|---|---|---|
| VerifyWrappedArgumentThatCanOnlyProduceNullOrDefaultComparerIsNotReported | expects a report on `items.Distinct((null as IEqualityComparer<S>))` | expected "3" actual "2" |
| VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported | expects a report on the `S` of `b` | expected "2" actual "1" |

## Not covered, and why

- **The further wrapped inputs listed in `19-spec-author.md` (finding 2).** The task was to pin Q5 with the issue's own input. The spec author's other inputs show the same behavior and are not part of the issue text, so I added no tests for them.
- **The `as` clarification of SHARED-EQUAL-002.** `19-spec-author.md` left the example block unchanged, so its tagged test still covers it verbatim. No new tagged test is needed.
- **FC-010 to FC-014, FC-019, FC-007/FC-008, FC-033 (line endings).** No code fix and no formatter phase is involved. The diagnostics are located by markup, not by layout.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project architecture
```

Results on this tree:

| Command | Result |
|---|---|
| Focused RH1001/RH1002 run | "Passed! - Failed: 0, Passed: 74, Skipped: 0, Total: 74" |
| `scripts/test.sh --project architecture` | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" |
| `scripts/format.sh` on the two test files | "Formatted 0 of 2 file(s)." |
