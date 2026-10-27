Role: spec author (repair round after the skeptic)
Commit: 64dd31e
Verdict: spec written (all seven issues; clarifications, no change doc)

Inputs: `02-skeptic.md`, `squad/decisions/2026-10-27-withhold-unsafe-fixes-skeptic-round.md` (answers 1A, 2B, 3B, 4C, 5A, 6A, 7B, 8A), `00-lead.md`, and the specs as of 64dd31e. Statements about today's output come from the skeptic's runs or from the fixtures in `01-spec-author.md`. No new fixtures were run in this round.

## thoenissen/Reihitsu#1136

### Changed requirements

- SHARED-FIX-003 — clarified (brace split narrowed to the maintainer's clarification in `00-lead.md`, as the skeptic found). A pair is split when the fix moves one directive across a brace while its partner stays, or adds a pair of braces with one brace between the two directives and the other outside them. A pair of braces added wholly between the two directives, or wholly outside them, does not split the pair. New counter-example "a pair of braces added wholly between the two directives of a pair, for RH7004": the `#pragma` pair is around the declaration and every following statement, and the fix is offered. The skeptic observed today that both pragmas stay outside the new block.
- SHARED-FIX-003 — clarified (answer 3B). The removal exception now covers only a diagnostic that reports a whole region on its `#region`, as RH7310 does. A diagnostic that reports one region directive (RH7303) is named as not such a report.
- SHARED-FIX-003 — clarified (answer 2B). The *Run time* bullet now says that the instance fields of a layout type include the backing fields of auto-properties, of properties that use `field`, and of instance field-like events.

### Examples for the tests

SHARED-FIX-003, the new RH7004 counter-example. The skeptic's axis matrix applies: a brace pair with one brace between the two directives and the other outside is withheld; a pair wholly between them is not withheld by this sentence; a pair wholly outside is not withheld.

### Open questions for the maintainer

_None._

## thoenissen/Reihitsu#1134

### Changed requirements

- RH7109-005 — clarified: "Unless RH7109-006 withholds it".
- RH7109-006 — clarified: the fix is also withheld when the move would reorder the instance fields of a type whose field order sets its layout (SHARED-FIX-003). `Why:` and `Uses` (SHARED-FIX-003) updated. A closing sentence names the cases that withhold the fix: the SHARED-FIX-003 struct example, its `record struct` and `ref struct` forms, and a class declared with `[StructLayout(LayoutKind.Sequential)]`. This resolves the skeptic's note that RH7109, read on its own, contradicted SHARED-FIX-003.

### Examples for the tests

The SHARED-FIX-003 layout examples (default struct withheld; static fields offered) and RH7109-006. The skeptic lists further members to test.

## thoenissen/Reihitsu#1135

### Changed requirements

- RH7309-006 — clarified by answer 2B: bullet 2 inherits the backing-field sentence of SHARED-FIX-003. New example: an auto-property `Handle` in a layout struct, fix withheld.

### Examples for the tests

The RH7309-006 initializer example, the struct example, and the new auto-property example.

## thoenissen/Reihitsu#1262

### Changed requirements

- RH7309-004 — clarified (answer 6A). The claim now also covers bodies with comments above a block's `#region` line (with only blank lines between), and states that such a comment stays above its block when the block keeps its place among the blocks. New example: `// The fields` above `Fields`, with `Methods` moved in front of `IDisposable`. Today's result was verified in round 1 (`unmoved.cs`).
- RH7309-006 — clarified (answers 5A and 6A):
  - Bullet 3 now covers a comment above the `#region` line of a block that moves, with nothing but blank lines between them. A block moves when its place among the blocks changes.
  - The coverage sentence is replaced by "does not withhold the fix (RH7309-004)" for a block that keeps its place.
  - New example: the blank-line variant, withheld.
- RH7303-005 — The fix is withheld for a region that reaches between declarations — added (answer 3B). No fix for a reported directive whose partner lies between declarations. The example covers both shapes, the decided one and the mirror. `Uses` gains FMT-REGION-003.
- RH7306-003 — clarified (answer 1A). The partner is judged by its description (SHARED-REGDESC-001), as RH7306-001 judges it, not by its line.
- RH7306-004 — clarified (answer 7B). It also withholds the fix of a reported directive whose partner's description is the word alone. New example: `#region IDisposable implementation` with `#endregion // implementation`, no fix for either directive.

Out of this run (answer 4C): the regions that `RegionDirectiveUtilities.GetTopLevelRegions` counts although they belong to a nested enum or lie inside an expression. No spec in this run claims them, and no example uses them. In `01-spec-author.md` I listed them as needing no spec change and implied the run would fix them; that statement is withdrawn. The follow-up issue the lead files covers the helper and the SHARED-TYPEREGION-005 gap (equals-value clause, arrow expression, attribute argument, parameter default).

### Examples for the tests

RH7303-005; RH7306-004, both examples; RH7309-004, the kept-place example; RH7309-006, the blank-line example.

### Open questions for the maintainer

_None._ Q1, Q2 and Q3 are answered (5A, 6A, 7B) and written into the specs.

## thoenissen/Reihitsu#1332

### Changed requirements

None in this round. RH7501-004 stands as written in `01-spec-author.md`; the skeptic found it consistent.

## thoenissen/Reihitsu#1336

### Changed requirements

- SHARED-MODORDER-003 — clarified (answer 8A). A comment between the last modifier and the token after it also withholds the fix, like a directive there. New example with both members the skeptic named: a comment on its own line, and a `/* */` comment in front of the type on the next line.

No spec change for the skeptic's wider RH7004 member set (`using /* c */ var …`, a comment in front of `;`): dropping a comment is already SHARED-FIX-003 condition 3.

### Examples for the tests

SHARED-MODORDER-003, the new comment example together with the directive example.

## thoenissen/Reihitsu#1340

### Changed requirements

None in this round. A namespace nested only in an excluded `#if` branch is disabled text, not a member, so RH7003-005 does not apply to it; it stays a follow-up candidate, as the skeptic notes.

## Whole run

- Files changed in this round: `sdd/specs/shared/code-fixes.md`, `sdd/specs/shared/modifier-ordering.md`, `sdd/specs/analyzer/RH7109.md`, `RH7303.md`, `RH7306.md`, `RH7309.md`, `sdd/specs/requirement-ids.txt` (RH7303-005 inserted in sorted order).
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 50 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which lists exactly the six new requirements still waiting for tests: RH7003-005, RH7101-006, RH7303-005, RH7304-005, RH7306-004, RH7309-006.

### Open questions for the maintainer

_None._

### Architecture findings

_None new._ The RH7306 finding is decided (1A), and RH7306-003 now states the single judgment the shared predicate implements. The note from round 1 still applies: the layout predicate (RH7109, RH7309) and the initializer-order predicate (RH7103, RH7109, RH7309) each need one owner, or a copy in a fix becomes a second owner (FC-015).
