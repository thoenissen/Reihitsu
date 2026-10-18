Role: test implementer
Commit: 5ffd719
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It carried constraints (no production change, every covering test passes today, no tests for the known-bug sides), and I followed them.

Lane: specs and tests for existing behavior inside the run's tooling claim. Every test below passes on 5ffd719 (production code unchanged since 77a01eb). No test derived from a spec example failed, so nothing is reported as a finding against the spec.

## Tests

All in `src/Reihitsu.Analyzer.Test`. "New" = added in 5ffd719; "tagged" = existing test that is exactly the spec example and only gained the attribute. Expected values come from the spec's after-blocks; where the spec states a result in prose only, the column says so.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5002…AnalyzerTests.OneApplicationCorrectsTheLineInFrontOfTheReportedIf` (new) | SHARED-FIX-001; FC-007, FC-017 | `Verify`, `NumberOfIncrementalIterations = 1` | pass |
| `…OneApplicationCorrectsTheLineInFrontOfTheReportedIfUnderCarriageReturnLineFeed` (new) | SHARED-FIX-001; FC-033 | same, CRLF | pass |
| `RH5028…AnalyzerTests.OneApplicationRemovesTheWholeReportedRun` (new) | SHARED-FIX-001 (whole run; RH5028 half of the two-rule example) | `Verify`, iterations = 1 | pass |
| `…OneApplicationRemovesTheWholeReportedRunUnderCarriageReturnLineFeed` (new) | SHARED-FIX-001 | same, CRLF | pass |
| `RH5023…AnalyzerTests.OneApplicationPerDiagnosticCorrectsTheRun` (new) | SHARED-FIX-001 | `Verify`, iterations = 2 | pass |
| `…OneApplicationPerDiagnosticCorrectsTheRunUnderCarriageReturnLineFeed` (new) | SHARED-FIX-001 | same, CRLF | pass |
| `…OneApplicationPerDiagnosticCorrectsTheRunAtTheStartOfTheFile` (new) | SHARED-FIX-001 (RH5023 half of the two-rule example: leaves the first blank line, not reported again) | `Verify`, iterations = 2 | pass |
| `…OneApplicationPerDiagnosticCorrectsTheRunAtTheStartOfTheFileUnderCarriageReturnLineFeed` (new) | SHARED-FIX-001 | same, CRLF | pass |
| `RH4106…AnalyzerTests.NoCodeActionIsRegisteredWhenTheRenameWouldCauseACompileError` (new) | SHARED-FIX-003 compile error, triggering side; FC-009 | `Verify` (reported) + `GetCodeFixActionsAsync` empty | pass |
| `RH4106….FixIsOfferedWhenTheRenameCausesNoCompileError` (new) | SHARED-FIX-003 compile error, other side (FC-003) | `Verify` | pass |
| `RH4106….FixRenamesFieldReferencedInNameof` (tagged) | SHARED-FIX-003 run time: the new name does not count; FC-036 | `Verify` | pass |
| `RH4106….FixRenamesTheBoundDocumentationReferenceAndKeepsTheCommentText` (new) | SHARED-FIX-003 comment text: bound `cref` renamed, look-alike word kept; FC-018 | `Verify` | pass |
| `RH4106….FixLeavesAnExcludedBranchThatDoesNotNameTheFieldUnchanged` (new) | SHARED-FIX-003 `#if` condition, non-triggering side; FC-020 | `Verify` | pass |
| `RH4106….NoCodeActionIsRegisteredWhenTheFixWouldNotChangeTheDocument` (new) | SHARED-FIX-005; FC-009 | `Verify` (reported) + `GetCodeFixActionsAsync` empty | pass |
| `RH7103…AnalyzerTests.NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField` (new) | SHARED-FIX-003 run time, triggering side (`_b`); both fields reported on the original document | `Verify` + `GetCodeFixActionsAsync` empty | pass |
| `RH7103….CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField` (new) | SHARED-FIX-003 run time, other side (`_a`) | `GetCodeFixActionsAsync` non-empty; applied result has `_a` before `_instance` and before `_b` (from the spec's reason; the spec gives no after-block) | pass |
| `RH7109…AnalyzerTests.FixReordersTheFieldsOfAnExplicitLayoutStructAndKeepsEachOffset` (new) | SHARED-FIX-003 layout qualifier, explicit side | `Verify`, exact spec block | pass |
| `RH7109….FixReordersTheFieldsOfAnAutoLayoutStruct` (new) | SHARED-FIX-003, `LayoutKind.Auto` sentence | `Verify` (spec prose: `_b` moves in front of `_a`) | pass |
| `RH7109….FixReordersTheFieldsOfAClassWithoutLayoutAttribute` (new) | SHARED-FIX-003, class-without-attribute sentence | `Verify` (spec prose) | pass |
| `RH7302…AnalyzerTests.VerifyCodeFixRenamesOnlyTheEndRegionCommentThatRepeatsTheRegionName` (new) | SHARED-FIX-003 `#endregion` comment, both sides in one block | `Verify`, exact spec block | pass |
| `RH3202…AnalyzerTests.VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals` (new) | SHARED-FIX-003 layout; FC-022, FC-030 | `Verify`, exact spec block | pass |
| `RH5601…AnalyzerTests.VerifyCodeFixKeepsTabInsideTheTextOfATrailingComment` (new) | SHARED-FIX-003 whitespace inside comment text is content | `Verify`, exact spec block | pass |
| `…VerifyCodeFixKeepsTabInsideTheTextOfATrailingCommentUnderCarriageReturnLineFeed` (new) | SHARED-FIX-003; FC-033 | same, CRLF | pass |
| `RH8107…AnalyzerTests.VerifyCodeFixChangesOnlyTheReportedElementOfTheDocumentationComment` (new) | SHARED-FIX-003 text the rule reports; space in front is layout | `Verify`, exact spec block | pass |
| `RH5604…AnalyzerTests.VerifyCodeFixConvertsEveryLineEndingWhicheverDiagnosticItIsAppliedTo` (tagged) | SHARED-FIX-003 RH5604 sentence: doc-comment line breaks rewritten, verbatim-literal `\r\n` kept | `GetCodeFixActionsAsync` + applied text | pass |
| `Shared/CodeFixInteractionTests.RH4106FixDoesNotRestoreTheCodeTheRH2005FixStartedFrom` (new class) | SHARED-FIX-004; FC-017 | three verifiers in sequence: RH2005 fix `public string first` -> `private string first`; RH4106 fix on that exact text -> `private string _first`; RH2005 analyzer reports nothing on the result | pass |

The SHARED-FIX-001 tests pin the exact iteration count (one per diagnostic) instead of the framework's default upper bound. The two-rule example has no single test, because each surface only runs its own analyzer. Its two halves are `RH5028….OneApplicationRemovesTheWholeReportedRun`, on exactly the example's text, and `RH5023….OneApplicationPerDiagnosticCorrectsTheRunAtTheStartOfTheFile`.

`CodeFixInteractionTests` is a plain `[TestClass]` and does not derive from `AnalyzerTestsBase<,>`, so `CodeFixTestBaseCoverageTests` does not discover it. It instantiates `CSharpCodeFixVerifierTest` with `ReferenceAssemblies.Net.Net90`, as the bases do.

## Scenarios (bug fix)

_N/A._ No bug fix in this run.

## Not covered, and why

- **Known-bug sides, deliberately not tested** (listed in `03-skeptic.md` and `05-spec-author.md` as failing today):
  - RH5022 on a run;
  - RH4106 with `Value` used under `#if NEVER` (thoenissen/Reihitsu#1073);
  - the RH5024/RH5032 cycle and RH5023 at the end of the file (thoenissen/Reihitsu#1076);
  - RH7109 on a default-layout struct or a `[StructLayout(LayoutKind.Sequential)]` class;
  - RH5011 inside `/* */` (thoenissen/Reihitsu#1075);
  - RH7103 with an unrelated `#region helpers`;
  - RH7302 substring renaming inside the `#endregion` text (open question in `05-spec-author.md`).

  So SHARED-FIX-003's layout qualifier, its comment-content qualifier and its `#if` condition are covered on the non-triggering side only. The triggering side of each is a regression test for its follow-up.
- **RH2005 then RH4106 Fix All (FC-008):** the chain is single-diagnostic. SHARED-FIX-002 stays with `FixAllCorrectsEveryDiagnostic` in every batch class, unchanged by this run.
- **Line endings:** CRLF variants exist for the SHARED-FIX-001 tests and for RH5601, the layout-sensitive edits. The RH3202, RH7302, RH8107, RH4106, RH7103 and RH7109 tests are LF only. SHARED-FIX-003 decides which text a fix may change, not the line endings of inserted text, and each rule's existing CRLF tests stay in place.
- **FC-019/FC-021 (comment and directive in a gap):** this run adds no gap rule. The `#if` test (FC-020) and the RH5601 comment test are the trivia cases the spec examples carry.
- **FC-010 to FC-014 (formatter stability) and FC-028:** no formatter or code-fix change in this run.
- **FC-035:** expected text is copied from the spec blocks. For three tests the spec states the result in prose only, and their expected values follow from that prose:
  - the RH7103 `_a` order;
  - the RH7109 `Auto` struct;
  - the RH7109 class without the attribute.

  For the RH7103 `_a` case I deliberately did not assert the iterated two-fix result, as `05-spec-author.md` asks.
- **Watch-fail:** every test here covers existing behavior and must pass, so none was driven red. Some sides have no counterpart test in this table:
  - The `GetCodeFixActionsAsync`-empty assertions have counterparts that the same helper resolves to a registered action: the `_a` test in this run, and the existing RH5604 test.
  - The collision and `_1st` cases each also run `Verify` with markup, so the diagnostic is proven reported.

## Commands

```shell
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-001|TestCategory=SHARED-FIX-003|TestCategory=SHARED-FIX-004|TestCategory=SHARED-FIX-005"
scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~RuleDocumentation"
```

Results on 5ffd719:
- Analyzer, the four categories: 252 passed, 0 failed. That count includes every class's `FixAllCorrectsEveryDiagnostic`, which carries SHARED-FIX-001.
- The 26 tests above, run by name: 26 passed.
- Architecture spec tests: 39 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered`.
- Full architecture project: 72 passed.
- Analyzer `SelfHosting` and `Shared` namespaces: 22 passed.

The changed files were formatted with `scripts/format.sh` before the run.
