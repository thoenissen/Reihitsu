Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions (46–50) and audit 6 (#17–#22). No test assertion, input, or expected output was changed. The tests of `50-test-implementer.md` were watched red before the change: 18 failing rows of 575 in the five rule classes (RH7102, RH7103, RH7109, RH7309, RH2001), as that handover says. All 575 are green after it.

Every item is a refusal or a simplification (`AGENTS.md`, *Special cases do not grow the source*). No new type, no new member, no new public member; one private method is replaced by a shorter one. The round shrinks the production diff (net −24 lines against the previous head).

## thoenissen/Reihitsu#1595

No production change of its own. Decisions 46 and 47 reach RH7102 through the shared `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `OrderingDeclarationUtilities.ChangesMemberOrder` path (see #1593).

## thoenissen/Reihitsu#1593

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Decision 46 (#17): `HasBackingField` drops the partial exemption, which `main` also had in its "all bodiless" form. A property that is not abstract or extern counts a backing field when any accessor has no body or any token is `field`; the method is now one expression. Over-counting only withholds layout reorders | SHARED-ORDER-007 |
| same | Decision 47 (#18): `DeclarationHoldsConditionalDirective` no longer sits next to the `WrapsWholeUnits` gate. It feeds the layout flag instead: `(fieldOrderSetsLayout \|\| DeclarationHoldsConditionalDirective(typeDeclaration)) && ChangesOrder(IsLayoutField)`. A conditional header now refuses only moves that reorder instance fields. The check itself is unchanged | SHARED-ORDER-008 (type-declaration paragraph) |
| same | #22 and FC-031: the summary of `IsLayoutField` now points to `HasBackingField` instead of "auto-property or `field`". `HasBackingField`'s summary states the shape-only count, and that the count can over-count. `DeclarationHoldsConditionalDirective`'s summary, the rank overload's summary, and both public `ChangesMemberOrder` summaries say that a conditional header makes the type count as layout-sensitive | FC-031 |

Decision 46 (#20) needed no production change: the clause *No backing field from a branch* in `WrapsWholeUnits` already refuses the RH7103 class input, and the new test is green on both sides.

### Policy owners

- "An accessor without a body gives a backing field": `rg -n "IsWithoutBody|Body == null && .*ExpressionBody == null" src -g '!*Test*'`. It has one owner, `OrderingDeclarationUtilities.IsWithoutBody`, used by `HasBackingField` and by `WrapsWholeUnits`. The other hits (`AttributeTargetUtilities.cs`, `RH5408…CodeFixProvider.cs`, `RH5047…Analyzer.cs`) decide "is a simple auto-property" for other rules. `rg -n "PartialKeyword" src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: no hit is left.
- Layout flag: `FieldOrderSetsLayout` (symbol) and `DeclarationHoldsConditionalDirective` (syntax) are combined only in the rank overload of `ChangesMemberOrder`. `ChangesInitializerExecutionOrder` keeps passing `false` and asks no layout question, so a conditional header no longer refuses there. That method's summary already says so.

## thoenissen/Reihitsu#1616

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Decision 48 (#19, #21 moot): `HasConditionalGroupOfOtherModifiers` (group spans, `SyntaxFactory.ParseTokens` over disabled text, six-keyword set, comment scan) is replaced by `HasConditionalDirectiveBetweenModifiers`. It returns true when a property with at least one modifier has an `#if`/`#elif`/`#else`/`#endif` trivia in the leading trivia of a modifier other than the first, or of the type, whatever the branches hold. The `Microsoft.CodeAnalysis.Text` using is removed. The `TryCreateFieldDeclaration` remarks name the refusal | RH2001-009 |

`ModifierOrderingUtilities.TryWriteModifiersInOrder` is unchanged. Its placement of an added modifier behind a comment or non-conditional directive still serves RH2001 (`#pragma`, comments) and the RH7105 family, so it is not dead code.

### Policy owners

- Reading of excluded text: `rg -n "ParseTokens|ParseInTypeBody" src -g '!*Test*'`. Only `OrderingDeclarationUtilities.ParseInTypeBody` remains, so the second reader is gone.
- Conditional-directive kind: `OrderingDeclarationUtilities.IsConditionalDirective` (private, on the directive syntax), `SyntaxTriviaUtilities.IsConditionalDirective` (private, on trivia), the inline pattern in `RH7004…CodeFixProvider.cs`, and the inline `SyntaxKind` pattern in the RH2001 fix. That is still four owners, as audit 6 counted; the RH2001 one is now a single line on trivia kinds. Reusing one of the others would need a public Core member, so I left the inline form.

## Production diff size

`git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'`:
- Before this round (6332e7e3): 9 files changed, 643 insertions(+), 69 deletions(−), net +574.
- After: 9 files changed, 629 insertions(+), 79 deletions(−), net +550.

No new type.

| File (against `origin/main`) | Before the round | After the round |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | +382 / −13 | +388 / −23 (net +369 → +365) |
| `src/Reihitsu.Core/ModifierOrderingUtilities.cs` | +173 / −1 | +173 / −1 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | +56 / −6 | +36 / −6 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ModifierOrderingCodeFixProviderBase.cs` | +11 / −25 | +11 / −25 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | +13 / −6 | +13 / −6 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103…CodeFixProvider.cs` | +0 / −8 | +0 / −8 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7109…CodeFixProvider.cs` | +0 / −8 | +0 / −8 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7102…Analyzer.cs` | +4 / −1 | +4 / −1 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7103…Analyzer.cs` | +4 / −1 | +4 / −1 |

Round delta (`git diff --numstat` against the previous head):
- `OrderingDeclarationUtilities.cs`: +22 / −26. The code shrinks: `HasBackingField` loses the partial branch, and the gate loses one condition. The rest is summary text.
- RH2001 fix: +14 / −34.

## Tests run

- Before the change: `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"`: Failed 18, Passed 557, Total 575.
- After, same filter: Passed 575, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7704, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 660, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115, Failed 0.
- `scripts/build.sh` (Release): 0 warnings, 0 errors.
- `scripts/format.sh` on both changed files: 0 of 2 changed.

## Disputed tests or scope

_None._ The fourth inline copy of the conditional-directive kind test is the one `45-implementer.md` already reported, in a shorter form.

## Architecture findings

_None._ Decision 46 ends the repeated backing-field class (#11, #14, #17). It does so by counting on shape alone, with no partial logic left.

## Follow-up candidates

_None._
