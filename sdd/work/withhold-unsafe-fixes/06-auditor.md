Role: auditor
Commit: bf33ab2
Verdict: pass

Inputs: issues thoenissen/Reihitsu#1136, #1134, #1135, #1262, #1332, #1336, #1340; `00-lead.md` (decisions 17–19; 19 supersedes 17); `03-auditor.md` (finding #14 and its architecture finding); `04-implementer.md`; `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs` and `RepositoryRoot.cs`; the PR description of thoenissen/Reihitsu#1481. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 19. No test was re-run: no finding turns on a result.

## Scope

Repair delta `036b3b4..bf33ab2` (excluding `sdd/work/` and `squad/notes/`). Head `bf33ab2` checked out and clean; `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073`, an ancestor of the head. `169692d..036b3b4` carries no compiled change, so the tree is the last fully audited `169692d` plus this delta. The net compiled delta is two files and matches `04-implementer.md`:

- `src/Reihitsu.Core/Reihitsu.Core.csproj`: the `<InternalsVisibleTo Include="Reihitsu.Core.Test" />` item group removed; the file is byte-identical to `main` (`git diff origin/main..HEAD` on it is empty).
- `src/Reihitsu.Core/Properties/AssemblyInfo.cs` (new): `[assembly: InternalsVisibleTo("Reihitsu.Core.Test")]`.

`src/Reihitsu.Core/OrderingDeclarationUtilities.cs` has no net change against `036b3b4`: `886f13c` made `ChangesInitializerExecutionOrder` `public` and `6641079` made it `internal` again (line 319). `FieldOrderSetsLayout`, `IsLayoutField`, and `HasInitializer` stay `private` (lines 414, 444, 465).

Self-hosting visibility: `SelfHostingSolution.CreateCompilation` compiles `CSharpCompilation.Create(project.AssemblyName)` from `EnumerateSourceFiles`, which takes every `*.cs` under the project directory except paths containing `/bin/` or `/obj/` (`RepositoryRoot.IsInBuildOutputPath`) and `.Designer.cs`/`.g.cs`/`.g.i.cs` (`RepositoryRoot.IsGeneratedSourceFile`). `Properties/AssemblyInfo.cs` matches neither, so the attribute is in the `Reihitsu.Core` compilation. The friend name matches: `SelfHostingProject` takes `AssemblyName` from the csproj or falls back to the project name, and `Reihitsu.Core.Test.csproj` sets none, so the test compilation is `Reihitsu.Core.Test`. That is the same mechanism `Reihitsu.Analyzer` → `Reihitsu.Analyzer.Test` and `Reihitsu.Formatter` → `Reihitsu.Formatter.Test` already rely on. The implementer's self-hosting run on `6641079` (no CS0122; one failure, `WorkingDocumentsDirectoryContainsNoFiles`, caused by this run's own `sdd/work/` directory) agrees.

### Status of the earlier findings

| # | Status | Evidence |
|---|---|---|
| 14 | closed | `ChangesInitializerExecutionOrder` is `internal`; `rg ChangesInitializerExecutionOrder src --glob '*.cs'` finds the declaration, the four calls in `src/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs` (540, 569, 598, 625), and three analyzer-test method names that do not call it. The only friend assembly of `Reihitsu.Core` is `Reihitsu.Core.Test`, so no code-fix assembly can reach the bypass |
| Architecture finding of `03-auditor.md` | closed | The public second entry point that fixed the layout flag to `false` is gone from every assembly except the Core tests. The single owner (`ChangesMemberOrder` symbol overloads, layout check from the type symbol) is again the only member-order entry point a code fix can call. The PR description's "no caller can switch the layout check off" holds again |

## Findings
_None._

## Guard scope
_N/A — the delta adds or changes no predicate, guard, or exemption; only where an existing friend-assembly grant is declared changes._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member order (initializers, layout fields), counting excluded members | `rg -n "ChangesInitializerExecutionOrder\|ChangesMemberOrder\|FieldOrderSetsLayout\|IsLayoutField\|StructLayout" src --glob '*.cs'`: one implementation in `OrderingDeclarationUtilities`; public entry points are the two symbol overloads (RH7103, RH7109, `RegionBlockLayout` → RH7309); `ChangesInitializerExecutionOrder` is `internal`, called by Core tests only | One owner; #14 closed |
| Friend-assembly grants | `rg -l "<InternalsVisibleTo" --glob '*.csproj' --glob '*.props' --glob '*.targets'`: none in the repository. Every grant is an `[assembly: InternalsVisibleTo(...)]` attribute in a `src/*/Properties/AssemblyInfo.cs`, which both the MSBuild build and the self-hosting compilation see | One form; no other member of the CI failure's class (a csproj-item grant the self-hosting compilation cannot see) remains |

## Assertion adequacy
_N/A — no test is added or changed in the delta. The four Core tests that call `ChangesInitializerExecutionOrder` are unchanged from the audited `169692d` and compile against the `internal` method through the new attribute (implementer's core run: 607 passed)._

## Failure classes dismissed
- FC-001, FC-002, FC-003: no predicate or boundary changes.
- FC-015: covered by the policy-ownership rows; one owner each.
- FC-016: the delta closes #14; the sibling search (other public bypasses of the layout check, other csproj-item grants) found none.
- FC-004, FC-005, FC-019, FC-020, FC-021: no trivia or directive handling changes.
- FC-006, FC-022–FC-029, FC-036: no rewrite changes; accessibility of an internal test hook only, and no assembly is a public API (`AGENTS.md`, *Supported inputs*).
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changes; RH7103, RH7109, RH7309 still call the symbol overloads.
- FC-010–FC-014: no formatter or analyzer behavior changes.
- FC-017, FC-033–FC-035: no test changes.
- FC-031: no method body or summary changes; the summary of `ChangesInitializerExecutionOrder` does not describe accessibility and stays accurate.
- FC-032: the change cannot flip an analyzer or code-fix result; the implementer's core (607) and analyzer (6635) runs on `6641079` are the evidence.
- FC-037: no comment-only claim is made.

Code quality and conventions: the new file has the same form as `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`, `src/Reihitsu.Analyzer/Properties/AssemblyInfo.cs`, and `src/Reihitsu.Cli/Properties/AssemblyInfo.cs` (UTF-8 BOM, `using System.Runtime.CompilerServices;`, blank line, attributes, LF, no final newline), which the self-hosting scans already accept; `scripts/format.sh` changed nothing. `InternalsVisibleToAttribute` exists in `netstandard2.0`. No `TODO`, no commented-out code, no tracker number. Scope: both files are within the run's Core change; the csproj returns to `main`.

## Architecture findings
_None._ The structural change of decision 19 closes `03-auditor.md`'s architecture finding and introduces no new shared decision.

## Hints
- The PR description's lead-decisions table ends at decision 16 and its decision-15 row reads only "`ChangesInitializerExecutionOrder` internal". The CI decisions 17–19 are not in it yet, and the grant now lives in `src/Reihitsu.Core/Properties/AssemblyInfo.cs` rather than in the csproj. Not compiled; for the lead's run report.
- `WorkingDocumentsDirectoryContainsNoFiles` fails while `sdd/work/withhold-unsafe-fixes/` exists; this run's directory must be deleted before CI's self-hosting scans can go green.
