Role: test implementer
Commit: adb1c35 (production head unchanged; tests added on top)
Verdict: tests written — 16 new tests fail on the current head, 9 new tests pass

Called after audit round 1 for lead decisions 13 and 14: tests for every failing member of the auditor's findings 1–3 and for the test gaps of finding 4, with the counterparts that must keep the fix offered. My prompt named the structural change the lead decided (one shared determination of what the rename reaches). It carried no expected values, and I took none from it. Every expected value comes from SHARED-FIX-003, SHARED-CASING-007, and SHARED-CASING-009 as they stand.

Every test uses `Verify(source, fixedSource[, onConfigure])` on the rule's `BatchCodeFixTestsBase`. A "no fix" test passes the source as its own fixed source, so the iterative single fix and Fix All in document, project, and solution scope must all leave every document unchanged (FC-008). Multi-document cases add `/0/Test1.cs` to `TestState` and `FixedState`. Cross-project cases use `TestState.AdditionalProjects[...]` with `AdditionalProjectReferences`; the primary project is `TestProject`. Every diagnostic in every document is marked, and every analyzer stage passed.

Each of the 16 failing tests fails at `Context: Iterative code fix application`, with `content of '/0/Test0.cs' did not match`: a fix is offered and renames. That is the auditor's observed-versus-expected difference. None fails at compile time, in a helper, or on the diagnostics. I checked the diffs of the two cross-project tests: in the native case, `Native.getpid` in `TestProject` is renamed to `Getpid` by the fix on `INative.getpid` in the other project; in the query case, `select` is renamed to `Select`.

## thoenissen/Reihitsu#1070

Finding 1: the native entry point is reached through the rename cascade. Finding 3: compile errors in unchanged documents (the auditor relates it to #1070 mechanism 1).

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWithoutEntryPoint` (auditor's counterexample: `static abstract` + `[DllImport]`) | SHARED-FIX-003 | `Verify` | fails: both `getpid` renamed |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWhoseEntryPointIsNameofTheMethod` | SHARED-FIX-003 | `Verify` | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToALibraryImportImplementationWithoutEntryPoint` (CS8795 marked, no generator) | SHARED-FIX-003 | `Verify` | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameOfAStaticVirtualMethodCascadesToADllImportImplementation` | SHARED-FIX-003 | `Verify` | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationInADependentProject` (interface in project `Contracts`, implementation in `TestProject`) | SHARED-FIX-003 | `Verify` + additional project | fails: `Native.getpid` renamed in the dependent project |
| `RH4103…FixRenamesAStaticAbstractMethodTogetherWithADllImportImplementationWithALiteralEntryPoint` (other side of the condition) | SHARED-FIX-003 | `Verify` | passes: both renamed |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesAQueryOfAnotherDocumentAmbiguous` (auditor's counterexample, CS1940 in the unchanged document) | SHARED-CASING-009, SHARED-FIX-003 | `Verify`, two documents | fails: `select` → `Select` |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesAQueryOfADependentProjectAmbiguous` (query in project `Consumer`, which references `TestProject`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + additional project | fails |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesADeconstructionOfAnotherDocumentAmbiguous` (extension `deconstruct` vs. `Ext2.Deconstruct`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesAForeachOfAnotherDocumentAmbiguous` (extension `getEnumerator`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesAnAwaitOfAnotherDocumentAmbiguous` (extension `getAwaiter`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheNewNameMakesAQueryOfTheSameDocumentAmbiguous` (the error lands in a changed document; holds per the auditor) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | passes (characterizes the side that holds) |
| `RH4103…FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees` (other document queries with `System.Linq` only, without `using Ext;`) | SHARED-CASING-009 | `Verify`, two documents | passes: `select` → `Select`, other document unchanged |

## thoenissen/Reihitsu#1073

Finding 2: excluded code is reached through the rename cascade. Finding 4: test gaps of the reference-scope branches.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToAMethodReferencedInExcludedCode` (auditor's q4a) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: `Base.process` and the override renamed, excluded `process();` left |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToAMethodReferencedInExcludedCodeWithCarriageReturnLineFeed` | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, CRLF | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAPrivateNestedOverrideToAMethodReferencedInExcludedCode` (q4b) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAPrivateNestedImplementationToAnInterfaceMethodReferencedInExcludedCode` (q4c) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToATypeWhoseExcludedCodeDeclaresTheNewName` (excluded text declares the new name) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails |
| `RH4103…NoFixIsOfferedWhenTheRenameCascadesFromAnOverrideToAMethodOfAReferencedProjectReferencedInExcludedCode` (auditor's xp: base in `BaseProject`, override in `TestProject`) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` + additional project | fails |
| `RH4103…FixRenamesAFileLocalOverrideTogetherWithTheOverriddenMethodWhenNoExcludedCodeNamesIt` (excluded branch present, names neither name) | SHARED-CASING-007 | `Verify`, two documents | passes: both renamed, excluded `Stop();` unchanged |
| `RH4103…FixRenamesAnOverrideTogetherWithTheOverriddenMethodOfAReferencedProject` (cascade into another source project, no excluded code) | SHARED-CASING-007 | `Verify` + additional project | passes: renamed in both projects |
| `RH4105MethodParameterCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheParameter` (parameter → method branch) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | passes |
| `RH4121TypeParameterNameCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheTypeParameter` (type parameter → declaring symbol branch) | SHARED-CASING-007, SHARED-FIX-003 | `Verify` | passes |
| `RH4106PrivateFieldCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchOfAnotherPartReferencesTheField` (private → every part of the partial type) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | passes |
| `RH4106…FixIsOfferedWhenOnlyAnExcludedBranchOfAnUnrelatedTypeInAnotherFileNamesTheField` (narrowing side: the old name `Value` and the new name `_value` occur only in excluded code of an unrelated type `D` in another file) | SHARED-CASING-007 | `Verify`, two documents | passes: field renamed, other file unchanged |

The last test pins SHARED-CASING-007's bound "in a document where the symbol can be referred to" from the offered side. A private field cannot be referred to from an unrelated type in another document, so the fix stays offered there. Decision 13 says the excluded-code check takes "every document of every affected project". If the check applied that region to this case, this test would fail. That test is the observation the lead and the implementer should weigh, not a diagnosis.

## Whole run

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `static abstract`/`static virtual` interface method implemented by `[DllImport]` without `EntryPoint`, with `nameof(self)`, or by `[LibraryImport]` | LF | code fix, single + Fix All | reproduced | interface method and implementation renamed vs. no fix |
| the same with the interface in another project | LF | code fix | reproduced | the implementation in the dependent project is renamed vs. no fix |
| implementation with a literal `EntryPoint` | LF | code fix | holds | both renamed, as specified |
| file-local / private nested override, private nested implementation, excluded reference or excluded declaration of the new name in the base's document | LF (+ CRLF for the file-local case) | code fix, two documents | reproduced | base and override renamed, excluded text left vs. no fix |
| override in `TestProject` of a base in a referenced project with an excluded reference | LF | code fix, two projects | reproduced | renamed across projects vs. no fix |
| file-local override and cross-project override without excluded naming | LF | code fix | holds | renamed together, as specified |
| query `select`, extension `deconstruct`, `getEnumerator`, `getAwaiter` made ambiguous in an unchanged document; query in a dependent project | LF | code fix, two documents / two projects | reproduced | renamed vs. no fix |
| query in the same (changed) document; query that cannot see the extension | LF | code fix | holds | withheld / renamed, as specified |
| parameter, type parameter, partial-part private field named in excluded code; private field's names in excluded code of an unrelated file | LF | code fix | holds | withheld / offered, as specified |

### Not covered, and why
- **Finding 1 class members per member kind.** The cascade into a native binding needs a `static` method implementing a `static abstract`/`virtual` interface member. Overrides cannot occur, because `[DllImport]` requires `static` (auditor). Events and properties cannot be `extern` P/Invoke bindings. Only RH4103 reaches it. An extern local function (RH4104) has no cascade.
- **Finding 3, other query clauses (`Where`, `SelectMany`, `Join`, `GroupBy`, `OrderBy`, …).** Each binds through the same name-based query-pattern lookup as `Select`, and the same method-body error comparison decides it. One clause (`select`) plus the three other pattern bindings (`Deconstruct`, `GetEnumerator`, `GetAwaiter`) cover the distinct binding kinds the auditor listed.
- **Finding 2, interface implementation across projects and file-local implementations.** The cross-project member is covered by the override relation, the private-nested member by both relations, and the file-local member by the override relation. The remaining combinations are the same relations crossed with the same scopes. I wrote no test per combination; the lead may ask for them in a repair round.
- **CRLF.** These are rename-only fixes with no layout. One CRLF variant covers the excluded-text scan of another document, which is the only place where line breaks are content (disabled text).
- **Formatter counterpart.** The formatter does not rename; it is not involved.
- **Finding 5 and the `FixAllCandidate` summary hint.** Comment-only; routed to the implementer (decision 14). They need no test.
- **Failure classes.** FC-001/FC-020 (the guards' regions vs. the renamer's reach) are the subject of these tests. FC-003: each new condition has a test on both sides: native (unbound vs. literal entry point), excluded code (named vs. not named, reachable vs. unrelated document), compile errors (ambiguous vs. not visible, changed vs. unchanged document). FC-008: covered through the Fix All scopes of `Verify`. FC-017: every withheld test asserts that every document is unchanged, and every offered test asserts the full text of every document. The auditor's dismissals of FC-004/005/007/009–014/018/019/021–023/025–030/032/035/037 hold unchanged: no new production path is involved.

### Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4103MethodNameCasingAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4105MethodParameterCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheParameter|FullyQualifiedName~RH4121TypeParameterNameCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchReferencesTheTypeParameter|FullyQualifiedName~RH4106PrivateFieldCasingAnalyzerTests.NoFixIsOfferedWhenAnExcludedBranchOfAnotherPartReferencesTheField|FullyQualifiedName~RH4106PrivateFieldCasingAnalyzerTests.FixIsOfferedWhenOnlyAnExcludedBranchOfAnUnrelatedTypeInAnotherFileNamesTheField"
```
Naming run on head adb1c35 with these tests: 542 total, 526 passed, 16 failed (exactly the 16 marked "fails" above).
