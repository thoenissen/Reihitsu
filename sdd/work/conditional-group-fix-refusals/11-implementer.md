Role: implementer
Commit: see the commit that adds this file (subject "Implementer: repair round 1 for conditional-group fix refusals [skip ci]")
Verdict: tests pass

Repair round 1 (Decisions #12, #13, #15, #16, #17; audit 08 findings #1, #2, #4, #5, #6). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the findings and decisions. No test assertion, input, or expected output was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Finding #1: `WrapsWholeUnitsOf(checksNesting)` now checks only "ends with `#endif`, no group inside a branch"; the "lies inside no branch of another group" half moved into `WrapsWholeUnits` (spans and in front of them), the only reader whose spec still states it. RH2001 (`ConditionalGroupsWrapWholeMembers`) thus drops it; the first-token reading never had it. Both halves use a new private `GetConditionalDirectives(root, span)`, which scans only the span it needs instead of the whole tree's trivia | RH2001-010, SHARED-ORDER-008 |
| same | Finding #4: `DeclarationHoldsConditionalDirective` reads a declaration that ends in `;` up to and including that `;` (`OpenBraceToken` is `None` there) | SHARED-ORDER-008 |
| same | Finding #5: private `ChangesMemberOrder(…, getNewRank)` evaluates `ChangesOrder(IsLayoutField)` before the layout conditions; same result | — |
| same | Finding #6: `ChangesInitializerExecutionOrder` `<returns>` names the group in front of the spans; `ConditionalGroupsWrapWholeMembers` summary/param describe the new nesting reading and the behind-the-property token | FC-031 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Finding #2: `TryCreateFieldDeclaration` also withholds for a property without an initializer when `ConditionalGroupsWrapWholeMembers(node.GetLastToken().GetNextToken())` fails (next member's first token or the type body's closing brace); remarks updated | RH2001-010 |

Every guard is a cheap syntactic check on the existing shared reading: directive positions, `Initializer == null`, `OpenBraceToken` kind, and one parse per excluded branch. No new type, no new public member.

## Policy owners
- `rg -n "GetRelatedDirectives|ReadsAsWholeUnits\(|WholeUnits" src --glob '!*Test*/**'`: only `OrderingDeclarationUtilities.cs` decides "group wraps whole units"; RH2001's `HasConditionalDirectiveBetweenModifiers` (RH2001-009) decides a different question. Unchanged from 05.
- `rg -n "OpenBraceToken" src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: the header reading (`DeclarationHoldsConditionalDirective`) and the front-of-spans span start (`ChangesMemberOrder`), which only runs on the reordered declaration, which always has a body.

## Production diff size
- Whole run, working tree vs `95aa3e6` (production only): 2 files; code lines without XML doc and blank lines +90 / -35, about 55 net. Under 100.
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 2 files changed, 164 insertions(+), 61 deletions(-).
- This round: 2 files changed, 54 insertions(+), 39 deletions(-); code lines +32 / -28, about 4 net. No new type.

## Deleted special cases
_N/A._

## Tests run
```shell
scripts/build.sh --configuration Debug                     # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
# Passed! - Failed: 0, Passed: 719, Total: 719
scripts/test.sh --configuration Debug --project analyzer      # Passed! - Failed: 0, Passed: 7980, Total: 7980
scripts/test.sh --configuration Debug --project core          # Passed! - Failed: 0, Passed: 660, Total: 660
scripts/test.sh --configuration Debug --project architecture  # Passed! - Failed: 0, Passed: 117, Total: 117
```
`scripts/format.sh` over both changed files: 0 of 2 formatted.

## Disputed tests or scope
_None._ Notes for the lead:
- The front-of-modifier and behind-the-property readings both go through `ConditionalGroupsWrapWholeMembers`. For a next member that is a nested type, the token's `FirstAncestorOrSelf<TypeDeclarationSyntax>` is that nested type; the parse reads the branch as type-body members either way, so the result is the same.
- The auditor's hint (three adjacent RH2001 guards on directive trivia could fold into one check) remains a possible later cleanup, not done here.

## Architecture findings
_None._

## Follow-up candidates
_None._
