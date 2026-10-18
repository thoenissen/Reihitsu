Role: spec author
Commit: 8db5038
Verdict: spec written

Repair round on the skeptic's handover `02-skeptic.md` for thoenissen/Reihitsu#987. I re-derived each change against the code before making it. The cases the skeptic lists for the lead to file (N1–N4) stay out of the specs. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- RH6024-002 — Directive conditions and patterns are not reported — clarified (example only).
  - The example now has a mis-spaced `is` before each pattern: `o is  null`, `o  is string s`, `a is  > 0 and < 5`. The directive condition `#if DEBUG&&TRACE` was already mis-spaced.
  - Re-derived: RH6024 registers only the `BinaryExpressionSyntax` kinds, not `IsPatternExpression`. My harness reports nothing for those inputs, nor for `o is  not null` or `a is  > 0  and  < 5`.
  - The example uses only the inputs the requirement names. Pattern combinators stay a non-goal.
- RH6016-002 — A side across a line break and other `?` symbols are not reported — clarified (example only).
  - `int? n = null;` is now `int ? n = null;`, so the nullable `?` has whitespace next to it.
  - Re-derived: RH6016 takes a `?` only when its parent is a `ConditionalAccessExpressionSyntax`. The harness reports nothing for `int ? n` or `int  ?[] m`.
- RH6021-005 — A comment in a rewritten gap withholds the fix — clarified (`Why:` only).
  - The old reason said that inserting the space changes a placement the author chose, but the formatter inserts that very space (FMT-SPACE-013).
  - The new reason is the one SHARED-GAP-001 gives: the fix does not decide what changes next to a comment, and the author decides where a comment goes (`documentation/rules/general-notes.md`).

Not changed: the skeptic's optional point about folding RH6016-005 and RH6021-005 into the fix requirements, as RH6006-003 does.
- `sdd/templates/analyzer-spec.md` puts "what the fix produces" and "when the fix is withheld" in separate requirements, and RH6006-003's folded form is not a rule. Both forms pass the checks.
- RH6021-005 has a case where the fix is withheld (`class C /* note */: B`). RH6016-005 states the other half of SHARED-GAP-001: a comment on a side the fix does not change keeps the fix.
- No ID is added or removed, and `sdd/specs/requirement-ids.txt` is unchanged.

Checks:
- I re-ran my example harness on RH6016, RH6021, and RH6024. Every reported span and every before/after pair matches the real output, and Fix All in one pass equals the single fixes in every block.
- None of the example blocks of the 14 specs contains a shape from the skeptic's N2 table: every block passed the Fix All comparison in this round or the earlier ones.
- `scripts/test.sh --project architecture`, filtered to `SpecConsistencyTests`, `SpecStructureTests`, `RuleDocumentation*`, and `RequirementCoverageTests`: 38 pass. `EveryRequirementOfAnEnforcedSpecIsCovered` fails only on the 49 new IDs, which no test tags yet.

## Examples for the tests

- Unchanged from `01-spec-author.md`.
- In addition, the new negative inputs of RH6024-002 and RH6016-002 above.
- The test implementer must not encode the current Fix All result for the skeptic's N2 shapes, or a no-op fix for Q2/N3. No spec example contains either.

## Open questions to file

_None new._ Q1–Q3 from `01-spec-author.md` and N1–N4 from `02-skeptic.md` stand as listed there.

## Decisions needed

_None._
