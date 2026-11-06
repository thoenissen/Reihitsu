Role: auditor
Commit: 98349b99483c672e31034a0366e9e2a2d1040707
Verdict: findings

## Scope

Full audit. Head 98349b99483c672e31034a0366e9e2a2d1040707 (checked out); base `origin/main` 61718e254d608e0937c561c3e8839a05158f6338, which is an ancestor of head (merged). `scripts/lane-check.sh bug-fix`: confirmed. The diff matches the handovers: one production file (`src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`), the helper `src/Reihitsu.Analyzer.Test/Base/CompiledAssemblyReference.cs`, 18 naming test classes, the spec clarifications in `sdd/specs/shared/casing.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/analyzer/RH4104.md`, `sdd/specs/analyzer/RH4120.md`, and `sdd/work/`.

My prompt carried no intended fix or verdict. `00-lead.md` repeats the issue's suspected cause for #1009 Q1; I did not use it and traced the code myself.

How the findings were established:
- `scripts/apply-fix.sh RH4103` on single-document fixtures in the scratchpad. LF and CRLF agreed.
- A scratch harness outside the repository for multi-document and two-project cases. It is a file-based app referencing `src/Reihitsu.Tooling/Reihitsu.Tooling.csproj`. It builds an `AdhocWorkspace`, runs `RH4103MethodNameCasingAnalyzer`, calls `RH4103MethodNameCasingCodeFixProvider.RegisterCodeFixesAsync` on each diagnostic, applies the first action, and prints the changed documents and the compile errors after the fix.

## thoenissen/Reihitsu#1070

### Findings

See `## Whole run`, finding #1 (native entry point reached through the rename cascade) and finding #3 (compile errors outside the inspected region; the same mechanism as #1070 mechanism 1).

### Against the specs

- **SHARED-FIX-003 run-time bullet (native entry point), RH4104-002.** These hold for the declaration the diagnostic is on. Covered: `[DllImport]` without `EntryPoint`, `EntryPoint = nameof(self)`, `[LibraryImport]` without `EntryPoint`, the literal `EntryPoint` kept, and the extern local function. They fail for a method that the rename reaches through its cascade (finding #1).
- **SHARED-CASING-009 cascade bullet.** This holds for the metadata-rooted cascades the tests cover: method, field-like and custom event, and protected, internal and public property override chains; the three interface shapes for methods and one for properties. Declaration diagnostics are compared for the whole compilation of every affected project. The traced and constructed cells hold, except finding #3.
- **Issue goals.** #1070 is narrowed per lead decision 8, and RH4116 is out of scope (no fix exists). Both are addressed.

## thoenissen/Reihitsu#1073

### Findings

See `## Whole run`, finding #2 (excluded code reached through the rename cascade) and finding #4 (test gaps of the reference-scope branches).

### Against the specs

- **Q1 (SHARED-CASING-009, namespace-level types).** This holds. The compile-error check now runs for namespace-level types. Class, struct, delegate, record and record-struct collisions are withheld. The enum member and the interface method stay offered.
- **Q4 (SHARED-CASING-007 coverage, SHARED-FIX-003 build configurations).** This holds when the renamed symbol's own reference scope contains every renamed declaration. It fails when the renamer renames a cascade member whose referring documents lie outside that scope (finding #2).
- **Q5 (SHARED-CASING-007 coverage, RH4120-003).** This holds. `GetPositionalPropertyFlagsAsync` compares the flags of every positional parameter of every record in the changed projects and their dependents, before and after the rename. Split, join, base, derived and record-struct cells were traced. The flag sequences stay aligned because a rename adds, removes or reorders no declaration.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `CasingCodeFixProviderBase{T}.cs`, `RenameWithoutConflictAsync` (`GetNativeEntryPoint(declaredSymbol)` vs. `GetNativeEntryPoint(renamedDeclaration.Value.Symbol)`) | FC-001, FC-016, FC-036 | – | same requirement | The native-entry-point check compares only the declaration the diagnostic is on. The renamer also renames the cascade members, and a `static abstract` or `static virtual` interface member is implemented by a `static extern` method. **Counterexample** (run, LF and CRLF): `public interface INative { static abstract int getpid(); }` with `public class Native : INative { [DllImport("libc")] public static extern int getpid(); }`. RH4103's fix on `INative.getpid` is offered and renames both to `Getpid`. `Native.Getpid` then binds to the native function `Getpid`, which `libc` does not export. SHARED-FIX-003 (run-time bullet, as clarified by this run) withholds it. **Class members.** Implementing method `[DllImport]` without `EntryPoint`: fails (run). `EntryPoint = nameof(getpid)`: fails (derived, the value changes). `partial` method with `[LibraryImport]`: fails (derived). `static virtual` interface member: fails (derived). Single fix and Fix All both fail (shared `RenameWithoutConflictAsync`). **Holds:** the fix on the extern method itself; an implementing method with a literal `EntryPoint`. Overrides cannot occur because `[DllImport]` requires `static`. Tests cover only the declaration's own entry point. | Compare the native entry point of every method the rename changes, not only the declared symbol. One way is every source method in the changed documents that has a `[DllImport]`/`[LibraryImport]` binding, before and after, the way the positional flags are compared. Add a test for the static abstract interface shape. |
| 2 | high | `CasingCodeFixProviderBase{T}.cs`, `HasExcludedCodeOccurrenceAsync` → `GetReferringDocuments`/`GetReferenceScope` | FC-001, FC-020 | – | same requirement | The excluded-code check takes its documents from the reference scope of the declared symbol only. The renamer also renames cascade members (the overridden base member, the implemented interface member), and their referring documents can lie outside that scope. **Counterexamples** (run with the harness). Document A in every case: `public class Base { public virtual void process() { } public void Run() { #if NEVER process(); #endif } }`. (a) Document B: `file class Leaf : Base { public override void process() { } }`. The fix on `Leaf.process` is offered and renames `Base.process` in A to `Process`. The excluded `process();` stays, so a build defining `NEVER` fails (SHARED-FIX-003, build configurations). (b) The same with `public class Outer { private class Leaf : Base { ... } }`: fails. (c) A: `public interface IRunner { void process(); }` with `#if NEVER runner.process(); #endif` in another type of A; B: `private class Impl : IRunner` nested in `Outer`, fix on `Impl.process`: fails. (d) Two projects, B referencing A: A as above, B `public class Leaf : Base { public override void process() { } }`, fix on `Leaf.process`: fails. The scope is project B plus its dependents, but the renamer renames A's source member. **Class members.** Scope of the renamed member: file-local type, private nested type, project that depends on the cascade member's project. Relation: override of a base member, implementation of an interface member. Excluded text: a reference to the old name, a declaration of the new name. All of these fail: (a)–(d) run, the rest derived from the same branch. **Holds:** a public `Leaf` in the same project (run); renaming the base or interface member itself, because its scope covers the derived members' documents. No test covers a cascade case for Q4. | Derive the inspected documents from what the rename reaches, not from the declared symbol alone. For example, take the union of `GetReferringDocuments` over every symbol the renamer renames (the declared symbol plus its overridden, overriding, implemented and implementing members), or use every document of the changed projects and their dependents whenever the cascade leaves the declared symbol's scope. Add a test with a private nested or file-local override and the excluded reference in the base's document. |
| 3 | high | `CasingCodeFixProviderBase{T}.cs`, `GetCompileErrorsAsync` (method-body diagnostics only for `changedDocumentIds`) | FC-001 | – | same requirement | SHARED-CASING-009 as amended, and lead decision 3, count a compile error "wherever the rename puts it". The check compares declaration diagnostics of the whole compilation, but method-body diagnostics only of the documents the renamer changes. A rename that makes a name-based pattern binding ambiguous puts the error into a method body of a document that does not contain the name, so the renamer does not change it and its conflict detection does not see it. **Counterexample** (run). Document A: `namespace Ext; public static class MyLinq { public static IEnumerable<TResult> select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) { ... } }`. Document B: `using Ext; using System.Linq; ... return from value in values select value * 2;`. RH4103's fix on `select` is offered and renames it to `Select`. After the fix, B has `error CS1940: Multiple implementations of the query pattern were found for source type 'List<int>'. Ambiguous call to 'Select'.` B is unchanged, so its method-body errors are not compared. **Class members.** Bindings that use no identifier token of the new name: query clauses (`Select`, `Where`, `SelectMany`, `Join`, `GroupBy`, `OrderBy`, ...) fail (`Select` run, the others derived). Extension `Deconstruct` in a deconstruction, extension `GetAwaiter` in `await`, and extension `GetEnumerator` in `foreach`, each made ambiguous with another imported extension: derived fail. **Holds:** an error in a changed document; any declaration error anywhere. The gap exists on `main` too (the old check covered only the containing type's declarations). It stays inside the region this run decided to cover, so it is reported as `same requirement`, not as a hint. The implementer's handover named the gap and asked for a constructed input; this is one. | Compare method-body diagnostics of every document of the affected projects, or `Compilation.GetDiagnostics()` filtered to errors. The cost is the same order as the existing whole-compilation declaration check. Add a two-document test such as the query case. |
| 4 | medium | `src/Reihitsu.Analyzer.Test/Naming/*` (excluded-code tests) vs. `GetReferenceScope` | FC-003, FC-017 | – | same requirement | `GetReferenceScope` has six branches: type parameter → declaring symbol; parameter → its method; local / local function / lambda / file-local type → itself; private → containing type (every part of a partial type); otherwise → declaring project plus dependents. The tests cover three of them on the withholding side only: a private field in one document, a local, and a public class referenced in another file. No test covers a parameter or type parameter in excluded code, or a private member referenced in excluded code of another `partial` part, which needs the containing-type scope to reach the other document. No test pins the narrowing side either: an occurrence in excluded code of a document outside the scope, where the fix must stay offered. A guard that silently widened or narrowed one branch would keep every test green. Finding #2 is a member that no test noticed. | Add a test per untested branch on the withholding side (parameter or type parameter; private field named in another partial part's excluded code). Add one test on the offered side (a private member whose name occurs in excluded code of an unrelated type in another document). Add the cascade tests of findings #1–#3. |
| 5 | low | `CasingCodeFixProviderBase{T}.cs:727` (`HasRenameConflictAsync` summary) | FC-031 | – | PR-introduced | The summary still reads "produces Roslyn conflict annotations or added declaration errors". Through `RenameWithoutConflictAsync` the method now also withholds for excluded-code occurrences, a changed native entry point, any added compile error (declaration and method body), and changed positional properties. | Reword the summary to "whether the rename is unsafe as `RenameWithoutConflictAsync` decides", or list the conditions. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Native entry point changes (SHARED-FIX-003 run time) | the declared symbol before and after the rename | the renamer's edit set: declared symbol plus cascade members | disagree: a cascade member's binding is not inspected | finding #1 |
| Name in excluded code (SHARED-CASING-007 coverage, SHARED-FIX-003 build configurations) | disabled text of `GetReferringDocuments(declaredSymbol)` | documents that can refer to any symbol the renamer renames | disagree when a cascade member's scope is wider (file-local, private nested, upstream project) | finding #2 |
| Added compile error (SHARED-CASING-009, SHARED-FIX-003) | declaration errors of every affected project's compilation; method-body errors of changed documents | every error in the affected projects | disagree for method bodies of unchanged documents | finding #3 |
| Positional property changes (Q5) | every record parameter list in every document of the affected projects | records whose parameter match the rename can change: the record of a renamed member, its derived records, dependents | agree: inspected span is a superset | holds |
| Namespace collision (`HasNamespaceCollision`) | the containing namespace's source members of the new name | the namespace bullet of SHARED-CASING-009 | agree (unchanged from `main`; arity and file-local exceptions kept) | holds |
| Affected projects (`GetDependentProjectIds`) | projects with changed documents plus transitive dependents | projects whose compilation the rename can change | agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Native entry point of a renamed method | `CasingCodeFixProviderBase{T}.GetNativeEntryPoint` only (`rg "GetDllImportData\|LibraryImport" src`, non-test) | one owner |
| Name occurrence in excluded code | `CasingCodeFixProviderBase{T}.HasExcludedCodeOccurrenceAsync` only. `DisabledTextTrivia` elsewhere (`SyntaxTriviaUtilities`, `OrderingDeclarationUtilities`, `FormattingTextAnalysisUtilities`, `ModifierOrderingCodeFixProviderBase`, RH3001/RH3103/RH7501 fixes, RH7310) only classifies trivia | one owner. The other renamer user, `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`, has no such check; that is the follow-up already listed in `00-lead.md` |
| Added compile error after a rename | `CasingCodeFixProviderBase{T}.HasAddedCompileErrorsAsync`/`GetCompileErrorsAsync` only (`rg "GetDeclarationDiagnostics\|GetMethodBodyDiagnostics" src`) | one owner. The removed domain machinery (`AnnotateBindingDomainsAsync`, `GetBindingDomainErrorsAsync`, `HasAddedBindingErrorsAsync`) has no remaining copy |
| Positional property synthesis | `CasingCodeFixProviderBase{T}.GetPositionalPropertyFlagsAsync` only | one owner |
| Compile-error rule statement (specs) | SHARED-FIX-003; SHARED-CASING-009 now refers to it and lists regions as a refinement | one statement; the skeptic's architecture finding is resolved in the spec |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NoFixIsOffered…` (all withheld-fix tests, 18 classes) | no fix changes the document; diagnostic kept | any registered action that changes text, single or Fix All (document/project/solution) | `Verify(source, source)` on `BatchCodeFixTestsBase` (0 iterations expected) | adequate per test |
| `RH4103…FixIsWithheldForANativeEntryPointTakenFromTheMethodName` | `getpid` keeps its diagnostic and is not renamed; `getppid` renamed | a rename of `getpid`, or a missing remaining diagnostic | `Verify` with `MarkupMode.Allow` on the fixed and batch-fixed states | adequate |
| Metadata cascade tests (RH4102, RH4103, RH4112–RH4114) | no fix when the cascade reaches a member overriding or implementing a metadata member | `Middle`/`Leaf` renamed | `CompiledAssemblyReference.CreateAsync` (emitted image, not a project reference) | adequate; the helper genuinely makes the base metadata (FC-033/FC-034) |
| `RH4002…FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`, `…FixIsOfferedWhenOnlyAnInheritedMemberHasTheNewName`, `RH4004…`, `RH4005…FixRenamesANamespaceInterface…`, `RH4103…FixRenamesAnOverrideChainDeclaredInSourceTogether`, `…FixRenamesADllImportMethodWithALiteralEntryPoint`, `…FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface` | the offered side of each new condition | fix withheld, or different text | `Verify` with exact fixed source | adequate |
| Excluded-code tests (RH4002, RH4106, RH4115) | withheld when excluded code names the old or new name | — | `Verify` | weak: only three of six scope branches, withholding side only, no cascade (finding #4) |
| Native, compile-error and excluded-code cascade cells | — | — | — | missing (findings #1–#3) |

### Visible output is decided

- **Withheld fixes.** Every test that expects a withheld fix is decided by SHARED-FIX-003. Its compile-error and build-configuration conditions are unchanged on `main`. For the run-time cases (native entry point; record properties appearing or disappearing), the maintainer's decisions cover it: #1013 (Q1/Q2 "not reported", P2, recorded on the issue 2026-10-18 and narrowed to fix safety by the #1070 triage) and #1009 Q5 ("withhold", P1). Lead decisions 2 and 7 record this.
- **Offered fixes.** Every test that expects an offered fix asserts today's output on `main`, which the unchanged rename clause of SHARED-CASING-007 demands (lead decisions 10 and 12).
- **Rule pages.** No rule page changed.

Nothing undecided.

### Failure classes dismissed
- FC-004, FC-005, FC-019, FC-021, FC-022, FC-025, FC-026, FC-027, FC-029: the fix only renames identifiers through the renamer, so no gap, trivia, comment or literal layout is edited.
- FC-007: a withheld fix needs no pass. The offered fixes are unchanged single-pass renames, and `Verify` checks the iterations.
- FC-008: Fix All shares `RenameWithoutConflictAsync`, and every test runs the three Fix All scopes. Findings #1–#3 fail on Fix All the same way.
- FC-009: every registered action is a rename that changes a name; a withheld one is not registered.
- FC-010 to FC-014, FC-028, FC-030: the formatter is not involved; casing fixes do not format.
- FC-018: references are retargeted through `Renamer.RenameSymbolAsync`, unchanged.
- FC-023: `ParseTokens` + `ValueText` compares `@order` as `order`, which is correct.
- FC-024 (partial, static interface members): partial types are covered by the RH4002 test. Static interface members are finding #1.
- FC-032: no formatter change.
- FC-035: the expected values come from specs and the skeptic's inputs. The one output taken from running code (`N.Order`) was observed on unchanged `main`.
- FC-037: no text-only claim.
- FC-002: the qualifiers "where that declaration is a compile error" and "in a document where the symbol can be referred to" are explicit conditions. The latter is the one finding #2 shows narrower than SHARED-FIX-003.
- FC-015: see *Policy ownership*.

### Architecture findings

**The casing fix base predicts the renamer's reach instead of reading it.** Components: `CasingCodeFixProviderBase<T>` (its safety guards) and the Roslyn renamer (`Renamer.RenameSymbolAsync`, which decides which declarations and documents change).

Shared decision: which symbols and documents a rename touches, and therefore which bindings, excluded texts and errors the guards must inspect.

Places where the base derives that set itself:
- `GetNativeEntryPoint` is applied to the declared symbol only (finding #1);
- `GetReferenceScope` is applied to the declared symbol only (finding #2);
- `changedDocumentIds` bounds the method-body errors (finding #3; here the base uses the renamer's result, but the renamer's changed documents are not the set of documents whose binding the rename changes).

Each guard is FC-001 against the renamer's actual edit or binding set, and the inputs above show them disagreeing. This is the same structural cause the skeptic's finding named for the compile-error region, which decision 3 fixed only for declaration diagnostics.

A structural change within the run's component would derive every guard's region from the cascade the renamer renames (for example `SymbolFinder` overrides/implementations plus the declared symbol) and from every document of the affected projects. The lead decides.

### Hints
- **Pre-existing, follow-up candidate: run-time rebinding without a compile error.** RH4103's fix renames `public IEnumerator<int> getEnumerator()` on a class that also implements `IEnumerable<int>` explicitly to `GetEnumerator`. `foreach` over the class in another type then binds to the pattern method instead of the interface. It compiles and runs different code (run with `apply-fix.sh`, LF and CRLF: fix offered). The same holds for a query clause that rebinds to a closer extension `Select`/`Where` (run: no error, meaning changed). This is SHARED-FIX-003's run-time condition, and no guard of the base inspects it, on `main` or here. It is outside both issues.
- **Cost.** Each `RegisterCodeFixesAsync` on a casing diagnostic now renames and computes whole-compilation declaration diagnostics of the renamed solution for every affected project, including dependents. It also scans every record of those projects twice. In Fix All, `CandidatesConflictAsync` does this twice per same-target pair. This is not a hot path (code fixes run on demand) and was traded for correctness under decision 4. Worth watching on large solutions.
- `FixAllCandidate.ContainingSymbol`'s summary "Declaration or binding domain owner" uses a term of the removed binding-domain machinery. Unchanged line, pre-existing wording.
- Errors are compared as counts per ID. A rename that removes one error and adds another with the same ID passes. No input from a caller was constructed; inputs with pre-existing errors are rare.

### Commands
```shell
scripts/lane-check.sh bug-fix        # LANE CHECK: confirmed
scripts/apply-fix.sh RH4103 <scratch>/fx4103   # StaticAbstract.cs: fixed (renames INative.getpid and Native.getpid); Enumerator.cs: fixed (foreach rebinding)
dotnet run <scratch>/harness/run.cs -- <scratch>/q4a|q4b|q4c|q4d process   # q4a, q4b, q4c: 1 action, A.cs excluded `process` left behind; q4d: 0 actions
dotnet run <scratch>/harness/run2.cs -- <scratch>/xp process               # two projects: 1 action, A's excluded `process` left behind
dotnet run <scratch>/harness/run.cs -- <scratch>/qe select                  # 1 action; AFTER ERROR CS1940 in B.cs
```
