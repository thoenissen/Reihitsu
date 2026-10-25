Role: spec checker
Commit: 819a9b2
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No intended behavior changes (tooling lane, test layout only); no change doc required and none present.
2. Specs match the diff: ok. The diff touches no file under `sdd/specs/`; no spec names the self-hosting scans, the test layout, or `scripts/test.sh`, so none contradicts the diff. The `sdd/README.md` attribute-declaration list now names `Reihitsu.SelfHosting.Test`, which declares `RequirementAttribute` and tags two tests (confirmed).
3. IDs: ok. No ID added, renumbered, or retired; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched; no type renamed in a spec owner list (`grep` of `sdd/specs` finds no reference to `SelfHostingTests` or the moved types).
5. User documentation: ok. No rule page or diagnostic affected; `scripts/README.md`, `AGENTS.md`, `squad/roles/{validator,implementer,test-implementer}.md` agree with `scripts/test.sh` (default `all` leaves out `selfhosting`, alias selects only `src/Reihitsu.SelfHosting.Test/...csproj`) and with `SonarCloud.yml` (loop includes `Reihitsu.SelfHosting.Test`). `.claude/commands/create-rule-doc.md` still correctly names `RuleDocumentationExampleTests` in `Analyzer.Test/SelfHosting/`, which stays.
6. Open questions: ok. No spec author ran; the handovers carry no open questions.
7. Coverage: ok. The diff does not touch `sdd/specs/`; ran `scripts/test.sh --no-install --project architecture` anyway: "Passed!  - Failed:     0, Passed:    89, Skipped:     0, Total:    89" (includes `SpecConsistencyTests` and the `TestScriptProjectSelectionTests`).
8. Requirement tags: ok. Only `FormatterIsIdempotentOnSourceFile` and `FormatterProducesNoChangesOnSourceFile` carry `[Requirement("FMT-PIPE-004")]` (lines 55 and 86 of `FormatterSelfHostingTests.cs`); the test implementer's table lists both against FMT-PIPE-004. No spec author, so no `Examples for the tests`. The tool tests carry no tag and verify no requirement, which is not a finding.
9. Nothing left behind: ok. `sdd/work/` holds only `self-hosting-test-projects/` (this run's); the lead deletes it before publishing. Working tree clean before this handover.
