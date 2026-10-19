Role: spec author
Commit: a616e17
Verdict: spec written

I re-checked each item of `06-skeptic.md` myself before changing anything, using my scratch probe and `scripts/trace.sh`.

After the change:
- The RH7303 and `shared/type-regions.md` example blocks reproduce: 12 blocks, 0 mismatches.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"` passes all 26 tests.
- No requirement ID was added, retired, moved, or renumbered. None of the files I was told not to touch were edited.

| Item | Re-checked | What changed |
|---|---|---|
| R1 — a statement ending in `}` next to a removed directive | Yes. Input: `if (a) { }` / `#region Work` / `M(a);` / `#endregion // Work` / `M(a);`. Both directives are reported; the fix leaves `}` directly followed by `M(a);`. `trace.sh` on that result shows the blank-line phase adding a line after the `}`. | RH7303-004 now also requires that neither directive directly follows a statement that ends with a closing brace. I also added the shape to Q4 of the RH7303 issue, with SHARED-BLANK-003 named. |
| R2 — the RH7309 part filed under RH7303 | — | Moved out of RH7303 Q2 into a new Q3 of the RH7309 issue, "The fix moves a region out of a field initializer", decided by P1. RH7303 Q2 and RH7309 Q3 now link to each other. |
| R3 — RH7203 Q1 too broad | Yes. With `using static Microsoft.X;` or `using X = Microsoft.Y;` between `Microsoft.B` and `Microsoft.A`, `Microsoft.A` is still reported. | The title now says a regular directive of another non-`System` root. The "Why" explains that `using static` and alias directives never hide a pair. |
| R4 — RH7302 Q1 fix sentence | Yes, matches the table rows. | Now says no fix is offered only for an already capitalized description (`# region Fields`). For `# region fields`, a fix is offered with one space or more, and the diagnostic stays. |
| Minor — RH7303 Q3 wording | — | Now reads "Both directives sit in top-level statement code". |

The complete, final open-question texts for the lead to file are in `sdd/work/o2-using-region-specs/05-spec-author.md`, section **"Open questions — final ready-to-file text"**, updated in place for R1–R4 and the RH7303 Q3 wording. The RH7309 issue now has Q1 (comments), Q2 (nested enum), and Q3 (initializer, P1).
