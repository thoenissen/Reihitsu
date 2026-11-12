Role: implementer
Commit: (this commit; parent eef0112)
Verdict: tests pass

Repair round 1, decisions #23 and #24 of `00-lead.md` (auditor findings #1 and #2, hint on the "one owner" summary). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those decisions. No test touched.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | New `CanInsertBlankLineBefore(root, token)`: true when no token precedes, when a line break outside comments separates the tokens (insert path), or when the moved statement gets a column. `GetIndentation`/`GetSwitchSectionIndentation` became `TryGetIndentation`/`TryGetSwitchSectionIndentation`; they return false in exactly the cell finding #1 named (initializer, anchor line is a label continuation, no other statement of the section starts its own line) and still give the continuation line's whitespace there. Class summary no longer claims to be the only owner: it names its callers and that RH5011's fix inserts its own blank line | SHARED-STMT-004 (withholding paragraph); decision #24 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs` | `RegisterCodeFixesAsync` skips a diagnostic whose statement fails `CanInsertBlankLineBefore` — the shared path of all 20 SHARED-STMT-004 rules, so Fix All skips it too | SHARED-STMT-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/WhitespaceSpanRemovalCodeFixProviderBase.cs` | Class summary: the guard inspects the reported span; `GetRemovalSpan` may widen it only by whitespace-only lines; the blank-line specialization widens to the run for rules that name it | SHARED-WS-001 (as amended by the spec author); decision #24 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/BlankLineSpanRemovalCodeFixProviderBase.cs` | Class summary: shares the guard; a rule that reports one line of a run names the whole run as its deleted span and the fix widens to it; other rules remove the reported span only | SHARED-WS-001; decision #24 |

## Policy owners
- Column of a moved statement in an initializer's switch section: `rg -n "GetSwitchSectionIndentation|TryGetSwitchSectionIndentation|ComputeAnchorDerivedIndentation" src --glob '!*Test*/**'` — `BlankLineCodeFixUtilities` (only owner of the new predicate, reusing its own column computation; no second copy) and `RH5103…CodeFixProvider` via `SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation` (unchanged, decision #9).
- Withholding predicate: `rg -n "CanInsertBlankLineBefore" src` — defined once, called from the statement base only.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 24 files changed, 432 insertions(+), 275 deletions(-). This round: 4 files changed, 77 insertions(+), 26 deletions(-) (mostly XML documentation). No new type; one new internal member (`CanInsertBlankLineBefore`).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — Build succeeded, 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH50|FullyQualifiedName~RH3208|FullyQualifiedName~RH6004"` — Passed: 1032, Failed: 0 (includes the two new `VerifyNoCodeFixWhenOnlyStatementsOfSectionShareLabelContinuationLine…` tests, which failed before as 13-test-implementer reports).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 8213, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 117, Failed: 0.

## Disputed tests or scope
- RH3208 (SHARED-AFTERBRACE) also calls `InsertBlankLineBefore` and does not check `CanInsertBlankLineBefore`, so in the same rare cell (local function on a label's continuation line inside an initializer, the next statement left on the closing-brace line) it keeps today's output: the continuation line's whitespace. Decision #23 names the SHARED-STMT-004 rules; RH3208 computes its action lazily after formatting the local function, so withholding there would need the formatted tree at registration time. Left unchanged; the lead decides whether it is a follow-up. RH5030 cannot reach the cell: it registers only where a line break separates the tokens (insert path).

## Architecture findings
_None new._ The cell belongs to decision #9's structure (the fix computes a column the indentation phase owns); withholding avoids a prediction.

## Follow-up candidates
- RH3208: same withholding for a local function standing on a `case` label's continuation line inside an initializer, if the lead wants SHARED-AFTERBRACE to match (see above; rare).
