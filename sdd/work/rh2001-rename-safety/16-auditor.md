Role: auditor
Commit: 85890464fafb3a01634c7af27d047d9e880484aa
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: 8589046. `git ls-remote origin refs/heads/main` returns 4838e9a03f0736d9e4593286d0f251f3a92fb704, which matches the prompt and is an ancestor of the head.
- Repair delta against 0696846, the commit `10-auditor.md` audited. Production: `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (+83/−13). Tests: 18 new tests in `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`. Spec: RH2001-004 (coverage pointer) and RH2001-011 (paragraphs 1, 3, 4, `Why`, examples). Change doc: `rh2001-rename-binding.md`. The diff matches handovers 11–15. The repair stayed within RH2001's fix, its spec, and its tests. Scope did not grow.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Evidence: I ran `scripts/apply-fix.sh RH2001` on the head and in temporary worktrees of `main` 4838e9a and the round-0 head 0696846 (both since removed). LF and CRLF gave the same status for every fixture. Fixtures: `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/fx16`, `fx16g`, `fx16t`, `fx16d`, `fx16q`. Outputs: the `fx16*-head.txt`, `fx16*-main.txt`, and `fx16-r0.txt` files in the same directory. A `dotnet build` of `new { Value! }` and `new { (Value) }` gave CS0746 on the input, so those inputs are not valid code and are not findings.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `GivesInferredMemberNameAsync` withholds members (a)–(f), and tests pin each one. The explicit-name members stay offered. #5 reports the opposite side of this guard. |
| 2 | closed | `DescribeQualifier` maps the property to one marker. Tests pin the main member, r1, r2, r4, and `Value?._value`, all offered as on `main`. r3 and r5 take the same `MemberAccessExpressionSyntax` path. A member the enumeration did not name still fails: see #4. |
| 3 | closed | The scan now descends into structured trivia, and `VerifyNoFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheField` pins the cref. The renamed `cref="Value"` and the qualified `cref="Base._value"` are pinned as offered. I also ran `<see cref="Sample._value"/>` to an inherited field: it is withheld, which is correct. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | medium | `RH2001…CodeFixProvider.cs`: `GetOtherSymbolReferencesAsync` skip predicate (`SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, property)`), `DescribeQualifier` marker, `GivesInferredMemberNameAsync` | FC-001, FC-016 | #2. Same decision (does the rename change another reference) in the same component, and it fails the same way: the rename of the property's own references counts as a change. | PR-introduced | **Failing members** (withheld on the head; `main` offers a correct fix): (g2) `class Sample<T> { private int Value { get; set; } int Get(Sample<int> other) => other.Value + Value; }`, where `main` gives `other._value + _value`. After the rename, `other._value` binds to the constructed `Sample<int>._value`, which is not `Default`-equal to the definition, so the scan describes it as another symbol's reference. Measured withheld on round-0 0696846 as well. (g5) `new Sample<TOther> { Value = Value }` in `Cast<TOther>()`, where `main` gives `{ _value = _value }`. (q1) `Outer`/`Inner` with a local `var _value = 1; return Value._value + _value;`, where `main` gives `this._value._value + _value`. The renamer adds `this.` to the receiver, so the receiver's description changes from the marker to `this` plus the marker. RH2001-011 ¶4 says this shape keeps the fix: a local named like the field next to a reference that becomes `this._value`, and a member reached through the property's own value. **Holding:** `Sample<T> other` (g4, withheld correctly through the inferred-name guard), the receiver tests of #2. **Dependent member:** `new { other.Value }` and `(other.Value, 1)` with `Sample<int> other` (g1, g3) would ship CS1061. They are withheld today only because the comparison makes the same identity mistake. `GivesInferredMemberNameAsync` uses the same `Default` equality and misses them, so a narrowing of the comparison alone would expose them. The classes are rare: a private property read through another construction of its own generic type, and a local named like the field (which breaks local naming) next to a receiver. | Refuse this class: state in RH2001-011 that these shapes are withheld, and pin g2 and g1 with tests. The alternative is to compare `OriginalDefinition` in all three places, as `SpeculativeRebindingHelper` does. That closes g2 and g5, keeps g1 and g3 withheld, and leaves q1 to the spec text. |
| 5 | high | `GivesInferredMemberNameAsync`, the `ArgumentSyntax { NameColon: null, Parent: TupleExpressionSyntax }` arm; RH2001-011 ¶3 ("whether or not any code reads the member or element"); change doc *Why* ("withholding it is a fix against SHARED-FIX-003") | FC-001, FC-002 | – (the repair of #1 reaches too wide; this is not the failure #1 enumerated) | PR-introduced; undecided output | The guard withholds the fix for every tuple element whose name is inferred from the property. But a tuple's inferred names exist only where the tuple keeps its natural type. A deconstruction target, the source of a deconstruction, and a tuple converted to a target type drop them, so the rename changes nothing there. SHARED-FIX-003 does not demand withholding for those inputs, `main` offers a correct fix, and no maintainer answer covers the change from offered to withheld. **Failing members** (head: `no-fix-offered`; `main`: correct fix): (t1) the deconstructing constructor `internal Sample(int value, string name) => (Value, Name) = (value, name);` with get-only private properties. `main` gives `(_value, _name) = (value, name)` and `readonly` fields. This idiom is commonly written. (t5) `(Value, other) = pair;`. (t6) `(value, count) = (Value, 1);` in a `Deconstruct` method. (t2) `return (Value, 1);` from a method returning `(int, int)`. (t3) `(int First, int Second) pair = (Value, 1);`. (t4) `Use((Value, 1))` with a `(int, int)` parameter. A `foreach ((Value, var x) in …)` target takes the same arm (traced in the code, not run). **Holding:** `var pair = (Value, 1); return pair.Value;` (e, withheld correctly), every anonymous-object member (anonymous types are never target-typed, so the name is always part of the type), and explicit names. | Narrow the arm: skip a tuple that is a deconstruction target (left of `=`, or a `foreach` variable, nested tuples included) or the right side of a deconstruction assignment. Skip a tuple element whose converted type (`GetTypeInfo(tuple).ConvertedType`) does not carry the inferred name. Align RH2001-011 ¶3 and the change doc. If the lead keeps the syntactic rule, withholding for the deconstructing-constructor idiom is a refusal for commonly written code, and that is a maintainer question (`AGENTS.md`, *Code-fix coverage*; escalation 9). |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Does a reference to the property give a member its inferred name (`GivesInferredMemberNameAsync`)? | Every `AnonymousObjectMemberDeclaratorSyntax` without `NameEquals`, and every tuple-expression `ArgumentSyntax` without `NameColon`, in the referring documents of the original solution. The inferred name comes from `SyntaxFacts.TryGetInferredMemberName`, and the last `IdentifierNameSyntax` must be `Default`-equal to the property definition | The compiler's own rule: an inferred tuple name survives only in the natural type. Deconstruction and target-typed conversion drop it | Too wide for tuples (#5). Too narrow for a property reached through another construction of a generic type (#4, g1/g3, hidden by the comparison) | #4, #5 |
| Is a reference the property's own, so it is skipped (`GetOtherSymbolReferencesAsync`)? | `IdentifierNameSyntax` spelled like the field, now including structured trivia, compared to the side's property by `Default` equality. The original side now passes `originalSymbol` instead of `null` | The renamer renames every reference to the definition, including references through constructed types | Disagree on constructed generic types (#4, g2/g5). On the original side the change is a no-op: no reference spelled like the field binds to the property before the rename | #4 |
| Does a qualifier change only by the rename (`DescribeQualifier`)? | Tokens of `MemberAccessExpressionSyntax.Expression` and `QualifiedNameSyntax.Left`, where an identifier bound to the property becomes one marker | The renamer's rewrite of a receiver: the rename, plus `this.` or the type name where a local or parameter named like the field is in scope | Disagree when the renamer qualifies the receiver (#4, q1). Hold for `Value`, `this.Value`, `Value!`, the cast, and `Value?` (tests) | #4 |
| Does a cref count as a reference (`descendIntoTrivia: true`)? | Structured trivia: crefs, `paramref`/`typeparamref` names, directive identifiers | The renamer renames crefs to the property (`VerifyFixRenamesADocumentationCommentReferenceToTheProperty`). Directive and `paramref` identifiers bind the same way on both sides | Agree | holds |
| Order in `RenameChangesBindingAsync` | The inferred-name check runs per referring document, before the comparison. `originalSymbol == null` withholds | — | — | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Inferred anonymous or tuple member names | `rg -n "TryGetInferredMemberName\|AnonymousObjectMemberDeclaratorSyntax" src --glob '!*Test*'` finds the RH2001 provider (lines 213, 219) and `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs:490`. RH3002 asks a different question (parentheses), so it is not a copy | holds |
| Symbol identity across constructed generics | `SpeculativeRebindingHelper.cs:32,42` compares `OriginalDefinition`. The RH2001 provider compares the symbols directly in three places | #4 (the RH2001 provider does not follow the repository's precedent) |
| Whether a rename may be applied | Unchanged from `10-auditor.md`: `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync` and the RH2001 provider. #1's input is now withheld by both. The FC-015 follow-up stays recorded in `00-lead.md` | accepted by decisions 3 and 9 |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 7 `VerifyNoFixIsOffered…InferredName` tests (a)–(f) and the expression-bodied (f) | Diagnostic reported, no action registered | An action is registered | `VerifyNoCodeFixIsOffered` (`Assert.IsEmpty(actions)`). All were red on head 68612fc and on `main` (12) | holds |
| The 3 explicit-name tests | The fix is offered, and the explicit name stays | Withheld, or the name is renamed | `Verify` with exact text and re-analysis | holds (FC-003, the other side of the qualifier) |
| The 5 receiver tests (`Value._value`, `this.`, `!`, cast, `?.`) | Offered, with `main`'s exact output | Withheld, or other text | `Verify`. Four were red before the repair, and the `?.` test was green on both | holds. No test covers q1 or g2 (#4) |
| The 3 cref tests | The inherited `cref="_value"` is withheld. `Base._value` stays offered and unchanged, and `cref="Value"` is renamed | The opposite outcome | `VerifyNoCodeFixIsOffered` and `Verify` | holds |
| Missing | — | — | — | No test for #4 (g2, g1) or #5 (t1 at least, plus one target-typed member) |

## Visible output

- The withheld inferred-name members (a)–(f) and the cref member are demanded by unchanged SHARED-FIX-003 and RH2001-011 ¶1. The offered tests keep `main`'s output.
- #5 is a visible change (offered → withheld) that no unchanged requirement demands and no maintainer answer covers. RH2001-011 ¶3 was written in this round, and the change doc classifies it as a fix against SHARED-FIX-003. That classification is wrong for t1–t6.
- #4 is a visible change (offered → withheld). It is acceptable only as a refusal of a rare class, but the spec states the opposite for q1, and states nothing for g2.

## Failure classes dismissed

- FC-003: both sides of the explicit-name qualifier and of the conditional receiver are pinned.
- FC-004, FC-005, FC-019 to FC-021: the repair touched no trivia handling. `TryCreateFieldDeclaration` and the guards of RH2001-007/009/010 are unchanged.
- FC-006: compile breaks of the inferred-name class are closed (#1). The remaining members are false refusals, not broken output.
- FC-007, FC-008, FC-009: the guard runs per diagnostic at registration, and `VerifyNoCodeFixIsOffered` asserts no action. Fix All only applies registered actions.
- FC-010 to FC-014, FC-028, FC-038, FC-039, FC-041 to FC-043: no formatter phase, precheck, or formatter call changed.
- FC-015: see *Policy ownership*. No new copy.
- FC-017: exact-output and empty-action assertions.
- FC-018: still `Renamer.RenameSymbolAsync`.
- FC-022, FC-023: no literal is touched. A verbatim `@Value` has `ValueText` `Value`, and `TryGetInferredMemberName` returns `ValueText`.
- FC-024: partial parts go through `GetReferringDocuments`, which is unchanged.
- FC-025, FC-026: no expression-body or token-position logic changed. The expression-bodied (f) is pinned.
- FC-027: the query projection (d) and the expression body are pinned. Enclosing scopes of tuples are #5.
- FC-029, FC-030: not involved. The fix still formats only the field declaration.
- FC-031: the summaries and remarks of `RenameChangesBindingAsync`, `GivesInferredMemberNameAsync`, `GetOtherSymbolReferencesAsync`, and `DescribeQualifier`, and the `property` parameter doc, describe the new code. There is no `TODO` and no tracker number.
- FC-032: the cleaner's Release build and focused run are at df5b018. Only `[skip ci]` text commits follow it.
- FC-033, FC-034: apply-fix gave identical results for LF and CRLF. The tests are the reported shapes.
- FC-035: expected values were measured on `main` (12).
- FC-036: an anonymous projection and `ToString()` are #1, now closed. A tuple name is #5.
- FC-037: no text-only claim.
- FC-040: see *Hints*. The renamer and the scan both read crefs from the same parse, so they cannot disagree with each other.

## Architecture findings

#4 repeats the class of #2. The component is `RH2001…CodeFixProvider.GetOtherSymbolReferencesAsync` with `DescribeQualifier`. The decision is whether the rename changes another reference. The comparison is built on a description that the property's own rename can change: it uses the receiver's text and the identity of the constructed symbol. Each repair enumerates one more way it can change. Inputs: g2, g5, q1. The lead decides under `README.md`, *Architecture findings*, refusing the rare class first. #5 has no structural cause: it is the scope of one new guard.

## Hints

- FC-040: in a project whose parse options use `DocumentationMode.None`, `///` is plain comment trivia. The scan then sees no cref, and the renamer renames none, so the fix is offered and the inherited cref is left as written. The compiler does not bind crefs in that mode, so the build is unaffected. Not a finding.
- `(Rest, 1)` with a property `Rest`: a tuple does not infer a reserved name, but `TryGetInferredMemberName` returns `Rest`, so the fix is withheld (n6). `main` offers it. Such a name is rare, and the narrowing in #5 would cover it if it checks the converted type.
- The validator should confirm the analyzer project's full test count. 13 reports 8432 after an OOM kill, against 9887 earlier.
