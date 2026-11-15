Role: spec checker
Commit: 343c1bb
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/rh5102-arguments-on-one-line.md:5` | The header says `Status \| Draft`, but `00-lead.md` decision 3 (b969706) records the change doc as Accepted, and all its visible output changes have the maintainer's answer under `Decisions`. `sdd/README.md` has the lead set `Accepted` before the PR is marked ready for review. Set it before the PR is marked ready; no other change is needed. | spec author (the lead may set the field itself) |

## Checked
1. Change doc exactly when required: finding 1. Everything else is ok. The doc is required because RH5102-001's behavior changed. It has no task list, has `Visible output changes`, and its `Decisions` section records answer A for every entry.
2. Specs match the diff: ok. RH5102-001 states the arguments-only reading the analyzer and `SyntaxNodeUtilities.IsPartlyWrapped` implement. FMT-BREAK-012's Why is reworded only. The shared helper changes no RH5109 behavior, and RH5109-001 is unchanged. The format holds: normative sentence first, `Why:`, and examples with `{|RH5102:…|}` markup.
3. IDs: ok. No new ID, `requirement-ids.txt` is untouched, and RH5102-001 is changed in place with no ID retired.
4. Code owners exist: ok. No header owner changed. The `SpecConsistencyTests` ran inside the architecture project (see 7). The new `IsPartlyWrapped` sits in the existing `SyntaxNodeUtilities`, which no spec header names.
5. User documentation: ok. `documentation/rules/RH5102.md` is unchanged and states the rule by its arguments, so it does not contradict the spec. The fix output is unchanged and RH5102-003 holds. The category range RH5 fits Layout.
6. Open questions routed: ok. The spec author's entries 2 to 4 are decision 2, the skeptic's follow-ups are decision 6, and the declaration option is decision 7. `Follow-up candidates` is empty, which fits those decisions. Nothing is written into a spec as an open question.
7. Requirements covered and specs consistent: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. RH5102-001 is covered by many `[Requirement]` tests in the analyzer, formatter, and Core test classes.
8. Test tags: ok. Every test the diff adds carries the `[Requirement]` tags its `Tests` table names (RH5102-001, -003, -004, RH5109-001, SHARED-DIAG-010). Every requirement named under `Examples for the tests` is tagged on at least one added test.
9. Nothing left behind: ok. `sdd/work/` holds only `rh5102-wrap-behind-paren`, which the lead deletes before publishing.
