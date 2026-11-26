Role: implementer
Commit: the commit "Route duplicated helpers to one owner [skip ci]" (production changes and this handover)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The handed-over tests are characterization tests that pass before and after the change (lead decision 1), so step 1 of the job ("see them fail") does not apply; they passed on 4c4a9ec and pass after the change.

## thoenissen/Reihitsu#1956

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | New `public static bool IsOwningConstruct(SyntaxNode, SyntaxToken)` next to `IsIndentingScope`; body moved verbatim (all eleven arms, `AccessorListSyntax` kept, default arm calls `IsIndentingScope`). Its summary lists every arm, the accessor list included, and names both consumers | SHARED-COMMENT-006; FC-015 |
| `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs` | Copy and its "must stay in step" summary deleted; `IsOwnedByNestedConstruct` calls the Core owner, its cref points there | FC-015, FC-031 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/ListElementInteriorUtilities.cs` | Private copy deleted; `LiesInListElement` and `FindOwningConstruct` call the Core owner; `FindOwningConstruct`'s summary now refers to the owner instead of repeating a construct list that omitted the accessor list | FC-015, FC-031 |

Owning class: `SyntaxIndentationUtilities` (lead decision 2), so no new type. The skeptic's note on `ListElementInteriorJoinRewriter.cs:36` and `ListElementAligner.cs:84` needs no change: both name `FindOwningConstruct`, which stays.

## thoenissen/Reihitsu#1958

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/ConditionalGroupUnitUtilities.cs` | Node-form `IsConditionalDirective(DirectiveTriviaSyntax)` deleted; `GetConditionalDirectives` filters the trivia with `SyntaxTriviaUtilities.IsConditionalDirective` before taking the structure (summary points at the owner) | FC-015 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `DeclarationHoldsConditionalDirective` filters leading trivia with the owner before taking the structure; the separate membership check is gone | FC-015 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.cs` | `WouldSeparateDirective` uses `SyntaxTriviaUtilities.IsConditionalDirective(trivia)` instead of the inline type pattern | FC-015 |

Node-form choice (skeptic internal choice 1): the callers already hold the trivia, so the node form is deleted rather than given an overload; no new public member. The left-alone uses the issue asks to name are the ones in `01-skeptic.md` (*Premise*); `DirectivePairSplitScan` (per-kind switch) is among them.

## thoenissen/Reihitsu#1963

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `FindTriviaBehindToken` is the one statement of the token-to-node read. `FindTriviaBehindConditionalOperator` (both gaps, `?` then `:`, same order as before), `FindTriviaBehindBinaryOperator`, `FindTriviaBehindExpressionBodyArrow`, and `FindTriviaBetweenLinkDotAndName` call it. The four token-to-token readers are unchanged (lead decision 3, B). `FindTriviaBehindToken`'s summary states the read order and lists every gap it serves (FMT-UNSUP-008, -011, -012, -016, -020 to -022) | FMT-UNSUP-008, -011, -012, -016, -020, -021, -022; FC-015, FC-016 |

The named wrappers stay as one-line methods, so the scope facts in the binary-expression and arrow-clause summaries (what counts as a binary expression, what an arrow clause is) stay next to the dispatch (skeptic internal choice 2).

## Whole run

### Policy owners
- `rg -n -U "AnonymousObjectCreationExpressionSyntax\s*\n\s*or SwitchExpressionSyntax" src` — one hit: `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` (the owner).
- `rg -n "ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax|IsConditionalDirective\b" src` — the owner `SyntaxTriviaUtilities.IsConditionalDirective` (`SyntaxTriviaUtilities.cs:127`) and its callers only (`SyntaxTriviaUtilities.cs:456, 490`, `OrderingDeclarationUtilities.cs:583`, `ConditionalGroupUnitUtilities.cs:29`, RH2001 fix, RH7004 fix); no other statement of the set. `rg -U "ElifDirectiveTrivia[\s\S]{0,120}ElseDirectiveTrivia[\s\S]{0,120}EndIfDirectiveTrivia" src` additionally finds `DirectivePairSplitScan.cs:86-88`, a per-kind switch the skeptic excluded.
- `rg -n "FindFirstUnsupportedOperatorTrivia\([^)]*\.GetFirstToken\(\)" src` — one hit: `PrecheckPhase.cs:758`, inside `FindTriviaBehindToken`.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src` (with this commit): 7 files changed, 56 insertions(+), 91 deletions(-). No new type. One new public member: `SyntaxIndentationUtilities.IsOwningConstruct` (decided by lead decision 2).

### Deleted special cases
_N/A._

### Tests run
```shell
scripts/build.sh --configuration Debug                       # Build succeeded, 0 Warning(s), 0 Error(s)
scripts/format.sh <the 7 changed files>                       # Formatted 0 of 7 file(s)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~CommentPositionUtilitiesTests|FullyQualifiedName~RH7004UsingDeclarations|FullyQualifiedName~RH710"
#   Passed! - Failed: 0, Passed: 973
scripts/test.sh --configuration Debug --project core
#   Passed! - Failed: 0, Passed: 901
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Unit.Precheck"
#   Passed! - Failed: 0, Passed: 1865
```
Whole test projects of the changed components (Debug):
```shell
scripts/test.sh --configuration Debug --project analyzer       # Passed! - Failed: 0, Passed: 12079
scripts/test.sh --configuration Debug --project formatter      # Passed! - Failed: 0, Passed: 7493
scripts/test.sh --configuration Debug --project architecture   # Passed! - Failed: 0, Passed: 198
scripts/test.sh --configuration Debug --project cli            # Passed! - Failed: 0, Passed: 441, Skipped: 1
```

### Disputed tests or scope
_None._

### Architecture findings
_None_ beyond the issues themselves (each is the FC-015 finding the run fixes).

### Follow-up candidates
_None._
