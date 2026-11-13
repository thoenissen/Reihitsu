Role: implementer
Commit: see the commit that carries this file (production and handover in one commit)
Verdict: tests pass

Repair after decision 20 of `00-lead.md`. The prompt carried no intended fix, cause, or hoped-for verdict beyond decision 20 and its size target. No test file was changed. Analyzer 8308/8308, architecture 117/117, core 655/655 (Debug). Net production growth against `origin/main` is 87 lines (330 insertions, 243 deletions), and the change adds no new type.

## thoenissen/Reihitsu#1140

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementBracesCodeFixProviderBase.cs` (RH5405–RH5407) | Restored to `origin/main` (`git checkout origin/main --`). | SHARED-BRACE-002 as on `main` (decision 20) |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5403StatementMustNotBeOnSingleLineCodeFixProvider.cs` | Restored to `origin/main`. | RH5403-003 as on `main` (decision 20) |
| `src/Reihitsu.Analyzer.CodeFixes/Core/LineLayoutCodeFixUtilities.cs` | Deleted. Its brace-layout part (`ReplaceWithBlockOnOwnLinesAsync`, `IsFollowingClause`) had only the reverted callers. The shared-line part moves into `BlankLineCodeFixUtilities` (see #1144). | — |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs` | Size only, no behavior change: the private `EndsMultiLineCommentOnLine` is folded into the comment loop that `AlignCommentIndentation` already runs over the token's leading trivia. The same kinds are checked (`/* */`, `/** */`), with the same lines (starts above the token's line, ends on it). | RH5204-001, SHARED-BLANK-008 (#1561) |

The 7 brace tests that 06 names as red are green. The RH5204 fix and the other changes of 04 are unchanged.

## thoenissen/Reihitsu#1141
No change in this round.

## thoenissen/Reihitsu#1144

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | Two new methods replace the shared-line part of the deleted type. `SharesLineWithSiblingMember(member, left, right)` is true when one of two adjacent tokens belongs to another member of the same list and the gap holds only whitespace trivia. It merges the old `SharesLineWithPreviousSiblingMember`, `IsSiblingMemberBoundary`, and `IsWhitespaceGap` into one cheap check: the token next to a member is always the boundary token of its sibling, so the boundary test was redundant. `FormatMemberOnLinesOfItsOwnAsync` is the old method with `SeparateFromSiblingMembers` and `FormatAnnotatedNodeAsync` inlined. The gap is known to be whitespace only, so the token in front gets exactly one line break as trailing trivia and the token behind gets exactly the indentation. `EndWithLineBreak` is no longer needed. The indentation reuses the class's private `GetLineIndentation`. The class summary now names the second job. | SHARED-FIX-007 (shared line) |
| `RH5417…CodeFixProvider.cs`, `RH3202…CodeFixProvider.cs` | Call `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` instead of the deleted type. `DocumentOrderFixAllProvider` stays as in 04. | SHARED-FIX-007, SHARED-FIX-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/TrailingCommaCodeFixHelper.cs` | Size only, no behavior change. The private `SkipSpacesForward` becomes one `TrimStart(' ', '\t')` over the rest of the line, and the private `GetMatchingOpenBrace` becomes an inline pattern in the condition. | SHARED-FIX-007 |

## thoenissen/Reihitsu#1145
No change in this round.

## thoenissen/Reihitsu#1094

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs` | `IsInnermostEnclosingBraceOffItsDepthColumn` is removed, and its check moves into the existing ancestor loop of `HasIncompleteEnclosingScope`, renamed `HasUnusableEnclosingScope`. That loop already switches over the same brace owners. Outside every anchor scope, it compares the leading whitespace of the first enclosing `{` in front of the list (tab = 4) with `ComputeBaseIndentLevel(owner) × 4`. The behavior is the same as in 04: initializers never reach the check because `HasAnchorScopeAncestor` turns it off. The shared-line break in front of a list uses `BlankLineCodeFixUtilities.SharesLineWithSiblingMember(member, previousToken, openBracket)`, which is also false when the list is not the member's first token. The token in front gets the line break as its whole trailing trivia, because the gap is whitespace only. | SHARED-ATTR-021, SHARED-FIX-007 |

## thoenissen/Reihitsu#1393
No change in this round.

## thoenissen/Reihitsu#1511
No change in this round.

## thoenissen/Reihitsu#1561
The `EndsMultiLineCommentOnLine` fold is listed under #1140 (same file). There is no behavior change.

## Whole run

### Policy owners
- Shared line between sibling members: `rg -n "SharesLineWithSiblingMember|FormatMemberOnLinesOfItsOwnAsync|EndWithLineBreak" src --glob '!*Test*'` finds one owner, `BlankLineCodeFixUtilities`. Its callers are RH5417, RH3202, and `TargetAttributePlacementCodeFixProviderBase`. `EndWithLineBreak` no longer exists.
- Line indentation: `rg -n "static string GetLineIndentation" src --glob '!*Test*'` finds `BlankLineCodeFixUtilities.GetLineIndentation` (reused, private) and `UsingDirectiveOrderingUtilities.GetLineIndentation`, which takes a using directive and is not applicable.
- Trailing-whitespace trimming: `TrimTrailingWhitespace` has two private copies, in `BlankLineCodeFixUtilities` and `Reihitsu.Core.DeclarationModifierUtilities`. Both predate the run, and the run uses neither.
- Attribute depth model: `SyntaxIndentationUtilities.ComputeBaseIndentLevel`/`HasAnchorScopeAncestor`, reused in the existing loop.

### Production diff size
`git diff --shortstat origin/main -- src ':(exclude)*Test*'`: 14 files changed, 330 insertions(+), 243 deletions(-). That is 87 net lines, down from 408 in 04.
New type: none. `LineLayoutCodeFixUtilities` is deleted.
New members: `BlankLineCodeFixUtilities.SharesLineWithSiblingMember` and `FormatMemberOnLinesOfItsOwnAsync` (both on an internal class). `FormattingTextAnalysisUtilities.IsOperatorGapViolation` is public, as in 04. Making it public in place instead of moving it trips RH7110 (member order), so the move stays.
Delta of this round against `ba83087`: the brace fixes are back to `main`, the new file (294 lines) is removed, the attribute guard is folded (about -35), and two private helpers each in the trailing-comma helper and the RH5204 analyzer are inlined (about -50).

### Deleted special cases
_N/A._ No shape is declared unsupported.

### Tests run
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 8308, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117, Failed 0.
- `scripts/test.sh --configuration Debug --project core`: Passed 655, Failed 0.
- `scripts/format.sh` over the changed production files: no further change.

### Disputed tests or scope
_None._ Within the target of decision 20: no new type, and 87 net lines.

### Architecture findings
_None new._ The #1140 finding stands as recorded. The remaining column computations are `GetLineIndentation` (shared line) and the trailing-comma `}` column, each a cheap read of one line's leading whitespace.

### Follow-up candidates
_None new._ The candidates already in `00-lead.md` (brace-fix instances of #1140, the `else if` column with #1416) still apply.
