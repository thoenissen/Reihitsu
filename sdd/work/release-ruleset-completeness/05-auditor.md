Role: auditor
Commit: 0374545c66b8f93fcb9e1653983861552cca832c
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `0374545c`, the checkout. Base `origin/main` = `725ea1511410af028a950d1829eb02289f9b1787`, which is also the merge base, so the branch is not behind. The diff matches the handovers: `Release.ruleset` gains 14 entries (RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7308–RH7311, RH7409–RH7412, all `Action="Error"`), `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.cs` changes, and `sdd/work/release-ruleset-completeness/` holds the handovers. `scripts/lane-check.sh tooling`: confirmed.

Requirements of thoenissen/Reihitsu#1890, together with the triage comment that `00-lead.md` summarizes:
- Every shipped rule is listed. Holds. The Reihitsu block of `Release.ruleset` has 351 IDs, sorted, with no duplicates. Every rule file under `src/Reihitsu.Analyzer/Rules/` has an entry, except RH7305A, which is on the exemption list (lead decision 1).
- The entries are "in ID order". Holds: `sort -c` passes over the block.
- A test fails, naming the rule, when a shipped rule has no entry and is not exempt. Holds: `ReleaseRulesetListsEveryShippedRule`. Its message lists the missing IDs, and the test implementer recorded the red run, which named all 14 rules.
- The exemption list is explicit and commented. Holds: `_rulesWithoutReleaseEntry` has an XML summary.

The file keeps its BOM and LF line endings. I trust the cleaner's Release build result (0 warnings, 0 errors) and the focused `RulesetTests` result (24 of 24 passed).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.cs:71-72` (summary of `ReleaseRulesetListsEveryShippedRule`), also `:114-115` (summary of `ReleaseRulesetReportsRuleAsError`) | FC-031 (a comment that states a rationale the code does not have) | – | PR-introduced | The summary says the test exists "so no new rule is silently left out of the repository's own Release build". That is false for every rule enabled by default, which is every shipped rule except RH7305A. A rule with no ruleset entry keeps its default severity, `Warning`, and `TreatWarningsAsErrors` (`Directory.Build.props:48`, Release) turns it into an error. I checked this with a scratch project outside the repository that loads `binaries/*.dll` with `origin/main`'s `Release.ruleset` (no RH5417 entry), `TreatWarningsAsErrors=true`, and the code `public int GetValue() { return 1; }`. The build reports `error RH5417`; with `TreatWarningsAsErrors=false` it reports `warning RH5417`. No `.editorconfig`, `.globalconfig`, `NoWarn`, or `WarningsNotAsErrors` changes the default. The other summary says the ruleset "turns on as errors … the shipped rules it once had no entry for", which suggests these rules were off before. They were not. | Reword both summaries to state what the test actually protects: every shipped rule's Release action is written out explicitly, independent of `TreatWarningsAsErrors`, and a rule that ships disabled by default stays off in Release unless it is listed. Text only. |
| 2 | low | PR description / `00-lead.md` (no file in the tree) | – (issue intake, `AGENTS.md`: "Say so when a report turns out to be wrong, overstated") | – | same requirement | The premise of thoenissen/Reihitsu#1890 is wrong. The issue says a rule missing from the ruleset "is not reported as an error by the repository's own Release build … and nothing notices". The construction in finding 1 shows that on `main` the 14 missing rules were already reported as errors in Release, through `TreatWarningsAsErrors`. The change makes their action explicit; it does not turn them on. Neither `00-lead.md` nor any handover records this, so the PR would repeat the issue's premise. | The PR states how the result differs from the issue: the 14 rules were already errors in the Release build through `TreatWarningsAsErrors`; the change makes their actions explicit, and the new test protects against a rule that ships disabled by default, or a Release build without `TreatWarningsAsErrors`, leaving a rule off silently. The issue's goals (every rule listed, a test for the missing direction) are still met, so whether to close the issue or only refer to it is the lead's decision. |

Why `pass, text cleanup`: finding 1 is confined to an XML documentation comment in a test. `scripts/verify-text-only.sh` would prove the rewording text-only, and it cannot reach compiled behavior, diagnostics, or rule metadata. Finding 2 concerns the PR description and the run record only.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A shipped rule is missing from Release (`ReleaseRulesetListsEveryShippedRule`) | All IDs in `SupportedDiagnostics` of every concrete `[DiagnosticAnalyzer]` type in `Reihitsu.Analyzer` (`AnalyzerCatalog.DiscoverAnalyzerTypes`, via `SelectMany`, so analyzers with several descriptors are covered), checked against the `Rule` elements under `Rules[@AnalyzerId="Reihitsu.Analyzer"]` of `Release.ruleset`, minus `_rulesWithoutReleaseEntry` (ordinal) | `ReleaseRulesetExemptionsNameShippedRulesWithoutEntry`: an exemption is stale if it is not shipped or if it has an entry | Complementary. An exempt ID is accepted by the first test only while the second confirms that it is shipped and has no entry. Both read the same span through the same helpers. No other assembly declares `[DiagnosticAnalyzer]` (rg) | ok |
| An entry names a shipped rule (`EveryReihitsuEntryNamesAShippedRule`, discovery moved into `GetShippedRuleIds`) | Same discovery; the projection is byte-identical to the removed inline copy | The new missing-direction test | The same set in both directions | ok |

FC-003, both sides of the exemption boundary: an exempt ID that has no entry passes, an exempt ID that has an entry fails, an exempt ID that is not shipped fails, and a shipped ID that is neither listed nor exempt fails. The tests read the real files, so only the current state is executed. The red run of the test implementer shows the failing side of the missing-direction test.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Action of each Reihitsu rule in the Release build | `Release.ruleset` only. It is applied by `Directory.Build.props:47`. `scripts/{apply-fix,proof,benchmark,lane-check,trace}/Directory.Build.props` opt out on purpose. No `.globalconfig` exists, and `.editorconfig` sets no `RH` severity | one owner |
| Set of shipped rule IDs (test side) | `RulesetTests.GetShippedRuleIds` (`Reihitsu.SelfHosting.Test`; it replaces the old inline copy in the same file); `AnalyzerMetadataDiscovery` (`src/Reihitsu.Analyzer.Test/SelfHosting/Utilities/AnalyzerMetadataDiscovery.cs:226`) is in a different test assembly and serves per-analyzer metadata | The diff removes a copy and adds none; the other discovery predates the run and has another purpose. ok |
| Release action of RH7305A | Exemption field in `RulesetTests`; `Debug.ruleset:98` lists it as `None`; the analyzer constructor passes `isEnabledByDefault: false` | Consistent: off in both configurations. ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReleaseRulesetListsEveryShippedRule` | Every shipped ID is listed or exempt | Delete any entry: the test fails and names the ID (shown red with 14 IDs at `403719dd`); the empty-set guard catches discovery that finds nothing | `GetShippedRuleIds`, `LoadReihitsuEntries` | Adequate for what the issue asks: presence of an entry. It accepts any action, so a new rule added as `None` passes (see Hints) |
| `ReleaseRulesetExemptionsNameShippedRulesWithoutEntry` | Every exemption is shipped and unlisted | Add `RH7305A` to Release, or rename the exemption: the test fails and names the ID | same | adequate |
| `ReleaseRulesetReportsRuleAsError` (renamed, 14 rows added) | Each named row is exactly `Error` | Any other action, or a missing entry (`GetValueOrDefault` gives `null`): the test fails | `LoadReihitsuEntries` | Adequate: an exact-equality assertion. The 14 rows are a fixed list that the completeness test does not need, but they do no harm |
| `EveryReihitsuEntryNamesAShippedRule` (refactor only) | Unchanged | Unchanged | `GetShippedRuleIds` | Adequate; same projection as before |

No test of a visible surface changed, so check 10 (visible output is decided) has nothing to cover. The rulesets configure only this repository's own build, and no shipped package carries them.

## Failure classes dismissed
- FC-001, FC-002, FC-003: traced above under *Guard scope*. None fails.
- FC-015: traced above under *Policy ownership*. One owner.
- FC-016 (sibling shapes): `Debug.ruleset` is the only other ruleset (`find -name '*.ruleset'`). It lists Reihitsu rules only to switch them off, so a missing entry there is intended, and the test implementer's handover keeps it out of scope.
- FC-017: covered in the table above.
- FC-031: finding 1. All other changed summaries (`GetShippedRuleIds`, `_rulesWithoutReleaseEntry`, `ReleaseRulesetExemptionsNameShippedRulesWithoutEntry`) describe their code.
- FC-032: the Release build uses `binaries/Reihitsu.Analyzer.dll`, while the tests read the source assembly. An entry for a rule the binaries lack yet is ignored by the compiler. The implementer confirmed that the binaries contain all 14 IDs, and my experiment ran the binaries' RH5417.
- FC-035: the expected IDs come from the analyzer assembly at run time, not from the patched ruleset. The 14 DataRows match the triage list and the red run.
- FC-004 to FC-014, FC-018 to FC-030, FC-033, FC-034, FC-036 to FC-044, and the class matrices: they concern formatter phases, code fixes, rewrites, trivia, reproductions of formatter or analyzer behavior, and unsupported shapes. The diff changes none of these. It touches an MSBuild ruleset file and a self-hosting test that reads XML and reflects over analyzer types.
- FC-037: no comment-only change is claimed.

Code quality: no `TODO`, no tracker number in source (rg). Naming and XML documentation follow the file's conventions. Changes outside `sdd/work/` stay within the issue's two named files. The implementer's role file forbids a ruleset change, but that rule is about resolving warnings; here the ruleset is the subject of the work, and the 14 entries cannot remove a warning, only add one. No conflict.

## Architecture findings
_None._

## Hints
- `ReleaseRulesetListsEveryShippedRule` checks only presence. A future rule added to `Release.ruleset` as `Action="None"` or `"Warning"` passes it, and is then off or not an error. The issue asks only for presence, and an explicit `None` is visible in review, so this is not a finding. A test that requires `Error` for every listed ID except a commented list (`RH8402`) would close it, if the lead wants that.
- The issue body says "Lane: tests". The diff touches `Release.ruleset`, a tooling path, so the lead's tooling claim is the correct one, and the lane check confirms it.

## Follow-up candidates
_None._
