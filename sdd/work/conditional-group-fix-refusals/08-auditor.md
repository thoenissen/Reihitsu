Role: auditor
Commit: fce5e462136ff29c883cfcc99c68cdaa5b5b1050
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Full audit (first audit of the run). Head `fce5e46`; base and `origin/main` `95aa3e6` (`git ls-remote` agrees), an ancestor of head, so `main` is merged.
- The diff matches the handovers: two production files (`OrderingDeclarationUtilities.cs`, the RH2001 code fix), five test files, specs, change doc, work files. `scripts/lane-check.sh behavior-change`: confirmed.
- Counterparts opened outside the diff: `TypeMemberOrderingCodeFixProviderBase` (RH7102/RH7103/RH7109), `RegionBlockLayout` and the RH7309 fix, `ExcludedCodeUtilities`, the RH2001 fix's other guards (`IsSupportedPropertyShape`, `HasConditionalDirectiveBetweenModifiers`, `KeepsEveryCommentAndDirective`).
- Executed: `scripts/apply-fix.sh RH2001` and `RH7109` on head over fixtures in the session scratchpad, LF and CRLF. One fixture was compiled with `NET` defined (`dotnet build`, net10.0) to show that the input is valid C#. Fixtures are quoted below.

## thoenissen/Reihitsu#1666

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` `ConditionalGroupsWrapWholeMembers` → `WrapsWholeUnitsOf(..., checksNesting: true, ...)`; tests `VerifyNoCodeFixIsOfferedForAGroupOfAWholeMemberInsideAnotherGroupInFrontOfTheFirstModifier`, `VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier` | check 10; FC-001 (guard wider than the decision) | – | undecided output | See *Finding 1* below. RH2001-010's "lies inside no branch of another group" withholds the fix on safe code that `main` fixes. This includes a common shape that neither the change doc nor a maintainer answer covers. | Ask it as an output question; #1666's own *Done when* says "if it withholds common code that `main` fixes, the maintainer decides". Or drop the "around" half of the nesting check for RH2001-010 only, which keeps `main`'s output. |
| 2 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` `TryCreateFieldDeclaration` (no reading of the trivia behind the property's last token) | FC-016, FC-020 | – | same requirement (SHARED-FIX-003; RH2001-004 coverage, which the run edits). Pre-existing on `main` | See *Finding 2* below. A conditional group behind the property whose excluded branch joins the declaration in another build gives a field that does not compile there. It is the remaining member of #1666's class. | Refuse this class: also withhold when the leading trivia of the token behind the property holds a group that does not wrap whole members (the same helper, one call). Or file it as a follow-up; the lead decides. |
| 3 | low | `sdd/changes/2026-11/conditional-group-fix-refusals.md`, *Visible output changes* | check 10 (rare classes) | – | undecided output (rare class, see text) | See *Finding 3* below. The list says each listed case is "a fix that main offers, which changes no build", and reads as complete. Further rare-class withholdings exist that are not listed. | List them in the change doc, or name them as rare classes under the PR's `Review notes` (squad README, *What reaches the maintainer*). |

#### Finding 1

RH2001-010's no-nesting clause has two halves:
- a group that holds another group;
- a group that lies inside a branch of another group.

The first half is needed: `GetExcludedText` reads only the disabled text directly behind each directive of the group, so a branch with a nested group would be read only in part. The second half protects nothing for this fix. When the property stands in the active branch of an outer group, every excluded branch of the inner group lies inside that active branch and is read whole. An excluded branch of the outer group never stands in front of the property in a build that compiles the property. A rename into excluded code is refused separately (`ExcludedCodeUtilities`).

Failing member, confirmed on head (`apply-fix.sh RH2001`, LF and CRLF: `no-fix-offered`). A file wrapped in `#if`, as in multi-targeting code:

```csharp
#define NET
#if NET
class C
{
#if DEBUG
    private int _trace;
#endif
    private int Value { get; }

    int Read() => Value;
}
#endif
```

On `main`, the fix is offered and writes `private readonly int _value;` behind the unchanged group. That is valid in every build: the guard on `main` does not read the first modifier's trivia. The test implementer recorded the class-body variant (`#if !RELEASE` around the property) as red on `main`, which confirms it.

The change doc's *Alternatives* calls a group of whole members above a property common (`#if DEBUG` fields above a property). Whole files under a target-framework `#if` are common in multi-targeting libraries.

Members:

| Member | Head | `main` |
|---|---|---|
| group inside an active branch: whole file | withheld | fixed, safe |
| group inside an active branch: region of the class body | withheld | fixed, safe |
| group that holds another group (`#if DEBUG #if TRACE … #endif #endif`) | withheld | fixed |
| whole-member group with no enclosing group | fixed | fixed (holds) |

The group that holds another group is rare, and the first half of the clause covers it; it falls under *Finding 3*, not this one.

No unchanged requirement demands these expected values. The three maintainer answers cover an attribute list (RH2001), a conditional attribute on another declaration of a partial type (RH7109), and a modifier in front of the spans (RH7109). None is about nesting.

#### Finding 2

Input, without `X` defined. Confirmed on head, LF and CRLF: fixed in one iteration.

```csharp
class C
{
    private int Value { get; }
#if X
    = 5;
#endif

    int Read() => Value;
}
```

Output:

```csharp
class C
{
    private readonly int _value;
#if X
    = 5;
#endif

    int Read() => _value;
}
```

In a build with `X`, the input is `private int Value { get; } = 5;`, which is valid. The output is `private readonly int _value; = 5;`, a syntax error.

Class enumeration: the positions where an excluded branch can join the property's declaration.

| Position | Covered by | Status |
|---|---|---|
| in front of the first modifier | RH2001-010 | holds |
| between modifiers, or in front of the type | RH2001-009 | holds |
| inside the declaration | `KeepsEveryCommentAndDirective` | holds |
| behind the last token (no initializer) | nothing | fails |

A property with an initializer ends in `;`, so nothing can join it behind the last token; that member holds.

Not caused by the PR. It is on `main` and violates SHARED-FIX-003. The RH2001-004 coverage the run edits states the fix for directives "after its last" token.

#### Finding 3

Rare-class withholdings that the change doc does not list, each fixed on `main` without changing any build:

| Input | Rule | Test |
|---|---|---|
| `#if DEBUG static #endif` in front of the first modifier: a static get-only property becomes a static readonly field, equally valid | RH2001 | `VerifyNoCodeFixIsOfferedForAConditionalStaticInFrontOfTheFirstModifier` |
| an excluded branch of text that is not code: the build with the symbol does not compile before or after | RH2001 | `…HoldingTextThatIsNotCode` |
| a nested group of whole members in front of the first modifier | RH2001 | `…ANestedGroupOfAWholeMember…` |
| a conditional base list on another declaration of a partial class | RH7109 | `NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfAnotherDeclarationOfAPartialClass` |

All are rare classes, so withholding them puts nothing in question. They must still be named (squad README; `sdd/README.md` says the section lists every visible change). This finding is text only.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001: may a conditional group in front of the property change its declaration in another build | `node.Modifiers[0].LeadingTrivia` directives, then every excluded branch of each group, with nesting checked | `HasConditionalDirectiveBetweenModifiers` (modifiers 2..n and the type's trivia); `IsSupportedPropertyShape` (refuses attribute lists, so the first token is a modifier or the type) | Together they cover every token in front of the type. Nothing reads the trivia behind the last token | gap behind the property (#2); nesting half wider than the decision (#1) |
| "group wraps whole members", RH2001 vs. SHARED-ORDER-008 first-token reading | `WrapsWholeUnitsOf` with `checksNesting: true` (RH2001, spans) and `false` (declaration first token, Decision #11) | `WrapsWholeUnits` (spans) | One parse helper; the nesting flag differs on purpose, as the specs differ | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A conditional group wraps whole members (excluded-branch parse plus nesting) | `rg -n "DisabledTextTrivia\|GetRelatedDirectives\|ReadsAsWholeUnits\|WholeUnits" src --glob '!*Test*/**'` finds only `OrderingDeclarationUtilities.WrapsWholeUnitsOf` (readers: `WrapsWholeUnits`, `DeclarationHoldsConditionalDirective`, `ConditionalGroupsWrapWholeMembers`). Other `DisabledTextTrivia` readers decide other questions: `ExcludedCodeUtilities` (names in excluded code), `HasModifiersSplitByConditionalDirective`/`SyntaxTriviaUtilities` (SHARED-DIAG-008), `ModifierOrderingUtilities`, the formatter phases | one owner; ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| the 11 `VerifyNoCodeFixIsOffered…` RH2001-010 tests | diagnostic reported, no action registered | a registered action | `VerifyNoCodeFixIsOffered` (asserts the diagnostic too) | adequate. Expected values: see #1 and #3 |
| the 3 `VerifyFixKeeps…` tests | exact output, convergence | any output difference, or a remaining RH diagnostic | `Verify` (exact text, re-analysis) | adequate |
| `VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier` | Fix All fixes 2 of 3 and leaves `First` reported | `First` converted, or another left | `Verify` with `MarkupMode.Allow` on the fixed and batch states | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

## thoenissen/Reihitsu#1667

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | high | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:808-809` `DeclarationHoldsConditionalDirective` (`TakeWhile(token => token.SpanStart <= typeDeclaration.OpenBraceToken.SpanStart)`), reached through `AnyDeclarationHoldsConditionalDirective` | FC-024, FC-016, FC-001 | – | same requirement (SHARED-ORDER-008 "every other declaration … each is read … by the same test"; SHARED-FIX-003) | See *Finding 4* below. A declaration of a partial type with a `;` body has no opening brace, so its header is not read, and the reorder that #1667 refuses is still offered for that shape. | Refuse this class: for a declaration without an opening brace, read its tokens up to its end, or count such a declaration with any conditional directive in its trivia as one that sets the layout. Add a test. SHARED-ORDER-008's "in front of its body's opening brace" may need "or its `;`". |

#### Finding 4

Input, without `NET` defined. Confirmed on head with `apply-fix.sh RH7109`, LF and CRLF: fixed, and `_c` is moved in front of `_a`.

```csharp
using System.Runtime.InteropServices;

#if NET
[StructLayout(LayoutKind.Sequential)]
#endif
public partial class C;

public partial class C
{
    private int _a;
    private readonly int _c;
}
```

The file compiles with `NET` defined. In that build, `C` has sequential layout, so the fix reorders it. That is the exact defect #1667 refuses. The control with `{ }` instead of `;` is withheld on head (`no-fix-offered`).

Members of the class:

| Member | Head |
|---|---|
| `;`-bodied other declaration, directive in its first token's trivia (above) | fails |
| `;`-bodied other declaration, directive behind its first token (base list, between modifiers) | fails (traced) |
| `class`, `struct`, `record`, `record struct` | the same path, all fail |
| other declaration with `{ }`, same document | holds (tested) |
| other declaration with `{ }`, other document | holds (tested) |

The reordered declaration always has a brace, so it holds.

Cause: `OpenBraceToken` of a declaration without a body is not a token at the declaration's head. Its `SpanStart` is 0 (the observed result agrees), so `TakeWhile` takes no token. The input class is rare, but it is a member of the run's own requirement, and no test covers it.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Does a conditional directive in a declaration of the type give it a layout in another build | each `DeclaringSyntaxReferences` declaration: tokens up to and including `OpenBraceToken`; the first token's group reads every excluded branch (no nesting check) plus `DeclaresTypeWithAttributes` | `FieldOrderSetsLayout(typeSymbol)`, the attributes of every declaration | agree for declarations with braces. A `;`-bodied declaration is read by the symbol but not by the header check | gap (#4) |
| A5: an excluded partial declaration of the type with attributes, in front of the reordered declaration | the outermost type declarations of each first-token branch parse: name (`ValueText`, so `@C` matches), arity, `partial`, attribute list | the same | agree; nested and other-name counterparts are tested | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which declarations of a type decide its layout | `FieldOrderSetsLayout` (symbol attributes) and `AnyDeclarationHoldsConditionalDirective`/`DeclarationHoldsConditionalDirective` (syntax), both in `OrderingDeclarationUtilities`; `rg -n "StructLayout\|DeclaringSyntaxReferences" src/Reihitsu.Core src/Reihitsu.Analyzer.CodeFixes` finds no other owner | one owner per half; the halves disagree only on #4 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| the RH7109/RH7309 `NoCodeFixWhen…` partial tests (m01, m02, m09, base list, A4, A5, active `NET`) | no action | a registered action | `VerifyNoCodeFixIsOffered` | adequate |
| `NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfADeclarationOfAPartialClassInAnotherDocument` | the document is unchanged after the code-fix and Fix All passes | any move | `Verify` with fixed == source, a second source, `MarkupMode.Allow` | adequate. It does not tell "no action" from "an action with no change", but `Verify` fails on an action that changes nothing, so FC-009 is also caught |
| the `FixMoves…` counterparts (plain partial, static move, body group, enclosing type, other document, A5 name/arity/nesting/no-attribute) | exact output, convergence | output difference | `Verify` | adequate |
| `FixAllSkipsTheFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute` | Fix All moves only `D` | `C` reordered | `Verify`, `MarkupMode.Allow` | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

## thoenissen/Reihitsu#1665

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | low | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:474-479` | check 7 (performance) | – | PR-introduced | The front-of-spans scan runs before `ChangesOrder(IsLayoutField)`. `WrapsWholeUnits` scans the whole tree's trivia once per group and candidate list. So every move that reorders no instance field (every RH7102/RH7103 move, every method move) now pays for a whole-tree scan for each group between the opening brace and the spans, for each diagnostic in a Fix All. | Evaluate `ChangesOrder(IsLayoutField)` first, then the layout conditions (the same result, cheaper). |
| 6 | low | `src/Reihitsu.Core/OrderingDeclarationUtilities.cs:369` `ChangesInitializerExecutionOrder` `<returns>` | FC-031 | – | PR-introduced | It says "or the layout of a type whose declaration holds a conditional directive". The summary was updated to add the group in front of the spans, but `<returns>` was not. | Make `<returns>` match the summary. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A group in front of the spans may change which type, or which kind of type, the spans belong to | `GetGroups(OpenBrace.Span.End .. reorderedSpan.Start)`; each group decided by `WrapsWholeUnits` (Place lists include nested types in front of the spans) | `GetGroups(reorderedSpan)` (refuses every move) | contiguous spans: the in-span check starts where the front check ends. A group straddling the spans is read in both (both tested). Boundary at the target's first trivia: both sides tested (m13 in-span, m04 in front, FC-003). A nested type's body in front of the spans and a method body in front of them: traced, read through the Place lists | ok |
| Which moves the layout status refuses | `ChangesOrder(IsLayoutField)` | SHARED-ORDER-007 initializer checks (unchanged) | RH7102/RH7103 moves keep the fix (tested) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which conditional groups in a type body refuse or restrict a move | `OrderingDeclarationUtilities.ChangesMemberOrder` (private, `Func` overload) only. Its callers are `TypeMemberOrderingCodeFixProviderBase:80` and `RegionBlockLayout:71` (RH7309); `OrderingMoveSafety.MoveRangeContainsDirectives` is the SHARED-ORDER-006 decision, a different one | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7109 m04, m08, straddling-group, RH7309 m04 `NoCodeFixWhen…` | no action | an action | `VerifyNoCodeFixIsOffered` | adequate |
| RH7103 s1, RH7102 const, RH7109 m11, whole statements, attribute lists, nested type, group behind the spans, straddling whole-member group | exact output, convergence | output difference | `Verify` | adequate |
| RH7109 m13 (group in the target's trivia) | no action | an action | `VerifyNoCodeFixIsOffered` | adequate (the boundary's other side) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

## Whole run

### Visible output is decided (check 10)

| Expected value | Covered by |
|---|---|
| the three issue inputs, A5 (m06), Decision #3 inputs (m05, n3), A4, m02, the other-document test | unchanged SHARED-FIX-003 |
| m09 (`[Serializable]` on another part) | maintainer answer 2 |
| m08 (modifier in front of the spans) | maintainer answer 3 |
| r5 (attribute list, RH2001) | maintainer answer 1 |
| every `FixMoves…`/`VerifyFixKeeps…` test | `main`'s output, unchanged |
| the nesting expected values | not covered: #1 |
| the rare-class withholdings | not listed: #3 |

No rule page changes.

### Cost of special cases (check 11)

`DeclaresTypeWithAttributes` and the front-of-spans reading are refusals, built on the parse that already exists; each is a cheap check. Declaring a shape unsupported does not apply: the formatter is not involved, and the change doc says so. No FC-042 guard exists.

### Comments, scope (checks 8 and 9)

- No `TODO`, no commented-out code, and no tracker numbers in the changed source.
- Changed summaries describe the new code, except #6.
- No edit outside the run's scope.

### Failure classes dismissed

- **FC-002**: the qualifiers "in front of the spans", "outermost level", "partial", "attribute list", and "excluded" are each an explicit condition in the code.
- **FC-004, FC-019, FC-021, FC-022, FC-026, FC-029**: the run adds only refusals and changes no rewrite; kept fixes compare exact output.
- **FC-005**: no comment guard is involved; only conditional directives are read, and other directives are unchanged (RH2001-009 tests, SHARED-ORDER-006).
- **FC-006**: no rewrite changed. Partial types: #4.
- **FC-007, FC-008**: `Verify` re-analyzes and runs Fix All for every kept fix. Fix All for refusals is tested for RH2001 and RH7109. RH7103/RH7309 share the per-diagnostic guard.
- **FC-009**: refusals return before registration. No action without a change was observed.
- **FC-010, FC-011, FC-012, FC-013, FC-014, FC-028, FC-030, FC-038 to FC-043**: the formatter is not touched, and no shape is declared unsupported.
- **FC-017**: see the assertion tables.
- **FC-018, FC-036**: the RH2001 rename path is unchanged.
- **FC-023**: `Identifier.ValueText` normalizes `@C`; traced.
- **FC-025, FC-027**: the RH2001 guard reads one token's trivia whatever the property shape. Nested scopes in front of the spans are read through the Place lists (nested type, method body).
- **FC-032**: no project-wide result reused.
- **FC-033, FC-034**: every scenario runs LF and CRLF on the code-fix path, with the issues' own inputs.
- **FC-035**: expected values come from the specs and `main` measurements (04).
- **FC-037**: not a text-only claim.
- **FC-040**: the branch parse uses the document's parse options, so the documentation mode is the same.

### Architecture findings

_None._ #1 is a guard wider than its decision. #4 is a missed member of a new reading. Neither has two components owning one decision.

### Hints

- RH2001 `HasConditionalDirectiveBetweenModifiers` and the new call are two guards on adjacent trivia. A third for #2 would make three. Folding them into one "conditional directives around the declaration" check is a possible later cleanup, not a defect.
- The fixtures used: `/tmp/claude-0/-home-user-Reihitsu/f0b6b3e1-bd2b-54f9-b9f7-643b09526771/scratchpad/rh2001/f1_filewrapped.cs`, `.../rh2001/f2_trailing_init.cs`, `.../rh7109/g1_semicolon_body.cs`, `.../rh7109/g2_semicolon_body_ctrl.cs` (that scratchpad directory also holds older fixtures from earlier roles).
