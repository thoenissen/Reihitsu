Role: spec author
Commit: eefadaa8d92d0f2cc13b2174c2d20be5b1815922
Verdict: #946: nothing to write; #972: nothing to write; #1096: nothing to write; #1076: nothing to write; #1626: nothing to write; #1625: spec written

Architecture decision #28 (00-lead.md; 23-auditor finding #5). My prompt named the change asked for (widen the withheld cell into the refused class of decision #28). That is the lead's decision, not a hoped-for verdict, and I wrote it from the decision and the audit. No change doc and no rule page: a fix withheld for a rare class is not a visible output change in the normal case and is not listed on a rule page (`AGENTS.md`, *Code-fix coverage*; `general-notes.md` covers it).

## thoenissen/Reihitsu#946, #972, #1096, #1076, #1626

Nothing to write: decision #28 concerns none of them.

## thoenissen/Reihitsu#1625 (and the statement fixes that share the owner)

### Changed requirements

- SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments — clarified. The narrow paragraph (label continuation without another line-starting statement) is replaced. Inside an object initializer or anonymous object, the column is read from the line on which the statement it follows starts, and only when one of these starts that line with nothing but whitespace in front of it: the first token of a statement of the same block or switch section (that line's column); the first token of a label of its switch section (one level deeper); a token inside a label of its switch section behind its first token, a continuation line (the column of another line-starting statement of the section, such as `break;`). In every other case the fix is not offered for a statement it would move and the diagnostic stays — named examples: a comment's last line, any comment, a wrapped continuation, a nested statement, an earlier block's `}`, and a label continuation with no other line-starting statement. Why rewritten for the class. Both existing initializer examples stay valid (the `break;` column; the two label-continuation cells). New example: RH5002, the preceding `value++;` starts on the last line of a block comment, no code fix is offered; a closing sentence names the other side (same line started by `value++;` → offered).
- SHARED-AFTERBRACE-002 — A statement behind a comment that holds every line break moves below it — clarified. The RH3208 paragraph now references every case SHARED-STMT-004 withholds, read with the converted local function as the statement it follows, and lists the offered side (line started by a statement of the list such as the local function itself, by the label, or a continuation with a line-starting `break;` below). Why updated. The existing label-continuation example stays. New example: the local function starts on the continuation line of a wrapped argument list (`Add(value,⏎ 1); int M() => 1; /* a⏎ b */ M();`, 23-auditor counterexample (b)), no code fix is offered.
- RH3208-002 — pointer reworded from "the initializer cell" to "the initializer cases that requirement withholds it for".

### Examples for the tests

- SHARED-STMT-004 — the new comment-led RH5002 example (statement base caller).
- SHARED-AFTERBRACE-002 — the new continuation-led RH3208 example.
The audit asked for one comment-led and one continuation-led cell per caller; the other two cells (RH5002 continuation-led `Add(value,⏎ 1); a++; …`, RH3208 comment-led counterexample (a)) are the test implementer's to add from 23-auditor; they are not separate spec examples.

### Output questions

_None._ Withholding the fix for this rare class is the rule's local decision (decision #28).

## Internal choices

- "A label of its switch section" is taken literally from decision #28: a line started by a label of another section of the same switch, or by a comment in front of a label (`/* c */ case 1: a++; b++;`), is refused. Today's owner (`SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion`) accepts any section's label and the label's leading trivia, so these two rare cells change from offered to withheld. Alternative: "a label of the switch statement, or a comment in front of one" — keeps those cells offered but also keeps the multi-line comment tail in front of a label (`/* a⏎ b */ case 1: a++; b++;`), a member of the very class #28 refuses. The implementer should follow the spec as written or the lead should amend it.
- "Starts the line" is defined as "nothing but whitespace in front of it on the line", so a comment in front of a statement's first token refuses too; this states both sides of the qualifier (FC-002).
- The continuation-line fallback stays limited to label continuations, as today; other refused leaders get no fallback to another line-starting statement, per decision #28 and 23-auditor.
- The `}`-led cell (23-auditor: holds today) is refused, which the audit names acceptable and decision #28's wording implies.

## Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.
- Files changed: `sdd/specs/shared/statement-blank-lines.md`, `sdd/specs/shared/blank-line-after-brace-fixes.md`, `sdd/specs/analyzer/RH3208.md`, this handover.
