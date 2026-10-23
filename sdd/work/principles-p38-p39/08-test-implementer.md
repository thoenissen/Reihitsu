Role: test implementer
Commit: cf65539
Verdict: tests written

This is the repair round for finding 2 of `07-auditor.md` (FC-031). The prompt asked for a change to comments only. It carried no intended fix beyond the auditor's suggestion, which I treated as a suggestion only: I worked out the new wording from the amended SHARED-DOC-004 and SHARED-FIX-003.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (LF, CRLF): summary only | SHARED-DOC-004, SHARED-FIX-003, SHARED-ORDER-002; FC-031 | unchanged | pass |

Old sentence: "no preprocessor directive stands between the comment and `Create`, the shape the specification covers". That was the boundary of the old SHARED-DOC-004. The amended SHARED-DOC-004 drops it. In mode `Parse`/`Diagnose`, a fix that moves the declaration must carry the comment, and the `#if DEBUG`/`#endif` example now covers a directive between the comment and the declaration.

The new summary states three facts:
- The fix carries the comment, so the comment still documents `Create` after the fix. This is the offered side of SHARED-FIX-003's fifth condition, which the test's new tag covers.
- The test runs in mode `Parse`, the mode in which the comment documents `Create`.
- This input has no preprocessor directive between the comment and `Create`. That is a property of the input, not a coverage limit.

The sentence that the comment's start, its line layout, and the rest of the type are not asserted stays. SHARED-DOC-004 still leaves where the comment starts uncovered.

## Not covered, and why

Finding 1 of `07-auditor.md`: the auditor's recommendation for it is no code or spec change in this run. The run's decision `2026-10-23-1415-conforming-examples.md` rules out tests that assert future behavior. I wrote no test for it.

## Commands

```shell
scripts/verify-text-only.sh --base HEAD --head worktree
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests"
```

## Whole run

- `scripts/format.sh` on the file changed 0 files.
- `scripts/verify-text-only.sh --head worktree` compares against `origin/main` (c81091b). It reports FAIL because it also counts the run's earlier test changes (new tests, tags). That run therefore cannot isolate this repair.
- `scripts/verify-text-only.sh --base HEAD --head worktree` (base cf65539) isolates this repair. Result: PASS. One file is comment/layout-only: 3651 tokens identical, directives and disabled text identical, changed trivia limited to documentation.
- RH7103 test class: 46 passed, 0 failed.
