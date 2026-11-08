Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions (34, 38–40, 42, 43, 45). No test assertion, input, or expected output was changed. The tests of `44-test-implementer.md` were watched red before the change: 20 failing rows of 555 in the five rule classes, all "No code fix may be offered", as the handover says. They are green after it.

Each item is a refusal or a rule stated in one place (`AGENTS.md`, *Code-fix coverage*, *Special cases do not grow the source*). No new type, no new public member.

## thoenissen/Reihitsu#1595

No production change of its own. The type-declaration refusal (decision 39) is in the shared guard and reaches RH7102 through the same `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` → `ChangesMemberOrder` path (see #1593).

## thoenissen/Reihitsu#1593

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | Decision 38 / #14: `HasBackingField` counts a property when *any* accessor has neither a body nor an expression body (was: all), through the new private `IsWithoutBody`. `HasConditionalAccessorWithoutBody` deleted, with its hook in `HasBackingField` and its own group-span computation and accessor parse | SHARED-ORDER-007 |
| same | Decision 38/43 (2A): `WrapsWholeUnits` keeps the parsed units of each excluded branch and, for an accessor list, refuses when an accessor of the group (active accessors between the group's directives, plus the accessors of the parsed excluded branches) has no body while an accessor of the list, in the group or around it, has one. The `field` check reads the same parsed branches (no second parse) | SHARED-ORDER-008 *No backing field from a branch*, *No `field`* |
| same | Decision 39 (and 43, 1A): new private `DeclarationHoldsConditionalDirective`, called with the `WrapsWholeUnits` gate in the rank overload of `ChangesMemberOrder`: refuses every move of the type when a conditional directive stands in the leading trivia of a declaration token behind the first one, up to and including `{`, or in the first token's leading trivia with an excluded branch that does not read as whole members (`ReadsAsWholeUnits` with the type-body list — the existing parse, no new reader) | SHARED-ORDER-008 (type-declaration paragraph) |
| same | #16: summaries of both public `ChangesMemberOrder` overloads and of the private rank overload name the SHARED-ORDER-008 refusals; `HasBackingField`'s and `WrapsWholeUnits`' summaries describe the new rules | FC-031 |

`RegionBlockLayout` (RH7309) and `TypeMemberOrderingCodeFixProviderBase` are unchanged: both reach the new checks through the rank overload.

### Policy owners

- "An accessor without a body gives a backing field": `rg -n "Body == null && .*ExpressionBody == null" src -g '!*Test*'` — the only backing-field statement is now `HasBackingField` via `IsWithoutBody` (`OrderingDeclarationUtilities.cs`). The other hits (`AttributeTargetUtilities.cs:49`, `RH5408…CodeFixProvider.cs:84`, `RH5047…Analyzer.cs:50`) decide "is a simple auto-property" for other rules, not a backing field. `rg -n HasConditionalAccessorWithoutBody src sdd/specs`: no hit.
- Parse of excluded text: still only `ParseInTypeBody`, used by `GetMembersOfDisabledText` and `ReadsAsWholeUnits`; the type-declaration check reuses `ReadsAsWholeUnits`.

## thoenissen/Reihitsu#1616

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | Decision 40: new private `HasConditionalGroupOfOtherModifiers`, checked next to `IsSupportedPropertyShape` in `TryCreateFieldDeclaration`. For every conditional group with a directive in the leading trivia of a modifier behind the first or of the type, every token of every branch — active tokens between the group's first and last directive, and the tokens of each excluded branch through `SyntaxFactory.ParseTokens` — must be `public`, `private`, `protected`, `internal`, `new`, or `static`, and no branch may hold a comment; otherwise the fix is withheld. One keyword set decides every group | RH2001-009 |

`ModifierOrderingUtilities.TryWriteModifiersInOrder` is unchanged; decision 36's placement of `readonly` behind the group applies to the groups that pass.

### Policy owners

- Conditional-directive kind test: `rg -n "ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax" src -g '!*Test*'` — `OrderingDeclarationUtilities.IsConditionalDirective` (private), `SyntaxTriviaUtilities.IsConditionalDirective` (private, on trivia), `RH7004…CodeFixProvider.cs:130` (inline), and now the inline pattern in the RH2001 fix. See *Disputed tests or scope*.

## Production diff size

`git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'`: 9 files changed, 643 insertions(+), 69 deletions(−). Before this round (`origin/main...HEAD` at 0f8f731c): 578 insertions(+), 62 deletions(−). No new type.

Per file against `origin/main` (added/removed):

| File | Before the round | After the round |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | +357 / −7 | +382 / −13 |
| `src/Reihitsu.Core/ModifierOrderingUtilities.cs` | +173 / −1 | +173 / −1 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | +16 / −5 | +56 / −6 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ModifierOrderingCodeFixProviderBase.cs` | +11 / −25 | +11 / −25 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | +13 / −6 | +13 / −6 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103…CodeFixProvider.cs` | +0 / −8 | +0 / −8 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7109…CodeFixProvider.cs` | +0 / −8 | +0 / −8 |

Round delta: `OrderingDeclarationUtilities.cs` +72 / −53 (code lines without blanks and comments: +36 / −33, net +3 — decision 38 removed about 29 code lines, decision 39 added about 15, the accessor refusal about 14 and `IsWithoutBody` 3); RH2001 fix +40 / −1 (code +30 / −1). **The round grew the production diff**: net +58 lines, net +32 code lines, almost all of it decision 40's RH2001 check; the Core guard stayed level in code lines while gaining two refusals.

## Tests run

- Before the change (stashed): `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109ReadonlyElements|FullyQualifiedName~RH7103StaticElements|FullyQualifiedName~RH7102Constants|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH2001PrivateAuto"`: Failed 20, Passed 535, Total 555.
- After: same filter: Passed 555, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7684, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 660, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115, Failed 0.
- `scripts/build.sh` (Release): 0 warnings, 0 errors. `scripts/format.sh` on both changed files: 0 changed.

## Disputed tests or scope

- The RH2001 check writes the conditional-directive kind pattern inline, a fourth copy of a test the auditor already counted three of (`42-auditor.md`, *Policy ownership*). Reusing one would need a public member in Core; I left the inline pattern (one line) and leave the choice to the lead.
- `HasConditionalGroupOfOtherModifiers` reads a group's whole span when its `#if` stands earlier (for example in front of the first modifier, with the `#endif` between modifiers): every token in that span must be an allowed modifier, so such a group refuses unless it holds only allowed modifiers. That is the safe side; the spec leaves a group in front of the first modifier outside RH2001-009 (decision 44).

## Architecture findings

_None._

## Follow-up candidates

_None new._
