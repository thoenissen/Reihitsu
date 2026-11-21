Role: auditor
Commit: 7107bb8ed65ae5fc9defe664697487a70d59b6e3
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing ignored.

## Scope

Repair round 1 (CI), repair delta `c369147..7107bb8`. Checkout is the head SHA `7107bb8`, working tree clean. `origin/main` fetched = `b69a6ae347048699a871c3f15ce1cea97d4917c5`, equal to the SHA in the prompt; `git merge-base --is-ancestor` confirms it is merged. The merge commit's own diff (`7107bb8^1..7107bb8`) is byte-identical to `merge-base..b69a6ae`: `sdd/specs/formatter/indentation.md`, `sdd/specs/shared/diagnostics.md`, `squad/notes/2026-11-21-spec-wording-indent-diag.md` — none of this run's files, no conflict resolution. `scripts/lane-check.sh behavior-change`: confirmed.

Outside `sdd/work/` and the merged main content, the repair touches two test files only, as `10-ci.md` to `13-spec-checker.md` describe:

- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MethodChainsShouldBeAlignedAnalyzerTests.cs`: `git diff origin/main HEAD` on it is empty — back to the content `main` passes RH2111 with, usings included.
- `src/Reihitsu.Analyzer.Test/Formatting/RH5201MemberAccessChainWordingTests.cs` (new, 134 lines): the three wording tests, compared mechanically against the removed block — verbatim (summaries, inputs, assertions). Plus a new `GetFixAllScenario`.

Scope did not grow: no production, spec, rule page, or resource change in the repair. Evidence of `08-auditor.md` for the production change, rule page, resources, and package README is reused unchanged (byte-identical since `b427430`).

### Previous findings

`08-auditor.md` reported none; nothing to close.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|

_N/A — the repair adds or changes no predicate, guard, or exemption; it moves tests and adds one test scenario._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|

_N/A — no policy moved or changed in the repair; the two owners recorded in `08-auditor.md` (`AnalyzerResources.resx` RH5201 title/message, `CodeFixResources.resx` RH5201 title) are untouched._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH5201MemberAccessChainWordingTests.VerifyTitleNamesMemberAccessChain` (moved) | as in `08` | as in `08` | as in `08` | adequate; verbatim |
| `RH5201MemberAccessChainWordingTests.VerifyMisalignedLinkOfCallLessChainIsReportedWithMemberAccessChainMessage` (moved) | as in `08` | as in `08` | as in `08` | adequate; verbatim |
| `RH5201MemberAccessChainWordingTests.VerifyCodeFixTitleNamesMemberAccessChain` (moved) | as in `08` | as in `08` | as in `08` | adequate; verbatim |
| `RH5201MemberAccessChainWordingTests` Fix All via `GetFixAllScenario` (new) | two misaligned links of a call-less chain (`.City`, `.Name`) are both reported on their `.` and both aligned under `.Address`'s dot in one Fix All iteration | a missing or extra diagnostic, other locations, a result not under the anchor, or needing a second iteration | `BatchCodeFixTestsBase` with `Diagnostics(id, MessageFormat, 2)` and `NumberOfFixAllIterations = 1` | adequate for what it guards (the batch base's Fix All contract); `MessageFormat` rather than the literal is fine because the literal is pinned by the moved message test |

Visible output is decided: the new scenario's expected result is demanded by unchanged requirements — RH5201-002/RH5201-004 (each misaligned wrapped link reported on its operator) and RH5201-005 with SHARED-CHAIN-007 (wrapped links align to the anchor, here `.Address`) — the same layout the moved test already expects for `.City`, extended to a second link. The base class choice matches the provider (it returns a Fix All provider; `BatchCodeFixTestsBase` asserts this), per `AGENTS.md`. The second class for one rule follows the `RH5111AssignmentsMustHaveProperLineBreaksConditionalAccessTests` precedent, in the same folder and namespace as the analyzer test class, which fits: the moved tests are analyzer/code-fix tests, not the formatter tests under `Formatter/Formatting/`.

Run evidence trusted, not re-run: `11` and `12` report `--filter RH5201` 147 passed and the self-hosting `AnalyzerReportsNoViolationsOnOwnSourceCode` set 352 passed (RH2111 row included); `13` reports the architecture run 198 passed. No finding turns on a re-run.

## Failure classes dismissed
- FC-001 to FC-016, FC-018 to FC-030, FC-036, FC-038 to FC-044 — no production code, guard, rewrite, formatter phase, or unsupported shape is touched by the repair; reasons in `08-auditor.md` unchanged.
- FC-017 — see assertion table.
- FC-031 — no changed method summary in production; the new class's `<summary>` and the scenario's inline comment describe what the class and scenario verify.
- FC-032 — the repair moves tests only; `12` built and ran the focused tests after the move.
- FC-033 to FC-035 — expected values are the maintainer's literal answers (moved) and the requirement-demanded layout (new scenario), not copied from patched output.
- FC-037 — not claimed text-only; tests only.

## Architecture findings
_None._

## Hints
- `squad/notes/2026-11-21-rh5201-member-access-wording.md` says "no repair rounds"; the run now has one. The lead rewrites the note at the end of the run (`squad/lead.md`, step 4), so this is for that rewrite, not a finding.

## Follow-up candidates
_None._
