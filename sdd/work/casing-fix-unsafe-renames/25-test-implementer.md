Role: test implementer
Commit: ca74343 (production head unchanged; tests added on top)
Verdict: tests written — 3 new tests fail on the current head, 3 new tests pass

These tests are for lead decision 19, the structural change after finding 8 of `23-auditor.md`. They cover the new SHARED-CASING-007 example "an old name of a differently spelled part in excluded code, for RH4105, in two documents" and its prose cases, as they stand at ca74343. My prompt named the cases to cover and no expected values. Every expected value comes from SHARED-CASING-007 and SHARED-FIX-003, not from the code. I used the auditor's fixtures `r4/pp`, `r4/ppr`, `r4/pidx` and `r4/ptp` only for the input shapes.

All tests use the same setup:
- `Verify(source, fixedSource, onConfigure)` on the class's `BatchCodeFixTestsBase`.
- The second part is the document `/0/Test1.cs`, added to `TestState` and `FixedState`.
- Every diagnostic in both documents is marked.

A "no fix" test passes the source as its own fixed source. The single fix applied iteratively, and Fix All in document, project and solution scope, must then leave both documents unchanged (FC-008, FC-017).

Each failing test fails at `Context: Iterative code fix application` with `content of '/0/Test0.cs' did not match`: a fix is offered and renames both parts. That is the observed-versus-expected difference of finding 8. No test fails at compile time, in a helper, or on the diagnostics. The partial indexer compiles at the test's default language version.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesItsDifferentlySpelledParameter` (the example verbatim: `Arg` reported, `other++` excluded in the second part) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: `Run(int Arg)` → `Run(int arg)`, and the second part is renamed too |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheDefiningPartOfAPartialMethodNamesItsDifferentlySpelledParameter` (the reverse direction: `Other` reported in the second part, `#if NEVER [System.Obsolete(nameof(Arg))]` on the first part, which is reported as well) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: the fix for `Arg` is withheld, but the fix for `Other` is offered and turns `Run(int Arg)` into `Run(int other)` |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialIndexerNamesItsDifferentlySpelledParameter` (`this[int Index]` / `this[int position]`, `return position;` excluded) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: `this[int Index]` → `this[int index]`, and the second part is renamed too |
| `RH4105…NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName` (the prose "names `Arg`, `other`, or `arg`", new-name arm: `int arg = other;` excluded in the second part) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | passes |
| `RH4105…FixRenamesTheDifferentlySpelledParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt` (the offered case: the second part has the active `other++` and an excluded branch that names none of the names) | SHARED-CASING-007 | `Verify`, two documents | passes: both parameters and the active reference become `arg`, and the excluded branch is unchanged |
| `RH4121…FixIsOfferedWhenAnExcludedBranchNamesTheDifferentlySpelledTypeParameterOfTheOtherPartOfAPartialMethod` (`Run<Key>` / `Run<TItem>`, `TItem copy = value;` excluded) | SHARED-CASING-007 | `Verify`, two documents | passes: only the first part becomes `Run<TKey>(TKey value)`, and the second part is unchanged |

Test classes: `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs` and `src/Reihitsu.Analyzer.Test/Naming/RH4121TypeParameterNameCasingAnalyzerTests.cs`.

The RH4121 test is the guard against over-withholding. If old names are taken from every definition the reference search returns, and that search returns the other part's `TItem`, this test turns red.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Partial-method parameter, the other part's spelling named in its excluded code | LF | code fix, single + Fix All | reproduced | both parts renamed to `arg` vs. no fix |
| Reverse direction, `nameof(Arg)` excluded on the first part, `Other` reported | LF | code fix, single + Fix All | reproduced | both parts renamed to `other` vs. no fix for `Other` |
| Partial-indexer parameter, `position` named in excluded code of the other part | LF | code fix, single + Fix All | reproduced | both parts renamed to `index` vs. no fix |
| New name `arg` in excluded code of the other part | LF | code fix, single + Fix All | holds | no fix, as specified |
| No excluded branch names `Arg`, `other` or `arg` | LF | code fix, single + Fix All | holds | both parts and the reference renamed, as specified |
| Differently spelled type parameter, `TItem` in excluded code | LF | code fix, single + Fix All | holds | only the first part renamed, as specified |

## Not covered, and why
- **CRLF.** This is a rename-only fix with no layout. `11-test-implementer.md` already holds a CRLF variant of the excluded-text scan of another document. A differently spelled cascade member changes which names are searched, not how disabled text is read.
- **Partial constructor parameter.** The spec does not name it (`24-spec-author.md`, follow-up candidates). The auditor's harness reported no RH4105 diagnostic for it.
- **"A symbol whose name source never writes adds no old name"** (a property's accessor, a type's constructor). The spec has no example for it. The existing tests of `11-` and `19-` (property, override and type renames with excluded code) pin the offered outputs that over-withholding through such a symbol would break.
- **Formatter counterpart.** The formatter does not rename.
- **Failure classes.**
  - FC-001, FC-016, FC-020 and FC-024 (the guard's region and names against the renamer's reach) are the subject of these tests.
  - FC-002 / FC-003: the old-name condition is tested on both sides. A differently spelled cascade member withholds (parameter, indexer, both directions). A differently spelled symbol outside the cascade does not (the RH4121 type parameter). The new-name arm and the offered "names nothing" arm are tested too.
  - FC-008: covered by `Verify`'s Fix All scopes.
  - FC-017: every withheld test asserts both documents unchanged. Every offered test asserts the full text of both documents.
  - The auditor's dismissals of the other classes in `23-auditor.md` hold unchanged.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4121"
```
