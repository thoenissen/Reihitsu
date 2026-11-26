Role: implementer
Commit: 6a670b6 (production change and this handover; this line amended in the follow-up commit)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I followed `00-lead.md` (decisions 1–3, answer A) and the RH2001-010 text.

Before the change, the trivia test class failed 26 of 168 cases: 12 single-property tests × LF/CRLF plus the 2 Fix All tests, exactly the rows marked "fails" in `04-test-implementer.md`'s table (its prose says "13 tests × 2"; the table has 12 — a miscount, nothing missing).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `TryCreateFieldDeclaration` no longer calls `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers`; new private `HasConditionalDirectiveNextToTheProperty` tests the leading trivia of the first modifier, and, without an initializer, of the token behind the property, for any `#if`/`#elif`/`#else`/`#endif`. The kind test is one private `IsConditionalDirective(SyntaxTrivia)`, now shared with `HasConditionalDirectiveBetweenModifiers` (its inline copy replaced). Remarks of `TryCreateFieldDeclaration` updated (FC-031) | RH2001-010, RH2001-009 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Deleted public `ConditionalGroupsWrapWholeMembers` (only caller was RH2001) | RH2001-010; lead decision 2 |
| `src/Reihitsu.Core/ConditionalGroupUnitUtilities.cs` | Deleted `ParseInTypeBody`, `WrapsWholeMembers`, `GetExcludedText`, `ReadsAsWholeMembers`; removed the now-unused `Microsoft.CodeAnalysis.CSharp` using; type summary no longer names RH2001-010 or the parse. `IsConditionalDirective`, `GetConditionalDirectives`, `IsWithoutBody` kept (callers in `OrderingDeclarationUtilities`, `OrderingMoveSafety`) | lead decision 2 |

No `SyntaxFactory.Parse*` of excluded text remains in Core, Analyzer, or CodeFixes (`rg -n "SyntaxFactory.Parse" src/Reihitsu.Core src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes`: only token/expression/trivia parses of generated or active text; `ExcludedCodeUtilities` reads tokens for names).

## Policy owners
Conditional-directive kind test: `rg -n "IfDirectiveTrivia or SyntaxKind.ElifDirectiveTrivia|IsKind\(SyntaxKind.IfDirectiveTrivia\)|is ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax" src`
- `SyntaxTriviaUtilities.IsConditionalDirective(SyntaxTrivia)` — Core, `private`.
- `ConditionalGroupUnitUtilities.IsConditionalDirective(DirectiveTriviaSyntax)` — Core, `internal` (CodeFixes cannot see it; `InternalsVisibleTo` only for `Reihitsu.Core.Test`).
- RH2001 fix — private `IsConditionalDirective(SyntaxTrivia)`; this was already an inline copy in `HasConditionalDirectiveBetweenModifiers` before the run, now named and reused by both RH2001 checks, so the count of copies did not grow.
- RH7004 fix line 197 — an inline structure-type test combined with a related-directive condition (different decision).
- `ReihitsuFormatterHelpers` line 238 — `#if` only (different decision).
Reusing a Core owner would need making `SyntaxTriviaUtilities.IsConditionalDirective` public (new public member); I did not, see *Follow-up candidates*.

## Production diff size
`git diff --shortstat -- src` (uncommitted, before this commit): 3 files changed, 32 insertions(+), 99 deletions(-). No new type, no new public member; one public member deleted.

## Deleted special cases
_N/A._ No shape is declared unsupported. (The deleted parse is the run's FC-044 removal per lead decision 2, listed under *Changes*.)

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests"` — before: Failed 26, Passed 142, Total 168.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001"` — Passed 305, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 11927, Failed 0.
- `scripts/test.sh --configuration Debug --project core` — Passed 887, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed 198, Failed 0.
- `scripts/build.sh --configuration Debug` — succeeded, no warnings in output. `scripts/format.sh` over the three files: 0 of 3 changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
- The conditional-directive kind test exists as three small private/internal copies (`SyntaxTriviaUtilities`, `ConditionalGroupUnitUtilities`, RH2001 fix). Consolidating into one public Core predicate is a small internal cleanup (≈10 lines), outside this issue.

## Continuation — lead decision 5 (adjacent cleanup)

Commit: this section's commit (`git log -1 -- sdd/work/rh2001-conditional-group-refusal/05-implementer.md`). Verdict: tests pass. No behavior change.

| File | What changed |
|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | `IsConditionalDirective(SyntaxTrivia)` changed from `private` to `public` (same body: `#if`/`#elif`/`#else`/`#endif`), moved among the public members next to `IsRegionDirective`, and its summary names the four kinds. Its two internal callers are unchanged |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Private `IsConditionalDirective` copy deleted; `HasConditionalDirectiveBetweenModifiers` and `HasConditionalDirectiveNextToTheProperty` call `SyntaxTriviaUtilities.IsConditionalDirective` |

New public member: `SyntaxTriviaUtilities.IsConditionalDirective` (it was private; it is no new logic). `ConditionalGroupUnitUtilities.IsConditionalDirective(DirectiveTriviaSyntax)` stays as is: it takes a directive node, not trivia, and was outside the decision's scope. Round delta: `git diff --shortstat` 2 files, 16 insertions(+), 26 deletions(-).

Tests run (Debug): RH2001 filter 305/305 passed; `--project core` 887/887 passed; `--project architecture` 198/198 passed. Build succeeded; `scripts/format.sh` changed 0 of 2 files.
