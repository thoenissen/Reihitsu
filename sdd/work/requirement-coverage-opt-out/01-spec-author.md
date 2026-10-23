Role: spec author
Commit: 5352a27
Verdict: change doc written

## Changed requirements

_None._ No requirement is added, changed, retired, moved, or renumbered; `sdd/specs/` is untouched. The change is a process change:

- `sdd/changes/2026-10/requirement-coverage-opt-out.md` — new change doc, `Status: Accepted` (initially `Draft`), `Affected requirements: none — process change`, recording the decisions of `00-lead.md` and `squad/decisions/2026-10-23-998-untagged-tests-allowed.md`.
- `sdd/README.md`, *Tests and requirements* — coverage is opt-out: every spec is enforced unless `RequirementCoverageTests` exempts it, each exemption names one spec and states why, the list is empty today; a test derived from a requirement tags it from the start (a reproduction tags the requirement the bug violates); a test is not required to carry a tag, and architecture and tooling tests carry none and need no exemption. "A test that verifies a requirement only incidentally does not tag it" is kept verbatim.
- `sdd/README.md`, *Checks* — the coverage check fails for an uncovered requirement of every spec it does not exempt and for an exemption without a reason; it reports exempted specs without failing; it does not check that every test is tagged, and the spec checker checks the tags of new tests.
- `squad/roles/test-implementer.md` — new section *Requirement tags*: every test derived from a requirement carries its tag; incidental and requirement-free tests carry none; the handover's `Requirement / failure class` column names exactly the requirement IDs each test is tagged with.
- `squad/roles/spec-checker.md` — new check 8 *New tests carry their requirement's tag* (old check 8 is now 9; nothing references check numbers); Inputs now include the test implementer's handover.
- `squad/README.md` — the spec checker's row in the role table names the new check. Outside the three files the prompt listed; a one-phrase consistency edit, approved by the maintainer.

### Why the spec checker, not the auditor

The spec checker already compares what the run wrote down (handovers, specs) with the diff, and its check 7 already looks at `[Requirement]` coverage of new requirements. It runs in every lane, including the tests lane where most tags are written; the auditor runs only in the bug-fix, behavior-change, and tooling lanes, and its test check (assertion adequacy) is about strength, not tags.

## Decisions for the test implementer and implementer

These are stated in the change doc and the rulebook; the code has to match them.

1. **Every spec is enforced by default; the exemption list is empty.** A static scan on 5352a27 (every `### <ID> —` heading under `sdd/specs/` against every `Requirement("<ID>")` in `src/**/*.cs`) found 1,445 requirement IDs and 1,431 tagged; the 14 untagged are exactly the 14 retired requirements. So every active requirement of every spec, including all `shared/` specs, already has a tag, and an empty exemption list is expected to pass. If the real check finds an uncovered requirement, that is not a reason for an exemption without asking the lead — and the rulebook's "it is empty today" would have to change.
2. **An exemption without a reason fails the check.** I added this as the mechanical form of "each entry carries a reason"; a structure that makes a reason mandatory (for example spec path → reason) plus a test for a blank reason satisfies it. Flagging it because it is my wording of the maintainer's decision, not the maintainer's.
3. **`Uses` keeps its meaning for exempted shared specs.** A requirement of an exempted `shared/` spec stays enforced when a non-exempted spec lists it under `Uses` — today's behavior, carried over so an exemption never releases a requirement an enforced spec relies on. With an empty list this is dormant, but the existing `CoverageCheckEnforcesSharedRequirementsThatAnEnforcedSpecUses` behavior should survive in the opt-out form. This is a process choice I made; the lead may overrule it.
4. **The report of unenforced specs becomes a report of exempted specs.** With an empty list it reports nothing.
5. No per-test check, no exemption attribute, no exemption list for tests; architecture and tooling tests are not touched.

## Examples for the tests

_None from specs._ The test implementer's inputs are decisions 1–4 above (the rulebook's *Checks* section is the normative text).

## Open questions to file

_None._

## Notes

- `scripts/lane-check.sh tooling --head worktree --no-install`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- The change doc is `Accepted`: the maintainer approved its What and Why as written, including the spec checker as the role that checks tags and the `squad/README.md` role-table edit (`squad/decisions/2026-10-23-998-change-doc-approved.md`, option A). Status set in the follow-up commit on 7834bb7.
- My prompt carried no intended fix or hoped-for verdict beyond the maintainer's narrowed scope, which I followed.
- `squad/roles/skeptic.md`, `squad/failure-classes.md`, `squad/roles/auditor.md`, and every `.cs` file are untouched.

## Repair round (auditor 06 finding 2, hint 3; head c555800)

- **Finding 2 — closed.** `sdd/README.md` *Checks*: the coverage bullet now states the full failing set as `IsEnforced` computes it. It covers an uncovered active requirement whose spec is not exempted, and one of an exempted `shared/` spec that a spec that is not exempted lists under `Uses`. This matches *Tests and requirements* and the change doc. The report sentence now says that for each exempted spec the check writes that spec's uncovered requirements (`DescribeCoverageOfExemptedSpecs` lists every one of them, including those enforced through `Uses`), that this report never fails the build, and that an uncovered requirement of an exempted spec fails only through the `Uses` case. I wrote the wording from `IsEnforced`, `ReadSharedIdsUsedByEnforcedSpecs`, and `DescribeCoverageOfExemptedSpecs` on c555800, not from the auditor's suggestion.
- **Hint 3 — closed.** In `.claude/agents/spec-checker.md`, `description` now reads "verifies specs, change doc, user documentation, and the requirement tags of new tests against the diff", matching the spec checker's row in `squad/README.md`. The `.claude/agents/README.md` row ("Compares written text with the diff against a fixed checklist") is generic, still true, and unchanged.
- I did not touch any `.cs` file, `squad/roles/skeptic.md`, or `squad/failure-classes.md`. The change doc is unchanged and stays `Accepted`.
- `scripts/lane-check.sh tooling --head worktree --no-install`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
