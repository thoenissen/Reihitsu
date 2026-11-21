Role: skeptic
Commit: ff78b28
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Premise

Real as a feature request. `rg -il complexity src` finds no complexity analyzer; RH2009 is free (`rg RH2009 src` finds nothing; `src/Reihitsu.Analyzer/Rules/Design/` holds RH2001–RH2008 and RH2101–RH2111). Every caller of an analyzer reaches the rule: the compiler runs it on every member of every build. There is no reported defect to reproduce; the premise questions are the metric's definition and the issue's goals, both below.

The maintainer's clarification 5 says "never reaches nesting level 2 — i.e. has no nesting". In the spec's numbering (RH2009-003: level 0 at the top of the body), a structure inside one other structure is at level 1, so "level 2" read literally in that numbering would mean three deep. The gloss "i.e. has no nesting" and the example (only flat `if` statements) both fit the spec's reading (one structure inside another is enough), and RH2009-003's first example and RH2009-004's `else { if … }` example show it. No question needed; the concept the lead presents should say "one structure inside another is enough" in those words, so the maintainer confirms it on sight.

Hand-computed scores, re-derived independently from RH2009-003/004/005 — all correct:

| Example | Stated | Re-derived |
|---|---|---|
| RH2009-001 `Score` (= change doc 1, rule page violation) | 16 | foreach 1, if 2, if 3, for 4, if 5, else if 1 = 16 |
| RH2009-002 `get`, `Sign`, `+`, `Flat`; partial `M` | 3, 3, 3, 2; 3 | as stated (`Sign`'s inner `?:` is in the true branch, nesting 1) |
| RH2009-003 `M`/`N` | 6 / 3 | 1+2+3 / 1+1+1 |
| RH2009-003 second example | 11 | foreach 1; if 2 + `&&` 1; else if 1 + `or` 1; else 1; `?:` in the else block nesting 2 → 3; catch 1 |
| RH2009-003 sequences A, B, D, E, F, G | 4, 5, 5, 5, 4, 5 | as stated (E: `!` starts a new tree, two `&&` trees) |
| RH2009-003 switch | 4 | switch 1, switch expression in a section 2, `goto case` 1 |
| RH2009-003 lock/using/try | 3 | 1 + 2 |
| RH2009-004 flat `M` | 9, no nesting; 10 when the `?:` moves into the `if` | as stated |
| RH2009-004 `else { if }`; try/catch `M`/`N` | 4; 1 / 3 | as stated |
| RH2009-005 lambda `M`; `M`/`Local` | 2; 6 / 3 | as stated |
| RH2009-006 indexer, conversion (FEATURE / not), finalizer | 3, 3 / 1, 3 | as stated |
| Rule page correction: `Score`, `ScoreValue`, `AdjustForIndex` | 1, 3, 2 | as stated |

The scores carry no FC-035 risk yet (no implementation exists), but the test implementer must take them from the spec, not from the analyzer's output.

## Mechanism and candidates

_N/A_ — new rule, no bug. The dispatch the analyzer will need is the member set of RH2008 (`RH2008MethodsShouldNotExceedTheLineLimitAnalyzer`, lines 47–59 register `MethodDeclaration`, `ConstructorDeclaration`, `DestructorDeclaration`, `OperatorDeclaration`, `ConversionOperatorDeclaration`, `LocalFunctionStatement`, `Get/Set/Init/Add/RemoveAccessorDeclaration`, `PropertyDeclaration`, `IndexerDeclaration`; `TryGetCountedSpan` maps each to body, expression body, name token). Candidates the spec decides and the tests must cover, by defining syntax type:

- Members: every kind above, block and expression body (FC-025: the seven expression-bodied kinds), C# 14 compound-assignment operators (`OperatorDeclarationSyntax`, location on the operator token), members inside an `extension(...)` block, members of an interface with a default implementation, static constructor, partial method and partial property implementing part (FC-024, SHARED-DIAG-012).
- Local functions in unusual hosts: a local function in top-level statements (judged on its own — RH2009-002 judges every local function; the non-goal covers only the top-level statements), a local function inside a lambda in a field initializer (judged on its own; the lambda is not), a local function inside a local function (counts toward both holders and is judged on its own).
- Structures (RH2009-003 item 1): `IfStatementSyntax` (not under `ElseClauseSyntax`), `ConditionalExpressionSyntax`, `SwitchStatementSyntax`, `SwitchExpressionSyntax`, `ForStatementSyntax`, `ForEachStatementSyntax` and `ForEachVariableStatementSyntax` (`foreach (var (a, b) in …)` — a separate syntax kind, easy to miss), `WhileStatementSyntax`, `DoStatementSyntax`, `CatchClauseSyntax`; `ElseClauseSyntax`; `BinaryExpressionSyntax` (`LogicalAnd`/`LogicalOr`), `BinaryPatternSyntax` (`and`/`or`), `GotoStatementSyntax` (all three kinds).
- Nesting constructs: the above plus `ParenthesizedLambdaExpressionSyntax`, `SimpleLambdaExpressionSyntax`, `AnonymousMethodExpressionSyntax`, `LocalFunctionStatementSyntax`.
- Non-nesting by the spec's list (must score nothing and not nest): query expressions (`from … where … select`, `QueryExpressionSyntax`) — a `?:` in a `where` clause stays at the level where the query is written, while the same `?:` in `.Where(x => …)` nests. Decidable by the list, but see *Internal choices*.

## Breaking inputs

Classes that reach the change: FC-002 (the qualifiers "direct", "the statement of an `else` clause", "joined directly or through parentheses" must each become a condition), FC-003 (the limit and the exception each need both sides: equal-to-limit with nesting; above-limit without nesting), FC-020 / SHARED-DIAG-008 (`#if` inside a body — RH2009-006 has one; add one that removes the only nesting), FC-024, FC-025, FC-027 (one nesting level only; the multi-line form; a lambda inside a lambda), FC-039 (the walk must descend into a lambda inside a lambda, a local function inside a lambda, a switch expression inside a `when` clause, a `?:` inside a `foreach` expression), FC-015 (see *Architecture findings*).

Inputs the test implementer must cover beyond the spec examples, each decided by the spec as written:

- `else` followed by a block that holds only `if … else` (nests) next to `else if` (does not).
- A nested `if` without braces: `if (a) if (b) x();` (nests).
- A structure in each non-nested position: an `if` condition, an `else if` condition, a loop condition, a `for` incrementor, a `foreach` expression, a `do … while` condition, a switch's governing expression, a `catch` filter (`when`), a `?:` condition — each stays at the construct's own level.
- A lambda inside a nested part (`if (a) { list.ForEach(x => { if (x > 0) … }); }` → inner `if` 3).
- `ForEachVariableStatementSyntax` and `await foreach`.
- Logical trees broken by each kind of intervening expression: invocation argument, `!`, `?:`, `==`, `??`, `is` (`a && x is 1 or 2` → 2 sequences), and `not` around a parenthesized pattern (`x is not (> 0 and < 10) and not 20` → 2 sequences, since `not` is neither "directly" nor "through parenthesized patterns").
- Recursion through `this.M(…)` and through a local function calling itself (both 0).
- The boundary pair per FC-003: a nested body at exactly the limit (not reported) and one above; a flat body far above the limit (not reported).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (`Reihitsu.Analyzer`, new RH2009 analyzer) | Must change (new) |
| Configuration loading and RH0001 (`ConfigurationManager`, design-section validation) | Must change: accept `maximumMethodComplexity` (RH0001-007) |
| RH2008, RH2111, `LineCountAnalyzerBase` | Must stay (behavior); see *Architecture findings* for sharing the member selection |
| Code fix, Fix All | Not involved (no fix) |
| Formatter, CLI | Not involved. No formatter phase rewrites a control structure, a logical operator, or a pattern combinator, so formatter output never changes a score; no SHARED-DIAG-010 / FC-013 / FC-014 conflict. `rg` for code-fix code that builds `IfStatement`, `ConditionalExpression`, switch, loop, lambda, or logical/binary-pattern nodes finds none, so no existing fix raises RH2009 by its rewrite. |
| `Reihitsu.Core` | Not involved, unless the shared member selection moves there |
| Rule page `documentation/rules/RH2009.md` | Must change (new); two wording points below |
| Resources (title, message) | Must change |
| `Release.ruleset` and `Reihitsu.SelfHosting.Test` | Must change — see below |

Self-hosting: `Reihitsu.SelfHosting.Test/Repository/RulesetTests.ReleaseRulesetListsEveryShippedRule` requires an entry (or an exemption) for every shipped rule, and `AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` runs every analyzer of the source assembly over the repository with the root `reihitsu.json` (which today sets only `maximumTypeLines`). CI runs that scan on every pull request, so any RH2009 violation in this repository fails CI on this PR — the build itself is unaffected, because it loads the analyzers from `binaries/`. Maintainer clarification 3 ("ignored for now") therefore needs a mechanism in this run; the lead decides which (an entry in the root `reihitsu.json`, a ruleset action plus a scan exemption, …). An `Action="Error"` entry would also make the next `/bump-version` build fail on the violations.

## Refuse option

A new rule has no input class to refuse the way a fix does. The cheaper alternatives to what the spec asks:

- No rule at all — the issue's purpose is unmet; the maintainer has asked for the rule. Not recommended.
- Drop the local-function double count (Q1 B of the spec author) — one fewer mechanism (no re-walk of local functions inside the holder), but changes visible output; it is already an output question.
- The sequence rule for logical operators and pattern combinators is the most intricate part (trees, breaks by `!` and other expressions, parentheses, patterns). Scoring nothing for pattern combinators (spec author's Q3 B) or for those in case labels and arms (Q-B below) shrinks it; visible, so asked.

Declaring a shape unsupported: not an option — the rule is an analyzer with no formatting path and no code fix.

## Output questions

Additional to the spec author's five (which I checked and leave as they are, with one changed recommendation and more evidence for Q2). All within thoenissen/Reihitsu#352. Examples A, B, D, E compile with the .NET 11 SDK (`dotnet build` of a scratch library project: `Build succeeded`, 0 warnings); the scores are derived by hand from RH2009-003.

**A · RH2009 · Methods should not exceed the complexity limit — chained conditional expression**

Input, without a `reihitsu.json`:
```csharp
class C
{
    string Name(int code) => code == 1 ? "one" : code == 2 ? "two" : code == 3 ? "three" : code == 4 ? "four" : code == 5 ? "five" : code == 6 ? "six" : "other";
}
```

Before (`main`): not reported.

After, A (the spec as written) — each `?:` in another's false branch nests one level deeper: 1 + 2 + 3 + 4 + 5 + 6 = 21:
```csharp
string {|RH2009:Name|}(int code) => …   // "This member has a cognitive complexity of 21, which exceeds the limit of 15."
```

After, B (recommended) — a `?:` that is the false branch of another `?:` continues the chain like `else if`: 1 each, no nesting increment, 6 in all:
```csharp
string Name(int code) => …   // not reported
```

Why B: a chained conditional is the expression form of an `if … else if` chain, which the spec scores flat and the no-nesting exception never reports; under A the same dispatch is reported at the default limit only because it is written as an expression. A `?:` in the true branch, or in a branch inside parentheses after another operator, still nests under both options. What would change the pick: if the maintainer wants the published definition, which nests every conditional, followed exactly.

**B · RH2009 · Methods should not exceed the complexity limit — `or` in switch arms and case labels**

Input, with `{ "design": { "maximumMethodComplexity": 3 } }`:
```csharp
class C
{
    int M(char c, bool upper)
    {
        if (upper)
        {
            return c switch
            {
                'A' or 'E' or 'I' => 1,
                'Y' or 'W' => 2,
                _ => 0,
            };
        }

        return 0;
    }
}
```

Before (`main`): not reported.

The spec gives two readings: RH2009-003 item 3 counts every `and`/`or` sequence, while its "Nothing else" sentence says "the case labels, `when` clauses … score nothing".

After, A — combinators count in arms and case labels too: `if` 1, switch expression 2, one `or` sequence per arm 2 = 5:
```csharp
int {|RH2009:M|}(char c, bool upper)   // "This member has a cognitive complexity of 5, which exceeds the limit of 3."
```

After, B (recommended) — combinators in a switch arm's or case label's pattern score nothing; elsewhere (`is` patterns, `&&`/`||` in a `when` clause) they count: 3:
```csharp
int M(char c, bool upper)   // not reported
```

Why B: the spec scores a switch once "however many cases it has" because it is read as one decision; under A a switch's score grows again with every arm that lists alternatives.

**C · RH2009 · Methods should not exceed the complexity limit — nesting only through a lambda (spec author's Q2, more evidence)**

The spec author's input shows an `if` in a block lambda. The common shape is a `?:` in a LINQ lambda inside an otherwise flat method. Input, with `{ "design": { "maximumMethodComplexity": 5 } }`:
```csharp
using System.Linq;

class C
{
    int[] M(int[] values, int mode)
    {
        if (mode == 1) { return values; }
        if (mode == 2) { return new int[0]; }
        if (mode == 3) { return values.Reverse().ToArray(); }
        if (mode == 4) { return values.Skip(1).ToArray(); }
        if (mode == 5) { return values.Take(1).ToArray(); }

        return values.Select(value => value > 0 ? value : 0).ToArray();
    }
}
```

Before (`main`): not reported.

After, A (spec author's recommendation) — the `?:` in the lambda has a nesting increment, so the exception no longer holds: 5 + 2 = 7:
```csharp
int[] {|RH2009:M|}(int[] values, int mode)   // "This member has a cognitive complexity of 7, which exceeds the limit of 5."
```

After, B (my recommendation) — a lambda or local function nests for the score, but only a structure inside another structure's nested part counts for the exception:
```csharp
int[] M(int[] values, int mode)   // not reported
```

Why B: the exception exists because flat decisions never make the reader hold one condition while following another (RH2009-004, *Why*); a lambda is not a condition, so a one-line projection should not turn a flat method into a reported one. What would change the pick: if "nesting" in the maintainer's clarification 5 means indentation depth, which a lambda body has. Under B, RH2009-005's first example (`M` reported with 2 at limit 1) and change doc item 4 become "not reported".

**D · RH2009 · Methods should not exceed the complexity limit — lambda in a field initializer (dropped issue goal)**

The issue asks for lambdas to be judged as methods of their own; RH2009-002 and the non-goals drop that, recorded only under *Alternatives considered*. Dropping an issue goal that changes visible output is an output question.

Input, with `{ "design": { "maximumMethodComplexity": 5 } }`:
```csharp
using System;

class C
{
    private static readonly Func<int, int> Compute = value =>
    {
        foreach (var digit in value.ToString())
        {
            if (digit == '0')
            {
                if (value > 10)
                {
                    return 2;
                }
            }
        }

        return 0;
    };
}
```

Before (`main`): not reported.

After, A (recommended, the spec as written) — a lambda is never judged on its own; one in a field, property, or event initializer counts toward no member: not reported.

After, B — a lambda outside every judged body (initializer of a field, property, or event) is judged on its own, foreach 1 + if 2 + if 3 = 6, reported on its `=>`:
```csharp
private static readonly Func<int, int> Compute = value {|RH2009:=>|}   // "This member has a cognitive complexity of 6, which exceeds the limit of 5."
```

Why A: a lambda inside a member already counts toward it, and RH2008 leaves the same initializer lambdas unjudged; B adds a second kind of judged body with a location no other rule uses, for a shape that is uncommon. The cost of A is that complex logic in an initializer lambda is never reported.

**Spec author's questions:** Q1 (local function), Q3 (`and`/`or` in `is` patterns), Q4 (one limit for test code), Q5 (recursion) — I agree with the recommendations (A, A, A, A). Q3 and B above are separate: Q3 is about patterns in conditions, B about patterns in case labels and arms. Q2 is C above, where I recommend B.

## Internal choices

- **`when` clauses and case patterns** — whichever way B is answered, RH2009-003 must say explicitly whether the logical operators inside a `when` clause count (recommend: yes) and whether a structure inside a `when` clause or an arm's pattern is in the switch's nested part (by syntax they are part of the section or arm; recommend saying so). Two readings differ in score only for a `?:`/switch/lambda inside a `when` clause, which is rare; the wording fix removes it.
- **`not` patterns** — RH2009-003 names `!` as starting a new tree but leaves `not` to "joined directly or through parenthesized patterns". The text decides it (`not` breaks the tree), but naming `not` next to `!` removes the doubt. No output difference from the current text.
- **Query expressions** — the spec's construct list does not nest a query clause, so `from x in xs where x > 0 ? … ` scores at the level where the query stands, and `xs.Where(x => …)` nests. Decidable as written; recommend stating it in the "Nothing else" list so it reads as a decision.
- **Rule page, logical operators** — "Each run of `&&` or `||` operators adds 1, and each switch from one to the other adds 1 more" gives 1 for `!(a && b) && c`, the spec 2 (RH2009-003, `E`). The change doc's summary ("each change of operator inside an expression 1 more") has the same gap. Recommend adding "a `!` starts a new run" or accepting the simplification on the rule page only.
- **Rule page, exception** — "A method is reported only when it nests at least one of these structures inside another" contradicts the spec as written for nesting through a lambda or local function (RH2009-004, third bullet); it matches C answered B. Align it with the answer to C.
- **Rule ID and setting name** — agree with RH2009 and `maximumMethodComplexity`.

## Issue goals

thoenissen/Reihitsu#352 is `agent-filed`.

- "Lambdas" among the judged method-shaped members — dropped by the spec; visible, so asked as D.
- "Configuration key shape … `dotnet_diagnostic.RH####.<option>`" — replaced by `reihitsu.json` per maintainer clarification 4; covered by the maintainer.
- "Increments for … jumps" — only `goto` exists to count in C# (no labeled `break`/`continue`); `return`, `throw`, `break`, `continue` score nothing. Matches the issue's intent; no change.
- "Follow the cognitive-complexity definition" — the spec departs from the published definition for recursion (author's Q5) and, under A and B above if answered as recommended, for chained conditionals and patterns in switch arms. Each departure is in an output question; the change doc's *Alternatives considered* should list every departure the maintainer accepts.
- The issue's compliant example scores 1, 3, 2 and stays unreported; its violating example scores 16 and is reported at the default. Both goals hold.

## Architecture findings

**One rule stated in two places (member selection and location).** RH2009-002 and RH2009-006 restate RH2008-002 (members judged), RH2008-004 (members without a body), and RH2008-005 (location tokens) word for word, and the spec's *Why* says the point is that "the two rules about one member stand at one place". In code, RH2008's selection lives in a private `switch` in `RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.TryGetCountedSpan` (member → body, expression body, name token). A second copy in the RH2009 analyzer would be FC-015: the two drift as soon as one gains a member kind (C# 14 compound-assignment operators and extension-block members are the current candidates). Recommend one owner in code — the member → (body, expression body, name token) mapping extracted from RH2008 and used by both — and in the spec, either RH2009 referencing RH2008-002/004/005 in `Uses` instead of restating them, or a `shared/` statement of the member selection. Size is small (a few dozen lines moved, no new behavior).

**Default-on-invalid stated twice.** RH2009-001 restates the fallback rule of SHARED-LINES-002 because that requirement is scoped to line counts. Two statements of "a configured limit that RH0001 reports keeps its default" can drift. Lower priority than the first; the spec author named the widening of SHARED-LINES into a shared configured-limit spec as the alternative. Also check whether the configuration-reading code in `LineCountAnalyzerBase` gets copied rather than shared.

## Follow-up candidates

- Self-hosting violations of RH2009 and a review of the default (already on the lead's list; note that CI's self-hosting scan forces a mechanism in this run, see *Surfaces*).
- If D is answered A: complex lambdas in field, property, and event initializers are judged by no rule; a follow-up could judge them if the self-hosting review shows they matter.
