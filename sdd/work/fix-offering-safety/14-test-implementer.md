Role: test implementer
Commit: 417800b8
Verdict: #1600: tests written; #1150: nothing; #1274: nothing; #1143: nothing; #1673: nothing; #1678: nothing

Repair round 2 for audit findings #1 and #2 (13-auditor.md; 00-lead.md decisions 21–22). My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-FIX-012 (a document holding a hiding carrier gets no fix, so Fix All leaves it unchanged with its diagnostic reported) and FMT-PIPE-033 (only the words `elif`, `else`, `endif` count). "Fails today" means: run on bc39767c plus these tests, the test fails on the carrier document's content (the Fix All edited it), not on a compile error or unrelated layout.

## thoenissen/Reihitsu#1600

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged` | SHARED-FIX-012 | `Verify` with a second document `/0/Test1.cs`, markup kept in fixed and batch state, one iteration each | fails ("Context: Fix all in project", `value = (value); //";` written as `value = value; //";`) |
| `RH3002…FixAllInTheSolutionLeavesADocumentWithAHidingCarrierInAnotherProjectUnchanged` | SHARED-FIX-012 | `Verify` with the carrier document in an additional project `Other` | fails ("Context: Fix all in solution", same edit); project scope passes because the carrier is in another project |
| `RH7101DoNotCombineFieldsAnalyzerTests.FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged` | SHARED-FIX-012 | as above | fails ("Context: Fix all in project", `private int c, d;` split into two declarations) |
| `RH7101…FixAllInTheSolutionLeavesADocumentWithAHidingCarrierInAnotherProjectUnchanged` | SHARED-FIX-012 | as above, additional project | fails ("Context: Fix all in solution", same split) |
| `RH3102CodeMustNotContainEmptyStatementsAnalyzerTests.FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged` | SHARED-FIX-012 | as above | passes (the `DocumentOrderFixAllProvider` path; pins it against the restructuring of decision 21) |
| `ReihitsuFormatterHiddenCodeTests.EveryLineKindThatEndsTheGroupCounts`, new rows `#else// c`, `#elif(Y)` | FMT-PIPE-033 | `AssertLeftAlone` (existing) | passes |
| `ReihitsuFormatterHiddenCodeTests.CarrierWithoutALineThatEndsTheGroupIsFormatted`, new rows `#elsewhere` (letter), `#endif2` (digit), `#else_1` in a verbatim string (underscore), `#elifx Y` (letter) | FMT-PIPE-033 | `AssertFormattedThroughEntryPointContains` (existing) | passes; each row is refused if the word-boundary condition is dropped |

Carrier documents (RH3002 and RH7101 as class constants `CarrierDocumentSource`, RH3102 inline): a `/* */` comment that starts inside `#if !X` and holds `#else`, with the reported construct behind it (RH3002: on the line a build with `X` reads as string content, the auditor's counterexample; RH7101: the combined declaration in front of the carrier, in the same type).

### Fix All paths checked

`rg -n "override FixAllProvider" -A3 src/Reihitsu.Analyzer.CodeFixes --include=*.cs` and `rg -n "FixAllProvider.Create|: FixAllProvider" src/Reihitsu.Analyzer.CodeFixes --include=*.cs`: 88 `WellKnownFixAllProviders.BatchFixer` (re-registers through the base; covered by RH6024 `FixAllLeavesADocumentWithAHidingCarrierUnchanged`), 15 `DocumentOrderFixAllProvider.Create` (re-registers at `DocumentOrderFixAllProvider.cs:167`; covered by the new RH3102 test), 1 `CasingFixAllProvider` (restates the check in `RenameWithoutConflictAsync`; covered by RH4103 `FixAllLeavesADocumentWithAHidingCarrierUnchanged`), 1 `null`, and 2 `FixAllProvider.Create(ApplyFixAllAsync)` — RH3002 and RH7101, the only paths that bypass registration. Every rule provider derives from the base (`rg -L "CodeFixProviderBase" src/Reihitsu.Analyzer.CodeFixes/Rules` is empty).

### Not covered, and why

- Solution scope for BatchFixer, DocumentOrder, and Casing with the carrier in another project: they reach the decision through the same per-document registration (or rename check) as project scope; the existing and new project-scope tests also run the framework's solution-scope pass over one project.
- SHARED-FIX-011 on the two custom paths: RH3002 does not format, and RH7101 reports nothing in a file with a syntax error (13-auditor.md #1).
- Audit #3 (CLI seam): an internal seam with unchanged behavior; existing CLI-021 tests cover the output.

## thoenissen/Reihitsu#1150

Nothing in this round.

## thoenissen/Reihitsu#1274

Nothing in this round.

## thoenissen/Reihitsu#1143

Nothing in this round.

## thoenissen/Reihitsu#1673

Nothing in this round.

## thoenissen/Reihitsu#1678

Nothing in this round.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~HidingCarrier"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHiddenCodeTests"
```
