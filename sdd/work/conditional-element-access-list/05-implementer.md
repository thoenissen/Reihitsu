Role: implementer
Commit: 938ad38c
Verdict: #1877: tests pass; #1878: test disputed (3 tests, 6 rows; production behavior matches the spec in each)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Lead decision 1 (one shared predicate in an existing `Reihitsu.Core` utility class, no new type) is carried out in 938ad38c.

## thoenissen/Reihitsu#1877

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxNodeUtilities.cs` | New `public static bool IsIndexArgumentList(BracketedArgumentListSyntax)`: parent is `ElementAccessExpressionSyntax`, `ElementBindingExpressionSyntax`, or `ImplicitElementAccessSyntax` (fixed-size buffer excluded). The one statement of which lists hold an index | FMT-BREAK-014, RH5307-001 (lead decision 1) |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | `IsElementAccessArgumentList` asks `SyntaxNodeUtilities.IsIndexArgumentList` instead of its own parent-kind test; still adds the root-construct fact for a parentless root. Summary rewritten (FC-031). Its two callers (`CanSafelyCollapseBracketedArguments`, `ReihitsuFormatterHelpers.CaptureRootConstruct`) are unchanged, so the node-level root fact `ElementAccessArguments` now also covers an element binding's list | FMT-BREAK-014, FMT-PIPE-024 |
| `src/Reihitsu.Formatter/Data/Enumerations/RootConstruct.cs` | Summary of `ElementAccessArguments` names the element binding (comment only) | FC-031 |

### Policy owners

`rg -n "IsElementAccessArgumentList|IsIndexerArgumentList|IsIndexArgumentList" src --glob '!*Test*'` before: `LineBreakListRewriter.IsElementAccessArgumentList` (formatter, also behind `CaptureRootConstruct`) and `RH5307…Analyzer.IsIndexerArgumentList` (analyzer, also used by the code fix) — two copies with the same omission. After: one owner, `SyntaxNodeUtilities.IsIndexArgumentList`; the formatter wrapper only adds the root fact. `rg -n "Parent is (ElementAccessExpressionSyntax|ElementBindingExpressionSyntax|ImplicitElementAccessSyntax)" src --glob '!*Test*'` finds only the new owner and RH5307's location choice (element access → whole access, else the list), which is the location decision of RH5307-003, not eligibility. No spec or doc names either old predicate.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 5 files changed, 25 insertions(+), 21 deletions(-) (both issues together). No new type; one new public member in `Reihitsu.Core` (the shared predicate decision 1 calls for).

### Deleted special cases

_N/A._

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ConditionalElementAccessArgumentListTests|FullyQualifiedName~ConditionalElementAccessJoinsWrappedArgumentsWhenNestedIndexerCollapses|FullyQualifiedName~ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed"` — `Passed! - Failed: 0, Passed: 23`.
- The four code-fix tests (RH5101, RH5111, RH5302, RH5308 `VerifyFixJoinsConditionalElementAccessList…`) and the parity tests in `RH5307…FormatterTests` pass (inside the analyzer run below).

### Disputed tests or scope

_None._

### Architecture findings

_None_ beyond the skeptic's, which decision 1 resolves.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1878

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.cs` | Private copy `IsIndexerArgumentList` deleted; `OnBracketedArgumentList` asks `SyntaxNodeUtilities.IsIndexArgumentList`. Location code unchanged: an element binding's list falls into the existing "the list" branch (answer 2A). Summary updated | RH5307-001, RH5307-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider.cs` | `TryGetFormatTarget` asks the shared predicate (`using Reihitsu.Core` added); its summary names the element binding among the list-located diagnostics. The existing `BracketedArgumentListSyntax list when list.Span == span` arm resolves the binding's list | RH5307-004, SHARED-FIX-006 |

### Policy owners

See #1877 (same owner).

### Production diff size

See #1877.

### Deleted special cases

_N/A._

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~VerifyFixJoinsConditionalElementAccessList|FullyQualifiedName~RH5307IndexerBracketedArgumentsShouldBeSingleLined"` — `Failed! - Failed: 6, Passed: 91, Total: 97`; the six rows are the three disputed tests below.

### Disputed tests or scope

All three are in `src/Reihitsu.Analyzer.Test/Formatting/RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.cs`. Not changed by me.

1. `VerifyFixJoinsConditionalElementAccessListsAndKeepsTheCodeAround` (LF, CRLF) and 2. `VerifyFixesJoinTheListsOfTwoConditionalElementAccesses` (LF, CRLF).
   - Asserts: through `VerifyCodeFixWithEndOfLine`, which after the Roslyn verifier (`Verify`, iterations + Fix All) also asserts that **one** application of the first offered action (`ApplyCodeFixAsync(plainSource)`, first fixable diagnostic only) yields the fully fixed source. Both sources hold several diagnostics (four in test 1: `a`, the two `n` lists, `r`; two in test 2).
   - Spec: RH5307-004 / SHARED-FIX-006 — a fix changes only the list its diagnostic is on. So one application fixes only the first list. Observed: test 1 single application gives `var a = c?[1, 2] ?? 0;` with `n` and `r` still wrapped; test 2 gives `e?[0, 1]?[1,⏎2]`.
   - The `Verify` part (all diagnostics located as marked, iterations and Fix All reach the expected source) passes in both; the failure is at `AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs:132`. The existing multi-diagnostic test in the same file, `VerifyFixesJoinEveryReportedListWhateverItsReceiverOrContainer`, uses `Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine))` instead. No visible output is in question: the expected final text is right, the helper is not.
3. `VerifyFixAndIndexAccessFixConvergeInEitherOrder` (LF, CRLF).
   - Asserts: RH5115 on `c?⏎[1, 2]` and `c?⏎[1,⏎2]` is located on the `[` (`{|RH5115:[|}`, expected 6,13–6,14).
   - Observed: RH5115 reports on the `?` (5,18–5,19). That is RH5115's unchanged behavior: `RH5115IndexAccess…Analyzer.TryGetFirstToken` takes a conditional access's `OperatorToken` as the first token of the index access and reports on it; this run does not touch RH5115.
   - Probe (file restored with `git checkout` afterwards, nothing committed): with the two markups moved to `c{|RH5115:?|}⏎[…` the test passes in both rows, so every RH5307 and RH5115 step of the convergence it checks holds; only the RH5115 location in the markup is wrong. RH5115's location is existing behavior, not in question.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/format.sh` over the five changed production files: `Formatted 0 of 5 file(s).`
- `scripts/test.sh --configuration Debug --project formatter`: `Passed! - Failed: 0, Passed: 6479`.
- `scripts/test.sh --configuration Debug --project analyzer`: `Failed! - Failed: 6, Passed: 11607, Total: 11613` — only the six disputed rows above.
- `scripts/test.sh --configuration Debug --project core`: `Passed! - Failed: 0, Passed: 807`. The new public `SyntaxNodeUtilities.IsIndexArgumentList` has no direct Core test; it is covered through the formatter and RH5307 tests (including the fixed-size-buffer characterization). The test implementer may add one if the lead wants it.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed! - Failed: 0, Passed: 198`.
