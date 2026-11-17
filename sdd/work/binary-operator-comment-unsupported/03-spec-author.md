Role: spec author
Commit: 0f1cbf7
Verdict: spec written

Follow-up after batch 1 (all A). The prompt carried the maintainer's answer and the lead's accepted skeptic recommendations, no intended fix or suspected cause. The "follow-up for B" part of the answer is not handled here.

## Changed requirements

- FMT-UNSUP-011 — changed (this run's draft): the operator condition decides an `is` syntactically — an `is` the parser reads as a binary expression counts whatever the name behind it denotes (`o is /* c */ Color.Red` refused), an `is` read as a pattern match does not (`o is /* c */ 5`, `o is /* c */ not null`); *Why* gains that reason and the rarity sentence (RH5049 covers comments; directives and disabled text are unreported but rare in practice). New example: the enum-member block, refused, location `/* c */` at line 10, column 21. The other-side example gains `var u = M3() is /* c7 */ 5;`.
- RH2001-007 — changed: a comment inside the initializer's value withholds the fix when it stands between a binary operator and its right operand (FMT-UNSUP-011, SHARED-FIX-010); elsewhere in the value it does not. Two new examples. `Uses` gains FMT-UNSUP-011.
- RH7101-007 — changed: new paragraph, the fix is also withheld when the declaration or its type holds a shape refused under FMT-UNSUP-008, FMT-UNSUP-009, or FMT-UNSUP-011, also inside an initializer value; *Why* gains one sentence; new example. `Uses` gains FMT-UNSUP-008, FMT-UNSUP-009, FMT-UNSUP-011. FMT-UNSUP-008/-009 are a clarification of today's behavior (both withheld on `main`, checked), FMT-UNSUP-011 is the change.
- `sdd/specs/formatter/indentation.md` non-goal — the "right operand of a wrapped binary expression" example dropped.
- Change doc `sdd/changes/2026-11/binary-operator-comment-unsupported.md` — `Status: Accepted`; *Affected requirements* gains RH2001-007 and RH7101-007; *What* names the indentation non-goal; *Why* carries the rarity sentence; *Visible output changes* 7 (`is` + enum member), 8 (RH2001), 9 (RH7101 type-wide) added; *Decisions* record the answers per question (1–8: A) and two new decisions (`is` decided syntactically; fixes withheld wherever their formatted node holds the shape); the same-line decision states it deletes nothing of its own; *Alternatives* gain the B options of questions 6–8.
- No rule page changes (withheld fixes only; general-notes covers them).

## Examples for the tests

- FMT-UNSUP-011: the new enum-member block (refused; location line 10, column 21) and the `M3() is /* c7 */ 5` line of the other-side block (formatted: `scripts/format.sh` on the block adds only the FMT-BLANK-011 blank line above `// c1`, as before).
- RH2001-007: `= 2 + /* three */ 3;` reported, no fix (today `scripts/apply-fix.sh RH2001` fixes it, so the test flips; `VerifyFixIsOfferedForACommentInsideTheInitializerValue` needs the second example's value instead); `= 2 /* two */ + Offset(/* three */ 3);` fixed to `private int _value = 2 /* two */ + Offset(/* three */3);` — produced by `apply-fix.sh` on the current code, LF and CRLF.
- RH7101-007: the new example (both declarations reported, no fix). On the current code `apply-fix.sh RH7101` fixes it (output shown as *Before* in change 9); fixtures with FMT-UNSUP-008 (`b ? /* c */ 1 : 2`) and FMT-UNSUP-009 (comment before a comma on a later line) in a method already give `no-fix-offered`.
- Change doc changes 7–9: *Before* of 8 and 9 produced with `apply-fix.sh`; 7's *Before* is the skeptic's `format.sh` run; CLI column (8,30) computed from the input.

`scripts/test.sh --project architecture`: 184 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, "FMT-UNSUP-011: no test covers it", unchanged from 01 and resolved by the first tagged test.

## Output questions

_None._

## Internal choices

- RH7101-007 extended with a second paragraph instead of a new requirement; its title ("for a comment inside the declaration") is kept, so existing tags keep their meaning. Alternative: a new RH7101 requirement for the other shapes.
- FMT-UNSUP-011 states the `is` decision itself instead of amending SHARED-COMMENT-007, which keeps leaving that `is` open for the rules that use it (RH5038, RH5049). Alternative: decide it in SHARED-COMMENT-007, which would touch those rules' scope.
- RH2001-007 names only comments behind an operator; a directive in the value stays "not covered by this requirement", as before.

## Follow-up candidates

_None_ new. The lead's existing candidate (update thoenissen/Reihitsu#953) stands; the RH7101 Uses candidate is folded in here.
