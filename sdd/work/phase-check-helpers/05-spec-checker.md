Role: spec checker
Commit: 60bc2f5
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from the role file, `00-lead.md`, handovers 01 to 04, and `git diff origin/main...HEAD`.

The diff touches no file under `sdd/specs/` or `sdd/changes/`; outside `src/` and `sdd/work/` it changes only `scripts/README.md`. No behavior a spec states changes (test helpers, test call sites, and a project reference).

## thoenissen/Reihitsu#1505

### Findings
None.

### Checked
1. ok: no intended behavior changes, so no change doc is required and none exists.
2. ok: no behavior the diff changes is stated by a spec; no requirement contradicts it.
3. ok: no ID added; FMT-PIPE-027 is in `requirement-ids.txt` (line 165), unchanged.
4. ok: no spec header touched; no owner renamed, added, or removed (no production type added).
5. ok: no rule page affected.
6. ok: no output question in handovers 01 to 04 is open; `Maintainer answers` is `(none)` and the skeptic's decisions are all recorded as non-visible.
7. ok (not required): the diff touches no `sdd/specs/`, so the architecture run is not needed; the implementer reports `--project architecture` at `Passed: 118, Failed: 0`. FMT-PIPE-027 already has covering tests; the new ones add to it.
8. ok: `ObservedPipelineTests` additions verify no requirement and the test implementer's table lists them as untagged ("tool test"), which is allowed.
9. ok for now: `sdd/work/phase-check-helpers/` is this run's only working directory; the lead deletes it before publishing (the self-hosting `WorkingDocumentsDirectoryContainsNoFiles` failure is the expected reminder).

## thoenissen/Reihitsu#1577

### Findings
None.

### Checked
1 to 6, 9: ok, as above.
7: ok, as above; the seven call sites add an incidental check to existing tests.
8: ok. The five new `PhaseStabilityAssertionTests` methods are the ones the test implementer's table lists against FMT-PIPE-027, and all five carry `[Requirement("FMT-PIPE-027")]` (lines 60, 74, 94, 118, 142). The seven call sites are listed as untagged with the reason "check is incidental to the test's own requirement", which `sdd/README.md` (*Tests and requirements*) allows. No spec author ran, so there is no `Examples for the tests` list to cover.

## thoenissen/Reihitsu#1576

### Findings
None.

### Checked
1 to 6, 9: ok, as above.
5: `scripts/README.md` sentence ("...and for the stability of the formatted output under each formatter phase run alone") matches the new method and the scan; it is repository documentation, not a rule page.
7: ok, as above; the skeptic and lead chose a test-only covering method rather than a spec edit.
8: ok. `FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` is listed against FMT-PIPE-027 and carries `[Requirement("FMT-PIPE-027")]` (line 119). Its skips (unsupported output, hiding carrier FMT-PIPE-033) match the requirement's "Output of a run that formatted" bullet.

## Notes (not findings)
- Source comments added by the diff contain no tracker numbers or `TODO` (grep over the changed `src/` files returned nothing).
