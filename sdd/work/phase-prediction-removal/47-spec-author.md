Role: spec author
Commit: fc36f9d
Verdict: text written

This is a documentation repair of finding #13 of `46-auditor.md`. The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It named the two phrases to correct and limited the change to the change doc.

## Changed text

Only `sdd/changes/2026-10/phase-prediction-removal.md` changes. It stays **`Status: Accepted`**. Both edits are in the last paragraph of the decision "The node-level entry points lay out their node at its own column".

**First phrase: "and the output stays as it is today".**

It now reads "and the write-back stays as it is today", followed by two new sentences:
- The output of a target in the gap is today's output wherever the context stands at its depth.
- Where the context is misindented, the output can differ from today's. The reason is that the context is now laid out from its first token's column in the document, instead of being formatted at its depth and moved afterwards. There, the lines aligned to an anchor on the target's first line stand off that anchor both today and after this change.

This follows the auditor's evidence:
- the misindented raw-string member gives content at 28 on head and 26 on `main`, under quotes at 31;
- the comment-tail member gives `3` at 37 on head and 39 on `main`, with `2` at 24;
- the source is the decided column step, not the write-back.

**Second phrase: the parenthetical "with the same output as today".**

It now reads "with the same output as today for a statement at its depth". The auditor confirms that the example as written gives content at 26 under quotes at 29, identical to `main`.

"As it does today" at the start of the paragraph, and in the What bullet, is about the write-back, which is unchanged. Those phrases stay.

No spec, requirement ID, or behavior changes.

## Proof

```
TEXT-ONLY PROOF: PASS — base=fc36f9d head=worktree — 1 changed file(s) (1 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
```

(`scripts/verify-text-only.sh --base HEAD --head worktree --strict-docs`.)

## Examples for the tests

_None._

## Open questions for the maintainer

_None._
