Role: skeptic
Commit: e464685
Verdict: proceed narrower

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issue text itself; the issue's prescribed solution (one shared component, "comparison by binding as the shared rule") is treated as a proposal and challenged below.

## Premise

Reachable: yes. Both families are code fixes (`CasingCodeFixProviderBase<T>.RegisterCodeFixesCoreAsync` -> `HasRenameConflictAsync` -> `RenameWithoutConflictAsync`, and the casing Fix All through `ApplyFixAllAsync`/`CandidatesConflictAsync`; `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.RegisterCodeFixesCoreAsync` -> `RenameChangesBindingAsync`). The CLI and the formatter are not involved.

The issue's claim "two components decide the binding part of SHARED-CASING-009 differently" is true. The specs state the binding rule once: RH2001-011 adopts the two binding paragraphs of SHARED-CASING-009 by reference and states that "the other paragraphs of SHARED-CASING-009 are not part of this requirement" (also recorded in `sdd/changes/2026-11/rh2001-rename-binding.md`). The code implements it twice: the casing base by a text diff of the renamed tree (`CasingRenameSafetyUtilities.RewritesOtherCodeAsync`) plus a reference count; RH2001 by a per-identifier binding/qualifier comparison (`RenameChangesBindingAsync`, `GetOtherSymbolReferencesAsync`). RH2001 does not call `CasingRenameSafetyUtilities` at all.

The two inputs, run on the current tree (`scripts/apply-fix.sh <ID> <dir>`, LF and CRLF identical):

**Input 1 — no longer a disagreement.** Both families withhold the fix today; the issue itself notes RH2001 gained its inferred-name guard.

```csharp
class Sample
{
    private int Value;                       // RH2001 fixture: private int Value { get; set; }

    int Get()
    {
        var x = new { Value };
        return x.Value;
    }
}
```

`RH4106`: no-fix-offered. `RH2001`: no-fix-offered.

**Input 2 — reproduces, and the casing side is the one that is wrong.** The deconstruction local is irrelevant; a plain local gives the same result.

```csharp
class Sample
{
    private int Value;                       // RH2001 fixture: private int Value { get; set; }

    int Get(int other)
    {
        var _value = 1;                      // also: var (_value, other) = pair;
        return Value + _value + other;
    }
}
```

`RH4106`: no-fix-offered. `RH2001`: fixed, `return this._value + _value + other;`.

SHARED-CASING-009 decides this input: "A reference that the renamer renames, or renames and qualifies with `this.`, `base.`, a type, or a namespace, does not withhold the fix", and "So the fix changes no code other than the declarations of the rename cascade, the names of its references, and the qualifiers in front of those names." The renamer's output above is exactly that. RH2001-011 (paragraph on a symbol named like the field) demands RH2001's result too. So the casing fix is an over-refusal against an unchanged requirement: a guard-scope defect (`AGENTS.md`, *Code-fix coverage*), not unsafe output. No safety rail is involved.

**A disagreement the issue does not mention, and which is intended.** Tuple elements with an inferred name:

```csharp
class Sample
{
    private int Value;                       // RH2001 fixture: private int Value { get; set; }

    int Get(int input)
    {
        return (Value, input) switch { (1, 2) => 3, _ => 0 };
    }
}
```

`RH4106`: fixed (`(_value, input) switch …`). `RH2001`: no-fix-offered. Same for `var pair = (Value, 1); return pair.Item1;`. RH2001-011 withholds these positions as a rare-class refusal, and the tuple `switch` by a maintainer decision (`rh2001-rename-binding.md`, *Decisions*); SHARED-CASING-009 withholds only on a compile error, and none occurs. The families are specified to differ here.

## Mechanism and candidates

Mechanism: `CasingRenameSafetyUtilities.RewritesOtherCodeAsync` attributes each change that `SyntaxTree.GetChanges` reports to the original tokens it overlaps or touches, but `GetChanges` is a heuristic diff, not the renamer's own edit list; when the renamer qualifies a reference of the cascade and a token spelled like the new name (the inner-scope symbol that forced the qualification) stands later in the changed region, the reported change can land on that token, which carries no old name, and a rename that only renamed and qualified is read as a rewrite of other code.

This is FC-044 in shape: the guard reconstructs what the Renamer changed from a diff instead of reading what the Renamer computed, so which token sequences trip it is a property of Roslyn's differ, not of a syntactic cause (see the irregular boundary in the matrix).

Dispatch: the casing check is generic over `T`, so every casing rule whose rename the renamer can qualify is a candidate — member renames qualified with `this.` or the type (RH4101-RH4114: fields, properties, methods, events), and type renames qualified with a namespace (RH4002-RH4012, RH4121 cases where a nested type or other symbol captures). Renames of locals, parameters, tuple elements, and deconstruction variables (RH4105, RH4115-RH4127) are never qualified and are not candidates. RH2001 is a single rule over `PropertyDeclarationSyntax`; it does not fail on this class.

## Breaking inputs

Failure classes that reach the change: FC-015 (a policy with two owners), FC-016 (sibling shapes), FC-044 (a guard models a Renamer result), FC-008 (casing Fix All runs the same check), FC-036 (not affected: no rename semantics change).

Class matrix for the casing over-refusal. Axes: rule family (field `_camelCase`, property/method PascalCase, type), qualifier (`this.`, type, namespace), capturing symbol (local, deconstruction local, parameter, lambda parameter, local function), and the token sequence around the qualified reference. Every row is a `class Sample` with the reported declaration and `int Get(int other)` unless stated; all run with `scripts/apply-fix.sh`, LF and CRLF identical.

| # | Rule | Capturing symbol | Expression | Result | Fails? |
|---|---|---|---|---|---|
| 1 | RH4106 field `Value` | local `_value` | `Value + _value + other` | no fix | **fails** |
| 2 | RH4106 | deconstruction local `_value` | `Value + _value + other` | no fix | **fails** (issue's input) |
| 3 | RH4106 | local `_value` | `Value + _value + 1` | no fix | **fails** |
| 4 | RH4106 | local `_value` | `Value + _value + _value` | no fix | **fails** |
| 5 | RH4106 | local `_value` | `Value * _value * Value` | no fix | **fails** |
| 6 | RH4106 (`Box`, `Write(object)`) | local `_value` | `box.Write(Value + _value + 2)` | no fix | **fails** |
| 7 | RH4111 property `value` | local `Value` | `value + Value + other` | no fix | **fails** |
| 8 | RH4103 method `getValue()` | local `Func<int> GetValue` | `getValue() + GetValue() + other` | no fix | **fails** |
| 9 | RH4103 method `getValue()` | local function `int GetValue() => 2;` | `getValue() + GetValue() + other` | no fix | **fails** (capturing symbol violates no rule) |
| 10 | RH4106 | local `_value` | `Value + _value` | `this._value + _value` | holds |
| 11 | RH4106 | local `_value` | `_value + Value + other` | `_value + this._value + other` | holds |
| 12 | RH4106 | local `_value` | `other + Value + _value` | `other + this._value + _value` | holds |
| 13 | RH4106 | local `_value` | `Value + _value - Value` | `this._value + _value - this._value` | holds |
| 14 | RH4106 | local `_value` | `Use(Value, _value)` | `Use(this._value, _value)` | holds |
| 15 | RH4106 | local `_value` | `var a = Value; var b = _value;` | `var a = this._value;` | holds |
| 16 | RH4106 | parameter `_value` | `Value + _value` | `this._value + _value` | holds |
| 17 | RH4106 static field | local `_value` | `Value + _value` | `Sample._value + _value` | holds |
| 18 | RH4106 | local `_value` | `Value = _value;` | `this._value = _value;` | holds |
| 19 | RH4111 | local `Value` / parameter `Value` | `value + Value`, `value = Value;` | `this.Value + Value`, `this.Value = Value;` | holds |
| 20 | RH4103 | local function `GetValue` | `getValue() + GetValue()` | `this.GetValue() + GetValue()` | holds |
| 21 | RH4002 class `order` | method `Order()` in `X` | `order.Create() + Order() + other` | `N.Order.Create() + Order() + other` | holds |

The boundary between rows 1/3/4/5 and 10/13 is not syntactic; it follows the differ. Fixture for row 9, the one member whose capturing symbol violates no Reihitsu rule:

```csharp
class Sample
{
    private int getValue() => 1;

    int Get(int other)
    {
        int GetValue() => 2;

        return getValue() + GetValue() + other;
    }
}
```

`scripts/apply-fix.sh RH4103 <dir>`: no-fix-offered. Expected under SHARED-CASING-009: `private int GetValue() => 1;` and `return this.GetValue() + GetValue() + other;`.

Rarity: every failing member needs a symbol of an inner scope spelled exactly like the corrected member name. For the `_camelCase` fields and for PascalCase targets captured by a local or parameter, that symbol violates RH4115, RH4117, or RH4105 itself, so it is rare by definition; a local function named like the corrected method (row 9) is the conforming exception, still uncommon. On top of that it needs a token sequence the differ misaligns.

Existing coverage (`README.md`, *Coverage first*):

- Input 1, RH2001: pinned — `RH2001PrivateAutoPropertiesShouldNotBeUsedNameBindingTests.VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName` (and the `ToString` and `this.` variants).
- Input 1, RH4106 (`new { Value }` + `x.Value`): none found.
- Input 2, RH2001: the two-operand shape is pinned by `VerifyFixQualifiesThePropertyReferenceNextToALocalNamedLikeTheField` (`return Value + _value;` -> `return this._value + _value;`); the issue's three-operand shape: none found.
- Input 2 class, casing: no casing test pins a `this.`/type-qualified rename that keeps its fix next to a local. The namespace-qualified case is pinned by `RH4002ClassNameCasingAnalyzerTests.FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` (holds). Rows 1, 2, 7, 9 and one holding row each (10, 17) are the inputs the test implementer must cover; none found for any of them.
- The rewrites that must stay withheld are pinned in `RH4103MethodNameCasingNameBindingTests` (`box.Write((object)1)`, `Extensions.ToList(values)`) and the RH4105 qualifier examples; any change to `RewritesOtherCodeAsync` must keep them.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `CasingRenameSafetyUtilities.RewritesOtherCodeAsync` (casing single fix and Fix All) | Must change, if the defect is fixed: it is the one place the over-refusal comes from |
| `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and its other checks | Must stay |
| RH2001 code fix (`RenameChangesBindingAsync` and its helpers) | Must stay: correct on both inputs per RH2001-011 |
| Analyzers (RH2001, RH41xx, RH40xx) | Not involved |
| Formatter, CLI, `Reihitsu.Core` | Not involved |
| SHARED-CASING-009, RH2001-011 | Must stay; both already decide input 2 |
| Rule pages | Not involved |
| Tests | New casing tests for the matrix rows above; one characterization test for input 1 on RH4106 |

## Refuse option

Refusing is today's behavior: the casing fix is withheld for the input-2 class, a supported end state, and the class is rare. Cost: zero production lines. But it leaves the code contradicting an unchanged sentence of SHARED-CASING-009, and the refusal cannot be written into the spec as a decision because its membership is a property of Roslyn's differ, not a cheap check on a cause. A cause-based refusal ("withhold whenever the renamer qualifies a cascade reference next to an inner-scope symbol with the new name") would withhold rows 10-20, which are fixed today and partly pinned by tests (RH4002 nested type; RH2001 is unaffected) — a visible output change I do not recommend.

Declaring a shape unsupported: not an option. The decision is semantic (what the Renamer rewrites), not a shape the formatter precheck can see; no formatter special case would be deleted.

Requested change (the issue's extraction): one component, "comparison by binding as the shared rule", called by both families. Cost and problems:

- A binding comparison alone does not enforce SHARED-CASING-009 for the casing fixes. Its first example — `box.Write(1)` rewritten to `box.Write((object)1)` — and the cascade-call rewrites (`values.ToList()` -> `Extensions.ToList(values)`, a cast argument on a call of the renamed method) keep every binding; only the text check catches them. So the text check stays in the casing path in any case, and the over-refusal stays unless that check is changed anyway.
- RH2001's comparison is scoped to one private property in the documents of its type, matched by the new name only. Generalizing it to a solution-wide cascade across projects, with old and new names, constructed symbols, and `cref`s, is well over 100 production lines and a new type: escalation 9.
- "Inputs 1 and 2 give the same verdict from both families" is reachable without the extraction (input 1 already agrees; input 2 agrees once the casing check stops over-refusing). "The two families withhold the same inputs" as a general goal conflicts with RH2001-011's tuple positions and the maintainer's tuple-`switch` decision.

Simpler option: a narrow change inside `RewritesOtherCodeAsync` so that a change consisting only of a renamed reference and the qualifier the renamer put in front of it is recognized whatever the differ's alignment, keeping the pinned rewrite cases withheld. Its size looks like one method, no new type; whether it can be done by reading the renamer's computed result rather than modelling the differ (FC-044) is for the implementer. If it cannot be made complete with a cheap check, the fallback is the refuse option above (no change, issue closed with `Refs`).

## Output questions

_None._ Fixing the casing over-refusal brings the code back to an unchanged SHARED-CASING-009 sentence (nothing in question). Keeping the refusal changes nothing. Adopting RH2001's tuple verdicts in the casing fixes, or the casing verdicts in RH2001, would put outputs in question; I recommend neither and the issue does not need either.

## Internal choices

1. **Fix the casing over-refusal narrowly vs. leave it.** Fix narrowly: casing offers the fix for rows 1-9, as SHARED-CASING-009 demands; costs tests and a change in one method that guards a safety-relevant rule. Leave it: zero cost, a known spec deviation in a rare class remains. Recommendation: fix narrowly if the implementer can do it without modelling the differ; otherwise leave it and say so in the PR.
2. **Extract a shared component now vs. follow-up vs. drop.** Recommendation: drop it from this run (escalation-9 size, and its stated shared rule is wrong for the casing fixes); if the lead still wants one owner, file a follow-up that scopes the shared part to the reference-binding comparison only, keeping the casing text check and RH2001's inferred-name guard as they are.

## Issue goals

- "One component decides … the casing fixes and RH2001 both call it": drop. Not needed to make the inputs agree; its prescribed rule (comparison by binding) would weaken SHARED-CASING-009 enforcement in the casing fixes; escalation-9 size. No visible output change from dropping it.
- "Inputs 1 and 2 give the same verdict from both families": keep, it is the real purpose. Input 1 is already met on `main`. Input 2 is met by the narrow casing change (casing then offers the fix, RH2001 unchanged). The visible change is the casing fix offered for the input-2 class, demanded by SHARED-CASING-009.
- "The shared component has its own tests": drop with the first goal.
- "Existing RH2001 and casing code-fix tests stay green": keep.
- The issue's framing of input 1 ("RH2001 produced CS1061") is stale; that was fixed by the RH2001-011 inferred-name guard.

## Architecture findings

Components: `CasingRenameSafetyUtilities.RewritesOtherCodeAsync` + `CountReferenceLocationsAsync` (via `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`) and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.RenameChangesBindingAsync`. Shared decision: whether a rename changes the binding of, or rewrites, a reference to a symbol outside the rename cascade (the two binding paragraphs of SHARED-CASING-009, which RH2001-011 adopts by reference). Input showing them disagree: input 2 / matrix rows 1-9 (casing withholds, RH2001 offers; RH2001 is right). The disagreement is an over-refusal of a rare class, and the casing guard reconstructs the Renamer's edits from a heuristic diff (FC-044). The intentional tuple-position difference is spec-decided and not part of the finding.

## Follow-up candidates

- If the lead keeps the refusal instead of the narrow fix: an `agent-filed` issue for the casing over-refusal (rows 1-9), naming `RewritesOtherCodeAsync`'s reliance on `SyntaxTree.GetChanges` as the cause.
- If the lead wants one owner later: an `agent-filed` issue scoped to sharing the reference-binding comparison only, stating that the casing text check (cast arguments, static-call rewrites) and RH2001's inferred-name guard remain separate by spec.
- Not checked: whether RH2001 under-refuses anywhere that the casing compile-error check would catch, beyond input 1. Its scope (a private, non-overriding property; only the documents of its type) leaves little room, and I found no such input; no candidate filed.
