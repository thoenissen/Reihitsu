Role: auditor
Commit: 732004cf2a3193c860845bbe506c7e7885084f80
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This is the repair-delta audit, the run's second audit. It compares against `3425508`, the head of the first audit (`09-auditor.md`).

State checks:

- Head is `732004c`, checked out with a clean tree.
- `origin/main` is `66fd1d7`, fetched again. It matches the prompt and is an ancestor of the head.

Outside `sdd/work/`, the repair diff `3425508..732004c` touches four files:

- `PreparationPhase.cs`: `Execute`.
- `ControlFlowBraceTransform.cs:53`: a param comment.
- `FormattingPipelineTests.cs`: one test reworked and two added.
- `AGENTS.md:122`: one table row.

This matches `10-implementer.md` to `14-spec-checker.md`. No spec, change doc, or ledger changed, and no new decision entry was added. The scope did not grow beyond the four findings, so this is a delta audit. Evidence from `09-auditor.md` for files the repair did not touch is reused unchanged.

Status of the previous findings:

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `ControlFlowBraceTransform.cs:53` now names all three places where `WrapWithBraces` writes `endOfLine`: behind the opening brace (`:91`), behind each kept leading comment (through `NormalizeLeadingTrivia`, `:74`), and after the closing brace (`:84`). These are the only uses in the method. |
| 2 | closed | `PreparationPhase.Execute` no longer has its own loop. It returns `FormattingPipeline.ExecutePhases(root, context, [this], phaseObserver: null, cancellationToken)`. The `phase is PreparationPhase` branch there (`FormattingPipeline.cs:76`) expands it into `SubPhases` and runs each sub-phase through `ExecutePhase`. Production now has one runner of the sequence. There is no recursion, because `SubPhases` holds only `TabReplacementPhase` and `LineEndingNormalizationPhase`. The members named in #2: the pipeline runner holds. `PreparationPhase.Execute` now delegates and is tested. `PhaseStabilityCheck.CreateProductionPhases` expands the list so each phase can run alone, and `CheckRunsTheProductionPhasesInPipelineOrder` still proves its parity with the pipeline. |
| 3 | closed | `PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder` uses `new PreparationPhase()`. The reflection lookup and its stale comment are gone, and the assertions are unchanged. |
| 4 | closed | The `AGENTS.md:122` row now reads "comments that are not documentation comments", which matches FMT-PIPE-019 and FMT-EOL-003. The phase names and the row count are unchanged, so `PhaseOwnershipDocumentationTests` is unaffected (`14`: architecture 89/89). |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which phases are expanded into reported sub-phases | `FormattingPipeline.ExecutePhases`: `phase is PreparationPhase` → `SubPhases` (unchanged) | `PreparationPhase.Execute`, which now delegates to that branch; `PhaseStabilityCheck.CreateProductionPhases` (same predicate, unchanged) | All runs of the sequence now go through the one branch. The test helper only expands the list and does not run it in sequence | holds |

No other predicate or guard changed in the repair.

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Order and running of the preparation sub-phases | `rg -n "SubPhases\|foreach \(var subPhase" src --type cs`: `PreparationPhase.cs:38` (the list), `FormattingPipeline.cs:78` (the one runner), `PhaseStabilityCheck.cs:182` (test helper; expands the list to run each phase alone) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FormattingPipelineTests.PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks` (new, FMT-PIPE-019) | Run directly, the preparation phase replaces tabs in code whitespace and between directive tokens, and rewrites end-of-line trivia, directive line breaks, and documentation newlines to the context's line ending | The output differs from the explicit expected text. The input carries `\r\n`, lone `\r`, U+2028, and U+0085, plus tabs in the indentation and after `#region`. An `Execute` that skipped either sub-phase or returned its input would fail. | direct `Execute`, exact string | adequate. The second assertion (equal to `ExecutePhases`) holds by construction now, but adds a check without weakening the first |
| `FormattingPipelineTests.PreparationPhaseExecutedDirectlyWithCancelledTokenThrowsOperationCanceled` (new, untagged) | A cancelled token stops `Execute` | A result is returned, or an exception of another type is thrown. `ExecutePhase` calls `ThrowIfCancellationRequested`, which throws exactly `OperationCanceledException` | `Assert.ThrowsExactly` | adequate |
| `FormattingPipelineTests.PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder` (reworked) | as in `09` | as in `09` | direct construction instead of reflection | adequate |

## Failure classes dismissed

- FC-001, FC-002, FC-003: no predicate changed. The one decision the repair touches is listed under *Guard scope*.
- FC-004 to FC-009, FC-018 to FC-030, FC-036: the repair changes no rewrite, trivia handling, code fix, or syntax shape. Its only compiled change is the body of `PreparationPhase.Execute`.
- FC-010, FC-011, FC-012: no phase output changes. The production pipeline never calls `PreparationPhase.Execute`: `ExecutePhases` expands the type before it reaches `ExecutePhase`. Delegating cannot recurse, because no sub-phase is a `PreparationPhase`.
- FC-013, FC-014: no analyzer change.
- FC-015, FC-016: one runner, as shown in the policy table. The sibling statements named in #2 were checked as listed in the status table.
- FC-017: covered in the assertion table.
- FC-031: the new `<remarks>` on `PreparationPhase.Execute` describes the delegation correctly. The class summary ("the pipeline runs and reports it under its sub-phase name, and this phase itself is never reported") still holds. The `ControlFlowBraceTransform` comment is accurate (#1).
- FC-032: the compiled change is limited to a method that no production path, and so no code fix, reaches. `10` ran formatter and CLI (3148/3148, 251/251) and `14` ran architecture (89/89) after the change. The validator runs the default set on the head.
- FC-033, FC-034, FC-035: the new test's expected text is written out from the FMT-TAB and FMT-EOL rules, not taken from an output of the code.
- FC-037: `AGENTS.md` is Markdown, and the C# comment change in `ControlFlowBraceTransform.cs` sits inside an XML documentation comment. No text-only proof is claimed or needed for the verdict.
- Tab width across formatter phases (matrix): not reached, as in `09`.

## Architecture findings

_None._

## Hints

- `PreparationPhase.Execute` is now correct only because `FormattingPipeline.ExecutePhases` treats `PreparationPhase` as a special case. If that branch were removed, `Execute` would call itself through `ExecutePhase` until the stack overflows. Every pipeline test would then fail loudly, and the `<remarks>` names the dependency, so this is not a finding. It is also the first phase that refers back to `FormattingPipeline`. The implementer rejected the alternatives with reasons in `10-implementer.md`.
- The hints of `09-auditor.md` still apply. None was addressed or needed to be.
