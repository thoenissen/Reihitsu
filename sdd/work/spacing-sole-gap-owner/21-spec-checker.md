Role: spec checker
Commit: ae99b2e
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None | – |

## Checked
1. Change doc: ok. Clarification of FMT-SPACE-021 only; no new behavior, no change doc edit needed.
2. Specs match the diff: ok. Both example blocks of FMT-SPACE-021 declare `unsafe struct C` and match, line for line, the `input` and `expected` literals of `MembersAndStatementsThatDeclareAVariableAreSpacedLikeTheSpecificationExample`. The parameter bullet names every `ParameterSyntax` owner. `DeclaredNameSpacingRule` has the single arm `ParameterSyntax parameter when right == parameter.Identifier`, and `ParameterOfEveryParameterListGetsOneSpaceBehindTheirType` (as `ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType`) covers the conversion operator, the anonymous method and the extension block that the repair added.
3. IDs: ok. None added.
4. Code owners: ok. The header is untouched and the architecture project passes.
5. User documentation: ok. No rule page is touched.
6. Open questions: ok. The spec author's handover has none.
7. Coverage: ok. `scripts/test.sh --project architecture` -> `Passed! - Failed: 0, Passed: 92, Skipped: 0, Total: 92`.
8. Tags: ok. The changed tests carry `[Requirement("FMT-SPACE-021")]`, as does the test implementer's table; the second example is tagged on the verbatim test.
9. Leftovers: ok. Only this run's `sdd/work/` directory is present; the lead deletes it.
