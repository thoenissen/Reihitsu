Role: spec author
Commit: 99030b7
Verdict: spec written

Repair round for the test implementer's finding in `04-test-implementer.md`. The prompt carried no intended fix or verdict.

## Changed requirements

- RH5419-001 — A wrapped indexer is reported — example repaired. The unreported line `int this[uint a, uint b] => a;` did not compile: CS0266, because there is no implicit conversion from `uint` to `int`. It now reads `int this[uint a, uint b] => 1;`. The layout and the reported spans are unchanged, and the example is now equal to the tests that use it.

Other examples checked for the same defect:
- Every other indexer in `sdd/specs/analyzer/RH5419.md` and in `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md` returns `1`, `i` or `a` of `int`, `short`, `byte`, `sbyte` or `char` (each converts implicitly to `int`), a `char + char` sum, or `_values[index]`.
- `long`, `ulong` and `uint` parameters already return `1` everywhere else.
- The indexer signatures within each example are distinct, also under both branches of the `#if DEBUG` example.
- No other example has the defect. The test implementer ran every example through the harness with a throwaway stub, and only this line failed.

## Examples for the tests

RH5419-001. The tests already use `=> 1;`, so no test changes.

## Output questions

_None._

## Internal choices

- I kept the suggested `=> 1;` and did not change the parameter types: it matches the tests as written. The alternative was to make the parameters `int`, which would duplicate the `int this[int i]` signature's parameter type pattern and change the tests.

## Follow-up candidates

_None._
