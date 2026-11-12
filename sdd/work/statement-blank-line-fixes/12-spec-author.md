Role: spec author
Commit: 2f6a269
Verdict: #946: nothing to write; #972: nothing to write; #1096: spec written; #1076: spec written; #1626: nothing to write; #1625: nothing to write

Repair round 1 (00-lead.md decisions #23, #24; 11-auditor findings #1, #2, #3). My prompt carried no intended fix, suspected cause, or hoped-for verdict. No change doc and no rule page: nothing a user sees in the normal case changes, and a withheld fix for a rare class is not listed on a rule page (`AGENTS.md`, key conventions; `general-notes.md` covers it).

## thoenissen/Reihitsu#946

Nothing to write: no finding of this round concerns it.

## thoenissen/Reihitsu#972

Nothing to write: no finding of this round concerns it.

## thoenissen/Reihitsu#1096

### Changed requirements

- SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments — clarified (decision #23, finding #1). New paragraph: inside an object initializer or anonymous object, the fix MUST NOT be offered for a statement it would move when the statement it follows starts on a continuation line of a `case` label (a line inside the label that starts behind its first token) and no statement of the section other than the reported one starts its own line; the diagnostic stays. Both sides stated: offered when another statement of the section starts its own line (the existing `break;` example) and when the statement it follows starts on the label's first line. The Why gains the reason (no column of the section in the text; rare class, an RH5103 violation; *Code-fix coverage*). New example with both cells of the class: continuation one level deeper than the label (`case 1`) and in the label's column (`case 2`), RH5029, "no code fix is offered". This is the place for the gap: the 20 rules take their fix from the shared base, and SHARED-STMT-004 is the requirement each of their specs cites; no rule spec states this column.
- SHARED-BLANK-001 — A blank line holds nothing but whitespace — clarified (finding #3). A line inside disabled text (an inactive `#if`/`#elif`/`#else` branch) MUST NOT count, like a line inside a comment's text; the sentence about the line below a directive now excludes that line when it is itself disabled text, as the first line of an inactive branch is. Why extended by one sentence. New example (RH5002): `a++;⏎#if NEVER_DEFINED⏎⏎#endif⏎// note⏎if (…)` is reported, opposite the existing `#endif⏎⏎// note` example that is not.

### Examples for the tests

- SHARED-STMT-004 — the new "no code fix is offered" example (two sections, both cells of finding #1). The test implementer's tests for both absent cells derive from it.
- SHARED-BLANK-001 — the new disabled-text example (RH5002 reported). It is `main`'s output and the auditor's fixture; a characterization test may be tagged with it (test implementer's call).

### Output questions

_None._

## thoenissen/Reihitsu#1076

### Changed requirements

- SHARED-WS-001 — retitled "The fix deletes exactly the reported whitespace or the run its rule names" — clarified (finding #2). A rule that reports one line of a run of whitespace-only lines MAY name the whole run as its deleted span — the reported line and the whitespace-only lines next to it on the side its spec names, up to the nearest line that holds anything else; that run then takes the place of the reported span in every sentence of the requirement. A rule that names no run deletes the reported span only. Why gives RH5022 and RH5024–RH5027 as the case and SHARED-FIX-001 as the reason, and says the run stays whitespace-only. New one-line example that points to RH5022 (run) and RH5023 (no run). `Uses` gains SHARED-FIX-001 (`UsesListsExactlyTheReferencedRequirements`). The rule specs RH5022-004 and RH5024-…–RH5027 requirements already say "as SHARED-WS-001 specifies for the span of that run" and now match it; they are unchanged.

### Output questions

_None._

## thoenissen/Reihitsu#1626

Nothing to write. The auditor's "one owner" hint on the `BlankLineCodeFixUtilities` summary is source text, the implementer's (decision #24); SHARED-STMT names no such claim.

## thoenissen/Reihitsu#1625

Nothing to write: no finding of this round concerns it.

## Internal choices

- The SHARED-STMT-004 gap is written as a paragraph in that requirement, not as a new SHARED-STMT requirement: decision #23 names SHARED-STMT-004, and the condition only narrows the column rule the same requirement states. Alternative: a new SHARED-STMT-005 "fix withheld after a label continuation inside an initializer".
- SHARED-WS-001 references the rules (RH5022, RH5023) by rule ID, not by requirement ID, so the shared spec does not list rule requirements under `Uses`. Alternative: cite RH5022-004/RH5023-004 and add them to `Uses`, which makes a shared spec depend on rule specs.

## Follow-up candidates

_None_ new.

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.
- Files changed: `sdd/specs/shared/statement-blank-lines.md`, `sdd/specs/shared/blank-line-spacing.md`, `sdd/specs/shared/whitespace-removal-fixes.md`, this handover.
