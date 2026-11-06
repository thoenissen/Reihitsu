Role: implementer
Commit: 0684485
Verdict: #1136: tests pass; #1134: tests pass; #1135: tests pass; #1262: tests pass; #1332: tests pass; #1336: tests pass; #1340: tests pass

Repair round 1. Inputs: `00-lead.md` (decisions 6–9), `07-implementer.md`, `10-auditor.md`, `11-test-implementer.md`, `12-spec-author.md`, and the specs they name. Worklist: audit findings #1, #2, #3, #4 with decision 6, #5, #8, decision 8, and the `CanMoveBreak` summary. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond that worklist. The auditor's suggested changes were taken as suggestions and checked against the specs. On d428302, the focused filter of `11-test-implementer.md` gave exactly its 20 failures. After 0684485 all of them pass. No test assertion, input, or expected output was changed. `sdd/specs/shared/ordering-move-safety.md` was not touched.

## thoenissen/Reihitsu#1136

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` `SplitsDirectivePair` (finding #1 c–e) | Several pair types changed. `#pragma warning` now pairs per warning it names, with the next `#pragma warning` that names that warning or names none; a directive that names none stands for every warning. `#nullable` pairs the same way for `warnings`/`annotations` (no target means both). `#line` and `#pragma checksum` pair with the next directive of their kind. The end of the file is no longer a partner, so a directive without a partner splits nothing. New private `GetWarningCode` makes `219` and `CS0219` name the same warning. The conditional and region pairing is unchanged | SHARED-FIX-003 (a directive and the directive that ends its effect) |

### Policy owners

`rg -n "SplitsDirectivePair\(|ErrorCodes" src --glob '!*Test*'`: the owner is `SyntaxTriviaUtilities`, and its only consumer is the RH7004 fix. No other code reads `#pragma warning` codes.

## thoenissen/Reihitsu#1134

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` `FieldOrderSetsLayout` (finding #2) | Now takes an `INamedTypeSymbol` instead of a declaration. It reads `StructLayoutAttribute` from `GetAttributes()`, which merges every part of a partial type, resolves aliases, and binds a named-colon argument (`layoutKind: LayoutKind.Auto`). It reads the layout value from the constructor argument. A value it cannot read counts as sequential, as before. A struct (record struct included) has sequential layout unless the value is `Auto` or `Explicit`; a class (record included) has it only with the attribute. The syntax-only overload and the name-matching `IsStructLayoutAttribute(AttributeSyntax)` are replaced | SHARED-FIX-003 *Run time*, RH7109-006, RH7309-006 |
| `OrderingDeclarationUtilities.HasBackingField` (decision 8) | Only a `FieldKeyword` token counts. The parser produces that token only from C# 14 on; before, `field` is an identifier that names a member | SHARED-FIX-003, RH7309-006 |
| `RH7109…CodeFixProvider.IsMoveSafe` | Resolves the type symbol and asks the one owner `ChangesMemberOrder(type, FieldOrderSetsLayout(symbol), moved, target)` (see #1135). The fix is not offered when the symbol cannot be resolved | RH7109-006, SHARED-ORDER-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | `IsMoveSafe` gets the document's `SemanticModel` as its first parameter, fetched once in `RegisterCodeFixesAsync`. RH7102 does not override it, so RH7102 is unaffected | — (plumbing for finding #2) |

### Policy owners

`rg -l "StructLayout|LayoutKind" src --glob '!*Test*'`: only `OrderingDeclarationUtilities`. Its consumers are RH7109 (`IsMoveSafe`) and RH7309 (`TryCreateReorder`), and both pass the symbol.

## thoenissen/Reihitsu#1135

### Changes (decision 6, finding #4)

| File | What changed | Requirement |
|---|---|---|
| `OrderingDeclarationUtilities.ChangesMemberOrder(type, fieldOrderSetsLayout, Func<int,int> getNewRank)` (new, public) | The one owner of the member-order decision. It collects the type's members, plus the members parsed from body-level disabled text at the position of that text (`GetMembersOfDisabledText`, moved here from `RegionBlockLayout`). It then compares the written order with the order after the reorder for three sets: static members with initializers, instance members with initializers, and layout fields when the field order sets the layout. The reorder is given as the new rank of the part of the body that holds a position; code within one part keeps its order | SHARED-ORDER-007, SHARED-FIX-003 *Build configurations* and *Run time* |
| `OrderingDeclarationUtilities.ChangesMemberOrder(type, fieldOrderSetsLayout, moved, target)` (new, public) | The single forward move expressed as four parts: in front of the target's full span, the moved member's full span, the target up to the moved member, and behind the moved member. Excluded code in the moved member's leading trivia moves with it (SHARED-ORDER-002/-006), which matches `MoveNodeBeforePreservingSeparators` ("directives in a node's leading trivia stay with that node"). Excluded code in the target's leading trivia, or between target and moved member, is crossed | SHARED-ORDER-007 |
| `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` | Now a wrapper: `ChangesMemberOrder(type, false, moved, target)`. It is kept for RH7103, which moves only static members and so never reorders layout fields, and for the existing Core tests that call it. `ChangesLayoutOrder` is removed; its decision is part of `ChangesMemberOrder` | SHARED-ORDER-007 (RH7103 adjacent fix, decision 6) |
| `src/Reihitsu.Analyzer.CodeFixes/Core/RegionBlockLayout.cs` | `ChangesMemberOrder(type, fieldOrderSetsLayout)` delegates to the Core owner with `position => GetNewRank(GetSegment(position))`. Its private disabled-text parser and order comparison are removed | RH7309-006 |
| `RH7103…CodeFixProvider.IsMoveSafe` | Signature follows the base; it still calls `ChangesInitializerExecutionOrder`, which now counts excluded members | SHARED-ORDER-007, RH7103-007 |

### Policy owners

`rg -n "ChangesMemberOrder\(|ChangesInitializerExecutionOrder\(|FieldOrderSetsLayout\(|GetMembersOfDisabledText\(" src --glob '!*Test*'`: the owner is `OrderingDeclarationUtilities` (`ChangesMemberOrder` with its single-move overload, `ChangesInitializerExecutionOrder` as a wrapper, `GetMembersOfDisabledText`). The consumers are RH7103, RH7109, and `RegionBlockLayout` → RH7309. No second comparison of initializer or layout order is left; Outside the owner, no code combines `HasInitializer`, `IsStatic`, and `IsLayoutField` into an order comparison.

## thoenissen/Reihitsu#1262

### Changes (finding #3)

| File | What changed | Requirement |
|---|---|---|
| `RH7309…CodeFixProvider.ChangesDocumentedDeclaration` | Also checks the documentation comments in the leading trivia of the type's closing brace. The parser never puts a `///` in trailing trivia, so `} /// …` behind the last member lands there too. Such a comment documents nothing, and it keeps documenting nothing only when `KeepsSequenceBetween(comment, closeBrace)` holds. Otherwise, after the reorder it would stand in front of the first member of the block that follows | RH7309-006 bullet 4, SHARED-FIX-003 *Documentation* |

The opposite case — a last block that keeps its place — still gets the fix (`CodeFixKeepsADocumentationCommentThatDocumentsNothingInALastBlockThatKeepsItsPlace`).

## thoenissen/Reihitsu#1332

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7501…CodeFixProvider.CanMoveBreak` | Summary only: "the `break` keyword is the first token on its line, so not even the block's opening brace precedes it there", matching RH7501-004 as the spec author clarified it | RH7501-004 (FC-031) |

## thoenissen/Reihitsu#1336

### Changes

| File | What changed | Requirement |
|---|---|---|
| `RH7004…CodeFixProvider.ApplyCodeFixAsync` (finding #5) | A comment in the semicolon's trailing trivia moves onto the header line only when that trivia ends with a line break. A comment that code follows on its line stays behind the new block, which gives `main`'s output `} /* c */ }` | RH7004-004, decision 7 |
| `RH7004…CodeFixProvider.WouldSeparateDirective` (finding #1 a, b) | A conditional directive in front of the enclosing block's closing brace now withholds the fix only when its group's `#if` (from `GetRelatedDirectives`) lies behind the declaration. A group whose `#if` lies in front of the declaration holds the declaration in one of its branches, so no build configuration compiles another branch of it together with the declaration. Disabled text always follows the directive that opens its branch in the same trivia list, so checking the directives covers it | SHARED-FIX-003 *Build configurations*, RH7004-004 |
| `ModifierOrderingCodeFixProviderBase.ReorderingWouldDropCommentOrDirective` (finding #8) | `<returns>` now names a comment, a directive, or disabled text that would be dropped or relocated | FC-031 |

Findings #6 and the full-output tests are the test implementer's; they pass unchanged.

## thoenissen/Reihitsu#1340

No changes; the audit passed.

## Whole run

### Tests run

```shell
scripts/build.sh --configuration Debug
# Build succeeded, 0 Warning(s), 0 Error(s) (after moving GetWarningCode behind the public members for RH7110)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004Using|FullyQualifiedName~RH7103Static|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309|FullyQualifiedName~RH7501"
# before (d428302): Failed: 20, Passed: 219 — exactly the 20 of 11-test-implementer.md
scripts/test.sh --configuration Debug --project analyzer --filter "...same...|FullyQualifiedName~RH7102|FullyQualifiedName~RH7105|FullyQualifiedName~RH7106"
# after: Passed: 309, Failed: 0
scripts/test.sh --configuration Debug
# Analyzer 6620/6620, Formatter 4517/4517, Core 607/607, CLI 251/251, Architecture 115/115, Tooling 125/125, Playground 17/17 — all passed
```

`scripts/format.sh` was run over every changed source file; it reformatted one line in the RH7309 provider (RH5302). The Release build was not run; that is the cleaner's.

### Disputed tests or scope

No test is disputed.

Scope:
- `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` is a protected virtual member, and its signature changed: it now takes a `SemanticModel`. This was needed for finding #2 (the layout must come from the symbol). RH7103 and RH7109 override it; RH7102 inherits it.
- New public members in `Reihitsu.Core`: two `ChangesMemberOrder` overloads. `FieldOrderSetsLayout` changes its parameter from `TypeDeclarationSyntax` to `INamedTypeSymbol`. `ChangesLayoutOrder` is removed. All of these were added in this run, so no caller outside the run is affected.
- RH7103 is taken in as the adjacent fix named by decision 6.
- Production files changed in this round: 10. Each one was already changed by the run, except `TypeMemberOrderingCodeFixProviderBase` and the RH7103 provider; the RH7103 provider was named by decision 6.

### Architecture findings

_None._ Decision 6's structure is implemented: `OrderingDeclarationUtilities.ChangesMemberOrder` is the one owner of the member-order decision, and both the single move and the block permutation call it.

### Output questions

_None._ Every changed output is `main`'s output restored (#1, #5), a withheld fix that an accepted requirement demands (#2 a–c, #3, #4), or a fix that an accepted requirement demands offered again (#2 d, decision 8).

### Follow-up candidates

- `SplitsDirectivePair` pairs two `#pragma warning disable X` directives with each other, and the same for two `restore X`. A brace between them changes nothing that the directives govern, so the fix is withheld where it could be offered. This over-withholds, and no test or caller input reaches it today.
- Unchanged from earlier handovers: the RH7309 identity-order fix (SHARED-FIX-005); the member that an `#if` splits across active and disabled text is not counted; the RH7304 report for a region whose first token does not start its line.
