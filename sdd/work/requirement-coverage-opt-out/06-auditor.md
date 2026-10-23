Role: auditor
Commit: 17833162596daddc7fbc7019c569463c44a7c315
Verdict: findings

## Scope

Full audit. Head `17833162596daddc7fbc7019c569463c44a7c315` (clean checkout of `claude/clever-clarke-4ww2jt`). Base `origin/main` = `c857b2d97e7d4822cc9adebd0388b4953de5ebd7`, which matches `git ls-remote origin refs/heads/main` and is an ancestor of head. The diff (14 files) is what handovers 00–05 describe. The only compiled file is `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`. No `src/` change follows the implementer's 8158833, and all of 8158833's hunks lie outside the `Tests` region, so no test or assertion was changed after 55e5b23. `scripts/lane-check.sh tooling --no-install` returns `LANE CHECK: confirmed — claimed tooling, diff needs tooling`. `squad/roles/skeptic.md`, `squad/failure-classes.md`, `squad/roles/auditor.md` and `sdd/specs/` are untouched. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The test and build results of the implementer, cleaner and spec checker (24/24 focused, 83/83 architecture, Release 0 warnings) are trusted and were not re-run. No finding depends on them.

Issue requirements, thoenissen/Reihitsu#998, against the maintainer's narrowed scope (`squad/decisions/2026-10-23-998-untagged-tests-allowed.md`, `-change-doc-approved.md`):

- Opt-out coverage with an empty or reasoned exemption list: done. `ExemptedSpecs` is empty, `IsEnforced` enforces every spec not on it, and `FindExemptionsWithoutReason` and `EveryExemptionStatesAReason` fail a blank reason.
- One rule for incidental tests, written into the rulebook: done (`sdd/README.md`, *Tests and requirements*).
- Architecture and tooling tests: decided. They carry no tag and need no exemption.
- `sdd/README.md` *Tests and requirements* and *Checks*, and `squad/roles/test-implementer.md`: updated. See finding 2 for a gap in *Checks*.
- Process change doc: present, `Accepted`, `Affected requirements: none — process change`.
- "Every test method is tagged or visibly exempted, and the check enforcing that runs in CI": explicitly out of scope by the maintainer's decision. This is not a finding on the tree, but the PR must state the difference (hint 5).
- "No `sdd/work/` in the PR": the lead's step at publish time.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`, `CoverageCheckSkipsOnlyTheExemptedSpec` and the other exemption fixtures | FC-003, FC-001 | PR-introduced | The implementer removed `IsInPaths`, the arm that matched a key ending in `/` by prefix, and now matches exemptions by exact key, as "each exemption names one spec" (`sdd/README.md`) requires. No test exercises the removed arm: every exemption key in the class is a `.md` file path (`formatter/cleanup.md`, `formatter/indentation.md`, `shared/policy.md`, `shared/casing.md`, `cli/reihitsu-format.md`), and none ends in `/`. Static trace: putting the prefix arm back into `IsEnforced`, `ReadSharedIdsUsedByEnforcedSpecs` and the report would leave every test green. With it back, an entry `["formatter/"] = "…"` would silently release all 13 formatter specs. The test-implementer handover lists "FC-001 (an exemption names one spec, not its directory)" against `CoverageCheckSkipsOnlyTheExemptedSpec`, but that test's file key cannot tell exact matching from prefix matching. | Add a fixture test with two specs in one directory and an exemption keyed by the directory (`formatter/`), asserting that both requirements are still reported. The other side of the removed condition is already covered by `CoverageCheckSkipsOnlyTheExemptedSpec`. Owner: test implementer. |
| 2 | low | `sdd/README.md`, *Checks*: the bullet "an active requirement of an enforced spec — every spec it does not exempt — has no covering test" and the sentence "For an exempted spec, it writes the uncovered requirements to the test output without failing" | FC-031 (text describes less than the code) | PR-introduced | The new appositive limits the failing set to specs that are not exempted. The check also fails for an uncovered requirement of an exempted `shared/` spec that a non-exempted spec lists under `Uses`. That case is in `IsEnforced` (lines 688–690), `CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses`, the change doc, and the same rulebook's *Tests and requirements* (line 81). Read on its own, *Checks* says that requirement never fails. Text only: it cannot reach compiled behavior. | Name the `Uses` case in the bullet, e.g. "an active requirement of an enforced spec — every spec it does not exempt — or a `shared/` requirement such a spec lists under `Uses` has no covering test". Owner: spec author. |
| 3 | low | `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs:586`, summary of `FindUncoveredRequirements`: "Finds every active requirement of an enforced spec that no test method covers" | FC-031 | PR-introduced | The method also returns an uncovered `shared/` requirement of an exempted spec that a non-exempted spec uses, and by the class summary's own definition that requirement is not "of an enforced spec". This is XML documentation of a private method, so it cannot reach compiled behavior or public API documentation. | Describe the requirement-level predicate, e.g. "Finds every enforced active requirement (see `IsEnforced`) that no test method covers". Owner: implementer. |

The verdict is `findings`, not `pass, text cleanup`, because finding 1 is a missing test. Findings 2 and 3 alone would qualify as text cleanup.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A spec is exempted | `SpecRequirement.SpecPath` and `SpecHeader.SpecPath`: path relative to `sdd/specs`, forward slashes (`Path.GetRelativePath(...).Replace('\\', '/')`). Matched by `ExemptedSpecs.ContainsKey`, exact and ordinal | The same `ContainsKey` on the same dictionary in `IsEnforced` (688), `ReadSharedIdsUsedByEnforcedSpecs` (702) and `DescribeCoverageOfExemptedSpecs` (623); rulebook "each exemption names one spec" | Agree. All three share one comparer. A directory key, a backslash key, or a key with a `sdd/specs/` prefix exempts nothing, so a wrong key fails closed and the spec stays enforced | Correct in code. The non-file-key side is untested (finding 1) |
| A requirement of an exempted spec stays enforced through `Uses` | `requirement.SpecPath.StartsWith("shared/")`, plus the ID in a `Uses` cell of a non-exempted spec's header (rows before the first `## `, `SpecCatalog.ReadHeaders`, `SplitCell`) | `SpecStructureTests`' `Uses` rule: `Uses` lists active requirements of any other spec, `FMT-…` included | The `shared/` qualifier is an explicit condition and matches the rulebook and change doc wording. `Uses` itself also names non-shared IDs: 8 analyzer specs (RH5602, RH6006, RH6007, RH6013, RH6018, RH6019, RH6020, RH6022) list `FMT-CLEAN-001`, and 11 formatter specs plus 2 `shared/` specs list `FMT-PIPE-002` | As specified. See hint 1 |
| Fail versus report | Fail: `IsEnforced` and uncovered. Report: exempted membership | Old report: the complement of the old `IsEnforced` | Different by design. A used `shared/` requirement of an exempted spec is both reported and failing. The report line's `<m>` is the spec's full active count, which matches "For an exempted spec, it writes the uncovered requirements" | Acceptable. Finding 2 is about the text only |
| An exemption states a reason | `string.IsNullOrWhiteSpace(value)` per entry | None | n/a. Tested on null, empty, whitespace, and a stated reason | ok |
| What counts as a spec | `SpecCatalog.EnumerateSpecFiles`: every `*.md` below `sdd/specs` except the root `README.md` | The same enumerator in `SpecConsistencyTests` and `SpecStructureTests` | Agree, one owner. Opt-out now enforces all 387 spec files, including 30 `shared/` specs and any top-level or new-area spec (fixture `CoverageCheckEnforcesEverySpecByDefault`) | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which requirements are enforced | `rg 'IsEnforced\|EnforcedSpecPaths\|SharedDirectory\|"shared/"\|ContainsKey\(.*SpecPath' src/`: only `RequirementCoverageTests` (`IsEnforced`, `SharedDirectory`, the three `ContainsKey` sites on one dictionary) | One owner |
| The exemption list | `rg 'ExemptedSpecs\|EnforcedSpecPaths\|NotYetEnforced\|IsInPaths\|ReadSharedIdsUsedBy'` outside `sdd/work/`: only `RequirementCoverageTests.cs`. The five `RequirementAttribute` summaries name `RequirementCoverageTests` only as the reader of the attribute, which is unchanged | One owner, no stale consumer |
| Reading of `Uses` | `ReadSharedIdsUsedByEnforcedSpecs` through `SpecCatalog.ReadHeaders` and `SplitCell`; `SpecStructureTests` uses the same `SpecCatalog` members for its own check | Shared parser, one owner |
| Rulebook text on coverage | `rg -i 'enforc\|opt-in\|not yet enforced\|enforced today'`: `sdd/README.md` (lines 81, 169), the change doc, history entries. `squad/README.md:45` and spec-checker check 7 ("an enforced spec") still read correctly under opt-out. `introduce-sdd.md` never described the opt-in list | Consistent apart from finding 2 |
| Tag duty for new tests | `sdd/README.md` *Tests and requirements*, `squad/roles/test-implementer.md` *Requirement tags*, `squad/roles/spec-checker.md` check 8 and *Inputs*, `squad/README.md` role row. `rg` for other tagging duties in `AGENTS.md`, `CLAUDE.md`, `squad/lead.md`, `.claude/`, `sdd/templates/`: none. `.claude/agents/spec-checker.md` still repeats the old role-row sentence | Consistent. See hint 3 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `EveryRequirementOfAnEnforcedSpecIsCovered` | No active requirement of any non-exempted spec is uncovered in the real repository | Any uncovered active requirement in any of the 387 spec files | real repository | Adequate |
| `EveryExemptionStatesAReason` | Every real entry has a non-blank reason | An entry with `""`, whitespace or `null` | real repository | Adequate |
| `ReportCoverageOfExemptedSpecs` | Writes the report and never fails | — (a report by design) | `TestContext` | Adequate for a report |
| `ReferenceCheckRecognizesEveryAttributeSpelling`, `CoverageCheckReportsUncoveredRequirementOfEnforcedSpec`, `CoverageCheckCountsOnlyTestMethodsInTestProjects` | Unchanged meaning; only the `["cli/"]` argument became `CreateExemptions()` | As before | `SpecFixtureDirectory` | Adequate |
| `CoverageCheckEnforcesEverySpecByDefault` | Every area is enforced without an exemption: old-list areas, an unlisted `shared/` spec, a new area, a top-level spec | Any of the 7 IDs missing, or an extra finding (`HasCount(7)` plus each ID) | `SpecFixtureDirectory` | Adequate |
| `CoverageCheckEnforcesSharedRequirementThatNoSpecUses` | A `shared/` requirement no spec uses is enforced | `SHARED-P-002` missing, or extra findings | `SpecFixtureDirectory` | Adequate |
| `CoverageCheckIgnoresRetiredRequirements` | A retired requirement needs no test; an active sibling does | `CLI-001` reported (count 2) or `CLI-002` missing | `SpecFixtureDirectory` | Adequate |
| `CoverageCheckSkipsOnlyTheExemptedSpec` | An exemption releases exactly its spec | Exemption ignored (2 findings), sibling released (0) | `SpecFixtureDirectory` | Adequate for its file key. Exact versus prefix matching is not falsified (finding 1) |
| `CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses` | A used requirement of an exempted `shared/` spec stays enforced; an unused one does not | Uses rule dropped (0 findings) or exemption ignored (`SHARED-P-002` also reported) | `SpecFixtureDirectory` | Adequate on the `shared/` side. Dropping the `shared/` qualifier is not falsified by any test (hint 1) |
| `CoverageCheckDoesNotEnforceExemptedSharedRequirementsThatOnlyAnExemptedSpecUses` | `Uses` of exempted specs keeps nothing enforced | `SHARED-P-001` or `CLI-001` reported | `SpecFixtureDirectory` | Adequate |
| `ExemptionCheckReportsEmptyReason`, `...WhitespaceReason`, `...MissingReason` | A blank reason is reported with its spec | No finding, or a finding without the path. `" \t "` separates `IsNullOrWhiteSpace` from `IsNullOrEmpty` | none | Adequate |
| `ExemptionCheckReportsOnlyExemptionsWithoutReason` | A stated reason is not reported | Count 2, or the wrong path | none | Adequate |
| `ExemptionCheckAcceptsEmptyList` | An empty list gives no finding | Any finding | none | Adequate (trivial) |
| `CoverageReportCountsUncoveredRequirementsPerExemptedSpec` | The report lists exempted specs only, with the exact line format | Line missing or different; a non-exempted spec named. `Assert.DoesNotContain(substring, value)` order confirmed in MSTest.TestFramework 4.5.1 XML docs, so the negatives are not vacuous | `SpecFixtureDirectory` | Adequate |
| `CoverageReportNamesNoSpecWhenNothingIsExempted` | No spec is named when nothing is exempted | A spec path in the report | `SpecFixtureDirectory` | Adequate |

## Failure classes dismissed

- FC-001, FC-003: traced. Finding 1.
- FC-002: the `shared/` qualifier is an explicit condition of `IsEnforced` and tested on its `shared/` side. Its other side is hint 1.
- FC-015, FC-016: one owner per policy (table above). No sibling check carries a private enforcement list.
- FC-017: every new assertion is exact (counts plus IDs or paths). The negatives use the confirmed `DoesNotContain(substring, value)` order.
- FC-031: traced. Finding 3 and hint 4. The other changed summaries (class, `ExemptedSpecs`, `SharedDirectory`, `TestContext`, `IsEnforced`, `ReadSharedIdsUsedByEnforcedSpecs`, `DescribeCoverageOfExemptedSpecs`, `FindExemptionsWithoutReason`) describe the new code.
- FC-032: the roles' results were gathered at 8158833 or later, and no `src/` change follows it.
- FC-033: the fixtures are LF only. `SpecCatalog` reads with `File.ReadLines` (either separator) and normalizes path separators to `/`, so line endings and the platform cannot change a key match.
- FC-034: no bug reproduction in this run.
- FC-035: the tests (55e5b23) predate the implementation (8158833), and the expected values follow the change doc and rulebook. The test implementer's reference implementation was never committed.
- FC-037: no text-only claim is made. The lane is tooling, with one compiled test-project file.
- FC-004 to FC-014, FC-018 to FC-030, FC-036: no analyzer, code fix, formatter phase, trivia, syntax rewrite, rename, or self-hosted formatting is reachable. The change is a Markdown and C#-source scanner inside an architecture-test project.

## Hints

1. **The `shared/` qualifier and non-shared `Uses`.** Exempting a non-shared spec releases its requirements even when enforced specs rely on them. For example, exempting `formatter/cleanup.md` would release `FMT-CLEAN-001`, which 8 enforced analyzer specs list under `Uses`. The change doc gives its reason as "so exempting a shared spec never releases a requirement that an enforced spec relies on", and that reasoning fits `FMT-…` uses just as well. The case is new with this PR: `formatter/` used to be enforced as a whole. The code follows the accepted text literally, and the case is dormant while the list is empty. Changing it would be a process decision for the maintainer, not a repair. If someone fixes it, no test currently falsifies dropping the qualifier.
2. **Stale or misspelled key.** Such a key exempts nothing and fails closed. However, the report then prints `Coverage of exempted specs: no spec is exempted` while `ExemptedSpecs` is not empty. This belongs to the follow-up candidate the test implementer and implementer already named (report keys that name no spec file).
3. **`.claude/agents/spec-checker.md` description.** It still reads "verifies specs, change doc, and user documentation against the diff", the role-table sentence before this run. The approved edit changed only `squad/README.md`. The description is still true but incomplete. Aligning it would touch a file the maintainer's approval did not name.
4. **Test summary wording.** `CoverageCheckEnforcesEverySpecByDefault` (line 248) says "the areas the former opt-in list named", which refers to history that will not be in the code after merge. Naming the areas would be durable.
5. **For the PR description.** thoenissen/Reihitsu#998's done-when item "every test method is tagged or visibly exempted, and the check enforcing that runs in CI" is not shipped, by the maintainer's decision. Per that decision the PR closes #998 and must state this difference.
