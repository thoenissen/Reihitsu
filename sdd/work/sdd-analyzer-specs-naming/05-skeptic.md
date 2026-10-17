Role: skeptic
Commit: 747b31a
Verdict: proceed narrower

## Scope

I checked only the repair, `git diff 7d6b36c 747b31a -- sdd/specs`, against the code and against `04-spec-author.md`. Parts confirmed in `03-skeptic.md` were not re-reviewed.

I ran new inputs through `scripts/apply-fix.sh <RH####> <fixture-dir>`, with LF and CRLF agreeing in every arm. All fixtures were outside the repository and have been deleted.

## Result

Each finding is repaired where it was raised. Three repairs leave residual claims, and one repaired requirement still claims an input I missed in round 1 (N1). Four small repairs remain (R1–R4). No new requirement conflicts with another spec.

## Findings as repaired

| Finding | Status | Evidence |
|---|---|---|
| F1 | Holds in the shared spec; residual in the rule specs (R1) | The narrowed clause of SHARED-CASING-009 matches `RenameWithoutConflictAsync`. The coverage sentence of SHARED-CASING-007 matches the code on all three controls: enum members are covered (`enum color { Color }` → `enum Color { Color }` compiles), members and type parameters are not, and nested types still go through the compile-error check |
| F2 | Holds | `URL` → `IURL` and `Document_STORE` → `IDocumentStore` match my runs. N1 is a separate, older claim in the same requirement |
| F3 | SHARED-CASING-008 holds; the keyword sentence of SHARED-CASING-007 is imprecise (R2) | See R2 |
| F4 | Holds | The coverage of RH4103-001 excludes `extern` and `[LibraryImport]`. It is wider than the question needs (an `extern` method with an explicit `EntryPoint` renames safely), which is acceptable for an open-question carve-out |
| F5 | Holds, with one unverified edge (below) | RH4102-001 and RH4103-001 now name overrides and implementations of members declared in source; the examples compile |
| F6 | RH4001-002, RH4001-005 and RH4009-002 hold; residual in RH4001-003 (R4) | See R4 |
| F7 | Holds | The condition now matches the domain of thoenissen/Reihitsu#971; `iPhone` → `_iPhone` |
| F8a | Holds | Coverage is limited to names of letters, digits, and `_` |
| F8b | Holds | "in the active code of the solution" |
| F8c | Holds | RH4001-001 covers only files without top-level statements |
| RH4105-001 | Holds | Anonymous methods are reported |
| RH4009-001 | Holds | Matches `ParseAllowedNamespaceDeclarations`. The configuration is returned even when it has validation errors, so `[ "A", 1 ]` stays active |

## Remaining repairs

### R1 — The rule fix requirements do not inherit the carve-outs of SHARED-CASING-007 (residual of F1 and F3)

The carve-outs for namespace types and for keywords sit only in SHARED-CASING-007. Each affected rule still states its fix as an unrestricted MUST and cites SHARED-CASING-007 only for "every reference":

- **Namespace types:** RH4002-002, RH4003-002, RH4005-002, RH4006-002, RH4010-002 and RH4011-002. Read on its own, each says "MUST rename … to the PascalCase name that SHARED-CASING-006 derives". That covers, for example, `class order { public int Order; }` → `class Order { public int Order; }`, which does not compile (CS0542) and is shared Q1.
- **Keywords:** RH4105-003 covers `int Class` → `@class`, which is shared Q2.

The same repair round scoped RH4102-003 and RH4103-003 explicitly ("for the events/methods that RH4102-001/RH4103-001 covers"). These seven requirements need equivalent scoping to what SHARED-CASING-007 covers. RH4004-002 is not affected, because enums have no type parameters and their members are enum members.

### R2 — "not a C# keyword" in SHARED-CASING-007 matches neither reading of "keyword"

Observed with `apply-fix.sh RH4105`, each fixed in one iteration unless stated otherwise:

| Input | Result |
|---|---|
| `async Task<int> M(int Await) { await Task.Yield(); return Await; }` | `@await` in the declaration and at the use |
| `Value => Value` | `value => value` |
| `int M(int Var) { var y = Var; … }` | `var` |
| `int M(int Nameof) => Nameof` | `nameof` |
| `M(int Yield) { yield return Yield; }` | `yield` |
| `int this[int Field] { get { return Field; } }` | `field` |
| `Field => Field + 1` inside a property `get` accessor | No fix offered |

Neither reading of "C# keyword" fits these results:

- **Contextual keywords included:** RH4105-003's own example (`Value` → `value`) falls outside SHARED-CASING-007's coverage.
- **Reserved keywords only:** `@await` is covered, and that is exactly the verbatim output that shared Q2 questions.

The carve-out has to describe the outcome instead: the derived name is not one the renamer has to write as a verbatim identifier. Today that means a reserved keyword anywhere, and `await` inside an async member. Add the `await` input to shared Q2.

### R3 (N1) — RH4005-002 claims letterless names, which the code fixes in two applications

I missed this in round 1. Observed with `apply-fix.sh RH4005`:

| Input | First application (`--max-iterations 1`) | Final, after 2 iterations |
|---|---|---|
| `interface _ { }` | `interface I_ { }`, still reported (not-converged) | `interface II { }` |
| `interface __ { }` | `interface I__ { }`, still reported (not-converged) | `interface II { }` |

Mechanism: `ToPascalCase` returns a letterless input unchanged, and `OnTransformIdentifier` then puts `I` in front. The result is a valid identifier that differs from the old name, so `TryGetFixedIdentifier` registers the fix.

RH4005-002 claims these inputs:

- "starts with any other character than `i` or `I` → nothing dropped";
- "withheld as SHARED-CASING-008", which withholds the fix when the old name has no letter or digit.

The observed fix also needs two applications, which contradicts SHARED-FIX-001. Keep old names without a letter or digit out of RH4005-002, and add them to RH4005 Q1. They belong to the same family as the bare `I` → `II`.

### R4 — RH4001-003 still claims verbatim type-parameter names (residual of F6)

Observed with `apply-fix.sh RH4001`: the file `Cache{TKey}.cs` with `public class Cache<@TKey> { }` is reported, and the fix renames it to `Cache{@TKey}.cs`.

`GetExpectedFileNameStem` uses `Identifier.Text` for type parameters too. RH4001-002 and RH4001-005 now exclude a type name written with `@`, but RH4001-003 ("the names of its type parameters") still covers `@TKey`. Scope RH4001-003 to type parameters written without `@`, and add the input to RH4001 Q1.

## Smaller points (no repair required)

- **"letter" in SHARED-CASING-001/-002/-003/-006.** The C# identifier grammar counts letter numbers (category Nl, for example `Ⅻ`) as letters, but `char.IsLetterOrDigit` does not. Likewise, `char.IsUpper` accepts only Lu, not titlecase Lt. Saying "Unicode letter (categories Lu, Ll, Lt, Lm, Lo)" would keep `ClassⅫ` out of the covered domain unambiguously. Low priority.
- **Transitive overrides in RH4103-001 and RH4102-001 — not run.** Take a source override of a source method that itself overrides a method of a referenced assembly. Whether "neither overrides … a method declared in a referenced assembly" includes that chain is ambiguous. The renamer follows the chain to its original definition, so the fix outcome may differ from RH4103-003. Running it needs a metadata reference that `apply-fix.sh` cannot supply; the spec author's harness can. Either verify it, or word the coverage as "whose original definition is declared in source".

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4002, RH4003, RH4005, RH4006, RH4010, RH4011 (`-002`) and RH4105 (`-003`) | Must change (R1) |
| `shared/casing.md`, SHARED-CASING-007 | Must change (R2) |
| RH4005-002 | Must change (R3) |
| RH4001-003 | Must change (R4) |
| Every other part of the repair | Must stay |
| Production code, rule pages, formatter | Not involved |

## Open questions to extend

These go into the issues listed in `04-spec-author.md`:

- **Shared Q2:** add `await` inside an async member → `@await`.
- **RH4005 Q1:** add `_` and `__` → `I_`/`I__`, then `II`, in two applications.
- **RH4001 Q1:** add `Cache<@TKey>` → `Cache{@TKey}.cs`.

## Questions for the maintainer

_None._
