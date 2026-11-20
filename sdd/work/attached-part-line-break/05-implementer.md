Role: implementer
Commit: d86230e
Verdict: tests pass

Prompt note: the prompt carried no intended fix, suspected cause, or hoped-for verdict. It named decision #1 of `00-lead.md` (move the predicate to Core), which is a recorded lead decision, and I carried it out.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/AttachedPartGapUtilities.cs` (moved from `src/Reihitsu.Formatter/Pipeline/Core/Utilities/`) | Moved unchanged in logic to namespace `Reihitsu.Core`. The class, `IsAttachedPartGap`, `StartsAttachedPart`, and `IsElementBindingOpenBracket` (was private) are now `public`. The summary names RH5115/RH5116 as further users. The type name is kept, so the FMT-BREAK code-owner lists, the spec introductions, and the `AGENTS.md` phase table stay valid. | Decision #1, FMT-BREAK-040, FMT-UNSUP-014 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/AttachedPartLineStartRewriter.cs`, `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | Removed the `using Reihitsu.Formatter.Pipeline.Core.Utilities;` that the move left unused. `ListElementInteriorJoinRewriter.cs` keeps it for `ListElementInteriorUtilities`. All three already import `Reihitsu.Core`. | Decision #1 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5115IndexAccessMustStayOnExpressionLineAnalyzer.cs` (new) | Node action on `ElementAccessExpression` and `ConditionalAccessExpression`. The first token (`[` or `?`) must satisfy `AttachedPartGapUtilities.StartsAttachedPart`. It is reported when it starts on a later line than the previous token ends on, or, for `?`, when the `[` behind it (`IsElementBindingOpenBracket`) starts on a later line than the `?`. Lines are compared by line numbers, so comments, directives, disabled text, and blank lines in the gap do not prevent the report. A file with a syntax error is skipped through `SyntaxTreeErrorVerdictCache`. | RH5115-001, -002, -003, SHARED-JOIN-002 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer.cs` (new) | Node action on `SuppressNullableWarningExpression`. The `!` must satisfy `StartsAttachedPart`, which excludes `!.` and `!?.`. It is reported when it starts on a later line than the previous token ends on. Syntax-error files are skipped the same way. | RH5116-001, -002, -003, SHARED-JOIN-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/AttachedPartLineBreakCodeFixProviderBase.cs` (new) | The shared fix for both rules. The gaps are the gap in front of the reported token, plus the `?`→`[` gap when the next token is an element-binding `[`. The fix is withheld when any gap holds a comment or directive (`SyntaxNodeUtilities.SpanContainsCommentOrDirective`; disabled text always sits between directives). When the part's outermost chain node (`FluentChain.GetOutermostChainNode`) yields a chain with links (`FluentChain.Create(...) != null`), the fix uses `FluentChainCodeFixHelper.FormatChainAsync` and is withheld when `IsFormattingRefused` holds (SHARED-FIX-010). Otherwise it deletes the non-empty gaps and nothing else. `FormatsThroughFormatter => true`. Fix All is `DocumentOrderFixAllProvider`. | RH5115-004, -005, -006, RH5116-004, -005, -006, SHARED-WS-003, SHARED-FIX-002, SHARED-FIX-010 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5115…CodeFixProvider.cs`, `RH5116…CodeFixProvider.cs` (new) | Thin subclasses of the base, passing the analyzer instance, the ID, and the title. | — |
| `AnalyzerResources.resx`, `CodeFixResources.resx`, `CodeFixResources.cs` | `RH5115Title`/`MessageFormat` and `RH5116Title`/`MessageFormat` reuse the spec titles. Code-fix titles: "Move index access onto the line of its expression" and "Move null-forgiving operator onto the line of its expression", each with a wrapper property. | — |
| `Release.ruleset`, `Reihitsu.sln`, `src/Reihitsu.Analyzer.Package/README.MD` | Added entries for RH5115 and RH5116. The README rows are marked ✔ for analyzer, code fix, and formatter, because the formatter joins the parts (FMT-BREAK-040). | — |

The rule pages already existed (spec author), so I did not write them.

**Fix All choice.** The first version used `WellKnownFixAllProviders.BatchFixer`. It failed `FixAllCorrectsAJoinedAccessInsideAChainItLaysOut` with "Expected '1' iterations but found '2'": the batch merger drops the join of `arr⏎[0]` because it overlaps the chain fix's text change. `DocumentOrderFixAllProvider` (the existing SHARED-FIX-002 provider, also used by RH5110 and RH5307) applies the join first and then lays out the chain on the joined code. It reaches the expected text in one iteration.

## Policy owners

- Attached-part predicate: `rg -n "SuppressNullableWarningExpression|ElementBindingExpressionSyntax" src --glob '!*Test*' -l` lists `Reihitsu.Core/AttachedPartGapUtilities.cs` (the owner) and `Reihitsu.Core/FluentChain.cs` (the chain model, whose attached-part folding is a different decision; see 02-skeptic, decision #2). The other hits are unrelated: RH1001, RH3002, `EqualityComparerArgumentUtilities`, `FluentChainAnalyzerBase`. RH5116 names the kind only to register its node action. `rg -n "AttachedPartGapUtilities" src` lists the formatter precheck, `AttachedPartLineStartRewriter`, `ListElementInteriorJoinRewriter`, both analyzers, and the fix base. No copy is left in the formatter or the analyzers.
- Chain membership and chain layout: `rg -n "AttachedParts|GetOutermostChainNode" src --glob '!*Test*'` lists `FluentChain` (Core), `FluentChainCodeFixHelper`, `FluentChainAnalysisHelper`, and the RH5111 fix. The new base uses `FluentChain` and `FluentChainCodeFixHelper` directly.
- Gap join (SHARED-WS-003): `CollapseTokenGapCodeFixProviderBase` (RH5105) deletes one gap. I did not use it, because the RH5115 `?[` case deletes two gaps and the chain branch formats. The base deletes the gaps with plain `TextChange`s.
- Comment/directive guard: `SyntaxNodeUtilities.SpanContainsCommentOrDirective` is reused.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 12 files changed, 558 insertions(+), 10 deletions(-). That includes the resx, README, and wrapper additions; the predicate move counts as a rename.

New types: `RH5115IndexAccessMustStayOnExpressionLineAnalyzer`, `RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer`, their two code-fix providers (all four named by the specs), and `AttachedPartLineBreakCodeFixProviderBase` (shared fix base).

New public members: `AttachedPartGapUtilities` (now public in Core), including the formerly private `IsElementBindingOpenBracket`.

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5115|FullyQualifiedName~RH5116"`: Passed 112, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 11528, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 6405, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 747, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198, Failed 0.
- `scripts/build.sh --configuration Debug`: 0 errors, no warnings after `scripts/format.sh` on the new files.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Disputed tests or scope

- **Scope: the shared helper gained consumers.** `AttachedPartGapUtilities` is now a public Core type with consumers in the formatter and in both analyzers. This is decision #1 itself. `IsElementBindingOpenBracket` became public because the analyzer and the fix need the `?`→`[` gap.
- **One new type beyond the four the specs name**: `AttachedPartLineBreakCodeFixProviderBase`. Without it, the two providers would carry an identical copy of the gap, guard, and chain-or-join logic.

## Architecture findings

_None._

## Follow-up candidates

_None._
