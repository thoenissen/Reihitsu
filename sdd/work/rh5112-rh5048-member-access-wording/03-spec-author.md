Role: spec author
Commit: 2350c82
Verdict: change doc written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's recorded answers, which are binding.

## What changed in this round

- `sdd/changes/2026-11/rh5112-rh5048-member-access-wording.md`: `Status: Accepted`; new `Decisions` subsection *Maintainer answers* records answers 1-5 from `00-lead.md`, mapped to the change doc's visible output changes; change 5's RH5048 "After" sentence now reads "The first member access of a chain belongs on the line its receiver ends on."
- `documentation/rules/RH5048.md`, *Why is this a problem?*: answer 5 (option B) applied: "The first member access of a chain belongs on the line its receiver ends on. A comment in front of it keeps the member access apart from its receiver." The UTF-8 BOM of the page is kept.
- A search of `sdd/specs`, `documentation`, and `src` (`.md`, `.MD`, `.resx`, `.cs`) found the old phrase "on the line of the expression it accesses" nowhere else. The RH5112 description already uses "on the line its receiver ends on".

Answers 1-4 need no further spec or page edit: the headings, package README rows, and RH5112 body text were already written in 2523a56 as accepted.

## Changed requirements

None. Wording only, as in 01-spec-author.md.

## Examples for the tests

No requirement examples change. Left for the test implementer and implementer, unchanged from 01-spec-author.md:
- `AnalyzerResources.resx`: `RH5112Title`/`RH5112MessageFormat` = "Wrapped chains should keep the first member access on the original line." (period); `RH5048Title`/`RH5048MessageFormat` = "Comments must not be placed before the first member access of a wrapped chain" (no period).
- `CodeFixResources.resx`: `RH5112Title` = "Keep first wrapped member access on original line".
- The `<summary>` comments of the two analyzer classes follow the new titles.

Checks: `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"` passes 70/70. `AnalyzerPackageMetadataTests.AnalyzerTitleResourcesMatchRuleDocumentationTitles` fails on RH5048 and RH5112 only, as intended until the resources change; `RuleDocumentationConventionTests` and the README test pass.

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None_ beyond the RH5201 candidate already recorded in `00-lead.md` from 02-skeptic.md.
