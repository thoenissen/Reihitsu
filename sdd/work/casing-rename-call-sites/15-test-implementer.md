Role: test implementer
Commit: bff02fe (tests written against this head; committed on top of it)
Verdict: tests written

Architecture-finding repair (decision 10 of `00-lead.md`, auditor finding #4 in `13-auditor.md`, SHARED-CASING-009 as clarified in `14-spec-author.md`). The prompt said decision 10 makes the two controls that asserted a fix where the type does not list the interface wrong. That matches decision 10 and the spec text as committed in 141b218, so I followed it. Apart from that, the prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs and expected values come from the SHARED-CASING-009 interface paragraph and its examples, never from patched code. No production code touched.

## Tests

All in `src/Reihitsu.Analyzer.Test/Naming/`. "Fails" means it fails on bff02fe because the fix is offered and renames the member (diff shown), where no fix is required. "Passes" means it passes now and must keep passing.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4103 `NoFixIsOfferedWhenTheNewNameIsThatOfAnInterfaceMethodThatTheTypeImplementsThroughItsBaseType` (former control `FixRenamesAMethodThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface`, converted per decision 10; the spec's a3 `class Derived : Base` variant) | SHARED-CASING-009 | `Verify(src, src)` | fails: `work` → `Work` |
| RH4103 `NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAnInterfaceWithAMethodOfTheNewName` (b1, spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `process` → `Process` |
| RH4103 `NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAnInterfaceWhoseMethodOfTheNewNameHasADefaultImplementation` (b2) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `process` → `Process` |
| RH4103 `NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAgainAnInterfaceThatTheBaseTypeImplements` (b4) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `process` → `Process` |
| RH4103 `NoFixIsOfferedWhenATypeImplementingTheMethodsInterfaceImplementsAnotherInterfaceWithAMethodOfTheNewName` (declaring type is an interface: "also by a type that implements it") | SHARED-CASING-009 | `Verify(src, src)` | fails: `ISecond.process` and `int ISecond.process()` renamed |
| RH4103 `NoFixIsOfferedWhenAnInterfaceInheritingFromTheMethodsInterfaceInheritsAnotherInterfaceWithAMethodOfTheNewName` (declaring type is an interface: "or an interface that inherits from it") | SHARED-CASING-009 | `Verify(src, src)` | fails: `process` → `Process` |
| RH4103 `FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy` (new control, spec example `Job : System.IDisposable`) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4114 `NoFixIsOfferedWhenTheNewNameIsThatOfAnInterfacePropertyThatTheTypeImplementsThroughItsBaseType` (former control `FixRenamesAPropertyThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface`, converted per decision 10) | SHARED-CASING-009 | `Verify(src, src)` | fails: `size` → `Size` |
| RH4114 `NoFixIsOfferedWhenATypeDerivedFromThePropertysTypeListsAnInterfaceWithAPropertyOfTheNewName` (b5) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `size` → `Size` |
| RH4102 `NoFixIsOfferedWhenATypeDerivedFromTheEventsTypeListsAnInterfaceWithAnEventOfTheNewName` (b6) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `changed` → `Changed` |
| RH4109 `FixRenamesAFieldToTheNameOfAMethodOfAnInterfaceThatADerivedTypeImplements` (field side: "A renamed field … is not withheld by this paragraph"; `Derived` implements `IProcessor.Process` explicitly, so no hiding or mapping is involved) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |

The two converted controls carry only SHARED-CASING-009, because no interface mapping changes there. The b-members also carry SHARED-FIX-003 because their run-time target changes (auditor). The interface-as-declaring-type tests carry only SHARED-CASING-009, because nothing changes at run time and they are withheld by name alone.

Totals on bff02fe: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` gives 9 failed and 741 passed. The 9 failures are exactly the "fails" rows above, and each fails in the first fix iteration on the rename alone.

Existing controls the repair must keep green, on the other side of the name check:
- RH4103 `FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface`: the interface member is in the cascade and today has the old name `process`, so no interface member "already has" the new name.
- RH4103 `NoFixIsOfferedWhenTheNewNameWouldChangeWhichMethodImplementsAnInterfaceMethod`, and its RH4114 and RH4102 counterparts (a3, own type lists the interface): unchanged and still withheld.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| b1, b2, b4 (RH4103), b5 (RH4114), b6 (RH4102) | LF | single fix | reproduced | renamed vs no fix |
| former controls (RH4103, RH4114) | LF | single fix | fixed, which the spec now forbids | renamed vs no fix |
| interface as declaring type, implementing type / inheriting interface (RH4103) | LF | single fix | fixed | renamed vs no fix |
| `Job` control (RH4103), field control (RH4109) | LF | single fix | fixed, as required | renamed vs renamed |

Fix All: once the single fix is withheld, `Verify(src, src)` also runs Fix All in document, project, and solution. On this head, the single-fix check fails first.

## Not covered, and why

- **CRLF.** These are binding and name questions only. The auditor and the spec author ran b1 and the control under LF and CRLF with identical results, and the renamer edits identifiers only.
- **Property and event variants of b2 and b4, and of the interface-as-declaring-type cases.** The paragraph is one name check per renamed member for methods, properties, and events. The method tests cover every hierarchy axis, and b5 and b6 cover the member-kind axis on the b1 shape. If the implementation branches per member kind and per axis, these are cheap to add.
- **Other kinds excluded by the paragraph (parameter, local, type, type parameter).** The field test pins the excluded side. A parameter, local, or type parameter cannot share an interface member's name scope in a way that implements it. A nested type named like an interface member of a derived type is a further shape the spec author's *Internal choices* left out; not added.
- **Member-kind mismatch** (a renamed property whose new name is that of an interface method). The spec author's *Internal choices* says any member kind withholds. The interface-as-declaring-type tests use methods only, so this choice is not pinned. Add a test if the lead wants it fixed in place.
- **Multi-project hierarchy** (derived type in a dependent project). The spec says "a type in the solution". Not added, because the round-0 cross-project tests cover the guard's solution-wide arm, and b1 is the stricter single-document input. Worth one test if the implementation searches derived types per project.
- **FC-019/020/021, FC-022, FC-023, FC-038–FC-043, formatter surface.** As in `03-test-implementer.md` and `09-test-implementer.md`, these involve no token gap, literal, or formatter.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ThatTheTypeImplementsThroughItsBaseType|FullyQualifiedName~ATypeDerivedFromThe|FullyQualifiedName~ATypeImplementingTheMethodsInterface|FullyQualifiedName~AnInterfaceInheritingFromTheMethodsInterface|FullyQualifiedName~OutsideItsTypesHierarchy|FullyQualifiedName~FixRenamesAFieldToTheNameOfAMethodOfAnInterfaceThatADerivedTypeImplements"
```

The second filter selects the 11 tests above: 9 fail and 2 pass on bff02fe.
