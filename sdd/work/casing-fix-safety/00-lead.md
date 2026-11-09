# Lead — casing-fix-safety

- Issues (prompt order): thoenissen/Reihitsu#1597, #1598, #1016, #1015, #1084 — all labelled `agent-filed` (goals are proposals).
- Slug: `casing-fix-safety`
- Branch: `claude/relaxed-franklin-o3mkrk` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1675
- Lane: **bug fix** (all five)
  - #1597 — observed vs expected: casing fixes rebind `foreach`/query clauses or leave a `[CallerArgumentExpression]` literal behind (SHARED-FIX-003).
  - #1598 — suspected (unconfirmed) excluded-code miss in a multi-targeted project (SHARED-FIX-003); must be reproduced first.
  - #1016 — casing conversion produces names the same rule still reports (SHARED-FIX-001).
  - #1015 — RH4102 Fix All corrects only one event kind (SHARED-FIX-002).
  - #1084 — after the triage split (comment on the issue), only the bug part remains: an interface named `_` becomes `I_`, still reported; the fix must be withheld where no usable name remains (SHARED-CASING-008, SHARED-FIX-001). Keeping an existing prefix and every identifier character moved to #1451 and is out of this run.
- Roles: skeptic → (spec author only for a clarification) → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist.
- Maintainer clarifications: #1598 triage comment — unconfirmed, reproduce with a test before building anything. #1084 triage comment — scope narrowed as above.

## Maintainer answers

- Escalation 4 (#1598 not reproduced): the run ships only the characterization tests for #1598 and the PR closes the issue with them (`Closes #1598`). The maintainer considered a general rule that analyzers and fixes consider only active code; the lead explained the existing cheap disabled-text guard; the maintainer asked for nothing further in this run, so no follow-up for the general rule.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/relaxed-franklin-o3mkrk` instead of `claude/batch-1597-<slug>` | The session environment only permits pushing to this branch | Branch name is internal | claim commit |
| 2 | Skeptic | #1597 goal "rename the `CallerArgumentExpression` literal" | Dropped; the fix is withheld instead | SHARED-FIX-003 forbids changing a user literal; the issue already allows withholding | Withholding is the issue's own required output; rare class | — |
| 3 | Skeptic | #1597 refusal shape | Cheap refusal: withhold RH4103 when the new name is a compiler pattern-lookup name; withhold a parameter rename when a `CallerArgumentExpression` string names the old or new name. No binding comparison | Binding comparison is machinery for a rare class (AGENTS.md, Special cases) | Withholding a fix for a rare class | — |
| 4 | Skeptic | Architecture finding (#1016, #1084): acceptability of the new name decided in several places, none asks the rule's style check | Change the structure in this run: one check in the casing base withholds the fix when the rule would still report the new name | Narrow per-converter fixes would add further copies of the rule; stays within the casing base and its converters | Brings the code back to SHARED-FIX-001 / SHARED-CASING-008; withholding only | — |
| 5 | Skeptic | Spec clarifications | Spawn the spec author for SHARED-CASING-007 (#1597) and SHARED-CASING-008 (#1016) clarifications | Specs currently contradict SHARED-FIX-003 / leave the inputs out | Clarifications follow existing SHARED-FIX requirements | 5fed711 |
| 6 | Spec author | Change doc for SHARED-CASING-011 (withholds fixes `main` offers, e.g. a lone `getEnumerator`) | No change doc; bug-fix lane holds | Withholding a fix for a rare class (lower-case pattern-method names already violate RH4103) | Withholding for a rare class puts nothing in question (README, What reaches the maintainer) | 5fed711 |
| 7 | Spec author | Unbuilt list members of SHARED-CASING-011 | Keep the list as written; test implementer covers representative members | Same mechanism; cheap fixed list | Withholding only | 5fed711 |
| 8 | Implementer | Ten new Fix All tests fail only because the test library's default `MarkupHandling` drops the expected RH4xxx markup | Test implementer sets `MarkupMode.Allow` on the fixed states (as existing RH4103/RH4107 tests do); no assertion changes | Harness configuration, not behavior | Asserted outputs unchanged | — |
| 9 | Implementer | Older test `FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees` expects `select` → `Select`, which SHARED-CASING-011 now withholds | Test implementer keeps its SHARED-CASING-009 purpose with a method name outside the SHARED-CASING-011 list; the withheld `select` case is already covered by the new tests | Decisions 3 and 6 accepted withholding for the rare class | Withholding for a rare class | — |
| 10 | Implementer | Empty `RH4102CustomEventNameCasingCodeFixProvider` kept only for two test helpers (S2094) | Test implementer switches the helpers to `RH4102EventNameCasingCodeFixProvider`; implementer then deletes the empty type | Dead type; one provider now covers both event kinds | Internal type | — |
| 11 | Implementer | SHARED-CASING-012 check also withholds a positional record property rename that cascades to a primary-constructor parameter named in `CallerArgumentExpression` | Accept; test implementer adds one test locking it | Within SHARED-FIX-003 Run time; rare class | Withholding for a rare class | — |
| 12 | Test implementer | The three `NoFixIsOfferedWhenTheNewNameMakesAQueryOf…Ambiguous` tests now pass through SHARED-CASING-011 alone; SHARED-CASING-009's ambiguity path has no test with a name outside the list, because the renamer rewrites call sites (follow-up candidate) | Accept | Covering it needs the follow-up's behavior first | Tests only | 05f7b33 |
| 13 | Auditor (round 1) | Finding 1: SHARED-CASING-012 guard misses `nameof` of another symbol with the old/new name, and C# 14 extension-block receivers | Repair round 1: refuse the class with a cheap check — owners re-derive (spec author narrows SHARED-CASING-012, test implementer adds one test per case, implementer widens the guard) | Same requirement; rare class → wider refusal, no binding analysis | Withholding for a rare class | — |
| 14 | Auditor (round 1) | Finding 2: vacuous linked-document assertion in the #1598 test helper | Repair round 1: test implementer makes it check that the copies are linked | PR-introduced test weakness | Tests only | — |
| 15 | Auditor (round 1) | Hint: SHARED-CASING-011 also withholds `add`, `where`, `dispose` in Java-style code | Keep decision 6 | The input already violates RH4103 itself; withholding only removes a fix for those exact 21 names | Withholding for a rare class | — |
| 16 | Repair 1 spec | Spec author's departures: C# 11 condition on the `nameof` exemption; no exemption for extension-block members | Accept both | Conservative cheap refusal of a rare class; without the C# 11 condition the finding returns under C# 10 | Withholding for a rare class | 2499cbe |
| 17 | Repair 1 tests | Example (d): test implementer observed that under C# 10 the renamer renames the `nameof` operand and behavior is kept, contradicting the spec's *Why* | Keep the C# 11 condition (conservative, cheap language-version check; evidence between the two roles conflicts); spec author rewords the *Why* so it states a conservative refusal instead of a binding claim | A conservative refusal of a rare class is always allowed; a spec must not assert an unverified compiler fact | Withholding for a rare class | — |

## Follow-up candidates

- Renamer rewrites call sites to keep binding (`values.Select(...)` → `Enumerable.Select(values, ...)`, `box.Add(1)` → `box.Add((object)1)`), conflicting with SHARED-FIX-006; carries an output question (01-skeptic.md).

## Worklist / next step

- Done: skeptic (01-skeptic.md, eb79b88). No output questions.
- Done: spec author (02-spec-author.md, 5fed711). No output questions. Lane check: confirmed.
- Done: test implementer (03-test-implementer.md, 1c84020). #1597, #1016, #1015, #1084 reproduced (74 red tests); #1598 NOT reproduced (5 green characterization tests in RH4109 tests).
- #1598 answered: tests only, PR uses `Closes #1598`; #1598 skips the remaining build roles (spec checker and validator still cover its tests).
- Done: implementer (04-implementer.md, 87ce0ad): 11 failing tests disputed; decisions 8–11.
- Done: test implementer repair (05-test-implementer.md, 05f7b33): analyzer 7842/0, architecture 117/0 (Debug). Release breaks on RH7110 in CasingAnalyzerBase (cleaner).
- Done: implementer deleted empty provider (06-implementer.md, e5dffc1); architecture test EveryCodeOwnerNamesAnExistingType red until spec rows removed.
- Done: spec author removed owner rows (07-spec-author.md, 457db3f); architecture 117/117.
- Done: cleaner (08-cleaner.md, 5290043): Release 0 warnings.
- Done: spec checker (09-spec-checker.md, 3d0271b): consistent, no findings.
- Done: auditor (10-auditor.md, 92ee46f), audited head 5a1e702 vs main b673099: findings 1 (high), 2 (low).
- Repair round 1 worklist: (1) DONE spec author (11-spec-author.md, 2499cbe); (2) DONE test implementer (12-test-implementer.md, 97b1643; 5 red); (3) DONE implementer (13-implementer.md, ba93a6f; analyzer 7849/0); (3b) DONE spec author (14-spec-author.md, 776c97b); cleaner DONE (15-cleaner.md, f4d6c6b); spec checker DONE (16-spec-checker.md, 4b371ec); next: auditor (repair only), validator. then cleaner, spec checker, auditor (repair only), validator.
- (old) implementer for #1597, #1016, #1015, #1084 (note: merging the two RH4102 providers would require changing existing tests that name `RH4102CustomEventNameCasingCodeFixProvider`).
