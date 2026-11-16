Role: spec author
Commit: 300f6a6
Verdict: change doc written

Follow-up round: record the maintainer's batch-1 answers (1A, 2A, 3A, 4A) and the skeptic's change-doc amendment. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Status: both change docs are set to `Accepted`. `sdd/README.md` (*Change docs*) prescribes `Accepted` once every visible output change has the maintainer's answer and assigns setting it to the lead in a squad run; I set it on the lead's instruction, and every listed change, the two amendments included (decision row 4 of `00-lead.md`), is answered.

## thoenissen/Reihitsu#1762

File: `sdd/changes/2026-11/union-declaration-layout.md`.

- Status `Draft` → `Accepted`.
- *Visible output changes*: added change 7, the RH5404 code-fix result for `class C { union U(int, string) { public int X => 1; } }` (before: union stays on one line inside the laid-out class; after: union laid out as in change 1), as a consequence of change 1 (skeptic's 1762-A).
- *Decisions*: replaced "Pending maintainer answers" with three decisions — wrapped case-type list keeps the primary-constructor layout (Q1 A); empty union body becomes `;` (Q2 A); a union gets a class's layout and rewrites, RH7101 fix offered, and fixes that format through the formatter (RH5404, change 7) follow (Q3 A).
- Spec check: FMT-BREAK-002, -007, -011 (aligned wrapped example), -023, FMT-STRUCT-017 (union from C# 15), FMT-STRUCT-021, FMT-PIPE-031, RH7101-002 Why match the answers. No spec change; specs are unchanged since 2272e74.

## thoenissen/Reihitsu#1763

File: `sdd/changes/2026-11/with-element-spacing.md`.

- Status `Draft` → `Accepted`.
- *Visible output changes*: added change 3, the RH5305 code-fix result for `[with (4),⏎    "a"]` (before: `with (4)` on its own line; after: `with(4)`), as a consequence of change 1 (skeptic's 1763-A).
- *Decisions*: replaced "Pending maintainer answer" with "No space between `with` and `(`" — Q4 A, `with()` included, RH5305 result follows.
- Spec check: FMT-SPACE-028 (incl. `with()` and the comment case) and FMT-SPACE-002's order match the answer; FMT-BREAK-022 unchanged. No spec change.

## Changed requirements
_None_ in this round (change docs only).

## Examples for the tests
Unchanged from `01-spec-author.md`; in addition the two amended change-doc examples: RH5404 fix on a nested union (#1762, change 7) and RH5305 fix on `with (4)` (#1763, change 3).

## Output questions
_None._

## Internal choices
- Set `Accepted` myself on the lead's instruction rather than leaving it to the lead; alternative: leave `Draft` for the lead to flip.

## Follow-up candidates
_None_ new.
