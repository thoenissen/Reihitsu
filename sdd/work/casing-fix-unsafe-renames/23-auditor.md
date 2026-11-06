Role: auditor
Commit: 20b2618e49fad03a0adf5e95bc01b763392bc696
Verdict: findings

## Scope

This audits only the repair delta against 73dbf84e28e3d8b7d0944e07979a57d3e78fa97c. That delta is the structural change of lead decision 17, which answers the architecture finding and findings #6 and #7 of `17-auditor.md`. Head 20b2618e49fad03a0adf5e95bc01b763392bc696 is checked out. Base `origin/main` 61718e254d608e0937c561c3e8839a05158f6338 is an ancestor of head, so it is merged. `scripts/lane-check.sh bug-fix` returns `confirmed`. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The delta matches the handovers `18-` to `22-`:
- One production method: `GetRenameCascadeAsync` in `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`. It now reads the definitions of `SymbolFinder.FindReferencesAsync` instead of walking overrides and implementations by hand. Its summary and remark changed with it.
- Four RH4121 tests.
- SHARED-CASING-007 and SHARED-CASING-009 in `sdd/specs/shared/casing.md`.
- `sdd/work/`.

Scope did not grow beyond the casing fix base and its spec.

Evidence. All of it is under `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad/`, and all of it was built from head:
- `harness/run3.cs` runs the real code fix.
- `h3/cmp.cs` is a new probe. For every declaration of a name, it compares the source definitions that `FindReferencesAsync` returns with the declarations the Roslyn renamer actually renames.
- `r1/nevercheck.cs` compiles a fixture with `NEVER` defined.

I trusted the roles' focused test results and the cleaner's build. Evidence that is byte-identical to the earlier audits is reused.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–5 | closed | Closed in `17-auditor.md`. The delta does not touch their guards' code paths. It changes only where the cascade comes from, and the native-entry-point comparison stays symmetric (see *Guard scope*). |
| 6 | closed | Every member the enumeration listed holds now. I reran `r1/pm` and `r1/pmr2`: no action is offered on either `Key`, in either direction. The new-name arm is pinned by `NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodNamesTheNewName`. A member the enumeration did not name still fails: finding #8. |
| 7 | closed | SHARED-CASING-007 now states the cascade once, as the renamer's own determination, and gives the kinds only as examples. SHARED-CASING-009's third bullet and its `Why:` refer to that statement. No closed list remains in `casing.md`. The bullet's "the old name" is part of finding #8. |

## thoenissen/Reihitsu#1070

### Findings

None new. The native-entry-point check takes its cascade from the reference search both before and after the rename. Both sides drop metadata symbols, and the entry points are compared as sorted lists, so the order in which the search returns them cannot matter. The extra symbols that the search returns (accessors, backing fields, constructors) can only make the check withhold a fix, which is the safe direction. For example, an `extern` property accessor whose entry point is its `get_…` name would now be withheld, as SHARED-FIX-003's run-time bullet requires.

### Against the specs

SHARED-FIX-003 run-time bullet, RH4103 and RH4104-002: unchanged, and they hold.

## thoenissen/Reihitsu#1073

### Findings

See `## Whole run`, finding #8.

### Against the specs

- **Q4: SHARED-CASING-007 and SHARED-FIX-003, build configurations.**
  - **Document region.** This holds for every relation I probed. In each one, the `FindReferencesAsync` source definitions include every declaration the renamer renamed (`h3/cmp.cs`):
    - the type parameter of a partial method;
    - partial method, partial indexer and partial constructor parameters;
    - partial property and partial event;
    - an interface member, with its implicit, explicit and sibling implementations;
    - an override chain, including `new virtual` hiding, where both sides correctly stop;
    - a generic override's method type parameter, which is not cascaded by either side;
    - a record positional parameter and its property;
    - a derived record's matching parameter, which is not cascaded by either side;
    - a type with its constructors and destructor;
    - a partial type's type parameter;
    - an extension-block receiver and its property;
    - lambda, local-function and deconstruction symbols.

    The extra definitions are the accessors, the backing fields, and the constructors of a renamed type. Each of them has a reference scope equal to or narrower than the symbol that owns it, so the region does not grow beyond the documents the renamed symbols already give.
  - **Old-name axis.** This fails (finding #8). The renamer renames the other part's parameter of a partial method or a partial indexer even when that part spells the parameter differently. The check, however, searches excluded code only for the reported symbol's own old name. SHARED-CASING-007's bullet says the same ("neither the old name nor the new name"), so the spec has the same gap.
- **Q1 and Q5.** The delta does not touch them.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | high | `CasingCodeFixProviderBase{T}.cs`, `PrepareRenameAsync`, which passes `declaredSymbol.Name` as the only old name to `HasExcludedCodeOccurrenceAsync`; `sdd/specs/shared/casing.md`, SHARED-CASING-007, excluded-code bullet ("Neither the old name nor the new name") | FC-001, FC-016, FC-020, FC-024 | #6 (class of #2) | same requirement | See *Finding #8 in detail* below. | Take the old names from the cascade, not from the reported symbol: the `Name` of every cascade symbol that can be referred to by name (`CanBeReferencedByName`), or, more directly, the identifiers the renamer replaced. Then scan the region for any of them and for the new name. The spec author restates 007's bullet as "neither the old name of a symbol of the rename cascade nor the new name". Add an RH4105 test for both directions of the partial-method parameter, and one for a partial indexer. The owner re-derives this after the lead decides the architecture finding. |

#### Finding #8 in detail

**The defect.** The renamer renames the parameter in both parts of a partial method or a partial indexer, even when the two parts spell the parameter differently. That is legal C#, and it compiles without error. The excluded-code check looks only for the reported parameter's own old name and for the new name. A reference to the other part's spelling in excluded code is therefore left behind.

**Counterexample (run, RH4105, `r4/pp`).**

Document A:
```csharp
public partial class C
{
    public void Use()
    {
        Run(1);
    }

    partial void Run(int Arg);
}
```

Document B:
```csharp
public partial class C
{
    partial void Run(int other)
    {
#if NEVER
        other++;
#endif
    }
}
```

- The fix on `Arg` is offered.
- It renames both parts to `arg`, and B keeps `other++` in its excluded branch.
- With `NEVER` defined, the input compiles without error. After the fix, it fails with CS0103 `'other'` at B(6,9).
- SHARED-FIX-003 (build configurations) withholds this fix.

**Reverse direction (run, `r4/ppr`).**
- Document A carries `#if NEVER [System.Obsolete(nameof(Arg))] #endif` on `partial void Run(int Arg);`. Document B is `partial void Run(int Other) { }`.
- The fix on B's `Other` is offered. It renames A's `Arg` to `other` and leaves `nameof(Arg)` behind.
- With `NEVER` defined, the code compiles without error before the fix and fails with CS0103 `'Arg'` at A(9,29) after it.
- The fix on A's `Arg` is withheld, as it should be.

Fix All shares `RenameWithoutConflictAsync` and fails the same way.

**Class members.** The axis is a declaration that the renamer renames whose spelling differs from the reported symbol's spelling, crossed with which part's excluded code names it.
- **Fails:** a partial-method parameter, defining to implementing part (run) and implementing to defining part (run).
- **Fails:** a partial-indexer parameter, `this[int Index]` against `this[int position]` (run, `r4/pidx`). The fix on `Index` renames both parts to `index` and leaves `return position;` behind.
- **Fails, derived and not constructed:** a partial-constructor parameter.
  - The probe shows the renamer renames both parts.
  - RH4105 reported no diagnostic for a partial constructor in my harness, so I did not run the fix.
- **Holds:** a partial-method type parameter spelled differently (run, `r4/ptp`). The renamer renames only the reported part, so the other part's spelling stays valid.
- **Holds:** every same-spelled relation in *Against the specs*, because there the reported name is the only old name.
- **Holds:** override and implementation parameters, which the renamer does not cascade (probe).
- **Holds:** the new-name arm, because one new name is used everywhere.

This is the same decision as findings #2 and #6: which code the excluded-code check must search so that it covers what the renamer renames. It fails between the same components, the guard and the renamer. It differs only in the axis: the old names instead of the documents. The guard-scope row "Old name in excluded code … agree" of `17-auditor.md` was wrong for differently spelled partial parts, so this is a member that the earlier enumeration did not name.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Region of the excluded-code check (SHARED-CASING-007, SHARED-FIX-003 build configurations) | union of `GetReferringDocuments` over the source definitions of `FindReferencesAsync(symbol)` | documents where a symbol the renamer renames can be referred to | agree. The definitions are a superset of the renamed declarations in every probed relation, and the extra definitions do not widen the region | holds |
| Old names searched in excluded code | `declaredSymbol.Name` only | the old identifier of every declaration the renamer renames | disagree for differently spelled partial-method, partial-indexer and partial-constructor parameters | finding #8 |
| Native entry point (SHARED-FIX-003 run time) | sorted entry points of the cascade before the rename and of the cascade recomputed after it | methods the renamer renames that carry an import binding | agree. The extra symbols are compared symmetrically, and a binding taken from an accessor name now withholds the fix, which is correct | holds |
| Added compile error (SHARED-CASING-009) | unchanged: errors of the changed projects and their dependents | unchanged | agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rename cascade: which symbols are renamed | Roslyn (truth). The code has one reader of it, `GetRenameCascadeAsync`: `rg -n "FindReferencesAsync\|FindOverridesAsync\|FindImplementationsAsync\|FindImplementedInterfaceMembersAsync" src --glob '!*.Test*/**'` finds no hand-written walk. The spec has one statement of it, in SHARED-CASING-007, and 009 refers to it. | one owner |
| Old names a rename replaces | `PrepareRenameAsync` passes `declaredSymbol.Name`; SHARED-CASING-007 says "the old name". Both silently assume that every cascade member shares the reported spelling. | a model of the renamer's reach that drifts from it: finding #8 and the architecture finding |
| What a rename renames, described per rule | `RH4103.md`:123, RH4112, RH4113 and RH4114 fix requirements ("Renaming a property also renames, in source, …"). These predate the run and are not in the diff. | They describe the output and bound no guard, and none of them claims to be exhaustive. See *Hints*. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodReferencesTheTypeParameter` | No fix on either `Key` when the implementing part's excluded code names `Key`. The defining part's fix is withheld only through the cascade. | Either part is renamed. | `Verify` with two documents; source passed as the fixed state, with a single fix and three Fix All scopes | adequate. It failed on 73dbf84 (`19-test-implementer.md`). |
| `NoFixIsOfferedWhenAnExcludedBranchOfTheDefiningPartOfAPartialMethodReferencesTheTypeParameter` | The same in the reverse direction. The implementing part's fix is withheld only through the cascade. | Either part is renamed. | the same | adequate |
| `NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodNamesTheNewName` | The new-name arm across the partial relation. | Either part is renamed. | the same | adequate |
| `FixRenamesTheTypeParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt` | The fix is offered when an excluded branch names neither name. Both parts and the parameter's type are renamed, and the excluded text is unchanged. | The fix is withheld, or a part or the excluded text is wrong. | full text of both documents | adequate |
| Missing | Differently spelled partial parameters, in both directions, and the partial indexer. | — | — | missing (finding #8) |

### Visible output is decided

- **Withheld fixes.** The three new withheld-fix tests are decided by SHARED-FIX-003 (build configurations) under P1 (lead decision 17).
- **Offered fix.** The offered test asserts the renamer's output on `main`, which the unchanged rename clause of SHARED-CASING-007 demands. Its new example equals the tests.
- **Rule pages.** No rule page changed.

Nothing undecided.

### Failure classes dismissed

- **Unchanged from `17-auditor.md`.** The delta adds no edit path, so these still hold: FC-004, FC-005, FC-007, FC-009 to FC-014, FC-018, FC-019, FC-021, FC-022, FC-025 to FC-030, FC-032, FC-036 (the `[CallerArgumentExpression]` member is already a follow-up, decision 18), FC-037.
- **FC-003.** The new tests cover both sides: the old name named, the new name named, neither named, and both directions.
- **FC-006.** No rewrite is added; the renamer's output is unchanged.
- **FC-008.** Fix All shares the changed path. The new tests run three Fix All scopes. Finding #8 fails in Fix All the same way.
- **FC-015.** Covered by *Policy ownership*: one cascade owner. The old-name model is finding #8.
- **FC-017.** Covered by *Assertion adequacy*.
- **FC-023.** Verbatim and Unicode identifiers: `ValueText` comparison, unchanged.
- **FC-031.** The summary and remark of `GetRenameCascadeAsync` describe the new code. The extra definitions (accessors, backing fields, constructors) are symbols whose names change with the rename, so "every symbol … that a rename of it renames with it" is accurate. The `RenameWithoutConflictAsync` and `PrepareRenameAsync` summaries still hold. There is no `TODO` and no tracker number.
- **FC-033, FC-034 and FC-035.** The tests use the auditor's shapes verbatim, and the expected values come from the spec example.

### Architecture findings

**The class of findings #2 and #6 repeated a third time. The excluded-code check still predicts part of the renamer's reach instead of reading it.**

Components:
- the Roslyn renamer, which decides which declarations a rename changes and what each of them was called;
- `CasingCodeFixProviderBase<T>.PrepareRenameAsync` and `HasExcludedCodeOccurrenceAsync`;
- SHARED-CASING-007's excluded-code bullet.

Shared decision: which identifiers the rename replaces, and therefore which old names excluded code must not contain.

Decision 17 moved the document region onto Roslyn's own search, and that axis now holds. The old-name set is still a model: `declaredSymbol.Name`, under the unstated assumption that every cascade member is spelled like the reported symbol. Differently spelled partial parameters break that assumption (finding #8).

A structural change within the run's component would derive both axes from the same reading of Roslyn:
- the old names from the cascade symbols that can be referred to by name, or from the identifiers the renamer replaced;
- the documents from the cascade, as now.

The spec would state the condition over the cascade's names. The lead decides.

### Hints

- **Cost, a delta to the hint of `17-auditor.md` (decision 18).** Every rename evaluation now runs two full-solution `FindReferencesAsync` searches in place of the hierarchy walks. Fix All repeats this for every candidate. Not measured.
- **Per-rule restatements, pre-existing and not in the diff.** `RH4103.md`:123 and the RH4112, RH4113 and RH4114 fix requirements each describe in their own words what a rename also renames. None of them mentions partial parts. They bound no guard and are not exhaustive, so nothing fails today. They are, however, further statements of the renamer's reach that could drift from SHARED-CASING-007.
- **Not constructed.** RH4105 reported no diagnostic for a partial-constructor parameter in my harness. I did not find out why. The rename probe shows that the renamer renames both partial-constructor parts.

### Commands
```shell
scripts/lane-check.sh bug-fix                                        # LANE CHECK: confirmed
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pm Key RH4121    # A.cs Key: 0 actions; B.cs Key: 0 actions (finding 6 closed)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pmr2 Key RH4121  # A.cs Key: 0 actions; B.cs Key: 0 actions
dotnet run <scratch>/h3/cmp.cs -- <scratch>/r3/<ppar|iface|ovr|rec|types> <name>   # FAR source definitions ⊇ renamed declarations, every probe
dotnet run <scratch>/h3/cmp.cs -- <scratch>/r3/pdiff Arg              # renamed: Parameter A.cs and B.cs (B spelled Other)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/pp Arg RH4105    # 1 action; B keeps excluded `other++`
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/ppr Other RH4105 # B.cs Other: 1 action; A keeps excluded `nameof(Arg)`; A.cs Arg: 0 actions
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/pidx Index RH4105 # 1 action; B keeps excluded `return position;`
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/ptp Key RH4121   # 1 action; only A renamed, B's TItem intact (holds)
dotnet run <scratch>/r1/nevercheck.cs -- <scratch>/r4/pp/*.cs         # NEVER defined: no error
dotnet run <scratch>/r1/nevercheck.cs -- <scratch>/r4/pp_after/*.cs   # CS0103 'other' at B.cs(6,9)
dotnet run <scratch>/r1/nevercheck.cs -- <scratch>/r4/ppr_after/*.cs  # CS0103 'Arg' at A.cs(9,29)
```
`<scratch>` is `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad`.
