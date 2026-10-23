Role: test implementer
Commit: 4b41f93
Verdict: tests written

Inputs: `00-lead.md` (including the third repair round), `16-auditor.md` findings 8 and 9, `17-spec-author.md`, `git show 4b41f93`, the amended SHARED-DOC-004 and SHARED-DIAG-004 text, and the RH7106 spec. My prompt carried no intended fix or hoped-for verdict.

I re-derived what the amended text requires instead of taking the spec author's list over:

- **SHARED-DOC-004.** The only test tagged `SHARED-DOC-004` that runs a code fix which moves a declaration is the RH7103 one below. Every other tagged test (RH8001, RH8013, RH8019, RH8020, RH8026, RH8030, RH8031, RH8304, RH8308) is about which comments a declaration's documentation consists of. The move clause does not touch those tests. The RH7103 test runs through `ApplyCodeFixAsync`. Its `CreateTestDocument` builds `CSharpParseOptions` with no `documentationMode` argument, so the default mode `Parse` applies. Its input has no directive between the comment and `Create`, so it pins the covered side only. The not-covered shapes (mode `None`, or a directive in between) are not pinned, and no test was added for them.
- **SHARED-DIAG-004.** The tagged tests are RH2004 (`VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic`, a partial type), RH2102 and RH7104. None of them judges keyword order on a partial member, and none of their summaries cites the old wording ("same accessibility modifiers"): `rg` over `src` finds no hit. Before this round, no test covered the ¶2/¶3 boundary for a partial member. The new spec example is that boundary, and it conforms today. The RH7105 tests on partial members (`async partial` on a method) are untagged and agree with ¶2.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH7106ProtectedMustComeBeforeInternalAnalyzerTests.PartOfAPartialPropertyIsReportedAndFixedOnItsOwn` (new) | RH7106-001, RH7106-004, SHARED-DIAG-004 ¶2 on a partial member; FC-002 (the ¶2 side of the boundary) | `Verify(source, fixedSource)` on `BatchCodeFixTestsBase` | pass: one diagnostic on `protected` of the first part, and the fix swaps only that part's keywords. The input and output are the spec example verbatim |
| `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration` (summary only) | SHARED-DOC-004, SHARED-ORDER-002 | `ApplyCodeFixAsync` | pass, unchanged. The summary now says the fix runs in documentation mode `Parse` with no directive between the comment and `Create`, the covered shape. Tags and assertions are unchanged |

No existing assertion or tag changed.

## Not covered, and why

- **SHARED-DOC-004 not-covered shapes.** These are documentation mode `None` and a preprocessor directive between the comment and the declaration, including the spec's `#if DEBUG`/`#endif` block and the RH7309 region case. The spec states them as not covered, so a test would pin no requirement. The lead files them on thoenissen/Reihitsu#1407.
- **SHARED-DIAG-004 ¶3 side (which accessibility a part of a partial member states).** The existing RH2004 partial-property tests already cover it. The spec example's verdict is unchanged, so nothing is added.
- **A CRLF variant for the RH7106 test.** It is not needed: the fix swaps two keyword tokens in place and inserts no line break.
- **Comment and directive in the gap.** No gap or line join is touched. The RH7106 directive cases already exist in the class: `PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix` and `MisorderedCompoundAccessModifiersWithDirectiveAreReportedWithoutFix`.
- **Fix All.** It is already covered by the class's `GetFixAllScenario`. The new input has only one diagnostic.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7106ProtectedMustComeBeforeInternalAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests"
scripts/test.sh --project architecture
```

Results:

- Both test classes: 56 of 56 passed.
- The new test alone: 1 of 1 passed.
- Architecture: 72 of 72 passed.
- `scripts/format.sh` on both test files: formatted 0 of 2 files.
