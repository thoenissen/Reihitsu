Role: auditor
Commit: e3a65bbb1eb79f0f39da5f574f0610129d2cc07f
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Delta audit of the architecture change of `00-lead.md` decision 3, against the previously audited head `84d1ee7d4602fd110b1615d02dcbdc164f0c21c9`. Head `e3a65bbb1eb79f0f39da5f574f0610129d2cc07f` on `claude/determined-mccarthy-dpombd`, checked out, clean tree. `origin/main` = `707d4b805f6da94910a921acdfa5c0f281bd1f42` (matches `git ls-remote origin refs/heads/main`), an ancestor of the head. The delta matches `11`–`14`: `src/Reihitsu.Tooling/FixtureRunner.cs` (`GetCompileErrorsAsync` returns an ordinal `ImmutableHashSet<string>` of error IDs; `RunAsync` reports `InvalidResult` when `resultErrors.IsSubsetOf(originalErrors) == false`; comment and docs; unused `System.Globalization` using removed), `Enumerations/FixtureOutcome.cs` (summary), `scripts/README.md` (`invalid-result` row), `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs` (two tests dropped, one added, three summaries), and `sdd/work/`. `scripts/lane-check.sh tooling`: confirmed. Scope did not grow. Evidence from `05` and `10` on unchanged code (project setup in `CreateDocument`, parse check, severity filter, iteration loop) is reused.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | Closed in `10`; the ID-only key keeps every member it listed `fixed` (the three single-symbol rename tests still assert `Fixed`; an existing ID never counts as new) |
| 2 | closed | Closed in `10`; unchanged since |
| 3 | closed | No message text is read any more. Every member it listed re-run on this head through `scripts/apply-fix.sh`, LF and CRLF: RH4002 (`sample` in two namespaces, CS0161), RH4115 (two `int Value;` locals, CS0165), RH4104 (two local functions `compute`, CS0161): `2 fixed, 0 invalid-result` each; RH4105 (two `out int Value`) run by `12` (`0 invalid-result`) and covered by the new test `RunAsyncReportsFixedWhenTheFixRenamesOneOfSeveralSameNamedSymbolsPerIteration`, 2 iterations. `scripts/README.md:88` now states the ID-only rule, which these members agree with |
| 4 | closed | `FixtureRunner.cs:65-68`: "the compile check of each fix result", true under any key |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix result is `InvalidResult` (`FixtureRunner.cs:135`) | IDs of every error-severity diagnostic of the result's one-document compilation (parse and binding), under `state.DocumentPath` and the run's `languageVersion`, after every iteration | `originalErrors` (`:86`): the same function over the normalized fixture under `fixturePath` | Same function, same comparer (ordinal) on both sides; the document path a rename fix changes enters no diagnostic ID. The key now holds nothing the Renamer or any fix rewrites, so the intermediate states of a multi-iteration rename compare like the final one. "An ID the fixture lacks counts" is the `IsSubsetOf` direction (result ⊆ original), as the docs say | holds |
| Fixture is `ParseError` (`HasSyntaxErrors`, unchanged) | Syntax-tree errors under `new CSharpParseOptions(languageVersion)` | Parse options of `CreateDocument` | Same options, so the fixture's ID set holds no parse error of the tree, and the comment "every parse error of the result counts as new" holds for parser-only IDs. Probed with Roslyn 5.12.0 (the repository's version): language-version feature errors under C# 10 are compilation diagnostics, not tree diagnostics, so they never make a fixture `parse-error` | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Validity of a fix result in the fixture runner | `rg -n "GetDiagnostics\(|HasSyntaxErrors|GetCompilationAsync|InvalidResult|IsSubsetOf" src/Reihitsu.Tooling`: `FixtureRunner.RunAsync` with `GetCompileErrorsAsync` only; `CodeFixRunCommand` maps the outcome | one owner |
| "Did a change add a compile error" elsewhere | `CasingRenameSafetyUtilities.HasAddedCompileErrorsAsync` (`src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs:154`): per-ID counts of one rename across projects | a separate decision on another surface (whether a casing fix is offered, decided once before and after one rename); its multiset key would flag the field split that `...RepeatsACompileErrorTheFixtureAlreadyHad` pins, so the different key is justified by decision 1 and 3 and not a copy |
| Statement of the ID-only rule | `FixtureRunner.cs:130-132` comment, `GetCompileErrorsAsync` summary and `<returns>`, `FixtureOutcome.InvalidResult` summary, `scripts/README.md:88`, `FixtureRunnerCompilationTests` class summary; `rg -i "distinct message|ID and message"` over `src`, `tests/Reihitsu.Tooling.Test`, `scripts`: nothing left | consistent |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RunAsyncReportsFixedWhenTheFixRenamesOneOfSeveralSameNamedSymbolsPerIteration` (new) | An intermediate result of a rename applied one occurrence per iteration, whose existing error ID shows two different names, is not invalid | The decision-2 key (distinct messages per ID) reports `InvalidResult` after iteration 1 (observed by `11`); a check on the final result only would not be caught, but the runner's per-iteration check is what the test exercises, and it also pins 2 iterations and the final source | `TextReportingFakeAnalyzer` + inline `SourceTransformingFakeCodeFix`, `RunAsync`, LF, `Latest` | adequate |
| Remaining tests (unchanged assertions) | As in `05`/`10` | `...KeepsACompileError...` falsifies "any error is invalid"; `...RepeatsACompileError...` falsifies a multiset key; the rename tests falsify an (ID, message) key; `...AddsAnotherCompileError...`, `...IntroducesAnUndefinedName`, `...ReplacesAnExistingErrorWithOneOfAnotherId`, and the CS8967 tests falsify a check that misses a new ID | as before | adequate |
| Dropped: `...AddsAnErrorWithTheSameIdForAnotherName`, `...RenamesATypeAndAddsAnErrorWithTheSameId` | They pinned a per-name distinction that decision 3 gives up | — | — | removal required by decision 3; no test pins the accepted limit, which is a decision, not a guarantee |

## Failure classes dismissed
- FC-001, FC-002, FC-003: one guard changed; both sides use one function, and its qualifiers (errors only, IDs, parse errors included, result against fixture) are each tested on both sides.
- FC-004 to FC-006, FC-019 to FC-029: the runner rewrites no source; it classifies. FC-026 (identical tokens re-resolved after an edit) is the shape of the new test's fake fix, which replaces the first occurrence per iteration on purpose; the runner itself resolves nothing by text.
- FC-007 to FC-014, FC-018, FC-030, FC-038 to FC-043: no analyzer, code fix, formatter phase, or unsupported shape changes.
- FC-015, FC-016: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-031: every changed summary, `<returns>`, param doc, and the inline comment describe the ID-only code.
- FC-032 to FC-035, FC-037: tests run the changed runner through `RunAsync`, expected values are hand-written, and IDs carry no line ending or position (the LF and CRLF arms of the re-runs above agree); no text-only claim.
- FC-036: the runner judges a rename only by compile errors; what a rename changes beyond them is the rename rules' own spec (`shared/casing.md`), not this check.
- FC-044: the cause of #3 is removed; the key reads the compiler's ID, which no fix rewrites, and models nothing.

## Architecture findings
_None._ The architecture finding of `10` is resolved by the refusal of decision 3.

## Hints
- The accepted limit of decision 3 is wider for the issue's own subject than its row suggests. Under a lower `--lang-version`, every C# 11 feature reports one ID, CS8936 "Feature '…' is not available in C# 10.0" (probed with Roslyn 5.12.0: generic attributes, raw string literals, list patterns, file types, required members, UTF-8 literals; CS8703 for `static abstract`), so a fixture that already uses one unavailable feature hides a fix result that writes another; likewise a snippet fixture with an undefined name (CS0103) hides a new undefined name. The issue's case, CS8967, has its own ID and is caught. The PR's statement of how the result differs from the issue's "Done when" ("compile errors that the original document did not have") could name this.
- `sdd/changes/2026-11/tooling-lang-version.md:15` still calls `invalid-result` "the check whether a fix result parses". It is the record of an earlier change, not a spec, so not a finding.
- Unchanged from `05`: one extra compilation per iteration next to `RunIterationAsync`'s own, and `HasSyntaxErrors` staying `public`.

## Follow-up candidates
_None._
