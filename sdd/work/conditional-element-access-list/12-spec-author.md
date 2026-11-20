Role: spec author
Commit: ec971405
Verdict: text written

Text repair for finding 1 and the architecture finding of `11-auditor.md` (decision 10 in `00-lead.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict; the repair follows the auditor's suggested change, which I checked against FMT-BREAK-014.

Changed:

- `sdd/specs/formatter/pipeline.md`, FMT-PIPE-024 fact table, row *Element-access arguments*: "Whether the root is the bracketed argument list of an element access or of an implicit element access" → "Whether the root is a bracketed argument list that holds an index, as FMT-BREAK-014 defines it". The row no longer restates which lists hold an index, so FMT-BREAK-014 is the one statement (element access, element binding, implicit element access; fixed-size buffer excluded), and the row now covers what `ReihitsuFormatterDetachedRootTests.ConditionalElementAccessArgumentListTargetGetsItsArgumentsCollapsed` asserts.
- Same requirement, line-break bullet: "may collapse the arguments of an element access onto one line" → "may join the root's list when it holds an index (FMT-BREAK-014)".
- `sdd/changes/2026-11/conditional-element-access-list.md`, `Affected requirements`: adds "FMT-PIPE-024 (clarified: … refers to FMT-BREAK-014 … no behavior change of its own)".

The fact's label *Element-access arguments* stays, matching `RootConstruct.ElementAccessArguments`, which decision 11 keeps; renaming only the spec label would make the two disagree.

Proof: `scripts/verify-text-only.sh --base 82d818ae6ce4bd7ab01f45ea09e307ae2973363b --head worktree` →
`TEXT-ONLY PROOF: PASS — base=82d818ae head=worktree — 4 changed file(s) (4 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`.
`scripts/test.sh --project architecture` (a requirement-ID reference was added): 198 passed, 0 failed.

## Changed requirements
FMT-PIPE-024 — Node-level input facts are plain values — clarified (normative wording; the spec checker confirms)

## Examples for the tests
_None new._ The existing test tagged FMT-PIPE-024 already asserts the clarified wording.

## Output questions
_None._

## Internal choices
- Kept the fact label *Element-access arguments* instead of renaming it (e.g. *Index list*), so it keeps matching the code name decision 11 leaves unchanged.

## Follow-up candidates
_None._ (The auditor's hints on `indentation.md:544` and SHARED-DIAG-010 are already in `00-lead.md`.)
