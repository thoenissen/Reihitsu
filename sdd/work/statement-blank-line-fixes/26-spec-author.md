Role: spec author
Commit: 607f4fd
Verdict: #946: nothing to write; #972: nothing to write; #1096: nothing to write; #1076: nothing to write; #1626: nothing to write; #1625: spec written

Decision #31 (00-lead.md; 25-test-implementer, *Conflict for the lead*). My prompt named the change to write. That is the lead's recorded decision, not a hoped-for verdict, and I wrote it from the decision and the test table. No change doc and no rule page: the narrowing keeps `main`'s output for the four cells, and withholding a fix for a rare class is not on a rule page (`AGENTS.md`, *Code-fix coverage*).

## thoenissen/Reihitsu#946, #972, #1096, #1076, #1626

Nothing to write: decision #31 concerns none of them.

## thoenissen/Reihitsu#1625 (and the statement fixes that share the owner)

### Changed requirements

- SHARED-STMT-004 — The fix inserts one blank line above the statement and its comments — clarified (amends 24's wording):
  - New definition: a token starts a line when nothing stands in front of it on that line except whitespace and comments whose text starts on that same line. A line's column is the column of its first non-whitespace character, so a comment in front of the token gives the column.
  - Bullet 1: the first token of a statement of the same block, or of any section of the same switch.
  - Bullet 2: the first token of a label of any section of the same switch. The column is one level deeper.
  - Bullet 3: a label continuation of any section of the same switch. The fallback stays limited to a statement of the reported statement's own section that starts its line. That keeps decision #29's two-sections-plus-`default:` test refused.
  - New sentence: only statements of the block's or section's own list count, not nested statements.
  - Refused list: "a comment of any kind" is removed. It now reads "the rest of a comment whose text starts on an earlier line". The wrapped continuation, the nested statement, the earlier `}`, and the label continuation without an own-section fallback stay refused.
  - Why: updated.
  - Closing sentence of the comment-tail example: names the same-line block comment as the offered side.
  - New before/after example (RH5029): the `/* note */ Method(1); var y` cell and the `case 1: break; case 2: Method(1); var z` cell.
- SHARED-AFTERBRACE-002 — A statement behind a comment that holds every line break moves below it — clarified. The RH3208 paragraph now reads what starts a line as SHARED-STMT-004 defines it, with statements and labels of any section of the switch and the own-section fallback. The offered side names a label behind a comment that starts on its line. Why: "the tail of a comment". Examples unchanged.

Existing RH5029 tests (25's table), decided under the new wording:

| Test | Line start under the wording | Result |
|---|---|---|
| `…KeepsSiblingColumnWhenPrecedingLineStartsWithComment` | `/* note */` starts on the line, then `Method(1);`, a statement of the section | offered, the line's column |
| `…WhenCommentPrecedesTheLabelOnItsLine` | `/* note */` on the line, then `case 0:` | offered, one level deeper |
| `…WhenLineStartsWithAnotherSectionsLabel` | `case 1:`, a label of another section | offered, one level deeper |
| `…WhenLineStartsWithAnEarlierSiblingSectionsStatement` | `break;` of another section | offered, the line's column |

Every new refusal test of 25 stays refused:
- Comment tail (`y */`): cell a, cell d, and the RH5002 spec example.
- Wrapped argument (`1);`): cell b and the RH3208 spec example.
- Nested unbraced statement: cell c.
- Earlier block's `}`.
- Label continuations with only `default:`'s `return;` starting a line, which is another section: the decision #29 tests.

The three RH3208 holding tests and the three RH5029 holding tests keep their fix.

### Examples for the tests

- SHARED-STMT-004: the new before/after RH5029 example covers two diagnostics in one document. The four existing tests already cover both cells. The test implementer may add the example verbatim or point the existing tests at it.

### Output questions

_None._ The narrowing keeps `main`'s output for the four cells (decision #31).

## Internal choices

- "Starts its line" in the label-continuation fallback uses the same definition (comments starting on that line allowed). The alternative was whitespace-only there. One definition for the whole paragraph is simpler, and no test distinguishes the two.
- The fallback stays with the reported statement's own section, while the line starter may belong to any section. Widening the fallback too would offer the fix in decision #29's refused test.
- A label continuation of another section's label counts as a continuation line (decision #31: labels of any section count). No test exercises it.

## Follow-up candidates

_None._

## Whole run

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 51 passed.
- Files changed: `sdd/specs/shared/statement-blank-lines.md`, `sdd/specs/shared/blank-line-after-brace-fixes.md`, this handover.
