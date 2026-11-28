Role: auditor
Commit: 84d1ee7d4602fd110b1615d02dcbdc164f0c21c9
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `7847c6ca90171381a6ac9aab9f31406dbbf2fcd4`. Head `84d1ee7d4602fd110b1615d02dcbdc164f0c21c9` on `claude/determined-mccarthy-dpombd`, checked out, clean tree. `origin/main` = `707d4b805f6da94910a921acdfa5c0f281bd1f42` (matches `git ls-remote`), an ancestor of the head. The repair diff matches `06`–`09`: `src/Reihitsu.Tooling/FixtureRunner.cs` (`GetCompileErrorsAsync` returns per-ID distinct-message counts, `RunAsync` compares counts), `Enumerations/FixtureOutcome.cs` (doc), `scripts/README.md` (`invalid-result` row), `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs` (five tests, one helper, summaries), `FixtureRunnerTests.cs` (summary). `scripts/lane-check.sh tooling`: confirmed. Scope did not grow. Evidence from `05-auditor.md` on unchanged code (project setup, parse check, severity filter, policy owners) is reused.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every member it listed now holds: RH4002 single type (`class sample` with CS0161), RH4103 (`compute`), RH4105 (`out int Value`) — covered by the three new rename tests; the RH4105 single-parameter member re-run through `scripts/apply-fix.sh`: `fixed`, exit 0 under LF and CRLF. A member it did not name still fails: see #3 |
| 2 | closed | `FixtureRunnerTests.cs:95` summary now says only that it is the fixture's parse check |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | high | `src/Reihitsu.Tooling/FixtureRunner.cs:131-137` (the per-iteration check), `:187-194` (key: distinct message text per ID); `scripts/README.md:87` | FC-044 | #1 | PR-introduced | A rename fix that renames one of several distinct symbols sharing one display name, one per iteration, turns the fixture's single message of an error ID into two distinct messages after the first iteration, so the run stops with `invalid-result` and exits 2, though the converged result would carry no new error. The check runs after every iteration and the runner applies one action per iteration (`RunIterationAsync`, `diagnostics[0]`). Run on this head through `scripts/apply-fix.sh`, all `invalid-result (1 iteration(s))`, exit 2, under LF and CRLF: **RH4105** on `internal class Sample { internal void First(out int Value) { } internal void Second(out int Value) { } }` (CS0177 `The out parameter 'Value' ...` twice; after iteration 1 `'value'` and `'Value'`); **RH4115** on two methods each `int Value; return Value;` (CS0165); **RH4104** on two methods each with a local function `int compute() { }` (CS0161); **RH4002** on `namespace First { internal class sample { internal int M() { } } }` plus the same in `namespace Second` (CS0161; the message shows the type without its namespace). Holds: the same fixtures with a single symbol (RH4105 `p1`, RH4115 `l1`: `fixed`, exit 0). Class axes: symbol kind whose display omits what tells the symbols apart (parameter, local, local function, type in another namespace; members of distinct types differ by the containing type and hold) × number of such symbols (≥ 2) × the fix applied one occurrence per iteration. `scripts/README.md` states that an existing error does not count "when a rename changes the name its message shows", which these members contradict. No test covers the member | Repeat of #1 (the same decision — whether an existing error is new after a rename — failing between the runner's message key and the Renamer), so the lead handles it under *Architecture findings*. Refuse by the cause: stop keying on message text, which the Renamer rewrites, e.g. compare error IDs only (an ID the fixture lacks is new). This drops `RunAsyncReportsInvalidResultWhenTheFixAddsAnErrorWithTheSameIdForAnotherName` and `...RenamesATypeAndAddsAnErrorWithTheSameId` and revises decision 2; the README row follows. Add a test for the split member |
| 4 | low | `src/Reihitsu.Tooling/FixtureRunner.cs:67-69` (`RunAsync` `languageVersion` doc) | FC-031 | – | PR-introduced | Says the fix-result check is "whether a fix result compiles without an error the fixture did not have". Under decision 2 a result whose CS0246 for `Missing` became CS0246 for `Other` is `fixed` although it has an error the fixture did not have | Restate as "the compile check of each fix result" or match whatever key #3 settles on. Comment only |

#4 alone would be `pass, text cleanup` (an XML parameter doc, no compiled behavior); #3 is behavioral, so the verdict is `findings`.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix result is `InvalidResult` (`resultErrors.Any(e => e.Value > originalErrors.GetValueOrDefault(e.Key))`) | Error-severity diagnostics of the result's one-document compilation, grouped by ID (ordinal), counted by distinct invariant-culture message (ordinal) | `originalErrors`: same function over the normalized fixture under `fixturePath` | Same project setup and comparers on both sides; a missing ID counts from zero, as the docs say. The key still holds message text the Renamer rewrites, and the result side is an intermediate state of a multi-iteration rename: see #3 | finding #3 |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Validity of a fix result | `rg -n "GetDiagnostics\(|HasSyntaxErrors|GetCompilationAsync|InvalidResult|IsSubsetOf" src/Reihitsu.Tooling`: `FixtureRunner.RunAsync` with `GetCompileErrorsAsync` only; `CodeFixRunCommand` maps the outcome | one owner |
| Statement of the comparison key | `FixtureRunner.cs:131-134` comment, `GetCompileErrorsAsync` summary, `FixtureOutcome.InvalidResult` summary, `scripts/README.md:87`, test class summary | consistent with each other; the README's rename claim is wrong for #3's members, and the `RunAsync` param doc is imprecise (#4) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RunAsyncReportsFixedWhenTheFixRenamesTheContainingTypeOfAMemberAnExistingErrorNames` | A type rename that changes an existing error's message is `fixed` | The (ID, message) set of decision 1 reports `InvalidResult`; also pins `FinalSource` | `RunReplacingFixAsync` (stub analyzer + replacing stub fix, LF, `Latest`) | adequate |
| `...RenamesTheMemberAnExistingErrorNames` | Same, member's own name | same | same | adequate |
| `...RenamesTheParameterAnExistingErrorNames` | Same, parameter name | same | same | adequate |
| `RunAsyncReportsInvalidResultWhenTheFixRenamesATypeAndAddsAnErrorWithTheSameId` | A rename that also adds a same-ID error is invalid | An ID-only set reports `Fixed` | same | adequate for decision 2 (dropped if #3 moves to ID-only) |
| `RunAsyncReportsInvalidResultWhenTheFixReplacesAnExistingErrorWithOneOfAnotherId` | Counts are per ID, not in total | A total distinct-error count reports `Fixed` | same | adequate |
| missing | A rename of one of several same-named symbols, one per iteration, is `fixed` | — | — | missing; part of #3 |

## Failure classes dismissed
- FC-001, FC-002, FC-003: one guard changed; its two sides use one function (table above). The qualifiers (per ID, from zero, distinct) are each tested on both sides.
- FC-004, FC-005, FC-019 to FC-021, FC-006, FC-022 to FC-029: the runner rewrites no source; it classifies.
- FC-007 to FC-014, FC-018, FC-030, FC-038 to FC-043: no analyzer, code fix, formatter phase, or unsupported shape changes.
- FC-015, FC-016: one owner (above).
- FC-017: table above.
- FC-031: the changed summaries and the inline comment describe the new key; the remaining mismatch is #4.
- FC-032 to FC-035, FC-037: tests run the changed runner through `RunAsync`, expected values are hand-written; compiler messages carry no line ending or position, so LF-only rename tests suffice (confirmed by the LF and CRLF arms of #3's runs agreeing); no text-only claim.
- FC-036: no shared result reused across arms.
- FC-044: reached; finding #3.

## Architecture findings
Finding #3 repeats the class of #1: the decision "is this compile error new" is made by `FixtureRunner` on message text, while the Renamer (through every RH4xxx rename fix) rewrites the names that text shows. Round 1 refined the key (set of messages → distinct-message count per ID) and the next rename shape (several same-named symbols renamed one per iteration) fails it; any key built on message text has to model what the Renamer does to it (FC-044). Places: `src/Reihitsu.Tooling/FixtureRunner.cs:187-194` (key), `:137` (comparison), inputs in #3. Refusing by the cause means a key that holds no symbol name, such as the error ID alone.

## Hints
- Under decision 2 a fix that replaces an existing error with another of the same ID for another name (CS0246 `Missing` → CS0246 `Other`) is `fixed`; accepted by decision 2, untested, and also the behavior of an ID-only key.
- Unchanged from `05`: the extra compilation per iteration, and `HasSyntaxErrors` staying `public`.

## Follow-up candidates
_None._
