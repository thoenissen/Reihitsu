Role: auditor
Commit: 23cf14c
Verdict: findings

Inputs: issues thoenissen/Reihitsu#1136, #1134, #1135, #1262, #1332, #1336, #1340; `00-lead.md` (decisions 17 and 18); `01-implementer.md`; the run's earlier audits as committed in history (`24-auditor.md` at d3cf852, `29-auditor.md` at ecca33f); the PR description of thoenissen/Reihitsu#1481 (decision 15 and the *Shared Core helpers* claim); `src/Reihitsu.SelfHosting.Test/Utilities/SelfHostingSolution.cs`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 17. No test was re-run: no finding turns on a result.

## Scope

CI-repair delta `036b3b4..23cf14c` (excluding `sdd/work/` and `squad/notes/`). Head `23cf14c` checked out and clean; `origin/main` = `a59a003f6a9f85126892eac7646fc8945a4b9073`, an ancestor of the head. The compiled diff is two hunks and matches `01-implementer.md`:

- `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:319`: `ChangesInitializerExecutionOrder` `internal` → `public`.
- `src/Reihitsu.Core/Reihitsu.Core.csproj`: the `<InternalsVisibleTo Include="Reihitsu.Core.Test" />` item group removed; the file is now byte-identical to `main` (`git diff origin/main..HEAD -- src/Reihitsu.Core/Reihitsu.Core.csproj` is empty).

Decision 17 is carried out as stated: the method is `public` with `main`'s signature, the item is gone, `FieldOrderSetsLayout`, `HasInitializer`, and `IsLayoutField` stay `private` (lines 414, 444, 465), and the file has no `internal` member left. The CI cause is confirmed: `SelfHostingSolution.CreateCompilation` builds each project from its `*.cs` files with `CSharpCompilation.Create(project.AssemblyName)` and never reads csproj items, so a csproj `InternalsVisibleTo` item is invisible to it, while the `[assembly: InternalsVisibleTo(...)]` attributes in `Properties/AssemblyInfo.cs` of `Reihitsu.Analyzer`, `Reihitsu.Formatter`, and `Reihitsu.Cli` are compiled with the source.

### Status of the earlier findings

| # | Status | Evidence |
|---|---|---|
| 13 | closed | `HasInitializer` and `IsLayoutField` stay `private`; the delta does not touch them. The class #13 belongs to is reopened by a different member, reported as #14 |
| Architecture finding of `18-auditor.md` | superseded by #14 | `24-auditor.md` closed it with "`ChangesInitializerExecutionOrder` is `internal` and called only by `Reihitsu.Core.Test`. No code fix can choose the sets any more." The first half no longer holds |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 14 | medium | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:319` (`ChangesInitializerExecutionOrder`) | FC-015 | #13 | PR-introduced (relative to the published `036b3b4`; the signature is `main`'s, but on `main` no layout check existed to bypass) | The narrowing reopens the enforcement half of the single-owner decision. `ChangesInitializerExecutionOrder` returns `ChangesMemberOrder(typeDeclaration, false, …)`: the member-order question with the layout check switched off, now callable from every assembly, including `Reihitsu.Analyzer.CodeFixes`. A code fix that calls it decides the layout check itself (always off), which decisions 12 and 15 ruled out and which the closure of `18-auditor.md`'s architecture finding and of #13 rested on ("no building block reachable from another assembly"). No production code calls it today (`rg ChangesInitializerExecutionOrder src --glob '*.cs'`: the declaration, four calls in `Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs:540,569,598,625`, and three analyzer-test method names), so no output is wrong now; the guarantee is held only by the XML summary's "no code fix asks this question". The concrete exposure: RH7102 is the one `TypeMemberOrderingCodeFixProviderBase` consumer without the owner call (follow-up #1595); a fix there that reached for this method, as RH7103 and RH7109 did on `main`, would move a field of a `[StructLayout(LayoutKind.Sequential)]` struct past another instance field and change its layout with no test failing. The PR description's "no caller can switch the layout check off" and its decision-15 row are now inaccurate | Either keep decision 15 and fix CI the way the other projects do — `[assembly: InternalsVisibleTo("Reihitsu.Core.Test")]` in a `src/Reihitsu.Core/Properties/AssemblyInfo.cs`, which the self-hosting compilation sees (same mechanism as `Reihitsu.Analyzer` → `Reihitsu.Analyzer.Test`) — or remove the bypass: delete `ChangesInitializerExecutionOrder` and let its four Core tests call the public symbol overload with the symbol of a non-layout class from a small `CSharpCompilation`. If the lead keeps the method public under the narrowing, record that as an accepted design gap and correct the PR description's claim and decision-15 row |

## Guard scope
_N/A — the delta adds or changes no predicate, guard, or exemption; only the accessibility of an existing method changes._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Member order (initializers, layout fields), counting excluded members | `rg -n "ChangesInitializerExecutionOrder\|ChangesMemberOrder\|FieldOrderSetsLayout\|IsLayoutField\|StructLayout" src --glob '*.cs'`: one implementation, the private `ChangesMemberOrder(…, bool fieldOrderSetsLayout, …)` overloads in `OrderingDeclarationUtilities`; public entry points are the two symbol overloads (callers RH7103, RH7109, `RegionBlockLayout` → RH7309) and, again, `ChangesInitializerExecutionOrder` (callers: Core tests only). No code outside `OrderingDeclarationUtilities` mentions `StructLayout` or the layout flag | One implementation, but a second public entry point that fixes the layout flag to `false` (#14) |

## Assertion adequacy
_N/A — no test is added or changed in the delta. The four Core tests that call `ChangesInitializerExecutionOrder` compile again against the `public` method; their assertions are unchanged from the audited `169692d`._

## Failure classes dismissed
- FC-001, FC-002, FC-003: no predicate or boundary changes.
- FC-016: the delta closes no reported instance; the sibling search for other bypasses is the policy-ownership row above (none besides #14).
- FC-004, FC-005, FC-019, FC-020, FC-021: no trivia or directive handling changes.
- FC-006, FC-022–FC-029, FC-036: no rewrite changes. FC-036 (public API): no assembly is a public API (`AGENTS.md`, *Supported inputs*); the restored signature is `main`'s, so nothing outside the repository can observe a difference.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changes; RH7103, RH7109, RH7309 still call the symbol overloads.
- FC-010–FC-014: no formatter or analyzer behavior changes.
- FC-017, FC-033–FC-035: no test changes.
- FC-031: the XML summary of `ChangesInitializerExecutionOrder` (lines 305–318) still describes the code: it ranks as `ChangesMemberOrder` does without the layout check and names the overload a code fix should call. It does not describe accessibility, so the visibility change leaves it accurate.
- FC-032: the change is accessibility only; it cannot flip an analyzer or code-fix result. The implementer's core (607) and analyzer (6635) runs on `886f13c` and the self-hosting run without the CS0122 error are the evidence.
- FC-037: no comment-only claim is made.

No `TODO`, no commented-out code, no tracker number in the delta. Scope: both files are within the run's Core change; the csproj returns to `main`.

## Architecture findings
`OrderingDeclarationUtilities.ChangesMemberOrder` (the single owner of the member-order decision, including the layout check from the type symbol) and the public `OrderingDeclarationUtilities.ChangesInitializerExecutionOrder` both answer the member-order question for a code-fix assembly, and the second answers it with the layout check decided by the caller's choice of method rather than by the type symbol. Shown by the declaration at `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:319` together with its accessibility; no current input shows a disagreement, because no code fix calls the second entry point. This is #14, the class of #13 (the member-order comparison reachable from another assembly around the symbol-based layout decision) on a member #13 did not name, and it reopens the closure of `18-auditor.md`'s architecture finding. Under `squad/README.md`, *Architecture findings*, the lead decides it.

## Hints
- The run's earlier `24-auditor.md` hint called the csproj-item form of `InternalsVisibleTo` "a style difference only". It was not: the self-hosting compilation does not read csproj items, which is what failed CI. A `Properties/AssemblyInfo.cs` attribute, as in the other projects, is the form that both builds see.
