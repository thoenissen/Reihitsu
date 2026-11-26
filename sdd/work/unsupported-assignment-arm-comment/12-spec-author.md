Role: spec author
Commit: 71519cb
Verdict: spec written

Merge of `origin/main` (eed7195) into the branch, base 1788f94, completed as 71519cb. The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `03-spec-author.md`, `05-spec-author.md`, and `08-spec-author.md`.

## Changed requirements

None. Only the two conflicting `Uses` rows were resolved; no requirement text changed.

- `sdd/specs/formatter/blank-lines.md` `Uses` — union: main's added FMT-INDENT-025 and this run's FMT-UNSUP-020.
- `sdd/specs/formatter/line-breaks.md` `Uses` — union: main's added FMT-PIPE-008, FMT-SPACE-009, FMT-STRUCT-001, FMT-SWITCH-017, and this run's FMT-UNSUP-020 and FMT-UNSUP-021 in place of the retired FMT-UNSUP-019, which main still listed.
- `sdd/specs/requirement-ids.txt` merged cleanly: it has main's seven new IDs and FMT-UNSUP-020, 021, and 022.

## Main's specs under FMT-UNSUP-020/021/022

How I checked:
- I extracted every `csharp` block (643) from the merged working-tree versions of all 13 spec and change-doc files main changed. That covers main's new FMT-BLANK, FMT-BREAK, FMT-SWITCH, playground, RH4120, and casing examples, plus the three new change docs.
- I ran all of them through this branch's formatter: `scripts/format.sh <dir> --dry-run`.
- No block is refused with the reason of FMT-UNSUP-020, 021, or 022.
- Three blocks are refused, each intentionally and under an older shape:
  - `sdd/specs/playground/playground.md` PLAY-008, and the same input in `sdd/changes/2026-11/playground-highlighting.md` section 4: FMT-UNSUP-004 (`int /* c */ a = 1;`), the example of a refused input.
  - `sdd/specs/formatter/line-breaks.md` around line 3651: FMT-UNSUP-014, an example stated as refused.

Main's version of `blank-lines.md` and `line-breaks.md`, run the same way, had eight blocks refused under FMT-UNSUP-020. All eight are base text (1788f94) that this run had already rewritten in FMT-BLANK-018, FMT-BLANK-020, and FMT-BREAK-030, and the merge kept the rewritten versions. I checked each block's text against the base.

Other checks:
- A grep of main's added spec and change-doc prose for "behind the `=`", "switch arm", "assignment operator", and "initializer" finds no sentence that depends on a join or a layout across a comment in the refused gaps.
- A grep of main's added test lines for a comment or directive behind `=`, a compound assignment operator, or `=>` finds none.
- No reference to FMT-UNSUP-019 remains outside its retired heading, `requirement-ids.txt`, the change docs, and a squad note.
- `scripts/test.sh --project architecture` on the merged tree: 198 passed, 0 failed. The Release build of the solution in that run succeeded.

## Examples for the tests

_None._ No example changed.

## Output questions

_None._

## Internal choices

- Resolved both `Uses` rows as the union of both sides. Main's FMT-UNSUP-019 entry was dropped from `line-breaks.md`, because this run retires that requirement and replaces it with FMT-UNSUP-020 and FMT-UNSUP-021. Alternative: keep main's row, which would reference a retired requirement.

## Follow-up candidates

_None._

Not checked here: main's production changes. They touch `LineBreakPhase`, `BlankLinePhase`, `SwitchCaseBraceRewriter`, and others, and I did not check them against the special cases this run deleted. That belongs to the auditor and the validator.
