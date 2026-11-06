Role: auditor
Commit: 73dbf84e28e3d8b7d0944e07979a57d3e78fa97c
Verdict: findings

## Scope

This audits only the repair delta against 98349b99483c672e31034a0366e9e2a2d1040707. That delta is the structural change of lead decisions 13–16 and the repairs of decision 14. Head 73dbf84e28e3d8b7d0944e07979a57d3e78fa97c is checked out. Base `origin/main` 61718e254d608e0937c561c3e8839a05158f6338 is an ancestor of head, so it is merged. `scripts/lane-check.sh bug-fix` returns `confirmed`.

The delta matches the handovers:
- one production file, `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`;
- tests in RH4103, RH4105, RH4106 and RH4121;
- the SHARED-CASING-007 clarification in `sdd/specs/shared/casing.md`;
- `sdd/work/`.

Scope did not grow beyond the casing fix base and its spec. My prompt carried no intended fix or verdict.

Evidence:
- The scratch harness of the first audit, generalized to any casing rule (`harness/run3.cs`). It runs the rule's analyzer and calls `RegisterCodeFixesAsync` on each diagnostic in an `AdhocWorkspace`, built from head. When the fix is offered, it prints the changed documents.
- A check that compiles the fixtures with `NEVER` defined (`r1/nevercheck.cs`).
- A probe of `SymbolFinder.FindReferencesAsync` definitions (`harness/defs.cs`).

All of these files are under `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad/`. I trusted the roles' focused test results. Evidence that is byte-identical to the first audit is reused.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member the enumeration listed is now pinned by a test and holds: `static abstract` and `static virtual` with `[DllImport]`; `nameof(self)`; `[LibraryImport]`; a dependent project; the literal `EntryPoint` side. `GetNativeEntryPoints` compares the ordered entry points of the whole cascade before and after the rename. |
| 2 | closed | Every listed member is pinned and holds: file-local override, private nested override, private nested implementation, excluded declaration of the new name, base in a referenced project, plus the offered sides. A member the enumeration did not name still fails: finding #6. |
| 3 | closed | `GetCompileErrorsAsync` takes `Compilation.GetDiagnostics` of every affected project. The query, deconstruction, `foreach` and `await` cases, and the dependent-project query, are pinned and hold. |
| 4 | closed | Tests now cover the parameter, type parameter and partial-part private field branches, and the offered side for an unrelated type in another file. |
| 5 | closed | The summaries of `HasRenameConflictAsync` and `FixAllCandidate.ContainingSymbol`, and the other changed summaries, describe the new code. |

## thoenissen/Reihitsu#1070

### Findings

None new. Findings #1 and #3 are closed. The native-entry-point and compile-error checks hold for every member I traced; see *Guard scope*.

### Against the specs

- **SHARED-FIX-003, run-time bullet, and RH4104-002.** These hold for the declared method and for every override and implementation in its cascade, also across projects. A `[LibraryImport]` partial definition keeps its attribute because Roslyn merges partial-method attributes, and the generator's implementation part needs no separate check.
- **SHARED-CASING-009.** This holds. Every error of every affected project is compared, both before and after the rename.

## thoenissen/Reihitsu#1073

### Findings

See `## Whole run`, findings #6 and #7.

### Against the specs

- **Q4, SHARED-CASING-007 and SHARED-FIX-003 (build configurations).**
  - This holds for override and implementation cascades.
  - It fails for the cascade between the two parts of a partial method's type parameter (finding #6). The renamer applies that cascade, but `GetRenameCascadeAsync` does not model it.
  - SHARED-CASING-007 now lists the cascade as a closed list that leaves this relation out, while SHARED-CASING-009 lists it open with "such as" (finding #7).
- **Q1 and Q5.** The delta does not touch these. The positional-property and namespace checks are unchanged.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | high | `CasingCodeFixProviderBase{T}.cs`, `GetRenameCascadeAsync`, and `GetReferenceScope` type-parameter branch (`return symbol.ContainingSymbol`) | FC-001, FC-016, FC-020, FC-024 | #2 | same requirement | See *Finding #6 in detail* below. | Take the cascade from Roslyn's own reference engine instead of modelling it: the definitions of `SymbolFinder.FindReferencesAsync(symbol, solution)` (`ReferencedSymbol.Definition`). These are the cascaded symbols the renamer uses. On my probes they gave the partial parts (`Key @A.cs`, `Key @B.cs`), override and base (q4a), and interface and private nested implementation (q4c). Feed both the excluded-code region and `GetNativeEntryPoints` from it. Add an RH4121 test for both directions of the partial-method type parameter. Also correct the `GetRenameCascadeAsync` summary and remark. |
| 7 | medium | `sdd/specs/shared/casing.md`, SHARED-CASING-007, excluded-code bullet; compare SHARED-CASING-009, third bullet | FC-002, FC-015 | #2 | same requirement | See *Finding #7 in detail* below. | State the cascade once, as "every member the renamer renames with the symbol". Give its kinds only as examples ("for example, a member it overrides, …, or the other part of a partial method"). Have 009 point to that statement. The spec author re-derives the wording after the lead decides the architecture finding. |

#### Finding #6 in detail

**The defect.** The excluded-code region is now taken from `GetRenameCascadeAsync`. That method is a hand-written model of the renamer's cascade, covering overrides and interface implementations only. The renamer cascades further.

The case that fails:
- A method type parameter of a `partial` method is a separate symbol in each part; `12-implementer.md` calls the parts "the same symbol", which is not correct.
- Roslyn's renamer renames both parts' type parameters.
- `GetReferenceScope` returns only the reported part's method, so the other part's document, where excluded code can name the type parameter, is never scanned.

**Counterexample (run, RH4121).**

Document A:
```csharp
public partial class C
{
    public void Use()
    {
        Run<int>(1);
    }

    partial void Run<Key>(Key value);
}
```

Document B:
```csharp
public partial class C
{
    partial void Run<Key>(Key value)
    {
#if NEVER
        Key copy = value;
#endif
    }
}
```

- The fix on `Key` in A is offered.
- It renames both parts to `TKey` and leaves `Key copy` in B.
- Compiled with `NEVER` defined, the code is error-free before the fix and fails after it with CS0246 at B(6,9).
- SHARED-FIX-003 (build configurations) withholds this fix. The fix on B's `Key` is withheld, as it should be.

**Reverse direction (run).** Document A carries `#if NEVER [System.Obsolete(nameof(Key))] #endif` on the definition, and B has no excluded code. The fix on B's `Key` is offered, renames A's `Key`, and leaves `nameof(Key)` behind. This input compiles error-free with `NEVER` defined before the fix.

Fix All shares `RenameWithoutConflictAsync` and fails the same way.

**Class members** (axis: the cascade relation the renamer applies, crossed with whether that relation's other member lies outside the reported symbol's reference scope):
- **Fails:** a partial-method type parameter, definition to implementation (run) and implementation to definition (run).
- **Fails:** the same with excluded text that declares the new name (derived, same branch).
- **Holds:** override and implementation (tests).
- **Holds:** a private partial method itself, because the scope is the containing type's parts (run, RH4103).
- **Holds:** a partial-method parameter, because the parameter's scope walks up to the containing type or the project (run, RH4105).
- **Holds:** a record positional parameter cascading to its synthesized property, because the scopes are equal (run, RH4120).
- **Holds:** delegate `Invoke`/`BeginInvoke` parameters, property and accessor, type and constructor, partial-property and partial-indexer parameters (derived: same declaration or the same scope walk).
- **Holds for the native-entry-point check:** no partial relation carries a separate binding.

#### Finding #7 in detail

The spec author made the rename cascade an exhaustive list in SHARED-CASING-007: "a member it overrides, a member that overrides it, an interface member it implements, and an implementation of it, each with its own cascade in turn". The list mirrors `GetRenameCascadeAsync` (`15-spec-author.md`, *Internal choices*). SHARED-CASING-009 lists the same cascade with "such as".

This gives two spec statements of the renamer's cascade that disagree, plus a third statement in the code. The closed list in 007 is narrower than its own definitional clause, "every member the rename renames with the symbol". It leaves out the partial-method parts of finding #6. Read literally, it puts the other part's document outside the region, while SHARED-FIX-003 withholds the fix.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Name in excluded code (SHARED-CASING-007, SHARED-FIX-003 build configurations) | disabled text of the union of `GetReferringDocuments` over `GetRenameCascadeAsync` (override and implementation closure) | documents where any symbol the renamer renames can be referred to (the renamer's cascade = `FindReferencesAsync` definitions) | disagree for partial-method type parameters | finding #6 |
| Native entry point changes (SHARED-FIX-003 run time) | ordered entry points of the cascade before the rename and of the cascade recomputed from the renamed declaration | methods the renamer renames that carry an import binding | agree: the missing partial relation carries no separate binding; a cascade that grows after the rename can only withhold (safe direction) | holds |
| Added compile error (SHARED-CASING-009) | `Compilation.GetDiagnostics` errors of the projects with changed documents plus transitive dependents | every error in a project whose compilation the rename changes | agree | holds |
| Old name in excluded code | `declaredSymbol.Name` for every cascade document | the identifier of every cascade member | agree: overrides and implicit implementations share the name; explicit implementations are not reported by the casing analyzers (RH4103:47, RH4102:60) | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rename cascade (which symbols a rename renames) | Roslyn renamer (truth); `CasingCodeFixProviderBase{T}.GetRenameCascadeAsync` (`rg "FindOverridesAsync\|FindImplementationsAsync\|FindImplementedInterfaceMembersAsync" src`, non-test: this method only); SHARED-CASING-007 (closed list); SHARED-CASING-009 (open list) | more than one owner, and they drift. This is findings #6 and #7 and the architecture finding below |
| Native entry point | `GetNativeEntryPoint` / `GetNativeEntryPoints` only | one owner |
| Compile errors after a rename | `GetCompileErrorsAsync` / `HasAddedCompileErrorsAsync` only (`rg "GetMethodBodyDiagnostics\|GetDeclarationDiagnostics\|GetDiagnostics\(" src`, production) | one owner |
| Excluded-code occurrence | `HasExcludedCodeOccurrenceAsync` only | one owner; its region comes from the cascade model (finding #6) |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4103 native cascade tests (5 withheld, 1 offered) | no fix when a cascade member's native binding takes its name from the method; offered with a literal `EntryPoint` | any rename of either `getpid`; a missing rename on the literal side | `Verify` (single fix and three Fix All scopes); cross-project via `AdditionalProjects` | adequate. The cross-project test failed at `Test0.cs` before the repair (`11-test-implementer.md`), so the additional project is compared |
| RH4103 compile-error tests (query, deconstruction, `foreach`, `await`, dependent project; same-document characterization; offered when not visible) | no fix when an error appears in an unchanged document or project | the rename of `select`, `deconstruct`, `getEnumerator` or `getAwaiter` | `Verify`, two documents or two projects | adequate |
| RH4103 excluded-code cascade tests (file-local, CRLF, private nested override, nested implementation, new-name declaration, referenced project; two offered) | no fix when a cascade member's document excludes the old or new name | base and override renamed | `Verify` | adequate for the override and implementation relations; no test covers the partial-method relation (finding #6) |
| RH4105 / RH4121 / RH4106 scope-branch tests | the parameter, type parameter and partial-part branches withhold; an unrelated type's excluded code does not | a fix offered or withheld wrongly | `Verify` | adequate. The RH4121 test covers a type's type parameter, not a method's |
| Missing | the partial-method type parameter in both directions | — | — | missing (finding #6) |

### Visible output is decided

- **Withheld fixes.** Every new test that expects a withheld fix is decided by SHARED-FIX-003, either its compile-error or build-configuration condition, or the run-time bullet under P1 (lead decision 2).
- **Offered fixes.** Every new test that expects an offered fix asserts the renamer's output, which is unchanged from `main` and demanded by the unchanged rename clause of SHARED-CASING-007.
- **Spec examples.** The two new SHARED-CASING-007 examples equal those tests.
- **Rule pages.** No rule page changed.

Nothing undecided.

### Failure classes dismissed

- **Unchanged from the first audit.** The delta adds no edit path, so these still hold: FC-004, FC-005, FC-007, FC-009 to FC-014, FC-018, FC-019, FC-021, FC-022, FC-025 to FC-030, FC-032.
- **FC-008.** Fix All shares `RenameWithoutConflictAsync`. Finding #6 fails Fix All the same way, and the closed findings hold in Fix All through `Verify`'s three scopes.
- **FC-023.** The `ValueText` comparison against `declaredSymbol.Name` is unchanged in behavior.
- **FC-033 and FC-034.** The tests use the auditor's shapes verbatim, with real metadata or project references.
- **FC-035.** The expected values come from the specs and the shapes of the first audit. The offered outputs are the renamer's outputs on `main`.
- **FC-036.** A `[CallerArgumentExpression]` literal is a member of the run-time class already recorded as a follow-up; see *Hints*.
- **FC-037.** No text-only claim.
- **FC-031.** This holds for every changed summary, except the remark of `GetRenameCascadeAsync`. That remark says the renamer renames "this override and implementation cascade", which is part of finding #6.

### Architecture findings

**The class of finding #2 repeated: the casing fix base still models the renamer's reach instead of reading it.**

Components:
- the Roslyn renamer, which decides which symbols a rename renames;
- `CasingCodeFixProviderBase<T>.GetRenameCascadeAsync`, which re-derives that set by hand for overrides and interface implementations;
- SHARED-CASING-007, which states the same set as a closed list;
- SHARED-CASING-009, which states it as an open list.

Shared decision: which symbols the rename renames, and therefore which documents the excluded-code check must scan.

The repair moved the three per-guard regions into one place, as decision 13 asked. That one place is still a prediction of another component's behavior, so the next relation it does not model fails again: finding #6, partial-method type parameters. This is the same decision failing between the same components (the guard region against the renamer), with a member the earlier enumeration did not name. Under `squad/README.md`, *Architecture findings*, it is handled as an architecture finding, not as another repair round.

A structural change within the run's component would read the cascade from Roslyn, via `FindReferencesAsync` definitions, which the renamer's cascade uses. It would state the cascade once in the spec, as "every member the renamer renames with the symbol". The lead decides.

### Hints

- **Cost, larger than in the first audit.** Every evaluation of a rename now does all of the following:
  - It runs two full `Compilation.GetDiagnostics` (all method bodies) per affected project, where an affected project is a project with changed documents or any of its transitive dependents. This replaces the cheaper declaration diagnostics.
  - It runs two `SymbolFinder` closures.
  - Fix All repeats this per candidate, and twice per same-target pair in `CandidatesConflictAsync`.

  For a public member of a core project, the lightbulb compiles every dependent project twice. Not measured. Code fixes run on demand, and decision 16 accepted the region. If it matters, the original solution's errors could be computed once per Fix All.
- **Pre-existing, outside both issues; adds a member to the run-time rebinding follow-up (decision 15).** RH4105 renames `Condition` to `condition` in `Check(bool Condition, [CallerArgumentExpression("Condition")] string expression = null)` and leaves the literal unchanged (run: fix offered). This gives warning CS8963 only. At run time, `expression` is no longer filled with the caller's argument text.
- **Pre-existing, uncertain, not constructed.** Multi-targeted projects: the renamer's linked-document handling against the excluded-code check of the project the fix is invoked from. An `#if NET8_0` branch is excluded in one target project and active in the other.

### Commands
```shell
scripts/lane-check.sh bug-fix                                   # LANE CHECK: confirmed
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pm Key RH4121      # A.cs Key: 1 action, B.cs keeps excluded `Key copy`; B.cs Key: 0 actions
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pmr2 Key RH4121    # B.cs Key: 1 action, A.cs keeps excluded `nameof(Key)`
dotnet run <scratch>/r1/nevercheck.cs -- <pm before>                    # NEVER defined: no error
dotnet run <scratch>/r1/nevercheck.cs -- <pm after>                     # NEVER defined: CS0246 'Key' at B.cs(6,9)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pmm run RH4103     # 0 actions (holds)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/pmp Value RH4105   # 0 actions (holds)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/rec value RH4120   # 0 actions (holds)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r1/cae Condition RH4105  # 1 action, literal "Condition" left (hint)
dotnet run <scratch>/harness/defs.cs -- <scratch>/r1/pm Key             # FindReferencesAsync definitions: Key @A.cs | Key @B.cs
dotnet run <scratch>/harness/defs.cs -- <scratch>/q4a process           # Base.process @A.cs | Leaf.process @B.cs
dotnet run <scratch>/harness/defs.cs -- <scratch>/q4c process           # IRunner.process @A.cs | Outer.Impl.process @B.cs
```
`<scratch>` is `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad`.
