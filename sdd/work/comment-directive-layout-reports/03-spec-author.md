Role: spec author
Commit: 0407a4f
Verdict: spec written; change docs written (both `Accepted`)

Amendment round after the maintainer's answers to batch 1 (`00-lead.md`, *Maintainer answers* item 3) and decisions 5–9. My prompt carried no intended fix or suspected cause. I checked the spec architecture tests on the edited specs (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RuleDocumentation"`): 37/37 pass.

## Changed requirements

- RH5109-001 — Parameters sharing a line in a multi-line list are reported — changed (OQ4 A, OQ5 A):
  - The claim "as FMT-BREAK-012 decides it" is removed from the normative sentence.
  - The unreported shapes now name a `)` that a comment holds on a later line.
  - `Why` states where the rule agrees with FMT-BREAK-012 (a list wrapped only behind `(` or in front of `)`) and where it does not: the formatter lays out a comment- or directive-held first parameter one per line, which P11 allows.
  - The unreported example gains `P` (`(⏎#pragma …⏎int a, int b)`) and `Q` (`int a, int b // c⏎)`).
- RH5111-005 — renamed "A conditional directive in the construct keeps it unreported; a comment or directive withholds the fix" — changed back to `main`'s report behavior (OQ2 B, OQ3 B):
  - A comment, `#region`, `#endregion`, or `#pragma` in the span around `=` keeps the construct reported. This holds when every wrapped gap holds one, for every kind of construct, and for a declarator whatever stands elsewhere in its declaration. No FMT-UNSUP-004 copy is needed.
  - The conditional-directive paragraph (SHARED-DIAG-008) and the fix-withholding sentence are kept as on the base.
  - The example now shows the enum member, the property, the declarator, the `/* c */` assignment, the `#pragma` assignment, and the OQ3 declarator `int /* t */ x⏎#pragma …⏎= 1;`, all reported without a fix.
  - `Uses` goes back to the base list plus SHARED-DIAG-010.
- SHARED-DIAG-010 — A layout rule does not report a shape the formatter keeps — changed: names RH5111 as an exception for a comment or a `#region`/`#endregion`/`#pragma` directive around the `=`, with a sentence in `Why` (OQ2 B). Keeps `main`'s output, so it is recorded here and not in a change doc, as the prompt asked.
- RH7304-001 — clarified (decision 6):
  - The comparison is with "the region's first element": the first token when it starts its line, otherwise the comment that starts the line. That is the comment that starts the token's line, or, when the token's line begins with the tail of a multi-line comment, the comment that starts the earlier line.
  - The old precondition "the first token after the `#region` starts its line" is folded into that definition.
  - The example gains `Others` (directives in the token's column, comment to the left: reported, the skeptic's missing case) and `Lines` (multi-line comment: not reported).
  - `Uses` gains SHARED-INIT-001.
- RH7304-004 — The fix indents both directives of the region — changed (OQ1 A): it indents to "the region's first element … its first token, or the comment that leads the first line of its code". New example: one-line and multi-line comment-led regions, before and after.
- SHARED-COMMENT-001 — clarified (decision 7): a `///`/`/** */` comment followed in the same leading trivia by a later one, with an ordinary comment or directive between them, documents nothing. A blank line alone does not detach it, and neither do ordinary comments that only follow it. New example: the issue's `/// a⏎// note⏎/// b⏎void N()`, not reported by RH5034.
- Change doc `sdd/changes/2026-11/rh5109-parameters-on-one-line.md`:
  - Status `Accepted`.
  - New entry 3, a `)` held by a comment (OQ4).
  - Entry 4, formerly 3, now has the `#pragma` variant (OQ5).
  - The entries are renumbered to 1–6.
  - `Decisions` records #1736 triage A, OQ4 A, and OQ5 A, with the corrected FMT-BREAK-012 statement.
  - `Why` and `Alternatives` no longer claim "the fix result is not a fixed point" (decision 9). They now say that one input had two stable results.
- New change doc `sdd/changes/2026-11/rh7304-fix-for-comment-led-region-line.md` (`Accepted`): one visible output change (fix offered), and `Decisions` records OQ1 A.
- `documentation/rules/RH5109.md`: no edit. Its Description already reads as change-doc entry 6 (answered A in OQ4). The page names no comment case, and none belongs there.

## Examples for the tests

- RH5109-001 (both blocks, now with `P` and `Q`), RH5109-003.
- RH5111-005 (all three blocks; the OQ3 declarator `x` included).
- RH7304-001 (`Fields`, `Values`, `Names`, `Others`, `Lines`), RH7304-004 (new comment-led example).
- SHARED-COMMENT-001 (RH5034 `/// a⏎// note⏎/// b` example, in Parse and None mode).
- Unchanged from 01: SHARED-INIT-001, RH5204-005, SHARED-ATTR-019/020.

## Output questions

_None._ Every visible output change the two change docs list has the maintainer's answer.

## Internal choices

- **OQ1 recorded in its own change doc** instead of the RH5109 one. The RH5109 doc's title and cause cover #1736 only, and `spec-author.md` gives an issue with another cause its own doc. Alternative: add the RH7304 entry and decision to the RH5109 doc.
- **The RH7304 change doc lists only the fix result as a visible change.** That a formatter-shaped comment-led region is no longer reported follows from SHARED-COMMENT-011/SHARED-DIAG-010 (decision 2). It is stated in *Why*, not listed, so that the doc lists exactly answered changes. Alternative: list it as a second entry.
- **The shared specs name RH5111 and RH5034 without requirement IDs.** The `Uses` test would otherwise make a shared spec depend on a rule spec. Alternative: list the rule IDs in the shared specs' `Uses`.
- **RH5111-005 keeps the base's "sits in that span" wording** for the fix, so that only the report sentence changes against the base.

## Follow-up candidates

_None_ new. F1, F2, F4, and F5 in `00-lead.md` stand as recorded.

Note for the PR (decision 5): RH5304 and RH5306 join #1147 without a spec edit, because both specs already use SHARED-COMMENT-011/SHARED-INIT-001. The issue's "same shapes without the comment" Done-when item is replaced by the misplaced-comment variant.

## Files

- /home/user/Reihitsu/sdd/specs/analyzer/RH5109.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH5111.md
- /home/user/Reihitsu/sdd/specs/analyzer/RH7304.md
- /home/user/Reihitsu/sdd/specs/shared/comment-placement.md
- /home/user/Reihitsu/sdd/specs/shared/diagnostics.md
- /home/user/Reihitsu/sdd/changes/2026-11/rh5109-parameters-on-one-line.md
- /home/user/Reihitsu/sdd/changes/2026-11/rh7304-fix-for-comment-led-region-line.md
