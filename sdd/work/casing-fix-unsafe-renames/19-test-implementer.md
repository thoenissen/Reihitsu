Role: test implementer
Commit: 03f33f2 (production head unchanged; tests added on top)
Verdict: tests written — 3 new tests fail on the current head, 1 new test passes

This run is for lead decision 17, the architecture finding, which repeats the class of audit finding 2. I wrote tests for the new SHARED-CASING-007 example added in 03f33f2: a name in excluded code reached through the other part of a partial method, for RH4121. The tests also cover the example's prose cases. My prompt named the change asked for and no expected values. Every expected value comes from SHARED-CASING-007 and SHARED-FIX-003 as they stand at 03f33f2. None comes from the code or from the auditor's suggested remedy. I used the auditor's scratch fixtures `r1/pm` and `r1/pmr2` only for the input shapes.

All tests use the same setup:
- Every test is in `src/Reihitsu.Analyzer.Test/Naming/RH4121TypeParameterNameCasingAnalyzerTests.cs`.
- Every test uses `Verify(source, fixedSource, onConfigure)` on the class's `BatchCodeFixTestsBase`.
- The second document `/0/Test1.cs` is added to `TestState` and `FixedState`, and every diagnostic in both documents is marked.

The "no fix" tests pass the source as its own fixed source. So the iterative single fix, and Fix All in document, project and solution scope, must leave both documents unchanged. This covers the Fix All that decision 17 asks for (FC-008).

Each failing test fails at `Context: Iterative code fix application`, with `content of '/0/Test0.cs' did not match`. The observed diff is `partial void Run<Key>(Key value);` → `partial void Run<TKey>(TKey value);`: a fix is offered and renames both parts. This is the observed-versus-expected difference of the auditor's finding 6. No test fails at compile time, in a helper, or on the diagnostics.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodReferencesTheTypeParameter` (the spec example verbatim: definition → implementation; `Key copy = value;` excluded in the implementing part) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: the fix on the defining part's `Key` renames both parts |
| `NoFixIsOfferedWhenAnExcludedBranchOfTheDefiningPartOfAPartialMethodReferencesTheTypeParameter` (the prose reverse direction: `#if NEVER [System.Obsolete(nameof(Key))] #endif` on the declaration without a body; the implementing part has no excluded code) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: the fix on the implementing part's `Key` renames the defining part |
| `NoFixIsOfferedWhenAnExcludedBranchOfTheImplementingPartOfAPartialMethodNamesTheNewName` (the prose condition "names `Key` or `TKey`", new-name arm: `TKey copy = default;` excluded in the implementing part) | SHARED-CASING-007, SHARED-FIX-003 | `Verify`, two documents | fails: renames both parts |
| `FixRenamesTheTypeParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt` (the prose offered side: an excluded branch is present in the implementing part but names neither `Key` nor `TKey`) | SHARED-CASING-007 | `Verify`, two documents | passes: both parts and `value`'s type become `TKey`, and the excluded branch is unchanged |

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Partial-method type parameter, old name referenced in excluded code of the implementing part | LF | code fix, single + Fix All | reproduced | both parts renamed to `TKey`, excluded `Key` left vs. no fix |
| The same, with `nameof(Key)` in excluded code on the defining part | LF | code fix, single + Fix All | reproduced | both parts renamed vs. no fix |
| The same, with the new name `TKey` declared in excluded code of the implementing part | LF | code fix, single + Fix All | reproduced | both parts renamed vs. no fix |
| The same, with an excluded branch that names neither name | LF | code fix, single + Fix All | holds | both parts renamed, as specified |

## Not covered, and why
- **CRLF.** This is a rename-only fix with no layout. `11-test-implementer.md` already holds a CRLF variant of the excluded-text scan of another document. The partial-method relation does not change how disabled text is read, so there is no new line-ending case.
- **A partial-method parameter, a partial method itself, a record positional parameter, and the other relations the auditor marked "holds".** The auditor ran each one and found it holds, and the spec author added no example for them (`18-spec-author.md`, follow-up candidates). These tests are not derived from the new example. The existing override, implementation and cross-project tests from `11-` still pin those relations. They are the regression guard when the cascade is read from Roslyn instead of being modeled.
- **The SHARED-CASING-009 rewording.** No input or output changed (`18-spec-author.md`), and the existing 009 tests cover it unchanged.
- **Formatter counterpart.** The formatter does not rename.
- **Failure classes.**
  - FC-001, FC-016, FC-020 and FC-024 (the guard's region against the renamer's reach) are the subject of these tests.
  - FC-003: the excluded-code condition is tested on both sides: old name named, new name named, and neither named. It is also tested in both directions of the partial relation.
  - FC-008: covered by `Verify`'s Fix All scopes.
  - FC-017: every withheld test asserts that both documents are unchanged. The offered test asserts the full text of both documents.
  - The auditor's dismissals of the other classes in `17-auditor.md` hold unchanged.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4121"
```
