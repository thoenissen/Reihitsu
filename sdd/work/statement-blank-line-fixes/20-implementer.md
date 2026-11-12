Role: implementer
Commit: (this commit; parent f042be5)
Verdict: tests pass

Scope: architecture decision #26 (`00-lead.md`; 17-auditor finding #4), spec 18-spec-author (SHARED-AFTERBRACE-002, RH3208-002), tests 19-test-implementer. My prompt named the change asked for, which is decision #26 itself; it carried no suspected cause or hoped-for verdict beyond that.

## thoenissen/Reihitsu#946, #972, #1096, #1076, #1626

Nothing to do: decision #26 concerns none of them. The owner change below is the #1626 owner; its other callers (statement base, RH5030) keep their outputs.

## thoenissen/Reihitsu#1625

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `InsertBlankLineBefore` became `TryInsertBlankLineBefore(root, token, behindLeadingDirectives, out updatedRoot)`: in the refused cell (rebuild path, `TryGetIndentation` false) it inserts nothing, returns `false` and the root unchanged. `TryGetSwitchSectionIndentation` gives `null` instead of the continuation line's whitespace, so no caller can receive a fallback column. `CreateBlankLineLeadingTrivia` takes the column instead of computing it (the comment about callers that must check first is gone). Summaries updated | SHARED-STMT-004, SHARED-AFTERBRACE-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | New `protected virtual bool CanRegisterCodeFix(SyntaxNode root, TNode node)` (default `true`), checked at registration after the formatter's refusals | SHARED-FIX-005 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider.cs` | Overrides `CanRegisterCodeFix`: withheld when a next statement exists, it is not the terminal `break` (`BlankLineSpacingPolicy.RequiresBlankLineBehindClosingBrace(CloseBraceToken, IsTerminalDirectSwitchSectionBreak(next))`), the gap below the local function spans lines, and the owner's `CanInsertBlankLineBefore` is false for the next statement on the original tree. `GetNextStatement` extracted and reused by `ApplyCodeFixAsync`, which calls `TryInsertBlankLineBefore` (fallback branch unreachable: returns the original document) | SHARED-AFTERBRACE-002, RH3208-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` | Calls `TryInsertBlankLineBefore`; registration guard unchanged | SHARED-STMT-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5030BlankLineAfterClosingBraceCodeFixProvider.cs` | Calls `TryInsertBlankLineBefore` (cannot reach the rebuild path, `CanRegisterCodeFix`) | SHARED-AFTERBRACE-001 |

Why the decision is taken on the original tree: the formatter replaces only the local function and keeps its first token, so the facts the owner reads — the line on which the preceding sibling starts, the label continuation, which other statements start their own lines, the comment in the gap — are the same before and after the conversion. The decision is the owner's own `CanInsertBlankLineBefore`, not a copy of it.

Probes (`scripts/apply-fix.sh RH3208`, LF and CRLF, fixtures in the scratchpad): in the cell with a multi-line comment → `no-fix-offered`; same cell with no line break in the gap (`/* a */ Two(); return 2;`) → fixed, statement stays on the brace's line (nothing moves, as spec says); same cell where the statement behind the comment is the terminal `break;` → fixed (no blank line, nothing moves).

### Policy owners
- `rg -n "InsertBlankLineBefore\(|CanInsertBlankLineBefore" src --glob '!*Test*/**'`: one owner, `BlankLineCodeFixUtilities` (`CanInsertBlankLineBefore`, `TryInsertBlankLineBefore`); callers: statement base (register + apply), RH3208 (register + apply), RH5030 (apply; its own `CanRegisterCodeFix` keeps it on the insert path).
- `rg -n "RequiresBlankLineBehindClosingBrace|IsTerminalDirectSwitchSectionBreak" src --glob '!*Test*/**'`: owner `BlankLineSpacingPolicy` (Core); RH3208 now also calls both.

### Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 24 files changed, 492 insertions(+), 284 deletions(-).
- This round: 5 files changed, 101 insertions(+), 32 deletions(-). No new type; one new protected virtual member on `ExpressionBodyToBlockCodeFixProviderBase<TNode>`, one public method renamed (`InsertBlankLineBefore` → `TryInsertBlankLineBefore`, internal class).

### Deleted special cases
_N/A._

### Disputed tests or scope
- New protected member `CanRegisterCodeFix(SyntaxNode, TNode)` on the shared expression-body base (four providers derive from it; only RH3208 overrides). `RegisterCodeFixesAsync` is sealed, so registration needs a hook; the lead decides whether that is acceptable scope.
- RH3208's "the gap spans lines" check (`lastLine == nextLine` on the original tree) restates the line condition of `BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace`, which `ApplyCodeFixAsync` evaluates on the formatted tree. It is needed so the fix is not withheld where nothing would move (spec: "the statement it would move"); the policy's other two parts are called, not copied. Not an architecture finding in my reading (the fix reasons about its own conversion, which keeps the gap), but named here for the auditor.

### Architecture findings
_None._ The finding #4 cause (owner returns a fallback column, each caller must remember to ask first) is removed: the owner refuses itself.

### Follow-up candidates
_None_ new.

## Whole run

### Tests run
- `scripts/build.sh --configuration Debug`: Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3208|FullyQualifiedName~RH5030|FullyQualifiedName~PrecededByABlankLine|FullyQualifiedName~RH5002|FullyQualifiedName~RH5029|FullyQualifiedName~RH5011"`: Passed! Failed: 0, Passed: 475.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed: 0, Passed: 8217.
- `scripts/format.sh` over the five changed files: Formatted 0 of 5 file(s).
