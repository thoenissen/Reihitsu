Role: auditor
Commit: d7c71862dc9118074c6309eefba67cf201c6c319
Verdict: findings

## Scope

State:
- The checkout is the head `d7c7186`, and the working tree was clean before this file.
- `origin/main` and `git ls-remote origin refs/heads/main` both give `e84b26c9854b4014c96c30f8b3a549998a5b7396`, which is an ancestor of the head.
- `git show --cc` and `--remerge-diff` of the merge `d7c7186` are empty, so the merge was clean. `git diff f299f87 HEAD` on every file the repair touched is empty, so the merge brought only main's own content: the RH8101–RH8204 specs, tests, and rule pages, `shared/element-documentation.md`, the ledger and index rows, and one decisions entry.
- The ledger passes `LC_ALL=C sort -c` and has no duplicates.

What I audited:
- This is a repair delta against my previous audit, `14-auditor.md` at `9f735c5`, whose handover commit is `53efd05`.
- The repair commits are `f64144f`, `c5569d1`, `58cd1a2`, `f1735c4`, `c22e86c`, `9f8ae8e`, `ecb84ce`, and `f299f87`. Outside `sdd/work/`, they change these files:
  - specs: `sdd/specs/README.md`, `analyzer/RH1001.md`, `analyzer/RH1002.md`, `analyzer/RH2001.md` (header only), `shared/struct-equality.md`;
  - the decisions entry `squad/decisions/2026-10-20-996-auditor-block-extra-repair.md`;
  - the RH1001 and RH1002 test classes.
- No file under `src/Reihitsu.Analyzer`, `src/Reihitsu.Analyzer.CodeFixes`, `src/Reihitsu.Core`, or `src/Reihitsu.Formatter` changed since `53efd05`. So the code evidence of `14-auditor.md` is byte-identical, and I reuse it.
- `scripts/lane-check.sh tooling`: "LANE CHECK: confirmed — claimed tooling, diff needs tooling".
- The scope did not grow: the repair stays within the worklist that the decisions entry allows.
- My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the scope and the constraints of 00-lead.

Status of the previous findings and hints:

| Item of 14 | Status | Evidence |
|---|---|---|
| Finding 1: RH1002-001 and query syntax | closed | RH1002-001 has an invocation-only coverage sentence that leaves out `group … by`, `join`, and `join … into`. RH1002 Q2 is filed, and `VerifyQueryExpressionClausesAreNotReported` pins it. The analyzer registers only `InvocationExpression`. |
| Finding 2: `throw` and `with(…)` in RH1001-003; SHARED-EQUAL-002 `throw` | closed | RH1001-003 now lists its forms exhaustively, with coverage sentences for a `throw` branch or arm and for a custom `with(…)`. SHARED-EQUAL-002 has the matching `throw` coverage sentence. Struct-equality Q4 and RH1001 Q1 and Q2 are filed and pinned. Both lists agree with `HasExplicitEqualityComparerInitializer` and `EqualityComparerArgumentUtilities` for every form named in 14. The SHARED-EQUAL-002 list rewritten for this finding has new gaps; see findings 1 to 3. |
| Finding 3: not-reported sides | closed | The SHARED-EQUAL-002 example and its tagged test now cover the custom side of parentheses, `!`, `??`, a `throw` branch, `holder?.Comparer`, a user-defined cast, and a local holding `null`. The RH1001-003 example and its tagged test cover the user-defined cast pair, `??`, a variable, `default`, `[]`, and the exempt cast `p`. `16-test-implementer.md` corrects the claim made in `04`. |
| Hint 1: a local holding `null` | closed, superseded in part | A non-`const` local is now specified behavior and is in the example. The generalization to "a field" contradicts the code for constants; see finding 1. |
| Hint 2: `PropertySymbolExtensions` | closed | It is claimed in the RH2001 header and the index row. Its only consumer is `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer` (line 46), and no other spec claims it. The `IsAutoPropertyAccessorList` helpers in `LineBreakDetection`, RH5047, and RH5408 decide accessor-list syntax, which is a different decision. |
| 18's observation on `as` | closed | 19 names the `as` operator in the list. `UnwrapOperation` strips every conversion that is not user-defined, and an `as` conversion never is. |

How I checked:
- I traced the code statically, using `EqualityComparerArgumentUtilities` and `RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer` at the head.
- I settled each suspicion with a scratch `net10.0` / `LangVersion preview` project in the scratchpad, outside the repository and deleted afterwards.
  - It used the head's Release `Reihitsu.Analyzer.dll` and `Reihitsu.Core.dll` as `<Analyzer>` items. They were built 2026-10-20, after the last analyzer or Core source change of 2026-10-17.
  - I read the results from `dotnet build`, which reported 0 errors.
- I compared the issue-text inputs mechanically, with a Python script in the scratchpad, also deleted. It extracted each issue-text block from 15 and 19, and each characterization test's raw string, removed the indentation and the `{|#n:…|}` markup, and compared them.
  - Struct-equality Q4 and Q5, RH1001 Q1 to Q3, and RH1002 Q2 are identical.
  - The SHARED-EQUAL-002 and RH1001-003 spec examples are identical to their tagged tests, with the markup kept at the same positions: 11 and 15 markups.
- The RH1001 lines of Q4 and Q5 sit unchanged in a minimal frame. Their outputs match the issue texts.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `sdd/specs/shared/struct-equality.md`, SHARED-EQUAL-002: the first bullet and the sentence "Every other argument MUST count as custom … a variable, a field, a property, or a method call counts as custom whatever value it has at run time" | FC-002, FC-027 | PR-introduced (the repair's form-based rewrite; the earlier "can produce" wording agreed with the code here) | The list is now exhaustive ("exactly when its expression has one of these forms"). Its first bullet names only the literal `null` and `default`. A named constant is therefore custom, and the next sentence says outright that a field is. The code decides every compile-time constant by its value: `IsNullLikeExpression` calls `GetConstantValue`, and `IsNullLikeOperation` checks `ConstantValue` at every level. Verified in the scratch build, with `const IEqualityComparer<S> NoComparer = null;` as a member and `const IEqualityComparer<S> none = null;` as a local, all of these are reported as not custom: `items.Distinct(NoComparer)`, `items.Distinct(none)`, `items.Distinct((NoComparer))`, `items.Distinct(flag ? null : NoComparer)`, `items.Distinct(checked((IEqualityComparer<S>)null))`, and `items.Distinct(NullHolder?.Comparer)` with `const Holder NullHolder = null;`. The spec example's non-`const` `IEqualityComparer<S> none = null; items.Distinct(none);` is not reported, so only `const` makes the difference. Through RH1001-003's first sentence the same mismatch reaches RH1001. `var e = new Dictionary<S, int>(NoComparer);` is reported on its `S`, and so is the declared type of `Dictionary<S, int> a = new(NoComparer);`. RH1001-003 says a creation that passes a custom comparer MUST NOT be reported. The current behavior looks intended, because a constant `null` falls back to `EqualityComparer<T>.Default`. So the spec is what is wrong. | State the current behavior. The first bullet could read "a constant with the value `null`, such as the literal, a cast of it, or a `const` field or local initialized with it, or `default` of …" (the Q5 coverage sentence already uses "a constant with the value `null`"). Limit the "a variable, a field …" sentence to members that are not constants. Add a `const` line on the reported side of the example and its tagged test. |
| 2 | medium | SHARED-EQUAL-002, first bullet ("`default` of a reference type or of a nullable value type") together with "Every other argument MUST count as custom" | FC-002, FC-027 | same requirement (the run's spec text; the repair restated the list as exhaustive) | `IsNullLikeDefaultType` treats every type with `IsValueType == false` as null-like, including a type parameter that is not constrained to a value type. Verified with `void G<TC, TV>(IEnumerable<S> items) where TC : IEqualityComparer<S> where TV : struct, IEqualityComparer<S>`: `items.Distinct(default(TC))` is reported, and `items.Distinct(default(TV))` is not. `TC` is neither a reference type nor a nullable value type, so the spec says it counts as custom and is not reported. The behavior also looks questionable. When `TC` is a struct comparer, `default(TC)` is a real comparer, and the requirement's own Why says that one "is boxed into a real instance and is used". | Leave out `default` of a type parameter that is not constrained to a reference type or a value type, with a coverage sentence. File it as a new question in the `SDD open questions: sdd/specs/shared/struct-equality.md` text. Pin it with an untagged characterization test that uses the `TC`/`TV` input. Alternatively, if the maintainer's reading is already known, state it in the first bullet. |
| 3 | low | SHARED-EQUAL-002, "a `switch` expression whose arms all have one of these forms"; RH1001-003, "a `switch` expression whose arms all have one of these forms" | FC-002 | same requirement | A `switch` expression without arms compiles, with only warning CS8509, and both statements are vacuously true for it. The code requires at least one arm (`ISwitchExpressionOperation { Arms.Length: > 0 }`, `SwitchExpressionSyntax { Arms.Count: > 0 }`). Verified: `items.Distinct(mode switch { })` counts as custom and is not reported, although SHARED-EQUAL-002 says it is not custom. `Dictionary<S, int> c = mode switch { };` is reported on the declared type, although RH1001-003 says MUST NOT. Such a switch always throws, so this is the degenerate case of struct-equality Q4 and RH1001 Q1. | Write "a `switch` expression that has at least one arm and whose arms all have …" in both places. Or name a switch without arms in the existing `throw` coverage sentences, which leave it out. |

## Guard scope

No code predicate changed. The table compares each changed requirement statement, coverage sentence included, with the predicate that makes the decision.

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| SHARED-EQUAL-002: an argument is not custom ("exactly when") | the argument expression: literal `null`, `default`, `Default`, wrappers including `as`, `??` including `Default ?? x`, conditional, `switch`, null-conditional | `IsDefinitelyNonCustomEqualityComparerExpression`: `GetConstantValue`, then `IsNullLikeOperation`, `IsFrameworkDefaultEqualityComparerExpression` (a syntactic walk through parentheses, `!`, and casts that are not user-defined), and `IsDefinitelyNonCustomEqualityComparerOperation` | agree for every listed form, `as`, `Default ?? comparer`, a user-defined cast, a non-`const` local, and the `throw` coverage. Disagree for constants (finding 1), `default` of a type parameter (finding 2), and a switch without arms (finding 3) | findings 1–3 |
| SHARED-EQUAL-002 coverage: a whole argument wrapped in parentheses or `!` | the form inside the outermost wrapper | `GetConstantValue` plus the syntactic default walk only; `GetOperation` of a parenthesized or `!` expression gives no operation | agree on 19's inputs and the Q5 input; the sentence's "a constant with the value `null`" already covers `(NoComparer)`, which the first bullet does not (finding 1) | ok |
| RH1001-003: the declared or cast type is exempt | the initial value, or the operand of the cast, through parentheses, `!`, casts that are not user-defined, conditionals (a constant condition picks a branch), `switch` arms, and explicit or target-typed creations | `AreInitializerCreationsUsingExplicitEqualityComparers` / `HasExplicitEqualityComparerInitializer` | agree. Verified on the declared type: `new Dictionary<S, int>(_comparer) as Dictionary<S, int>` is reported, as "every other" says; `(Dictionary<S, int>)(object)new Dictionary<S, int>(_comparer)` is exempt; `false ? new(_comparer) : throw …` is reported, which the coverage sentence leaves out. A switch without arms disagrees (finding 3) | finding 3 |
| RH1001-002 coverage: collection expressions | creations written with `new` | `OnObjectCreation` / `OnImplicitObjectCreation`; collection expressions are not registered | agree (RH1001 Q3, pinned) | ok |
| RH1002-001 coverage: invocations only | calls written as an invocation | `RegisterSyntaxNodeAction(…, InvocationExpression)` | agree (RH1002 Q2, pinned) | ok |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Custom comparer argument (SHARED-EQUAL-002) | `EqualityComparerArgumentUtilities`. Its only callers are `RH1001…Analyzer.HasExplicitEqualityComparer` and the RH1002 analyzer. No code changed since 14 | single owner |
| Initial value exempts the declared type (RH1001-003) | `RH1001…Analyzer.HasExplicitEqualityComparerInitializer`, which delegates each creation's argument to `EqualityComparerArgumentUtilities` | single owner |
| Auto-implemented property (RH2001-001) | `PropertySymbolExtensions.IsAutoProperty` (its one consumer is RH2001), now a code owner of `analyzer/RH2001.md`. No other spec claims it. `IsAutoPropertyAccessorList` in `LineBreakDetection`, RH5047, and RH5408 is a syntax-level layout decision | ok |
| Clarification 4 utilities | `DeclarationModifierUtilities` and `ModifierOrderingUtilities` are still claimed by no spec of this run (`rg` over `sdd/specs`) | ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH1002 · `VerifyFormOfArgumentDecidesWhetherComparerIsCustom` (`SHARED-EQUAL-002`) | the spec example, both sides of each listed form | a diagnostic on a custom line, or one missing on a non-custom line; 16's mutation on `holder?.Comparer` failed (12 vs 11) | `Verify` + `Diagnostics(…, 11)`, markup keys, exact set | adequate for the example. Findings 1 to 3 need new lines |
| RH1001 · `VerifyOnlyInitialValuesOfRecognizedCreationFormsExemptTheDeclaredType` (`RH1001-003`) | the spec example, including the `Wrapper` operator signatures | an extra or missing diagnostic (mutation on `p` failed, 16 vs 15) | exact, 15 markups | adequate |
| RH1002 · `VerifyArgumentThatCanOnlyProduceNullOrThrowCountsAsCustom` | characterizes struct-equality Q4 | a report on the first or second call | exact | adequate; input identical to the issue text |
| RH1001 · `VerifyCreationWithComparerArgumentThatCanOnlyProduceNullOrThrowIsNotReported` | characterizes Q4 (RH1001 lines) | a report on `map` | exact | adequate |
| RH1002 · `VerifyQueryExpressionClausesAreNotReported` | characterizes RH1002 Q2 | a report on a clause | exact | adequate; identical |
| RH1001 · `VerifyDeclarationPickingBetweenCustomComparerCreationAndThrowIsReported` | characterizes RH1001 Q1 | `a` or `b` not reported, or `c` reported | exact | adequate; identical |
| RH1001 · `VerifyCollectionExpressionPassingCustomComparerThroughWithIsReported` | characterizes RH1001 Q2 under `LanguageVersion.Preview` | the report moves to `other` or disappears | exact, with a `SolutionTransforms` preview entry | adequate; identical |
| RH1001 · `VerifyCollectionExpressionArgumentIsNotReportedWhereTargetTypedCreationIs` | characterizes RH1001 Q3 | a report on `[_item]` | exact | adequate; identical |
| RH1002 · `VerifyWrappedArgumentThatCanOnlyProduceNullOrDefaultComparerIsNotReported` | characterizes struct-equality Q5 | a report on a wrapped call (mutation failed, 3 vs 2) | exact | adequate; identical |
| RH1001 · `VerifyCreationWithWrappedAsExpressionComparerArgumentIsNotReported` | characterizes Q5 (RH1001 lines) | a report on `b` | exact | adequate |

All characterization tests carry no `[Requirement]` tag, and their summaries say "Characterizes …".

## Failure classes dismissed

- FC-001, FC-005, FC-016: no guard was added or narrowed, because no production code changed.
- FC-003: the not-reported sides that 14 asked for are present (finding 3 of 14 is closed). The new gaps are findings 1 to 3.
- FC-004, FC-019 to FC-021, FC-030: RH1001 and RH1002 offer no fix, and no trivia path is involved.
- FC-006, FC-022 to FC-026, FC-036: no rewrite. The repair concerns argument and initial-value forms only.
- FC-007 to FC-009, FC-018: no code fix changed.
- FC-010 to FC-014, FC-028, FC-029: no formatter change.
- FC-015: see Policy ownership.
- FC-017: see Assertion adequacy. Each new test asserts the exact diagnostic set, and 16 and 20 recorded a failing mutation for each.
- FC-027: the gaps found are findings 1 to 3. I checked these other forms and found them consistent with the specs:
  - `as` in initial values and in arguments, a cast through `object`, and a constant-`false` conditional with a `throw`;
  - `default(Holder)?.Comparer`, which is a "receiver that is `null`" under the list's own reading, as the existing example `((Holder)null)?.Comparer` already requires;
  - named arguments, and wrappers nested inside the expression.
- FC-031: no production method changed. The summaries of the new tests describe their inputs.
- FC-032, FC-035: no compiled change. The expected values come from the unchanged analyzer and the spec examples.
- FC-033: the diagnostics are located by markup, and the preview language version is set explicitly where the input needs it.
- FC-034: every characterization test uses its issue text's exact input (mechanical comparison above).
- FC-037: no claim that a `.cs` change is text-only.

## Hints

1. No spec-checker or cleaner handover covers `ecb84ce` (19) or `f299f87` (20). 18 checked `c22e86c`.
   - I audited 19's SHARED-EQUAL-002 change above.
   - 20 ran `scripts/format.sh` ("Formatted 0 of 2") and the architecture tests.
   - No Release build is recorded after `f299f87`, and no architecture run after the merge `d7c7186`. The validator covers both.
2. The summary of the older tagged test `RH1002 · VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom` still says that expressions which can only produce `null` or `Default` count as not custom "through parentheses, `!`, casts, …". Struct-equality Q5 shows that this is false for a whole argument wrapped around a non-constant composite form. The test's own wrapped inputs are constants, so the summary is true for what it tests. It is uncertain whether this needs a change.
3. RH1001-003's `throw` coverage sentence also leaves out a conditional whose condition is a constant, such as `true ? new(_comparer) : throw …`. The code exempts it, and bullet 3 would decide it. The over-exclusion is harmless.
4. Hints 3 and 4 of `14-auditor.md` (RH2004's fix on `abstract void M();`, and RH2001's fix changing a property into a field as reflection sees it) are still with the lead (`15-spec-author.md`, "Not addressed").
