Role: spec author
Commit: eff9db0
Verdict: change doc written

Repair round 3 (routed implementer findings D1 and D2 of `27-implementer.md`, `00-lead.md` rows 36 and 37) of thoenissen/Reihitsu#1496. The prompt named the two corrections. It carried no suspected cause and no hoped-for verdict. No spec requirement changes; only the change doc `sdd/changes/2026-11/blank-line-sole-owner.md` (entry 20), which stays Accepted: both corrections fall under the maintainer's guidance (a comment on a line with code).

Verified against the code on eff9db0 (CLI from the branch, fixtures in the session scratchpad under `r29/`):
- `d1.cs`: `}⏎/* x⏎ */ int I;` after a method stays without a blank line above `/* x`; `}⏎/* z⏎ */ int i = 0;` inside a block gets one (SHARED-BLANK-003). No requirement asks for a blank line in front of a member after `}`: SHARED-BLANK-003 says "followed by another statement in the same block or switch section".
- `d2.cs`: whole-document formatting of `while (a)⏎/* note⏎*/ if (b)` inserts no blank line above `/* note`. The RH5204 code-fix tests `VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` (LF and CRLF) pass with the test implementer's working-tree expectation, which drops that line; on `main` the expectation held it.

## Changed requirements

_None._ Change doc, entry 20:
- D1: "a member or statement after `}` (SHARED-BLANK-003)" now reads "a statement after `}`", followed by a sentence that no rule asks for a blank line in front of a member after `}`, so above a comment glued to such a member none is inserted and the author's is kept.
- D2: a new paragraph with input, before, and after: the RH5204 code fix on an unbraced embedded statement behind a glued multi-line comment no longer puts a blank line between `while (a)` and the comment.

## Correction to `25-spec-author.md`

Example 4 is wrong in its last item. `}⏎/* x⏎ */ int I;` (a member after `}`) does not get a blank line above `/* x`; the input is left unchanged. Only the statement items hold: `a.B();⏎/* x⏎ */ return;`, `break;⏎/* x⏎ */ case 2:`, and a statement after `}` (`}⏎/* z⏎ */ int i = 0;` inside a block) get the blank line above the comment. The existing-test scan of that handover also missed the RH5204 code-fix test of D2.

## Examples for the tests

Entry 20 as corrected: member half of `InsertsBlankLineAfterClosingBraceAboveMultiLineCommentGluedToStatementOrMember` expects the input unchanged above `/* x`; `EmbeddedIfBehindMultiLineCommentFixedData` without the blank line under `while (a)`. Both are the test implementer's (rows 36 and 37).

## Output questions

_None._ Both outputs are covered by the maintainer's guidance (rows 36 and 37).

## Internal choices

_None._

## Follow-up candidates

_None._
