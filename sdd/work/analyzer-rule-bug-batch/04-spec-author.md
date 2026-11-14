Role: spec author
Commit: e2d65ed
Verdict: spec written

Repair round on the two findings in `03-test-implementer.md` ("Findings for the lead"). No requirement text changed.

## Changed requirements

- RH5602-001 — Whitespace before a line break is reported — example corrected: the verbatim literal field `w` is renamed `p` and the raw literal field `x` is renamed `q`, so the example no longer declares `w` and `x` twice (CS0102). The reported spans and what each line demonstrates are unchanged; the sentence below the example names `string p = @"a··`.
- RH5404 — `Uses` gains SHARED-FIX-001, which the RH5404-003 `Why:` references since 6ab9935. `UsesListsExactlyTheReferencedRequirements` passes.

## Examples for the tests

RH5602-001: the test `VerifyEveryRunOfTheRequirementExampleIsReported` copies the old example verbatim with `CompilerDiagnostics.None`. The test implementer can now copy the corrected example (`string p`, `string q`) and drop that setting and its comment.

## Output questions

_None._

## Internal choices

- Renamed the later string fields rather than the earlier `int x;`/`int w;` lines, so the lines the other requirements and the RH5602-004 example share stay as they are (alternative: rename the `int` fields).

## Follow-up candidates

_None._

## Commands

```shell
scripts/test.sh --project architecture --filter "FullyQualifiedName~UsesListsExactlyTheReferencedRequirements"   # passed
```
