Role: spec author
Commit: 312b615
Verdict: change doc written

Change doc: `sdd/changes/2026-11/conditional-group-fix-refusals.md` (`Status: Draft`). It is required: the three new readings withhold a few fixes that `main` offers and that change no build (listed under *Visible output changes*, measured with `scripts/apply-fix.sh` at `95aa3e6`). The issues' own inputs are withheld as bug fixes against SHARED-FIX-003, which is unchanged, as in `member-move-conditional-groups.md`. One doc covers all three issues: the shared cause is a conditional group that a fix's guard did not read.

The prompt carried no intended fix or verdict beyond the issues; the issues' "Affected" sections and proposed checks were treated as proposals.

## Changed requirements

- RH2001-010 — The fix is withheld for a conditional group in front of the first modifier that does not wrap whole members — added (thoenissen/Reihitsu#1666). Test: no nesting, and every excluded branch of the group, wherever it stands, parsed alone as type-body content, reads as whole members without a syntax error. The active branch is not parsed (a modifier there makes the next directive stand between modifiers, RH2001-009).
- RH2001-004 — The fix replaces the property with a private field — changed: its coverage excludes that group and points to RH2001-010.
- RH2001-009 — conditional directives between the modifiers — changed: the pointer for a directive in front of the first modifier now names RH2001-010; the last example says the fix stays because the group holds a whole member.
- SHARED-ORDER-008 — Only conditional groups of whole units keep a member move — changed:
  - "a declaration of the type": every declaration of a `partial` type, in any document, is read for a conditional directive in front of its body, by the same test (thoenissen/Reihitsu#1667);
  - a group with a directive in the body of the reordered declaration between its opening brace and the start of the moved/crossed span (crossed span starts at the target's first trivia; for RH7309 the first block whose place changes) that does not wrap whole units makes the type count as one whose field order sets its layout. So only moves that reorder instance fields are refused; a static or const move stays (thoenissen/Reihitsu#1665). This also covers a group whose first directive is in front of the spans and whose other directives are behind them, so the spans lie in one branch. Directives behind the spans are still not read;
  - Place bullet: nested types in front of the spans count as unit lists too;
  - the sentence "a conditional directive outside both spans … is not read" was replaced to say which ones are read.
- SHARED-ORDER-007 — Initializer execution order is kept — changed: the layout clause refers to SHARED-ORDER-008's wider set of directives instead of "a type whose declaration holds".
- SHARED-FIX-009 — A fix does not work out another build configuration — changed: the sentence that said RH2001's fix reads no excluded code now names RH2001-010 as reading excluded branches whole. `Uses` row gains RH2001-010.

RH7102, RH7103, RH7109, RH7309 specs need no change: each refers to SHARED-ORDER-008 as a whole. No rule page changes (withheld fixes only). `sdd/specs/README.md` code owners unchanged.

`scripts/test.sh --project architecture --filter Sdd`: all pass except `EveryRequirementOfAnEnforcedSpecIsCovered`, which needs a test tagged `[Requirement("RH2001-010")]`. That is the test implementer's job.

## Examples for the tests

- RH2001-010: `extern` (class) and `readonly` (struct) in the excluded branch; the excluded `#else` branch behind a whole member; the excluded `#if` branch in front of the active `#else` member (only `#endif` is in the first modifier's trivia); `[System.Obsolete]` in the excluded branch; and the counterpart, a whole member that keeps the fix. LF and CRLF.
- SHARED-ORDER-008, new examples at the end of the spec:
  - the partial class with conditional `[StructLayout(Sequential)]` on the other part (RH7109, no fix), and the plain-partial counterpart (fix);
  - the group in front of the spans that closes the type (RH7109, no fix) and the same group with a static move (RH7103, fix kept);
  - a modifier group in front of the spans (RH7109, no fix);
  - a whole-member group in front of the spans (RH7109, fix kept).
- The issue also names the partial struct with `#if !X [StructLayout(LayoutKind.Auto)] #endif` on the other part. The spec text names it; it is measured fixed on `main` and must be withheld.
- Per consumer for thoenissen/Reihitsu#1665: RH7309 should get a reorder test with the closing group in front of the first moved block. RH7102 and RH7103 moves reorder no instance field, so they keep the fix. The issue's "a test per consumer asserts no fix" is a proposal that the analysis narrows: only moves that reorder instance fields are unsafe.

## Output questions

The lead turns the three listed cases of the change doc's *Visible output changes* into output questions:

1. RH2001 with `[System.Obsolete]` in the excluded branch: fix on `main`, withheld after.
2. RH7109 on a partial class with conditional `[System.Serializable]` on the other part: fix on `main`, withheld after.
3. RH7109 with `#if DEBUG static #endif` in front of an uncrossed member in front of the spans: fix on `main`, withheld after.

Recommendation for all three: withhold, because the structural test cannot tell them from the harmful cases without working out branches (SHARED-FIX-009). No other open output question.

## Internal choices

- Groups in front of the spans make the type count as a layout type (refusing instance-field reorders only) instead of refusing every move. The alternative, refusing every move, would also withhold safe static/const moves.
- RH2001-010 uses SHARED-ORDER-008's whole-members parse plus its no-nesting clause. The alternatives were refusing every conditional group in front of the first modifier (no parse, but it loses the fix on a common shape that RH2001-009 shows as fixed) and a token-based text check (rejected as a parser-like rule).
- One change doc for the three issues. The alternative was one per issue.
- Declaring the shapes unsupported is not an option: the formatter has no special case for them that a declaration would delete, and the moving fixes' group lies outside the formatted node.

## Follow-up candidates

- **SHARED-ORDER-008, group of the declaration's first token: only the branch behind the directive is read** (bug against the spec as already written, outside these issues' inputs). This is the same function the implementer changes for thoenissen/Reihitsu#1667 (`OrderingDeclarationUtilities.DeclarationHoldsConditionalDirective`), so the lead may fold it in. The spec counts a directive in the first token's trivia that "belongs to a group with an excluded branch that does not read as whole members". The code reads only the excluded text that follows that directive in the same trivia, so it misses an excluded branch that stands in front of another declaration. Measured on `main` at `95aa3e6`, without `X` defined:

  ```csharp
  using System.Runtime.InteropServices;

  #if X
  [StructLayout(LayoutKind.Sequential)]
  #else
  public class D
  {
  }
  #endif
  public class C
  {
      private int _a;
      private readonly int _c;
  }
  ```

  Today, RH7109 moves `_c` in front of `_a`. In a build with `X`, that reorders `C`'s sequential layout. The spec says no fix is offered. RH2001-010 is written to read every excluded branch of the group, so the RH2001 implementation must not copy this narrower reading.
