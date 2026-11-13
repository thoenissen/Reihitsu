Role: spec checker
Commit: 877dcfb
Verdict: findings

Re-check after repair round 2 (spec 0669c17, tests 8f807d1, production 959256d). My prompt carried no intended fix or hoped-for verdict. Earlier checks stand where the repair did not touch them.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/shared/code-fixes.md` SHARED-FIX-008 (new paragraph) vs `src/Reihitsu.Core/LineEndingUtilities.cs` `HoldsOtherLineBreak` | The spec withholds a move for a foreign line break "outside a literal and outside the text of a comment other than a documentation comment", so a foreign break inside a string or a non-doc comment would not withhold. `HoldsOtherLineBreak` counts every line break starting in the span, with no literal or comment exception, so the fix is withheld there too (e.g. a member that moves without a shift and holds a verbatim string or `/* */` comment with an LF in a CRLF document). The spec promises an offer the code does not make. Cheapest repair is to drop the exception from the spec, since carving literals and comments out would need machinery the code-fix rule does not allow; a one-line statement that a foreign break anywhere in the moved span withholds is all the code does. | spec author |

## Checked
1. Change doc: ok. `moved-code-keeps-its-lines.md` is `Accepted`, has `Visible output changes` (new "Rare-class refusals of repair round 2" section, After: no fix), and four new decisions (34, 35, 36, 37) that match `00-lead.md` decisions 34–37 and 40. No new output question; the round's refusals are rare-class, so no maintainer answer is needed. Note, not a finding: the header lists SHARED-FIX-008 as "changed" while the decision text says "unchanged in its demand"; the demand to write the detected ending is unchanged, the added refusal is the change.
2. Specs match the diff: finding 1. Otherwise ok. Interpolated strings in every `$` form (RH7004-007, SHARED-ORDER-010, RH3004-008, SHARED-FIX-007), the tab refusal on the declaration line and on a moving line (RH7004-007; the implementation also applies it on the empty-block path, which RH7004-007 does not restrict, implementer 21 disputed item 3), the accessor-list comment refusal outside every accessor (SHARED-ORDER-009; `HoldsCommentOutsideEveryAccessor` uses the accessor span, so a comment in an accessor's body, expression body or attribute list still swaps), and the detected-ending write with the whole-line-move, RH7004 and RH3004 block-body refusal (SHARED-FIX-008; the one-line swap stays offered) match the code. Format of the changed requirements is kept (normative sentence first, `Why:` present, examples with markup).
3. IDs: ok. The round adds no requirement; `requirement-ids.txt` is untouched.
4. Code owners: ok. No owner changed; `SpecConsistencyTests` passes (see 7). The new Core members are public methods of existing owners; no spec header names a new type.
5. User documentation: ok. No rule page changed; every change is a withheld fix, which the pages do not state (`general-notes.md`). RH7004 page still carries no withheld-fix text.
6. Open questions: ok. The spec author's and the test implementer's handovers list none; the implementer's three disputed items are decisions 41, accepted.
7. Requirements covered: `scripts/test.sh --project architecture` gave `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`. Every changed requirement has a covering `[Requirement]` test in 8f807d1 (RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010, SHARED-FIX-008, RH3004-008).
8. Test tags: ok. Every method 8f807d1 adds carries the tags its row in `20-test-implementer.md` lists (several rows list the requirement under a second ID that the method also carries, e.g. RH7109-006, RH7107-004/005, RH7108-004/005, SHARED-FIX-007), and every requirement under `Examples for the tests` in 19 (RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010, SHARED-FIX-008) is tagged on at least one added test. The table has no tag a method lacks.
9. Nothing left behind: ok. Only `sdd/work/fix-scope-moved-code/` exists under `sdd/work/`; the lead deletes it before publishing.
