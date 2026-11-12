Role: spec author
Commit: 3a16754d27e1835f7977995ee5c6b9a1cbb590c6
Verdict: #946: nothing to write; #972: nothing to write; #1096: nothing to write; #1076: nothing to write; #1626: nothing to write; #1625: spec written

Architecture decision #26 (00-lead.md; 17-auditor finding #4). My prompt named the change asked for (record the RH3208 withholding in SHARED-AFTERBRACE-002); that is the lead's decision #26, not a hoped-for verdict, and I wrote it from the decision and the audit. No change doc and no rule page: a withheld fix for a rare class is not a visible output change in the normal case and is not listed on a rule page (`AGENTS.md`, *Code-fix coverage* and key conventions; `general-notes.md` covers it).

## thoenissen/Reihitsu#946, #972, #1096, #1076, #1626

Nothing to write: decision #26 concerns none of them. The SHARED-BLANK-001 disabled-text example that #26 asks to pin with a test exists already (12-spec-author); its test is the test implementer's.

## thoenissen/Reihitsu#1625

### Changed requirements

- SHARED-AFTERBRACE-002 — A statement behind a comment that holds every line break moves below it — clarified. New paragraph: inside an object initializer or anonymous object, the RH3208 fix MUST NOT be offered when the statement it would move stands in the cell SHARED-STMT-004 withholds its fix for, read with the converted local function as the statement it follows (the local function starts on a continuation line of a `case` label, and no statement of the section other than the one it would move starts its own line); the diagnostic stays. Both sides stated: offered when another statement of the section starts its own line (`break;` below) and when the local function starts on the label's first line. The cell is referenced from SHARED-STMT-004 (already in `Uses`), not restated. Why gains one sentence (no column of the section in the text; rare class; *Code-fix coverage*). New example: both cells (continuation deeper than the label, `case 1`; in the label's column, `case 2` — 17-auditor's counterexample), RH3208 reported, "no code fix is offered".
- RH3208-002 — The fix separates the new block from the next statement — clarified: the sentence that points to SHARED-AFTERBRACE-002 adds that the fix is not offered in the initializer cell that requirement withholds it for.

### Examples for the tests

- SHARED-AFTERBRACE-002 — the new "no code fix is offered" example (two sections, both cells of finding #4). The offered side is covered by the existing SHARED-AFTERBRACE-002 example (method body) and, for a section with a line-starting `break;`, by the 17-auditor probe (column 20); a test for that side is the test implementer's call.

### Output questions

_None._

## Internal choices

- The RH3208 cell is a paragraph in SHARED-AFTERBRACE-002 that references SHARED-STMT-004's cell, with the role mapping stated ("read with the converted local function as the statement it follows"), instead of a new SHARED-AFTERBRACE-003 or a restatement of the condition. Alternative: a new requirement with the condition spelled out.
- The withholding is stated only under SHARED-AFTERBRACE-002, since only that path moves the statement and needs a column; SHARED-AFTERBRACE-001 (line break outside a comment) and a gap with no line break move nothing and keep the fix. RH3208's intro paragraph listing withheld cases is left unchanged; RH3208-002 carries the pointer.
- The example's local functions are named `One` and `Two`, because two sections of one switch block share a scope and could not both declare the same name.

## Follow-up candidates

_None_ new.

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.
- Files changed: `sdd/specs/shared/blank-line-after-brace-fixes.md`, `sdd/specs/analyzer/RH3208.md`, this handover.
