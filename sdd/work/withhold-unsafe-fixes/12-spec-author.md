Role: spec author
Commit: fbc573d
Verdict: spec written

Repair round 1. Worklist: audit finding #7, and whether lead decision 6 needs spec text. The prompt carried no intended fix or hoped-for verdict beyond the audit's suggested change, which I checked against the accepted change doc rather than adopting it.

## Changed requirements

- RH7501-004 — The fix moves the `break` behind the block — clarified. Condition 2 now reads "the `break` keyword is the first token on its line — no token, not even the block's opening brace, precedes it there, while a comment may". This is what the accepted change doc already decided ("withheld when the `break` shares its line with other code") and what `CanMoveBreak` does: for `{ break;` ⏎ `}` the fix is withheld. The old wording "first statement on its line" demanded the fix there. Also stated explicitly: a comment in front of the `break` on its own line moves with it (main and the branch agree, audit *Holds* for #1332). The *Why* names the opening brace. Examples: case 2 of the positive example gains `/* exit */` in front of the `break`; the negative example gains case 4, `{ {|RH7501:break|};` ⏎ `}`.
- SHARED-ORDER-007 — Initializer execution order is kept — clarified (lead decision 6). A member that code excluded by `#if`/`#elif`/`#else` declares counts like any other member: it is crossed when it lies in the crossed span (SHARED-ORDER-003), per SHARED-FIX-003 *Build configurations*. Excluded code in the moved declaration's own leading trivia travels with it (SHARED-ORDER-002) and is not crossed. RH7103 and RH7109 apply the guard, and RH7309 counts initializers the same way. RH7309-006 already says "as SHARED-ORDER-007 counts them", so the one owner of the decision now has one spec statement for all three fixes. Two examples were added, one on each side of the qualifier: the audit's confirmed input with no fix, and the same fields with the `#if` pair in front of the moved member, where the fix is offered. The spec's Uses row gains SHARED-FIX-003, which `SpecStructureTests.UsesListsExactlyTheReferencedRequirements` requires.

No new IDs. `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. `scripts/test.sh --project architecture --filter Sdd` passes (51/51).

## Examples for the tests

- RH7501-004: new negative case 4 (`{ break;` ⏎ `}`, no fix); case 2 of the positive example with `/* exit */ break;`. If an existing test mirrors the old case 2 without the comment, it still holds. The new form is an additional input, not a replacement.
- SHARED-ORDER-007: the excluded-member pair (RH7109: withheld when crossed; offered, with the exact output, when the `#if` pair leads the moved member). The test implementer should also derive the RH7103 counterpart (static fields, same shape) that decision 6 takes in.

## Output questions

_None._ RH7501: the accepted change doc decides the withheld fix for a `break` behind `{`. SHARED-ORDER-007: SHARED-FIX-003 *Build configurations* (accepted) allows one reading only.

## Internal choices

- RH7501 change doc left unchanged. Its row ("shares its line with other code") already covers the opening brace, and it is `Accepted`. Alternative: add "the block's opening brace included". I did not, to avoid touching an accepted doc for a reading it already gives.
- Change doc and SHARED-ORDER-007 / RH7103: the accepted change doc's *Affected requirements* list does not name SHARED-ORDER-007 or RH7103-007, and its RH7109 row does not list the excluded-member case. I left it unchanged. Lead decision 6 records that the *Build configurations* bullet demands it. Recommendation: the PR description states that RH7103 and RH7109 are withheld when a move crosses an initialized member in excluded code. If the lead prefers bookkeeping in the doc, append "SHARED-ORDER-007 (clarified)" to *Affected requirements*. That lists no new visible change, so the doc stays accepted.
- I put the excluded-member rule in SHARED-ORDER-007 and did not restate it in RH7103-007 or RH7109-006. Those rule specs refuse "by SHARED-ORDER-007" already. Alternative: an example per rule spec, which would restate a shared rule.

## Follow-up candidates

- RH7109-006 says "When no `#if`, `#elif`, `#else`, or `#endif` directive takes part in the refusal, the diagnostic is still reported". This leaves open whether the diagnostic is reported when a balanced `#if` pair in the crossed span holds the initializer that refuses the move. The new SHARED-ORDER-007 example states it is reported, which matches the analyzer (the diagnostic is independent of the fix). No change to RH7109 is needed in this run. A later edit may want that sentence to name the SHARED-ORDER-003 refusal it means.

## Architecture findings

_None_ beyond decision 6, which is being implemented. The spec now names one counting rule for the member-order decision.
