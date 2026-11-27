Role: skeptic
Commit: 20a49339
Verdict: #1087: output question (proceed on the maintainer's answer; spec amendment needed); #1088: proceed; #1978: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The maintainer's answers recorded in `00-lead.md` (#1087 round 3, #1088 camelCase, #1978 option A) are taken as binding and override the issue texts. I checked the spec diff of f9c9426f, both change docs, and the code that decides today's behavior: `RH4115LocalVariableCasingAnalyzer`, `RH4117DeconstructionVariableCasingAnalyzer`, `RH4105MethodParameterCasingAnalyzer`, `RH4124…Analyzer` (range-variable dispatch), `CasingCodeFixProviderBase{T}`, and the RH4105/RH4115/RH4117 fix providers.

Evidence commands (fixtures in the session scratchpad, outside the repository; the tree at 20a49339 carries `main`'s analyzer and fix code, only resource texts differ):

- `scripts/apply-fix.sh RH4105|RH4106|RH4115|RH4117 <dir>` — LF and CRLF agree in every result quoted below.
- The spec author's renamer probe (scratchpad `renprobe`, Roslyn Workspaces 5.12.0): `--compile <dir>` compiles C# files; `<file> <Name>` renames a range variable with `Renamer.RenameSymbolAsync` and prints the result and its compile errors. It is the bare renamer, without the fix base's guards.

## thoenissen/Reihitsu#1087

### Premise

Real; every reported split reproduces on `main`'s code. Fixture (`Deconstruct.cs`):

```csharp
using System.Collections.Generic;

class C
{
    int M((int, int) pair, (object, object) objects, List<(int, int)> pairs)
    {
        (var First, var Second) = pair;
        foreach ((var Key, var Value) in pairs)
        {
        }
        if (pair is var (Left, Right))
        {
            return Left + Right;
        }
        if (objects is (var Item, int Number and > 0))
        {
            return Number;
        }
        if (objects is (string { Length: > 0 } Text, _))
        {
            return Text.Length;
        }
        return First + Second;
    }
}
```

- `scripts/apply-fix.sh RH4117` → `no-diagnostic`.
- `scripts/apply-fix.sh RH4115` → renames `First`, `Second`, `Key`, `Value`, `Item`, `Number`, `Text`; `Left` and `Right` untouched. So `(var A, var B) = p` and `foreach ((var K, var V) …)` are RH4115's, and `is var (A, B)` is no rule's — Q3/Q4 of the issue.
- Code: RH4117 registers only `DeclarationExpression` with a `ParenthesizedVariableDesignation`; RH4115 registers `SingleVariableDesignation` with `Parent: not ParenthesizedVariableDesignation`. The `var` pattern's parenthesized designation is excluded by RH4115 and never visited by RH4117 — that is the gap.
- `_`: `RH4115LocalVariableCasingAnalyzerTests.NoCodeFixForLetterlessLocalVariable` pins `int _ = 42;` reported without fix. A `_` in a pattern, an `out`, or a deconstruction is a `DiscardDesignation` and never reaches the analyzer, so RH4115-004 only concerns declarators, `foreach`, `catch`, `using`, `for`, `fixed`.
- Local constant: `ConstLocal.cs` (`const int MaxValue = 3; … return MaxValue;`) is renamed to `maxValue` by RH4115.

Caller: the analyzers run by the compiler on user code, and the RH4115/RH4117 code fixes in the IDE.

### Mechanism and candidates

Not a bug against a spec; a behavior change. The structural cause of the reported gap is under *Architecture findings* (two analyzers each state half of one boundary). Dispatch: `SingleVariableDesignationSyntax` reached from `DeclarationExpressionSyntax` (deconstruction targets, `out`), `VarPatternSyntax`, `DeclarationPatternSyntax`, `RecursivePatternSyntax` (positional and property), `ListPatternSyntax`, plus `VariableDeclaratorSyntax`, `ForEachStatementSyntax.Identifier`, `CatchDeclarationSyntax.Identifier`. Every candidate is listed under *Breaking inputs*.

### Breaking inputs

Classes: FC-002 ("is itself a subpattern", "element of a tuple expression" must become explicit conditions), FC-003 (each boundary tested on both sides, in both rule test classes), FC-015/FC-016 (one boundary, two analyzers), FC-023 (`_`, `__`, `_count`), FC-027 (nesting: nested designations, nested tuples, nested positional patterns, positional inside property and list patterns, switch statement / switch expression / `is`).

**Spec defect to amend before tests.** RH4115-002's first sentence excludes "a variable declared by a subpattern of a positional pattern", and its second sentence keeps reporting "a variable of a property pattern", "the variable that names a whole property or positional pattern", and "a declaration pattern or `var` pattern that is not a subpattern of a positional pattern". For these inputs the two sentences contradict each other, and RH4117-003 (which says RH4115 reports them) only resolves it if the reader looks there:

| Input | RH4115-002 sentence 1 | RH4115-002 sentence 2 | RH4117-003 |
|---|---|---|---|
| `p is (int Number and > 0, _)` | not reported | reported | RH4115 |
| `p is ({ Length: var Size }, _)` | not reported | reported | RH4115 |
| `p is (string { Length: > 0 } Text, _)` | not reported | reported | RH4115 |

An implementer that reads sentence 1 as "has a positional subpattern ancestor" leaves all three reported by no rule. RH4115-002 should not restate the boundary; it should name the variables RH4117-001 and RH4117-003 name and nothing else (see *Internal choices* 1).

Candidates (existing test that pins it, or "none found"):

RH4115 no longer reports / RH4117 reports:
- `(var A, var B) = p` — none found (RH4117); none (RH4115).
- `(int A, var (B, C)) = (1, pair)` — none found.
- `((var A, var B), var C) = …` nested tuple — none found.
- `(existing, var B) = p` — `existing` reported by neither — none found.
- `foreach ((var K, var V) in pairs)` — none found.
- `p is var (A, B)`, `case var (L, R):`, switch-expression arm `var (A, B) => …`, nested `is var (A, (B, C))` — none found.
- Under output question 1 (A): `p is (var A, int B)`, typed `p is Point(var X, var Y)`, nested `(int A, (var B, var c))`, positional inside a property pattern `{ P: (var X, _) }`, positional inside a list pattern `[(var A, _), ..]` — none found.
- Existing: `VariablesOfVarDeconstructionsAreNotChecked` and `VerifyNoDiagnosticsForDeconstructionVariables` (RH4115) and `VariablesOfVarDeconstructionsAreCheckedInAssignmentsAndForeachStatements` (RH4117) pin `var (…)` only and stay valid.

RH4115 keeps reporting (RH4117 must not):
- `pair is (1, 2) Both`, `is string { Length: var Size }`, `is int Number`, `is var Value` — pinned by `LocalVariablesOfEveryDeclarationFormAreChecked` and `DesignationsInSwitchesInitializersAndSlicePatternsAreChecked` (RH4115 side only); RH4117 side none found.
- The three seam inputs of the table above, plus `p is ((var N), _)` and `p is ([_, _] Items, _)` — none found; their rule depends on output question 1b.
- `out var Parsed` — RH4115 pinned; RH4117 side none found.

`_` (RH4115-004): `int _ = 42;` — `NoCodeFixForLetterlessLocalVariable` asserts the opposite and must be changed, not duplicated. `foreach (var _ …)`, `catch (Exception _)`, `using var _ = …`, `for (int _ = 0; …)`, `using (var _ = …)`, `fixed (int* _ = …)` — none found; the exemption is by name, so a representative set suffices, but `for` and `foreach` differ in node kind (declarator vs `ForEachStatement.Identifier`) and both should be in it. `__` and `_count` stay reported — `_count` in the spec example; `__` none found in RH4115.

Local constants (RH4115-005): `const int MaxValue = 3, Max_Count = 4;` not reported by RH4115 — none found.

Fix on the new RH4117 positions: the RH4117 fix is `CasingCodeFixProviderBase<SingleVariableDesignationSyntax>`, so pattern designations are the same node type; the RH4117-002 second example (`Left`, `Right` with references in the `if` body; `Head` in a conditional) — none found.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4115 analyzer | Must change (`_`, `const`, deconstruction positions) |
| RH4117 analyzer | Must change (tuple declaration expressions, `var (…)` patterns, positional subpatterns per output question 1) |
| RH4115 / RH4117 code fixes | Must stay (node types already cover the new positions); the anonymous-member guard is under *Whole run* |
| Fix All | Must stay (shared base) |
| `Reihitsu.Core` | Not involved |
| Formatter | Not involved: no formatter phase renames; FC-013/FC-014 and P11 do not apply |
| Rule pages RH4115, RH4117 | Changed by the spec author; RH4117's `is (var a, int b)` depends on output question 1 |
| Tests | Must change: `NoCodeFixForLetterlessLocalVariable` |

### Refuse option

The run changes which rule reports, not a rewrite, so there is no rewrite to withhold. The refuse option is to keep the split as it is today (reading B of Q3/Q4), which the maintainer rejected. For positional subpatterns, the narrower option is reading B of output question 1 (they stay with RH4115). Declaring a shape unsupported is not an option: it is a formatter precheck refusal and decides nothing an analyzer reports. _None_ other.

### Output questions

**1 (spec author's, endorsed) · RH4117 · Deconstruction variable names should be in camelCase — the subpatterns of a positional pattern.** I agree with recommendation A. A positional pattern deconstructs the value exactly as `(var a, var b) = p` does (both call `Deconstruct` or read the tuple's elements). B keeps a spelling-dependent split, which is what the maintainer's decision removed.

**1b (new) · RH4117 / RH4115 — a variable inside a positional subpattern that is not a bare `var` or declaration pattern.** Option A of question 1 as specced sends only a `var` or declaration pattern that *is* the subpattern to RH4117. A variable that receives the same element through `and`, parentheses, or the designation of a nested recursive or list pattern stays with RH4115, although RH4117-003's own Why says that a positional pattern's variables are RH4117's because "each variable they declare receives one element of it".

Input:
```csharp
class C
{
    int M((object, object) pair)
    {
        if (pair is (int Count, _))
        {
            return Count;
        }

        if (pair is (int Number and > 0, _))
        {
            return Number;
        }

        if (pair is (string { Length: > 0 } Text, _))
        {
            return Text.Length;
        }

        if (pair is ({ } Any, string { Length: var Size }))
        {
            return Size;
        }

        return 0;
    }
}
```

Before (`main`): RH4115 is reported on `Count`, `Number`, `Text`, `Any`, and `Size`; RH4117 on none (`scripts/apply-fix.sh RH4115` on this input: `fixed`, 5 iterations, renaming all five to `count`, `number`, `text`, `any`, `size`; `scripts/apply-fix.sh RH4117`: `no-diagnostic`).

After, A (as specced): RH4117 on `Count`; RH4115 on `Number`, `Text`, `Any`, and `Size`.

After, B (recommended): RH4117 on `Count`, `Number`, `Text`, and `Any`; RH4115 on `Size`.

The fix result is the same under A and B (`count`, `number`, `text`, `any`, `size`).

Why B: every variable that receives one element of the deconstruction goes to the deconstruction rule however the subpattern is spelled. Only a variable that receives a property of an element (`Size`) stays with RH4115. That is the cause the spec's Why already states. Under A, the rule that reports depends again on spelling. The inputs are rare, and B is still a cheap syntactic check: walk up from the designation through `and` and parenthesized patterns to a subpattern of a positional pattern. If the maintainer picks B of question 1, this question disappears.

**2 · RH4128 rule ID** — listed under #1088.

### Internal choices

1. **RH4115-002 wording.** (a) Replace the restated boundary with a reference: "MUST NOT report a variable that RH4117-001 or RH4117-003 names; every other variable a pattern declares is reported as RH4115-001 states", keeping the examples. (b) Keep the restatement and make sentence 1 exact. Recommend (a): one statement of the boundary in the specs, matching the single owner recommended under *Architecture findings*. Nothing visible changes.
2. **Why of RH4117-003** has to match the answer to 1b. Under A it must say why `int Number and > 0` is not "an element" variable. Under B it already fits.

### Issue goals

`agent-filed`. "Each input of #1019 is reported by the decided rule or not at all, and each has a regression test" — keep it; it serves the purpose. Nothing to drop. The positional-subpattern cases (questions 1 and 1b) go beyond the #1019 inputs and are asked, not assumed.

### Architecture findings

**The deconstruction-variable boundary has two owners.** RH4117 decides which designations are deconstruction variables (today: `DeclarationExpression` with `ParenthesizedVariableDesignation`), and RH4115 decides separately which designations to skip (today: `Parent: not ParenthesizedVariableDesignation`). The reported Q4 gap (`p is var (A, B)` reported by no rule) is exactly these two statements drifting apart: RH4115's skip covers the `var` pattern's designation, RH4117's inclusion does not. The change widens the boundary to three more positions. If both analyzers keep their own statement, every position is another chance for a gap or a double report (FC-015). This is the structural cause of a defect of the run's own issue, so it is the run's architecture finding: one predicate ("is a deconstruction variable") that RH4117 includes and RH4115 excludes. The RH4115/RH4128 `const` boundary is the same shape (RH4115 excludes `const`, RH4128 includes it) but is a single modifier test with little drift risk. Size: a few lines and possibly one helper. No visible output depends on how it is built.

### Follow-up candidates

_None for this issue._

## thoenissen/Reihitsu#1088

### Premise

Real as a decided new rule: local constants are reported today by RH4115 (`ConstLocal.cs` above), and no rule of their own exists. RH4128 appears nowhere on `main` (`git log -S RH4128 main` is empty; `rg RH4128` only finds this run's files). The rule's fix is the RH4115 rename path the spec author ran (`scripts/apply-fix.sh RH4115`), so "comprehensive" is shown for the rename itself, except for the anonymous-member defect under *Whole run*, which this new fix inherits:

```csharp
class C
{
    string M()
    {
        const int MaxValue = 3;

        return new { MaxValue }.ToString();
    }
}
```

`scripts/apply-fix.sh RH4115` → `fixed`: `const int maxValue = 3;` and `return new { maxValue }.ToString();`. `ToString()` now returns `{ maxValue = 3 }` instead of `{ MaxValue = 3 }`. As specced ("the rename that RH4115 already applied"), RH4128 would ship a new fix with this SHARED-FIX-003 defect.

### Mechanism and candidates

_N/A_ (new rule). Dispatch: `LocalDeclarationStatementSyntax` with the `const` modifier, wherever it stands: method, constructor, operator, conversion operator, finalizer, accessor (`get`/`set`/`init`/`add`/`remove`), local function, lambda, anonymous method, top-level statements, switch section.

### Breaking inputs

Classes: FC-023 (`_`, `_limit`, `__`), FC-027 (every container), FC-008 (Fix All over several declarators of one statement), FC-036 (anonymous member, `nameof`).

- RH4128-001 example, including several declarators in one statement — none found.
- A local constant in a constructor and in an anonymous method — none found. RH4128-001's list of containers ("a method, a local function, a lambda, an accessor, or the top-level statements") omits constructors, operators, finalizers, and anonymous methods. Read as exhaustive, it leaves `C() { const int InCtor = 1; }` unreported. Compiled with the probe: a constructor with `const int InCtor = 1;` compiles. See *Internal choices*.
- The RH4128-003 Why claims an attribute argument of a local function can reference a local constant. Compiled with the probe: `const string Message = "x"; [Obsolete(Message)] void Local() { }` inside a method → `ok`. A fix test for that reference — none found.
- `const int _ = 1;` not reported; `const int _limit = 2;` reported (RH4128-002) — none found.
- Collision `const int MaxValue = 1; int maxValue = 2;` → fix withheld (SHARED-CASING-009) — none found for RH4128 (RH4115 has `CollisionIsRefusedForSupportedLocalDeclarationForms`, without `const`).
- Keyword-derived name, e.g. `const int Class = 1;` → `class` (SHARED-CASING-013) — none found for RH4128.
- `new { MaxValue }` (above) — depends on the *Whole run* decision.
- Title, page heading, and README row: pinned by `AnalyzerPackageMetadataTests`; resources by `AnalyzerResourcesTests`/`CodeFixResourcesTests` (coverage first: no new test).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| New analyzer + code fix (Fix All) | Must change (new) |
| `CasingCodeFixProviderBase{T}` | Must stay for RH4128 (already handles `VariableDeclaratorSyntax`); see *Whole run* |
| Resources (title, message, code-fix title), `Release.ruleset`, `Reihitsu.sln` items | Must change (implementer; spec author listed them) |
| Rule page, package README, specs index | Changed by the spec author |
| RH4115 | Must change (RH4115-005) |
| Formatter, `Reihitsu.Core` | Not involved |

### Refuse option

Ship RH4128 without a code fix (the maintainer allows "fix only if comprehensive"). That costs the user a manual rename for every local constant that RH4115's fix renames today, which is a regression in what is fixable. The cheaper option keeps the fix and withholds it only for the anonymous-member class (*Whole run*). Declaring a shape unsupported does not apply (no formatter involvement).

### Output questions

**2 · RH4128 · Local constant names should be in camelCase — the rule ID.** The maintainer asked for "the next free number in RH41xx". RH4119 is unused in the current code but shipped in `v1.0.0-beta5`/`beta6` and was retired. RH4128 has never been used. Both readings of "free" give a different visible ID. Recommendation: RH4128, because a user's old suppression of RH4119 would otherwise silence the new rule. The lead recorded this as decision 1. If the lead treats the maintainer's answer as covering it, no question is needed. Otherwise it is a one-line item in the batch.

### Internal choices

1. **RH4128-001 container list.** (a) Drop the list: "a variable of a local declaration statement with the `const` modifier, wherever the statement stands". (b) Complete the list. Recommend (a): the list adds nothing the syntax does not decide, and an incomplete list reads as an exclusion. Nothing visible changes if the implementation checks the statement kind alone.

### Issue goals

`agent-filed`. The PascalCase starting point is replaced by the maintainer's camelCase answer (binding). "Release coordinated with #1087" is met by the single run. "Fix only if comprehensive": keep it, with the anonymous-member guard (*Whole run*) as the condition that makes it true. Nothing to drop.

### Architecture findings

See *Whole run* (the anonymous-member guard; RH4128's new fix is one of its members).

### Follow-up candidates

_None for this issue._

## thoenissen/Reihitsu#1978

### Premise

Real. `RH4105MethodParameterCasingAnalyzer` registers only `SyntaxKind.Parameter`. RH4124 already dispatches range variables through `FromClause`, `LetClause`, `JoinClause`, `JoinIntoClause`, and `QueryContinuation`, and Roslyn's query syntax declares range variables in no other node (`WhereClause`, `OrderByClause`, `SelectClause`, and `GroupClause` declare none), so RH4105-005's clause list is complete. Caller: the analyzer on user code; the RH4105 fix in the IDE.

Severity: a real but minor gap. A single-letter range variable such as `from X in xs` will be reported by RH4105 and RH4124, as `X => X` is today. That is consistent, not a double-report defect.

### Mechanism and candidates

_N/A_ (behavior change). Dispatch candidates as above. The fix's node type changes: `RH4105MethodParameterCasingCodeFixProvider` is `CasingCodeFixProviderBase<ParameterSyntax>`, and the base's `GetDeclaredSymbol` switch has no case for a query clause, a `JoinIntoClause`, or a `QueryContinuation`, so both must change.

### Breaking inputs

Classes: FC-018 (references across a continuation), FC-036 (anonymous members), FC-023 (`_`, verbatim `@Value`), FC-027 (nested query, query inside a lambda, query in a field initializer), FC-008 (Fix All over every range variable of one query), FC-002 ("anywhere in the query expression that declares it").

The spec author's evidence that the fix is comprehensive is the bare `Renamer`, not the fix base. The base adds guards that have never run on an `IRangeVariableSymbol`: cascade, reference count, excluded code, rewritten-code check, and compile-error check. A guard that refuses every range variable would silently turn RH4105-006 into "no fix". The positive fix examples are the test that catches it.

Renamer results I ran (probe; bare renamer, no guards), each a must-cover input:

```csharp
return (from Group in values select Group).ToArray();
```
→ `from @group in values select @group` (no compile error). SHARED-CASING-013 must withhold the fix. Existing `NoFixIsOfferedWhenAReferenceInsideAQueryExpressionWouldNeedTheVerbatimPrefix` covers a *local* referenced in a query (RH4115), not a range variable — none found.

```csharp
int value = 1;
return (from Value in values select Value + value).ToArray();
```
→ `from value in values select value + value` with `error CS1931: The range variable 'value' conflicts with a previous declaration of 'value'`. SHARED-CASING-009 must withhold the fix — none found.

```csharp
return (from Value in values
        select (from other in values select new { Value }).First()).First();
```
→ `new { value }` in the nested query. RH4105-006 must withhold here too ("anywhere in the query expression"), not only in the declaring query's own `select` — none found.

Anonymous-member shapes. The compiler infers a member name only from a simple name or a member access. Compiled with the probe: `select new { Value! }` and `select new { (Value) }` are both `error CS0746`. So for a range variable, "the whole expression is the reference" is the complete set, and the specced condition matches the compiler's rule for this symbol kind. Members held (fix offered): `new { Item = Value }`, `new { Total = Value + 1 }`, `new { Value.Length }` (member name `Length`) — in the spec; none found in tests.

Other candidates, none found:
- Each clause: first `from`, later `from`, typed `from int X`, `let`, `join`, typed `join int X`, `join … into X`, continuation `into` after `select` and after `group`. A later use is not reported.
- `from _ in values` is not reported (RH4105-002 example).
- A camelCase query is not reported.
- `from @Value in values` is reported (the value text decides, SHARED-DIAG-002).
- A query nested in another query, inside a lambda, and in a field initializer.
- Fix across a continuation (RH4105-006 first example; the probe renamed it correctly).
- Fix All over all five range variables of the RH4105-006 example (sequential equality, SHARED-FIX-002).
- Fix All with `from Value in a from VALUE in b` (same new name, same method) → both skipped (SHARED-CASING-010).
- An `#if`-excluded line inside a multi-line query that names the range variable (SHARED-CASING-007).
- Title and message: pinned by `AnalyzerPackageMetadataTests` (coverage first).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH4105 analyzer | Must change (range variables, `_`) |
| RH4105 code fix + Fix All | Must change (node type; declared symbol of a clause) |
| `CasingCodeFixProviderBase{T}` | Must change (`GetDeclaredSymbol` for clauses), plus the anonymous-member guard under *Whole run* |
| RH4124 | Must stay |
| Resources `RH4105Title`/`RH4105MessageFormat` | Changed by the spec author; the code-fix title "Fix casing" stays |
| `RH4105MethodParameterCasingAnalyzer` doc comment | Must change (quotes the old title; FC-031) |
| Rule page RH4105, README row | Changed by the spec author |
| Formatter, `Reihitsu.Core` | Not involved |

### Refuse option

No fix for any range variable: the diagnostic only, which the maintainer's answer allows. It costs the user a manual rename of every range variable, including the many queries without anonymous projections. The specced option withholds the fix only where a reference names an anonymous member. That is a cheap syntactic check, but the spec itself calls the shape common ("Projecting range variables into an anonymous object is a common shape of query syntax"). Under `AGENTS.md`, *Code-fix coverage* (last bullet), and escalation 9, withholding a fix for commonly written code is a maintainer question, not a local refusal. That makes output question 3. Declaring a shape unsupported does not apply: the shape is semantic, it holds no comment, directive, or disabled text, and the precheck's refusal would not reach a rename fix, which does not call the formatter.

### Output questions

**3 · RH4105 · Parameter and query range variable names should be in camelCase — a range variable projected into an anonymous object**

Input:
```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        return (from Value in values
                let Doubled = Value * 2
                select new { Value, Doubled }).First();
    }
}
```

Before (`main`): not reported.

After, A (recommended): RH4105 is reported on `Value` and `Doubled`, and no fix is offered for either.

After, B: RH4105 is reported on `Value` and `Doubled`, and the fix keeps the member names:
```csharp
using System.Linq;

class C
{
    object M(int[] values)
    {
        return (from value in values
                let doubled = value * 2
                select new { Value = value, Doubled = doubled }).First();
    }
}
```

After, C: RH4105 is reported on `Value` and `Doubled`, and no fix is offered for any range variable, in any query.

Why A: the rename the fix uses cannot keep the member name, and A withholds only the queries that project a reported range variable by its bare name, at the cost of one cheap syntactic check. B adds a second rewrite to the shared rename. To stay consistent, every casing fix would need it (the same shape exists for parameters, locals, and fields; see *Whole run*), which is more machinery than one rule's gap justifies. C gives up the fix for every query to avoid one shape.

### Internal choices

1. **Where the anonymous-member guard lives** — see *Whole run*. Recommend the shared base, with RH4105-006 referencing it instead of stating its own condition.

### Issue goals

`agent-filed`. Option A and the widened title are the maintainer's answer (binding). The issue's "Whether the rename fix applies … to be decided" is settled by output question 3. Nothing to drop.

### Architecture findings

See *Whole run*.

### Follow-up candidates

_None for this issue._

## Whole run

### The anonymous-member guard (architecture finding)

**Components:** `CasingCodeFixProviderBase{T}`, which owns rename safety for every casing fix (SHARED-CASING-007/-009, SHARED-FIX-003), and RH4105-006's range-variable-only guard, specced in the RH4105 provider's scope. **Shared decision:** whether a rename changes the name of an anonymous object member that the compiler infers from a renamed reference. **Cause:** a reference of the rename cascade whose identifier is the inferred name of an anonymous object member declarator without `Name =`. That is a simple name, or the name of a member access such as `this.Value`.

The class spans every casing fix. Run on `main`'s code with `scripts/apply-fix.sh`, each member not read by name afterwards:

| Member | Rule | Input | Result |
|---|---|---|---|
| lambda parameter | RH4105 | `values.Select(Value => new { Value })` | fixed → `value => new { value }` (fails) |
| local | RH4115 | `var Total = …; var o = new { Total }; return o.ToString();` | fixed → `new { total }` (fails) |
| local constant (RH4128's new fix) | RH4115 today | `const int MaxValue = 3; return new { MaxValue }.ToString();` | fixed → `new { maxValue }` (fails) |
| deconstruction variable | RH4117 | `var (First, second) = pair; return new { First, second }.ToString();` | fixed → `new { first, second }` (fails) |
| private field, member access | RH4106 | `private int Value = 1; … new { this.Value }.ToString()` | fixed → `new { this._value }` (fails) |
| range variable | RH4105 (new) | `select new { Value }` | bare renamer → `new { value }` (fails without the guard) |
| member read by name | RH4115 | `var o = new { Total }; return o.Total;` | `no-fix-offered` (holds: compile-error guard; RH4106 pins it in `NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField`) |
| tuple element inferred from a parameter | RH4105 | `values.Select(Value => (Value, 1)).First().Value` | `no-fix-offered` (holds) |
| `Value!`, `(Value)` | — | — | not an inference (`CS0746`), not a member |

Not run, same base and renamer, so expected to fail alike: other field and property rules (RH4107 to RH4114), enum members (`new { E.Member }`, RH4101), and a conditional access `x?.Value`.

Why it is the run's finding and not only a follow-up: the run ships three new fixes or fix positions that carry it — RH4128's fix, RH4117's fix on the newly covered pattern and tuple positions, and RH4105's fix on range variables. With the guard specced only in RH4105-006, one rule treats one cause two ways: `from Value in values select new { Value }` withholds the fix, while `values.Select(Value => new { Value })` renames the member. That is refusal by the cell, not by the cause (FC-016, `README.md`, *Refuse by the cause*), and the later shared fix would leave RH4105-006 as a second statement (FC-015). Withholding a fix where SHARED-FIX-003 already forbids it is a repair against an unchanged requirement and puts nothing in question for the existing rules. For range variables it is output question 3.

Options for the lead:
- (a) State the condition once in `shared/casing.md` (a SHARED-CASING requirement, or a bullet of SHARED-CASING-009), implement it in the base, and let RH4105-006 reference it.
- (b) Keep the RH4105-006-only guard as specced and file the shared defect as a follow-up. Then RH4128 and RH4117 ship new offers with a known SHARED-FIX-003 defect.

Recommend (a). The check is the same cheap syntactic check, placed where the run already edits (`GetDeclaredSymbol`), so it adds neither a new type nor about 100 lines.

### Follow-up candidates

- If the lead chooses (b) above: the shared anonymous-member defect for RH4105 parameters, RH4106 to RH4115, RH4117, and RH4128, with the matrix above. It is the follow-up candidate already in `00-lead.md`, widened to fields, properties, and enum members.

### Evidence index

- Premise fixtures (#1087): `Deconstruct.cs`, `ConstLocal.cs` under *Premise* of #1087.
- Anonymous-member matrix: the fixtures in the table above (each a complete class with `using System.Linq;` where needed).
- Renamer probes (#1978): `kw`, `coll`, `nested` under *Breaking inputs* of #1978; CS0746 checks for `new { Value! }` and `new { (Value) }`.
- Compile checks (#1088): `[Obsolete(Message)]` on a local function referencing a local constant, and a `const` in a constructor — both `ok`.
