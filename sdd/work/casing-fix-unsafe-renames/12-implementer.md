Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: tests pass

Called after audit round 1 to carry out lead decision 13, as clarified by decision 16, and the two summary repairs of decision 14. My prompt named that structural change, which is the lead's recorded decision, so I followed it. The prompt carried no expected outputs. I changed no test. The 16 tests that `11-test-implementer.md` lists as failing now pass, and so do the 9 tests it lists as passing.

## thoenissen/Reihitsu#1070

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | Finding 1. The native-entry-point check used to compare only the declared symbol. It now compares the ordered entry points (`GetNativeEntryPoints`) of every member of the rename cascade, before and after the rename. The cascade after the rename is computed again from the renamed declaration. A `static abstract`/`static virtual` interface method implemented by a `[DllImport]`/`[LibraryImport]` method without `EntryPoint`, or with `EntryPoint = nameof(self)`, is withheld. This also holds across projects. A literal `EntryPoint` stays renamable. | SHARED-FIX-003 (run-time bullet), RH4104-002 |
| same file | Finding 3. `GetCompileErrorsAsync` now takes every error of the affected project's compilation (`Compilation.GetDiagnostics`). That covers the parse, declaration and method-body errors of every document, not only of the documents the renamer changes. CS1940 and the ambiguous `Deconstruct`/`GetEnumerator`/`GetAwaiter` bindings in an unchanged document or a dependent project now withhold the fix. The `changedDocumentIds` parameter is gone. | SHARED-CASING-009, SHARED-FIX-003; decision 16 (every document of every affected project) |

## thoenissen/Reihitsu#1073

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | Finding 2. `HasExcludedCodeOccurrenceAsync` takes the rename cascade and scans the union of `GetReferringDocuments` over its symbols. The reference scope of each symbol is unchanged. A file-local or private nested override, a private nested implementation, and an override of a base in a referenced project now reach the base member's documents. `RH4106…FixIsOfferedWhenOnlyAnExcludedBranchOfAnUnrelatedTypeInAnotherFileNamesTheField` stays offered, because the cascade of a field is the field alone. | SHARED-CASING-007, SHARED-FIX-003; decisions 5, 16 |

## Whole run

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | Decision 13 adds the shared determination, `GetRenameCascadeAsync`. It starts from the declared symbol. For a method, property or event, it closes transitively over the overridden member, the implemented interface members (`SymbolFinder.FindImplementedInterfaceMembersAsync`), the overrides (`FindOverridesAsync`) and, for interface members, the implementations (`FindImplementationsAsync`). Each found symbol is taken as its `OriginalDefinition`. `PrepareRenameAsync` computes the cascade once and returns it with the symbol. The excluded-code and native-entry-point checks take their region from it. The compile-error and positional-property checks keep the affected projects: the projects the rename changes and their transitive dependents. | decision 13, 16 |
| same file | Decision 14 (finding 5) rewords the `HasRenameConflictAsync` summary to "unsafe, as `RenameWithoutConflictAsync` decides it". Decision 14 (hint) replaces the removed "binding domain" term: the `FixAllCandidate.ContainingSymbol` summary and its constructor parameter now read "symbol containing the declaration", and the `GetDuplicateTargetCandidatesAsync` summary had the same term. The summaries of `RenameWithoutConflictAsync`, `PrepareRenameAsync`, `HasExcludedCodeOccurrenceAsync`, `GetCompileErrorsAsync` and `HasAddedCompileErrorsAsync` now describe the new regions (FC-031). | FC-031 |

### Policy owners
```shell
rg -n "FindOverridesAsync|FindImplementationsAsync|FindImplementedInterfaceMembersAsync|OverriddenMethod" src   # cascade: CasingCodeFixProviderBase{T}.GetRenameCascadeAsync only. OverrideMemberUtilities (analyzer) walks to the root override for reporting, a different decision; RH7401/RH7405/RH3101/DocumentationAnalysisUtilities test OverriddenMethod for their own rules
rg -ln "GetDllImportData|LibraryImport" src   # production: CasingCodeFixProviderBase{T} only
rg -n "GetMethodBodyDiagnostics|GetDeclarationDiagnostics|compilation.GetDiagnostics" src   # production: CasingCodeFixProviderBase{T}.GetCompileErrorsAsync only (SelfHostingSolution is test infrastructure)
```

### Tests run
```shell
scripts/build.sh --configuration Debug                                          # Build succeeded, 0 warnings, 0 errors
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"   # Passed: 542, Failed: 0
scripts/test.sh --configuration Debug --project analyzer                        # Passed: 6711, Failed: 0
scripts/test.sh --configuration Debug --project architecture                    # Passed: 115, Failed: 0
```
`scripts/format.sh` was run over the changed file. The first build raised S3267 on the new loop in `GetRenameCascadeAsync`. I rewrote the loop with `Where`, and the build is warning-free. No suppression was added.

### Disputed tests or scope
- No test is disputed.
- Scope: one production file. No new public member and no new dependency: `SymbolFinder` lives in `Microsoft.CodeAnalysis.Workspaces`, which the project already uses for `Renamer`.
- The cascade covers overrides and interface implementations in both directions, as decision 13 states. It does not model other renamer cascades, such as a parameter renamed in overriding methods, or a partial method's other part, which is the same symbol. No test or finding names one.
- Cost: each rename evaluation now also runs `SymbolFinder` searches for member symbols twice, before and after. The compile-error check now computes full `Compilation.GetDiagnostics` for every affected project twice, instead of declaration diagnostics plus the changed documents' method bodies. The auditor's cost hint applies, now somewhat larger.

### Architecture findings
_None_ new. The auditor's finding (the base predicts the renamer's reach separately per guard) is addressed by decision 13. The region of each guard now comes from `GetRenameCascadeAsync` or from the affected projects. No guard derives its own region from the declared symbol alone.

### Follow-up candidates
_None_ beyond those in `00-lead.md`.
