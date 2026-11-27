Role: test implementer
Commit: fc4cbc5
Verdict: reproduced

Repair round 1, auditor finding #1 (`06-auditor.md`), lead decision 5. The prompt carried no intended fix, suspected cause, or hoped-for verdict; decision 5's repair shape was not used for the tests. Expected values come from SHARED-CASING-009 ("A reference that the renamer renames, or renames and qualifies with `this.`, `base.`, a type, or a namespace, does not withhold the fix") and from the renamer's own qualifier: `main`'s fix output where `main` offers the fix, and a throwaway direct `Renamer.RenameSymbolAsync` probe (deleted, not committed) where it does not (`Base<(int, int)>.GetValue()`, `Base<(int A, string B)>.GetValue()`, `Base<System.Collections.Generic.List<int[]>>.GetValue()`). No value comes from patched code.

## Tests

Helper for every test: `Verify(source, fixedSource)` on `BatchCodeFixTestsBase` (single fix, one pass, re-analysis, Fix All).

| Test | Requirement / failure class | Helper | Expected now (head `bb9855c`) | On `main` |
|---|---|---|---|---|
| `RH4103MethodNameCasingNameBindingTests.FixQualifiesTheCallWithTheGenericBaseTypeNextToALocalFunctionWithTheNewName` row `int` | SHARED-CASING-009 | `Verify` | pass | pass |
| same, rows `int[]`, `int[,]`, `int[][]`, `int?`, `int?[]`, `List<int[]>` (with `using System.Collections.Generic;`) | SHARED-CASING-009 | `Verify` | **fail** (no fix offered) | pass — PR-introduced regression |
| same, rows `(int, int)`, `(int A, string B)` | SHARED-CASING-009 | `Verify` | **fail** | **fail** |
| same, row `List<int>` | SHARED-CASING-009 | `Verify` | pass | pass |
| `RH4103…FixQualifiesTheCallWithTheGenericBaseTypeWhoseTypeArgumentHasANamespace` row `System.Collections.Generic.List<int>` | SHARED-CASING-009 | `Verify` | pass | **fail** (head fixes what `main` withheld) |
| same, row `System.Collections.Generic.List<int[]>` | SHARED-CASING-009 | `Verify` | **fail** | **fail** |
| `RH4002ClassNameCasingAnalyzerTests.FixQualifiesAReferenceWithTheGenericBaseTypeThatANestedTypeOfTheNewNameWouldCapture` row `int` (qualified name in type context: `Base<int>.Item _value;`) | SHARED-CASING-009 | `Verify` | pass | pass |
| same, rows `int[]`, `(int, int)` | SHARED-CASING-009 | `Verify` | **fail** | pass — PR-introduced regression |

11 rows fail on the head, 4 pass. Every failing row fails on the finding's own difference: the diff is exactly the declaration and the reference left as written (no fix offered), nothing else.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH4103 `Base<int[]>` (auditor's counterexample), `int[,]`, `int?` | LF and CRLF (`scripts/apply-fix.sh`), LF (test) | code fix, head | reproduced | no fix vs `Base<…>.GetValue() + GetValue() + other` |
| Same on `main` (temporary worktree, removed) | LF and CRLF | code fix | holds on `main` | fixed as expected |
| RH4103 `int[][]`, `int?[]`, `List<int[]>` with a using | LF and CRLF | code fix | reproduced on head, holds on `main` | members the auditor did not list |
| RH4103 `(int, int)`, `(int A, string B)`, `System.Collections.Generic.List<int[]>` | LF and CRLF | code fix | fail on head and `main` | no fix vs qualified call |
| RH4002 nested type `item` of `Base<T>` referenced in `Derived : Base<…>` that declares `Item` | LF and CRLF | code fix | `int[]`, `(int, int)` reproduced on head, hold on `main`; `int` holds on both | no fix vs `Base<…>.Item _value;` (a `QualifiedNameSyntax`, not a member access) |

Correction to the auditor's member list: `List<int[]>` written with a `using` is fixed on `main` (so it is a regression, not "fail on both"); the fail-on-both member is the namespace-qualified spelling `System.Collections.Generic.List<int[]>`. `System.Collections.Generic.List<int>` is withheld on `main` and fixed on the head.

## Not covered, and why

- **`string?` type argument**: the renamer writes `Base<string>`, dropping the annotation; asserting that would pin an output this finding is not about. Not added.
- **Cref qualifier** (`<see cref="getValue"/>` with a nested type of the new name in `Derived : Base<int[]>`): `scripts/apply-fix.sh` showed the fix renames the method and leaves the cref text unchanged on both `main` and the head, so no qualified cref was produced and there is no member to test. Whether the runner parses documentation comments was not checked.
- **Other casing rules** (RH4106, RH4111 with a generic base): the check is one generic method for every casing rule (`rg -n "RewritesOtherCodeAsync" src/` → `CasingCodeFixProviderBase{T}.RenameWithoutConflictAsync` only); the qualifier shape does not depend on the renamed symbol's kind. Member access is covered on RH4103, qualified name on RH4002.
- **Alias-qualified name** (`global::`/alias `::` with generic type): pinned neighbor `RH4002…FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` covers the namespace qualifier; no generic alias-qualified rewrite was found in probing, none written.
- **Qualifier containing the old or new name** (namespace `N.Order` around class `order`): refused on `main` too; the auditor recorded it as not a regression and outside #1.
- **CRLF**: the check reads no line ending; every probe gave LF and CRLF the same result.
- Failure classes: FC-044, FC-001, FC-016 (sibling qualifier shapes) covered above; FC-003 both sides (holding `int`, `List<int>`, qualified `List<int>` next to the failing rows). Formatter, layout, and gap classes do not reach the rename check.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixQualifiesTheCallWithTheGenericBaseType|FullyQualifiedName~FixQualifiesAReferenceWithTheGenericBaseType"
```

## Follow-up candidates

_None._
