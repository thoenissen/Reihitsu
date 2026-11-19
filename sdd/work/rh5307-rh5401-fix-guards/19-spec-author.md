Role: spec author
Commit: 60a4f24
Verdict: spec written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 12 itself, which is a lead decision recorded in `00-lead.md` and which I carried out.

## Architecture refusal (decision 12)

**RH5401-004.** The whole-region total count is gone, and so are the false sentence "Each condition catches what the other does not …" (auditor 18, finding 4) and the explanation of what a list split by FMT-STRUCT-028 drops. In their place is the new condition: the fix MUST NOT be offered when one of the property's attribute lists holds a blank line or a comment between its brackets. That means in the trivia between two tokens of the list, from its `[` through its `]`, including inside an attribute's argument list, whether or not the formatter splits the list, and whether the comment stands on its own line or on a line with code. The other side of the condition is written out too (FC-002): trivia in front of `[` or behind `]` is not between the brackets. It counts only in the per-gap condition, which stays word for word. `Why:` explains the cause: a drop in one list offsets an insertion in another (`W`), FMT-BREAK-027 leaves the placement of a list with a comment between its brackets unspecified, and the class is rare (blank line: seldom written; comment in arguments: RH5038; list on the property's line: RH5515).

Examples. The withheld block now holds `N`, `O`, `P`, `Q` (moved here from the offered side), `R`, `T` (comment on its own line in the arguments), `U` (`[Obsolete /* c */] int U`), `V` (`[Obsolete("a" /* c */)]` on its own line), and `W` (the auditor's `a09`, built from real attributes: `[Obsolete,⏎⏎ CLSCompliant(false)]⏎[DefaultValue(typeof(int),⏎ // why⏎ "1")]`). `S` is removed: it existed only to show that the per-gap check is still needed next to the total, and `N` now covers its input class.

**RH5401-003.** The condition sentence now reads "no attribute list holds a blank line or a comment between its brackets and that layout keeps the blank lines behind the attribute lists". The kept-blank-lines example loses `Q` and gains `J` (`[Obsolete] // note⏎int J⏎    => 1;` → `[Obsolete] // note⏎int J => 1;`), the offered side for a comment behind `]`.

**Change doc** (`Status: Accepted` kept). The changes:
- *What* states the new refusal.
- The offered block swaps `Q` for `J`.
- The unchanged block gains `Q` and `T`, and its prose is reworded.
- A new before/after block covers `U` (see below).
- The decision "judges the property's own lines" drops the FMT-BREAK-027 comment exception.
- The question-6 decision replaces the total-count sentence with the cause-based refusal. It says the refusal is a run decision, not a maintainer answer.
- Two rejected alternatives are added: the total count, and an own-line-comment-only check.

Rule page `documentation/rules/RH5401.md`: unchanged. The new refusal is a withheld fix in a corner case.

Verified on 60a4f24 (`apply-fix.sh`, LF and CRLF):
- The RH5401-003 attribute example (`E`, `F`, `J`) gives exactly the spec's output.
- `T` is withheld today.
- `Q`, `U`, `V`, `W` are fixed today: these are the reproductions. `W` shows the moved blank line of finding 4.
- `N`, `O`, `P`, `R` are withheld today (from 13).
- Every member compiles (the withheld block needs `using System.ComponentModel;` for `W`).
- `scripts/test.sh --project architecture`: 198 passed.

## For the lead: one change against `main`

`U`, `[Obsolete /* c */] int U⏎    => 1;`, gets the fix on `main` (→ `[Obsolete /* c */] int U => 1;`). Under decision 12 it is withheld. Decision 12's row says "Q's offered fix was a lead decision" and does not mention this member. Before, the change doc said `U` "was fixed before and is fixed the same way after". It now lists `U` as a visible change: before fixed, after no fix. I carried decision 12 as written, because the class is rare: the list already violates RH5515, and FMT-BREAK-027 leaves its placement unspecified. Under `AGENTS.md` that makes the withheld fix local and silent, not an output question. If you read narrowing ground that `main` already fixes differently, the alternative is in the change doc: withhold only for a comment on its own line between the brackets. That keeps `U` and `V` fixed. My recommendation is to keep decision 12 as written.

## Changed requirements

- RH5401-004 — The fix is withheld when the property stays on several lines — changed (total count replaced by the blank-line-or-comment-between-brackets refusal; false sentence removed; examples `Q`, `T`, `U`, `V`, `W` added, `S` removed)
- RH5401-003 — The fix puts the property on one line — changed (condition wording; `Q` moved out, `J` added)

## Examples for the tests

- RH5401-004 withheld block: `Q`, `U`, `V`, `W` (offered today: the reproductions) and `T` (withheld today), one test each with `VerifyNoCodeFixIsOffered`. `N`, `O`, `P`, `R` keep their tests. The tests for `S` and for `Q` as offered (`VerifyFixKeepsABlankLineInsideAnAttributeListTheFormatterDoesNotSplit`) no longer match a spec example. The `Q` test contradicts the spec now. The `S` test still passes and may stay or go.
- RH5401-003: `J` (offered, exact output).

## Output questions

_None._ Decision 12 decides the class. It is rare, so the withheld fix needs no question (see *For the lead* about `U`).

## Internal choices

- **Directives and disabled text between the brackets are not part of the refusal.** Decision 12 names a blank line or a comment only. `[Obsolete,⏎#if X⏎ CLSCompliant(false)⏎#endif⏎ ]⏎int E⏎    => 1;` is fixed today with the directives kept (`int E => 1;`). That is safe, and on `main` it got no fix. The class is rare, and its output does not change what the fix offers for common code. Alternative: add directives and disabled text to the condition, because FMT-BREAK-027 leaves their placement unspecified too. Not done, so that decision 12 is not widened.
- **"Between its brackets" is defined by tokens.** It means the trivia between two tokens of the list, from `[` through `]`. So the leading trivia of `[` and the trailing trivia of `]` are outside, and `[Obsolete] // note` gets the fix (`J`).

## Follow-up candidates

_None._
