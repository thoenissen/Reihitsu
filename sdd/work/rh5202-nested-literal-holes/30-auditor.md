Role: auditor
Commit: c1b68ba
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audit covers the final narrowing round (lead decision 15) as a repair delta against a0535a8, the head of the third audit (`25-auditor.md`). It is not a full audit.

State:

- The checkout is c1b68ba and the working tree is clean.
- After fetching, `origin/main` is b4343557df67bc95a2278c405950476716fb9abf, as in the prompt, and it is an ancestor of the head.
- `scripts/lane-check.sh bug-fix` reports `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.

`git diff a0535a8 HEAD` without `sdd/work/` matches handovers 26–29:

- In `sdd/specs/analyzer/RH5202.md`, only the last sentences of the RH5202-006 *Why* changed. Its normative sentence, its conditions, its examples, and `requirement-ids.txt` are unchanged.
- Two tests were appended to `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs`.

No file under `src/` other than that test file changed since cc662a9. The last compiled production change is still cc662a9, so the build evidence of `28-cleaner.md` (0 warnings, RH5202 91/91) covers the head (FC-032). The scope did not grow.

Evidence. All fixtures, probes, and worktrees were in the scratchpad and were deleted afterwards.

- **Mutation run.** In a detached scratch worktree of the head, I made `StartsInOwnHole` effectively constant `false` at the guard's call site with `(StartsInOwnHole(nested, line.Start) && line.Start < 0) == false`. The RH5202 class then failed exactly the two new tests, 89/91.
- **Comparison with `main`.** `scripts/apply-fix.sh RH5202 --max-iterations 1` ran over the new test's input in a worktree of `origin/main` and at the head, under LF and CRLF. The diffs are identical, and both equal the test's expected output.
- **Multi-diagnostic probe.** A scratch file-based app (`#:project` on the CodeFixes project). On each iteration it registers actions for every RH5202 diagnostic, applies the first offered action on any of them, and prints the final text and the literal values. It ran under LF and CRLF.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed (unchanged since `19-auditor.md`) | — |
| 2 | superseded (unchanged since `19-auditor.md`) | — |
| 3 | closed (unchanged since `25-auditor.md`) | The guard and the rewrite did not change |
| 4 | closed (unchanged since `25-auditor.md`) | — |
| 5 | closed | **The input is finding #5's own shape.** The outer `$"""` shifts 8 to the left. The nested `$"""` has `k = 4`. Its text line `{n` has `w = 4 = k`, which is the text-side boundary. Its line `+ 1}` lies in its own hole with `w = 14 > k`. So only the own-hole qualifier keeps the fix offered.<br>**The tests pin it.** They assert the exact single-fix output, under LF and CRLF, and that the nested literal is still reported.<br>**The tests can fail.** Under the mutation, both tests fail and the other 89 pass, independently of `27-test-implementer.md`'s run.<br>**The expected output equals `main`'s** under LF and CRLF (`apply-fix` on both trees). |
| 6 | closed | Members (a) and (b) of #6 are depth-1 nested literals opening at a column below the shift. The new *Why* text states them correctly: the outer diagnostic stays without a fix. The release case (`c ≥ d`) is stated correctly too: the nested fix is rightward because `c ≥ d > k`, so it is never withheld, and after it `k' = c ≥ d` releases the outer guard. The claim "always the case when the nested literal opens on a content line of the outer literal" holds: `c > w ≥ K ≥ d`. A member that #6 did not enumerate fails the new text; that is #7. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | low | `sdd/specs/analyzer/RH5202.md`, RH5202-006 *Why*, last sentence: "When they stand closer to the start of their line, … no fix of the nested literal can widen its closing whitespace to the shift, and the outer diagnostic stays without a fix." | FC-031 (spec text against the code), FC-027 (nesting depth) | #6 | PR-introduced | **The "stays without a fix" side is stated for every input, but it fails at depth 2.** It fails when the value-changing literal N2 is nested in a hole of a raw literal N1 that is itself in the outer literal's hole, and N1's own fix (rightward, never withheld) moves N2's lines far enough. N2's own fix cannot widen its closing to the shift. N1's fix can.<br>**Failing member (probe, LF and CRLF).** Inside `string M(int n)`: `return $"""` / 25 spaces `Hello {$"""` / 4 spaces `{"""` / 6 spaces `x` / 2 spaces `"""}` / 4 spaces `"""}` / 25 spaces `""";`. Here the outer `d = 8`, N1 has `c1 = 33` and `k1 = 4`, and N2 has `c2 = 5 < d` and `k2 = 2`, with `x` at `w = 6 > k2`.<br>• Iteration 0: the outer literal is withheld and N1's fix is offered.<br>• After N1's fix (+29), the outer fix is offered.<br>• Then N2's fix runs. The final value of N2 is `    x`, which is kept.<br>The behavior is correct (the guard is exact, see #3). Only the spec's claim is too general.<br>**Members that hold.** Depth 1 with `c < d`, which is #6 (a) and (b) as traced in `25-auditor.md`. Depth 2 with N1 already aligned, so N1 has no fix: `$"""` → `$"""` closing at 33, N2 `"""` in column 0 on a line of N1's hole code. In that case only N2's fix (−2) is offered, and the outer stays withheld (probe). | Limit the sentence to what holds. Either say that no fix of the nested literal itself can widen its closing whitespace, and that the outer diagnostic then keeps no fix unless a fix of a raw string literal enclosing the nested one moves its lines far enough to the right; or replace the clause with "the outer fix may stay withheld". Spec *Why* text only. |

## Guard scope

_N/A — the round changed no guard, predicate, or exemption. The guard-scope rows of `25-auditor.md` are byte-identical at the head (no `src/` production change since cc662a9). The row "Starts in its own hole", which was untested there, is now covered (#5)._

## Policy ownership

_N/A — no policy owner changed in this round. The rows of `25-auditor.md` stand unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixShiftsLeftWhenWiderLineOfInterpolatedRawStringNestedInHoleStartsInItsOwnHole` | A wider line of a nested `$"""` that starts in its own hole does not withhold the outer fix. The outer rewrite gives exactly `min(8, w)` per line. The nested literal stays reported | Fix withheld (mutation: "No code fix action was registered"), a different output, or a missing nested diagnostic | `Verify` with two exact markup locations, then `ApplyCodeFixAsync` on the first diagnostic (the outer literal) with an exact `Assert.AreEqual`, then `Verify` with the nested location | adequate. Mutation-tested. The text line sits at the `w = k` boundary, so the hole line alone decides the outcome |
| `…WithCrLf` | The same under CRLF | The same | The same plus `NormalizeToCarriageReturnLineFeed` | adequate. Mutation-tested |

**Visible output (check 10).** The new expected value is the outer literal's fix result for an offered input. It is identical to `main`'s first-iteration output under LF and CRLF (`apply-fix` on both trees), so no visible output changes. RH5202-003 and RH5202-004 demand it. The rule page did not change. No expected value is undecided.

## Failure classes dismissed

- FC-001, FC-002, FC-005, FC-015, FC-016: no guard or policy changed. FC-002's qualifier "rather than in one of its own holes" is now pinned (#5).
- FC-003: the new test sits at the text-side boundary `w = k`. The withheld side `w > k` is pinned by the existing tests.
- FC-004, FC-019, FC-020, FC-021, FC-022: no rewrite changed. The output equals `main`'s.
- FC-006, FC-023 to FC-026, FC-029, FC-036: no compiled change, no rename, and no new member kind.
- FC-007, FC-008, FC-009: the registration and the rewrite are unchanged. The depth-2 probe converges in 3 iterations and keeps the nested value.
- FC-010 to FC-014, FC-028, FC-030: no formatter or analyzer change in this round.
- FC-017: see *Assertion adequacy*.
- FC-027: the depth axis of the spec text is #7. The code holds at depth 2 (probe).
- FC-031: the summaries of the new tests describe their inputs and assertions. The spec text is #7.
- FC-032: no compiled production change since cc662a9.
- FC-033: a CRLF variant exists, and the harness keeps the shape: `apply-fix` on the same input under both endings gives the same diff.
- FC-034: the input is #5's own shape.
- FC-035: the expected output matches `main`'s independent output.
- FC-037: no text-only claim. The spec change is Markdown.

## Architecture findings

_None._ #7 is spec *Why* text only and repeats #6's class, which is the release claim of RH5202-006 against the fix's guard. No component pair disagrees: the guard and the rewrite still share `GetShift`, `GetLeadingWhitespaceLength`, and `SourceText.Lines`. Under `README.md`, *Architecture findings*, the lead decides the repeat.

## Hints

The verdict is `pass, text cleanup` because #7 is confined to the non-normative *Why* paragraph of a spec. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata.
