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
- Q1, second round: the **shipped default** for production types is meant to be 1500. This repository **may set lower limits** (in its own configuration), but never higher than the shipped default — this replaces the earlier "no repository-specific limits" clarification. Partial classes are **not** an option for `AnalyzerResources`; the maintainer asks how such a class should be designed instead. 
- Final answer (2026-11-19), options as asked in chat:
  - **1A** — shipped defaults: RH2008 production methods 60 (unchanged), test methods **120** (was 100); RH2111 production types **1500** (was 500), test types **3000** (was 1000). Rules, specs and rule pages RH2008/RH2111 updated accordingly.
  - **2A** — this repository sets lower limits in a root `reihitsu.json`, only where lower than the default: production types **500**. Methods 60/120 and test types 3000 follow the defaults. Never set a limit above the shipped default. Expected work: 18 production methods, 9 test methods, 11 test classes, production types over 500 (`CasingCodeFixProviderBase{T}`, `RH3002…Analyzer`, `OrderingDeclarationUtilities`, `SyntaxTriviaUtilities`, `AnalyzerResources`).
  - **3C** (partial classes are not an option) — drop the per-rule properties of `AnalyzerResources`: the analyzer base derives the resource names from the rule ID (`<ID>Title`, `<ID>MessageFormat`); the class keeps the `ResourceManager`. The maintainer added: the texts can be published per property (on the analyzers), so a unit test can check for all analyzers that their texts exist; tests take the message texts from that property instead of `AnalyzerResources.RH####MessageFormat`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane | `behavior-change` for both issues | Binding scope fixes production violations next to tooling changes; only `behavior-change` with a change doc admits that diff. #1866 joins it as the maintainer bundled it | Lane is internal | — |
| 2 | Skeptic | Architecture finding: the build properties the analyzers see would be listed twice (package `buildTransitive` targets and the repository build) | The repository build imports the package's `buildTransitive/Reihitsu.Analyzer.targets` instead of copying its items, so one file states the list | One statement of the rule; cheap; within components the run changes | Repository build only; no shipped output changes | — |
| 3 | Skeptic | How the self-hosting scan tells test projects from production | Skeptic's recommendation: from the project and package data the scan already reads, plus a self-hosting test with one over-limit test method and one over-limit production method through the scan's own path; implementer keeps it a cheap check (FC-044) | Generated editorconfig is missing in focused runs | Test tooling only | — |
| 4 | Skeptic | #1865 goals "build reports no new diagnostics" and "self-hosting scans under `scripts/`" | Adjusted: the committed `binaries/` predate the rules, so the build check is met trivially; the self-hosting scan in `src/Reihitsu.SelfHosting.Test` is the real check. PR states it | Evidence in 02-skeptic | No visible output | — |
| 5 | Skeptic | Ruleset values | `Release.ruleset`: `Action="Error"` like the other enabled entries; `Debug.ruleset`: delete the two `None` lines | Consistency | Repository build only | — |
| 6 | Skeptic | #1866: rename vs drop; ruleset-ID test | Rename `RH08402` → `RH8402`; add a cheap test that every Reihitsu ID in both rulesets names a shipped rule (adjacent fix, guards the class) | Both rulesets read the same; small, tested | RH8402 reports nothing without a copyright config; no output changes | — |
| 7 | Skeptic | `AnalyzerResources` over 500 lines | Superseded by maintainer answer 3C (no partial classes): drop the per-rule properties; watch FC-036 (resource manifest name, `nameof`, reflection) | Fixes the violation without custom limits | Resource strings unchanged | — |

| 8 | Spec author 03 | Change doc acceptance | Accepted (status set by the spec author on the lead's prompt); all visible output changes answered (1A, 2A, 3C) | Every listed change has an answer | Answers cover them | 65d58eb7 |
| 9 | Spec author 03 | Resource naming as a spec requirement? | No: recorded in the change doc and the procedure doc only | No user-visible text changes | Internal | 65d58eb7 |
| 10 | Spec author 03 | Test that the root `reihitsu.json` never sets a limit above the shipped default | Yes, a cheap unit test | Guards the maintainer's "never above the default" | Repository config only | — |
| 11 | Spec author 03 | `AGENTS.md` names `DiagnosticAnalyzerBase<TAnalyzer>`, but the class is not generic | Adjacent text fix by the spec author after the implementer, matching the final base shape | Text the run's base change touches anyway | Text only | — |
| 12 | Test implementer 04 | Who restructures the test code over the limits (11 test types > 3000, 9 test methods > 120) | A fresh test implementer after the implementer, so the split is verified against the finished scan; the implementer does production only | Test files belong to the test implementer; splitting changes no assertion, input or expected output | Test code only | — |
| 13 | Implementer 05 | The committed `binaries/` (1.4.0) RH0001 does not know the `design` section and would report the root `reihitsu.json` in every project, failing Release/CI | The repository build does not pass the root `reihitsu.json` as `AdditionalFiles` until `binaries/` is rebuilt (the build cannot run RH2008/RH2111 before that anyway); the self-hosting scan, which runs the analyzers built from source, reads it and enforces 500. `BuildConfigurationTests.RepositoryBuildPassesRootConfigurationToTheAnalyzers` is wrong for this state and is reworked by the test implementer to assert the item is absent / or removed. Passing it to the build once `binaries/` know the section is a follow-up | Avoids a suppression or ruleset change (escalation 6) and a version bump outside the run | Repository build only | — |
| 14 | Implementer 05 | `CodeFixResourcesTests.cs:19` summary says "AnalyzerResources" | Adjacent text fix by the test implementer in its next stage | One word in a file the run's area touches | Text only | — |
| 15 | Implementer 05 | Four new internal types from the production refactors | Accepted: they are the issue's own fix (fixing the violations the maintainer asked for), not a structural change under escalation 9 | `squad/README.md`, escalation 9 | Behavior unchanged | 261a706b |
| 16 | Implementer 05 | Implementer stopped mid-run (large context) | Fresh implementer continues from 05-implementer.md | Agent large | — | — |

## Follow-up candidates

- Pass the root `reihitsu.json` to the repository build as `AdditionalFiles` once `binaries/` are rebuilt with an RH0001 that knows the `design` section (so the build enforces the repository's 500 type limit too) — from 05-implementer.md, decision 13.

## Worklist / next step

- Done: 01 spec author (change doc Draft, `sdd/changes/2026-11/enable-line-count-rules.md`), 02 skeptic.
- Output questions answered (1A, 2A, 3C).
- Done: 03 spec author (65d58eb7). Lane check after it: confirmed.
- Done: 04 test implementer (d0c6e803).
- Done: 05 implementer (261a706b, handover aa355b4e) — stopped with items left.
- Next: fresh implementer (06): remaining production violations after re-running the RH2008/RH2111 scan rows, decision 13, Release build check; then fresh test implementer (07): decision 13 test rework, decision 14, test-code splits (decision 12); then cleaner.
