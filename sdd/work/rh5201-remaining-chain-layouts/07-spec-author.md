Role: spec author
Commit: 2ca820d
Verdict: spec written

Follow-up round: carries the maintainer's answer to Q7 ("a") into the specs and the change doc, and makes the RH5201-006 examples compile without CS8602. My prompt asked for these two changes only; it carried no intended fix or hoped-for verdict.

## Changed requirements

- RH5201-002 — clarified (Q7 A): a link on the last line of an own-line multi-line comment, behind its `*/`, is reported whether only whitespace or also other comments stand between the `*/` and the operator (`*/ /* y */ .Self()`). The "only whitespace" condition now applies only to what stands in front of the comment. The *Why* says that a comment between does not give the link a line of its own. The `*/` example gains `f` (reported on the `.` behind `/* y */`), with fix result `*/ /* y */` ⏎ `.Self();`. Qualifier examples (FC-002): `a` (whitespace only) and `f` (second comment), both reported; `e` (comment starts behind code), not reported.
- RH5201-006 — examples changed, behavior unchanged: example 2 and example `g` now declare `C Next` plus `C? Opt` and write `c.Opt?` ⏎ `.Next.Self()` / `c.Opt? // keep` ⏎ `.Next.Self()`, so `.Self()` is no longer called on a `C?` (CS8602). Tokens, operators, columns, and layout are unchanged, and they match the tests (`04-test-implementer.md`). The fix result reads `c.Opt` ⏎ `?.Next` ⏎ `.Self();`. RH5201-003's example (`d.Next?` ⏎ `.Next`) has no dereference behind the conditional access, so it stays as it is.
- SHARED-CHAIN-005 — clarified (exception paragraph): it now names the comment as one that stands below the token in front of the link (a line break between them), and it says the exception also holds with other comments between the `*/` and the operator. RH5201 reports the link also when a second comment stands there. A comment that starts on the line of the token in front of the link is not covered and is judged as code on its line (SHARED-CHAIN-007). Before this, the exception named every multi-line comment, which contradicted RH5201-002 example `e` and the shared helper `SyntaxTokenPositionUtilities.IsBehindMultiLineCommentTail`, which reads only the operator's leading trivia. No output changes, because the formatter and RH5201 already behave this way.
- SHARED-COMMENT-012 — unchanged. Its reporting clause is a floor ("only whitespace … MUST report") and says nothing about a comment between the `*/` and the token. Widening it would bind every line-start rule (RH5203 and others), which is outside this run. RH5201-002 states the RH5201 case.
- Change doc `sdd/changes/2026-11/rh5201-first-call-behind-broken-root-line.md`: *Affected requirements* names the SHARED-CHAIN-005 exception wording and the Q7 case for RH5201-001/-002. *Visible output changes* item 5 uses `c.Opt?`, and item 6 gains `f` with input, diagnostic, and fix result. *Decisions* → *Maintainer answers* records the second-batch answer to Q7 with its output.
- Rule page: unchanged. The case is a corner case, not the normal flow.

Checked on the worktree: the RH5201-002 `*/` example (markup removed) goes through `scripts/apply-fix.sh RH5201` under LF and CRLF and gives exactly the fix results in the spec, `f` included. `scripts/format.sh` gives the same `*/ /* y */` ⏎ `.Self();`. The RH5201-006 example 2 fixture (`C Next`, `C? Opt`) gives the spec's fix result, and `g` gives `no-diagnostic`. `scripts/test.sh --project architecture` passed (198/198). `scripts/lane-check.sh behavior-change --head worktree` confirmed the lane.

## Examples for the tests

- RH5201-002 — new case `f` (`*/ /* y */ .Self()` with `*/` in the anchor column): reported on the `.` behind `/* y */`, fix `*/ /* y */` ⏎ `.Self();`, under LF and CRLF. No RH5201 test covers it yet (a grep for `*/ /* y */` finds none under the RH5201 tests).
- RH5201-006 — examples 2 and `g`: the tests already use the `C Next` + `C? Opt` form, so nothing changes.

## Output questions

**RH5201 · Member-access chains should be aligned — a comment in front of the own-line comment, on its first line.** The shared helper also reports a link behind the `*/` when the multi-line comment starts behind another comment on its line. Q7 did not cover this case (`05-implementer.md` names both). The spec stays silent on it until the maintainer answers. It is no blocker: no test depends on it.

Input:
```csharp
var g = c.Self()
         .Self()

         /* w */ /* x
         */ .Self();
```

Before (`main`): not reported. RH5201 judged the `*/` line's column, and that column is the anchor column.

After, A (recommended, today's branch behavior): reported on the `.` of the last `.Self`. The fix gives:
```csharp
var g = c.Self()
         .Self()

         /* w */ /* x
         */
         .Self();
```

After, B: not reported, and no fix offered. The formatter still breaks the line as in A, so the formatter changes a layout that RH5201 does not report.

Why A: the formatter gives this output today, and the link has no line of its own, the same as in Q7. A needs no code change. B needs a narrower check in RH5201 than the formatter's shared helper.

On A, the spec change is one clause in RH5201-002: "only whitespace stands between the line break in front of the comment and the comment" becomes "a line break stands between the token in front of the link and the comment", with `g` added as an example.

## Internal choices

- SHARED-CHAIN-005's exception was narrowed in wording to comments that stand below the token in front of the link, to match RH5201-002 `e`, SHARED-CHAIN-007, and the shared helper. The alternative was to leave it as written, which contradicts `e`.
- The Q7 case is stated in RH5201-002 and SHARED-CHAIN-005, not in SHARED-COMMENT-012. The alternative, widening SHARED-COMMENT-012, would bind every line-start rule.

## Follow-up candidates

_None._
