Role: test implementer
Commit: 1c4c35b
Verdict: tests written

Repair round for the five disputed tests (`00-lead.md`, decision 8; `04-implementer.md`, *Disputed tests or scope*). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

"`main`" below is `origin/main` at `962d0c4`, in a separate worktree. Outputs were measured there in two ways:

- with `scripts/apply-fix.sh RH2001` (LF and CRLF, identical);
- for the cross-document case, which `apply-fix.sh` cannot express, by running the test against `main`'s production code with a temporary expected value that exposed the diff.

The worktree was restored afterwards. All tests are in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`.

## Findings on `main`

- **The original inputs keep every binding.**
  - f09, s01, s12, and the f09 partial: the lambda stands in an *expression body*, and `main` qualifies the renamed reference (`this._value` / `Sample._value`).
  - f07: the property is `int`, and `main` leaves `this._value()` as written, still bound to the extension method.
  - This confirms `04-implementer.md`.
- **Lambda class (f09/s01/s12/partial).** `main` misbinds when the lambda stands in a *block body* (a statement). Each of these gives `_value => _value + _value`:
  - `return _value => Value + _value;` in a block-bodied method;
  - `Func<int, int> add = _value => Value + _value;` as a local declaration;
  - `add = _value => Value = _value;` as an assignment;
  - `_add = _value => Value + _value;` in a constructor.

  The same holds for the parenthesized form, for a static property in a static method, and in another `partial` document.
- **Expression-body variants.** `main` qualifies correctly in all of these: `Select(_value => …)`, a generic call, overload resolution, a `dynamic` argument, a nested lambda, two parameters, an `async` lambda, the parameter written before `Value`, and a static field initializer (`Sample._value`).
- **Extension-method class (f07).** When the property's type is invocable (a delegate type such as `Func<int>`), `main` rewrites `this._value()` to `Extensions._value(this)`, which is the change doc's rewrite. With an `int` property, `main` leaves the call as written in every variant tried: a receiver `other._value()`, an argument `this._value(1)`, the method group `Func<int> f = this._value`, and `nameof`.
- **Branch head.** `apply-fix.sh RH2001` on 1c4c35b withholds every misbinding or rewriting variant above, and offers the original f07, f09, s01, and s12 inputs.

## Tests

Each disputed test now has an input of its own row's class on which `main` misbinds or rewrites, and still asserts that the fix is withheld. Next to each single-document one, a new test asserts the fix `main` offers on the original input, with its exact output, so both sides of the condition are covered.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenASimpleLambdaParameterInABlockBodyIsNamedLikeTheField` (renamed from `…WhenASimpleLambdaParameterIsNamedLikeTheField`) — f09 | RH2001-011; FC-006 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` (action registered) |
| `VerifyFixQualifiesThePropertyReferenceNextToASimpleLambdaParameterInAnExpressionBodyNamedLikeTheField` (new) — f09 original | RH2001-011; FC-003 | `Verify` | passes on head and `main` |
| `VerifyNoFixIsOfferedWhenAParenthesizedLambdaParameterWithAnInferredTypeInABlockBodyIsNamedLikeTheField` (renamed) — s01 | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` |
| `VerifyFixQualifiesThePropertyReferenceNextToAParenthesizedLambdaParameterWithAnInferredTypeInAnExpressionBodyNamedLikeTheField` (new) — s01 original | RH2001-011; FC-003 | `Verify` | passes on head and `main` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInTheBlockBodyOfAStaticMethodIsNamedLikeTheFieldOfAStaticProperty` (renamed) — s12 | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` |
| `VerifyFixQualifiesTheStaticPropertyReferenceWithTheTypeNextToALambdaParameterInAnExpressionBodyNamedLikeTheField` (new) — s12 original | RH2001-011; FC-003 | `Verify` | passes on head and `main` |
| `VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField` (name kept; other document now block-bodied) — f09, other part | RH2001-011; FC-024 | `Verify(source, source, …)` + second document | passes on head; fails on `main` (fix renames `Value`) |
| `VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheFieldOfADelegateTypedProperty` (renamed from `…WhenAnExtensionMethodIsNamedLikeTheField`) — f07 | RH2001-011 | `VerifyNoCodeFixIsOffered` | passes on head; fails on `main` |
| `VerifyFixIsOfferedNextToAnExtensionMethodNamedLikeTheFieldWhoseCallKeepsItsBinding` (new) — f07 original | RH2001-011; FC-003 | `Verify` | passes on head and `main` |

Per test, the input, `main`'s output, and the assertion:

| Row | Input (changed line) | `main` output | Assertion |
|---|---|---|---|
| f09 | `Func<int, int> Add() { return _value => Value + _value; }` | `return _value => _value + _value;`, which misbinds | withheld |
| f09 original | `Func<int, int> Add() => _value => Value + _value;` | `… => _value => this._value + _value;` | offered, with that output |
| s01 | `Func<int, int> Add() { return (_value) => Value + _value; }` | `return (_value) => _value + _value;`, which misbinds | withheld |
| s01 original | `Func<int, int> Add() => (_value) => Value + _value;` | `… => (_value) => this._value + _value;` | offered, with that output |
| s12 | `static Func<int, int> Add() { return _value => Value + _value; }` (static property) | `return _value => _value + _value;`, which misbinds | withheld |
| s12 original | `static Func<int, int> Add() => _value => Value + _value;` | `… => _value => Sample._value + _value;` | offered, with that output |
| f09, other part | other document: `Func<int, int> Add() { return _value => Value + _value; }` | `Test0.cs`: `private int _value;`; `Test1.cs`: `return _value => _value + _value;`, which misbinds | withheld; both documents unchanged |
| f07 | `private Func<int> Value { get; set; } = () => 2;` … `return Value() + this._value();` | `private Func<int> _value = () => 2;` … `return _value() + Extensions._value(this);`, a rewrite (answer B) | withheld |
| f07 original | `private int Value { get; set; }` … `return Value + this._value();` | `private int _value;` … `return _value + this._value();`, left as written and still bound to the extension method | offered, with that output |

No other test was changed. Class run on head: 207 passed, 0 failed (203 before, plus the four new tests).

## Spec and change-doc text that no longer matches

- **`sdd/specs/analyzer/RH2001.md:969`** (RH2001-011, paragraph 2): "a parameter of an implicitly typed lambda". On `main`:
  - an implicitly typed lambda in an expression body is qualified correctly and keeps the fix;
  - in a block body, both an implicitly typed lambda and an explicitly typed one (`Func<int, int> add = (int _value) => Value + _value;` → `(int _value) => _value + _value`) misbind;
  - so does a query range variable: `from _value in … select Value + _value` → `select _value + _value`.

  The branch withholds all three. The distinguishing fact is the block body, not the implicit typing.
- **`sdd/specs/analyzer/RH2001.md:971`** (paragraph 3): "a parameter of an explicitly typed lambda" keeps the fix. That holds only in an expression body, as the existing `VerifyFixQualifiesThePropertyReferenceNextToAnExplicitlyTypedLambdaParameterNamedLikeTheField` shows; in a block body the fix is withheld (above).
- **`sdd/specs/analyzer/RH2001.md:1017`** (example: an extension method whose call the rename would turn into `Extensions._value(this)`). The example's `int` property is offered on `main` and on head; its output is `_value + this._value()`. The rewrite happens only with a delegate-typed property, as in the new withheld test.
- **`sdd/specs/analyzer/RH2001.md:1038`** (first block of the example: `Func<int, int> Add() => _value => Value + _value;`, "no code fix is offered"). This input is offered on `main` and on head; the new offered test asserts `this._value + _value`. The other two blocks (`out var`, primary constructor) were not part of this worklist.
- **`sdd/changes/2026-11/rh2001-rename-binding.md:13`**: "a parameter of an implicitly typed lambda". Same correction as spec line 969.
- **`sdd/changes/2026-11/rh2001-rename-binding.md:63`–`91`** (*Visible output changes*, the extension-method example). The `int` input's Before is not `return _value + Extensions._value(this);` but `return _value + this._value();`, and its After is that the fix stays offered. The rewrite Before holds for a delegate-typed property: `return _value() + Extensions._value(this);`.
- **`sdd/changes/2026-11/rh2001-rename-binding.md:26`**: it cites `01-skeptic.md` at `52ae88c` as the measurement. The f07 and f09/s01/s12 rows of that table do not reproduce with these inputs on `962d0c4`.

## Not covered, and why

- **Block-body variants beyond `return`:** the local declaration, assignment, and constructor (`v10`, `v11`, `v14`), the explicitly typed lambda in a block, and the query range variable. All misbind on `main` and are withheld on head (`apply-fix.sh`). I added no tests for them because they are not part of this worklist. Whether the spec names them is for the spec author, and a test is cheap to add if the lead wants one.
- **An offered counterpart for the cross-document lambda in an expression body:** the single-document offered tests cover that side of the condition, and the document boundary is covered by the withheld test.
- **Formatter and trivia classes:** unchanged from `03-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyNoFixIsOfferedWhenASimpleLambdaParameterInABlockBody|FullyQualifiedName~InferredTypeInABlockBody|FullyQualifiedName~InTheBlockBodyOfAStaticMethod|FullyQualifiedName~ALambdaParameterInAPartialPartInAnotherDocument|FullyQualifiedName~OfADelegateTypedProperty|FullyQualifiedName~InAnExpressionBodyNamedLikeTheField|FullyQualifiedName~WhoseCallKeepsItsBinding"
scripts/apply-fix.sh RH2001 <fixture-directory>   # run from an origin/main worktree for the main outputs
```
