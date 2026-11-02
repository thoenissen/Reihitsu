Role: implementer
Commit: (this commit; see `git log`)
Verdict: tests pass

Repair round 1, for lead decisions 10 and 11 (auditor finding #1 and the architecture finding). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions; I worked from 00-lead.md, 12, 13, 14 and FMT-INDENT-031. On fb8a999 the 13 tests 14-test-implementer.md lists fail as it says (13 failed, 27 passed in `AnonymousMethodHoleAlignmentTests`); all 40 pass now, with no test changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | New `TryGetRawStringHole(lineNumber, out hole)`: returns the hole a line is registered with by `RegisterRawStringHoleLine`, i.e. the innermost multi-line raw hole around the token that starts the line. | FMT-INDENT-031, FMT-RAW-007 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` | `IsAlignedInRawStringHole` takes the model and no longer walks to the innermost hole of any interpolated string. It reads the hole registered for the opening brace's line from the model, requires that hole to hold the anonymous method, and walks the ancestors only to check that no `BlockSyntax` lies between the anonymous method and that hole. Class summary and method summary updated. | FMT-INDENT-031 |

**Decision 11.** "Which multi-line raw hole owns this line" is now stated once, in `LayoutComputer.RegisterRawStringHoleLines` (innermost multi-line raw hole, by registering outer holes first). `LayoutModel.AcceptsWrite`/`RecordPlacement` and the FMT-INDENT-031 guard all read that registration; the guard no longer re-derives it from syntax. The block exception ("no block body between the anonymous method and the hole") remains in the contributor because it is the contributor's own decision; it now stops at the registered hole, so a nested regular, verbatim, or single-line raw string between body and anonymous method no longer ends the walk early, as the changed spec's exception paragraph says.

The `hole.Span.Contains(anonymousMethod.Span)` check is a guard for a brace that does not start its line (then the registered hole belongs to the line's first token, not to the brace). FMT-BREAK-002 makes that unreachable per the auditor; the check keeps the guard from acting on another construct's hole if it ever occurs.

## Policy owners

- `rg -n "InterpolatedMultiLineRawStringStartToken|IsMultiLineRawStringLiteral\(|TryGetRawStringHole|RegisterRawStringHoleLine\b" src/Reihitsu.Formatter`
  - `LayoutComputer.RegisterRawStringHoleLines` (line 355): the one statement of which hole owns a line.
  - `LayoutModel.RegisterRawStringHoleLine` / `TryGetRawStringHole`: storage and the new read.
  - `LambdaAlignmentContributor.IsAlignedInRawStringHole`: now reads the model; no own statement.
  - `RawStringLineShifts.IsMultiLineRawStringLiteral` (line 109) and its uses at 52 and 164, and `IsNonRawStringText` (line 548): the literal-kind predicate for the literal moves, not the hole-ownership decision. `RawStringLineShifts.IsNestedInRawStringHole` (line 473) decides whether a literal lies in any multi-line raw hole, for composing shifts; a different decision, unchanged.

## Tests run

All Debug, on the working tree of this commit.

```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~AnonymousMethodHoleAlignmentTests"
  before: Failed: 13, Passed: 27, Total: 40
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~AnonymousMethodHoleAlignmentTests|FullyQualifiedName~RawStringInterpolationHoleTests|FullyQualifiedName~ParenthesizedLambdaLineStartTests"
  after:  Failed: 0, Passed: 98, Total: 98
scripts/test.sh --configuration Debug --project formatter   Failed: 0, Passed: 4163, Total: 4163
scripts/test.sh --configuration Debug --project analyzer    Failed: 0, Passed: 6397, Total: 6397
scripts/build.sh --configuration Debug                      0 Warning(s), 0 Error(s)
```

`scripts/format.sh` over the two changed files changed nothing.

## Disputed tests or scope

_None._ One new public member on an internal type (`LayoutModel.TryGetRawStringHole`), which decision 11 asks for; no new dependency, two production files.

## Architecture findings

_None_ beyond the one decision 11 repairs, which this change resolves.

## Follow-up candidates

_None._
