Role: spec author
Commit: a20fd132
Verdict: #1087: spec written; #1088: spec written; #1978: spec written

Follow-up stage. Inputs: `00-lead.md` (batch 1 answered "as recommended" = 1B, 2A, 3B; decisions 2, 3, 5), `01-spec-author.md`, `02-skeptic.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Both change docs are set to `Status: Accepted`. No rule page changes: 1B, 2A, and 3B are corner cases of what is reported or of the fix result, and the pages already say what they need (RH4117 lists `is (var a, int b)`; RH4105 names the `_` exemption).

## thoenissen/Reihitsu#1087

Change doc `sdd/changes/2026-11/local-casing-rule-split.md`: case 1 example split into three methods (it did not compile: CS0136, `_` declared twice in one scope); case 6 now shows the 1B inputs (`(var Third, int Fourth)`, `(int Number and > 0, string { Length: > 0 } Text)`, `({ } Any, string { Length: var Size })`), with *Before* confirmed by `scripts/apply-fix.sh RH4115` (6 renames) and `RH4117` (no-diagnostic) on today's code; *What* and *Decisions* record answer 1B; three alternatives added; a pointer to the SHARED-CASING-015 change (recorded in `range-variable-casing.md`, case 4).

### Changed requirements

- RH4115-002 — Variables of a deconstruction are not checked — changed (decision 5, skeptic choice 1a: it names no boundary of its own, only "a variable that RH4117-001 or RH4117-003 names"; first example gains `(int Positive and > 0, _)`, `(string { Length: > 0 } Text, { } Any)`; second example's `Size` now stands inside a positional pattern).
- RH4115-003 — The fix renames the variable to camelCase — changed (refers to SHARED-CASING-015).
- RH4117-002 — The fix renames the variable to camelCase — changed (refers to SHARED-CASING-015).
- RH4117-003 — Variables of a deconstruction pattern are in camelCase — changed (answer 1B: every variable that receives a whole element of a positional pattern — the designation of a `var`, declaration, recursive, or list pattern that is a positional subpattern's pattern or stands in it only behind `and` and parentheses; RH4115 keeps property-pattern variables, list-pattern element and slice variables, a whole-pattern variable that is not an element, and simple pattern variables). Why rewritten to match. Examples extended on both sides.
- RH4117 non-goals — the RH4115 line narrowed to match.

### Examples for the tests

RH4115-002 (both blocks), RH4117-003 (both blocks; the second marks `Whole` as RH4117 next to six RH4115 variables), RH4117-002 (unchanged examples).

Boundary facts checked with a Roslyn parse probe: `(int Wrapped)` as a positional subpattern and `(var N)` inside `((var N), _)` are `ParenthesizedPattern`s, not one-element positional patterns; `p is (var A)` is a parenthesized pattern too, so it stays with RH4115 (top-level, not an element).

### Output questions

_None._

### Internal choices

- RH4117-003 states the 1B boundary by the syntax tree (the pattern of a `SubpatternSyntax` of a positional pattern clause, reached through `and` binary patterns and parenthesized patterns) and gives spellings only as examples. Alternative: list `and`, recursive, list patterns as spellings — rejected (a list misses members). `or` and `not` need no mention: they cannot declare variables.
- A positional pattern nested inside a property or list pattern (`{ Pair: (var a, _) }`, `[(var a, _), ..]`) counts: the nearest owner decides. Follows from 1B's cause; no output question, since the maintainer's answer covers "every variable that receives a whole element of a positional pattern".

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1088

### Changed requirements

- RH4128-001 — Local constant names are in camelCase — changed (decision 5, skeptic choice 1a: "wherever that statement stands"; the container list is gone; example gains a constructor and an anonymous method).
- RH4128-002 — A local constant named `_` is not reported — unchanged text; answer 2A confirms it. Change doc *Decisions* records 2A.
- RH4128-003 — The fix renames the constant to camelCase — changed (refers to SHARED-CASING-015; `Uses` gains it).

### Examples for the tests

RH4128-001 (with the new constructor and anonymous-method constants), RH4128-002, RH4128-003.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1978

Change doc `sdd/changes/2026-11/range-variable-casing.md`, retitled *RH4105 checks the casing of query range variables, and casing fixes keep anonymous object member names*. It carries the SHARED-CASING-015 change for every casing fix (decision 3), because that is where the case arose (old RH4105-006). Case 2 now shows the fix result `select new { Value = value, Doubled = doubled }` instead of "no fix offered"; new case 4 lists the fix-result changes for RH4105 (lambda parameter), RH4115, RH4117, RH4128, and RH4106 (member access, and a member read by name, which goes from no fix to fixed), and names RH4101, RH4102, RH4107 to RH4114, and RH4120 as reached by the same cause. *Decisions* and *Alternatives* record 3B and reject 3A (withhold) and 3C (no fix).

### Changed requirements

- RH4105-003 — The fix renames the parameter to camelCase — changed (refers to SHARED-CASING-015).
- RH4105-006 — The fix renames the range variable to camelCase — changed (the withholding paragraph is removed; it refers to SHARED-CASING-015; the second example now shows the fixed output).
- RH4105 `Uses`: SHARED-CASING-015 added, SHARED-FIX-003 removed (the spec no longer cites it).

### Examples for the tests

RH4105-006 (both examples), RH4105-003 (unchanged).

### Output questions

_None._

### Internal choices

- SHARED-CASING-015 lives in `range-variable-casing.md` rather than a third change doc; `local-casing-rule-split.md` points to it. Alternative: its own change doc — rejected, the prompt keeps two docs and the case came from this issue.

### Follow-up candidates

See *Whole run*.

## Whole run

### SHARED-CASING-015 — The fix keeps the name of an anonymous object member (added)

Stated once in `sdd/specs/shared/casing.md`, by its cause: the member has no `Name =` and the compiler takes its name from the identifier of a renamed reference — the member's whole expression (`new { Value }`), or the name that ends a member access or conditional access that is the whole expression (`new { this.Value }`, `new { other?.Value }`). The fix writes `OldName = ` in front of the renamed expression; other trivia stays. A member with its own name, or named after another identifier (`new { Value.Length }`), is only renamed. Ledger entry inserted. A new shared non-goal: tuple element names inferred from a renamed reference (a read by old name still withholds through SHARED-CASING-009).

Reading of answer 3B: the answer says "every casing fix (RH4105, RH4106, RH4115, RH4117, RH4128)". I read the list as the examples it was asked with, not as a limit, and stated the requirement for every fix that uses the shared rename, because the cause does not depend on the symbol kind and the shared base implements it once (refuse by the cause). That reaches RH4101, RH4102, RH4107 to RH4114, and RH4120 too, listed in case 4 of `range-variable-casing.md`. If the lead reads the list as exhaustive, those rules would keep the old rename, which is a visible difference: then it is an output question, and the requirement would need a list of rules.

Also reading 3B: "the renamed name is the member's whole expression" covers `new { this.Value }` and `new { other?.Value }` only by cause (the compiler infers the name from that identifier); both were checked to compile and to infer the name (`new { x?.Value }` compiles).

Evidence (`scripts/apply-fix.sh` on today's code, LF and CRLF agree): RH4105 `Select(Value => new { Value })` → `new { value }`; RH4115 `new { Total }` → `new { total }`; RH4115 on a const `new { MaxValue }` → `new { maxValue }`; RH4117 `new { First, second }` → `new { first, second }`; RH4106 `new { this.Value }` → `new { this._value }`, and with `item.Value` read → no-fix-offered; RH4101 `new { Color.red }` → `new { Color.Red }`; RH4111 `new { other.value }` → `new { other.Value }`. RH4102 and RH4120 were not run.

Existing test that the change reverses: `RH4106PrivateFieldCasingAnalyzerTests.NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField` (tagged SHARED-CASING-009) expects no fix; under SHARED-CASING-015 the fix is offered and gives `var x = new { Value = _value }; return x.Value;`.

### Examples for the tests

SHARED-CASING-015 (both examples: RH4105 plus RH4115 in one; RH4106 with `this.Value`, `other?.Value`, and a read by name).

### Checks

- All 54 C# blocks of the four rule specs, both change docs, and SHARED-CASING-015 compile (scratch Roslyn compilation, preview language, `allowUnsafe`), after the case 1 fix above.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed, the same three as after `01-spec-author.md` and expected until tests and code exist: `AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType` (RH4128 types), `EveryRequirementOfAnEnforcedSpecIsCovered` (now also SHARED-CASING-015).

### Output questions

_None._

### Internal choices

- SHARED-CASING-015 is a requirement of its own, not a bullet of SHARED-CASING-007 or -009: it is a rewrite the fix adds, not a coverage limit or a withholding.
- Only the run's specs (RH4105, RH4115, RH4117, RH4128) refer to SHARED-CASING-015; the other casing rule specs are not edited, since the requirement states that it holds for every casing fix. Alternative: add it to every casing rule's fix requirement and `Uses` — rejected as churn outside the run.

### Follow-up candidates

**RH2001 · Private auto-implemented properties should not be used — anonymous member named after the property.** RH2001-011 withholds the fix when a reference to the property gives an anonymous-object member its inferred name (`new { Value }`, `new { this.Value }`). After this run, the casing fixes keep such a member by writing `Value = _value`; RH2001's rename to a field could do the same. Input:

```csharp
class C
{
    private int Value { get; set; }

    object M() => new { Value };
}
```

Today: RH2001 reported, no fix offered. Reading A (unchanged): no fix. Reading B: the fix converts the property and gives `new { Value = _value }`. Recommendation: B, for consistency with SHARED-CASING-015, but it is outside the run's issues and the maintainer's to decide.
