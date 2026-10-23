Role: test implementer
Commit: 7f66b4f
Verdict: tests written

## Inputs

`00-lead.md` (maintainer's narrowed scope), `01-spec-author.md`, `sdd/changes/2026-10/requirement-coverage-opt-out.md` (Accepted), `sdd/README.md` *Tests and requirements* and *Checks*. My prompt carried no intended fix or hoped-for verdict. Skeptic skipped by the lane, so there is no skeptic candidate list.

## API the tests need

Everything is in `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`. I changed only `[TestMethod]`s and added one fixture helper, `CreateExemptions(params (string SpecPath, string Reason)[])`, next to `CreateTestSource`. The check's own members are left to the implementer. On this commit the project **does not compile**: the tests use these members, which do not exist yet:

| Member | Shape the tests use |
|---|---|
| `ExemptedSpecs` | `IReadOnlyDictionary<string, string>` static member: spec path relative to `sdd/specs` mapped to its reason. Used by the real-repository tests. |
| `FindUncoveredRequirements` | `(string repositoryRoot, IReadOnlyDictionary<string, string> exemptedSpecs)` returns `List<string>`. One finding per uncovered requirement, starting with its ID and containing `<specPath>:` (kept from today's format). |
| `FindExemptionsWithoutReason` | `(IReadOnlyDictionary<string, string> exemptedSpecs)` returns `List<string>`. One finding per exemption whose reason is `null`, empty, or whitespace, naming its spec path. |
| `DescribeCoverageOfExemptedSpecs` | `(string repositoryRoot, IReadOnlyDictionary<string, string> exemptedSpecs)` returns `string`. One line per exempted spec, `<specPath>: <n> of <m> uncovered (<IDs>)` (today's line format). |

The old opt-in members (`EnforcedSpecPaths`, the `IReadOnlyList<string>` overload of `FindUncoveredRequirements`, `DescribeCoverageOfSpecsNotYetEnforced`) are no longer called by any test (`S1144` on `DescribeCoverageOfSpecsNotYetEnforced` in the current build). The class and method XML summaries still describe the opt-in check (FC-031); I did not touch them because they document the check's code.

## Tests

No test carries a `[Requirement]` tag. These are tooling tests, `Reihitsu.ArchitectureTests` declares no attribute, and `sdd/README.md` says such tests carry none.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryRequirementOfAnEnforcedSpecIsCovered` (real repo, changed: `ExemptedSpecs`) | none (tooling); change doc *Every spec is enforced* | real repository | compile error; passed on the reference run with an empty list |
| `EveryExemptionStatesAReason` (real repo, new) | none; change doc: an entry without a reason fails | real repository | compile error |
| `ReportCoverageOfExemptedSpecs` (real repo, renamed from `ReportCoverageOfSpecsNotYetEnforced`) | none; *Checks*: exempted specs reported without failing | `TestContext` | compile error |
| `ReferenceCheckRecognizesEveryAttributeSpelling` (adapted: no exemptions) | none | `SpecFixtureDirectory` | compile error; unchanged meaning |
| `CoverageCheckReportsUncoveredRequirementOfEnforcedSpec` (adapted) | none | `SpecFixtureDirectory` | compile error; unchanged meaning |
| `CoverageCheckCountsOnlyTestMethodsInTestProjects` (adapted) | none | `SpecFixtureDirectory` | compile error; unchanged meaning |
| `CoverageCheckEnforcesEverySpecByDefault` (new) | none; opt-out default, FC-003 (not exempted side) | `SpecFixtureDirectory` | red: on the stub only 4 of 7 reported (analyzer, cli, formatter, playground); `shared/casing.md`, `new-area/surface.md`, `top-level.md` missing |
| `CoverageCheckEnforcesSharedRequirementThatNoSpecUses` (new) | none; FC-002 ("shared" is no longer a qualifier) | `SpecFixtureDirectory` | red: stub reports 0, expected `SHARED-P-002` |
| `CoverageCheckIgnoresRetiredRequirements` (split from `CoverageCheckIgnoresSpecsNotEnforcedAndRetiredRequirements`) | none | `SpecFixtureDirectory` | passes on the stub; existing behavior kept |
| `CoverageCheckSkipsOnlyTheExemptedSpec` (new) | none; FC-003 (exempted vs. sibling not exempted), FC-001 (an exemption names one spec, not its directory) | `SpecFixtureDirectory` | red: stub reports both `FMT-CLEAN-001` and `FMT-INDENT-001` |
| `CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses` (replaces `CoverageCheckEnforcesSharedRequirementsThatAnEnforcedSpecUses`) | none; change doc *Uses* sentence, FC-003 (used vs. unused shared requirement) | `SpecFixtureDirectory` | red: the using spec is `new-area/surface.md`, which the old list does not enforce, so the stub reports 0 instead of `SHARED-P-001` |
| `CoverageCheckDoesNotEnforceExemptedSharedRequirementsThatOnlyAnExemptedSpecUses` (new) | none; FC-003 (other side of *Uses*: the using spec is exempted) | `SpecFixtureDirectory` | red: stub reports `CLI-001` and `SHARED-P-001` |
| `ExemptionCheckReportsEmptyReason` (new) | none; FC-003 | none (pure function) | red: stub reports 0 |
| `ExemptionCheckReportsWhitespaceReason` (new) | none; FC-003 | none | red: stub reports 0 |
| `ExemptionCheckReportsMissingReason` (new, `null` reason) | none; FC-003 | none | red: stub reports 0 |
| `ExemptionCheckReportsOnlyExemptionsWithoutReason` (new) | none; FC-003 (stated reason not reported) | none | red: stub reports 0 |
| `ExemptionCheckAcceptsEmptyList` (new) | none | none | passes on the stub (no exemption, no finding) |
| `CoverageReportCountsUncoveredRequirementsPerExemptedSpec` (replaces `...PerSpecNotYetEnforced`) | none; *Checks*: exempted specs reported | `SpecFixtureDirectory` | red: stub also lists `shared/policy.md`, a spec that is not exempted |
| `CoverageReportNamesNoSpecWhenNothingIsExempted` (new) | none; spec author decision 4 | `SpecFixtureDirectory` | red: stub lists `shared/casing.md` |
| `EveryRequirementReferenceNamesAnActiveRequirement`, `ReferenceCheckReports*` (4) | none | unchanged | unchanged, pass |

### How "fail for the right reason" was established

The committed tests cannot compile without the new members, so I ran them twice against throwaway code that I inserted locally and removed before committing. That code is not in the commit, and `git diff` shows only test methods and `CreateExemptions`.

1. **Stub of today's behavior under the new signatures.** `FindUncoveredRequirements(root, _)` delegates to the old opt-in overload with `EnforcedSpecPaths`, `FindExemptionsWithoutReason` returns no findings, and `DescribeCoverageOfExemptedSpecs` delegates to `DescribeCoverageOfSpecsNotYetEnforced`. Result: 11 failed and 13 passed of 24. Every failure is an assertion about the opt-out difference, as listed in the table, and none is a setup or format failure.
2. **Reference opt-out implementation**, written only to prove the tests are jointly satisfiable: enforce every spec not exempted, keep *Uses* from non-exempted specs, flag `IsNullOrWhiteSpace` reasons, and report exempted specs only. Result: 24 of 24 passed, including the real-repository `EveryRequirementOfAnEnforcedSpecIsCovered` with an empty exemption list. This confirms the spec author's static scan: no active requirement in the repository is uncovered. The expected values come from the change doc and the rulebook, not from this code (FC-035). The reference only checked that they can be met.

After the stub and reference code were removed, the build fails with `CS0103` and `CS1503` on the four members above, plus `S1144` on the now-unused `DescribeCoverageOfSpecsNotYetEnforced`, and nothing else.

## Not covered, and why

- **The exemption list being empty.** The change doc says it "is empty when this change is merged". That is a merge-time state, not an invariant: the rulebook allows exemptions that state a reason. A test asserting emptiness would make every legitimate exemption fail. The spec checker can check that `ExemptedSpecs` is empty in the diff.
- **An exemption naming a path that is no spec (stale or misspelled entry, or a directory key).** No rulebook or change-doc sentence makes this a failure ("names one spec" is stated, but *Checks* lists only uncovered requirements and missing reasons). `CoverageCheckSkipsOnlyTheExemptedSpec` makes sure a file key does not exempt its siblings. A check for unknown keys would be a new rule, so it is left out. Follow-up candidate if wanted.
- **Whether a requirement of an exempted shared spec that is enforced through *Uses* also appears in the exempted-spec report.** Unspecified ("writes the uncovered requirements" of an exempted spec), so I did not assert it either way.
- **Whether an exemption without a reason still exempts its spec.** Unspecified, since the reason check fails the build anyway. Not asserted.
- **Per-test tagging.** Dropped by the maintainer. No test asserts that an untagged test is allowed beyond the existing `CoverageCheckCountsOnlyTestMethodsInTestProjects`, because the check reads no untagged tests. The spec checker's new-test tag check is a process step and cannot be tested here.
- Failure classes: FC-001, FC-002, and FC-003 are covered as listed. FC-017: the assertions check exact counts and IDs. FC-035: the expected values come from the change doc. FC-031: applies to the implementer (stale summaries, see above). FC-004 to FC-014, FC-018 to FC-030, FC-036, and FC-037 cannot reach this change. It touches no analyzer, code fix, formatter phase, trivia, rename, or text-only proof, only a Markdown/source scanner in an architecture-test project.
- CRLF: the fixture specs are written with LF only, as all existing fixtures are. Line endings are not part of the change, and `SpecCatalog` reads with `File.ReadLines`, which splits both.

## Commands

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"
```
