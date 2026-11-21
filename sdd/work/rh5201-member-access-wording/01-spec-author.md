Role: spec author
Commit: 9d752c2
Verdict: change doc written

Prompt carried no intended fix or verdict beyond the lead's recorded maintainer answer; nothing ignored.

## Files

- `sdd/changes/2026-11/rh5201-member-access-wording.md` — new, `Status: Draft`. Changes 1-3 (title/message, code-fix title, rule page heading and package README row) carry the maintainer's answer; change 4 (rule page body) is open.
- `sdd/specs/analyzer/RH5201.md` — spec title only: "Member-access chains should be aligned". No requirement changes.
- `documentation/rules/RH5201.md` — heading only: `# RH5201 — Member-access chains should be aligned` (BOM kept). Body left as is until change 4 is answered.

Left for the implementer (code and generated-text surfaces, not mine): `RH5201Title`/`RH5201MessageFormat` in `src/Reihitsu.Analyzer/AnalyzerResources.resx` → "Member-access chains should be aligned."; `RH5201Title` in `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` → "Align member-access chain"; the RH5201 row of `src/Reihitsu.Analyzer.Package/README.MD` → "Member-access chains should be aligned."; the `/// RH5201: Method chains should be aligned` summary of `RH5201MethodChainsShouldBeAlignedAnalyzer`. Until the README row follows, `AnalyzerPackageMetadataTests.PackageReadmeDescriptionsMatchRuleDocumentationTitles` fails against the new heading — expected at this commit.

Test text pinning the old message: `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5201CommentExemptChainWithNonInvokedPrefixDotTests.cs:159` passes "Method chains should be aligned." to `Diagnostics(...)`.

Checks: the change doc's example ran through `scripts/apply-fix.sh RH5201` (LF and CRLF): reported, fixed in one action, `.City` moved under `.Address` as written. `scripts/test.sh --project architecture` passes (198/198).

## Changed requirements

None. Wording only; the spec title changes.

## Examples for the tests

None new. The tests pin the wording: the message of an RH5201 diagnostic (e.g. on the `.` of `.City` in change 1's input) is "Member-access chains should be aligned.", and the RH5201 code action's title is "Align member-access chain". RH5201-001 to RH5201-005's examples are unchanged.

## Output questions

**RH5201 · Member-access chains should be aligned — rule page body**

Input: `documentation/rules/RH5201.md` after the answered heading change.

Before (`main`, kept by option B):
- Description: "This rule checks the layout of a method chain that spans multiple lines:"
- Why: "Misaligned method chains make it difficult to follow the sequence of operations, especially when chains are long. Consistent dot alignment provides a clear visual guide through the chain of calls."

After, A (recommended):
- Description: "This rule checks the layout of a member-access chain that spans multiple lines:"
- Why: "Misaligned member-access chains make it difficult to follow the sequence of operations, especially when chains are long. Consistent dot alignment provides a clear visual guide through the chain of member accesses."

After, B: unchanged; only the heading changes.

Why A: the page's first sentence would otherwise call the chain a "method chain" directly under a heading that calls it a member-access chain, as was done for RH5112's and RH5048's bodies.

Not blocking: no spec requirement or test depends on it. On A, the spec author (or implementer) applies the two sentences; the "first call" bullets and the examples stay.

## Internal choices

- Spec title follows the new rule title; requirement text already says "member-access chain" and is unchanged. Alternative: leave the spec title — rejected, the precedent renamed RH5112/RH5048 spec titles.
- Type names (`RH5201MethodChainsShouldBeAligned*`) stay, keeping the code-fix equivalence key; recorded in the change doc. Alternative: rename — rejected, no user sees them and #893's run edits the same files.
- FMT-INDENT-029's heading and other rules' pages (RH5035/5038/5039/5043/5044 "a later call of a method chain"; RH5115/5116 "in a wrapped method chain") are not touched: not RH5201's text. Recorded in the change doc.
- No rule-page changes beyond the heading until change 4 is answered.

## Follow-up candidates

_None._
