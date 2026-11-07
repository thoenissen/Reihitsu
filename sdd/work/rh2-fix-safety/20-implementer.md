Role: implementer
Commit: 20cbbee (base); this handover's commit carries the change
Verdict: #967: tests pass; #1286: tests pass (nothing to change); #1287: nothing to do; #1288: nothing to do

Repair round 2: audit finding 5 (`17-auditor.md`), carried out along decision 25 (`00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. No test assertion, input, or expected output was changed.

## thoenissen/Reihitsu#967

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | New private `ReplaceAccessibilityModifiers(modifiers, kinds, ref followingToken)` is the one place that decides where both sides of every removed accessibility keyword's trivia go, for any position: a first keyword hands its leading trivia to the first inserted keyword and its trailing trivia to the last (unchanged from `main`); a later keyword that starts its line gives its leading trivia to the token that follows it (that token's own leading trivia and the keyword's trailing trivia are dropped); a later keyword that does not start its line ends the token in front of it with its trailing trivia (that token's trailing whitespace is dropped). Inserted keywords no longer take the trivia of a later accessibility keyword. Both `AddAccessibilityModifiers` overloads delegate to it; the member overload passes the token behind the modifier list and writes the updated token back. `CarryLeadingTriviaOfLaterAccessibilityModifiers` and `RemoveAccessibilityModifiers` are gone (subsumed). The guard is renamed `HasCommentOrDirectiveNextToLaterAccessibilityModifier` and now inspects exactly the gaps the method merges: a later keyword's leading and trailing trivia and the leading trivia of the token behind it, so the dropped trivia is whitespace and line breaks only when the guard is false. Summaries and remarks of every changed method rewritten (FC-031). | RH2005-003 (Q5, Q7), RH2005-005 (Q6, Q8), SHARED-FIX-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2005FieldsMustBePrivateCodeFixProvider.cs` | Calls the renamed guard; comment updated to "next to" | RH2005-005 |

Cases of finding 5: (a) `static public⏎        int` → `private static⏎        int`; (f) `static⏎        public⏎        int` → `private static⏎        int` (also with `int` at another column, which moves to the column of `public`); (b), (c), (d), (e) and the inline block comment withhold the fix. All covered by the tests of `19-test-implementer.md`, LF and CRLF, and the Fix All test.

The token-list overload (`AddAccessibilityModifier(s)(SyntaxTokenList, …)`) has no production caller (`rg` below); it now places a removed first keyword's trivia as before, and, since it cannot see the token behind the list, appends trivia that would go to that token to the end of the list. Its only test (`AddAccessibilityModifierReplacesExistingAccessibilityAndPreservesTrivia`) passes unchanged.

### Policy owners
- `rg -n "AddAccessibilityModifiers?\(" src --glob '!*Test*'`: callers are `RH2005FieldsMustBePrivateCodeFixProvider.cs:45` and `RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs:62` (member overload); the token-list overloads are called only from inside `DeclarationModifierUtilities` and from `DeclarationModifierUtilitiesTests`.
- Where the trivia of a removed accessibility keyword goes: `ReplaceAccessibilityModifiers` only (the member overload restores only the declaration's own first-token leading trivia, as before).
- Which modifiers are removed: `rg -n "IsAccessibilityModifier\(" src --glob '!*Test*'`: `DeclarationModifierUtilities` only (guard, `ReplaceAccessibilityModifiers`, `IsExplicitAccessibilityModifier`).
- Trivia kinds that count as comment or directive: the guard uses `SyntaxTriviaUtilities.IsCommentTrivia` / `IsDirectiveOrDisabledTextTrivia`; no private list.
- `rg -n "InFrontOfLaterAccessibility|CarryLeadingTrivia" src sdd/specs`: no remaining reference.

### Tests run
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001"
# before the change: Failed: 39, Passed: 113 (as 19-test-implementer.md states)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH200"
# Passed! - Failed: 0, Passed: 250 (RH2001-RH2005, RH2004 insertion tests included)
```

### Disputed tests or scope
_None._ Production paths: the two files the handovers name. The public guard was renamed (no caller outside the RH2005 fix, no test references it).

### Architecture findings
_None._ Decision 25 is carried out: one method decides both sides for every position; the guard inspects the same gaps.

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1286

Finding 6 (`VerifyFixKeepsACommentLineInFrontOfTheType`) was a test repair by the test implementer; it passes with the production code unchanged. Nothing to do.

## thoenissen/Reihitsu#1287

Not in this round's worklist. Nothing to do.

## thoenissen/Reihitsu#1288

Not in this round's worklist. Nothing to do.

## Whole run

```shell
scripts/format.sh src/Reihitsu.Core/DeclarationModifierUtilities.cs src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2005FieldsMustBePrivateCodeFixProvider.cs
scripts/test.sh --configuration Debug --project analyzer       # Passed! - Failed: 0, Passed: 7202
scripts/test.sh --configuration Debug --project core           # Passed! - Failed: 0, Passed: 649
scripts/test.sh --configuration Debug --project architecture   # Passed! - Failed: 0, Passed: 115
```
