Role: spec author
Commit: 8d529bc
Verdict: #1695: spec written; #1696: change doc written; #1707: nothing to do; #1721: nothing to do

Applied the maintainer's answers (Q1 = B, Q2 = B; `00-lead.md`, Decisions 2 and 8) and the skeptic's wording note (a) from `02-skeptic.md`.

## thoenissen/Reihitsu#1695

### Changed requirements

- FMT-PIPE-010 — Node-level entry points format only their target — changed (Q1 = B) and clarified (line ending, ec7c0c6):
  - Note (a) applied: the line-ending sentence now reads "When that trivia ends in a line break, both MUST write that line break as the line ending FMT-PIPE-007 detects …". A trailing trivia without a line break, where code follows the target on its line (skeptic's T7), is kept as it stands.
  - New: both entry points MUST remove the whitespace directly in front of that line break, as FMT-CLEAN-001 removes it in document formatting. This holds behind the result's last token and behind a `/* */` comment in the trivia (skeptic's W1, W4), and whether or not formatting changed the target's last line (W5).
  - Unchanged: whitespace in front of a comment stays (W2), and whitespace inside a `//` comment's text stays (W3, FMT-PIPE-002).
  - Note (b) applied: the redundant sentence "A trailing trivia whose line break already is the detected line ending is written back as it stands" is gone.
  - New `Why:` sentence and a new example: RH3202-shaped `GetValue` with `/* c */···`, with prose for no comment, a kept last token (property `P`), and `// c`.
- The whitespace half is listed in the change doc because it changes FMT-PIPE-010's "keep the trailing trivia" for a target whose last line formatting leaves unchanged. No other requirement decided that case.

### Examples for the tests

FMT-PIPE-010:
- the CRLF/LF property `P` example and its lone-`\r` prose (from ec7c0c6);
- the new whitespace example: W1, W4, W5 removed; W2, W3 kept; T7 untouched.

Cover both entry points: `FormatNodeInDocumentWithContextAsync` with an RH5111-style target that ends its line in whitespace.

### Output questions

_None._ Q1 is answered.

### Internal choices

- Q1 is recorded in the same change doc as #1696, as Decision 8 asks. The rule allows one doc for issues that share a cause, and here the shared cause is fix output at a line end not brought to what the formatter writes. Alternative: a separate change doc for FMT-PIPE-010.

### Follow-up candidates

_None._ The leading-trivia candidate was dropped by the lead (Decision 5).

## thoenissen/Reihitsu#1696

### Changed requirements

- RH8402-002 — The expected header is the configured text with its placeholders filled in — changed. The first-line-ending sentence now applies "for the analyzer's comparison" only. The header the fix writes takes RH8402-005's line ending. The analyzer's behavior is unchanged.
- RH8402-005 — The fix writes the header at the start of the file — changed. Every line break of the header the fix writes, and the separator behind it, MUST be the line ending FMT-PIPE-007 detects (SHARED-FIX-008). That is `\n` for an all-lone-`\r` file and `Environment.NewLine` for a file without a line break.
  - Extended `Why:` (the mixed-file flip; the analyzer still matches after the fix).
  - New example: a mixed file with an LF first line and CRLF elsewhere, plus prose for the lone-`\r` and one-line files.
- `Uses` gains SHARED-FIX-008 and FMT-PIPE-007.
- RH8402-006 still refers to RH8402-005 for the separator. Its header now follows RH8402-005's line ending through "the header the fix writes".
- Rule page `documentation/rules/RH8402.md`: unchanged. It carries no line-ending text, and the normal case does not change for the user.

### Change doc

`sdd/changes/2026-11/fix-line-end-as-formatter-writes.md`, `Status: Draft`.
- It covers RH8402-002/005 and the FMT-PIPE-010 whitespace change.
- `Decisions` records both maintainer answers.
- `Visible output changes` shows both cases as input, before, and after.
- The lead sets `Accepted`.

### Examples for the tests

RH8402-005: the new mixed-file example, the all-lone-`\r` file (header in `\n`), and the one-line file (`Environment.NewLine`). These go through `ApplyCodeFixAsync` on exact text, as the skeptic notes; `apply-fix.sh` normalizes line endings. RH8402-002's existing examples stay as the analyzer's.

### Output questions

_None._ Q2 is answered.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1707

Nothing for the spec author. SHARED-FIX-006, RH3003-004, and RH3005-004 decide it; Decision 7 narrows the issue to RH3003.

## thoenissen/Reihitsu#1721

Nothing for the spec author. FMT-PIPE-007 and FMT-EOL-004 decide it (Decision 6).

## Whole run

- Lane: behavior change (Decision 8).
- Files:
  - `sdd/specs/formatter/pipeline.md`
  - `sdd/specs/analyzer/RH8402.md`
  - `sdd/changes/2026-11/fix-line-end-as-formatter-writes.md`
- No new requirement IDs; `requirement-ids.txt` is unchanged.
