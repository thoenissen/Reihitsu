Role: implementer
Commit: 0d7ddc2
Verdict: test disputed (#1070: 1 disputed test, the others pass; #1073: 1 disputed test, the others pass)

The prompt carried no intended fix or suspected cause. `00-lead.md` repeats the issue's suspected cause for #1009 Q1. I did not use it. The compile-error check below follows lead decisions 3 and 4, not that cause.

41 of the 43 handed-over failing tests now pass. The other 2 cannot pass with any production behavior. Each one asserts something the spec contradicts, or is set up so that it cannot see the specified output (*Disputed tests or scope*). I changed no test.

## thoenissen/Reihitsu#1070

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `RenameWithoutConflictAsync` compares the native entry point (`GetNativeEntryPoint`) of the declaration before and after the rename and withholds the rename when it changes. The entry point is `DllImportData.EntryPointName`, or the `EntryPoint` of `[LibraryImport]`; without one, it is the method's own name. Comparing values covers a missing `EntryPoint` and `nameof(self)`, and leaves a literal `EntryPoint` renamable. This applies to methods and to local functions. | SHARED-FIX-003 (run-time bullet), RH4104-002 |
| same file | The compile-error check now covers every project the rename changes, plus every project that depends on one (`HasAddedCompileErrorsAsync`). It compares the declaration errors of the whole compilation, plus the method-body errors of every changed document, counted by ID. This catches cascade errors in other types and documents: CS0115, CS0534, CS0535 in `Middle` or in an implementing type. | SHARED-CASING-009, SHARED-FIX-003 |

### Tests run

All `RH4102`, `RH4103`, `RH4104`, `RH4112`, `RH4113`, `RH4114` reproduction tests of `03-test-implementer.md` pass except `RH4103…FixIsWithheldForANativeEntryPointTakenFromTheMethodName` (disputed below).

### Disputed tests or scope

**`RH4103MethodNameCasingAnalyzerTests.FixIsWithheldForANativeEntryPointTakenFromTheMethodName`**
- What it asserts: the SHARED-FIX-003 example. `getpid` keeps `{|RH4103:getpid|}` in the fixed code, and `getppid` becomes `Getppid`.
- The production code now produces exactly that output. The iterative-fix stage that failed before passes.
- The test still fails at `Diagnostics of fixed state: expected "0" actual "1"`. Microsoft.CodeAnalysis.Testing's default `FixedState.MarkupHandling` is `IgnoreFixable`, so it drops the fixed-code markup for a fixable ID and expects 0 diagnostics.
- The test cannot pass with any production behavior. Other tests in the repository with remaining fixable markup set `test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow;`, for example `RH8204DoNotUsePlaceholderElementsAnalyzerTests.cs:426`.
- I confirmed this with a temporary copy of the test that sets those two lines through `onConfigure`. It passed against this commit. The copy was deleted and never committed.
- Owner: test implementer (test configuration only; the expected output is unchanged).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1073

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | Q1: the compile-error check above now also runs for symbols declared directly in a namespace. The old namespace path skipped it. CS0542 and CS0694 inside the renamed type are seen, including a partial part in another file. `HasNamespaceCollision` stays as the pre-check of the namespace bullet, so two `partial` types that would merge under one name stay withheld. | SHARED-CASING-009 |
| same file | Q4: `PrepareRenameAsync` calls `HasExcludedCodeOccurrenceAsync`. It tokenizes every `DisabledTextTrivia` of every document whose code can refer to the symbol and withholds the rename when an identifier token equals the old or the new name. `GetReferringDocuments`/`GetReferenceScope` pick those documents, as described after this table. | SHARED-CASING-007, SHARED-FIX-003; lead decision 5 |
| same file | Q5: `ChangesPositionalPropertiesAsync` records, before and after the rename, whether the compiler declares a property for each positional parameter of every record in the affected projects. A property counts as declared for the parameter when its declaring syntax lies inside the parameter. The two flag sequences are compared position by position, and any change withholds the rename. This covers split and join, base and derived records, and record structs. | SHARED-CASING-007, RH4120-003, SHARED-FIX-003; lead decisions 6, 7 |

How `GetReferenceScope` picks the documents for the Q4 check:
- locals, local functions, lambdas, and file-local types: their own declaring documents;
- type parameters: the documents of the type or method that declares them;
- parameters: the same documents as their method, because a named argument can name a parameter wherever the method can be called;
- a private symbol, or anything inside one: the documents of the containing type, which covers every part of a partial type;
- every other symbol: all documents of the declaring project and of every project that depends on it.

Removed: `AnnotateBindingDomainsAsync`, `GetBindingDomainErrorsAsync`, `HasAddedBindingErrorsAsync`, and the `HasNamespaceOwner` branch. That was the narrower restatement the skeptic's architecture finding names (lead decision 3). Single fix and Fix All share `RenameWithoutConflictAsync`, so every new guard applies to both (SHARED-CASING-010).

### Tests run

All #1073 reproduction tests of `03-test-implementer.md` pass except `RH4005InterfaceNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceInterfaceWouldHaveAMethodOfItsName` (disputed below).

### Disputed tests or scope

**`RH4005InterfaceNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceInterfaceWouldHaveAMethodOfItsName`**
- What it asserts: no fix for `namespace N { interface iOrder { void IOrder(); } }`.
- The rename adds no compile error. C# forbids a member named like its enclosing type only for classes, structs, and records (CS0542), not for interfaces.
- I checked this: a scratch `net10.0` library containing `interface IOrder { void IOrder(); }` builds with no error. The skeptic marked this CS code as "derived, not compiled".
- SHARED-CASING-009 withholds only for an added compile error or a renamer conflict, and SHARED-FIX-003 names no other condition that applies. Under the spec, the fix is offered and the expected output is `interface IOrder { void IOrder(); }`.
- Which outputs this touches: the fix result for RH4005. Today's output on `main` (fix offered) matches the spec, so no unchanged requirement demands what the test asserts.
- I see two routes: the test implementer changes the test to assert the fix, or the lead asks the maintainer whether this input should be withheld anyway (an output question).

### Follow-up candidates

_None_ beyond the RH2001 candidate the lead already lists.

## Whole run

### Policy owners

```shell
rg -n "AnnotateBindingDomainsAsync|GetBindingDomainErrorsAsync|HasAddedBindingErrorsAsync|HasNamespaceOwner|PrepareRenameAsync" src/   # only CasingCodeFixProviderBase{T}.cs
rg -n "GetDllImportData|LibraryImport|EntryPoint" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core   # no other owner
rg -n "DisabledTextTrivia" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core   # trivia-classification helpers only (SyntaxTriviaUtilities, ModifierOrderingCodeFixProviderBase, OrderingDeclarationUtilities); none checks names in excluded text
rg -n "ParseTokens" src/ --glob '!*.Test/**'   # TokenSeparationUtilities only (token separation, unrelated)
rg -n "GetDeclarationDiagnostics|ParameterList.Parameters" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core   # DocumentationAnalysisUtilities lists parameters for docs; no positional-property owner
```

Every new policy has one owner: `CasingCodeFixProviderBase<T>`, the shared casing fix path.

### Tests run

```shell
scripts/build.sh --configuration Debug                          # Build succeeded, 0 warnings, 0 errors
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
# Failed: 2, Passed: 515, Total: 517 (before the change: Failed 43, Passed 474)
scripts/test.sh --configuration Debug --project analyzer         # Failed: 2, Passed: 6684, Total: 6686 — the same 2 disputed tests
scripts/test.sh --configuration Debug --project architecture     # Passed: 115
```

`scripts/format.sh` was run over the changed file. Formatter, CLI, and Core were not changed and not run.

### Disputed tests or scope

- The two tests above.
- Scope:
  - One production file, as the handovers name.
  - No new public member or dependency.
  - The compile-error check is wider than before: it covers the whole compilation's declaration diagnostics plus the method-body diagnostics of changed documents, in every affected project. Lead decision 4 left the region to the implementer.
  - The skeptic's option 2 (containing types of cascade members) would miss two reproductions. In `NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` and `NoFixIsOfferedWhenASourceInterfaceMethodIsImplementedByAReferencedAssemblyMember`, the error stands in a type that contains no renamed member.
  - Cost: every rename evaluation now computes declaration diagnostics for the affected projects twice, plus a positional-record scan.
- A method-body error in a document the rename does not change is not inspected. I found no input that produces one: references to the renamed symbol are in changed documents, and the renamer reports rebinding conflicts itself. The auditor may try to construct one.

### Architecture findings

_None_ new. The finding from `01-skeptic.md` (one compile-error rule stated twice) is resolved in code by decision 3. The domain machinery that implemented the narrower casing statement is removed, and one check implements SHARED-FIX-003's condition.

### Follow-up candidates

_None._
