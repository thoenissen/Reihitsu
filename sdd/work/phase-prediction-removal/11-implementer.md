Role: implementer
Commit: 37a5ea4
Verdict: test disputed

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This round implements RH5303-006 and RH5304-005, the maintainer's answer to escalations 5 and 12 (option A, `squad/decisions/2026-10-25-phase-prediction-removal-node-level-shift.md`).

All tests in `10-test-implementer.md` pass except two. Those two cannot pass as written, because the test framework ignores the diagnostic they mark in their fixed source; see "Disputed tests or scope". I did not change them.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs` | New public `ContainsComplexElementBehindMultiLineCommentTail(node)`: is there a complex element initializer anywhere in `node` whose opening brace starts its line inside a comment? The test, in the new private `StartsLineInsideComment`: a comment in the gap before the brace (the previous token's trailing trivia plus the brace's leading trivia) starts on an earlier line and ends on the brace's line, in front of the brace. It uses the trivia's full span, so a `/** … */` counts with its delimiters. A `///` comment ends at the start of the next line, so it never matches. | RH5303-006, RH5304-005 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider.cs` | `RegisterCodeFixesAsync` registers no action when that check is true for the code the fix formats (`TryGetFormatTarget`: the declaration value, statement, or expression body around the reported creation, or the creation itself). The diagnostic stays reported. | RH5303-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider.cs` | The same guard, same scope. | RH5304-005 |

The guard covers the whole code the fix formats, not only a diagnostic on the element, as `09-spec-author.md` states. So a diagnostic elsewhere in that code also gets no fix: RH5303-001 on the outer braces, or RH5304-001 on another assignment. Fix All uses `RegisterCodeFixesAsync` per diagnostic, so it skips the withheld creations and fixes the others.

Scope:
- The new method is added to the shared helper. Its existing methods and their callers (RH5301, RH5303, RH5304) are unchanged.
- No new type, so no spec owner list changes. SHARED-INIT's owner `InitializerLayoutAnalysisUtilities` holds the check, and both fixes call it, so the policy has one owner.
- No production formatter code changed in this round.

## Policy owners

- `rg -n "ContainsComplexElementBehindMultiLineCommentTail|StartsLineInsideComment" src --glob '*.cs'` → declared in `InitializerLayoutAnalysisUtilities`; called by the RH5303 and RH5304 providers. No other copy.
- The question of whether a line starts inside a comment is also answered elsewhere, for other purposes:
  - The formatter has `LayoutComputer.TryGetLineTextColumn` (layout, fa7fb0f) and `RawStringLineShifts.StartsInsideNestedText` (raw-string move). Both are formatter-internal; the analyzer assembly cannot reference them.
  - `rg -n "StartsInside|InsideComment" src/Reihitsu.Core src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes` finds no shared helper for this check, so none was reused.

## Tests run

All counts were observed on the tree of 37a5ea4.

| Command | Result |
|---|---|
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5301\|FullyQualifiedName~RH5303\|FullyQualifiedName~RH5304\|FullyQualifiedName~RH5305"` | `Failed: 2, Passed: 148, Total: 150` |
| `scripts/test.sh --configuration Debug --project analyzer` | `Failed: 2, Passed: 6210, Total: 6212`. On b826c6b the test implementer saw 8 failures. |
| `scripts/test.sh --configuration Debug --project formatter` | `Passed: 2972` |
| `scripts/test.sh --configuration Debug --project architecture` | `Passed: 84` |
| `scripts/test.sh --configuration Debug --project core` | `Passed: 533` |
| `scripts/test.sh --configuration Debug --project cli` | `Passed: 250` |
| `scripts/test.sh --configuration Debug --project tooling` | `Passed: 120` |
| `scripts/test.sh --configuration Debug --project playground` | `Passed: 17` |

`scripts/format.sh --force` changed none of the three files. The Debug build output shows no warning for the changed files: one RH7110 member-order warning came up in an intermediate state and was resolved by moving the private method behind the public ones, inside the file's `Methods` region as the other `Analyzer/Core` helpers do.

## Disputed tests or scope

Two tests, the same shape under two rules:
- `Reihitsu.Analyzer.Test.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`
- `Reihitsu.Analyzer.Test.Formatting.RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`

**What they assert.** The fixed source keeps the withheld creation's diagnostic as markup (`{|RH5303:{ … }|}`, and `{|RH5304:…|}` in the RH5304 test). The tests mean that diagnostic to remain after fixing, as RH5303-006 and RH5304-005 require.

**What happens.** Both fail before any text is compared, with:

```
Context: Diagnostics of fixed state
Mismatch between number of diagnostics returned, expected "0" actual "1"
```

The one actual diagnostic is the withheld creation's: RH5303 at (10,29)-(13,22), RH5304 at (13,21)-(19,26).

**Why.** The two tests set no markup handling for their fixed state, so Microsoft.CodeAnalysis.Testing applies its default there:
- `MarkupMode.IgnoreFixable`: "For fixed (output) sources, the default value is IgnoreFixable", which means "fixable diagnostics suggested by markup syntax are ignored". Source: the XML docs of `Microsoft.CodeAnalysis.CodeFix.Testing` 1.1.4.
- RH5303 and RH5304 are in their providers' `FixableDiagnosticIds`, so the marked diagnostic is dropped, and the framework expects none.

No production behavior can satisfy "expected 0": the diagnostic remains because the spec requires it.

**Evidence.**
- I temporarily added `config.FixedState.MarkupHandling = MarkupMode.Allow;` to both tests, next to `NumberOfFixAllIterations`. Both then passed: the remaining diagnostic matches, and the fixed text matches as well (32/28 for RH5303, 40/36 for RH5304).
- I reverted that edit with `git checkout`. Neither test file is modified in any commit of mine.

Whether to fix the tests that way, or with an explicit `FixedState.ExpectedDiagnostics`, is the test implementer's call (escalation 4 route).

## Architecture findings

_None new._ The node-level column steps (`ComputeTokenColumn` / `AdjustNodeIndentation`) are the follow-up the maintainer's decision records.

## Follow-up candidates

- Restructuring the node-level column steps, together with the skeptic's follow-ups 1 and 2 (`02-skeptic.md`) and the finding in `08-implementer.md`. This is already recorded by the decision and the change doc.
- From `05-implementer.md`, still open:
  - A node-level target with a tab in its own whitespace is detached from its document by the first phase.
  - No committed test runs the indentation phase on an attached node that contains a raw string.
- The RH5201 check of change doc row 10 (`07-test-implementer.md`, follow-up 2).
- The RH5304 statement and expression-body targets of the guard are untested (`10-test-implementer.md`). The guard does not depend on the target kind: it inspects whatever node `TryGetFormatTarget` returns, and that method is identical in the two providers.
