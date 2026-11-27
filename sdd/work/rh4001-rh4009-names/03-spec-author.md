Role: spec author
Commit: 8d9acc5
Verdict: change doc written

Amendment for lead decision 7 (skeptic 02, EF Core rationale) and acceptance of the change doc. Behavior is unchanged; no normative sentence and no example was edited. My prompt carried no intended fix or hoped-for verdict beyond the lead's recorded decisions 7 and 8.

## Changed requirements

- RH4001-008 — The fix keeps a migration timestamp prefix — clarified (`Why:` only). The false claim that EF Core orders migrations by the prefix is replaced by the true reason: EF Core names a migration's files after its ID (prefix + name), so the prefix sorts the files in creation order and keeps each next to its `.Designer.cs`; dropping it moves the file away from both. It also states that EF Core orders by the `[Migration]` attribute ID, which the fix does not change.
- `sdd/changes/2026-11/rh4001-rh4009-names.md`: `## Why` reworded the same way (the "breaks the project's migration history" claim removed); `Status: Accepted`; `## Decisions` opens with a line recording that the maintainer decided visible output changes 1 and 2 in question round 3 (2026-10-18, issue #1085; `00-lead.md` *Maintainer answers* 1–2), and that change 3 restates them on the rule pages.
- `documentation/rules/RH4001.md`: "The code fix keeps the prefix, so a migration keeps its place in the order of migrations:" becomes "The code fix keeps the prefix and replaces only the name after it:" — behavior only, no claim about order.

## Examples for the tests

Unchanged from 01-spec-author.md (RH4001-008, RH4001-006, RH4009-003).

## Output questions

_None._ The rule-page sentence now only restates maintainer answer 1.

## Internal choices

- The spec `Why:` also names what EF Core does read (the `[Migration]` ID), so the reason cannot be misread as runtime order again (alternative: state only the file-sorting reason).

## Follow-up candidates

_None._
