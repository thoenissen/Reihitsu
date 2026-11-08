Role: implementer
Commit: e190498
Verdict: #1595: tests pass (no change); #1593: tests pass; #1616: tests pass (no change)

Repair round 1, worklist items 1–3 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test assertion, input, or expected output was changed. The 11 tests of `10-test-implementer.md` were red at 28d76dd as that handover states (taken from it; the first focused run after the change was green).

## thoenissen/Reihitsu#1595

No change. Finding #1 reaches RH7102 through the shared guard (row d1); it is repaired under #1593.

## thoenissen/Reihitsu#1593

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | **Finding #1.** New `IsStaticInExcludedCodeInFront`: excluded code in the leading trivia of a field's, event field's, or property's first token declares the declaration `static` when a top-level `static` lies in the part of an excluded branch behind its last complete member (behind its last top-level `;` or `}`). Walking the excluded texts backwards, a text whose branch completes a member ends the walk at the start of its outermost conditional group, so a `static` in an earlier group continues that member instead; other branches of the same group still count. `#else`/`#elif` with no open `#if` in the trivia starts a group, so a branch opened inside the previous member is read too. `GetOrderedMembersOfDeclaration` ORs it into "static in some configuration"; the members that the excluded code declares are still counted once, by `GetOrderedMembersOfDisabledText`. Covers a1, a2, a4, c1, d1, e1, b1, open groups (`#endif` inside the span) and complete groups in front alike | SHARED-ORDER-007 (split-member bullet 2) |
| same | **Finding #2.** `=` and `static` in excluded text count only outside every parenthesis, bracket, and brace. Inside a declaration (`GetOrderedMembersOfDeclaration`), the depth starts at the active declaration's open brackets in front of the excluded text (`GetNestingDepth`), so an excluded accessor inside the accessor list (g2) or an excluded named argument inside an active attribute starts nested; the declaration's own `;` is also found only at depth 0. In `GetOrderedMembersOfText`, each incomplete member and each skipped-token run is checked separately from depth 0 (`ContainsTopLevelToken`), so `[Obsolete("x", DiagnosticId = "y")]` (o1, o2) and `[DllImport("a", EntryPoint = "b")]` (g1) give no initializer, while `= Log("b");` behind an accessor list (1593/e, 1593b/c) still does. Depth helpers: `GetNestingDepths`, `GetNestingDelta` | SHARED-ORDER-007 (no withholding beyond it); decision 6 |
| same | **Performance hint.** `GetOrderedMembersOfDeclaration` returns the plain member when `memberDeclaration.ContainsDirectives` is false; `GetConditionalTriviaOfDeclaration` descends only into nodes and tokens that contain directives, so the ancestor walk runs only for tokens that carry one | — |

Summaries of `GetOrderedMembersOfDeclaration`, `GetConditionalTriviaOfDeclaration`, and `GetOrderedMembersOfText` updated to the new code (FC-031).

### Policy owners

- `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: one owner, `OrderingDeclarationUtilities.ChangesMemberOrder`; callers `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` and `RegionBlockLayout.ChangesMemberOrder`. Unchanged.
- `rg -n "OpenParenToken or SyntaxKind.OpenBracketToken|NestingDepth|GetNestingDelta" src -g '!*Test*'`: only the new private helpers in `OrderingDeclarationUtilities` (the `NestingDepth` hit in `RegionDirectiveUtilities` is region nesting, a different decision).
- `rg -n "IsKind\(SyntaxKind.EqualsToken\)" src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes/Core`: only the depth-guarded scan in `GetOrderedMembersOfDeclaration`.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7109|FullyQualifiedName~RH7103|FullyQualifiedName~RH7102|FullyQualifiedName~RH7309"`: Passed 276, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7502, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 660, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115, Failed 0.
- `scripts/format.sh src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: 0 of 1 files changed. Debug build: 0 warnings.

### Disputed tests or scope

_None._ All changes are private members of `OrderingDeclarationUtilities`.

Remaining over-approximations, all in the withholding direction and none covered by a test: a `static` in excluded code in front of a declaration, followed by a later group whose other branch completes a member, is still attributed to the declaration (only a branch that completes a member ends the walk); a nested depth can be misread when an excluded branch inside the declaration closes a bracket the active code opened in another branch.

### Architecture findings

_None._ Both repairs stay inside the one owner of the decision.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1616

No change; the audit has no finding for it.
