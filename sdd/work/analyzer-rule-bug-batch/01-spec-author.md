Role: spec author
Commit: aee16ec
Verdict: spec written

No change doc: every issue is a bug fix; the spec edits are clarifications (#1079, #1276, #1613, #1708) or the alignment of RH5601-001/RH5602-001 (and RH6009-001/RH6010-001, whose "outside interpolated strings" wording states the same exemption) to SHARED-DIAG-003 that the #1074 triage comment orders as a clarification in the same PR. No rule page changes (see each section).

## thoenissen/Reihitsu#1074

### Changed requirements

- RH5601-001 — Every tab in the code is reported — changed (aligned to SHARED-DIAG-003): the conditions "the last code token before it is not a string or character literal", "no `/** */` documentation comment stands between", and "the next code token after it is not a literal" are removed. Example gains `={|→|}"abc"`, a tab starting the line in front of a literal, `/** c */{|→|}"abc"`, a tab in front of a char literal, and tabs inside an interpolation hole.
- RH5602-001 — Whitespace before a line break is reported — changed (aligned to SHARED-DIAG-003): "no string literal touches the line" is replaced by "the run is not part of the text of a literal"; a literal's last character and a token in a hole count as code tokens; a whitespace-only line in a hole is reported. Example gains plain, interpolated, verbatim, raw, and multi-line-verbatim endings and a hole line, with the in-literal whitespace on each side of the qualifier. `Uses` gains SHARED-DIAG-003.
- RH6009-001 / RH6010-001 — glued brace reported — changed: "outside interpolated strings" becomes "other than the braces of an interpolation (RH600x-002)", plus one sentence that a brace inside a hole is code. Examples in -001 and -004 gain `$"{new int[]{1 }.Length}"` / `$"{new int[] { 1}.Length}"`. `Uses` gains SHARED-DIAG-003.
- RH5601-002, RH5602-002, RH6009-002, RH6010-002 — unchanged; they are the other side of each qualifier.

### Examples for the tests

RH5601-001, RH5602-001, RH6009-001, RH6009-004, RH6010-001, RH6010-004.

### Rule pages

Unchanged: RH5601/RH5602 pages exempt only the inside of literals and comments; RH6009/RH6010 pages exempt only the interpolation's own braces. Both remain true.

### Output questions

_None._ The outputs follow SHARED-DIAG-003 and the maintainer's P5 decision quoted in the issue.

### Internal choices

- Rewrote the existing -001 requirements instead of adding new IDs, because the old text states exactly the exemption SHARED-DIAG-003 removes; per the triage comment this is a clarification without a change doc (alternative: retire and re-add — rejected, it would orphan the existing test tags).

## thoenissen/Reihitsu#1079

Scope: only the RH5603 part (triage split; the RH5601 `///`, RH6004/RH7302, RH6005, RH6021 parts are #1444).

### Changed requirements

- RH5603-001 — Line breaks at the end of the file are reported — clarified: the condition is on the last non-whitespace character before the line breaks, and whitespace-only lines between the trailing line breaks belong to them. Two new examples, one on each side: `}⏎···⏎` (tail includes the space line) and `}··⏎` (the spaces behind `}` are RH5602's, not in the tail).
- RH5603-003 — One diagnostic on the trailing line breaks — clarified: the span runs from the first line break after the last non-whitespace character to the end of the file.
- RH5603-004 — The fix deletes the trailing line breaks — clarified: one application deletes the whole tail (SHARED-FIX-001, as FMT-CLEAN-005); whitespace behind the last character on its line stays. Examples `}⏎···⏎` → `}` and `}··⏎` → `}··`.
- Non-goal for RH5602 sharpened; `Uses` gains SHARED-FIX-001; `·` marker declared.

### Examples for the tests

RH5603-001, RH5603-003, RH5603-004.

### Notes for the skeptic

- The issue comment says the fix leaves `}⏎···`, "which RH5603 reports again". Per `RH5603FileMustNotEndWithANewlineAnalyzer`, a text ending in spaces is not reported by RH5603 (RH5602 reports the last line). The defect that reproduces is the fix result (`}⏎···` instead of `}`), i.e. the analyzer's span covers only the last `⏎`. The spec is written on the maintainer's decision (whole tail in one application), not on the "reported again" claim.
- A file ending in `}⏎···` without a final line break stays RH5602's; RH5603 does not report it (current behavior, written as a non-goal).

### Rule page

Unchanged: it already says "Delete the line break — and any blank lines — after the last source character".

### Output questions

_None._ The fix result was decided by the maintainer (issue comment of 2026-10-18).

### Internal choices

- Whitespace behind the last code character on its line (`}··⏎`) is left out of the RH5603 span, keeping the current split with RH5602 (alternative: include it, so the RH5603 fix also deletes `··` — rejected, it would make RH5603 delete what RH5602 owns and change the result for an input the decision did not cover).

## thoenissen/Reihitsu#1276

Scope: only the location bug (triage split; the P11 widening and the formatter's extension-block braces are #1446).

### Changed requirements

- RH5404-001 — A type body on one line is reported — clarified: extension blocks are listed (the analyzer already registers `ExtensionBlockDeclaration`); example gains a one-line and a multi-line extension block.
- RH5404-002 — The diagnostic is on the type's name — clarified: an extension block is reported on its `extension` keyword (SHARED-DIAG-007). Example gains `static class {|X|} { {|extension|}(int value) { } }`.
- RH5404-003 — The fix lays out the declaration — clarified: no code fix is offered for an extension block; the fix of a containing type is still offered. Example: one-line extension block, "no code fix is offered".
- Non-goal added pointing at #1446; `Uses` gains SHARED-DIAG-007.

### Examples for the tests

RH5404-001, RH5404-002, RH5404-003.

### Rule page

Unchanged: it already lists extension blocks and says the fix does not apply to them ("except for extension blocks, which have to be expanded by hand").

### Output questions

_None._ See the internal choice below.

### Internal choices

- The lead's note says "the fix must act on the extension block". I wrote the fix as **withheld** for an extension block instead, and I recommend that reading: the rule page already promises no fix for extension blocks, and the formatter does not place extension-block braces on their own lines (that is #1446), so a fix that formats the block in place would leave its own diagnostic (SHARED-FIX-001). Today's fix, offered on a `Location.None` diagnostic, formats the type at the file's first token — a bug, not a reading. Alternative: offer a fix that formats the extension block — rejected, it cannot clear the diagnostic until #1446 ships. When #1446 lands, RH5404-003 changes with it.

## thoenissen/Reihitsu#1613

### Changed requirements

- RH5202-001 — A misaligned multi-line raw string literal is reported — clarified: the closing line is the text after the literal's last line break, the opening column is counted from the line break before the opening quotes, a line break is every C# line terminator (`\r\n`, `\n`, lone `\r`, U+0085, U+2028, U+2029), and the analyzer and the fix measure the same way. New example in a lone-`\r` document (marked `⏎`): an aligned plain literal (not reported) and a misaligned interpolated one (reported).
- RH5202-003 — The fix shifts the lines — clarified by a new example: the misaligned interpolated literal in a lone-`\r` document is aligned in one application (column 17), and the literal's lone `\r` line breaks stay (SHARED-FIX-008). `Uses` gains SHARED-FIX-008.

### Examples for the tests

RH5202-001 (lone-`\r` example), RH5202-003 (lone-`\r` example). The issue's Done-when also asks for `u8` and U+2028/U+2029/U+0085 variants; the requirement text covers them, the test implementer derives them from it.

### Notes for the skeptic

- The issue's "already aligned" input `    var a = """⏎        Hello⏎        """;` is in fact misaligned: the opening quotes are in column 12, the closing quotes in column 8. The spec example uses a genuinely aligned literal (column 16).

### Rule page

Unchanged: the page states the alignment rule, not line-break handling.

### Output questions

_None._

### Internal choices

_None._

## thoenissen/Reihitsu#1708

### Changed requirements

- RH4105-001 — Parameter names are in camelCase — clarified: the receiver parameter of an extension block is listed (already reported and fixed; existing tests cover it); example gains `extension(int {|RH4105:Value|})`.
- RH4105-004 — A parameter without a name is not reported — added (clarification of the SHARED-DIAG-007 bug): `extension(long)`, `extension<T>(T[])`, `extension(int)` are not reported. Registered in `sdd/specs/requirement-ids.txt`; needs a covering test in this PR. `Uses` gains SHARED-DIAG-007.

### Examples for the tests

RH4105-001 (extension receiver line), RH4105-004.

### Rule page

Unchanged: an unnamed receiver is a corner case.

### Output questions

_None._

### Internal choices

- New ID RH4105-004 rather than folding the case into RH4105-002 ("Discards and record parameters are not checked"), whose title would no longer fit (alternative: widen -002 and retitle it).

## Follow-up candidates

_None._

## Amendment

Commit: 1ec15b8 (after the skeptic's handover `02-skeptic.md`).

- **#1276 — RH5404-003 `Why:` reworded.** The old sentence said formatting an extension block "would leave the diagnostic in place". That holds only for an empty block. For a non-empty block, formatting clears RH5404 but leaves the opening brace on the header line and the closing brace on the last member's line. The `Why:` now gives both outcomes: an empty block stays reported (SHARED-FIX-001), and any other block gets a layout that is not the expanded body the rule asks for. The requirement itself is unchanged: no fix is offered for an extension block.
- **#1613 — placement of examples checked, no change.** The aligned literal that is not reported is already in the RH5202-001 example, and the one-application fix example is in RH5202-003. Tests for "aligned, not reported" carry RH5202-001; only the fix tests carry RH5202-003.
