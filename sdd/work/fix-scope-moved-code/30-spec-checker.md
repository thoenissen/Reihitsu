Role: spec checker
Commit: 863d57d
Verdict: findings

Check of the repair 0d587f2..863d57d (auditor 26, test implementer 27, implementer 28; lead decisions 43-45). My prompt carried no intended fix or hoped-for verdict. Earlier checks (25-spec-checker.md) stand otherwise. `git diff 0d587f2..863d57d` touches no spec, change doc, or rule page: only `RH7004...CodeFixProvider.cs`, `RH7004...AnalyzerTests.cs`, a summary in `SyntaxIndentationUtilities.cs`, and `sdd/work/`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/analyzer/RH7004.md` RH7004-007 (last sentence); `sdd/specs/shared/code-fixes.md` SHARED-FIX-008 (lone-`\r` sentence of paragraph 2, and the Why's "cheap check of the line breaks in the moved span") | The code now refuses on `FromBounds(usingDeclaration.SpanStart, movedSpan.End)`: the rewritten declaration, its trailing line break, and the moved statements, on both the statements path and the empty-block path (tests `NoCodeFixWhenTheLineBreakBehindTheDeclaration...`, `...BehindTheLastStatementDeclaration...`, `NoCodeFixForALastStatementDeclarationInADocumentWithLoneCarriageReturnLineBreaks`, `NoCodeFixWhenTheDeclarationHoldsALineBreak...` and `...AndTheBlockStaysEmpty`). RH7004-007 says the fix "is withheld where the code it moves holds a line break other than that line ending", and SHARED-FIX-008 says that in a lone-`\r` document the fix is withheld "wherever the code it moves holds a line break" and that the check is "of the line breaks in the moved span". None of the three names the rewritten declaration or its trailing break, and a last-statement declaration moves nothing, yet is now withheld in a lone-`\r` document. The first sentence of paragraph 2 ("moves or rewrites") is correct; the three places above are narrower than the code. | spec author |

## Checked
1. Change doc: ok, no change doc edit needed (a refusal of a rare class under existing SHARED-FIX-008, recorded as decision 43; no requirement added, changed in intent, or retired).
2. Specs match the diff: finding 1.
3. IDs: ok, untouched.
4. Code owners: ok, no owner changed.
5. User documentation: ok, no rule page changed.
6. Open questions: ok, 26 to 28 raise none.
7. Requirements covered: ok, no spec changed since 25 (`scripts/test.sh --project architecture` Passed 117, Failed 0 per 28 and 25 stands); the five new tests cover existing active requirements.
8. Test tags: ok. All five new RH7004 tests (`git diff 0d587f2..863d57d` on the test file) carry `[Requirement("SHARED-FIX-008")]` and `[Requirement("RH7004-007")]`, both of which the spec states for this behavior; the test implementer's table names the same two for each.
9. Nothing left behind: ok, only this run's `sdd/work/`.
