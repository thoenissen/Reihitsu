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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Lane | `behavior-change` for both issues | Binding scope fixes production violations next to tooling changes; only `behavior-change` with a change doc admits that diff. #1866 joins it as the maintainer bundled it | Lane is internal | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Next: spawn spec author (01).
