Role: test implementer
Commit: d0ac1b6 (tests written against 60dc70a)
Verdict: tests written

Repair round 1, lead decision 14, amended RH3004-010 (09-spec-author.md, ac343a0). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision.

All changes are in `tests/Reihitsu.Analyzer.Test/Clarity/RH3004UseLambdaSyntaxAnalyzerTests.cs`. No production code or test helper changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ExpressionBodyInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` (spec cell 2; replaces `ExpressionBodyInsideANonVerbatimInterpolationIsFixedBeforeCSharp11`, which asserted the fix the amendment withholds) | RH3004-010 | `VerifyNoCodeFixIsOffered(source, CSharp10)` | Fails now: a fix is offered ("Expected collection to be empty") |
| `ExpressionBodyWithAnObjectInitializerInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11` (spec cell 3, `new P { A = p }.A`) | RH3004-010 | same | Fails now: a fix is offered |
| `ExpressionBodyInsideANonVerbatimInterpolationBehindAVerbatimOneOffersNoFixBeforeCSharp11` (`$"x {$@"a {G(delegate (int p) { return p; })} b"} y"`) | RH3004-010 | same | Fails now: a fix is offered |
| `FixAllLeavesTheExpressionBodyInsideANonVerbatimInterpolationBeforeCSharp11` (hole method stays reported, `N` becomes `(int p) => p`) | RH3004-010 | `Verify(source, fixed, configure)` with C# 10 transform and `MarkupHandling.Allow` on fixed and batch states | Fails now: "Iterative code fix application", the hole is rewritten to `$"a {G((int p) => p)} b"` |
| `ExpressionBodyInsideANonVerbatimInterpolationIsFixedFromCSharp11` (spec cell 4) | RH3004-010 | `Verify`, C# 11 | Passes |
| `ExpressionBodyInsideAVerbatimInterpolationIsFixedBeforeCSharp11` (spec cell 5, `$@"`) | RH3004-010 | `Verify`, C# 10 | Passes |

Unchanged and still passing: spec cell 1 (`BlockBodyInsideANonVerbatimInterpolationOffersNoFixBeforeCSharp11`), the block-path nested, C# 11, `$@"`, contains-a-string, and Fix All tests. RH3004 class run on 60dc70a + tests: 53 passed, 4 failed (the four above, each for the reason stated).

## Scenarios (bug fix)
Not re-run; the reproductions are in `03-test-implementer.md`. The auditor's finding #1 is reproduced by the three no-fix tests and the expression-body Fix All test above.

## Not covered, and why
- **Each brace construct of the auditor's failing list** (`new { A = p }.A`, collection initializer, `with`, switch expression, nested lambda/anonymous method with a block). RH3004-010 decides by position alone; the cause the amended guard checks is shared by every returned expression, so one brace cell (spec cell 3) and one brace-free cell (cell 2) cover both sides of what the requirement now says does not matter. A test per construct would pin the formatter's layout, which the requirement deliberately does not predict (FC-044).
- **CRLF.** The new C# 10 tests assert "no action"; the fixed cells are one-line outputs, and the block-path CRLF layout is covered by existing RH3004 tests.
- **Other existing tests pinning the changed output.** `rg 'p\) => p\)\} b'` over `tests`, `src`, `sdd` finds only the two new passing tests and the spec examples; no other test asserted the withdrawn expression-path fix.
- Failure classes: as in `03-test-implementer.md`; FC-028/FC-044 are the subject of this round and covered above.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests"
scripts/test.sh --project architecture
```

## Follow-up candidates
_None_ beyond the lead's existing `apply-fix.sh` candidate.
