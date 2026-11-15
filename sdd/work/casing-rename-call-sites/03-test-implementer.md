Role: test implementer
Commit: 9fc76b7 (tests written against this head; committed on top of it)
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-CASING-009 (as extended in `02-spec-author.md`) and from the inputs themselves, never from patched code.

## Reproduction

The issue's own inputs (`select`, `add`) are withheld on this head by SHARED-CASING-011 and are covered by the existing tests `NoFixIsOfferedWhenTheDerivedNameIsANameTheCompilerLooksUp` (DataRows `select`, `add`) and the `...TheRenameWouldChangeTheMethodA...Calls` tests in `RH4103MethodNameCasingAnalyzerTests`; they pass. The class the issue describes reproduces with names outside that list: `NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCall` (the spec's first new example) fails with the fix offered and `values.ToList()` rewritten to `Enumerable.ToList(values)`.

Every new `NoFix…`/`FixAll…` test below fails on this head in "Iterative code fix application" (single fix) with the diff shown in *Observed*; the four controls pass. With `Verify(source, source)`, the harness also runs Fix All in document, project, and solution, so once the single fix is withheld the same tests assert Fix All; on this head the single-fix check fails first and masks it. Fix All was therefore observed separately with a throw-away test (deleted, not committed): with `FixedState` set to today's single-fix output and `BatchFixedState` set to the input, both the R1 input (`toList`) and the R2 input (`process`) failed in "Fix all in document" with the same rename/rewrite as the single fix.

## Tests

All in `src/Reihitsu.Analyzer.Test/Naming/`. "Fails" = fails on 9fc76b7; "passes" = control, passes now and must keep passing.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCall` (f03, spec example) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `FixRenamesAnExtensionMethodWhenTheCallOfAnotherNamespaceKeepsItsBinding` (spec control, namespace `M`) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfANestedNamespace` (f16) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinOverAnExtensionMethodCalledOnTheType` (f05) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldMakeAnExtensionMethodCallAmbiguous` (f12) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinOverloadResolution` (f04, spec example) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameOfAGenericMethodWouldWinOverloadResolution` (f17) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnOuterMethodInANestedClass` (f07) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinOverAMethodOfAUsingStaticImport` (f08) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideATypeUsedInTheClass` (f10) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledInTheClass` (f21, `base.`) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledOnTheDerivedType` (f06, R2, spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedVirtualMethodCalledOnTheDerivedType` (f20, R2) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedStaticMethodCalledThroughTheDerivedType` (f23, R2) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails |
| RH4103 `FixRenamesAMethodThatHidesAnInheritedMethodCalledOnlyOnTheBaseType` (R2 control: receiver `Base`, binding unchanged) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfAnotherDocument` (R1, reference in another document) | SHARED-CASING-009 | `Verify` + second `TestState` source | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfADependentProject` (R1, reference in a dependent project, namespace `N.Consumer`) | SHARED-CASING-009 | `Verify` + `AdditionalProjects["Consumer"]` | fails |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledInADependentProject` (R2, dependent project) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` + `AdditionalProjects["Consumer"]` | fails |
| RH4103 `FixAllSkipsTheRenameThatWouldWinAnExtensionMethodCall` (R1 + plain rename in one document) | SHARED-CASING-009 | `Verify` with `BatchFixedState` markup | fails |
| RH4103 `FixAllSkipsTheRenameThatWouldWinOverloadResolution` (R1 + plain) | SHARED-CASING-009 | `Verify` with `BatchFixedState` markup | fails |
| RH4103 `FixAllSkipsTheRenameThatWouldHideAnInheritedMethodCalledOnTheDerivedType` (R2 + plain) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` with `BatchFixedState` markup | fails |
| RH4104 `NoFixIsOfferedWhenTheNewNameWouldHideAMethodOfTheClass` (f24, `this.`) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldHideAFieldUsedUnqualified` (f14, spec example) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4105 `FixRenamesAParameterWhoseNewNameHidesAFieldReferencedOnlyThroughThis` (control: `this.name`, binding unchanged) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldHideAStaticFieldUsedUnqualified` (f18, type qualification) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldChangeTheOperandOfANameof` (f11) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4105 `FixAllSkipsTheRenameThatWouldHideAFieldUsedUnqualified` (R1 + plain) | SHARED-CASING-009 | `Verify` with `BatchFixedState` markup | fails |
| RH4115 `NoFixIsOfferedWhenTheNewNameWouldHideAFieldUsedUnqualified` (f13) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4117 `NoFixIsOfferedWhenTheNewNameWouldHideAFieldUsedUnqualified` (deconstruction variable; not run by the skeptic) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4002 `NoFixIsOfferedWhenTheNewNameWouldWinOverAnImportedClass` (f09) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4114 `NoFixIsOfferedWhenTheNewNameWouldHideATypeUsedInTheClass` (f19) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4114 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedPropertyReadOnTheDerivedType` (f22, R2) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails |
| RH4114 `FixRenamesAPropertyThatHidesAnInheritedPropertyReadOnlyOnTheBaseType` (R2 control) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4114 `FixAllSkipsTheRenameThatWouldHideAnInheritedPropertyReadOnTheDerivedType` (R2 + plain) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` with `BatchFixedState` markup | fails |
| RH4109 `NoFixIsOfferedWhenTheNewNameWouldHideAnOuterFieldInANestedClass` (f25) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4109 `NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedFieldReadOnTheDerivedType` (f26, R2) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails |
| RH4102 `NoFixIsOfferedWhenTheNewNameWouldHideAnOuterMethodInANestedClass` (f27) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4101 `NoFixIsOfferedWhenTheNewNameWouldHideAConstantUsedInAnInitializer` (f28) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4121 `NoFixIsOfferedWhenTheNewNameWouldHideATypeUsedInTheClass` (f29) | SHARED-CASING-009 | `Verify(src, src)` | fails |
| RH4120 `NoFixIsOfferedWhenTheNewNameWouldHideAConstantOfTheEnclosingClass` (record primary constructor parameter; not run by the skeptic) | SHARED-CASING-009 | `Verify(src, src)` | fails |

Totals on 9fc76b7: 36 fail, 4 pass (`scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: 36 failed, 693 passed; every failure is a new test).

Existing controls that must keep passing (not changed): `FixRenamesTheDeclarationAndEveryReference` and `FixRenamesOverridesAndImplementationsWithTheirBase` (references to cascade symbols are renamed; the qualifier "a reference to a symbol of the rename cascade does not count"), the `GetFixAllScenario` of each casing class, the SHARED-CASING-011 tests (issue's `select`/`add`), and the SHARED-CASING-009 collision tests such as RH4106's `_value` example.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue's `select` / `add(int)`+`Add(object)` inputs | LF | RH4103 code fix | not reproduced (withheld by SHARED-CASING-011) | no fix offered vs no fix offered |
| f03 `toList` + `values.ToList()` | LF | single fix | reproduced | `Enumerable.ToList(values)` written vs no fix |
| f03 | LF | Fix All in document (throw-away test) | reproduced | same rewrite vs no fix |
| f06 `process` hiding `Base.Process`, `derived.Process()` | LF | single fix | reproduced | declaration renamed, call text unchanged (now binds to `Derived.Process`) vs no fix |
| f06 | LF | Fix All in document (throw-away test) | reproduced | same vs no fix |
| f03 with `User` in a dependent project | LF | single fix (throw-away test with the consumer in `FixedState`) | reproduced | consumer's `values.ToList()` rewritten to `Enumerable.ToList(values)` vs no fix |
| f04, f05, f07–f13, f16–f29, RH4117, RH4120 shapes | LF | single fix | reproduced | each rewrite as in the skeptic's table (`this.`, `base.`, `Outer.`, `N.`, `Other.`, `System.Math.`, `(object)`, `Enumerable.X(...)`, `A.Shuffle(...)`, `nameof(Guard.value)`) or, for R2, the bare rename vs no fix |

Observed rewrites for the two shapes the skeptic did not run: RH4117 `total = Total + retry; return total;` became `this.total = total + retry; return this.total;`; RH4120 `Label => Name` became `Label => Outer.Name`.

## Not covered, and why

- **CRLF.** The change is semantic (which symbol a name binds to); no layout or trivia is involved, and the renamer edits only identifiers and qualifiers. The skeptic ran every fixture under LF and CRLF with identical results. Line endings are not a dimension here.
- **Formatter counterpart surface.** The casing fixes never pass through the formatter (skeptic, *Surfaces*); there is no formatter behavior for a rename.
- **FC-019/020/021 (comments and directives in a gap).** The fix rewrites no token gap; the existing SHARED-FIX-012 and excluded-code tests in RH4103 cover directives in renamed documents.
- **FC-023 (verbatim/Unicode identifiers), FC-024 (partial, explicit interface implementations).** Not on the binding question; existing cascade tests cover partial and explicit-implementation renames, and they must keep passing.
- **FC-018.** References are retargeted through the renamer; the tests assert exact output, so a syntax-only rename would fail them.
- **FC-007/FC-008 (convergence, stale Fix All positions).** `Verify` re-analyses after each pass and runs Fix All in document, project, and solution; the four multi-diagnostic Fix All tests put a withheld and a renamed diagnostic in one document.
- **One test per rule for the shared mechanism, not one per lookup kind per rule.** All 25 providers derive from `CasingCodeFixProviderBase<T>` and reach the renamer through `RenameWithoutConflictAsync` (`rg -n "RenameWithoutConflictAsync" src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`); but `GetDeclaredSymbol` switches over the declaration kind, so every declared kind in the skeptic's table has at least one test. The lookup-kind axis is covered fully on RH4103, the only rule where every lookup kind can occur.
- **R2 for events (RH4102) and for a nested type that hides a base type's nested type; R1 for RH4003–RH4006/RH4010/RH4011, RH4106–RH4108/RH4110, RH4111–RH4113.** Same declared symbol kinds as tested rules (named type, field, property, event), same dispatch arm; not added.
- **Same-text R2 with the reference in another document of the same project.** Covered for R1 (another document) and for R2 (dependent project), which is the harder of the two.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4103MethodNameCasingAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~NoFixIsOfferedWhenTheNewNameWould|FullyQualifiedName~NoFixIsOfferedWhenTheNewNameOfAGenericMethod|FullyQualifiedName~FixAllSkipsTheRenameThatWould|FullyQualifiedName~FixRenamesAnExtensionMethodWhenTheCallOfAnotherNamespaceKeepsItsBinding|FullyQualifiedName~FixRenamesAMethodThatHidesAnInheritedMethodCalledOnlyOnTheBaseType|FullyQualifiedName~FixRenamesAParameterWhoseNewNameHidesAFieldReferencedOnlyThroughThis|FullyQualifiedName~FixRenamesAPropertyThatHidesAnInheritedPropertyReadOnlyOnTheBaseType"
```

The last filter selects the 40 new tests plus the four existing SHARED-CASING-011 tests `FixAllSkipsTheRenameThatWouldChangeTheMethodA…Calls`, which pass: 36 fail and 8 pass on 9fc76b7.
