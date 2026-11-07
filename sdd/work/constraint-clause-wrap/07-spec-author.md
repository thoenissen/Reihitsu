Role: spec author
Commit: 4e8ff76
Verdict: spec written

This repair carries the maintainer's answers Q10 and Q11 (`00-lead.md`, *Maintainer answers*) into the specs and the change doc. The prompt carried no intended fix or suspected cause. No requirement ID was added, so `sdd/specs/requirement-ids.txt` is unchanged.

## Changed requirements

- **FMT-SPACE-009 — One space before braces and around declaration colons — changed (Q10).**
  - The normative sentence now also covers the colon of a type-parameter constraint clause (`where T : class`).
  - The `Why` says that this colon separates a type parameter from its constraints, as a base-list colon separates a type from its base types. It also covers a colon that a join wrote without a space (FMT-BREAK-031, FMT-BREAK-037). This resolves D1: FMT-BREAK-037's own examples showed `where T : IDisposable`, but no spacing rule produced it.
  - The example gains `where T:class`, which becomes `where T : class`.
  - FMT-BREAK-037 is added to the spec's `Uses`.
- **FMT-SPACE-019 — Name colons — clarified.** Its list of colons that are not name colons now names the constraint-clause colon next to the base list and the constructor initializer (FMT-SPACE-009). The rule itself does not change.
- **FMT-BREAK-037 — changed (Q11).**
  - A clause is now also wrapped when its colon starts a line because a comment, a directive, or disabled text keeps the colon off the type parameter's line.
  - The paragraph on blocked joins now reads "a colon kept off the type parameter's line starts its own line and wraps the clause".
  - The `Why` gains one sentence on the reason.
  - New example: `where T // c⏎ : class, new()` becomes `: class,⏎ new()`, with the colon at column 12 and both constraints at column 14. As the other side of the qualifier (FC-002), `where T⏎ : class, new()` is joined to one line.
- **FMT-INDENT-032 — unchanged.** Its wording already places a colon that starts a line (one level deeper than `where`) and aligns the constraints behind it. Once FMT-BREAK-037 wraps the clause, its constraints "span several lines" and the requirement applies as written. Its example `P` already shows the resulting columns.
- **RH5110-003 — unchanged.** It already says the fix gives the formatter's layout. Change doc entry 15 shows the fix result, which covers D3's test.
- **RH6021 spec — one non-goal reworded, no requirement changed.**
  - The constraint colon is taken out of the "other colons … which no analyzer rule checks with this rule's spacing" bullet and gets its own bullet.
  - The new bullet says the formatter spaces that colon like a base-list colon (FMT-SPACE-009). It also says the colon belongs to a different construct, so reporting it would take a rule of its own, not a wider RH6021 (P11).
  - The old wording was still literally true. It lumped the constraint colon with colons that have a different spacing, though, and after Q10 that reads like an oversight. RH6021-002's example `where T:class`, which is not reported, stays correct for the analyzer.
- **Change doc** `sdd/changes/2026-11/constraint-clause-wrap.md`:
  - FMT-SPACE-009 (changed) is added to `Affected requirements`.
  - *What* gains a spacing bullet and the clause "a colon kept that way wraps its clause".
  - New entry 14 (Q10). The "before" output is the real `scripts/format.sh` output at `main` e3924e2, which leaves all four tight colons unchanged.
  - New entry 15 (Q11) shows the formatter result and RH5110's fix result. Both "before" outputs are real, from `format.sh` and `apply-fix.sh RH5110` at e3924e2: `: class, new()` at column 4.
  - *Unchanged* notes that a one-line clause changes only in its colon spacing.
  - *Decisions* gains Q10 and Q11, and the intro now says the answers came in several batches.
  - *No analyzer change* names RH6021.
  - Two alternatives are added as rejected.
  - `Status` stays `Accepted`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51/51 pass.

## Examples for the tests

- FMT-SPACE-009: the new `void N<T>() where T:class` gives `where T : class`. Change doc entry 14 adds `where T :new()`, `where T: struct`, and a type's clause `class D<T> where T:class`. This resolves D1: the 7 tests that expect `where T : …` after a join become correct under the spec.
- FMT-BREAK-037: the new example (`M` wraps, `N` joins). This resolves D3: `VerifyFixIsOfferedWhereCommentKeepsColonOnItsOwnLine` expects `: class,⏎  new()`, which the spec now states. The formatter suite has no counterpart test for this shape, and the test implementer should add one from this example.

## Output questions

_None._

## Internal choices

- **FMT-INDENT-032 left unchanged.** Alternative: add an example with a colon on its own line and constraints that were written on one line. Rejected, because the requirement places lines and does not decide whether they exist, and FMT-BREAK-037 now carries that example.
- **RH6021: new-rule candidate, not the rule's own gap (P11).** RH6021's spec names two constructs: the base-list colon and the constructor-initializer colon. The constraint clause is a third construct, so I worded the non-goal as a new-rule candidate. Alternative: read RH6021's generic title ("Colons must be spaced correctly") as naming every declaration colon. That would make this RH6021's own gap and a behavior change of RH6021 that adds diagnostics. Either way it is outside this run. The non-goal follows the narrower reading.
- **Rejected options in Q10 and Q11.** I recorded them from D1 option (b) and D3's alternative in `06-implementer.md`. Q10's rejected option is "only the join writes the space, as an exception to FMT-BREAK-031". Q11's is "judge by the constraints alone". The lead should check that these match the options the maintainer was shown.

## Follow-up candidates

- **New-rule candidate (P11): spacing of the constraint-clause colon.** After Q10 the formatter turns `where T:class` into `where T : class`, and no rule reports the tight form. A new spacing rule could report it, the counterpart of RH6021 for constraint clauses. If the lead prefers the other reading above, this becomes a behavior change of RH6021.
