Role: spec author
Commit: 29d3add
Verdict: spec written

Correction round (decision 12; `05-test-implementer.md`, *Spec and change-doc text that no longer matches*). The prompt carried no intended fix or hoped-for verdict. No visible output is decided here: the corrections bring the text to `main`'s measured output (962d0c4) and the tests at 29d3add.

## Changed requirements

- RH2001-011 — The fix is withheld when the field name is already bound at a reference — changed (text only, within this run):
  - Paragraph 2 no longer names "a parameter of an implicitly typed lambda". Its examples are now a lambda parameter or a query range variable in a statement of a block body, an `out` variable in the same expression, and a primary-constructor parameter. It states that the renamer's result for the reference decides, not the symbol's kind, and that the same lambda in an expression body keeps the fix.
  - Paragraph 3 replaces "a parameter of an explicitly typed lambda" with "a parameter of a lambda in an expression body". It adds qualification with the type's name for a static property, and an extension method whose call is still reached, written as before.
  - The extension-method withheld example now uses a delegate-typed property (`Func<int> Value`, `Value() + this._value()`), as in `VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheFieldOfADelegateTypedProperty`.
  - The first block of the lambda/out/primary-constructor example is now block-bodied (`return _value => Value + _value;`), as in `VerifyNoFixIsOfferedWhenASimpleLambdaParameterInABlockBodyIsNamedLikeTheField`.
  - Two offered examples are added at the end:
    - the expression-bodied lambda, giving `this._value + _value`;
    - the `int` extension case, giving `_value + this._value()`.

    They match `VerifyFixQualifiesThePropertyReferenceNextToASimpleLambdaParameterInAnExpressionBodyNamedLikeTheField` and `VerifyFixIsOfferedNextToAnExtensionMethodNamedLikeTheFieldWhoseCallKeepsItsBinding`.

Change doc `sdd/changes/2026-11/rh2001-rename-binding.md`:

- *What*: the paragraph-2 list follows the spec, the rewrite list marks `Extensions._value(this)` as a delegate-typed property, and the offered list adds lambdas in an expression body and extension calls still reached.
- *Visible output changes*:
  - The measurement line now cites `main` 962d0c4 (05) and keeps 52ae88c (01) for the rows first measured there.
  - The extension example's input and Before are now the delegate-typed property (`return _value() + Extensions._value(this);`).
  - One line states that the `int` variant stays offered, unchanged from `main`.
- *Alternatives considered*: adds the rejected syntactic refusal (every inferred-type lambda parameter, or every extension method, named like the field).

## Examples for the tests

- RH2001-011: the new withheld block-body lambda example, the delegate-typed extension example, and the two new offered examples. All four already have tests at 29d3add.
- The test implementer is adding siblings in parallel (decision 11). Paragraph 2 names the block-body local declaration, assignment, constructor, and query range variable shapes only as "a statement of a block body" and "a query range variable", and they derive from that.

## Output questions

_None._ Every corrected line now states `main`'s output or answer B.

## Internal choices

- Paragraph 2 states the condition by the renamer's result ("leaves it unqualified … would bind to a symbol other than the field"), with block-body examples. The alternative was a syntactic condition ("in a block body"), which would claim more than was measured, for example for an explicitly typed lambda in an expression body. The tests and the implementer's binding comparison both decide on the result.

## Follow-up candidates

_None._

Staged only `sdd/specs/analyzer/RH2001.md`, `sdd/changes/2026-11/rh2001-rename-binding.md`, and this handover. The test file the parallel test implementer is changing is left unstaged.
