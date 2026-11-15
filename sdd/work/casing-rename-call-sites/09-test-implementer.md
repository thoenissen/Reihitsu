Role: test implementer
Commit: a151c93 (tests written against this head; committed on top of it)
Verdict: tests written

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named decisions 6–8 of `00-lead.md`. Inputs and expected values come from the four new SHARED-CASING-009 examples in `sdd/specs/shared/casing.md` (`08-spec-author.md`) and the auditor's members in `07-auditor.md`, never from patched code. No production code touched.

## Tests

All in `src/Reihitsu.Analyzer.Test/Naming/`. "Fails" = fails on a151c93 in "Iterative code fix application" with exactly the auditor's observed rewrite (shown); "passes" = passes now and must keep passing.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldHideAStaticFieldSpelledLikeTheParameterOfTheOtherPart` (c4, spec example) | SHARED-CASING-009 | `Verify(src, src)` | fails: `return total + Counter.value;` |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldHideAnInstanceFieldSpelledLikeTheParameterOfTheOtherPart` (c6, "the same holds for an instance field") | SHARED-CASING-009 | `Verify(src, src)` | fails: `return total + this.value;` |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldChangeANameofOperandSpelledLikeTheParameterOfTheOtherPart` (c7, "and for `nameof(value)`") | SHARED-CASING-009 | `Verify(src, src)` | fails: `nameof(Counter.value).Length` |
| RH4105 `NoFixIsOfferedWhenTheNewNameWouldHideAFieldSpelledLikeTheParameterOfTheOtherPartNextToTheParameter` (c1, `return Value + value;`: the adjacent shape the auditor found withheld only by how the differ merges spans) | SHARED-CASING-009 | `Verify(src, src)` | passes |
| RH4121 `NoFixIsOfferedWhenTheNewNameWouldHideAClassSpelledLikeTheTypeParameterOfTheOtherPart` (d1, "It holds as well for RH4121") | SHARED-CASING-009 | `Verify(src, src)` | fails: `return total + N.TItem.Size();` |
| RH4103 `NoFixIsOfferedWhenTheRenamerWouldTurnACallOfTheRenamedExtensionMethodIntoAStaticCall` (a1, spec example) | SHARED-CASING-009 | `Verify(src, src)` | passes (withheld on HEAD; `main` fixed to `Extensions.ToList(values)`, per auditor and `08-spec-author.md`) |
| RH4103 `NoFixIsOfferedWhenTheRenamerWouldCastAnArgumentOfACallOfTheRenamedMethod` (a2, spec example) | SHARED-CASING-009 | `Verify(src, src)` | passes (withheld on HEAD; `main` fixed to `box.Write((object)1)`) |
| RH4103 `NoFixIsOfferedWhenTheNewNameWouldChangeWhichMethodImplementsAnInterfaceMethod` (a3, spec example) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `process` renamed to `Process` |
| RH4103 `FixRenamesAMethodThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface` (a3 control, `WorkerDerived : WorkerBase`) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4114 `NoFixIsOfferedWhenTheNewNameWouldChangeWhichPropertyImplementsAnInterfaceProperty` (a3, "the same holds for a property (RH4114)") | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `size` renamed to `Size` |
| RH4114 `FixRenamesAPropertyThatHidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface` (property control) | SHARED-CASING-009 | `Verify(src, fixed)` | passes |
| RH4102 `NoFixIsOfferedWhenTheNewNameWouldChangeWhichEventImplementsAnInterfaceEvent` (a3, "or an event") | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `changed` renamed to `Changed` |

Totals on a151c93 with these tests: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → 7 failed, 734 passed; the 7 failures are exactly the "fails" rows above.

Controls already in the suite that the repair must keep green: the a1/a2 counterpart RH4002 `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` (a qualified cascade reference stays fixed); for decision 6, every cascade-reference rename, in particular RH4105 `FixRenamesTheDifferentlySpelledParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt` (the other part's `other++` is a cascade reference spelled by a name the cascade gives up) and RH4121 `FixIsOfferedWhenAnExcludedBranchNamesTheDifferentlySpelledTypeParameterOfTheOtherPartOfAPartialMethod`; for decision 7, `FixRenamesAMethodThatHidesAnInheritedMethodCalledOnlyOnTheBaseType` and its RH4114 counterpart.

### Earlier handover, f26 row

The spec checker's finding 1 (`06-spec-checker.md`, decision 5) says the f26 row of `03-test-implementer.md` lists only SHARED-CASING-009. The row as committed in b66f38a already reads `SHARED-CASING-009, SHARED-FIX-003`, matching the test's two `[Requirement]` attributes in `RH4109PublicFieldCasingAnalyzerTests.cs`, so no change was needed; `03-test-implementer.md` is left as it is.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| c4, c6, c7 (RH4105) | LF | single fix | reproduced | field reference qualified (`Counter.value`, `this.value`, `nameof(Counter.value)`) vs no fix |
| c1 (RH4105, adjacent) | LF | single fix | not reproduced | no fix vs no fix |
| d1 (RH4121) | LF | single fix | reproduced | `N.TItem.Size()` vs no fix |
| a3 method / property / event | LF | single fix | reproduced | renamed vs no fix |
| a1, a2 (RH4103) | LF | single fix | withheld on HEAD, as required | no fix vs no fix |

Fix All: `Verify(src, src)` runs Fix All in document, project, and solution once the single fix is withheld; on this head the single-fix check fails first.

## Not covered, and why

- **CRLF.** Binding question only; the auditor ran every member under LF and CRLF with identical results, and the renamer edits only identifiers and qualifiers.
- **a3b** (a3 with `processor.Process()` in source). Already withheld on HEAD by the R2 count (auditor); a3 is the stricter input, since it has no reference at all.
- **Event control** for a3 (event not re-listing the interface). The method and property controls cover the counterpart side; the event arm is reached by the same `GetDeclaredSymbol` dispatch for an event, and adding it was not needed to pin the boundary. Cheap to add if the implementer's check turns out to branch per member kind.
- **Indexer re-implementation.** No casing rule reports an indexer's name.
- **`dynamic` receiver, reflection, serialization.** Outside the requirement (`08-spec-author.md`, *Internal choices*).
- **Multi-document / dependent-project variants of c4–a3.** The partial-method shapes put both parts in one class declaration, as the spec's example; the cross-document arm of the guard is covered by the round-0 tests (another document, dependent project).
- **FC-019/020/021, FC-022, FC-023, FC-038–FC-043, formatter surface.** As in `03-test-implementer.md` and the auditor's dismissals: no token gap, literal, or formatter involvement.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~TheRenamerWouldTurnACallOfTheRenamedExtensionMethodIntoAStaticCall|FullyQualifiedName~TheRenamerWouldCastAnArgumentOfACallOfTheRenamedMethod|FullyQualifiedName~ImplementsAnInterface|FullyQualifiedName~HidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface|FullyQualifiedName~SpelledLikeTheParameterOfTheOtherPart|FullyQualifiedName~SpelledLikeTheTypeParameterOfTheOtherPart"
```

The second filter selects the 12 new tests: 7 fail and 5 pass on a151c93.
