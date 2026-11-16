Role: test implementer
Commit: db6aae6 (tests written on top of it; see the commit that adds this file)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. In this tests lane the test code is the whole change: the checks themselves (helper methods) live in the two test classes, written test-first.

## thoenissen/Reihitsu#1507

File: `src/Reihitsu.ArchitectureTests/Formatter/PhaseOwnershipDocumentationTests.cs`.

Check (`FindUnresolvedGuardNames`), per lead decisions 1, 4, 5:
- reads only the third cell of each body row of the table between the `formatter-phase-ownership` markers; a `|` inside a backticked span does not split a cell;
- skips a span with no letter or containing `/` (`?`, `:`, the spec path); any other span must be a plain or dotted name, or it fails ("is no type or member name");
- resolves against syntax declarations (not text) of `Reihitsu.Formatter` and every project reachable through its `ProjectReference` items (today `Reihitsu.Core`), outside `bin`/`obj`; a plain name resolves as a type, a member, or a `SyntaxKind` member; `Type.Member` needs the member on that type (partial parts merged); a name behind `Type.A`/`B` is checked as a plain name;
- reports every unresolved span at once, naming its phase row.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryGuardNameInOwnershipMapResolves` (real `AGENTS.md`) | — | fixture-free | pass |
| `GuardCheckAcceptsNamesDeclaredInFormatter` (type, member, `Type.Member` method/field/enum member) | FC-003 | `SpecFixtureDirectory` | pass |
| `GuardCheckAcceptsMemberOfAnotherPartialPart` | FC-003 | same | pass |
| `GuardCheckAcceptsNamesDeclaredInReferencedProject` (Core-only, plain and qualified) | FC-003 (scope boundary) | same | pass |
| `GuardCheckAcceptsNameDeclaredInTransitivelyReferencedProject` | FC-003 | same | pass |
| `GuardCheckReportsNameDeclaredOnlyInUnreferencedProject` (`CanRewrite` in a code-fix project) | FC-017, FC-034 | same | pass |
| `GuardCheckReportsNameOfProjectTheFormatterDoesNotReference` | FC-003 | same | pass |
| `GuardCheckReportsUndeclaredName` (`IsAfterSourceOnSameLine`, message names span and phase) | — | same | pass |
| `GuardCheckReportsNamesOnlyInCommentOrString` | FC-017 | same | pass |
| `GuardCheckReportsNameDeclaredOnlyInTestProject` | — | same | pass |
| `GuardCheckReportsNameDeclaredOnlyInBuildOutput` (`obj/`) | — | same | pass |
| `GuardCheckReportsMemberOfAnotherType` | FC-003 | same | pass |
| `GuardCheckReportsMemberOfTypeOutsideScope` | FC-003 | same | pass |
| `GuardCheckResolvesSyntaxKindMembers` (`EndOfLineTrivia`/`XmlTextLiteralNewLineToken` pass, `EndOfLineTrivai` fails) | FC-003 | same | pass |
| `GuardCheckSkipsTokensAndPaths` | FC-003 | same | pass |
| `GuardCheckReportsDecoratedName` (`IsGuarded()`) | FC-003 | same | pass |
| `GuardCheckChecksSlashContinuedNamesAsPlainNames` | FC-003 | same | pass |
| `GuardCheckReportsEveryUnresolvedName` (three names over two rows) | — | same | pass |
| `GuardCheckReadsOnlyTheGuardColumn` (undeclared names in phase/decision columns, prose, and a table outside the markers) | FC-003 | same | pass |

No test carries a `[Requirement]` tag: no spec requirement covers the table (spec author).

Watched failing for the right reason (temporary mutations, all reverted):
- scope widened to every project under `src/`: the five scope tests fail, and the real check with `CanRewrite` and `IsAfterSourceOnSameLine` inserted into the `CleanupPhase` row reports only `IsAfterSourceOnSameLine`;
- with the correct scope and the same two names inserted, the real check fails naming both, under `CleanupPhase`;
- skip rule widened to every non-name and `SyntaxKind` lookup disabled: `GuardCheckResolvesSyntaxKindMembers`, `GuardCheckReportsDecoratedName`, and the real check (on `EndOfLineTrivia`) fail.

`AGENTS.md` is not edited; every one of its current guard names resolves.

## Not covered, and why
- Column 2 and names outside the marked table are non-goals (spec author); `GuardCheckReadsOnlyTheGuardColumn` pins that they are ignored.
- Whether a guard is used by its own phase: non-goal.
- Local functions, constructors, and operators are not collected as members; no current guard is one. Adding one later fails loudly rather than passing silently.
- LF/CRLF: `ExtractOwnershipMap` already normalizes CRLF to LF before parsing; no layout is involved.

## thoenissen/Reihitsu#1428

File: `src/Reihitsu.ArchitectureTests/Sdd/RequirementCoverageTests.cs`.

Changes:
- new check `FindExemptionsWithoutSpecFile` and real-data test `EveryExemptionNamesASpecFile`: a key must equal, ordinally, the forward-slash relative path of a file `SpecCatalog.EnumerateSpecFiles` yields (decision 2);
- `IsEnforced` drops the `shared/` condition; `SharedDirectory` removed; `ReadSharedIdsUsedByEnforcedSpecs` renamed `ReadIdsUsedByEnforcedSpecs`; the class, `ExemptedSpecs`, `FindUncoveredRequirements`, and `IsEnforced` summaries updated (FC-031);
- `DescribeCoverageOfExemptedSpecs` builds one line per exemption key, so "no spec is exempted" prints only for an empty list (decision 6).
- `ExemptedSpecs` stays empty.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `EveryExemptionNamesASpecFile` (real, empty list) | — | repository | pass |
| `CoverageCheckEnforcesExemptedRequirementsOutsideSharedThatAnEnforcedSpecUses` (`formatter/cleanup.md` exempted, enforced `analyzer/RH6006.md` uses FMT-CLEAN-001: reported; unused FMT-CLEAN-002 released) | FC-003 | `SpecFixtureDirectory` | pass |
| `CoverageCheckDoesNotEnforceExemptedRequirementsOutsideSharedThatOnlyAnExemptedSpecUses` | FC-003 | same | pass |
| `CoverageCheckDoesNotEnforceExemptedRequirementsOutsideSharedThatNoSpecUses` | FC-003 | same | pass |
| `ExemptionCheckReportsMisspelledSpecPath` (`formatter/cleanu.md`) | — | same | pass |
| `ExemptionCheckReportsDirectoryKey` (`formatter/`) | — | same | pass |
| `ExemptionCheckReportsDirectoryKeyWithoutSlash` (`formatter`) | — | same | pass |
| `ExemptionCheckReportsSpecIndexKey` (`README.md`) | — | same | pass |
| `ExemptionCheckReportsLedgerKey` (`requirement-ids.txt`) | — | same | pass |
| `ExemptionCheckReportsKeyInOtherCase` (`Formatter/Cleanup.md`) | — | same | pass |
| `ExemptionCheckReportsKeyWithBackslash` (`formatter\cleanup.md`) | — | same | pass |
| `ExemptionCheckAcceptsSpecPaths` (nested and top-level) | FC-003 | same | pass |
| `ExemptionCheckReportsEveryKeyWithoutSpecFile` (two bad keys reported, the valid one not) | — | same | pass |
| `ExemptionSpecFileCheckAcceptsEmptyList` | — | same | pass |
| `CoverageReportNamesExemptedSpecWithoutActiveRequirement` (`0 of 0 uncovered`, no "no spec is exempted") | — | same | pass |
| `CoverageReportStatesNoSpecIsExemptedForEmptyList` | FC-003 | same | pass |

No `[Requirement]` tags: `sdd/README.md` is the rulebook, not a spec.

Watched failing before the change (old `IsEnforced` and report, stub key check returning nothing): the enforcement test outside `shared/`, the nine key tests that expect a finding, and the report test failed on their own assertions; the two "does not enforce" tests and the accepting tests passed, as they must on both sides.

Existing tests asserting changed output, checked one by one:
- `CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses`, `CoverageCheckDoesNotEnforceExemptedSharedRequirementsThatOnlyAnExemptedSpecUses`: left unchanged, still pass under the wider rule.
- `CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory`: left unchanged (asserts only coverage findings).
- `CoverageReportCountsUncoveredRequirementsPerExemptedSpec`, `CoverageReportNamesNoSpecWhenNothingIsExempted`: left unchanged, still pass with the key-built report.
- `EveryExemptionStatesAReason` and the `ExemptionCheckReports*Reason` tests: unchanged.

## Not covered, and why
- `SpecStructureTests` (`Uses` validation) is a non-goal and unchanged.
- LF/CRLF: no layout; the spec reader is line-based and unchanged.

## Commands
```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~PhaseOwnershipDocumentationTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```
Full project: 153 passed.

## Follow-up candidates
_None._ The `SpecFixtureDirectory` summary still says it serves `SpecConsistencyTests` only, while three test classes (now four) use it; trivial, left for the cleaner.
