Role: test implementer
Commit: c8a10ce (tests written against this head; committed on top of it)
Verdict: tests written

Repair round 2. This covers decisions 12 and 13 of `00-lead.md` and auditor findings 5 and 6 in `19-auditor.md`, against SHARED-CASING-009 as clarified in `20-spec-author.md` (8938f16). The prompt named the examples to cover (e1, e3, a property or event variant, l1 and l2). It carried no intended fix, suspected cause, or hoped-for verdict. Inputs and expected values come from the SHARED-CASING-009 examples and the fixtures in `19-auditor.md`, never from patched code. No production code was touched.

## Tests

All tests are in `src/Reihitsu.Analyzer.Test/Naming/`. "Fails" means the test fails on c8a10ce for the reason shown in the third column's note.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4103 `NoFixIsOfferedWhenAnotherImplementerOfTheSameInterfaceMethodImplementsAnInterfaceWithAMethodOfTheNewName` (e1, the spec example verbatim, all three spans marked) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: the fix on `A.process` renames the whole cascade to `Process` |
| RH4103 `NoFixIsOfferedWhenATypeDerivedFromTheOverriddenMethodsTypeImplementsAnInterfaceWithAMethodOfTheNewName` (e3, the spec example verbatim) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `process` → `Process` on `Base` and `Derived` |
| RH4114 `NoFixIsOfferedWhenAnotherImplementerOfTheSameInterfacePropertyImplementsAnInterfaceWithAPropertyOfTheNewName` (p1, the property shape of e1, `int Size => 1;` in `IOther`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `size` → `Size` on all three |
| RH4114 `NoFixIsOfferedWhenATypeDerivedFromTheOverriddenPropertysTypeImplementsAnInterfaceWithAPropertyOfTheNewName` (p3, the property shape of e3) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `size` → `Size` |
| RH4102 `NoFixIsOfferedWhenAnotherImplementerOfTheSameInterfaceEventImplementsAnInterfaceWithAnEventOfTheNewName` (v1, the auditor's fixture: `Root : IOther` declares `Changed`, `B : Root, INotifier, IOther`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | fails: `changed` → `Changed` on all three |
| RH4104 `FixRenamesALocalFunctionToTheNameOfAMethodOfAnInterfaceThatTheEnclosingTypeImplements` (l2, the spec example's input and output) | SHARED-CASING-009 | `Verify(src, fixed)` | fails: no fix offered |
| RH4104 `FixRenamesALocalFunctionToTheNameOfTheDisposeMethodInATypeThatImplementsIDisposable` (l1, a local `dispose` inside `void System.IDisposable.Dispose()`) | SHARED-CASING-009 | `Verify(src, fixed)` | fails: no fix offered |

In e1, p1, and v1 the interface member and the second implementer are also reported, as the spec example marks them. `Verify(src, src)` requires that no diagnostic offers a fix. On this head only the fix on `A`'s member is offered, because the other two are already withheld through their own hierarchies (e5 and the own-type check). The test therefore fails on the member that finding 5 names. In e3 and p3 both spans are reported. The fix that the base member's own hierarchy check misses is the one on the override.

Totals on c8a10ce: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` gives 7 failed and 750 passed. The 7 failures are exactly the rows above, and each fails in the first single-fix iteration for the stated reason.

Existing tests that must stay green on the other side of the changed conditions:
- Decision 12, the reported symbol's own hierarchy: b1–b6, a3, and the interface-as-declaring-type tests from `15-test-implementer.md`.
- Decision 12, a cascade with no hierarchy that holds the interface: RH4103 `FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy` (`Job`) and `FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface`. The latter's cascade holds the interface member, but no interface has the new name.
- Decision 13, the withheld side for type members: the RH4103 tests above. The local-function side with no interface: the existing RH4104 fix tests (l3-like, `Worker` without interfaces).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| e1, e3 (RH4103) | LF | single fix | reproduced | renamed vs no fix |
| p1, p3 (RH4114) | LF | single fix | reproduced | renamed vs no fix |
| v1 (RH4102) | LF | single fix | reproduced | renamed vs no fix |
| l2, l1 (RH4104) | LF | single fix | reproduced | no fix vs renamed |

Fix All: `Verify` also runs Fix All in document, project, and solution. On this head, the single-fix check fails first.

## Not covered, and why

- **CRLF.** The auditor ran e1, e3, p1, p3, v1, l1, and l2 under LF and CRLF with identical results (`19-auditor.md`, Evidence). The renamer edits identifiers only, and none of these tests involves layout.
- **Event variant of e3** (an overridden virtual event), **and e2/e4/e5.** p3 covers the override axis for a second member kind, and v1 covers the event kind on the sibling-implementer axis. e2, e4, and e5 hold on the head per the auditor, and b1–b6 and a3 already pin the reported symbol's own hierarchy.
- **Fix All with a co-occurring fixable diagnostic.** Each cascade fixture reports only members of one cascade, all withheld. No other instance co-occurs in these shapes.
- **Local functions under the other SHARED-CASING-009 paragraphs** (collision, rebinding). These are unchanged by decision 13 and covered by the existing RH4104 tests, for example `helper` next to a member `Helper`.
- **FC-019/020/021, FC-022, FC-023, FC-038–FC-043, formatter surface.** As in `03-`, `09-`, and `15-test-implementer.md`: no token gap, literal, or formatter is involved.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AnotherImplementerOfTheSameInterface|FullyQualifiedName~TheOverriddenMethodsType|FullyQualifiedName~TheOverriddenPropertysType|FullyQualifiedName~FixRenamesALocalFunctionToTheNameOf"
```

The second filter selects the 7 tests above, all failing on c8a10ce.
