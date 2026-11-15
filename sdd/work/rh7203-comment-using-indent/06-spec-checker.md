Role: spec checker
Commit: 972d7ea
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. FMT-USING-011 is a clarification (maintainer answers 1A/2A in 00-lead.md write down behavior the phase already gives the same block without the comments); no requirement changes meaning or is retired, so no change doc is required and none exists.
2. Specs match the diff: ok. FMT-USING-011 states the two-file production change (header split keeps the original first directive's line whitespace; first-position prefix gets that whitespace, tab included); one requirement per heading, normative sentence first, `Why:` present, examples for both sides of the split, tab and file-scoped variants. RH7201-RH7207 `Uses` and RH72xx-004 sentences name FMT-USING-011 (checked RH7203 in full; the other six are the same 4-line diff). The non-goal sentence in using-directives.md is consistent.
3. IDs: ok. FMT-USING-011 is the next free number, inserted after FMT-USING-010 in `requirement-ids.txt`; nothing reused, renumbered, or deleted.
4. Code owners: ok. No owner renamed, added, or removed; `sdd/specs/README.md` needs no change. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118" (includes SpecConsistencyTests).
5. User documentation: ok. No rule page affected: what is reported and how to fix it do not change.
6. Open questions: ok. Both output questions (skeptic) are answered 1A/2A under `Maintainer answers`; spec author lists none open; follow-up candidates: none.
7. Coverage: ok. Architecture run above passes, including RequirementCoverageTests; FMT-USING-011 is covered by 41 `[Requirement("FMT-USING-011")]` tests.
8. Tags: ok. The RH7201-RH7207 and RH7203 code-fix tests carry both `RH72xx-004` and `FMT-USING-011` as the test implementer's table says; formatter tests carry FMT-USING-011; the two whole-document guard tests are listed as FC-033/FC-010 without a tag, which the table states. Every example the spec author named (all of FMT-USING-011) is tagged on at least one test.
9. Nothing left behind: ok. `sdd/work/` holds only `rh7203-comment-using-indent`.
