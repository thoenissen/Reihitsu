Role: spec author
Commit: 3da77dc
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decisions 24, 25, and 26 of `00-lead.md`, which it named as the work.

## Changed requirements

- SHARED-ORDER-007 — Initializer execution order is kept — clarified (decisions 24, 26) and extended (decision 25), in `sdd/specs/shared/ordering-move-safety.md`. No new requirement ID; no change doc: decision 26 narrows the guard to what SHARED-FIX-003 *Build configurations* already demands (it withholds only where some build changes), and decision 25 is the maintainer's standing answer, which the lead states in the PR.
  - New paragraph: both guards judge each build configuration on its own. A configuration chooses one branch (or none) in every `#if` group that declares or splits a member the move moves or crosses. The move is refused when, in some configuration, a moved and a crossed member have a runtime initializer of one context, or it reorders a layout type's instance fields. Nothing one configuration compiles is combined with another's, and declarations in mutually exclusive branches are not joined by name (finding #7). The fix may read combinations no symbol definition selects (`#if DEBUG` beside `#if !DEBUG`); they can only refuse (the auditor's second hint).
  - Split-member paragraph rewritten per configuration: initializer, context, and instance field are each per configuration; a `const` configuration has none of them; excluded members inside the declaration count in the configurations that compile them. The union wording ("counts with what any configuration contributes", "counts in both contexts") is gone. Added: a member that is `static` without initializer in one configuration and instance with initializer in another has no static initializer in any configuration.
  - New paragraph (decision 25): at most 256 configurations per move; when the groups that declare or split a moved or crossed member have more, the move is refused; a group that holds no moved or crossed member neither counts nor refuses.
  - `Why:` updated for both (union describes a member no build compiles; the limit's refusal is the safe side, and only touched groups can change an order).
  - Example intros reworded: the RH7309 `!DEBUG static` example ("leaves the member an instance field in the other configuration" instead of "counts in both contexts") and the `K` example ("gives it no context"). Their outcomes are unchanged under per-configuration reading.
  - New examples, before "Crossing an instance initializer is allowed": the genuinely split `_b` of decision 26 (fix offered), its `private readonly _c` counterpart (no fix), and a sentence that two complete declarations of `_b` (j1s) are judged the same way; far9 (fix offered, with output) and the over-cap counterpart (no fix).

## Examples for the tests

- SHARED-ORDER-007, genuinely split `_b` (`#if DEBUG private static int _b #else private int _b = Log("b") #endif ;`, static initialized `_c`): fix offered. No expected text is given on purpose: the lone `;` continuation line is decision 15's shape, so `VerifyCodeFixIsOffered` fits, as for the `K` example. Counterpart with `private readonly int _c = Log("c");`: `VerifyNoCodeFixIsOffered`. This is the case `23-test-implementer.md` left unwritten pending this decision.
- SHARED-ORDER-007, far9 and the over-cap input: both already exist (`FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove`, `NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit`). They can now carry `[Requirement("SHARED-ORDER-007")]` (and RH7109-006 like their neighbors); `23-test-implementer.md` listed them with no requirement.
- j1s, j2s, j1w, j2w already match the new text (two complete declarations of one name).

## Output questions

_None._ Decision 25 is covered by the standing answer; decision 26 is the lead's.

## Internal choices

- Stated the limit as the number 256, not as "a bound the fix sets": the over-cap test pins it (nine groups, 512 configurations, refused), so the spec is testable on both sides. Alternative: leave the number out and say "a limit"; rejected because the test would then rest on no stated condition.
- Defined a configuration only over the groups that declare or split a moved or crossed member, matching decision 25's scope, and allowed combinations no symbol definition selects as "may also read … can only refuse", so the spec neither requires evaluating conditions across independent groups nor forbids the implementation's independent reading. Alternative: require only combinations a real build can select; rejected, it would oblige condition evaluation the run does not do and add no fix main offers.
- Did not touch RH7102/RH7103/RH7109/RH7309 specs: RH7102.md line on split crossed declarations ("in a configuration that compiles it so") already reads per configuration; the others reference SHARED-ORDER-007.
- No rule page changed: a withheld fix in a corner case.

## Follow-up candidates

_None._
