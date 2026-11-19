# Lead — enable-line-count-rules

- Issues: thoenissen/Reihitsu#1865 (`agent-filed`), thoenissen/Reihitsu#1866 (`agent-filed`)
- Branch: `claude/gifted-wright-81i8g8` (session-designated); draft PR thoenissen/Reihitsu#1879
- Slug: `enable-line-count-rules`
- Lane: `behavior-change`

## Lane and reason

- #1865: enables RH2008 and RH2111 in this repository's own build (rulesets, `Directory.Build.props`, self-hosting scans under `scripts/`: tooling) and, by the maintainer's binding scope, fixes every violation in the repository's own code, which changes production paths under `src/`. Production next to tooling is admitted only with a change doc, so the issue's own lane is `behavior-change`; its text says `tooling`, which does not admit the production refactors.
- #1866: corrects the misspelled RH8402 entry in `Debug.ruleset` (and checks `Release.ruleset`): tooling on its own; it shares the run's lane because the maintainer bundled it with #1865 (same ruleset files) and `behavior-change` is wider than `tooling` along docs → tests → tooling → behavior change.
- Roles run: spec author, skeptic, test implementer, implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if follow-up candidates exist. None skipped.

## Maintainer clarifications (from the issue comments, summarized; binding — they override the issue text)

- #1865 and #1866 run together in one run, in milestone 1.5.0.
- #1865: use the rules' **default** limits of RH2008 and RH2111. No repository-specific limits: no `reihitsu.json` entry or build setting overrides them.
- #1865: turn both rules on for this repository and fix every violation they report in the repository's own code. The maintainer expects only a few changes.
- #1865 tests the default limits: if the defaults prove poorly chosen on this code base (for example many violations in methods or types that read well), the run does not work around them with custom limits; it brings the evidence to the maintainer as an output question with proposed new defaults. A change of the defaults then updates the rules, their specs, and their rule pages.

## Maintainer answers

- Q1 (RH2111 test default), first round: the maintainer weighs limits; goal of the defaults is to prompt design thought in time without being a big obstacle. Asked for counts at 60/120 (methods prod/test) and 1500/3000 (types prod/test); some test classes definitely need restructuring.
- Q1, second round: the **shipped default** for production types is meant to be 1500. This repository **may set lower limits** (in its own configuration), but never higher than the shipped default — this replaces the earlier "no repository-specific limits" clarification. Partial classes are **not** an option for `AnalyzerResources`; the maintainer asks how such a class should be designed instead. Pending: confirmation of the full default set and the repository limits, and the `AnalyzerResources` design (asked 2026-11-19).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane | `behavior-change` for both issues | Binding scope fixes production violations next to tooling changes; only `behavior-change` with a change doc admits that diff. #1866 joins it as the maintainer bundled it | Lane is internal | — |
| 2 | Skeptic | Architecture finding: the build properties the analyzers see would be listed twice (package `buildTransitive` targets and the repository build) | The repository build imports the package's `buildTransitive/Reihitsu.Analyzer.targets` instead of copying its items, so one file states the list | One statement of the rule; cheap; within components the run changes | Repository build only; no shipped output changes | — |
| 3 | Skeptic | How the self-hosting scan tells test projects from production | Skeptic's recommendation: from the project and package data the scan already reads, plus a self-hosting test with one over-limit test method and one over-limit production method through the scan's own path; implementer keeps it a cheap check (FC-044) | Generated editorconfig is missing in focused runs | Test tooling only | — |
| 4 | Skeptic | #1865 goals "build reports no new diagnostics" and "self-hosting scans under `scripts/`" | Adjusted: the committed `binaries/` predate the rules, so the build check is met trivially; the self-hosting scan in `src/Reihitsu.SelfHosting.Test` is the real check. PR states it | Evidence in 02-skeptic | No visible output | — |
| 5 | Skeptic | Ruleset values | `Release.ruleset`: `Action="Error"` like the other enabled entries; `Debug.ruleset`: delete the two `None` lines | Consistency | Repository build only | — |
| 6 | Skeptic | #1866: rename vs drop; ruleset-ID test | Rename `RH08402` → `RH8402`; add a cheap test that every Reihitsu ID in both rulesets names a shipped rule (adjacent fix, guards the class) | Both rulesets read the same; small, tested | RH8402 reports nothing without a copyright config; no output changes | — |
| 7 | Skeptic | `AnalyzerResources` over 500 lines | Split into partial parts by category, update `.claude/commands/add-resource-texts.md`; watch FC-036 (resource manifest name, `nameof`, reflection) | Fixes the violation without custom limits | Resource strings unchanged | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: 01 spec author (change doc Draft, `sdd/changes/2026-11/enable-line-count-rules.md`), 02 skeptic.
- Open: output question 1 (RH2111 test-type default: A 1000 / B 3000 recommended / C 2000) asked to maintainer.
- Next after answer: route to spec author (spec/rule page if default changes; set change doc Accepted), then test implementer.
