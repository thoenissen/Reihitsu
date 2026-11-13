Role: implementer
Commit: 9a3dded
Verdict: tests pass

How the statements below were established: `00-lead.md`, `05-test-implementer.md`, the change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md` and the spec diff against `origin/main` read; the 102 failing rows reproduced on 7cede9c (`Failed: 102, Passed: 765`) before any change; every handed-over test made to pass without touching a test file. Ignored input: my prompt carried process guidance only (no intended fix, cause, or hoped-for verdict).

## Whole run

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingMoveSafety.cs` | New `TryCreateWholeLineMove` (text change of a member/accessor move: whole-line guard, separators stay at their positions, every moved line shifted by the first token's shift to the target's column, crossed nodes that share a line move as one block, a `///` comment behind code in front of the moved node travels onto its own line) and `TryCreateSwapWithinLine` (one-line accessor list) | SHARED-ORDER-002, SHARED-ORDER-009, SHARED-ORDER-010, SHARED-FIX-007, SHARED-DOC-004 |
| `src/Reihitsu.Core/SyntaxIndentationUtilities.cs` | New `TryShiftMovedLines`: one owner of the moved-code line rule (blank lines, column-0 comment lines, non-region directive lines keep their column; refusal when a line starts inside a token or a multi-line comment, or is disabled text, and the shift is not zero) | SHARED-FIX-007, SHARED-ORDER-010, RH7004-007 |
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | New `TryCreateMove`: swap within the line for a one-line accessor list, whole-line move otherwise | SHARED-ORDER-009, RH7107-004, RH7108-004 |
| `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs` | New `RemoveDirectiveLines`: removes directive lines and the blank lines the removal leaves at a brace edge (any construct) or doubled; inserts none | SHARED-FIX-007 (*Removed directive line*), RH7303-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TypeMemberOrderingCodeFixProviderBase.cs` | RH7102/RH7103/RH7109: apply the whole-line move as a text change; no formatter call; `FindUnsupportedSyntax` guard replaced by the move's own guard | SHARED-FIX-006, SHARED-FIX-007, SHARED-ORDER-009/010, decision 6 |
| `RH7107…CodeFixProvider.cs`, `RH7108…CodeFixProvider.cs` | Same for accessors, via `AccessorOrderingUtilities.TryCreateMove` | as above, RH7107-004/005, RH7108-004/005 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ModifierOrderingCodeFixProviderBase.cs` | RH7105/RH7106: write only the reordered modifiers, no formatter call, no `FindUnsupportedSyntax` guard | SHARED-FIX-006, decision 6 |
| `RH7104…CodeFixProvider.cs` | Insert only the accessibility keywords, no formatter call, no `FindUnsupportedSyntax` guard | SHARED-FIX-006, RH7104-004, decision 6 |
| `RH7004…CodeFixProvider.cs` | Rewritten as three text changes: `(` before the declaration, `)` for `;`, and the block `{` / shifted statements / `}` in the declaration's column; `CreateUsingStatement` and the formatter call removed | RH7004-004, RH7004-007, SHARED-FIX-006, SHARED-FIX-007 |
| `RH7302…CodeFixProvider.cs` | `#endregion` renamed only when its description (SHARED-REGDESC-001, via `RegionDirectiveUtilities.GetEndRegionDescription`/`GetRegionDescription`) equals the region's; only the description text is replaced; `ReplaceExactText` deleted | RH7302-005, SHARED-FIX-003 |
| `RH7303…CodeFixProvider.cs`, `RH7310…CodeFixProvider.cs` | Line removal through `RemoveDirectiveLines` | SHARED-FIX-007, RH7303-004 |

### Policy owners

- Moved-code line shift: `rg -l --glob '!*Test*/**' "columnDelta|ColumnDelta|TryShiftMovedLines" src` → `SyntaxIndentationUtilities` (new owner; callers `OrderingMoveSafety`, RH7004) and `RH3004UseLambdaSyntaxCodeFixProvider` (its own shifter; see *Architecture findings*).
- Removed-directive blank lines: `rg -l --glob '!*Test*/**' "RemoveDirectiveLines|removal leaves" src` → `RegionDirectiveBlankLineUtilities` (owner), RH7303, RH7310; `EmptyParenthesesCodeFixProviderBase` matches only on unrelated wording (spaces).
- Member/accessor move: `rg -l --glob '!*Test*/**' "TryCreateWholeLineMove|TryCreateSwapWithinLine|MoveNodeBeforePreservingSeparators" src` → `OrderingMoveSafety` (owner), `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase`, and `OrderingDeclarationUtilities` (old `MoveMemberBefore`, now reached only by Core tests; see *Disputed tests or scope*).
- End-region description: `rg -l "GetEndRegionDescription" src` → `RegionDirectiveUtilities` (owner), reused by RH7302 fix, RH7301, RH7306, `RegionDescriptionAnalyzerBase`.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 13 files changed, 700 insertions(+), 242 deletions(-). No new type. The additions are three shared helpers in existing Core classes (`OrderingMoveSafety` about +200, `SyntaxIndentationUtilities` about +145, `RegionDirectiveBlankLineUtilities` about +125, all with XML comments) plus the rewritten fix providers.

### Deleted special cases

_N/A._ (no shape declared unsupported)

### Tests run

- Baseline on 7cede9c: `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7004|FullyQualifiedName~RH710|FullyQualifiedName~RH7302|FullyQualifiedName~RH7303|FullyQualifiedName~RH7310"` → `Failed: 102, Passed: 765, Total: 867`.
- Focused, after: `--filter "FullyQualifiedName~RH710"` → `Passed: 724`; `~RH7004` → `Passed: 72`; `~RH7302` → `Passed: 23`; `~RH7303|~RH7310` → `Passed: 48`.
- `scripts/test.sh --configuration Debug --project analyzer` → `Failed: 0, Passed: 8428`.
- `scripts/test.sh --configuration Debug --project core` → `Failed: 0, Passed: 655`.
- `scripts/test.sh --configuration Debug --project architecture` → `Failed: 0, Passed: 117`.
- `scripts/build.sh --configuration Debug --no-incremental` → 0 warnings, 0 errors. `scripts/format.sh` over the changed files: no change left.
- Not run: formatter and CLI projects (the Core changes only add methods the formatter does not call); the validator runs them.

### Disputed tests or scope

1. **Scope.** Three shared helpers with more than one consumer were added (`TryShiftMovedLines`: ordering moves and RH7004; `TryCreateWholeLineMove`: RH7102/7103/7109 and, through `TryCreateMove`, RH7107/7108; `RemoveDirectiveLines`: RH7303, RH7310). Together about +470 lines in Core, which is more than double the production paths the handovers name. No new type.
2. **Old move helpers now unused in production.** `OrderingMoveSafety.MoveNodeBeforePreservingSeparators`, `OrderingDeclarationUtilities.MoveMemberBefore`, and `AccessorOrderingUtilities.MoveAccessorBefore` are now called only by `Reihitsu.Core.Test` (21 references in `OrderingDeclarationUtilitiesTests`, `AccessorOrderingUtilitiesTests`). I kept them because deleting them deletes tests. They still state SHARED-ORDER-002's separator rule, with the old indentation handling (see *Architecture findings*).
3. **RH7004 refusals no spec requirement lists** (cheap guards, rare classes, the diagnostic stays):
   - code stands in front of the using declaration on its line (`{ using var s = …;`): the new braces need a column, and only a line indentation exists;
   - the last moved statement shares its line with the closing brace of the block (`s.Flush(); }`): the new `}` would need a line break;
   - a comment or directive in the leading trivia of `;`: kept so that the existing test `NoFixIsOfferedForACommentInFrontOfTheSemicolon` (tagged SHARED-FIX-010) still passes. Its stated reason, that the formatter refuses the built using statement, no longer applies, because the fix no longer formats. The guard is now a local rare-class refusal, and the test's summary and tag no longer describe why.
   The spec author may want to record these in RH7004's spec (AGENTS.md, *Code-fix coverage*).
4. **Member/accessor move refusal no spec requirement lists.** The move is refused when the target's leading trivia starts behind code on the line in front of it. This happens only for a `///` comment behind code in the `Parse` documentation mode, where the comment documents the target. Moving a declaration there would hand the comment to the moved declaration (SHARED-FIX-003, documentation bullet). This is rare, and SHARED-ORDER-009 states only the token condition.
5. **Layout decisions the specs leave open:**
   - a `///` comment behind code that travels with the moved declaration (SHARED-DOC-004 existing tests) starts its own line at the target's indentation, and the trailing whitespace it leaves on the code's line is removed. SHARED-DOC-004 says where the comment then starts is not covered;
   - crossed declarations that share a line move as one block;
   - for RH7303/RH7310, a removal that joins blank lines keeps the blank lines in front of the first removed line and removes the later ones;
   - shifts count each character as one column, so a tab counts as one. A shift to the left removes up to that many leading whitespace characters.
6. **Fixes now offered next to a refused comment** (decision 6), in addition to the seven test-implementer finding 4 lists: RH7004 and RH7106 also lost their `FindUnsupportedSyntax` guard, because neither fix formats any more.

### Architecture findings

1. **Moved-code line rule stated twice.** `SyntaxIndentationUtilities.TryShiftMovedLines` (new; ordering moves and RH7004) and `RH3004UseLambdaSyntaxCodeFixProvider.IsMovingLine`/`AppendLaterLines` both decide which moved lines keep their column. The rules differ:
   - RH3004 keeps a line only when the line holds no code and its first comment starts in column 0;
   - SHARED-FIX-007 as now written, and the new helper, keep every line that starts with a comment in column 0, and also keep non-region directive lines.
   Input that shows the difference: a line `/* x */ Foo();` in column 0 among moved code. RH3004 moves it; the RH7004 and member-move fixes keep it in column 0. Decision 3 dropped the shared helper, and I did not merge the two.
2. **SHARED-ORDER-002 separator handling stated twice** until finding 2 under *Disputed tests or scope* is settled. The old syntax-level `MoveNodeBeforePreservingSeparators` gives each slot's indentation to the node that moves into it. The new text-level `TryCreateWholeLineMove` keeps each node's own indentation and shifts only the moved node. Input: the misindented field in `FieldIndentedDeeperThanItsSlotMovesAllItsLinesByTheSameShift`. The old helper would put `_a` in column 8, and the new one keeps it in column 4.

### Follow-up candidates

- Delete the old syntax-level move helpers and their Core tests (finding 2 above), or turn those Core tests into tests of the new text-level helpers.

## thoenissen/Reihitsu#969

- RH7102/RH7103/RH7109 now move whole lines through `TryCreateWholeLineMove` and format nothing (SHARED-FIX-006): the issue example (`FixLeavesTheOtherMembersOfTheTypeAsWritten`, LF/CRLF), `Decoy`/`Counter`, and three blank lines above the moved member all pass.
- Shared-line refusals (SHARED-ORDER-009): moved member with a field in front of it or behind it, target behind the type's `{`, moved member in front of the type's `}`.
- Shift refusals (SHARED-ORDER-010): verbatim string, `/* */` comment in leading trivia, raw string, disabled text. Where the declaration is indented like its slot (shift 0), the move is offered.
- Column-0 comment and `#region` lines in a shifted method: `ShiftedMethodKeepsAColumnZeroCommentAndMovesItsRegionDirectives`.
- The existing SHARED-DOC-004 tests (`FixCarriesADocumentationCommentBehindCode…`) keep passing; see *Disputed tests or scope* 5.

## thoenissen/Reihitsu#1335

- RH7004: text changes only (header, braces, statements moved one level by `TryShiftMovedLines`); no blank line inserted; RH7004-007 refusals, plus the three listed under *Disputed tests or scope* 3. `FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlock` (empty block, `/* c */ }` behind it) keeps passing through the empty-block branch.
- RH7104/RH7105/RH7106: only the modifiers change.
- RH7107/RH7108: a one-line accessor list swaps within the line; a list that spans lines moves whole lines, and a shared line there is refused.

## thoenissen/Reihitsu#1263

- RH7303 and RH7310 remove lines through `RemoveDirectiveLines`. Blank lines at the edge of a type, block, accessor list, switch, or initializer are removed, and so are doubled ones. A blank line behind a `case` label stays (`CodeFixKeepsASingleBlankLineBehindACaseLabel`).

## thoenissen/Reihitsu#1151

- RH7302 renames the `#endregion` only when its SHARED-REGDESC-001 description equals the region's description, and replaces only that description text. The keyword (`#endregion`), `//`, and spacing are never touched.

## thoenissen/Reihitsu#1604

- Items (a)–(c) pass through the #969 changes (`UntouchedExpressionBodiedMethodKeepsItsBodyWhenAFieldMoves`, `LineBreakInsideTheMovedFieldStays`, the continuation-line tests). Item (d) stays a passing characterization test (`NoCodeFixWhenTheMoveCrossesAPropertyWithAConditionalPairInItsBody`). The dropped shared-helper goal (decision 3): no RH3004 change; see *Architecture findings* 1.
