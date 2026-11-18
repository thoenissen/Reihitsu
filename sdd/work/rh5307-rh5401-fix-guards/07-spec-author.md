Role: spec author
Commit: 8f17ef3
Verdict: spec written

My prompt carried the maintainer's answer to escalation 9 (B) and the task to carry it out; no intended fix, suspected cause, or hoped-for verdict beyond that.

## Answer carried out

Question 6, answer B ("B is ok"): the RH5401 fix is withheld when formatting would insert or remove a blank line behind an attribute list, decided by comparing blank-line counts. It supersedes the offered-fix part of answers 3 and 5.

- **RH5401-003** loses its rule-local exception. The fix formats the property as the formatter does in place; it is offered only where that keeps every blank line behind an attribute list as written (RH5401-004), so its result has those blank lines as written. The line break that moves a list off the property's line, or separates the lists FMT-STRUCT-028 splits one list into, is not a blank line. Examples: the comment-between example and `G`, `H`, `K` moved to RH5401-004; the attributed example gains `L` (`[Obsolete, CLSCompliant(false)]` split into two lists, fix offered); the blank-line example now holds `E` (one blank line in front of a comment) and `F` (one blank line between two lists), both offered and kept.
- **RH5401-004** gains the condition: no fix when the formatted property holds, in the gap behind one of its attribute lists, a different number of blank lines than as written. The gap runs from the `]` closing an attribute list as written to the next token (the next list's `[` or the property's first token behind its lists), comments, directives, and disabled text included. Both sides of the qualifier have examples: withheld `D`, `G` (FMT-BLANK-011 insertion), `H`, `K`, `M` (collapse, with comment, between lists, without comment); offered `E`, `F`, and `L` (a gap created by a split has no counterpart as written and holds no blank line). `Why:` names blank lines as outside the diagnostic.
- `Uses` gains FMT-STRUCT-028.
- **Change doc** (`Status: Accepted` kept): *What* rewritten for RH5401; *Visible output changes* — the attributed example now shows `A`, `B`, and `E` (one blank line in front of a comment) fixed, and a new block shows `D` and `H` as unchanged (reported, no fix, before and after); a new decision for question 6 that names the supersession; the decisions for questions 3 and 5 now state their rule and that their offered fix is superseded; rejected alternatives: the write-back (question 6) and a formatter request to keep blank lines inside a node-level target.
- Rule page `documentation/rules/RH5401.md`: unchanged. The new no-fix inputs are a corner case of a withheld fix, which `general-notes.md` covers; the page's sentence from answer 2 still holds.

Verified: `scripts/apply-fix.sh RH5401` on the current code (b64257f) under LF and CRLF. The offered examples (`A`, `B`, `E`, `F`, `L`) give exactly the spec's outputs, `L` included. The withheld examples compile and are reported; the current code still offers them a fix (the write-back), so their withholding is for the implementer. Whole-document formatting of `[Obsolete]⏎// a⏎⏎int E` and `[Obsolete]⏎// a⏎⏎// b⏎int F` inserts a blank line in front of `// a` and removes none, the earlier table (04) shows insertion and collapse only, so a per-gap count matches "insert or remove" for every observed input. `scripts/test.sh --project architecture`: 198 passed.

## Changed requirements

- RH5401-003 — The fix puts the property on one line — changed (exception removed; examples moved, `L` and `F` added)
- RH5401-004 — The fix is withheld when the property stays on several lines — changed (blank-line condition, new withheld example)

## Examples for the tests

- RH5401-003: attributed example (`A`, `B`, `L`), blank-line example (`E`, `F`).
- RH5401-004: new example `D`, `G`, `H`, `K`, `M` (reported, no fix).
- Tests written for the earlier reading (fix offered with blank lines kept or none inserted for `D`/`G`/`H`/`K`, the comment-between example) change to no-fix tests.

## Output questions

_None._ The answer states the condition generally, so it decides every case answer 3 and 5 covered: a comment between two lists, several blank lines without a comment, and between two lists.

## Internal choices

- The condition counts blank lines per gap behind an attribute list as written, not per stretch between comments inside the gap; the maintainer chose a count comparison, and no observed input has the formatter move a blank line within a gap without changing its count. Alternative: compare per stretch between comments, which would catch a move the formatter is not seen to make.
- A gap the formatter creates by splitting a list is not compared (example `L`); alternative: count it as a gap with zero blank lines as written, which gives the same result.
- The alternative "formatter keeps blank lines inside a node-level target" is recorded as rejected on the special-cases rule, not as a maintainer decision.

## Follow-up candidates

_None._
