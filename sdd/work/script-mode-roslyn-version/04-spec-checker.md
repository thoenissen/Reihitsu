Role: spec checker
Commit: fc1f5c0
Verdict: consistent

My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. No intended behavior changes (a file mode, a shared version property, two guard tests); no change doc required, none added.
2. Specs match the diff: ok. The diff touches nothing under `sdd/specs/` and changes no behavior a spec states; the tooling lane ran no spec author.
3. IDs: ok. No requirement added, changed, or retired; `requirement-ids.txt` untouched.
4. Code owners: ok. No spec header touched, no type renamed, added, or removed in a production project.
5. User documentation: ok. No rule page, rule ID, or `helpLinkUri` affected.
6. Open questions: ok. No output question exists; the follow-up candidates (the `scripts/proof/verify-text-only.cs` Roslyn pin, the S1125 warnings) are under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok / n/a. The diff does not touch `sdd/specs/`, so the architecture run is not required for spec consistency; no new active requirement exists.
8. Requirement tags: ok / n/a. No requirement exists for either invariant; the test implementer's tables list none and the new tests carry no `[Requirement]` tag.
9. Nothing left behind: ok. The only `sdd/work/` directory on the branch is this run's (`script-mode-roslyn-version`), for the lead to delete before publishing.

Also confirmed against the diff: `scripts/trace.sh` is `100755` in the index; `RoslynVersion` is defined once in `Directory.Build.props` and every non-Testing `Microsoft.CodeAnalysis*` reference in `src/` uses `$(RoslynVersion)`.
