Role: implementer
Commit: 750237e
Verdict: #1595: tests pass; #1593: tests pass; #1616: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decisions 4, 7, and 16 of `00-lead.md`, which are the lead's recorded decisions, not a hoped-for verdict. No test assertion, input, or expected output was changed. Every handed-over test was red at 06ce8ef as `05-test-implementer.md` states (taken from its handover; the first focused run after the change was already green).

## thoenissen/Reihitsu#1595

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | `IsMoveSafe` is now `private static` and applies both guards for every derived fix: `MoveRangeContainsDirectives` and `OrderingDeclarationUtilities.ChangesMemberOrder` (decision 4). The opt-in `protected virtual` extension point is gone, so no derived fix can skip the member-order guard | RH7102-006, SHARED-ORDER-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7103…CodeFixProvider.cs`, `RH7109…CodeFixProvider.cs` | Their `IsMoveSafe` overrides (the two copies of the guard call) removed | SHARED-ORDER-007 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7102ConstantsMustAppearBeforeFieldsAnalyzer.cs` | A constant whose modifiers a conditional directive splits is not reported (decision 16, rows s1/s2) | RH7102-001, SHARED-DIAG-008 |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | New `HasModifiersSplitByConditionalDirective`: a conditional directive or disabled text from the first modifier up to the token behind the last one (reuses `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`) | SHARED-DIAG-008 |

The split crossed `J` (`NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations`) is decided by the #1593 change below.

### Policy owners

- `rg -n "ChangesMemberOrder\(" src -g '!*Test*'`: one owner, `OrderingDeclarationUtilities.ChangesMemberOrder` (Core); callers `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` (RH7102, RH7103, RH7109) and `RegionBlockLayout.ChangesMemberOrder` (RH7309). The RH7103/RH7109 copies of the call are gone.
- `rg -n "HasModifiersSplitByConditionalDirective|ContainsConditionalCompilationBoundary\(" src -g '!*Test*'`: `ContainsConditionalCompilationBoundary` (Core) is the one owner of "conditional directive or disabled text in a span"; the new helper only chooses the modifier span and is used by the RH7102 and RH7103 analyzers. `rg -ln "IfDirectiveTrivia|DisabledTextTrivia|ConditionalCompilation" src/Reihitsu.Analyzer/Rules` finds no other modifier-split check.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests"`: Passed 55, Failed 0.

## thoenissen/Reihitsu#1593

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `ChangesMemberOrder` ranks `OrderedMember` values instead of syntax nodes. `GetOrderedMembersOfDeclaration`: a field, event field, or property whose declaration holds a conditional directive or disabled text (outside accessor bodies, expression bodies, and lambda blocks; `GetConditionalTriviaOfDeclaration`) counts with what any configuration gives it — static when a `static` is written or excluded text in front of its first `;` holds one; instance when no `static` is written or every written `static` lies in a conditional part of the span (`GetConditionalSpans`, which also covers a branch whose `#if` or `#endif` lies outside the span); non-constant likewise for `const`; an initializer when one is written or excluded text in front of its first `;` holds `=`; a layout field when it is instance and non-constant in some configuration. Excluded text inside the declaration behind a `;` is parsed as further members at its position. `GetOrderedMembersOfText` (shared with the disabled text outside every member): text that parses as no complete member (an `IncompleteMemberSyntax` or skipped tokens) and holds `=` counts as a runtime initializer of both contexts at its position (1593/e, 1593b/c). `HasInitializer`/`IsLayoutField` split into `HasInitializerSyntax`/`DeclaresLayoutField` so the split path reuses them. Summaries of both public `ChangesMemberOrder` overloads updated | SHARED-ORDER-007 (split-member paragraph), RH7309-006, RH7109-006, SHARED-FIX-003 |
| `src/Reihitsu.Core/OrderedMember.cs` (new, `internal`) | Position plus static initializer, instance initializer, and layout-field flags of one member in any configuration | SHARED-ORDER-007 |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.cs` | A static member whose modifiers a conditional directive splits is not reported (decision 16, row 1593c/a) | SHARED-DIAG-008 |

The guard over-approximates where the configurations cannot be told apart by tokens (a conditional `static` counts in both contexts; excluded text with `=` counts as an initializer). This only withholds more. The counterparts that keep a fix (RH7309 1593/a; RH7102 h, f, k, p, c2, e) and every existing test still pass.

### Policy owners

- `rg -n "GetMembersOfDisabledText|DisabledTextTrivia" src/Reihitsu.Core/OrderingDeclarationUtilities.cs src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs`: excluded code is read only in `OrderingDeclarationUtilities` (`GetOrderedMembersOfDisabledText` outside members, `GetOrderedMembersOfDeclaration` inside them); `RegionBlockLayout` only supplies the rank function.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7309RegionsShouldFollowCategoryOrderAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests|FullyQualifiedName~RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests"`: Passed 193, Failed 0.

## thoenissen/Reihitsu#1616

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/ModifierOrderingUtilities.cs` | New `TryWriteModifiersInOrder(modifiers, nextToken, orderedModifiers, out rewrittenModifiers, out nextTokenLeadingTrivia)`, the one owner of writing reordered modifiers (decision 7): the leading trivia of the written first modifier goes in front of the new first one, each modifier gets a single space, so the modifiers stand on one line. A gap between two written modifiers, or between the last one and the next token, that holds a comment, directive, or disabled text keeps its place between the modifiers written in front of it and those behind it; a modifier the new order adds at that place goes behind it. It returns `false` when the new order moves a written modifier across such a gap. Written modifiers are matched to the ordered ones by kind, in written order, so callers may pass a list built with `SyntaxTokenList.Add` | SHARED-MODORDER-002, RH2001-004, RH2001-009 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ModifierOrderingCodeFixProviderBase.cs` | Private `NormalizeModifierTrivia` (the first copy of SHARED-MODORDER-002) removed; RH7105/RH7106 now call `TryWriteModifiersInOrder`. Its own guard (`ReorderingWouldDropCommentOrDirective`, SHARED-MODORDER-003) is unchanged and leaves no commented gap, so the output equals the old one | SHARED-MODORDER-002, SHARED-MODORDER-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` | `TryCreateFieldDeclaration` writes the modifiers through `TryWriteModifiersInOrder` when it adds `readonly`, puts the returned leading trivia on the type, and withholds when the helper refuses. The extra `.WithLeadingTrivia(node.GetLeadingTrivia())` (which wrote the leading trivia a second time when the first modifier moved) is removed: without attribute lists, which the fix does not convert, the node's leading trivia is the first modifier's or the type's and stays there. `KeepsEveryCommentAndDirective` stays as the net for the rest of the declaration. Remarks of the method updated | RH2001-004, RH2001-009 |

### Policy owners

- `rg -n "OrderModifiersForRh7105|OrderModifiersForRh7106|TryWriteModifiersInOrder|NormalizeModifierTrivia" src -g '!*Test*'`: the order is owned by `OrderModifiersForRh7105`/`OrderModifiersForRh7106`; writing the reordered list is owned by `TryWriteModifiersInOrder`, called by `ModifierOrderingCodeFixProviderBase` (RH7105, RH7106) and the RH2001 fix. `NormalizeModifierTrivia` no longer exists.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"`: Passed 97, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7105|FullyQualifiedName~RH7106"`: Passed 58, Failed 0.

## Whole run

### Tests run

- `scripts/test.sh --configuration Debug` (every project but self-hosting), before the last two edits: analyzer 7474, formatter 4816, core 660, cli 251, architecture 115, tooling 126, playground 17 passed, 0 failed.
- After the last edits (a `null` return turned into a `Try` method for S1168, a blank line, doc wording): `scripts/test.sh --configuration Debug --project analyzer`: Passed 7474, Failed 0; `--project core`: Passed 660; `--project architecture`: Passed 115.
- `scripts/build.sh` (Release): 0 warnings, 0 errors.
- `scripts/format.sh` on the ten changed source files: 0 of 10 changed.

### Disputed tests or scope

- **New public members in Core:** `ModifierOrderingUtilities.TryWriteModifiersInOrder` (needed by both the CodeFixes base and RH2001, decision 7) and `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective` (needed by two analyzers, decision 16). `OrderedMember` is `internal`.
- **Removed extension point:** `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` changed from `protected virtual` to `private static`, so the guard cannot be overridden away (decision 4). No derived class overrides it any more.
- The production paths stay within those the handovers name (the member-moving base and its three fixes, `OrderingDeclarationUtilities`, the two analyzers of decision 16, the RH2001 fix, `ModifierOrderingCodeFixProviderBase`/`ModifierOrderingUtilities`).

### Architecture findings

_None_ new. Decisions 4 and 7 are carried out: one call of the member-order guard for every member-moving fix, and one writer of reordered modifiers for RH7105, RH7106, and RH2001.

### Follow-up candidates

- Not verified: other analyzers that judge modifiers (RH7105, RH7106, RH7109, and the earlier "seen non-constant" field that RH7102 compares against) may still report when a conditional directive splits the judged modifiers (SHARED-DIAG-008). Decision 16 covered only the reported member of RH7102 and RH7103; the RH2001 candidate already in `00-lead.md` is of the same kind.
