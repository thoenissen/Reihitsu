Role: implementer
Commit: 8158833
Verdict: tests pass

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs` | `EnforcedSpecPaths` (opt-in list) replaced by `ExemptedSpecs`, an empty `IReadOnlyDictionary<string, string>` (spec file relative to `sdd/specs` mapped to its reason, ordinal keys). `FindUncoveredRequirements` takes the exemptions and enforces every spec not on them. New `FindExemptionsWithoutReason` reports every entry whose reason is `null`, empty, or whitespace (`<specPath>: the exemption states no reason`). `DescribeCoverageOfSpecsNotYetEnforced` replaced by `DescribeCoverageOfExemptedSpecs`, keeping the line format `<specPath>: <n> of <m> uncovered (<IDs>)`. `IsEnforced` now reads "not exempted, or a `shared/` requirement a non-exempted spec uses"; `ReadSharedIdsUsedBy` became `ReadSharedIdsUsedByEnforcedSpecs` (headers of non-exempted specs). `IsInPaths` removed, since an exemption names one spec file and is matched by exact key. Class, constant, property, and method XML summaries rewritten for the opt-out check (FC-031). | none — process change (change doc `sdd/changes/2026-10/requirement-coverage-opt-out.md`) |

No test method, assertion, fixture input, or `CreateExemptions` helper changed (checked with `git diff -U0` against 55e5b23: no changed line contains `TestMethod`, `Assert`, or a `CreateExemptions(` call).

Choices the tests leave open, stated so the next roles can check them:

- **Report scope.** `DescribeCoverageOfExemptedSpecs` lists every active requirement of each exempted spec, so `<m>` is the spec's full count and a requirement of an exempted `shared/` spec that is enforced through `Uses` also appears in the report (it fails the check as well when uncovered). The test implementer noted this as unspecified; I chose it because the report test's summary reads "per exempted spec, how many active requirements no test covers".
- **Empty report.** With nothing exempted the report is the single line `Coverage of exempted specs: no spec is exempted`.
- **An exemption without a reason still exempts its spec** in `FindUncoveredRequirements`; the reason check fails separately. Unspecified, not asserted.
- **Keys are spec files only.** A directory key (`formatter/`) exempts nothing, matching "each exemption names one spec" in `sdd/README.md`.

## Policy owners

- `rg -n "EnforcedSpecPaths|SpecsNotYetEnforced|ExemptedSpecs|not yet enforced" --glob '!sdd/work/**' .` outside `RequirementCoverageTests.cs`: no hits, so the opt-in list and its report had no other consumer.
- The enforcement predicate has one owner, `IsEnforced`; the `Uses` reading has one owner, `ReadSharedIdsUsedByEnforcedSpecs`; both are private to `RequirementCoverageTests`. Spec and reference parsing stays in `SpecCatalog` and `RequirementReferenceCatalog`, both unchanged.

## Tests run

```shell
scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"
# Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed:     0, Passed:    83, Skipped:     0, Total:    83
scripts/lane-check.sh tooling --head worktree --no-install
# LANE CHECK: confirmed — claimed tooling, diff needs tooling
```

The real-repository `EveryRequirementOfAnEnforcedSpecIsCovered` passes with the empty exemption list, so every active requirement of every spec has a covering test on this commit. The Release build's warnings are left to the cleaner.

## Disputed tests or scope

_None._ My prompt carried no intended fix, suspected cause, or hoped-for verdict. No production project, `squad/roles/skeptic.md`, or `squad/failure-classes.md` was touched.

## Follow-up candidates

- A check that every `ExemptedSpecs` key names an existing spec file, so a stale or misspelled entry (or a directory key, which now exempts nothing) is reported. Already noted by the test implementer; no rulebook sentence requires it today.

## Repair round (auditor 06, finding 3)

Commit: c3d77ea (base 6057b00)

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs` | XML summary of `FindUncoveredRequirements` only (FC-031). It now reads: "Finds every active requirement that no test method covers and that is enforced: its spec is not exempted, or it belongs to an exempted `shared/` spec and a spec that is not exempted lists it under `Uses`". I derived the wording from the method's filter, `IsEnforced` combined with `coveredIds.Contains(...) == false`, together with `ReadSharedIdsUsedByEnforcedSpecs`. I did not take over the auditor's suggested wording. | none — process change |

No code, test, or other comment changed.

```shell
scripts/verify-text-only.sh --no-install --base HEAD --head worktree
# TEXT-ONLY PROOF: PASS — base=6057b00 head=worktree — 1 changed file(s) (0 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s), no public API documentation
scripts/format.sh --no-install src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs
# Formatted 0 of 1 file(s).
scripts/test.sh --configuration Debug --no-install --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd.RequirementCoverageTests"
# Passed!  - Failed:     0, Passed:    25, Skipped:     0, Total:    25
```

The 25 tests include the test implementer's new directory-key test from 6057b00, which passes against the exact-key matching unchanged since 8158833.
