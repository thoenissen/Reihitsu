Role: skeptic
Commit: b7588a6
Verdict: proceed narrower

S1, S2, S4, S5 and S6 are resolved. S3 is resolved for the input I reported, but RH7303-004 still covers one more shape the formatter changes (R1). Three question texts in `05-spec-author.md` need small corrections (R2–R4). None of this needs the maintainer.

I checked only the changes from f9a7832 to b7588a6. The coordinator's message named the scope and nothing else. Evidence: `scripts/apply-fix.sh` (LF and CRLF) and `scripts/format.sh` on scratch fixtures, since deleted, plus a reading of `RegionDirectiveUtilities`. The spec consistency and structure tests on b7588a6: 26 passed.

## Premise

What now holds against the code:

- **SHARED-TYPEREGION-006 (S1, S2).** The new condition leaves the nested-enum and initializer inputs out. Where it holds, the code's top-level regions are the ones the spec defines. Every remaining directive lies in executable code (which the code drops), in a nested type the code already excludes, or between the type's own members. No pair crosses a member boundary, so dropping the inner pairs does not change how the outer ones pair or nest. Excluding extension blocks as well is harmless.
- **SHARED-TYPEREGION-005 (S6).** The code decides by the token after the directive, the spec by position. For every position the spec names, the two give the same answer. Positions in neither list stay out of the spec: between a member's header and `{`, between modifiers, inside a parameter list.
- **RH7309-004 (S4).** A comment above the first block, between blocks, or after the last block is now excluded.
- **RH7310-004 (S5).** The class's own `}` is excluded. I also checked a case that is still covered: `void A() { }` / `#region Methods` / blank / `#endregion` / blank / `void B() { }`, with no blank line above the region. The fix leaves `A` and `B` adjacent, and `format.sh` does not change that.

## Mechanism and candidates

_N/A._

## Breaking inputs

**R1 — RH7303-004 still covers a statement ending with `}` next to a removed directive.** Input:

```
if (a)
{
}
#region Work
M(a);
#endregion // Work
M(a);
```

Both directives are reported, both lie between two statements, and neither has a blank line next to it. The fix gives `if (a) { }` / `M(a);` / `M(a);`, and `format.sh` then adds a blank line after the `}` (SHARED-BLANK-003). It is the same P10-versus-P9 question as Q4 of the RH7303 issue. The same happens when the last statement inside the region ends with `}`.

Either narrow RH7303-004 so that neither directive directly follows a statement that ends with a closing brace, or add the shape to that Q4. Narrowing matches how the other cases were handled.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH7303 spec | Must change (R1), or the shape goes to RH7303 Q4 |
| Open-question texts in `05-spec-author.md` | Must change (R2–R4) before the lead files them |
| Everything else in this revision | Must stay |

## Simpler option

_None._

## Questions for the maintainer

None. Corrections to the final question texts:

- **R2 — the RH7309 fix part of the initializer case is filed under the wrong spec.** "RH7303 issue, Q2, second part" describes the RH7309 fix moving `#region Values` / `1,` / `#endregion` out of the field initializer. That is a bug in the RH7309 fix against SHARED-FIX-003 (decided by P1), so it belongs in the RH7309 issue next to its Q2 (nested enum), with a cross-link from RH7303 Q2. The description itself is accurate.
- **R3 — the RH7203 Q1 title and "Why" are too broad.** "Hidden by any other non-`System` group" is not what the code does. Only a regular directive with another non-`System` root hides the pair. A `using static` directive or an alias of any root is filtered out before the comparison, so it never hides one: `using static Microsoft.X;` between `Microsoft.B` and `Microsoft.A` is reported (code reading), like Input C. Say "another regular group with a non-`System` root".
- **R4 — RH7302 Q1: "with a single space it offers nothing" is wrong.** No fix is offered only when the description is already capitalized (`# region Fields`). For `# region fields`, also with a single space, a fix is offered, and the diagnostic stays after it, as the table's second row shows.
- **Minor — RH7303 Q3 says "both directives sit between top-level statements".** The `#endregion` follows the last statement. "Both directives sit in top-level statement code" is accurate.

All other final texts are accurate, and their principle markings are right:
- #1043 Q1 and P7 (RH7302).
- P1 (RH7303 Q1, RH7304 Q1, RH7309 Q2).
- P11 (RH7303 Q2, Q3).
- P12 (RH7304 Q1).
- P7 and P9 (RH7304 Q2).
- P7 (RH7306 Q1, second part).
- P10 candidates (RH7303 Q4, RH7310 Q1).

The claim that RH7305 and RH7305A cannot change their verdict because of a nested enum's region also holds. The enum is itself a counted member, and it can only lie outside the regions inside it.
