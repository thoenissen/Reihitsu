Role: auditor
Commit: 65c826e426bdb7fd083567be7173a5ac053a64f6
Verdict: findings

## Scope

This audits only the repair delta against 20b2618e49fad03a0adf5e95bc01b763392bc696. That delta is the structural change of lead decisions 19 and 20, which answers finding #8 and the architecture finding of `23-auditor.md`.

- Head 65c826e426bdb7fd083567be7173a5ac053a64f6 is checked out, and the working tree is clean.
- Base `origin/main` 61718e254d608e0937c561c3e8839a05158f6338 matches `git ls-remote` and is an ancestor of head, so it is merged.
- `scripts/lane-check.sh bug-fix` returns `confirmed`.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The delta matches handovers `24-` to `28-`:
- **One production file**, `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`:
  - a new private `GetOldNames`, the multiset difference between the referable names of the cascade before and after the rename;
  - `HasExcludedCodeOccurrenceAsync` now takes a set of old names;
  - the excluded-code check moved from `PrepareRenameAsync` to `RenameWithoutConflictAsync`, after the renamed cascade is read;
  - the summaries and remarks that change with it.
- **Tests:** five RH4105 tests and one RH4121 test.
- **Spec:** SHARED-CASING-007 in `sdd/specs/shared/casing.md`.
- **Work files** under `sdd/work/`.

Scope did not grow beyond the casing fix base and its spec.

Evidence lives under `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad/` and was built from head:
- `harness/run3.cs` runs the real code fix.
- `h5/oldnames.cs` is a new probe. For every declared symbol of a corpus, it renames the symbol with the Roslyn renamer and collects the identifiers the renamer actually replaced, across all documents. It then compares that set with what `GetOldNames` computes from the same two cascades.
- The corpora are `h5/c1` and `h5/c2`.

I trusted the roles' focused test results and the cleaner's build. Evidence that is byte-identical to the earlier audits is reused.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1–7 | closed | Closed in `17-auditor.md` and `23-auditor.md`. The delta leaves their guards alone: the native-entry-point and compile-error checks are unchanged, and the cascade and document region are unchanged. |
| 8 | closed | Every member the enumeration listed holds when rerun at head. `r4/pp`: 0 actions on `Arg`. `r4/ppr`: 0 actions on `Arg` and 0 on `Other`. `r4/pidx`: 0 actions on `Index`. Each member is pinned by a test that failed before the change (`25-test-implementer.md`). The partial-constructor member is still unreachable, because RH4105 reports no diagnostic on `r4/pctor`; the probe shows that `GetOldNames` would cover it anyway (`Seed`/`start` match). The member that held, a differently spelled type parameter (`r4/ptp`), still holds: 1 action, and only part A is renamed. |

## thoenissen/Reihitsu#1070

### Findings

None new. The native-entry-point comparison is unchanged and still runs before the excluded-code check. Both checks return `null`, so the order cannot change an outcome.

### Against the specs

SHARED-FIX-003 run-time bullet, RH4103 and RH4104-002: unchanged, and they hold.

## thoenissen/Reihitsu#1073

### Findings

See `## Whole run`, finding #9 (test adequacy only).

### Against the specs

- **Q4: SHARED-CASING-007 and SHARED-FIX-003, build configurations, old-name axis.** This holds.
  - **Method.** The probe compares the computed old names with the identifiers the renamer replaced, for every declaration in two corpora. The shapes covered:
    - interface, abstract, override and explicit-implementation members (method, property, event, indexer parameter);
    - partial method parameters and type parameters, same and differently spelled;
    - partial indexer, property, event and constructor parameters;
    - records: positional parameters, a derived record that repeats them, and an explicit property matching a positional parameter;
    - class primary-constructor parameters, enum members, delegates and delegate parameters, generic type and method type parameters, overriding method type parameters;
    - fields, including multi-declarator fields, field-like and custom events, `static abstract`/`static virtual` members, default interface members;
    - extension-block receivers, properties and methods;
    - locals, deconstruction, `foreach`, `catch`, pattern and `out var` variables, local functions and their parameters, lambda parameters, range variables, labels, anonymous-type members, tuple elements;
    - `file` and private nested types, type rename with constructors and destructor, partial generic types;
    - verbatim (`@class`) and Unicode-escaped (`Abc`) identifiers.
  - **Result.** 171 declarations match exactly. The only mismatches are:
    - Renaming a constructor or destructor token directly. No casing rule reports that token, and the type's own rename matches.
    - The over-withhold case under *Hints*.
  - **The reported name is always in the set.** No reachable shape loses it.
  - **No under-withholding.** For a name to go missing, the cascade after the rename would have to gain a symbol that still carries an old name. Every relation the reference search cascades along links by the new name or by position, and the probe found no such symbol.
- **Spec against code.** The restated bullet defines the old names over "every symbol of the cascade that source can refer to by its name". The cascade is the renamer's own determination. `GetOldNames` reads that set from the renamer's result instead of predicting it.
  - The negative side is consistent: `TItem`, a differently spelled type parameter of the other part, adds no old name. The test pins it.
  - The corrected example in the cascade paragraph ("however that part spells it"; a type parameter "of the same name") matches the probe.
- **Q1 and Q5.** The delta does not touch them.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | medium | `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs`, `NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName` | FC-017 | – | PR-introduced | The test's name, its summary, and its row in `25-test-implementer.md` all claim it pins the new-name arm. Its excluded branch is `int arg = other;`. It names the new name `arg`, but also `other`, which since this repair is an old name of the cascade. A sibling test in the same class pins exactly that: `other++` alone withholds the fix. If the new-name arm were removed, this test would stay green. The input cannot falsify the invariant the test states. Nothing is broken today: the new-name arm across the partial relation is pinned on its own by `RH4121…NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodNamesTheNewName`, whose excluded code is `TKey copy = default;`. The test passed when it was written, at ca74343, because the old code searched only `Arg`; the repair made it weak. | Make the excluded branch name only the new name, for example `int arg = 0;`. The expected value stays the same. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Old names searched in excluded code (SHARED-CASING-007, SHARED-FIX-003 build configurations) | `GetOldNames`: the referable names of the cascade before the rename, minus, as counts, those of the cascade of the renamed symbol after it | the old identifiers the renamer replaces | agree on every reachable shape the probe covers. The reported name is always included. A symbol the search reaches but the renamer leaves alone (`TItem`) cancels out. A non-referable symbol (accessor, constructor, explicit implementation) adds nothing, and its identifier is covered by the referable symbol it belongs to. The only difference is one over-withhold (Hints). | holds |
| Region of the excluded-code check | unchanged: the union of `GetReferringDocuments` over the cascade before the rename | documents where a renamed symbol can be referred to | unchanged since `23-auditor.md`; a superset, which is the safe direction (decision 20) | holds |
| Order of the checks | the excluded-code check now runs after the renamer, the conflict annotations, the renamed declaration, and the native entry points | — | every check returns `null`, so the order changes no outcome. The renamer now also runs for renames that excluded code withholds, which costs time only. | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Old names a rename replaces | `rg -n "oldName\|GetOldNames\|HasExcludedCodeOccurrence\|CanBeReferencedByName\|FindReferencesAsync" src --glob '!*.Test*/**'` finds only `CasingCodeFixProviderBase{T}`: `GetOldNames` computes the set, and `HasExcludedCodeOccurrenceAsync` has one call site. The spec states it once, in the SHARED-CASING-007 bullet. | one owner |
| Rename cascade | unchanged: one reader, `GetRenameCascadeAsync`, and one statement, in SHARED-CASING-007 | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesItsDifferentlySpelledParameter` | The spelling of a differently spelled cascade member withholds the fix (from the defining part to the implementing part). | The fix is offered or a part is renamed. | `Verify` with two documents; the source is passed as the fixed state; single fix plus three Fix All scopes | adequate. It failed before the repair. |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheDefiningPartOfAPartialMethodNamesItsDifferentlySpelledParameter` | The reverse direction: `Other` is withheld only through `Arg`, the cascade member's spelling. | `Other`'s fix is offered. | the same. Both diagnostics are marked, so the iterative single fix tries each one. | adequate. It failed before the repair. |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialIndexerNamesItsDifferentlySpelledParameter` | The same, for a partial indexer. | The fix is offered. | the same | adequate. It failed before the repair. |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName` | The new name in the other part's excluded code withholds the fix. | The fix is offered when only the new name is named. | the same | weak: the input also names the old name `other` (finding #9) |
| `RH4105…FixRenamesTheDifferentlySpelledParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt` | The fix is offered when excluded code names none of the names; both parts and the active reference become `arg`; the excluded text is unchanged. | The fix is withheld, or the output is wrong. | full text of both documents | adequate |
| `RH4121…FixIsOfferedWhenAnExcludedBranchNamesTheDifferentlySpelledTypeParameterOfTheOtherPartOfAPartialMethod` | A symbol that the search reaches but the renamer leaves alone adds no old name (FC-002, the other side). | The fix is withheld (over-withholding), or part B is renamed. | full text of both documents | adequate. It turned red under the literal reading of decision 19 (`26-implementer.md`). |

No test pins "a non-referable symbol adds no old name" through an accessor name in excluded code. That is the over-withholding direction only, and the probe shows it holds for every accessor, constructor and explicit-implementation shape, so it is not a finding.

### Visible output is decided

- **Withheld fixes.** The four new withheld-fix tests are decided by SHARED-FIX-003 (build configurations) under P1 (decisions 19 and 20).
- **Offered fixes.** The two offered tests assert the renamer's output on `main`, which the unchanged rename clause of SHARED-CASING-007 demands. They match the spec's new example and its prose.
- **Rule pages.** No rule page changed.

Nothing undecided.

### Failure classes dismissed

- **Unchanged from `17-auditor.md` and `23-auditor.md`.** The delta adds no edit path and no rewrite, so these still hold: FC-004, FC-005, FC-006, FC-007, FC-009 to FC-014, FC-018, FC-019, FC-021, FC-022, FC-025 to FC-030, FC-033 to FC-035, FC-036 (the `[CallerArgumentExpression]` follow-up, decision 18), FC-037.
- **FC-001, FC-016, FC-020, FC-024.** These are the subject of the repair. See *Guard scope* and *Against the specs*: partial, explicit-implementation and `file` shapes are in the probe corpus.
- **FC-002 and FC-003.** Both sides of the old-name condition are tested: a differently spelled cascade member withholds, a differently spelled non-cascade member does not, and the offered side is covered. The non-referable side is established by the probe.
- **FC-008.** Fix All shares `RenameWithoutConflictAsync`, and each candidate computes its own before and after cascades on the accumulated solution. The new tests run three Fix All scopes.
- **FC-015.** See *Policy ownership*: one owner.
- **FC-017.** See *Assertion adequacy*; finding #9.
- **FC-023.** `GetOldNames` uses `ISymbol.Name`, which is unescaped, and the scan compares `token.ValueText`, also unescaped. The probe matches for `@class` and `Abc`.
- **FC-031.** The summary and remarks of `GetOldNames` describe the code.
  - The summary and parameter documentation of `HasExcludedCodeOccurrenceAsync`, and the remark of `RenameWithoutConflictAsync`, are updated.
  - The summary of `PrepareRenameAsync` ("rejects the renames that are unsafe before the renamer runs") still holds; only the namespace collision remains there.
  - The new remark of `GetRenameCascadeAsync` states the superset. Its summary wording is a hint, not a finding.
  - There is no `TODO` and no tracker number.
- **FC-032.** The cascade after the rename is read from `renamedSolution` after the renamer runs. No result is reused across the change.

### Architecture findings

_None._ The class of findings #2, #6 and #8 asked the same question: which code the excluded-code check must search so that it covers what the renamer reaches.
- **Documents.** The repair reads them from Roslyn's reference search, a superset of what the renamer renames, which errs in the safe direction.
- **Old names.** The repair reads them from the renamer's own result.

Nothing in the guard is left to predict the renamer's reach, and the probe found no reachable shape where the two differ in the unsafe direction.

### Hints

- **Over-withholding for a property of an extension block, not reached by a test.** The reference search from such a property (`extension(string Text) { public int Len => … }`) returns its implementation method `get_Len`. That method is a referable `Method` symbol, so `GetOldNames` counts `get_Len` as an old name (probe `h5/c1`). The fix is then withheld if excluded code names `get_Len`. This is the safe direction and an unusual input. It is not a finding.
- **Summary of `GetRenameCascadeAsync`.** It reads "every symbol declared in source that a rename of it renames with it". The remark added in this repair says the search can reach more than that. The summary is not false, since it does not say "only", but "at least every symbol …" would match the remark. This is a wording nit.
- **Cost, a delta to decision 18.** The renamer and the second reference search now also run for renames that excluded code withholds. Not measured.

### Commands
```shell
scripts/lane-check.sh bug-fix                                          # LANE CHECK: confirmed
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/pp Arg RH4105      # A.cs Arg: 0 actions (finding 8 closed)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/ppr Other RH4105   # A.cs Arg: 0 actions; B.cs Other: 0 actions
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/pidx Index RH4105  # A.cs Index: 0 actions
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/ptp Key RH4121     # 1 action; only A renamed to TKey (holds)
dotnet run <scratch>/harness/run3.cs -- <scratch>/r4/pctor Index RH4105 # no RH4105 diagnostic (member unreachable)
dotnet run <scratch>/h5/oldnames.cs -- <scratch>/h5/c1                  # ok=108; mismatches: partial ctor token renamed directly (x2), extension get_Len (extra)
dotnet run <scratch>/h5/oldnames.cs -- <scratch>/h5/c2                  # ok=63; mismatches: ctor/dtor tokens renamed directly only
```
`<scratch>` is `/tmp/claude-0/-home-user-Reihitsu/f3a4f4d5-2eae-5be2-bb46-c5ca05533a2e/scratchpad`.
