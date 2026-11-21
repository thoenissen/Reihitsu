Role: spec author
Commit: 03b5963
Verdict: text written

The prompt carried the maintainer's answer ("a") and the change asked; no intended fix, suspected cause, or hoped-for verdict beyond that; nothing ignored.

## Files

- `documentation/rules/RH5201.md` — change 4, option A: the Description's first sentence says "member-access chain"; the Why sentences say "Misaligned member-access chains" and "the chain of member accesses". The "first call" bullets, "How to fix it", and the examples are unchanged. BOM kept.
- `sdd/changes/2026-11/rh5201-member-access-wording.md` — `Status: Accepted`; `Decisions` → `Maintainer answers` entry 2 records option A; the What sentence no longer says "if the maintainer takes change 4"; the alternative "change only the resources, the heading, and the README row" is now marked not taken.

No spec or requirement-ID change, so the architecture tests were not rerun for this edit.

## Changed requirements

None.

## Examples for the tests

Unchanged from `01-spec-author.md`: the message "Member-access chains should be aligned." and the code-fix title "Align member-access chain" (tests listed in `02-skeptic.md`). The rule page body is not pinned by a test.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None._
