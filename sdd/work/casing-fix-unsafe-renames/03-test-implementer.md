Role: test implementer
Commit: f3d856d
Verdict: #1070: reproduced; #1073: reproduced

The prompt carried no intended fix or suspected cause. `00-lead.md` repeats the issue's suspected cause for #1009 Q1. I did not use it: every test asserts only what the specs state, and every result below was observed by running it.

Every test runs through `Verify(source, fixedSource[, onConfigure])` on the rule's `BatchCodeFixTestsBase`. A "no fix" test passes the source as its own fixed source. With that setup, Microsoft.CodeAnalysis.Testing checks four things: the iterative single fix, and Fix All in document, project, and solution scope. Each must leave the document unchanged; the package docs say an expected Fix All output equal to the input means 0 iterations. So every withheld-fix test also covers the Fix All path (SHARED-CASING-010, FC-008).

The diagnostics marked in the sources are the ones `main` reports today; every test's analyzer stage passed. Every failing test below fails at `Iterative code fix application` with `content of '/0/Test0.cs' did not match`: the fix is offered and renames. That is the issue's own expected-versus-actual difference. No test fails at compile, at a helper, or on the diagnostics.

Metadata cases use the new helper `CompiledAssemblyReference.CreateAsync`. It compiles a source string against `ReferenceAssemblies.Net.Net90`, emits it, and adds the image with `test.TestState.AdditionalReferences.Add(...)`. No project reference is used, so the base members are real metadata (FC-033, FC-034). RH4113's referenced assembly declares `[assembly: InternalsVisibleTo("TestProject")]`, which matches the testing library's default project name. Custom events are tested with `RH4102CustomEventNameCasingCodeFixProvider`, the provider that fixes them, through the new private helper `VerifyCustomEventWithReference`.

## thoenissen/Reihitsu#1070

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.FixIsWithheldForANativeEntryPointTakenFromTheMethodName` (SHARED-FIX-003 spec example) | SHARED-FIX-003 | `Verify` | fails: `getpid` renamed to `Getpid` |
| `RH4103…NoFixIsOfferedForADllImportMethodWithoutEntryPoint` (with a call) | SHARED-FIX-003 | `Verify` | fails |
| `RH4103…NoFixIsOfferedForADllImportMethodWhoseEntryPointIsNameofTheMethod` | SHARED-FIX-003 | `Verify` | fails: `nameof(getpid)` → `nameof(Getpid)` |
| `RH4103…FixRenamesADllImportMethodWithALiteralEntryPoint` | SHARED-FIX-003 (other side of the condition) | `Verify` | passes |
| `RH4103…NoFixIsOfferedForALibraryImportMethodWithoutEntryPoint` (CS8795 marked, no generator) | SHARED-FIX-003 | `Verify` | fails |
| `RH4103…FixRenamesALibraryImportMethodWithALiteralEntryPoint` | SHARED-FIX-003 (other side) | `Verify` | passes |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToAnOverrideOfAReferencedAssemblyMember` (SHARED-CASING-009 spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails: `Middle.process` and `Leaf.process` renamed |
| `RH4103…NoFixIsOfferedForADirectOverrideOfAReferencedAssemblyMember` | SHARED-CASING-009 (characterization, already withheld) | `Verify` + metadata | passes |
| `RH4103…FixRenamesAnOverrideChainDeclaredInSourceTogether` | SHARED-CASING-009 (must stay) | `Verify` | passes |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterface` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4103…NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4103…FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface` | SHARED-CASING-009 (must stay) | `Verify` | passes |
| `RH4103…NoFixIsOfferedWhenASourceInterfaceMethodIsImplementedByAReferencedAssemblyMember` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4104LocalFunctionNameCasingAnalyzerTests.NoFixIsOfferedForAnExternLocalFunctionBoundByItsName` (RH4104-002 spec example) | RH4104-002, SHARED-FIX-003 | `Verify` | fails |
| `RH4102EventNameCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameCascadesToAFieldLikeEventOverrideOfAReferencedAssemblyEvent` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4102…NoFixIsOfferedWhenTheRenameCascadesToACustomEventOverrideOfAReferencedAssemblyEvent` | SHARED-CASING-009, SHARED-FIX-003 | `VerifyCustomEventWithReference` | fails |
| `RH4112ProtectedPropertyCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameCascadesToAPropertyOverrideOfAReferencedAssemblyProperty` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4113InternalPropertyCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameCascadesToAPropertyOverrideOfAReferencedAssemblyProperty` (IVT) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4114PublicPropertyCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameCascadesToAPropertyOverrideOfAReferencedAssemblyProperty` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |
| `RH4114…NoFixIsOfferedWhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterfaceProperty` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + metadata | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `[DllImport("libc")] extern int getpid()` (RH4103) | LF | code fix, single + Fix All | reproduced | renamed to `Getpid` vs. no fix |
| `EntryPoint = nameof(getpid)` | LF | code fix | reproduced | `nameof(Getpid)` + `Getpid` vs. no fix |
| `[LibraryImport("libc")] partial` | LF | code fix | reproduced | renamed vs. no fix |
| extern local function (RH4104) | LF | code fix | reproduced | call and declaration renamed vs. no fix |
| `Leaf : Middle : MetaBase` method / field-like event / custom event / protected, internal (IVT), public property | LF | code fix, metadata reference | reproduced | `Middle` and `Leaf` members renamed vs. no fix |
| interface shapes (source base implements metadata interface; derived type inherits implementation; source interface implemented by metadata base member) | LF | code fix, metadata reference | reproduced | renamed vs. no fix |
| direct metadata override; source-only chain; source interface via base; literal `EntryPoint` | LF | code fix | holds | already withheld / renamed together / renamed, as specified |

### Not covered, and why

- RH4116 tuple element names (#1023 Q1): RH4116 has no code fix. Lead decision 8 dropped it.
- Custom-event and property variants of the three interface shapes: one property interface shape (RH4114) and the method shapes are covered. The skeptic derives the others from the same cascade mechanism; I did not write them, one test per member kind per shape. The implementer may ask for them in a repair round.
- CRLF for this issue: rename-only fixes with no layout. The skeptic's runs agreed on LF and CRLF, and #1073 carries one CRLF test of the shared fix path.
- Formatter counterpart: the formatter does not rename (`AGENTS.md`, phase table); not involved.
- `extern` without an import attribute, `[MethodImpl(InternalCall)]`, P/Invoke parameters: the skeptic lists them as not members.

## thoenissen/Reihitsu#1073

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4002ClassNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceClassWouldHaveAFieldOfItsName` (spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenAFileScopedNamespaceClassWouldHaveAMethodOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenAFileLevelClassWouldHaveANestedTypeOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenANamespaceClassWouldHaveATypeParameterOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenAnotherPartOfAPartialClassHasAMemberOfTheNewName` (two files) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…FixIsOfferedWhenOnlyAnInheritedMemberHasTheNewName` | SHARED-CASING-009 (other side) | `Verify` | passes |
| `RH4002…FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` | SHARED-CASING-009 (other side) | `Verify` | passes |
| `RH4003StructNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceStructWouldHaveAFieldOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4004EnumNameCasingAnalyzerTests.FixIsOfferedWhenAnEnumMemberHasTheNewName` (spec example) | SHARED-CASING-009 | `Verify` | passes |
| `RH4005InterfaceNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceInterfaceWouldHaveAMethodOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4006DelegateNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceDelegateWouldHaveATypeParameterOfItsName` (spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4010RecordNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceRecordWouldHaveAPositionalParameterOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4011RecordStructNameCasingAnalyzerTests.NoFixIsOfferedWhenANamespaceRecordStructWouldHaveAPositionalParameterOfItsName` | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | fails |
| `RH4106PrivateFieldCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedIfBranchReferencesTheField` (spec example) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…NoFixIsOfferedWhenAnExcludedIfBranchReferencesTheFieldWithCarriageReturnLineFeed` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…NoFixIsOfferedWhenAnExcludedElifBranchReferencesTheField` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…NoFixIsOfferedWhenAnIfFalseBlockReferencesTheField` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…NoFixIsOfferedWhenAnExcludedBranchDeclaresTheFieldAgain` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…NoFixIsOfferedWhenAnExcludedBranchDeclaresTheNewName` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4106…FixLeavesAnExcludedBranchThatDoesNotNameTheFieldUnchanged` (existing; tag `SHARED-CASING-007` added) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | passes |
| `RH4115LocalVariableCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheLocalVariable` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenExcludedCodeReferencesTheClass` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4002…NoFixIsOfferedWhenExcludedCodeOfAnotherFileReferencesTheClass` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameWouldMatchAnInheritedProperty` (spec example) | RH4120-003, SHARED-FIX-003 | `Verify` | fails |
| `RH4120…NoFixIsOfferedWhenTheRenameWouldGiveADerivedRecordAnInheritedPropertyOfItsParameterName` (spec prose, reverse) | RH4120-003, SHARED-FIX-003 | `Verify` | fails |
| `RH4120…NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord` (lead decision 6) | RH4120-003, SHARED-FIX-003 | `Verify` | fails |
| `RH4120…NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4120…NoFixIsOfferedWhenTheRenameWouldSplitARecordStructParameterFromItsDeclaredProperty` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4114…NoFixIsOfferedWhenTheRenameWouldSplitThePropertyFromItsPositionalParameter` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4114…NoFixIsOfferedWhenTheRenameWouldSplitARecordStructPropertyFromItsPositionalParameter` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4111PrivatePropertyCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameWouldMatchAPositionalParameter` (join) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |
| `RH4109PublicFieldCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameWouldSplitTheFieldFromItsPositionalParameter` | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | fails |

Must-stay counterparts that existing tests already cover, all passing: `RH4120…FixRenamesParameterItsPropertyAndEveryReferenceToPascalCase` covers a positional parameter with no member of the same name. `RH4002…NoFixIsOfferedWhenRenamingANestedClassAddsACompileErrorInItsContainingType` covers the nested-type case. The RH4002 tests on namespace collisions cover the namespace collision and its arity and file-local exceptions.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Q1 `namespace N { class order { public int Order; } }` and siblings: method, nested type, type parameter, partial part in another file; file-scoped, block, and global namespace; struct, interface, delegate, record, record struct | LF | code fix, single + Fix All | reproduced | type renamed so a member, type parameter, or positional parameter has its name, vs. no fix |
| Q1 enum `color { Color }` | LF | code fix | holds | renamed, as specified |
| Q1 inherited member of the new name | LF | code fix | holds | renamed |
| Q1 reference that a nested `Order` would capture | LF | code fix | holds, differently than the skeptic derived | the skeptic predicted a renamer conflict and no fix. Observed: the fix is offered and the renamer qualifies the reference as `N.Order _value;`, which keeps it bound to the class. Kept as a passing test with that output |
| Q4 field referenced in `#if NEVER`, `#elif`, `#if false`; field redeclared in an excluded branch; new name declared in an excluded branch; local (RH4115); class referenced in an excluded branch in the same file and in another file | LF | code fix | reproduced | active code renamed and excluded text left behind, vs. no fix |
| Q4 field referenced in `#if NEVER` | CRLF | code fix | reproduced | same as LF |
| Q5 split: parameter (RH4120), property (RH4114), field (RH4109), record and record struct | LF | code fix | reproduced | one side renamed, vs. no fix |
| Q5 join: derived parameter matching an inherited property, base parameter giving a derived record an inherited property, private property matching a parameter (RH4111), base and derived both `count` | LF | code fix | reproduced | renamed, vs. no fix |

### Not covered, and why

- One excluded-code test per symbol family (type: RH4002, member: RH4106, local: RH4115), as the skeptic asked. The other kinds go through the same `GetDeclaredSymbol` dispatch into the shared `CasingCodeFixProviderBase<T>` path. That is not a static proof that one call site decides them all, because the implementer has not yet placed the check. If the implementer's guard branches per symbol kind, the auditor should ask for more.
- Q5 on RH4112/RH4113 properties and RH4106–RH4108/RH4110 fields: the skeptic says they share the path. RH4114, RH4111, and RH4109 are covered, one per accessibility group shape.
- A class or struct primary-constructor parameter with a field of the same name: no property is synthesized, so nothing is in question. Not written.
- Q1 for RH4004 enum with a non-member collision: not a candidate.

## Whole run

### Failure classes walked

- Covered:
  - FC-001: cascade members in other types and other documents, errors inside the renamed type.
  - FC-003: both sides of each condition. Literal versus own-name entry point, metadata versus source chain, inherited versus declared member, excluded text that names the symbol versus text that does not, record match versus no match.
  - FC-008: Fix All through `Verify` in every test.
  - FC-016: sibling type kinds and member kinds.
  - FC-018: references are retargeted through the renamer (`FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface`, `FixQualifiesAReference…`).
  - FC-020: `#if`, `#elif`, `#else`, `#if false`.
  - FC-024: partial type, `partial` LibraryImport method.
  - FC-033 and FC-034: metadata through an emitted image, not a project reference. The reported shapes are used verbatim.
  - FC-036: native binding and record public API.
- Not applicable:
  - FC-004/005/019/021/022/023/025–029: no trivia, layout, or literal edits. The rename touches identifiers only.
  - FC-007: a withheld fix needs no pass.
  - FC-009: covered by existing tests.
  - FC-010–014: formatter not involved.
  - FC-030: no formatting in a rename.
  - FC-035: expected values come from the spec examples and the skeptic's breaking inputs. The only output taken from running code is the `N.Order` qualification, observed on unchanged `main`.

### Files

- `src/Reihitsu.Analyzer.Test/Base/CompiledAssemblyReference.cs` (new helper)
- Test classes under `src/Reihitsu.Analyzer.Test/Naming/` for RH4002, RH4003, RH4004, RH4005, RH4006, RH4010, RH4011, RH4102, RH4103, RH4104, RH4106, RH4109, RH4111, RH4112, RH4113, RH4114, RH4115, RH4120

### Results

- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: 517 total, 474 passed, 43 failed. The 43 are exactly the "fails" rows above (41 distinct names, because the property cascade test name occurs in three classes).
- `scripts/test.sh --project architecture`: 115 passed (requirement tags and conventions).

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4103MethodNameCasingAnalyzerTests|FullyQualifiedName~RH4104LocalFunctionNameCasingAnalyzerTests|FullyQualifiedName~RH4102EventNameCasingAnalyzerTests|FullyQualifiedName~RH4112|FullyQualifiedName~RH4113|FullyQualifiedName~RH4114"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-FIX-003"
scripts/test.sh --project architecture
```
