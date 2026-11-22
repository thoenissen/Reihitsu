# Lead — casing-fix-names

- Branch: `claude/squad-tickets-971-1072-8ywd6y` (session-designated)
- PR: thoenissen/Reihitsu#1934 (draft)
- Slug: `casing-fix-names`
- Lane: behavior change

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#971 | yes | behavior change | Leading acronyms in camelCase / `_camelCase` targets (`HTTPClient` → `_httpClient`). Filed as a bug, but triage reclassified it as an extension: SHARED-CASING-006 does not cover names that start with two capitals, so the spec changes. |
| thoenissen/Reihitsu#1072 | yes | behavior change | Casing fixes withheld when the derived name is a reserved keyword (today `Class` → `@class`); changes SHARED-CASING-007/008. |
| thoenissen/Reihitsu#1451 | yes | behavior change | RH4005/RH4121 and casing fixes keep an existing prefix (`i_document` → `IDocument`, `T_Key` → `TKey`) and every identifier character (Mn, Mc, Nl, Pc); changes SHARED-CASING-006 and the RH4005/RH4121 specs. |
| thoenissen/Reihitsu#1434 | yes | behavior change | Casing rules do not report names dictated by metadata (override/implementation of a referenced-assembly member, tuple element names it must repeat) or by a native entry point (`extern` with `DllImport`/`LibraryImport` without `EntryPoint`); new shared casing requirement. Note: the issue proposes the ID SHARED-CASING-011, which is taken today; the ID is the spec author's choice. |

All four goals are proposals (agent-filed). Roles: all of the behavior-change lane — spec author, skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if there are follow-up candidates.

## Maintainer answers

- Output batch 1 (Q1–Q12, numbering as in the Progress log): the maintainer chose the recommended option for every question — 1A, 2A, 3A, 4A, 5A, 6A, 7A, 8A, 9A, 10A, 11A, 12A.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Start | Split the batch (escalation 8)? #1434 changes reporting, the others the fix's name derivation, and #1434 names six rules. | Keep one run. | All four sit in the shared casing component: the rules all derive from `CasingAnalyzerBase`, the fixes share `CasingUtilities`, and all change `sdd/specs/shared/casing.md`. The maintainer composed the batch. | Process only. | claim |
| 2 | Implementer | Disputed test `RH4106…FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField`: the framework demands no Fix All action when Fix All changes nothing. | Test implementer adds a non-colliding field so Fix All changes something; the asserted skip of both colliding fields stays. | The asserted output (both colliding fields skipped, SHARED-CASING-010) is unchanged; only the test shape changes. Offering no Fix All action broke 7 RH4115 tests. | SHARED-CASING-010 decides the output; nothing visible changes. | 4022a0c |
| 3 | Audit 1 | Auditor finding 2 (`ToPascalCaseWord` upper-cases a non-letter first character, e.g. U+0345 → U+0399): repair or restate spec? | Repair the code to the unchanged SHARED-CASING-006 text (kept as written), with tests. | The spec already decides it; the derivation is a cheap check on the character's category. | An unchanged requirement demands the output. | 96f8123 |
| 4 | Audit 2 | Architecture finding: auditor finding 3 (RH4116 record positional parameter whose generated property implements/overrides a referenced-assembly member) repeats finding 1's class — RH4116 selects the dictated signature by a list of syntax kinds. | Change the structure in this run: RH4116 decides by cause — the symbol the compiler declares for the declaration that holds the tuple (for a record positional parameter, its generated property), asked through `CasingAnalyzerBase.IsDictatedMember` — instead of a syntax-kind list. Routed as a structural repair (test implementer, then implementer); does not use up a repair round. | Refusing (reporting) is today's behavior and contradicts SHARED-CASING-014; enumerating one more syntax kind would miss the next cell. Stays inside RH4116 and the base it already uses; small. | SHARED-CASING-014 bullet 2 decides the output; unchanged requirement. | b7651b3 |
| 5 | Audit 2 | Auditor finding 4: stale summary of `SplitIntoPascalCaseWords`. | Cleaner fixes the comment; proven with verify-text-only. | Comment only. | No compiled change. | 46d5587 |
| 6 | Audit 3 | Architecture finding: auditor finding 5 (record's compiler-generated `Deconstruct` implementing a referenced-assembly `Deconstruct`; tuple names still reported) repeats findings 1 and 3. | Structural repair by cause: the members the compiler declares from a positional parameter's type are exactly the primary constructor, the parameter's property, and the record's generated `Deconstruct`; RH4116 asks all of them (not "any generated member", which would exempt every record through `Equals(R)`). Comments claiming a complete set are corrected with it. Does not use up a repair round. | Closes the set the language defines instead of adding a cell; keeping the case reported would contradict SHARED-CASING-014 and need an output question for a rare shape. | SHARED-CASING-014 bullet 2 decides the output; unchanged requirement. | e1c71ec |
| 7 | Audit 3 | Auditor finding 6: the positional-parameter → generated-property mapping exists twice (RH4116 and `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync`). | One shared helper in `Reihitsu.Analyzer` that both use (CodeFixes reach it via `InternalsVisibleTo`); finding 5's repair extends it. Repair round 3. | One rule in one place, within components the run changes; small. | Internal structure; no output changes. | e1c71ec |
| 8 | Audit 3 | Auditor finding 7: no RH4116 tests for indexer parameters and events with accessors on the new code path (both sides). | Test implementer adds them. Repair round 3. | Missing coverage. | Tests only. | a7305e2 |
| 9 | Round 3 spec check | Spec checker finding: new `RecordPositionalParameterUtilities` is in no code-owner list. | Spec author decides listing (precedent: `CasingRenameSafetyUtilities`, decision in `09-spec-author.md`); handled inside repair round 3's worklist, not a new round. | Docs only, same round. | Spec metadata only. | f143857 |

## Follow-up candidates

- RH4120 (note from `18-implementer.md`: the shared helper `RecordPositionalParameterUtilities.GetDeclaredProperty` (added in this run) is what RH4120 would reuse): a record's positional parameter whose generated property implicitly implements a property declared in a referenced assembly has a dictated name but is still reported (`01-spec-author.md`, #1434).

## Worklist / next step

- Claim done (empty commit, draft PR #1934), SDK installed.
- Next: spec author (change doc + spec diff for all four issues).

## Progress log

- Spec author done (`8939ef7`, `a351dd2`): change docs written for all four; nine output questions (handover `01-spec-author.md`, *Whole run*). New IDs SHARED-CASING-013 (keyword `@`) and SHARED-CASING-014 (dictated names).
- Follow-up candidate: RH4120 positional record parameter whose generated property implicitly implements a referenced-assembly property is still reported (from `01-spec-author.md`).
- Skeptic done (`ff32292`): #971 output question, #1072 proceed, #1451 proceed, #1434 two output questions. No architecture findings.
- Output batch 1 posted to the maintainer: Q1–Q9 from `01-spec-author.md` (Q1 with skeptic's `URLs` row), Q10 = skeptic #971 Q1 (first-word vs joined acronym run), Q11 = skeptic #1434 Q1 (base-class implementer), Q12 = skeptic #1434 Q2 (tuple names from a type argument).
- Answers received (all A). Spec author follow-up done (`f1a2cff`, `03-spec-author.md`): answers recorded, change docs Accepted, SHARED-CASING-006/014 amended for Q10–Q12.
- Test implementer done (`e33eea6`, `d76fdcb`, `04-test-implementer.md`): 58 analyzer + 39 core tests fail on unchanged code, exactly the new/rewritten ones; architecture green.
- Implementer done (`bc565c2`, `05-implementer.md`): all tests pass but one disputed (decision 2). Full suites green otherwise; self-hosting scan not run.
- Test repair done (`4022a0c`, `06-test-implementer.md`): disputed test passes.
- Cleaner done (`3acd20e`): clean, 0 warnings.
- Spec checker done (`2d321c1`, `08-spec-checker.md`): one low finding — `CasingRenameSafetyUtilities` missing from the code-owner lists in `sdd/specs/shared/casing.md` and `sdd/specs/README.md`. Repair round 1: spec author.
- Spec author repair done (`4da9fe1`, `4e658ab`, `09-spec-author.md`): type listed as SHARED-CASING owner.
- Spec checker repair check done (`8266151`, `10-spec-checker.md`): consistent.
- origin/main 75cc39f, branch not behind; build green.
- Auditor 1 done (`b0074e4`, `11-auditor.md`), audited HEAD 396887f: #971 pass, #1072 pass, #1451 finding 2 (low), #1434 finding 1 (RH4116 operators / conversion operators / multi-declarator events). Both same requirement.
- Repair round 2 (round 1 was the spec-checker code-owner repair): test implementer (tests for findings 1 and 2), then implementer, cleaner, spec checker, auditor on the repair, validator.
- Round 2 test implementer done (`d90597e`, `ff266a7`, `12-test-implementer.md`): 5 RH4116 + 12 core + 2 code-fix tests fail on current code. Next: implementer.
- Round 2 implementer done (`96f8123`, `13-implementer.md`): all green in Debug. Note for the run note: the implementer used `git stash` (no parallel role was running). Next: cleaner, spec checker, auditor on the repair.
- Round 2 cleaner done (`9a22054`): clean. Next: spec checker, then auditor on the repair.
- Round 2 spec checker done (`73fc031`): consistent. Next: auditor on the repair (396887f..HEAD).
- Audit 2 done (`f1f2644`, `16-auditor.md`, audited c94249b): findings 1 and 2 closed; finding 3 (high, repeats 1 → architecture finding, decision 4); finding 4 (comment, decision 5). Hint (not a finding): explicit interface specifier repeating a tuple type argument reports names twice — pre-existing, not taken.
- Next: structural repair — test implementer (tests for finding 3, both sides), implementer, cleaner (+ finding 4 comment), spec checker, auditor on the repair, validator.
- Structural repair: test implementer done (`a2f386b`, `17-test-implementer.md`): 6 failing, 1 passing as intended. Next: implementer (decision 4).
- Structural repair implementer done (`b7651b3`, `18-implementer.md`): RH4116 decides by declared symbol; default set green in Debug. Next: cleaner (+ decision 5 comment), spec checker, auditor on the repair.
- Structural repair cleaner done (`46d5587`): comment fixed, text-only proof PASS. Next: spec checker, then auditor on c94249b..HEAD.
- Structural repair spec checker done (`c8b4355`): consistent. Build green. Next: auditor on c94249b..HEAD.
- Audit 3 done (`95766b1`, `21-auditor.md`, audited a85f8e0): findings 3, 4 closed; finding 5 (architecture, decision 6), 6 (decision 7), 7 (decision 8).
- Repair round 3 (third and last) + structural repair 2: test implementer (finding 5 both sides, finding 7), implementer (decisions 6, 7), cleaner, spec checker, auditor on the repair, validator. A block after this round is resolved by narrowing.
- Round 3 test implementer done (`a7305e2`, `22-test-implementer.md`): 3 failing (finding 5), 7 passing guards. Next: implementer (decisions 6, 7).
- Round 3 implementer done (`e1c71ec`, `23-implementer.md`): new internal `RecordPositionalParameterUtilities`; default set green in Debug. Next: cleaner, spec checker, auditor on 8a39005^..HEAD (a85f8e0..HEAD).
- Round 3 cleaner done (`0a77261`): clean. Next: spec checker.
- Round 3 spec checker (`323123c`, `25-spec-checker.md`): one low finding (decision 9). Next: spec author, spec checker, auditor on a85f8e0..HEAD.
- Round 3 spec author done (`f143857`): type listed as SHARED-CASING owner. Next: spec checker, auditor.
- Round 3 spec checker re-check done (`033d24b`): consistent. Build green. Next: auditor on a85f8e0..HEAD.
- Audit 4 done (`c487ffd`, `28-auditor.md`): PASS for all four issues. Audited SHA e63741f8e577c9029fa0e188ccaaaddbff0c5d46. Next: validator, then publish (lane check, issue manager for RH4120, note, delete sdd/work, trigger commit, PR body).
- Issue manager done (`be626f6`): follow-up thoenissen/Reihitsu#1939 (RH4120). Waiting on validator.
