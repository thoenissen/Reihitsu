Role: spec author
Commit: d22b1084f5767a6e0ac168d726995b8ea7b0a3af
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh5112-rh5048-member-access-wording.md` (`Status: Draft`). Its *Visible output changes* lists five items for the lead's output questions: (1) RH5112 title and message, (2) RH5048 title and message, (3) RH5112 code-fix title, (4) rule-page headings and package README rows, (5) rule-page "Why"/"How to fix" sentences that still say "call".

## Premise

Confirmed from the specs: RH5112-001 and RH5048-001 already cover a first link that is a property read (their examples report `d` ⏎ `.Next` and `c` ⏎ `// next` ⏎ `.Next`), so "first call" does not describe every reported case. No caller question: the diagnostic text is shown for every report. The issue is `agent-filed`; its proposal is followed except where noted below.

## Changed requirements

None. `sdd/README.md` keeps diagnostic messages and code-fix titles out of requirements. Changed text:

- `sdd/specs/analyzer/RH5112.md` — spec title: "Wrapped chains should keep the first member access on the original line".
- `sdd/specs/analyzer/RH5048.md` — spec title: "Comments must not be placed before the first member access of a wrapped chain".
- `sdd/specs/analyzer/RH5038.md`, `sdd/specs/shared/comment-placement.md` — the non-goal pointers to RH5048 say "first member access of a wrapped chain".
- `documentation/rules/RH5112.md`, `documentation/rules/RH5048.md` — headings, plus the "Why" and "How to fix" sentences that still said "call" (change doc item 5). The RH5112 sentence about property reads in front of the first *call* (`settings.Items.Where(...)`) means a call and stays.
- `src/Reihitsu.Analyzer.Package/README.MD` — the RH5048 and RH5112 rows. Edited here because `AnalyzerPackageMetadataTests.PackageReadmeDescriptionsMatchRuleDocumentationTitles` ties them to the rule-page headings.

Left for the implementer (production files): `src/Reihitsu.Analyzer/AnalyzerResources.resx` (`RH5112Title`, `RH5112MessageFormat` keep their trailing period; `RH5048Title`, `RH5048MessageFormat` keep none), `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` (`RH5112Title` → "Keep first wrapped member access on original line"), and the `<summary>` comments of the two analyzer classes that repeat the title. There are no localized resource files. `binaries/` is not touched (that is `/bump-version`'s job).

## Examples for the tests

No requirement examples change. The test that pins the new titles already exists and now fails as intended: `AnalyzerPackageMetadataTests.AnalyzerTitleResourcesMatchRuleDocumentationTitles` reports RH5112 and RH5048 until the resources change. The message format and code-fix title have no test of their own; whether one is worth adding is the test implementer's call (the RH5112 code-fix title is visible only through `CodeAction.Title`).

## Output questions

The five visible output changes are in the change doc, each with before and after, for the lead to ask. No change is in doubt beyond the wording choices themselves:

- Code-fix title: recommended "Keep first wrapped member access on original line" (word-for-word substitution); alternative "Keep first member access on original line".
- Item 5 (rule-page body sentences) goes beyond the issue, which asks only for headings; recommended because otherwise the pages' "Why" sentences contradict their new headings. It can be dropped on its own.

## Internal choices

- Wrote a change doc although no requirement changes (strictly, `sdd/README.md` asks for one only on a requirement change); the lead set behavior-change lane, and the visible text changes need a place with before and after. `Affected requirements` says "none — wording only" with the touched spec titles. Alternative: no change doc, record the decisions only in `00-lead.md`.
- Kept each string's trailing punctuation as today. Alternative: normalize both.
- Type names (`…FirstCall…` analyzer, code-fix, and test classes) are not renamed: not visible, and the code-fix equivalence key is the provider type name. Alternative: rename, which also changes the code owners in both specs and `sdd/specs/README.md`.
- Updated the RH5038 and comment-placement non-goal pointers to the new wording (spec text, no behavior).

## Follow-up candidates

_None._
