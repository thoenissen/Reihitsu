Role: test implementer
Commit: 19d62f8 (tests written against this head; committed on top of it)
Verdict: tests written

Repair round 3: decisions 14 and 15 of `00-lead.md`, auditor findings 7 and 8 in `25-auditor.md`, against SHARED-CASING-009 as clarified in `26-spec-author.md` (861e6b8). The prompt named the examples to cover (RH4002 t1 with exact output, one more type kind, the RH4120 positional-record no-fix case and its control). It carried no intended fix, suspected cause, or hoped-for verdict. Inputs and expected values come from the SHARED-CASING-009 examples (`casing.md`, the RH4002 and RH4120 examples) and the auditor's fixtures t2 and t5, never from patched code. No production code was touched.

## Tests

All in `src/Reihitsu.Analyzer.Test/Naming/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH4002 `FixRenamesAClassToTheNameOfAMemberOfAnInterfaceItImplements` (t1, the spec example's input and output verbatim) | SHARED-CASING-009 | `Verify(src, fixed)` | fails: no fix offered (`processor` stays) |
| RH4002 `FixRenamesAClassToTheNameOfAMemberOfAnInterfaceThatADerivedClassImplements` (t2, `Derived : processor, IHasProcessor`; reference in the base list retargeted) | SHARED-CASING-009 | `Verify(src, fixed)` | fails: no fix offered |
| RH4003 `FixRenamesAStructToTheNameOfAMemberOfAnInterfaceItImplements` (t5) | SHARED-CASING-009 | `Verify(src, fixed)` | fails: no fix offered |
| RH4120 `NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty` (the spec example `Person(string name) : INamed`, `INamed.Name => "default"`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify(src, src)` | passes (head already withholds; pins decision 15 against the allow-list narrowing) |
| RH4120 `FixRenamesTheParameterWhenTheInterfaceHasNoMemberOfTheNewName` (control: interface member `Other`; the spec's "The fix is offered when `INamed` has no member named `Name`") | SHARED-CASING-009 | `Verify(src, fixed)` | passes |

Run on 19d62f8: 3 failed, 2 passed, exactly as above. Each failure is in "Iterative code fix application" with the diff `-public class/struct Processor` / `+... processor`, i.e. the issue's expected-versus-actual difference (fix withheld vs fix applied).

Both sides of the changed condition (FC-003): the type-rename side (t1, t2, t5: fixed) next to the withheld ordinary-member side already pinned by RH4103 e1/e3, RH4114 p1/p3, RH4102 v1 (`21-test-implementer.md`); the positional-parameter side withheld (RH4120 new test) next to its fixed control.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| t1 (RH4002, own type) | LF | single fix + Fix All | reproduced | no fix vs `Processor` |
| t2 (RH4002, derived type) | LF | single fix + Fix All | reproduced | no fix vs `Processor` |
| t5 (RH4003) | LF | single fix + Fix All | reproduced | no fix vs `Processor` |
| RH4120 `Person` | LF | single fix + Fix All | holds | no fix, as required |

## Not covered, and why

- **t4 (RH4010 record) and t7 (RH4011 record struct).** The lead asked for one more type kind; t5 (struct) is a second type kind next to the class, and t2 covers the derived-hierarchy axis. All five share one call site: every type rule's fix goes through `CasingCodeFixProviderBase{T}.RenameWithoutConflictAsync` into `HasInterfaceMemberOfNameInHierarchyAsync` with the cascade (`25-auditor.md`, policy table), and the constructor in the cascade is the same `MethodKind.Constructor` for all four kinds. The auditor ran t4 and t7 with the same result as t1.
- **CRLF.** The renamer edits identifiers only; the auditor saw identical LF and CRLF results in earlier rounds; no layout is involved.
- **Explicit constructor, static constructor, finalizer in the type.** Same `MethodKind` filter; the allow-list drops them by the same check the implicit constructor exercises. Not added beyond the requested scope.
- **FC-019–023, FC-038–043, formatter surface.** No token gap, literal, or formatter is involved (as in `03-`, `09-`, `15-`, `21-test-implementer.md`).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixRenamesAClassToTheNameOfAMemberOfAnInterface|FullyQualifiedName~FixRenamesAStructToTheNameOfAMemberOfAnInterface|FullyQualifiedName~NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty|FullyQualifiedName~FixRenamesTheParameterWhenTheInterfaceHasNoMemberOfTheNewName"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
```
