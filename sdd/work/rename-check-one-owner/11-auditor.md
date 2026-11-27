Role: auditor
Commit: b49e789626242fc0ac64874148eee76e5ed747d4
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lead decision 5 names the repair shape. I audited the code against SHARED-CASING-009 and finding #1, not against that decision.

## Scope

This audits only the repair delta. Head: `b49e789626242fc0ac64874148eee76e5ed747d4`. Previous audit: `123929e54882ee2b1a6471a577a160f28af33e9e`. Base: `origin/main` `2c7702214d596aa331304502d68d83d7818750f4`. `git ls-remote` returns that SHA, `main` is merged into the head, and the checkout is the head. `scripts/lane-check.sh bug-fix` confirms the lane.

The diff matches handovers 07–10:
- In `CasingRenameSafetyUtilities`, `IsQualifierToken` is deleted.
- `FindQualifiedName` now reads the qualifier from the renamed tree. It uses the new private helpers `FindQualifiedNameToken` and `GetQualifierAndName`, and two remarks are updated.
- There are new RH4103 and RH4002 test rows. Everything else is under `sdd/work/`.

Scope did not grow. No spec, no other component, and no other source file changed.

How I checked: static tracing, plus `scripts/apply-fix.sh` probes on the head (LF and CRLF). The fixtures were in the scratchpad. I reused the evidence from `06-auditor.md` for everything the repair did not touch. That covers the token walk in `OnlyRenamesOrQualifies`, `RewritesOtherCodeAsync`, Fix All composition, and the excluded-code and compile-error checks.

## Previous findings

- **#1: closed.** Every member it listed now gets the fix the spec requires. Tests pin `int[]`, `int[,]`, `int?`, `(int, int)`, and `List<int[]>`, plus `int[][]`, `int?[]`, `(int A, string B)`, and `System.Collections.Generic.List<int[]>`. These rows are in `RH4103…FixQualifiesTheCallWithTheGenericBaseType…` and `RH4002…FixQualifiesAReferenceWithTheGenericBaseType…`. The members that held before (`int`, `List<int>`, `this.`, `Sample.`, the namespace qualifier) still hold. That covers both the regression rows and the rows that already failed on `main`.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` `FindQualifiedNameToken` / `GetQualifierAndName` (about lines 655–693), no test pins them | FC-017, FC-016 | – | new mechanism | The repair replaces a token list with a decision per syntax shape. The new tests reach only two of those shapes: a simple member access whose left side is a name or generic name, and a qualified name in a type context. Four other shapes that the renamer writes are unpinned, and they hold today only by probe. **(a)** A `base` qualifier: RH4103 with an instance `getValue` in `Base` and a local function `GetValue` gives `base.GetValue()`. This holds only through `BaseExpressionSyntax` in the accepted-qualifier check. **(b)** The renamed name as the right side of an alias-qualified name: RH4002 with a global `order`, a nested `Order`, and `order _value;` gives `global::Order _value;`. This holds only through the `AliasQualifiedNameSyntax` arm. **(c)** A multi-part qualifier in an expression context: `Outer.Base.GetValue()` (RH4103, nested base type), `A.B.Base.GetValue()` (namespaced base type), `N.Sample._value` and `global::Sample._value` (RH4106, outer type shadowed by a nested `Sample`), and `M.Order.Count` (RH4002). These pass only because the renamer builds the qualifier as a `QualifiedNameSyntax` node, and `QualifiedNameSyntax` is a `TypeSyntax`. The same text, parsed, is a `MemberAccessExpressionSyntax` chain, and that would be refused. **(d)** The `QualifiedCrefSyntax` arm. No test and no probe reaches it, because `scripts/apply-fix.sh` does not rename crefs (see Hints). If any of these arms is dropped or narrowed, the casing fixes withhold renames that `main` offers. That is the same outcome as #1, and every test still passes. | Add one focused `Verify` test each for (a), (b), and (c). For (c), use one expression-context row such as `N.Sample._value` on RH4106 or `Outer.Base.GetValue()` on RH4103. Each expected value is the renamer output shown above, which SHARED-CASING-009 requires. For (d), add a cref row through the test harness, which parses documentation comments. If the renamer qualifies no cref, drop the arm instead (FC-042 style: a branch no input reaches). |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A renamed reference may carry a qualifier the renamer inserted | Renamed tree: the innermost `SimpleMemberAccessExpression`, `QualifiedName`, `AliasQualifiedName`, or `QualifiedCref` that starts at the reference's first renamed token and whose name is spelled with the old or the new name. Its left side must be `this`, `base`, or any `TypeSyntax`. The name is then found by token identity, and every token in between is the qualifier | SHARED-CASING-009 ("renames and qualifies with `this.`, `base.`, a type, or a namespace"); RH2001 `RenameChangesBindingAsync`, which also takes the qualifier from member-access and qualified-name syntax | Agrees. The renamer builds qualifiers as name nodes, so `TypeSyntax` covers type and namespace qualifiers in every context I probed. Lookup by identity fixes a type argument spelled like the name (`Base<Order>.Order`): the old walk stopped at the first matching spelling. A qualifier that contains the name itself (`N.Order` around `order`) is still refused, as on `main`. A local or parameter used as a qualifier is syntactically a `TypeSyntax`, but the renamer never inserts one, and the old token list accepted such identifiers too | holds |
| Tokens after the name (type arguments, cref parameters, a cast or call-shape change) | Compared one by one by the unchanged token walk, after `nameIndex + 1` | SHARED-CASING-009 "beyond renaming it and qualifying" | Unchanged from the previous audit. The pinned cast-argument and static-call tests still refuse | holds |
| Pointer member access, conditional access | `GetQualifierAndName` accepts only `SimpleMemberAccessExpression`. Member bindings are never qualified | — | The renamer does not qualify these | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Qualifier shape of a renamed reference | `CasingRenameSafetyUtilities.FindQualifiedNameToken`/`GetQualifierAndName`, called once from `FindQualifiedName`; `RH2001…CodeFixProvider.RenameChangesBindingAsync` (`rg -n "IsQualifierToken\|GetQualifierAndName\|FindQualifiedName\|RenameChangesBindingAsync\|RewritesOtherCodeAsync" src --glob '*.cs'`) | Decision 1 keeps two owners, and the spec allows that. Both now read syntax instead of predicting tokens. `IsQualifierToken` is gone and has no remaining callers |
| Whether a rename only renamed and qualified | `RewritesOtherCodeAsync` (one caller, `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync:463`) | One owner, unchanged |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4103…FixQualifiesTheCallWithTheGenericBaseTypeNextToALocalFunctionWithTheNewName` (10 rows) | A member access whose left side is a generic type is accepted, whatever the type argument | No fix offered, or any other token changes | `Verify` (full text, single fix, Fix All) | adequate. Covers both sides of the token-kind boundary of #1 (FC-003) |
| `RH4103…FixQualifiesTheCallWithTheGenericBaseTypeWhoseTypeArgumentHasANamespace` (2 rows) | Same, with a qualified type argument (the old walk stopped at an identifier) | same | `Verify` | adequate |
| `RH4002…FixQualifiesAReferenceWithTheGenericBaseTypeThatANestedTypeOfTheNewNameWouldCapture` (3 rows) | A `QualifiedName` in a type context with a generic left side is accepted | same | `Verify` | adequate |
| The `base`, alias-qualified, expression-context multi-part, and cref arms | Each accepted shape stays accepted | An arm is dropped and fixes are silently withheld | none | missing, see #2 |

The expected values come from SHARED-CASING-009, which this run does not change (check 10). The rows that `main` withheld and the head now fixes (`(int, int)`, `(int A, string B)`, `System.Collections.Generic.List<int[]>`) are required by that same sentence, and lead decision 5 covers them.

## Failure classes dismissed

- FC-001, FC-002, FC-044: the guard now reads the renamer's result. See Guard scope.
- FC-003: both sides of the old boundary are tested.
- FC-004, FC-005, FC-019, FC-020, FC-021, FC-022, FC-023, FC-024, FC-026, FC-027, FC-036: unchanged from `06-auditor.md`. The repair does not touch the trivia handling of the token walk, its comparison of literals, verbatim identifiers, and partial-method names, its positional alignment, or `nameof`.
- FC-006, FC-018: the compile-error check and the renamer call are unchanged.
- FC-007, FC-009 to FC-014, FC-028 to FC-030, FC-038 to FC-043: the formatter, precheck, and layout are not involved.
- FC-008: Fix All passes each candidate's own `identifier` through the same check. The existing Fix All tests pass (implementer: 12102 analyzer tests, cleaner: 1108 casing tests).
- FC-015: covered under Policy ownership.
- FC-031: the remarks of `OnlyRenamesOrQualifies` and `FindQualifiedName`, and the summaries of the two new helpers, describe the new code. The innermost-node rule in the remarks matches the early `return` in `FindQualifiedNameToken`. The RH2009 extraction is form-only: the logic is the same as in the implementer's version.
- FC-032 to FC-035, FC-037: the expected values come from `main` and from direct renamer probes, not from patched code. LF and CRLF give the same result in every probe.
- Performance: one ancestor walk, bounded by the nodes that start at the token, plus a forward scan to a nearby identifier. Negligible next to the compile check.
- No `TODO` comments, no commented-out code, no tracker numbers.

## Architecture findings

_None._ The structural cause of #1, a guard that predicts the renamer's qualifier, is removed. #2 is only a test gap.

## Hints

- `scripts/apply-fix.sh` does not rename crefs. RH4103 on `Derived : Base<int[]>`, with `/// <see cref="getValue"/>` and no conflict, renames the method and its call but leaves the cref text as `getValue`. The analyzer test harness renames crefs (`RH4106…:550/560`, under `DocumentationMode.Parse`). So the difference is in the runner's workspace setup, which I did not trace further. The runner therefore cannot probe the cref arm, nor any cref behavior of the casing fixes. This is pre-existing tooling and not part of this run.
- Still in place from the previous audit: the `RewritesOtherCodeAsync` remark "instead of a text diff whose alignment is a heuristic" (line 357). It gives the reason for the design, so it is not FC-031.

## Follow-up candidates

_None._ The cref behavior of the fixture runner is only a hint, because I did not confirm its cause.
