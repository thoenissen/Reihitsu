Role: Lead
Commit: 036b3b4
Verdict: CI failure after the third repair round — narrowed

# Run

Issues thoenissen/Reihitsu#1136, #1134, #1135, #1262, #1332, #1336, #1340 (all `agent-filed`); slug `withhold-unsafe-fixes`; lane behavior change; branch `claude/sweet-johnson-msb5hu`; PR thoenissen/Reihitsu#1481. The run's working docs were deleted before publishing; this directory holds only the CI repair. Earlier decisions are in the PR's run report.

# CI failure

`sonarcloud` on 036b3b4 runs the self-hosting scans. `Reihitsu.SelfHosting.Test` compiles every project itself and reports for `Reihitsu.Core.Test`:

    src/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs:540 [CS0122] 'OrderingDeclarationUtilities.ChangesInitializerExecutionOrder(TypeDeclarationSyntax, MemberDeclarationSyntax, MemberDeclarationSyntax)' is inaccessible due to its protection level
    (also lines 569, 598, 625)

340 of 3836 self-hosting tests fail on that compilation error. The cause is the run's decision 15: `ChangesInitializerExecutionOrder` was made `internal` and exposed to `Reihitsu.Core.Test` through an `<InternalsVisibleTo>` item in `src/Reihitsu.Core/Reihitsu.Core.csproj`, which the self-hosting compilation does not see.

# Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 17 | CI | CI failure after the third repair round | Narrow the run: undo decision 15's visibility change for `ChangesInitializerExecutionOrder` only — it is `public` again, as on `main`, and the `<InternalsVisibleTo>` item is removed from `Reihitsu.Core.csproj`. `FieldOrderSetsLayout`, `HasInitializer`, and `IsLayoutField` stay private; no production caller uses `ChangesInitializerExecutionOrder`. | The playbook resolves a block after the third round by narrowing, not a fourth round | Internal API visibility only; no output changes | (CI repair) |
| 18 | CI | Cleaner after the narrowing | Skipped: the implementer ran `scripts/format.sh` (0 of 1 changed) and the validator runs the Release build; the change restores `main`'s visibility and `Reihitsu.Core.csproj` as on `main` | Budget; nothing for the cleaner to change | No output changes | 886f13c |
| 19 | CI audit | Finding #14 repeats #13's class (architecture finding): the public `ChangesInitializerExecutionOrder` again lets any assembly ask the member-order question with the layout check switched off | Decided under *Architecture findings* as a structural change, not a repair round: restore decision 15 in the form the self-hosting compilation sees — `ChangesInitializerExecutionOrder` `internal` again, and `[assembly: InternalsVisibleTo("Reihitsu.Core.Test")]` in a new `src/Reihitsu.Core/Properties/AssemblyInfo.cs`, as Analyzer, Formatter, and Cli declare it; supersedes decision 17 | The narrowing reopened a class three audits closed; the convention every other project uses fixes CI without that | Internal API visibility only; no output changes | (structural change) |
