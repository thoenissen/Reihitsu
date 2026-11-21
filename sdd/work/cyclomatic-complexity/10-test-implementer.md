Role: test implementer
Commit: 31c7360
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. Expected values come from RH2009-003 as changed in 09-spec-author.md (maintainer answer 26: recursion is an invocation that binds to the member's own symbol, on any receiver) and from its new `Node.Depth` example; not from the implementation (FC-035).

## Tests

All in `src/Reihitsu.Analyzer.Test/Design/RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests.cs`, on `Verify` through the local helpers `VerifyComplexity`, `Complexity`, `Limit`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RecursionOnAnotherInstanceExample (new) — the spec's `Node.Depth` example, limit 3, reported with 4 | RH2009-003 | Verify | passes |
| SameNamedMemberOfAnotherTypeIsNotRecursion (new) — same body, `Left`/`Right` typed `Leaf` whose `Depth()` has the same name and signature; 3, not reported at limit 3 (the other side of the example, FC-003, FC-016) | RH2009-003 | Verify | passes |
| OnlyAnInvocationOfTheMemberItselfIsRecursion, new row `new C().M(n - 1);` → 4 | RH2009-003 | Verify | passes |
| OnlyAnInvocationOfTheMemberItselfIsRecursion, new row `new C().M(n - 1); M(n - 2);` → 4 (call on another instance and a direct call together, 1 point in total) | RH2009-003 | Verify | passes |
| OnlyAnInvocationOfTheMemberItselfIsRecursion, new row `new C().M("text");` → 3 (another overload on another instance) | RH2009-003 | Verify | passes |

The summary of `OnlyAnInvocationOfTheMemberItselfIsRecursion` was updated to name the new rows. No existing assertion changed: `rg "Recursion|\.M\(|Depth" src/Reihitsu.Analyzer.Test` found no test asserting that a call on another instance scores nothing.

The new tests pass on the current head because the implementation (08) already binds by symbol with no receiver check, which 09 records as the specified behavior. They did not fail first; the failure reason is shown instead by the pair: the two `Depth` bodies are token-identical except for the receiver type, and differ by exactly the recursion point (4 reported vs 3 not reported at limit 3).

Run: RH2009 class — Passed 179, Failed 0.

## Scenarios (bug fix)

_Not a bug fix._

## Not covered, and why

- Line endings: the analyzer counts syntax and binding, no layout; not applicable.
- A self-call on a base-typed or interface-typed receiver that dispatches to this member at run time binds to another symbol and so is not recursion by the spec's wording; no example asks for it and it is the same "other symbol" arm as `SameNamedMemberOfAnotherTypeIsNotRecursion` (one `SymbolEqualityComparer.Default.Equals(Normalize(target), _member)` in `src/Reihitsu.Analyzer/Core/CognitiveComplexityWalker.cs:607`, found with `rg SymbolEqualityComparer src/Reihitsu.Analyzer/Core/CognitiveComplexityWalker.cs`), so one test covers it.

## Commands

scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests"

## Follow-up candidates

_None._
