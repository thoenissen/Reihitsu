Role: test implementer
Commit: 403719dd
Verdict: reproduced

The issue is tooling, not a code defect: the "reproduction" is the missing-direction test the issue asks for, run red on this head. No intended fix, suspected cause, or hoped-for verdict was in my prompt.

## Tests

All in `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.cs`. None carries a `[Requirement]` tag, because no spec covers the rulesets (tooling).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReleaseRulesetListsEveryShippedRule` (new) | issue #1890 "Done when"; the gap that `EveryReihitsuEntryNamesAShippedRule` leaves | `AnalyzerCatalog` + `LoadReihitsuEntries` | red: "Release.ruleset has no entry for these shipped rules: RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7308, RH7309, RH7310, RH7311, RH7409, RH7410, RH7411, RH7412" |
| `ReleaseRulesetExemptionsNameShippedRulesWithoutEntry` (new) | FC-003: the other side of the exemption condition, so a stale exemption cannot hide anything | same | green |
| `ReleaseRulesetReportsRuleAsError` (renamed from `ReleaseRulesetReportsLineCountRuleAsError`, 14 rows added) | the Release ruleset turns the rules it enables on as `Error` (the issue prescribes `Action="Error"`) | same | red for the 14 new rows; green for RH2008 and RH2111 |
| `EveryReihitsuEntryNamesAShippedRule` (unchanged assertion; shipped-ID discovery moved into `GetShippedRuleIds`) | the existing direction | same | green |

Run result on this head: 15 failed (1 + 14 rows), 9 passed, 24 total.

Why the rename: as a separate method, a test for the 14 rules had the same body as the line-count test, and the build failed with S4144 (identical methods). I merged the rows into the existing method and widened its summary. No other file references the old name.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Shipped rule IDs (from the analyzer assembly) minus `Release.ruleset` entries, on current head (725ea15 + run commits) | n/a | self-hosting test | reproduced | 14 rules missing, exactly the list from the maintainer's triage comment; RH5115 and RH5116 (from #892) are already listed. Expected: none missing |

## Exemption list (a decision for the lead)

`_rulesWithoutReleaseEntry = ["RH7305A"]`. RH7305A is the only other shipped ID that `Release.ruleset` does not list. It is the alternative rule to RH7305, which Release enables as `Error`. It is constructed with `isEnabledByDefault: false`, and `Debug.ruleset` lists it as `None`. The triage list leaves it out, so the test treats it as deliberately out. That makes `ReleaseRulesetListsEveryShippedRule` pass once the 14 entries are added, and nothing in the rule's behavior changes. The other choice is to list it in Release as `None` and empty the exemption list. If the lead prefers that, the implementer changes the array to empty, and `ReleaseRulesetExemptionsNameShippedRulesWithoutEntry` then has nothing to check. That case can be left as it is or removed with the list.

## Not covered, and why

- `Debug.ruleset` completeness: it lists Reihitsu rules only to switch them off, so missing entries are intended. Out of scope.
- The ID order of entries in `Release.ruleset` (the issue says "in ID order"): it is not asserted, because no existing test checks it and it is not user-visible. The implementer should still insert the entries in order.
- Failure classes: the catalog covers formatter, code-fix, trivia, and rewrite behavior, and none of that can reach a ruleset file. FC-003 is covered by the exemption test. FC-017 is covered because the action is asserted as exactly `Error`, not only as present. FC-035 does not apply: the expected IDs come from the analyzer assembly and the triage comment, not from a patched ruleset.

## Commands

```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Repository.RulesetTests"
```

## Follow-up candidates

_None._
