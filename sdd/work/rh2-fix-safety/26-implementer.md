Role: implementer
Commit: 6e0f297
Verdict: tests pass

Repair round 3. Inputs: `00-lead.md` (Q9 = A, decisions 25 and 30), `23-auditor.md` findings 7 and 8, `24-spec-author.md`, `25-test-implementer.md`, `20-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. The 10 handed-over reds failed as `25-test-implementer.md` states before the change.

## thoenissen/Reihitsu#967 — audit finding 7 (RH2005-003, Q9 = A)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` (`ReplaceAccessibilityModifiers`) | The branch for a removed non-first accessibility modifier that does not start its line is split: when the modifier ends its line, the token in front still ends with the modifier's trailing trivia (Q7, unchanged); when it neither starts nor ends its line, the modifier is dropped together with its trailing whitespace and the token in front keeps its own trailing gap as written. The `<remarks>` list now names the three cases. | RH2005-003 (Q9 = A), Q7, decision 25 |

The decision stays in the one method decision 25 made its owner; no second placement path was added. RH2004 uses the same method but only ever replaces a first modifier or inserts one, so its outputs do not change (RH2004 tests green).

## Audit finding 8 (decision 30)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DeclarationModifierUtilities.cs` | Removed `AddAccessibilityModifier(SyntaxTokenList, SyntaxKind)` and `AddAccessibilityModifiers(SyntaxTokenList, IReadOnlyList<SyntaxKind>)`, the `followingToken.RawKind == 0` branch (trivia appended to the list's last token), and the `tokens.Count == 0` operand. The member overload now always passes a real following token (the token behind the last modifier, or the first token behind the attribute lists when there are no modifiers, where no trivia can be carried). The `file`-keyword rationale moved from the removed overload's remarks to `IsAccessibilityModifier`'s summary, which now `cref`s the member overload; the public overload's remarks no longer `cref` the private method. | – (decision 30; no caller, no output) |

## Policy owners
- `rg -n "AddAccessibilityModifiers?\(|ReplaceAccessibilityModifiers" src` — production callers: `RH2005FieldsMustBePrivateCodeFixProvider.cs:45`, `RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs:62` (member overload only); tests: `DeclarationModifierUtilitiesTests.cs` (member overloads only). One owner of the trivia decision: `ReplaceAccessibilityModifiers`.
- `rg -n "TrimTrailingWhitespace|ContainsEndOfLine\(" src --glob '!*Test*'` — besides this file, a private `TrimTrailingWhitespace` in `StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs:168` (pre-existing, not touched by this round; neither copy was introduced here).

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005FieldsMustBePrivateAnalyzerTests.VerifyFixRemovesAOneLinePublic"` — before: Failed 10, Passed 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2004"` — Passed 149, Failed 0.
- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.DeclarationModifierUtilitiesTests"` — Passed 23, Failed 0.
- Whole projects: analyzer — Passed 7288, Failed 0; core — Passed 660, Failed 0; architecture — Passed 115, Failed 0.
- `scripts/format.sh src/Reihitsu.Core/DeclarationModifierUtilities.cs` — formatted 0 of 1 file.

## Disputed tests or scope
_None._ One production file changed, net 45 lines removed; no new public member.

## Architecture findings
_None._

## Follow-up candidates
_None._
