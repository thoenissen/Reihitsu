Role: spec checker
Commit: see the commit that adds this file
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Repair round 1; head e08e6fc, repair diff `eb6470e...HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The repair rewrites the FMT-PIPE-008 `FormatNode` clause to state the node's reported `SyntaxTree` options and the cases that decide it. This is the escalation-4 decision (narrow the clause to the code) restated precisely; the code is unchanged, so it is a clarification and no change doc is needed or added.
2. Specs match the diff: ok. The diff changes no compiled production behavior. The `FormatNode` bullet, the four-case list, and the example paragraph agree with the new tests (keeps C# 11: `ReplaceToken`, `WithoutTrivia`, `NormalizeWhitespace`, `ReplaceNode`, parsed with options, `CSharpSyntaxTree.Create`, unchanging `WithIdentifier`; default options: `WithIdentifier`, `WithAdditionalAnnotations`, `CompilationUnit().AddMembers`, edit of a changed node). The requirement keeps its format (normative sentence first, `Why:`, examples). The `ReihitsuFormatter.FormatNode` remarks now say the same as the spec.
3. IDs: ok. `sdd/specs/requirement-ids.txt` is unchanged; nothing added, renumbered, or retired.
4. Code owners: ok. No owner or `Uses` change in the repair; `sdd/specs/README.md` unchanged. The architecture suite passes (see 7).
5. User documentation: ok. No rule page touched.
6. Open questions: ok. `09-spec-author.md` states none. Escalation 7 reached the maintainer and is recorded in `squad/decisions/2026-10-30-phase-docs-match-code-formatnode-remarks.md`.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture`: Passed! Failed: 0, Passed: 89, Skipped: 0, Total: 89. `scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterLanguageVersionTests"`: Passed! Failed: 0, Passed: 36, Skipped: 0, Total: 36. FMT-PIPE-008 keeps covering `[Requirement]` tests.
8. Test tags: ok. All 10 added tests carry `[Requirement("FMT-PIPE-008")]`, and `10-test-implementer.md` lists them against it. Every example the spec author named under `Examples for the tests` is pinned by an added or existing tagged test. Named edits without a test of their own (`ReplaceTrivia`, `RemoveNode`, `Insert…`, `WithLeadingTrivia`, `WithTrailingTrivia`, `Add…` on a parsed node) are stated as probed Roslyn behavior and are explained in the test implementer's handover; not a finding.
9. Nothing left behind: ok. `sdd/work/` holds only `phase-docs-match-code`.

## Mechanical results
- `scripts/verify-text-only.sh --base eb6470e --head HEAD`: FAIL, as expected. The new tests are compiled behavior; `ReihitsuFormatter.cs` alone is text-only (1446 tokens identical, trivia: documentation), the edit accepted under escalation 7.
- `scripts/lane-check.sh tooling`: `no lane`, caused only by the comment-only `ReihitsuFormatter.cs` remarks edit; accepted by the maintainer under escalation 7 and stated in the run report.
