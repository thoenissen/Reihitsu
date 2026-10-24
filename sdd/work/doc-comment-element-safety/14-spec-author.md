Role: spec author
Commit: 9161d75
Verdict: F4: spec fixed; F5: rule pages fixed

Repair round 2, covering the two findings of `13-auditor.md` that are routed to me. This file lists only the changes. I edited and staged spec and documentation files only. The test files modified in the worktree belong to the test implementer.

## F4 — FMT-DOC-013 and comments that FMT-DOC-002 moves (#1329)

**Re-derived.**
- FMT-DOC-004 normalizes a comment that FMT-DOC-001 or FMT-DOC-002 moves, "unless FMT-DOC-013 leaves its text as written". FMT-DOC-013, however, named only comments that start their own line and comments that FMT-DOC-001 moves.
- So a comment such as `/* c */ /// <summary>S` was covered by neither requirement.
- The outcome follows from answers 1C and 2A: the comment is incomplete documentation, and moving it changes none of its text.

**Observed.** `bash scripts/trace.sh` on the worktree:
- `class C⏎{⏎    int A;⏎⏎    /* c */ /// <summary>S⏎    int B;⏎}`: `DocumentationCommentFormattingPhase` puts a line break after `/* c */`, and `IndentationPhase` indents `/// <summary>S` to column 4. The result is stable in pass 2, with the text unchanged.
- `/* banner */ /// <summary>S⏎class C…`: the result is `/* banner */⏎/// <summary>S`, stable.

**What I did** in `sdd/specs/formatter/documentation-comments.md`:
- FMT-DOC-013's third normative sentence now also names the case where FMT-DOC-002 puts a line break between the comment and an earlier comment on its line. In that case the formatter moves the comment and leaves its text as written.
- The coverage sentence now reads "one that FMT-DOC-001 or FMT-DOC-002 moves".
- New before/after example: the first trace input above.

**Examples for the tests.** FMT-DOC-013 (and FMT-DOC-002): the new example, through the full pipeline, with an idempotency pass. The `/* banner */` form at the start of a file is a second input with the same outcome.

## F5 — Rule pages and an end tag that differs only in case (#1241, #1329, #1344)

**Re-derived.**
- Answer 5 states that end-tag names are compared exactly, and SHARED-DOC-017 and FMT-DOC-013 say the same.
- So the rule pages have to name the case-only difference. They also have to cover an end tag with another name where no other element exists, such as `<summary>S</returns>`. "Closed by the end tag of another element" did not describe that input.

**What I did.** I rewrote the gap notes and exception bullets with one wording on every affected page: "never closed, or is closed by an end tag with a different name, including one that differs only in case", with an example per page.
- `documentation/rules/RH8107.md`: `<Returns>Nothing</returns>`
- `documentation/rules/RH8202.md`: `<Value>The size</value>`
- `documentation/rules/RH8204.md`: `<Placeholder>Later text</placeholder>`
- `documentation/rules/RH8301.md`, `RH8304.md`, `RH8305.md`, `RH8307.md` (the `## Exceptions` bullet): `<summary>S</returns>` or `<Summary>S</summary>`
- `documentation/rules/RH8309.md`: the gap note, with the same wording and no example.

Text only. No spec requirement changed for F5.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RuleDocumentation|FullyQualifiedName~RequirementCoverage"`: 51 passed, 0 failed. This ran in the shared worktree, so it includes the test implementer's in-progress files.
