Role: spec author
Commit: dbdf5db
Verdict: change doc written

This is an amendment round for decisions 8–10 in `00-lead.md`, after `04-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. `03-spec-author.md` stays as the earlier record. This file supersedes its claim that RH7103 withholds its fix for a type with a region.

## Changed requirements

- FMT-REGION-004 — Regions without a description keep their text — changed again:
  - (decision 8) A `#region` without a description and an `#endregion` without text are written as the bare keyword. Whitespace behind `region` or `endregion` is removed, and a line without such whitespace is left unchanged. The earlier "MUST NOT change a `#region` without a description" now reads "MUST NOT add text to …".
  - (decision 9) The requirement states that it holds inside a conditional-compilation branch the build does not compile. FMT-REGION-006 is unchanged.
  - It also states the pairing scope, by reference to FMT-REGION-003, as FMT-REGION-007 does.
  - Two examples are added: `#region···` / `#endregion···` → `#region` / `#endregion`, and an `#if false` branch with `#region···` and `#endregion    // Stale···` → `#region` and `#endregion // Stale`.
- `region-formatting.md` introduction — it now also names the end of every region directive line.
- Change doc `sdd/changes/2026-11/region-directive-separator.md`:
  - *What* states the bare-keyword form and the reach into disabled branches.
  - Two visible output changes are added: trailing whitespace behind a region directive without a description or label, and the same inside `#if false`.
  - *Unchanged* is narrowed to "when nothing follows the keyword".
  - (decision 10) RH7103 is added to the list of affected fixes. A worked example is added, taken from the existing test `CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions`. Before the change, the fix writes `#endregion // Decoy` and `// Factories`. After it, both `#endregion` lines stay bare.
  - *Decisions* gains *Taken by the lead on the maintainer's answers*, which records decisions 8 and 9.
  - `Status` stays `Accepted`. The new visible changes are covered by maintainer answers 3 and 7, as the lead recorded.

No new requirement ID. No rule page changes. `scripts/test.sh --project architecture`: 92 passed.

## Examples for the tests

- FMT-REGION-004 — the two new examples. The existing test `RegionLineFormTests.RegionWithoutDescriptionAndTrailingWhitespaceIsKept` now contradicts the spec. Its expectation has to become `#region` without trailing whitespace, and it should be renamed. Bare `#endregion···` and the `#if false` case with `#region···` are not covered yet.
- RH7103 — no new test. The existing changed test already pins the change-doc example.

## Output questions

_None._ Decisions 8 and 9 come from maintainer answers 7 and 3.

## Internal choices

- The bare-keyword rule and the disabled-branch reach go into FMT-REGION-004 itself, not into FMT-REGION-006 or FMT-REGION-007. The alternative was to widen FMT-REGION-006's list. That would put a second statement next to FMT-REGION-007's own qualifier, and decision 3 placed that qualifier in the requirement it qualifies.
- Decisions 8 and 9 are recorded in the change doc under a separate heading, *Taken by the lead on the maintainer's answers*. They are not added to the numbered maintainer answers, so the doc does not attribute words to the maintainer that the maintainer did not write.

## Follow-up candidates

Unchanged from `00-lead.md`. `04-test-implementer.md` finding 3 is resolved: an undescribed `#region···` is in this run's scope (FMT-REGION-004), and it is not part of the follow-up for other directives.
