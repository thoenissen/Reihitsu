Role: spec author
Commit: 4f8602a
Verdict: change doc written

Inputs: thoenissen/Reihitsu#1925 and its one comment (maintainer, 2026-11-22: option A — report the literal aligned in characters behind tabs, fix writes the FMT-RAW-010 layout; the run decides whether the message needs new wording), `00-lead.md`, `sdd/specs/analyzer/RH5202.md`, `sdd/specs/formatter/raw-string-alignment.md` (FMT-RAW-001, FMT-RAW-010), `sdd/specs/analyzer/RH5601.md` (RH5601-002), `sdd/changes/2026-11/raw-string-tab-indentation.md`, `documentation/rules/RH5202.md`, the RH5202 resource entries. The prompt carried no intended fix or cause.

Files written:

- `sdd/changes/2026-11/rh5202-tab-aligned-raw-strings.md` — change doc, `Status: Draft`, four visible output changes (three changed, one unchanged control case).
- `sdd/specs/analyzer/RH5202.md` — spec diff below; `Uses` gains SHARED-DIAG-003 (referenced in RH5202-009's Why).
- `sdd/specs/requirement-ids.txt` — RH5202-009.

## Changed requirements

- RH5202-009 — A literal aligned behind tabs is reported — added. Condition: closing quotes in the column of the opening quotes (RH5202-001's measure, each whitespace character one column) and the whitespace in front of the closing quotes holds a tab (U+0009). Covers plain, `u8`, interpolated, and a literal nested in a hole at any depth (reported on its own). The fix writes the RH5202-007 spaces with a zero shift; RH5202-008 still withholds an interpolated literal's fix when a nested literal's closing whitespace holds a tab.
- RH5202-001 — the "MUST NOT be reported" sentence gains "unless the whitespace in front of them holds a tab (RH5202-009)" — changed.
- RH5202-003 — one clause: for a literal RH5202-009 reports the difference is zero and the fix writes only the RH5202-007 spaces — clarified.
- RH5202-007 — the paragraph "The analyzer reports the same literals as before … not reported, and no fix is offered" is replaced by the zero-shift statement pointing at RH5202-009 — changed.
- RH5202-008 — the condition also covers an outer literal that does not shift (RH5202-009); its Why no longer says the tab-aligned nested literal is unreported; its first example now marks the nested literal `{|RH5202:…|}` and shows the nested fix's result — changed.
- Spec intro paragraph updated to name the new case.

Rule page `documentation/rules/RH5202.md`: left unchanged. What is reported changes only for a literal aligned in characters behind tabs, which in an editor rendering a tab wider than one column does not stand under its opening quotes, so the page's "aligned to the same column" already describes it from the user's side; the issue also proposed keeping the page as short as it is.

Resource texts: unchanged (see Internal choices).

Checks run:

- `scripts/apply-fix.sh RH5202` on `main`-equivalent code (branch at 4f8602a, no production change) over four fixtures — the RH5202-009 examples 1–3 and the RH5202-008 first example: `a_aligned` (example 1) no-diagnostic, `b_interp` (example 2) no-diagnostic, `d_notab` (example 3, the negative case) no-diagnostic, `c_nested` (RH5202-008 first example) no-fix-offered — under LF and CRLF. This is the "before" in the change doc.
- `scripts/format.sh` on copies of the same fixtures: the formatter writes exactly the "after" outputs of RH5202-009 examples 1 and 2 and the final output of the RH5202-008 first example (outer moved 13 columns, nested indentation as spaces); example 3 keeps its tab. The intermediate result of the RH5202-008 first example (nested fixed, outer not yet) is derived by hand: the nested lines become 11 spaces, the same column.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered`: "RH5202-009: no test covers it". Expected until the test implementer tags a test with `[Requirement("RH5202-009")]`; no other spec-structure test fails.

## Examples for the tests

- RH5202-009 — all four examples: plain literal reported and fixed with zero shift; interpolated literal reported, whitespace behind the width (spaces and tabs) kept; no tab in front of the closing quotes but a tab on a content line → not reported; nested-literal case via RH5202-008.
- RH5202-008 — the first example changed: nested literal now reported, outer fix withheld, nested fix's result, then the outer fix as in the second example.
- RH5202-001 — the qualifier "in the column … unless a tab" has its two sides in RH5202-009 examples 1 and 3.
- RH5202-007 — the existing example is unchanged; existing tests that assert a tab-aligned literal is *not* reported (if any) now contradict RH5202-009 and RH5202-008 and need their expectation changed. I did not search the tests (outside my lane); the test implementer should search the RH5202 test class for the RH5202-007/008 "not reported" cases.

## Output questions

_None._ The maintainer's answer (option A) covers every changed output in the change doc: report the literal, fix writes the FMT-RAW-010 layout. The nested case (change-doc item 3) is the consequence the issue itself listed under *Additional context* and that the maintainer accepted with A. The lead still turns the change doc's visible output changes into its accept step as usual.

## Internal choices

- The diagnostic message, the diagnostic title, and the code-fix title stay unchanged (the maintainer left the message to the run). The message "Raw string literals should be formatted correctly." names neither a column nor alignment and covers tab indentation; `AnalyzerPackageMetadataTests` pins this message against the title, so a different wording would also split title and message. Alternative: a message naming the tabs — rejected in the change doc. Keeping the texts puts no visible output in question.
- The report condition is the tab in front of the closing quotes, not a tab anywhere in the stripped width of the content lines — the formatter's own condition in FMT-RAW-010, so analyzer, fix, and formatter share one condition. Alternative (content-line tabs) rejected in the change doc; it is also the only reading consistent with option A's "the layout the formatter writes".
- New requirement RH5202-009 rather than widening RH5202-001's text — keeps RH5202-001 about misalignment and gives the new case its own covering test.
- Change doc dated `2026-11` (month of the run); the lead moves it if the merge lands in December.

## Follow-up candidates

_None._
