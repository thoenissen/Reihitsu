Role: skeptic
Commit: 6aebd7e7f984d61a60fc533526cc871375d48055
Verdict: #1597: proceed narrower (withhold only; the literal-renaming alternative is dropped); #1598: proceed to the reproduction, predicted not to reproduce as written; #1016: proceed; #1015: proceed; #1084: proceed

The prompt carried no intended fix or hoped-for verdict. It named the #1084 triage narrowing and the #1598 triage note, which are the maintainer's clarifications and are applied below. Every observation below was made with `scripts/apply-fix.sh` (single fix, LF and CRLF, which agreed in every case) on `main`'s code at the commit above. Run-time effects were checked by compiling and running each input before and after the rename as a file-based program (`dotnet run <file>.cs`), with a top-level statement that prints the result. All fixtures lived in the scratchpad and are deleted.

## thoenissen/Reihitsu#1597

### Premise

Real, for all three reported cases, and wider than reported. Caller: the casing code fix in the IDE, single fix and Fix All; both use `RenameWithoutConflictAsync` in `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs`. Its guards are: a namespace collision, Roslyn conflict annotations, changed native entry points, a name in excluded code, more compile *errors* by ID in the affected projects, and changed positional record properties. None of them sees a binding change that adds no error.

The table gives the observed results. Each "before → after" value is the printed output of the compiled program. Every input compiles before and after the fix, and the fix is offered on each one today.

| # | Rule | Input (renamed member) | What binds differently | Run-time output before → after |
|---|---|---|---|---|
| 1 | RH4103 | `getEnumerator` on a class that implements `IEnumerable<int>` explicitly (the issue's case 1, written so that it compiles) | `foreach` binds to the pattern method instead of the interface | `2` → `1` |
| 2 | RH4103 | extension `select` in `namespace N`, query `from v in values select v * 2` in `N` (case 2) | the query clause binds to `N.Extensions.Select` instead of `Enumerable.Select` | `2,4` → `1,2` |
| 3 | RH4103 | instance `Rows select(Func<int,int>)` on a type that `Enumerable.Select` already serves | the query clause binds to the instance method | `IEnumerableSelectIterator`2` → `Rows` |
| 4 | RH4103 | instance `getEnumerator` next to an extension `GetEnumerator` | `foreach` binds to the instance method | `2` → `1` |
| 5 | RH4103 | instance `getAsyncEnumerator` next to an extension `GetAsyncEnumerator` | `await foreach` binds to the instance method | `2` → `1` |
| 6 | RH4103 | instance `getAwaiter` next to an extension `GetAwaiter` | `await` binds to the instance method | `extension` → `instance` |
| 7 | RH4103 | instance `deconstruct` next to an extension `Deconstruct` | `var (x, y) = …` binds to the instance method | `7` → `3` |
| 8 | RH4103 | instance `add(int)` next to an extension `Add` | the collection initializer `new Box { 1, 2 }` binds to the instance method | `extension extension` → `instance instance` |
| 9 | RH4103 | `add(int)` next to an instance `Add(object)` | overload resolution of the collection initializer picks the new `Add(int)` | `object object` → `int int` |
| 10 | RH4105 | `Check(bool Condition, [CallerArgumentExpression("Condition")] string expression = null)` (case 3) | the attribute names no parameter after the rename (warning CS8963, no error) | `1 > 0` → `<null>` |
| 11 | RH4105 | as 10, with the attribute naming the *new* name: `[CallerArgumentExpression("condition")]` | the attribute starts to name a parameter (CS8963 goes away) | `<null>` → `1 > 0` |

Also offered and reproduced as diffs (the literal stays as written), not run: rows 10 and 11 hold for a constructor, a local function, a delegate, `private const string Name = "Condition"` used as the argument, and RH4120 on `record R(bool flag, [CallerArgumentExpression("flag")] string Expression = null)`, where the fix renames `flag` to `Flag` and leaves `"flag"`.

These hold, meaning the existing guards work:

- `[CallerArgumentExpression(nameof(Condition))]`: the renamer renames the `nameof`.
- `[InterpolatedStringHandlerArgument("Prefix")]` on a renamed `Prefix`: no fix is offered, because the stale name is a compile error.
- A rebinding of a call whose name is written in source (`values.Select(…)`, `box.Add(1)`): the renamer keeps the old binding by rewriting the call. See the follow-up candidates for what it writes.

### Mechanism and candidates

Mechanism: the casing fix withholds a rename only for added compile errors and renamer conflicts. It therefore offers a rename that changes run-time behavior without a new error, in two shapes:

- **(a) Implicit lookup.** The new method name is a name the compiler looks up for a construct that does not write it in source. The renamed method then wins that lookup over the previous target: an instance method beats an extension, a closer extension beats a farther one, a pattern member beats an interface, a better overload beats a worse one.
- **(b) Parameter named by a string.** A `CallerArgumentExpression` argument names a parameter by a constant string, which the renamer does not treat as a reference, and the compiler only warns when it names no parameter.

The dispatch for (a) is the compiler's pattern lookups, not Reihitsu code. The candidates, each with the construct that triggers it:

- `GetEnumerator`: `foreach` (rows 1, 4)
- `GetAsyncEnumerator`: `await foreach` (row 5)
- the query methods `Select`, `Where`, `SelectMany`, `Join`, `GroupJoin`, `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `GroupBy`, `Cast`: query clauses (rows 2, 3)
- `GetAwaiter`: `await` (row 6)
- `Deconstruct`: deconstruction (row 7)
- `Add`: collection initializers (rows 8, 9)
- `Dispose` and `DisposeAsync`: `using` on a ref struct or with a pattern
- `GetPinnableReference`: `fixed`
- `AppendLiteral` and `AppendFormatted`: interpolated-string handlers

The first six names are settled by the rows above. The last three are the same mechanism and were not constructed. Only RH4103 renames a method that such a lookup finds. RH4104 renames local functions, which no member lookup finds. For properties (`Current`, `IsCompleted`, `Length`, `Count`), I found no shape that compiles before the rename and binds differently after it; none was constructed.

The rules that can produce (b) are the parameter renames: RH4105 on methods, constructors, local functions, delegates, indexers and primary constructors of classes and structs, and RH4120 on record primary constructors. A rename cascades to the other part of a partial method, whose attribute is a further position for the string. That position was not constructed.

### Breaking inputs

Failure classes: FC-036 (a rename treated as behavior-preserving), FC-006 (overload selection), FC-016 (sibling shapes keep the hazard).

Class matrix of (a), on two axes: the construct (foreach, await foreach, query clause, await, deconstruction, collection initializer) and what the previous target was (an extension in a farther scope, an extension against a new instance method, an interface against a pattern method, a worse overload). Rows 1–9 settle one member per construct and four previous-target kinds, and all fail. The test implementer must cover at least:

- rows 1, 2, 7 and 8, each as its own test, in single-fix and Fix All mode;
- a holding control for each: the same rename with no competing binding, such as `getEnumerator` on a class without the interface, the extension and any `foreach`. The fix stays offered unless the run takes the name-set option under *Internal choices*, in which case it is withheld too.

Class matrix of (b), on two axes: whether the string names the old or the new name, and the argument form (string literal, constant, `nameof`). Old name with a literal or a constant fails; new name with a literal fails; `nameof` holds. The parameter owner varies too: method, constructor, local function and delegate under RH4105, record primary constructor under RH4120. All fail with a literal.

Tests needed for (b):

- rows 10 and 11;
- the RH4120 record case;
- the `nameof` control, which stays fixed.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers RH4103, RH4105, RH4120 | must stay: the diagnostics are unchanged |
| `CasingCodeFixProviderBase<T>` (single fix and Fix All share `RenameWithoutConflictAsync`) | must change |
| Fix All | covered by the same guard; needs its own tests (the issue's "Done when") |
| `Reihitsu.Core` (`CasingUtilities`) | not involved |
| Rule pages | not involved: a withheld fix is covered by `general-notes.md` |
| Specs | clarification needed, see *Internal choices* |

### Refuse option

The class is rare. It needs a method spelled in lowercase with a pattern name such as `getEnumerator`, `select`, `add` or `deconstruct`, plus a competing binding that already compiles. Or it needs a wrongly cased parameter plus a `CallerArgumentExpression` literal. That is code people seldom write, and every such input already violates RH4103 or RH4105.

The requested change for (a), comparing the binding of every implicit construct before and after the rename, is a new mechanism:

- it walks every foreach, query, await, deconstruction and initializer node of the affected projects in both solutions;
- it maps nodes across a text change;
- it compares symbols from two compilations.

That is the machinery that *Special cases do not grow the source* rules out for a rare class.

The refuse options:

- **(a) by name.** Withhold the RH4103 fix when the derived name is one of the names the compiler looks up implicitly: a fixed set of about 20 names, checked once against the derived name. It also withholds renames that are safe, such as `getEnumerator` with no competing binding. Those inputs are rare too.
- **(b) by attribute.** Withhold a parameter rename when a `CallerArgumentExpression` argument of the parameter list has a constant value equal to the old or the new name. Simpler still: the existing compile-diagnostic comparison already sees CS8963, which appears in row 10 and disappears in row 11, so comparing the count of that one warning ID (equal, not "not more") covers both directions with the existing helper.

Declaring a shape unsupported is not an option here. These fixes do not format, and the shapes are semantic (bindings), not something the precheck can recognize from tokens.

### Output questions

_None._ Withholding a fix for this rare class puts nothing in question. The issue's alternative for case 3, renaming the literal, is not a reading of the spec: SHARED-FIX-003 forbids changing a literal the user wrote, and its exceptions name only documentation-comment names and `#endregion` comments.

### Internal choices

1. **Precision of (a).** One option is the name set (cheap; over-withholds rare safe renames). The other is the binding comparison (exact; a new mechanism). Recommendation: the name set. The difference is visible only for a rare class, and the rule in `AGENTS.md` rules out the mechanism.
2. **Spec clarification.** SHARED-CASING-007 lists what its coverage excludes: excluded code and positional record properties. A `foreach` rebinding sits inside that coverage, so RH4103-003 reads as demanding the rename, while SHARED-FIX-003 (run time) demands withholding. Recommendation: run the spec author for a clarification that adds a bullet to SHARED-CASING-007's coverage, phrased to match choice 1. It is the analogue of the positional-record bullet. Add one example each for an implicit lookup and for `CallerArgumentExpression`. Optionally, add one sentence to SHARED-FIX-003 *Run time*: like the native entry point, a caller-argument expression is not the new name.

### Issue goals

- "or the rename carries the literal along for case 3": drop it, because SHARED-FIX-003 forbids changing a literal. No visible output is in question, since withholding is what the unchanged requirement demands.
- "A guard needs a semantic check, for example comparing the binding…": a prescribed solution. Drop it in favour of the refuse option; see above.
- "each with a regression test, in single and Fix All modes": keep it.

### Architecture findings

_None._

### Follow-up candidates

The renamer rewrites code it does not rename, to keep a binding. This is outside the run's issues. It is a candidate against SHARED-FIX-006, which says a fix changes only the code its diagnostic is about.

**RH4103 · Method names should use PascalCase. An extension method rename that would capture a call**

Input:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace N
{
    public static class Extensions
    {
        public static IEnumerable<int> select(this IEnumerable<int> source, Func<int, int> selector) => source;
    }

    public class User
    {
        public IEnumerable<int> Get(List<int> values) => values.Select(value => value * 2);
    }
}
```

Before (`main`): the fix renames `select` to `Select` and rewrites the call to `Enumerable.Select(values, value => value * 2)`. Likewise, renaming `add(int)` next to `Add(object)` rewrites `box.Add(1)` to `box.Add((object)1)`.

After, A (recommended): no fix offered; the diagnostic stays.

After, B: as today.

Why A: the fix's result should differ from the input only in the renamed name, and a rewritten call site is an edit nobody asked for.

## thoenissen/Reihitsu#1598

### Premise

As written, predicted not to reproduce. It can be settled only by the test the maintainer asked for, because `scripts/apply-fix.sh` runs one file in one project.

Reasoning from the code:

- A multi-targeted project compiles the same `B.cs` in each target's project, as linked documents.
- In the project the fix runs from, `NET8_0` is not defined, so `var x = new Holder().key;` is disabled text in that project's own copy of `B.cs`.
- `ExcludedCodeUtilities.GetReferringDocuments` takes the documents of the declaring project and its dependents for a public member, and `HasExcludedCodeOccurrenceAsync` scans their disabled text for the old and the new name. So it finds `key` without looking at the other target's project.
- The issue's suspected mechanism, "only the project the fix is invoked from", does not lead to a miss for the issue's own input.

Reachable: yes. The caller is a code fix in an IDE workspace that holds one project per target framework.

### Mechanism and candidates

No defect is established. The shapes where the other target's reference is *not* disabled text in the invoking project are the candidates the reproduction must cover:

1. The issue's input as written: the same file in both target projects, with the branch active only in the other target. The prediction is that no fix is offered, by the existing check. Expected: no fix offered.
2. A file compiled only in the other target's project (a conditional `Compile` item), with an active reference to the renamed member. Expected: the fix renames that reference too, or is withheld. SHARED-CASING-007 says "every reference in the active code of the solution", and SHARED-FIX-003 applies.
3. A project that references only the other target's project, with an active reference. Expected: as for 2.

For 2 and 3, the outcome depends on whether the renamer's reference search reaches the other target's copy of the symbol. Whether the compile-error check then inspects that project also matters: `affectedProjectIds` is the changed projects and their dependents. I did not run this and do not claim a result.

### Breaking inputs

FC-033 applies: the harness may normalize the scenario away. A test that adds two projects must make their copies of the shared file linked documents (the same file path), as the IDE's workspace does. Otherwise it tests two unrelated files. FC-034 applies to candidate 1: it must be the issue's own shape, not only a single-project `#if`, which SHARED-CASING-007's existing RH4106 example already covers.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers | not involved |
| `CasingCodeFixProviderBase<T>`, `ExcludedCodeUtilities` | must stay, unless candidate 2 or 3 reproduces |
| Fix All, `Reihitsu.Core`, rule pages, specs | not involved |

### Refuse option

Withholding every casing fix whose declaring document has linked documents would refuse the fix in every multi-targeted project, which is code people commonly write. That is escalation 9 if it is ever proposed. If candidate 2 or 3 reproduces, the narrower option is to widen the region the existing checks inspect to the projects that hold a linked copy of a cascade declaration's document. That is the same check over more projects, not a new mechanism, and it does not evaluate `#if` conditions (SHARED-FIX-009). If nothing reproduces, there is no change, and the passing tests stay as characterization tests.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

"Done when … or does not reproduce and the issue is closed": keep it. If none of candidates 1–3 reproduces, this is escalation 4, the PR uses `Refs #1598`, and the maintainer closes the issue.

### Architecture findings

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1016

### Premise

Real, as reported, including the comment's `ß` cases. "no-fix-offered with a diff" means the fix applied once and the result is still reported with no fix. "Two iterations" means the fix applied twice for one diagnostic. Observed:

| Rule (target) | Input → result | Outcome |
|---|---|---|
| RH4106 (`_camelCase`) | `__1st` → `_1st`; `_1_value` → `_1Value`; `注文_明細` → `_注文明細`; `注文` → `_注文`; `ϒ_x` → `_ϒx` | still reported, no further fix |
| RH4106 | `ß_x` → `_ßX`; `_1st` (no fix, SHARED-CASING-008) | holds |
| RH4105 (camelCase) | `_注文` → `注文`; `_ϒx` → `ϒx` | still reported, no further fix |
| RH4105 | `_ŉx` → `ŉx`; `__1st` (no fix) | holds |
| RH4002 (PascalCase) | `_注文` → `注文`; `注文_明細` → `注文明細`; `_ßx` → `ßx`; `ß_x` → `ßx`; `_ŉx` → `ŉx` | still reported, no further fix |
| RH4005 (`I` + PascalCase) | `_2x` → `II2x`; `注文` → `II注文`; `ßx` → `IIßx` | two applications |
| RH4121 (type parameter) | `_注文`, `ßx` (no fix); `_1` → `T1` | holds |

Reachable: yes, through the casing code fix and its Fix All, which uses the same `TryGetFixedIdentifier`.

### Mechanism and candidates

Mechanism: the fix checks only that the derived name is a valid identifier different from the old one (`TryGetFixedIdentifier`), never that the rule accepts it. Every old name whose derivation lands outside the rule's style is offered a fix that leaves its diagnostic.

These are the old names whose first word starts with:

- a digit;
- a letter without case (Lo, Lm);
- a letter whose invariant uppercase, for a PascalCase or `I` target, or lowercase, for a camelCase target, is not a cased letter of the target case: `ß`, `ŉ`, `ϒ`.

These are exactly the inputs SHARED-CASING-006 leaves uncovered.

Dispatch: the conversion each provider passes to the base. The candidates:

- `ToPascalCase`: RH4002, RH4003, RH4004, RH4006, RH4010, RH4011, RH4101, RH4102 (both providers), RH4103, RH4104, RH4108, RH4109, RH4110, RH4111 to RH4114, RH4120;
- `ToCamelCase`: RH4105, RH4115, RH4117;
- `ToUnderlineCamelCase`: RH4106, RH4107;
- `ToTypeParameterName`: RH4121 (holds: its converter rejects a suffix that is not PascalCase or digits);
- RH4005's `OnTransformIdentifier` (fails).

### Breaking inputs

FC-007 (more than one pass), FC-023 (digit, `_`, Unicode identifiers), FC-016 (sibling conversions).

Class matrix on two axes: the target family (PascalCase, camelCase, `_camelCase`, `I` prefix, `T` prefix), and the first-word start (a digit; a caseless letter; a letter without a one-character mapping to the target case). Each family also varies by whether the first word follows a leading `_` or starts the name.

Failing members:

- **PascalCase:** caseless, no mapping. A leading digit gives an invalid identifier, which is already withheld.
- **camelCase:** caseless, no lowercase mapping (`ϒ`). A digit is already withheld.
- **`_camelCase`:** digit, caseless, no lowercase mapping.
- **`I` prefix:** digit, caseless, no uppercase mapping.

Holding members:

- `T` prefix: all;
- `_camelCase` with `ß`, which is lowercase already;
- camelCase with `ŉ`, which is lowercase already.

The tests need, per family, one failing member of each start kind, both in a leading position and after `_`, and the holding controls `ß_x` (RH4106), `_ŉx` (RH4105) and `_1` (RH4121). One test per family in Fix All mode.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers | must stay |
| `CasingCodeFixProviderBase<T>` | must change |
| RH4005 provider | changes, or is covered by the base check |
| `CasingUtilities` (`Reihitsu.Core`) | not required; its converters and their tests may stay as they are |
| Fix All | same guard; needs tests |
| Rule pages | not involved |
| Specs | clarification: SHARED-CASING-008 |

### Refuse option

The requested outcome *is* the refuse option: withhold the fix where the derived name is still reported. These are rare names: a digit after an underscore, CJK-leading names, `ß`-leading names. For most of them no name in the rule's style exists without dropping or inventing characters. The only alternative that converges is RH4005 reaching `II2x` or `II注文` in one step, and that is not what the derivation SHARED-CASING-006 and RH4005-002 specify produces, so it is not a competing reading. The cost is one check in the base. One condition, "the rule would still report the derived name", subsumes all three existing SHARED-CASING-008 conditions, which `#1084` needs too.

### Output questions

_None._ SHARED-FIX-001 (unchanged) forbids today's output, the spec's derivation admits no converging name, and withholding a fix for a rare class puts nothing in question.

### Internal choices

1. **Spec clarification.** Add a condition to SHARED-CASING-008: the fix is not offered when the rule reports the derived name. Add examples from the table above, such as RH4106 `__1st` and RH4002 `_注文`. No change doc is needed: it records how the unchanged SHARED-FIX-001 is met for the inputs SHARED-CASING-006 leaves uncovered. The issue says the #978 specs left these inputs out "until this is decided"; this is that decision. Recommendation: run the spec author for it in this run.

### Issue goals

_None to drop._ "One application corrects the diagnostic, or the fix is not offered" is met by withholding.

### Architecture findings

Which derived name is acceptable is decided in several places, and none of them reads the rule's predicate:

- `CasingCodeFixProviderBase<T>.TryGetFixedIdentifier`: valid and different;
- each `CasingUtilities.To*` converter: returns its input unchanged when it cannot convert, which the base then reads as "equal to old, so withheld";
- `RH4005InterfaceNameCasingCodeFixProvider.OnTransformIdentifier`, which prepends `I` and so defeats that convention (this is #1084);
- `ToTypeParameterName`, which validates its own suffix (the only family that holds).

The analyzers own the style predicate (`CasingUtilities.Is*` plus each rule's exceptions). This is one rule stated in several places, with the fix not consulting the analyzer's decision. Inputs: the table above, and #1084's `_`.

## thoenissen/Reihitsu#1015

### Premise

Real, by the code. Not run, because `scripts/apply-fix.sh` runs single fixes only, so the reproduction test settles it.

RH4102 has two providers for one diagnostic ID:

- `RH4102EventNameCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>`;
- `RH4102CustomEventNameCasingCodeFixProvider : CasingCodeFixProviderBase<EventDeclarationSyntax>`.

`GetFixAllCandidatesAsync` takes every RH4102 diagnostic in scope and drops each whose `root.FindNode(span)` is not `T`. So each provider's Fix All renames only its own kind. Reachable: Fix All in the IDE on a type with both kinds of event. RH4102 is the only diagnostic ID in `Reihitsu.Analyzer.CodeFixes` with more than one provider.

### Mechanism and candidates

Mechanism: one diagnostic ID is split across providers by node type, and each provider's Fix All filters candidates by its own node type. Candidates:

- field-like events with one declarator or several (`event EventHandler a, b;`);
- events with accessors;
- Fix All started from either kind, in document, project and solution scope.

### Breaking inputs

FC-008 (Fix All). Test matrix:

- the issue's input, started from `dataChanged` and started from `saved`, each expected to rename both;
- a field-like declaration with two declarators plus a custom event;
- the result must equal the single fixes applied one after another (SHARED-FIX-002);
- a mixed collision group, for example `dataChanged` (field-like) and `DATA_CHANGED` with accessors in one type: SHARED-CASING-010 skips both.

The test base `BatchCodeFixTestsBase<TAnalyzer, TCodeFix>` takes one provider. A mixed test needs both providers registered, unless the run ends with one provider.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer RH4102 | must stay |
| The two RH4102 providers | must change |
| `CasingCodeFixProviderBase<T>` | not necessarily: `RH4115LocalVariableCasingCodeFixProvider` already handles several node kinds in one provider (`T = SyntaxNode`, with `GetIdentifier` switching on the kind) |
| Specs | none needed: SHARED-FIX-002 already demands the mixed Fix All, and RH4102's header says `Fix All` |
| Rule page | not involved |

### Refuse option

Withholding Fix All, by returning no Fix All provider, would remove Fix All for every RH4102 user. That is common code, and it is worse than the defect. The simpler option is the existing pattern of one provider over both node kinds (RH4115). It deletes a provider rather than adding code.

### Output questions

_None._ SHARED-FIX-002 (unchanged) decides the output.

### Internal choices

_None._ How the providers are combined is the implementer's choice.

### Issue goals

_None to drop._

### Architecture findings

_None beyond the mechanism above._ Two providers owning parts of one ID's Fix All is the cause, and it is removed by the RH4115 pattern.

## thoenissen/Reihitsu#1084

### Premise

Real, within the narrowed scope (the triage comment: only "no usable name remains"; prefix keeping and identifier characters moved to #1451). Observed with RH4005:

- `interface _` becomes `II` in two applications (`_` → `I_` → `II`);
- `interface __` becomes `II` in two applications.

The issue says `I_` is still reported; it is, and the second application turns it into `II`. The spec already decides the case. SHARED-CASING-008 says the fix is not offered when the old name has no letter or digit, and RH4005-002 says "withheld as SHARED-CASING-008 … specify". This is a plain bug against an unchanged requirement, and also against SHARED-FIX-001. Reachable: the RH4005 code fix and its Fix All.

### Mechanism and candidates

Mechanism: SHARED-CASING-008's first condition is enforced only indirectly, by each converter returning a letterless input unchanged, which the base then rejects as equal to the old name. RH4005's `OnTransformIdentifier` prepends `I` to that unchanged input, so the result differs from the old name and the fix is offered.

Candidates: every casing provider's conversion with a letterless old name (`_`, `__`, `___`). Only RH4005 fails. RH4121 holds: `ToTypeParameterName` rejects `_`. The PascalCase, camelCase and `_camelCase` families hold, because their result equals the old name.

### Breaking inputs

FC-023 (`_` identifiers), FC-007. Tests:

- RH4005 `_` and `__`, no fix offered, single fix and Fix All;
- control: RH4005 `I`, which today gets one application to `II`. That is consistent with RH4005-002 as it stands and is #1451's territory, so it is not changed here.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer | must stay |
| RH4005 provider, or the base check from #1016 | must change |
| Specs | none: SHARED-CASING-008 and RH4005-002 already decide it |
| Rule page | not involved |

### Refuse option

The requested change is itself a refusal: withhold the fix. The cost is one condition, and the same base check that #1016 needs covers it.

### Output questions

_None._

### Internal choices

_None._

### Issue goals

The original body's goals for `i_document`, `T_Key`, `Café`, `ClassⅫ` and `I` are out of scope per the triage comment (#1451). Only `_`/`__` remains, and that goal stands.

### Architecture findings

The same finding as #1016: the acceptability of the derived name is not decided in one place.

## Whole run

- **Lane.** Bug fix holds for all five issues. Spec-author clarifications are needed for #1597 (SHARED-CASING-007 coverage, plus an optional sentence in SHARED-FIX-003 *Run time*) and #1016 (SHARED-CASING-008). No change doc is needed: both write down how unchanged requirements (SHARED-FIX-003, SHARED-FIX-001) are met, and both only withhold fixes for rare classes. #1015 and #1084 need no spec change. #1598 needs none unless it reproduces.
- **Output questions within the run's issues.** _None._
- **Escalations foreseeable.** #1598 is escalation 4 if the reproduction (candidates 1–3) does not reproduce.
- **Shared change.** #1016 and #1084 share one mechanism, the fix never checking the rule's acceptance of the derived name, so one base-level check serves both. #1597 adds two withholding conditions to the same `RenameWithoutConflictAsync`. #1015 is independent.
