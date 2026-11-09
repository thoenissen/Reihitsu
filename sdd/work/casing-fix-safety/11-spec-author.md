Role: spec author
Commit: 514cfd0
Verdict: #1597: spec written; #1598: nothing to do; #1016: nothing to do; #1015: nothing to do; #1084: nothing to do

Repair round 1, lead decision 13 (auditor finding 1). The prompt named the repair and called the auditor's suggested change a suggestion; it carried no intended fix or hoped-for verdict beyond that. I re-derived the requirement and departed from the suggestion in two places (Internal choices 2 and 3). Clarification of how the unchanged SHARED-FIX-003 *Run time* is met; withholding only, for a rare class; no change doc, no rule page change (general-notes.md already covers a diagnostic without a fix).

## thoenissen/Reihitsu#1597

### Changed requirements

- SHARED-CASING-012 — The fix is withheld when a caller-argument expression names the old or the new name — clarified (`sdd/specs/shared/casing.md`):
  - inspected lists: a parameter list that declares a cascade symbol, **and** the parameter list of every member of an extension block whose receiver parameter is in the cascade;
  - every argument whose constant value is an old or the new name withholds the fix, `nameof` included, unless exempt;
  - exempt only when all hold: a `nameof` whose operand is a single unqualified identifier; that identifier is the old name of a cascade parameter the attribute's own parameter list declares; the document is compiled with C# 11 or later;
  - *Why* rewritten: the renamer changes the value only for a `nameof` that refers to the renamed parameter; the old statement "a `nameof` argument is a reference, which the rename changes" is removed; the check is stated as a cheap one (constant values and the `nameof` shape, never its binding), citing AGENTS.md *Special cases*.
- RH4105-003, RH4120-003 — clarified: the closing sentence that restated SHARED-CASING-012 ("names by a string") now refers to it without restating: "A parameter whose rename could change which parameter a `CallerArgumentExpression` attribute names, or whether it names one, is left to manual correction as SHARED-CASING-012 specifies."

### Examples for the tests

SHARED-CASING-012, all RH4105, each single fix (Fix All optional, as for the existing rows):
- (a) `nameof(Names.Condition)` with `Names.Condition` a `const` in another class — no fix.
- (b) `private const int condition = 0;` and `nameof(condition)` — no fix.
- (c) extension block `extension(bool Condition)` whose member carries `[CallerArgumentExpression("Condition")]` — no fix; same with `nameof(Condition)` in the member's list — no fix; control: the block without such an attribute — fix offered, receiver renamed to `condition`.
- (d) C# 10 (`LanguageVersion.CSharp10`) with `public const int Condition = 0;` in the type and `nameof(Condition)` — no fix. Other side: the existing `nameof(Condition)` example under the default (C# 11+) language version — fix offered, both renamed (already tested).

Verified in the scratchpad (`dotnet run`, file-based): under C# 10, `nameof(Condition)` with a `const int Condition` in the type compiles and binds to the field; under C# 14, `nameof(Condition)` binds to the parameter in a method, in a delegate, and to the receiver in an extension member.

### Output questions

_None._ Every change only withholds a fix for a rare class (SHARED-FIX-003; lead decision 13).

### Internal choices

1. Rule specs refer to SHARED-CASING-012 rather than restate its condition. Alternative: widen their restatement to list `nameof` and extension blocks — rejected as restating a shared rule.
2. Added the C# 11 condition to the exemption (not in the auditor's suggestion). Before C# 11 an unqualified `nameof` in a parameter attribute binds to a type member of that name, not to the parameter, so the exemption would let finding 1's failure through again. The check reads the syntax tree's parse options, which is cheap. Alternative: no version condition — rejected, it leaves a known SHARED-FIX-003 violation.
3. An extension-block member's attribute is never exempt, `nameof(Receiver)` included, although that `nameof` binds to the receiver (verified) and the renamer probably renames it. Alternative: also exempt an unqualified identifier equal to the receiver's old name — rejected because whether the renamer changes that reference was not verified, and the class (C# 14 extension block + PascalCase receiver + CallerArgumentExpression naming it) is rare enough that a conservative refusal costs nothing worth keeping.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1598

Nothing to do in this round.

## thoenissen/Reihitsu#1016

Nothing to do in this round.

## thoenissen/Reihitsu#1015

Nothing to do in this round.

## thoenissen/Reihitsu#1084

Nothing to do in this round.

## Whole run

- Files: `sdd/specs/shared/casing.md`, `sdd/specs/analyzer/RH4105.md`, `sdd/specs/analyzer/RH4120.md`. No new requirement IDs.
- `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` → Passed 51, Failed 0.
- No change doc; no rule page change.
