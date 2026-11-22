Role: spec checker
Commit: 35b6eaf
Verdict: consistent

Repair round 1 (`git diff 5a0472e HEAD`, excluding `sdd/work`): the anchor of SHARED-ORDER-008's layout clause (decision 13), the change doc, SHARED-ORDER-007/008, RH7309-005, `OrderingDeclarationUtilities.ChangesMemberOrder`, and four RH7309 tests. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. ok. The existing change doc `ordering-move-refuses-conditional-groups.md` stays `Accepted`; its *Affected requirements*, *What*, and *Where the type-layout check reads* carry the new anchor; the new RH7309 example sits under *Visible output changes*; the reading of "in front of the moved fields" is recorded under `Decisions`, covered by answer 1668-2 A (decision 13). No task list.
2. ok. SHARED-ORDER-008 third place, SHARED-ORDER-007 and RH7309-005 state the anchor "last member of the spans that counts as an instance field of the layout"; the code (`lastCrossedLayoutFieldOffset`: last `IsLayoutField` member at a crossed position; directives inside the spans already refuse earlier) and its summary match. The member-move case is stated as unchanged and the code agrees. Requirement format kept (`Why:` present, examples with `{|RH7309:…|}` markup).
3. ok. No requirement added, retired, or renumbered; `requirement-ids.txt` untouched.
4. ok. No spec header changed; `scripts/test.sh --configuration Debug --project architecture`: `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.
5. ok. No rule page touched; a withheld fix in a corner case stays out of the page.
6. ok. No new output question (15-spec-author: none); the new no-fix case is covered by 1668-2 A and the change doc lists it as a visible output change. Follow-up candidates: none.
7. ok. The new RH7309-005 example has covering tests (the changed no-fix test and the prose opposite); the architecture project is green.
8. ok. All four added tests (`NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct`, `NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields`, `FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns`, `FixReordersTheBlocksWhenAGroupBetweenTwoReorderedRunsPrecedesAnInstanceFieldWhoseOrderStays`) carry `[Requirement("RH7309-005")]` and `[Requirement("SHARED-ORDER-008")]`, matching the test implementer's table; both requirements named under `Examples for the tests` are tagged.
9. ok. `sdd/work/` holds only this run's directory; the lead deletes it before publishing.
