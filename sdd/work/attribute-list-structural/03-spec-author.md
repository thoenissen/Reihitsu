Role: spec author
Commit: 4add787
Verdict: spec written

Amendment round. My prompt named the changes to make, decisions 4–7 and maintainer answer 1 of `00-lead.md`, which are recorded decisions, not a suggested fix; I worked from `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, and the specs. Outputs quoted as `main` were measured on this branch (no production change yet) with `scripts/format.sh` in the session scratchpad.

## Changed requirements

- FMT-STRUCT-028 — A declaration's attribute list holds one attribute — changed (skeptic amendment 1, maintainer answer 1): a created list holds none of the whitespace, line breaks, and blank lines that separated two attributes; they are dropped with the comma. `[A,⏎ B]`, `[A⏎ , B]`, `[A,⏎⏎ B]` all give `[A]⏎[B]` in one pass. Whitespace behind the original `[` and in front of the original `]` (`[A, B⏎]`) is outside the requirement, so follow-up 4 of `00-lead.md` stays open. Second example added; Why extended.
- FMT-STRUCT-029 — The lists of one target on a parameter are merged — changed (skeptic amendment 2, decision 5): the merge keeps one line break when the author wrote one between the `]` of the first list and the token behind the last list, may drop the rest, and keeps none when there was none. Example added (`P`, `Q` wrapped; `R` not). Why extended.
- FMT-BREAK-032 — The attribute lists of an accessor follow its accessor list's layout — changed: the split inherits FMT-STRUCT-028's content of a created list; the accessor merge keeps no line break, and its space stays FMT-BREAK-031's exception.
- `structural-transforms.md` `Uses` — adds FMT-BREAK-013, FMT-BREAK-014, FMT-PIPE-004, FMT-SPACE-004.
- Change doc `sdd/changes/2026-11/attribute-list-structural.md` (still Draft): *What* gains the trivia rule, the lambda split (amendment 3, decision 6), the kept line break, and the single split and merge (decision 7). *Visible output changes* now lists two changes with input, before and after in C#: `[A,⏎ B]` settles in one pass (demanded by FMT-PIPE-004 and FMT-SPACE-004), and `[A⏎ , B]` / `[A,⏎⏎ B]` become `[A]⏎[B]` (maintainer answer A). Both are shown for a declaration and for an accessor. *Decisions* gains four sections and *Alternatives* four entries.

`scripts/test.sh --project architecture --filter Spec`: everything passes except `EveryRequirementOfAnEnforcedSpecIsCovered`, which still lists FMT-BREAK-032, FMT-STRUCT-028, and FMT-STRUCT-029 as uncovered. That is unchanged from round 1, and the test implementer covers them.

## Examples for the tests

FMT-STRUCT-028 (both examples and the prose negatives), FMT-STRUCT-029 (both examples), FMT-BREAK-032, FMT-BREAK-027. The two *Visible output changes* blocks of the change doc, run for one pass and for two passes. Add the inputs of the skeptic's matrices in `02-skeptic.md` under LF and CRLF.

## Output questions

_None._ Answer 1 is written into FMT-STRUCT-028 and the change doc. Every other output listed is unchanged from `main`.

## Internal choices

1. The accessor merge keeps the code fix's separators (one space, no line break), so FMT-BREAK-031's recorded exception stays. Alternative: give it the structural separators. I rejected that because a line break kept inside a one-line accessor list could change output, and nothing is gained.
2. FMT-STRUCT-029 fixes neither where the kept line break stands nor its indentation. It only requires one line break between the merged list and the token behind the last list. The final output does not depend on the position, because FMT-BREAK-027 joins the lists onto the parameter's line. Alternative: require it behind the merged list. That over-constrains the shared merge.
3. The lambda split is recorded in the change doc only. FMT-STRUCT-028 still leaves lambdas unspecified, because their output looks wrong (follow-up 1 of `00-lead.md`). Alternative: specify today's lambda output. I rejected that because the role forbids specifying output that looks wrong.
4. The code fix's split (SHARED-ATTR-017) is not named in the single-split decision. It has its own line-and-indent output and conditions, and decision 7 names only the structural and accessor splits.

## Follow-up candidates

_None new._ The five candidates in `00-lead.md` stand. FMT-STRUCT-028 now leaves `[A, B⏎]` outside its scope, so follow-up 4 is still needed.
