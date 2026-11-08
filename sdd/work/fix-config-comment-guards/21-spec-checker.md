Role: spec checker
Commit: ba02f25
Verdict: consistent

The prompt carried no intended fix, suspected cause, or verdict; none was used. Inputs: `00-lead.md` (decisions 21 to 23), `14-spec-checker.md`, `15`-`20` handovers, and `git diff 3b3f696..HEAD` (structural change: `ConditionalGroup.cs`, `OrderingDeclarationUtilities.cs`, `ordering-move-safety.md`, three analyzer test classes). Checks on the unchanged parts keep their results in `08-spec-checker.md` and `14-spec-checker.md`.

## thoenissen/Reihitsu#1595

### Findings
_None._

### Checked
- Check 1: no change doc needed (decision 19 stands; the structural change withholds nothing new for RH7102 and restores main's fixes).
- Checks 2 to 8: RH7102 spec and tests untouched in this range; RH7102 reaches the guard through `ChangesMemberOrder`, whose behavior SHARED-ORDER-007 states. Results of 14 stand.

## thoenissen/Reihitsu#1593

### Findings
_None._

### Checked
- Check 1: no change doc. SHARED-ORDER-007's per-configuration wording and the `const` example (decision 22) are a clarification of unchanged requirements; the code only withholds where SHARED-FIX-003 demands and restores main's fixes where audit 2 found over-withholding.
- Check 2: the split-member paragraph (runtime initializer from a non-`const` configuration, static/instance context, instance field, `const` contributes nothing) matches `TryAddOrderedMembersOfRegion`'s reading as `17-implementer.md` describes it. The new `K` example is `FixIsOfferedWhenACrossedDeclarationIsAStaticReadonlyFieldInOneBranchAndAConstantInTheOther` and its counterpart `NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantInTheActiveBranch`. Format holds (normative sentence, `Why:` extended, markup `{|RH7109:_b|}`). Observation, not a finding: the spec does not state the bracket rule, the 256-configuration cap, or the bound for groups the body does not open or close. They only keep main's fixes or withhold as a safety bound (decision 23), so no requirement is contradicted; the cap test is a characterization test that names no requirement.
- Check 3: no new ID; `requirement-ids.txt` untouched in the range.
- Check 4: code owners of `ordering-move-safety.md` (`OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase`) exist; the new internal `ConditionalGroup` is a helper, not an owner. `sdd/specs/README.md` row matches.
- Check 5: no rule page affected.
- Check 6: no output question; the follow-up candidates (accessor-block `field` keyword case) are in `00-lead.md`.
- Check 7: no new requirement. `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`.
- Check 8: the 18 structural tests of `16-test-implementer.md` and the 7 tagged pins of `19-test-implementer.md` carry `SHARED-ORDER-007` plus `RH7109-006`, `RH7103-007`, or `RH7309-006` as the tables list. The over-cap test is untagged on purpose (verifies no requirement), not a finding. The spec's `K` example is tagged on a test the diff adds.

## thoenissen/Reihitsu#1616

### Findings
_None._

### Checked
- Checks 1 to 8: nothing in the range touches RH2001, its change doc, tests, or rule page; results of 14 stand.

## Whole run

- Check 9: `sdd/work/` holds only `fix-config-comment-guards`; the lead deletes it before publishing.
- No findings for any role.
