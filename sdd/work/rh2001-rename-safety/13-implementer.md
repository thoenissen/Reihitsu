Role: implementer
Commit: ab3369d (base of this round; the round's own commit carries this file)
Verdict: tests pass

Repair round 1 for `10-auditor.md` findings #1–#3 (`00-lead.md` decisions 15–17). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the recorded decisions; I worked from the spec (RH2001-011 as of b223706) and the tests of 4f66948.

Start state: `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests` — 231 tests, 12 failed (7 inferred-name, 4 receiver, 1 cref), as `12-test-implementer.md` states.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `RenameChangesBindingAsync`: resolves the property symbol before the rename as well and, per referring document, withholds when `GivesInferredMemberNameAsync` holds; passes the original symbol to the original-side scan. | RH2001-011 ¶3 (finding #1) |
| same | New `GivesInferredMemberNameAsync`: an `AnonymousObjectMemberDeclaratorSyntax` without `NameEquals` or a tuple-expression `ArgumentSyntax` without `NameColon` whose expression's inferred name (`SyntaxFacts.TryGetInferredMemberName`, Roslyn's own rule) equals the property name and whose last identifier binds to the property. Cheap syntactic check plus one `GetSymbolInfo` per candidate. | RH2001-011 ¶3 (finding #1) |
| same | `GetOtherSymbolReferencesAsync`: scans with `DescendantNodes(descendIntoTrivia: true)`, so `cref` names are compared like other references; the `property` parameter is now always the property on that side (before or after the rename). | RH2001-011 ¶1 (finding #3) |
| same | New `DescribeQualifier`: the qualifier of a member access or qualified name is described by its tokens, with every identifier bound to the property replaced by one marker, instead of `ToString()`. The rename of the property therefore no longer changes the description of `Value._value`, `this.Value._value`, `Value!._value`, `((Outer)Value)._value`. This is the auditor's alternative ("describe the receiver by what it binds to, with the property mapped to one marker"), not a refused class, so RH2001-011 ¶4 (offered) stands as the spec author wrote it. | RH2001-011 ¶4 (finding #2) |
| same | Summaries and remarks of `RenameChangesBindingAsync` and `GetOtherSymbolReferencesAsync` updated to the new code (FC-031). | — |

## Policy owners

- `rg -n "TryGetInferredMemberName|AnonymousObjectMemberDeclaratorSyntax \{ NameEquals" src --glob '!*Test*'` — only the new code in the RH2001 provider. No other private copy of the inferred-name check.
- The rename-safety owners are unchanged from `10-auditor.md`, *Policy ownership* (`CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and the RH2001 provider; the FC-015 unification follow-up stays recorded in `00-lead.md`). Finding #1 is now an input where both owners withhold.

## Production diff size

- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 1 file changed, 141 insertions(+), 7 deletions(-).
- After this round (`origin/main` to the working tree): 1 file changed, 211 insertions(+), 7 deletions(-).
- Round delta: 1 file changed, 83 insertions(+), 13 deletions(-) — mostly XML docs. Two new private methods, no new type, no new public member, no new dependency.

## Deleted special cases

_N/A._

## Tests run

```shell
scripts/format.sh src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs
# Formatted 1 of 1 file(s).
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
# Passed!  - Failed: 0, Passed: 231, Skipped: 0, Total: 231
scripts/test.sh --configuration Debug --project analyzer
# Passed!  - Failed: 0, Passed: 8432, Skipped: 0, Total: 8432
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118
```

## Disputed tests or scope

_None._ Finding #2 was narrowed cheaply (one token walk over the qualifier), so it is not disputed.

## Architecture findings

_None_ beyond the FC-015 follow-up already recorded.

## Follow-up candidates

_None._
