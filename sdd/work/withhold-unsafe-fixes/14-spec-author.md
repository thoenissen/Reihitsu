Role: spec author
Commit: d428302
Verdict: spec written

Repair round 1, worklist: lead decision 9 only. The prompt carried no intended fix or hoped-for verdict beyond decision 9 itself.

## Changed requirements

- SHARED-ORDER-007 — Initializer execution order is kept — clarified (example only). The second new example (the `#if DEBUG` pair in front of the moved `_b`, fix offered) now declares `Log` with a block body (`{ return 0; }`) in both the input and the fixed output. Before this change it kept `private static int Log(string name) => 0;` unchanged in the output. The RH7109 and RH7103 fixes do not produce that output, because they format the whole type and rewrite the expression body (FC-030, thoenissen/Reihitsu#969, which stays out of this run). The guard's verdict does not change: the fix is offered, and the excluded code moves with `_b`. The normative text and the first example are unchanged. The first example keeps `=> 0` because it offers no fix, so it shows no output. Its test, `NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode`, uses `=> 0` as well.

No new IDs. `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. `scripts/test.sh --project architecture --filter Sdd` passes (51/51).

## Examples for the tests

- SHARED-ORDER-007, second new example: `RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests.FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt` now matches the example line for line, in both input and output. No test change is needed.

## Output questions

_None._ The guard's verdict is unchanged. Only the incidental shape of an unrelated member changed, to an output the fix produces today.

## Internal choices

- I changed only the second example. Alternative: also change the first, the negative example, for symmetry. I did not, because it shows no fix output, and its test mirrors the `=> 0` form already.

## Follow-up candidates

_None_ new. Whole-type formatting by the RH7103/RH7109 fixes stays with thoenissen/Reihitsu#969.

## Architecture findings

_None._
