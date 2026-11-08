Role: spec checker
Commit: 04e5d25
Verdict: consistent

The prompt carried no intended fix, suspected cause, or verdict; none was used. Inputs: `00-lead.md` (decisions 24 to 26), `21-spec-checker.md`, `22` to `27` handovers, and `git diff 45c37c5..HEAD` (`ordering-move-safety.md`, `OrderingDeclarationUtilities.cs`, `ConditionalGroup.cs`, `OrderedMember.cs`, the RH7109 test class). Checks on the unchanged parts keep their results in `08`, `14`, and `21`.

## thoenissen/Reihitsu#1595

### Findings
_None._

### Checked
- Checks 1 to 8: no RH7102 file changed in the range; it reaches the guard through the shared owner. Results of 21 stand.

## thoenissen/Reihitsu#1593

### Findings
_None._

### Checked
- Check 1: no change doc. The spec edit narrows the guard to what SHARED-FIX-003 (*Build configurations*) already demands and records the maintainer's standing answer for the cap (decision 25); no requirement is added, changed in intent, or retired.
- Check 2: SHARED-ORDER-007 states per-configuration judgment, no join by name, the 256-configuration limit scoped to touched groups (matches `MaximumConfigurationCount = 256`), and the rewritten split-member paragraph (matches decisions 24 to 26 and `24-implementer.md`). Format holds: `Why:` extended, markup `{|RH7109:_c|}`. The two new examples (split `_b`, far9 with over-cap counterpart) have tests. The expression-body withholding of `24-implementer.md` is a case of the "member the move crosses" rule, covered by the spec text; its test is tagged.
- Check 3: no new ID; `requirement-ids.txt` untouched in the range.
- Check 4: owners of `ordering-move-safety.md` exist; `ConditionalGroup` is an internal helper, not an owner; `sdd/specs/README.md` unchanged and matching.
- Check 5: no rule page affected.
- Check 6: no output question open; decision 25 is covered by the standing answer. The blank-line follow-up of `26-test-implementer.md` is the lead's to file under `Follow-up candidates`; it is not written into a spec.
- Check 7: no new requirement. `scripts/test.sh --project architecture` -> `Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115`.
- Check 8: the six tests of `26-test-implementer.md` (four new, two re-tagged) each carry `SHARED-ORDER-007` and `RH7109-006`, as its table lists. Both new spec examples are tagged on a test the diff adds or newly tags.

## thoenissen/Reihitsu#1616

### Findings
_None._

### Checked
- Checks 1 to 8: nothing in the range touches RH2001; results of 21 stand.

## Whole run

- Check 9: `sdd/work/` holds only `fix-config-comment-guards`; the lead deletes it before publishing.
- No findings for any role.
