Role: test implementer
Commit: d0cdf4e
Verdict: tests written

This is the follow-up for decision 11 in `00-lead.md`. It adds tests for the block-body siblings that `05-test-implementer.md` listed under *Not covered*, and an offered counterpart in another `partial` document. All tests are in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, inserted after `VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField`. No existing test was changed.

`main` is 962d0c4. I checked it in a temporary worktree by running these tests against `main`'s production code, and removed the worktree afterwards. The `main` outputs come from `scripts/apply-fix.sh RH2001`, run under LF and CRLF during the 05 round.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenALambdaParameterInALocalDeclarationIsNamedLikeTheField` | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` at `Assert.IsEmpty(actions)` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAnAssignmentStatementIsNamedLikeTheField` | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` at `Assert.IsEmpty(actions)` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAConstructorIsNamedLikeTheField` | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` at `Assert.IsEmpty(actions)` |
| `VerifyNoFixIsOfferedWhenAnExplicitlyTypedLambdaParameterInABlockBodyIsNamedLikeTheField` | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` at `Assert.IsEmpty(actions)` |
| `VerifyNoFixIsOfferedWhenAQueryRangeVariableIsNamedLikeTheField` | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` at `Assert.IsEmpty(actions)` |
| `VerifyFixQualifiesThePropertyReferenceNextToALambdaParameterInAnExpressionBodyInAPartialPartInAnotherDocument` | RH2001-011; FC-024 | `Verify` + second document | passes on head and on `main` |

## Input and `main` output

| Test | Input (relevant line) | `main` output |
|---|---|---|
| Local declaration | `Func<int, int> add = _value => Value + _value;` in a method block | `_value => _value + _value` (misbinds) |
| Assignment | `add = _value => Value = _value;` | `_value => _value = _value` (misbinds) |
| Constructor | `_add = _value => Value + _value;` in a constructor | `_value => _value + _value` (misbinds) |
| Explicitly typed, block | `Func<int, int> add = (int _value) => Value + _value;` | `(int _value) => _value + _value` (misbinds) |
| Query range variable | `from _value in Enumerable.Range(0, 3) select Value + _value` in an expression body | `select _value + _value` (misbinds) |
| Partial, expression body | other document: `Func<int, int> Add() => _value => Value + _value;` | `Test0.cs`: `private int _value;`; `Test1.cs`: `_value => this._value + _value` (asserted) |

## Run

- Head, whole RH2001 class: 213 passed, 0 failed.
- `main`, the 6 new tests: the 5 withheld tests fail, and the offered partial test passes.

## Spec text

`05-test-implementer.md` already listed the spec text these tests bear on:

- `sdd/specs/analyzer/RH2001.md:969` names only "a parameter of an implicitly typed lambda". It covers neither an explicitly typed lambda in a block nor a query range variable.
- `sdd/specs/analyzer/RH2001.md:971` says an explicitly typed lambda keeps the fix. That holds only in an expression body.

## Not covered, and why

- **More statement contexts** (`using`, `foreach`, a local function body): `main` already misbinds in `return`, a local declaration, an assignment, and a constructor body. More statement kinds would test the same class again, so I added no test for each one.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~InALocalDeclarationIsNamedLikeTheField|FullyQualifiedName~InAnAssignmentStatementIsNamedLikeTheField|FullyQualifiedName~InAConstructorIsNamedLikeTheField|FullyQualifiedName~ExplicitlyTypedLambdaParameterInABlockBody|FullyQualifiedName~AQueryRangeVariableIsNamedLikeTheField|FullyQualifiedName~InAnExpressionBodyInAPartialPartInAnotherDocument"
```
