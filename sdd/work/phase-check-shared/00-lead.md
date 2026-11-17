# Lead — issue 1805

- Issue: thoenissen/Reihitsu#1805 — FMT-PIPE-027 per-phase stability check in the analyzer and CLI full-run idempotency tests. Label `agent-filed`: its goals are proposals.
- Slug: `phase-check-shared`
- Branch: `claude/intelligent-hopper-li5bmz` (session-designated); draft PR thoenissen/Reihitsu#1824.
- Lane: **tooling** (claimed tests, widened, decision 5) — — the issue adds a check to tests of existing behavior (FMT-PIPE-027); no production behavior changes. The issue's own wiring needs a `.csproj` change (a reference or a shared project), which the lane check classifies as tooling; expect a widening to `tooling` once that path is in the diff.
- Roles (tests lane): skeptic (premise, reachability, and the reference-vs-shared-project structure), test implementer, cleaner, spec checker, validator. Spec author skipped: the run adds no spec unless the skeptic finds a spec gap. Implementer and auditor skipped in the tests lane; a widening to tooling adds them.
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Keep the CLI half of the issue? | Drop it; the PR states the departure and still closes the issue | The CLI writes the formatter's output unchanged; the check there would only re-test the formatter on one fixture and can never find a CLI defect (01-skeptic) | Test-only change; no surface output changes | – |
| 2 | Skeptic | Reference vs. shared test-support project | `ProjectReference` from `Reihitsu.Analyzer.Test` to `Reihitsu.Formatter.Test` | Precedent of `Reihitsu.SelfHosting.Test`; a shared project needs a new `InternalsVisibleTo` in production code; failure text keeps its one owner | Test wiring only | – |
| 3 | Skeptic | How the base runs the check | Text overload with the line ending and parse options the base formatted with; check the output under both line endings; skip when the precheck refused the input; still check generated/hidden-code output | Skeptic's recommendations 1–3 | Test-only | – |
| 4 | Skeptic | Adjacent: RH5604 `VerifyFormatterEntryPointsAndIdempotency` | Take it in if it is the one-line call the skeptic describes | Same mechanism, same file set, gets the check | Test-only | – |
| 5 | Test implementer | Lane check: tests → tooling (the analyzer test `.csproj`) | Widen to tooling; roles added: implementer, auditor. Implementer skipped: the test implementer wrote the whole change as test code and every test passes, so the implementer has nothing to make pass | Widening order tests → tooling | Test wiring only | 44b2ecb |
| 6 | Test implementer | Also check `AssertExemptInteriorsSurviveNormalization` in RH5604 | Keep it | Same shape as decision 4; covers a detected line ending on a mixed file | Test-only | 44b2ecb |
| 7 | Auditor | Follow-up list of direct `FormatSyntaxTree` idempotency tests | Drop all members but RH6001; take `RH6001...VerifyFormatterFixesGluedFixedKeyword` into the run as an adjacent fix (same one-line shape as decision 4) | Auditor showed the others assert no idempotency or are already covered; one line, same mechanism, cheaper here than a run of its own | Test-only | – |
| 8 | Auditor | Finding #1 (stale XML summaries in the base class) | Repair round 1: cleaner rewords both summaries; together with decision 7, the auditor re-audits the repair | Comment-only, PR-introduced | Test-only | – |

## Follow-up candidates

- ~~Other direct `FormatSyntaxTree` tests~~ — dropped / taken in, decision 7 (02-test-implementer, 05-auditor).
- `scripts/format.sh` prints S1125 warnings for `src/Reihitsu.Cli/FormatCommandHandler.cs` (lines 647, 658, 712) while building the CLI; the Release build reports none; file untouched by this run (03-cleaner).

## Worklist / next step

- Claimed (commit "Claim issue #1805"), prepare.sh green.
- Skeptic done (01-skeptic.md): proceed narrower.
- Test implementer done (02): lane widened to tooling.
- Cleaner done (03): clean, bb2f598.
- Spec checker done (04): consistent, no findings.
- Merged origin/main abe1313 (clean, build green) as ec74419.
- Auditor (05) on 091e588: pass with one comment finding.
- Repair round 1 worklist: (a) test implementer adds the per-phase check to RH6001 `VerifyFormatterFixesGluedFixedKeyword` (decision 7); (b) cleaner rewords the two summaries (finding #1).
- (a) done (06-test-implementer), dc6c41f.
- (b) done (07-cleaner), 1931271.
- Spec checker (08) consistent; repair audit (09) pass on e20a302. **Audited SHA: e20a302.**
- Next: validator → issue manager (S1125 candidate) → note → publish.
