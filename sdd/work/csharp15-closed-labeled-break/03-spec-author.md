Role: spec author
Commit: 4bf6963
Verdict: #1764: change doc written; #1765: spec written (unchanged since 01-spec-author.md)

Inputs: `00-lead.md` (maintainer answers 1B and 2B, decisions 2–8), `02-skeptic.md`. Every new C# block in `RH7105.md` was compiled with Roslyn 5.12 at `LanguageVersion.Preview` (markup stripped) in a scratch program outside the repository: no compile errors.

## thoenissen/Reihitsu#1764

### Change doc

`sdd/changes/2026-11/rh7105-closed-safe-modifiers.md`, `Status: Draft` (setting `Accepted` is the lead's). `Visible output changes` repeats the inputs of output questions 1 and 2 as the skeptic restated them (`02-skeptic.md`), with before (`main`) and after (answer B), plus the rule-page order line before and after. `Decisions` records both answers and the undecided pair order; `Alternatives considered` records the excluded "reported as today, no fix", the skeptic's "every unnamed modifier in front of `ref`/`partial`", and "`safe` before `extern`".

### Changed requirements

- RH7105-001 — Modifiers out of the canonical order are reported — changed: `closed` stands in the position of `sealed`, `safe` in the position of `unsafe`; their order relative to `sealed`/`unsafe` is not decided (C# allows neither pair on one declaration); `Why:` gains the reason; new example with `new closed partial` and `static safe extern` reported.
- RH7105-002 — Modifiers in the canonical order are not reported — changed: new negative example (`closed partial`, `closed new partial`, `closed unsafe partial`, `static extern safe`, `safe partial` both parts, `safe required`, `safe ref struct`).
- RH7105-004 — The fix writes the canonical order — changed: new before/after example (`public new closed partial class N` → `public closed new partial class N`; `public static safe extern void M();` → `public static extern safe void M();`).
- Rule page `documentation/rules/RH7105.md`: the order line under *How to fix it* reads `` `sealed` or `closed` `` and `` `unsafe` or `safe` ``. Nothing else on the page changes; "Any modifier not listed above, such as `const` or `fixed`, belongs at the end" still holds.

No requirement ID added; `requirement-ids.txt` unchanged.

### Examples for the tests

RH7105-001 (the `closed`/`safe` example), RH7105-002 (the `closed`/`safe` negative example), RH7105-004 (the `closed`/`safe` fix example). Every `safe` example needs `LanguageVersion.Preview` and the fixed code must be compiled by the verifier (CS0267/CS1585 are not syntax errors; `02-skeptic.md`, FC-033).

### Output questions

_None._

### Internal choices

- The relative order of `closed`/`sealed` and `safe`/`unsafe` is left undecided in the spec rather than declared a shared position, because such declarations do not compile; alternative: "each pair shares its position, so its order among itself is never reported". The implementer may give each pair one rank or two.
- `closed` and `safe` are named in the order list (the maintainer's answers place them by kind), not covered by a generic "every unnamed modifier in front of `ref`/`partial`" sentence, which the answers rejected; "every other modifier last" stays for `const` and `fixed`.
- `SHARED-DIAG-011` was not added to the spec's `Uses` row: it applies to every rule without being listed (`sdd/README.md`, *Analyzer specs*).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1765

Nothing new in this round; the clarifications of RH5010-002, RH5011-001/-002/-008, RH7501-001/-002 from commit 87f9643 stand (see `01-spec-author.md`).
