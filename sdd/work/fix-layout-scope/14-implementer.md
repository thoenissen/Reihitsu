Role: implementer
Commit: 5f2163f (changes on this head; the commit carrying them follows it)
Verdict: tests pass

Repair round 1: D25 (audit 11, finding #1) and the code side of D26 (finding #2). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

No fix is withheld for a new class. Every D25 member is routed through the existing helper `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`, as D8 routed the earlier members, and no column is computed inside a fix. The helper's own rare classes (D27) apply unchanged.

The RH5412–RH5416 routing sits in the shared base `EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.ApplyCodeFixAsync`. The five providers override only `DiagnosticId`, `CodeFixTitle`, `DeclarationKind`, and `MinimumLanguageVersion`, so RH5414–RH5416 take the same path as RH5412/RH5413. That answers the question in 13-test-implementer.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `ReplaceAnnotatedPropertyWithFieldAsync` formats the new field through the helper instead of `ReihitsuFormatter.FormatNodeInDocumentAsync`; summary updated | SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider.cs` | `ApplyCodeFixAsync`: a `MemberDeclarationSyntax` declaration goes through the helper. Any other owner of the constraint clause, which is a local function, is not a member and is formatted as before. Summary updated | SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.cs` | `ApplyCodeFixAsync`: the property target goes through the helper. Its context node is the property itself, so the helper's default step, `FormatNodeInDocumentAsync`, is the call it replaces. Assignments, declarators, parameters, and enum members are unchanged. Summary updated | SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs` | `ApplyCodeFixAsync` calls the helper (RH5412–RH5416); summary updated | SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider.cs` | D26, code side. The `CanApplyCodeFix` summary no longer says it mirrors the formatter's line-break guard: it states the comment guard on its own terms (SHARED-FIX-003). The `FormatsThroughFormatter` override keeps `<inheritdoc/>`, because RH8201 rejects a replacing summary, and gains a `<remarks>` saying the fix lays out nothing but is withheld on a syntax error by RH5113-004. The `FindUnsupportedSyntax` check carries a comment that names RH5113-004. Behavior is unchanged (D22) | RH5113-004 |

## Policy owners

`rg -n "FormatMemberOnLinesOfItsOwnAsync\(|SharesLineWithSiblingMember\(" src -g '!*Test*' -g '*.cs'` finds the helper, `BlankLineCodeFixUtilities`, and these callers:
- the three bases: `EmptyTypeDeclaration…Base` (new), `ExpressionBodyToBlock…Base`, and `TargetAttributePlacement…Base`;
- the rules RH2001 (new), RH3202, RH3203, RH5110 (new), RH5111 (new), RH5113, RH5401, RH5404, RH5408, and RH5417.

The in-front break has one remaining second owner: the attribute base's private before-break, which is known (D23, FU5).

## Production diff size

- Round delta: `git diff --shortstat -- src ':(exclude,glob)src/*Test*/**'` reports 5 files changed, 32 insertions(+), 16 deletions(-). No new type and no new public member.
- Branch, before this round's commit: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` reports 33 files changed, 281 insertions(+), 683 deletions(-).

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ALineOfItsOwn&(FullyQualifiedName~RH2001|FullyQualifiedName~RH5110|FullyQualifiedName~RH5111|FullyQualifiedName~RH5412|FullyQualifiedName~RH5413)"` reports Passed: 20, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5113"` reports Passed: 35, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` reports Passed: 9590, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` reports Passed: 118, Failed: 0.
- `scripts/build.sh --configuration Debug` reports 0 warnings and 0 errors.

## Disputed tests or scope

_None._ The RH5414–RH5416 routing is in the shared base, so it is covered through RH5412/RH5413, as D19 decided.

## Architecture findings

_None_ beyond D23/FU5.

## Follow-up candidates

_None_ new. The RH5111 enum-member case stays with FU1.
