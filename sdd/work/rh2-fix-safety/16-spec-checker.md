Role: spec checker
Commit: f4be099
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so there was nothing to ignore. Diff checked: `162e7e6..HEAD`, with this round's changes (`42db22c..HEAD`) read in full: specs RH2001, RH2005, `requirement-ids.txt`, the RH2001/RH2005 code fix providers, `DeclarationModifierUtilities`, `PartialMemberUtilities`, the RH2004 analyzer, `InterfaceImplementationUtilities`, the RH7409-RH7412 analyzers, and the tests. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`.

Finding 1 of `09-spec-checker.md` (type-parameter half of RH2001-008) was answered A by the maintainer as Q4 and is recorded under `Maintainer answers`; it is closed.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

## thoenissen/Reihitsu#967
- RH2005-003 (sentence and example for a removed non-first modifier that starts its own line, Q5 = A) matches `DeclarationModifierUtilities.CarryLeadingTriviaOfLaterAccessibilityModifiers`; RH2005-005 (new, Q6 = A: comment, directive, or `#if` block in front of a later accessibility modifier withholds the fix) matches `HasCommentOrDirectiveInFrontOfLaterAccessibilityModifier` and the one-line guard in `RH2005FieldsMustBePrivateCodeFixProvider`. RH2005-005 has format (one heading, normative sentence, `Why:`, three markup examples), is in the ledger at its sorted position after RH2005-004, and is covered by five tagged no-fix tests (with CRLF variants). RH2005-003 carries two new tagged tests (LF, CRLF).
- Code owner `DeclarationModifierUtilities` was already in the RH2005 header and `sdd/specs/README.md`; the new public member lives in it. `Uses` already lists SHARED-FIX-003.
- Rule pages `RH2004.md` and `RH2005.md` are unchanged in the run (`git log 42db22c..HEAD -- documentation` is empty); the examples of the pages are not affected by the repair, and no withheld-case note was added to a page.

## thoenissen/Reihitsu#1286
- RH2001-007 now lists the places (name on a line below the type, name-to-brace gap, accessor list, `}`-to-`=` gap, `;` behind an initializer on a later line) and states the boundary (comment behind the type, in front of the type, inside the initializer, line break alone). This matches `KeepsEveryCommentAndDirective` (decision 17: the withhold follows from what the built field keeps). All new examples are tagged on added tests (`RH2001-007`; the name-on-its-own-line pair also `RH2001-004` and `SHARED-FIX-007`, listed in 13's table or follows decision 21). The two multi-outcome tests (comment line in front of the type; comment behind the type) carry only SHARED-FIX-003, as 13 states: the spec says "not covered" for both, consistent with the diff.
- No ID added this round for RH2001; no code owner changed.

## thoenissen/Reihitsu#1287
- No spec text changed this round. The code (decisions 16, 23, 24) is consistent with SHARED-DIAG-012 (generated part never chosen; fallback to the implementing part when the defining part is generated; the defining part otherwise), with RH2004-001 ("the defining part, unless it lies in generated code"), and with RH7409-001 (covers a partial method only when each part would be reported; the only-some-parts case stays uncovered, as 14 states). `PartialMemberUtilities` stays the listed owner of `shared/diagnostics.md`; `InterfaceImplementationUtilities` stays listed in `shared/member-region-grouping.md`. The moved RH2004 report (symbol action instead of node action) is an implementation detail no requirement states.
- New tests are tagged SHARED-DIAG-012 (constructor, indexer, event, property, method, override cells of RH2004, RH74xx, RH8306).

## thoenissen/Reihitsu#1288
- Not touched this round; RH2004-003 and its test are as in `09-spec-checker.md`.

## Checked
1. Change doc: none present and none required (bug fix and clarifications; Q5/Q6 outputs are withheld-fix and keep-in-place cases decided by the maintainer's answers 5A, 6A, recorded under `Maintainer answers`); ok.
2. Specs match the diff: ok. RH2005-005 and the extended RH2001-007 follow the format, with markup examples.
3. IDs: ok. RH2005-005 is the only ID added this round, at its sorted position in `requirement-ids.txt`; nothing renumbered or deleted.
4. Code owners: ok. `SpecConsistencyTests` pass (architecture run above); no owner renamed, added, or removed this round; `sdd/specs/README.md` matches.
5. User documentation: ok. No rule page touched or needed; helpLink pages `RH2001.md` and `RH2005.md` exist.
6. Open questions: ok. Q5 and Q6 are answered under `Maintainer answers`; the spec author 12 found no open question; follow-up candidates 4, 5, 6 (outside the run) are under `Follow-up candidates`; none is written into a spec.
7. Coverage: ok. Architecture project 115 passed; RH2005-005 is covered.
8. Tags: ok. Every test added this round carries `[Requirement]` for its requirement; every requirement under `Examples for the tests` of `12-spec-author.md` (RH2005-003 new example, RH2005-005, RH2001-007) is tagged on an added test, and the `Tests` tables of 11 and 13 name the tags. The two untagged-by-spec RH2001 characterization tests carry only SHARED-FIX-003, which verifies a requirement they are written against.
9. Nothing left behind: ok. `sdd/work/` holds only `rh2-fix-safety`, which the lead deletes before publishing.
