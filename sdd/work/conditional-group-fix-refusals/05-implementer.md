Role: implementer
Commit: see the commit that adds this file (subject "Implementer: conditional-group fix refusals [skip ci]")
Verdict: #1666: tests pass; #1667: tests pass; #1665: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded Decisions #3, #6, #9, #11, which are run decisions and were followed. No test assertion, input, or expected output was changed.

## thoenissen/Reihitsu#1666

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `TryCreateFieldDeclaration` withholds the conversion when the first modifier's leading trivia holds a directive of a group that `OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers` does not accept; summary updated | RH2001-010 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | New public `ConditionalGroupsWrapWholeMembers(SyntaxToken)`: every group with a directive in the token's leading trivia passes the shared test `WrapsWholeUnitsOf` with the nesting check, read as members of the containing type body | RH2001-010 |

### Policy owners
See *Whole run*.

### Disputed tests or scope
_None._

### Architecture findings
_None._ (Decision #6 is implemented; see *Whole run*.)

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1667

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Both public `ChangesMemberOrder` overloads count the type as a layout type when `AnyDeclarationHoldsConditionalDirective(typeSymbol)`: every `DeclaringSyntaxReferences` declaration (same or other document) is read by `DeclarationHoldsConditionalDirective` | SHARED-ORDER-008, SHARED-ORDER-007 |
| same | `DeclarationHoldsConditionalDirective`: a first-token directive now reads every excluded branch of its group (Decision #3) through `WrapsWholeUnitsOf` without the nesting check (Decision #11), and also counts when a branch declares, at its outermost level, a `partial` declaration of the type's name and arity with an attribute list (`DeclaresTypeWithAttributes`, A5, Decision #9) | SHARED-ORDER-008 |

### Policy owners
See *Whole run*.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond A6, already recorded by the lead (Decision #8).

## thoenissen/Reihitsu#1665

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Private `ChangesMemberOrder(…, getNewRank)`: the group query became a local `GetGroups(span)`; the layout check also applies when a group with a directive between the body's opening brace and `reorderedSpan.Start` fails `WrapsWholeUnits`. Groups behind the spans are not read. Instance-field reorders only, so RH7103/RH7102 keep their fixes | SHARED-ORDER-008, SHARED-ORDER-007 |

### Policy owners
See *Whole run*.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## Whole run

### Decision #6 — one helper

`WrapsWholeUnitsOf(group, list, checksNesting, out branches)` (private, `OrderingDeclarationUtilities`) is the one test of "a conditional group wraps whole units of a list": the nesting check (moved unchanged out of `WrapsWholeUnits`, applied only when `checksNesting`) plus one `ReadsAsWholeUnits` parse of every excluded branch (`GetExcludedText` over every directive of the group). Its three readers:
- `WrapsWholeUnits` (spans and in front of them; `checksNesting: true`, then its Place, `field`, and accessor checks, unchanged);
- `DeclarationHoldsConditionalDirective` (first-token reading; `checksNesting: false`, per Decision #11 — SHARED-ORDER-008's first-token item states no nesting clause; uses `branches` for the A5 check);
- `ConditionalGroupsWrapWholeMembers` (public, RH2001-010; `checksNesting: true`).

No new type. One new public member (`ConditionalGroupsWrapWholeMembers`), which the RH2001 fix needs to reach the Core reading; it is what Decision #6 asked for.

### Policy owners

- `rg -n "GetRelatedDirectives|ReadsAsWholeUnits\(|IfDirectiveTrivia or SyntaxKind.ElifDirectiveTrivia" src --glob '!*Test*/**'`: outside `OrderingDeclarationUtilities.cs`, only RH2001's own `HasConditionalDirectiveBetweenModifiers` (RH2001-009, a different decision: any directive between modifiers), RH7306/RH7302 (region pairing) and RH7004 (using declarations) — none decides whole members.
- `rg -n -i "whole member|wholemember|WholeUnits" src --glob '!*Test*/**'`: only `OrderingDeclarationUtilities.cs`.

### Production diff size

`git diff --shortstat 95aa3e6..HEAD -- src ':(exclude,glob)src/*Test*/**'` (production only, this commit): 2 files changed, 164 insertions(+), 61 deletions(-). Without XML-doc and blank lines: +88 / -37 code lines, about 51 net. No new type. Under the 100-line threshold for code; the doc comments make up the rest.

### Deleted special cases

_N/A._ (No shape is declared unsupported.)

### Tests run

```shell
scripts/build.sh --configuration Debug                     # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests|FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests|FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests"
# Passed! - Failed: 0, Passed: 688, Skipped: 0, Total: 688
scripts/test.sh --configuration Debug --project analyzer      # Passed! - Failed: 0, Passed: 7949, Total: 7949
scripts/test.sh --configuration Debug --project core          # Passed! - Failed: 0, Passed: 660, Total: 660
scripts/test.sh --configuration Debug --project architecture  # Passed! - Failed: 0, Passed: 117, Total: 117
```

`scripts/format.sh` over both changed files: 0 of 2 formatted.

### Disputed tests or scope

_None._ Notes for the lead:
- Every guard is a cheap syntactic check on an already-specified parse: no new parse kind, no condition evaluated.
- `WrapsWholeUnits` now runs the nesting scan once per candidate unit list instead of once per group (the lists are usually one or two); behavior is unchanged, and the cost is the same order as on `main`.
- `AnyDeclarationHoldsConditionalDirective` is evaluated for every `ChangesMemberOrder` call, also when no instance field is reordered; it reads only each declaration's header tokens.

### Architecture findings

_None_ new.

### Follow-up candidates

_None._
