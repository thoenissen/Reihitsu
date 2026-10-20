Role: auditor
Commit: 820eba63041b7f17761fc3bd20509a2997c0a8f2
Verdict: findings

## Scope

State:
- The checkout is the head `820eba6`, and the working tree was clean before I wrote this file.
- `origin/main` and `git ls-remote origin refs/heads/main` both give `e84b26c9854b4014c96c30f8b3a549998a5b7396`, which is an ancestor of the head.
- `scripts/lane-check.sh tooling` gives "LANE CHECK: confirmed — claimed tooling, diff needs tooling".

What I audited:
- This is a repair delta against my previous audit, `21-auditor.md` at `d7c7186`. Its handover commit `c458d8c` adds only that file.
- The repair commits are `ed95c81`, `3266c8c`, `3fbc1e2`, `9e806da`, `76d14dd`, `98d7339`, and `820eba6`. Outside `sdd/work/`, `git diff c458d8c HEAD` changes these files:
  - `sdd/specs/shared/struct-equality.md` (SHARED-EQUAL-002, list and coverage text only);
  - `sdd/specs/analyzer/RH1001.md` (RH1001-003, list and coverage text only);
  - the decisions entry `squad/decisions/2026-10-20-996-shared-equal-002-bounded-list.md`;
  - the RH1001 and RH1002 test classes. They gain 11 tests at the end of the classes, and one summary changed. Every other test is byte-identical to `d7c7186`.
- No file under `src/Reihitsu.Analyzer`, `src/Reihitsu.Core`, `src/Reihitsu.Analyzer.CodeFixes`, or `src/Reihitsu.Formatter` changed. The last change to analyzer or Core source is from 2026-10-17, so I reuse the code evidence of `21-auditor.md`.
- Both requirement example blocks are byte-identical, and so are `requirement-ids.txt` and `sdd/specs/README.md`.
- The scope did not grow. The repair stays within what the decisions entry allows. The entry describes my three findings correctly, and its answer matches what the repair did.
- My prompt named the maintainer's decided repair and the checks to make. It carried no suspected cause and no hoped-for verdict.

Status of the previous findings and hints:

| Item of 21 | Status | Evidence |
|---|---|---|
| Finding 1: a constant holding `null` | closed | The first item of the not-custom list reads "a constant with the value `null` — the literal `null`, a cast of it, or a `const` field or local initialized with `null`". The custom list names only "a field that is not a constant" and "a local variable that is not a `const`". The Why sentence was added. Tagged tests: `RH1002 · VerifyConstantWithValueNullIsNotCustomComparer` and `RH1001 · VerifyCreationWithConstantNullComparerIsNotExempt`. Each has a `const` field, a `const` local, and a non-`const` local as the contrast, and the RH1002 test also has `flag ? null : NoComparer`. This matches `IsNullLikeExpression` (`GetConstantValue` first) and `IsNullLikeOperation` (`ConstantValue`). |
| Finding 2: `default` of a type parameter | closed | The coverage sentence of SHARED-EQUAL-002 leaves it out, "whatever the type parameter's constraints". Both lists name only non-type-parameter types, so neither claims it. Struct-equality Q6 is in `22-spec-author.md`. The untagged characterization tests are `RH1002 · VerifyDefaultOfTypeParameterNotConstrainedToValueTypeCountsAsNoComparer` (marks `TC`, `TN`, `TR`, not `TV`; 3 diagnostics) and `RH1001 · VerifyCreationWithDefaultOfTypeParameterNotConstrainedToValueTypeIsReported` (marks `map`; 1 diagnostic). A mechanical comparison shows both inputs are identical to the Q6 input, and the markup matches the Q6 output sentence. `IsNullLikeDefaultType` is `IsValueType == false`, so the "Why" claim is correct. |
| Finding 3: a `switch` without arms | closed | Both `switch` items of both requirements now say "has at least one arm" or "one of whose arms", and both coverage sentences name a `switch` without arms as left out. This agrees with `Arms.Length: > 0` and `Arms.Count: > 0`. No question was filed, which is the lead's call recorded in 22. The decisions entry asks only that unlisted forms be left unspecified. |
| Hint 1: no checker or cleaner handover after `f299f87` | closed | 24 and 26 spec-checked the repair. 23 and 25 ran `scripts/format.sh` ("Formatted 0 of 2"), the Release build (0 warnings), and the architecture tests (72 passed). The validator still runs. |
| Hint 2: the summary of `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` | closed, residue in finding 2 | 23 rewrote the summary to the form-based wording. One phrase still misstates the input, and the method name keeps the old claim. See finding 2. |
| Hint 3: RH1001-003 `throw` coverage and a constant condition | superseded | The bounded coverage sentence leaves out a `throw` branch only when the other branches all have an exempt form. A constant condition is decided by the conditional items. |
| Hint 4: hints 3 and 4 of 14 | open, with the lead | 22, "Not addressed". |

How I checked:
- I traced every item of both lists in both requirements against `EqualityComparerArgumentUtilities` and `RH1001…Analyzer.HasExplicitEqualityComparerInitializer` at the head.
  - Every base form of the two lists of a requirement is disjoint from every base form of the other list. The conditional and `switch` items partition by "both or all" against "one", and by "constant" against "not constant", so no form lands in both lists.
  - No form on a custom or reported list is null-like or framework-default under the code: variables, non-`const` fields, properties, calls, creations, `default` of a non-nullable value type, a user-defined cast, and wrappers, `??`, conditionals, `switch`es, and null-conditional accesses built on these.
- Scratch project, `net10.0` / `LangVersion preview`, in the scratchpad (deleted afterwards). It used the head's Release `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` (built 2026-10-20) as `<Analyzer>` items. I read the RH1001 and RH1002 lines from `dotnet build`, which reported 0 errors. Results, where "reported" means the argument is not custom:
  - Reported: `NullHolder?.Comparer` (`const Holder NullHolder = null;`), `isConst?.Comparer` (a `const` local), `default(Holder)?.Comparer`, `default(VH?)?.Comparer` (with `struct VH` holding a `Comparer` property), `(ValueComparer?)null`, `Other.Nothing` (a `const` of another class), `MyEq.Default` (with `MyEq : EqualityComparer<S>`), and `mode switch { _ => NoComparer }`.
  - Not reported, so custom: `notConst?.Comparer` (a non-`const` local holding `null`), `new ValueComparer?()` and `new ValueComparer?(default)` (object creations), `(ValueComparer)null` through a user-defined conversion from `string`, `true ? comparer : null`, `(IEqualityComparer<S>)comparer`, a `static readonly` field holding `null`, `_arr[0]`, `_arr?[0]`, `new ValueComparer()`, `(IEqualityComparer<S>)(ValueComparer)null`, and `default(ValueComparer?)!`. The last one is whole-argument `!` around a form that is not a constant, which the Q5 coverage sentence leaves unspecified.
  - RH1001: `new(NullHolder?.Comparer)` and `new(default(Holder)?.Comparer)` are reported. `new(new ValueComparer?())` and `new((ValueComparer)null)` are exempt.
  - Every observation agrees with the lists.
- Open-question texts: a `diff` shows that every issue text in `22-spec-author.md` from the RH1001 issue to the RH2109 issue is identical to `15-spec-author.md`. In the struct-equality issue, Q1 to Q4 are identical to 15, Q5 (checklist line and question) is identical to `19-spec-author.md`, and Q6 is new. The characterization tests that 21 compared mechanically are byte-identical since `d7c7186`, so that evidence stands. The Q4, RH1001 Q1, and RH1001 Q2 "Touches … coverage sentence leaves this case out" claims hold for the rewritten coverage sentences. Q6's claim holds for the new one.
- Open questions in specs: `rg` over the three specs finds no question, Q number, or tracker reference. The coverage sentences name the left-out forms neutrally, as the decisions entry asks.
- Maintainer constraints of 00-lead: no production change, no requirement ID added, moved, or retired, no shared utility newly claimed, and no open question written into a spec. All hold.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/shared/struct-equality.md:95` (SHARED-EQUAL-002, not-custom list, "a null-conditional access whose receiver has a form of the first item"); tagged tests in `RH1002…AnalyzerTests.cs` and `RH1001…AnalyzerTests.cs` | FC-003, FC-017 | PR-introduced (the repair widened the item from "whose receiver is `null`" to the forms of the first item) | The item now names these receivers by reference: a cast of `null`, a `const` field, a `const` local, `default` of a reference type, and `default` of a nullable value type. Only the cast-of-`null` receiver has a tagged test: `((Holder)null)?.Comparer` in the example and its tests, and `((ComparerHolder)null)?.Comparer` in RH1002 `CompositeNullComparerTestData` and in the RH1001 test data. Nothing tagged pins `NullHolder?.Comparer` with `const Holder NullHolder = null;`, a `const` local receiver, `default(Holder)?.Comparer`, or `default(VH?)?.Comparer`. A change that dropped `IsNullLikeOperation(conditionalAccess.Operation)` for constants or `default` receivers would pass every tagged test. This is the same kind of gap as item (d) of `24-spec-checker.md`, but on the not-custom side, which that check did not cover. Today all four are reported, as the item says (scratch run above), so only the tests are missing. The decisions entry asked for a tagged test for the `null` constant, and a `const` receiver is one of its forms. | Add a tagged `SHARED-EQUAL-002` test to the RH1002 class. Mark the four receivers on the reported side. Add an unmarked contrast with a non-`const` local receiver, `Holder notConst = null; items.Distinct(notConst?.Comparer);`, which is custom today. No spec text changes. |
| 2 | low | `RH1002…AnalyzerTests.cs:1738` (summary) and `:1750` (name of `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom`) | FC-031 | PR-introduced (the test came in this run at `0c9ba77`, and the summary was rewritten at `9e806da`) | The rewritten summary says "a variable passed as a named or non-trailing named argument". In `Enumerable.Distinct(source: items, comparer)` the variable `comparer` is a positional argument that follows the non-trailing named argument `source:`. It is not itself a named argument. The method name still asserts that only arguments that can produce a comparer count as custom. SHARED-EQUAL-002 now says the opposite for a non-`const` local holding `null` and for a call returning `null`, which `VerifyPropertyOrMethodCallArgumentIsCustomComparer` pins. The name and the summary are test text only. They cannot reach compiled analyzer behavior, diagnostics, or rule metadata. | Reword the phrase, for example "a variable passed as a named argument or positionally after a non-trailing named argument". Rename the method to a form-based name, for example `VerifyFormOfComparerArgumentDecidesWhetherItIsCustom`, and keep its tags. |

## Guard scope

No code predicate changed. The table compares each changed requirement statement with the predicate that makes the decision.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-EQUAL-002, not-custom item 1: a constant with the value `null`, or `default` of a reference or nullable value type | the argument expression and its constant value | `IsNullLikeExpression` (`GetConstantValue`), `IsNullLikeOperation` (`ConstantValue` after `UnwrapOperation`), `IsNullLikeDefaultType` | agree: `const` field, `const` local, another class's `const`, `(ValueComparer?)null`, and the existing literal, cast, `default`, and `default(ValueComparer?)` lines. Type parameters are left out by the coverage sentence. A user-defined cast of the literal is custom in the code and in the custom list (hint 1) | ok |
| SHARED-EQUAL-002, not-custom item 7: a null-conditional access with a receiver of the first item | `conditionalAccess.Operation` | `IsNullLikeOperation(conditionalAccess.Operation)` in both `IsNullLikeOperation` and `IsDefinitelyNonCustomEqualityComparerOperation` | agree for a cast of `null`, `const` field, `const` local, `default(Holder)`, and `default(VH?)` (scratch) | ok; tests are finding 1 |
| SHARED-EQUAL-002, `switch` items: at least one arm, or one arm custom | the arms | `ISwitchExpressionOperation { Arms.Length: > 0 }` in all three predicates | agree; a switch without arms is left out | ok |
| SHARED-EQUAL-002, custom list | the argument expression through wrappers | every predicate falls through to `false` for these forms | agree for a variable, a non-`const` field, a `static readonly` field holding `null`, a property, a call, a creation including `new ValueComparer?()`, an array element, a user-defined cast, `as`, a cast, parentheses, `!`, `??` with a custom left operand, both conditional clauses, a `switch` with a custom arm, and a null-conditional access on a variable, field, property, or call | ok |
| SHARED-EQUAL-002, the coverage sentence on a whole argument in parentheses or behind `!` | the inner form | `GetConstantValue` and the syntactic default walk only | agree; `const`-based inner forms are constants, and `default(ValueComparer?)!` is custom and unspecified | ok |
| RH1001-003, reported list | the initial value, through parentheses, `!`, non-user-defined casts, conditionals, and `switch` arms; for several variables, any one of them | `HasExplicitEqualityComparerInitializer` (`default: false`; `Variables.All`) | agree for every item. `as` and a creation of a derived type are on neither list and are unspecified | ok |
| RH1001-003, the exempt `switch` item: at least one arm | the arms | `SwitchExpressionSyntax { Arms.Count: > 0 }` | agree | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Custom comparer argument (SHARED-EQUAL-002) | `EqualityComparerArgumentUtilities.HasExplicitEqualityComparerArgument`. Its only callers are `RH1001…Analyzer.HasExplicitEqualityComparer` (line 239) and the RH1002 analyzer (line 315). Unchanged since 21 | single owner |
| Initial value exempts the declared type (RH1001-003) | `RH1001…Analyzer.HasExplicitEqualityComparerInitializer`, which delegates each creation's argument to the utility above | single owner |
| Constant-condition branch choice | `IsNullLikeConditional`, `IsDefinitelyNonCustomConditional`, `IsFrameworkDefaultConditional` (the utility), and `HasExplicitEqualityComparerInitializer`. Each decides its own question with the same `bool` constant rule | unchanged since 14; ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH1002 · `VerifyConstantWithValueNullIsNotCustomComparer` (`SHARED-EQUAL-002`) | a `const` field, a `const` local, and a conditional of `null` and a `const` are not custom; a non-`const` local is custom | a missing or extra report (23: a mutation on `notConst` failed, 4 vs 3) | `Diagnostics(…, 3)`, exact set and message | adequate |
| RH1001 · `VerifyCreationWithConstantNullComparerIsNotExempt` (`RH1001-003`, `SHARED-EQUAL-002`) | a creation passing a `const` `null` is reported; one passing a non-`const` local is exempt | a mutation on `c` failed, 3 vs 2 | exact, 2 | adequate |
| RH1002 · `VerifyDefaultOfTypeParameterNotConstrainedToValueTypeCountsAsNoComparer` (untagged) | characterizes Q6 | a report on `default(TV)` (mutation failed, 4 vs 3) | exact, 3; input identical to Q6 | adequate |
| RH1001 · `VerifyCreationWithDefaultOfTypeParameterNotConstrainedToValueTypeIsReported` (untagged) | characterizes Q6 (RH1001 line) | a report on `other` (mutation failed, 2 vs 1) | exact, 1; input identical to Q6 | adequate |
| RH1002 · `VerifyConstantNullConvertedByAsIsNotCustomComparer` | `as` around a constant `null` is not custom | a missing report (mutation failed, 1 vs 2) | exact, 2 | adequate; the custom side of `as` is in the next-but-one test |
| RH1002 · `VerifyPropertyOrMethodCallArgumentIsCustomComparer` | a property and a call are custom; `Default` is not | a report on `Prop` (mutation failed) | exact, 1 | adequate |
| RH1002 · `VerifyCustomFormConvertedByAsIsCustomComparer` | `as` around a field, an `object` field, or a creation is custom; `null as` is not | a report on `_field as …` (mutation failed) | exact, 1 | adequate |
| RH1002 · `VerifyNullConditionalAccessOnFieldPropertyOrCallIsCustomComparer` | a field, property, or call receiver is custom; a cast-of-`null` receiver is not | a report on `_holder?.Comparer` (mutation failed) | exact, 1 | adequate for the custom side; the not-custom receivers are finding 1 |
| RH1001 · `VerifyWrappedVariableInitialValueLeavesDeclaredTypeReported` | `(_other)` and `_other!` leave the declared type reported | a missing report on `b` (mutation failed) | exact, 3 including the field | adequate |
| RH1001 · `VerifyConstantConditionChoosingCreationWithoutComparerLeavesDeclaredTypeReported` | the chosen branch decides, on both `true` and `false` | a missing report on `a`, or a report on `c` | exact, 2 | adequate |
| RH1001 · `VerifySwitchWithArmWithoutCustomComparerLeavesDeclaredTypeReported` | one reported arm, or a variable arm, leaves the type reported; all-custom arms exempt it | a missing report on `a`, or a report on `c` | exact, 3 including the field | adequate |
| RH1002 · `VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` (summary changed) | unchanged input and assertion | unchanged | exact, 10 | assertion adequate; text is finding 2 |

The new tagged tests contain only specified forms. The two characterization tests carry no `[Requirement]`, and their summaries start with "Characterizes".

## Failure classes dismissed

- FC-001, FC-005, FC-016: no guard was added, narrowed, or moved, because no production code changed.
- FC-002: every qualifier of the rewritten lists has a counterpart condition in the code (Guard scope above). The qualifiers I checked: "at least one arm", "not a constant", "whose condition is a constant", and "a form of the first item".
- FC-003: both sides of each changed item are tested, except the not-custom null-conditional receivers (finding 1).
- FC-004, FC-019 to FC-021, FC-030: RH1001 and RH1002 offer no fix, and no trivia path is involved.
- FC-006, FC-022 to FC-026, FC-036: no rewrite.
- FC-007 to FC-009, FC-018: no code fix changed.
- FC-010 to FC-014, FC-028, FC-029: no formatter change.
- FC-015: see Policy ownership.
- FC-017: see Assertion adequacy. Every new test asserts the exact diagnostic set with messages, and 23 and 25 recorded one failing mutation per test.
- FC-027: I checked these sibling shapes of the changed items against the code, and all agree:
  - a nullable creation, an array element, and `?[]`;
  - `Default` through a derived class;
  - a `const` of another class;
  - a single-arm `switch`;
  - `(ValueComparer?)null` and a user-defined cast of the literal;
  - a non-user-defined cast around a user-defined cast;
  - `!` around `default` of a nullable value type.
- FC-031: no production method changed. The changed test summary is finding 2. The summaries of the 11 new tests describe their inputs.
- FC-032, FC-035: no compiled change. The expected values come from the unchanged analyzer.
- FC-033: the diagnostics are located by markup, and no language version or line ending is involved.
- FC-034: the Q6 characterization tests use the issue text's exact input (mechanical comparison).
- FC-037: no claim that a `.cs` change is text-only.

## Hints

1. Not-custom item 1 explains "a constant with the value `null`" as "the literal `null`, a cast of it, or a `const` field or local". A cast of the literal through a user-defined conversion, such as `(ValueComparer)null` with `implicit operator ValueComparer(string s)` on a struct comparer, is not a constant. The code counts it as custom, and the custom list says so ("a cast with a user-defined conversion, whatever its operand"). A reader who takes "a cast of it" literally could still place it on both lists. Such a conversion exists only for a value-type target, because for a reference type the null-literal conversion wins. It is uncertain whether this needs wording such as "a cast of it that is not a user-defined conversion".
2. The `switch` without arms was not filed as a question. That matches the decisions entry and the lead's call recorded in 22. A switch without arms always throws, so if the maintainer decides struct-equality Q4 or RH1001 Q1 toward "a `throw` produces no value", the decision also reaches this form.
3. Hints 3 and 4 of `14-auditor.md` are still with the lead.
