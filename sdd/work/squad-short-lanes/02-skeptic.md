Role: skeptic
Commit: 01046ba
Verdict: proceed

The prompt carried no intended fix or verdict; it set the scope (the maintainer accepted the five rules, so the question is whether the text and the specified lane-check behavior carry them out). I took the maintainer's triage comment on thoenissen/Reihitsu#1906 as `00-lead.md` summarizes it.

Findings below are ordered by weight. F1–F4 are amendments the spec author can make before the test implementer; none puts a visible output in question.

## Premise

Real. Checked against the code and history:

- `LaneClassifier.ClassifyPath` classifies `src/Reihitsu.Analyzer/AnalyzerResources.resx` as production today, pinned by `LaneClassifierTests` DataRows at `src/Reihitsu.Tooling.Test/Unit/LaneClassifierTests.cs:51` and `:128`.
- `Directory.Build.props:59-64` confirms that the build's analyzers from `binaries/` do not receive the root `reihitsu.json`. The squad note of the RH5201 run (`squad/notes/2026-11-21-rh5201-member-access-wording.md:19`) confirms the RH2111 failure surfaced only in CI.
- `AnalyzerPackageMetadataTests` (`src/Reihitsu.Analyzer.Test/SelfHosting/AnalyzerPackageMetadataTests.cs`) has `AnalyzerTitleResourcesMatchRuleDocumentationTitles`, `PackageReadmeDescriptionsMatchRuleDocumentationTitles`, `MessageFormatResourcesMatchTitleWhenNoPlaceholders`. The *Coverage first* section describes them correctly.
- Only two `.resx` files exist in the repository (`git ls-files '*.resx'`), with no localized variants and no generated designer files (`AnalyzerResources.cs` is a hand-written `ResourceManager` holder that quotes no value). The two-exact-paths category therefore covers every diagnostic and code-fix text today.

## Mechanism and candidates

_N/A._ This is a process and tooling change, not a bug.

## Breaking inputs

### F1 — After widening out of the wording lane, no role may write the resource texts

`squad/roles/spec-author.md` (Wording) says: "Before you write, search the test projects for every text you change ...; a test that quotes it ... is outside the lane: stop and report it, so the lead widens." So the spec author stops **before** it writes the `.resx` values. The lead then widens (`squad/lead.md` §4 row "Spec author: needs a test") to `tests` without a change doc or `behavior-change` with one. After that:

- In `tests`, nobody may write the `.resx`. The spec author's permission is limited to "the resource texts of the wording lane" (`spec-author.md`, *Must not*). The test implementer must not touch production code. The tests lane has no implementer.
- `squad/roles/test-implementer.md`, *When you run*, tests lane: "every test must pass on the unchanged code ... a test that fails is a finding against the spec". A quoted literal updated to the new text fails on the unchanged code, so this duty contradicts the widened wording run.
- In `behavior-change`, the implementer could write the `.resx`, but nothing says the spec author's texts move to the implementer. The README Roles table still gives the spec author "every change of the ... wording lanes" only.

Members of the class: the starting lane is wording, and the run can widen to `tests` (no change doc) or `behavior-change` (change doc). The resource text can be already written or not yet written when the spec author stops. With `tests` and not yet written, nobody may write it. With `tests` and already written, the texts stand, but no role owns a repair of them, and the test implementer's tests-lane duty contradicts the updated literal. With `behavior-change`, the implementer can write it, but no file says so.

Simplest repair: the spec author owns the wording category, meaning the two resource files' `<value>` texts, in every lane where the run changes what a diagnostic or code fix says. It writes the whole text change before it reports `needs a test`. The test implementer's tests-lane duty gets one clause: in a run widened from wording, an existing test that quotes a changed text is updated to the new text and passes on the changed tree. These are role-file edits only.

### F2 — The motivating run would not land in the wording lane

The RH5201 run's final diff (commit 3caebda) holds one test path, `RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs`, with a one-line message literal, and a change doc. Under the specified rule 4, a wording path with a test path and a change doc needs `behavior-change`, which is the same full lane it ran. Without the change doc it needs `tests`, which still runs the skeptic and the test implementer. The spec author's handover says as much ("with its test file it needs behavior-change"), but the change doc's *Why* cites this run as the evidence for the lane and never says the lane would not have applied to it.

The maintainer's rule is that the short lane applies "unless a **new** test turns out to be needed". The specified lane check also widens for an **updated** literal in an existing test. That is stricter than the rule as stated. The spec author's decision 3 makes the choice consciously, on path-only grounds.

How often does it bite? `rg -o 'RH\d{4}[A-Z]?\w*\.DiagnosticId, *"'` finds 16 distinct rules, out of about 350 message-format resources, whose message a test quotes as a literal. For those rules, rule 1 has no effect until follow-up candidate 1 lands.

Options (an internal choice, since no visible output is in question; the lead decides):

- **A** — Keep the design as written. In the change doc's *Why* or *Decisions*, state that a wording change whose message a test quotes still widens, that the RH5201 run is such a case, and that follow-up 1 removes the cause. Say so in the PR's `Review notes` as a departure from the maintainer's wording ("unless a new test"). Cost: text only.
- **B** — Take follow-up 1 into this run: replace the quoted literals with the analyzer's `MessageFormat` in the 21 files, which are test-only, admitted by the claimed lane, and invisible to users. Cost: about 260 lines across 21 test files. That fails *Scope*'s "a few lines" test.
- **C** — Admit test paths in the wording lane when a literal-only proof passes. This is the alternative the change doc already rejects: a second proof mode.

Recommendation: **A**, and the lead should file follow-up 1 so the gap closes. Because the maintainer's answer is binding, the lead may also prefer to name the departure to the maintainer in chat rather than only in the PR. That is the lead's call.

### F3 — `squad/roles/cleaner.md` still runs in the wording lane

`cleaner.md:9`: "Every lane except docs, after the implementer ...". The lanes table gives the wording lane no cleaner. Fix: "Every lane except docs and wording". This is text only.

### F4 — The push list's rationale is false, and it weakens a README guarantee

The change doc (*Stage boundaries are the points where the branch has to hold the run*) says: "Between them a lost session loses at most one stage's local commits." Under the specified list (claim, before maintainer stops, before ending a blocked run, the trigger commit, CI repairs), a run with no output question and no escalation pushes nothing between the claim and the trigger commit. A lost session in a fresh sandbox then loses every stage, not one. `squad/README.md`, *Handovers are files*, still says "`sdd/work/` is committed to the branch, so a run can continue in a later session". That now holds only from the last push.

The maintainer's words were "pushes handover commits at stage boundaries only". The README uses "stage" for each role's turn ("the lead delegates every stage to a role"), so the literal reading is one push per role stage, which already batches a role's commits. Decision 6 narrows "stage boundary" to the list. That is defensible, and it cuts more events. Options (internal):

- **A** — Keep the list. Correct the change-doc sentence ("a lost session loses the local commits since the last push") and the README sentence ("a run can continue in a later session from its last push").
- **B** — Push once per role stage, after the role's handover commit, as the literal reading suggests. Pre-publish pushes reach the session only while it is subscribed, and §9 subscribes only after publishing, so these pushes cost the lead no events. They cost only check-suite notifications on the draft PR.

Recommendation: **A** if the lead accepts that losing a session costs a whole unstopped run. Otherwise **B**. Either way, the false sentence must go.

### F5 — The line-count filter selects other rows than `scripts/README.md` says

`scripts/README.md`, *The line-count rows*, and the change doc both say the filter "selects the two analyzer rows and the tests of the scan's line-count setup". I ran it on the existing Release build:

```
$ scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"
Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18, Duration: 43 s
```

The same filter with a console logger (`dotnet test <SelfHosting dll> --filter ... --logger "console;verbosity=normal"`, for inspection only) lists the 18 tests:

- 2 rows of `AnalyzerReportsNoViolationsOnOwnSourceCode`, for RH2008 and RH2111;
- 4 ruleset rows: `ReleaseRulesetReportsRuleAsError` and `DebugRulesetHasNoEntryForLineCountRule`, each for `"RH2008"` and `"RH2111"`;
- 12 formatter rows (`FormatterIsIdempotentOnSourceFile`, `FormatterProducesNoChangesOnSourceFile`, `NoPhaseChangesTheFormattedOutputOfSourceFile`) for the four source files named after the two rules.

None of the six `LineCountSelfHostingTests` methods is selected, because their names contain no rule ID. The rows that matter are selected, so the rule works; only the description is wrong.

`--filter "FullyQualifiedName~AnalyzerSelfHostingTests&(Name~RH2008|Name~RH2111)"` selects exactly the two analyzer rows (Total 2, 40 s). The solution load dominates the run time, so narrowing gains no time. Recommendation (internal): correct the sentence in both files to name what is selected. Narrowing the filter is optional.

### F6 — The lane list in the follow-up issue template is stale

`sdd/templates/follow-up-issue.md:19` and `:104` list the lanes "as `scripts/lane-check.sh` takes it: `docs`, `tests`, `bug-fix`, `behavior-change`, or `tooling`", without `wording`. The issue manager fills `Lane:` from it. This is text only. The usage strings in `scripts/lane-check.sh:4` and `:65` and the doc comments in `LaneClassifier.cs` (`Covers`, `TryParseLane`) and `Enumerations/Lane.cs` belong to the implementer, and the spec author's handover already names them.

### Candidate inputs for the classifier (test implementer)

These follow from the five rules in `scripts/README.md`, *The lane check*. Existing coverage per *Coverage first*:

| Input (categories) | Required lane | Existing test |
|---|---|---|
| wording | wording | none found |
| wording + text | wording | none found |
| wording + change doc | wording | none found |
| wording + test | tests | none found |
| wording + test + change doc | behavior-change | none found |
| wording + tooling | none (exit 3, reason names wording and tooling) | none found; `TryGetRequiredLaneFindsNoLaneForProductionAndToolingWithoutChangeDoc` is the production analogue |
| wording + tooling + change doc | behavior-change | none found |
| wording + production | bug-fix | none found |
| wording + production + change doc | behavior-change | none found |
| wording + unknown | none | `TryGetRequiredLaneFindsNoLaneForAnUnknownPath` (other categories) |
| change doc + text, no wording | behavior-change (unchanged) | `TryGetRequiredLaneReturnsTheNarrowestAdmittingLane` |

`Covers`: claimed `wording` covers docs and wording only. Claimed `docs` and `tooling` do not cover wording. Claimed `tests`, `bug-fix`, and `behavior-change` cover wording. The existing `CoversFollowsTheWideningOrders` gets rows. Note a consequence the README bullets do not state: since a wording diff with a change doc needs `wording`, a `tests` or `bug-fix` claim now passes a diff that holds a change doc. Before this change, a change doc always forced `behavior-change`. It is harmless (the claim is wider), but the *Tests* and *Bug fix* bullets in `squad/README.md` read as if they never admit a change doc. That is an internal choice: state it, or accept it.

`ClassifyPath`:

- The two exact paths, also in other letter case, give `Wording`, with the proof passed or not. Two existing DataRows assert `Production` for `src/Reihitsu.Analyzer/AnalyzerResources.resx` (`LaneClassifierTests.cs:51`, `:128`) and are updated, not duplicated.
- Another `.resx` under a production project (for example `src/Reihitsu.Cli/Foo.resx`) gives `Production`.
- A `.resx` under a test project gives `Test`.
- One under `src/Reihitsu.Tooling/` gives `Tooling`.

`LaneNamesRoundTrip` covers `wording` once the enum value exists.

`verify-text-only --exclude` and the `lane-check.sh` hand-off have no test project, since the proof is outside the solution. Their behavior is checked by running them, which is the implementer's demonstration.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Tooling` (`LaneClassifier`, `LaneCheckCommand`, `Enumerations/Lane`, `PathCategory`) | must change |
| `src/Reihitsu.Tooling.Test` (`LaneClassifierTests`, `LaneCheckCommandTests`) | must change (two resx DataRows updated, new rows) |
| `scripts/lane-check.sh`, `scripts/lane-check/lane-check.cs` | must change (lane name, usage, `--exclude` hand-off) |
| `scripts/verify-text-only.sh`, `scripts/proof/verify-text-only.cs` | must change (`--exclude`, line suffix); behavior for every other caller must stay |
| `squad/roles/cleaner.md`, `squad/roles/test-implementer.md`, `squad/roles/spec-author.md`, `sdd/templates/follow-up-issue.md`, change doc, `squad/README.md` | must change (F1, F3, F4, F5, F6) |
| Analyzer, code fixes, formatter, CLI, rule pages, `Reihitsu.Core` | not involved |
| `Reihitsu.SelfHosting.Test` | not involved (only run) |

## Refuse option

There is no input class to refuse; this is a process change. The cheapest version of rule 1 would skip the lane check entirely and let the lead claim `behavior-change` with fewer roles. That contradicts the maintainer's rule that `lane-check.sh` recognizes such a diff, so it is not an option. No unsupported shape is in question.

## Output questions

_None._ No formatter output, diagnostic, code-fix result, rule page, or CLI behavior changes.

## Internal choices

1. **The source of the wording paths for `--exclude`.** `scripts/lane-check.sh:95` runs the proof before it hands the path list to the classifier (`:107`), so the shell must know the wording paths first. Hard-coding the two paths in bash would state the classification a second time beside `LaneClassifier` and the one table in `scripts/README.md`. That is the "one rule stated in more than one place" shape of `squad/README.md`, *Architecture findings*. Recommended: the shell gets the wording paths from the classifier (for example, an extra query of `lane-check.cs`) or the order of the two steps changes, so the bash script holds no path list. The spec is silent on this; the lead should name it for the implementer.
2. **F2 options A/B/C**, recommended A, with follow-up 1 filed.
3. **F4 options A/B**, recommended A with the corrected sentences.
4. **F5**: correct the description; narrowing the filter is optional.
5. **Rule 5 extension.** "Red, also on `main`" unsubscribes once every failure of the final commit is decided as a follow-up. The maintainer's text says only "unsubscribes once green". The extension is needed, since a failure that also exists on `main` never turns green within the run, and it is internal. Recommended: keep it and name it in the PR's `Review notes` as a reading of the maintainer's rule.

## Issue goals

The issue is `agent-filed`, but the maintainer's triage comment accepted goals 1–4 and added 5, so they are binding as the maintainer stated them. Each goal against the text:

- **1, wording lane.** Carried out, except for F1 (no role owns the texts after widening) and F2 (stricter than "unless a new test"; the evidence run would not qualify).
- **2, coverage first.** Carried out: `squad/README.md`, *Coverage first*; `skeptic.md` *Breaking inputs*; `test-implementer.md` *The test matrix*.
- **3, self-hosting check.** Carried out. The trigger (test-project C# with added lines, from `--numstat`) and the command work; the command was verified above. The description is wrong (F5).
- **4, fewer pushes.** Carried out, with a narrower reading of "stage boundaries" than the README's own use of "stage", and a false rationale (F4).
- **5, CI-only watch.** Carried out in `squad/lead.md` §9 and `CLAUDE.md`, with the internal extension in choice 5. No other file assumes a subscription past CI: `rg -i subscribe` over `squad/`, `.claude/`, `CLAUDE.md`, and `AGENTS.md` finds only §9, `CLAUDE.md`, and `bump-version.md`, which is not a squad run.

Nothing here changes visible output.

## Architecture findings

_None._ Internal choice 1 is the place where one could arise, if the implementer copies the wording paths into bash.

## Follow-up candidates

1. This confirms the spec author's follow-up 1, with numbers: 16 distinct rules have a message quoted as a test literal. Replacing the literals is what makes goal 1 effective for those rules (F2).
2. The spec author's follow-up 2 (line-count rows for production files) stands as a process question for the maintainer. I add nothing to it.
